# Hệ thống OCR + Tìm kiếm câu hỏi trắc nghiệm (Python Offline)

Dự án giúp bạn xây dựng hệ thống:

- Đọc chữ từ ảnh câu hỏi trắc nghiệm bằng Tesseract OCR
- Tự động tách câu hỏi và đáp án
- Tạo embedding bằng SentenceTransformers
- Tìm kiếm câu hỏi theo ngữ nghĩa (semantic search)
- Chạy hoàn toàn OFFLINE

---

# 1. YÊU CẦU

- Python 3.9+
- Tesseract OCR 5.x
- Ảnh câu hỏi (.jpg, .png)
- Windows 10/11

---

# 2. CÀI ĐẶT TESSERACT OCR

## 2.1. Tải về

Link tải chính thức:

https://github.com/UB-Mannheim/tesseract/wiki

Tải file:

```
tesseract-ocr-w64-setup-5.x.x.exe
```

## 2.2. Cài đặt theo mặc định

```
C:\Program Files\Tesseract-OCR
```
## 2.3. Kiểm tra Tesseract

```
tesseract --version
```

## 2.4. Cài tiếng Việt
```
Tải file `vie.traineddata` từ:
https://github.com/tesseract-ocr/tessdata

Copy vào:
C:\Program Files\Tesseract-OCR	essdata
```

---

# 3. CẤU TRÚC THƯ MỤC

```
project/
│
├── PythonScripts/
│   ├── extract_text.py
│   ├── build_embedding.py
│   ├── search.py
│   ├── requirements.txt
│
└── wwwroot/
    └── images/
        └── cau_hoi/
            ├── 001.jpg
            ├── 002.jpg
            ├── ...
```

---

# 4. CÀI THƯ VIỆN PYTHON

Tạo file:

```
requirements.txt
```

Chạy:

```
pip install -r requirements.txt
```

---
# 5. CHẠY CODE

## Bước 1 — OCR:

```
python extract_text.py
```

## Bước 2 — Tạo embedding:

```
python build_embedding.py
```

## Bước 3 — Tìm kiếm:

```
python search.py
```

---

# 6. LỖI THƯỜNG GẶP
- Không có vie.traineddata  
- Sai đường dẫn ảnh  
- Chưa cài Tesseract vào PATH  

---

# 7. KẾT LUẬN
Bạn đã có hệ thống:
- OCR thông minh  
- Tách câu hỏi + đáp án  
- Nhúng semantic  
- Tìm kiếm nhanh, offline  