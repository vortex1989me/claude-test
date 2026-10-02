# ==============================================================================
#  Vid2Vid restyle для Google Colab (GPU T4) — ОДНА ЯЧЕЙКА, просто запусти.
#  Drive: Мой диск/Colab Notebooks/content/input_videos  ->  .../output_iphone
#  Пайплайн: HFLIP -> SD1.5 img2img + ControlNet Tile (лёгкая перерисовка текстур)
#            -> антидрожание: сглаживание по времени только изменений нейросети (по оптическому потоку)
#            -> полное разрешение + возврат мелких деталей оригинала (без мыла)
#            -> сборка обратно в mp4 с оригинальным звуком
# ==============================================================================

# ----------------------------- НАСТРОЙКИ --------------------------------------
BASE_DIR   = "/content/drive/MyDrive/Colab Notebooks/content"
INPUT_DIR  = f"{BASE_DIR}/input_videos"
OUTPUT_DIR = f"{BASE_DIR}/output_iphone"

PROMPT     = "photo of a person, natural skin texture, realistic, high quality, sharp focus"

STRENGTH       = 0.20   # сила перерисовки: 0.15–0.30. Выше 0.3 — появляются выдуманные детали
STEPS          = 20     # реально выполняется STEPS*STRENGTH ≈ 4 шага
TILE_SCALE     = 1.00   # вес ControlNet Tile: держит детали (тату, текст, фон) на месте
STABILITY      = 0.90   # антидрожание (0 = выкл, 0.95 = макс). Выше = меньше «плавания»
SEED           = 42     # одинаковый шум на всех кадрах = меньше «плавания»
DETAIL         = 1.00   # резкость: сколько мелких деталей вернуть с оригинала (0 = мыло, 1 = как в оригинале)
SCENE_CUT_DIFF = 35.0   # порог смены сцены (на склейке сглаживание не применяется)
SHORT_SIDE     = 512    # рабочее разрешение по КОРОТКОЙ стороне. 512 = родное для SD1.5 (448 = быстрее, хуже лица)
MAX_FPS        = 30     # если исходник 60 fps — обработаем 30
CRF            = 17     # качество итогового x264 (меньше = лучше)
TEST_SECONDS   = 0      # >0 = обработать только первые N секунд (быстрый тест настроек)
# ------------------------------------------------------------------------------

import os, sys, glob, json, shutil, subprocess, time

def sh(cmd):
    subprocess.run(cmd, shell=True, check=True)

# 1) Drive
from google.colab import drive
drive.mount("/content/drive")
os.makedirs(OUTPUT_DIR, exist_ok=True)

# 2) Зависимости
sh(f"{sys.executable} -m pip -q install -U diffusers transformers accelerate opencv-python-headless")

import cv2, numpy as np, torch
from PIL import Image
from tqdm.auto import tqdm
from diffusers import StableDiffusionControlNetImg2ImgPipeline, ControlNetModel, UniPCMultistepScheduler

assert torch.cuda.is_available(), "Нет GPU: Среда выполнения -> Сменить среду -> T4 GPU"
DEV = "cuda"

# 3) Модели
print("Загрузка моделей...")
cn_tile = ControlNetModel.from_pretrained("lllyasviel/control_v11f1e_sd15_tile", torch_dtype=torch.float16)
pipe = StableDiffusionControlNetImg2ImgPipeline.from_pretrained(
    "stable-diffusion-v1-5/stable-diffusion-v1-5",
    controlnet=cn_tile, torch_dtype=torch.float16,
    safety_checker=None, requires_safety_checker=False,
).to(DEV)
pipe.scheduler = UniPCMultistepScheduler.from_config(pipe.scheduler.config)
try:
    pipe.vae.enable_slicing()  # в новых diffusers pipe.enable_vae_slicing() удалён
except AttributeError:
    pass
pipe.set_progress_bar_config(disable=True)

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
    k = SHORT_SIDE / min(w, h)
    return max(8, int(w * k) // 8 * 8), max(8, int(h * k) // 8 * 8)

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

def process_video(src, dst):
    t0 = time.time()
    W, H, fps, hdr = probe(src)
    fps = min(fps, MAX_FPS)
    ww, wh = work_size(W, H)
    tmp = "/content/_v2v"
    shutil.rmtree(tmp, ignore_errors=True)
    os.makedirs(f"{tmp}/in"); os.makedirs(f"{tmp}/out")

    # HFLIP + (HDR->SDR) + fps — кадры в ПОЛНОМ разрешении (нужны для возврата резкости)
    limit = f"-t {TEST_SECONDS}" if TEST_SECONDS > 0 else ""
    base_vf = f"hflip,fps={fps},scale={W}:{H}"
    sigma = max(1.0, 1.0 * W / ww)  # граница «мелких деталей», которые теряются при уменьшении
    tonemap = ("zscale=t=linear:npl=100,format=gbrpf32le,zscale=p=bt709,"
               "tonemap=tonemap=hable:desat=0,zscale=t=bt709:m=bt709:r=tv,format=yuv420p,")
    try:
        sh(f'ffmpeg -v error {limit} -i "{src}" -vf "{tonemap if hdr else ""}{base_vf}" "{tmp}/in/%06d.bmp"')
    except subprocess.CalledProcessError:
        print("Тонмаппинг HDR недоступен, извлекаю без него")
        for f in glob.glob(f"{tmp}/in/*.bmp"): os.remove(f)
        sh(f'ffmpeg -v error {limit} -i "{src}" -vf "{base_vf}" "{tmp}/in/%06d.bmp"')
    frames = sorted(glob.glob(f"{tmp}/in/*.bmp"))

    prev_gray = prev_cur = prev_resid = None
    for i, fp in enumerate(tqdm(frames, desc=os.path.basename(src))):
        full = cv2.cvtColor(cv2.imread(fp), cv2.COLOR_BGR2RGB)
        cur = cv2.resize(full, (ww, wh), interpolation=cv2.INTER_AREA)
        cur_gray = cv2.cvtColor(cur, cv2.COLOR_RGB2GRAY).astype(np.float32)
        cur_pil = Image.fromarray(cur)

        # Каждый кадр генерируется ТОЛЬКО из оригинала (без подмешивания прошлых результатов),
        # поэтому артефакты не копятся и не «прилипают» к экрану.
        g = torch.Generator(DEV).manual_seed(SEED)
        out = pipe(
            prompt=PROMPT, image=cur_pil, control_image=cur_pil,
            strength=STRENGTH, num_inference_steps=STEPS, guidance_scale=1.0,
            controlnet_conditioning_scale=TILE_SCALE, generator=g,
        ).images[0]
        cur_f = cur.astype(np.float32)

        # Антидрожание. «Плавание» живёт только в разнице (нейросеть − оригинал), сам оригинал стабилен.
        # Сглаживаем по времени именно эту разницу, сдвигая прошлую по движению (оптический поток).
        # Там, где поток ошибается (быстрое движение, руки перекрывают лицо), сглаживание ослабевает —
        # поэтому шлейфов нет: основа кадра всегда берётся из текущего оригинала.
        resid = np.array(out).astype(np.float32) - cur_f
        if STABILITY > 0 and prev_resid is not None and np.abs(cur_gray - prev_gray).mean() < SCENE_CUT_DIFF:
            maps = flow_maps(cur_gray, prev_gray)
            err = np.abs(warp(prev_cur, maps) - cur_f).mean(axis=2)
            conf = cv2.GaussianBlur(np.exp(-err / 20.0), (0, 0), 2)[..., None]
            w = STABILITY * conf
            resid = w * warp(prev_resid, maps) + (1 - w) * resid
        prev_resid, prev_cur, prev_gray = resid, cur_f, cur_gray
        out_np = np.clip(cur_f + resid, 0, 255)

        # Обратно в полное разрешение + мелкие детали оригинала (ресницы, волосы, кожа) — без мыла
        big = cv2.resize(out_np, (W, H), interpolation=cv2.INTER_LANCZOS4)
        f32 = full.astype(np.float32)
        hi = f32 - cv2.GaussianBlur(f32, (0, 0), sigma)
        final = np.clip(big + DETAIL * hi, 0, 255).astype(np.uint8)
        cv2.imwrite(f"{tmp}/out/{i + 1:06d}.bmp", cv2.cvtColor(final, cv2.COLOR_RGB2BGR))

    # Сборка: обратно в исходное разрешение, звук из оригинала
    audio_in  = f'{limit} -i "{src}"' if has_audio(src) else ""
    audio_map = "-map 1:a:0 -c:a aac -b:a 192k -shortest" if audio_in else ""
    sh(f'ffmpeg -v error -y -framerate {fps} -i "{tmp}/out/%06d.bmp" {audio_in} '
       f'-map 0:v:0 {audio_map} -vf "scale=out_color_matrix=bt709:out_range=tv,format=yuv420p" '
       f'-colorspace bt709 -color_primaries bt709 -color_trc bt709 '
       f'-c:v libx264 -preset medium -crf {CRF} -pix_fmt yuv420p -movflags +faststart "{dst}"')
    shutil.rmtree(tmp, ignore_errors=True)
    print(f"Время: {(time.time() - t0) / 60:.1f} мин")

# 5) Обработка всех видео (уже готовые пропускаются — можно перезапускать)
EXTS = (".mp4", ".mov", ".m4v", ".mkv", ".avi", ".webm")
videos = sorted(f for f in glob.glob(f"{INPUT_DIR}/*") if f.lower().endswith(EXTS))
print(f"Найдено видео: {len(videos)}")
suffix = "_test" if TEST_SECONDS > 0 else "_v2v"
for src in videos:
    dst = f"{OUTPUT_DIR}/{os.path.splitext(os.path.basename(src))[0]}{suffix}.mp4"
    if os.path.exists(dst):
        print("Пропуск (уже есть, удалите файл чтобы переделать):", dst); continue
    try:
        process_video(src, dst)
        print("Готово:", dst)
    except Exception as e:
        print("Ошибка на", src, "->", e)
print("Всё.")
