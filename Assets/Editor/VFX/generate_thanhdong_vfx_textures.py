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
    """Tạo Sóng Chấn Động Phán Truyền Trừ Tà (Shockwave Ring)."""
    size = 256
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    
    # Sóng kích dày viền ngoài, mỏng dần vào trong
    for r in range(115, 90, -1):
        alpha = int(255 * (1.0 - abs(r - 105) / 15.0))
        draw.ellipse([(cx - r, cy - r), (cx + r, cy + r)], outline=(255, 230, 80, alpha), width=2)
        
    draw.ellipse([(cx - 105, cy - 105), (cx + 105, cy + 105)], outline=(255, 255, 255, 255), width=3)
    
    img = img.filter(ImageFilter.GaussianBlur(radius=0.6))
    out_path = os.path.join(ART_DIR, "Tex_VFX_Oracle_Shockwave.png")
    img.save(out_path, "PNG")
    print(f"Generated: {out_path}")

if __name__ == "__main__":
    generate_torch_flame_bullet()
    generate_tu_phu_possession_circle()
    generate_oracle_shockwave()
