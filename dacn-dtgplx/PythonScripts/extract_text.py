import pytesseract
from PIL import Image
import os
import json

# đường dẫn Tesseract
pytesseract.pytesseract.tesseract_cmd = r"C:\Program Files\Tesseract-OCR\tesseract.exe"

# lấy đường dẫn thư mục project (ra ngoài PythonScripts)
base = os.path.dirname(os.path.dirname(__file__))

# đường dẫn tới thư mục ảnh
folder = os.path.join(base, "wwwroot", "images", "cau_hoi")
folder = os.path.abspath(folder)

print("Đang đọc ảnh từ:", folder)

data = []

for file in os.listdir(folder):
    if file.lower().endswith((".jpg", ".png", ".jpeg")):
        path = os.path.join(folder, file)
        img = Image.open(path)
        text = pytesseract.image_to_string(img, lang="vie")
        data.append({"image": file, "text": text.strip()})

out_file = os.path.join(os.path.dirname(__file__), "questions.json")

json.dump(data, open(out_file, "w", encoding="utf8"), ensure_ascii=False, indent=2)

print("DONE OCR! ➜", out_file)