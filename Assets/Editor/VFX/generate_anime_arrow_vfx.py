import os
import math
from PIL import Image, ImageDraw, ImageFilter
import numpy as np

OUTPUT_PATHS = [
    r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Skills\VFX_ThachSanh_SonicArrow.png",
    r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Projectiles\Arrow_ThachSanh.png"
]

def draw_sonic_ring(draw, cx, cy, rx, ry, width, color_outer, color_inner):
    """Vẽ vòng sóng âm xé gió hình oval dạng anime gợn sóng."""
    # Outer glow
    for offset in range(width, 0, -1):
        alpha = int(255 * (1.0 - offset / (width + 1)))
        draw.ellipse([cx - rx - offset, cy - ry - offset, cx + rx + offset, cy + ry + offset], 
                     outline=(color_outer[0], color_outer[1], color_outer[2], alpha), width=2)
    # Core sharp ring
    draw.ellipse([cx - rx, cy - ry, cx + rx, cy + ry], 
                 outline=color_inner, width=width)

def generate_anime_sonic_arrow():
    width = 1024
    height = 512
    img = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cy = height // 2

    # 1. Subtle Dark Smoke/Energy Backing Cloud
    smoke_layer = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    sdraw = ImageDraw.Draw(smoke_layer)
    sdraw.ellipse([250, cy - 80, 750, cy + 80], fill=(20, 15, 30, 140))
    sdraw.ellipse([320, cy - 100, 680, cy + 100], fill=(40, 30, 20, 100))
    sdraw.ellipse([450, cy - 70, 800, cy + 70], fill=(60, 45, 10, 80))
    smoke_layer = smoke_layer.filter(ImageFilter.GaussianBlur(radius=18))
    img.alpha_composite(smoke_layer)

    # 2. Main Glow Base (Aura vàng cam)
    glow_layer = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    gdraw = ImageDraw.Draw(glow_layer)

    # Vùng phát sáng đầu mũi tên
    gdraw.ellipse([700, cy - 90, 960, cy + 90], fill=(255, 160, 0, 180))
    gdraw.ellipse([760, cy - 60, 950, cy + 60], fill=(255, 220, 50, 230))

    # Tia tia sáng cạnh bên (Side Streaks)
    gdraw.line([(500, cy - 35), (780, cy - 35)], fill=(255, 200, 50, 220), width=12)
    gdraw.line([(520, cy + 35), (780, cy + 35)], fill=(255, 200, 50, 220), width=12)
    gdraw.line([(620, cy - 20), (820, cy - 20)], fill=(255, 140, 20, 240), width=8)
    gdraw.line([(620, cy + 20), (820, cy + 20)], fill=(255, 140, 20, 240), width=8)

    # Dải tia năng lượng chính
    gdraw.line([(120, cy), (880, cy)], fill=(255, 180, 0, 255), width=24)
    gdraw.line([(120, cy), (880, cy)], fill=(255, 240, 100, 255), width=14)
    gdraw.ellipse([100, cy - 20, 150, cy + 20], fill=(255, 220, 50, 255))

    glow_layer = glow_layer.filter(ImageFilter.GaussianBlur(radius=5))
    img.alpha_composite(glow_layer)

    # 3. Sonic Shockwave Mach Rings (3 Vòng Xé Gió Anime)
    rings_layer = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    rdraw = ImageDraw.Draw(rings_layer)

    # Ring 1 (Lớn nhất ở sau: X ~ 280)
    draw_sonic_ring(rdraw, 280, cy, 32, 130, 8, (255, 140, 0), (255, 235, 80, 255))
    # Ring 2 (Vừa ở giữa: X ~ 380)
    draw_sonic_ring(rdraw, 380, cy, 28, 115, 7, (255, 150, 0), (255, 240, 100, 255))
    # Ring 3 (Nhỏ hơn ở trước: X ~ 480)
    draw_sonic_ring(rdraw, 480, cy, 24, 100, 6, (255, 160, 0), (255, 245, 120, 255))

    # Cắt rãnh răng cưa/gợn sóng đặc trưng Anime trên rings
    rings_layer = rings_layer.filter(ImageFilter.GaussianBlur(radius=1.2))
    img.alpha_composite(rings_layer)

    # 4. Sharp Anime Arrow Core (Mũi Tên Nhọn Sắc + Đầu Nón Phản Lực)
    core_layer = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    cdraw = ImageDraw.Draw(core_layer)

    # Đầu Mũi Nón Anime (Arrowhead Cone)
    head_points = [
        (940, cy),         # Mũi nhọn đâm tới
        (820, cy - 65),    # Cánh trên
        (840, cy - 30),    # Eo khuyết trên
        (860, cy),         # Khuyết lõi giữa
        (840, cy + 30),    # Eo khuyết dưới
        (820, cy + 65),    # Cánh dưới
    ]
    cdraw.polygon(head_points, fill=(255, 215, 0, 255), outline=(255, 140, 0, 255), width=4)

    # Lõi sáng trắng bên trong đầu mũi tên
    inner_head = [
        (930, cy),
        (840, cy - 40),
        (855, cy),
        (840, cy + 40)
    ]
    cdraw.polygon(inner_head, fill=(255, 255, 255, 255))

    # Tia sáng trắng siêu nét ở trục trung tâm
    cdraw.line([(120, cy), (880, cy)], fill=(255, 255, 255, 255), width=6)
    cdraw.ellipse([110, cy - 12, 140, cy + 12], fill=(255, 255, 255, 255))

    # Đốm tia phóng nhỏ (Micro Sparks)
    sparks = [
        (760, cy - 45, 800, cy - 45),
        (740, cy + 45, 780, cy + 45),
        (810, cy - 50, 830, cy - 50),
        (810, cy + 50, 830, cy + 50)
    ]
    for (x1, y1, x2, y2) in sparks:
        cdraw.line([(x1, y1), (x2, y2)], fill=(255, 255, 220, 255), width=3)
        cdraw.ellipse([x2 - 3, y2 - 3, x2 + 3, y2 + 3], fill=(255, 255, 255, 255))

    img.alpha_composite(core_layer)

    # Lưu ảnh ra các thư mục
    for p in OUTPUT_PATHS:
        os.makedirs(os.path.dirname(p), exist_ok=True)
        img.save(p, "PNG")
        print(f"Generated High Quality Anime Sonic Arrow: {p}")

if __name__ == "__main__":
    generate_anime_sonic_arrow()
