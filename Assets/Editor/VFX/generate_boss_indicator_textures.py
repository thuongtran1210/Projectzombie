import os
import math
from PIL import Image, ImageDraw, ImageFilter

def create_indicator_textures():
    output_dir = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\VFX\Indicators"
    os.makedirs(output_dir, exist_ok=True)

    # -------------------------------------------------------------
    # 1. TEX_Indicator_Circle_Border.png (Vòng tròn cảnh báo AOE Cổ Phong) - 512x512
    # -------------------------------------------------------------
    size = 512
    cx, cy = size // 2, size // 2
    r_outer = 240
    r_inner = 215

    img_circle = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw_c = ImageDraw.Draw(img_circle)

    # Viền chu sa đỏ phát sáng (Vermilion Red & Crimson Blood)
    red_bright = (255, 45, 45, 255)
    red_dark = (160, 20, 20, 255)
    red_glow = (255, 90, 60, 220)
    bg_tint = (220, 30, 30, 45) # Nền mờ cảnh báo

    # Nền mờ bên trong vùng nguy hiểm
    draw_c.ellipse([cx - r_outer, cy - r_outer, cx + r_outer, cy + r_outer], fill=bg_tint)

    # Vòng tròn đôi ngoài
    draw_c.ellipse([cx - r_outer, cy - r_outer, cx + r_outer, cy + r_outer], outline=red_bright, width=5)
    draw_c.ellipse([cx - (r_outer - 12), cy - (r_outer - 12), cx + (r_outer - 12), cy + (r_outer - 12)], outline=red_dark, width=2)
    draw_c.ellipse([cx - r_inner, cy - r_inner, cx + r_inner, cy + r_inner], outline=red_glow, width=3)

    # 16 Răng cưa cảnh báo nguy hiểm hình tam giác quanh viền
    for i in range(16):
        angle = math.radians(i * 22.5)
        p_tip = (cx + (r_outer - 2) * math.cos(angle), cy + (r_outer - 2) * math.sin(angle))
        angle_l = math.radians(i * 22.5 - 4)
        angle_r = math.radians(i * 22.5 + 4)
        p_l = (cx + (r_outer - 16) * math.cos(angle_l), cy + (r_outer - 16) * math.sin(angle_l))
        p_r = (cx + (r_outer - 16) * math.cos(angle_r), cy + (r_outer - 16) * math.sin(angle_r))
        draw_c.polygon([p_tip, p_l, p_r], fill=red_bright)

    # Tâm cảnh báo chữ thập
    draw_c.line([(cx - 20, cy), (cx + 20, cy)], fill=red_bright, width=3)
    draw_c.line([(cx, cy - 20), (cx, cy + 20)], fill=red_bright, width=3)
    draw_c.ellipse([cx - 8, cy - 8, cx + 8, cy + 8], outline=red_bright, width=2)

    circle_path = os.path.join(output_dir, "TEX_Indicator_Circle.png")
    img_circle.save(circle_path, "PNG")
    print(f"Generated Circle Indicator at: {circle_path}")

    # -------------------------------------------------------------
    # 2. TEX_Indicator_Box_Border.png (Đường thẳng húc / Vệt càn quét 9-Slice) - 256x256
    # -------------------------------------------------------------
    img_box = Image.new("RGBA", (256, 256), (0, 0, 0, 0))
    draw_b = ImageDraw.Draw(img_box)

    # Nền mờ đỏ
    draw_b.rectangle([6, 6, 250, 250], fill=(220, 30, 30, 45))
    # Viền ngoài đôi
    draw_b.rectangle([6, 6, 250, 250], outline=red_bright, width=4)
    draw_b.rectangle([12, 12, 244, 244], outline=red_dark, width=2)

    # Mũi tên cảnh báo hướng lao tới ở giữa (Arrow Chevrons)
    for y_offset in [50, 110, 170]:
        draw_b.line([(60, y_offset + 25), (128, y_offset), (196, y_offset + 25)], fill=red_bright, width=5)
        draw_b.line([(60, y_offset + 35), (128, y_offset + 10), (196, y_offset + 35)], fill=red_dark, width=3)

    box_path = os.path.join(output_dir, "TEX_Indicator_Box.png")
    img_box.save(box_path, "PNG")
    print(f"Generated Box Indicator at: {box_path}")

    # -------------------------------------------------------------
    # 3. TEX_Indicator_Fill_Disc.png (Đĩa đỏ lấp đầy nở từ tâm) - 256x256
    # -------------------------------------------------------------
    img_fill = Image.new("RGBA", (256, 256), (0, 0, 0, 0))
    draw_f = ImageDraw.Draw(img_fill)
    fcx, fcy = 128, 128
    draw_f.ellipse([10, 10, 246, 246], fill=(255, 50, 50, 210))
    draw_f.ellipse([10, 10, 246, 246], outline=(255, 180, 160, 255), width=4)
    fill_path = os.path.join(output_dir, "TEX_Indicator_Fill.png")
    img_fill.save(fill_path, "PNG")
    print(f"Generated Fill Disc Indicator at: {fill_path}")

if __name__ == "__main__":
    create_indicator_textures()
