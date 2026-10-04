#!/usr/bin/env python3
"""
TikTok Video Uniqualizer v4 — invisible to humans, unique for AI.
Deterministic: same input = same output (seed from file hash).

Strategy: heavy frequency-domain attacks (invisible) + gentle spatial (below JND).
Fixes v3: removed per-frame flicker from color/gamma, removed visible perspective/gradient.
"""

import os
import sys
import subprocess
import random
import json
import wave
import math
import time
import hashlib
from pathlib import Path

import numpy as np
import cv2

# ─── Config ───────────────────────────────────────────────────────────
SPEED = 1.015
PITCH_FACTOR = 1.015
CRF = 21
PRESET = "medium"
CROP_PX = 4
MESH_GRID = 8
MESH_AMP = 2.5
DCT_STRENGTH = 0.35
LUMA_NOISE = 2
CHROMA_NOISE_AMP = 12
DILUTION_AMP = 3
DILUTION_THRESHOLD = 12
COLOR_SHIFT_DEG = 2
GAMMA_SHIFT_VAL = 0.03
ZOOM_DRIFT_MAX = 0.008
BLEND_ALPHA = 0.03
SUBPIXEL_SHIFT = 0.4
FRAME_SWAP_INTERVAL = 25
TEMPORAL_JITTER_INTERVAL = 90
AUDIO_PHANTOM_OFFSET_HZ = 75
AUDIO_PHANTOM_AMP = 0.85
AUDIO_WOBBLE_HZ = 3.5
AUDIO_WOBBLE_DEPTH = 0.05
AUDIO_WARP_SEGMENTS = 20
AUDIO_WARP_RANGE = (0.97, 1.04)
STEREO_ROTATION_DEG = 10
INPUT_DIR = "input"
OUTPUT_DIR = "output"
SUPPORTED_EXT = {".mp4", ".mov", ".MP4", ".MOV"}


def compute_file_seed(path):
    h = hashlib.sha256()
    with open(path, "rb") as f:
        h.update(f.read(1024 * 1024))
        f.seek(0, 2)
        h.update(str(f.tell()).encode())
    h.update(os.path.basename(path).encode())
    return int.from_bytes(h.digest()[:4], "big")


def get_video_info(path):
    cmd = [
        "ffprobe", "-v", "quiet", "-print_format", "json",
        "-show_format", "-show_streams", str(path)
    ]
    r = subprocess.run(cmd, capture_output=True, text=True)
    data = json.loads(r.stdout)

    vstream = None
    astream = None
    for s in data.get("streams", []):
        if s["codec_type"] == "video" and vstream is None:
            vstream = s
        elif s["codec_type"] == "audio" and astream is None:
            astream = s

    rotation = 0
    if vstream:
        rotation = int(vstream.get("rotation", 0))
        if rotation == 0:
            tags = vstream.get("tags", {})
            rotation = int(tags.get("rotate", "0"))
        if "side_data_list" in vstream:
            for sd in vstream["side_data_list"]:
                if sd.get("side_data_type") == "Display Matrix" and "rotation" in sd:
                    rotation = int(sd["rotation"])

    w = int(vstream["coded_width"] or vstream["width"])
    h = int(vstream["coded_height"] or vstream["height"])

    fps_str = vstream.get("r_frame_rate", "30/1")
    if "/" in fps_str:
        num, den = fps_str.split("/")
        fps = float(num) / float(den) if float(den) != 0 else 30.0
    else:
        fps = float(fps_str)

    duration = float(data.get("format", {}).get("duration", 0))
    has_audio = astream is not None
    audio_sr = int(float(astream.get("sample_rate", 44100))) if astream else 44100
    audio_ch = int(astream.get("channels", 2)) if astream else 2

    return {
        "width": w, "height": h, "fps": fps, "duration": duration,
        "rotation": rotation, "has_audio": has_audio,
        "audio_sr": audio_sr, "audio_ch": audio_ch
    }


def build_transpose_filter(rotation):
    r = rotation % 360
    if r == 90 or r == -270:
        return "transpose=2"
    elif r == -90 or r == 270:
        return "transpose=1"
    elif r == 180 or r == -180:
        return "transpose=1,transpose=1"
    return None


def get_display_dims(w, h, rotation):
    r = abs(rotation) % 360
    if r == 90 or r == 270:
        return h, w
    return w, h


# ─── Frame Processing ─────────────────────────────────────────────────

def mesh_warp(frame):
    h, w = frame.shape[:2]
    grid = MESH_GRID

    dx = np.random.uniform(-MESH_AMP, MESH_AMP, (grid + 1, grid + 1)).astype(np.float32)
    dy = np.random.uniform(-MESH_AMP, MESH_AMP, (grid + 1, grid + 1)).astype(np.float32)
    dx[0, :] = dx[-1, :] = dx[:, 0] = dx[:, -1] = 0
    dy[0, :] = dy[-1, :] = dy[:, 0] = dy[:, -1] = 0

    full_dx = cv2.resize(dx, (w, h), interpolation=cv2.INTER_LINEAR)
    full_dy = cv2.resize(dy, (w, h), interpolation=cv2.INTER_LINEAR)

    base_x = np.broadcast_to(np.arange(w, dtype=np.float32)[np.newaxis, :], (h, w)).copy()
    base_y = np.broadcast_to(np.arange(h, dtype=np.float32)[:, np.newaxis], (h, w)).copy()

    return cv2.remap(frame, base_x + full_dx, base_y + full_dy,
                     cv2.INTER_LINEAR, borderMode=cv2.BORDER_REFLECT_101)


def subpixel_translate(frame):
    h, w = frame.shape[:2]
    tx = np.random.uniform(-SUBPIXEL_SHIFT, SUBPIXEL_SHIFT)
    ty = np.random.uniform(-SUBPIXEL_SHIFT, SUBPIXEL_SHIFT)
    M = np.float32([[1, 0, tx], [0, 1, ty]])
    return cv2.warpAffine(frame, M, (w, h), borderMode=cv2.BORDER_REFLECT_101)


def dct_butterfly(frame):
    yuv = cv2.cvtColor(frame, cv2.COLOR_BGR2YUV)
    y_ch = yuv[:, :, 0].astype(np.float32)

    dft = np.fft.fft2(y_ch)
    h, w = y_ch.shape

    for band_h, band_w in [(slice(h//6, 5*h//6), slice(w//6, 5*w//6)),
                            (slice(h//3, 2*h//3), slice(w//3, 2*w//3))]:
        perturbation = np.random.normal(0, DCT_STRENGTH, dft[band_h, band_w].shape)
        magnitude = np.abs(dft[band_h, band_w])
        median_mag = np.median(magnitude[magnitude > 0]) if np.any(magnitude > 0) else 1.0
        mask = (magnitude > median_mag * 0.2) & (magnitude < median_mag * 5.0)
        dft[band_h, band_w] += perturbation * mask * median_mag

    phase = np.angle(dft)
    mid_h = slice(h // 4, 3 * h // 4)
    mid_w = slice(w // 4, 3 * w // 4)
    phase_rot = np.random.uniform(-0.15, 0.15, phase[mid_h, mid_w].shape)
    dft[mid_h, mid_w] = np.abs(dft[mid_h, mid_w]) * np.exp(1j * (phase[mid_h, mid_w] + phase_rot))

    y_new = np.clip(np.fft.ifft2(dft).real, 0, 255).astype(np.uint8)
    yuv[:, :, 0] = y_new
    return cv2.cvtColor(yuv, cv2.COLOR_YUV2BGR)


def apply_static_color_gamma(frame, hue_shift, sat_factor, gamma_inv_table):
    hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV).astype(np.int16)
    hsv[:, :, 0] = (hsv[:, :, 0] + hue_shift) % 180
    hsv[:, :, 1] = np.clip(hsv[:, :, 1] * sat_factor, 0, 255)
    frame = cv2.cvtColor(hsv.astype(np.uint8), cv2.COLOR_HSV2BGR)
    frame = cv2.LUT(frame, gamma_inv_table)
    return frame


def chroma_noise(frame):
    yuv = cv2.cvtColor(frame, cv2.COLOR_BGR2YUV)
    noise_u = np.random.randint(-CHROMA_NOISE_AMP, CHROMA_NOISE_AMP + 1,
                                yuv[:, :, 1].shape, dtype=np.int16)
    noise_v = np.random.randint(-CHROMA_NOISE_AMP, CHROMA_NOISE_AMP + 1,
                                yuv[:, :, 2].shape, dtype=np.int16)
    yuv[:, :, 1] = np.clip(yuv[:, :, 1].astype(np.int16) + noise_u, 0, 255).astype(np.uint8)
    yuv[:, :, 2] = np.clip(yuv[:, :, 2].astype(np.int16) + noise_v, 0, 255).astype(np.uint8)
    return cv2.cvtColor(yuv, cv2.COLOR_YUV2BGR)


def luma_noise(frame):
    noise = np.random.randint(-LUMA_NOISE, LUMA_NOISE + 1,
                              frame.shape, dtype=np.int16)
    return np.clip(frame.astype(np.int16) + noise, 0, 255).astype(np.uint8)


def zoom_drift(frame, frame_idx, total_frames):
    if total_frames <= 0:
        total_frames = 300
    t = frame_idx / total_frames
    zoom = 1.0 + ZOOM_DRIFT_MAX * math.sin(2 * math.pi * t * 1.3)

    h, w = frame.shape[:2]
    cx, cy = w / 2, h / 2
    new_w, new_h = int(w / zoom), int(h / zoom)
    x1 = max(0, min(int(cx - new_w / 2), w - new_w))
    y1 = max(0, min(int(cy - new_h / 2), h - new_h))

    cropped = frame[y1:y1 + new_h, x1:x1 + new_w]
    return cv2.resize(cropped, (w, h), interpolation=cv2.INTER_LINEAR)


def attention_dilution(frame):
    gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    h, w = gray.shape
    block = 16
    result = frame.copy()

    for by in range(0, h - block, block):
        for bx in range(0, w - block, block):
            patch = gray[by:by + block, bx:bx + block]
            if np.std(patch) < DILUTION_THRESHOLD:
                texture = np.random.randint(
                    -DILUTION_AMP, DILUTION_AMP + 1,
                    (block, block, 3), dtype=np.int16
                )
                blk = result[by:by + block, bx:bx + block].astype(np.int16) + texture
                result[by:by + block, bx:bx + block] = np.clip(blk, 0, 255).astype(np.uint8)

    return result


class FrameProcessor:
    def __init__(self, total_frames, seed):
        self.total_frames = total_frames
        self.prev_frame = None
        self.frame_buffer = None
        self.prev_for_jitter = None

        rng = random.Random(seed + 100)
        self.hue_shift = rng.randint(-COLOR_SHIFT_DEG, COLOR_SHIFT_DEG)
        self.sat_factor = rng.uniform(0.97, 1.03)
        gamma = 1.0 + rng.uniform(-GAMMA_SHIFT_VAL, GAMMA_SHIFT_VAL)
        inv_gamma = 1.0 / gamma
        self.gamma_table = np.array([
            np.clip(((i / 255.0) ** inv_gamma) * 255.0, 0, 255)
            for i in range(256)
        ], dtype=np.uint8)

    def process(self, frame, frame_idx):
        frame = mesh_warp(frame)
        frame = subpixel_translate(frame)
        frame = dct_butterfly(frame)
        frame = apply_static_color_gamma(frame, self.hue_shift, self.sat_factor, self.gamma_table)
        frame = chroma_noise(frame)
        frame = luma_noise(frame)
        frame = zoom_drift(frame, frame_idx, self.total_frames)
        frame = attention_dilution(frame)

        if self.prev_frame is not None and self.prev_frame.shape == frame.shape:
            frame = cv2.addWeighted(frame, 1.0 - BLEND_ALPHA, self.prev_frame, BLEND_ALPHA, 0)

        self.prev_frame = frame.copy()

        if frame_idx > 0 and frame_idx % TEMPORAL_JITTER_INTERVAL == 0:
            if self.prev_for_jitter is not None:
                dup = self.prev_for_jitter
                self.prev_for_jitter = frame.copy()
                return ("jitter", dup, frame)
            self.prev_for_jitter = frame.copy()
            return frame

        self.prev_for_jitter = frame.copy()

        if frame_idx % FRAME_SWAP_INTERVAL == 0 and self.frame_buffer is not None:
            out = self.frame_buffer
            self.frame_buffer = frame
            return out
        elif frame_idx % FRAME_SWAP_INTERVAL == 0:
            self.frame_buffer = frame
            return None
        elif self.frame_buffer is not None:
            buffered = self.frame_buffer
            self.frame_buffer = None
            return ("pair", buffered, frame)
        else:
            return frame


# ─── Audio Processing ─────────────────────────────────────────────────

def load_wav(path):
    with wave.open(str(path), "rb") as wf:
        params = wf.getparams()
        raw = wf.readframes(params.nframes)
    if params.sampwidth == 2:
        samples = np.frombuffer(raw, dtype=np.int16).astype(np.float64)
    elif params.sampwidth == 4:
        samples = np.frombuffer(raw, dtype=np.int32).astype(np.float64)
    else:
        samples = np.frombuffer(raw, dtype=np.uint8).astype(np.float64) - 128.0
    if params.nchannels > 1:
        samples = samples.reshape(-1, params.nchannels)
    return samples, params


def save_wav(path, samples, params):
    flat = samples.flatten() if samples.ndim > 1 else samples
    if params.sampwidth == 2:
        flat = np.clip(flat, -32768, 32767).astype(np.int16)
    elif params.sampwidth == 4:
        flat = np.clip(flat, -2147483648, 2147483647).astype(np.int32)
    else:
        flat = np.clip(flat + 128, 0, 255).astype(np.uint8)
    with wave.open(str(path), "wb") as wf:
        wf.setparams(params)
        wf.writeframes(flat.tobytes())


def nonuniform_time_warp(audio, sr):
    n = len(audio)
    seg_len = n // AUDIO_WARP_SEGMENTS
    if seg_len < 100:
        return audio

    segments = []
    for i in range(AUDIO_WARP_SEGMENTS):
        start = i * seg_len
        end = start + seg_len if i < AUDIO_WARP_SEGMENTS - 1 else n
        seg = audio[start:end]

        factor = np.random.uniform(AUDIO_WARP_RANGE[0], AUDIO_WARP_RANGE[1])
        new_len = max(10, int(len(seg) / factor))
        x_old = np.linspace(0, 1, len(seg))
        x_new = np.linspace(0, 1, new_len)
        segments.append(np.interp(x_new, x_old, seg))

    return np.concatenate(segments)


def stft_constellation_poison(audio, sr):
    win_size = 2048
    hop = 512
    n = len(audio)
    result = audio.copy()

    for start in range(0, n - win_size, hop):
        window = result[start:start + win_size].copy()
        spectrum = np.fft.rfft(window)
        magnitudes = np.abs(spectrum)
        top_k = min(10, len(magnitudes))
        peak_indices = np.argsort(magnitudes)[-top_k:]

        for idx in peak_indices:
            freq = idx * sr / win_size
            for offset in [AUDIO_PHANTOM_OFFSET_HZ, -AUDIO_PHANTOM_OFFSET_HZ,
                           AUDIO_PHANTOM_OFFSET_HZ * 2, -AUDIO_PHANTOM_OFFSET_HZ * 2]:
                target_f = freq + offset
                if 20 < target_f < sr / 2:
                    target_idx = int(target_f * win_size / sr)
                    if 0 < target_idx < len(spectrum):
                        phase = np.random.uniform(0, 2 * np.pi)
                        spectrum[target_idx] += (
                            magnitudes[idx] * AUDIO_PHANTOM_AMP * np.exp(1j * phase)
                        )

        poisoned = np.fft.irfft(spectrum, n=win_size)
        hann = np.hanning(win_size)
        result[start:start + win_size] = (
            result[start:start + win_size] * (1 - hann) + poisoned * hann
        )

    return result


def amplitude_wobble(audio, sr):
    n = len(audio)
    t = np.arange(n, dtype=np.float64) / sr
    wobble = 1.0 + AUDIO_WOBBLE_DEPTH * np.sin(2 * np.pi * AUDIO_WOBBLE_HZ * t)
    return audio * wobble


def stereo_rotation(left, right, sr):
    angle_rad = STEREO_ROTATION_DEG * np.pi / 180
    n = len(left)
    t = np.linspace(0, angle_rad, n)
    cos_t = np.cos(t)
    sin_t = np.sin(t)
    return left * cos_t - right * sin_t, left * sin_t + right * cos_t


def process_audio_mono(audio, sr):
    audio = nonuniform_time_warp(audio, sr)
    audio = stft_constellation_poison(audio, sr)
    audio = amplitude_wobble(audio, sr)
    return audio


def extract_audio(video_path, wav_path, sr, channels):
    cmd = [
        "ffmpeg", "-y", "-i", str(video_path),
        "-vn", "-acodec", "pcm_s16le",
        "-ar", str(sr), "-ac", str(channels),
        str(wav_path)
    ]
    return subprocess.run(cmd, capture_output=True, text=True).returncode == 0


def process_audio(video_path, output_wav, info):
    sr = info["audio_sr"]
    ch = info["audio_ch"]
    tmp_wav = str(output_wav) + ".tmp.wav"

    if not extract_audio(video_path, tmp_wav, sr, ch):
        return False

    samples, params = load_wav(tmp_wav)

    if samples.ndim == 1:
        samples = process_audio_mono(samples, sr)
    else:
        col0 = process_audio_mono(samples[:, 0], sr)
        col1 = process_audio_mono(samples[:, 1], sr)
        min_len = min(len(col0), len(col1))
        col0, col1 = col0[:min_len], col1[:min_len]
        col0, col1 = stereo_rotation(col0, col1, sr)
        samples = np.column_stack([col0, col1])

    new_params = wave._wave_params(
        params.nchannels, params.sampwidth, params.framerate,
        len(samples) if samples.ndim == 1 else samples.shape[0],
        params.comptype, params.compname
    )
    save_wav(str(output_wav), samples, new_params)

    try:
        os.remove(tmp_wav)
    except OSError:
        pass
    return True


# ─── Output Naming ────────────────────────────────────────────────────

def generate_output_name(output_dir, seed):
    rng = random.Random(seed)
    existing = set()
    for f in Path(output_dir).glob("IMG_*.mp4"):
        try:
            num = int(f.stem.split("_")[1])
            existing.add(num)
        except (ValueError, IndexError):
            pass
    while True:
        num = rng.randint(1000, 9999)
        if num not in existing:
            return f"IMG_{num:04d}.mp4"


def random_creation_time(seed):
    rng = random.Random(seed + 1)
    base = 1727400000
    offset = rng.randint(0, 86400 * 60)
    return time.strftime("%Y-%m-%dT%H:%M:%S", time.gmtime(base - offset))


# ─── Main Pipeline ────────────────────────────────────────────────────

def process_video(input_path, output_dir, info):
    seed = compute_file_seed(input_path)
    random.seed(seed)
    np.random.seed(seed % (2**31))
    print(f"  Seed: {seed} (deterministic)")

    output_name = generate_output_name(output_dir, seed)
    output_path = os.path.join(output_dir, output_name)

    w = info["width"]
    h = info["height"]
    fps = info["fps"]
    rotation = info["rotation"]

    disp_w, disp_h = get_display_dims(w, h, rotation)
    transpose = build_transpose_filter(rotation)

    dec_filters = []
    if transpose:
        dec_filters.append(transpose)

    dec_cmd = ["ffmpeg", "-nostdin", "-noautorotate", "-i", str(input_path)]
    if dec_filters:
        dec_cmd += ["-vf", ",".join(dec_filters)]
    dec_cmd += ["-f", "rawvideo", "-pix_fmt", "bgr24", "-v", "error", "pipe:1"]

    frame_w = disp_w
    frame_h = disp_h
    frame_size = frame_w * frame_h * 3
    crop_w = frame_w - 2 * CROP_PX
    crop_h = frame_h - 2 * CROP_PX

    audio_wav = None
    if info["has_audio"]:
        audio_wav = os.path.join(output_dir, f".tmp_audio_{output_name}.wav")
        print(f"  Audio: non-uniform warp + STFT poison + stereo rotation + wobble")
        if not process_audio(input_path, audio_wav, info):
            print(f"  Warning: audio processing failed")
            audio_wav = None

    enc_vf = [
        f"crop={crop_w}:{crop_h}:{CROP_PX}:{CROP_PX}",
        f"scale={crop_w}:{crop_h}",
        f"setpts=PTS/{SPEED}",
    ]

    new_fps = fps * SPEED
    audio_rate = int(info["audio_sr"] * PITCH_FACTOR)

    enc_cmd = [
        "ffmpeg", "-nostdin", "-y",
        "-f", "rawvideo", "-pix_fmt", "bgr24",
        "-s", f"{frame_w}x{frame_h}", "-r", f"{fps}",
        "-i", "pipe:0",
    ]
    if audio_wav:
        enc_cmd += ["-i", audio_wav]

    enc_cmd += ["-vf", ",".join(enc_vf)]

    if audio_wav:
        enc_cmd += ["-af", f"asetrate={audio_rate},aresample={info['audio_sr']}"]

    creation_time = random_creation_time(seed)
    enc_cmd += [
        "-c:v", "libx264", "-crf", str(CRF),
        "-preset", PRESET, "-profile:v", "high", "-pix_fmt", "yuv420p",
        "-r", f"{new_fps:.4f}", "-movflags", "+faststart",
        "-metadata", f"creation_time={creation_time}",
        "-metadata", "handler_name=Core Media Video",
        "-metadata:s:v", "handler_name=Core Media Video",
        "-map_metadata", "-1", "-threads", "1",
    ]
    if audio_wav:
        enc_cmd += ["-c:a", "aac", "-b:a", "128k"]
    else:
        enc_cmd += ["-an"]
    enc_cmd += ["-v", "error", str(output_path)]

    total_frames = int(info["duration"] * fps) if info["duration"] > 0 else 0
    processor = FrameProcessor(total_frames, seed)

    print(f"  Video: warp + subpixel + DCT + color + chroma + noise + zoom + dilute + blend + swap + jitter")
    print(f"  Encoding...")

    decoder = subprocess.Popen(dec_cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    encoder = subprocess.Popen(enc_cmd, stdin=subprocess.PIPE, stderr=subprocess.PIPE)

    frame_idx = 0
    written = 0
    last_pct = -1

    def write_frame(f):
        nonlocal written
        encoder.stdin.write(f.tobytes())
        written += 1

    try:
        while True:
            raw = decoder.stdout.read(frame_size)
            if not raw or len(raw) < frame_size:
                break

            frame = np.frombuffer(raw, dtype=np.uint8).reshape((frame_h, frame_w, 3)).copy()
            result = processor.process(frame, frame_idx)

            if result is None:
                pass
            elif isinstance(result, tuple):
                if result[0] == "jitter":
                    write_frame(result[1])
                    write_frame(result[2])
                elif result[0] == "pair":
                    write_frame(result[1])
                    write_frame(result[2])
            else:
                write_frame(result)

            frame_idx += 1

            if total_frames > 0:
                pct = int(frame_idx / total_frames * 100)
                if pct != last_pct and pct % 10 == 0:
                    print(f"  Progress: {pct}%")
                    last_pct = pct

    except BrokenPipeError:
        pass
    finally:
        if processor.frame_buffer is not None:
            try:
                write_frame(processor.frame_buffer)
            except:
                pass
        try:
            encoder.stdin.close()
        except:
            pass
        decoder.stdout.close()

    decoder.wait()
    encoder.wait()
    enc_err = encoder.stderr.read()
    encoder.stderr.close()
    if encoder.returncode != 0:
        print(f"  Encoder error: {enc_err.decode(errors='replace')}")
        return None

    if audio_wav:
        try:
            os.remove(audio_wav)
        except OSError:
            pass

    print(f"  Done! {frame_idx} frames read, {written} written → {output_name}")
    return output_path


def main():
    script_dir = os.path.dirname(os.path.abspath(__file__))
    input_dir = os.path.join(script_dir, INPUT_DIR)
    output_dir = os.path.join(script_dir, OUTPUT_DIR)

    os.makedirs(input_dir, exist_ok=True)
    os.makedirs(output_dir, exist_ok=True)

    videos = []
    for f in sorted(os.listdir(input_dir)):
        ext = os.path.splitext(f)[1]
        if ext in SUPPORTED_EXT:
            videos.append(os.path.join(input_dir, f))

    if not videos:
        print(f"No videos found in {input_dir}/")
        print(f"Supported formats: {', '.join(sorted(SUPPORTED_EXT))}")
        return

    print(f"Found {len(videos)} video(s) to process")
    print(f"v4 Stealth | warp={MESH_AMP}px subpx={SUBPIXEL_SHIFT}px dct={DCT_STRENGTH} "
          f"chroma=±{CHROMA_NOISE_AMP} crop={CROP_PX}px speed={SPEED}x\n")

    for i, vpath in enumerate(videos, 1):
        name = os.path.basename(vpath)
        print(f"[{i}/{len(videos)}] Processing: {name}")

        info = get_video_info(vpath)
        print(f"  {info['width']}x{info['height']} @ {info['fps']:.1f}fps, "
              f"rotation={info['rotation']}, audio={'yes' if info['has_audio'] else 'no'}")

        result = process_video(vpath, output_dir, info)
        if result:
            orig_size = os.path.getsize(vpath) / (1024 * 1024)
            new_size = os.path.getsize(result) / (1024 * 1024)
            print(f"  Size: {orig_size:.1f}MB → {new_size:.1f}MB\n")
        else:
            print(f"  FAILED!\n")

    print("All done!")


if __name__ == "__main__":
    main()
