import os
import math
from PIL import Image, ImageDraw, ImageFilter
import numpy as np

PROJ_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Projectiles"
os.makedirs(PROJ_DIR, exist_ok=True)

def generate_arrow_thach_sanh(output_path):
    """Tạo Sprite Mũi Tên Thần Cung Thạch Sanh (Hướng +X, 100% Transparent)."""
    w, h = 256, 64
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cy = h // 2
    
    # 1. Đuôi lông vũ mũi tên (Xanh ngọc bích)
    draw.polygon([(20, cy), (4, cy - 18), (35, cy)], fill=(0, 220, 160, 240))
    draw.polygon([(20, cy), (4, cy + 18), (35, cy)], fill=(0, 220, 160, 240))
    
    # 2. Thân mũi tên gỗ thần
    draw.line([(25, cy), (210, cy)], fill=(34, 139, 34, 255), width=8)
    draw.line([(25, cy), (210, cy)], fill=(120, 255, 200, 255), width=3)
    
    # 3. Đầu mũi tên ngọc sắc nhọn hình tam giác hướng sang phải (+X)
    draw.polygon([
        (250, cy),        # Mũi nhọn
        (195, cy - 20),   # Cánh trên
        (205, cy),        # Khuyết giữa
        (195, cy + 20)    # Cánh dưới
    ], fill=(0, 255, 200, 255), outline=(255, 255, 255, 255), width=2)
    
    # Lõi sáng trắng trên đầu tên
    draw.polygon([
        (245, cy),
        (205, cy - 10),
        (212, cy),
        (205, cy + 10)
    ], fill=(255, 255, 255, 255))
    
    # Hào quang viền
    img = img.filter(ImageFilter.GaussianBlur(radius=0.6))
    img.save(output_path, "PNG")
    print(f"Generated Arrow Thach Sanh: {output_path}")

def generate_arrow_no_than(output_path):
    """Tạo Sprite Mũi Tên Đồng Cổ Nỏ Thần (Hướng +X, 100% Transparent)."""
    w, h = 256, 64
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cy = h // 2
    
    # Đuôi lông vũ vàng kim
    draw.polygon([(20, cy), (4, cy - 16), (35, cy)], fill=(212, 175, 55, 240))
    draw.polygon([(20, cy), (4, cy + 16), (35, cy)], fill=(212, 175, 55, 240))
    
    # Thân mũi tên đồng
    draw.line([(25, cy), (210, cy)], fill=(184, 115, 51, 255), width=8)
    draw.line([(25, cy), (210, cy)], fill=(255, 230, 120, 255), width=3)
    
    # Đầu mũi tên đồng 3 cạnh cổ phong
    draw.polygon([
        (250, cy),
        (195, cy - 18),
        (205, cy),
        (195, cy + 18)
    ], fill=(255, 215, 0, 255), outline=(255, 255, 255, 255), width=2)
    
    draw.polygon([
        (245, cy),
        (205, cy - 8),
        (212, cy),
        (205, cy + 8)
    ], fill=(255, 255, 255, 255))
    
    img = img.filter(ImageFilter.GaussianBlur(radius=0.6))
    img.save(output_path, "PNG")
    print(f"Generated Arrow No Than: {output_path}")

def generate_phitieu_batquai_sprite(output_path):
    """Tạo Sprite Phi Tiêu Bát Quái (128x128, 100% Transparent)."""
    size = 128
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    
    # 8 Cánh phi tiêu sắc bén
    for i in range(8):
        angle = i * (2 * math.pi / 8)
        tip_x = cx + 55 * math.cos(angle)
        tip_y = cy + 55 * math.sin(angle)
        
        a1 = angle - 0.25
        a2 = angle + 0.25
        x1 = cx + 22 * math.cos(a1)
        y1 = cy + 22 * math.sin(a1)
        x2 = cx + 22 * math.cos(a2)
        y2 = cy + 22 * math.sin(a2)
        
        draw.polygon([(tip_x, tip_y), (x1, y1), (cx, cy), (x2, y2)], fill=(220, 240, 255, 255), outline=(0, 180, 255, 255), width=1)
        
    # Vòng tròn âm dương ở tâm
    draw.ellipse([cx - 20, cy - 20, cx + 20, cy + 20], fill=(20, 40, 80, 255), outline=(255, 215, 0, 255), width=2)
    draw.ellipse([cx - 8, cy - 8, cx + 8, cy + 8], fill=(255, 255, 255, 255))
    
    img = img.filter(ImageFilter.GaussianBlur(radius=0.5))
    img.save(output_path, "PNG")
    print(f"Generated Phi Tieu Bat Quai Sprite: {output_path}")

if __name__ == "__main__":
    generate_arrow_thach_sanh(os.path.join(PROJ_DIR, "Arrow_ThachSanh.png"))
    generate_arrow_no_than(os.path.join(PROJ_DIR, "Arrow_NoThan.png"))
    generate_phitieu_batquai_sprite(os.path.join(PROJ_DIR, "PhiTieu_BatQuai.png"))
