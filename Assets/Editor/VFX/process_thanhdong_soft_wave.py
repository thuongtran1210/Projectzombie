import os
from PIL import Image
import numpy as np

SRC_PATH = r"C:\Users\thuon\.gemini\antigravity-ide\brain\879351c2-8af8-40a9-84a5-fef3c5ecce1a\.user_uploaded\media_1787808939895.png"
dst_dir = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Skills"
os.makedirs(dst_dir, exist_ok=True)

img = Image.open(SRC_PATH).convert("RGBA")

# Xoay 90 độ ngược chiều kim đồng hồ để lưng cong quay sang PHẢI (+X) - hướng bay của đạn
rotated = img.transpose(Image.ROTATE_270)
rw, rh = rotated.size

pixels = np.array(rotated, dtype=np.float32)
r, g, b = pixels[:, :, 0], pixels[:, :, 1], pixels[:, :, 2]
max_c = np.maximum(np.maximum(r, g), b)
alpha = np.clip((max_c - 10) / (255 - 10) * 255.0, 0, 255)
alpha = np.where(max_c > 20, np.minimum(255.0, alpha * 1.35), alpha)

rgba = np.zeros_like(pixels, dtype=np.uint8)
rgba[:, :, 0] = np.clip(r, 0, 255).astype(np.uint8)
rgba[:, :, 1] = np.clip(g, 0, 255).astype(np.uint8)
rgba[:, :, 2] = np.clip(b, 0, 255).astype(np.uint8)
rgba[:, :, 3] = np.clip(alpha, 0, 255).astype(np.uint8)

out_raw = Image.fromarray(rgba, mode="RGBA")

# Scale vừa vặn nội tại (chiều cao khoảng 260px trên canvas 1024x1024)
target_size = 1024
inner_scale = 0.30
scaled_w = int(rw * (target_size * inner_scale / max(rw, rh)))
scaled_h = int(rh * (target_size * inner_scale / max(rw, rh)))
resized_inner = out_raw.resize((scaled_w, scaled_h), Image.LANCZOS)

canvas = Image.new('RGBA', (target_size, target_size), (0, 0, 0, 0))
canvas.paste(resized_inner, ((target_size - scaled_w) // 2, (target_size - scaled_h) // 2))

out_path = os.path.join(dst_dir, "Tex_ThanhDong_AirWave.png")
canvas.save(out_path, "PNG")
print(f"Successfully processed and updated {out_path} with smooth crescent wave!")
