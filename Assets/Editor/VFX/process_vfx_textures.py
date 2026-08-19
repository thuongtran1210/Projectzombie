import os
import math
from PIL import Image, ImageDraw, ImageFilter
import numpy as np

SKILLS_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Skills"
os.makedirs(SKILLS_DIR, exist_ok=True)

def process_black_to_alpha(image_path, output_path, black_threshold=0.08, boost=1.5):
    """Chuyển nền đen thành Alpha trong suốt hoàn hảo với cạnh mịn mềm mại."""
    if not os.path.exists(image_path):
        return
    img = Image.open(image_path).convert("RGBA")
    arr = np.array(img, dtype=np.float32) / 255.0
    
    r, g, b, a = arr[:, :, 0], arr[:, :, 1], arr[:, :, 2], arr[:, :, 3]
    
    # Tính độ sáng (Luminance / Max RGB)
    lum = np.maximum(r, np.maximum(g, b))
    
    # Tính alpha mượt mà dựa trên độ sáng
    alpha = np.clip((lum - black_threshold) / (1.0 - black_threshold) * boost, 0.0, 1.0)
    
    # Gán lại alpha
    arr[:, :, 3] = alpha
    
    result = Image.fromarray((arr * 255.0).astype(np.uint8), "RGBA")
    result.save(output_path, "PNG")
    print(f"Processed: {output_path}")

def generate_pro_slash_texture(output_path):
    """Tạo Texture Vệt Chém Thư Họa Hình Trăng Khuyết (Calligraphy Crescent Arc) 100% trong suốt."""
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    center_x, center_y = size // 2, size // 2
    outer_radius = 210
    inner_radius = 160
    
    # Vẽ các dải năng lượng cung trăng khuyết mềm mại
    for angle_deg in range(30, 240):
        rad = math.radians(angle_deg)
        t = (angle_deg - 30) / 210.0  # 0 đến 1
        
        # Độ dày vuốt nhọn 2 đầu
        thickness = math.sin(t * math.pi)
        if thickness <= 0: continue
        
        r_out = inner_radius + (outer_radius - inner_radius) * (0.8 + 0.2 * thickness)
        r_in = inner_radius - 20 * thickness
        
        # Tọa độ
        x_out = center_x + r_out * math.cos(rad)
        y_out = center_y + r_out * math.sin(rad)
        x_in = center_x + r_in * math.cos(rad)
        y_in = center_y + r_in * math.sin(rad)
        
        # Màu vàng kim cổ phong & Lõi sáng trắng
        alpha = int(255 * thickness)
        # Gradient từ vàng kim sang cam đỏ
        r_c = 255
        g_c = int(200 + 55 * thickness)
        b_c = int(100 * (1.0 - thickness))
        
        draw.line([(x_in, y_in), (x_out, y_out)], fill=(r_c, g_c, b_c, alpha), width=3)
    
    # Lõi sáng trắng rực rỡ ở giữa cung chém
    for angle_deg in range(60, 210):
        rad = math.radians(angle_deg)
        t = (angle_deg - 60) / 150.0
        thickness = math.sin(t * math.pi)
        r_mid = (inner_radius + outer_radius) / 2.0
        
        x = center_x + r_mid * math.cos(rad)
        y = center_y + r_mid * math.sin(rad)
        
        alpha = int(255 * thickness)
        draw.ellipse([x - 4*thickness, y - 4*thickness, x + 4*thickness, y + 4*thickness], fill=(255, 255, 240, alpha))
        
    img = img.filter(ImageFilter.GaussianBlur(radius=1.5))
    img.save(output_path, "PNG")
    print(f"Generated Pro Slash Texture: {output_path}")

def generate_pro_spark_streak(output_path):
    """Tạo hạt tia sáng vuốt nhọn (Stretched Spark Streak) trong suốt cho Particle tóe lực."""
    w, h = 256, 64
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cy = h // 2
    # Vẽ tia sáng elip vuốt nhọn 2 đầu
    for x in range(w):
        t = x / float(w) # 0 to 1
        factor = math.sin(t * math.pi) # 0 to 1 to 0
        half_height = (h // 2 - 2) * (factor ** 2)
        
        alpha = int(255 * factor)
        r_c = 255
        g_c = int(220 * factor + 35)
        b_c = int(120 * factor)
        
        draw.line([(x, cy - half_height), (x, cy + half_height)], fill=(r_c, g_c, b_c, alpha), width=1)
        
        # Lõi trắng ở giữa
        core_h = half_height * 0.4
        draw.line([(x, cy - core_h), (x, cy + core_h)], fill=(255, 255, 255, alpha), width=1)
        
    img = img.filter(ImageFilter.GaussianBlur(radius=1.0))
    img.save(output_path, "PNG")
    print(f"Generated Spark Streak: {output_path}")

def generate_circular_ground_crack(output_path):
    """Tạo Decal Vết Nứt Đất Tròn Mờ Dần (Không Bị Ô Vuông)."""
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    center = size // 2
    # Vẽ các nhánh nứt tỏa tròn từ tâm
    np.random.seed(42)
    num_cracks = 12
    for _ in range(num_cracks):
        angle = np.random.uniform(0, 2 * math.pi)
        curr_x, curr_y = center, center
        max_dist = np.random.uniform(120, 220)
        dist = 0
        
        while dist < max_dist:
            step = np.random.uniform(8, 20)
            angle += np.random.uniform(-0.3, 0.3)
            dist += step
            
            next_x = curr_x + step * math.cos(angle)
            next_y = curr_y + step * math.sin(angle)
            
            t = dist / max_dist
            alpha = int(255 * (1.0 - t))
            width = max(1, int(4 * (1.0 - t)))
            
            draw.line([(curr_x, curr_y), (next_x, next_y)], fill=(255, 180, 50, alpha), width=width)
            curr_x, curr_y = next_x, next_y

    # Vòng tròn năng lượng ở tâm mờ dần ra mép
    for r in range(160, 0, -2):
        alpha = int(60 * (1.0 - r / 160.0))
        draw.ellipse([center - r, center - r, center + r, center + r], fill=(255, 120, 20, alpha))
        
    img = img.filter(ImageFilter.GaussianBlur(radius=2.0))
    img.save(output_path, "PNG")
    print(f"Generated Circular Ground Crack Decal: {output_path}")

if __name__ == "__main__":
    # 1. Tách nền các ảnh đã sinh nếu có
    ink_arc_in = os.path.join(SKILLS_DIR, "InkSlash_Arc.png")
    process_black_to_alpha(ink_arc_in, ink_arc_in)
    
    fox_flame_in = os.path.join(SKILLS_DIR, "FoxFlame_Stream.png")
    process_black_to_alpha(fox_flame_in, fox_flame_in)
    
    # 2. Sinh các Texture Texture chất lượng cao trong suốt
    pro_slash = os.path.join(SKILLS_DIR, "Pro_InkSlash_Arc.png")
    generate_pro_slash_texture(pro_slash)
    
    spark_streak = os.path.join(SKILLS_DIR, "Spark_Streak.png")
    generate_pro_spark_streak(spark_streak)
    
    ground_crack = os.path.join(SKILLS_DIR, "Decal_Cracked_Circle.png")
    generate_circular_ground_crack(ground_crack)
