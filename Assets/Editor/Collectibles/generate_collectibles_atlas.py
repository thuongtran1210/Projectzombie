import os
import math
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

def draw_faceted_gem(draw, center, radius, color_base, color_light, color_dark, color_white=(255, 255, 255, 255)):
    cx, cy = center
    # 8-sided faceted Diamond/Crystal geometry
    # Outer ring
    outer_pts = []
    inner_pts = []
    r_outer = radius
    r_inner = radius * 0.52
    
    for i in range(8):
        angle = math.radians(i * 45 - 22.5)
        outer_pts.append((cx + r_outer * math.cos(angle), cy + r_outer * math.sin(angle)))
        inner_pts.append((cx + r_inner * math.cos(angle), cy + r_inner * math.sin(angle)))

    # Outer border / Shadow
    draw.polygon(outer_pts, fill=(10, 15, 20, 240))

    # Outer facet triangles
    for i in range(8):
        p1 = outer_pts[i]
        p2 = outer_pts[(i + 1) % 8]
        p3 = inner_pts[(i + 1) % 8]
        p4 = inner_pts[i]
        
        # Shade by light direction (top-left)
        if i in [6, 7, 0]: # Top & Top-left
            facet_col = color_light
        elif i in [1, 2]: # Right
            facet_col = color_base
        else: # Bottom & bottom-right
            facet_col = color_dark
            
        draw.polygon([p1, p2, p3, p4], fill=facet_col)
        draw.line([p1, p2], fill=(255, 255, 255, 120), width=1)
        draw.line([p1, p4], fill=(0, 0, 0, 100), width=1)

    # Inner table facet (Center octagon)
    draw.polygon(inner_pts, fill=color_light)
    
    # Brilliant Sparkle Highlight (Top-left)
    sx = cx - radius * 0.25
    sy = cy - radius * 0.25
    draw.ellipse([sx - 3, sy - 3, sx + 3, sy + 3], fill=color_white)
    draw.line([(sx - 6, sy), (sx + 6, sy)], fill=color_white, width=1)
    draw.line([(sx, sy - 6), (sx, sy + 6)], fill=color_white, width=1)

def draw_ancient_chest(draw, x0, y0, x1, y1, is_boss=False):
    # Vietnamese Ancient Wooden & Bronze Chest (Rương Cổ Phong Đông Sơn)
    w = x1 - x0
    h = y1 - y0
    
    # 1. Base Drop Shadow
    draw.ellipse([x0 + 4, y1 - 12, x1 - 4, y1 + 4], fill=(0, 0, 0, 120))
    
    # 2. Chest Body (Gỗ mun / Gỗ lim cổ)
    wood_base = (70, 42, 28, 255) if not is_boss else (45, 25, 60, 255) # Boss chest is U Minh Purple
    wood_dark = (45, 25, 15, 255) if not is_boss else (28, 12, 40, 255)
    wood_light = (95, 60, 40, 255) if not is_boss else (75, 45, 95, 255)
    
    body_y0 = y0 + int(h * 0.38)
    body_y1 = y1 - 4
    draw.rectangle([x0 + 6, body_y0, x1 - 6, body_y1], fill=wood_base)
    # Wood grain lines
    for gy in range(body_y0 + 6, body_y1 - 4, 8):
        draw.line([(x0 + 8, gy), (x1 - 8, gy)], fill=wood_dark, width=1)
        
    # 3. Chest Lid (Nắp rương vòm cong)
    lid_y0 = y0 + 6
    lid_y1 = body_y0 + 4
    draw.chord([x0 + 4, lid_y0, x1 - 4, lid_y1 + 12], 180, 360, fill=wood_light)
    draw.line([(x0 + 4, lid_y1), (x1 - 4, lid_y1)], fill=wood_dark, width=2)
    
    # 4. Bronze / Gold Corner Reinforcements (Nẹp đồng cổ Đông Sơn)
    metal_base = (212, 175, 55, 255) if not is_boss else (235, 80, 180, 255)
    metal_light = (255, 225, 120, 255) if not is_boss else (255, 160, 230, 255)
    metal_dark = (140, 100, 25, 255) if not is_boss else (130, 30, 95, 255)
    
    # Metal Corner Straps
    strap_w = 8
    # Left strap
    draw.rectangle([x0 + 10, lid_y0 + 4, x0 + 10 + strap_w, body_y1], fill=metal_base)
    draw.line([(x0 + 10, lid_y0 + 4), (x0 + 10, body_y1)], fill=metal_light, width=1)
    draw.line([(x0 + 10 + strap_w, lid_y0 + 4), (x0 + 10 + strap_w, body_y1)], fill=metal_dark, width=1)
    
    # Right strap
    draw.rectangle([x1 - 10 - strap_w, lid_y0 + 4, x1 - 10, body_y1], fill=metal_base)
    draw.line([(x1 - 10 - strap_w, lid_y0 + 4), (x1 - 10 - strap_w, body_y1)], fill=metal_light, width=1)
    draw.line([(x1 - 10, lid_y0 + 4), (x1 - 10, body_y1)], fill=metal_dark, width=1)
    
    # Center Bronze Dragon Lock (Ổ khóa mặt rồng / Phù ấn cổ)
    lock_cx = (x0 + x1) // 2
    lock_cy = body_y0 + 4
    draw.ellipse([lock_cx - 9, lock_cy - 9, lock_cx + 9, lock_cy + 9], fill=metal_dark)
    draw.ellipse([lock_cx - 7, lock_cy - 7, lock_cx + 7, lock_cy + 7], fill=metal_base)
    draw.ellipse([lock_cx - 4, lock_cy - 4, lock_cx + 4, lock_cy + 4], fill=metal_light)
    # Keyhole
    draw.line([(lock_cx, lock_cy - 2), (lock_cx, lock_cy + 4)], fill=(20, 10, 5, 255), width=2)
    
    # 5. Glowing aura for Boss Chest
    if is_boss:
        # Rune marks
        draw.arc([lock_cx - 14, lock_cy - 14, lock_cx + 14, lock_cy + 14], 0, 360, fill=(255, 100, 220, 180), width=1)

def generate_gems_and_chests():
    # 512x256 Atlas containing:
    # Row 0: 4 Gems (Tier 1: Cyan Lam Ngọc, Tier 2: Emerald Lục Bảo, Tier 3: Purple Tím U Minh, Tier 4: Gold Hoàng Kim) - 64x64 each
    # Row 1: 2 Chests (Normal Chest, Boss U Minh Chest) - 128x128 each
    
    atlas = Image.new("RGBA", (512, 256), (0, 0, 0, 0))
    draw = ImageDraw.Draw(atlas)
    
    # 1. GEMS (Row 0: Y = 0..64)
    # Tier 1: Cyan Lam Ngọc
    draw_faceted_gem(draw, (32, 32), 24, 
                     color_base=(30, 190, 220, 255), 
                     color_light=(140, 240, 255, 255), 
                     color_dark=(10, 110, 150, 255))
    
    # Tier 2: Emerald Lục Bảo
    draw_faceted_gem(draw, (96, 32), 25, 
                     color_base=(35, 200, 95, 255), 
                     color_light=(135, 255, 175, 255), 
                     color_dark=(15, 115, 45, 255))
                     
    # Tier 3: Purple Tím U Minh
    draw_faceted_gem(draw, (160, 32), 26, 
                     color_base=(165, 65, 235, 255), 
                     color_light=(225, 160, 255, 255), 
                     color_dark=(95, 20, 155, 255))
                     
    # Tier 4: Gold Hoàng Kim (Boss Gem)
    draw_faceted_gem(draw, (224, 32), 27, 
                     color_base=(245, 180, 25, 255), 
                     color_light=(255, 240, 140, 255), 
                     color_dark=(165, 105, 10, 255))

    # Single Pure White Gem Template (for runtime shader/sprite tinting)
    draw_faceted_gem(draw, (288, 32), 24,
                     color_base=(200, 210, 225, 255),
                     color_light=(255, 255, 255, 255),
                     color_dark=(130, 140, 160, 255))

    # 2. CHESTS (Row 1: Y = 64..192)
    # Normal Wood & Bronze Chest (128x128) -> (0, 64) to (128, 192)
    draw_ancient_chest(draw, 14, 74, 114, 182, is_boss=False)
    
    # Boss U Minh Legendary Chest (128x128) -> (128, 64) to (256, 192)
    draw_ancient_chest(draw, 142, 74, 242, 182, is_boss=True)

    # Save to Assets
    out_dir = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Collectibles"
    os.makedirs(out_dir, exist_ok=True)
    out_path = os.path.join(out_dir, "Collectibles_Atlas.png")
    atlas.save(out_path, "PNG")
    print(f"Successfully generated Collectibles & Chests Atlas at: {out_path}")

if __name__ == "__main__":
    generate_gems_and_chests()
