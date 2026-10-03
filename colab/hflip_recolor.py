# ==============================================================================
#  HFLIP + перекраска одежды + лёгкое кадрирование (кроп/зум/поворот) — Google Colab (GPU T4), ОДНА ЯЧЕЙКА.
#  Drive: Мой диск/Colab Notebooks/content/input_videos  ->  .../output_iphone
#  Для каждого видео:
#    1) HFLIP (зеркально)
#    2) Одежда перекрашивается в случайный естественный цвет; лицо, волосы, кожа, тату — нет
#    3) Кадрирование: кроп по краям со случайным сдвигом центра, зум, лёгкий поворот —
#       всё одной операцией (Lanczos), без чёрных углов, обратно в исходный размер
#  Результат называется как исходник: IMG_0038.MOV -> output_iphone/IMG_0038.mp4
#  Журнал: output_iphone/edit_log.csv (цвет, кроп, зум, угол, сдвиг для каждого видео)
# ==============================================================================

# ----------------------------- НАСТРОЙКИ --------------------------------------
BASE_DIR   = "/content/drive/MyDrive/Colab Notebooks/content"
INPUT_DIR  = f"{BASE_DIR}/input_videos"
OUTPUT_DIR = f"{BASE_DIR}/output_iphone"

TEST_SECONDS = 5        # >0 = обработать только первые N секунд (файл будет с припиской _test); 0 = всё видео

HFLIP = True            # зеркальное отражение

RECOLOR_CLOTHES = True  # перекрашивать одежду в естественный цвет
CLOTHES_COLORS = ["#7b2d3a", "#2c3e66", "#6b7445", "#c99a2e", "#b5583c", "#c98d94",
                  "#8fa58a", "#9d8ac0", "#2f7f7f", "#b08a5a", "#3a5bb0", "#2f7a55"]

# ---------------------------------------------------------------- кроп
CROP = (0.03, 0.05)     # обрезка по краям 3–5 % (случайно на видео) и растяжение обратно в исходный размер.
                        # None = выключено.
CROP_SHIFT = True       # центр кропа случайно смещается в пределах обрезанных краёв

# ---------------------------------------------------------------- зум
ZOOM = (1.03, 1.05)     # приближение 3–5 % с обрезкой краёв обратно в исходный размер.
                        # МАКСИМУМ без потери качества: 1.06. До 1.06 размытие на глаз не
                        # видно (Lanczos), дальше картинка мягчеет и заметно «наезжает».
                        # Скрипт сам увеличит зум, если его не хватает на поворот. None = выключено.

# ---------------------------------------------------------------- поворот
ROTATE = 1.5            # поворот на случайный угол в пределах ±1.5°. МАКСИМУМ: 1.5°. Дальше заметен
                        # завал горизонта. Цена: поворот 1.5° на вертикальном видео съедает ≈4.6 % зума
                        # (иначе были бы чёрные углы) — скрипт добавит это автоматически. 0 = выключено.

# Кроп и зум — по сути одна операция (обрезать края и растянуть обратно), поэтому они НЕ
# складываются: берётся наибольшее из трёх (кроп, зум, запас на поворот). Иначе 5 % + 5 % = 10 %
# и картинка заметно мягчеет.

MAX_FPS = 30            # если исходник 60 fps — обработаем 30
CRF     = 17            # качество итогового x264 (меньше = лучше)
# ------------------------------------------------------------------------------

import os, sys, glob, json, shutil, subprocess, time, random, csv, gc, math

def sh(cmd):
    subprocess.run(cmd, shell=True, check=True)

# 1) Drive
from google.colab import drive
drive.mount("/content/drive")
os.makedirs(OUTPUT_DIR, exist_ok=True)

import cv2, numpy as np, torch
from tqdm.auto import tqdm

assert torch.cuda.is_available(), "Нет GPU: Среда выполнения -> Сменить среду -> T4 GPU"
DEV = "cuda"

# >>> MODELS
import torch.nn.functional as F
from transformers import AutoModelForSemanticSegmentation

print("Загрузка моделей...")
rvm = torch.hub.load("PeterL1n/RobustVideoMatting", "mobilenetv3", trust_repo=True).to(DEV).eval()
person_model = None
for n in ["mattmdjaga/segformer_b2_clothes", "sayeed99/segformer_b3_clothes"]:
    try:
        person_model = AutoModelForSemanticSegmentation.from_pretrained(n).to(DEV).half().eval()
        print("Одежда:", n); break
    except Exception as e:
        print(f"{n} недоступна ({type(e).__name__})")
PERSON_LABELS = {int(k): v.lower() for k, v in person_model.config.id2label.items()} if person_model else {}

MEAN = torch.tensor([0.485, 0.456, 0.406], device=DEV).view(1, 3, 1, 1)
STD  = torch.tensor([0.229, 0.224, 0.225], device=DEV).view(1, 3, 1, 1)

@torch.no_grad()
def person_probs(rgb):
    h, w = rgb.shape[:2]
    x = torch.from_numpy(rgb).to(DEV).permute(2, 0, 1)[None].float() / 255.0
    x = ((x - MEAN) / STD).half()
    logits = person_model(pixel_values=x).logits.float()
    logits = F.interpolate(logits, size=(h, w), mode="bilinear", align_corners=False)
    return logits.softmax(1)[0].permute(1, 2, 0).cpu().numpy()

class Matter:
    """RobustVideoMatting: альфа человека (0..1) по кадрам, с памятью между кадрами."""
    def reset(self, h):
        self.rec = [None] * 4
        self.ratio = min(1.0, 512 / h)
    @torch.no_grad()
    def __call__(self, rgb):
        x = torch.from_numpy(rgb).to(DEV).permute(2, 0, 1)[None].float() / 255.0
        _, pha, *self.rec = rvm(x, *self.rec, downsample_ratio=self.ratio)
        return pha[0, 0].clamp(0, 1).cpu().numpy()
# <<< MODELS

matter = Matter()
KEEP_WORDS = ("hair", "face", "arm", "leg", "glass", "skin")
P_BG   = [i for i, n in PERSON_LABELS.items() if n.startswith("background")]
P_KEEP = [i for i, n in PERSON_LABELS.items() if any(w in n for w in KEEP_WORDS)]
P_RECOLOR = [i for i in PERSON_LABELS if i not in P_BG + P_KEEP]

# 2) Утилиты
def probe(path):
    out = subprocess.check_output(
        f'ffprobe -v error -select_streams v:0 '
        f'-show_entries stream=width,height,r_frame_rate,color_transfer:stream_tags=rotate:stream_side_data=rotation '
        f'-of json "{path}"', shell=True)
    s = json.loads(out)["streams"][0]
    n, d = s["r_frame_rate"].split("/")
    w, h = int(s["width"]), int(s["height"])
    rot = s.get("tags", {}).get("rotate")                 # вертикальные видео iPhone
    for sd in s.get("side_data_list", []):
        rot = sd.get("rotation", rot)
    if rot is not None and abs(int(float(rot))) % 180 == 90:
        w, h = h, w
    hdr = s.get("color_transfer") in ("arib-std-b67", "smpte2084")
    return w, h, float(n) / float(d), hdr

def has_audio(path):
    out = subprocess.check_output(
        f'ffprobe -v error -select_streams a -show_entries stream=index -of csv=p=0 "{path}"', shell=True)
    return bool(out.strip())

def size_short(w, h, short, mult):
    k = short / min(w, h)
    return max(mult, round(w * k / mult) * mult), max(mult, round(h * k / mult) * mult)

def hex_rgb(hx):
    return np.array([int(hx[i:i + 2], 16) for i in (1, 3, 5)], np.float32) / 255

# 3) Кадрирование: кроп + зум + поворот одной матрицей
def framing(W, H):
    """Случайные параметры на видео -> матрица 2x3 (выход -> источник) и описание для журнала."""
    crop = random.uniform(*CROP) if CROP else 0.0
    zoom = random.uniform(*ZOOM) if ZOOM else 1.0
    ang = random.uniform(-ROTATE, ROTATE) if ROTATE else 0.0
    t = math.radians(abs(ang))
    rot_need = max(math.cos(t) + H / W * math.sin(t), math.cos(t) + W / H * math.sin(t))   # без чёрных углов
    rot_need += 6.0 / min(W, H) if ang else 0.0                                              # +пара пикселей запаса
    s = max(1.0 / (1.0 - crop), zoom, rot_need)
    # куда можно сдвинуть центр, чтобы углы кадра не вышли за исходник
    c, si = math.cos(math.radians(ang)), math.sin(math.radians(ang))
    qx = max(abs(c * x + si * y) for x in (-W / 2, W / 2) for y in (-H / 2, H / 2)) / s
    qy = max(abs(-si * x + c * y) for x in (-W / 2, W / 2) for y in (-H / 2, H / 2)) / s
    mx, my = max(0.0, W / 2 - qx - 2), max(0.0, H / 2 - qy - 2)          # запас 2 px
    dx = random.uniform(-0.9, 0.9) * mx if CROP_SHIFT else 0.0
    dy = random.uniform(-0.9, 0.9) * my if CROP_SHIFT else 0.0
    # выход p -> источник: R(-ang) * (p - центр) / s + центр + сдвиг
    M = np.array([[c / s, si / s, 0.0], [-si / s, c / s, 0.0]])
    M[:, 2] = np.array([W / 2 + dx, H / 2 + dy]) - M[:, :2] @ np.array([W / 2, H / 2])
    info = f"кроп {100 * crop:.1f}%, зум {100 * (zoom - 1):.1f}%, поворот {ang:+.2f}°, " \
           f"итог x{s:.3f}, сдвиг ({dx:+.0f}, {dy:+.0f}) px"
    return M, info

# 4) Перекраска одежды (естественный цвет: оттенок + светлота «ткани»)
def clothes_recolor(rgb, w, target_rgb, gain):
    """w — вес одежды (0..1) в полном разрешении."""
    x = rgb.astype(np.float32) / 255.0
    lin = x ** 2.2 * (1 + w * (gain - 1))[..., None]
    lab = cv2.cvtColor(np.clip(lin, 0, 1) ** (1 / 2.2), cv2.COLOR_RGB2Lab)
    t_lab = cv2.cvtColor(target_rgb[None, None], cv2.COLOR_RGB2Lab)[0, 0]
    tc = float(np.hypot(t_lab[1], t_lab[2])) + 1e-6
    da, db = t_lab[1] / tc, t_lab[2] / tc
    L, a, b = lab[..., 0], lab[..., 1], lab[..., 2]
    lum_k = np.clip(np.minimum(L, 100 - L) / 35.0, 0.25, 1.0)
    new_c = np.maximum(np.hypot(a, b), tc * lum_k)
    lab[..., 1] = w * da * new_c + (1 - w) * a
    lab[..., 2] = w * db * new_c + (1 - w) * b
    return (np.clip(cv2.cvtColor(lab, cv2.COLOR_Lab2RGB), 0, 1) * 255 + 0.5).astype(np.uint8)

def skin_like(rgb):
    ycc = cv2.cvtColor(rgb, cv2.COLOR_RGB2YCrCb).astype(np.float32)
    cr, cb = ycc[..., 1], ycc[..., 2]
    return ((cr > 135) & (cr < 175) & (cb > 85) & (cb < 135)).astype(np.float32)

def fill_holes(mask, max_frac=0.03):
    m = (mask > 0.5).astype(np.uint8)
    m = cv2.morphologyEx(m, cv2.MORPH_CLOSE, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (9, 9)))
    n, lab, st, _ = cv2.connectedComponentsWithStats(1 - m, connectivity=4)
    h, w = m.shape
    out = m.copy()
    for k in range(1, n):
        x, y, bw, bh, area = st[k]
        if not (x == 0 or y == 0 or x + bw == w or y + bh == h) and area < max_frac * h * w:
            out[lab == k] = 1
    return out.astype(np.float32)

def clothes_weight(rgb_small, alpha_small):
    pp = person_probs(rgb_small)
    on_person = 1 - pp[..., P_BG].sum(-1)
    keep = np.maximum(pp[..., P_KEEP].sum(-1), skin_like(rgb_small) * on_person)
    keep = np.maximum(keep, fill_holes(keep) * on_person)
    cl = pp[..., P_RECOLOR].sum(-1)
    # Пропуски внутри одежды (складки, тени, кружево) — закрываем, чтобы не было серых пятен
    solid = cv2.morphologyEx((cl > 0.35).astype(np.uint8), cv2.MORPH_CLOSE,
                             cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (11, 11))).astype(np.float32)
    w = np.maximum(cl, solid * 0.95) * (keep < 0.5) * np.clip(alpha_small * 1.5, 0, 1)
    return np.clip(cv2.GaussianBlur(w.astype(np.float32), (0, 0), 1.2), 0, 1)

# 5) Видео целиком
def process_video(src, dst, log_rows):
    t0 = time.time()
    W, H, fps, hdr = probe(src)
    fps = min(fps, MAX_FPS)
    sw, shh = size_short(W, H, 512, 32)               # для поиска одежды
    tmp = "/content/_edit"
    shutil.rmtree(tmp, ignore_errors=True)
    for d in ("in", "out"):
        os.makedirs(f"{tmp}/{d}")

    limit = f"-t {TEST_SECONDS}" if TEST_SECONDS > 0 else ""
    base_vf = f"{'hflip,' if HFLIP else ''}fps={fps},scale={W}:{H}"
    tonemap = ("zscale=t=linear:npl=100,format=gbrpf32le,zscale=p=bt709,"
               "tonemap=tonemap=hable:desat=0,zscale=t=bt709:m=bt709:r=tv,format=yuv420p,")
    try:
        sh(f'ffmpeg -v error {limit} -i "{src}" -vf "{tonemap if hdr else ""}{base_vf}" "{tmp}/in/%06d.bmp"')
    except subprocess.CalledProcessError:
        for f in glob.glob(f"{tmp}/in/*.bmp"): os.remove(f)
        sh(f'ffmpeg -v error {limit} -i "{src}" -vf "{base_vf}" "{tmp}/in/%06d.bmp"')
    frames = sorted(glob.glob(f"{tmp}/in/*.bmp"))

    use_clothes = RECOLOR_CLOTHES and person_model is not None
    c_hex = random.choice(CLOTHES_COLORS)
    c_rgb = hex_rgb(c_hex)
    M, frame_info = framing(W, H)
    use_frame = bool(CROP or ZOOM or ROTATE)
    print(("Цвет одежды: " + c_hex + " | ") * use_clothes + ("Кадр: " + frame_info) * use_frame)

    matter.reset(H)
    prev_w, gain = None, 1.0
    for i, fp in enumerate(tqdm(frames, desc=os.path.basename(src))):
        out = cv2.cvtColor(cv2.imread(fp), cv2.COLOR_BGR2RGB)
        if use_clothes:
            alpha = matter(out)
            small = cv2.resize(out, (sw, shh), interpolation=cv2.INTER_AREA)
            w = clothes_weight(small, cv2.resize(alpha, (sw, shh)).astype(np.float32))
            if prev_w is not None:
                w = 0.4 * w + 0.6 * prev_w                # меньше дрожания краёв
            prev_w = w
            if i == 0:                                   # светлота «ткани» под выбранный цвет
                wm = w > 0.5
                y_old = ((small[wm].astype(np.float32) / 255) ** 2.2 @ [0.2126, 0.7152, 0.0722]).mean() if wm.sum() > 50 else 0.2
                y_new = (c_rgb ** 2.2) @ np.array([0.2126, 0.7152, 0.0722], np.float32)
                gain = float(np.clip(y_new / max(y_old, 1e-3), 0.35, 1.6))
            out = clothes_recolor(out, cv2.resize(w, (W, H), interpolation=cv2.INTER_LINEAR), c_rgb, gain)
        if use_frame:
            out = cv2.warpAffine(out, M, (W, H), flags=cv2.INTER_LANCZOS4 | cv2.WARP_INVERSE_MAP,
                                 borderMode=cv2.BORDER_REFLECT)
        cv2.imwrite(f"{tmp}/out/{i + 1:06d}.bmp", cv2.cvtColor(out, cv2.COLOR_RGB2BGR))

    audio_in  = f'{limit} -i "{src}"' if has_audio(src) else ""
    audio_map = "-map 1:a:0 -c:a aac -b:a 192k -shortest" if audio_in else ""
    sh(f'ffmpeg -v error -y -framerate {fps} -i "{tmp}/out/%06d.bmp" {audio_in} '
       f'-map 0:v:0 {audio_map} -vf "scale=out_color_matrix=bt709:out_range=tv,format=yuv420p" '
       f'-colorspace bt709 -color_primaries bt709 -color_trc bt709 '
       f'-c:v libx264 -preset medium -crf {CRF} -pix_fmt yuv420p -movflags +faststart "{dst}"')
    shutil.rmtree(tmp, ignore_errors=True)

    name = os.path.basename(dst)
    log_rows.append([name, "hflip" if HFLIP else "-", c_hex if use_clothes else "-",
                     frame_info if use_frame else "-"])
    print(f"Время: {(time.time() - t0) / 60:.1f} мин")

# 6) Обработка всех видео (уже готовые пропускаются — можно перезапускать)
EXTS = (".mp4", ".mov", ".m4v", ".mkv", ".avi", ".webm")
videos = sorted(f for f in glob.glob(f"{INPUT_DIR}/*") if f.lower().endswith(EXTS))
print(f"Найдено видео: {len(videos)}")
suffix = "_test" if TEST_SECONDS > 0 else ""
log_path = f"{OUTPUT_DIR}/edit_log.csv"
for src in videos:
    dst = f"{OUTPUT_DIR}/{os.path.splitext(os.path.basename(src))[0]}{suffix}.mp4"
    if os.path.exists(dst):
        print("Пропуск (уже есть, удалите файл чтобы переделать):", dst); continue
    rows = []
    try:
        process_video(src, dst, rows)
        new_log = not os.path.exists(log_path)
        with open(log_path, "a", newline="", encoding="utf-8-sig") as f:
            wr = csv.writer(f, delimiter=";")
            if new_log: wr.writerow(["файл", "отражение", "цвет одежды", "кадр"])
            wr.writerows(rows)
        print("Готово:", dst)
    except Exception as e:
        print("Ошибка на", src, "->", e)
    gc.collect(); torch.cuda.empty_cache()
print("Всё.")
