import os
import math
from PIL import Image, ImageDraw, ImageFilter
import numpy as np

ART_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\VFX"
SKILL_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Skills"
os.makedirs(ART_DIR, exist_ok=True)
os.makedirs(SKILL_DIR, exist_ok=True)

def generate_torch_flame_bullet():
    """Tạo Sprite Đạn Lửa Mồi Định Hướng (Màu Đỏ Cam Chu Sa + Lõi Trắng Vàng)."""
    w, h = 128, 128
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = w // 2, h // 2
    
    # 1. Hào quang lửa lan tỏa (Đỏ Chu Sa)
    for r in range(50, 10, -2):
        alpha = int(180 * (1.0 - r / 50.0))
        draw.ellipse([(cx - r, cy - r), (cx + r, cy + r)], fill=(230, 60, 30, alpha))
        
    # 2. Ngọn lửa hình giọt nước hướng sang phải (+X)
    flame_pts = [
        (cx + 45, cy),          # Mũi lửa
        (cx + 10, cy - 25),     # Cánh trên
        (cx - 30, cy - 20),     # Thân trên
        (cx - 40, cy),          # Đuôi mồi
        (cx - 30, cy + 20),     # Thân dưới
        (cx + 10, cy + 25)      # Cánh dưới
    ]
    draw.polygon(flame_pts, fill=(255, 140, 0, 240))
    
    # 3. Lõi lửa nhiệt độ cao (Vàng Hoàng Kim -> Trắng)
    core_pts = [
        (cx + 35, cy),
        (cx + 5, cy - 14),
        (cx - 20, cy - 10),
        (cx - 25, cy),
        (cx - 20, cy + 10),
        (cx + 5, cy + 14)
    ]
    draw.polygon(core_pts, fill=(255, 240, 150, 255))
    
    # Lõi trắng tinh khiết
    draw.ellipse([(cx - 15, cy - 6), (cx + 15, cy + 6)], fill=(255, 255, 255, 255))
    
    img = img.filter(ImageFilter.GaussianBlur(radius=0.5))
    out_path = os.path.join(ART_DIR, "Tex_VFX_TorchFlame_Bullet.png")
    img.save(out_path, "PNG")
    print(f"Generated: {out_path}")

def generate_tu_phu_possession_circle():
    """Tạo Vòng Sáng Trận Pháp Tứ Phủ / Hào Quang Thánh Giáng Ngự (512x512)."""
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    
    # 1. Vòng tròn ngoài hào quang vàng kim
    draw.ellipse([(cx - 230, cy - 230), (cx + 230, cy + 230)], outline=(255, 215, 0, 200), width=6)
    draw.ellipse([(cx - 215, cy - 215), (cx + 215, cy + 215)], outline=(255, 180, 50, 160), width=3)
    
    # 2. Vòng tròn 4 cung Tứ Phủ (Đỏ, Xanh, Trắng, Vàng)
    # 4 biểu tượng hoa văn mây Tứ Phủ tại 4 góc
    r_quad = 175
    quad_colors = [
        (230, 50, 50, 240),    # Thiên Phủ (Đỏ) - 0 độ (Phải)
        (50, 180, 80, 240),    # Nhạc Phủ (Xanh Mộc) - 90 độ (Dưới)
        (220, 240, 255, 240),  # Thoải Phủ (Trắng Lam) - 180 độ (Trái)
        (255, 215, 0, 240)     # Địa Phủ (Vàng) - 270 độ (Trên)
    ]
    
    for idx, col in enumerate(quad_colors):
        ang = idx * (math.pi / 2.0)
        qx = int(cx + r_quad * math.cos(ang))
        qy = int(cy + r_quad * math.sin(ang))
        draw.ellipse([(qx - 25, qy - 25), (qx + 25, qy + 25)], fill=col, outline=(255, 255, 255, 255), width=2)
        # Tâm sáng
        draw.ellipse([(qx - 10, qy - 10), (qx + 10, qy + 10)], fill=(255, 255, 255, 255))
        
    # 3. Hoa văn Bát Tiên / Chữ Phù Trung Tâm
    draw.ellipse([(cx - 110, cy - 110), (cx + 110, cy + 110)], outline=(255, 230, 120, 220), width=4)
    
    # Hình sao 8 cánh nối tâm
    for i in range(8):
        a1 = i * (math.pi / 4.0)
        a2 = (i + 1) * (math.pi / 4.0)
        x1 = int(cx + 110 * math.cos(a1))
        y1 = int(cy + 110 * math.sin(a1))
        x2 = int(cx + 50 * math.cos((a1 + a2) / 2.0))
        y2 = int(cy + 50 * math.sin((a1 + a2) / 2.0))
        draw.line([(cx, cy), (x1, y1)], fill=(255, 215, 0, 180), width=2)
        draw.line([(x1, y1), (x2, y2)], fill=(255, 255, 255, 220), width=2)
        
    # Tâm sáng thiêng liêng
    for r in range(45, 0, -2):
        alpha = int(255 * (1.0 - r / 45.0))
        draw.ellipse([(cx - r, cy - r), (cx + r, cy + r)], fill=(255, 255, 230, alpha))
        
    img = img.filter(ImageFilter.GaussianBlur(radius=0.8))
    out_path = os.path.join(SKILL_DIR, "Tex_VFX_TuPhu_PossessionCircle.png")
    img.save(out_path, "PNG")
    print(f"Generated: {out_path}")

def generate_oracle_shockwave():
    """
    Tạo Texture Sóng Xung Kích Khí Ba (High-Energy Anime Shockwave) 512x512.
    - Mép ngoài sắc nét, nén khí siêu cao (Sharp Pure White Rim).
    - Thân sóng chuyển sắc vàng kim linh lực (Golden Energy Falloff).
    - Các đường vân sóng phụ đồng tâm (Layered Sonic Ripples).
    """
    size = 512
    arr = np.zeros((size, size, 4), dtype=np.float32)
    cx, cy = size / 2.0, size / 2.0
    
    y_coords, x_coords = np.ogrid[:size, :size]
    dist_from_center = np.sqrt((x_coords - cx) ** 2 + (y_coords - cy) ** 2)
    
    r_max = 240.0
    r_thickness = 55.0
    inward_dist = r_max - dist_from_center
    
    mask_main = (dist_from_center <= r_max) & (inward_dist >= 0) & (inward_dist <= r_thickness)
    decay = np.exp(-inward_dist / 14.0)
    sub1 = np.exp(-((inward_dist - 18.0) ** 2) / (2.0 * (3.5 ** 2))) * 0.75
    sub2 = np.exp(-((inward_dist - 34.0) ** 2) / (2.0 * (4.5 ** 2))) * 0.45
    
    total_intensity = np.clip(decay + sub1 + sub2, 0.0, 1.0)
    
    for y in range(size):
        for x in range(size):
            if mask_main[y, x]:
                val = total_intensity[y, x]
                d = inward_dist[y, x]
                if d <= 4.0:
                    edge_factor = d / 4.0
                    r = 255
                    g = 255
                    b = int(255 * (1.0 - edge_factor) + 220 * edge_factor)
                    a = int(255 * val)
                elif d <= 22.0:
                    t = (d - 4.0) / 18.0
                    r = 255
                    g = int(220 * (1.0 - t) + 160 * t)
                    b = int(80 * (1.0 - t) + 30 * t)
                    a = int(255 * val)
                else:
                    t = (d - 22.0) / (r_thickness - 22.0)
                    r = 255
                    g = int(160 * (1.0 - t) + 80 * t)
                    b = int(30 * (1.0 - t) + 10 * t)
                    a = int(255 * val * (1.0 - t))
                arr[y, x] = [r, g, b, a]

    out_img = Image.fromarray(arr.astype(np.uint8), "RGBA").filter(ImageFilter.GaussianBlur(radius=0.4))
    out_path = os.path.join(ART_DIR, "Tex_VFX_Oracle_Shockwave.png")
    out_img.save(out_path, "PNG")
    print(f"Generated Masterpiece Shockwave: {out_path}")

def generate_shockwave_smoke_puff():
    """Tạo Texture Khói Bụi Xung Kích / Vòng Khói Tỏa 512x512."""
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2

    lobes = [
        (cx, cy, 140, 255),
        (cx - 55, cy - 40, 105, 230),
        (cx + 60, cy - 35, 110, 230),
        (cx - 70, cy + 30, 95, 210),
        (cx + 65, cy + 40, 100, 220),
        (cx, cy - 80, 85, 200),
        (cx, cy + 75, 90, 200),
    ]

    for lx, ly, lr, alpha_peak in lobes:
        for r in range(lr, 10, -3):
            factor = (1.0 - (r / float(lr))) ** 1.3
            alpha = int(alpha_peak * factor * 0.4)
            draw.ellipse([(lx - r, ly - r), (lx + r, ly + r)], fill=(255, 245, 210, alpha))

    for lx, ly, lr, alpha_peak in lobes:
        r_core = int(lr * 0.55)
        for r in range(r_core, 5, -2):
            factor = (1.0 - (r / float(r_core))) ** 1.5
            alpha = int(alpha_peak * factor * 0.6)
            draw.ellipse([(lx - r, ly - r), (lx + r, ly + r)], fill=(255, 255, 240, alpha))

    img = img.filter(ImageFilter.GaussianBlur(radius=8))
    out_path = os.path.join(ART_DIR, "Tex_VFX_Shockwave_SmokePuff.png")
    img.save(out_path, "PNG")
    print(f"Generated Shockwave Smoke Puff: {out_path}")

if __name__ == "__main__":
    generate_torch_flame_bullet()
    generate_tu_phu_possession_circle()
    generate_oracle_shockwave()
    generate_shockwave_smoke_puff()
