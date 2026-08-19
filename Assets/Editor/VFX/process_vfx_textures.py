import os
import math
from PIL import Image, ImageDraw, ImageFilter
import numpy as np

SKILLS_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Skills"
os.makedirs(SKILLS_DIR, exist_ok=True)

def generate_pro_slash_texture(output_path):
    """Tạo Texture Vệt Chém Thư Họa Hình Trăng Khuyết (Calligraphy Crescent Arc) 100% trong suốt."""
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    center_x, center_y = size // 2, size // 2
    outer_radius = 210
    inner_radius = 160
    
    for angle_deg in range(30, 240):
        rad = math.radians(angle_deg)
        t = (angle_deg - 30) / 210.0
        thickness = math.sin(t * math.pi)
        if thickness <= 0: continue
        
        r_out = inner_radius + (outer_radius - inner_radius) * (0.8 + 0.2 * thickness)
        r_in = inner_radius - 20 * thickness
        
        x_out = center_x + r_out * math.cos(rad)
        y_out = center_y + r_out * math.sin(rad)
        x_in = center_x + r_in * math.cos(rad)
        y_in = center_y + r_in * math.sin(rad)
        
        alpha = int(255 * thickness)
        r_c = 255
        g_c = int(200 + 55 * thickness)
        b_c = int(100 * (1.0 - thickness))
        
        draw.line([(x_in, y_in), (x_out, y_out)], fill=(r_c, g_c, b_c, alpha), width=3)
    
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
    """Tạo hạt tia sáng vuốt nhọn (Stretched Spark Streak) trong suốt."""
    w, h = 256, 64
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cy = h // 2
    for x in range(w):
        t = x / float(w)
        factor = math.sin(t * math.pi)
        half_height = (h // 2 - 2) * (factor ** 2)
        
        alpha = int(255 * factor)
        r_c = 255
        g_c = int(220 * factor + 35)
        b_c = int(120 * factor)
        
        draw.line([(x, cy - half_height), (x, cy + half_height)], fill=(r_c, g_c, b_c, alpha), width=1)
        
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

    for r in range(160, 0, -2):
        alpha = int(60 * (1.0 - r / 160.0))
        draw.ellipse([center - r, center - r, center + r, center + r], fill=(255, 120, 20, alpha))
        
    img = img.filter(ImageFilter.GaussianBlur(radius=2.0))
    img.save(output_path, "PNG")
    print(f"Generated Circular Ground Crack Decal: {output_path}")

def generate_talisman_ribbon_trail(output_path):
    """Tạo Dải Lụa Phát Sáng (Trail Ribbon) Cho Bùa Trấn Yêu W003."""
    w, h = 256, 64
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cy = h // 2
    for x in range(w):
        t = x / float(w) # Đầu đến đuôi
        factor = math.sin(t * math.pi * 0.5) # Vuốt mỏng về đuôi
        half_height = (h // 2 - 2) * factor
        
        alpha = int(255 * factor)
        # Vàng kim hoàng gia sang cam đỏ
        draw.line([(x, cy - half_height), (x, cy + half_height)], fill=(255, 215, 0, alpha), width=1)
        
        # Lõi sáng trắng ở giữa
        core_h = half_height * 0.4
        draw.line([(x, cy - core_h), (x, cy + core_h)], fill=(255, 255, 255, alpha), width=1)
        
    img = img.filter(ImageFilter.GaussianBlur(radius=1.5))
    img.save(output_path, "PNG")
    print(f"Generated Talisman Ribbon Trail: {output_path}")

def generate_batquai_wind_vortex(output_path):
    """Tạo Vòng Xoáy Gió Lốc Bát Quái Cho Phi Tiêu W012."""
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size // 2, size // 2
    num_arms = 4
    for arm in range(num_arms):
        base_angle = arm * (2 * math.pi / num_arms)
        for i in range(150):
            t = i / 150.0
            r = 30 + t * 200
            angle = base_angle + t * math.pi * 1.5
            
            x = cx + r * math.cos(angle)
            y = cy + r * math.sin(angle)
            
            alpha = int(220 * math.sin(t * math.pi))
            radius_dot = int(2 + 6 * math.sin(t * math.pi))
            # Xanh băng bạch kim
            draw.ellipse([x - radius_dot, y - radius_dot, x + radius_dot, y + radius_dot], fill=(180, 240, 255, alpha))
            
    img = img.filter(ImageFilter.GaussianBlur(radius=2.0))
    img.save(output_path, "PNG")
    print(f"Generated Bat Quai Wind Vortex: {output_path}")

def generate_repulsion_pulse_ring(output_path):
    """Tạo Vòng Sóng Đẩy Lùi Linh Khí (Pulse Ring) 100% trong suốt."""
    size = 256
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size // 2, size // 2
    r_outer = 110
    r_inner = 85
    
    for r in range(r_inner, r_outer):
        t = (r - r_inner) / float(r_outer - r_inner)
        alpha = int(255 * math.sin(t * math.pi))
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], outline=(255, 230, 100, alpha), width=2)
        
    img = img.filter(ImageFilter.GaussianBlur(radius=1.5))
    img.save(output_path, "PNG")
    print(f"Generated Repulsion Pulse Ring: {output_path}")

if __name__ == "__main__":
    generate_pro_slash_texture(os.path.join(SKILLS_DIR, "Pro_InkSlash_Arc.png"))
    generate_pro_spark_streak(os.path.join(SKILLS_DIR, "Spark_Streak.png"))
    generate_circular_ground_crack(os.path.join(SKILLS_DIR, "Decal_Cracked_Circle.png"))
    generate_talisman_ribbon_trail(os.path.join(SKILLS_DIR, "Talisman_Ribbon_Trail.png"))
    generate_batquai_wind_vortex(os.path.join(SKILLS_DIR, "BatQuai_Wind_Vortex.png"))
    generate_repulsion_pulse_ring(os.path.join(SKILLS_DIR, "Repulsion_Pulse_Ring.png"))
