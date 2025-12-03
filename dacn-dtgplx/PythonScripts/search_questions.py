import sys
import os
import json
import math
from sentence_transformers import SentenceTransformer

def safe_print(obj):
    sys.stdout.write(json.dumps(obj, ensure_ascii=False))
    sys.stdout.flush()

def read_stdin_json():
    raw = sys.stdin.read()
    if not raw.strip():
        return {}
    try:
        return json.loads(raw)
    except:
        return {}

def cosine(a, b):
    dot = sum(x*y for x, y in zip(a,b))
    mag1 = math.sqrt(sum(x*x for x in a))
    mag2 = math.sqrt(sum(y*y for y in b))
    return dot / (mag1 * mag2) if mag1 and mag2 else 0.0

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
JSON_PATH = os.path.join(BASE_DIR, "questions_with_emb.json")

# Model NHỎ – Chạy nhanh
model = SentenceTransformer("all-MiniLM-L6-v2")

def main():
    data = read_stdin_json()
    query = data.get("query", "").strip()

    if not query:
        safe_print([])
        return

    with open(JSON_PATH, "r", encoding="utf-8") as f:
        questions = json.load(f)

    qvec = model.encode(query).tolist()

    scored = []

    for q in questions:
        emb = q.get("embedding")
        score = cosine(qvec, emb)

        # Lấy ID từ tên ảnh
        image = q.get("image", "")
        name = image.split("/")[-1].split(".")[0]
        name = name.lstrip("0") or "0"

        try:
            qid = int(name)
        except:
            continue

        scored.append((qid, score))

    scored.sort(key=lambda x: x[1], reverse=True)

    top_ids = [x[0] for x in scored[:50]]

    safe_print(top_ids)

if __name__ == "__main__":
    main()
