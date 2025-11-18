import sys
import json
import numpy as np
from openai import OpenAI

client = OpenAI(api_key="YOUR_OPENAI_KEY")

# Input từ C#
raw = sys.stdin.read()
data = json.loads(raw)
query = data["query"]

# Load embeddings
with open("questions_with_emb.json", "r", encoding="utf8") as f:
    QUESTIONS = json.load(f)

# Tạo embedding cho query
emb = client.embeddings.create(
    model="text-embedding-3-small",
    input=query
).data[0].embedding

def cosine(a, b):
    a = np.array(a)
    b = np.array(b)
    return float(np.dot(a, b) / (np.linalg.norm(a) * np.linalg.norm(b)))

# Tính độ giống nhau
results = []
for q in QUESTIONS:
    score = cosine(emb, q["embedding"])
    results.append({
        "id": q["id"],
        "similarity": score,
        "question": q["question"]
    })

# Lấy top 10
results = sorted(results, key=lambda x: x["similarity"], reverse=True)[:10]

# Trả JSON về C#
print(json.dumps(results, ensure_ascii=False))
