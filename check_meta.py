#!/usr/bin/env python3
"""
Проверка метаданных видео из папки output/.
Показывает: телефон, дату, контейнер, цвет, аудио, маркеры ffmpeg.
"""

import subprocess
import json
import sys
import os
from pathlib import Path

OUTPUT_DIR = "output"

EXPECTED = {
    "major_brand": "qt",
    "com.apple.quicktime.make": "Apple",
    "com.apple.quicktime.model": "iPhone 15",
    "com.apple.quicktime.software": "26.6",
}

VIDEO_EXPECTED = {
    "color_space": "bt709",
    "color_transfer": "bt709",
    "color_primaries": "bt709",
    "color_range": "tv",
    "handler_name": "Core Media Video",
}

AUDIO_EXPECTED = {
    "sample_rate": "48000",
    "handler_name": "Core Media Audio",
}

FFMPEG_MARKERS = ["Lavf", "Lavc", "FFMP", "libav", "ffmpeg"]


def check_file(filepath):
    cmd = [
        "ffprobe", "-v", "quiet",
        "-print_format", "json",
        "-show_format", "-show_streams",
        filepath,
    ]
    r = subprocess.run(cmd, capture_output=True, text=True)
    if r.returncode != 0:
        print(f"  ОШИБКА: ffprobe не смог прочитать файл")
        return

    d = json.loads(r.stdout)
    tags = d.get("format", {}).get("tags", {})

    print(f"\n{'='*60}")
    print(f"  ФАЙЛ: {os.path.basename(filepath)}")
    print(f"{'='*60}")

    # Device info
    print(f"\n  --- Устройство ---")
    make = tags.get("com.apple.quicktime.make", "НЕТ")
    model = tags.get("com.apple.quicktime.model", "НЕТ")
    sw = tags.get("com.apple.quicktime.software", "НЕТ")
    print(f"  Производитель: {make}")
    print(f"  Модель:        {model}")
    print(f"  iOS:           {sw}")

    # Date
    print(f"\n  --- Дата ---")
    cdate = tags.get("com.apple.quicktime.creationdate", "НЕТ")
    ctime = tags.get("creation_time", "НЕТ")
    print(f"  Дата записи:   {cdate}")
    print(f"  UTC:           {ctime}")

    # Container
    print(f"\n  --- Контейнер ---")
    brand = tags.get("major_brand", "НЕТ").strip()
    compat = tags.get("compatible_brands", "НЕТ").strip()
    print(f"  major_brand:      {brand}", "✓" if "qt" in brand else "✗ ПАЛЕВО")
    print(f"  compatible_brands: {compat}", "✓" if "qt" in compat else "✗ ПАЛЕВО")

    # Streams
    for s in d.get("streams", []):
        st = s.get("codec_type")
        stags = s.get("tags", {})

        if st == "video":
            print(f"\n  --- Видео ---")
            print(f"  Кодек:         {s.get('codec_name', '?')} {s.get('profile', '')}")
            print(f"  Разрешение:    {s.get('width', '?')}x{s.get('height', '?')}")
            print(f"  FPS:           {s.get('r_frame_rate', '?')}")
            print(f"  Пиксели:       {s.get('pix_fmt', '?')}")

            cs = s.get("color_space", "НЕТ")
            ct = s.get("color_transfer", "НЕТ")
            cp = s.get("color_primaries", "НЕТ")
            cr = s.get("color_range", "НЕТ")
            print(f"  color_space:    {cs}", "✓" if cs == "bt709" else "✗")
            print(f"  color_transfer: {ct}", "✓" if ct == "bt709" else "✗")
            print(f"  color_primaries:{cp}", "✓" if cp == "bt709" else "✗")
            print(f"  color_range:    {cr}", "✓" if cr == "tv" else "✗")

            hn = stags.get("handler_name", "НЕТ")
            vid = stags.get("vendor_id", "НЕТ")
            enc = stags.get("encoder", "НЕТ")
            print(f"  handler_name:   {hn}", "✓" if "Core Media" in hn else "✗ ПАЛЕВО")
            print(f"  vendor_id:      {vid}", "✓" if vid in ("[0][0][0][0]", "НЕТ") else "✗ ПАЛЕВО")
            print(f"  encoder:        {enc}", "✓" if enc in ("H.264", "НЕТ") else "✗ ПАЛЕВО")

        elif st == "audio":
            print(f"\n  --- Аудио ---")
            print(f"  Кодек:         {s.get('codec_name', '?')} {s.get('profile', '')}")
            sr = s.get("sample_rate", "?")
            print(f"  Sample rate:   {sr} Hz", "✓" if sr == "48000" else "✗ ПАЛЕВО (не 48000)")
            print(f"  Каналы:        {s.get('channels', '?')}")

            hn = stags.get("handler_name", "НЕТ")
            vid = stags.get("vendor_id", "НЕТ")
            print(f"  handler_name:   {hn}", "✓" if "Core Media" in hn else "✗ ПАЛЕВО")
            print(f"  vendor_id:      {vid}", "✓" if vid in ("[0][0][0][0]", "НЕТ") else "✗ ПАЛЕВО")

    # ffmpeg marker scan
    print(f"\n  --- Маркеры ffmpeg ---")
    all_text = json.dumps(d)
    found = [m for m in FFMPEG_MARKERS if m.lower() in all_text.lower()]
    if found:
        print(f"  НАЙДЕНЫ: {', '.join(found)}  ✗ ПАЛЕВО!")
    else:
        print(f"  Не найдены ✓ ЧИСТО")

    print()


def main():
    folder = sys.argv[1] if len(sys.argv) > 1 else OUTPUT_DIR

    files = sorted(
        p for p in Path(folder).iterdir()
        if p.is_file() and p.suffix.lower() in (".mov", ".mp4", ".m4v")
    )

    if not files:
        print(f"Нет видео в {folder}/")
        return

    print(f"Проверка метаданных: {len(files)} файл(ов) в {folder}/")

    for f in files:
        check_file(str(f))

    print("=" * 60)
    print("Готово!")


if __name__ == "__main__":
    main()
