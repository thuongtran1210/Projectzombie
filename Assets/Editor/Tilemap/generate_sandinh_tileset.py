import os
import math
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

TILE_SIZE = 64
TILES_X = 8
TILES_Y = 8
ATLAS_WIDTH = TILE_SIZE * TILES_X  # 512
ATLAS_HEIGHT = TILE_SIZE * TILES_Y # 512

def create_tileset():
    # 512x512 RGBA canvas
    img = Image.new("RGBA", (ATLAS_WIDTH, ATLAS_HEIGHT), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    def tile_rect(col, row):
        return (col * TILE_SIZE, row * TILE_SIZE, (col + 1) * TILE_SIZE, (row + 1) * TILE_SIZE)

    # -------------------------------------------------------------
    # 1. GẠCH BÁT TRÀNG (Ground Tiles) - Row 0 & 1 (Cols 0..3)
    # -------------------------------------------------------------
    base_brick = (180, 72, 42, 255)       # Đỏ gạch nung Bát Tràng
    brick_shadow = (130, 48, 28, 255)     # Viền mạch vữa đậm
    brick_highlight = (210, 95, 60, 255)  # Vân sáng gạch cổ
    mortar_color = (75, 60, 52, 255)      # Mạch vữa đất cổ

    for r in range(2):
        for c in range(4):
            x0, y0, x1, y1 = tile_rect(c, r)
            draw.rectangle([x0, y0, x1 - 1, y1 - 1], fill=mortar_color)
            
            sub_w = 30
            sub_h = 30
            gap = 2
            
            for sub_y in range(2):
                for sub_x in range(2):
                    bx0 = x0 + gap + sub_x * (sub_w + gap)
                    by0 = y0 + gap + sub_y * (sub_h + gap)
                    bx1 = bx0 + sub_w - 1
                    by1 = by0 + sub_h - 1
                    
                    var = (c * 7 + r * 13 + sub_x * 5 + sub_y * 11) % 15 - 7
                    col_cur = (
                        max(0, min(255, base_brick[0] + var * 2)),
                        max(0, min(255, base_brick[1] + var)),
                        max(0, min(255, base_brick[2] + var)),
                        255
                    )
                    draw.rectangle([bx0, by0, bx1, by1], fill=col_cur)
                    draw.line([(bx0, by0), (bx1, by0)], fill=brick_highlight, width=1)
                    draw.line([(bx0, by0), (bx0, by1)], fill=brick_highlight, width=1)
                    draw.line([(bx0, by1), (bx1, by1)], fill=brick_shadow, width=1)
                    draw.line([(bx1, by0), (bx1, by1)], fill=brick_shadow, width=1)

            if (c, r) == (1, 0): # Rêu bám nhẹ
                draw.ellipse([x0 + 10, y0 + 10, x0 + 26, y0 + 26], fill=(50, 95, 45, 180))
                draw.ellipse([x0 + 40, y0 + 35, x0 + 55, y0 + 50], fill=(60, 110, 50, 160))
            elif (c, r) == (2, 0): # Nứt nẻ cổ kính
                draw.line([(x0 + 15, y0 + 8), (x0 + 22, y0 + 25), (x0 + 18, y0 + 40)], fill=(50, 30, 20, 240), width=1)
            elif (c, r) == (3, 0): # Đọng nước mưa bóng
                draw.ellipse([x0 + 20, y0 + 20, x0 + 45, y0 + 45], fill=(30, 60, 80, 140))

    # -------------------------------------------------------------
    # 2. BỜ AO SEN & NƯỚC (Water & Lotus Pond) - Row 0..2 (Cols 4..7)
    # -------------------------------------------------------------
    water_deep = (18, 52, 65, 255)       # Xanh lục thủy thẩm
    water_mid = (28, 85, 98, 255)        # Xanh ngọc ao làng
    water_wave = (65, 145, 155, 220)     # Gợn sóng phản chiếu
    stone_edge = (95, 90, 80, 255)       # Đá xanh kè bờ ao
    stone_highlight = (140, 135, 120, 255)

    # Pure water tile (Col 4, Row 0)
    x0, y0, x1, y1 = tile_rect(4, 0)
    draw.rectangle([x0, y0, x1 - 1, y1 - 1], fill=water_mid)
    for wy in range(y0 + 8, y1 - 8, 14):
        draw.arc([x0 + 8, wy, x0 + 32, wy + 8], 0, 180, fill=water_wave, width=2)
        draw.arc([x0 + 34, wy + 4, x0 + 58, wy + 12], 0, 180, fill=water_wave, width=2)

    # Water with Lotus Leaves (Col 5, Row 0)
    x0, y0, x1, y1 = tile_rect(5, 0)
    draw.rectangle([x0, y0, x1 - 1, y1 - 1], fill=water_mid)
    draw.ellipse([x0 + 12, y0 + 12, x0 + 44, y0 + 44], fill=(34, 120, 52, 255))
    draw.ellipse([x0 + 16, y0 + 16, x0 + 40, y0 + 40], fill=(45, 150, 68, 255))
    draw.line([(x0 + 28, y0 + 28), (x0 + 44, y0 + 28)], fill=water_deep, width=2)
    draw.ellipse([x0 + 36, y0 + 10, x0 + 52, y0 + 26], fill=(235, 110, 145, 255))
    draw.ellipse([x0 + 40, y0 + 14, x0 + 48, y0 + 22], fill=(255, 215, 0, 255))

    # Bờ kè đá ao sen (Border Tiles for Rule Tile)
    # Top edge (Col 6, Row 0)
    x0, y0, x1, y1 = tile_rect(6, 0)
    draw.rectangle([x0, y0, x1 - 1, y1 - 1], fill=water_mid)
    draw.rectangle([x0, y0, x1 - 1, y0 + 20], fill=stone_edge)
    draw.line([(x0, y0 + 20), (x1 - 1, y0 + 20)], fill=(40, 40, 35, 255), width=2)
    draw.line([(x0, y0), (x1 - 1, y0)], fill=stone_highlight, width=2)

    # Corner Edge (Col 7, Row 0)
    x0, y0, x1, y1 = tile_rect(7, 0)
    draw.rectangle([x0, y0, x1 - 1, y1 - 1], fill=water_mid)
    draw.polygon([(x0, y0), (x1 - 1, y0), (x1 - 1, y0 + 30), (x0 + 30, y1 - 1), (x0, y1 - 1)], fill=stone_edge)
    draw.line([(x1 - 1, y0 + 30), (x0 + 30, y1 - 1)], fill=(40, 40, 35, 255), width=2)

    # -------------------------------------------------------------
    # 3. TƯỜNG RÊU & VẬT CẢN (Mossy Walls & Props) - Row 2..5
    # -------------------------------------------------------------
    wall_brick_dark = (110, 45, 30, 255)
    wall_moss = (55, 105, 45, 255)
    wall_moss_bright = (85, 150, 60, 255)
    roof_tile_color = (60, 30, 25, 255)
    roof_trim_gold = (200, 150, 50, 255)

    # Wall Top (Col 0, Row 2)
    x0, y0, x1, y1 = tile_rect(0, 2)
    draw.rectangle([x0, y0, x1 - 1, y0 + 24], fill=roof_tile_color)
    draw.line([(x0, y0 + 2), (x1 - 1, y0 + 2)], fill=roof_trim_gold, width=2)
    for rx in range(x0 + 4, x1 - 4, 12):
        draw.arc([rx, y0 + 6, rx + 12, y0 + 22], 0, 180, fill=(90, 50, 40, 255), width=2)
    draw.rectangle([x0, y0 + 24, x1 - 1, y1 - 1], fill=wall_brick_dark)
    draw.polygon([(x0, y1 - 1), (x0 + 16, y1 - 18), (x0 + 32, y1 - 8), (x0 + 48, y1 - 22), (x1 - 1, y1 - 1)], fill=wall_moss)
    draw.polygon([(x0 + 4, y1 - 1), (x0 + 16, y1 - 12), (x0 + 28, y1 - 4), (x1 - 1, y1 - 1)], fill=wall_moss_bright)

    # Wall Mid / Column (Col 1, Row 2)
    x0, y0, x1, y1 = tile_rect(1, 2)
    draw.rectangle([x0, y0, x1 - 1, y1 - 1], fill=wall_brick_dark)
    for wy in range(y0 + 12, y1, 14):
        draw.line([(x0, wy), (x1 - 1, wy)], fill=(50, 20, 15, 255), width=2)
    draw.polygon([(x0, y1 - 1), (x0 + 20, y1 - 30), (x0 + 40, y1 - 10), (x1 - 1, y1 - 25), (x1 - 1, y1 - 1)], fill=wall_moss)

    # Bia Đá / Rùa Đội Bia Cổ (Ancient Stone Stela Prop)
    # Bottom turtle base (Col 2, Row 3)
    x0, y0, x1, y1 = tile_rect(2, 3)
    draw.ellipse([x0 + 8, y0 + 24, x1 - 8, y1 - 6], fill=(85, 90, 85, 255))
    draw.ellipse([x0 + 24, y0 + 12, x0 + 40, y0 + 28], fill=(70, 75, 70, 255))
    draw.line([(x0 + 12, y1 - 12), (x1 - 12, y1 - 12)], fill=(40, 45, 40, 255), width=2)

    # Top Stela (Col 2, Row 2)
    x0, y0, x1, y1 = tile_rect(2, 2)
    draw.rectangle([x0 + 16, y0 + 8, x1 - 16, y1 - 1], fill=(120, 125, 115, 255))
    draw.arc([x0 + 16, y0 + 4, x1 - 16, y0 + 24], 180, 360, fill=(150, 155, 145, 255), width=2)
    for gy in range(y0 + 24, y1 - 8, 8):
        draw.line([(x0 + 24, gy), (x0 + 40, gy)], fill=(60, 65, 58, 255), width=2)

    # Lư Hương Đồng / Đỉnh Đồng Cổ
    # Chân Đỉnh (Col 4, Row 3)
    x0, y0, x1, y1 = tile_rect(4, 3)
    draw.polygon([(x0 + 14, y0 + 4), (x1 - 14, y0 + 4), (x0 + 20, y1 - 10), (x1 - 20, y1 - 10)], fill=(160, 120, 40, 255))
    draw.ellipse([x0 + 16, y0 + 2, x1 - 16, y0 + 20], fill=(130, 95, 30, 255))
    # Thân & Khói Trầm (Col 4, Row 2)
    x0, y0, x1, y1 = tile_rect(4, 2)
    draw.ellipse([x0 + 14, y0 + 36, x1 - 14, y1 - 8], fill=(190, 145, 45, 255))
    draw.line([(x0 + 12, y0 + 42), (x0 + 6, y0 + 30)], fill=(150, 110, 35, 255), width=3)
    draw.line([(x1 - 12, y0 + 42), (x1 - 6, y0 + 30)], fill=(150, 110, 35, 255), width=3)
    draw.arc([x0 + 24, y0 + 12, x0 + 40, y0 + 32], 0, 180, fill=(220, 220, 240, 160), width=2)
    draw.arc([x0 + 20, y0 + 2, x0 + 36, y0 + 18], 180, 360, fill=(220, 220, 240, 120), width=2)

    # Đèn Lồng Đỏ Treo Đình (Col 5, Row 2)
    x0, y0, x1, y1 = tile_rect(5, 2)
    draw.line([(x0 + 32, y0), (x0 + 32, y0 + 16)], fill=(50, 40, 30, 255), width=2)
    draw.ellipse([x0 + 18, y0 + 16, x1 - 18, y1 - 14], fill=(220, 38, 38, 255))
    draw.ellipse([x0 + 24, y0 + 18, x1 - 24, y1 - 16], fill=(255, 90, 60, 255))
    draw.line([(x0 + 32, y1 - 14), (x0 + 32, y1 - 2)], fill=(212, 175, 55, 255), width=3)

    output_dir = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Tilemaps\SanDinhLangCo"
    os.makedirs(output_dir, exist_ok=True)
    out_path = os.path.join(output_dir, "Tileset_SanDinhLangCo.png")
    img.save(out_path, "PNG")
    print(f"Successfully generated tileset at: {out_path}")

if __name__ == "__main__":
    create_tileset()
