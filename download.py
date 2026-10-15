#!/usr/bin/env python3
"""TikTok downloader через RapidAPI. Ссылки: links.txt, прокси: proxy.txt, видео: tiktok_downloads/

pip install requests pysocks

Запуск:
  python download.py                 # без прокси
  python download.py --proxy         # видео качаются через SOCKS5 из proxy.txt (по кругу)
  python download.py --proxy --api-proxy   # через прокси и запросы к API
  python download.py --threads 5
"""
import argparse, itertools, os, re, sys, threading
from concurrent.futures import ThreadPoolExecutor
import requests

# ===== НАСТРОЙКИ (сверьте с вкладкой Endpoints -> Code Snippets на RapidAPI) =====
API_KEY = os.environ.get("RAPIDAPI_KEY", "ВСТАВЬТЕ_КЛЮЧ")
API_HOST = "tiktok-video-downloader-api.p.rapidapi.com"  # x-rapidapi-host
API_PATH = "/"                                           # путь эндпоинта, напр. "/media"
API_METHOD = "GET"
URL_PARAM = "videoUrl"                                   # имя параметра со ссылкой (по скриншоту)
# =================================================================================

BASE = os.path.dirname(os.path.abspath(__file__))  # папка, где лежит скрипт
OUT_DIR = os.path.join(BASE, "tiktok_downloads")
LINKS_FILE = os.path.join(BASE, "links.txt")
PROXY_FILE = os.path.join(BASE, "proxy.txt")
lock = threading.Lock()


def load_lines(path):
    if not os.path.exists(path):
        return []
    with open(path, encoding="utf-8") as f:
        return [l.strip() for l in f if l.strip() and not l.startswith("#")]


def to_proxy_url(line):
    """host:port:user:pass -> socks5h://user:pass@host:port (socks5h = DNS через прокси)"""
    p = line.split(":")
    if len(p) == 4:
        host, port, user, pw = p
        return f"socks5h://{user}:{pw}@{host}:{port}"
    if len(p) == 2:
        return f"socks5h://{p[0]}:{p[1]}"
    raise ValueError(f"Неверный формат прокси: {line}")


def proxies_dict(purl):
    return {"http": purl, "https": purl} if purl else None


def find_video_url(data):
    """Ищет в JSON прямую ссылку на видео: сначала по типичным ключам, потом любой .mp4/video URL."""
    preferred = ("downloadurl", "nowm", "no_watermark", "play", "download", "video", "url", "hdplay", "hd")
    urls = []

    def walk(o, key=""):
        if isinstance(o, dict):
            for k, v in o.items():
                walk(v, str(k).lower())
        elif isinstance(o, list):
            for v in o:
                walk(v, key)
        elif isinstance(o, str) and o.startswith("http"):
            urls.append((key, o))

    walk(data)
    for pk in preferred:
        for k, u in urls:
            if pk in k and "watermark" not in k.replace("no_watermark", "") and "cover" not in k and "avatar" not in k and "music" not in k:
                return u
    for k, u in urls:
        if ".mp4" in u or "video" in u:
            return u
    return None


def get_video_url(link, purl):
    headers = {"x-rapidapi-key": API_KEY, "x-rapidapi-host": API_HOST}
    url = f"https://{API_HOST}{API_PATH}"
    if API_METHOD == "GET":
        r = requests.get(url, headers=headers, params={URL_PARAM: link}, proxies=proxies_dict(purl), timeout=60)
    else:
        r = requests.post(url, headers=headers, json={URL_PARAM: link}, proxies=proxies_dict(purl), timeout=60)
    r.raise_for_status()
    v = find_video_url(r.json())
    if not v:
        raise RuntimeError(f"Ссылка на видео не найдена в ответе: {r.text[:300]}")
    return v


def download(video_url, path, purl):
    with requests.get(video_url, stream=True, proxies=proxies_dict(purl), timeout=120,
                      headers={"User-Agent": "Mozilla/5.0", "Referer": "https://www.tiktok.com/"}) as r:
        r.raise_for_status()
        tmp = path + ".part"
        with open(tmp, "wb") as f:
            for chunk in r.iter_content(1 << 16):
                f.write(chunk)
    os.replace(tmp, path)


def name_for(link, idx):
    m = re.search(r"tiktok\.com/(?:.*/video/)?([A-Za-z0-9_]+)/?(?:\?|$)", link)
    return f"{idx:04d}_{m.group(1) if m else 'video'}.mp4"


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--proxy", action="store_true", help="качать видео через прокси из proxy.txt")
    ap.add_argument("--api-proxy", action="store_true", help="запросы к API тоже через прокси")
    ap.add_argument("--threads", type=int, default=3)
    a = ap.parse_args()

    if "ВСТАВЬТЕ" in API_KEY:
        sys.exit("Укажите API_KEY в скрипте или переменной RAPIDAPI_KEY")
    links = load_lines(LINKS_FILE)
    if not links:
        sys.exit(f"{LINKS_FILE} пуст")
    os.makedirs(OUT_DIR, exist_ok=True)

    cycle = None
    if a.proxy:
        plist = [to_proxy_url(p) for p in load_lines(PROXY_FILE)]
        if not plist:
            sys.exit(f"{PROXY_FILE} пуст")
        cycle = itertools.cycle(plist)

    def next_proxy():
        if cycle is None:
            return None
        with lock:
            return next(cycle)

    def work(item):
        idx, link = item
        path = os.path.join(OUT_DIR, name_for(link, idx))
        if os.path.exists(path):
            print(f"[skip] {link}")
            return True
        for attempt in range(3):  # при ошибке пробуем со следующим прокси
            purl = next_proxy()
            try:
                v = get_video_url(link, purl if a.api_proxy else None)
                download(v, path, purl)
                print(f"[ok]   {link} -> {path}")
                return True
            except Exception as e:
                print(f"[err]  {link} (попытка {attempt+1}): {e}")
        return False

    with ThreadPoolExecutor(a.threads) as ex:
        res = list(ex.map(work, enumerate(links, 1)))
    print(f"Готово: {sum(res)}/{len(res)}")


if __name__ == "__main__":
    main()
