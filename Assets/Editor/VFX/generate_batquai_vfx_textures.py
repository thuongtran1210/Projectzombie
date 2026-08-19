import os
import math
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

def create_bat_quai_tran_textures():
    SIZE = 1024
    CENTER = SIZE // 2
    RADIUS = 480
    
    # -------------------------------------------------------------
    # 1. TEX_BatQuai_GroundDecal.png: Mặt trận Thái Cực Bát Quái Cổ Phong
    # -------------------------------------------------------------
    img = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Outer Glow Ring / Vòng sáng vàng kim
    gold_glow = (212, 175, 55, 255)
    gold_bright = (255, 235, 130, 255)
    gold_dark = (140, 105, 25, 255)
    cyan_energy = (60, 210, 240, 255)
    
    # Outer Octagon (Bát giác viền ngoài)
    outer_pts = []
    inner_pts = []
    for i in range(8):
        angle = math.radians(i * 45 - 22.5)
        outer_pts.append((CENTER + RADIUS * math.cos(angle), CENTER + RADIUS * math.sin(angle)))
        inner_pts.append((CENTER + (RADIUS - 24) * math.cos(angle), CENTER + (RADIUS - 24) * math.sin(angle)))
        
    draw.polygon(outer_pts, fill=(20, 15, 10, 160)) # Nền mờ trận đồ
    for i in range(8):
        p1 = outer_pts[i]
        p2 = outer_pts[(i + 1) % 8]
        draw.line([p1, p2], fill=gold_bright, width=6)
        
    # Vòng tròn thần chú trung tâm
    draw.ellipse([CENTER - (RADIUS - 30), CENTER - (RADIUS - 30), CENTER + (RADIUS - 30), CENTER + (RADIUS - 30)], outline=gold_glow, width=4)
    draw.ellipse([CENTER - (RADIUS - 90), CENTER - (RADIUS - 90), CENTER + (RADIUS - 90), CENTER + (RADIUS - 90)], outline=gold_dark, width=3)
    
    # 8 Cung Quẻ Bát Quái (Trigram Symbols: Càn, Khảm, Cấn, Chấn, Tốn, Ly, Khôn, Đoài)
    # Vẽ 3 vạch hào âm/dương cho 8 hướng
    trigrams = [
        [1, 1, 1], # Càn (3 vạch liền)
        [0, 1, 0], # Khảm (liền ở giữa, đứt 2 bên)
        [1, 0, 0], # Cấn
        [0, 0, 1], # Chấn
        [0, 1, 1], # Tốn
        [1, 0, 1], # Ly
        [0, 0, 0], # Khôn (3 vạch đứt)
        [1, 1, 0], # Đoài
    ]
    
    trigram_radius = RADIUS - 60
    for idx, tri in enumerate(trigrams):
        angle_deg = idx * 45
        angle_rad = math.radians(angle_deg)
        tx = CENTER + trigram_radius * math.cos(angle_rad)
        ty = CENTER + trigram_radius * math.sin(angle_rad)
        
        # Draw 3 bars
        bar_len = 36
        bar_spacing = 10
        tangent_angle = angle_rad + math.pi / 2
        cos_t = math.cos(tangent_angle)
        sin_t = math.sin(tangent_angle)
        cos_n = math.cos(angle_rad)
        sin_n = math.sin(angle_rad)
        
        for bar_idx, is_yang in enumerate(tri):
            offset_n = (bar_idx - 1) * bar_spacing
            bx = tx + offset_n * cos_n
            by = ty + offset_n * sin_n
            
            if is_yang == 1:
                # Vạch Liền (Dương)
                p_start = (bx - (bar_len / 2) * cos_t, by - (bar_len / 2) * sin_t)
                p_end = (bx + (bar_len / 2) * cos_t, by + (bar_len / 2) * sin_t)
                draw.line([p_start, p_end], fill=gold_bright, width=5)
            else:
                # Vạch Đứt (Âm - có khe hở ở giữa)
                half_gap = 5
                p1 = (bx - (bar_len / 2) * cos_t, by - (bar_len / 2) * sin_t)
                p2 = (bx - half_gap * cos_t, by - half_gap * sin_t)
                p3 = (bx + half_gap * cos_t, by + half_gap * sin_t)
                p4 = (bx + (bar_len / 2) * cos_t, by + (bar_len / 2) * sin_t)
                draw.line([p1, p2], fill=gold_bright, width=5)
                draw.line([p3, p4], fill=gold_bright, width=5)
                
    # Vòng Bát Quái Phù Văn Chữ Nho / Pháp Ấn
    draw.ellipse([CENTER - 260, CENTER - 260, CENTER + 260, CENTER + 260], outline=cyan_energy, width=4)
    draw.ellipse([CENTER - 210, CENTER - 210, CENTER + 210, CENTER + 210], outline=gold_dark, width=3)
    
    # -------------------------------------------------------------
    # 2. TÂM TRẬN THÁI CỰC ĐỒ (Yin-Yang Taiji Symbol)
    # -------------------------------------------------------------
    TAIJI_R = 200
    # Nửa Trắng (Dương / Bạch)
    draw.chord([CENTER - TAIJI_R, CENTER - TAIJI_R, CENTER + TAIJI_R, CENTER + TAIJI_R], 90, 270, fill=(245, 245, 255, 230))
    # Nửa Đen (Âm / Hắc Khí)
    draw.chord([CENTER - TAIJI_R, CENTER - TAIJI_R, CENTER + TAIJI_R, CENTER + TAIJI_R], 270, 90, fill=(25, 25, 35, 230))
    
    # 2 Đường cong nửa đường kính tạo xoáy Thái Cực
    # Phần đầu Dương (Trên): Vòng tròn Trắng nửa bán kính
    draw.ellipse([CENTER - TAIJI_R // 2, CENTER - TAIJI_R, CENTER + TAIJI_R // 2, CENTER], fill=(245, 245, 255, 230))
    # Phần đầu Âm (Dưới): Vòng tròn Đen nửa bán kính
    draw.ellipse([CENTER - TAIJI_R // 2, CENTER, CENTER + TAIJI_R // 2, CENTER + TAIJI_R], fill=(25, 25, 35, 230))
    
    # Mắt Thái Cực (2 Điểm Âm trong Dương & Dương trong Âm)
    # Mắt Đen trên đầu Trắng
    EYE_R = 28
    draw.ellipse([CENTER - EYE_R, CENTER - TAIJI_R // 2 - EYE_R, CENTER + EYE_R, CENTER - TAIJI_R // 2 + EYE_R], fill=(25, 25, 35, 255))
    draw.ellipse([CENTER - 10, CENTER - TAIJI_R // 2 - 10, CENTER + 10, CENTER - TAIJI_R // 2 + 10], fill=cyan_energy)
    # Mắt Trắng trên đầu Đen
    draw.ellipse([CENTER - EYE_R, CENTER + TAIJI_R // 2 - EYE_R, CENTER + EYE_R, CENTER + TAIJI_R // 2 + EYE_R], fill=(245, 245, 255, 255))
    draw.ellipse([CENTER - 10, CENTER + TAIJI_R // 2 - 10, CENTER + 10, CENTER + TAIJI_R // 2 + 10], fill=gold_bright)
    
    # Viền Thái Cực Kim Quang
    draw.ellipse([CENTER - TAIJI_R, CENTER - TAIJI_R, CENTER + TAIJI_R, CENTER + TAIJI_R], outline=gold_bright, width=5)

    # Lưu Texture Decal
    output_dir = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Skills\VFX"
    os.makedirs(output_dir, exist_ok=True)
    out_path = os.path.join(output_dir, "TEX_BatQuai_GroundDecal.png")
    img.save(out_path, "PNG")
    print(f"Successfully generated Bat Quai Ground Decal Texture at: {out_path}")

    # -------------------------------------------------------------
    # 3. Phù Chú Bay Lơ Lửng (Talisman Particle) - 256x256
    # -------------------------------------------------------------
    talisman = Image.new("RGBA", (256, 256), (0, 0, 0, 0))
    tdraw = ImageDraw.Draw(talisman)
    
    # Giấy Phù Vàng Hoàng Chỉ (Yellow Taoist Talisman)
    px0, py0, px1, py1 = 68, 28, 188, 228
    tdraw.rectangle([px0, py0, px1, py1], fill=(240, 200, 45, 255)) # Nền giấy vàng hoàng kim
    tdraw.rectangle([px0, py0, px1, py1], outline=(180, 120, 20, 255), width=3)
    
    # Mực Chu Sa Đỏ Vẽ Phù Chú (Vermilion Ink)
    vermilion = (220, 35, 30, 255)
    # Đỉnh phù chú (Đầu Lôi Đình)
    tdraw.line([(px0 + 30, py0 + 20), (px1 - 30, py0 + 20)], fill=vermilion, width=4)
    tdraw.line([(128, py0 + 20), (128, py0 + 55)], fill=vermilion, width=4)
    tdraw.arc([100, py0 + 40, 156, py0 + 80], 0, 180, fill=vermilion, width=4)
    
    # Thân phù (Triệu Lệnh Phong Lôi Bát Quái)
    for fy in range(py0 + 85, py1 - 30, 18):
        tdraw.line([(105, fy), (151, fy)], fill=vermilion, width=4)
        tdraw.line([(128, fy - 6), (128, fy + 12)], fill=vermilion, width=3)
        
    # Chân phù (Tam Đạo Lôi Hỏa)
    tdraw.line([(110, py1 - 25), (100, py1 - 10)], fill=vermilion, width=4)
    tdraw.line([(128, py1 - 25), (128, py1 - 8)], fill=vermilion, width=4)
    tdraw.line([(146, py1 - 25), (156, py1 - 10)], fill=vermilion, width=4)

    talisman_path = os.path.join(output_dir, "TEX_Taoist_Talisman.png")
    talisman.save(talisman_path, "PNG")
    print(f"Successfully generated Taoist Talisman Texture at: {talisman_path}")

if __name__ == "__main__":
    create_bat_quai_tran_textures()
