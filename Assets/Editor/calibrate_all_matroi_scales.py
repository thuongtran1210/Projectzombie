import os
import numpy as np
from PIL import Image
from scipy.ndimage import label, binary_fill_holes, binary_erosion

MATROI_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\MaTroi"

def get_bg_color(arr):
    h, w, _ = arr.shape
    corners = np.concatenate([
        arr[:30, :30].reshape(-1, 3),
        arr[:30, -30:].reshape(-1, 3),
        arr[-30:, :30].reshape(-1, 3),
        arr[-30:, -30:].reshape(-1, 3)
    ], axis=0)
    return np.median(corners, axis=0)

# =========================================================================
# 1. IDLE (Master Reference: SCALE = 0.3515)
# =========================================================================
def build_idle():
    img = Image.open(os.path.join(MATROI_DIR, "raw_idle.png")).convert('RGB')
    arr = np.array(img, dtype=float)
    bg = get_bg_color(arr)
    diff = np.sqrt(np.sum((arr - bg)**2, axis=2))
    
    # 2 hàng x 3 cột
    h, w, _ = arr.shape
    row_h, col_w = h // 2, w // 3
    
    strip = Image.new("RGBA", (128 * 6, 128), (0, 0, 0, 0))
    crops = []
    
    for r in range(2):
        for c in range(3):
            y1, y2 = r * row_h, (r + 1) * row_h
            x1, x2 = c * col_w, (c + 1) * col_w
            
            sub_arr = arr[y1:y2, x1:x2]
            sub_diff = diff[y1:y2, x1:x2]
            mask = sub_diff > 25.0
            
            labeled, num = label(mask)
            if num > 0:
                sizes = np.bincount(labeled.ravel())
                sizes[0] = 0
                max_lbl = np.argmax(sizes)
                mask = (labeled == max_lbl)
                mask = binary_fill_holes(mask)
                
            eroded = binary_erosion(mask, iterations=1)
            edge = mask & (~eroded)
            alpha = np.zeros(sub_arr.shape[:2], dtype=np.uint8)
            alpha[mask] = 255
            alpha[edge] = 160
            
            rgba = np.dstack([sub_arr.astype(np.uint8), alpha])
            pos = np.where(mask)
            min_y, max_y = np.min(pos[0]), np.max(pos[0])
            min_x, max_x = np.min(pos[1]), np.max(pos[1])
            crops.append(Image.fromarray(rgba[min_y:max_y+1, min_x:max_x+1]))
            
    SCALE_IDLE = 0.3515
    for i, crop in enumerate(crops):
        new_w = int(round(crop.width * SCALE_IDLE))
        new_h = int(round(crop.height * SCALE_IDLE))
        resized = crop.resize((new_w, new_h), Image.Resampling.LANCZOS)
        pos_x = i * 128 + (128 - new_w) // 2
        pos_y = (128 - new_h) // 2
        strip.paste(resized, (pos_x, pos_y), resized)
        
    out_path = os.path.join(MATROI_DIR, "MaTroi-Idle.png")
    strip.save(out_path)
    print("Saved calibrated Idle")

# =========================================================================
# 2. RUN (Được phóng to lên với SCALE = 0.4150 để bằng khối đầu Idle)
# =========================================================================
def build_run():
    img = Image.open(os.path.join(MATROI_DIR, "raw_run.png")).convert('RGB')
    arr = np.array(img, dtype=float)
    bg = get_bg_color(arr)
    diff = np.sqrt(np.sum((arr - bg)**2, axis=2))
    
    # 6 ROIs chính xác
    rois = [
        (130, 40, 390, 260),
        (530, 60, 940, 260),
        (30, 290, 500, 480),
        (530, 290, 970, 480),
        (70, 520, 490, 720),
        (710, 520, 950, 720)
    ]
    
    crops = []
    for x1, y1, x2, y2 in rois:
        sub_arr = arr[y1:y2, x1:x2]
        sub_diff = diff[y1:y2, x1:x2]
        mask = sub_diff > 25.0
        
        labeled, num = label(mask)
        if num > 0:
            sizes = np.bincount(labeled.ravel())
            sizes[0] = 0
            max_lbl = np.argmax(sizes)
            mask = (labeled == max_lbl)
            mask = binary_fill_holes(mask)
            
        eroded = binary_erosion(mask, iterations=1)
        edge = mask & (~eroded)
        alpha = np.zeros(sub_arr.shape[:2], dtype=np.uint8)
        alpha[mask] = 255
        alpha[edge] = 160
        
        rgba = np.dstack([sub_arr.astype(np.uint8), alpha])
        pos = np.where(mask)
        min_y, max_y = np.min(pos[0]), np.max(pos[0])
        min_x, max_x = np.min(pos[1]), np.max(pos[1])
        crops.append(Image.fromarray(rgba[min_y:max_y+1, min_x:max_x+1]))
        
    SCALE_RUN = 0.4150
    strip = Image.new("RGBA", (128 * 6, 128), (0, 0, 0, 0))
    for i, crop in enumerate(crops):
        new_w = int(round(crop.width * SCALE_RUN))
        new_h = int(round(crop.height * SCALE_RUN))
        
        # Nếu chiều dài vệt lửa vượt quá 124px thì scale vừa vặn
        if new_w > 124:
            s_adj = 124.0 / float(new_w)
            new_w = 124
            new_h = int(round(new_h * s_adj))
        if new_h > 124:
            s_adj = 124.0 / float(new_h)
            new_h = 124
            new_w = int(round(new_w * s_adj))
            
        resized = crop.resize((new_w, new_h), Image.Resampling.LANCZOS)
        pos_x = i * 128 + (128 - new_w) // 2
        pos_y = (128 - new_h) // 2
        strip.paste(resized, (pos_x, pos_y), resized)
        
    out_path = os.path.join(MATROI_DIR, "MaTroi-Run.png")
    strip.save(out_path)
    print("Saved calibrated Run")

# =========================================================================
# 3. ATTACK (Được phóng to lên với SCALE = 0.4213 để bằng khối đầu Idle)
# =========================================================================
def build_attack():
    img = Image.open(os.path.join(MATROI_DIR, "raw_attack.png")).convert('RGB')
    arr = np.array(img, dtype=float)
    bg = get_bg_color(arr)
    diff = np.sqrt(np.sum((arr - bg)**2, axis=2))
    
    rois = [
        (60, 40, 310, 250),
        (400, 40, 630, 250),
        (710, 40, 960, 250),
        (60, 300, 310, 540),
        (370, 300, 670, 540),
        (720, 300, 930, 510)
    ]
    
    crops = []
    for idx, (x1, y1, x2, y2) in enumerate(rois):
        sub_arr = arr[y1:y2, x1:x2]
        sub_diff = diff[y1:y2, x1:x2]
        mask = sub_diff > 25.0
        
        if idx == 5:
            labeled, num = label(mask)
            if num > 0:
                sizes = np.bincount(labeled.ravel())
                sizes[0] = 0
                max_lbl = np.argmax(sizes)
                mask = (labeled == max_lbl)
        elif idx == 4:
            mask = binary_fill_holes(mask)
        else:
            labeled, num = label(mask)
            if num > 0:
                sizes = np.bincount(labeled.ravel())
                sizes[0] = 0
                valid = np.isin(labeled, np.where(sizes > 30)[0])
                mask = binary_fill_holes(valid)
                
        eroded = binary_erosion(mask, iterations=1)
        edge = mask & (~eroded)
        alpha = np.zeros(sub_arr.shape[:2], dtype=np.uint8)
        alpha[mask] = 255
        alpha[edge] = 160
        
        rgba = np.dstack([sub_arr.astype(np.uint8), alpha])
        pos = np.where(mask)
        min_y, max_y = np.min(pos[0]), np.max(pos[0])
        min_x, max_x = np.min(pos[1]), np.max(pos[1])
        crops.append(Image.fromarray(rgba[min_y:max_y+1, min_x:max_x+1]))
        
    SCALE_ATTACK = 0.4213
    strip = Image.new("RGBA", (128 * 6, 128), (0, 0, 0, 0))
    for i, crop in enumerate(crops):
        new_w = int(round(crop.width * SCALE_ATTACK))
        new_h = int(round(crop.height * SCALE_ATTACK))
        
        if new_w > 124:
            s_adj = 124.0 / float(new_w)
            new_w = 124
            new_h = int(round(new_h * s_adj))
        if new_h > 124:
            s_adj = 124.0 / float(new_h)
            new_h = 124
            new_w = int(round(new_w * s_adj))
            
        resized = crop.resize((new_w, new_h), Image.Resampling.LANCZOS)
        pos_x = i * 128 + (128 - new_w) // 2
        pos_y = (128 - new_h) // 2
        strip.paste(resized, (pos_x, pos_y), resized)
        
    out_path = os.path.join(MATROI_DIR, "MaTroi-Attack.png")
    strip.save(out_path)
    print("Saved calibrated Attack")

# =========================================================================
# 4. DEAD (Được thu nhỏ lại với SCALE = 0.3457 và lọc sạch chữ F1..F6)
# =========================================================================
def build_dead():
    img = Image.open(os.path.join(MATROI_DIR, "raw_dead.png")).convert('RGB')
    arr = np.array(img, dtype=float)
    bg = get_bg_color(arr)
    diff = np.sqrt(np.sum((arr - bg)**2, axis=2))
    
    # 6 ROIs loại trừ chữ F1..F6 ở đáy
    # Hàng 1 (y: 40..340, bỏ y > 340 có chữ)
    # Hàng 2 (y: 420..710, bỏ y > 710 có chữ)
    rois = [
        (40, 40, 320, 340),
        (370, 40, 640, 340),
        (700, 40, 980, 340),
        (40, 420, 340, 710),
        (370, 420, 640, 710),
        (700, 420, 980, 710) # Frame 6 biến mất
    ]
    
    crops = []
    for idx, (x1, y1, x2, y2) in enumerate(rois):
        if idx == 5:
            crops.append(None)
            continue
            
        sub_arr = arr[y1:y2, x1:x2]
        sub_diff = diff[y1:y2, x1:x2]
        
        # Ngưỡng tách hạt tan biến
        th = 16.0 if idx >= 3 else 25.0
        mask = sub_diff > th
        
        # Xóa chữ và rác
        labeled, num = label(mask)
        if num > 0:
            sizes = np.bincount(labeled.ravel())
            sizes[0] = 0
            if idx < 3:
                # 3 frame đầu giữ khối lớn
                valid = np.isin(labeled, np.where(sizes > 40)[0])
                mask = binary_fill_holes(valid)
            else:
                # 2 frame sau là bụi hạt tan biến
                valid = np.isin(labeled, np.where(sizes > 8)[0])
                mask = valid
                
        eroded = binary_erosion(mask, iterations=1)
        edge = mask & (~eroded)
        alpha = np.zeros(sub_arr.shape[:2], dtype=np.uint8)
        alpha[mask] = 255
        alpha[edge] = 160
        
        rgba = np.dstack([sub_arr.astype(np.uint8), alpha])
        if np.sum(mask) > 0:
            pos = np.where(mask)
            min_y, max_y = np.min(pos[0]), np.max(pos[0])
            min_x, max_x = np.min(pos[1]), np.max(pos[1])
            crops.append(Image.fromarray(rgba[min_y:max_y+1, min_x:max_x+1]))
        else:
            crops.append(None)
            
    SCALE_DEAD = 0.3457
    strip = Image.new("RGBA", (128 * 6, 128), (0, 0, 0, 0))
    for i, crop in enumerate(crops):
        if crop is None:
            continue
        new_w = int(round(crop.width * SCALE_DEAD))
        new_h = int(round(crop.height * SCALE_DEAD))
        
        if new_w > 124:
            s_adj = 124.0 / float(new_w)
            new_w = 124
            new_h = int(round(new_h * s_adj))
        if new_h > 124:
            s_adj = 124.0 / float(new_h)
            new_h = 124
            new_w = int(round(new_w * s_adj))
            
        resized = crop.resize((new_w, new_h), Image.Resampling.LANCZOS)
        pos_x = i * 128 + (128 - new_w) // 2
        pos_y = (128 - new_h) // 2
        strip.paste(resized, (pos_x, pos_y), resized)
        
    out_path = os.path.join(MATROI_DIR, "MaTroi-Dead.png")
    strip.save(out_path)
    print("Saved calibrated Dead")

# Thực thi
build_idle()
build_run()
build_attack()
build_dead()

# Cập nhật toàn bộ các single frames
out_frames_dir = os.path.join(MATROI_DIR, "IndividualFrames")
for act in ['Idle', 'Run', 'Attack', 'Dead']:
    png_path = os.path.join(MATROI_DIR, f'MaTroi-{act}.png')
    im = Image.open(png_path)
    for i in range(6):
        frame = im.crop((i * 128, 0, (i + 1) * 128, 128))
        frame.save(os.path.join(out_frames_dir, f"{act}_{i}.png"))

print("ALL 4 ACTIONS CALIBRATED AND SYNCHRONIZED PERFECTLY TO IDLE SIZE!")
