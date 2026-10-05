#!/usr/bin/env python3
"""
TikTok Video Uniqualizer v6
============================
Processes TikTok-downloaded videos to appear as unique iPhone-recorded content.

Defeats 6 levels of duplicate detection:
  1. File hash          -- any re-encode breaks this
  2. Perceptual hashing -- HFLIP + low-frequency luminance pattern + color grading
  3. Audio fingerprint  -- STFT phase perturbation + constellation poisoning + echo
  4. Deep embeddings    -- vignette + fade + grain change semantic signature
  5. Invisible watermarks -- re-encode overwrites DCT coefficients
  6. Container forensics -- binary MOV patching: qt brand, iPhone handler names,
                            Apple metadata, bt709 color space, no ffmpeg markers

Key innovation over previous 40 failed methods: fixing container-level metadata
that instantly flags ffmpeg processing BEFORE content analysis even begins.
"""

# ============================================================
# CONFIGURATION -- all tunable parameters in one place
# ============================================================

# --- Paths ---
INPUT_DIR = "input"
OUTPUT_DIR = "output"
OUTPUT_PREFIX = "IMG_"
OUTPUT_START_NUMBER = 1001
SUPPORTED_EXTENSIONS = {".mp4", ".mov", ".m4v", ".avi", ".mkv", ".webm"}

# --- Video encoding ---
HFLIP = True                       # Horizontal mirror -- ALWAYS ON
CRF = 18                          # Quality (18 = visually lossless)
PRESET = "slow"                   # x264 preset (slow = good compression)
VIDEO_PROFILE = "high"            # H.264 profile
VIDEO_LEVEL = "4.0"               # H.264 level
GOP_SIZE = 30                     # Keyframe interval (1 sec at 30fps)
SPEED_FACTOR = 1.008              # Playback speed multiplier (0.8% faster)

# --- Per-channel color grading (gamma) ---
GAMMA_R = 1.03                    # Warm reds slightly
GAMMA_G = 1.01                    # Near neutral greens
GAMMA_B = 0.98                    # Cool blues slightly

# --- Visual effects ---
FADE_FRAMES = 20                  # Frames for fade-in / fade-out
VIGNETTE_STRENGTH = 0.08          # Corner darkening intensity
GRAIN_STRENGTH = 1.2              # Film grain sigma per frame
LF_PATTERN_AMPLITUDE = 1.5       # Low-freq luminance pattern peak value
LF_PATTERN_TEMPORAL_SPEED = 0.02  # Temporal drift rate of pattern

# --- Audio processing ---
AUDIO_SAMPLE_RATE = 48000         # iPhone records at 48 kHz
AUDIO_BITRATE = "192k"            # AAC output bitrate
AUDIO_PHASE_PERTURBATION = 0.15   # STFT phase noise amplitude (radians)
AUDIO_STFT_NOISE_FACTOR = 0.003   # Spectral noise relative to mean magnitude
AUDIO_MICRO_ECHO_DELAY_MS = 7     # Micro-echo delay (ms)
AUDIO_MICRO_ECHO_DECAY = 0.06     # Micro-echo amplitude
AUDIO_HARMONIC_STRENGTH = 0.003   # Second-harmonic injection level

# --- iPhone device metadata ---
DEVICE_MAKE = "Apple"
DEVICE_MODEL = "iPhone 15"
DEVICE_SOFTWARE = "26.6"
TIMEZONE_HOURS = 3                # UTC offset for com.apple.quicktime.creationdate
BASE_DATE = "2026-10-04"          # Base date for deterministic timestamps

# ============================================================
# IMPORTS
# ============================================================

import os
import sys
import hashlib
import subprocess
import struct
import json
from datetime import datetime, timedelta, timezone
from pathlib import Path
import numpy as np

# ============================================================
# UTILITY FUNCTIONS
# ============================================================


def log(msg):
    """Print timestamped log message."""
    print(f"[uniqualizer] {msg}", flush=True)


def sha256_file(filepath):
    """Compute SHA-256 of a file for deterministic seeding."""
    h = hashlib.sha256()
    with open(filepath, "rb") as f:
        while True:
            chunk = f.read(65536)
            if not chunk:
                break
            h.update(chunk)
    return h.hexdigest()


def get_seed(file_hash):
    """Derive a 31-bit deterministic seed from a hex hash."""
    return int(file_hash[:8], 16) % (2**31)


def get_video_info(filepath):
    """Return ffprobe JSON for a media file."""
    cmd = [
        "ffprobe", "-v", "quiet",
        "-print_format", "json",
        "-show_format", "-show_streams",
        filepath,
    ]
    result = subprocess.run(cmd, capture_output=True, text=True)
    if result.returncode != 0:
        raise RuntimeError(f"ffprobe failed on {filepath}: {result.stderr}")
    return json.loads(result.stdout)


def parse_fps(rate_str):
    """Parse an FPS string like '30/1' or '29.97' to float."""
    if "/" in str(rate_str):
        parts = str(rate_str).split("/")
        denom = float(parts[1])
        if denom == 0:
            return 30.0
        return float(parts[0]) / denom
    val = float(rate_str)
    return val if val > 0 else 30.0


def get_rotation(info):
    """Extract rotation degrees from ffprobe output."""
    for stream in info.get("streams", []):
        if stream.get("codec_type") != "video":
            continue
        # Check side_data_list (modern ffprobe)
        for sd in stream.get("side_data_list", []):
            if "rotation" in sd:
                return int(sd["rotation"])
        # Fallback to tags
        rot = stream.get("tags", {}).get("rotate", "0")
        try:
            return int(rot)
        except (ValueError, TypeError):
            pass
    return 0


def make_creation_date(file_hash):
    """Deterministic creation date derived from file hash."""
    tz = timezone(timedelta(hours=TIMEZONE_HOURS))
    base = datetime.strptime(BASE_DATE, "%Y-%m-%d").replace(tzinfo=tz)
    hour = int(file_hash[8:10], 16) % 24
    minute = int(file_hash[10:12], 16) % 60
    second = int(file_hash[12:14], 16) % 60
    return base.replace(hour=hour, minute=minute, second=second)


def format_apple_date(dt):
    """Format datetime for com.apple.quicktime.creationdate (ISO + tz)."""
    off = dt.strftime("%z")  # e.g. +0300
    if len(off) >= 5:
        tz_str = f"{off[:3]}:{off[3:]}"
    else:
        tz_str = f"+{TIMEZONE_HOURS:02d}:00"
    return dt.strftime(f"%Y-%m-%dT%H:%M:%S{tz_str}")


def format_utc_date(dt):
    """Format datetime as UTC string for creation_time tag."""
    utc = dt.astimezone(timezone.utc)
    return utc.strftime("%Y-%m-%dT%H:%M:%S.000000Z")


def check_dependencies():
    """Verify ffmpeg and ffprobe are available."""
    for tool in ("ffmpeg", "ffprobe"):
        try:
            r = subprocess.run([tool, "-version"], capture_output=True)
            if r.returncode != 0:
                log(f"ERROR: {tool} returned non-zero exit code")
                sys.exit(1)
        except FileNotFoundError:
            log(f"ERROR: {tool} not found -- install ffmpeg first")
            sys.exit(1)


# ============================================================
# MOV / QUICKTIME BINARY PATCHER
# ============================================================

class Atom:
    """A QuickTime / ISO BMFF atom (box)."""
    __slots__ = ("type", "data", "children", "header_extra")

    def __init__(self, atype, data=None, children=None, header_extra=None):
        self.type = atype            # 4-byte type code
        self.data = data             # raw payload for leaf atoms
        self.children = children or []  # child atoms for containers
        self.header_extra = header_extra  # extra header bytes (e.g. fullbox ver+flags)

    def serialize(self):
        """Serialize this atom (and children) to bytes."""
        if self.children:
            body = self.header_extra or b""
            for child in self.children:
                body += child.serialize()
            total = 8 + len(body)
            return struct.pack(">I", total) + self.type + body
        else:
            body = self.data or b""
            total = 8 + len(body)
            return struct.pack(">I", total) + self.type + body


# Types that contain child atoms (parsed recursively)
_CONTAINERS = {
    b"moov", b"trak", b"mdia", b"minf", b"stbl",
    b"udta", b"edts", b"dinf", b"sinf", b"schi",
    b"tref", b"gmhd", b"ilst",
}
# Full-box containers: 4 extra bytes (version + flags) before children
_FULLBOX_CONTAINERS = {b"meta"}


def parse_atoms(data, start=0, end=None):
    """Parse a flat sequence of atoms from *data[start:end]*."""
    if end is None:
        end = len(data)
    atoms = []
    pos = start
    while pos + 8 <= end:
        raw_size = struct.unpack(">I", data[pos : pos + 4])[0]
        atype = bytes(data[pos + 4 : pos + 8])

        if raw_size == 0:
            atom_end = end
        elif raw_size == 1:
            if pos + 16 > end:
                break
            atom_end = pos + struct.unpack(">Q", data[pos + 8 : pos + 16])[0]
        else:
            atom_end = pos + raw_size

        if atom_end > end:
            atom_end = end
        if atom_end <= pos + 8:
            # Degenerate atom -- skip remaining
            break

        if atype in _CONTAINERS:
            children = parse_atoms(data, pos + 8, atom_end)
            atoms.append(Atom(atype, children=children))
        elif atype in _FULLBOX_CONTAINERS:
            he = bytes(data[pos + 8 : min(pos + 12, atom_end)])
            children = parse_atoms(data, pos + 12, atom_end)
            atoms.append(Atom(atype, children=children, header_extra=he))
        else:
            atoms.append(Atom(atype, data=bytes(data[pos + 8 : atom_end])))

        pos = atom_end
    return atoms


def _build_iphone_ftyp():
    """Build a QuickTime ftyp atom (major_brand=qt)."""
    payload = b"qt  "                          # major_brand
    payload += struct.pack(">I", 0x00000200)   # minor_version  (QT 2.0)
    payload += b"qt  "                         # compatible_brand
    return Atom(b"ftyp", data=payload)


def _patch_hdlr(atom):
    """Replace handler name in hdlr atom with iPhone Core Media name."""
    d = atom.data
    if not d or len(d) < 24:
        return atom
    handler_type = d[8:12]
    names = {
        b"vide": b"Core Media Video\x00",
        b"soun": b"Core Media Audio\x00",
        b"mdta": b"Core Media Metadata\x00",
    }
    new_name = names.get(handler_type)
    if new_name:
        return Atom(b"hdlr", data=d[:24] + new_name)
    return atom


def _modify_atoms(atoms):
    """Walk atom tree: fix ftyp, hdlr, remove encoder tags."""
    out = []
    for atom in atoms:
        if atom.type == b"ftyp":
            out.append(_build_iphone_ftyp())
            continue
        if atom.type == b"hdlr":
            out.append(_patch_hdlr(atom))
            continue
        # Remove encoder-tool atoms
        if atom.type in (b"\xa9too", b"\xa9enc", b"\xa9swr"):
            continue
        if atom.children:
            atom.children = _modify_atoms(atom.children)
        out.append(atom)
    return out


def _adjust_stco(atoms, delta):
    """Shift all chunk-offset entries (stco / co64) by *delta* bytes."""
    for atom in atoms:
        if atom.type == b"stco" and atom.data and len(atom.data) >= 8:
            d = bytearray(atom.data)
            count = struct.unpack(">I", d[4:8])[0]
            for i in range(count):
                off = 8 + i * 4
                if off + 4 <= len(d):
                    val = struct.unpack(">I", d[off : off + 4])[0]
                    struct.pack_into(">I", d, off, val + delta)
            atom.data = bytes(d)
        elif atom.type == b"co64" and atom.data and len(atom.data) >= 8:
            d = bytearray(atom.data)
            count = struct.unpack(">I", d[4:8])[0]
            for i in range(count):
                off = 8 + i * 8
                if off + 8 <= len(d):
                    val = struct.unpack(">Q", d[off : off + 8])[0]
                    struct.pack_into(">Q", d, off, val + delta)
            atom.data = bytes(d)
        if atom.children:
            _adjust_stco(atom.children, delta)


def _has_apple_metadata(atoms):
    """Return True if the atom tree already contains Apple mdta-style metadata."""
    for atom in atoms:
        if atom.type == b"keys":
            return True
        if atom.children and _has_apple_metadata(atom.children):
            return True
    return False


def _build_keys_atom(key_names):
    """Build a QuickTime 'keys' atom from a list of key name strings."""
    body = struct.pack(">I", 0)              # version + flags
    body += struct.pack(">I", len(key_names))
    for name in key_names:
        nb = name.encode("utf-8")
        body += struct.pack(">I", 8 + len(nb))
        body += b"mdta"
        body += nb
    return Atom(b"keys", data=body)


def _build_ilst_atom(values):
    """Build a QuickTime 'ilst' atom with UTF-8 data items for each value."""
    children = []
    for i, value in enumerate(values):
        idx = i + 1
        item_type = struct.pack(">I", idx)
        val_bytes = value.encode("utf-8")
        data_payload = struct.pack(">I", 1) + struct.pack(">I", 0) + val_bytes
        data_atom = Atom(b"data", data=data_payload)
        children.append(Atom(item_type, children=[data_atom]))
    return Atom(b"ilst", children=children)


def _ensure_apple_metadata(atoms, apple_date_str, utc_date_str):
    """Add Apple QuickTime metadata to moov/udta/meta if not already present."""
    if _has_apple_metadata(atoms):
        return

    # Find moov
    moov = None
    for atom in atoms:
        if atom.type == b"moov":
            moov = atom
            break
    if moov is None or not moov.children:
        return

    # Find or create udta inside moov
    udta = None
    for child in moov.children:
        if child.type == b"udta":
            udta = child
            break
    if udta is None:
        udta = Atom(b"udta", children=[])
        moov.children.append(udta)

    # Build metadata handler
    hdlr_data = (
        b"\x00\x00\x00\x00"      # version + flags
        b"\x00\x00\x00\x00"      # pre_defined
        b"mdta"                   # handler_type
        + b"\x00" * 12            # reserved
        + b"Core Media Metadata\x00"
    )
    hdlr_atom = Atom(b"hdlr", data=hdlr_data)

    keys_atom = _build_keys_atom([
        "com.apple.quicktime.make",
        "com.apple.quicktime.model",
        "com.apple.quicktime.software",
        "com.apple.quicktime.creationdate",
    ])

    ilst_atom = _build_ilst_atom([
        DEVICE_MAKE,
        DEVICE_MODEL,
        DEVICE_SOFTWARE,
        apple_date_str,
    ])

    meta_atom = Atom(
        b"meta",
        children=[hdlr_atom, keys_atom, ilst_atom],
        header_extra=b"\x00\x00\x00\x00",
    )
    udta.children.append(meta_atom)


def patch_mov_file(filepath, apple_date_str, utc_date_str):
    """
    Binary-patch a MOV file so its container metadata matches a real iPhone.

    Fixes: ftyp brand -> qt, hdlr handler names -> Core Media *,
    removes encoder tags, adds Apple QuickTime metadata if absent.
    Adjusts stco / co64 chunk offsets after any size changes.
    """
    log("  Patching MOV binary metadata...")

    with open(filepath, "rb") as f:
        file_data = f.read()

    atoms = parse_atoms(file_data)

    # Split into pre-mdat, mdat, post-mdat
    pre_mdat = []
    mdat_atom = None
    post_mdat = []
    found = False
    for atom in atoms:
        if atom.type == b"mdat" and not found:
            mdat_atom = atom
            found = True
        elif not found:
            pre_mdat.append(atom)
        else:
            post_mdat.append(atom)

    if mdat_atom is None:
        log("  WARNING: no mdat atom -- skipping binary patch")
        return

    # Measure original pre-mdat byte size
    old_pre_size = sum(len(a.serialize()) for a in pre_mdat)

    # Modify atoms
    pre_mdat = _modify_atoms(pre_mdat)
    post_mdat = _modify_atoms(post_mdat)

    # Add Apple metadata if missing
    _ensure_apple_metadata(pre_mdat, apple_date_str, utc_date_str)
    _ensure_apple_metadata(post_mdat, apple_date_str, utc_date_str)

    # Compute byte-offset delta and fix chunk offsets
    new_pre_size = sum(len(a.serialize()) for a in pre_mdat)
    delta = new_pre_size - old_pre_size
    if delta != 0:
        log(f"    Chunk-offset delta: {delta:+d} bytes")
        _adjust_stco(pre_mdat, delta)
        _adjust_stco(post_mdat, delta)

    # Reassemble file
    output = bytearray()
    for a in pre_mdat:
        output.extend(a.serialize())
    output.extend(mdat_atom.serialize())
    for a in post_mdat:
        output.extend(a.serialize())

    with open(filepath, "wb") as f:
        f.write(output)

    log("    MOV patch complete")


# ============================================================
# AUDIO PROCESSING
# ============================================================


def _extract_audio(input_path, output_wav):
    """Extract audio from input as 48 kHz stereo WAV. Returns False if no audio."""
    cmd = [
        "ffmpeg", "-y", "-nostdin",
        "-i", input_path,
        "-vn",
        "-ar", str(AUDIO_SAMPLE_RATE),
        "-ac", "2",
        "-acodec", "pcm_s16le",
        "-f", "wav",
        output_wav,
    ]
    r = subprocess.run(cmd, capture_output=True)
    return r.returncode == 0 and os.path.isfile(output_wav)


def _read_wav(filepath):
    """Read a PCM WAV file into (int16 ndarray, sample_rate, channels)."""
    with open(filepath, "rb") as f:
        if f.read(4) != b"RIFF":
            raise ValueError("not a WAV file")
        f.read(4)  # file size
        if f.read(4) != b"WAVE":
            raise ValueError("not a WAV file")

        channels = 2
        sample_rate = AUDIO_SAMPLE_RATE
        bits = 16
        audio_bytes = b""

        while True:
            hdr = f.read(8)
            if len(hdr) < 8:
                break
            cid = hdr[:4]
            csz = struct.unpack("<I", hdr[4:8])[0]
            if cid == b"fmt ":
                fmt = f.read(csz)
                channels = struct.unpack("<H", fmt[2:4])[0]
                sample_rate = struct.unpack("<I", fmt[4:8])[0]
                bits = struct.unpack("<H", fmt[14:16])[0]
            elif cid == b"data":
                audio_bytes = f.read(csz)
                break
            else:
                f.read(csz)
                if csz % 2:
                    f.read(1)

        arr = np.frombuffer(audio_bytes, dtype=np.int16)
        if channels > 1:
            arr = arr.reshape(-1, channels)
        return arr, sample_rate, channels


def _write_wav(filepath, data, sample_rate, channels=2):
    """Write int16 ndarray to WAV file."""
    if data.ndim == 2:
        n_samples = data.shape[0]
    else:
        n_samples = len(data)
        channels = 1

    bps = 2  # bytes per sample (16-bit)
    data_size = n_samples * channels * bps

    with open(filepath, "wb") as f:
        f.write(b"RIFF")
        f.write(struct.pack("<I", 36 + data_size))
        f.write(b"WAVE")
        f.write(b"fmt ")
        f.write(struct.pack("<I", 16))
        f.write(struct.pack("<H", 1))             # PCM
        f.write(struct.pack("<H", channels))
        f.write(struct.pack("<I", sample_rate))
        f.write(struct.pack("<I", sample_rate * channels * bps))
        f.write(struct.pack("<H", channels * bps))
        f.write(struct.pack("<H", 16))             # bits
        f.write(b"data")
        f.write(struct.pack("<I", data_size))
        f.write(data.astype(np.int16).tobytes())


def _process_audio_channel(samples, rng, sample_rate):
    """
    Process one channel of audio (float32, range -1..1).

    1. STFT phase perturbation + constellation noise  (defeats Shazam)
    2. Micro-echo at non-standard delay               (shifts spectral peaks)
    3. Second-harmonic injection                       (adds energy Content ID
                                                        does not expect)
    """
    n = len(samples)
    window_size = 2048
    hop_size = 512
    window = np.hanning(window_size).astype(np.float32)

    # Frequency-bin range for Shazam-sensitive band (200 Hz -- 5000 Hz)
    low_bin = max(1, int(200 * window_size / sample_rate))
    high_bin = min(window_size // 2, int(5000 * window_size / sample_rate))
    n_target = max(1, high_bin - low_bin)

    n_frames = max(0, (n - window_size) // hop_size + 1)
    output = np.zeros(n, dtype=np.float32)
    win_sum = np.zeros(n, dtype=np.float32)

    for i in range(n_frames):
        start = i * hop_size
        end = start + window_size
        if end > n:
            break

        frame = samples[start:end] * window
        spectrum = np.fft.rfft(frame)
        mag = np.abs(spectrum)
        phase = np.angle(spectrum)

        # Phase perturbation in Shazam-sensitive band
        phase_noise = rng.uniform(
            -AUDIO_PHASE_PERTURBATION, AUDIO_PHASE_PERTURBATION, n_target
        ).astype(np.float32)
        phase[low_bin:high_bin] += phase_noise

        # Add spectral noise to shift constellation peaks
        mean_mag = max(float(np.mean(mag[low_bin:high_bin])), 1e-10)
        noise_mag = rng.exponential(
            AUDIO_STFT_NOISE_FACTOR * mean_mag, n_target
        ).astype(np.float32)
        mag[low_bin:high_bin] += noise_mag

        spectrum = mag * np.exp(1j * phase)
        recon = np.fft.irfft(spectrum, n=window_size).astype(np.float32)

        output[start:end] += recon * window
        win_sum[start:end] += window * window

    # Normalize overlap-add; keep unprocessed tail as-is
    mask = win_sum > 1e-8
    output[mask] /= win_sum[mask]
    output[~mask] = samples[~mask]

    # Micro-echo
    delay = int(AUDIO_MICRO_ECHO_DELAY_MS * sample_rate / 1000)
    if 0 < delay < n:
        echoed = np.copy(output)
        echoed[delay:] += output[:-delay] * AUDIO_MICRO_ECHO_DECAY
        output = echoed

    # Second-harmonic injection
    if AUDIO_HARMONIC_STRENGTH > 0:
        harmonic = output * output * AUDIO_HARMONIC_STRENGTH
        harmonic -= np.mean(harmonic)  # strip DC
        output += harmonic

    return output


def process_audio(input_wav, output_wav, rng):
    """Full audio processing pipeline: STFT + echo + harmonics + speed."""
    log("  Processing audio...")
    data, sr, channels = _read_wav(input_wav)

    # To float
    fdata = data.astype(np.float32) / 32768.0

    # Process each channel independently (different phase perturbations break
    # Shazam's mono-mix heuristic)
    if channels > 1 and fdata.ndim == 2:
        for ch in range(channels):
            ch_rng = np.random.default_rng(int(rng.integers(0, 2**31)) + ch)
            fdata[:, ch] = _process_audio_channel(fdata[:, ch], ch_rng, sr)
    else:
        fdata = _process_audio_channel(fdata.ravel(), rng, sr)

    # Speed change (resample to match video speed factor)
    if SPEED_FACTOR != 1.0:
        old_len = fdata.shape[0]
        new_len = int(old_len / SPEED_FACTOR)
        old_x = np.linspace(0, 1, old_len)
        new_x = np.linspace(0, 1, new_len)
        if fdata.ndim == 2:
            new_data = np.zeros((new_len, channels), dtype=np.float32)
            for ch in range(channels):
                new_data[:, ch] = np.interp(new_x, old_x, fdata[:, ch])
            fdata = new_data
        else:
            fdata = np.interp(new_x, old_x, fdata).astype(np.float32)

    # Back to int16
    fdata = np.clip(fdata, -1.0, 1.0)
    int_data = (fdata * 32767).astype(np.int16)
    _write_wav(output_wav, int_data, sr, channels)
    log("    Audio done")


# ============================================================
# VIDEO FRAME PROCESSOR
# ============================================================

class FrameProcessor:
    """
    Deterministic per-frame video processing.

    Precomputes lookup tables, vignette map, low-frequency pattern, and grain
    weights once.  The ``process`` method is called for every decoded frame.
    """

    def __init__(self, width, height, total_frames, rng):
        self.w = width
        self.h = height
        self.total_frames = total_frames
        self.eff_fade = min(FADE_FRAMES, total_frames // 3) if total_frames > 0 else 0

        # Per-channel gamma LUTs (uint8 -> uint8)
        x = np.arange(256, dtype=np.float64) / 255.0
        self.lut_b = np.clip(255.0 * x ** (1.0 / GAMMA_B), 0, 255).astype(np.uint8)
        self.lut_g = np.clip(255.0 * x ** (1.0 / GAMMA_G), 0, 255).astype(np.uint8)
        self.lut_r = np.clip(255.0 * x ** (1.0 / GAMMA_R), 0, 255).astype(np.uint8)

        # Vignette map (h, w, 1) for broadcasting
        Y = np.linspace(-1, 1, height).reshape(-1, 1).astype(np.float32)
        X = np.linspace(-1, 1, width).reshape(1, -1).astype(np.float32)
        radius = np.sqrt(X * X + Y * Y)
        vig = 1.0 - VIGNETTE_STRENGTH * np.clip(radius - 0.7, 0, None) ** 2
        self.vignette = np.clip(vig, 0, 1).astype(np.float32)[:, :, np.newaxis]

        # Low-frequency luminance pattern (h, w, 3)
        phase_x = float(rng.uniform(0, 2 * np.pi))
        phase_y = float(rng.uniform(0, 2 * np.pi))
        Xp = np.arange(width, dtype=np.float32).reshape(1, -1) * (2 * np.pi * 3 / width)
        Yp = np.arange(height, dtype=np.float32).reshape(-1, 1) * (2 * np.pi * 2 / height)
        base_pattern = (np.sin(Xp + phase_x) * np.sin(Yp + phase_y) * LF_PATTERN_AMPLITUDE)
        # Channel weights: B*0.8, G*1.0, R*0.8
        self.lf_pattern = np.stack(
            [base_pattern * 0.8, base_pattern, base_pattern * 0.8], axis=2
        ).astype(np.float32)

        # Grain channel weights (1, 1, 3) for broadcasting
        self.grain_weights = np.array(
            [0.8, 1.0, 0.8], dtype=np.float32
        ).reshape(1, 1, 3)

        # Base seed for per-frame grain RNG
        self.grain_seed = int(rng.integers(0, 2**31))

    def process(self, frame, frame_idx):
        """
        Process a single BGR uint8 frame in-place-ish. Returns uint8 ndarray.

        Operations (order matters for quality):
          1. Horizontal flip
          2. Per-channel gamma via LUT
          3. Low-frequency luminance pattern (temporally drifting)
          4. Film grain
          5. Vignette
          6. Fade-in / fade-out
        """
        # 1. HFLIP  (mandatory)
        frame = np.ascontiguousarray(frame[:, ::-1, :])

        # 2. Color grading via LUT  (uint8 domain -- fast)
        frame[:, :, 0] = self.lut_b[frame[:, :, 0]]
        frame[:, :, 1] = self.lut_g[frame[:, :, 1]]
        frame[:, :, 2] = self.lut_r[frame[:, :, 2]]

        # Switch to float for additive / multiplicative ops
        f = frame.astype(np.float32)

        # 3. Low-frequency pattern with temporal drift
        temporal_mod = 1.0 + 0.2 * np.sin(frame_idx * LF_PATTERN_TEMPORAL_SPEED)
        f += self.lf_pattern * temporal_mod

        # 4. Film grain (deterministic per frame via seed)
        grain_rng = np.random.default_rng(self.grain_seed + frame_idx)
        grain = grain_rng.normal(0, GRAIN_STRENGTH, (self.h, self.w)).astype(np.float32)
        f += grain[:, :, np.newaxis] * self.grain_weights

        # 5. Vignette
        f *= self.vignette

        # 6. Fade-in / fade-out
        if self.eff_fade > 0:
            if frame_idx < self.eff_fade:
                factor = 0.92 + 0.08 * (frame_idx / self.eff_fade)
                f *= factor
            elif (self.total_frames > 0
                  and frame_idx >= self.total_frames - self.eff_fade):
                remaining = self.total_frames - 1 - frame_idx
                factor = 0.92 + 0.08 * max(0.0, remaining / self.eff_fade)
                f *= factor

        return np.clip(f, 0, 255).astype(np.uint8)


# ============================================================
# MAIN PIPELINE
# ============================================================


def process_video(input_path, output_path):
    """
    End-to-end processing of one video file.

    Pipeline:
      1. Hash input for deterministic seed
      2. Probe input properties (resolution, fps, rotation, duration)
      3. Extract and process audio
      4. Decode -> per-frame processing -> encode  (pipe-based)
      5. Binary-patch the output MOV for iPhone metadata
    """
    log(f"Processing: {input_path}")

    # --- Deterministic seed ---
    file_hash = sha256_file(input_path)
    seed = get_seed(file_hash)
    rng = np.random.default_rng(seed)
    log(f"  Hash: {file_hash[:16]}...  Seed: {seed}")

    # --- Probe input ---
    info = get_video_info(input_path)

    v_stream = None
    a_stream = None
    for s in info.get("streams", []):
        if s.get("codec_type") == "video" and v_stream is None:
            v_stream = s
        elif s.get("codec_type") == "audio" and a_stream is None:
            a_stream = s

    if v_stream is None:
        log("  ERROR: no video stream found")
        return False

    coded_w = int(v_stream["width"])
    coded_h = int(v_stream["height"])
    fps_str = v_stream.get("r_frame_rate",
                           v_stream.get("avg_frame_rate", "30/1"))
    fps = parse_fps(fps_str)
    if fps <= 0 or fps > 240:
        fps = 30.0
    rotation = get_rotation(info)

    # Display dimensions (after rotation)
    if rotation in (90, -90, 270, -270):
        display_w, display_h = coded_h, coded_w
    else:
        display_w, display_h = coded_w, coded_h

    # Ensure even dimensions (required by yuv420p)
    display_w = display_w // 2 * 2
    display_h = display_h // 2 * 2

    # Duration and frame count
    duration = 0.0
    try:
        duration = float(info.get("format", {}).get("duration", 0))
    except (ValueError, TypeError):
        pass
    if duration <= 0:
        try:
            duration = float(v_stream.get("duration", 0))
        except (ValueError, TypeError):
            pass
    if duration <= 0:
        nb = int(v_stream.get("nb_frames", 0))
        if nb > 0:
            duration = nb / fps
        else:
            duration = 30.0
    total_frames = int(duration * fps)

    out_fps = fps * SPEED_FACTOR

    log(f"  Input:   {coded_w}x{coded_h} @ {fps:.2f}fps, "
        f"{duration:.1f}s, rotation={rotation}")
    log(f"  Output:  {display_w}x{display_h} @ {out_fps:.2f}fps, "
        f"~{total_frames} frames")

    # --- Creation date ---
    creation_dt = make_creation_date(file_hash)
    apple_date = format_apple_date(creation_dt)
    utc_date = format_utc_date(creation_dt)
    log(f"  Date:    {apple_date}")

    # --- Temp files ---
    temp_dir = os.path.join(os.path.dirname(os.path.abspath(output_path)),
                            ".uniqualizer_temp")
    os.makedirs(temp_dir, exist_ok=True)
    temp_audio_in = os.path.join(temp_dir, "audio_in.wav")
    temp_audio_out = os.path.join(temp_dir, "audio_out.wav")

    # --- Audio ---
    has_audio = a_stream is not None
    if has_audio:
        has_audio = _extract_audio(input_path, temp_audio_in)
        if has_audio:
            audio_rng = np.random.default_rng(int(rng.integers(0, 2**31)))
            process_audio(temp_audio_in, temp_audio_out, audio_rng)

    # --- Build ffmpeg DECODE command ---
    decode_vf = []
    if rotation in (90, -270):
        decode_vf.append("transpose=2")       # counter-clockwise 90
    elif rotation in (-90, 270):
        decode_vf.append("transpose=1")       # clockwise 90
    elif rotation in (180, -180):
        decode_vf.append("transpose=2,transpose=2")
    # Force even dimensions
    decode_vf.append(f"scale={display_w}:{display_h}")

    decode_cmd = [
        "ffmpeg", "-nostdin", "-noautorotate",
        "-i", input_path,
        "-vf", ",".join(decode_vf),
        "-f", "rawvideo", "-pix_fmt", "bgr24",
        "-v", "quiet",
        "pipe:1",
    ]

    # --- Build ffmpeg ENCODE command ---
    encode_cmd = [
        "ffmpeg", "-y", "-nostdin",
        # raw video from pipe
        "-f", "rawvideo",
        "-pix_fmt", "bgr24",
        "-s", f"{display_w}x{display_h}",
        "-r", f"{out_fps:.6f}",
        "-i", "pipe:0",
    ]
    if has_audio:
        encode_cmd.extend(["-i", temp_audio_out])

    encode_cmd.extend([
        # video codec
        "-c:v", "libx264",
        "-preset", PRESET,
        "-crf", str(CRF),
        "-profile:v", VIDEO_PROFILE,
        "-level:v", VIDEO_LEVEL,
        "-pix_fmt", "yuv420p",
        "-g", str(GOP_SIZE),
        "-bf", "2",
        # color space
        "-colorspace", "bt709",
        "-color_trc", "bt709",
        "-color_primaries", "bt709",
        "-color_range", "1",
        "-tag:v", "avc1",
        # bitexact for determinism and to suppress encoder tag
        "-flags:v", "+bitexact",
        "-flags:a", "+bitexact",
        "-fflags", "+bitexact",
        # handler names (best-effort; binary patcher fixes if ignored)
        "-metadata:s:v:0", "handler_name=Core Media Video",
    ])

    if has_audio:
        encode_cmd.extend([
            "-c:a", "aac",
            "-b:a", AUDIO_BITRATE,
            "-ar", str(AUDIO_SAMPLE_RATE),
            "-ac", "2",
            "-metadata:s:a:0", "handler_name=Core Media Audio",
        ])

    encode_cmd.extend([
        # strip input metadata, then set our own
        "-map_metadata", "-1",
        "-movflags", "+write_colr+use_metadata_tags+faststart",
        "-metadata", f"com.apple.quicktime.make={DEVICE_MAKE}",
        "-metadata", f"com.apple.quicktime.model={DEVICE_MODEL}",
        "-metadata", f"com.apple.quicktime.software={DEVICE_SOFTWARE}",
        "-metadata", f"com.apple.quicktime.creationdate={apple_date}",
        "-metadata", f"creation_time={utc_date}",
        "-metadata", "encoder=",
        "-f", "mov",
        output_path,
    ])

    # --- Initialize frame processor ---
    frame_proc = FrameProcessor(display_w, display_h, total_frames, rng)

    # --- Run pipeline ---
    log("  Starting pipe-based encode...")
    frame_size = display_w * display_h * 3

    decode_proc = subprocess.Popen(
        decode_cmd,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        bufsize=frame_size,
    )
    encode_proc = subprocess.Popen(
        encode_cmd,
        stdin=subprocess.PIPE,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        bufsize=frame_size,
    )

    frame_idx = 0
    try:
        while True:
            raw = decode_proc.stdout.read(frame_size)
            if len(raw) < frame_size:
                break
            frame = np.frombuffer(raw, dtype=np.uint8).reshape(
                display_h, display_w, 3
            )
            processed = frame_proc.process(frame, frame_idx)
            encode_proc.stdin.write(processed.tobytes())
            frame_idx += 1
            if frame_idx % 100 == 0:
                log(f"    Frame {frame_idx}/{total_frames}")

        encode_proc.stdin.close()
        enc_out, enc_err = encode_proc.communicate(timeout=300)
        decode_proc.wait(timeout=30)

    except Exception as exc:
        decode_proc.kill()
        encode_proc.kill()
        raise RuntimeError(f"Pipe error: {exc}") from exc

    log(f"  Encoded {frame_idx} frames")

    if encode_proc.returncode != 0:
        err_msg = enc_err.decode("utf-8", errors="replace")[:500]
        log(f"  ENCODE FAILED: {err_msg}")
        return False

    # --- Binary-patch MOV ---
    patch_mov_file(output_path, apple_date, utc_date)

    # --- Cleanup ---
    for tmp in (temp_audio_in, temp_audio_out):
        if os.path.isfile(tmp):
            os.remove(tmp)
    try:
        os.rmdir(temp_dir)
    except OSError:
        pass

    out_size = os.path.getsize(output_path) / (1024 * 1024)
    log(f"  Done: {output_path}  ({out_size:.1f} MB)")
    return True


def main():
    """Entry point: scan input/ directory, process each video, write to output/."""
    log("TikTok Video Uniqualizer v6")
    log("=" * 60)

    check_dependencies()

    os.makedirs(INPUT_DIR, exist_ok=True)
    os.makedirs(OUTPUT_DIR, exist_ok=True)

    # Collect input files (case-insensitive extension matching)
    input_files = []
    for entry in sorted(Path(INPUT_DIR).iterdir()):
        if entry.is_file() and entry.suffix.lower() in SUPPORTED_EXTENSIONS:
            input_files.append(str(entry))

    if not input_files:
        log(f"No input files in {INPUT_DIR}/")
        log(f"Supported: {', '.join(sorted(SUPPORTED_EXTENSIONS))}")
        return

    log(f"Found {len(input_files)} file(s)")

    ok = 0
    for i, inp in enumerate(input_files):
        num = OUTPUT_START_NUMBER + i
        out_name = f"{OUTPUT_PREFIX}{num:04d}.MOV"
        out_path = os.path.join(OUTPUT_DIR, out_name)
        try:
            if process_video(inp, out_path):
                ok += 1
        except Exception as exc:
            log(f"  FAILED: {exc}")
            import traceback
            traceback.print_exc()

    log("=" * 60)
    log(f"Complete: {ok}/{len(input_files)} succeeded")


if __name__ == "__main__":
    main()
