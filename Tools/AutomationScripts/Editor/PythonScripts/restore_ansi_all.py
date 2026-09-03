import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects, binary_erosion

MEDIA_DIR = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded"
OUTPUT_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\AnSi"
os.makedirs(OUTPUT_DIR, exist_ok=True)

# Map các file gốc của Ẩn Sĩ:
# Idle: media_1787724668386.png (6 frames)
# Run: media_1787724657658.png (6 frames)
# Attack: media_1787724663701.png (6 frames)
# Dash: media_1787725607232.png (4 frames)
# Dead: media_1787725660323.png (6 frames)

FILES = {
    "Idle": ("media_1787724668386.png", 6),
    "Run": ("media_1787724657658.png", 6),
    "Attack": ("media_1787724663701.png", 6),
    "Dash": ("media_1787725607232.png", 4),
    "Dead": ("media_1787725660323.png", 6)
}

# 1. Master Height của Ẩn Sĩ Idle
idle_img = Image.open(os.path.join(MEDIA_DIR, FILES["Idle"][0])).convert('RGB')
idle_arr = np.array(idle_img, dtype=float)
bg_idle = np.median(idle_arr[:20, :20], axis=(0, 1))
lab_idle, _ = label(np.sqrt(np.sum((idle_arr - bg_idle)**2, axis=2)) > 35.0)
sl_idle = [s for s in find_objects(lab_idle) if s is not None and np.sum(lab_idle[s] > 0) > 1500]
master_idle_raw_h = float(sl_idle[0][0].stop - sl_idle[0][0].start)
GLOBAL_SCALE = 96.0 / master_idle_raw_h
print(f"AnSi Master Idle Raw Height: {master_idle_raw_h} px, GLOBAL_SCALE: {GLOBAL_SCALE:.4f}")

def process_action_ansi(action_name, filename, frame_count):
    src_path = os.path.join(MEDIA_DIR, filename)
    dst_path = os.path.join(OUTPUT_DIR, f"AnSi-{action_name}.png")
    
    img = Image.open(src_path).convert('RGB')
    w, h = img.size
    bg = np.median(np.array(img)[:20, :20], axis=(0, 1))
    
    if action_name == "Dash":
        # Grid 2x2 cho Dash (4 frames)
        half_w, half_h = w // 2, h // 2
        boxes = [
            (0, 0, half_w, half_h),
            (half_w, 0, w, half_h),
            (0, half_h, half_w, h),
            (half_w, half_h, w, h)
        ]
        frames = []
        for idx, box in enumerate(boxes):
            sub_img = img.crop(box)
            sub_rgb = np.array(sub_img, dtype=np.uint8)
            r, g, b = sub_rgb[:, :, 0].astype(float), sub_rgb[:, :, 1].astype(float), sub_rgb[:, :, 2].astype(float)
            dist_bg = np.sqrt((r - bg[0])**2 + (g - bg[1])**2 + (b - bg[2])**2)
            fg = dist_bg > 35.0
            labeled, _ = label(fg)
            sizes = np.bincount(labeled.ravel())
            sizes[0] = 0
            if idx == 3: # Frame 4: Lấy nhân vật bên phải
                slices = find_objects(labeled)
                rightmost = -1
                max_x = -1
                for l_idx, sl in enumerate(slices):
                    if sl is not None and sizes[l_idx + 1] > 500:
                        if sl[1].start > max_x:
                            max_x = sl[1].start
                            rightmost = l_idx + 1
                char_mask = binary_fill_holes(labeled == rightmost)
            else:
                main_label = np.argmax(sizes)
                char_mask = binary_fill_holes(labeled == main_label)
                
            alpha = np.zeros(char_mask.shape, dtype=np.uint8)
            alpha[char_mask] = 255
            core = binary_erosion(char_mask, iterations=2)
            alpha[(dist_bg < 35.0) & (~core)] = 0
            
            # Xóa bóng đất
            h_sub = sub_rgb.shape[0]
            is_bottom = np.zeros_like(char_mask)
            is_bottom[int(h_sub * 0.80):, :] = True
            is_dark = (r < 45) & (g < 45) & (b < 45)
            is_skin = (r > 165) & (g > 105)
            is_brown = (r > 100) & (g > 60) & (b < 60)
            is_safe = is_dark | is_skin | is_brown
            alpha[is_bottom & (dist_bg < 65.0) & (~is_safe)] = 0
            
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
            
    else:
        # Grid 2x3 (6 frames)
        arr = np.array(img, dtype=float)
        diff = np.sqrt(np.sum((arr - bg)**2, axis=2))
        fg = diff > 30.0
        labeled, _ = label(fg)
        sizes = np.bincount(labeled.ravel())
        sizes[0] = 0
        top_labels = np.argsort(sizes)[::-1][:6]
        char_mask = binary_fill_holes(np.isin(labeled, top_labels))
        
        labeled_chars, _ = label(char_mask)
        slices = find_objects(labeled_chars)
        valid_slices = [s for s in slices if s is not None and np.sum(labeled_chars[s] > 0) > 1000]
        row_height = img.height // 2
        valid_slices.sort(key=lambda s: (s[0].start // (row_height - 30), s[1].start))
        
        arr_uint = np.array(img, dtype=np.uint8)
        frames = []
        for idx, sl in enumerate(valid_slices[:6]):
            sy, sx = sl
            sub_rgb = arr_uint[sy, sx].copy()
            sub_mask = (labeled_chars[sy, sx] > 0)
            r, g, b = sub_rgb[:, :, 0].astype(float), sub_rgb[:, :, 1].astype(float), sub_rgb[:, :, 2].astype(float)
            dist_bg = np.sqrt((r - bg[0])**2 + (g - bg[1])**2 + (b - bg[2])**2)
            lum = 0.299 * r + 0.587 * g + 0.114 * b
            sat = (np.maximum(np.maximum(r, g), b) - np.minimum(np.minimum(r, g), b)) / (np.maximum(np.maximum(r, g), b) + 1e-5)
            h_sub, w_sub = sub_mask.shape
            
            alpha = np.zeros(sub_mask.shape, dtype=np.uint8)
            alpha[sub_mask] = 255
            core = binary_erosion(sub_mask, iterations=2)
            alpha[(dist_bg < 35.0) & (~core)] = 0
            
            # Xóa bóng đất
            if action_name != "Dead":
                is_bottom = np.zeros_like(sub_mask)
                is_bottom[int(h_sub * 0.80):, :] = True
                is_dark = (r < 45) & (g < 45) & (b < 45)
                is_skin = (r > 165) & (g > 105)
                is_brown = (r > 100) & (g > 60) & (b < 60)
                is_safe = is_dark | is_skin | is_brown
                alpha[is_bottom & (dist_bg < 65.0) & (~is_safe)] = 0
                
            # Xóa sparkle ở góc phải frame 6
            if idx == 5 and action_name != "Dead":
                is_sparkle_area = np.zeros_like(sub_mask)
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
    print(f"Generated AnSi-{action_name}.png ({len(frames)} frames) successfully!")

for act, (fn, cnt) in FILES.items():
    process_action_ansi(act, fn, cnt)
