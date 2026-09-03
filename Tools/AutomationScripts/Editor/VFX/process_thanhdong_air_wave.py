import os
from PIL import Image
import numpy as np

SRC_PATH = r"C:\Users\thuon\.gemini\antigravity-ide\brain\879351c2-8af8-40a9-84a5-fef3c5ecce1a\.user_uploaded\media_1787808247933.png"
dst_dir = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Skills"
os.makedirs(dst_dir, exist_ok=True)

img = Image.open(SRC_PATH).convert("RGBA")
w, h = img.size

# 1. Cắt Vệt Sóng Khí Mũi Tên Xé Gió (Chỉ lấy duy nhất 1 vệt hình mũi tên lớn góc phải: x từ 0.79w -> 0.98w, y từ 0.58h -> 0.94h)
box_wave = (int(w * 0.795), int(h * 0.585), int(w * 0.975), int(h * 0.935))
crop_wave = img.crop(box_wave)

# 2. Cắt Hạt Particle Ngôi Sao / Linh Lực (Góc trên: x từ 0.45w -> 0.55w, y từ 0.1h -> 0.25h)
box_spark = (int(w * 0.47), int(h * 0.08), int(w * 0.56), int(h * 0.25))
crop_spark = img.crop(box_spark)

def process_alpha_scaled(crop_img, target_size=1024, inner_scale=0.32):
    cw, ch = crop_img.size
    pixels = np.array(crop_img, dtype=np.float32)
    r, g, b = pixels[:, :, 0], pixels[:, :, 1], pixels[:, :, 2]
    max_c = np.maximum(np.maximum(r, g), b)
    alpha = np.clip((max_c - 10) / (255 - 10) * 255.0, 0, 255)
    alpha = np.where(max_c > 25, np.minimum(255.0, alpha * 1.35), alpha)
    
    rgba = np.zeros_like(pixels, dtype=np.uint8)
    rgba[:, :, 0] = np.clip(r, 0, 255).astype(np.uint8)
    rgba[:, :, 1] = np.clip(g, 0, 255).astype(np.uint8)
    rgba[:, :, 2] = np.clip(b, 0, 255).astype(np.uint8)
    rgba[:, :, 3] = np.clip(alpha, 0, 255).astype(np.uint8)
    
    out_raw = Image.fromarray(rgba, mode="RGBA")
    
    # Scale nội tại bên trong canvas
    scaled_w = int(cw * (target_size * inner_scale / max(cw, ch)))
    scaled_h = int(ch * (target_size * inner_scale / max(cw, ch)))
    resized_inner = out_raw.resize((scaled_w, scaled_h), Image.LANCZOS)
    
    canvas = Image.new('RGBA', (target_size, target_size), (0, 0, 0, 0))
    canvas.paste(resized_inner, ((target_size - scaled_w) // 2, (target_size - scaled_h) // 2))
    return canvas

final_wave = process_alpha_scaled(crop_wave, 1024, inner_scale=0.32)
final_wave.save(os.path.join(dst_dir, "Tex_ThanhDong_AirWave.png"), "PNG")

final_spark = process_alpha_scaled(crop_spark, 512, inner_scale=0.5)
final_spark.save(os.path.join(dst_dir, "Tex_ThanhDong_PetalSpark.png"), "PNG")

print("Successfully processed Tex_ThanhDong_AirWave.png and Tex_ThanhDong_PetalSpark.png!")
