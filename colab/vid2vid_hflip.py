# ==============================================================================
#  Vid2Vid restyle для Google Colab (GPU T4) — ОДНА ЯЧЕЙКА, просто запусти.
#  Drive: Мой диск/Colab Notebooks/content/input_videos  ->  .../output_iphone
#  Пайплайн: HFLIP -> SD1.5 img2img + ControlNet (Tile + Depth)
#            -> оптический поток (Farneback) для временной стабильности
#            -> сборка обратно в mp4 с оригинальным звуком
# ==============================================================================

# ----------------------------- НАСТРОЙКИ --------------------------------------
BASE_DIR   = "/content/drive/MyDrive/Colab Notebooks/content"
INPUT_DIR  = f"{BASE_DIR}/input_videos"
OUTPUT_DIR = f"{BASE_DIR}/output_iphone"

PROMPT     = "high quality photo, natural lighting, detailed textures, sharp focus"
NEG_PROMPT = "blurry, lowres, artifacts, deformed, distorted, watermark, text, cartoon"

STRENGTH       = 0.30   # denoising strength: 0.15–0.35 (выше = сильнее перерисовка, больше мерцания)
STEPS          = 25     # реально выполняется STEPS*STRENGTH шагов (~8)
CFG            = 5.0
TILE_SCALE     = 0.60   # вес ControlNet Tile (держит детали/цвета)
DEPTH_SCALE    = 0.45   # вес ControlNet Depth (держит геометрию)
SEED           = 42     # одинаковый шум на всех кадрах = меньше мерцания
FLOW_BLEND     = 0.30   # доля предыдущего (сгенерированного и сдвинутого потоком) кадра
SCENE_CUT_DIFF = 35.0   # порог смены сцены (на склейке поток не применяется)
MAX_SIDE       = 768    # рабочее разрешение по длинной стороне (T4: 640–768)
MAX_FPS        = 30     # если исходник 60 fps — обработаем 30 (вдвое быстрее)
CRF            = 17     # качество итогового x264 (меньше = лучше)
# ------------------------------------------------------------------------------

import os, sys, glob, json, shutil, subprocess

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
from diffusers import (StableDiffusionControlNetImg2ImgPipeline, ControlNetModel,
                       UniPCMultistepScheduler)
from transformers import pipeline as hf_pipeline

assert torch.cuda.is_available(), "Нет GPU: Среда выполнения -> Сменить среду -> T4 GPU"
DEV = "cuda"

# 3) Модели
print("Загрузка моделей...")
cn_tile  = ControlNetModel.from_pretrained("lllyasviel/control_v11f1e_sd15_tile",  torch_dtype=torch.float16)
cn_depth = ControlNetModel.from_pretrained("lllyasviel/control_v11f1p_sd15_depth", torch_dtype=torch.float16)
pipe = StableDiffusionControlNetImg2ImgPipeline.from_pretrained(
    "stable-diffusion-v1-5/stable-diffusion-v1-5",
    controlnet=[cn_tile, cn_depth], torch_dtype=torch.float16,
    safety_checker=None, requires_safety_checker=False,
).to(DEV)
pipe.scheduler = UniPCMultistepScheduler.from_config(pipe.scheduler.config)
pipe.enable_vae_slicing()
pipe.set_progress_bar_config(disable=True)

depth_est = hf_pipeline("depth-estimation", model="depth-anything/Depth-Anything-V2-Small-hf", device=0)

# 4) Утилиты
def probe(path):
    out = subprocess.check_output(
        f'ffprobe -v error -select_streams v:0 -show_entries stream=width,height,r_frame_rate '
        f'-of json "{path}"', shell=True)
    s = json.loads(out)["streams"][0]
    n, d = s["r_frame_rate"].split("/")
    return int(s["width"]), int(s["height"]), float(n) / float(d)

def work_size(w, h):
    k = MAX_SIDE / max(w, h)
    return max(8, int(w * k) // 8 * 8), max(8, int(h * k) // 8 * 8)

def has_audio(path):
    out = subprocess.check_output(
        f'ffprobe -v error -select_streams a -show_entries stream=index -of csv=p=0 "{path}"', shell=True)
    return bool(out.strip())

def warp(prev_img, cur_gray, prev_gray):
    flow = cv2.calcOpticalFlowFarneback(cur_gray, prev_gray, None, 0.5, 3, 21, 3, 5, 1.2, 0)
    h, w = cur_gray.shape
    gx, gy = np.meshgrid(np.arange(w), np.arange(h))
    mx = (gx + flow[..., 0]).astype(np.float32)
    my = (gy + flow[..., 1]).astype(np.float32)
    return cv2.remap(prev_img, mx, my, cv2.INTER_LINEAR, borderMode=cv2.BORDER_REPLICATE)

def process_video(src, dst):
    W, H, fps = probe(src)
    fps = min(fps, MAX_FPS)
    ww, wh = work_size(W, H)
    tmp = "/content/_v2v"
    shutil.rmtree(tmp, ignore_errors=True)
    os.makedirs(f"{tmp}/in"); os.makedirs(f"{tmp}/out")

    # HFLIP + масштаб + fps — сразу при извлечении кадров
    sh(f'ffmpeg -v error -i "{src}" -vf "hflip,fps={fps},scale={ww}:{wh}:flags=lanczos" '
       f'-q:v 1 "{tmp}/in/%06d.png"')
    frames = sorted(glob.glob(f"{tmp}/in/*.png"))

    prev_gray = prev_out = None
    for i, fp in enumerate(tqdm(frames, desc=os.path.basename(src))):
        cur = cv2.cvtColor(cv2.imread(fp), cv2.COLOR_BGR2RGB)
        cur_gray = cv2.cvtColor(cur, cv2.COLOR_RGB2GRAY)

        init = cur
        if prev_out is not None and np.abs(cur_gray.astype(np.float32) - prev_gray).mean() < SCENE_CUT_DIFF:
            warped = warp(prev_out, cur_gray, prev_gray)
            init = cv2.addWeighted(cur, 1 - FLOW_BLEND, warped, FLOW_BLEND, 0)

        cur_pil = Image.fromarray(cur)
        depth = depth_est(cur_pil)["depth"].convert("RGB").resize((ww, wh))
        g = torch.Generator(DEV).manual_seed(SEED)
        out = pipe(
            prompt=PROMPT, negative_prompt=NEG_PROMPT,
            image=Image.fromarray(init), control_image=[cur_pil, depth],
            strength=STRENGTH, num_inference_steps=STEPS, guidance_scale=CFG,
            controlnet_conditioning_scale=[TILE_SCALE, DEPTH_SCALE],
            generator=g,
        ).images[0]

        out_np = np.array(out)
        Image.fromarray(out_np).save(f"{tmp}/out/{i + 1:06d}.png")
        prev_out, prev_gray = out_np, cur_gray.astype(np.float32)

    # Сборка: обратно в исходное разрешение, звук из оригинала
    audio_in  = f'-i "{src}"' if has_audio(src) else ""
    audio_map = "-map 1:a:0 -c:a aac -b:a 192k -shortest" if audio_in else ""
    sh(f'ffmpeg -v error -y -framerate {fps} -i "{tmp}/out/%06d.png" {audio_in} '
       f'-map 0:v:0 {audio_map} -vf "scale={W}:{H}:flags=lanczos" '
       f'-c:v libx264 -preset slow -crf {CRF} -pix_fmt yuv420p -movflags +faststart "{dst}"')
    shutil.rmtree(tmp, ignore_errors=True)

# 5) Обработка всех видео (уже готовые пропускаются — можно перезапускать)
EXTS = (".mp4", ".mov", ".m4v", ".mkv", ".avi", ".webm")
videos = sorted(f for f in glob.glob(f"{INPUT_DIR}/*") if f.lower().endswith(EXTS))
print(f"Найдено видео: {len(videos)}")
for src in videos:
    dst = f"{OUTPUT_DIR}/{os.path.splitext(os.path.basename(src))[0]}_v2v.mp4"
    if os.path.exists(dst):
        print("Пропуск (уже есть):", dst); continue
    try:
        process_video(src, dst)
        print("Готово:", dst)
    except Exception as e:
        print("Ошибка на", src, "->", e)
print("Всё.")
