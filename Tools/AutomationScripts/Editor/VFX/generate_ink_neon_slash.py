import os
import math
from PIL import Image, ImageDraw, ImageFilter
import numpy as np

SKILLS_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Skills"
os.makedirs(SKILLS_DIR, exist_ok=True)

def generate_black_ink_brush_arc(output_path):
    """
    Tạo vệt cọ mực tàu đen (Black Calligraphy Ink Arc) với gân xước bút lông Enso.
    100% Alpha Transparent, màu đen mực đậm (Charcoal Black).
    """
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2

    r_outer = 220
    r_inner = 135
    steps = 220

    # Vẽ nhiều sợi lông cọ xước (Bristle Striae)
    for stroke_idx in range(12):
        r_offset = (stroke_idx - 6) * 7
        outer_pts = []
        inner_pts = []
        
        # Góc quét từ -70 đến +70 độ
        for i in range(steps + 1):
            t = i / float(steps)
            angle_deg = -70 + t * 140
            rad = math.radians(angle_deg)
            
            # Gợn ngẫu nhiên tạo chất xước mực tàu
            noise = math.sin(t * 25 + stroke_idx * 3) * 4 + math.cos(t * 40) * 3
            thickness = math.sin(t * math.pi) ** 0.85
            if thickness <= 0.02: continue
            
            mid_r = (r_outer + r_inner) / 2 + r_offset + noise
            span = (r_outer - r_inner) * 0.45 * thickness
            
            ox = cx + (mid_r + span) * math.cos(rad)
            oy = cy + (mid_r + span) * math.sin(rad)
            ix = cx + (mid_r - span) * math.cos(rad)
            iy = cy + (mid_r - span) * math.sin(rad)
            
            outer_pts.append((ox, oy))
            inner_pts.append((ix, iy))
            
        if len(outer_pts) > 3:
            poly = outer_pts + list(reversed(inner_pts))
            alpha = int(230 * (0.8 + 0.2 * math.sin(stroke_idx)))
            draw.polygon(poly, fill=(12, 12, 18, alpha))

    # Thêm các vệt mực bắn li ti ở viền cọ (Ink Splatters)
    for s in range(30):
        t = s / 30.0
        angle_deg = -65 + t * 130 + (math.sin(s * 8) * 6)
        rad = math.radians(angle_deg)
        dist = r_outer + 8 + math.sin(s * 15) * 15
        sx = cx + dist * math.cos(rad)
        sy = cy + dist * math.sin(rad)
        sr = 2 + (s % 3)
        draw.ellipse([sx - sr, sy - sr, sx + sr, sy + sr], fill=(15, 15, 22, 220))

    img = img.filter(ImageFilter.GaussianBlur(radius=0.7))
    img.save(output_path, "PNG")
    print(f"Generated Black Ink Brush Arc: {output_path}")

def generate_neon_blade_glow_arc(output_path):
    """
    Tạo dải quang năng Neon rực rỡ (Cyan -> Emerald Green -> Hot Pink/Magenta) với lõi trắng cực sáng.
    """
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    cx, cy = size // 2, size // 2

    r_mid = 195
    steps = 180

    # 1. Hào quang tỏa màu Neon Bloom
    glow = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    gdraw = ImageDraw.Draw(glow)

    for i in range(steps):
        t = i / float(steps) # 0 (đuôi) to 1 (đầu)
        angle_deg = -55 + t * 110
        rad = math.radians(angle_deg)
        
        factor = math.sin(t * math.pi) ** 0.9
        if factor <= 0.02: continue

        # Gradient màu Anime: Đuôi hồng tím -> Giữa Cyan -> Đầu Xanh Ngọc Lục Bảo
        if t < 0.35:
            # Hot Pink / Magenta (255, 40, 180)
            r, g, b = 255, 40, 180
        elif t < 0.7:
            # Electric Cyan (0, 220, 255)
            r, g, b = 0, 220, 255
        else:
            # Bright Emerald / Mint Green (0, 255, 160)
            r, g, b = 0, 255, 160

        span = 32 * factor
        x1 = cx + (r_mid - span) * math.cos(rad)
        y1 = cy + (r_mid - span) * math.sin(rad)
        x2 = cx + (r_mid + span) * math.cos(rad)
        y2 = cy + (r_mid + span) * math.sin(rad)

        alpha = int(220 * (factor ** 0.5))
        gdraw.line([(x1, y1), (x2, y2)], fill=(r, g, b, alpha), width=6)

    glow = glow.filter(ImageFilter.GaussianBlur(radius=7))
    img.alpha_composite(glow)

    # 2. Lõi Sáng Trắng Cực Nét (Sharp White Laser Core)
    core = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    cdraw = ImageDraw.Draw(core)

    core_outer = []
    core_inner = []
    for i in range(steps + 1):
        t = i / float(steps)
        angle_deg = -50 + t * 100
        rad = math.radians(angle_deg)
        
        factor = math.sin(t * math.pi) ** 1.3
        if factor <= 0.05: continue
        
        span = 8 * factor
        ox = cx + (r_mid + span) * math.cos(rad)
        oy = cy + (r_mid + span) * math.sin(rad)
        ix = cx + (r_mid - span) * math.cos(rad)
        iy = cy + (r_mid - span) * math.sin(rad)
        
        core_outer.append((ox, oy))
        core_inner.append((ix, iy))

    if core_outer and core_inner:
        poly = core_outer + list(reversed(core_inner))
        cdraw.polygon(poly, fill=(255, 255, 255, 255))

    core = core.filter(ImageFilter.GaussianBlur(radius=0.8))
    img.alpha_composite(core)

    img.save(output_path, "PNG")
    print(f"Generated Neon Blade Glow Arc: {output_path}")

if __name__ == "__main__":
    generate_black_ink_brush_arc(os.path.join(SKILLS_DIR, "Ink_Black_Brush_Arc.png"))
    generate_neon_blade_glow_arc(os.path.join(SKILLS_DIR, "Neon_Blade_Glow_Arc.png"))
