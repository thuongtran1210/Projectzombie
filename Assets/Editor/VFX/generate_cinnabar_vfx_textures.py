import os
import math
from PIL import Image, ImageDraw, ImageFilter
import numpy as np

ART_VFX_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\VFX"
SKILL_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Skills"
os.makedirs(ART_VFX_DIR, exist_ok=True)
os.makedirs(SKILL_DIR, exist_ok=True)

def generate_cinnabar_fireball_burst():
    """
    1. Quả Cầu Bão Lửa Thần Sa (Cinnabar Fireball Explosion Burst) 512x512:
    - Cấu trúc đa thùy (Multi-lobed anime explosion billows).
    - Lõi nổ vàng trắng cực nhiệt (White-hot core #FFFDE0).
    - Thân lửa đổi sắc đỏ chu sa / khoáng hỏa rực rỡ (#FF3B00, #E60026).
    - Viền ngoài có gợn lửa xé rách (Fractured flame tendrils) và khói khoáng đỏ thẫm.
    """
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size / 2.0, size / 2.0

    lobes = [
        (0, 0, 160, 1.0),
        (-60, -45, 125, 0.95),
        (65, -40, 130, 0.95),
        (-70, 50, 120, 0.9),
        (65, 55, 125, 0.9),
        (0, -95, 110, 0.85),
        (0, 90, 115, 0.85),
        (-95, 0, 110, 0.85),
        (95, 0, 110, 0.85),
        (-50, -100, 85, 0.75),
        (55, -95, 85, 0.75),
        (-55, 95, 85, 0.75),
        (50, 100, 85, 0.75),
    ]

    # 1. Lớp viền ngoài: Khói khoáng Thần Sa đỏ sẫm
    for lx, ly, lr, _ in lobes:
        px, py = cx + lx, cy + ly
        r_outer = int(lr * 1.35)
        for r in range(r_outer, 20, -5):
            t = 1.0 - (r / float(r_outer))
            alpha = int(120 * (t ** 1.6))
            draw.ellipse([(px - r, py - r), (px + r, py + r)], fill=(120, 15, 10, alpha))

    # 2. Lớp thân lửa: Đỏ Chu Sa Hỏa Linh (#FF2200 -> #FF5500)
    for lx, ly, lr, w in lobes:
        px, py = cx + lx, cy + ly
        for r in range(lr, 15, -4):
            t = 1.0 - (r / float(lr))
            alpha = int(240 * (t ** 1.2) * w)
            red = 255
            green = int(50 + 130 * t)
            blue = int(10 + 30 * t)
            draw.ellipse([(px - r, py - r), (px + r, py + r)], fill=(red, green, blue, alpha))

    # 3. Lớp lõi lửa nhiệt độ cao: Vàng Cam Hoàng Kim (#FFD000)
    for lx, ly, lr, _ in lobes:
        px, py = cx + lx * 0.6, cy + ly * 0.6
        r_core = int(lr * 0.65)
        for r in range(r_core, 10, -3):
            t = 1.0 - (r / float(r_core))
            alpha = int(255 * (t ** 1.1))
            red = 255
            green = int(190 + 55 * t)
            blue = int(40 + 140 * t)
            draw.ellipse([(px - r, py - r), (px + r, py + r)], fill=(red, green, blue, alpha))

    # 4. Lõi trung tâm nổ cực nhiệt (White-hot epicenter)
    for r in range(75, 0, -2):
        t = 1.0 - (r / 75.0)
        alpha = int(255 * (t ** 0.9))
        red = 255
        green = int(240 + 15 * t)
        blue = int(180 + 75 * t)
        draw.ellipse([(cx - r, cy - r), (cx + r, cy + r)], fill=(red, green, blue, alpha))

    # 5. Thêm các tia lửa xé rách đa hướng
    num_spikes = 16
    for i in range(num_spikes):
        ang = (i / float(num_spikes)) * 2 * math.pi + (0.1 if i % 2 == 0 else -0.1)
        length = 190 + (35 if i % 2 == 0 else 15)
        sx = cx + length * math.cos(ang)
        sy = cy + length * math.sin(ang)
        ang_perp = ang + math.pi / 2
        w = 12
        p1 = (cx + (length - 40) * math.cos(ang) + w * math.cos(ang_perp), cy + (length - 40) * math.sin(ang) + w * math.sin(ang_perp))
        p2 = (cx + (length - 40) * math.cos(ang) - w * math.cos(ang_perp), cy + (length - 40) * math.sin(ang) - w * math.sin(ang_perp))
        draw.polygon([(sx, sy), p1, p2], fill=(255, 120, 20, 180))

    img = img.filter(ImageFilter.GaussianBlur(radius=1.2))
    out_path = os.path.join(ART_VFX_DIR, "Tex_VFX_Cinnabar_Fireball_Burst.png")
    img.save(out_path, "PNG")
    print(f"[VFX Texture] Generated: {out_path}")

def generate_cinnabar_shockwave_ring():
    """
    2. Vành Sóng Xung Kích Thần Sa (Cinnabar Shockwave Blast Ring) 512x512:
    - Định hình chính xác biên độ nổ 3.5m.
    - Mép ngoài nén khí siêu thanh màu trắng - vàng rực rỡ, dốc dứng.
    - Thân vành chứa 16 răng cưa hỏa linh và gợn sóng phụ đồng tâm màu Đỏ Chu Sa (#FF2800).
    """
    size = 512
    arr = np.zeros((size, size, 4), dtype=np.float32)
    cx, cy = size / 2.0, size / 2.0

    y_coords, x_coords = np.ogrid[:size, :size]
    dist_from_center = np.sqrt((x_coords - cx) ** 2 + (y_coords - cy) ** 2)
    angle = np.arctan2(y_coords - cy, x_coords - cx)

    r_max = 246.0
    r_thickness = 65.0
    inward_dist = r_max - dist_from_center

    sawtooth = 0.85 + 0.15 * np.cos(16 * angle)
    
    wave1 = np.exp(-((inward_dist - 4.0) ** 2) / (2.0 * (4.5 ** 2))) * 1.0
    wave2 = np.exp(-((inward_dist - 22.0) ** 2) / (2.0 * (6.0 ** 2))) * 0.75 * sawtooth
    wave3 = np.exp(-((inward_dist - 42.0) ** 2) / (2.0 * (7.5 ** 2))) * 0.45

    total_profile = (wave1 + wave2 + wave3) * (inward_dist >= 0) * (inward_dist <= r_thickness)
    total_profile = np.clip(total_profile, 0.0, 1.0)

    for y in range(size):
        for x in range(size):
            d = inward_dist[y, x]
            if 0 <= d <= r_thickness:
                val = total_profile[y, x]
                if val <= 0.001:
                    continue
                if d <= 6.0:
                    t = d / 6.0
                    r = 255
                    g = int(255 * (1.0 - t) + 210 * t)
                    b = int(230 * (1.0 - t) + 50 * t)
                    a = int(255 * val)
                elif d <= 26.0:
                    t = (d - 6.0) / 20.0
                    r = 255
                    g = int(210 * (1.0 - t) + 60 * t)
                    b = int(50 * (1.0 - t) + 10 * t)
                    a = int(255 * val)
                else:
                    t = (d - 26.0) / (r_thickness - 26.0)
                    r = int(255 * (1.0 - t) + 180 * t)
                    g = int(60 * (1.0 - t) + 20 * t)
                    b = int(10 * (1.0 - t) + 5 * t)
                    a = int(255 * val * (1.0 - t * 0.8))
                arr[y, x] = [r, g, b, a]

    out_img = Image.fromarray(arr.astype(np.uint8), "RGBA").filter(ImageFilter.GaussianBlur(radius=0.5))
    out_path = os.path.join(ART_VFX_DIR, "Tex_VFX_Cinnabar_Shockwave_Ring.png")
    out_img.save(out_path, "PNG")
    print(f"[VFX Texture] Generated: {out_path}")

def generate_cinnabar_magic_array():
    """
    3. Trận Đồ Chu Sa Khắc Đất (Cinnabar Alchemical Ground Array / Phù Trận Hỏa Linh) 512x512:
    - Họa tiết Trận đồ Hỏa Luyện Đan Đông Chu Sa:
      + Vòng tròn bát quái ngoại vi viền vàng kim - đỏ chu sa.
      + 8 cung Phù Hỏa Chu Sa đối xứng.
      + Tam giác tam muội chân hỏa / hoa văn sao 8 cánh ở tâm.
    """
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size / 2.0, size / 2.0

    # 1. Vòng tròn ngoài cùng
    draw.ellipse([(cx - 235, cy - 235), (cx + 235, cy + 235)], outline=(255, 80, 20, 240), width=5)
    draw.ellipse([(cx - 220, cy - 220), (cx + 220, cy + 220)], outline=(255, 200, 50, 220), width=3)
    draw.ellipse([(cx - 205, cy - 205), (cx + 205, cy + 205)], outline=(255, 50, 10, 180), width=2)

    # 2. 8 nốt phù văn Chu Sa tại 8 hướng
    r_nodes = 180
    for i in range(8):
        ang = i * (math.pi / 4.0)
        nx = cx + r_nodes * math.cos(ang)
        ny = cy + r_nodes * math.sin(ang)
        draw.ellipse([(nx - 18, ny - 18), (nx + 18, ny + 18)], fill=(255, 40, 20, 220), outline=(255, 230, 100, 255), width=2)
        draw.ellipse([(nx - 7, ny - 7), (nx + 7, ny + 7)], fill=(255, 255, 220, 255))
        
        next_ang = (i + 1) * (math.pi / 4.0)
        nnx = cx + r_nodes * math.cos(next_ang)
        nny = cy + r_nodes * math.sin(next_ang)
        draw.line([(nx, ny), (nnx, nny)], fill=(255, 160, 40, 190), width=2)

    # 3. Vòng tròn nội giới
    draw.ellipse([(cx - 120, cy - 120), (cx + 120, cy + 120)], outline=(255, 220, 80, 230), width=4)
    draw.ellipse([(cx - 105, cy - 105), (cx + 105, cy + 105)], outline=(255, 50, 10, 160), width=2)

    # 4. Sao 8 cánh Hỏa Phù nối từ tâm
    for i in range(8):
        a1 = i * (math.pi / 4.0)
        a2 = (i + 1) * (math.pi / 4.0)
        x_outer = cx + 105 * math.cos(a1)
        y_outer = cy + 105 * math.sin(a1)
        x_inner = cx + 45 * math.cos((a1 + a2) / 2.0)
        y_inner = cy + 45 * math.sin((a1 + a2) / 2.0)
        draw.line([(cx, cy), (x_outer, y_outer)], fill=(255, 80, 20, 220), width=3)
        draw.line([(x_outer, y_outer), (x_inner, y_inner)], fill=(255, 240, 120, 240), width=2)

    # 5. Tâm sáng bốc cháy
    for r in range(40, 0, -2):
        t = 1.0 - (r / 40.0)
        alpha = int(255 * (t ** 0.8))
        draw.ellipse([(cx - r, cy - r), (cx + r, cy + r)], fill=(255, int(220 + 35 * t), int(100 + 155 * t), alpha))

    img = img.filter(ImageFilter.GaussianBlur(radius=0.6))
    out_path = os.path.join(ART_VFX_DIR, "Tex_VFX_Cinnabar_Magic_Array.png")
    img.save(out_path, "PNG")
    print(f"[VFX Texture] Generated: {out_path}")

def generate_cinnabar_smoke_puff():
    """
    4. Khói Khoáng Thần Sa (Cinnabar Mineral Smoke Billow) 512x512:
    - Đám khói hữu cơ cuộn bồng bềnh sau vụ nổ.
    - Màu đỏ thẫm chu sa / xám than khoáng hỏa (#600D15, #8A1520).
    - Lõi khói vẫn còn tàn dư phát quang cam mờ.
    """
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size / 2.0, size / 2.0

    smoke_lobes = [
        (cx, cy, 140, 240),
        (cx - 60, cy - 45, 110, 220),
        (cx + 65, cy - 40, 115, 220),
        (cx - 70, cy + 35, 100, 200),
        (cx + 70, cy + 45, 105, 200),
        (cx, cy - 85, 95, 190),
        (cx, cy + 80, 95, 190),
    ]

    for lx, ly, lr, alpha_max in smoke_lobes:
        for r in range(lr, 10, -4):
            factor = (1.0 - (r / float(lr))) ** 1.3
            alpha = int(alpha_max * factor * 0.55)
            draw.ellipse([(lx - r, ly - r), (lx + r, ly + r)], fill=(95, 18, 22, alpha))

    for lx, ly, lr, alpha_max in smoke_lobes:
        r_mid = int(lr * 0.75)
        for r in range(r_mid, 8, -3):
            factor = (1.0 - (r / float(r_mid))) ** 1.4
            alpha = int(alpha_max * factor * 0.7)
            draw.ellipse([(lx - r, ly - r), (lx + r, ly + r)], fill=(165, 40, 25, alpha))

    for lx, ly, lr, alpha_max in smoke_lobes:
        r_core = int(lr * 0.4)
        for r in range(r_core, 5, -2):
            factor = (1.0 - (r / float(r_core))) ** 1.5
            alpha = int(alpha_max * factor * 0.8)
            draw.ellipse([(lx - r, ly - r), (lx + r, ly + r)], fill=(255, 130, 40, alpha))

    img = img.filter(ImageFilter.GaussianBlur(radius=6.0))
    out_path = os.path.join(ART_VFX_DIR, "Tex_VFX_Cinnabar_Smoke_Puff.png")
    img.save(out_path, "PNG")
    print(f"[VFX Texture] Generated: {out_path}")

if __name__ == "__main__":
    generate_cinnabar_fireball_burst()
    generate_cinnabar_shockwave_ring()
    generate_cinnabar_magic_array()
    generate_cinnabar_smoke_puff()
