# ==============================================================================
#  Правка родного фона + перекраска одежды для Google Colab (GPU T4) — ОДНА ЯЧЕЙКА.
#  Drive: Мой диск/Colab Notebooks/content/input_videos  ->  .../output_iphone
#  Для каждого видео:
#    1) HFLIP
#    2) Нейросеть находит предметы на фоне и УБИРАЕТ несколько случайных (LaMa дорисовывает
#       стену/полку/асфальт за ними). Двигающиеся предметы (люди, машины, волны) не трогаются.
#    3) Стены (и фасады зданий на улице) перекрашиваются в случайный «настоящий» цвет краски
#    4) Одежда перекрашивается в случайный цвет (лицо, волосы, кожа, тату не трогаются)
#    5) Всё остальное — пиксели оригинала. Человек не вырезается и не вклеивается.
#       Свет/тени/экспозиция в правленых местах берутся из оригинала кадр за кадром.
#  Результат называется как исходник: IMG_0038.MOV -> output_iphone/IMG_0038.mp4
#  Журнал «что убрано и какие цвета»: output_iphone/edit_log.csv
# ==============================================================================

# ----------------------------- НАСТРОЙКИ --------------------------------------
BASE_DIR   = "/content/drive/MyDrive/Colab Notebooks/content"
INPUT_DIR  = f"{BASE_DIR}/input_videos"
OUTPUT_DIR = f"{BASE_DIR}/output_iphone"

REMOVE_COUNT   = 3      # сколько предметов убирать (если подходящих меньше — уберёт сколько есть)
OBJ_MIN_AREA   = 0.002  # размер предмета: от 0.2% кадра...
OBJ_MAX_AREA   = 0.06   # ...до 6% кадра (крупное мебель/машины дорисовываются хуже)
RECOLOR_WALLS  = True   # перекрашивать стены / фасады
RECOLOR_CLOTHES = True  # перекрашивать одежду
CLOTHES_CHROMA = (25, 50)  # насыщенность новых цветов одежды (от, до)
STABILITY      = 0.70   # сглаживание масок по времени (0 = выкл, 0.9 = макс)
SEG_SHORT_SIDE = 512    # разрешение для нейросетей по короткой стороне
MAX_FPS        = 30     # если исходник 60 fps — обработаем 30
CRF            = 17     # качество итогового x264 (меньше = лучше)
TEST_SECONDS   = 0      # >0 = обработать только первые N секунд (файл будет с припиской _test)

# Что можно убирать (названия классов модели интерьера/улицы ADE20K)
REMOVABLE = ("painting", "poster", "picture", "box", "bottle", "vase", "lamp", "sconce", "light",
             "chandelier", "clock", "basket", "bag", "book", "plaything", "pillow", "cushion", "towel",
             "pot", "plant", "flower", "signboard", "trade name", "ashcan", "bucket", "barrel", "flag",
             "fan", "tray", "plate", "glass", "ball", "bicycle", "minibike", "bench", "stool", "chair",
             "bulletin board", "monitor", "television", "computer", "microwave", "sculpture",
             "streetlight", "traffic light", "car", "van", "truck", "boat", "palm", "umbrella", "food")
WALLS = ("wall", "building", "house", "skyscraper")
# Цвета стен — реальные цвета краски (случайный на видео)
WALL_COLORS = ["#9caf88", "#d8a7a7", "#d9c7a7", "#9fb4c7", "#c77b58", "#a8d5ba", "#b8a9c9",
               "#8a8f5c", "#d6c29a", "#8cbfbf", "#e8d7a0", "#e9b49a", "#a3b8a0", "#c9b6a6"]
# ------------------------------------------------------------------------------

import os, sys, glob, json, shutil, subprocess, time, random, csv, gc, urllib.request

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
rvm = torch.hub.load("PeterL1n/RobustVideoMatting", "mobilenetv3", trust_repo=True).to(DEV).eval()
_seg = lambda n: AutoModelForSemanticSegmentation.from_pretrained(n).to(DEV).half().eval()
person_model = load_first(_seg, ["mattmdjaga/segformer_b2_clothes", "sayeed99/segformer_b3_clothes"], "Одежда")
scene_model = load_first(_seg, ["nvidia/segformer-b2-finetuned-ade-512-512",
                                "nvidia/segformer-b0-finetuned-ade-512-512"], "Предметы/стены")
PERSON_LABELS = {int(k): v.lower() for k, v in person_model.config.id2label.items()}
SCENE_LABELS  = {int(k): v.lower().strip() for k, v in scene_model.config.id2label.items()}

# LaMa — нейросеть для удаления предметов (дорисовывает фон продолжением текстуры)
LAMA_URL = "https://github.com/enesmsahin/simple-lama-inpainting/releases/download/v0.1.0/big-lama.pt"
try:
    if not os.path.exists("/content/big-lama.pt"):
        urllib.request.urlretrieve(LAMA_URL, "/content/big-lama.pt")
    lama = torch.jit.load("/content/big-lama.pt", map_location=DEV).eval()
    print("Удаление предметов: LaMa")
except Exception as e:
    lama = None
    print(f"LaMa недоступна ({type(e).__name__}) — будет простое заполнение, хуже качеством")

MEAN = torch.tensor([0.485, 0.456, 0.406], device=DEV).view(1, 3, 1, 1)
STD  = torch.tensor([0.229, 0.224, 0.225], device=DEV).view(1, 3, 1, 1)

@torch.no_grad()
def _probs(model, rgb):
    h, w = rgb.shape[:2]
    x = torch.from_numpy(rgb).to(DEV).permute(2, 0, 1)[None].float() / 255.0
    x = ((x - MEAN) / STD).half()
    logits = model(pixel_values=x).logits.float()
    logits = F.interpolate(logits, size=(h, w), mode="bilinear", align_corners=False)
    return logits.softmax(1)[0].permute(1, 2, 0).cpu().numpy()

def person_probs(rgb): return _probs(person_model, rgb)
def scene_probs(rgb):  return _probs(scene_model, rgb)

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

@torch.no_grad()
def inpaint(rgb, mask):
    """Убрать предмет: rgb uint8, mask 0/1 -> rgb uint8."""
    if lama is None:
        return cv2.inpaint(rgb, mask.astype(np.uint8), 7, cv2.INPAINT_TELEA)
    h, w = mask.shape
    ph, pw = (8 - h % 8) % 8, (8 - w % 8) % 8
    img = np.pad(rgb, ((0, ph), (0, pw), (0, 0)), mode="reflect")
    m = np.pad(mask.astype(np.float32), ((0, ph), (0, pw)), mode="reflect")
    it = torch.from_numpy(img).to(DEV).permute(2, 0, 1)[None].float() / 255.0
    mt = (torch.from_numpy(m).to(DEV)[None, None] > 0).float()
    out = lama(it, mt)[0].permute(1, 2, 0).clamp(0, 1).cpu().numpy()
    return (out[:h, :w] * 255 + 0.5).astype(np.uint8)
# <<< MODELS

matter = Matter()

KEEP_WORDS = ("hair", "face", "arm", "leg", "glass", "skin")
P_BG   = [i for i, n in PERSON_LABELS.items() if n.startswith("background")]
P_KEEP = [i for i, n in PERSON_LABELS.items() if any(w in n for w in KEEP_WORDS)]
P_RECOLOR = [i for i in PERSON_LABELS if i not in P_BG + P_KEEP]
S_WALL = [i for i, n in SCENE_LABELS.items() if any(n.split(";")[0].strip() == w for w in WALLS)]
S_REMOVE = [i for i, n in SCENE_LABELS.items() if any(w in n for w in REMOVABLE)]
print("Стены:", [SCENE_LABELS[i] for i in S_WALL])

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

def seg_size(w, h):
    k = SEG_SHORT_SIDE / min(w, h)
    return max(32, round(w * k / 32) * 32), max(32, round(h * k / 32) * 32)

def hex_dir(hx):
    """Цвет -> (направление оттенка в Lab, насыщенность)."""
    rgb = np.array([[[int(hx[i:i + 2], 16) for i in (1, 3, 5)]]], np.float32) / 255
    _, a, b = cv2.cvtColor(rgb, cv2.COLOR_RGB2Lab)[0, 0]
    c = float(np.hypot(a, b))
    return np.array([a / c, b / c, c], np.float32)

def lab_hex(v):
    lab = np.array([[[60.0, v[0] * v[2], v[1] * v[2]]]], np.float32)
    r, g, b = (np.clip(cv2.cvtColor(lab, cv2.COLOR_Lab2RGB)[0, 0], 0, 1) * 255).astype(int)
    return f"#{r:02x}{g:02x}{b:02x}"

def random_palette(n):
    hues = np.array([random.uniform(0, 2 * np.pi) for _ in range(n)], np.float32)
    cmin = np.array([random.uniform(*CLOTHES_CHROMA) for _ in range(n)], np.float32)
    return np.stack([np.cos(hues), np.sin(hues), cmin], 1)

def skin_like(rgb):
    ycc = cv2.cvtColor(rgb, cv2.COLOR_RGB2YCrCb).astype(np.float32)
    cr, cb = ycc[..., 1], ycc[..., 2]
    return ((cr > 135) & (cr < 175) & (cb > 85) & (cb < 135)).astype(np.float32)

def fill_holes(mask, max_frac=0.03):
    """Закрашивает «дырки» внутри кожи (татуировки, родинки), чтобы их не перекрасить."""
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

def recolor(rgb, ctrl):
    """Меняет оттенок, сохраняя яркость (складки, тени, фактура остаются)."""
    lab = cv2.cvtColor(rgb.astype(np.float32) / 255.0, cv2.COLOR_RGB2Lab)
    L, a, b = lab[..., 0], lab[..., 1], lab[..., 2]
    da, db, cmin, w = (ctrl[..., i] for i in range(4))
    norm = np.maximum(np.hypot(da, db), 1e-6)
    da, db = da / norm, db / norm
    lum_k = np.clip(np.minimum(L, 100 - L) / 35.0, 0.25, 1.0)
    new_c = np.maximum(np.hypot(a, b), cmin * lum_k)
    lab[..., 1] = w * da * new_c + (1 - w) * a
    lab[..., 2] = w * db * new_c + (1 - w) * b
    return (np.clip(cv2.cvtColor(lab, cv2.COLOR_Lab2RGB), 0, 1) * 255 + 0.5).astype(np.uint8)

def flow_maps(cur_gray, prev_gray):
    flow = cv2.calcOpticalFlowFarneback(cur_gray, prev_gray, None, 0.5, 4, 21, 3, 5, 1.2, 0)
    h, w = cur_gray.shape
    gx, gy = np.meshgrid(np.arange(w, dtype=np.float32), np.arange(h, dtype=np.float32))
    return gx + flow[..., 0], gy + flow[..., 1]

def warp_flow(img, maps):
    return cv2.remap(img, maps[0], maps[1], cv2.INTER_LINEAR, borderMode=cv2.BORDER_REPLICATE)

class BgTracker:
    """Движение фона (камеры) относительно кадра 0: весь видимый фон сравнивается с опорным
    кадром (ECC), человек исключён. Без накопления ошибок; опорный кадр обновляется при уходе."""
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

def norm_blur(img, weight, sigma):
    """Размытие только по «весу» (по фону без человека) — чтобы человек не протекал в свет фона."""
    num = cv2.GaussianBlur(img * weight[..., None], (0, 0), sigma)
    den = cv2.GaussianBlur(weight, (0, 0), sigma)[..., None]
    return num / np.maximum(den, 1e-3)

# 3) Выбор и удаление предметов (один раз на видео, по кадру 0)
def pick_objects(frame0, alpha0, sw, shh):
    small = cv2.resize(frame0, (sw, shh), interpolation=cv2.INTER_AREA)
    sp = scene_probs(small)
    lab = sp.argmax(-1)
    a_s = cv2.resize(alpha0, (sw, shh))
    person = cv2.dilate((a_s > 0.2).astype(np.uint8), np.ones((7, 7), np.uint8))
    cands = []
    for c in set(np.unique(lab).tolist()) & set(S_REMOVE):
        m = ((lab == c) & (person == 0)).astype(np.uint8)
        n, comp, stats, _ = cv2.connectedComponentsWithStats(m, connectivity=8)
        for k in range(1, n):
            area = stats[k, 4] / (sw * shh)
            if not (OBJ_MIN_AREA <= area <= OBJ_MAX_AREA):
                continue
            obj = (comp == k).astype(np.uint8)
            # Предмет не должен заметно прятаться за человеком (иначе дорисовка ненадёжна)
            ring = cv2.dilate(obj, np.ones((9, 9), np.uint8)) - obj
            if (ring * (a_s > 0.2)).sum() > 0.3 * max(ring.sum(), 1):
                continue
            cands.append((SCENE_LABELS[c].split(";")[0], obj, area))
    return cands

def is_static(obj_full, frames, alphas_dir, A_list, idxs, W, H):
    """Проверка, что предмет не двигается сам (люди, машины, волны): сравниваем с кадром 0
    после компенсации движения камеры."""
    f0 = cv2.cvtColor(cv2.imread(frames[0]), cv2.COLOR_BGR2RGB).astype(np.float32)
    ys, xs = np.nonzero(obj_full)
    if len(xs) == 0:
        return False
    ring = cv2.dilate(obj_full, np.ones((31, 31), np.uint8)) - obj_full
    for t in idxs:
        ft = cv2.cvtColor(cv2.imread(frames[t]), cv2.COLOR_BGR2RGB)
        back = cv2.warpAffine(ft, A_list[t][:2], (W, H), flags=cv2.INTER_LINEAR | cv2.WARP_INVERSE_MAP,
                              borderMode=cv2.BORDER_REFLECT).astype(np.float32)
        at = cv2.imread(f"{alphas_dir}/{t:06d}.png", cv2.IMREAD_GRAYSCALE)
        at = cv2.warpAffine(at, A_list[t][:2], (W, H), flags=cv2.WARP_INVERSE_MAP) < 50
        d = np.abs(back - f0).mean(-1)
        o, r = (obj_full > 0) & at, (ring > 0) & at
        if o.sum() < 50:
            continue
        d_obj, d_ring = d[o].mean(), d[r].mean() if r.sum() > 50 else 0
        if d_obj > 18 and d_obj > 2.5 * d_ring + 4:
            return False
    return True

def remove_objects(frame0, objs):
    """Удаление выбранных предметов на кадре 0 (по кусочкам вокруг каждого предмета)."""
    plate = frame0.copy()
    total = np.zeros(frame0.shape[:2], np.float32)
    H, W = frame0.shape[:2]
    for _, m in objs:
        m = cv2.dilate(m, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (21, 21)))  # + тень/кант
        ys, xs = np.nonzero(m)
        x0, x1, y0, y1 = xs.min(), xs.max(), ys.min(), ys.max()
        pw, ph = x1 - x0 + 1, y1 - y0 + 1
        X0, X1 = max(0, x0 - pw), min(W, x1 + pw + 1)
        Y0, Y1 = max(0, y0 - ph), min(H, y1 + ph + 1)
        crop, mc = plate[Y0:Y1, X0:X1], m[Y0:Y1, X0:X1]
        k = min(1.0, 1024 / max(crop.shape[:2]))
        cs = cv2.resize(crop, None, fx=k, fy=k, interpolation=cv2.INTER_AREA) if k < 1 else crop
        ms = cv2.resize(mc, (cs.shape[1], cs.shape[0]), interpolation=cv2.INTER_NEAREST) if k < 1 else mc
        res = inpaint(cs, ms)
        if k < 1:
            res = cv2.resize(res, (crop.shape[1], crop.shape[0]), interpolation=cv2.INTER_CUBIC)
        feather = cv2.GaussianBlur(mc.astype(np.float32), (0, 0), 3)
        feather = np.maximum(feather, mc.astype(np.float32))[..., None]
        plate[Y0:Y1, X0:X1] = (feather * res + (1 - feather) * crop).astype(np.uint8)
        total[Y0:Y1, X0:X1] = np.maximum(total[Y0:Y1, X0:X1], feather[..., 0])
    return plate, total

# 4) Видео целиком
def process_video(src, dst, log_rows):
    t0 = time.time()
    W, H, fps, hdr = probe(src)
    fps = min(fps, MAX_FPS)
    sw, shh = seg_size(W, H)
    tmp = "/content/_editbg"
    shutil.rmtree(tmp, ignore_errors=True)
    for d in ("in", "out", "alpha"):
        os.makedirs(f"{tmp}/{d}")

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
    n = len(frames)
    S = np.diag([W / sw, H / shh, 1.0])

    # Проход 1: маска человека + движение камеры
    matter.reset(H)
    A_list, tracker, lost, A = [], None, 0, np.eye(3)
    for i, fp in enumerate(tqdm(frames, desc="Анализ")):
        full = cv2.cvtColor(cv2.imread(fp), cv2.COLOR_BGR2RGB)
        alpha = matter(full)
        cv2.imwrite(f"{tmp}/alpha/{i:06d}.png", (alpha * 255 + 0.5).astype(np.uint8))
        gray = cv2.cvtColor(cv2.resize(full, (sw, shh), interpolation=cv2.INTER_AREA), cv2.COLOR_RGB2GRAY)
        a_s = cv2.resize(alpha, (sw, shh))
        if tracker is None:
            tracker = BgTracker(gray, a_s, S)
        else:
            A_t = tracker.update(gray, a_s)
            if A_t is None:
                lost += 1
            else:
                A = A_t
        A_list.append(A.copy())
    shift = max(np.abs(A[:2, 2]).max() for A in A_list) / min(W, H)
    shaky = lost > 0.2 * n or shift > 0.25

    # Выбор и удаление предметов
    frame0 = cv2.cvtColor(cv2.imread(frames[0]), cv2.COLOR_BGR2RGB)
    alpha0 = cv2.imread(f"{tmp}/alpha/{0:06d}.png", cv2.IMREAD_GRAYSCALE) / 255.0
    removed, edit_mask, plate = [], None, None
    if REMOVE_COUNT > 0 and not shaky:
        cands = pick_objects(frame0, alpha0, sw, shh)
        random.shuffle(cands)
        idxs = sorted({n // 3, 2 * n // 3, n - 1} - {0})
        for name, obj, area in cands:
            if len(removed) >= REMOVE_COUNT:
                break
            obj_full = cv2.resize(obj, (W, H), interpolation=cv2.INTER_NEAREST)
            if is_static(obj_full, frames, f"{tmp}/alpha", A_list, idxs, W, H):
                removed.append((name, obj_full, area))
        if removed:
            plate, edit_mask = remove_objects(frame0, [(nm, m) for nm, m, _ in removed])
            cv2.imwrite(f"{OUTPUT_DIR}/{os.path.splitext(os.path.basename(dst))[0]}_cleaned_frame.jpg",
                        cv2.cvtColor(plate, cv2.COLOR_RGB2BGR), [cv2.IMWRITE_JPEG_QUALITY, 92])
    elif shaky:
        print("Камера сильно двигается — предметы не убираю (только перекраска)")
    print("Убрано:", [nm for nm, _, _ in removed] or "ничего")

    # Цвета на это видео
    wall_hex = random.choice(WALL_COLORS)
    wall_c = hex_dir(wall_hex)
    p_pal = random_palette(len(PERSON_LABELS))[P_RECOLOR]
    p_area = np.zeros(len(P_RECOLOR)); wall_area = 0.0

    # Проход 2: сборка кадров
    prev_gray = prev_ctrl = None
    for i, fp in enumerate(tqdm(frames, desc=os.path.basename(src))):
        orig = cv2.cvtColor(cv2.imread(fp), cv2.COLOR_BGR2RGB)
        alpha = cv2.imread(f"{tmp}/alpha/{i:06d}.png", cv2.IMREAD_GRAYSCALE).astype(np.float32) / 255
        out = orig
        if plate is not None:
            M = A_list[i][:2]
            pl = cv2.warpAffine(plate, M, (W, H), flags=cv2.INTER_LINEAR, borderMode=cv2.BORDER_REFLECT)
            p0 = cv2.warpAffine(frame0, M, (W, H), flags=cv2.INTER_LINEAR, borderMode=cv2.BORDER_REFLECT)
            em = cv2.warpAffine(edit_mask, M, (W, H), flags=cv2.INTER_LINEAR)
            m = em * (1 - alpha)                                   # человек спереди — остаётся оригинал
            if m.max() > 0.01:
                # Свет/экспозиция/тени: отношение «текущий кадр / кадр 0» на том же месте фона
                q = 4
                ws = cv2.resize(1 - alpha, (W // q, H // q))
                o_s = cv2.resize(orig, (W // q, H // q)).astype(np.float32)
                p_s = cv2.resize(p0, (W // q, H // q)).astype(np.float32)
                ratio = (norm_blur(o_s, ws, 6) + 2) / (norm_blur(p_s, ws, 6) + 2)
                ratio = cv2.resize(np.clip(ratio, 0.5, 2.0), (W, H), interpolation=cv2.INTER_LINEAR)
                region = np.clip(pl.astype(np.float32) * ratio, 0, 255)
                out = (m[..., None] * region + (1 - m[..., None]) * orig).astype(np.uint8)

        # Карта перекраски: стены (по правленому кадру) + одежда
        small = cv2.resize(out, (sw, shh), interpolation=cv2.INTER_AREA)
        gray = cv2.cvtColor(small, cv2.COLOR_RGB2GRAY).astype(np.float32)
        a_s = cv2.resize(alpha, (sw, shh))
        tgt = np.zeros((shh, sw, 3), np.float32); wsum = np.zeros((shh, sw), np.float32)
        if RECOLOR_WALLS and S_WALL:
            pw = scene_probs(small)[..., S_WALL].sum(-1) * (1 - a_s)
            tgt += pw[..., None] * wall_c; wsum += pw; wall_area += pw.sum()
        if RECOLOR_CLOTHES:
            pp = person_probs(cv2.resize(orig, (sw, shh), interpolation=cv2.INTER_AREA))
            on_person = 1 - pp[..., P_BG].sum(-1)
            keep = np.maximum(pp[..., P_KEEP].sum(-1), skin_like(small) * on_person)
            keep = cv2.GaussianBlur(np.maximum(keep, fill_holes(keep) * on_person), (0, 0), 1.5)
            pr = pp[..., P_RECOLOR] * (1 - np.clip(keep, 0, 1))[..., None]
            tgt += pr @ p_pal; wsum += pr.sum(-1)
            p_area += pp[..., P_RECOLOR].reshape(-1, len(P_RECOLOR)).sum(0)
        ctrl = np.dstack([tgt / np.maximum(wsum, 1e-6)[..., None], np.clip(wsum, 0, 1)]).astype(np.float32)
        if STABILITY > 0 and prev_ctrl is not None and np.abs(gray - prev_gray).mean() < 35:
            ctrl = STABILITY * warp_flow(prev_ctrl, flow_maps(gray, prev_gray)) + (1 - STABILITY) * ctrl
        prev_ctrl, prev_gray = ctrl, gray
        if ctrl[..., 3].max() > 0.01:
            out = recolor(out, cv2.resize(ctrl, (W, H), interpolation=cv2.INTER_LINEAR))
        cv2.imwrite(f"{tmp}/out/{i + 1:06d}.bmp", cv2.cvtColor(out, cv2.COLOR_RGB2BGR))

    # Сборка, звук из оригинала
    audio_in  = f'{limit} -i "{src}"' if has_audio(src) else ""
    audio_map = "-map 1:a:0 -c:a aac -b:a 192k -shortest" if audio_in else ""
    sh(f'ffmpeg -v error -y -framerate {fps} -i "{tmp}/out/%06d.bmp" {audio_in} '
       f'-map 0:v:0 {audio_map} -vf "scale=out_color_matrix=bt709:out_range=tv,format=yuv420p" '
       f'-colorspace bt709 -color_primaries bt709 -color_trc bt709 '
       f'-c:v libx264 -preset medium -crf {CRF} -pix_fmt yuv420p -movflags +faststart "{dst}"')
    shutil.rmtree(tmp, ignore_errors=True)

    name, total = os.path.basename(dst), n * sw * shh
    for nm, _, area in removed:
        log_rows.append([name, "убрано", nm, f"{100 * area:.1f}%", ""])
    if RECOLOR_WALLS and wall_area / total > 0.01:
        log_rows.append([name, "стены", "wall", f"{100 * wall_area / total:.1f}%", wall_hex])
    for j, c in enumerate(P_RECOLOR):
        if p_area[j] / total > 0.005:
            log_rows.append([name, "одежда", PERSON_LABELS[c], f"{100 * p_area[j] / total:.1f}%", lab_hex(p_pal[j])])
    print(f"Время: {(time.time() - t0) / 60:.1f} мин")

# 5) Обработка всех видео (уже готовые пропускаются — можно перезапускать)
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
            if new_log: wr.writerow(["файл", "что", "описание", "площадь", "цвет"])
            wr.writerows(rows)
        print("Готово:", dst)
        for r in rows: print("   ", " | ".join(x for x in r[1:] if x))
    except Exception as e:
        print("Ошибка на", src, "->", e)
    gc.collect(); torch.cuda.empty_cache()
print("Всё.")
