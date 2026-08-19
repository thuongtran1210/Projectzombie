import os
import math
from PIL import Image, ImageDraw, ImageFilter
import numpy as np

SKILLS_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Skills"
ICONS_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Weapons"
os.makedirs(SKILLS_DIR, exist_ok=True)
os.makedirs(ICONS_DIR, exist_ok=True)

# -------------------------------------------------------------
# 1. BỘ SINH VẬT PHẨM ICON CỔ PHONG 512x512 CHO ĐỢT 3
# -------------------------------------------------------------
def generate_icon_no_than(output_path):
    """Icon Nỏ Thần An Dương Vương (Kim Quy Hoàng Kim)."""
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    
    # Hào quang kim quang
    for r in range(220, 120, -5):
        alpha = int(120 * (1.0 - (r - 120) / 100.0))
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(255, 215, 0, alpha))
        
    # Cánh nỏ đồng mạ vàng
    draw.arc([cx - 180, cy - 80, cx + 180, cy + 180], start=190, end=350, fill=(212, 175, 55, 255), width=22)
    # Thân nỏ
    draw.rectangle([cx - 16, cy - 140, cx + 16, cy + 140], fill=(139, 69, 19, 255), outline=(255, 215, 0, 255), width=4)
    # Mũi tên ánh sáng trên rãnh nỏ
    draw.polygon([(cx, cy - 180), (cx - 18, cy - 120), (cx + 18, cy - 120)], fill=(255, 255, 255, 255))
    draw.line([(cx, cy - 120), (cx, cy + 80)], fill=(255, 240, 150, 255), width=8)
    
    img.save(output_path, "PNG")
    print(f"Generated Icon No Than: {output_path}")

def generate_icon_ho_trao(output_path):
    """Icon Cửu Vĩ Hồ Trảo (Vuốt Cáo Lửa Đỏ Rực)."""
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    
    # Hào quang lửa hồ ly
    for r in range(220, 110, -5):
        alpha = int(130 * (1.0 - (r - 110) / 110.0))
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(255, 30, 20, alpha))
        
    # 3 Vệt vuốt cáo sắc bén
    offsets = [-60, 0, 60]
    for dx in offsets:
        draw.polygon([
            (cx + dx - 12, cy + 120),
            (cx + dx + 12, cy + 120),
            (cx + dx * 1.3 + 20, cy - 140)
        ], fill=(255, 240, 200, 255))
        # Lõi đỏ rực
        draw.polygon([
            (cx + dx - 6, cy + 110),
            (cx + dx + 6, cy + 110),
            (cx + dx * 1.3 + 16, cy - 120)
        ], fill=(255, 50, 0, 255))
        
    img.save(output_path, "PNG")
    print(f"Generated Icon Ho Trao: {output_path}")

def generate_icon_cung_thach_sanh(output_path):
    """Icon Cung Thạch Sanh (Cánh Cung Gỗ Thần Ngọc Bích)."""
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    
    # Hào quang xanh ngọc bích
    for r in range(220, 120, -5):
        alpha = int(120 * (1.0 - (r - 120) / 100.0))
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(0, 255, 180, alpha))
        
    # Cánh cung cong
    draw.arc([cx - 160, cy - 180, cx + 160, cy + 180], start=100, end=260, fill=(34, 139, 34, 255), width=20)
    # Dây cung phát sáng
    draw.line([(cx - 40, cy - 170), (cx - 40, cy + 170)], fill=(200, 255, 240, 255), width=4)
    # Mũi tên ngọc
    draw.line([(cx - 80, cy), (cx + 140, cy)], fill=(0, 255, 200, 255), width=8)
    draw.polygon([(cx + 170, cy), (cx + 130, cy - 16), (cx + 130, cy + 16)], fill=(255, 255, 255, 255))
    
    img.save(output_path, "PNG")
    print(f"Generated Icon Cung Thach Sanh: {output_path}")

def generate_icon_truong_long_vuong(output_path):
    """Icon Trượng Long Vương (Đầu Rồng Lôi Thủy)."""
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    
    # Hào quang lôi điện tím xanh
    for r in range(220, 110, -5):
        alpha = int(130 * (1.0 - (r - 110) / 110.0))
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(0, 150, 255, alpha))
        
    # Thân trượng ngọc rồng
    draw.line([(cx, cy - 140), (cx, cy + 190)], fill=(20, 80, 160, 255), width=18)
    # Ngọc rồng phát sáng trên đỉnh trượng
    draw.ellipse([cx - 55, cy - 185, cx + 55, cy - 75], fill=(0, 230, 255, 255), outline=(255, 255, 255, 255), width=6)
    draw.ellipse([cx - 30, cy - 160, cx + 30, cy - 100], fill=(255, 255, 255, 255))
    
    img.save(output_path, "PNG")
    print(f"Generated Icon Truong Long Vuong: {output_path}")

def generate_icon_linh_phu_ma_da(output_path):
    """Icon Linh Phù Ma Da (Lá Bùa Âm Ty Xanh Tím)."""
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    
    # Hào quang ma quái xanh tím
    for r in range(220, 110, -5):
        alpha = int(130 * (1.0 - (r - 110) / 110.0))
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(138, 43, 226, alpha))
        
    # Lá bùa chữ nhật
    draw.rectangle([cx - 85, cy - 160, cx + 85, cy + 160], fill=(40, 20, 60, 255), outline=(186, 85, 211, 255), width=8)
    # Chu sa phong ấn phát sáng
    draw.line([(cx, cy - 120), (cx, cy + 120)], fill=(0, 255, 200, 255), width=10)
    draw.ellipse([cx - 35, cy - 60, cx + 35, cy + 10], outline=(0, 255, 200, 255), width=6)
    
    img.save(output_path, "PNG")
    print(f"Generated Icon Linh Phu Ma Da: {output_path}")

# -------------------------------------------------------------
# 2. BỘ SINH SPRITE/TEXTURE VFX RGBA 100% TRONG SUỐT CHO ĐỢT 3
# -------------------------------------------------------------
def generate_arrow_golden_beam(output_path):
    """Tia Sáng Mũi Tên Vuốt Nhọn (Arrow Golden Beam) cho W001 & W007."""
    w, h = 512, 64
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cy = h // 2
    
    for x in range(w):
        t = x / float(w) # 0 (đuôi) đến 1 (đầu)
        thickness = (t ** 1.5)
        half_h = (h // 2 - 2) * thickness
        alpha = int(255 * (t ** 0.5))
        
        # Thân tia sáng vàng kim
        draw.line([(x, cy - half_h), (x, cy + half_h)], fill=(255, 215, 0, alpha), width=1)
        # Lõi trắng cực sáng
        core_h = half_h * 0.4
        draw.line([(x, cy - core_h), (x, cy + core_h)], fill=(255, 255, 255, alpha), width=1)
        
    img = img.filter(ImageFilter.GaussianBlur(radius=1.0))
    img.save(output_path, "PNG")
    print(f"Generated Arrow Golden Beam: {output_path}")

def generate_wind_pierce_ring(output_path):
    """Vòng Xé Gió Bung Đầu Mũi Tên (Wind Pierce Ring)."""
    size = 256
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    
    r_outer = 110
    r_inner = 70
    for r in range(r_inner, r_outer):
        t = (r - r_inner) / float(r_outer - r_inner)
        alpha = int(255 * math.sin(t * math.pi))
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], outline=(255, 255, 255, alpha), width=2)
        
    img = img.filter(ImageFilter.GaussianBlur(radius=1.5))
    img.save(output_path, "PNG")
    print(f"Generated Wind Pierce Ring: {output_path}")

def generate_fox_claws_slash(output_path):
    """3 Vệt Vuốt Cáo Cào Xé Bán Nguyệt (Fox Claws Slash)."""
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    
    radii = [130, 170, 210]
    for r in radii:
        for angle_deg in range(30, 150):
            rad = math.radians(angle_deg)
            t = (angle_deg - 30) / 120.0
            thick = math.sin(t * math.pi) * 12.0
            
            x = cx + r * math.cos(rad)
            y = cy + r * math.sin(rad)
            
            alpha = int(255 * math.sin(t * math.pi))
            draw.ellipse([x - thick/2, y - thick/2, x + thick/2, y + thick/2], fill=(255, 40, 20, alpha))
            draw.ellipse([x - thick/4, y - thick/4, x + thick/4, y + thick/4], fill=(255, 255, 200, alpha))
            
    img = img.filter(ImageFilter.GaussianBlur(radius=1.5))
    img.save(output_path, "PNG")
    print(f"Generated Fox Claws Slash: {output_path}")

def generate_soul_drain_orb(output_path):
    """Hạt Sinh Khí Xanh Ngọc Hút Máu (Soul Drain Orb)."""
    size = 128
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    
    for r in range(55, 0, -2):
        alpha = int(255 * (1.0 - r / 55.0))
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(0, 255, 180, alpha))
        
    draw.ellipse([cx - 15, cy - 15, cx + 15, cy + 15], fill=(240, 255, 255, 255))
    img = img.filter(ImageFilter.GaussianBlur(radius=1.5))
    img.save(output_path, "PNG")
    print(f"Generated Soul Drain Orb: {output_path}")

def generate_lightning_bolt_segment(output_path):
    """Vệt Sét Nước Lôi Thủy (Lightning Bolt Segment) cho W009."""
    w, h = 512, 64
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cy = h // 2
    
    # Tia sét gãy khúc
    np.random.seed(99)
    curr_x, curr_y = 0, cy
    while curr_x < w:
        step_x = np.random.randint(20, 50)
        next_x = min(w, curr_x + step_x)
        next_y = cy + np.random.randint(-22, 22)
        
        draw.line([(curr_x, curr_y), (next_x, next_y)], fill=(0, 180, 255, 200), width=10)
        draw.line([(curr_x, curr_y), (next_x, next_y)], fill=(220, 255, 255, 255), width=4)
        curr_x, curr_y = next_x, next_y
        
    img = img.filter(ImageFilter.GaussianBlur(radius=1.2))
    img.save(output_path, "PNG")
    print(f"Generated Lightning Bolt Segment: {output_path}")

def generate_poison_swamp_mist(output_path):
    """Đầm Lầy Khói Độc Âm Ty Xanh Tím (Poison Swamp Mist) cho W010."""
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    
    for r in range(220, 20, -5):
        t = r / 220.0
        alpha = int(100 * math.sin(t * math.pi))
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(120, 30, 200, alpha))
        
    for r in range(120, 0, -3):
        alpha = int(140 * (1.0 - r / 120.0))
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(0, 230, 160, alpha))
        
    img = img.filter(ImageFilter.GaussianBlur(radius=3.5))
    img.save(output_path, "PNG")
    print(f"Generated Poison Swamp Mist: {output_path}")

if __name__ == "__main__":
    # Sinh 5 Icons Cổ Phong Đợt 3
    generate_icon_no_than(os.path.join(ICONS_DIR, "Icon_W001_NoThan.png"))
    generate_icon_ho_trao(os.path.join(ICONS_DIR, "Icon_W004_HoTrao.png"))
    generate_icon_cung_thach_sanh(os.path.join(ICONS_DIR, "Icon_W007_CungThachSanh.png"))
    generate_icon_truong_long_vuong(os.path.join(ICONS_DIR, "Icon_W009_TruongLongVuong.png"))
    generate_icon_linh_phu_ma_da(os.path.join(ICONS_DIR, "Icon_W010_LinhPhuMaDa.png"))
    
    # Sinh 6 VFX Textures RGBA trong suốt Đợt 3
    generate_arrow_golden_beam(os.path.join(SKILLS_DIR, "Arrow_Golden_Beam.png"))
    generate_wind_pierce_ring(os.path.join(SKILLS_DIR, "Wind_Pierce_Ring.png"))
    generate_fox_claws_slash(os.path.join(SKILLS_DIR, "Fox_Claws_Slash.png"))
    generate_soul_drain_orb(os.path.join(SKILLS_DIR, "Soul_Drain_Orb.png"))
    generate_lightning_bolt_segment(os.path.join(SKILLS_DIR, "Lightning_Bolt_Segment.png"))
    generate_poison_swamp_mist(os.path.join(SKILLS_DIR, "Poison_Swamp_Mist.png"))
