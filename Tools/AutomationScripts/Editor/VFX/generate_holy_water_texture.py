import os
import math
from PIL import Image, ImageDraw

def create_holy_water_texture():
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size / 2.0, size / 2.0
    max_radius = size * 0.45
    
    # 1. Base Sacred Water Pool (Cyan / Aquamarine with soft edges)
    for r in range(int(max_radius), 0, -2):
        t = r / max_radius  # 1.0 at rim, 0.0 at center
        alpha = int(220 * math.sin((1.0 - t) * math.pi * 0.5))
        
        # Color gradient: Center is bright glowing cyan/white, rim is deep sacred blue-green
        red = int(80 + 175 * (1.0 - t))
        green = int(210 + 45 * (1.0 - t))
        blue = int(240 + 15 * (1.0 - t))
        
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(red, green, blue, alpha))
        
    # 2. Golden Buddhist Radiance & Concentric Holy Ripples
    ripple_radii = [max_radius * 0.95, max_radius * 0.72, max_radius * 0.48, max_radius * 0.25]
    for i, r in enumerate(ripple_radii):
        w = 3 if i > 0 else 5
        gold_r = 255
        gold_g = 220
        gold_b = 100
        alpha = 180 if i > 0 else 240
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], outline=(gold_r, gold_g, gold_b, alpha), width=w)
        
    # 3. 8-Petal Sacred Lotus Petals in Center
    petals = 8
    lotus_r = max_radius * 0.38
    for p in range(petals):
        angle = p * (2 * math.pi / petals)
        px = cx + math.cos(angle) * (lotus_r * 0.6)
        py = cy + math.sin(angle) * (lotus_r * 0.6)
        pr = lotus_r * 0.35
        draw.ellipse([px - pr, py - pr, px + pr, py + pr], outline=(255, 235, 140, 200), width=2)
        
    # 4. Center Gold Lotus Core
    core_r = max_radius * 0.15
    draw.ellipse([cx - core_r, cy - core_r, cx + core_r, cy + core_r], fill=(255, 245, 180, 255))
    
    out_dir = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\VFX"
    os.makedirs(out_dir, exist_ok=True)
    out_path = os.path.join(out_dir, "Tex_VFX_HolyWater_Puddle.png")
    img.save(out_path, "PNG")
    print(f"Generated Holy Water Texture at: {out_path}")

if __name__ == "__main__":
    create_holy_water_texture()
