# Video Uniqualizer

`video_uniqualizer.py` делает из одного видео N уникальных копий через FFmpeg.
Промпт, по которому он написан, лежит в [`PROMPT.md`](PROMPT.md).

## Что меняется

| Параметр | Как |
|---|---|
| Визуал (pHash) | микро-кроп, поворот, eq/hue, шум, unsharp, виньетка, `--mirror`; pHash измеряется, `--min-phash` усиливает изменения до порога |
| MD5 / SHA-256 | перекодирование, случайные параметры энкодера, новые метаданные, удаление SEI |
| Lanczos micro-scaling | `scale=...:flags=lanczos+accurate_rnd+full_chroma_int` на 1–7 % + кроп со сдвигом |
| Custom AQ / CQM | случайные `aq-mode`/`aq-strength` + своя матрица квантования (`cqmfile`, только x264) |
| Дополнительно | темп ±1–3.5 %, срез кадров, GOP/B-кадры/ref/me/subme/deblock/psy-rd, CRF, аудио: питч, громкость, EQ, high-pass, sample rate, битрейт |

## Установка

```bash
# нужен ffmpeg с libx264 (и libx265 для --codec h265)
pip install numpy   # для подсчёта pHash
```

## Примеры

```bash
python video_uniqualizer.py input.mp4
python video_uniqualizer.py input.mp4 -n 5 --intensity high -o out/ --report report.json
python video_uniqualizer.py videos/ -n 3 --min-phash 10 --seed 42
python video_uniqualizer.py input.mp4 --codec h265 --format mkv
python video_uniqualizer.py input.mp4 --dry-run          # только показать команду ffmpeg
```

Все случайные параметры берутся из `--seed` и сохраняются в JSON-отчёт.
