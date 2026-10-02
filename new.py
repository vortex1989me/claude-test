#!/usr/bin/env python3
"""
new.py — уникализация видео через FFmpeg.

Структура папок (рядом со скриптом):
  new.py
  input_videos/   <- сюда кладём исходные видео
  output_videos/  <- сюда попадают уникализированные копии

Запуск: двойной клик по new.py или в консоли `python new.py`.
Все настройки — в блоке «НАСТРОЙКИ» ниже. Меняете число, сохраняете файл, запускаете.
Аргументы командной строки (python new.py --help) имеют приоритет над этим блоком.

Требования: ffmpeg + ffprobe в PATH, Python 3.9+, numpy (pip install numpy) для pHash.
"""

from __future__ import annotations

import argparse
import datetime as dt
import hashlib
import json
import math
import os
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

# ╔══════════════════════════════════════════════════════════════════════════════╗
# ║                                 НАСТРОЙКИ                                    ║
# ╚══════════════════════════════════════════════════════════════════════════════╝
#
# Как читать диапазоны:  (мин, макс) — для каждой копии берётся случайное значение
# внутри диапазона. Одиночное число X — случайное значение от -X до +X (в обе стороны).
# 0 или (0, 0) — фильтр ВЫКЛЮЧЕН.

# ---------------------------------------------------------------- основное
COPIES = 1              # сколько уникальных копий делать из каждого видео
LEVEL = "custom"        # "custom" — берутся значения из FILTERS ниже;
                        # "low" / "medium" / "high" — готовые пресеты (см. PRESETS)
STRENGTH = 2.0          # общий множитель силы всех фильтров: 0.5 = вдвое мягче, 2.0 = вдвое сильнее.
                        # Умножает ВСЁ в FILTERS (кроме mirror): при 2.0 scale (1.04, 1.06) даёт
                        # приближение 8–12 %, rotate 0.7 -> до ±1.4°, hue 2.5 -> до ±5° и т. д.
SEED = None             # None = каждый запуск разный; число (например 42) = одинаковый результат
OUTPUT_FORMAT = "mp4"   # "mp4", "mov", "mkv" или None (как у исходника).
                        # Имя файла сохраняется: IMG_0205.MOV -> IMG_0205.mp4;
                        # при COPIES > 1: IMG_0205.mp4, IMG_0205_2.mp4, IMG_0205_3.mp4 ...
MIN_PHASH = None        # None = не проверять; например 10 — если средний pHash (из 64) меньше,
                        # копия переделывается с силой ×1.4 (до MAX_ATTEMPTS раз)
MAX_ATTEMPTS = 3

# ---------------------------------------------------------------- фильтры (LEVEL = "custom")
FILTERS = dict(
    # HFLIP — зеркальное отражение по горизонтали.
    #   True  -> лево и право меняются местами. Самое сильное изменение pHash (≈25–35 из 64),
    #            глазом почти незаметно, но зеркальным станет текст/надписи в кадре.
    #   False -> без зеркала.
    mirror=True,

    # Lanczos micro-scaling: увеличение кадра и обрезка обратно в исходный размер.
    #   (1.02, 1.04) -> приближение на 2–4 %, края срезаются. Меняет геометрию кадра для pHash.
    #   (1.0, 1.0)   -> выключено.   Больше 1.08 — уже заметно «наехала» камера.
    scale=(1.04, 1.06),

    # Поворот кадра в градусах (±). 0.5 -> до ±0.5°. Масштаб сам подрастёт, чтобы не было
    # чёрных углов. Больше 2° — заметно глазу. 0 = выкл.
    rotate=0.7,

    # Цвет. Значения ± от исходного:
    brightness=0.02,    # яркость: 0.02 = ±2 %. Больше 0.06 — заметно светлее/темнее.
    contrast=0.03,      # контраст: 0.03 = ±3 %.
    saturation=0.06,    # насыщенность: 0.06 = ±6 %. Больше 0.15 — цвета «кислотные»/блёклые.
    gamma=0.04,         # гамма (средние тона): 0.04 = ±4 %.
    hue=2.5,            # оттенок в градусах: 2.5 = ±2.5°. Больше 8° — кожа меняет цвет.

    # Шум (зерно), сила 0–100. (2, 5) -> лёгкое зерно, меняет каждый кадр и хеши.
    #   Больше 10 — видно «песок», файл станет тяжелее. (0, 0) = выкл.
    noise=(2, 5),

    # Резкость (unsharp): 0.3 -> до +0.3. Больше 1.0 — ореолы по краям. 0 = выкл.
    sharpen=0.3,

    # Виньетка (затемнение углов): 0.15 -> очень мягкая. 0.5 — заметная. 0 = выкл.
    vignette=0.15,

    # Скорость: 0.02 = от 0.98× до 1.02× (видео и звук вместе, длительность ±2 %).
    #   Больше 0.05 — заметно ускорено/замедлено. 0 = выкл.
    speed=0.02,

    # Срез кадров в начале и в конце: (1, 4) -> по 1–4 кадра с каждой стороны. (0, 0) = выкл.
    trim_frames=(1, 4),

    # Аудио:
    audio_pitch=0.008,  # высота тона: 0.008 = ±0.8 % (≈±0.14 полутона, на слух не заметно).
    volume=1.0,         # громкость в дБ: 1.0 = ±1 дБ.
    eq_gain=2.0,        # эквалайзер: одна случайная полоса ±2 дБ. Меняет аудио-отпечаток.
    # ---- Незаметные глазу изменения (добавлены; 0 или (0, 0) = выкл) ----

    # Цветовая температура: сдвиг теплее/холоднее (красный против синего). 0.02 = ±2 %.
    #   Глаз воспринимает как другой баланс белого. Больше 0.06 — кадр заметно жёлтый/синий.
    color_temp=0.02,

    # Линза (бочкообразная дисторсия): 0.01 -> центр чуть «выпуклый», края слегка сжаты.
    #   Нелинейно меняет геометрию — хорошо сбивает pHash и нейросетевые отпечатки,
    #   а глазу не видно. Больше 0.05 — эффект «рыбьего глаза». Чёрных углов нет.
    lens=0.01,

    # Дрейф камеры: кадр медленно «плавает» на N пикселей (период 6–12 с).
    #   3 -> до ±3 px — как лёгкое дыхание при съёмке с рук. Каждый кадр смещён по-разному.
    #   Больше 15 — заметно «качает». Ограничен запасом от scale.
    drift=3,

    # Выброс кадров: 1 кадр из каждых N заменяется предыдущим. (90, 150) -> 1 из 90–150
    #   (при 30 fps — раз в 3–5 с). Ломает покадровое сравнение. Меньше 30 — видны рывки.
    frame_drop=(90, 150),

    # Шумоподавление (hqdn3d) ДО добавления своего зерна: убирает родной шум матрицы
    #   камеры. 1.0 — едва заметно. Больше 4 — «пластиковая» кожа. 0 = выкл.
    denoise=1.0,

    # Фоновый шум в аудио (розовый), в дБ: -66 — не слышно, но меняет аудио-отпечаток
    #   в тишине. -50 и громче — слышно шипение. 0 = выкл. (STRENGTH не влияет)
    audio_noise=-66,

    # Ширина стерео: 0.05 = ±5 %. Только для стерео-звука. 0 = выкл.
    stereo=0.05,
)

# Готовые пресеты (используются при LEVEL = "low" / "medium" / "high"). Ключи — как в FILTERS.
PRESETS = {
    "low": dict(mirror=True, scale=(1.010, 1.020), rotate=0.0, brightness=0.010, contrast=0.015,
                saturation=0.03, gamma=0.02, hue=1.0, noise=(1, 3), sharpen=0.15, vignette=0.0,
                speed=0.010, trim_frames=(0, 2), audio_pitch=0.004, volume=0.5, eq_gain=1.0,
                color_temp=0.01, lens=0.005, drift=2, frame_drop=(150, 240), denoise=0.8, audio_noise=-66, stereo=0.03),
    "medium": dict(mirror=True, scale=(1.020, 1.040), rotate=0.5, brightness=0.020, contrast=0.030,
                   saturation=0.06, gamma=0.04, hue=2.5, noise=(2, 5), sharpen=0.30, vignette=0.15,
                   speed=0.020, trim_frames=(1, 4), audio_pitch=0.008, volume=1.0, eq_gain=2.0,
                   color_temp=0.02, lens=0.01, drift=3, frame_drop=(90, 150), denoise=1.0, audio_noise=-66, stereo=0.05),
    "high": dict(mirror=True, scale=(1.040, 1.070), rotate=1.2, brightness=0.035, contrast=0.050,
                 saturation=0.10, gamma=0.07, hue=5.0, noise=(4, 8), sharpen=0.50, vignette=0.30,
                 speed=0.035, trim_frames=(2, 8), audio_pitch=0.015, volume=1.5, eq_gain=3.0,
                 color_temp=0.035, lens=0.02, drift=5, frame_drop=(60, 100), denoise=1.5, audio_noise=-66, stereo=0.08),
}

# ---------------------------------------------------------------- кодирование / железо
# Настроено под: AMD Ryzen 5 2600 (6 ядер / 12 потоков), 8 ГБ ОЗУ, AMD Radeon R7 370.
ENCODER = "cpu"         # "cpu" -> libx264 на процессоре. Все функции (своя матрица CQM, AQ,
                        #          удаление подписи энкодера). Ryzen 5 2600: 1080p ≈ 1–2× realtime.
                        # "amd" -> h264_amf на видеокарте (R7 370 = VCE 1.0). В 2–4 раза быстрее,
                        #          но БЕЗ CQM/AQ и качество хуже. Нужен драйвер AMD с AMF.
                        #          Если AMF не заработает, скрипт сам переключится на "cpu".
CODEC = "h264"          # "h264" — рекомендуется. "h265" — файл меньше, но на Ryzen 2600
                        #          в 3–5 раз медленнее; видеокарта R7 370 HEVC не умеет.
CPU_THREADS = os.cpu_count() or 12   # потоки кодирования (на Ryzen 5 2600 = 12)
PRESET = "medium"       # скорость x264: "fast" (быстрее, файл больше), "medium" (баланс),
                        # "slow" (≈в 2 раза дольше, чуть лучше качество на тот же размер)
CRF = (18, 22)          # качество: меньше = лучше и тяжелее. 18 — визуально без потерь,
                        # 23 — по умолчанию x264, 26+ — заметна потеря качества
USE_CQM = True          # своя матрица квантования (только ENCODER="cpu" и CODEC="h264")
AQ_STRENGTH = (0.7, 1.3)  # сила адаптивной квантизации; aq-mode (1/2/3) выбирается случайно
TONEMAP_HDR = True      # HDR-видео с iPhone (HLG/Dolby Vision) переводить в обычный SDR,
                        # иначе цвета будут бледные/серые

# ════════════════════════════════════════════════════════════════════════════════
#              Ниже — код. Для обычной работы менять ничего не нужно.
# ════════════════════════════════════════════════════════════════════════════════

BASE_DIR = Path(__file__).resolve().parent
INPUT_DIR = BASE_DIR / "input_videos"
OUTPUT_DIR = BASE_DIR / "output_videos"

VIDEO_EXTS = {".mp4", ".mov", ".mkv", ".avi", ".webm", ".m4v", ".flv", ".ts", ".wmv"}

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

    # iPhone/Android пишут вертикальное видео как 1920x1080 + флаг поворота на 90°.
    # ffmpeg при декодировании сам поворачивает кадр, поэтому ширину и высоту меняем местами.
    rotation = 0
    for sd in video.get("side_data_list", []):
        if "rotation" in sd:
            rotation = int(float(sd["rotation"]))
    rotation = rotation or int(float(video.get("tags", {}).get("rotate", 0)))
    w, h = int(video["width"]), int(video["height"])
    if abs(rotation) % 180 == 90:
        w, h = h, w
    return {
        "width": w,
        "height": h,
        "rotation": rotation,
        "hdr": video.get("color_transfer") in ("arib-std-b67", "smpte2084"),
        "fps": fps or 30.0,
        "duration": duration,
        "has_audio": audio is not None,
        "sample_rate": int(audio["sample_rate"]) if audio else 48000,
        "channels": int(audio.get("channels", 2)) if audio else 0,
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
    color_temp: float = 0.0
    lens_k1: float = 0.0
    drift_amp: float = 0.0
    drift_period: float = 8.0
    frame_drop_n: int = 0
    frame_drop_k: int = 0
    denoise: float = 0.0
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
    audio_noise_db: float = 0.0
    stereo: float = 1.0
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


def filter_values(level: str) -> dict:
    return dict(FILTERS) if level == "custom" else dict(PRESETS[level])


def make_plan(info: dict, seed: int, intensity: str, strength: float, args) -> Plan:
    rng = random.Random(seed)
    p = filter_values(intensity)
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
    base = 1 + max(0.0, rng.uniform(*p["scale"]) - 1) * s
    plan.scale = round(base * rot_zoom, 5)
    sw, sh = even(w * plan.scale), even(h * plan.scale)
    # Кроп со сдвигом, но в пределах запаса, который остаётся после поворота.
    mx = max(0, (sw - w * rot_zoom) / 2)
    my = max(0, (sh - h * rot_zoom) / 2)
    # Часть запаса оставляем под дрейф камеры.
    plan.drift_amp = round(min(p.get("drift", 0) * s, 0.5 * min(mx, my)), 2)
    plan.drift_period = round(rng.uniform(6, 12), 2)
    mx, my = mx - plan.drift_amp, my - plan.drift_amp
    plan.crop_x = int((sw - w) / 2 + rng.uniform(-mx, mx))
    plan.crop_y = int((sh - h) / 2 + rng.uniform(-my, my))
    plan.mirror = p["mirror"] if args.mirror is None else args.mirror

    plan.brightness = round(sym(p["brightness"]), 4)
    plan.contrast = round(1 + sym(p["contrast"]), 4)
    plan.saturation = round(1 + sym(p["saturation"]), 4)
    plan.gamma = round(1 + sym(p["gamma"]), 4)
    plan.hue_deg = round(sym(p["hue"]), 3)
    plan.noise = round(rng.randint(*p["noise"]) * s)
    plan.sharpen = round(rng.uniform(0.3, 1.0) * p["sharpen"] * s, 3)
    plan.vignette = round(rng.uniform(0.5, 1.0) * p["vignette"] * s, 3) if p["vignette"] else 0.0
    plan.color_temp = round(sym(p.get("color_temp", 0)), 4)
    plan.lens_k1 = -round(rng.uniform(0.5, 1.0) * p.get("lens", 0) * s, 4)
    plan.denoise = round(p.get("denoise", 0) * rng.uniform(0.8, 1.2), 2)
    fd = p.get("frame_drop", (0, 0))
    if fd and fd[1]:
        plan.frame_drop_n = max(10, round(rng.randint(*fd) / max(1.0, s ** 0.5)))
        plan.frame_drop_k = rng.randint(0, plan.frame_drop_n - 1)

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
    plan.audio_noise_db = round(p.get("audio_noise", 0) + rng.uniform(-2, 2), 1) if p.get("audio_noise") else 0.0
    plan.stereo = round(1 + sym(p.get("stereo", 0)), 3)
    plan.out_sample_rate = rng.choice([44100, 48000])
    plan.audio_bitrate = rng.choice(["128k", "144k", "160k", "192k"])

    plan.crf = args.crf if args.crf is not None else rng.randint(*CRF)
    plan.preset = args.preset or PRESET
    plan.aq_mode = rng.choice([1, 2, 3])
    plan.aq_strength = round(rng.uniform(*AQ_STRENGTH), 2)
    plan.psy_rd = f"{rng.uniform(0.8, 1.2):.2f},{rng.uniform(0.0, 0.2):.2f}"
    plan.deblock = f"{rng.randint(-2, 1)},{rng.randint(-2, 1)}"
    fps_round = max(1, round(info["fps"]))
    plan.keyint = fps_round * rng.choice([2, 3, 4, 5, 8, 10])
    plan.bframes = rng.randint(2, 5)
    plan.refs = rng.randint(2, 5)
    plan.me = rng.choice(["hex", "umh"])
    plan.subme = rng.randint(6, 9)
    if args.codec == "h264" and args.encoder == "cpu" and USE_CQM and not args.no_cqm:
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

    v = []
    if info["hdr"] and TONEMAP_HDR:
        # HDR (HLG / PQ) -> SDR BT.709, иначе картинка бледная
        v.append("zscale=t=linear:npl=100,format=gbrpf32le,zscale=p=bt709,"
                 "tonemap=hable:desat=0,zscale=t=bt709:m=bt709:r=tv")
    if plan.frame_drop_n:
        # кадр выбрасывается, при выводе в постоянный FPS на его место встаёт предыдущий
        v.append(f"select='not(eq(mod(n,{plan.frame_drop_n}),{plan.frame_drop_k}))'")
    v.append(f"setpts=(PTS-STARTPTS)/{plan.speed}")
    if plan.denoise:
        d = plan.denoise
        v.append(f"hqdn3d={d}:{d * 0.75:.2f}:{d * 1.5:.2f}:{d * 1.1:.2f}")
    if plan.lens_k1:
        v.append(f"lenscorrection=k1={plan.lens_k1}:k2={plan.lens_k1 / 2:.4f}:i=bilinear")
    if (sw, sh) != (w, h):
        v.append(f"scale={sw}:{sh}:flags={lanczos}:param0=3")
    if plan.rotate_deg:
        v.append(f"rotate={plan.rotate_deg}*PI/180:ow=iw:oh=ih:bilinear=1:fillcolor=black")
    if plan.drift_amp:
        a, T = plan.drift_amp, plan.drift_period
        v.append(f"crop={w}:{h}:'{plan.crop_x}+{a}*sin(2*PI*t/{T})'"
                 f":'{plan.crop_y}+{a}*cos(2*PI*t/{T * 1.37:.2f})'")
    else:
        v.append(f"crop={w}:{h}:{plan.crop_x}:{plan.crop_y}")
    if plan.mirror:
        v.append("hflip")
    v.append(f"eq=brightness={plan.brightness}:contrast={plan.contrast}"
             f":saturation={plan.saturation}:gamma={plan.gamma}")
    if plan.color_temp:
        c = plan.color_temp
        v.append(f"colorbalance=rm={c}:bm={-c}:rh={c / 2:.4f}:bh={-c / 2:.4f}")
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
        *( [f"extrastereo=m={plan.stereo}"] if info.get("channels") == 2 and plan.stereo != 1 else [] ),
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
                  cqm_name: str | None, encoder: str = "cpu") -> list:
    vf, af = build_filters(plan, info)
    cmd = ["ffmpeg", "-hide_banner", "-loglevel", "error", "-stats", "-y",
           "-filter_threads", str(max(1, CPU_THREADS // 2))]
    if plan.trim_start:
        cmd += ["-ss", f"{plan.trim_start}"]
    cmd += ["-i", str(src)]
    if info["duration"] and (plan.trim_start or plan.trim_end):
        kept = info["duration"] - plan.trim_start - plan.trim_end
        cmd += ["-t", f"{kept / plan.speed:.4f}"]

    cmd += ["-map", "0:v:0", "-vf", vf]
    if info["has_audio"] and plan.audio_noise_db:
        amp = 10 ** (plan.audio_noise_db / 20)
        layout = "stereo" if info["channels"] == 2 else "mono"
        fc = (f"[0:a:0]{af}[a0];"
              f"anoisesrc=c=pink:r={plan.out_sample_rate}:a={amp:.6f}:seed={plan.seed % 100000},"
              f"aformat=channel_layouts={layout}[an];"
              f"[a0][an]amix=inputs=2:duration=first:normalize=0[aout]")
        cmd += ["-filter_complex", fc, "-map", "[aout]", "-ac", str(info["channels"] or 2),
                "-c:a", "aac", "-b:a", plan.audio_bitrate, "-ar", str(plan.out_sample_rate)]
    elif info["has_audio"]:
        cmd += ["-map", "0:a:0", "-af", af,
                "-c:a", "aac", "-b:a", plan.audio_bitrate, "-ar", str(plan.out_sample_rate)]

    common = (f"aq-mode={plan.aq_mode}:aq-strength={plan.aq_strength}"
              f":keyint={plan.keyint}:min-keyint={max(1, plan.keyint // 10)}"
              f":bframes={plan.bframes}:ref={plan.refs}:deblock={plan.deblock}")
    if encoder == "amd":
        # AMD AMF (Radeon R7 370 = VCE 1.0): только H.264, без B-кадров, CQM и AQ
        q = plan.crf + 1
        cmd += ["-c:v", "h264_amf", "-usage", "transcoding", "-quality", "quality",
                "-profile:v", "high", "-rc", "cqp", "-qp_i", str(q), "-qp_p", str(q + 2),
                "-g", str(plan.keyint), "-pix_fmt", "yuv420p"]
        tag = []
    elif codec == "h264":
        params = (f"{common}:psy-rd={plan.psy_rd}:me={plan.me}:subme={plan.subme}"
                  f":8x8dct=1:trellis=2")
        if cqm_name:
            params += f":cqmfile={cqm_name}"
        cmd += ["-c:v", "libx264", "-profile:v", "high", "-threads", str(CPU_THREADS),
                "-x264-params", params,
                "-bsf:v", "filter_units=remove_types=6"]   # SEI с настройками x264
        tag = []
    else:
        psy = plan.psy_rd.split(",")[0]
        params = (f"{common}:psy-rd={psy}:me={plan.me}:subme={min(plan.subme, 7)}"
                  f":log-level=error:info=0")                # info=0: без SEI-подписи
        cmd += ["-c:v", "libx265", "-threads", str(CPU_THREADS), "-x265-params", params]
        tag = ["-tag:v", "hvc1"]
    if encoder != "amd":
        cmd += ["-crf", str(plan.crf), "-preset", plan.preset, "-pix_fmt", "yuv420p"] + tag
    if info["hdr"] and TONEMAP_HDR:
        cmd += ["-color_primaries", "bt709", "-color_trc", "bt709", "-colorspace", "bt709"]

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
    name = src.stem if index == 1 else f"{src.stem}_{index}"
    dst = (out_dir / f"{name}{ext}").resolve()

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
            cmd = build_command(src.resolve(), dst, plan, info, args.codec, cqm_name, args.encoder)
            if args.dry_run:
                log(" ".join(f'"{c}"' if " " in c or ";" in c else c for c in cmd))
                return {"source": str(src), "plan": asdict(plan), "dry_run": True}
            log(f"[{src.name} #{index}] попытка {attempt}, seed={attempt_seed}, "
                f"сила={strength:.2f}, {info['width']}x{info['height']}"
                f"{', HDR->SDR' if info['hdr'] and TONEMAP_HDR else ''}, энкодер={args.encoder}")
            proc = subprocess.run(cmd, cwd=tmp)
            if proc.returncode != 0 and args.encoder == "amd":
                log("  AMD AMF не сработал (драйвер/видеокарта) — переключаюсь на процессор (cpu).")
                args.encoder = "cpu"
                plan = make_plan(info, attempt_seed, args.intensity, strength, args)
                if plan.cqm:
                    cqm_name = "cqm.cfg"
                    write_cqm(plan.cqm, Path(tmp) / cqm_name)
                cmd = build_command(src.resolve(), dst, plan, info, args.codec, cqm_name, "cpu")
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
        f"  HFLIP={'да' if p['mirror'] else 'нет'}  speed={p['speed']}  CRF={p['crf']}  aq-mode={p['aq_mode']}"
        f"  aq-strength={p['aq_strength']}  CQM={'да' if p['cqm'] else 'нет'}")


def main() -> None:
    ap = argparse.ArgumentParser(
        description="Уникализация видео: pHash, MD5/SHA-256, Lanczos micro-scaling, custom AQ/CQM.")
    ap.add_argument("input", type=Path, nargs="?", default=INPUT_DIR,
                    help="видеофайл или папка (по умолчанию input_videos рядом со скриптом)")
    ap.add_argument("-o", "--output", type=Path, default=OUTPUT_DIR,
                    help="папка вывода (по умолчанию output_videos рядом со скриптом)")
    ap.add_argument("-n", "--copies", type=int, default=COPIES, help="копий на каждый исходник")
    ap.add_argument("--intensity", choices=["custom", *PRESETS], default=LEVEL,
                    help="custom = значения FILTERS из начала скрипта")
    ap.add_argument("--strength", type=float, default=STRENGTH, help="множитель силы (0.5–2.0)")
    ap.add_argument("--seed", type=int, default=SEED, help="seed для воспроизводимости")
    ap.add_argument("--codec", choices=["h264", "h265"], default=CODEC)
    ap.add_argument("--encoder", choices=["cpu", "amd"], default=ENCODER,
                    help="cpu = libx264 (все функции), amd = h264_amf на видеокарте")
    ap.add_argument("--crf", type=int, help=f"фиксированный CRF (иначе {CRF[0]}–{CRF[1]})")
    ap.add_argument("--preset", help=f"preset x264/x265 (по умолчанию {PRESET})")
    ap.add_argument("--format", default=OUTPUT_FORMAT, help="расширение вывода: mp4, mkv, mov")
    ap.add_argument("--mirror", action=argparse.BooleanOptionalAction, default=None,
                    help="HFLIP: --mirror включить, --no-mirror выключить (иначе из FILTERS)")
    ap.add_argument("--keep-speed", action="store_true", help="не менять темп")
    ap.add_argument("--no-cqm", action="store_true", help="без пользовательской матрицы")
    ap.add_argument("--min-phash", type=float, default=MIN_PHASH,
                    help="минимальная средняя дистанция pHash; иначе повтор с усилением")
    ap.add_argument("--max-attempts", type=int, default=MAX_ATTEMPTS)
    ap.add_argument("--no-verify", action="store_true", help="не считать pHash")
    ap.add_argument("--report", type=Path, help="сохранить JSON-отчёт")
    ap.add_argument("--dry-run", action="store_true", help="только показать команды ffmpeg")
    args = ap.parse_args()

    require_tools()
    if args.encoder == "amd" and args.codec == "h265":
        log("Radeon R7 370 не кодирует H.265 — используется процессор (cpu).")
        args.encoder = "cpu"
    if np is None and not args.no_verify:
        log("Внимание: numpy не установлен — pHash не будет посчитан (pip install numpy).")

    if args.input == INPUT_DIR:
        INPUT_DIR.mkdir(exist_ok=True)
    sources = collect_inputs(args.input)
    if not sources:
        sys.exit(f"Нет видеофайлов для обработки в {args.input}")
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
    try:
        main()
    finally:
        # запуск двойным кликом в Windows: не закрывать окно сразу
        if os.name == "nt" and len(sys.argv) == 1 and sys.stdin and sys.stdin.isatty():
            input("\nНажмите Enter для выхода...")
