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
    
def generate_dongson_wave_sprite(output_path):
    """Tạo Sprite Sóng Âm Trống Đồng Đông Sơn (Hình cánh cung / lưỡi sóng âm hoàng kim hướng +X, 100% Transparent)."""
    w, h = 256, 160
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = 60, h // 2
    
    # 1. Vẽ các dải sóng âm hình vòng cung hoàng kim tỏa về bên phải (+X)
    for r in range(120, 40, -10):
        t = (r - 40) / 80.0
        alpha = int(220 * math.sin(t * math.pi))
        bbox = [cx - r, cy - r, cx + r, cy + r]
        draw.arc(bbox, start=-65, end=65, fill=(212, 175, 55, alpha), width=10)
        draw.arc(bbox, start=-65, end=65, fill=(255, 235, 150, alpha), width=4)
        
    # 2. Lưỡi sóng âm chính cực sáng ở mũi
    main_r = 115
    main_bbox = [cx - main_r, cy - main_r, cx + main_r, cy + main_r]
    draw.arc(main_bbox, start=-60, end=60, fill=(255, 255, 255, 255), width=5)
    
    # 3. Hoa văn tia sáng năng lượng hình thoi cổ phong
    for angle_deg in [-45, -20, 0, 20, 45]:
        rad = math.radians(angle_deg)
        px = cx + (main_r - 10) * math.cos(rad)
        py = cy + (main_r - 10) * math.sin(rad)
        draw.ellipse([px - 6, py - 6, px + 6, py + 6], fill=(255, 240, 180, 255), outline=(255, 255, 255, 255), width=2)
    
    img = img.filter(ImageFilter.GaussianBlur(radius=0.8))
    img.save(output_path, "PNG")
    print(f"Generated Dong Son Soundwave Sprite: {output_path}")

def generate_cinnabar_grenade_sprite(output_path):
    """Tạo Sprite Hạt Chu Sa Hỏa Lựu (128x128, 100% Transparent)."""
    size = 128
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2

    # 1. Hào quang lửa bên ngoài
    for r in range(54, 32, -2):
        t = (r - 32) / 22.0
        alpha = int(180 * (1.0 - t))
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(255, int(100 * (1 - t)), 20, alpha))

    # 2. Thân viên ngọc chu sa đỏ thẫm
    for r in range(32, 10, -2):
        t = (r - 10) / 22.0
        r_col = int(220 * (1 - t * 0.3))
        g_col = int(30 * (1 - t))
        b_col = int(20 * (1 - t))
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(r_col, g_col, b_col, 255))

    # 3. Lõi lửa thần sa hoàng kim rực sáng
    for r in range(16, 2, -2):
        t = (r - 2) / 14.0
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(255, int(220 + 35 * (1 - t)), int(140 * (1 - t)), 255))
    draw.ellipse([cx - 4, cy - 4, cx + 4, cy + 4], fill=(255, 255, 255, 255))

    # 4. Tia lửa xoáy quanh viên chu sa
    for i in range(4):
        ang = i * (math.pi / 2) + 0.3
        tip_x = cx + int(42 * math.cos(ang))
        tip_y = cy + int(42 * math.sin(ang))
        draw.ellipse([tip_x - 3, tip_y - 3, tip_x + 3, tip_y + 3], fill=(255, 240, 150, 230))

    img = img.filter(ImageFilter.GaussianBlur(radius=0.6))
    img.save(output_path, "PNG")
    print(f"Generated Cinnabar Grenade Sprite: {output_path}")

def generate_mada_talisman_sprite(output_path):
    """Tạo Sprite Lá Bùa Thủy Quỷ Ma Da (64x128, 100% Transparent)."""
    w, h = 64, 128
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # 1. Giấy bùa xanh chàm thủy mặc
    margin_x, margin_y = 8, 12
    draw.rectangle([margin_x, margin_y, w - margin_x, h - margin_y], fill=(25, 45, 65, 250), outline=(0, 200, 220, 255), width=2)

    # 2. Đầu bùa hình mái vòm cổ
    draw.polygon([(margin_x, margin_y), (w // 2, 4), (w - margin_x, margin_y)], fill=(35, 60, 85, 250), outline=(0, 200, 220, 255), width=2)

    # 3. Ấn chú thủy quỷ màu xanh ngọc phát quang
    cx = w // 2
    draw.ellipse([cx - 12, 28, cx + 12, 52], outline=(0, 240, 255, 255), width=2)
    draw.line([(cx, 34), (cx, 46)], fill=(0, 255, 255, 255), width=2)
    draw.line([(cx - 7, 40), (cx + 7, 40)], fill=(0, 255, 255, 255), width=2)

    # Chữ triện nguyền rủa sóng nước bên dưới
    for y_offset in [62, 78, 94]:
        draw.line([(cx - 14, y_offset), (cx + 14, y_offset)], fill=(0, 230, 255, 240), width=2)
        draw.line([(cx, y_offset - 4), (cx, y_offset + 8)], fill=(0, 230, 255, 240), width=2)

    draw.line([(cx - 8, 106), (cx + 8, 106)], fill=(0, 255, 255, 255), width=3)

    img = img.filter(ImageFilter.GaussianBlur(radius=0.5))
    img.save(output_path, "PNG")
    print(f"Generated Ma Da Talisman Sprite: {output_path}")

def generate_mada_water_orb_sprite(output_path):
    """Tạo Sprite Cầu Bọt Nước Thủy Quỷ (128x128, 100% Transparent)."""
    size = 128
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2

    # 1. Hào quang nước tỏa ra ngoài
    for r in range(56, 36, -2):
        t = (r - 36) / 20.0
        alpha = int(160 * (1.0 - t))
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(0, 180, 240, alpha))

    # 2. Khối cầu nước xanh lam ngọc
    for r in range(36, 10, -2):
        t = (r - 10) / 26.0
        g_val = int(140 + 100 * (1 - t))
        b_val = int(200 + 55 * (1 - t))
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(0, g_val, b_val, 240))

    # 3. Vân xoáy nước và bọt khí ma mị
    draw.arc([cx - 24, cy - 24, cx + 24, cy + 24], start=30, end=240, fill=(255, 255, 255, 220), width=3)
    draw.arc([cx - 16, cy - 16, cx + 16, cy + 16], start=120, end=330, fill=(220, 255, 255, 240), width=3)

    # 4. Lõi sáng tâm cầu nước
    draw.ellipse([cx - 8, cy - 8, cx + 8, cy + 8], fill=(240, 255, 255, 255))
    draw.ellipse([cx - 3, cy - 3, cx + 3, cy + 3], fill=(255, 255, 255, 255))

    img = img.filter(ImageFilter.GaussianBlur(radius=0.7))
    img.save(output_path, "PNG")
    print(f"Generated Ma Da Water Orb Sprite: {output_path}")

if __name__ == "__main__":
    generate_arrow_thach_sanh(os.path.join(PROJ_DIR, "Arrow_ThachSanh.png"))
    generate_arrow_no_than(os.path.join(PROJ_DIR, "Arrow_NoThan.png"))
    generate_phitieu_batquai_sprite(os.path.join(PROJ_DIR, "PhiTieu_BatQuai.png"))
    generate_dongson_wave_sprite(os.path.join(PROJ_DIR, "DongSon_Wave_Bullet.png"))
    generate_cinnabar_grenade_sprite(os.path.join(PROJ_DIR, "Cinnabar_Grenade.png"))
    generate_mada_talisman_sprite(os.path.join(PROJ_DIR, "Talisman_MaDa.png"))
    generate_mada_water_orb_sprite(os.path.join(PROJ_DIR, "Mada_Water_Orb.png"))

