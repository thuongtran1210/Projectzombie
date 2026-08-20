import os
import math
from PIL import Image, ImageDraw, ImageFilter
import numpy as np

ART_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\VFX"
os.makedirs(ART_DIR, exist_ok=True)

def generate_dong_son_sonic_wave():
    """
    Sinh Texture Sóng Âm Trống Đồng Đông Sơn (Acoustic Sonic Resonance Wave) 1024x1024:
    - 4 Tầng gợn sóng âm đồng tâm thanh mảnh, sắc bén (Multi-layered Thin Sonic Ripples).
    - Vành sao mặt trời 14 cánh Đông Sơn sắc nét ở tâm.
    - Họa tiết chấm dải hạt và vòng tròn đồng tâm truyền thống.
    - Phối màu Vàng Đồng Thau Cổ (#E5A93C, #FFD700) kết hợp ánh sáng nén khí siêu thanh (#FFF5CC).
    """
    size = 1024
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size / 2.0, size / 2.0
    
    # 1. Vẽ các vòng sóng âm đồng tâm (Concentric Acoustic Shockwave Rings)
    # Tầng ngoài cùng: Vành khí nén siêu thanh mỏng sắc nét (Radius 460 - 480)
    # Tầng giữa: Vành sóng âm chính với họa tiết rãnh răng cưa Đông Sơn (Radius 360 - 410)
    # Tầng trong 1: Gợn sóng dội âm (Radius 260 - 290)
    # Tầng trong 2: Vành hào quang mặt trời 14 cánh (Radius 120 - 200)

    # Sử dụng numpy để vẽ các vòng sóng âm gradient siêu mượt, không bị dày bệt
    y_coords, x_coords = np.ogrid[:size, :size]
    dist = np.sqrt((x_coords - cx) ** 2 + (y_coords - cy) ** 2)
    angle = np.arctan2(y_coords - cy, x_coords - cx) # [-pi, pi]
    
    # Ma trận RGBA float32
    arr = np.zeros((size, size, 4), dtype=np.float32)
    
    # --- RIPPLE 1: Vành Sóng Xung Kích Trước (Leading Sonic Wavefront) tại R = 460px ---
    # Bề dày chỉ 16px, dốc đứng ở mép ngoài (Sharp shock front)
    r1 = 460.0
    w1 = 18.0
    d1 = np.abs(dist - r1)
    mask1 = d1 <= w1
    intensity1 = np.exp(-((d1) ** 2) / (2.0 * (5.0 ** 2))) * (1.0 + 0.15 * np.cos(14 * angle))
    
    # --- RIPPLE 2: Vành Sóng Âm Trống Đồng Chính (Main Dong Son Resonant Ring) tại R = 380px ---
    # Chứa 14 nhịp sóng điều chế biên độ (14-lobe amplitude modulation)
    r2 = 385.0
    w2 = 24.0
    d2 = np.abs(dist - r2)
    mod2 = 0.75 + 0.25 * np.cos(14 * angle) # 14 đỉnh sóng âm
    intensity2 = np.exp(-((d2) ** 2) / (2.0 * (7.0 ** 2))) * mod2 * 0.9
    
    # --- RIPPLE 3: Vành Sóng Dội Âm (Harmonic Echo Ring) tại R = 300px ---
    r3 = 305.0
    d3 = np.abs(dist - r3)
    intensity3 = np.exp(-((d3) ** 2) / (2.0 * (6.0 ** 2))) * 0.7
    
    # --- RIPPLE 4: Vành Sóng Âm Nội Tuần Hoàn (Inner Sub-harmonic Ring) tại R = 220px ---
    r4 = 225.0
    d4 = np.abs(dist - r4)
    intensity4 = np.exp(-((d4) ** 2) / (2.0 * (5.5 ** 2))) * 0.55
    
    # --- TÂM MẶT TRỜI ĐÔNG SƠN 14 CÁNH (14-Pointed Solar Star Core) tại R <= 160px ---
    r_core = 160.0
    star_mod = np.cos(14 * angle) # 14 cánh nhọn
    star_r = 60.0 + 90.0 * np.maximum(0, star_mod)
    mask_star = dist <= star_r
    intensity_star = np.clip((1.0 - (dist / star_r)), 0, 1) ** 1.8 * 0.85

    # Tổng hợp các lớp cường độ sóng âm
    total_intensity = np.maximum.reduce([intensity1, intensity2, intensity3, intensity4, intensity_star])
    total_intensity = np.clip(total_intensity, 0.0, 1.0)
    
    # Phối màu chuẩn Mỹ Thuật Đông Sơn - Anime URP:
    # Lõi sáng nén khí = Trắng vàng kim (#FFFBE6)
    # Thân sóng = Vàng Đồng Thau Hoàng Kim (#FFD043)
    # Rãnh viền trầm = Đồng hun cổ (#C88A28)
    for y in range(size):
        for x in range(size):
            val = total_intensity[y, x]
            if val > 0.01:
                # Phối màu theo cường độ năng lượng sóng âm
                r = int(np.clip(255 * (0.85 + 0.15 * val), 0, 255))
                g = int(np.clip(180 + 75 * val, 0, 255))
                b = int(np.clip(35 + 200 * (val ** 2), 0, 255))
                a = int(np.clip(255 * val * 0.95, 0, 255))
                arr[y, x] = [r, g, b, a]
                
    base_img = Image.fromarray(arr.astype(np.uint8), "RGBA")
    
    # 2. Vẽ thêm các chi tiết viền nét khắc kim loại Đông Sơn siêu mảnh (Fine Vector Overlay)
    overlay = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    overlay_draw = ImageDraw.Draw(overlay)
    
    gold_sharp = (255, 235, 160, 230)
    gold_mid = (255, 205, 70, 190)
    gold_dim = (210, 145, 40, 150)
    
    # Vẽ các đường rãnh khắc vòng tròn đồng tâm siêu mảnh (1-2px)
    for radius, color, width in [(460, gold_sharp, 2), (385, gold_mid, 2), (305, gold_dim, 2), (225, gold_dim, 2)]:
        overlay_draw.ellipse([cx - radius, cy - radius, cx + radius, cy + radius], outline=color, width=width)
        
    # Vẽ các chấm dải hạt cườm âm hưởng quanh vành R = 345
    num_dots = 56
    dot_r = 345
    for i in range(num_dots):
        a_rad = i * (2 * math.pi / num_dots)
        dx = cx + dot_r * math.cos(a_rad)
        dy = cy + dot_r * math.sin(a_rad)
        overlay_draw.ellipse([dx - 2.5, dy - 2.5, dx + 2.5, dy + 2.5], fill=(255, 240, 180, 200))
        
    # Ghép lớp vector chi tiết lên ma trận gradient
    final_img = Image.alpha_composite(base_img, overlay)
    
    # Khử răng cưa nhẹ
    final_img = final_img.filter(ImageFilter.GaussianBlur(radius=0.3))
    
    out_path = os.path.join(ART_DIR, "Tex_VFX_DongSon_SonicWave.png")
    final_img.save(out_path, "PNG")
    print(f"Generated Dong Son Sonic Wave Texture: {out_path}")

if __name__ == "__main__":
    generate_dong_son_sonic_wave()
