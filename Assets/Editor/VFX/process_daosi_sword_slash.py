import os
from PIL import Image
import numpy as np

SRC_PATH = r"C:\Users\thuon\.gemini\antigravity-ide\brain\879351c2-8af8-40a9-84a5-fef3c5ecce1a\.user_uploaded\media_1787806050982.png"
dst_dir = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Skills"
os.makedirs(dst_dir, exist_ok=True)
dst_path = os.path.join(dst_dir, "Tex_DaoSi_SwordSlash.png")

img = Image.open(SRC_PATH).convert("RGBA")
w, h = img.size
pixels = np.array(img, dtype=np.float32)

# Xóa logo ngôi sao góc dưới phải
for y in range(int(h * 0.75), h):
    for x in range(int(w * 0.82), w):
        if pixels[y, x, 0] > 90 and pixels[y, x, 1] > 90 and pixels[y, x, 2] > 90:
            pixels[y, x] = [0, 0, 0, 255]

r = pixels[:, :, 0]
g = pixels[:, :, 1]
b = pixels[:, :, 2]

max_c = np.maximum(np.maximum(r, g), b)
alpha = np.clip((max_c - 10) / (255 - 10) * 255.0, 0, 255)
alpha = np.where(max_c > 25, np.minimum(255.0, alpha * 1.35), alpha)

final_rgba = np.zeros_like(pixels, dtype=np.uint8)
final_rgba[:, :, 0] = np.clip(r, 0, 255).astype(np.uint8)
final_rgba[:, :, 1] = np.clip(g, 0, 255).astype(np.uint8)
final_rgba[:, :, 2] = np.clip(b, 0, 255).astype(np.uint8)
final_rgba[:, :, 3] = np.clip(alpha, 0, 255).astype(np.uint8)

# Chuyển về khung vuông 1024x1024 có tâm ở chính giữa
out_img_raw = Image.fromarray(final_rgba, mode="RGBA")
new_size = max(w, h, 1024)
canvas = Image.new('RGBA', (new_size, new_size), (0, 0, 0, 0))
offset_x = (new_size - w) // 2
offset_y = (new_size - h) // 2
canvas.paste(out_img_raw, (offset_x, offset_y))
final_img = canvas.resize((1024, 1024), Image.LANCZOS)

final_img.save(dst_path, "PNG")
print(f"Processed Dao Si Sword Slash to: {dst_path}")
