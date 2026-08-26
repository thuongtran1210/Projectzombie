import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects, binary_erosion

MEDIA_DIR = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded"
OUTPUT_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\ThanhDong"

GLOBAL_SCALE = 0.4192

# Tọa độ 8 ô Grid 2x4 chuẩn cho ảnh 1024x572
# Hàng 1: y: 0..286, Hàng 2: y: 286..572
# 4 Cột: x0=0, x1=256, x2=512, x3=768, x4=1024
GRID_8_BOXES = [
    (0, 0, 256, 286),
    (256, 0, 512, 286),
    (512, 0, 768, 286),
    (768, 0, 1024, 286),
    (0, 286, 256, 572),
    (256, 286, 512, 572),
    (512, 286, 768, 572),
    (768, 286, 1024, 572)
]

ACTIONS = {
    "Idle": ("media_1787735726346.png", 8),
    "Run": ("media_1787735784404.png", 8),
    "Attack": ("media_1787735807779.png", 8),
    "Dash": ("media_1787735860850.png", 8),
    "Dead": ("media_1787735901791.png", 8)
}

def clean_thanhdong_perfect(action_name, filename, num_frames):
    src_path = os.path.join(MEDIA_DIR, filename)
    dst_path = os.path.join(OUTPUT_DIR, f"ThanhDong-{action_name}.png")
    
    img = Image.open(src_path).convert('RGB')
    bg = np.median(np.array(img)[:20, :20], axis=(0, 1))
    
    frames = []
    
    for idx in range(num_frames):
        box = GRID_8_BOXES[idx]
        sub_img = img.crop(box)
        sub_rgb = np.array(sub_img, dtype=np.uint8)
        r = sub_rgb[:, :, 0].astype(float)
        g = sub_rgb[:, :, 1].astype(float)
        b = sub_rgb[:, :, 2].astype(float)
        
        dist_bg = np.sqrt((r - bg[0])**2 + (g - bg[1])**2 + (b - bg[2])**2)
        lum = 0.299 * r + 0.587 * g + 0.114 * b
        max_c = np.maximum(np.maximum(r, g), b)
        min_c = np.minimum(np.minimum(r, g), b)
        sat = (max_c - min_c) / (max_c + 1e-5)
        
        h_sub, w_sub = sub_rgb.shape[:2]
        
        # Tiền cảnh
        fg = dist_bg > 30.0
        labeled, num_features = label(fg)
        sizes = np.bincount(labeled.ravel())
        sizes[0] = 0
        
        if np.max(sizes) < 300: # Không có nhân vật
            continue
            
        main_label = np.argmax(sizes)
        char_mask = binary_fill_holes(labeled == main_label)
        
        # Với Attack & Dash: Có hiệu ứng gió lụa trắng
        # Lấy thêm các cụm hiệu ứng gió gắn liền
        if action_name in ["Attack", "Dash"]:
            # Lấy tất cả các cụm > 300px
            top_lbls = [l_i for l_i, sz in enumerate(sizes) if sz > 300]
            char_mask = binary_fill_holes(np.isin(labeled, top_lbls))
            
        # Với Dead frame 7, 8: Giữ hoa sen linh hồn
        if action_name == "Dead" and idx >= 6:
            top_lbls = [l_i for l_i, sz in enumerate(sizes) if sz > 200]
            char_mask = binary_fill_holes(np.isin(labeled, top_lbls))
            
        alpha = np.zeros(char_mask.shape, dtype=np.uint8)
        alpha[char_mask] = 255
        core = binary_erosion(char_mask, iterations=2)
        
        # Xóa Halo
        alpha[(dist_bg < 40.0) & (~core)] = 0
        
        # Xóa bóng đổ xám dưới đất (22% đáy)
        is_bottom = np.zeros_like(char_mask)
        is_bottom[int(h_sub * 0.78):, :] = True
        is_dark = (r < 45) & (g < 45) & (b < 45)
        is_skin = (r > 165) & (g > 105)
        is_red = (r > 135) & (g < 65)
        is_green = (g > 80) & (r < 90)
        is_safe = is_dark | is_skin | is_red | is_green
        alpha[is_bottom & (dist_bg < 65.0) & (sat < 0.16) & (~is_safe)] = 0
        
        # Xóa sparkle ở góc dưới bên phải frame 8
        if idx == 7 and action_name != "Dead":
            is_sparkle_area = np.zeros_like(char_mask)
            is_sparkle_area[int(h_sub * 0.65):, int(w_sub * 0.65):] = True
            alpha[is_sparkle_area & (lum > 165) & (sat < 0.15)] = 0
            
        char_pil = Image.fromarray(np.dstack((sub_rgb, alpha)), 'RGBA')
        bbox = char_pil.getbbox()
        if bbox:
            char_pil = char_pil.crop(bbox)
            
        cw, ch = char_pil.size
        nw, nh = int(round(cw * GLOBAL_SCALE)), int(round(ch * GLOBAL_SCALE))
        resized = char_pil.resize((nw, nh), Image.Resampling.LANCZOS)
        
        res_arr = np.array(resized)
        res_arr[res_arr[:, :, 3] < 100, 3] = 0
        resized = Image.fromarray(res_arr, 'RGBA')
        
        target = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
        px = (128 - nw) // 2
        py = max(2, 128 - nh - 8)
        target.paste(resized, (px, py), resized)
        frames.append(target)
        
    strip = Image.new('RGBA', (len(frames) * 128, 128), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        strip.paste(f, (i * 128, 0), f)
        
    strip.save(dst_path)
    print(f"Cleaned Perfect: ThanhDong-{action_name}.png ({len(frames)} frames) done!")

for act, (file_name, count) in ACTIONS.items():
    clean_thanhdong_perfect(act, file_name, count)
