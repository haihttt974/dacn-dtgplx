from sentence_transformers import SentenceTransformer
import json
import numpy as np
import os

model = SentenceTransformer('sentence-transformers/all-MiniLM-L6-v2')

# Load câu hỏi đã OCR
in_file = os.path.join(os.path.dirname(__file__), "questions.json")
data = json.load(open(in_file, "r", encoding="utf8"))

# Tạo embedding cho từng câu
for item in data:
    item["embedding"] = model.encode(item["text"]).tolist()

# Lưu lại
out_file = os.path.join(os.path.dirname(__file__), "questions_with_emb.json")
json.dump(data, open(out_file, "w", encoding="utf8"), ensure_ascii=False, indent=2)

print("DONE BUILD EMBEDDING! →", out_file)
