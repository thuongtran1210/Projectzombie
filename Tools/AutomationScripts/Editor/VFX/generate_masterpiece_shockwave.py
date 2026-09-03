import os
import math
from PIL import Image, ImageDraw, ImageFilter
import numpy as np

ART_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\VFX"
os.makedirs(ART_DIR, exist_ok=True)

def generate_masterpiece_oracle_shockwave():
    """
    Tạo Texture Sóng Xung Kích Khí Ba (High-Energy Anime Shockwave / Sonic Wave) 512x512.
    - Mép ngoài sắc nét, nén khí siêu cao (Sharp Pure White Rim).
    - Thân sóng chuyển sắc vàng kim linh lực (Golden Energy Falloff).
    - Các đường vân sóng phụ đồng tâm (Layered Sonic Ripples) tạo độ sâu điện ảnh.
    """
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    
    # Tạo bằng ma trận Numpy để độ chuyển màu và độ mảnh sắc đạt chuẩn tuyệt đối
    arr = np.zeros((size, size, 4), dtype=np.float32)
    cx, cy = size / 2.0, size / 2.0
    
    y_coords, x_coords = np.ogrid[:size, :size]
    dist_from_center = np.sqrt((x_coords - cx) ** 2 + (y_coords - cy) ** 2)
    
    # Bán kính ngoài cùng của sóng (Radius = 240 px)
    r_max = 240.0
    r_thickness = 55.0 # Bề dày thân sóng chuyển dần
    
    # Chuẩn hóa khoảng cách từ mép sóng
    # dist = 0 tại r_max, dist > 0 khi đi vào trong
    inward_dist = r_max - dist_from_center
    
    # 1. Mép ngoài chính (Primary High-Energy Shockwave Crest)
    # Rất mỏng, dốc đứng ở mép ngoài (Steep Shock Front)
    mask_main = (dist_from_center <= r_max) & (inward_dist >= 0) & (inward_dist <= r_thickness)
    
    # Dạng hàm sóng khí nén: Tăng vọt tức thì ở mép trước, giảm dần theo hàm mũ vào trong
    decay = np.exp(-inward_dist / 14.0)
    
    # 2. Vân sóng phụ 1 (Sub-ripple 1 tại inward_dist = 18px)
    sub1 = np.exp(-((inward_dist - 18.0) ** 2) / (2.0 * (3.5 ** 2))) * 0.75
    
    # 3. Vân sóng phụ 2 (Sub-ripple 2 tại inward_dist = 34px)
    sub2 = np.exp(-((inward_dist - 34.0) ** 2) / (2.0 * (4.5 ** 2))) * 0.45
    
    total_intensity = decay + sub1 + sub2
    total_intensity = np.clip(total_intensity, 0.0, 1.0)
    
    # Phối màu Anime Cổ Phong:
    # Lõi sắc nhọn = Trắng tinh (255, 255, 255)
    # Thân sóng = Vàng Hoàng Kim (255, 220, 80)
    # Đuôi mờ = Cam Linh Lực (255, 140, 30)
    for y in range(size):
        for x in range(size):
            if mask_main[y, x]:
                val = total_intensity[y, x]
                d = inward_dist[y, x]
                
                # Mép ngoài cùng (0-4px) rực trắng
                if d <= 4.0:
                    edge_factor = d / 4.0
                    r = 255
                    g = 255
                    b = int(255 * (1.0 - edge_factor) + 220 * edge_factor)
                    a = int(255 * val)
                elif d <= 22.0:
                    t = (d - 4.0) / 18.0
                    r = 255
                    g = int(220 * (1.0 - t) + 160 * t)
                    b = int(80 * (1.0 - t) + 30 * t)
                    a = int(255 * val)
                else:
                    t = (d - 22.0) / (r_thickness - 22.0)
                    r = 255
                    g = int(160 * (1.0 - t) + 80 * t)
                    b = int(30 * (1.0 - t) + 10 * t)
                    a = int(255 * val * (1.0 - t))
                
                arr[y, x] = [r, g, b, a]

    out_img = Image.fromarray(arr.astype(np.uint8), "RGBA")
    
    # Áp dụng Gaussian Blur siêu nhẹ 0.4px để khử răng cưa
    out_img = out_img.filter(ImageFilter.GaussianBlur(radius=0.4))
    
    out_path = os.path.join(ART_DIR, "Tex_VFX_Oracle_Shockwave.png")
    out_img.save(out_path, "PNG")
    print(f"Generated Masterpiece Shockwave: {out_path}")

if __name__ == "__main__":
    generate_masterpiece_oracle_shockwave()
