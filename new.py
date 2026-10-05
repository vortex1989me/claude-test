#!/usr/bin/env python3
"""
TikTok Video Uniqualizer v5 — Radical Anti-Detection Pipeline

Strategy: break EVERY content detection vector with structural transforms,
not subtle frequency tricks. Previous versions (v1-v4) used invisible
perturbations that TikTok's detectors ignored. v5 uses major structural
changes that are visually clean but mathematically devastating.

Detection vector          Counter-measure
------------------------  ------------------------------------------------
PDQ / pHash (spatial)     HFLIP + 0.7-deg rotation + 5% crop + 4% downscale
TMK (temporal matching)   2.5% speed change + shifted frame rate
Audio fingerprint         Speed + 10ms comb echo + EQ shift + AAC re-encode
CLIP embedding (semantic) HFLIP mirrors spatial semantics + crop reframes
Invisible watermark       Pre-wash yuv422p round-trip + crop + double encode
Metadata / container      Full metadata strip + random creation time

Dependencies: Python 3.8+, numpy, opencv-python-headless, ffmpeg (libx264+aac)
Hardware: CPU-only, tested on Ryzen 5 2600 / 8 GB RAM
"""

import os
import sys
import json
import hashlib
import random
import subprocess
import tempfile
import shutil
import glob
import time
import numpy as np
import cv2


# ====================================================================
#  CONFIGURATION — all tunables in one place
# ====================================================================

# --- Spatial transforms ---
HFLIP               = True      # Horizontal mirror flip (biggest hash breaker)
ROTATION_DEG        = 0.7       # Slight rotation to force full-frame interpolation
CROP_PERCENT        = 0.05      # Crop 5% from EACH edge (strips watermark bands)
SCALE_FACTOR        = 0.96      # Post-crop scale (breaks pixel grid alignment)

# --- Color transforms (static per-file, no flicker) ---
HUE_SHIFT           = 4.0       # Hue rotation in OpenCV HSV degrees (0-180)
SATURATION_SCALE    = 1.06      # Saturation multiplier
BRIGHTNESS_OFFSET   = 3         # Additive brightness shift
GAMMA               = 1.04      # Gamma correction (>1 = brighter midtones)
WARMTH              = 2         # Color temp shift (positive = warmer: +R -B)

# --- Temporal ---
SPEED_FACTOR        = 1.025     # Playback speed multiplier

# --- Texture ---
NOISE_SIGMA         = 1.5       # Gaussian noise sigma per pixel
VIGNETTE_STRENGTH   = 0.12      # Edge darkening intensity

# --- Encoding ---
CRF                 = 22        # x264 quality (18-28 reasonable range)
PRESET              = "slow"    # x264 preset (slower = smaller file)
PIX_FMT_OUT         = "yuv420p" # Output chroma format

# --- Pre-wash: watermark destruction via chroma round-trip ---
PRE_WASH_ENABLED    = True      # Re-encode through yuv422p before processing
PRE_WASH_CRF        = 20        # Quality for the pre-wash pass

# --- Audio ---
AUDIO_HIGHPASS_HZ   = 25        # Sub-bass cut (changes spectral fingerprint)
AUDIO_ECHO_DELAY_MS = 10        # Below perception threshold (acts as comb filter)
AUDIO_ECHO_DECAY    = 0.10      # Echo level (very quiet)
AUDIO_EQ_BASS_DB    = 2         # Bass shelf adjustment
AUDIO_EQ_TREBLE_DB  = -1        # Treble shelf adjustment
AUDIO_BITRATE       = "192k"    # Output AAC bitrate

# --- I/O ---
INPUT_DIR           = "input"
OUTPUT_DIR          = "output"
SUPPORTED_EXT       = {".mp4", ".MP4", ".mov", ".MOV", ".m4v", ".M4V"}


# ====================================================================
#  UTILITY FUNCTIONS
# ====================================================================

def sha256_file(path):
    """Compute full-file SHA-256 for deterministic seeding."""
    h = hashlib.sha256()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(131072), b""):
            h.update(chunk)
    return h.hexdigest()


def pipe_read(pipe, nbytes):
    """Read exactly nbytes from a pipe, handling partial reads at EOF."""
    parts = []
    remaining = nbytes
    while remaining > 0:
        chunk = pipe.read(remaining)
        if not chunk:
            break
        parts.append(chunk)
        remaining -= len(chunk)
    return b"".join(parts)


def probe_json(path):
    """Run ffprobe and return parsed JSON metadata."""
    cmd = [
        "ffprobe", "-v", "quiet",
        "-print_format", "json",
        "-show_format", "-show_streams",
        str(path),
    ]
    proc = subprocess.run(cmd, capture_output=True, text=True)
    if proc.returncode != 0:
        raise RuntimeError(f"ffprobe failed on {path}: {proc.stderr}")
    return json.loads(proc.stdout)


def get_rotation(data):
    """Extract display rotation angle from ffprobe data."""
    for s in data.get("streams", []):
        if s.get("codec_type") != "video":
            continue
        # Check side_data_list first (modern ffmpeg)
        for sd in s.get("side_data_list", []):
            if "rotation" in sd:
                return round(float(sd["rotation"]))
        # Fallback to stream-level attribute
        if "rotation" in s:
            return round(float(s["rotation"]))
        # Fallback to stream tags
        rotate_tag = s.get("tags", {}).get("rotate")
        if rotate_tag:
            return int(rotate_tag)
    return 0


def get_video_meta(data):
    """Extract essential video and audio metadata from ffprobe data."""
    meta = {
        "has_audio": False,
        "width": 0, "height": 0,
        "fps": 30.0, "duration": 0.0,
    }
    for s in data.get("streams", []):
        codec_type = s.get("codec_type")
        if codec_type == "video" and meta["width"] == 0:
            meta["width"] = int(s.get("coded_width") or s.get("width", 0))
            meta["height"] = int(s.get("coded_height") or s.get("height", 0))
            r_fps = s.get("r_frame_rate", "30/1")
            parts = r_fps.split("/")
            num = int(parts[0])
            den = int(parts[1]) if len(parts) > 1 and parts[1] != "0" else 1
            meta["fps"] = num / den if den != 0 else 30.0
        elif codec_type == "audio":
            meta["has_audio"] = True
    meta["duration"] = float(data.get("format", {}).get("duration", 0))
    return meta


def build_vignette_mask(h, w, strength):
    """Create a float32 vignette attenuation mask with smooth falloff."""
    ys = np.linspace(-1.0, 1.0, h, dtype=np.float32).reshape(-1, 1)
    xs = np.linspace(-1.0, 1.0, w, dtype=np.float32).reshape(1, -1)
    r = np.sqrt(xs * xs + ys * ys)
    mask = 1.0 - strength * np.clip((r - 0.6) * 1.5, 0.0, 1.0) ** 1.5
    return mask


def random_creation_time(seed):
    """Generate a plausible random creation timestamp."""
    rng = random.Random(seed + 777)
    base = 1727400000  # Late Sept 2024
    offset = rng.randint(0, 86400 * 90)
    return time.strftime("%Y-%m-%dT%H:%M:%S", time.gmtime(base - offset))


# ====================================================================
#  FRAME PROCESSING PIPELINE
# ====================================================================

def process_frame(frame, rot_matrix, crop_box, out_size, vig_mask, rng):
    """
    Full uniqualization pipeline for a single BGR frame.

    Pipeline: HFLIP -> rotation -> crop -> color -> resize -> vignette -> noise

    Each step targets specific detection vectors:
      HFLIP:     Inverts all spatial hashes, mirrors CLIP semantics
      Rotation:  Forces Lanczos interpolation of every pixel
      Crop:      Removes watermark border bands, changes composition
      Color:     Shifts hue/sat/gamma/warmth (breaks perceptual hashes)
      Resize:    New pixel grid (breaks frame-level hash alignment)
      Vignette:  Adds unique radial luminance signature
      Noise:     Deterministic per-frame noise (breaks pixel-exact matching)
    """
    h_in, w_in = frame.shape[:2]
    cy1, cy2, cx1, cx2 = crop_box
    w_out, h_out = out_size

    # 1. HFLIP — the single most effective spatial hash breaker
    if HFLIP:
        frame = cv2.flip(frame, 1)

    # 2. Slight rotation — every pixel gets Lanczos-interpolated
    if ROTATION_DEG != 0:
        frame = cv2.warpAffine(
            frame, rot_matrix, (w_in, h_in),
            flags=cv2.INTER_LANCZOS4,
            borderMode=cv2.BORDER_REFLECT_101,
        )

    # 3. Crop borders (5% each edge removes watermark bands)
    frame = frame[cy1:cy2, cx1:cx2]

    # 4. Color adjustments in HSV space
    hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV).astype(np.float32)
    # Hue rotation
    hsv[:, :, 0] = (hsv[:, :, 0] + HUE_SHIFT) % 180.0
    # Saturation boost
    hsv[:, :, 1] = np.clip(hsv[:, :, 1] * SATURATION_SCALE, 0.0, 255.0)
    # Value: gamma correction + brightness offset
    v = hsv[:, :, 2] / 255.0
    v = np.power(v, 1.0 / GAMMA)
    hsv[:, :, 2] = np.clip(v * 255.0 + BRIGHTNESS_OFFSET, 0.0, 255.0)
    frame = cv2.cvtColor(hsv.astype(np.uint8), cv2.COLOR_HSV2BGR)

    # 5. Color temperature shift (warm: +R -B)
    if WARMTH != 0:
        f = frame.astype(np.int16)
        f[:, :, 2] = np.clip(f[:, :, 2] + WARMTH, 0, 255)   # Red up
        f[:, :, 0] = np.clip(f[:, :, 0] - WARMTH, 0, 255)   # Blue down
        frame = f.astype(np.uint8)

    # 6. Resize to output dimensions (Lanczos for quality)
    frame = cv2.resize(frame, (w_out, h_out), interpolation=cv2.INTER_LANCZOS4)

    # 7. Vignette (subtle edge darkening)
    if VIGNETTE_STRENGTH > 0:
        frame = (frame.astype(np.float32) * vig_mask[:, :, np.newaxis]).astype(np.uint8)

    # 8. Deterministic Gaussian noise
    if NOISE_SIGMA > 0:
        noise = rng.normal(0.0, NOISE_SIGMA, frame.shape).astype(np.float32)
        frame = np.clip(frame.astype(np.float32) + noise, 0.0, 255.0).astype(np.uint8)

    return frame


# ====================================================================
#  PRE-WASH: watermark destruction via chroma round-trip
# ====================================================================

def pre_wash(source_path, output_path):
    """
    Re-encode through yuv422p to destroy invisible watermarks.

    TikTok embeds watermarks tuned to yuv420p subsampling. Encoding to
    yuv422p forces chroma upsampling, then our final encode back to
    yuv420p downsamples differently. This double chroma conversion
    scrambles any watermark pattern embedded in the chroma plane.

    Combined with the main pipeline's re-encode, this is effectively
    a triple-pass destruction: decode(original) -> encode(422p) ->
    decode(422p) -> process -> encode(420p).
    """
    cmd = [
        "ffmpeg", "-y", "-hide_banner", "-loglevel", "error",
        "-noautorotate",
        "-i", str(source_path),
        "-c:v", "libx264",
        "-crf", str(PRE_WASH_CRF),
        "-preset", "fast",
        "-pix_fmt", "yuv422p",
        "-c:a", "copy",
        str(output_path),
    ]
    subprocess.run(cmd, check=True)


# ====================================================================
#  AUDIO PROCESSING
# ====================================================================

def process_audio(source_path, output_path, speed):
    """
    Mutate audio to break fingerprint matching.

    Combined effect:
      - atempo: shifts timing of all spectral features (breaks Shazam-style matching)
      - highpass: removes sub-bass frequencies (changes spectral shape)
      - aecho at 10ms: creates comb-filter effect below echo perception threshold
                        (smears transient peaks that fingerprinters lock onto)
      - bass/treble EQ: alters overall spectral balance
      - AAC re-encode: different codec artifacts vs original
    """
    af_parts = []

    # Speed change (matches video speed adjustment)
    if speed != 1.0:
        af_parts.append(f"atempo={speed:.6f}")

    # Sub-bass cut
    af_parts.append(f"highpass=f={AUDIO_HIGHPASS_HZ}")

    # Comb-filter echo (below perception threshold, changes spectral peaks)
    af_parts.append(
        f"aecho=0.8:0.75:{AUDIO_ECHO_DELAY_MS}:{AUDIO_ECHO_DECAY}"
    )

    # EQ adjustments
    if AUDIO_EQ_BASS_DB != 0:
        af_parts.append(f"bass=g={AUDIO_EQ_BASS_DB}")
    if AUDIO_EQ_TREBLE_DB != 0:
        af_parts.append(f"treble=g={AUDIO_EQ_TREBLE_DB}")

    af = ",".join(af_parts)

    cmd = [
        "ffmpeg", "-y", "-hide_banner", "-loglevel", "error",
        "-i", str(source_path),
        "-vn",
        "-af", af,
        "-c:a", "aac",
        "-b:a", AUDIO_BITRATE,
        "-ar", "44100",
        "-ac", "2",
        str(output_path),
    ]
    subprocess.run(cmd, check=True)


# ====================================================================
#  FINAL MUX
# ====================================================================

def mux_av(video_path, audio_path, output_path, creation_time):
    """Combine processed video and audio, strip all metadata."""
    cmd = [
        "ffmpeg", "-y", "-hide_banner", "-loglevel", "error",
        "-i", str(video_path),
        "-i", str(audio_path),
        "-c:v", "copy",
        "-c:a", "copy",
        "-shortest",
        "-map_metadata", "-1",
        "-metadata", f"creation_time={creation_time}",
        "-movflags", "+faststart",
        str(output_path),
    ]
    subprocess.run(cmd, check=True)


# ====================================================================
#  OUTPUT NAMING
# ====================================================================

def generate_output_name(output_dir, seed):
    """Generate deterministic IMG_XXXX.mp4 filename, avoiding collisions."""
    existing = set()
    for f in glob.glob(os.path.join(output_dir, "IMG_*.mp4")):
        base = os.path.splitext(os.path.basename(f))[0]
        parts = base.split("_")
        if len(parts) >= 2:
            try:
                existing.add(int(parts[1]))
            except ValueError:
                pass

    num = (seed % 9000) + 1000
    attempt = 0
    while num in existing:
        attempt += 1
        num = ((seed + attempt * 7919) % 9000) + 1000
        if attempt > 200:
            num = random.randint(1000, 9999)
            break

    return f"IMG_{num:04d}.mp4"


# ====================================================================
#  MAIN PROCESSING PIPELINE
# ====================================================================

def process_file(input_path):
    """Full uniqualization pipeline for one video file."""
    fname = os.path.basename(input_path)
    print(f"\n{'='*60}")
    print(f"  INPUT: {fname}")
    print(f"{'='*60}")

    t_start = time.time()

    # ── Deterministic seed from full file content ──
    fhash = sha256_file(input_path)
    seed = int(fhash[:8], 16)
    random.seed(seed)
    np.random.seed(seed % (2**32))
    rng = np.random.RandomState(seed % (2**32))

    # ── Probe original file ──
    orig_data = probe_json(input_path)
    orig_rotation = get_rotation(orig_data)
    orig_meta = get_video_meta(orig_data)

    if orig_meta["width"] == 0 or orig_meta["height"] == 0:
        print("  ERROR: no video stream found, skipping")
        return None

    print(f"  Source: {orig_meta['width']}x{orig_meta['height']} "
          f"@ {orig_meta['fps']:.2f} fps, rotation={orig_rotation}, "
          f"audio={'yes' if orig_meta['has_audio'] else 'no'}, "
          f"duration={orig_meta['duration']:.1f}s")
    print(f"  Seed: {seed} (deterministic from file hash)")

    # ── Output filename ──
    script_dir = os.path.dirname(os.path.abspath(__file__))
    output_dir = os.path.join(script_dir, OUTPUT_DIR)
    os.makedirs(output_dir, exist_ok=True)
    out_name = generate_output_name(output_dir, seed)
    out_path = os.path.join(output_dir, out_name)

    # ── Temp directory ──
    tmp = tempfile.mkdtemp(prefix="uniqualizer_")

    try:
        # ────────────────────────────────────────────────────────────
        #  PHASE 1: Pre-wash (watermark destruction)
        # ────────────────────────────────────────────────────────────
        if PRE_WASH_ENABLED:
            print("  [1/5] Pre-wash: yuv422p round-trip to destroy watermarks...")
            washed_path = os.path.join(tmp, "washed.mp4")
            pre_wash(input_path, washed_path)
            work_path = washed_path
        else:
            work_path = input_path
            print("  [1/5] Pre-wash: disabled")

        # ────────────────────────────────────────────────────────────
        #  COMPUTE GEOMETRY
        # ────────────────────────────────────────────────────────────
        # Use original rotation value (pre-wash uses -noautorotate)
        rotation = orig_rotation
        w_stored = orig_meta["width"]
        h_stored = orig_meta["height"]

        # Display dimensions after rotation correction
        if abs(rotation) in (90, 270):
            w_disp, h_disp = h_stored, w_stored
        else:
            w_disp, h_disp = w_stored, h_stored

        # Crop box (y1, y2, x1, x2) — removes 5% from each edge
        cx = int(w_disp * CROP_PERCENT)
        cy = int(h_disp * CROP_PERCENT)
        crop_box = (cy, h_disp - cy, cx, w_disp - cx)
        w_crop = w_disp - 2 * cx
        h_crop = h_disp - 2 * cy

        # Output dimensions (ensure even for H.264)
        w_out = int(w_crop * SCALE_FACTOR) // 2 * 2
        h_out = int(h_crop * SCALE_FACTOR) // 2 * 2

        if w_out < 16 or h_out < 16:
            print("  ERROR: output dimensions too small after crop+scale")
            return None

        # Output frame rate (speed-adjusted)
        fps_out = orig_meta["fps"] * SPEED_FACTOR

        print(f"  Geometry: {w_disp}x{h_disp} -> crop 5% -> "
              f"{w_crop}x{h_crop} -> scale {SCALE_FACTOR} -> {w_out}x{h_out}")
        print(f"  FPS: {orig_meta['fps']:.2f} -> {fps_out:.2f} "
              f"(speed {SPEED_FACTOR}x)")

        # ── Precompute transform data ──
        center = (w_disp / 2.0, h_disp / 2.0)
        rot_matrix = cv2.getRotationMatrix2D(center, ROTATION_DEG, 1.0)
        vig_mask = build_vignette_mask(h_out, w_out, VIGNETTE_STRENGTH)
        out_size = (w_out, h_out)

        # ────────────────────────────────────────────────────────────
        #  PHASE 2: Decode -> process -> encode (pipe-based)
        # ────────────────────────────────────────────────────────────
        print("  [2/5] Processing video: hflip + rotate + crop + color + "
              "scale + vignette + noise...")

        # Build decoder command
        dec_cmd = ["ffmpeg", "-hide_banner", "-loglevel", "error"]
        if rotation != 0:
            dec_cmd.append("-noautorotate")
        dec_cmd.extend(["-i", str(work_path)])

        # Rotation filter chain
        vf_filters = []
        rot_mod = rotation % 360 if rotation >= 0 else -((-rotation) % 360)
        if rot_mod == 90 or rot_mod == -270:
            vf_filters.append("transpose=2")   # Counter-clockwise 90
        elif rot_mod == -90 or rot_mod == 270:
            vf_filters.append("transpose=1")   # Clockwise 90
        elif abs(rot_mod) == 180:
            vf_filters.append("transpose=1,transpose=1")

        if vf_filters:
            dec_cmd.extend(["-vf", ",".join(vf_filters)])

        dec_cmd.extend(["-f", "rawvideo", "-pix_fmt", "bgr24", "-an", "pipe:1"])

        # Build encoder command
        tmp_video = os.path.join(tmp, "video.mp4")
        enc_cmd = [
            "ffmpeg", "-y", "-hide_banner", "-loglevel", "error",
            "-f", "rawvideo",
            "-pixel_format", "bgr24",
            "-video_size", f"{w_out}x{h_out}",
            "-framerate", f"{fps_out:.4f}",
            "-i", "pipe:0",
            "-c:v", "libx264",
            "-preset", PRESET,
            "-crf", str(CRF),
            "-pix_fmt", PIX_FMT_OUT,
            "-movflags", "+faststart",
            "-an",
            str(tmp_video),
        ]

        # Start pipe processes
        decoder = subprocess.Popen(
            dec_cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE,
        )
        encoder = subprocess.Popen(
            enc_cmd, stdin=subprocess.PIPE, stderr=subprocess.PIPE,
        )

        frame_bytes = w_disp * h_disp * 3
        n_frames = 0
        total_est = int(orig_meta["duration"] * orig_meta["fps"]) if orig_meta["duration"] > 0 else 0

        try:
            while True:
                raw = pipe_read(decoder.stdout, frame_bytes)
                if len(raw) < frame_bytes:
                    break

                frame = np.frombuffer(raw, dtype=np.uint8).reshape(
                    (h_disp, w_disp, 3)
                ).copy()

                out_frame = process_frame(
                    frame, rot_matrix, crop_box, out_size, vig_mask, rng,
                )

                encoder.stdin.write(out_frame.tobytes())
                n_frames += 1

                # Progress indicator
                if n_frames % 200 == 0:
                    elapsed = time.time() - t_start
                    proc_fps = n_frames / max(elapsed, 0.001)
                    if total_est > 0:
                        pct = min(99, int(n_frames / total_est * 100))
                        sys.stdout.write(
                            f"\r  Frames: {n_frames}/{total_est} "
                            f"({pct}%, {proc_fps:.1f} fps)"
                        )
                    else:
                        sys.stdout.write(
                            f"\r  Frames: {n_frames} ({proc_fps:.1f} fps)"
                        )
                    sys.stdout.flush()

        except BrokenPipeError:
            pass
        finally:
            try:
                encoder.stdin.close()
            except Exception:
                pass
            decoder.stdout.close()

        decoder.wait()
        encoder.wait()
        enc_stderr = encoder.stderr.read()
        encoder.stderr.close()
        decoder.stderr.close()

        if encoder.returncode != 0:
            print(f"\n  ENCODER ERROR: {enc_stderr.decode(errors='replace')}")
            return None

        elapsed_v = time.time() - t_start
        avg_fps = n_frames / max(elapsed_v, 0.001)
        print(f"\r  Frames: {n_frames} processed ({avg_fps:.1f} fps)       ")

        # ────────────────────────────────────────────────────────────
        #  PHASE 3: Audio processing
        # ────────────────────────────────────────────────────────────
        tmp_audio = None
        if orig_meta["has_audio"]:
            print("  [3/5] Audio: speed + highpass + comb-echo + EQ + AAC re-encode...")
            tmp_audio = os.path.join(tmp, "audio.m4a")
            try:
                process_audio(work_path, tmp_audio, SPEED_FACTOR)
            except subprocess.CalledProcessError as e:
                print(f"  WARNING: audio processing failed ({e}), trying without EQ...")
                # Fallback: simpler audio chain without bass/treble filters
                try:
                    fallback_af = f"atempo={SPEED_FACTOR:.6f},highpass=f={AUDIO_HIGHPASS_HZ}"
                    cmd = [
                        "ffmpeg", "-y", "-hide_banner", "-loglevel", "error",
                        "-i", str(work_path), "-vn",
                        "-af", fallback_af,
                        "-c:a", "aac", "-b:a", AUDIO_BITRATE,
                        "-ar", "44100", "-ac", "2",
                        str(tmp_audio),
                    ]
                    subprocess.run(cmd, check=True)
                except subprocess.CalledProcessError:
                    print("  WARNING: audio processing failed completely, proceeding without audio")
                    tmp_audio = None
        else:
            print("  [3/5] No audio track — skipped")

        # ────────────────────────────────────────────────────────────
        #  PHASE 4: Mux video + audio
        # ────────────────────────────────────────────────────────────
        print("  [4/5] Muxing final output (metadata stripped)...")
        creation_time = random_creation_time(seed)

        if tmp_audio and os.path.exists(tmp_audio):
            mux_av(tmp_video, tmp_audio, out_path, creation_time)
        else:
            # Video-only output
            cmd = [
                "ffmpeg", "-y", "-hide_banner", "-loglevel", "error",
                "-i", str(tmp_video),
                "-c:v", "copy",
                "-an",
                "-map_metadata", "-1",
                "-metadata", f"creation_time={creation_time}",
                "-movflags", "+faststart",
                str(out_path),
            ]
            subprocess.run(cmd, check=True)

        # ────────────────────────────────────────────────────────────
        #  PHASE 5: Done
        # ────────────────────────────────────────────────────────────
        elapsed_total = time.time() - t_start
        orig_size = os.path.getsize(input_path) / (1024 * 1024)
        out_size_mb = os.path.getsize(out_path) / (1024 * 1024)

        print(f"  [5/5] Complete: {out_name}")
        print(f"         {n_frames} frames, {w_out}x{h_out} @ {fps_out:.2f} fps")
        print(f"         Size: {orig_size:.1f} MB -> {out_size_mb:.1f} MB")
        print(f"         Time: {elapsed_total:.1f}s ({avg_fps:.1f} fps avg)")

        return out_path

    except Exception as e:
        print(f"  ERROR: {e}")
        import traceback
        traceback.print_exc()
        return None

    finally:
        shutil.rmtree(tmp, ignore_errors=True)


# ====================================================================
#  ENTRY POINT
# ====================================================================

def main():
    # Verify ffmpeg is available
    try:
        subprocess.run(
            ["ffmpeg", "-version"],
            capture_output=True, check=True,
        )
    except FileNotFoundError:
        print("ERROR: ffmpeg not found in PATH. Install ffmpeg first.")
        sys.exit(1)

    # Resolve paths relative to script location
    script_dir = os.path.dirname(os.path.abspath(__file__))
    input_dir = os.path.join(script_dir, INPUT_DIR)
    output_dir = os.path.join(script_dir, OUTPUT_DIR)

    os.makedirs(input_dir, exist_ok=True)
    os.makedirs(output_dir, exist_ok=True)

    # Find input files
    inputs = []
    for f in sorted(os.listdir(input_dir)):
        ext = os.path.splitext(f)[1]
        if ext in SUPPORTED_EXT:
            inputs.append(os.path.join(input_dir, f))

    if not inputs:
        print(f"No video files found in {input_dir}/")
        print("Place .mp4 or .mov files in the input/ directory and run again.")
        sys.exit(1)

    # Print banner
    print("=" * 60)
    print("  Video Uniqualizer v5 — Radical Anti-Detection")
    print("=" * 60)
    print(f"  Files: {len(inputs)} video(s) in {INPUT_DIR}/")
    print(f"  Spatial: hflip={HFLIP}, rotation={ROTATION_DEG}deg, "
          f"crop={CROP_PERCENT*100:.0f}%, scale={SCALE_FACTOR}")
    print(f"  Color: hue=+{HUE_SHIFT}, sat=x{SATURATION_SCALE}, "
          f"gamma={GAMMA}, warmth=+{WARMTH}")
    print(f"  Temporal: speed={SPEED_FACTOR}x")
    print(f"  Texture: noise={NOISE_SIGMA}, vignette={VIGNETTE_STRENGTH}")
    print(f"  Audio: highpass={AUDIO_HIGHPASS_HZ}Hz, echo={AUDIO_ECHO_DELAY_MS}ms, "
          f"bass=+{AUDIO_EQ_BASS_DB}dB, treble={AUDIO_EQ_TREBLE_DB}dB")
    print(f"  Pre-wash: {'yuv422p @ CRF ' + str(PRE_WASH_CRF) if PRE_WASH_ENABLED else 'disabled'}")
    print(f"  Encode: x264 CRF={CRF}, preset={PRESET}, pix_fmt={PIX_FMT_OUT}")

    # Process each file
    results = []
    for i, path in enumerate(inputs, 1):
        print(f"\n  [{i}/{len(inputs)}]", end="")
        result = process_file(path)
        results.append((path, result))

    # Final summary
    print(f"\n{'='*60}")
    ok = sum(1 for _, r in results if r is not None)
    fail = sum(1 for _, r in results if r is None)
    print(f"  SUMMARY: {ok} succeeded, {fail} failed")
    for inp, out in results:
        inp_name = os.path.basename(inp)
        out_name = os.path.basename(out) if out else "FAILED"
        print(f"    {inp_name} -> {out_name}")
    print(f"  Output: {output_dir}/")
    print("=" * 60)


if __name__ == "__main__":
    main()
