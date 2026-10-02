# ==============================================================================
#  Автоперекраска видео для Google Colab (GPU T4) — ОДНА ЯЧЕЙКА, просто запусти.
#  Drive: Мой диск/Colab Notebooks/content/input_videos  ->  .../output_iphone
#  Для каждого видео:
#    HFLIP -> нейросети находят одежду и всё на фоне (стены, пол, мебель, ~150 видов предметов)
#    -> каждому найденному классу случайный цвет -> перекраска с сохранением яркости,
#       складок, теней и фактуры. Лицо, волосы и кожа НЕ трогаются.
#    -> сглаживание масок по времени (без мерцания краёв) -> mp4 с оригинальным звуком
#  Результат называется как исходник: IMG_0038.MOV -> output_iphone/IMG_0038.mp4
#  Журнал «что в какой цвет перекрашено»: output_iphone/recolor_log.csv
# ==============================================================================

# ----------------------------- НАСТРОЙКИ --------------------------------------
BASE_DIR   = "/content/drive/MyDrive/Colab Notebooks/content"
INPUT_DIR  = f"{BASE_DIR}/input_videos"
OUTPUT_DIR = f"{BASE_DIR}/output_iphone"

CHROMA_MIN     = (25, 50)  # насыщенность новых цветов (от, до). Больше = ярче
STABILITY      = 0.70   # сглаживание масок по времени (0 = выкл, 0.9 = макс)
PROTECT_SKIN   = True   # дополнительно беречь пиксели цвета кожи на человеке (шея, декольте)
SEG_SHORT_SIDE = 512    # разрешение для нейросетей по короткой стороне
SCENE_CUT_DIFF = 35.0   # порог смены сцены
MAX_FPS        = 30     # если исходник 60 fps — обработаем 30
CRF            = 17     # качество итогового x264 (меньше = лучше)
TEST_SECONDS   = 0      # >0 = обработать только первые N секунд (файл будет с припиской _test)
# ------------------------------------------------------------------------------

import os, sys, glob, json, shutil, subprocess, time, random, csv

def sh(cmd):
    subprocess.run(cmd, shell=True, check=True)

# 1) Drive
from google.colab import drive
drive.mount("/content/drive")
os.makedirs(OUTPUT_DIR, exist_ok=True)

import cv2, numpy as np, torch
import torch.nn.functional as F
from tqdm.auto import tqdm

assert torch.cuda.is_available(), "Нет GPU: Среда выполнения -> Сменить среду -> T4 GPU"
DEV = "cuda"

# >>> MODELS
# 2) Нейросети сегментации (SegFormer): человек по частям + интерьер (ADE20K, 150 классов)
from transformers import AutoModelForSemanticSegmentation

def load_first(names):
    for n in names:
        try:
            m = AutoModelForSemanticSegmentation.from_pretrained(n).to(DEV).half().eval()
            print("Модель:", n)
            return m
        except Exception as e:
            print(f"{n} недоступна ({type(e).__name__}), пробую следующую")
    raise RuntimeError("Не удалось скачать модель сегментации")

print("Загрузка моделей...")
person_model = load_first(["mattmdjaga/segformer_b2_clothes", "sayeed99/segformer_b3_clothes"])
scene_model  = load_first(["nvidia/segformer-b2-finetuned-ade-512-512",
                           "nvidia/segformer-b0-finetuned-ade-512-512"])
PERSON_LABELS = {int(k): v.lower() for k, v in person_model.config.id2label.items()}
SCENE_LABELS  = {int(k): v.lower() for k, v in scene_model.config.id2label.items()}

MEAN = torch.tensor([0.485, 0.456, 0.406], device=DEV).view(1, 3, 1, 1)
STD  = torch.tensor([0.229, 0.224, 0.225], device=DEV).view(1, 3, 1, 1)

@torch.no_grad()
def _probs(model, rgb):
    h, w = rgb.shape[:2]
    x = torch.from_numpy(rgb).to(DEV).permute(2, 0, 1)[None].float() / 255.0
    x = ((x - MEAN) / STD).half()
    logits = model(pixel_values=x).logits.float()                       # 1/4 разрешения
    logits = F.interpolate(logits, size=(h, w), mode="bilinear", align_corners=False)
    return logits.softmax(1)[0].permute(1, 2, 0).cpu().numpy()          # (h, w, классы)

def person_probs(rgb): return _probs(person_model, rgb)
def scene_probs(rgb):  return _probs(scene_model, rgb)
# <<< MODELS

# 3) Какие классы не трогаем
KEEP_WORDS = ("hair", "face", "arm", "leg", "glass", "skin")   # волосы, лицо, руки, ноги, очки
P_BG   = [i for i, n in PERSON_LABELS.items() if n.startswith("background")]
P_KEEP = [i for i, n in PERSON_LABELS.items() if any(w in n for w in KEEP_WORDS)]
P_RECOLOR = [i for i in PERSON_LABELS if i not in P_BG + P_KEEP]
S_PERSON = [i for i, n in SCENE_LABELS.items() if n in ("person", "person;individual;someone;somebody;mortal;soul")]
print("Не трогаем:", [PERSON_LABELS[i] for i in P_KEEP])
print("Одежда/аксессуары:", [PERSON_LABELS[i] for i in P_RECOLOR])

# 4) Утилиты
def probe(path):
    out = subprocess.check_output(
        f'ffprobe -v error -select_streams v:0 '
        f'-show_entries stream=width,height,r_frame_rate,color_transfer:stream_tags=rotate:stream_side_data=rotation '
        f'-of json "{path}"', shell=True)
    s = json.loads(out)["streams"][0]
    n, d = s["r_frame_rate"].split("/")
    w, h = int(s["width"]), int(s["height"])
    # Вертикальные видео с iPhone хранятся горизонтально + метка поворота
    rot = s.get("tags", {}).get("rotate")
    for sd in s.get("side_data_list", []):
        rot = sd.get("rotation", rot)
    if rot is not None and abs(int(float(rot))) % 180 == 90:
        w, h = h, w
    # iPhone может снимать в HDR (HLG/Dolby Vision) — без тонмаппинга цвета будут блёклыми
    hdr = s.get("color_transfer") in ("arib-std-b67", "smpte2084")
    return w, h, float(n) / float(d), hdr

def work_size(w, h):
    k = SEG_SHORT_SIDE / min(w, h)
    return max(32, round(w * k / 32) * 32), max(32, round(h * k / 32) * 32)

def has_audio(path):
    out = subprocess.check_output(
        f'ffprobe -v error -select_streams a -show_entries stream=index -of csv=p=0 "{path}"', shell=True)
    return bool(out.strip())

def flow_maps(cur_gray, prev_gray):
    # Для каждого пикселя текущего кадра — где он был в предыдущем
    flow = cv2.calcOpticalFlowFarneback(cur_gray, prev_gray, None, 0.5, 4, 21, 3, 5, 1.2, 0)
    h, w = cur_gray.shape
    gx, gy = np.meshgrid(np.arange(w, dtype=np.float32), np.arange(h, dtype=np.float32))
    return gx + flow[..., 0], gy + flow[..., 1]

def warp(img, maps):
    return cv2.remap(img, maps[0], maps[1], cv2.INTER_LINEAR, borderMode=cv2.BORDER_REPLICATE)

def random_palette(n):
    # Случайный оттенок (направление в плоскости a*b* пространства Lab) + насыщенность на класс
    hues = np.array([random.uniform(0, 2 * np.pi) for _ in range(n)], np.float32)
    cmin = np.array([random.uniform(*CHROMA_MIN) for _ in range(n)], np.float32)
    return np.stack([np.cos(hues), np.sin(hues), cmin], 1)            # (n, 3)

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
        touches_border = x == 0 or y == 0 or x + bw == w or y + bh == h
        if not touches_border and area < max_frac * h * w:
            out[lab == k] = 1
    return out.astype(np.float32)

def control_maps(small, p_pal, s_pal):
    """Карта перекраски на кадр: (dir_a, dir_b, chroma_min, вес_перекраски)."""
    pp = person_probs(small)
    sp = scene_probs(small)
    if S_PERSON:                                  # «человека» из модели интерьера не красим
        sp[..., S_PERSON] = 0
        sp /= np.maximum(sp.sum(-1, keepdims=True), 1e-6)
    p_bg = pp[..., P_BG].sum(-1, keepdims=True)
    p_keep = pp[..., P_KEEP].sum(-1)
    on_person = 1 - p_bg[..., 0]
    if PROTECT_SKIN:
        p_keep = np.maximum(p_keep, skin_like(small) * on_person)
    # Всё, что со всех сторон окружено кожей/лицом/волосами (татуировки, родинки), тоже не трогаем
    p_keep = np.maximum(p_keep, fill_holes(p_keep) * on_person)
    p_keep = cv2.GaussianBlur(p_keep, (0, 0), 1.5)
    num = pp[..., P_RECOLOR] @ p_pal + p_bg * (sp @ s_pal)          # взвешенная сумма цветов
    wsum = pp[..., P_RECOLOR].sum(-1) + p_bg[..., 0]
    tgt = num / np.maximum(wsum, 1e-6)[..., None]
    weight = np.clip(wsum, 0, 1) * (1 - np.clip(p_keep, 0, 1))
    return np.dstack([tgt, weight]).astype(np.float32), pp, sp

def recolor(full_rgb, ctrl_full):
    lab = cv2.cvtColor(full_rgb.astype(np.float32) / 255.0, cv2.COLOR_RGB2Lab)
    L, a, b = lab[..., 0], lab[..., 1], lab[..., 2]
    da, db, cmin, w = (ctrl_full[..., i] for i in range(4))
    norm = np.maximum(np.hypot(da, db), 1e-6)
    da, db = da / norm, db / norm
    chroma = np.hypot(a, b)
    # Насыщенность: не меньше заданной (чтобы серое/белое тоже окрасилось), у очень светлых
    # и очень тёмных мест — меньше (иначе выходит за пределы цветов и «кислотит»)
    lum_k = np.clip(np.minimum(L, 100 - L) / 35.0, 0.25, 1.0)
    new_c = np.maximum(chroma, cmin * lum_k)
    lab[..., 1] = w * da * new_c + (1 - w) * a
    lab[..., 2] = w * db * new_c + (1 - w) * b
    rgb = cv2.cvtColor(lab, cv2.COLOR_Lab2RGB)
    return (np.clip(rgb, 0, 1) * 255 + 0.5).astype(np.uint8)

def process_video(src, dst, log_rows):
    t0 = time.time()
    W, H, fps, hdr = probe(src)
    fps = min(fps, MAX_FPS)
    ww, wh = work_size(W, H)
    tmp = "/content/_recolor"
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

    # Случайные цвета на каждое видео: свой цвет каждому виду одежды и каждому предмету фона
    p_pal = random_palette(len(PERSON_LABELS))[P_RECOLOR]
    s_pal = random_palette(len(SCENE_LABELS))
    p_area = np.zeros(len(P_RECOLOR)); s_area = np.zeros(len(SCENE_LABELS))

    prev_gray = prev_ctrl = None
    for i, fp in enumerate(tqdm(frames, desc=os.path.basename(src))):
        full = cv2.cvtColor(cv2.imread(fp), cv2.COLOR_BGR2RGB)
        small = cv2.resize(full, (ww, wh), interpolation=cv2.INTER_AREA)
        gray = cv2.cvtColor(small, cv2.COLOR_RGB2GRAY).astype(np.float32)

        ctrl, pp, sp = control_maps(small, p_pal, s_pal)
        p_area += pp[..., P_RECOLOR].reshape(-1, len(P_RECOLOR)).sum(0)
        s_area += (sp * pp[..., P_BG].sum(-1, keepdims=True)).reshape(-1, sp.shape[-1]).sum(0)

        # Сглаживание масок по времени (по оптическому потоку): края не мерцают
        if STABILITY > 0 and prev_ctrl is not None and np.abs(gray - prev_gray).mean() < SCENE_CUT_DIFF:
            maps = flow_maps(gray, prev_gray)
            ctrl = STABILITY * warp(prev_ctrl, maps) + (1 - STABILITY) * ctrl
        prev_ctrl, prev_gray = ctrl, gray

        ctrl_full = cv2.resize(ctrl, (W, H), interpolation=cv2.INTER_LINEAR)
        out = recolor(full, ctrl_full)
        cv2.imwrite(f"{tmp}/out/{i + 1:06d}.bmp", cv2.cvtColor(out, cv2.COLOR_RGB2BGR))

    # Сборка, звук из оригинала
    audio_in  = f'{limit} -i "{src}"' if has_audio(src) else ""
    audio_map = "-map 1:a:0 -c:a aac -b:a 192k -shortest" if audio_in else ""
    sh(f'ffmpeg -v error -y -framerate {fps} -i "{tmp}/out/%06d.bmp" {audio_in} '
       f'-map 0:v:0 {audio_map} -vf "scale=out_color_matrix=bt709:out_range=tv,format=yuv420p" '
       f'-colorspace bt709 -color_primaries bt709 -color_trc bt709 '
       f'-c:v libx264 -preset medium -crf {CRF} -pix_fmt yuv420p -movflags +faststart "{dst}"')
    shutil.rmtree(tmp, ignore_errors=True)

    # Журнал: что нашлось (больше 0.5% площади) и в какой цвет перекрашено
    total = len(frames) * ww * wh
    name = os.path.basename(dst)
    for j, c in enumerate(P_RECOLOR):
        if p_area[j] / total > 0.005:
            log_rows.append([name, "одежда", PERSON_LABELS[c], f"{100 * p_area[j] / total:.1f}%", lab_hex(p_pal[j])])
    for c in np.argsort(-s_area):
        if s_area[c] / total > 0.005 and c not in S_PERSON:
            log_rows.append([name, "фон", SCENE_LABELS[c], f"{100 * s_area[c] / total:.1f}%", lab_hex(s_pal[c])])
    print(f"Время: {(time.time() - t0) / 60:.1f} мин")

# 5) Обработка всех видео (уже готовые пропускаются — можно перезапускать)
EXTS = (".mp4", ".mov", ".m4v", ".mkv", ".avi", ".webm")
videos = sorted(f for f in glob.glob(f"{INPUT_DIR}/*") if f.lower().endswith(EXTS))
print(f"Найдено видео: {len(videos)}")
suffix = "_test" if TEST_SECONDS > 0 else ""
log_path = f"{OUTPUT_DIR}/recolor_log.csv"
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
            if new_log: wr.writerow(["файл", "где", "что", "площадь", "новый цвет"])
            wr.writerows(rows)
        print("Готово:", dst)
        for r in rows: print("   ", r[1], "|", r[2], "|", r[3], "->", r[4])
    except Exception as e:
        print("Ошибка на", src, "->", e)
print("Всё.")
