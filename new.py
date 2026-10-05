#!/usr/bin/env python3
"""
TikTok Video Uniqualizer v7
============================
Processes TikTok-downloaded videos to appear as unique iPhone-recorded content.

Processing pipeline:
  - HFLIP (horizontal mirror)
  - Mesh warp (cv2 spatial deformation on grid)
  - DCT butterfly (mid-frequency perturbation in YUV domain)
  - Attention dilution (texture injection in uniform areas)
  - Constellation poisoning (phantom audio frequency peaks)
  - iPhone metadata spoofing (QuickTime container, Apple metadata, bt709 color)
  - Binary MOV patching (ftyp, hdlr, stco/co64, ffmpeg marker scrub)
"""

# ============================================================
# CONFIGURATION
# ============================================================

# --- Paths ---
INPUT_DIR = "input"
OUTPUT_DIR = "output"
OUTPUT_PREFIX = "IMG_"
OUTPUT_START_NUMBER = 1001
SUPPORTED_EXTENSIONS = {".mp4", ".mov", ".m4v", ".avi", ".mkv", ".webm"}

# --- Video encoding ---
HFLIP = True
CRF = 23
PRESET = "slow"
VIDEO_PROFILE = "high"
VIDEO_LEVEL = "4.0"
GOP_SIZE = 30
SPEED_FACTOR = 1.03

# --- Fixed seed for identical processing across all videos ---
FIXED_PROCESSING_SEED = 0xDEAD_BEEF

# --- Mesh warp (cv2 spatial deformation) ---
MESH_GRID = 20           # Control points per axis (was 8→16→20)
MESH_AMP = 6.0           # Max pixel displacement (was 2.0→4.0→6.0)

# --- DCT butterfly (mid-frequency perturbation) ---
DCT_STRENGTH = 0.25      # Perturbation amplitude (was 0.08→0.15→0.25)

# --- Attention dilution (texture in flat areas) ---
DILUTION_AMP = 12        # Noise amplitude (was 4→8→12)
DILUTION_THRESHOLD = 25  # Flat-area std cutoff (was 12→18→25)
DILUTION_BLOCK = 16      # Block size for uniformity check

# --- Audio constellation poisoning ---
AUDIO_SAMPLE_RATE = 48000
AUDIO_BITRATE = "192k"
AUDIO_PHANTOM_OFFSET_HZ = 180   # Phantom peak offset Hz (was 75→120→180)
AUDIO_PHANTOM_AMP = 1.0         # Phantom peak amplitude (was 0.80→0.95→1.0)
AUDIO_PHANTOM_PEAKS_MULT = 12   # Peaks per second of audio (was 3→8→12)

# --- iPhone device metadata ---
DEVICE_MAKE = "Apple"
DEVICE_MODEL = "iPhone 15"
DEVICE_SOFTWARE = "26.6"
TIMEZONE_HOURS = 3
BASE_DATE = "2026-10-04"

# ============================================================
# IMPORTS
# ============================================================

import os
import sys
import hashlib
import subprocess
import struct
import json
import threading
from datetime import datetime, timedelta, timezone
from pathlib import Path
import numpy as np

try:
    import cv2
except ImportError:
    print("ERROR: opencv-python required. Install: pip install opencv-python-headless")
    sys.exit(1)

# ============================================================
# UTILITY FUNCTIONS
# ============================================================


def log(msg):
    print(f"[uniqualizer] {msg}", flush=True)


def sha256_file(filepath):
    h = hashlib.sha256()
    with open(filepath, "rb") as f:
        while True:
            chunk = f.read(65536)
            if not chunk:
                break
            h.update(chunk)
    return h.hexdigest()


def get_video_info(filepath):
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
    if "/" in str(rate_str):
        parts = str(rate_str).split("/")
        denom = float(parts[1])
        if denom == 0:
            return 30.0
        return float(parts[0]) / denom
    val = float(rate_str)
    return val if val > 0 else 30.0


def get_rotation(info):
    for stream in info.get("streams", []):
        if stream.get("codec_type") != "video":
            continue
        for sd in stream.get("side_data_list", []):
            if "rotation" in sd:
                return int(sd["rotation"])
        rot = stream.get("tags", {}).get("rotate", "0")
        try:
            return int(rot)
        except (ValueError, TypeError):
            pass
    return 0


def make_creation_date(file_hash):
    tz = timezone(timedelta(hours=TIMEZONE_HOURS))
    base = datetime.strptime(BASE_DATE, "%Y-%m-%d").replace(tzinfo=tz)
    hour = int(file_hash[8:10], 16) % 24
    minute = int(file_hash[10:12], 16) % 60
    second = int(file_hash[12:14], 16) % 60
    return base.replace(hour=hour, minute=minute, second=second)


def format_apple_date(dt):
    off = dt.strftime("%z")
    if len(off) >= 5:
        tz_str = f"{off[:3]}:{off[3:]}"
    else:
        tz_str = f"+{TIMEZONE_HOURS:02d}:00"
    return dt.strftime(f"%Y-%m-%dT%H:%M:%S{tz_str}")


def format_utc_date(dt):
    utc = dt.astimezone(timezone.utc)
    return utc.strftime("%Y-%m-%dT%H:%M:%S.000000Z")


def check_dependencies():
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
    __slots__ = ("type", "data", "children", "header_extra")

    def __init__(self, atype, data=None, children=None, header_extra=None):
        self.type = atype
        self.data = data
        self.children = children or []
        self.header_extra = header_extra

    def serialize(self):
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


_CONTAINERS = {
    b"moov", b"trak", b"mdia", b"minf", b"stbl",
    b"udta", b"edts", b"dinf", b"sinf", b"schi",
    b"tref", b"gmhd", b"ilst",
}
_FULLBOX_CONTAINERS = {b"meta"}


def parse_atoms(data, start=0, end=None):
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
        if atom_end < pos + 8:
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
    payload = b"qt  "
    payload += struct.pack(">I", 0x00000200)
    payload += b"qt  "
    return Atom(b"ftyp", data=payload)


def _patch_hdlr(atom):
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


def _patch_vendor_id(atom):
    if atom.data and len(atom.data) >= 4:
        d = bytearray(atom.data)
        ffmp = b"FFMP"
        idx = 0
        while idx <= len(d) - 4:
            if d[idx:idx+4] == ffmp:
                d[idx:idx+4] = b"\x00\x00\x00\x00"
            idx += 1
        atom.data = bytes(d)
    return atom


def _modify_atoms(atoms):
    out = []
    for atom in atoms:
        if atom.type == b"ftyp":
            out.append(_build_iphone_ftyp())
            continue
        if atom.type == b"hdlr":
            out.append(_patch_hdlr(atom))
            continue
        if atom.type in (b"\xa9too", b"\xa9enc", b"\xa9swr", b"\xa9nam"):
            continue
        if atom.type in (b"avc1", b"mp4a", b"hvc1"):
            atom = _patch_vendor_id(atom)
        if atom.children:
            atom.children = _modify_atoms(atom.children)
        out.append(atom)
    return out


def _adjust_stco(atoms, delta):
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
    for atom in atoms:
        if atom.type == b"keys":
            return True
        if atom.children and _has_apple_metadata(atom.children):
            return True
    return False


def _build_keys_atom(key_names):
    body = struct.pack(">I", 0)
    body += struct.pack(">I", len(key_names))
    for name in key_names:
        nb = name.encode("utf-8")
        body += struct.pack(">I", 8 + len(nb))
        body += b"mdta"
        body += nb
    return Atom(b"keys", data=body)


def _build_ilst_atom(values):
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
    if _has_apple_metadata(atoms):
        return

    moov = None
    for atom in atoms:
        if atom.type == b"moov":
            moov = atom
            break
    if moov is None or not moov.children:
        return

    udta = None
    for child in moov.children:
        if child.type == b"udta":
            udta = child
            break
    if udta is None:
        udta = Atom(b"udta", children=[])
        moov.children.append(udta)

    hdlr_data = (
        b"\x00\x00\x00\x00"
        b"\x00\x00\x00\x00"
        b"mdta"
        + b"\x00" * 12
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
    log("  Patching MOV binary metadata...")

    with open(filepath, "rb") as f:
        file_data = f.read()

    atoms = parse_atoms(file_data)

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

    old_pre_size = sum(len(a.serialize()) for a in pre_mdat)

    pre_mdat = _modify_atoms(pre_mdat)
    post_mdat = _modify_atoms(post_mdat)

    _ensure_apple_metadata(pre_mdat, apple_date_str, utc_date_str)
    _ensure_apple_metadata(post_mdat, apple_date_str, utc_date_str)

    new_pre_size = sum(len(a.serialize()) for a in pre_mdat)
    delta = new_pre_size - old_pre_size
    if delta != 0:
        log(f"    Chunk-offset delta: {delta:+d} bytes")
        _adjust_stco(pre_mdat, delta)
        _adjust_stco(post_mdat, delta)

    output = bytearray()
    for a in pre_mdat:
        output.extend(a.serialize())
    output.extend(mdat_atom.serialize())
    for a in post_mdat:
        output.extend(a.serialize())

    with open(filepath, "wb") as f:
        f.write(output)

    log("    Scrubbing ffmpeg markers...")
    with open(filepath, "rb") as f:
        raw = bytearray(f.read())

    ffmpeg_markers = [
        (b"FFMP", b"\x00\x00\x00\x00"),
        (b"Lavc libx264", b"H.264\x00\x00\x00\x00\x00\x00\x00"),
        (b"Lavc libx265", b"H.265\x00\x00\x00\x00\x00\x00\x00"),
        (b"Lavc60", b"\x00\x00\x00\x00\x00\x00"),
        (b"Lavc61", b"\x00\x00\x00\x00\x00\x00"),
        (b"Lavc", b"\x00\x00\x00\x00"),
        (b"Lavf60", b"\x00\x00\x00\x00\x00\x00"),
        (b"Lavf61", b"\x00\x00\x00\x00\x00\x00"),
        (b"Lavf", b"\x00\x00\x00\x00"),
        (b"libavformat", b"\x00" * 11),
        (b"libavcodec", b"\x00" * 10),
    ]
    scrubbed = 0
    for marker, replacement in ffmpeg_markers:
        idx = 0
        while True:
            idx = raw.find(marker, idx)
            if idx == -1:
                break
            raw[idx:idx + len(replacement)] = replacement
            scrubbed += 1
            idx += len(replacement)

    if scrubbed > 0:
        with open(filepath, "wb") as f:
            f.write(raw)
        log(f"    Scrubbed {scrubbed} ffmpeg marker(s)")
    else:
        log("    No ffmpeg markers found (clean)")

    log("    MOV patch complete")


# ============================================================
# AUDIO PROCESSING
# ============================================================


def _extract_audio(input_path, output_wav):
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
    with open(filepath, "rb") as f:
        if f.read(4) != b"RIFF":
            raise ValueError("not a WAV file")
        f.read(4)
        if f.read(4) != b"WAVE":
            raise ValueError("not a WAV file")

        channels = 2
        sample_rate = AUDIO_SAMPLE_RATE
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
    if data.ndim == 2:
        n_samples = data.shape[0]
    else:
        n_samples = len(data)
        channels = 1

    bps = 2
    data_size = n_samples * channels * bps

    with open(filepath, "wb") as f:
        f.write(b"RIFF")
        f.write(struct.pack("<I", 36 + data_size))
        f.write(b"WAVE")
        f.write(b"fmt ")
        f.write(struct.pack("<I", 16))
        f.write(struct.pack("<H", 1))
        f.write(struct.pack("<H", channels))
        f.write(struct.pack("<I", sample_rate))
        f.write(struct.pack("<I", sample_rate * channels * bps))
        f.write(struct.pack("<H", channels * bps))
        f.write(struct.pack("<H", 16))
        f.write(b"data")
        f.write(struct.pack("<I", data_size))
        f.write(data.astype(np.int16).tobytes())


def _constellation_poison_mono(samples, sr, rng):
    """Inject phantom frequency peaks near the strongest spectral peaks."""
    n = len(samples)
    spectrum = np.fft.rfft(samples)
    freqs = np.fft.rfftfreq(n, 1.0 / sr)

    n_peaks = max(10, n // sr * AUDIO_PHANTOM_PEAKS_MULT)
    peak_indices = np.argsort(np.abs(spectrum))[-n_peaks:]

    for idx in peak_indices:
        if idx < len(freqs):
            freq = freqs[idx]
            for target_f in [freq + AUDIO_PHANTOM_OFFSET_HZ,
                             freq - AUDIO_PHANTOM_OFFSET_HZ]:
                if 20 < target_f < sr / 2:
                    target_idx = int(target_f * n / sr)
                    if 0 < target_idx < len(spectrum):
                        phase = rng.uniform(0, 2 * np.pi)
                        spectrum[target_idx] += (
                            np.abs(spectrum[idx]) * AUDIO_PHANTOM_AMP
                            * np.exp(1j * phase)
                        )

    return np.fft.irfft(spectrum, n=n)


def process_audio(input_wav, output_wav, rng):
    """Constellation poisoning + speed change."""
    log("  Processing audio...")
    data, sr, channels = _read_wav(input_wav)

    fdata = data.astype(np.float64) / 32768.0

    if channels > 1 and fdata.ndim == 2:
        for ch in range(channels):
            ch_rng = np.random.default_rng(int(rng.integers(0, 2**31)) + ch)
            fdata[:, ch] = _constellation_poison_mono(fdata[:, ch], sr, ch_rng)
    else:
        fdata = _constellation_poison_mono(fdata.ravel(), sr, rng)

    if SPEED_FACTOR != 1.0:
        old_len = fdata.shape[0]
        new_len = int(old_len / SPEED_FACTOR)
        old_x = np.linspace(0, 1, old_len)
        new_x = np.linspace(0, 1, new_len)
        if fdata.ndim == 2:
            new_data = np.zeros((new_len, channels), dtype=np.float64)
            for ch in range(channels):
                new_data[:, ch] = np.interp(new_x, old_x, fdata[:, ch])
            fdata = new_data
        else:
            fdata = np.interp(new_x, old_x, fdata)

    fdata = np.clip(fdata, -1.0, 1.0)
    int_data = (fdata * 32767).astype(np.int16)
    _write_wav(output_wav, int_data, sr, channels)
    log("    Audio done")


# ============================================================
# VIDEO FRAME PROCESSOR
# ============================================================

class FrameProcessor:
    """
    Per-frame video processing with cv2-based spatial and frequency filters.

    Precomputes mesh warp displacement maps once. DCT and attention dilution
    use fixed-seed RNGs for determinism.
    """

    def __init__(self, width, height, rng):
        self.w = width
        self.h = height
        self._precompute_mesh(rng)
        self.dct_seed = int(rng.integers(0, 2**31))
        self.dilution_seed = int(rng.integers(0, 2**31))

    def _precompute_mesh(self, rng):
        h, w = self.h, self.w
        dx = rng.uniform(
            -MESH_AMP, MESH_AMP, (MESH_GRID + 1, MESH_GRID + 1)
        ).astype(np.float32)
        dy = rng.uniform(
            -MESH_AMP, MESH_AMP, (MESH_GRID + 1, MESH_GRID + 1)
        ).astype(np.float32)
        dx[0, :] = dx[-1, :] = dx[:, 0] = dx[:, -1] = 0
        dy[0, :] = dy[-1, :] = dy[:, 0] = dy[:, -1] = 0

        full_dx = cv2.resize(dx, (w, h), interpolation=cv2.INTER_LINEAR)
        full_dy = cv2.resize(dy, (w, h), interpolation=cv2.INTER_LINEAR)

        base_x = np.broadcast_to(
            np.arange(w, dtype=np.float32)[np.newaxis, :], (h, w)
        ).copy()
        base_y = np.broadcast_to(
            np.arange(h, dtype=np.float32)[:, np.newaxis], (h, w)
        ).copy()

        self.map_x = base_x + full_dx
        self.map_y = base_y + full_dy

    def process(self, frame, frame_idx):
        """
        Process a single BGR uint8 frame.

        1. Horizontal flip
        2. Mesh warp (precomputed displacement)
        3. DCT butterfly (mid-frequency perturbation)
        4. Attention dilution (texture in uniform areas)
        """
        # 1. HFLIP
        if HFLIP:
            frame = np.ascontiguousarray(frame[:, ::-1, :])

        # 2. Mesh warp
        frame = cv2.remap(
            frame, self.map_x, self.map_y,
            cv2.INTER_LINEAR, borderMode=cv2.BORDER_REFLECT_101,
        )

        # 3. DCT butterfly
        yuv = cv2.cvtColor(frame, cv2.COLOR_BGR2YUV)
        y_ch = yuv[:, :, 0].astype(np.float32)
        dft = np.fft.fft2(y_ch)
        fh, fw = y_ch.shape
        mid_h = slice(fh // 4, 3 * fh // 4)
        mid_w = slice(fw // 4, 3 * fw // 4)

        dct_rng = np.random.default_rng(self.dct_seed)
        perturbation = dct_rng.normal(0, DCT_STRENGTH, dft[mid_h, mid_w].shape)
        magnitude = np.abs(dft[mid_h, mid_w])
        median_mag = (
            float(np.median(magnitude[magnitude > 0]))
            if np.any(magnitude > 0) else 1.0
        )
        mask = (magnitude > median_mag * 0.5) & (magnitude < median_mag * 2.0)
        dft[mid_h, mid_w] += perturbation * mask * median_mag

        y_new = np.fft.ifft2(dft).real
        yuv[:, :, 0] = np.clip(y_new, 0, 255).astype(np.uint8)
        frame = cv2.cvtColor(yuv, cv2.COLOR_YUV2BGR)

        # 4. Attention dilution (vectorized)
        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        gh, gw = gray.shape
        block = DILUTION_BLOCK
        th = (gh // block) * block
        tw = (gw // block) * block

        if th > 0 and tw > 0:
            n_by = th // block
            n_bx = tw // block
            gray_blocks = (
                gray[:th, :tw]
                .reshape(n_by, block, n_bx, block)
                .transpose(0, 2, 1, 3)
            )
            stds = gray_blocks.astype(np.float32).std(axis=(2, 3))
            uniform_mask = stds < DILUTION_THRESHOLD

            if np.any(uniform_mask):
                dil_rng = np.random.default_rng(self.dilution_seed)
                noise = dil_rng.integers(
                    -DILUTION_AMP, DILUTION_AMP + 1,
                    (th, tw, 3), dtype=np.int16,
                )
                pixel_mask = np.repeat(
                    np.repeat(uniform_mask, block, axis=0),
                    block, axis=1,
                )
                region = frame[:th, :tw].astype(np.int16)
                region += noise * pixel_mask[:, :, np.newaxis]
                frame[:th, :tw] = np.clip(region, 0, 255).astype(np.uint8)

        return frame


# ============================================================
# MAIN PIPELINE
# ============================================================


def process_video(input_path, output_path):
    log(f"Processing: {input_path}")

    file_hash = sha256_file(input_path)
    rng = np.random.default_rng(FIXED_PROCESSING_SEED)
    log(f"  Hash: {file_hash[:16]}...  Seed: {FIXED_PROCESSING_SEED:#x} (fixed)")

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

    if rotation in (90, -90, 270, -270):
        display_w, display_h = coded_h, coded_w
    else:
        display_w, display_h = coded_w, coded_h

    display_w = display_w // 2 * 2
    display_h = display_h // 2 * 2

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

    creation_dt = make_creation_date(file_hash)
    apple_date = format_apple_date(creation_dt)
    utc_date = format_utc_date(creation_dt)
    log(f"  Date:    {apple_date}")

    temp_dir = os.path.join(os.path.dirname(os.path.abspath(output_path)),
                            ".uniqualizer_temp")
    os.makedirs(temp_dir, exist_ok=True)
    temp_audio_in = os.path.join(temp_dir, "audio_in.wav")
    temp_audio_out = os.path.join(temp_dir, "audio_out.wav")

    has_audio = a_stream is not None
    if has_audio:
        has_audio = _extract_audio(input_path, temp_audio_in)
        if has_audio:
            audio_rng = np.random.default_rng(FIXED_PROCESSING_SEED + 1)
            process_audio(temp_audio_in, temp_audio_out, audio_rng)

    # --- DECODE command ---
    decode_vf = []
    if rotation in (90, -270):
        decode_vf.append("transpose=2")
    elif rotation in (-90, 270):
        decode_vf.append("transpose=1")
    elif rotation in (180, -180):
        decode_vf.append("transpose=2,transpose=2")
    decode_vf.append(f"scale={display_w}:{display_h}")

    decode_cmd = [
        "ffmpeg", "-nostdin", "-noautorotate",
        "-i", input_path,
        "-vf", ",".join(decode_vf),
        "-f", "rawvideo", "-pix_fmt", "bgr24",
        "-v", "quiet",
        "pipe:1",
    ]

    # --- ENCODE command ---
    encode_cmd = [
        "ffmpeg", "-y", "-nostdin",
        "-f", "rawvideo",
        "-pix_fmt", "bgr24",
        "-s", f"{display_w}x{display_h}",
        "-r", f"{out_fps:.6f}",
        "-i", "pipe:0",
    ]
    if has_audio:
        encode_cmd.extend(["-i", temp_audio_out])

    encode_vf = "setparams=color_primaries=bt709:color_trc=bt709:colorspace=bt709:range=tv"

    encode_cmd.extend([
        "-c:v", "libx264",
        "-preset", PRESET,
        "-crf", str(CRF),
        "-profile:v", VIDEO_PROFILE,
        "-level:v", VIDEO_LEVEL,
        "-pix_fmt", "yuv420p",
        "-g", str(GOP_SIZE),
        "-bf", "2",
        "-vf", encode_vf,
        "-colorspace", "bt709",
        "-color_trc", "bt709",
        "-color_primaries", "bt709",
        "-color_range", "tv",
        "-tag:v", "avc1",
        "-flags:v", "+bitexact",
        "-flags:a", "+bitexact",
        "-fflags", "+bitexact",
        "-metadata:s:v:0", "handler_name=Core Media Video",
        "-metadata:s:v:0", "encoder=H.264",
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

    # --- Frame processor ---
    frame_proc = FrameProcessor(display_w, display_h, rng)

    # --- Pipeline ---
    log("  Starting pipe-based encode...")
    frame_size = display_w * display_h * 3

    decode_proc = subprocess.Popen(
        decode_cmd,
        stdout=subprocess.PIPE,
        stderr=subprocess.DEVNULL,
    )
    encode_proc = subprocess.Popen(
        encode_cmd,
        stdin=subprocess.PIPE,
        stdout=subprocess.DEVNULL,
        stderr=subprocess.PIPE,
    )

    enc_stderr_chunks = []

    def _drain_stderr():
        while True:
            chunk = encode_proc.stderr.read(4096)
            if not chunk:
                break
            enc_stderr_chunks.append(chunk)

    stderr_thread = threading.Thread(target=_drain_stderr, daemon=True)
    stderr_thread.start()

    progress_step = max(1, total_frames // 10)
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
            if frame_idx % progress_step == 0:
                pct = int(frame_idx / total_frames * 100)
                log(f"    Progress: {pct}%")

        encode_proc.stdin.close()
        encode_proc.wait(timeout=300)
        stderr_thread.join(timeout=10)
        decode_proc.wait(timeout=30)

    except Exception as exc:
        decode_proc.kill()
        encode_proc.kill()
        raise RuntimeError(f"Pipe error: {exc}") from exc

    log(f"  Encoded {frame_idx} frames")

    if encode_proc.returncode != 0:
        err_msg = b"".join(enc_stderr_chunks).decode("utf-8", errors="replace")[:500]
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
    in_size = os.path.getsize(input_path) / (1024 * 1024)
    log(f"  Done: {output_path}  ({in_size:.1f} MB -> {out_size:.1f} MB)")

    verify_metadata(output_path)
    return True


# ============================================================
# METADATA VERIFICATION
# ============================================================


def verify_metadata(filepath):
    """Comprehensive metadata check with indicators."""
    info = get_video_info(filepath)
    tags = info.get("format", {}).get("tags", {})

    log("")
    log("  " + "=" * 56)
    log(f"  ПРОВЕРКА МЕТАДАННЫХ: {os.path.basename(filepath)}")
    log("  " + "=" * 56)

    make = tags.get("com.apple.quicktime.make", "НЕТ")
    model = tags.get("com.apple.quicktime.model", "НЕТ")
    sw = tags.get("com.apple.quicktime.software", "НЕТ")
    log(f"    Производитель:     {make}")
    log(f"    Модель:            {model}")
    log(f"    iOS:               {sw}")

    cdate = tags.get("com.apple.quicktime.creationdate", "НЕТ")
    ctime = tags.get("creation_time", "НЕТ")
    log(f"    Дата записи:       {cdate}")
    log(f"    UTC:               {ctime}")

    brand = tags.get("major_brand", "НЕТ").strip()
    compat = tags.get("compatible_brands", "НЕТ").strip()
    ok_brand = "qt" in brand
    ok_compat = "qt" in compat
    log(f"    major_brand:       {brand}  {'OK' if ok_brand else 'FAIL'}")
    log(f"    compatible_brands: {compat}  {'OK' if ok_compat else 'FAIL'}")

    problems = []
    for s in info.get("streams", []):
        stags = s.get("tags", {})
        st = s.get("codec_type")

        if st == "video":
            log(f"    --- Видео ---")
            log(f"    Кодек:             {s.get('codec_name', '?')} {s.get('profile', '')}")
            log(f"    Разрешение:        {s.get('width', '?')}x{s.get('height', '?')}")
            log(f"    FPS:               {s.get('r_frame_rate', '?')}")
            log(f"    Пиксели:           {s.get('pix_fmt', '?')}")

            cs = s.get("color_space", "НЕТ")
            ct = s.get("color_transfer", "НЕТ")
            cp = s.get("color_primaries", "НЕТ")
            cr = s.get("color_range", "НЕТ")
            log(f"    color_space:       {cs}  {'OK' if cs == 'bt709' else 'FAIL'}")
            log(f"    color_transfer:    {ct}  {'OK' if ct == 'bt709' else 'FAIL'}")
            log(f"    color_primaries:   {cp}  {'OK' if cp == 'bt709' else 'FAIL'}")
            log(f"    color_range:       {cr}  {'OK' if cr == 'tv' else 'FAIL'}")

            hn = stags.get("handler_name", "НЕТ")
            vid = stags.get("vendor_id", "НЕТ")
            enc = stags.get("encoder", "НЕТ")
            ok_hn = "Core Media" in hn
            ok_vid = vid in ("[0][0][0][0]", "НЕТ")
            ok_enc = enc in ("H.264", "НЕТ")
            log(f"    handler_name:      {hn}  {'OK' if ok_hn else 'FAIL'}")
            log(f"    vendor_id:         {vid}  {'OK' if ok_vid else 'FAIL'}")
            log(f"    encoder:           {enc}  {'OK' if ok_enc else 'FAIL'}")

            if not ok_hn:
                problems.append(f"video handler={hn}")
            if not ok_vid:
                problems.append(f"vendor_id={vid}")
            if not ok_enc:
                problems.append(f"encoder={enc}")
            if ct != "bt709":
                problems.append(f"color_trc={ct or 'missing'}")

        elif st == "audio":
            log(f"    --- Аудио ---")
            log(f"    Кодек:             {s.get('codec_name', '?')} {s.get('profile', '')}")
            sr = s.get("sample_rate", "?")
            ok_sr = sr == "48000"
            log(f"    Sample rate:       {sr} Hz  {'OK' if ok_sr else 'FAIL'}")
            log(f"    Каналы:            {s.get('channels', '?')}")

            hn = stags.get("handler_name", "НЕТ")
            vid = stags.get("vendor_id", "НЕТ")
            ok_hn = "Core Media" in hn
            ok_vid = vid in ("[0][0][0][0]", "НЕТ")
            log(f"    handler_name:      {hn}  {'OK' if ok_hn else 'FAIL'}")
            log(f"    vendor_id:         {vid}  {'OK' if ok_vid else 'FAIL'}")

            if not ok_sr:
                problems.append(f"sample_rate={sr}")
            if not ok_hn:
                problems.append(f"audio handler={hn}")

    all_text = json.dumps(info)
    ffmpeg_markers = ["Lavf", "Lavc", "FFMP", "libav", "ffmpeg"]
    found = [m for m in ffmpeg_markers if m.lower() in all_text.lower()]
    if found:
        log(f"    Маркеры ffmpeg:    НАЙДЕНЫ: {', '.join(found)}  FAIL!")
        problems.append(f"ffmpeg markers: {', '.join(found)}")
    else:
        log(f"    Маркеры ffmpeg:    Не найдены  OK")

    log("")
    if problems:
        log(f"    РЕЗУЛЬТАТ: ПРОБЛЕМЫ НАЙДЕНЫ -- {', '.join(problems)}")
    else:
        log(f"    РЕЗУЛЬТАТ: ВСЕ ЧИСТО")
    log("  " + "=" * 56)


def main():
    log("TikTok Video Uniqualizer v7")
    log("=" * 60)
    log("Filters: HFLIP + mesh_warp + dct_butterfly + attention_dilution")
    log(f"  mesh_warp:     grid={MESH_GRID}  amp={MESH_AMP}")
    log(f"  dct_butterfly: strength={DCT_STRENGTH}")
    log(f"  attention:     amp={DILUTION_AMP}  threshold={DILUTION_THRESHOLD}")
    log(f"  audio poison:  offset={AUDIO_PHANTOM_OFFSET_HZ}Hz  amp={AUDIO_PHANTOM_AMP}")
    log(f"  speed:         {SPEED_FACTOR}x")
    log(f"  seed:          {FIXED_PROCESSING_SEED:#x} (fixed)")
    log("")

    check_dependencies()

    os.makedirs(INPUT_DIR, exist_ok=True)
    os.makedirs(OUTPUT_DIR, exist_ok=True)

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
