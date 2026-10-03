# ==============================================================================
#  Удаление/замена предметов на фоне + перекраска одежды — Google Colab (GPU T4), ОДНА ЯЧЕЙКА.
#  Drive: Мой диск/Colab Notebooks/content/input_videos  ->  .../output_iphone
#  Для каждого видео:
#    1) HFLIP
#    2) Из всех кадров собирается «чистый фон» (где человек закрывал фон в одном кадре —
#       этот участок берётся из другого кадра, где человек отошёл). На нём ищутся ОТДЕЛЬНЫЕ НЕБОЛЬШИЕ предметы
#       (SAM режет кадр на куски, ADE20K говорит, что это: шкафы/двери/стены/окна/кровати и т.п.
#       не трогаются никогда; куски у края кадра, полоски и крупные области — тоже).
#       REMOVE_COUNT — это МАКСИМУМ: если подходящих 2, уберутся 2. Двигающиеся предметы пропускаются
#    3) Удаление: камера почти неподвижна -> LaMa дорисовывает фон на одном кадре (чётко),
#       дальше он переносится на все кадры со светом/экспозицией оригинала;
#       камера двигается -> ProPainter берёт настоящий фон из кадров, где он виден
#    3б) (по желанию, только неподвижная камера) часть убранных мелких предметов заменяется
#       новым похожим предметом (Stable Diffusion Inpainting на одном кадре + перенос на все кадры)
#    4) Одежда (по желанию) перекрашивается в естественный цвет; лицо, волосы, кожа, тату — нет
#    5) Всё, что не правилось, — пиксели оригинала в полном разрешении
#  Результат называется как исходник: IMG_0038.MOV -> output_iphone/IMG_0038.mp4
#  Отчёт: output_iphone/IMG_0038_debug.jpg (жёлтым — подходящие предметы, красным — убранные,
#         зелёным — заменённые), output_iphone/IMG_0038_cleaned_frame.jpg — опорный кадр после правки,
#         output_iphone/IMG_0038_candidates.jpg — все куски SAM подходящего размера и почему отсеяны
# ==============================================================================

# ----------------------------- НАСТРОЙКИ --------------------------------------
BASE_DIR   = "/content/drive/MyDrive/Colab Notebooks/content"
INPUT_DIR  = f"{BASE_DIR}/input_videos"
OUTPUT_DIR = f"{BASE_DIR}/output_iphone"

REMOVE_COUNT   = 7      # МАКСИМУМ предметов на видео (подходящих меньше — уберёт сколько есть, лишнего не тронет)
OBJ_MIN_AREA   = 0.001  # размер предмета: от 0.1% кадра...
OBJ_MAX_AREA   = 0.025  # ...до 2.5% кадра (крупнее — обычно куски мебели)
OBJ_MIN_CONTRAST = 8    # насколько предмет должен отличаться от окружения (меньше = больше кандидатов)
REPLACE_CHANCE = 0.5    # вероятность, что убранный предмет заменится новым похожим (0 = только удалять)
REPLACE_MAX_AREA = 0.025 # заменять только небольшие предметы (до 2.5% кадра)
FILL_METHOD    = "auto" # "auto" | "lama" (чётко, для неподвижной камеры) | "propainter" (для движущейся)
RECOLOR_CLOTHES = True  # перекрашивать одежду в естественный цвет
PROC_SHORT_SIDE = 432   # разрешение для ProPainter по короткой стороне (T4: 384–480)
CHUNK_FRAMES   = 240    # ProPainter обрабатывает видео кусками по столько кадров (память T4)
MAX_FPS        = 30     # если исходник 60 fps — обработаем 30
CRF            = 17     # качество итогового x264 (меньше = лучше)
TEST_SECONDS   = 0      # >0 = обработать только первые N секунд (файл будет с припиской _test)

# Естественные цвета одежды (случайный на видео)
CLOTHES_COLORS = ["#7b2d3a", "#2c3e66", "#6b7445", "#c99a2e", "#b5583c", "#c98d94",
                  "#8fa58a", "#9d8ac0", "#2f7f7f", "#b08a5a", "#3a5bb0", "#2f7a55"]
# ------------------------------------------------------------------------------

import os, sys, glob, json, shutil, subprocess, time, random, csv, gc

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
# ProPainter (код + зависимости; веса он скачает сам при первом запуске)
PP_DIR = "/content/ProPainter"
if not os.path.exists(PP_DIR):
    sh(f"git clone -q --depth 1 https://github.com/sczhou/ProPainter.git {PP_DIR}")
sh(f"{sys.executable} -m pip -q install einops av addict future timm yapf imageio imageio-ffmpeg")
PP_ENV = ""

import torch.nn.functional as F
from transformers import AutoModelForSemanticSegmentation, pipeline as hf_pipeline

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
try:
    ade_model = AutoModelForSemanticSegmentation.from_pretrained(
        "nvidia/segformer-b2-finetuned-ade-512-512").to(DEV).half().eval()
    ADE_LABELS = {int(k): v.lower().split(";")[0].split(",")[0].strip()
                  for k, v in ade_model.config.id2label.items()}
    print("Что за предмет: ADE20K")
except Exception as e:
    ade_model, ADE_LABELS = None, {}
    print(f"ADE20K недоступна ({type(e).__name__}) — без проверки «мебель/стены»")
try:
    sam = hf_pipeline("mask-generation", model="facebook/sam-vit-base", device=0)
    print("Поиск предметов: SAM")
except Exception as e:
    sam = None
    print(f"SAM недоступна ({type(e).__name__}) — будет только поиск «заметных пятен»")

LAMA_PATH = "/content/big-lama.pt"
try:
    if not os.path.exists(LAMA_PATH):
        sh(f"wget -q -O {LAMA_PATH} https://github.com/enesmsahin/simple-lama-inpainting/releases/download/v0.1.0/big-lama.pt")
    lama = torch.jit.load(LAMA_PATH, map_location=DEV).eval()
    print("Дорисовка: LaMa")
except Exception as e:
    lama = None
    print(f"LaMa недоступна ({type(e).__name__}) — всегда ProPainter")

@torch.no_grad()
def lama_inpaint(rgb, mask):
    h, w = mask.shape
    ph8, pw8 = (8 - h % 8) % 8, (8 - w % 8) % 8
    it = torch.from_numpy(np.pad(rgb, ((0, ph8), (0, pw8), (0, 0)), mode="reflect")).to(DEV)
    it = it.permute(2, 0, 1)[None].float() / 255.0
    mt = torch.from_numpy(np.pad(mask.astype(np.float32), ((0, ph8), (0, pw8)), mode="reflect")).to(DEV)
    out = lama(it, (mt[None, None] > 0).float())[0].permute(1, 2, 0).clamp(0, 1).cpu().numpy()
    return (out[:h, :w] * 255 + 0.5).astype(np.uint8)

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

@torch.no_grad()
def ade_classes(rgb, short=768):
    """Карта классов ADE20K (номер класса на пиксель); картинка увеличивается до short по короткой стороне."""
    h, w = rgb.shape[:2]
    k = short / min(h, w)
    big = cv2.resize(rgb, (max(32, round(w * k / 32) * 32), max(32, round(h * k / 32) * 32)),
                     interpolation=cv2.INTER_CUBIC)
    x = torch.from_numpy(big).to(DEV).permute(2, 0, 1)[None].float() / 255.0
    logits = ade_model(pixel_values=((x - MEAN) / STD).half()).logits.float()
    logits = F.interpolate(logits, size=(h, w), mode="bilinear", align_corners=False)
    return logits.argmax(1)[0].cpu().numpy()

_sd = None
def sd_inpaint(rgb, mask, prompt):
    """Stable Diffusion Inpainting: рисует новый предмет внутри маски (rgb/mask 512x512)."""
    global _sd
    from PIL import Image
    if _sd is None:
        try:
            import diffusers
        except ImportError:
            sh(f"{sys.executable} -m pip -q install diffusers accelerate")
        from diffusers import StableDiffusionInpaintPipeline
        _sd = False
        for n in ["stable-diffusion-v1-5/stable-diffusion-inpainting", "Lykon/dreamshaper-8-inpainting",
                  "runwayml/stable-diffusion-inpainting"]:
            try:
                _sd = StableDiffusionInpaintPipeline.from_pretrained(
                    n, torch_dtype=torch.float16, safety_checker=None, requires_safety_checker=False).to(DEV)
                _sd.set_progress_bar_config(disable=True)
                print("Замена предметов:", n); break
            except Exception as e:
                print(f"{n} недоступна ({type(e).__name__})")
    if _sd is False:
        return None
    img = _sd(prompt=prompt + ", realistic photo, same lighting and perspective, natural shadow, sharp",
              negative_prompt="blurry, cartoon, illustration, text, watermark, logo, letters, deformed, "
                              "duplicate, person, hands",
              image=Image.fromarray(rgb), mask_image=Image.fromarray(mask), height=512, width=512,
              num_inference_steps=30, guidance_scale=7.0).images[0]
    return np.asarray(img.convert("RGB"))

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

def sam_masks(rgb):
    """Все маски предметов от SAM (0/1). Пороги снижены, чтобы находить и мелкие предметы."""
    if sam is None:
        return []
    from PIL import Image
    try:
        out = sam(Image.fromarray(rgb), points_per_batch=64, pred_iou_thresh=0.7,
                  stability_score_thresh=0.8)
        masks = [m.cpu().numpy() if hasattr(m, "cpu") else np.asarray(m) for m in out["masks"]]
        return [np.squeeze(m).astype(np.uint8) for m in masks]
    except Exception as e:                                   # сбой SAM не должен останавливать видео
        print(f"SAM не сработала ({type(e).__name__}: {str(e)[:80]}) — только поиск «заметных пятен»")
        return []
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

class BgTracker:
    """Движение камеры относительно кадра 0: весь видимый фон сравнивается с опорным кадром (ECC),
    человек исключён. Без накопления ошибок; опорный кадр обновляется, если фон «уехал»."""
    CRIT = (cv2.TERM_CRITERIA_EPS | cv2.TERM_CRITERIA_COUNT, 50, 1e-5)
    def __init__(self, gray, alpha_s, scale):
        self.S, self.A = scale, np.eye(3)
        self._set_key(gray, alpha_s)
    @staticmethod
    def _prep(gray):
        return cv2.GaussianBlur(gray, (0, 0), 1).astype(np.float32)
    def _set_key(self, gray, alpha_s):
        self.key, self.key_alpha, self.A_key = self._prep(gray), alpha_s, self.A.copy()
        self.Wm = np.eye(2, 3, dtype=np.float32)
    def update(self, gray, alpha_s):
        person = (self.key_alpha > 0.3) | (alpha_s > 0.3)
        mask = (cv2.dilate(person.astype(np.uint8), np.ones((9, 9), np.uint8)) == 0).astype(np.uint8)
        if mask.mean() < 0.05:
            return None
        try:
            _, Wn = cv2.findTransformECC(self.key, self._prep(gray), self.Wm.copy(),
                                         cv2.MOTION_AFFINE, self.CRIT, mask, 5)
        except cv2.error:
            self._set_key(gray, alpha_s)
            return None
        self.Wm = Wn
        self.A = self.S @ np.vstack([Wn, [0, 0, 1]]) @ np.linalg.inv(self.S) @ self.A_key
        if np.abs(Wn[:, 2]).max() > 0.15 * min(gray.shape) or abs(np.linalg.det(Wn[:, :2]) - 1) > 0.15:
            self._set_key(gray, alpha_s)
        return self.A

# 3) Поиск предметов на кадре 0
def contrast(lab_img, obj, person=None):
    """Насколько предмет отделён от окружения: разница среднего цвета (ΔE в Lab) или резкость
    его границы (бело-чёрная коробка на серой стене по среднему цвету почти не отличается,
    но край у неё чёткий). Человек не учитывается."""
    ring = cv2.dilate(obj, np.ones((15, 15), np.uint8)) - obj
    edge = obj - cv2.erode(obj, np.ones((3, 3), np.uint8)) | cv2.dilate(obj, np.ones((3, 3), np.uint8)) - obj
    if person is not None:
        obj, ring, edge = obj & (person == 0), ring & (person == 0), edge & (person == 0)
    if ring.sum() < 20 or obj.sum() < 20:
        return 0.0
    de = float(np.linalg.norm(lab_img[obj > 0].mean(0) - lab_img[ring > 0].mean(0)))
    L = cv2.GaussianBlur(lab_img[..., 0], (0, 0), 1)
    grad = np.hypot(cv2.Sobel(L, cv2.CV_32F, 1, 0), cv2.Sobel(L, cv2.CV_32F, 0, 1)) / 4
    sharp = float(np.median(grad[edge > 0])) if edge.sum() > 20 else 0.0
    return max(de, sharp)

def blob_masks(rgb, bg):
    """Запасной поиск без нейросети: области фона, заметно отличающиеся от своего окружения
    (коробки, лампы, вещи на полках, картины, вывески...)."""
    lab = cv2.cvtColor(rgb, cv2.COLOR_RGB2Lab).astype(np.float32)
    k = max(31, (min(rgb.shape[:2]) // 8) | 1)
    base = np.dstack([cv2.medianBlur(np.clip(lab[..., i], 0, 255).astype(np.uint8), k) for i in range(3)])
    diff = np.linalg.norm(lab - base.astype(np.float32), axis=-1)
    m = ((diff > 18) & bg).astype(np.uint8)
    m = cv2.morphologyEx(m, cv2.MORPH_CLOSE, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (9, 9)))
    m = cv2.morphologyEx(m, cv2.MORPH_OPEN, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (5, 5)))
    n, comp = cv2.connectedComponents(m)
    return [(comp == j).astype(np.uint8) for j in range(1, n)]

# Что НИКОГДА не трогаем (куски мебели, стены, проёмы, улица, природа, транспорт)
STRUCT_CLASSES = {
    "wall", "building", "sky", "floor", "tree", "ceiling", "road", "bed", "windowpane", "window", "grass",
    "cabinet", "sidewalk", "person", "earth", "door", "table", "mountain", "curtain", "chair", "car",
    "water", "sofa", "shelf", "house", "sea", "mirror", "rug", "field", "armchair", "seat", "fence", "desk",
    "rock", "wardrobe", "bathtub", "railing", "base", "column", "chest of drawers", "counter", "sand",
    "sink", "skyscraper", "fireplace", "refrigerator", "grandstand", "path", "stairs", "runway",
    "pool table", "screen door", "stairway", "river", "bridge", "bookcase", "blind", "coffee table",
    "toilet", "hill", "bench", "countertop", "stove", "palm", "kitchen island", "swivel chair", "boat",
    "bar", "hovel", "bus", "truck", "tower", "awning", "streetlight", "booth", "airplane", "dirt track",
    "apparel", "pole", "land", "bannister", "escalator", "buffet", "stage", "van", "ship", "fountain",
    "canopy", "washer", "swimming pool", "waterfall", "tent", "minibike", "oven", "step", "tank", "lake",
    "dishwasher", "blanket", "hood", "pier", "shower", "radiator", "bicycle", "animal", "cradle"}
# Что считаем отдельным предметом (в первую очередь)
OBJECT_CLASSES = {
    "box", "bottle", "vase", "lamp", "book", "plant", "flower", "pot", "basket", "bag", "plaything",
    "clock", "ball", "tray", "plate", "glass", "cushion", "pillow", "towel", "sculpture", "fan",
    "ashcan", "painting", "poster", "picture", "sconce", "light", "candlestick", "flowerpot", "food",
    "computer", "monitor", "crt screen", "screen", "television receiver", "kitchen appliance",
    "microwave", "bulletin board", "trade name", "signboard", "flag", "case", "barrel", "stool",
    "ottoman", "toy", "jar", "bowl", "mug", "cup"}
OUTDOOR_CLASSES = {"sky", "road", "sidewalk", "sea", "sand", "grass", "tree", "building", "mountain",
                   "water", "field", "earth", "skyscraper", "land", "palm", "river", "lake", "hill"}

# Короткие коды причин для картинки-диагностики (OpenCV не пишет кириллицу) и их цвета
REASONS = {"на человеке": ("P", (255, 0, 255)), "у края кадра": ("E", (0, 160, 255)),
           "не компактный": ("C", (255, 140, 0)), "мебель/стены": ("F", (255, 0, 0)),
           "слабый контраст": ("L", (160, 160, 160)), "подходят": ("OK", (0, 255, 0))}

def class_info(small, m, person):
    """Что под маской по ADE20K. Считается на УВЕЛИЧЕННОМ куске вокруг предмета — так мелкие вещи
    распознаются как вещи, а не как «шкаф/стена» вокруг них. Возвращает класс, долю «мебели/стен»,
    долю «предметов»."""
    if ade_model is None:
        return "?", 0.0, 0.0
    h, w = m.shape
    x, y, bw, bh = cv2.boundingRect(m)
    pad = max(16, max(bw, bh) // 2)
    X0, Y0, X1, Y1 = max(0, x - pad), max(0, y - pad), min(w, x + bw + pad), min(h, y + bh + pad)
    ade = ade_classes(small[Y0:Y1, X0:X1], short=384)
    sel = (m[Y0:Y1, X0:X1] > 0) & (person[Y0:Y1, X0:X1] == 0)
    if sel.sum() < 10:
        sel = m[Y0:Y1, X0:X1] > 0
    ids, cnt = np.unique(ade[sel], return_counts=True)
    names = [ADE_LABELS.get(int(k), "?") for k in ids]
    tot = max(cnt.sum(), 1)
    struct = sum(c for nm, c in zip(names, cnt) if nm in STRUCT_CLASSES) / tot
    obj = sum(c for nm, c in zip(names, cnt) if nm in OBJECT_CLASSES) / tot
    return names[int(np.argmax(cnt))], float(struct), float(obj)

def find_objects(frame0, alpha0, sw, shh, diag_path=None):
    """frame0/alpha0 — опорный кадр (где фон виден лучше всего).
    Возвращает отдельные небольшие компактные предметы (без дублей), сцену (outdoor) и уменьшенный кадр."""
    small = cv2.resize(frame0, (sw, shh), interpolation=cv2.INTER_AREA)
    lab_img = cv2.cvtColor(small.astype(np.float32) / 255, cv2.COLOR_RGB2Lab)
    a_s = cv2.resize(alpha0, (sw, shh))
    person = cv2.dilate((a_s > 0.2).astype(np.uint8), np.ones((15, 15), np.uint8))
    outdoor = False
    if ade_model is not None:
        ade = ade_classes(small, short=512)
        bg_names = [ADE_LABELS.get(int(k), "?") for k in ade[person == 0].ravel()[::7]]
        outdoor = np.mean([nm in OUTDOOR_CLASSES for nm in bg_names]) > 0.3 if bg_names else False
    stats = {"SAM масок": 0, "не тот размер": 0, "на человеке": 0, "у края кадра": 0,
             "не компактный": 0, "мебель/стены": 0, "слабый контраст": 0, "подходят": 0}
    found, diag = [], []
    for src, masks in (("SAM", sam_masks(small)), ("пятна", blob_masks(small, person == 0))):
        if src == "SAM":
            stats["SAM масок"] = len(masks)
        for m in masks:
            area = m.sum() / (sw * shh)
            if not (OBJ_MIN_AREA <= area <= OBJ_MAX_AREA):
                stats["не тот размер"] += 1; continue
            reason, cls, c, obj = None, "", 0.0, 0.0
            vis = m & (person == 0)                          # видимая часть (без волос/руки поверх)
            x, y, bw, bh = cv2.boundingRect(m)
            if vis.sum() < 0.5 * m.sum():
                reason = "на человеке"
            elif x <= 2 or y <= 2 or x + bw >= sw - 2 or y + bh >= shh - 2:
                reason = "у края кадра"                       # обрезанный край мебели/двери
            else:
                cnts, _ = cv2.findContours(m, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
                hull = cv2.contourArea(cv2.convexHull(np.vstack(cnts))) if cnts else 0
                solidity = m.sum() / max(hull, 1)
                if m.sum() < 0.35 * bw * bh or min(bw, bh) < 0.2 * max(bw, bh) or solidity < 0.7:
                    reason = "не компактный"                  # полоски, контуры, рваные куски
            if reason is None:
                cls, struct, obj = class_info(small, m, person)
                if (struct > 0.6 and obj < 0.15) or (src != "SAM" and obj < 0.3 and ade_model is not None):
                    reason = "мебель/стены"
            if reason is None:
                c = contrast(lab_img, m, person)
                if c < (0.5 * OBJ_MIN_CONTRAST if obj >= 0.3 else 1.5 * OBJ_MIN_CONTRAST):   # опознанным мягче
                    reason = "слабый контраст"
            reason = reason or "подходят"
            stats[reason] += 1
            if src == "SAM":
                diag.append((m, reason, cls, area))
            if reason == "подходят":
                found.append(dict(c=c, area=area, m=m, src=src, cls=cls, obj=obj))
    # Убираем дубли: из вложенных масок берём ВНЕШНЮЮ (вся коробка, а не рисунок на ней)
    found.sort(key=lambda d: -d["area"])
    taken, cands = np.zeros((shh, sw), np.uint8), []
    for d in found:
        if (d["m"] & taken).sum() > 0.3 * d["m"].sum():
            continue
        taken |= d["m"]
        cands.append(d)
    print("Поиск предметов:", ", ".join(f"{k}: {v}" for k, v in stats.items()),
          f"| без дублей: {len(cands)}" + (" | сцена: улица" if outdoor else ""))
    for d in cands:
        print(f"   {d['cls']:<16} {100 * d['area']:.2f}% кадра, контраст {d['c']:.0f}, "
              f"{'предмет' if d['obj'] >= 0.3 else 'не опознан'} ({d['src']})")
    if diag_path:                                            # все куски SAM подходящего размера + причина
        img = (small * 0.6).astype(np.uint8)
        for m, reason, cls, area in sorted(diag, key=lambda t: -t[3]):
            code, col = REASONS[reason]
            cnts, _ = cv2.findContours(m, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
            cv2.drawContours(img, cnts, -1, col, 2 if reason == "подходят" else 1)
            x, y, bw, bh = cv2.boundingRect(m)
            cv2.putText(img, code + (f" {cls}" if cls else ""), (x + 2, y + 12),
                        cv2.FONT_HERSHEY_SIMPLEX, 0.35, col, 1, cv2.LINE_AA)
        legend = "P=person E=edge C=not compact F=furniture/wall L=low contrast OK=fits"
        cv2.putText(img, legend, (6, shh - 8), cv2.FONT_HERSHEY_SIMPLEX, 0.35, (255, 255, 255), 1, cv2.LINE_AA)
        cv2.imwrite(diag_path, cv2.cvtColor(img, cv2.COLOR_RGB2BGR), [cv2.IMWRITE_JPEG_QUALITY, 92])
    return cands, small, outdoor

def build_background(frames, alphas_dir, A_list, ref, W, H, samples=40):
    """Чистый фон в координатах опорного кадра: участки, закрытые человеком в опорном кадре,
    берутся из других кадров (ближайших по времени), где фон там виден. Возвращает фон и маску
    «фон не виден ни в одном кадре»."""
    n = len(frames)
    ts = sorted(set(np.linspace(0, n - 1, min(n, samples)).astype(int).tolist()) - {ref},
                key=lambda t: abs(t - ref))
    bg = cv2.cvtColor(cv2.imread(frames[ref]), cv2.COLOR_BGR2RGB).astype(np.float32)
    a_ref = cv2.imread(f"{alphas_dir}/{ref:06d}.png", cv2.IMREAD_GRAYSCALE)
    k = np.ones((21, 21), np.uint8)
    base = cv2.dilate((a_ref > 12).astype(np.uint8), k) == 0      # фон, видимый в опорном кадре
    filled = base.copy()
    ones = np.ones((H, W), np.uint8)
    for t in ts:
        if filled.mean() > 0.995:
            break
        M = (A_list[t] @ np.linalg.inv(A_list[ref]))[:2]          # опорный кадр -> кадр t
        ft = cv2.warpAffine(cv2.cvtColor(cv2.imread(frames[t]), cv2.COLOR_BGR2RGB), M, (W, H),
                            flags=cv2.INTER_LINEAR | cv2.WARP_INVERSE_MAP, borderMode=cv2.BORDER_REPLICATE)
        at = cv2.warpAffine(cv2.imread(f"{alphas_dir}/{t:06d}.png", cv2.IMREAD_GRAYSCALE), M, (W, H),
                            flags=cv2.WARP_INVERSE_MAP, borderValue=255)
        inside = cv2.warpAffine(ones, M, (W, H), flags=cv2.INTER_NEAREST | cv2.WARP_INVERSE_MAP) > 0
        vis = (cv2.dilate((at > 12).astype(np.uint8), k) == 0) & inside
        new = vis & ~filled
        if new.sum() < 200:
            continue
        common = vis & base
        ft = ft.astype(np.float32)
        if common.sum() > 2000:                                    # подгонка экспозиции под опорный кадр
            ft *= np.clip(bg[common].mean(0) / np.maximum(ft[common].mean(0), 1), 0.7, 1.4)
        bg[new] = ft[new]
        filled |= vis
    print(f"Чистый фон: виден {100 * filled.mean():.0f}% кадра (остальное всё время закрыто человеком)")
    return np.clip(bg, 0, 255).astype(np.uint8), ~filled

def is_static(obj_full, frames, alphas_dir, A_list, idxs, W, H, ref, bg_ref):
    """Предмет не двигается сам (люди, машины, волны): сравнение с чистым фоном после
    компенсации движения камеры и автоэкспозиции."""
    f0 = bg_ref.astype(np.float32)
    ring = cv2.dilate(obj_full, np.ones((31, 31), np.uint8)) - obj_full
    checked = fails = 0
    for t in idxs:
        M = (A_list[t] @ np.linalg.inv(A_list[ref]))[:2]          # опорный кадр -> кадр t
        ft = cv2.cvtColor(cv2.imread(frames[t]), cv2.COLOR_BGR2RGB)
        back = cv2.warpAffine(ft, M, (W, H), flags=cv2.INTER_LINEAR | cv2.WARP_INVERSE_MAP,
                              borderMode=cv2.BORDER_REFLECT).astype(np.float32)
        at = cv2.imread(f"{alphas_dir}/{t:06d}.png", cv2.IMREAD_GRAYSCALE)
        at = cv2.warpAffine(at, M, (W, H), flags=cv2.WARP_INVERSE_MAP)
        at = cv2.dilate((at > 25).astype(np.uint8), np.ones((31, 31), np.uint8)) == 0   # с запасом от волос/рук
        o, r = (obj_full > 0) & at, (ring > 0) & at
        if o.sum() < 0.5 * (obj_full > 0).sum():           # предмет в этом кадре в основном закрыт
            continue
        if r.sum() > 50:
            back = back * (f0[r].mean(0) / np.maximum(back[r].mean(0), 1))
        d = np.abs(back - f0).mean(-1)
        d_obj, d_ring = d[o].mean(), d[r].mean() if r.sum() > 50 else 0
        checked += 1
        if d_obj > 20 and d_obj > 2.5 * d_ring + 5:
            fails += 1
    return not (checked and fails >= max(2, (checked + 1) // 2))   # «двигается» на половине кадров и больше

def save_debug(small, cands, removed_idx, replaced_idx, path, note):
    img = small.copy()
    for i, d in enumerate(cands):
        cnts, _ = cv2.findContours(d["m"], cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        col = (0, 255, 0) if i in replaced_idx else (255, 0, 0) if i in removed_idx else (255, 220, 0)
        cv2.drawContours(img, cnts, -1, col, 3 if i in removed_idx else 1)
    cv2.putText(img, note, (8, 24), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (255, 255, 255), 2, cv2.LINE_AA)
    cv2.imwrite(path, cv2.cvtColor(img, cv2.COLOR_RGB2BGR), [cv2.IMWRITE_JPEG_QUALITY, 90])

# Чем заменять (похожие предметы того же типа)
SIMILAR = {
    "box": ["a cardboard box", "a small wooden box", "a white storage box", "a shoe box"],
    "bottle": ["a glass bottle", "a plastic water bottle", "a wine bottle"],
    "vase": ["a ceramic vase", "a glass vase"], "flower": ["a vase with flowers"],
    "book": ["a stack of books", "a closed hardcover book"],
    "plant": ["a small potted plant", "a succulent in a pot"], "pot": ["a small potted plant"],
    "flowerpot": ["a small potted plant"], "lamp": ["a small table lamp"],
    "basket": ["a woven basket"], "bag": ["a handbag", "a paper shopping bag"],
    "painting": ["a framed picture"], "poster": ["a framed poster"], "picture": ["a framed picture"],
    "clock": ["a round clock"], "cushion": ["a decorative pillow"], "pillow": ["a decorative pillow"],
    "plaything": ["a plush toy"], "toy": ["a plush toy"], "towel": ["a folded towel"],
    "glass": ["a drinking glass"], "cup": ["a ceramic mug"], "mug": ["a ceramic mug"],
}
DEFAULT_SIMILAR = ["a cardboard box", "a small potted plant", "a stack of books", "a ceramic vase",
                   "a decorative candle", "a woven basket"]

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
    sw, shh = size_short(W, H, 512, 32)               # для анализа
    pw, ph = size_short(W, H, PROC_SHORT_SIDE, 8)     # для ProPainter
    tmp = "/content/_rmobj"
    shutil.rmtree(tmp, ignore_errors=True)
    for d in ("in", "out", "alpha"):
        os.makedirs(f"{tmp}/{d}")

    limit = f"-t {TEST_SECONDS}" if TEST_SECONDS > 0 else ""
    base_vf = f"hflip,fps={fps},scale={W}:{H}"
    tonemap = ("zscale=t=linear:npl=100,format=gbrpf32le,zscale=p=bt709,"
               "tonemap=tonemap=hable:desat=0,zscale=t=bt709:m=bt709:r=tv,format=yuv420p,")
    try:
        sh(f'ffmpeg -v error {limit} -i "{src}" -vf "{tonemap if hdr else ""}{base_vf}" "{tmp}/in/%06d.bmp"')
    except subprocess.CalledProcessError:
        for f in glob.glob(f"{tmp}/in/*.bmp"): os.remove(f)
        sh(f'ffmpeg -v error {limit} -i "{src}" -vf "{base_vf}" "{tmp}/in/%06d.bmp"')
    frames = sorted(glob.glob(f"{tmp}/in/*.bmp"))
    n = len(frames)
    S = np.diag([W / sw, H / shh, 1.0])

    # Проход 1: маска человека + движение камеры
    matter.reset(H)
    A_list, tracker, A = [], None, np.eye(3)
    for i, fp in enumerate(tqdm(frames, desc="Анализ")):
        full = cv2.cvtColor(cv2.imread(fp), cv2.COLOR_BGR2RGB)
        alpha = matter(full)
        cv2.imwrite(f"{tmp}/alpha/{i:06d}.png", (alpha * 255 + 0.5).astype(np.uint8))
        gray = cv2.cvtColor(cv2.resize(full, (sw, shh), interpolation=cv2.INTER_AREA), cv2.COLOR_RGB2GRAY)
        a_s = cv2.resize(alpha, (sw, shh))
        if tracker is None:
            tracker = BgTracker(gray, a_s, S)
        else:
            A = tracker.update(gray, a_s)
            A = A_list[-1] if A is None else A
        A_list.append(A.copy())

    # Опорный кадр: где человек закрывает меньше всего фона (просматриваем всё видео)
    areas = [(cv2.imread(f"{tmp}/alpha/{i:06d}.png", cv2.IMREAD_GRAYSCALE) > 128).mean()
             for i in range(0, n, max(1, n // 40))]
    ref = int(np.argmin(areas)) * max(1, n // 40)
    print(f"Опорный кадр: {ref} (человек занимает {100 * min(areas):.0f}% кадра)")
    # Чистый фон из всех кадров: предметы, закрытые человеком в опорном кадре, тоже находятся
    frame_r, never = build_background(frames, f"{tmp}/alpha", A_list, ref, W, H)
    alpha_r = never.astype(np.float32)                 # «человек» = там, где фон не виден никогда
    name0 = os.path.splitext(os.path.basename(dst))[0]
    cands, small0, outdoor = find_objects(frame_r, alpha_r, sw, shh, f"{OUTPUT_DIR}/{name0}_candidates.jpg")
    # Сначала опознанные предметы (коробки, бутылки, вазы, книги...) в случайном порядке,
    # потом неопознанные, но явно отделённые от фона (самые контрастные первыми)
    good = [i for i, d in enumerate(cands) if d["obj"] >= 0.3]
    other = sorted((i for i, d in enumerate(cands) if d["obj"] < 0.3), key=lambda i: -cands[i]["c"])
    random.shuffle(good)
    order = good + other
    idxs = sorted(set(np.linspace(0, n - 1, 9).astype(int).tolist()) - {ref})
    removed, moving = [], 0
    for i in order:
        if len(removed) >= REMOVE_COUNT:
            break
        obj_full = cv2.resize(cands[i]["m"], (W, H), interpolation=cv2.INTER_NEAREST)
        if is_static(obj_full, frames, f"{tmp}/alpha", A_list, idxs, W, H, ref, frame_r):
            removed.append(i)
        else:
            moving += 1
    if len(removed) < REMOVE_COUNT:
        print(f"Подходящих неподвижных предметов: {len(removed)} (лимит {REMOVE_COUNT}) — "
              f"больше ничего не трогаю, чтобы не портить мебель и стены")

    shift = max(np.abs((A @ np.linalg.inv(A_list[ref]))[:2, 2]).max() for A in A_list) / min(W, H)
    method = FILL_METHOD if FILL_METHOD != "auto" else ("lama" if shift < 0.03 else "propainter")
    if lama is None:
        method = "propainter"
    if removed:
        print(f"Сдвиг камеры: {100 * shift:.1f}% кадра -> дорисовка: {method}")

    # Какие из убранных заменить новым похожим (только мелкие, неподвижная камера, помещение)
    replace = {}
    if REPLACE_CHANCE > 0 and removed:
        if method != "lama":
            print("Замена предметов пропущена: камера двигается (только удаление)")
        elif outdoor:
            print("Замена предметов пропущена: сцена на улице (только удаление)")
        else:
            for i in removed:
                if cands[i]["area"] <= REPLACE_MAX_AREA and random.random() < REPLACE_CHANCE:
                    replace[i] = random.choice(SIMILAR.get(cands[i]["cls"], DEFAULT_SIMILAR))

    # LaMa: чистый опорный кадр (предметы + человек рядом с ними дорисованы), один раз
    plate = obj_ref = None
    if removed and method == "lama":
        obj_ref = np.zeros((H, W), np.uint8)
        for i in removed:
            obj_ref |= cv2.resize(cands[i]["m"], (W, H), interpolation=cv2.INTER_NEAREST)
        obj_ref = cv2.dilate(obj_ref, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (21, 21)))
        near = cv2.dilate(obj_ref, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (81, 81)))
        hole = obj_ref | ((alpha_r > 0.1) & (near > 0)).astype(np.uint8)   # и волосы поверх предмета
        plate = frame_r.copy()
        hole = cv2.morphologyEx(hole, cv2.MORPH_CLOSE,                    # обрывки -> цельные области
                                cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (31, 31)))
        n_c, comp = cv2.connectedComponents(hole)
        for j in range(1, n_c):
            mj = (comp == j).astype(np.uint8)
            if mj.sum() < 50:
                continue
            ys, xs = np.nonzero(mj)
            x0, x1, y0, y1 = xs.min(), xs.max(), ys.min(), ys.max()
            bw, bh = max(x1 - x0 + 1, 128), max(y1 - y0 + 1, 128)          # кусок для LaMa не меньше 256
            cx, cy = (x0 + x1) // 2, (y0 + y1) // 2
            X0, X1 = max(0, cx - bw), min(W, cx + bw + 1)
            Y0, Y1 = max(0, cy - bh), min(H, cy + bh + 1)
            crop, mc = plate[Y0:Y1, X0:X1], mj[Y0:Y1, X0:X1]
            k = min(1.0, 1024 / max(crop.shape[:2]))
            cs = cv2.resize(crop, None, fx=k, fy=k, interpolation=cv2.INTER_AREA) if k < 1 else crop
            ms = cv2.resize(mc, (cs.shape[1], cs.shape[0]), interpolation=cv2.INTER_NEAREST) if k < 1 else mc
            res = lama_inpaint(cs, ms)
            if k < 1:
                res = cv2.resize(res, (crop.shape[1], crop.shape[0]), interpolation=cv2.INTER_CUBIC)
            plate[Y0:Y1, X0:X1] = np.where(mc[..., None] > 0, res, crop)
        # Новый похожий предмет на месте части убранных (рисуется на уже очищенном кадре)
        for i, prompt in list(replace.items()):
            mi = cv2.resize(cands[i]["m"], (W, H), interpolation=cv2.INTER_NEAREST)
            mi = cv2.dilate(mi, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (7, 7)))
            ys, xs = np.nonzero(mi)
            side = int(min(max(256, 2.4 * max(xs.max() - xs.min(), ys.max() - ys.min())), min(W, H)))
            cx, cy = (xs.min() + xs.max()) // 2, (ys.min() + ys.max()) // 2
            X0 = int(np.clip(cx - side // 2, 0, W - side)); Y0 = int(np.clip(cy - side // 2, 0, H - side))
            crop, mc = plate[Y0:Y0 + side, X0:X0 + side], mi[Y0:Y0 + side, X0:X0 + side]
            res = sd_inpaint(cv2.resize(crop, (512, 512), interpolation=cv2.INTER_AREA),
                             cv2.resize(mc * 255, (512, 512), interpolation=cv2.INTER_NEAREST), prompt)
            if res is None:                                  # SD недоступна — остаётся просто удаление
                replace.clear(); break
            res = cv2.resize(res, (side, side), interpolation=cv2.INTER_CUBIC).astype(np.float32)
            fm = cv2.GaussianBlur(mc.astype(np.float32), (0, 0), 2)[..., None]
            plate[Y0:Y0 + side, X0:X0 + side] = np.clip(fm * res + (1 - fm) * crop, 0, 255).astype(np.uint8)
            print(f"Заменено: {cands[i]['cls']} -> {prompt}")
        cv2.imwrite(f"{OUTPUT_DIR}/{name0}_cleaned_frame.jpg", cv2.cvtColor(plate, cv2.COLOR_RGB2BGR),
                    [cv2.IMWRITE_JPEG_QUALITY, 92])
        obj_ref = cv2.GaussianBlur(obj_ref.astype(np.float32), (0, 0), 3)

    note = f"found {len(cands)}, removed {len(removed) - len(replace)}, replaced {len(replace)}" + \
           (f", moving {moving}" if moving else "")
    print("Предметы:", note)
    save_debug(small0, cands, set(removed), set(replace), f"{OUTPUT_DIR}/{name0}_debug.jpg", note)

    # Маска удаления в кадре 0 (полное разрешение), с запасом под тень/кант
    obj0 = np.zeros((H, W), np.uint8)
    for i in removed:
        obj0 |= cv2.resize(cands[i]["m"], (W, H), interpolation=cv2.INTER_NEAREST)
    if removed:
        obj0 = cv2.dilate(obj0, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (25, 25)))
        # маска найдена на опорном кадре — переводим её в координаты кадра 0
        obj0 = cv2.warpAffine(obj0, np.linalg.inv(A_list[ref])[:2], (W, H), flags=cv2.INTER_NEAREST)

    # ProPainter: кадры + маски в рабочем разрешении, по кускам
    pp_frames = None
    if removed and method == "propainter":
        pp_dir = f"{tmp}/pp"
        pp_frames = f"{tmp}/pp_out"
        os.makedirs(pp_frames)
        for c0 in range(0, n, CHUNK_FRAMES):
            c1 = min(n, c0 + CHUNK_FRAMES)
            if 0 < n - c1 < 30:                        # не оставляем крошечный хвост
                c1 = n
            shutil.rmtree(pp_dir, ignore_errors=True)
            os.makedirs(f"{pp_dir}/clip"); os.makedirs(f"{pp_dir}/mask")
            for i in range(c0, c1):
                f = cv2.imread(frames[i])
                cv2.imwrite(f"{pp_dir}/clip/{i:06d}.png", cv2.resize(f, (pw, ph), interpolation=cv2.INTER_AREA))
                m = cv2.warpAffine(obj0, A_list[i][:2], (W, H), flags=cv2.INTER_NEAREST)
                a = cv2.imread(f"{tmp}/alpha/{i:06d}.png", cv2.IMREAD_GRAYSCALE) > 128
                m = (m > 0) & ~a                       # человек спереди — не трогаем
                cv2.imwrite(f"{pp_dir}/mask/{i:06d}.png",
                            cv2.resize(m.astype(np.uint8) * 255, (pw, ph), interpolation=cv2.INTER_NEAREST))
            print(f"ProPainter: кадры {c0}–{c1 - 1}...")
            sh(f'cd {PP_DIR} && {PP_ENV} {sys.executable} inference_propainter.py -i {pp_dir}/clip '
               f'-m {pp_dir}/mask -o {pp_dir}/res --width {pw} --height {ph} --save_frames '
               f'--subvideo_length 50 --neighbor_length 10 --ref_stride 10 '
               f'{"--fp16" if DEV == "cuda" else ""} > {tmp}/propainter.log 2>&1 '
               f'|| (tail -30 {tmp}/propainter.log; exit 1)')
            outs = sorted(glob.glob(f"{pp_dir}/res/clip/frames/*.png"))
            assert len(outs) == c1 - c0, f"ProPainter вернул {len(outs)} кадров вместо {c1 - c0}"
            for k, fp in enumerate(outs):
                shutil.move(fp, f"{pp_frames}/{c0 + k:06d}.png")
            if c1 == n:
                break

    # Цвет одежды на это видео
    use_clothes = RECOLOR_CLOTHES and person_model is not None
    c_hex = random.choice(CLOTHES_COLORS)
    c_rgb = hex_rgb(c_hex)

    # Проход 2: сборка — правки только там, где нужно, остальное — оригинал в полном разрешении
    noise_sigma = None
    prev_w = None
    for i, fp in enumerate(tqdm(frames, desc=os.path.basename(src))):
        orig = cv2.cvtColor(cv2.imread(fp), cv2.COLOR_BGR2RGB)
        out = orig
        if plate is not None:
            alpha = cv2.imread(f"{tmp}/alpha/{i:06d}.png", cv2.IMREAD_GRAYSCALE).astype(np.float32) / 255
            M = (A_list[i] @ np.linalg.inv(A_list[ref]))[:2]      # опорный кадр -> текущий
            m = cv2.warpAffine(obj_ref, M, (W, H), flags=cv2.INTER_LINEAR) * (1 - alpha)
            if m.max() > 0.01:
                pl = cv2.warpAffine(plate, M, (W, H), flags=cv2.INTER_LINEAR, borderMode=cv2.BORDER_REFLECT)
                pr = cv2.warpAffine(frame_r, M, (W, H), flags=cv2.INTER_LINEAR, borderMode=cv2.BORDER_REFLECT)
                # свет/экспозиция/тени: «текущий кадр / опорный кадр» на том же месте фона
                q = 4
                ws = cv2.resize(1 - alpha, (W // q, H // q))
                def nb(img):
                    x = cv2.resize(img, (W // q, H // q)).astype(np.float32)
                    return cv2.GaussianBlur(x * ws[..., None], (0, 0), 6) / np.maximum(
                        cv2.GaussianBlur(ws, (0, 0), 6), 1e-3)[..., None]
                ratio = cv2.resize(np.clip((nb(orig) + 2) / (nb(pr) + 2), 0.5, 2.0), (W, H))
                fill = pl.astype(np.float32) * ratio
                out = np.clip(m[..., None] * fill + (1 - m[..., None]) * orig, 0, 255).astype(np.uint8)
        if pp_frames is not None:
            alpha = cv2.imread(f"{tmp}/alpha/{i:06d}.png", cv2.IMREAD_GRAYSCALE).astype(np.float32) / 255
            m = cv2.warpAffine(obj0, A_list[i][:2], (W, H), flags=cv2.INTER_LINEAR).astype(np.float32)
            m = cv2.GaussianBlur(m, (0, 0), 3) * (1 - alpha)
            if m.max() > 0.01:
                fill = cv2.cvtColor(cv2.imread(f"{pp_frames}/{i:06d}.png"), cv2.COLOR_BGR2RGB)
                fill = cv2.resize(fill, (W, H), interpolation=cv2.INTER_CUBIC).astype(np.float32)
                if noise_sigma is None:                # «зерно» камеры, чтобы заплатка не была гладкой
                    g = cv2.cvtColor(orig, cv2.COLOR_RGB2GRAY).astype(np.float32)
                    noise_sigma = float(np.median(np.abs(g - cv2.GaussianBlur(g, (0, 0), 1.5)))) * 1.2
                fill += np.random.normal(0, noise_sigma, fill.shape[:2])[..., None]
                out = np.clip(m[..., None] * fill + (1 - m[..., None]) * orig, 0, 255).astype(np.uint8)
        if use_clothes:
            small = cv2.resize(orig, (sw, shh), interpolation=cv2.INTER_AREA)
            a_small = cv2.resize(cv2.imread(f"{tmp}/alpha/{i:06d}.png", cv2.IMREAD_GRAYSCALE), (sw, shh)).astype(np.float32) / 255
            w = clothes_weight(small, a_small)
            if prev_w is not None:
                w = 0.4 * w + 0.6 * prev_w                # меньше дрожания краёв
            prev_w = w
            if i == 0:                                   # светлота «ткани» под выбранный цвет
                wm = w > 0.5
                y_old = ((small[wm].astype(np.float32) / 255) ** 2.2 @ [0.2126, 0.7152, 0.0722]).mean() if wm.sum() > 50 else 0.2
                y_new = (c_rgb ** 2.2) @ np.array([0.2126, 0.7152, 0.0722], np.float32)
                gain = float(np.clip(y_new / max(y_old, 1e-3), 0.35, 1.6))
            out = clothes_recolor(out, cv2.resize(w, (W, H), interpolation=cv2.INTER_LINEAR), c_rgb, gain)
        cv2.imwrite(f"{tmp}/out/{i + 1:06d}.bmp", cv2.cvtColor(out, cv2.COLOR_RGB2BGR))

    audio_in  = f'{limit} -i "{src}"' if has_audio(src) else ""
    audio_map = "-map 1:a:0 -c:a aac -b:a 192k -shortest" if audio_in else ""
    sh(f'ffmpeg -v error -y -framerate {fps} -i "{tmp}/out/%06d.bmp" {audio_in} '
       f'-map 0:v:0 {audio_map} -vf "scale=out_color_matrix=bt709:out_range=tv,format=yuv420p" '
       f'-colorspace bt709 -color_primaries bt709 -color_trc bt709 '
       f'-c:v libx264 -preset medium -crf {CRF} -pix_fmt yuv420p -movflags +faststart "{dst}"')
    shutil.rmtree(tmp, ignore_errors=True)

    name = os.path.basename(dst)
    for i in removed:
        d = cands[i]
        if i in replace:
            log_rows.append([name, "заменено", f"{d['cls']} -> {replace[i]}", f"{100 * d['area']:.2f}%"])
        else:
            log_rows.append([name, "убрано", f"{d['cls']} ({d['src']}, {method})", f"{100 * d['area']:.2f}%"])
    if not removed:
        log_rows.append([name, "предметы", "подходящих не найдено — фон не трогали", ""])
    if use_clothes:
        log_rows.append([name, "одежда", c_hex, ""])
    print(f"Время: {(time.time() - t0) / 60:.1f} мин")

# 6) Обработка всех видео (уже готовые пропускаются — можно перезапускать)
EXTS = (".mp4", ".mov", ".m4v", ".mkv", ".avi", ".webm")
videos = sorted(f for f in glob.glob(f"{INPUT_DIR}/*") if f.lower().endswith(EXTS))
print(f"Найдено видео: {len(videos)}")
suffix = "_test" if TEST_SECONDS > 0 else ""
log_path = f"{OUTPUT_DIR}/remove_log.csv"
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
            if new_log: wr.writerow(["файл", "что", "описание", "площадь"])
            wr.writerows(rows)
        print("Готово:", dst)
    except Exception as e:
        print("Ошибка на", src, "->", e)
    gc.collect(); torch.cuda.empty_cache()
print("Всё.")
