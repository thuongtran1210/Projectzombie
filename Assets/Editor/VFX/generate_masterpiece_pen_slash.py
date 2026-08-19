import os
import math
from PIL import Image, ImageDraw, ImageFilter
import numpy as np

SKILLS_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Skills"
os.makedirs(SKILLS_DIR, exist_ok=True)

def generate_calligraphy_ink_sweep():
    """
    Tạo vệt cọ mực tàu Enso phong cách Thư Pháp Cổ Phong đỉnh cao (1024x1024, 100% Alpha Transparent).
    Nét cọ đậm chất họa thủy mặc, có độ dày uyển chuyển và gân xước tự nhiên.
    """
    size = 1024
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2

    r_outer = 440
    r_inner = 260
    steps = 300

    # 1. Thân cọ mực chính (Nhiều lớp sợi cọ đan xen)
    for strand in range(25):
        outer_pts = []
        inner_pts = []
        r_offset = (strand - 12) * 6.5
        strand_alpha = int(240 * (0.7 + 0.3 * math.cos(strand * 0.4)))
        
        # Góc quét từ -70 đến +70 độ (hướng sang phải +X)
        for i in range(steps + 1):
            t = i / float(steps)
            angle_deg = -72 + t * 144
            rad = math.radians(angle_deg)
            
            # Gợn sóng mô phỏng áp lực tay ấn cọ thư pháp
            pressure = math.sin(t * math.pi) ** 0.75
            if pressure <= 0.01: continue
            
            noise = math.sin(t * 30 + strand * 2) * 6 + math.cos(t * 50) * 4
            
            mid_r = (r_outer + r_inner) / 2 + r_offset + noise
            span = (r_outer - r_inner) * 0.48 * pressure
            
            ox = cx + (mid_r + span) * math.cos(rad)
            oy = cy + (mid_r + span) * math.sin(rad)
            ix = cx + (mid_r - span) * math.cos(rad)
            iy = cy + (mid_r - span) * math.sin(rad)
            
            outer_pts.append((ox, oy))
            inner_pts.append((ix, iy))
            
        if len(outer_pts) > 5:
            poly = outer_pts + list(reversed(inner_pts))
            draw.polygon(poly, fill=(8, 8, 14, strand_alpha))

    # 2. Các vệt mực xước đuôi (Dry Brush Splatters & Bristle Streaks)
    for s in range(50):
        t = s / 50.0
        angle_deg = -68 + t * 136 + math.sin(s * 7) * 5
        rad = math.radians(angle_deg)
        dist = r_outer + 12 + math.sin(s * 11) * 20
        sx = cx + dist * math.cos(rad)
        sy = cy + dist * math.sin(rad)
        sr = 2 + (s % 4)
        draw.ellipse([sx - sr, sy - sr, sx + sr, sy + sr], fill=(10, 10, 16, 220))

    img = img.filter(ImageFilter.GaussianBlur(radius=1.0))
    p1 = os.path.join(SKILLS_DIR, "Ink_Black_Brush_Arc.png")
    p2 = os.path.join(SKILLS_DIR, "Pro_InkSlash_Arc.png")
    img.save(p1, "PNG")
    img.save(p2, "PNG")
    print(f"Generated Masterpiece Ink Brush Arc: {p1}")

def generate_neon_laser_blade():
    """
    Tạo lưỡi kiếm quang năng Neon sắc lẹm (1024x1024, 100% Alpha Transparent).
    Gồm quầng hào quang Cyan -> Emerald -> Pink và lõi Laser trắng sắc bén.
    """
    size = 1024
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    cx, cy = size // 2, size // 2
    r_blade = 390
    steps = 260

    # 1. Quầng sáng Neon Bloom tỏa rộng
    glow = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    gdraw = ImageDraw.Draw(glow)

    for i in range(steps):
        t = i / float(steps)
        angle_deg = -62 + t * 124
        rad = math.radians(angle_deg)
        
        factor = math.sin(t * math.pi) ** 0.85
        if factor <= 0.02: continue

        # Gradient màu sắc Cyberpunk / Anime:
        # Đuôi hồng tím -> Giữa Cyan điện tích -> Đầu Xanh ngọc lục bảo
        if t < 0.32:
            r, g, b = 255, 45, 170  # Neon Pink / Magenta
        elif t < 0.68:
            r, g, b = 0, 225, 255   # Electric Cyan
        else:
            r, g, b = 0, 255, 170   # Emerald Mint Green

        span = 55 * factor
        x1 = cx + (r_blade - span) * math.cos(rad)
        y1 = cy + (r_blade - span) * math.sin(rad)
        x2 = cx + (r_blade + span) * math.cos(rad)
        y2 = cy + (r_blade + span) * math.sin(rad)

        alpha = int(230 * (factor ** 0.5))
        gdraw.line([(x1, y1), (x2, y2)], fill=(r, g, b, alpha), width=10)

    glow = glow.filter(ImageFilter.GaussianBlur(radius=12))
    img.alpha_composite(glow)

    # 2. Lưỡi Cắt Laser Trắng Tinh Khiết (Razor Sharp White Core)
    core = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    cdraw = ImageDraw.Draw(core)

    core_outer = []
    core_inner = []
    for i in range(steps + 1):
        t = i / float(steps)
        angle_deg = -58 + t * 116
        rad = math.radians(angle_deg)
        
        factor = math.sin(t * math.pi) ** 1.1
        if factor <= 0.03: continue
        
        span = 14 * factor
        ox = cx + (r_blade + span) * math.cos(rad)
        oy = cy + (r_blade + span) * math.sin(rad)
        ix = cx + (r_blade - span) * math.cos(rad)
        iy = cy + (r_blade - span) * math.sin(rad)
        
        core_outer.append((ox, oy))
        core_inner.append((ix, iy))

    if core_outer and core_inner:
        poly = core_outer + list(reversed(core_inner))
        cdraw.polygon(poly, fill=(255, 255, 255, 255))

    core = core.filter(ImageFilter.GaussianBlur(radius=1.2))
    img.alpha_composite(core)

    p = os.path.join(SKILLS_DIR, "Neon_Blade_Glow_Arc.png")
    img.save(p, "PNG")
    print(f"Generated Neon Laser Blade Arc: {p}")

def generate_sharp_laser_sparks():
    """
    Tạo Texture tia lửa sắc như kim (Sharp Needle Sparks).
    Triệt tiêu hoàn toàn hiện tượng vảy xoắn / bắp ngô khi kéo dãn Particle Stretch.
    """
    w, h = 256, 32
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cy = h // 2

    # Vẽ vệt tia sáng nhọn 2 đầu từ trái sang phải
    for x in range(w):
        t = x / float(w) # 0 to 1
        factor = math.sin(t * math.pi) ** 1.5
        half_h = (h // 2 - 2) * factor
        alpha = int(255 * (factor ** 0.5))
        
        # Lõi vàng kim
        draw.line([(x, cy - half_h), (x, cy + half_h)], fill=(255, 200, 40, alpha), width=1)
        # Lõi sáng trắng
        core_h = half_h * 0.4
        draw.line([(x, cy - core_h), (x, cy + core_h)], fill=(255, 255, 255, alpha), width=1)

    img = img.filter(ImageFilter.GaussianBlur(radius=0.8))
    p = os.path.join(SKILLS_DIR, "Spark_Streak.png")
    img.save(p, "PNG")
    print(f"Generated Sharp Laser Sparks: {p}")

if __name__ == "__main__":
    generate_calligraphy_ink_sweep()
    generate_neon_laser_blade()
    generate_sharp_laser_sparks()
