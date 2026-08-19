import os
import math
from PIL import Image, ImageDraw, ImageFilter
import numpy as np

OUTPUT_PATHS = [
    r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Skills\Pro_InkSlash_Arc.png",
    r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Skills\InkSlash_Arc.png"
]

def generate_directional_slash_arc():
    """
    Tạo vệt chém hình lưỡi liềm (Slash Arc) chuẩn hướng +X (Bên phải / Phía trước).
    Góc quét đối xứng từ -65 độ đến +65 độ hướng thẳng sang phải.
    100% Alpha Transparent, phong cách Thư Pháp Cổ Phong (Anime Calligraphy).
    """
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2

    # Bán kính vòng cung
    r_outer = 220
    r_inner = 130

    # 1. Vẽ quầng sáng hào quang (Golden Glow Aura)
    glow = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    gdraw = ImageDraw.Draw(glow)
    
    # Quét góc từ -65 đến +65 độ (hướng sang +X)
    steps = 180
    for i in range(steps):
        t = i / float(steps) # 0 to 1
        angle_deg = -65 + t * 130 # -65 to +65 deg
        rad = math.radians(angle_deg)
        
        # Độ dày đạt cực đại ở chính giữa (0 độ)
        thickness_factor = math.sin(t * math.pi) # 0 at tips, 1 at center
        if thickness_factor <= 0: continue
        
        r_mid = (r_outer + r_inner) / 2
        span = (r_outer - r_inner) * thickness_factor * 0.7
        
        r1 = r_mid - span - 15
        r2 = r_mid + span + 15
        
        x1 = cx + r1 * math.cos(rad)
        y1 = cy + r1 * math.sin(rad)
        x2 = cx + r2 * math.cos(rad)
        y2 = cy + r2 * math.sin(rad)
        
        alpha = int(200 * (thickness_factor ** 0.5))
        gdraw.line([(x1, y1), (x2, y2)], fill=(255, 180, 20, alpha), width=5)

    glow = glow.filter(ImageFilter.GaussianBlur(radius=8))
    img.alpha_composite(glow)

    # 2. Vẽ Lưỡi Liềm Mực Thư Pháp Cốt Lõi (Sharp Calligraphy Crescent Blade)
    core = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    cdraw = ImageDraw.Draw(core)

    outer_pts = []
    inner_pts = []

    for i in range(steps + 1):
        t = i / float(steps)
        angle_deg = -65 + t * 130
        rad = math.radians(angle_deg)
        
        # Đường cong lưỡi liềm dày ở tâm, vuốt cực nhọn ở 2 đầu
        factor = math.sin(t * math.pi) ** 0.8
        
        curr_r_outer = r_outer + 10 * math.sin(t * math.pi * 2) # gợn sóng nhẹ
        curr_r_inner = r_outer - (r_outer - r_inner) * factor
        
        ox = cx + curr_r_outer * math.cos(rad)
        oy = cy + curr_r_outer * math.sin(rad)
        ix = cx + curr_r_inner * math.cos(rad)
        iy = cy + curr_r_inner * math.sin(rad)
        
        outer_pts.append((ox, oy))
        inner_pts.append((ix, iy))

    polygon_pts = outer_pts + list(reversed(inner_pts))
    cdraw.polygon(polygon_pts, fill=(255, 230, 100, 255), outline=(255, 150, 0, 255))

    # 3. Lõi Sáng Trắng Cực Quang Cắt Dọc Lưỡi Chém
    white_outer = []
    white_inner = []
    for i in range(steps + 1):
        t = i / float(steps)
        angle_deg = -55 + t * 110
        rad = math.radians(angle_deg)
        
        factor = math.sin(t * math.pi) ** 1.2
        if factor <= 0.05: continue
        
        mid_r = r_outer - 15
        span_w = 20 * factor
        
        ox = cx + (mid_r + span_w) * math.cos(rad)
        oy = cy + (mid_r + span_w) * math.sin(rad)
        ix = cx + (mid_r - span_w) * math.cos(rad)
        iy = cy + (mid_r - span_w) * math.sin(rad)
        
        white_outer.append((ox, oy))
        white_inner.append((ix, iy))

    if white_outer and white_inner:
        w_poly = white_outer + list(reversed(white_inner))
        cdraw.polygon(w_poly, fill=(255, 255, 255, 255))

    # Gợn vuốt bút lông thư pháp (Ink Trails at Tips)
    tip1 = outer_pts[0]
    tip2 = outer_pts[-1]
    cdraw.line([(tip1[0], tip1[1]), (tip1[0] - 30, tip1[1] + 15)], fill=(255, 200, 50, 220), width=4)
    cdraw.line([(tip2[0], tip2[1]), (tip2[0] - 30, tip2[1] - 15)], fill=(255, 200, 50, 220), width=4)

    core = core.filter(ImageFilter.GaussianBlur(radius=0.8))
    img.alpha_composite(core)

    # Lưu file
    for p in OUTPUT_PATHS:
        os.makedirs(os.path.dirname(p), exist_ok=True)
        img.save(p, "PNG")
        print(f"Generated Directional Slash Arc (+X Facing): {p}")

if __name__ == "__main__":
    generate_directional_slash_arc()
