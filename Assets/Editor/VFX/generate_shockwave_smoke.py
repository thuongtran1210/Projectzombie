import os
import math
from PIL import Image, ImageDraw, ImageFilter
import numpy as np

ART_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\VFX"
os.makedirs(ART_DIR, exist_ok=True)

def generate_shockwave_smoke_puff():
    """
    Tạo Texture Khói Bụi Xung Kích / Vòng Khói Tỏa (Anime Stylized Shockwave Smoke Puff) 512x512.
    - Dạng cụm khói mây cuộn tròn (Multi-lobe Volumetric Cloud).
    - Viền ngoài mềm mại tự nhiên, lõi đậm đặc chuyển sắc mượt mà.
    """
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2

    # Vẽ 7-8 đám mây hình cầu lồng ghép để tạo thành 1 cụm khói cuộn đa khối (Cloud Cluster)
    lobes = [
        (cx, cy, 140, 255),           # Lõi chính giữa
        (cx - 55, cy - 40, 105, 230),  # Lobe trên trái
        (cx + 60, cy - 35, 110, 230),  # Lobe trên phải
        (cx - 70, cy + 30, 95, 210),   # Lobe dưới trái
        (cx + 65, cy + 40, 100, 220),  # Lobe dưới phải
        (cx, cy - 80, 85, 200),        # Đỉnh trên
        (cx, cy + 75, 90, 200),        # Đáy dưới
    ]

    # Vẽ các lớp tán xạ ánh sáng khói vàng kim / trắng
    for lx, ly, lr, alpha_peak in lobes:
        for r in range(lr, 10, -3):
            # Hàm suy giảm mờ dần theo bán kính
            factor = (1.0 - (r / float(lr))) ** 1.3
            alpha = int(alpha_peak * factor * 0.4)
            # Khói sáng ánh vàng kim nhạt (Vàng cát / Khí thiêng)
            draw.ellipse([(lx - r, ly - r), (lx + r, ly + r)], fill=(255, 245, 210, alpha))

    # Vẽ phần tâm đặc của cụm khói
    for lx, ly, lr, alpha_peak in lobes:
        r_core = int(lr * 0.55)
        for r in range(r_core, 5, -2):
            factor = (1.0 - (r / float(r_core))) ** 1.5
            alpha = int(alpha_peak * factor * 0.6)
            draw.ellipse([(lx - r, ly - r), (lx + r, ly + r)], fill=(255, 255, 240, alpha))

    # Áp dụng Gaussian Blur để hòa trộn các lobes thành 1 khối mây khói mềm mịn
    img = img.filter(ImageFilter.GaussianBlur(radius=8))

    out_path = os.path.join(ART_DIR, "Tex_VFX_Shockwave_SmokePuff.png")
    img.save(out_path, "PNG")
    print(f"Generated Shockwave Smoke Puff Texture: {out_path}")

if __name__ == "__main__":
    generate_shockwave_smoke_puff()
