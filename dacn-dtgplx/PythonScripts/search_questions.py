import sys
import json
import math
from openai import OpenAI

def read_stdin_json():
    raw = sys.stdin.read()
    return json.loads(raw)

def cosine(a, b):
    if not a or not b:
        return 0.0
    if len(a) != len(b):
        m = min(len(a), len(b))
        a = a[:m]
        b = b[:m]
    dot = 0.0
    mag1 = 0.0
    mag2 = 0.0
    for x, y in zip(a, b):
        dot += x * y
        mag1 += x * x
        mag2 += y * y
    if mag1 == 0 or mag2 == 0:
        return 0.0
    return dot / math.sqrt(mag1 * mag2)

def main():
    # Nhận dữ liệu từ C#: { "query": "...", "apiKey": "..."}
    data = read_stdin_json()
    query = data.get("query", "").strip()
    api_key = data.get("apiKey", "").strip()

    if not query:
        print("[]")
        return

    # Load embeddings đã precompute từ file JSON
    # Cấu trúc: [ { "image": "001.jpg", "text": "...", "embedding": [ ... ] }, ... ]
    with open("questions_with_emb.json", "r", encoding="utf-8") as f:
        questions = json.load(f)

    # Tạo embedding cho query
    client = OpenAI(api_key=api_key)
    emb_res = client.embeddings.create(
        model="text-embedding-3-small",
        input=query
    )
    query_vec = emb_res.data[0].embedding

    scored = []

    for q in questions:
        emb = q.get("embedding", [])
        score = cosine(query_vec, emb)

        # image: "001.jpg" -> id: 1
        image = q.get("image", "")
        name = image.split("/")[-1]            # bỏ path folder
        name = name.split(".")[0]              # bỏ .jpg
        name = name.lstrip("0") or "0"         # "001" -> "1"
        try:
            qid = int(name)
        except:
            qid = 0

        if qid > 0:
            scored.append((qid, score))

    # Sắp xếp giảm dần theo score
    scored.sort(key=lambda x: x[1], reverse=True)

    top_ids = [item[0] for item in scored[:30]]

    # Trả về JSON list id: [12, 45, 7, ...]
    print(json.dumps(top_ids, ensure_ascii=False))


if __name__ == "__main__":
    main()
