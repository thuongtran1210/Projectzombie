import os
import math
from PIL import Image, ImageDraw, ImageFilter

TEX_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\VFX\SkillLibrary\Textures"
os.makedirs(TEX_DIR, exist_ok=True)

def create_mat_texture():
    # 512x320 - Tấm chiếu cói dệt hoa văn cổ phong, viền vải đỏ thêu, 4 góc bùa chú vàng
    w, h = 512, 320
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    pad_x, pad_y = 24, 20
    
    # 1. Nền chiếu cói vàng nhạt / nâu ấm
    base_color = (225, 205, 150, 255)
    border_red = (180, 40, 35, 255)
    border_gold = (245, 195, 60, 255)
    inner_straw1 = (210, 185, 130, 255)
    inner_straw2 = (235, 215, 165, 255)
    
    # Bo góc mềm mại
    radius = 16
    draw.rounded_rectangle([pad_x, pad_y, w - pad_x, h - pad_y], radius=radius, fill=base_color)
    
    # 2. Dệt nan cói (Weave pattern chéo & ngang)
    for y in range(pad_y + 14, h - pad_y - 14, 8):
        c = inner_straw1 if (y // 8) % 2 == 0 else inner_straw2
        draw.line([pad_x + 16, y, w - pad_x - 16, y], fill=c, width=3)
        
    for x in range(pad_x + 18, w - pad_x - 18, 12):
        draw.line([x, pad_y + 14, x, h - pad_y - 14], fill=(190, 165, 115, 120), width=1)
        
    # Họa tiết hoa văn Hoàng Tuyền ở giữa chiếu (Vòng xoáy mộc hệ / âm dương cách điệu)
    cx, cy = w // 2, h // 2
    draw.ellipse([cx - 48, cy - 36, cx + 48, cy + 36], outline=(170, 140, 90, 180), width=3)
    draw.ellipse([cx - 32, cy - 24, cx + 32, cy + 24], outline=(200, 70, 60, 160), width=2)
    # Cánh hoa sen cách điệu 4 hướng
    draw.line([cx - 24, cy, cx + 24, cy], fill=(180, 50, 45, 180), width=3)
    draw.line([cx, cy - 18, cx, cy + 18], fill=(180, 50, 45, 180), width=3)
    draw.ellipse([cx - 8, cy - 8, cx + 8, cy + 8], fill=(245, 195, 60, 220))
    
    # 3. Viền vải thổ cẩm đỏ bọc mép chiếu (Fabric Border)
    draw.rounded_rectangle([pad_x, pad_y, w - pad_x, h - pad_y], radius=radius, outline=border_red, width=12)
    draw.rounded_rectangle([pad_x + 6, pad_y + 6, w - pad_x - 6, h - pad_y - 6], radius=radius - 4, outline=border_gold, width=2)
    
    # 4. Họa tiết chỉ may viền đứt đoạn
    for x in range(pad_x + 10, w - pad_x - 10, 10):
        draw.line([x, pad_y + 3, x + 4, pad_y + 3], fill=(255, 235, 150, 220), width=2)
        draw.line([x, h - pad_y - 4, x + 4, h - pad_y - 4], fill=(255, 235, 150, 220), width=2)
    for y in range(pad_y + 10, h - pad_y - 10, 10):
        draw.line([pad_x + 3, y, pad_x + 3, y + 4], fill=(255, 235, 150, 220), width=2)
        draw.line([w - pad_x - 4, y, w - pad_x - 4, y + 4], fill=(255, 235, 150, 220), width=2)
        
    # 5. Bùa Chú Hoàng Tuyền (Talisman) tại 4 góc
    talisman_w, talisman_h = 24, 30
    corners = [
        (pad_x + 8, pad_y + 8),
        (w - pad_x - talisman_w - 8, pad_y + 8),
        (pad_x + 8, h - pad_y - talisman_h - 8),
        (w - pad_x - talisman_w - 8, h - pad_y - talisman_h - 8)
    ]
    for (tx, ty) in corners:
        # Giấy bùa vàng
        draw.rectangle([tx, ty, tx + talisman_w, ty + talisman_h], fill=(255, 215, 60, 255), outline=(200, 50, 30, 255), width=2)
        # Chữ triện / phù chú đỏ
        draw.line([tx + 4, ty + 6, tx + talisman_w - 4, ty + 6], fill=(220, 30, 30, 255), width=2)
        draw.line([tx + talisman_w // 2, ty + 6, tx + talisman_w // 2, ty + talisman_h - 6], fill=(220, 30, 30, 255), width=2)
        draw.line([tx + 6, ty + talisman_h - 8, tx + talisman_w - 6, ty + talisman_h - 8], fill=(220, 30, 30, 255), width=2)
        draw.ellipse([tx + talisman_w // 2 - 3, ty + 12, tx + talisman_w // 2 + 3, ty + 18], outline=(220, 30, 30, 255), width=1)

    out_path = os.path.join(TEX_DIR, "Tex_SleepingMat_Mat_Clean.png")
    img.save(out_path, "PNG")
    print(f"Saved: {out_path}")

def create_zzz_texture():
    # 256x256 - Ký tự Comic "Zzz" nghệ thuật phát sáng
    size = 256
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Vẽ 3 chữ Z kích thước tăng dần theo đường chéo
    def draw_comic_z(cx, cy, scale, alpha):
        w = 32 * scale
        h = 36 * scale
        th = max(3, int(6 * scale))
        
        # Outer glow
        glow_col = (80, 240, 180, int(160 * alpha))
        main_col = (255, 255, 255, int(255 * alpha))
        cyan_col = (100, 255, 220, int(240 * alpha))
        
        # Z lines: top bar, diagonal, bottom bar
        p1 = (cx - w/2, cy - h/2)
        p2 = (cx + w/2, cy - h/2)
        p3 = (cx - w/2, cy + h/2)
        p4 = (cx + w/2, cy + h/2)
        
        # Shadow/Glow
        draw.line([p1, p2], fill=glow_col, width=th + 6)
        draw.line([p2, p3], fill=glow_col, width=th + 6)
        draw.line([p3, p4], fill=glow_col, width=th + 6)
        
        # Color fill
        draw.line([p1, p2], fill=cyan_col, width=th + 2)
        draw.line([p2, p3], fill=cyan_col, width=th + 2)
        draw.line([p3, p4], fill=cyan_col, width=th + 2)
        
        # Highlight white
        draw.line([(p1[0]+2, p1[1]), (p2[0]-2, p2[1])], fill=main_col, width=max(1, th - 2))
        draw.line([(p2[0]-2, p2[1]+2), (p3[0]+2, p3[1]-2)], fill=main_col, width=max(1, th - 2))
        draw.line([(p3[0]+2, p3[1]), (p4[0]-2, p4[1])], fill=main_col, width=max(1, th - 2))
        
    draw_comic_z(70, 180, 0.9, 0.75)
    draw_comic_z(125, 125, 1.3, 0.9)
    draw_comic_z(185, 65, 1.8, 1.0)
    
    # Thêm vài ngôi sao ngủ lấp lánh nhỏ
    for sx, sy in [(50, 120), (190, 150), (120, 50)]:
        draw.line([sx - 6, sy, sx + 6, sy], fill=(255, 240, 120, 200), width=2)
        draw.line([sx, sy - 6, sx, sy + 6], fill=(255, 240, 120, 200), width=2)
        draw.ellipse([sx - 2, sy - 2, sx + 2, sy + 2], fill=(255, 255, 255, 255))
        
    out_path = os.path.join(TEX_DIR, "Tex_SleepingMat_SleepZzz_Clean.png")
    img.save(out_path, "PNG")
    print(f"Saved: {out_path}")

def create_bubble_texture():
    # 256x256 - Bong bóng ngủ / snot bubble hoạt hình comic phập phồng
    size = 256
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size // 2, size // 2
    r = 96
    
    # Vòng phát quang ngoài (Glow)
    for i in range(16, 0, -2):
        alpha = int(80 * (1.0 - i / 16))
        draw.ellipse([cx - r - i, cy - r - i, cx + r + i, cy + r + i], outline=(100, 240, 200, alpha), width=2)
        
    # Thân bong bóng bán trong suốt gradient
    bubble_fill = (160, 240, 220, 80)
    bubble_outline = (130, 255, 210, 230)
    draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=bubble_fill, outline=bubble_outline, width=6)
    
    # Highlight phản chiếu ánh sáng (Specular Crescent)
    draw.arc([cx - r + 14, cy - r + 14, cx + r - 14, cy + r - 14], start=200, end=310, fill=(255, 255, 255, 240), width=10)
    draw.ellipse([cx - 45, cy - 50, cx - 25, cy - 30], fill=(255, 255, 255, 255))
    draw.ellipse([cx + 35, cy + 40, cx + 45, cy + 50], fill=(255, 255, 255, 180))
    
    out_path = os.path.join(TEX_DIR, "Tex_SleepingMat_Bubble_Clean.png")
    img.save(out_path, "PNG")
    print(f"Saved: {out_path}")

def create_dream_mist_texture():
    # 256x256 - Làn khói sương hương thảo mộc ru ngủ (Dream Mist)
    size = 256
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size // 2, size // 2
    # Vẽ cụm mây tròn mờ ảo
    blobs = [
        (cx, cy, 70, (120, 235, 180, 90)),
        (cx - 35, cy + 15, 50, (100, 220, 160, 80)),
        (cx + 35, cy - 15, 55, (140, 250, 190, 85)),
        (cx - 20, cy - 30, 45, (160, 255, 210, 70)),
        (cx + 25, cy + 25, 48, (110, 230, 170, 75))
    ]
    for bx, by, br, bcol in blobs:
        draw.ellipse([bx - br, by - br, bx + br, by + br], fill=bcol)
        
    img = img.filter(ImageFilter.GaussianBlur(16))
    
    # Thêm vài hạt phấn hoa mộc vàng óng
    draw2 = ImageDraw.Draw(img)
    pollen_coords = [(80, 90), (170, 110), (110, 170), (150, 160), (130, 80)]
    for px, py in pollen_coords:
        draw2.ellipse([px - 4, py - 4, px + 4, py + 4], fill=(255, 245, 140, 220))
        draw2.ellipse([px - 2, py - 2, px + 2, py + 2], fill=(255, 255, 255, 255))
        
    out_path = os.path.join(TEX_DIR, "Tex_SleepingMat_DreamMist.png")
    img.save(out_path, "PNG")
    print(f"Saved: {out_path}")

def create_strike_impact_texture():
    # 256x256 - Tia sao nổ va chạm "STRIKE" và sóng chấn động bowling
    size = 256
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size // 2, size // 2
    # Vẽ các tia nhọn 12 cánh (12-point burst)
    rays = 12
    outer_r = 110
    inner_r = 28
    points = []
    for i in range(rays * 2):
        angle = i * (math.pi / rays)
        r = outer_r if i % 2 == 0 else inner_r
        if i % 4 == 0:
            r = outer_r + 15
        px = cx + r * math.cos(angle)
        py = cy + r * math.sin(angle)
        points.append((px, py))
        
    draw.polygon(points, fill=(255, 215, 60, 240), outline=(255, 90, 40, 255))
    
    # Tâm nổ trắng sáng rực
    draw.ellipse([cx - 30, cy - 30, cx + 30, cy + 30], fill=(255, 255, 255, 255), outline=(255, 240, 150, 255), width=3)
    
    # Sóng vòng tròn xung kích
    draw.ellipse([cx - 75, cy - 75, cx + 75, cy + 75], outline=(255, 255, 200, 180), width=4)
    
    out_path = os.path.join(TEX_DIR, "Tex_SleepingMat_StrikeImpact.png")
    img.save(out_path, "PNG")
    print(f"Saved: {out_path}")

if __name__ == "__main__":
    create_mat_texture()
    create_zzz_texture()
    create_bubble_texture()
    create_dream_mist_texture()
    create_strike_impact_texture()
    print("ALL SLEEPING MAT TEXTURES GENERATED SUCCESSFULLY!")
