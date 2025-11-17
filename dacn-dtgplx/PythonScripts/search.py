from sentence_transformers import SentenceTransformer
import numpy as np
import json
import os

# Load model
model = SentenceTransformer('sentence-transformers/all-MiniLM-L6-v2')

# Load dữ liệu đã có embedding
file_path = os.path.join(os.path.dirname(__file__), "questions_with_emb.json")
data = json.load(open(file_path, "r", encoding="utf8"))

# Tách embedding thành numpy để tính nhanh
embeddings = np.array([item["embedding"] for item in data])

def search(query, top_k=3):
    query_emb = model.encode(query)

    # Tính cosine similarity
    scores = embeddings @ query_emb / (np.linalg.norm(embeddings, axis=1) * np.linalg.norm(query_emb))

    # Lấy các kết quả tốt nhất
    top_idx = np.argsort(scores)[::-1][:top_k]

    results = []
    for idx in top_idx:
        results.append({
            "image": data[idx]["image"],
            "text": data[idx]["text"],
            "score": float(scores[idx])
        })

    return results

# Mode chạy trên CMD
print("=== SEARCH MODE ===")
print("Nhập từ khóa để tìm kiếm (gõ 'exit' để thoát).")

while True:
    q = input("\nTìm gì: ").strip()
    if q.lower() == "exit":
        break

    results = search(q)

    print("\nKết quả:")
    for r in results:
        print("\n-------------------------------")
        print("Điểm giống:", round(r["score"], 3))
        print("Ảnh:", r["image"])
        print("Câu hỏi OCR được:")
        print(r["text"])
