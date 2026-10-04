#!/usr/bin/env python3
"""
TikTok Video Uniqualizer — bypasses 5-layer duplicate detection:
  1. File hash (SHA-256) — re-encode breaks it
  2. Perceptual hash (TMK+PDQF/PDQ) — mesh warp + DCT butterfly
  3. Audio fingerprint (constellation map) — phantom peak injection
  4. Deep learning (CLIP/ViT) — attention dilution in uniform areas
  5. Invisible watermarks (C2PA) — CRF re-encode strips mid-freq DCT

Pipeline: ffmpeg decode → per-frame Python processing → ffmpeg encode
Memory: ~6 MB per frame (pipe-based, one frame at a time)
"""

import os
import sys
import struct
import subprocess
import random
import json
import wave
import math
import time
from pathlib import Path

import numpy as np
import cv2

# ─── Config ───────────────────────────────────────────────────────────
SPEED = 1.02
PITCH_FACTOR = 1.02
CRF = 22
PRESET = "medium"
CROP_PX = 3
MESH_GRID = 8
MESH_AMP = 2.0
DCT_STRENGTH = 0.08
NOISE_STRENGTH = 3
DILUTION_AMP = 4
DILUTION_THRESHOLD = 12
AUDIO_PHANTOM_OFFSET_HZ = 75
AUDIO_PHANTOM_AMP = 0.80
INPUT_DIR = "input"
OUTPUT_DIR = "output"
SUPPORTED_EXT = {".mp4", ".mov", ".MP4", ".MOV"}


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
            for tag_key in ("tags",):
                tags = vstream.get(tag_key, {})
                rot = tags.get("rotate", "0")
                rotation = int(rot)
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
    map_x = np.zeros((h, w), dtype=np.float32)
    map_y = np.zeros((h, w), dtype=np.float32)

    ctrl_y = np.linspace(0, h, grid + 1)
    ctrl_x = np.linspace(0, w, grid + 1)

    dx = np.random.uniform(-MESH_AMP, MESH_AMP, (grid + 1, grid + 1)).astype(np.float32)
    dy = np.random.uniform(-MESH_AMP, MESH_AMP, (grid + 1, grid + 1)).astype(np.float32)
    dx[0, :] = dx[-1, :] = dx[:, 0] = dx[:, -1] = 0
    dy[0, :] = dy[-1, :] = dy[:, 0] = dy[:, -1] = 0

    full_dx = cv2.resize(dx, (w, h), interpolation=cv2.INTER_LINEAR)
    full_dy = cv2.resize(dy, (w, h), interpolation=cv2.INTER_LINEAR)

    base_x = np.arange(w, dtype=np.float32)[np.newaxis, :]
    base_y = np.arange(h, dtype=np.float32)[:, np.newaxis]
    base_x = np.broadcast_to(base_x, (h, w)).copy()
    base_y = np.broadcast_to(base_y, (h, w)).copy()

    map_x = base_x + full_dx
    map_y = base_y + full_dy

    return cv2.remap(frame, map_x, map_y, cv2.INTER_LINEAR, borderMode=cv2.BORDER_REFLECT_101)


def dct_butterfly(frame):
    yuv = cv2.cvtColor(frame, cv2.COLOR_BGR2YUV)
    y_ch = yuv[:, :, 0].astype(np.float32)

    dft = np.fft.fft2(y_ch)
    h, w = y_ch.shape
    mid_h = slice(h // 4, 3 * h // 4)
    mid_w = slice(w // 4, 3 * w // 4)

    perturbation = np.random.normal(0, DCT_STRENGTH, dft[mid_h, mid_w].shape)
    magnitude = np.abs(dft[mid_h, mid_w])
    median_mag = np.median(magnitude[magnitude > 0]) if np.any(magnitude > 0) else 1.0
    mask = (magnitude > median_mag * 0.5) & (magnitude < median_mag * 2.0)

    dft[mid_h, mid_w] += perturbation * mask * median_mag

    y_new = np.fft.ifft2(dft).real
    y_new = np.clip(y_new, 0, 255).astype(np.uint8)
    yuv[:, :, 0] = y_new
    return cv2.cvtColor(yuv, cv2.COLOR_YUV2BGR)


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
                blk = result[by:by + block, bx:bx + block].astype(np.int16)
                blk += texture
                result[by:by + block, bx:bx + block] = np.clip(blk, 0, 255).astype(np.uint8)

    return result


def process_frame(frame, frame_idx):
    frame = mesh_warp(frame)
    if frame_idx % 3 == 0:
        frame = dct_butterfly(frame)
    frame = attention_dilution(frame)
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
    if samples.ndim > 1:
        flat = samples.flatten()
    else:
        flat = samples
    if params.sampwidth == 2:
        flat = np.clip(flat, -32768, 32767).astype(np.int16)
    elif params.sampwidth == 4:
        flat = np.clip(flat, -2147483648, 2147483647).astype(np.int32)
    else:
        flat = np.clip(flat + 128, 0, 255).astype(np.uint8)
    with wave.open(str(path), "wb") as wf:
        wf.setparams(params)
        wf.writeframes(flat.tobytes())


def constellation_poison_mono(audio, sr):
    n = len(audio)
    spectrum = np.fft.rfft(audio)
    freqs = np.fft.rfftfreq(n, 1.0 / sr)

    n_peaks = max(5, n // sr * 3)
    peak_indices = np.argsort(np.abs(spectrum))[-n_peaks:]

    for idx in peak_indices:
        if idx < len(freqs):
            freq = freqs[idx]
            offset_freq = freq + AUDIO_PHANTOM_OFFSET_HZ
            neg_offset_freq = freq - AUDIO_PHANTOM_OFFSET_HZ

            for target_f in [offset_freq, neg_offset_freq]:
                if 20 < target_f < sr / 2:
                    target_idx = int(target_f * n / sr)
                    if 0 < target_idx < len(spectrum):
                        phase = np.random.uniform(0, 2 * np.pi)
                        spectrum[target_idx] += (
                            np.abs(spectrum[idx]) * AUDIO_PHANTOM_AMP * np.exp(1j * phase)
                        )

    result = np.fft.irfft(spectrum, n=n)
    return result


def extract_audio(video_path, wav_path, sr, channels):
    cmd = [
        "ffmpeg", "-y", "-i", str(video_path),
        "-vn", "-acodec", "pcm_s16le",
        "-ar", str(sr), "-ac", str(channels),
        str(wav_path)
    ]
    r = subprocess.run(cmd, capture_output=True, text=True)
    return r.returncode == 0


def process_audio(video_path, output_wav, info):
    sr = info["audio_sr"]
    ch = info["audio_ch"]
    tmp_wav = str(output_wav) + ".tmp.wav"

    if not extract_audio(video_path, tmp_wav, sr, ch):
        return False

    samples, params = load_wav(tmp_wav)

    if samples.ndim == 1:
        samples = constellation_poison_mono(samples, sr)
    else:
        for c in range(samples.shape[1]):
            samples[:, c] = constellation_poison_mono(samples[:, c], sr)

    save_wav(str(output_wav), samples, params)

    try:
        os.remove(tmp_wav)
    except OSError:
        pass
    return True


# ─── Output Naming ────────────────────────────────────────────────────

def generate_output_name(output_dir):
    existing = set()
    for f in Path(output_dir).glob("IMG_*.mp4"):
        try:
            num = int(f.stem.split("_")[1])
            existing.add(num)
        except (ValueError, IndexError):
            pass
    while True:
        num = random.randint(1000, 9999)
        if num not in existing:
            return f"IMG_{num:04d}.mp4"


def random_creation_time():
    now = time.time()
    offset = random.randint(86400, 86400 * 30)
    t = now - offset
    return time.strftime("%Y-%m-%dT%H:%M:%S", time.localtime(t))


# ─── Main Pipeline ────────────────────────────────────────────────────

def process_video(input_path, output_dir, info):
    output_name = generate_output_name(output_dir)
    output_path = os.path.join(output_dir, output_name)

    w = info["width"]
    h = info["height"]
    fps = info["fps"]
    rotation = info["rotation"]

    disp_w, disp_h = get_display_dims(w, h, rotation)

    transpose = build_transpose_filter(rotation)

    # decoder filters
    dec_filters = []
    if transpose:
        dec_filters.append(transpose)

    dec_cmd = [
        "ffmpeg", "-nostdin", "-noautorotate",
        "-i", str(input_path),
    ]
    if dec_filters:
        dec_cmd += ["-vf", ",".join(dec_filters)]
    dec_cmd += [
        "-f", "rawvideo", "-pix_fmt", "bgr24",
        "-v", "error", "pipe:1"
    ]

    frame_w = disp_w
    frame_h = disp_h
    frame_size = frame_w * frame_h * 3

    # crop dimensions
    crop_w = frame_w - 2 * CROP_PX
    crop_h = frame_h - 2 * CROP_PX

    # process audio
    audio_wav = None
    if info["has_audio"]:
        audio_wav = os.path.join(output_dir, f".tmp_audio_{output_name}.wav")
        print(f"  Processing audio...")
        if not process_audio(input_path, audio_wav, info):
            print(f"  Warning: audio processing failed, encoding without audio")
            audio_wav = None

    # encoder filters
    enc_vf = []
    enc_vf.append(f"crop={crop_w}:{crop_h}:{CROP_PX}:{CROP_PX}")
    enc_vf.append(f"scale={crop_w}:{crop_h}")
    enc_vf.append(f"noise=c0s={NOISE_STRENGTH}:c0f=t")
    enc_vf.append(f"setpts=PTS/{SPEED}")

    new_fps = fps * SPEED
    audio_rate = int(info["audio_sr"] * PITCH_FACTOR)

    enc_cmd = [
        "ffmpeg", "-nostdin", "-y",
        "-f", "rawvideo", "-pix_fmt", "bgr24",
        "-s", f"{frame_w}x{frame_h}",
        "-r", f"{fps}",
        "-i", "pipe:0",
    ]

    if audio_wav:
        enc_cmd += ["-i", audio_wav]

    enc_cmd += ["-vf", ",".join(enc_vf)]

    if audio_wav:
        enc_cmd += [
            "-af", f"asetrate={audio_rate},aresample={info['audio_sr']}",
        ]

    creation_time = random_creation_time()
    enc_cmd += [
        "-c:v", "libx264",
        "-crf", str(CRF),
        "-preset", PRESET,
        "-profile:v", "high",
        "-pix_fmt", "yuv420p",
        "-r", f"{new_fps:.4f}",
        "-movflags", "+faststart",
        "-metadata", f"creation_time={creation_time}",
        "-metadata", "handler_name=Core Media Video",
        "-metadata:s:v", "handler_name=Core Media Video",
        "-map_metadata", "-1",
    ]

    if audio_wav:
        enc_cmd += ["-c:a", "aac", "-b:a", "128k"]
    else:
        enc_cmd += ["-an"]

    enc_cmd += ["-v", "error", str(output_path)]

    print(f"  Starting decode/encode pipeline...")
    decoder = subprocess.Popen(dec_cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    encoder = subprocess.Popen(enc_cmd, stdin=subprocess.PIPE, stderr=subprocess.PIPE)

    frame_idx = 0
    total_frames = int(info["duration"] * fps) if info["duration"] > 0 else 0
    last_pct = -1

    try:
        while True:
            raw = decoder.stdout.read(frame_size)
            if not raw or len(raw) < frame_size:
                break

            frame = np.frombuffer(raw, dtype=np.uint8).reshape((frame_h, frame_w, 3)).copy()
            frame = process_frame(frame, frame_idx)
            encoder.stdin.write(frame.tobytes())
            frame_idx += 1

            if total_frames > 0:
                pct = int(frame_idx / total_frames * 100)
                if pct != last_pct and pct % 10 == 0:
                    print(f"  Progress: {pct}%")
                    last_pct = pct

    except BrokenPipeError:
        pass
    finally:
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

    print(f"  Done! {frame_idx} frames processed → {output_name}")
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

    print(f"Found {len(videos)} video(s) to process\n")

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
