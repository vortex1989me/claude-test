# ==============================================================================
#  Новый фон + перекраска одежды для Google Colab (GPU T4) — ОДНА ЯЧЕЙКА, просто запусти.
#  Drive: Мой диск/Colab Notebooks/content/input_videos  ->  .../output_iphone
#  Для каждого видео:
#    1) HFLIP
#    2) Человек аккуратно вырезается (RobustVideoMatting — волосы по прядям, без дрожания)
#    3) Одежда перекрашивается в случайные цвета (лицо, волосы, кожа, тату не трогаются)
#    4) Генерируется НОВАЯ комната с той же планировкой (по карте глубины оригинала),
#       стиль интерьера случайный — ОДИН раз на видео, поэтому фон не «плавает»
#    5) Новый фон двигается ровно так же, как фон в оригинале (тряска камеры с рук)
#    6) Свет на человеке слегка подгоняется под новый фон -> mp4 с оригинальным звуком
#  Результат называется как исходник: IMG_0038.MOV -> output_iphone/IMG_0038.mp4
#  Журнал «стиль фона и цвета одежды»: output_iphone/background_log.csv
# ==============================================================================

# ----------------------------- НАСТРОЙКИ --------------------------------------
BASE_DIR   = "/content/drive/MyDrive/Colab Notebooks/content"
INPUT_DIR  = f"{BASE_DIR}/input_videos"
OUTPUT_DIR = f"{BASE_DIR}/output_iphone"

STYLES = [   # случайно выбирается один на видео; можно дописывать свои (по-английски)
    "cozy scandinavian bedroom, light wood furniture, white walls, green plants",
    "modern minimalist living room, beige walls, soft daylight, linen curtains",
    "loft apartment, exposed red brick wall, warm edison lamps, wooden shelves",
    "boho bedroom, rattan furniture, macrame wall hanging, warm earthy tones",
    "japandi interior, muted earth tones, paper lamp, low wooden furniture",
    "pastel bedroom, soft pink and mint walls, fairy lights, white furniture",
    "classic parisian apartment, white wall moldings, herringbone parquet, mirror",
    "moody room, deep green walls, brass lamps, dark wood bookshelves",
    "bright modern kitchen, white cabinets, marble countertop, morning sunlight",
    "hotel room, warm beige tones, upholstered headboard, bedside lamps",
]
STYLE_SUFFIX = ", empty room, interior photo, realistic, shot on phone, natural light, high detail"
NEG_PROMPT   = "person, people, face, body, text, watermark, logo, blurry, distorted, cartoon, painting"

CLOTHES_CHROMA = (25, 50)  # насыщенность новых цветов одежды (от, до)
BG_STEPS       = 30     # шаги генерации фона (больше = детальнее, дольше; это 1 раз на видео)
DEPTH_SCALE    = 0.85   # насколько строго новая комната повторяет планировку оригинала (0.5–1.0)
MARGIN         = 0.10   # запас фона по краям под движение камеры (доля кадра)
HARMONIZE      = 0.30   # подгонка цвета/света человека под новый фон (0 = выкл, 0.5 = сильно)
SEG_SHORT_SIDE = 512    # разрешение для сегментации одежды по короткой стороне
MAX_FPS        = 30     # если исходник 60 fps — обработаем 30
CRF            = 17     # качество итогового x264 (меньше = лучше)
TEST_SECONDS   = 0      # >0 = обработать только первые N секунд (файл будет с припиской _test)
# ------------------------------------------------------------------------------

import os, sys, glob, json, shutil, subprocess, time, random, csv, gc

def sh(cmd):
    subprocess.run(cmd, shell=True, check=True)

# 1) Drive
from google.colab import drive
drive.mount("/content/drive")
os.makedirs(OUTPUT_DIR, exist_ok=True)

import cv2, numpy as np, torch
from PIL import Image
from tqdm.auto import tqdm

assert torch.cuda.is_available(), "Нет GPU: Среда выполнения -> Сменить среду -> T4 GPU"
DEV = "cuda"

# >>> MODELS
import torch.nn.functional as F
from transformers import AutoModelForSemanticSegmentation, pipeline as hf_pipeline
from diffusers import StableDiffusionControlNetPipeline, ControlNetModel, UniPCMultistepScheduler

def load_first(loader, names, what):
    for n in names:
        try:
            m = loader(n)
            print(f"{what}: {n}")
            return m
        except Exception as e:
            print(f"{n} недоступна ({type(e).__name__}), пробую следующую")
    raise RuntimeError(f"Не удалось скачать: {what}")

print("Загрузка моделей...")
# Вырезание человека (видео-матинг: волосы по прядям, маска не дрожит)
rvm = torch.hub.load("PeterL1n/RobustVideoMatting", "mobilenetv3", trust_repo=True).to(DEV).eval()
# Части тела/одежда (для перекраски одежды)
person_model = load_first(
    lambda n: AutoModelForSemanticSegmentation.from_pretrained(n).to(DEV).half().eval(),
    ["mattmdjaga/segformer_b2_clothes", "sayeed99/segformer_b3_clothes"], "Одежда")
PERSON_LABELS = {int(k): v.lower() for k, v in person_model.config.id2label.items()}
# Глубина (планировка комнаты) и генератор фона
depth_est = load_first(lambda n: hf_pipeline("depth-estimation", model=n, device=0),
                       ["depth-anything/Depth-Anything-V2-Small-hf", "Intel/dpt-hybrid-midas"], "Глубина")
cn_depth = ControlNetModel.from_pretrained("lllyasviel/control_v11f1p_sd15_depth", torch_dtype=torch.float16)
bg_pipe = load_first(
    lambda n: StableDiffusionControlNetPipeline.from_pretrained(
        n, controlnet=cn_depth, torch_dtype=torch.float16,
        safety_checker=None, requires_safety_checker=False).to(DEV),
    ["SG161222/Realistic_Vision_V5.1_noVAE", "stable-diffusion-v1-5/stable-diffusion-v1-5"], "Генератор фона")
bg_pipe.scheduler = UniPCMultistepScheduler.from_config(bg_pipe.scheduler.config)
bg_pipe.set_progress_bar_config(disable=True)
try:
    bg_pipe.vae.enable_slicing()
except AttributeError:
    pass

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
    """RobustVideoMatting: на кадр -> (альфа 0..1, цвет переднего плана без ореола фона)."""
    def reset(self, h):
        self.rec = [None] * 4
        self.ratio = min(1.0, 512 / h)    # рекомендация авторов: ~0.25 для 1080p+
    @torch.no_grad()
    def __call__(self, rgb):
        x = torch.from_numpy(rgb).to(DEV).permute(2, 0, 1)[None].float() / 255.0
        fgr, pha, *self.rec = rvm(x, *self.rec, downsample_ratio=self.ratio)
        a = pha[0, 0].clamp(0, 1).cpu().numpy()
        f = (fgr[0].clamp(0, 1).permute(1, 2, 0).cpu().numpy() * 255 + 0.5).astype(np.uint8)
        return a, f

@torch.no_grad()
def make_plate(frame_rgb, person_mask, cw, ch, style):
    """Новая комната той же планировки размером cw x ch (кадр + запас по краям)."""
    H0, W0 = frame_rgb.shape[:2]
    k = 384 / min(W0, H0)                                   # глубина в малом размере — быстро
    w, h = int(W0 * k), int(H0 * k)
    small = cv2.resize(frame_rgb, (w, h), interpolation=cv2.INTER_AREA)
    depth = np.array(depth_est(Image.fromarray(small))["depth"].convert("L").resize((w, h)))
    # Убираем человека из карты глубины (дорисовываем стену/мебель за ним) и добавляем запас по краям
    hole = cv2.resize((person_mask > 0.3).astype(np.uint8), (w, h), interpolation=cv2.INTER_NEAREST)
    hole = cv2.dilate(hole, np.ones((9, 9), np.uint8))
    depth = cv2.inpaint(depth, hole, 9, cv2.INPAINT_TELEA)
    px, py = int((cw - W0) / 2 * k), int((ch - H0) / 2 * k)
    depth = cv2.copyMakeBorder(depth, py, py, px, px, cv2.BORDER_REPLICATE)
    depth = cv2.GaussianBlur(depth, (0, 0), 1.5)
    gh = 1024 if ch >= cw else 576                         # разрешение генерации (кратно 8)
    gw = int(round(gh * cw / ch / 8)) * 8
    ctrl = Image.fromarray(cv2.resize(depth, (gw, gh), interpolation=cv2.INTER_AREA)).convert("RGB")
    img = bg_pipe(prompt=style + STYLE_SUFFIX, negative_prompt=NEG_PROMPT, image=ctrl,
                  num_inference_steps=BG_STEPS, guidance_scale=7.0,
                  controlnet_conditioning_scale=DEPTH_SCALE,
                  generator=torch.Generator(DEV).manual_seed(random.randrange(2**31))).images[0]
    return cv2.resize(np.array(img), (cw, ch), interpolation=cv2.INTER_LANCZOS4)
# <<< MODELS

matter = Matter()

# 2) Какие части человека не трогаем при перекраске одежды
KEEP_WORDS = ("hair", "face", "arm", "leg", "glass", "skin")
P_BG   = [i for i, n in PERSON_LABELS.items() if n.startswith("background")]
P_KEEP = [i for i, n in PERSON_LABELS.items() if any(w in n for w in KEEP_WORDS)]
P_RECOLOR = [i for i in PERSON_LABELS if i not in P_BG + P_KEEP]

# 3) Утилиты
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

def seg_size(w, h):
    k = SEG_SHORT_SIDE / min(w, h)
    return max(32, round(w * k / 32) * 32), max(32, round(h * k / 32) * 32)

def random_palette(n):
    hues = np.array([random.uniform(0, 2 * np.pi) for _ in range(n)], np.float32)
    cmin = np.array([random.uniform(*CLOTHES_CHROMA) for _ in range(n)], np.float32)
    return np.stack([np.cos(hues), np.sin(hues), cmin], 1)

def lab_hex(v):
    lab = np.array([[[60.0, v[0] * v[2], v[1] * v[2]]]], np.float32)
    r, g, b = (np.clip(cv2.cvtColor(lab, cv2.COLOR_Lab2RGB)[0, 0], 0, 1) * 255).astype(int)
    return f"#{r:02x}{g:02x}{b:02x}"

def skin_like(rgb):
    ycc = cv2.cvtColor(rgb, cv2.COLOR_RGB2YCrCb).astype(np.float32)
    cr, cb = ycc[..., 1], ycc[..., 2]
    return ((cr > 135) & (cr < 175) & (cb > 85) & (cb < 135)).astype(np.float32)

def fill_holes(mask, max_frac=0.03):
    """Закрашивает «дырки» внутри кожи (татуировки, родинки, подвески), чтобы их не перекрасить."""
    m = (mask > 0.5).astype(np.uint8)
    m = cv2.morphologyEx(m, cv2.MORPH_CLOSE, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (9, 9)))
    n, lab, stats, _ = cv2.connectedComponentsWithStats(1 - m, connectivity=4)
    h, w = m.shape
    out = m.copy()
    for k in range(1, n):
        x, y, bw, bh, area = stats[k]
        if not (x == 0 or y == 0 or x + bw == w or y + bh == h) and area < max_frac * h * w:
            out[lab == k] = 1
    return out.astype(np.float32)

def clothes_ctrl(small, p_pal):
    """Карта перекраски одежды: (dir_a, dir_b, chroma_min, вес). Фон не трогаем — он заменяется."""
    pp = person_probs(small)
    p_bg = pp[..., P_BG].sum(-1)
    on_person = 1 - p_bg
    p_keep = np.maximum(pp[..., P_KEEP].sum(-1), skin_like(small) * on_person)
    p_keep = np.maximum(p_keep, fill_holes(p_keep) * on_person)
    p_keep = cv2.GaussianBlur(p_keep, (0, 0), 1.5)
    pr = pp[..., P_RECOLOR]
    wsum = pr.sum(-1)
    tgt = (pr @ p_pal) / np.maximum(wsum, 1e-6)[..., None]
    weight = np.clip(wsum, 0, 1) * (1 - np.clip(p_keep, 0, 1))
    return np.dstack([tgt, weight]).astype(np.float32), pr

def recolor(rgb, ctrl_full, harm_ab=(0.0, 0.0), harm_l=0.0):
    lab = cv2.cvtColor(rgb.astype(np.float32) / 255.0, cv2.COLOR_RGB2Lab)
    L, a, b = lab[..., 0], lab[..., 1], lab[..., 2]
    da, db, cmin, w = (ctrl_full[..., i] for i in range(4))
    norm = np.maximum(np.hypot(da, db), 1e-6)
    da, db = da / norm, db / norm
    chroma = np.hypot(a, b)
    lum_k = np.clip(np.minimum(L, 100 - L) / 35.0, 0.25, 1.0)
    new_c = np.maximum(chroma, cmin * lum_k)
    lab[..., 1] = w * da * new_c + (1 - w) * a + harm_ab[0]
    lab[..., 2] = w * db * new_c + (1 - w) * b + harm_ab[1]
    lab[..., 0] = np.clip(L + harm_l, 0, 100)
    return np.clip(cv2.cvtColor(lab, cv2.COLOR_Lab2RGB), 0, 1)

class BgTracker:
    """Движение фона оригинала: весь видимый фон сравнивается с опорным кадром (метод ECC),
    человек исключён. Сравнение НАПРЯМУЮ с опорным кадром, а не по цепочке, — ошибки не копятся.
    Если опорный фон почти ушёл из кадра, опорным становится текущий кадр."""
    CRIT = (cv2.TERM_CRITERIA_EPS | cv2.TERM_CRITERIA_COUNT, 50, 1e-5)

    def __init__(self, gray, alpha_s, scale):
        self.S = scale                       # малый кадр -> полный
        self.A = np.eye(3)                   # кадр 0 -> текущий (полный кадр)
        self._set_key(gray, alpha_s)

    @staticmethod
    def _prep(gray):
        return cv2.GaussianBlur(gray, (0, 0), 1).astype(np.float32)

    def _set_key(self, gray, alpha_s):
        self.key, self.key_alpha, self.A_key = self._prep(gray), alpha_s, self.A.copy()
        self.Wm = np.eye(2, 3, dtype=np.float32)                 # ключ -> текущий (малый кадр)

    def update(self, gray, alpha_s):
        """Возвращает матрицу «кадр 0 -> текущий» в координатах полного кадра (или None)."""
        person = (self.key_alpha > 0.3) | (alpha_s > 0.3)
        mask = (cv2.dilate(person.astype(np.uint8), np.ones((9, 9), np.uint8)) == 0).astype(np.uint8)
        if mask.mean() < 0.05:                                   # фона почти не видно
            return None
        try:
            _, Wn = cv2.findTransformECC(self.key, self._prep(gray), self.Wm.copy(),
                                         cv2.MOTION_AFFINE, self.CRIT, mask, 5)
        except cv2.error:
            self._set_key(gray, alpha_s)                         # не сошлось — новый опорный кадр
            return None
        self.Wm = Wn
        self.A = self.S @ np.vstack([Wn, [0, 0, 1]]) @ np.linalg.inv(self.S) @ self.A_key
        # Опорный кадр «уехал» больше чем на 15% — делаем опорным текущий
        if np.abs(Wn[:, 2]).max() > 0.15 * min(gray.shape) or abs(np.linalg.det(Wn[:, :2]) - 1) > 0.15:
            self._set_key(gray, alpha_s)
        return self.A

def process_video(src, dst, log_rows):
    t0 = time.time()
    W, H, fps, hdr = probe(src)
    fps = min(fps, MAX_FPS)
    sw, shh = seg_size(W, H)
    tmp = "/content/_newbg"
    shutil.rmtree(tmp, ignore_errors=True)
    os.makedirs(f"{tmp}/in"); os.makedirs(f"{tmp}/out")

    # HFLIP + (HDR->SDR) + fps
    limit = f"-t {TEST_SECONDS}" if TEST_SECONDS > 0 else ""
    base_vf = f"hflip,fps={fps},scale={W}:{H}"
    tonemap = ("zscale=t=linear:npl=100,format=gbrpf32le,zscale=p=bt709,"
               "tonemap=tonemap=hable:desat=0,zscale=t=bt709:m=bt709:r=tv,format=yuv420p,")
    try:
        sh(f'ffmpeg -v error {limit} -i "{src}" -vf "{tonemap if hdr else ""}{base_vf}" "{tmp}/in/%06d.bmp"')
    except subprocess.CalledProcessError:
        print("Тонмаппинг HDR недоступен, извлекаю без него")
        for f in glob.glob(f"{tmp}/in/*.bmp"): os.remove(f)
        sh(f'ffmpeg -v error {limit} -i "{src}" -vf "{base_vf}" "{tmp}/in/%06d.bmp"')
    frames = sorted(glob.glob(f"{tmp}/in/*.bmp"))

    # --- Новый фон: один раз на видео, по первому кадру ---
    matter.reset(H)
    first = cv2.cvtColor(cv2.imread(frames[0]), cv2.COLOR_BGR2RGB)
    a0, _ = matter(first)
    mx, my = int(W * MARGIN), int(H * MARGIN)
    cw, ch = W + 2 * mx, H + 2 * my
    style = random.choice(STYLES)
    print("Стиль фона:", style)
    plate = make_plate(first, a0, cw, ch, style)
    cv2.imwrite(f"{OUTPUT_DIR}/{os.path.splitext(os.path.basename(dst))[0]}_background.jpg",
                cv2.cvtColor(plate, cv2.COLOR_RGB2BGR), [cv2.IMWRITE_JPEG_QUALITY, 92])

    # Подгонка света: насколько новый фон теплее/холоднее/светлее старого
    lab_old = cv2.cvtColor(first.astype(np.float32) / 255, cv2.COLOR_RGB2Lab)[a0 < 0.1].mean(0)
    lab_new = cv2.cvtColor(plate.astype(np.float32) / 255, cv2.COLOR_RGB2Lab).reshape(-1, 3).mean(0)
    harm_l = HARMONIZE * 0.5 * (lab_new[0] - lab_old[0])
    harm_ab = (HARMONIZE * (lab_new[1] - lab_old[1]), HARMONIZE * (lab_new[2] - lab_old[2]))

    # --- Покадрово ---
    matter.reset(H)
    p_pal = random_palette(len(PERSON_LABELS))[P_RECOLOR]
    p_area = np.zeros(len(P_RECOLOR))
    A = np.eye(3)                                   # движение фона: кадр 0 -> текущий
    T = np.array([[1, 0, -mx], [0, 1, -my], [0, 0, 1]], np.float64)   # холст фона -> кадр 0
    S = np.diag([W / sw, H / shh, 1.0])             # малый кадр -> полный
    tracker, lost = None, 0
    for i, fp in enumerate(tqdm(frames, desc=os.path.basename(src))):
        full = cv2.cvtColor(cv2.imread(fp), cv2.COLOR_BGR2RGB)
        alpha, fgr = matter(full)

        # Движение камеры по фону оригинала (человек исключён из расчёта)
        small = cv2.resize(full, (sw, shh), interpolation=cv2.INTER_AREA)
        gray = cv2.cvtColor(small, cv2.COLOR_RGB2GRAY)
        a_s = cv2.resize(alpha, (sw, shh))
        if tracker is None:
            tracker = BgTracker(gray, a_s, S)
        else:
            A_t = tracker.update(gray, a_s)
            if A_t is None:
                lost += 1
            else:
                A = A_t
        bg = cv2.warpAffine(plate, (A @ T)[:2], (W, H), flags=cv2.INTER_LINEAR,
                            borderMode=cv2.BORDER_REFLECT).astype(np.float32) / 255

        # Перекраска одежды (на «чистом» цвете человека от матинга — без ореола старого фона)
        ctrl, pr = clothes_ctrl(small, p_pal)
        p_area += pr.reshape(-1, len(P_RECOLOR)).sum(0)
        person = recolor(fgr, cv2.resize(ctrl, (W, H), interpolation=cv2.INTER_LINEAR), harm_ab, harm_l)

        a = alpha[..., None]
        out = a * person + (1 - a) * bg
        cv2.imwrite(f"{tmp}/out/{i + 1:06d}.bmp",
                    cv2.cvtColor((np.clip(out, 0, 1) * 255 + 0.5).astype(np.uint8), cv2.COLOR_RGB2BGR))
    if lost:
        print(f"Движение фона не удалось измерить на {lost} кадрах (там фон держится на месте)")

    # Сборка, звук из оригинала
    audio_in  = f'{limit} -i "{src}"' if has_audio(src) else ""
    audio_map = "-map 1:a:0 -c:a aac -b:a 192k -shortest" if audio_in else ""
    sh(f'ffmpeg -v error -y -framerate {fps} -i "{tmp}/out/%06d.bmp" {audio_in} '
       f'-map 0:v:0 {audio_map} -vf "scale=out_color_matrix=bt709:out_range=tv,format=yuv420p" '
       f'-colorspace bt709 -color_primaries bt709 -color_trc bt709 '
       f'-c:v libx264 -preset medium -crf {CRF} -pix_fmt yuv420p -movflags +faststart "{dst}"')
    shutil.rmtree(tmp, ignore_errors=True)

    total = len(frames) * sw * shh
    name = os.path.basename(dst)
    log_rows.append([name, "фон", style, "", ""])
    for j, c in enumerate(P_RECOLOR):
        if p_area[j] / total > 0.005:
            log_rows.append([name, "одежда", PERSON_LABELS[c], f"{100 * p_area[j] / total:.1f}%", lab_hex(p_pal[j])])
    print(f"Время: {(time.time() - t0) / 60:.1f} мин")

# 4) Обработка всех видео (уже готовые пропускаются — можно перезапускать)
EXTS = (".mp4", ".mov", ".m4v", ".mkv", ".avi", ".webm")
videos = sorted(f for f in glob.glob(f"{INPUT_DIR}/*") if f.lower().endswith(EXTS))
print(f"Найдено видео: {len(videos)}")
suffix = "_test" if TEST_SECONDS > 0 else ""
log_path = f"{OUTPUT_DIR}/background_log.csv"
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
            if new_log: wr.writerow(["файл", "что", "описание", "площадь", "новый цвет"])
            wr.writerows(rows)
        print("Готово:", dst)
        for r in rows: print("   ", " | ".join(x for x in r[1:] if x))
    except Exception as e:
        print("Ошибка на", src, "->", e)
    gc.collect(); torch.cuda.empty_cache()
print("Всё.")
