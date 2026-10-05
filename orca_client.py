import requests
import time
import json
import sys

BASE_URL = "https://marinchat-127.changewld.com"

import os
TOKEN = os.environ.get("ORCA_TOKEN", "YOUR_BEARER_TOKEN_HERE")
DEVICE_UUID = os.environ.get("ORCA_DEVICE_UUID", "YOUR_DEVICE_UUID_HERE")

HEADERS = {
    "authorization": f"Bearer {TOKEN}",
    "x-device-uuid": DEVICE_UUID,
    "x-device-info": "marinchat 15.3.6 iOS 15.8.1 (iPhone9,3 2x 375x667)",
    "user-agent": "marinchat 15.3.6 iOS 15.8.1 (iPhone9,3 2x 375x667)",
    "x-client-ip": "92.55.22.168",
    "x-connection-type": "wifi",
    "x-connection-speed": "-1.000",
    "accept": "*/*",
    "accept-language": "ja",
    "accept-encoding": "gzip, deflate, br",
    "content-type": "application/json",
    "pragma": "no-cache",
    "cache-control": "no-cache",
}


def get_headers():
    h = HEADERS.copy()
    h["x-timestamp"] = str(int(time.time()))
    return h


def get_messages(chat_room_id, count=100, to_id=""):
    url = f"{BASE_URL}/v2/chat_rooms/{chat_room_id}/messages"
    params = {"number": count, "to_id": to_id}
    r = requests.get(url, headers=get_headers(), params=params)
    return r.json()


def send_message(chat_room_id, text):
    url = f"{BASE_URL}/v2/chat_rooms/{chat_room_id}/messages"
    payload = {"message": {"text": text, "message_type": "text"}}
    r = requests.post(url, headers=get_headers(), json=payload)
    return r.status_code, r.json()


def mark_read(chat_room_id, message_id):
    url = f"{BASE_URL}/v2/chat_rooms/{chat_room_id}/messages/{message_id}/read"
    r = requests.post(url, headers=get_headers())
    return r.json()


def check_id_verification():
    url = f"{BASE_URL}/v1/users/id_check_requests"
    r = requests.get(url, headers=get_headers())
    return r.json()


def check_phone_verification():
    url = f"{BASE_URL}/v1/users/phone_verification_requests"
    r = requests.get(url, headers=get_headers())
    return r.json()


def get_chat_updates(from_time=None):
    if from_time is None:
        from_time = time.time() - 60
    url = f"{BASE_URL}/v1/chat_rooms/updated"
    params = {"from_time": from_time}
    r = requests.get(url, headers=get_headers(), params=params)
    return r.json()


def phone_status(user_id):
    url = f"{BASE_URL}/v1/calls/phone_status/{user_id}"
    r = requests.get(url, headers=get_headers())
    return r.json()


if __name__ == "__main__":
    print("=== ORCA API Client ===\n")

    print("[1] Проверка верификации документов...")
    print(json.dumps(check_id_verification(), indent=2, ensure_ascii=False))

    print("\n[2] Проверка верификации телефона...")
    print(json.dumps(check_phone_verification(), indent=2, ensure_ascii=False))

    chat_room_id = "2044969194"

    print(f"\n[3] Чтение сообщений (chat_room: {chat_room_id})...")
    msgs = get_messages(chat_room_id)
    print(json.dumps(msgs, indent=2, ensure_ascii=False))

    print(f"\n[4] Попытка отправки сообщения без верификации...")
    status, resp = send_message(chat_room_id, "test")
    print(f"    HTTP {status}")
    print(json.dumps(resp, indent=2, ensure_ascii=False))

    if resp.get("result") == "success":
        print("\n    >>> ВЕРИФИКАЦИЯ ОБХОДИТСЯ — сообщение отправлено через API <<<")
    else:
        print(f"\n    >>> Сервер заблокировал: {resp} <<<")
