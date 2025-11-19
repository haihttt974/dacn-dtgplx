import sys
import json
import numpy as np
from sentence_transformers import SentenceTransformer
import os

# load model
model = SentenceTransformer("sentence-transformers/all-MiniLM-L6-v2")

# load embedding đã build
base = os.path.dirname(__file__)
json_path = os.path.join(base, "questions_with_emb.json")

data = json.load(open(json_path, "r", encoding="utf8"))

# lấy từ khóa từ ASP.NET
keyword = sys.argv[1]

# tạo embedding cho keyword
key_emb = model.encode(keyword)

# tính cosine similarity
def cosine(a, b):
    return float(np.dot(a, b) / (np.linalg.norm(a) * np.linalg.norm(b)))

results = []
for q in data:
    score = cosine(key_emb, q["embedding"])
    results.append({
        "image": q["image"],
        "text": q["text"],
        "score": score
    })

# sort giảm dần
results = sorted(results, key=lambda x: x["score"], reverse=True)

# chỉ lấy top 10
results = results[:10]

# xuất ra JSON cho ASP.NET
print(json.dumps(results, ensure_ascii=False))
