import os
import math
from PIL import Image, ImageDraw, ImageFilter

def main():
    output_dir = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Weapons\VFX"
    os.makedirs(output_dir, exist_ok=True)

    # -------------------------------------------------------------
    # 1. W_POT: Nồi Cơm Thạch Sanh
    # -------------------------------------------------------------
    # 1.1 Tex_Pot_Projectile.png - 256x256 (Nồi gang chibi xoay tít)
    size = 256
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2

    # Outer outline pot body (Dark cast iron)
    draw.ellipse([cx - 70, cy - 50, cx + 70, cy + 60], fill=(45, 42, 40, 255), outline=(15, 12, 10, 255), width=6)
    # Pot inner rim / opening
    draw.ellipse([cx - 60, cy - 55, cx + 60, cy - 15], fill=(70, 65, 60, 255), outline=(15, 12, 10, 255), width=5)
    # Golden rice glow inside
    draw.ellipse([cx - 50, cy - 50, cx + 50, cy - 20], fill=(255, 235, 150, 255))
    # Handles (ear handles)
    draw.arc([cx - 85, cy - 35, cx - 55, cy + 5], 90, 270, fill=(20, 18, 15, 255), width=8)
    draw.arc([cx + 55, cy - 35, cx + 85, cy + 5], 270, 90, fill=(20, 18, 15, 255), width=8)
    # Speed arc highlights
    draw.arc([cx - 85, cy - 65, cx + 85, cy + 75], 130, 220, fill=(255, 215, 80, 200), width=5)
    img.save(os.path.join(output_dir, "Tex_Pot_Projectile.png"), "PNG")

    # 1.2 Tex_Pot_Suction_Vortex.png - 512x512 (Lốc xoáy hút chân không Thổ/Hoàng Kim)
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    for r in range(40, 230, 25):
        alpha = int(220 * (1.0 - (r / 250.0)))
        draw.arc([cx - r, cy - r, cx + r, cy + r], r * 2, r * 2 + 160, fill=(255, 200, 80, alpha), width=12)
        draw.arc([cx - r + 5, cy - r + 5, cx + r - 5, cy + r - 5], r * 2 + 30, r * 2 + 140, fill=(255, 255, 255, alpha), width=6)
    img.save(os.path.join(output_dir, "Tex_Pot_Suction_Vortex.png"), "PNG")

    # 1.3 Tex_Rice_Collectible.png - 128x128 (Cơm nắm hồ lô/tam giác phát sáng)
    size = 128
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    # Triangle rice ball rounded
    points = [(cx, cy - 38), (cx + 42, cy + 32), (cx - 42, cy + 32)]
    draw.polygon(points, fill=(255, 255, 255, 255), outline=(220, 190, 140, 255))
    # Seaweed nori wrap at bottom
    draw.rectangle([cx - 20, cy + 10, cx + 20, cy + 32], fill=(30, 50, 30, 255))
    # Sparkle glow
    draw.line([(cx + 25, cy - 25), (cx + 35, cy - 25)], fill=(255, 220, 50, 255), width=3)
    draw.line([(cx + 30, cy - 30), (cx + 30, cy - 20)], fill=(255, 220, 50, 255), width=3)
    img.save(os.path.join(output_dir, "Tex_Rice_Collectible.png"), "PNG")

    # -------------------------------------------------------------
    # 2. R008: Chổi Lông Gà Gia Truyền
    # -------------------------------------------------------------
    # 2.1 Tex_ChickenBroom_Giant.png - 256x512 (Cây chổi lông gà sặc sỡ đập xuống)
    w, h = 256, 512
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx = w // 2
    # Bamboo stick handle
    draw.rectangle([cx - 10, 180, cx + 10, h - 30], fill=(215, 175, 100, 255), outline=(120, 90, 40, 255), width=4)
    # Feathers head (Layers of red, gold, purple, green feathers)
    colors = [(220, 40, 40, 255), (255, 180, 20, 255), (140, 40, 180, 255), (30, 160, 90, 255), (240, 80, 30, 255)]
    for i in range(12):
        fy = 40 + i * 16
        c = colors[i % len(colors)]
        draw.ellipse([cx - 55 - (i % 3) * 10, fy - 15, cx + 55 + (i % 3) * 10, fy + 35], fill=c, outline=(30, 20, 10, 255), width=4)
    # Highlight shine
    draw.ellipse([cx - 20, 60, cx + 10, 120], fill=(255, 255, 255, 160))
    img.save(os.path.join(output_dir, "Tex_ChickenBroom_Giant.png"), "PNG")

    # 2.2 Tex_Feather_Burst.png - 256x256 (Cụm lông gà xoay tròn văng ra)
    size = 256
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    # Stylized single feather curved
    f_pts = [(cx - 20, cy + 80), (cx - 40, cy), (cx - 10, cy - 80), (cx + 20, cy - 70), (cx + 40, cy + 10), (cx + 10, cy + 75)]
    draw.polygon(f_pts, fill=(255, 170, 30, 255), outline=(180, 80, 10, 255))
    # Feather quill shaft
    draw.line([(cx - 15, cy + 90), (cx + 5, cy - 75)], fill=(255, 255, 255, 255), width=4)
    img.save(os.path.join(output_dir, "Tex_Feather_Burst.png"), "PNG")

    # 2.3 Tex_Ground_Cracked_Shockwave.png - 512x512 (Vết nứt đất chấn động Manga)
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    for angle in [0, 45, 90, 135, 180, 225, 270, 315]:
        rad = math.radians(angle)
        ex = cx + int(math.cos(rad) * 190)
        ey = cy + int(math.sin(rad) * 190)
        mid_x = cx + int(math.cos(rad + 0.15) * 100)
        mid_y = cy + int(math.sin(rad + 0.15) * 100)
        draw.line([(cx, cy), (mid_x, mid_y), (ex, ey)], fill=(255, 210, 90, 240), width=9)
        draw.line([(cx, cy), (mid_x, mid_y), (ex, ey)], fill=(255, 255, 255, 255), width=4)
    img.save(os.path.join(output_dir, "Tex_Ground_Cracked_Shockwave.png"), "PNG")

    # -------------------------------------------------------------
    # 3. W007: Cung Thạch Sanh
    # -------------------------------------------------------------
    # 3.1 Tex_ThachSanh_Arrow_Trail.png - 512x128 (Dải năng lượng hoàng kim xé gió)
    w, h = 512, 128
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    # High speed piercing beam tapered from left to sharp right tip
    pts_outer = [(10, 20), (w - 20, h // 2), (10, h - 20), (80, h // 2)]
    draw.polygon(pts_outer, fill=(255, 195, 30, 220))
    pts_core = [(60, 40), (w - 10, h // 2), (60, h - 40), (120, h // 2)]
    draw.polygon(pts_core, fill=(255, 255, 255, 255))
    img.save(os.path.join(output_dir, "Tex_ThachSanh_Arrow_Trail.png"), "PNG")

    # 3.2 Tex_ThachSanh_Pierce_Shockwave.png - 256x256 (Nón áp suất xuyên thủng)
    size = 256
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    draw.arc([cx - 100, cy - 100, cx + 100, cy + 100], 120, 240, fill=(255, 225, 80, 255), width=14)
    draw.arc([cx - 80, cy - 80, cx + 80, cy + 80], 130, 230, fill=(255, 255, 255, 255), width=7)
    img.save(os.path.join(output_dir, "Tex_ThachSanh_Pierce_Shockwave.png"), "PNG")

    # -------------------------------------------------------------
    # 4. R007: Chiếu Trải Hoàng Tuyền
    # -------------------------------------------------------------
    # 4.1 Tex_Sleep_Zzz_Comic.png - 256x256 (Chữ Zzz truyện tranh bay bổng)
    size = 256
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    # Large Z
    z1 = [(120, 60), (180, 60), (130, 110), (190, 110)]
    draw.line(z1, fill=(100, 220, 255, 255), width=14)
    draw.line(z1, fill=(255, 255, 255, 255), width=6)
    # Medium z
    z2 = [(70, 130), (115, 130), (75, 170), (120, 170)]
    draw.line(z2, fill=(140, 240, 180, 255), width=10)
    draw.line(z2, fill=(255, 255, 255, 255), width=4)
    # Small z
    z3 = [(40, 195), (65, 195), (45, 220), (70, 220)]
    draw.line(z3, fill=(180, 255, 200, 255), width=7)
    img.save(os.path.join(output_dir, "Tex_Sleep_Zzz_Comic.png"), "PNG")

    # 4.2 Tex_Mat_Slide_Wind.png - 256x256 (Vệt gió cuộn lướt chiếu)
    size = 256
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    draw.arc([cx - 90, cy - 50, cx + 90, cy + 50], 40, 160, fill=(130, 235, 180, 220), width=10)
    draw.arc([cx - 70, cy - 30, cx + 70, cy + 30], 50, 150, fill=(255, 255, 255, 240), width=5)
    img.save(os.path.join(output_dir, "Tex_Mat_Slide_Wind.png"), "PNG")

    # -------------------------------------------------------------
    # 5. Giai đoạn 2: W008, W009, W004
    # -------------------------------------------------------------
    # 5.1 Tex_FoxFlame_Stream.png - 512x256 (Luồng lửa Cửu Vĩ hồ ly đỏ cam)
    w, h = 512, 256
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.polygon([(20, h//2 - 60), (w - 30, h//2), (20, h//2 + 60)], fill=(255, 70, 20, 220))
    draw.polygon([(60, h//2 - 35), (w - 40, h//2), (60, h//2 + 35)], fill=(255, 200, 40, 240))
    draw.polygon([(100, h//2 - 15), (w - 60, h//2), (100, h//2 + 15)], fill=(255, 255, 255, 255))
    img.save(os.path.join(output_dir, "Tex_FoxFlame_Stream.png"), "PNG")

    # 5.2 Tex_WaterLightning_Chain.png - 256x256 (Sét nước Thủy Cung)
    size = 256
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    bolt = [(30, 40), (110, 110), (90, 130), (190, 220), (130, 210), (220, 245)]
    draw.line(bolt, fill=(60, 180, 255, 255), width=12)
    draw.line(bolt, fill=(210, 245, 255, 255), width=5)
    img.save(os.path.join(output_dir, "Tex_WaterLightning_Chain.png"), "PNG")

    # 5.3 Tex_FoxClaw_BloodOrb.png - 128x128 (Linh châu huyết khí hút máu)
    size = 128
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size // 2, size // 2
    draw.ellipse([cx - 40, cy - 40, cx + 40, cy + 40], fill=(230, 30, 60, 220), outline=(255, 120, 140, 255), width=4)
    draw.ellipse([cx - 20, cy - 25, cx + 5, cy], fill=(255, 255, 255, 200))
    img.save(os.path.join(output_dir, "Tex_FoxClaw_BloodOrb.png"), "PNG")

    print("[SUCCESS] All Weapon VFX Textures generated cleanly in Art/Weapons/VFX")

if __name__ == "__main__":
    main()
