import os
import math
from PIL import Image, ImageDraw, ImageFilter
import numpy as np

SKILLS_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Skills"
ICONS_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Weapons"
os.makedirs(SKILLS_DIR, exist_ok=True)
os.makedirs(ICONS_DIR, exist_ok=True)

# -------------------------------------------------------------
# 1. BỘ SINH VẬT PHẨM ICON CỔ PHONG 512x512
# -------------------------------------------------------------
def generate_icon_trong_dong(output_path):
    """Icon Trống Đồng Đông Sơn Cổ Phong (Vàng Đồng & Viền Ngọc)."""
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    
    # Viền ngoài tròn vàng đồng cổ
    draw.ellipse([cx - 230, cy - 230, cx + 230, cy + 230], fill=(45, 30, 15, 255), outline=(212, 175, 55, 255), width=12)
    draw.ellipse([cx - 200, cy - 200, cx + 200, cy + 200], fill=(70, 50, 25, 255), outline=(255, 215, 0, 255), width=6)
    
    # Mặt trời 14 cánh ở giữa
    num_rays = 14
    for i in range(num_rays):
        angle = i * (2 * math.pi / num_rays)
        r_outer = 110
        r_inner = 30
        
        x_tip = cx + r_outer * math.cos(angle)
        y_tip = cy + r_outer * math.sin(angle)
        
        a_left = angle - math.pi / num_rays
        a_right = angle + math.pi / num_rays
        x_l = cx + r_inner * math.cos(a_left)
        y_l = cy + r_inner * math.sin(a_left)
        x_r = cx + r_inner * math.cos(a_right)
        y_r = cy + r_inner * math.sin(a_right)
        
        draw.polygon([(x_tip, y_tip), (x_l, y_l), (x_r, y_r)], fill=(255, 220, 80, 255))
        
    draw.ellipse([cx - 30, cy - 30, cx + 30, cy + 30], fill=(255, 245, 180, 255))
    
    # Vòng tròn chim Lạc bay ngược chiều kim đồng hồ
    for bird in range(6):
        b_angle = bird * (2 * math.pi / 6)
        r_bird = 155
        bx = cx + r_bird * math.cos(b_angle)
        by = cy + r_bird * math.sin(b_angle)
        draw.ellipse([bx - 12, by - 6, bx + 12, by + 6], fill=(255, 200, 50, 240))
        
    img.save(output_path, "PNG")
    print(f"Generated Icon Trong Dong: {output_path}")

def generate_icon_luu_dan(output_path):
    """Icon Lựu Đạn Thần Sa (Bình Gốm Chu Sa Lửa Rực)."""
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    
    # Hào quang lửa đỏ phía sau
    for r in range(220, 100, -5):
        alpha = int(120 * (1.0 - (r - 100) / 120.0))
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(255, 60, 0, alpha))
        
    # Bình gốm đỏ chu sa
    draw.ellipse([cx - 120, cy - 80, cx + 120, cy + 160], fill=(180, 25, 20, 255), outline=(255, 180, 50, 255), width=8)
    
    # Cổ bình và nắp phong ấn
    draw.rectangle([cx - 50, cy - 140, cx + 50, cy - 70], fill=(90, 20, 15, 255), outline=(255, 215, 0, 255), width=6)
    draw.polygon([(cx - 70, cy - 140), (cx + 70, cy - 140), (cx, cy - 190)], fill=(220, 40, 20, 255), outline=(255, 220, 80, 255), width=4)
    
    # Dây cháy phát sáng
    draw.line([(cx, cy - 190), (cx + 30, cy - 220)], fill=(255, 240, 100, 255), width=8)
    draw.ellipse([cx + 20, cy - 235, cx + 45, cy - 210], fill=(255, 255, 255, 255))
    
    img.save(output_path, "PNG")
    print(f"Generated Icon Luu Dan: {output_path}")

def generate_icon_nuoc_thanh(output_path):
    """Icon Nước Thánh Chùa Hương (Hồ Lô Ngọc Bích Linh Thủy)."""
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    
    # Hào quang xanh ngọc lam
    for r in range(220, 100, -5):
        alpha = int(140 * (1.0 - (r - 100) / 120.0))
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(0, 200, 255, alpha))
        
    # Thân dưới hồ lô
    draw.ellipse([cx - 130, cy - 40, cx + 130, cy + 180], fill=(15, 140, 170, 255), outline=(200, 255, 255, 255), width=8)
    # Thân trên hồ lô
    draw.ellipse([cx - 90, cy - 160, cx + 90, cy - 20], fill=(20, 170, 200, 255), outline=(200, 255, 255, 255), width=6)
    
    # Nơ đỏ thắt eo hồ lô
    draw.ellipse([cx - 60, cy - 45, cx + 60, cy - 15], fill=(220, 40, 50, 255), outline=(255, 220, 80, 255), width=4)
    
    # Giọt nước thánh phát sáng trên miệng bình
    draw.ellipse([cx - 25, cy - 210, cx + 25, cy - 160], fill=(240, 255, 255, 255))
    
    img.save(output_path, "PNG")
    print(f"Generated Icon Nuoc Thanh: {output_path}")

# -------------------------------------------------------------
# 2. BỘ SINH SPRITE/TEXTURE VFX RGBA 100% TRONG SUỐT
# -------------------------------------------------------------
def generate_dongson_shockwave_pattern(output_path):
    """Tạo Hoa Văn Trống Đồng Dập Sóng Âm (Shockwave Radial Mask)."""
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    
    # Các vòng hoa văn đồng tâm
    radii = [60, 100, 140, 180, 220]
    for r in radii:
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], outline=(255, 220, 80, 220), width=4)
        
    # Mặt trời trung tâm
    for i in range(14):
        angle = i * (2 * math.pi / 14)
        x_tip = cx + 80 * math.cos(angle)
        y_tip = cy + 80 * math.sin(angle)
        draw.line([(cx, cy), (x_tip, y_tip)], fill=(255, 240, 150, 240), width=3)
        
    img = img.filter(ImageFilter.GaussianBlur(radius=1.5))
    img.save(output_path, "PNG")
    print(f"Generated DongSon Shockwave Pattern: {output_path}")

def generate_fire_pillar_tornado(output_path):
    """Tạo Cột Lửa Cuộn Xoáy (Fire Pillar Tornado) cho Lựu Đạn Thần Sa."""
    w, h = 256, 512
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx = w // 2
    for y in range(h):
        t_y = y / float(h) # 0 (đỉnh) đến 1 (đáy)
        width_factor = math.sin(t_y * math.pi * 0.8 + 0.2)
        half_w = (w // 2 - 10) * width_factor
        
        # Cuộn xoáy sin
        offset_x = math.sin(t_y * 12.0) * 15.0
        
        alpha = int(255 * width_factor * (1.0 - (1.0 - t_y) ** 2))
        r_c = 255
        g_c = int(180 * (1.0 - t_y) + 40)
        b_c = int(40 * (1.0 - t_y))
        
        draw.line([(cx + offset_x - half_w, y), (cx + offset_x + half_w, y)], fill=(r_c, g_c, b_c, alpha), width=1)
        
        # Lõi trắng nóng
        core_w = half_w * 0.3
        draw.line([(cx + offset_x - core_w, y), (cx + offset_x + core_w, y)], fill=(255, 255, 220, alpha), width=1)
        
    img = img.filter(ImageFilter.GaussianBlur(radius=1.5))
    img.save(output_path, "PNG")
    print(f"Generated Fire Pillar Tornado: {output_path}")

def generate_holy_puddle_mist(output_path):
    """Tạo Bãi Sương Mù Nước Thánh (Gợn Sóng Tròn Mờ Dần)."""
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    
    # Các gợn sóng lan tỏa mờ dần
    for r in range(220, 20, -4):
        t = r / 220.0
        alpha = int(90 * math.sin(t * math.pi))
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(0, 220, 255, alpha))
        
    # Tâm phát sáng ngọc bích
    for r in range(80, 0, -2):
        alpha = int(140 * (1.0 - r / 80.0))
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(180, 255, 255, alpha))
        
    img = img.filter(ImageFilter.GaussianBlur(radius=3.0))
    img.save(output_path, "PNG")
    print(f"Generated Holy Puddle Mist: {output_path}")

def generate_holy_bubble_particle(output_path):
    """Tạo Hạt Bọt Khí Nước Thánh Linh Thiêng."""
    size = 128
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    
    r = 50
    # Viền bọt khí sáng
    draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(0, 180, 255, 60), outline=(200, 255, 255, 230), width=4)
    # Đốm sáng phản chiếu
    draw.ellipse([cx - 25, cy - 25, cx - 10, cy - 10], fill=(255, 255, 255, 240))
    
    img = img.filter(ImageFilter.GaussianBlur(radius=1.0))
    img.save(output_path, "PNG")
    print(f"Generated Holy Bubble: {output_path}")

if __name__ == "__main__":
    # Sinh 3 Icons Vũ Khí Cổ Phong
    generate_icon_trong_dong(os.path.join(ICONS_DIR, "Icon_W005_TrongDong.png"))
    generate_icon_luu_dan(os.path.join(ICONS_DIR, "Icon_W006_LuuDan.png"))
    generate_icon_nuoc_thanh(os.path.join(ICONS_DIR, "Icon_W011_NuocThanh.png"))
    
    # Sinh 4 VFX Textures RGBA trong suốt
    generate_dongson_shockwave_pattern(os.path.join(SKILLS_DIR, "DongSon_Shockwave_Pattern.png"))
    generate_fire_pillar_tornado(os.path.join(SKILLS_DIR, "Fire_Pillar_Tornado.png"))
    generate_holy_puddle_mist(os.path.join(SKILLS_DIR, "Holy_Puddle_Mist.png"))
    generate_holy_bubble_particle(os.path.join(SKILLS_DIR, "Holy_Bubble_Particle.png"))
