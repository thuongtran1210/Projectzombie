import os
import math
from PIL import Image, ImageDraw, ImageFilter

def create_dash_textures():
    output_dir = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\VFX\Dash"
    os.makedirs(output_dir, exist_ok=True)

    # 1. TEX_Dash_WindPuff.png (Cụm bụi gió xoáy đạp chân lướt) - 256x256
    size = 256
    img_dust = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw_dust = ImageDraw.Draw(img_dust)

    cx, cy = size // 2, size // 2
    # Wind swirl rings & Dust puffs
    wind_cyan = (130, 235, 255, 220)
    wind_white = (255, 255, 255, 240)
    dust_dirt = (210, 180, 140, 180)

    # Outer expanding puff clouds
    draw_dust.ellipse([cx - 90, cy - 50, cx + 90, cy + 50], fill=(240, 245, 255, 140))
    draw_dust.ellipse([cx - 70, cy - 65, cx + 20, cy + 30], fill=wind_white)
    draw_dust.ellipse([cx - 20, cy - 60, cx + 70, cy + 35], fill=wind_cyan)
    draw_dust.ellipse([cx - 50, cy - 20, cx + 50, cy + 60], fill=dust_dirt)

    # Wind arcs
    draw_dust.arc([cx - 100, cy - 60, cx + 100, cy + 60], 30, 150, fill=wind_white, width=6)
    draw_dust.arc([cx - 80, cy - 80, cx + 80, cy + 80], 200, 340, fill=wind_cyan, width=5)

    dust_path = os.path.join(output_dir, "TEX_Dash_WindPuff.png")
    img_dust.save(dust_path, "PNG")
    print(f"Generated Dash Wind Puff Texture at: {dust_path}")

    # 2. TEX_Dash_SpeedStreak.png (Vệt tốc độ Anime) - 256x128
    w, h = 256, 128
    img_streak = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw_streak = ImageDraw.Draw(img_streak)

    # Streaks tapered from left (tail) to right (head)
    for i in range(5):
        sy = 20 + i * 22
        sw = 180 + (i % 3) * 30
        draw_streak.polygon([(w - 20 - sw, sy + 3), (w - 20, sy), (w - 20 - sw, sy - 3)], fill=(200, 245, 255, 200))
        draw_streak.polygon([(w - 40 - sw, sy + 1), (w - 20, sy), (w - 40 - sw, sy - 1)], fill=(255, 255, 255, 255))

    streak_path = os.path.join(output_dir, "TEX_Dash_SpeedStreak.png")
    img_streak.save(streak_path, "PNG")
    print(f"Generated Dash Speed Streak Texture at: {streak_path}")

if __name__ == "__main__":
    create_dash_textures()
