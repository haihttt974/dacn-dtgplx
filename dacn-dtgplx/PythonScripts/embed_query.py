from sentence_transformers import SentenceTransformer
import sys, json

model = SentenceTransformer("keepitreal/vietnamese-sbert")

def main():
    raw = sys.stdin.read()
    payload = json.loads(raw)
    text = payload["query"]

    emb = model.encode(text).tolist()

    print(json.dumps(emb, ensure_ascii=False))

if __name__ == "__main__":
    main()
