import os
from PIL import Image
import numpy as np

TRAIL_SRC = r"C:\Users\thuon\.gemini\antigravity-ide\brain\879351c2-8af8-40a9-84a5-fef3c5ecce1a\.user_uploaded\media_1787804678478.png"
HEAD_SRC = r"C:\Users\thuon\.gemini\antigravity-ide\brain\879351c2-8af8-40a9-84a5-fef3c5ecce1a\.user_uploaded\media_1787804688376.jpg"

dst_dir = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Skills"
os.makedirs(dst_dir, exist_ok=True)

# 1. Xử lý Ảnh 1: Trail Effect (Tia Sét & Luồng Năng Lượng Xanh Ngọc)
def process_trail():
    img = Image.open(TRAIL_SRC).convert("RGBA")
    w, h = img.size
    pixels = np.array(img, dtype=np.float32)

    # Xóa logo ngôi sao ở góc dưới phải
    for y in range(int(h * 0.75), h):
        for x in range(int(w * 0.82), w):
            if pixels[y, x, 0] > 100 and pixels[y, x, 1] > 100 and pixels[y, x, 2] > 100:
                pixels[y, x] = [0, 0, 0, 255]

    r, g, b = pixels[:, :, 0], pixels[:, :, 1], pixels[:, :, 2]
    max_c = np.maximum(np.maximum(r, g), b)
    alpha = np.clip((max_c - 8) / (255 - 8) * 255.0, 0, 255)
    alpha = np.where(max_c > 25, np.minimum(255.0, alpha * 1.35), alpha)

    res = np.zeros_like(pixels, dtype=np.uint8)
    res[:, :, 0] = np.clip(r, 0, 255).astype(np.uint8)
    res[:, :, 1] = np.clip(g, 0, 255).astype(np.uint8)
    res[:, :, 2] = np.clip(b, 0, 255).astype(np.uint8)
    res[:, :, 3] = np.clip(alpha, 0, 255).astype(np.uint8)

    out = Image.fromarray(res, mode="RGBA")
    out_path = os.path.join(dst_dir, "Tex_DaoSi_TalismanTrail.png")
    out.save(out_path, "PNG")
    print(f"Processed Trail to: {out_path}")

# 2. Xử lý Ảnh 2: Đầu Đạn Bát Quái Linh Phù (Projectile Core / Head)
def process_head():
    img = Image.open(HEAD_SRC).convert("RGBA")
    w, h = img.size
    pixels = np.array(img, dtype=np.float32)

    # Xóa logo ngôi sao ở góc dưới phải
    for y in range(int(h * 0.75), h):
        for x in range(int(w * 0.82), w):
            if pixels[y, x, 0] > 80 and pixels[y, x, 1] > 80 and pixels[y, x, 2] > 80:
                pixels[y, x] = [0, 0, 0, 255]

    r, g, b = pixels[:, :, 0], pixels[:, :, 1], pixels[:, :, 2]
    max_c = np.maximum(np.maximum(r, g), b)
    
    # Nền đen xung quanh vòng tròn
    alpha = np.clip((max_c - 12) / (255 - 12) * 255.0, 0, 255)
    alpha = np.where(max_c > 25, np.minimum(255.0, alpha * 1.3), alpha)

    res = np.zeros_like(pixels, dtype=np.uint8)
    res[:, :, 0] = np.clip(r, 0, 255).astype(np.uint8)
    res[:, :, 1] = np.clip(g, 0, 255).astype(np.uint8)
    res[:, :, 2] = np.clip(b, 0, 255).astype(np.uint8)
    res[:, :, 3] = np.clip(alpha, 0, 255).astype(np.uint8)

    out = Image.fromarray(res, mode="RGBA")
    out_path = os.path.join(dst_dir, "Tex_DaoSi_TalismanHead.png")
    out.save(out_path, "PNG")
    print(f"Processed Head to: {out_path}")

if __name__ == "__main__":
    process_trail()
    process_head()
