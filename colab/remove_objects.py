# ==============================================================================
#  Удаление предметов с фона (ProPainter) + перекраска одежды — Google Colab (GPU T4), ОДНА ЯЧЕЙКА.
#  Drive: Мой диск/Colab Notebooks/content/input_videos  ->  .../output_iphone
#  Для каждого видео:
#    1) HFLIP
#    2) Находятся отдельные предметы на фоне (SAM; запасной вариант — поиск «заметных пятен»),
#       случайные REMOVE_COUNT из них выбираются, двигающиеся предметы пропускаются
#    3) ProPainter убирает их со ВСЕГО видео: берёт настоящий фон из тех кадров, где он виден,
#       остальное дорисовывает — согласованно по времени, без мерцания
#    4) Одежда (по желанию) перекрашивается в естественный цвет; лицо, волосы, кожа, тату — нет
#    5) Всё, что не правилось, — пиксели оригинала в полном разрешении
#  Результат называется как исходник: IMG_0038.MOV -> output_iphone/IMG_0038.mp4
#  Отчёт: output_iphone/IMG_0038_debug.jpg (жёлтым — найденные предметы, красным — убранные)
# ==============================================================================

# ----------------------------- НАСТРОЙКИ --------------------------------------
BASE_DIR   = "/content/drive/MyDrive/Colab Notebooks/content"
INPUT_DIR  = f"{BASE_DIR}/input_videos"
OUTPUT_DIR = f"{BASE_DIR}/output_iphone"

REMOVE_COUNT   = 3      # сколько предметов убирать
OBJ_MIN_AREA   = 0.001  # размер предмета: от 0.1% кадра...
OBJ_MAX_AREA   = 0.05   # ...до 5% кадра
OBJ_MIN_CONTRAST = 8    # насколько предмет должен отличаться от окружения (меньше = больше кандидатов)
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
    sam = hf_pipeline("mask-generation", model="facebook/sam-vit-base", device=0)
    print("Поиск предметов: SAM")
except Exception as e:
    sam = None
    print(f"SAM недоступна ({type(e).__name__}) — будет только поиск «заметных пятен»")

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

def sam_masks(rgb):
    """Все маски предметов от SAM (0/1). Пороги снижены, чтобы находить и мелкие предметы."""
    if sam is None:
        return []
    from PIL import Image
    out = sam(Image.fromarray(rgb), points_per_batch=64, pred_iou_thresh=0.7,
              stability_score_thresh=0.8, crops_n_layers=1)
    masks = [m.cpu().numpy() if hasattr(m, "cpu") else np.asarray(m) for m in out["masks"]]
    return [np.squeeze(m).astype(np.uint8) for m in masks]
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
def contrast(lab_img, obj):
    """Насколько предмет отличается по цвету от полоски вокруг (ΔE в Lab)."""
    ring = cv2.dilate(obj, np.ones((15, 15), np.uint8)) - obj
    if ring.sum() < 20 or obj.sum() < 20:
        return 0.0
    return float(np.linalg.norm(lab_img[obj > 0].mean(0) - lab_img[ring > 0].mean(0)))

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

def find_objects(frame0, alpha0, sw, shh):
    small = cv2.resize(frame0, (sw, shh), interpolation=cv2.INTER_AREA)
    lab_img = cv2.cvtColor(small.astype(np.float32) / 255, cv2.COLOR_RGB2Lab)
    a_s = cv2.resize(alpha0, (sw, shh))
    person = cv2.dilate((a_s > 0.2).astype(np.uint8), np.ones((9, 9), np.uint8))
    stats = {"SAM масок": 0, "не тот размер": 0, "на человеке": 0, "слабый контраст": 0, "подходят": 0}
    found = []
    for src, masks in (("SAM", sam_masks(small)), ("пятна", blob_masks(small, person == 0))):
        if src == "SAM":
            stats["SAM масок"] = len(masks)
        for m in masks:
            area = m.sum() / (sw * shh)
            if not (OBJ_MIN_AREA <= area <= OBJ_MAX_AREA):
                stats["не тот размер"] += 1; continue
            if (m & person).sum() > 0.05 * m.sum():
                stats["на человеке"] += 1; continue
            c = contrast(lab_img, m)
            if c < OBJ_MIN_CONTRAST:
                stats["слабый контраст"] += 1; continue
            stats["подходят"] += 1
            found.append((c, area, m, src))
    # Убираем дубли (вложенные и пересекающиеся маски), сначала самые заметные
    found.sort(key=lambda x: -x[0])
    taken, cands = np.zeros((shh, sw), np.uint8), []
    for c, area, m, src in found:
        if (m & taken).sum() > 0.3 * m.sum():
            continue
        taken |= m
        cands.append((c, area, m, src))
    print("Поиск предметов:", ", ".join(f"{k}: {v}" for k, v in stats.items()),
          f"| без дублей: {len(cands)}")
    return cands, small

def is_static(obj_full, frames, alphas_dir, A_list, idxs, W, H):
    """Предмет не двигается сам (люди, машины, волны): сравнение с кадром 0 после компенсации
    движения камеры и автоэкспозиции."""
    f0 = cv2.cvtColor(cv2.imread(frames[0]), cv2.COLOR_BGR2RGB).astype(np.float32)
    ring = cv2.dilate(obj_full, np.ones((31, 31), np.uint8)) - obj_full
    for t in idxs:
        ft = cv2.cvtColor(cv2.imread(frames[t]), cv2.COLOR_BGR2RGB)
        back = cv2.warpAffine(ft, A_list[t][:2], (W, H), flags=cv2.INTER_LINEAR | cv2.WARP_INVERSE_MAP,
                              borderMode=cv2.BORDER_REFLECT).astype(np.float32)
        at = cv2.imread(f"{alphas_dir}/{t:06d}.png", cv2.IMREAD_GRAYSCALE)
        at = cv2.warpAffine(at, A_list[t][:2], (W, H), flags=cv2.WARP_INVERSE_MAP) < 50
        o, r = (obj_full > 0) & at, (ring > 0) & at
        if o.sum() < 50:
            continue
        if r.sum() > 50:
            back = back * (f0[r].mean(0) / np.maximum(back[r].mean(0), 1))
        d = np.abs(back - f0).mean(-1)
        d_obj, d_ring = d[o].mean(), d[r].mean() if r.sum() > 50 else 0
        if d_obj > 20 and d_obj > 2.5 * d_ring + 5:
            return False
    return True

def save_debug(small, cands, removed_idx, path, note):
    img = small.copy()
    for i, (c, area, m, src) in enumerate(cands):
        cnts, _ = cv2.findContours(m, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        cv2.drawContours(img, cnts, -1, (255, 0, 0) if i in removed_idx else (255, 220, 0),
                         3 if i in removed_idx else 1)
    cv2.putText(img, note, (8, 24), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (255, 255, 255), 2, cv2.LINE_AA)
    cv2.imwrite(path, cv2.cvtColor(img, cv2.COLOR_RGB2BGR), [cv2.IMWRITE_JPEG_QUALITY, 90])

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

def clothes_weight(rgb_small):
    pp = person_probs(rgb_small)
    on_person = 1 - pp[..., P_BG].sum(-1)
    keep = np.maximum(pp[..., P_KEEP].sum(-1), skin_like(rgb_small) * on_person)
    keep = cv2.GaussianBlur(np.maximum(keep, fill_holes(keep) * on_person), (0, 0), 1.5)
    return np.clip(pp[..., P_RECOLOR].sum(-1) * (1 - np.clip(keep, 0, 1)), 0, 1)

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

    # Выбор предметов
    frame0 = cv2.cvtColor(cv2.imread(frames[0]), cv2.COLOR_BGR2RGB)
    alpha0 = cv2.imread(f"{tmp}/alpha/000000.png", cv2.IMREAD_GRAYSCALE) / 255.0
    cands, small0 = find_objects(frame0, alpha0, sw, shh)
    order = list(range(min(len(cands), 4 * REMOVE_COUNT)))      # из самых заметных — случайные
    random.shuffle(order)
    idxs = sorted({n // 3, 2 * n // 3, n - 1} - {0})
    removed, moving = [], 0
    for i in order:
        if len(removed) >= REMOVE_COUNT:
            break
        obj_full = cv2.resize(cands[i][2], (W, H), interpolation=cv2.INTER_NEAREST)
        if is_static(obj_full, frames, f"{tmp}/alpha", A_list, idxs, W, H):
            removed.append(i)
        else:
            moving += 1
    note = f"найдено {len(cands)}, убрано {len(removed)}" + (f", двигаются {moving}" if moving else "")
    print("Предметы:", note)
    name0 = os.path.splitext(os.path.basename(dst))[0]
    save_debug(small0, cands, set(removed), f"{OUTPUT_DIR}/{name0}_debug.jpg", note)

    # Маска удаления в кадре 0 (полное разрешение), с запасом под тень/кант
    obj0 = np.zeros((H, W), np.uint8)
    for i in removed:
        obj0 |= cv2.resize(cands[i][2], (W, H), interpolation=cv2.INTER_NEAREST)
    if removed:
        obj0 = cv2.dilate(obj0, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (25, 25)))

    # ProPainter: кадры + маски в рабочем разрешении, по кускам
    pp_frames = None
    if removed:
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
            w = clothes_weight(small)
            if prev_w is not None:
                w = 0.5 * w + 0.5 * prev_w                # меньше дрожания краёв
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
        c, area, m, src_ = cands[i]
        log_rows.append([name, "убрано", f"предмет ({src_})", f"{100 * area:.2f}%"])
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
