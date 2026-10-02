#!/usr/bin/env python3
"""
video_uniqualizer.py — уникализация видео через FFmpeg.

Что меняется (каждая копия получает свой набор случайных параметров из seed):
  * Визуал (pHash)      — микро-кроп, микро-поворот, цвет/гамма/оттенок, шум, резкость,
                           виньетка, опционально зеркало; расстояние pHash измеряется.
  * Файл (MD5/SHA-256)  — перекодирование, случайные параметры энкодера, чистка метаданных,
                           удаление SEI с подписью x264/x265.
  * Lanczos micro-scale — апскейл на 1–5 % с Lanczos + кроп обратно со случайным сдвигом.
  * Custom AQ / CQM     — случайные aq-mode/aq-strength + своя матрица квантования (x264).
  * Доп.: темп, тримминг, GOP, CRF/preset, аудио (питч/громкость/EQ/sample rate).

Требования: ffmpeg + ffprobe в PATH, Python 3.8+, numpy (для pHash; без него
проверка pHash пропускается).

Примеры:
  python video_uniqualizer.py input.mp4
  python video_uniqualizer.py input.mp4 -n 5 --intensity high -o out/
  python video_uniqualizer.py videos/ -n 3 --min-phash 6 --seed 42
  python video_uniqualizer.py input.mp4 --codec h265 --dry-run
"""

from __future__ import annotations

import argparse
import datetime as dt
import hashlib
import json
import math
import random
import shutil
import subprocess
import sys
import tempfile
import uuid
from dataclasses import asdict, dataclass, field
from pathlib import Path

try:
    import numpy as np
except ImportError:  # pHash станет недоступен, остальное работает
    np = None

VIDEO_EXTS = {".mp4", ".mov", ".mkv", ".avi", ".webm", ".m4v", ".flv", ".ts", ".wmv"}

# Диапазоны изменений для каждого уровня интенсивности.
INTENSITY = {
    "low": dict(
        scale=(1.010, 1.020), rotate=0.0, brightness=0.010, contrast=0.015,
        saturation=0.03, gamma=0.02, hue=1.0, noise=(1, 3), sharpen=0.15,
        vignette=0.0, speed=0.010, trim_frames=(0, 2), audio_pitch=0.004,
        volume=0.5, eq_gain=1.0,
    ),
    "medium": dict(
        scale=(1.020, 1.040), rotate=0.5, brightness=0.020, contrast=0.030,
        saturation=0.06, gamma=0.04, hue=2.5, noise=(2, 5), sharpen=0.30,
        vignette=0.15, speed=0.020, trim_frames=(1, 4), audio_pitch=0.008,
        volume=1.0, eq_gain=2.0,
    ),
    "high": dict(
        scale=(1.040, 1.070), rotate=1.2, brightness=0.035, contrast=0.050,
        saturation=0.10, gamma=0.07, hue=5.0, noise=(4, 8), sharpen=0.50,
        vignette=0.30, speed=0.035, trim_frames=(2, 8), audio_pitch=0.015,
        volume=1.5, eq_gain=3.0,
    ),
}

# JVT-матрицы по умолчанию (растровый порядок) — основа для пользовательской CQM.
JVT_4X4_INTRA = [6, 13, 20, 28, 13, 20, 28, 32, 20, 28, 32, 37, 28, 32, 37, 42]
JVT_4X4_INTER = [10, 14, 20, 24, 14, 20, 24, 27, 20, 24, 27, 30, 24, 27, 30, 34]
JVT_8X8_INTRA = [
    6, 10, 13, 16, 18, 23, 25, 27, 10, 11, 16, 18, 23, 25, 27, 29,
    13, 16, 18, 23, 25, 27, 29, 31, 16, 18, 23, 25, 27, 29, 31, 33,
    18, 23, 25, 27, 29, 31, 33, 36, 23, 25, 27, 29, 31, 33, 36, 38,
    25, 27, 29, 31, 33, 36, 38, 40, 27, 29, 31, 33, 36, 38, 40, 42,
]
JVT_8X8_INTER = [
    9, 13, 15, 17, 19, 21, 22, 24, 13, 13, 17, 19, 21, 22, 24, 25,
    15, 17, 19, 21, 22, 24, 25, 27, 17, 19, 21, 22, 24, 25, 27, 28,
    19, 21, 22, 24, 25, 27, 28, 30, 21, 22, 24, 25, 27, 28, 30, 32,
    22, 24, 25, 27, 28, 30, 32, 33, 24, 25, 27, 28, 30, 32, 33, 35,
]


# --------------------------------------------------------------------------- utils

def log(msg: str) -> None:
    print(msg, file=sys.stderr, flush=True)


def require_tools() -> None:
    for tool in ("ffmpeg", "ffprobe"):
        if shutil.which(tool) is None:
            sys.exit(f"Ошибка: {tool} не найден в PATH.")


def file_hashes(path: Path) -> dict:
    md5, sha = hashlib.md5(), hashlib.sha256()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            md5.update(chunk)
            sha.update(chunk)
    return {"md5": md5.hexdigest(), "sha256": sha.hexdigest()}


def probe(path: Path) -> dict:
    cmd = ["ffprobe", "-v", "error", "-print_format", "json",
           "-show_format", "-show_streams", str(path)]
    data = json.loads(subprocess.run(cmd, capture_output=True, check=True, text=True).stdout)
    video = next((s for s in data["streams"] if s["codec_type"] == "video"), None)
    if video is None:
        raise ValueError(f"{path}: нет видеопотока")
    audio = next((s for s in data["streams"] if s["codec_type"] == "audio"), None)

    num, den = (video.get("avg_frame_rate") or video.get("r_frame_rate") or "30/1").split("/")
    fps = float(num) / float(den) if float(den) else 30.0
    duration = float(data["format"].get("duration") or video.get("duration") or 0)
    return {
        "width": int(video["width"]),
        "height": int(video["height"]),
        "fps": fps or 30.0,
        "duration": duration,
        "has_audio": audio is not None,
        "sample_rate": int(audio["sample_rate"]) if audio else 48000,
    }


def even(x: float) -> int:
    return max(2, int(math.ceil(x / 2.0)) * 2)


# --------------------------------------------------------------------------- plan

@dataclass
class Plan:
    seed: int
    intensity: str
    strength: float
    # геометрия
    scale: float = 1.0
    rotate_deg: float = 0.0
    crop_x: int = 0
    crop_y: int = 0
    mirror: bool = False
    # цвет / текстура
    brightness: float = 0.0
    contrast: float = 1.0
    saturation: float = 1.0
    gamma: float = 1.0
    hue_deg: float = 0.0
    noise: int = 0
    sharpen: float = 0.0
    vignette: float = 0.0
    # время
    speed: float = 1.0
    trim_start: float = 0.0
    trim_end: float = 0.0
    # аудио
    audio_pitch: float = 1.0
    volume_db: float = 0.0
    eq_freq: int = 1000
    eq_gain: float = 0.0
    highpass: int = 20
    out_sample_rate: int = 48000
    audio_bitrate: str = "160k"
    # энкодер
    crf: int = 20
    preset: str = "medium"
    aq_mode: int = 1
    aq_strength: float = 1.0
    psy_rd: str = "1.00,0.00"
    deblock: str = "0,0"
    keyint: int = 250
    bframes: int = 3
    refs: int = 3
    me: str = "hex"
    subme: int = 7
    cqm: dict = field(default_factory=dict)
    # метаданные
    meta: dict = field(default_factory=dict)


def make_cqm(rng: random.Random, strength: float) -> dict:
    """Своя матрица квантования: смесь flat(16) и JVT + случайный джиттер."""
    def build(base: list) -> list:
        alpha = rng.uniform(0.15, 0.6)
        jitter = max(1, round(2 * strength))
        out = []
        for i, v in enumerate(base):
            q = 16 + alpha * (v - 16)
            if i:  # DC-коэффициент не трогаем, чтобы не плыла яркость блоков
                q += rng.randint(-jitter, jitter)
            out.append(int(min(64, max(4, round(q)))))
        return out

    return {
        "INTRA4X4_LUMA": build(JVT_4X4_INTRA),
        "INTRA4X4_CHROMAU": build(JVT_4X4_INTRA),
        "INTRA4X4_CHROMAV": build(JVT_4X4_INTRA),
        "INTER4X4_LUMA": build(JVT_4X4_INTER),
        "INTER4X4_CHROMAU": build(JVT_4X4_INTER),
        "INTER4X4_CHROMAV": build(JVT_4X4_INTER),
        "INTRA8X8_LUMA": build(JVT_8X8_INTRA),
        "INTER8X8_LUMA": build(JVT_8X8_INTER),
    }


def write_cqm(cqm: dict, path: Path) -> None:
    lines = []
    for name, values in cqm.items():
        n = 8 if len(values) == 64 else 4
        rows = [",".join(map(str, values[i:i + n])) for i in range(0, len(values), n)]
        lines.append(f"{name} =\n" + ",\n".join(rows) + "\n")
    path.write_text("\n".join(lines))


def rand_date(rng: random.Random) -> str:
    start = dt.datetime(2019, 1, 1, tzinfo=dt.timezone.utc)
    stamp = start + dt.timedelta(seconds=rng.randint(0, 6 * 365 * 86400))
    return stamp.strftime("%Y-%m-%dT%H:%M:%S.000000Z")


def make_plan(info: dict, seed: int, intensity: str, strength: float, args) -> Plan:
    rng = random.Random(seed)
    p = INTENSITY[intensity]
    s = strength

    def sym(r: float) -> float:
        """Случайное значение в ±r, но не ближе 25 % от нуля — изменение гарантировано."""
        v = rng.uniform(0.25 * r, r) * s
        return v if rng.random() < 0.5 else -v

    plan = Plan(seed=seed, intensity=intensity, strength=round(s, 3))

    # Lanczos micro-scaling + микро-поворот. Масштаб должен закрыть углы после поворота.
    plan.rotate_deg = round(sym(p["rotate"]), 3) if p["rotate"] else 0.0
    theta = math.radians(abs(plan.rotate_deg))
    w, h = info["width"], info["height"]
    rot_zoom = math.cos(theta) + math.sin(theta) * max(w / h, h / w)
    base = 1 + (rng.uniform(*p["scale"]) - 1) * s
    plan.scale = round(base * rot_zoom, 5)
    sw, sh = even(w * plan.scale), even(h * plan.scale)
    # Кроп со сдвигом, но в пределах запаса, который остаётся после поворота.
    mx = max(0, (sw - w * rot_zoom) / 2)
    my = max(0, (sh - h * rot_zoom) / 2)
    plan.crop_x = int((sw - w) / 2 + rng.uniform(-mx, mx))
    plan.crop_y = int((sh - h) / 2 + rng.uniform(-my, my))
    plan.mirror = args.mirror

    plan.brightness = round(sym(p["brightness"]), 4)
    plan.contrast = round(1 + sym(p["contrast"]), 4)
    plan.saturation = round(1 + sym(p["saturation"]), 4)
    plan.gamma = round(1 + sym(p["gamma"]), 4)
    plan.hue_deg = round(sym(p["hue"]), 3)
    plan.noise = max(1, round(rng.randint(*p["noise"]) * s))
    plan.sharpen = round(rng.uniform(0.3, 1.0) * p["sharpen"] * s, 3)
    plan.vignette = round(rng.uniform(0.5, 1.0) * p["vignette"] * s, 3) if p["vignette"] else 0.0

    if not args.keep_speed:
        plan.speed = round(1 + sym(p["speed"]), 4)
    lo, hi = p["trim_frames"]
    if info["duration"] > 3:
        plan.trim_start = round(rng.randint(lo, hi) / info["fps"], 4)
        plan.trim_end = round(rng.randint(lo, hi) / info["fps"], 4)

    plan.audio_pitch = round(1 + sym(p["audio_pitch"]), 5)
    plan.volume_db = round(sym(p["volume"]), 2)
    plan.eq_freq = rng.choice([250, 500, 1000, 2000, 4000, 8000])
    plan.eq_gain = round(sym(p["eq_gain"]), 2)
    plan.highpass = rng.randint(18, 35)
    plan.out_sample_rate = rng.choice([44100, 48000])
    plan.audio_bitrate = rng.choice(["128k", "144k", "160k", "192k"])

    plan.crf = args.crf if args.crf is not None else rng.randint(18, 22)
    plan.preset = rng.choice(["medium", "slow"]) if args.preset is None else args.preset
    plan.aq_mode = rng.choice([1, 2, 3])
    plan.aq_strength = round(rng.uniform(0.7, 1.3), 2)
    plan.psy_rd = f"{rng.uniform(0.8, 1.2):.2f},{rng.uniform(0.0, 0.2):.2f}"
    plan.deblock = f"{rng.randint(-2, 1)},{rng.randint(-2, 1)}"
    fps_round = max(1, round(info["fps"]))
    plan.keyint = fps_round * rng.choice([2, 3, 4, 5, 8, 10])
    plan.bframes = rng.randint(2, 5)
    plan.refs = rng.randint(2, 5)
    plan.me = rng.choice(["hex", "umh"])
    plan.subme = rng.randint(6, 9)
    if args.codec == "h264" and not args.no_cqm:
        plan.cqm = make_cqm(rng, s)

    plan.meta = {
        "creation_time": rand_date(rng),
        "title": "",
        "comment": uuid.UUID(int=rng.getrandbits(128)).hex,
        "handler_name": rng.choice(["VideoHandler", "Core Media Video", "Video", "ISO Media"]),
        "audio_handler": rng.choice(["SoundHandler", "Core Media Audio", "Audio", "ISO Media"]),
        # непустое значение, иначе ffmpeg сам впишет "Lavc libx264"
        "encoder": rng.choice(["AVC Coding", "H.264", "Video"]
                              if args.codec == "h264" else ["HEVC Coding", "H.265", "Video"]),
    }
    return plan


# --------------------------------------------------------------------------- ffmpeg

def build_filters(plan: Plan, info: dict) -> tuple[str, str]:
    w, h = info["width"], info["height"]
    sw, sh = even(w * plan.scale), even(h * plan.scale)
    lanczos = "lanczos+accurate_rnd+full_chroma_int+full_chroma_inp"

    v = [f"setpts=(PTS-STARTPTS)/{plan.speed}"]
    v.append(f"scale={sw}:{sh}:flags={lanczos}:param0=3")
    if plan.rotate_deg:
        v.append(f"rotate={plan.rotate_deg}*PI/180:ow=iw:oh=ih:bilinear=1:fillcolor=black")
    v.append(f"crop={w}:{h}:{plan.crop_x}:{plan.crop_y}")
    if plan.mirror:
        v.append("hflip")
    v.append(f"eq=brightness={plan.brightness}:contrast={plan.contrast}"
             f":saturation={plan.saturation}:gamma={plan.gamma}")
    if plan.hue_deg:
        v.append(f"hue=h={plan.hue_deg}")
    if plan.sharpen:
        v.append(f"unsharp=5:5:{plan.sharpen}:3:3:0")
    if plan.vignette:
        # angle близко к 0 = очень мягкая виньетка
        v.append(f"vignette=angle={plan.vignette:.3f}")
    if plan.noise:
        v.append(f"noise=alls={plan.noise}:allf=t+u")
    v.append("setsar=1,format=yuv420p")

    sr = info["sample_rate"]
    a = [
        "asetpts=PTS-STARTPTS",
        # питч: меняем частоту дискретизации, затем возвращаем темп через atempo
        f"asetrate={int(round(sr * plan.audio_pitch))}",
        f"aresample={sr}",
        f"atempo={plan.speed / plan.audio_pitch:.6f}",
        f"highpass=f={plan.highpass}",
        f"equalizer=f={plan.eq_freq}:t=q:w=1.0:g={plan.eq_gain}",
        f"volume={plan.volume_db}dB",
        f"aresample={plan.out_sample_rate}:resampler=soxr" if has_soxr() else
        f"aresample={plan.out_sample_rate}",
    ]
    return ",".join(v), ",".join(a)


_SOXR = None


def has_soxr() -> bool:
    global _SOXR
    if _SOXR is None:
        out = subprocess.run(["ffmpeg", "-hide_banner", "-buildconf"],
                             capture_output=True, text=True).stdout
        _SOXR = "--enable-libsoxr" in out
    return _SOXR


def build_command(src: Path, dst: Path, plan: Plan, info: dict, codec: str,
                  cqm_name: str | None) -> list:
    vf, af = build_filters(plan, info)
    cmd = ["ffmpeg", "-hide_banner", "-loglevel", "error", "-stats", "-y"]
    if plan.trim_start:
        cmd += ["-ss", f"{plan.trim_start}"]
    cmd += ["-i", str(src)]
    if info["duration"] and (plan.trim_start or plan.trim_end):
        kept = info["duration"] - plan.trim_start - plan.trim_end
        cmd += ["-t", f"{kept / plan.speed:.4f}"]

    cmd += ["-map", "0:v:0", "-vf", vf]
    if info["has_audio"]:
        cmd += ["-map", "0:a:0", "-af", af,
                "-c:a", "aac", "-b:a", plan.audio_bitrate, "-ar", str(plan.out_sample_rate)]

    common = (f"aq-mode={plan.aq_mode}:aq-strength={plan.aq_strength}"
              f":keyint={plan.keyint}:min-keyint={max(1, plan.keyint // 10)}"
              f":bframes={plan.bframes}:ref={plan.refs}:deblock={plan.deblock}")
    if codec == "h264":
        params = (f"{common}:psy-rd={plan.psy_rd}:me={plan.me}:subme={plan.subme}"
                  f":8x8dct=1:trellis=2")
        if cqm_name:
            params += f":cqmfile={cqm_name}"
        cmd += ["-c:v", "libx264", "-profile:v", "high", "-x264-params", params,
                "-bsf:v", "filter_units=remove_types=6"]   # SEI с настройками x264
        tag = []
    else:
        psy = plan.psy_rd.split(",")[0]
        params = (f"{common}:psy-rd={psy}:me={plan.me}:subme={min(plan.subme, 7)}"
                  f":log-level=error:info=0")                # info=0: без SEI-подписи
        cmd += ["-c:v", "libx265", "-x265-params", params]
        tag = ["-tag:v", "hvc1"]
    cmd += ["-crf", str(plan.crf), "-preset", plan.preset, "-pix_fmt", "yuv420p"] + tag

    m = plan.meta
    cmd += ["-map_metadata", "-1", "-map_chapters", "-1",
            "-fflags", "+bitexact", "-flags:v", "+bitexact", "-flags:a", "+bitexact",
            "-metadata", f"creation_time={m['creation_time']}",
            "-metadata", f"comment={m['comment']}",
            "-metadata:s:v:0", f"handler_name={m['handler_name']}",
            "-metadata:s:v:0", f"encoder={m['encoder']}"]
    if info["has_audio"]:
        cmd += ["-metadata:s:a:0", f"handler_name={m['audio_handler']}"]
    if dst.suffix.lower() in (".mp4", ".mov", ".m4v"):
        cmd += ["-movflags", "+faststart"]
    cmd.append(str(dst))
    return cmd


# --------------------------------------------------------------------------- pHash

def _dct_matrix(n: int):
    k = np.arange(n)
    m = np.cos(np.pi * (2 * k[None, :] + 1) * k[:, None] / (2 * n)) * np.sqrt(2 / n)
    m[0] /= np.sqrt(2)
    return m


def phash_frame(gray32) -> int:
    d = _dct_matrix(32)
    coeffs = d @ gray32 @ d.T
    low = coeffs[:8, :8].flatten()
    bits = low > np.median(low)
    return int("".join("1" if b else "0" for b in bits), 2)


def grab_gray32(path: Path, t: float):
    cmd = ["ffmpeg", "-v", "error", "-ss", f"{max(0.0, t):.3f}", "-i", str(path),
           "-frames:v", "1", "-vf", "scale=32:32:flags=area,format=gray",
           "-f", "rawvideo", "-"]
    raw = subprocess.run(cmd, capture_output=True).stdout
    if len(raw) < 1024:
        return None
    return np.frombuffer(raw[:1024], dtype=np.uint8).reshape(32, 32).astype(np.float64)


def phash_distance(src: Path, dst: Path, plan: Plan, info: dict, samples: int = 8):
    """Средняя дистанция Хэмминга (0–64) на соответствующих кадрах."""
    if np is None or not info["duration"]:
        return None
    usable = info["duration"] - plan.trim_start - plan.trim_end
    dists = []
    for i in range(samples):
        t_src = plan.trim_start + usable * (i + 0.5) / samples
        t_dst = (t_src - plan.trim_start) / plan.speed
        a, b = grab_gray32(src, t_src), grab_gray32(dst, t_dst)
        if a is None or b is None:
            continue
        dists.append(bin(phash_frame(a) ^ phash_frame(b)).count("1"))
    if not dists:
        return None
    return {"mean": round(sum(dists) / len(dists), 2), "min": min(dists),
            "max": max(dists), "frames": dists}


# --------------------------------------------------------------------------- main

def process(src: Path, out_dir: Path, index: int, args, base_seed: int) -> dict:
    info = probe(src)
    ext = args.format or src.suffix.lower() or ".mp4"
    if not ext.startswith("."):
        ext = "." + ext
    dst = (out_dir / f"{src.stem}_uniq_{index:02d}{ext}").resolve()

    strength = args.strength
    attempt_seed = base_seed
    result = None
    for attempt in range(1, args.max_attempts + 1):
        plan = make_plan(info, attempt_seed, args.intensity, strength, args)
        with tempfile.TemporaryDirectory() as tmp:
            cqm_name = None
            if plan.cqm:
                cqm_name = "cqm.cfg"   # относительный путь: в x264-params нельзя ':' (C:\...)
                write_cqm(plan.cqm, Path(tmp) / cqm_name)
            cmd = build_command(src.resolve(), dst, plan, info, args.codec, cqm_name)
            if args.dry_run:
                log(" ".join(f'"{c}"' if " " in c or ";" in c else c for c in cmd))
                return {"source": str(src), "plan": asdict(plan), "dry_run": True}
            log(f"[{src.name} #{index}] попытка {attempt}, seed={attempt_seed}, "
                f"сила={strength:.2f}")
            proc = subprocess.run(cmd, cwd=tmp)
            if proc.returncode != 0:
                raise RuntimeError(f"ffmpeg завершился с кодом {proc.returncode}")

        ph = phash_distance(src, dst, plan, info) if not args.no_verify else None
        result = {
            "source": str(src), "output": str(dst), "attempt": attempt,
            "hash_source": file_hashes(src), "hash_output": file_hashes(dst),
            "phash_distance": ph, "plan": asdict(plan),
        }
        if args.min_phash is None or ph is None or ph["mean"] >= args.min_phash:
            break
        log(f"  pHash {ph['mean']} < {args.min_phash}: усиливаем изменения")
        strength *= 1.4
        attempt_seed += 7919
    return result


def collect_inputs(path: Path) -> list:
    if path.is_dir():
        return sorted(p for p in path.iterdir() if p.suffix.lower() in VIDEO_EXTS)
    if path.is_file():
        return [path]
    sys.exit(f"Ошибка: {path} не найден")


def print_summary(r: dict) -> None:
    if r.get("dry_run"):
        return
    p = r["plan"]
    hs, ho = r["hash_source"], r["hash_output"]
    log(f"  -> {r['output']}")
    log(f"     MD5     {hs['md5']} -> {ho['md5']}")
    log(f"     SHA-256 {hs['sha256'][:24]}… -> {ho['sha256'][:24]}…")
    if r["phash_distance"]:
        d = r["phash_distance"]
        log(f"     pHash Хэмминг: mean={d['mean']} min={d['min']} max={d['max']} (из 64)")
    log(f"     Lanczos x{p['scale']}  rot={p['rotate_deg']}°  crop=({p['crop_x']},{p['crop_y']})"
        f"  speed={p['speed']}  CRF={p['crf']}  aq-mode={p['aq_mode']}"
        f"  aq-strength={p['aq_strength']}  CQM={'да' if p['cqm'] else 'нет'}")


def main() -> None:
    ap = argparse.ArgumentParser(
        description="Уникализация видео: pHash, MD5/SHA-256, Lanczos micro-scaling, custom AQ/CQM.")
    ap.add_argument("input", type=Path, help="видеофайл или папка")
    ap.add_argument("-o", "--output", type=Path, default=Path("uniq_out"), help="папка вывода")
    ap.add_argument("-n", "--copies", type=int, default=1, help="копий на каждый исходник")
    ap.add_argument("--intensity", choices=INTENSITY, default="medium")
    ap.add_argument("--strength", type=float, default=1.0, help="множитель силы (0.5–2.0)")
    ap.add_argument("--seed", type=int, help="seed для воспроизводимости")
    ap.add_argument("--codec", choices=["h264", "h265"], default="h264")
    ap.add_argument("--crf", type=int, help="фиксированный CRF (иначе 18–22)")
    ap.add_argument("--preset", help="фиксированный preset x264/x265")
    ap.add_argument("--format", help="расширение вывода: mp4, mkv, mov")
    ap.add_argument("--mirror", action="store_true", help="зеркально отразить по горизонтали")
    ap.add_argument("--keep-speed", action="store_true", help="не менять темп")
    ap.add_argument("--no-cqm", action="store_true", help="без пользовательской матрицы")
    ap.add_argument("--min-phash", type=float,
                    help="минимальная средняя дистанция pHash; иначе повтор с усилением")
    ap.add_argument("--max-attempts", type=int, default=3)
    ap.add_argument("--no-verify", action="store_true", help="не считать pHash")
    ap.add_argument("--report", type=Path, help="сохранить JSON-отчёт")
    ap.add_argument("--dry-run", action="store_true", help="только показать команды ffmpeg")
    args = ap.parse_args()

    require_tools()
    if np is None and not args.no_verify:
        log("Внимание: numpy не установлен — pHash не будет посчитан (pip install numpy).")

    sources = collect_inputs(args.input)
    if not sources:
        sys.exit("Нет видеофайлов для обработки.")
    args.output.mkdir(parents=True, exist_ok=True)
    master = random.Random(args.seed)

    results, failed = [], 0
    for src in sources:
        for i in range(1, args.copies + 1):
            try:
                r = process(src, args.output, i, args, master.getrandbits(32))
                results.append(r)
                print_summary(r)
            except Exception as e:  # продолжаем пакет
                failed += 1
                log(f"[{src.name} #{i}] ОШИБКА: {e}")

    if args.report:
        args.report.write_text(json.dumps(results, ensure_ascii=False, indent=2))
        log(f"Отчёт: {args.report}")
    log(f"Готово: {len(results)} успешно, {failed} с ошибкой.")
    sys.exit(1 if failed else 0)


if __name__ == "__main__":
    main()
