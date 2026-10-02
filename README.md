# Video Uniqualizer

`new.py` делает из одного видео N уникальных копий через FFmpeg.
Промпт, по которому он написан, лежит в [`PROMPT.md`](PROMPT.md).

## Структура папок

```
C:\Unic\15.09(CLAUDE)\
├── new.py
├── input_videos\    <- исходные видео
└── output_videos\   <- готовые копии
```

Без аргументов скрипт берёт все видео из `input_videos` и сохраняет результат в `output_videos`
(обе папки ищутся рядом с `new.py`, откуда бы его ни запустили). В Windows можно запускать двойным кликом.

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
python new.py                                   # input_videos -> output_videos, по 1 копии
python new.py -n 5 --intensity high --report report.json
python new.py -n 3 --min-phash 10 --seed 42
python new.py --codec h265 --format mkv
python new.py input_videos/clip.mp4 --dry-run   # только показать команду ffmpeg
```

Все случайные параметры берутся из `--seed` и сохраняются в JSON-отчёт.
