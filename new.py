#!/usr/bin/env python3
"""
new.py — уникализация видео: HFLIP + зум + поворот + Mesh Warp (OpenCV).

Структура папок (рядом со скриптом):
  new.py
  input_videos/   <- сюда кладём исходные видео
  output_videos/  <- сюда попадают готовые копии (IMG_0205.MOV -> IMG_0205.mp4)

Запуск: двойной клик по new.py или в консоли `python new.py`.
Установка один раз:  pip install opencv-python numpy
Нужен ffmpeg + ffprobe в PATH.

Как это работает:
  ffmpeg декодирует видео в «сырые» кадры -> Python/OpenCV делает ВСЮ геометрию
  (зеркало, зум, поворот, mesh warp) ОДНОЙ интерполяцией Lanczos -> ffmpeg (x264) кодирует.
  Одна интерполяция вместо нескольких подряд = максимум резкости.
  Звук копируется без изменений.
"""

from __future__ import annotations

import datetime as dt
import hashlib
import json
import math
import os
import queue
import random
import shutil
import subprocess
import sys
import tempfile
import threading
import time
import uuid
from pathlib import Path

try:
    import cv2
    import numpy as np
except ImportError:
    sys.exit("Нужны OpenCV и numpy. Установите:  pip install opencv-python numpy")

# ╔══════════════════════════════════════════════════════════════════════════════╗
# ║                                 НАСТРОЙКИ                                    ║
# ╚══════════════════════════════════════════════════════════════════════════════╝
# Диапазон (мин, макс) — для каждой копии берётся случайное значение внутри.
# Одно число X — случайное значение от -X до +X.

COPIES = 1              # сколько копий делать из каждого видео
SEED = None             # None = каждый запуск разный; число = одинаковый результат
OUTPUT_FORMAT = "mp4"   # "mp4" / "mov" / "mkv" / None (как у исходника)

# ---------------------------------------------------------------- HFLIP
MIRROR = True           # зеркало по горизонтали. Качество не теряется вообще (пиксели
                        # просто переставляются). Минус: текст в кадре станет зеркальным.

# ---------------------------------------------------------------- зум (Lanczos)
ZOOM = (1.03, 1.05)     # приближение 3–5 % с обрезкой краёв обратно в исходный размер.
                        # МАКСИМУМ без потери качества: 1.06. До 1.06 размытие на глаз не
                        # видно (Lanczos), дальше картинка мягчеет и заметно «наезжает».
                        # Скрипт сам увеличит зум, если его не хватает на поворот и mesh.

# ---------------------------------------------------------------- поворот
ROTATE = 1.0            # поворот ±1.0°. МАКСИМУМ: 1.5°. Дальше заметен завал горизонта.
                        # Цена: поворот 1.5° на вертикальном видео съедает ≈4.6 % зума
                        # (иначе были бы чёрные углы) — скрипт добавит это автоматически.

# ---------------------------------------------------------------- Mesh Warp (OpenCV)
# Кадр покрывается сеткой контрольных точек; каждая точка сдвигается на случайные
# несколько пикселей, между точками смещение плавно интерполируется (бикубически).
# Получается нелинейное «пластичное» искажение: глазу почти не видно, но геометрия
# каждого участка кадра своя — это ломает сравнение по pHash/ключевым точкам.
MESH_STRENGTH = 6.0     # макс. сдвиг точки в пикселях для видео 1080 px по короткой стороне
                        # (для других разрешений пересчитывается пропорционально).
                        # МАКСИМУМ без видимых искажений: 6 (≈0.55 % кадра).
                        # 8–10 — на прямых линиях (дверные косяки, мебель) видны изгибы.
MESH_CELL = 360         # размер ячейки сетки в px (при 1080 px). Меньше ячейка — искажение
                        # «мельче» и заметнее. 300–450 — норма; меньше 250 не ставьте.
MESH_ANIMATE = True     # True — сетка медленно «дышит» (каждый кадр чуть разный) — сильнее
                        # для уникализации. False — одно статичное искажение на всё видео.
MESH_PERIOD = (6, 10)   # период «дыхания» в секундах. МИНИМУМ 5 — быстрее будет эффект «желе».

# ---------------------------------------------------------------- качество / железо
INTERPOLATION = "lanczos"   # "lanczos" — максимум резкости; "cubic" — в ~3 раза быстрее,
                            # разница на глаз почти не видна
CRF = 18                # качество x264: 18 — визуально без потерь (рекомендуется), 16 — ещё
                        # лучше, файл тяжелее; 23 — заметно хуже
PRESET = "medium"       # "fast" / "medium" / "slow"
CPU_THREADS = os.cpu_count() or 12   # Ryzen 5 2600 = 12 потоков
STALL_TIMEOUT = 60      # если N секунд нет ни одного нового кадра — процесс останавливается
TONEMAP_HDR = True      # HDR с iPhone -> обычный SDR (иначе бледные цвета)

# ════════════════════════════════════════════════════════════════════════════════
#              Ниже — код. Для обычной работы менять ничего не нужно.
# ════════════════════════════════════════════════════════════════════════════════

BASE_DIR = Path(__file__).resolve().parent
INPUT_DIR = BASE_DIR / "input_videos"
OUTPUT_DIR = BASE_DIR / "output_videos"
VIDEO_EXTS = {".mp4", ".mov", ".mkv", ".avi", ".webm", ".m4v", ".flv", ".ts", ".wmv"}
NO_STDIN = subprocess.DEVNULL


def log(msg: str = "") -> None:
    print(msg, file=sys.stderr, flush=True)


def run_quiet(cmd: list, timeout: int = 120) -> subprocess.CompletedProcess:
    return subprocess.run(cmd, capture_output=True, text=True, stdin=NO_STDIN, timeout=timeout)


def even(x: float) -> int:
    return max(2, int(round(x / 2.0)) * 2)


# --------------------------------------------------------------------------- probe

def probe(path: Path) -> dict:
    out = run_quiet(["ffprobe", "-v", "error", "-print_format", "json",
                     "-show_format", "-show_streams", str(path)], 60)
    data = json.loads(out.stdout or "{}")
    streams = data.get("streams", [])
    video = next((s for s in streams if s["codec_type"] == "video"), None)
    if video is None:
        raise RuntimeError("в файле нет видеопотока")
    audio = next((s for s in streams if s["codec_type"] == "audio"), None)

    rate = video.get("r_frame_rate") or "30/1"
    num, _, den = rate.partition("/")
    fps = float(num) / float(den or 1) if float(den or 1) else 30.0
    if not 1 <= fps <= 240:
        rate, fps = "30/1", 30.0

    # вертикальное видео с телефона: 1920x1080 + флаг поворота 90° -> на деле 1080x1920
    rotation = 0
    for sd in video.get("side_data_list", []):
        if "rotation" in sd:
            rotation = int(float(sd["rotation"]))
    rotation = rotation or int(float(video.get("tags", {}).get("rotate", 0)))
    w, h = int(video["width"]), int(video["height"])
    if abs(rotation) % 180 == 90:
        w, h = h, w

    return {
        "width": even(w) if w % 2 else w, "height": even(h) if h % 2 else h,
        "rate": rate, "fps": fps,
        "duration": float(data.get("format", {}).get("duration") or video.get("duration") or 0),
        "hdr": video.get("color_transfer") in ("arib-std-b67", "smpte2084"),
        "color": {k: video.get(k) for k in ("color_space", "color_primaries",
                                            "color_transfer", "color_range")},
        "audio_codec": audio.get("codec_name") if audio else None,
    }


# --------------------------------------------------------------------------- геометрия

class Geometry:
    """Все преобразования собраны в одну карту remap: для каждого пикселя результата —
    координата в исходном кадре. Тогда кадр интерполируется ОДИН раз."""

    def __init__(self, w: int, h: int, rng: random.Random):
        self.w, self.h = w, h
        k = min(w, h) / 1080.0                       # пересчёт пикселей под разрешение
        self.mirror = MIRROR
        self.angle = rng.uniform(0.4, 1.0) * ROTATE * rng.choice([-1, 1]) if ROTATE else 0.0
        self.mesh_amp = MESH_STRENGTH * k
        margin = self.mesh_amp * 1.3 + 2             # запас под mesh (с учётом перелёта кубики)

        th = math.radians(abs(self.angle))
        cx, cy = (w - 1) / 2, (h - 1) / 2
        ext_x = cx * math.cos(th) + cy * math.sin(th)  # полуразмеры повёрнутого кадра
        ext_y = cx * math.sin(th) + cy * math.cos(th)
        need = max(ext_x / (cx - margin), ext_y / (cy - margin))
        self.zoom = max(rng.uniform(*ZOOM), need * 1.002)
        # оставшийся запас -> случайный сдвиг центра кадрирования
        mx = max(0.0, cx - margin - ext_x / self.zoom)
        my = max(0.0, cy - margin - ext_y / self.zoom)
        self.ox = rng.uniform(-mx, mx) * 0.8
        self.oy = rng.uniform(-my, my) * 0.8

        # базовая карта: зеркало -> поворот -> зум -> сдвиг (обратное отображение)
        xs, ys = np.meshgrid(np.arange(w, dtype=np.float32), np.arange(h, dtype=np.float32))
        if self.mirror:
            xs = (w - 1) - xs
        dx, dy = xs - cx, ys - cy
        c, s = math.cos(math.radians(self.angle)), math.sin(math.radians(self.angle))
        self.bx = ((c * dx + s * dy) / self.zoom + cx + self.ox).astype(np.float32)
        self.by = ((-s * dx + c * dy) / self.zoom + cy + self.oy).astype(np.float32)
        # карты для цветовых каналов (в yuv420p они вдвое меньше)
        hw, hh = w // 2, h // 2
        self.cbx = (cv2.resize(self.bx, (hw, hh), interpolation=cv2.INTER_LINEAR) - 0.5) / 2
        self.cby = (cv2.resize(self.by, (hw, hh), interpolation=cv2.INTER_LINEAR) - 0.5) / 2

        # mesh: сетка точек, у каждой своя амплитуда, фаза и период по X и Y
        cell = MESH_CELL * k
        self.gx = max(3, round(w / cell) + 1)
        self.gy = max(3, round(h / cell) + 1)
        shape = (self.gy, self.gx)

        def grid(lo=0.5):
            return np.array([[rng.uniform(lo, 1.0) for _ in range(self.gx)]
                             for _ in range(self.gy)], np.float32).reshape(shape)
        self.amp_x, self.amp_y = grid() * self.mesh_amp, grid() * self.mesh_amp
        self.ph_x = grid(0) * 2 * math.pi
        self.ph_y = grid(0) * 2 * math.pi
        self.period = rng.uniform(*MESH_PERIOD)
        self.static_t = rng.uniform(0, self.period)
        self.interp = cv2.INTER_LANCZOS4 if INTERPOLATION == "lanczos" else cv2.INTER_CUBIC
        self._static = None

    def maps(self, t: float):
        if not MESH_ANIMATE:
            if self._static is None:
                self._static = self._build(self.static_t)
            return self._static
        return self._build(t)

    def _build(self, t: float):
        if self.mesh_amp <= 0:
            return self.bx, self.by, self.cbx, self.cby
        ph = 2 * math.pi * t / self.period
        gx = (self.amp_x * np.sin(ph + self.ph_x)).astype(np.float32)
        gy = (self.amp_y * np.sin(ph * 0.83 + self.ph_y)).astype(np.float32)
        fx = cv2.resize(gx, (self.w, self.h), interpolation=cv2.INTER_CUBIC)
        fy = cv2.resize(gy, (self.w, self.h), interpolation=cv2.INTER_CUBIC)
        hw, hh = self.w // 2, self.h // 2
        cfx = cv2.resize(gx, (hw, hh), interpolation=cv2.INTER_CUBIC) / 2
        cfy = cv2.resize(gy, (hw, hh), interpolation=cv2.INTER_CUBIC) / 2
        return self.bx + fx, self.by + fy, self.cbx + cfx, self.cby + cfy

    def apply(self, frame: np.ndarray, t: float) -> np.ndarray:
        w, h = self.w, self.h
        y = frame[:w * h].reshape(h, w)
        u = frame[w * h:w * h * 5 // 4].reshape(h // 2, w // 2)
        v = frame[w * h * 5 // 4:].reshape(h // 2, w // 2)
        mx, my, cmx, cmy = self.maps(t)
        b = cv2.BORDER_REFLECT
        return np.concatenate([
            cv2.remap(y, mx, my, self.interp, borderMode=b).ravel(),
            cv2.remap(u, cmx, cmy, self.interp, borderMode=b).ravel(),
            cv2.remap(v, cmx, cmy, self.interp, borderMode=b).ravel(),
        ])


# --------------------------------------------------------------------------- конвейер

class Stalled(RuntimeError):
    pass


def color_args(info: dict) -> list:
    if info["hdr"] and TONEMAP_HDR:
        return ["-colorspace", "bt709", "-color_primaries", "bt709",
                "-color_trc", "bt709", "-color_range", "tv"]
    c, out = info["color"], []
    for opt, key in (("-colorspace", "color_space"), ("-color_primaries", "color_primaries"),
                     ("-color_trc", "color_transfer"), ("-color_range", "color_range")):
        if c.get(key) and c[key] != "unknown":
            out += [opt, c[key]]
    return out


def transform_video(src: Path, dst: Path, info: dict, geo: Geometry) -> None:
    """ffmpeg (декодер) -> OpenCV -> ffmpeg (x264). Три потока, связанные очередями;
    если очередь стоит дольше STALL_TIMEOUT — всё останавливается, а не висит вечно."""
    w, h = info["width"], info["height"]
    fsize = w * h * 3 // 2
    vf = [f"scale={w}:{h}:flags=lanczos"]          # на случай нечётных размеров исходника
    if info["hdr"] and TONEMAP_HDR:
        vf.insert(0, "zscale=t=linear:npl=100,format=gbrpf32le,zscale=p=bt709,"
                     "tonemap=hable:desat=0,zscale=t=bt709:m=bt709:r=tv")
    vf += [f"fps={info['rate']}", "format=yuv420p"]
    dec_cmd = ["ffmpeg", "-nostdin", "-hide_banner", "-loglevel", "error", "-i", str(src),
               "-map", "0:v:0", "-vf", ",".join(vf), "-f", "rawvideo", "-pix_fmt", "yuv420p", "-"]
    enc_cmd = ["ffmpeg", "-nostdin", "-hide_banner", "-loglevel", "error", "-y",
               "-f", "rawvideo", "-pix_fmt", "yuv420p", "-s", f"{w}x{h}", "-r", info["rate"],
               "-i", "-", "-c:v", "libx264", "-preset", PRESET, "-crf", str(CRF),
               "-threads", str(CPU_THREADS), "-profile:v", "high", "-pix_fmt", "yuv420p",
               *color_args(info),
               "-bsf:v", "filter_units=remove_types=6",      # SEI с подписью x264
               "-fflags", "+bitexact", "-flags:v", "+bitexact", str(dst)]

    logs = [tempfile.TemporaryFile(), tempfile.TemporaryFile()]
    dec = subprocess.Popen(dec_cmd, stdout=subprocess.PIPE, stderr=logs[0], stdin=NO_STDIN,
                           bufsize=fsize * 2)
    enc = subprocess.Popen(enc_cmd, stdin=subprocess.PIPE, stderr=logs[1], stdout=NO_STDIN)
    q_in, q_out = queue.Queue(maxsize=8), queue.Queue(maxsize=8)
    errors: list = []

    def reader():
        try:
            while True:
                buf = dec.stdout.read(fsize)
                if not buf or len(buf) < fsize:
                    break
                q_in.put(buf)
        except Exception as e:  # noqa: BLE001
            errors.append(e)
        finally:
            q_in.put(None)

    def writer():
        try:
            while True:
                item = q_out.get()
                if item is None:
                    break
                enc.stdin.write(item)
        except Exception as e:  # noqa: BLE001
            errors.append(e)
        finally:
            try:
                enc.stdin.close()
            except Exception:  # noqa: BLE001
                pass

    th = [threading.Thread(target=reader, daemon=True), threading.Thread(target=writer, daemon=True)]
    for t in th:
        t.start()

    total = int(info["duration"] * info["fps"]) or 0
    n, t0, last_print = 0, time.monotonic(), 0.0
    try:
        while True:
            try:
                buf = q_in.get(timeout=STALL_TIMEOUT)
            except queue.Empty:
                raise Stalled(f"декодер {STALL_TIMEOUT} с не выдаёт кадры")
            if buf is None:
                break
            frame = np.frombuffer(buf, np.uint8)
            out = geo.apply(frame, n / info["fps"])
            try:
                q_out.put(out.tobytes(), timeout=STALL_TIMEOUT)
            except queue.Full:
                raise Stalled(f"энкодер {STALL_TIMEOUT} с не принимает кадры")
            n += 1
            now = time.monotonic()
            if now - last_print > 0.5:
                last_print = now
                fps = n / max(1e-6, now - t0)
                pct = min(100.0, 100 * n / total) if total else 0.0
                bar = "#" * int(pct / 5) + "-" * (20 - int(pct / 5))
                print(f"\r  [{bar}] {pct:5.1f}%  {n}/{total or '?'} кадров, {fps:4.1f} fps   ",
                      end="", file=sys.stderr, flush=True)
        q_out.put(None, timeout=STALL_TIMEOUT)
        th[1].join(timeout=STALL_TIMEOUT * 3)
        if th[1].is_alive():
            raise Stalled("энкодер не завершился")
        enc.wait(timeout=STALL_TIMEOUT * 3)
        dec.wait(timeout=30)
    except BaseException:
        for p in (dec, enc):
            if p.poll() is None:
                p.kill()
        raise
    finally:
        print(file=sys.stderr)

    def tail(f):
        f.seek(0)
        return f.read().decode("utf-8", "replace").strip().splitlines()[-3:]
    if errors or enc.returncode != 0 or n == 0:
        raise RuntimeError(f"кодирование не удалось (кадров {n}): "
                           f"{errors or ''} {tail(logs[0])} {tail(logs[1])}")


def mux(video: Path, src: Path, dst: Path, info: dict, meta: dict) -> None:
    """Видео + оригинальный звук без перекодирования, чистые метаданные."""
    def cmd(audio_codec: list) -> list:
        c = ["ffmpeg", "-nostdin", "-hide_banner", "-loglevel", "error", "-y", "-i", str(video)]
        if info["audio_codec"]:
            c += ["-i", str(src), "-map", "0:v:0", "-map", "1:a:0", "-shortest"] + audio_codec
        else:
            c += ["-map", "0:v:0"]
        c += ["-c:v", "copy", "-map_metadata", "-1", "-map_metadata:s:v", "-1",
              "-map_metadata:s:a", "-1", "-map_chapters", "-1",
              "-fflags", "+bitexact", "-flags:v", "+bitexact", "-flags:a", "+bitexact",
              "-metadata", f"creation_time={meta['creation_time']}",
              "-metadata", f"comment={meta['comment']}",
              "-metadata:s:v:0", "handler_name=VideoHandler",
              "-metadata:s:v:0", "encoder=AVC Coding"]
        if dst.suffix.lower() in (".mp4", ".mov", ".m4v"):
            c += ["-movflags", "+faststart"]
        return c + [str(dst)]

    r = run_quiet(cmd(["-c:a", "copy"]), 600)
    if r.returncode != 0 and info["audio_codec"]:          # кодек звука не лезет в контейнер
        r = run_quiet(cmd(["-c:a", "aac", "-b:a", "192k"]), 600)
    if r.returncode != 0:
        raise RuntimeError("сборка файла: " + " ".join(r.stderr.strip().splitlines()[-3:]))


def duration_of(path: Path) -> float:
    try:
        r = run_quiet(["ffprobe", "-v", "error", "-show_entries", "format=duration",
                       "-of", "default=nw=1:nk=1", str(path)], 60)
        return float(r.stdout.strip() or 0)
    except (ValueError, subprocess.TimeoutExpired):
        return 0.0


# --------------------------------------------------------------------------- pHash (отчёт)

def phash(path: Path, t: float) -> int | None:
    r = subprocess.run(["ffmpeg", "-nostdin", "-v", "error", "-ss", f"{t:.3f}", "-i", str(path),
                        "-frames:v", "1", "-vf", "scale=32:32:flags=area,format=gray",
                        "-f", "rawvideo", "-"], capture_output=True, stdin=NO_STDIN, timeout=60)
    if len(r.stdout) < 1024:
        return None
    img = np.frombuffer(r.stdout[:1024], np.uint8).reshape(32, 32).astype(np.float32)
    low = cv2.dct(img)[:8, :8].flatten()
    bits = low > np.median(low)
    return int("".join("1" if b else "0" for b in bits), 2)


def phash_distance(a: Path, b: Path, duration: float, n: int = 6):
    d = []
    for i in range(n):
        t = duration * (i + 0.5) / n
        try:
            x, y = phash(a, t), phash(b, t)
        except subprocess.TimeoutExpired:
            continue
        if x is not None and y is not None:
            d.append(bin(x ^ y).count("1"))
    return round(sum(d) / len(d), 1) if d else None


def md5(path: Path) -> str:
    h = hashlib.md5()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()


# --------------------------------------------------------------------------- main

def process(src: Path, index: int, seed: int) -> None:
    info = probe(src)
    rng = random.Random(seed)
    geo = Geometry(info["width"], info["height"], rng)
    ext = OUTPUT_FORMAT or src.suffix.lstrip(".")
    name = src.stem if index == 1 else f"{src.stem}_{index}"
    dst = OUTPUT_DIR / f"{name}.{ext.lstrip('.')}"
    stamp = dt.datetime(2019, 1, 1, tzinfo=dt.timezone.utc) + dt.timedelta(
        seconds=rng.randint(0, 6 * 365 * 86400))
    meta = {"creation_time": stamp.strftime("%Y-%m-%dT%H:%M:%S.000000Z"),
            "comment": uuid.UUID(int=rng.getrandbits(128)).hex}

    log(f"[{src.name} -> {dst.name}] {info['width']}x{info['height']}, "
        f"{info['duration']:.1f} c{', HDR->SDR' if info['hdr'] and TONEMAP_HDR else ''}")
    log(f"  HFLIP={'да' if geo.mirror else 'нет'}  зум x{geo.zoom:.4f}  поворот {geo.angle:+.2f}°"
        f"  mesh ±{geo.mesh_amp:.1f}px сетка {geo.gx}x{geo.gy}"
        f"{f', период {geo.period:.1f} c' if MESH_ANIMATE else ', статичный'}")

    with tempfile.TemporaryDirectory() as tmp:
        vtmp, otmp = Path(tmp) / "video.mp4", Path(tmp) / f"out{dst.suffix}"
        transform_video(src, vtmp, info, geo)
        mux(vtmp, src, otmp, info, meta)
        got = duration_of(otmp)
        if info["duration"] and got < info["duration"] * 0.9:
            raise RuntimeError(f"длительность {got:.1f} c вместо {info['duration']:.1f} c")
        if dst.exists():
            dst.unlink()
        shutil.move(str(otmp), str(dst))

    ph = phash_distance(src, dst, info["duration"]) if info["duration"] else None
    log(f"  готово: MD5 {md5(dst)}" + (f", pHash {ph} из 64" if ph is not None else ""))


def main() -> None:
    import argparse
    ap = argparse.ArgumentParser(description="HFLIP + зум + поворот + OpenCV Mesh Warp")
    ap.add_argument("input", nargs="?", type=Path, default=INPUT_DIR,
                    help="видео или папка (по умолчанию input_videos)")
    ap.add_argument("-n", "--copies", type=int, default=COPIES)
    ap.add_argument("--seed", type=int, default=SEED)
    args = ap.parse_args()

    for tool in ("ffmpeg", "ffprobe"):
        if shutil.which(tool) is None:
            sys.exit(f"Ошибка: {tool} не найден в PATH.")
    INPUT_DIR.mkdir(exist_ok=True)
    OUTPUT_DIR.mkdir(exist_ok=True)
    cv2.setNumThreads(CPU_THREADS)

    if args.input.is_dir():
        sources = sorted(p for p in args.input.iterdir() if p.suffix.lower() in VIDEO_EXTS)
    else:
        sources = [args.input] if args.input.is_file() else []
    if not sources:
        sys.exit(f"Нет видео в {args.input}")

    master = random.Random(args.seed)
    ok = failed = 0
    for src in sources:
        for i in range(1, args.copies + 1):
            try:
                process(src, i, master.getrandbits(32))
                ok += 1
            except KeyboardInterrupt:
                log("\nОстановлено.")
                sys.exit(1)
            except Exception as e:  # noqa: BLE001 — идём дальше по списку
                failed += 1
                log(f"  ОШИБКА: {e}")
    log(f"Готово: {ok} успешно, {failed} с ошибкой.")


if __name__ == "__main__":
    try:
        main()
    finally:
        # запуск двойным кликом в Windows: окно не закрывается сразу
        if os.name == "nt" and len(sys.argv) == 1 and sys.stdin and sys.stdin.isatty():
            input("\nНажмите Enter для выхода...")
