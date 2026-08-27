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

def process_attack():
    img_path = os.path.join(MATROI_DIR, "raw_attack.png")
    img = Image.open(img_path).convert('RGB')
    arr = np.array(img, dtype=float)
    h, w, _ = arr.shape
    bg = get_bg_color(arr)
    diff = np.sqrt(np.sum((arr - bg)**2, axis=2))
    
    # 6 ô ROI chính xác (x1, y1, x2, y2)
    # Hàng 1: cy ~ 150 (y: 40..250)
    # Hàng 2: cy ~ 420 (y: 300..540)
    rois = [
        # Frame 1 (top-left)
        (60, 40, 310, 250),
        # Frame 2 (top-mid)
        (400, 40, 630, 250),
        # Frame 3 (top-right)
        (710, 40, 960, 250),
        # Frame 4 (bot-left)
        (60, 300, 310, 540),
        # Frame 5 (bot-mid, phóng cầu lửa lớn)
        (370, 300, 670, 540),
        # Frame 6 (bot-right, ma trơi cười, không lấy sparkle ở góc dưới phải)
        (720, 300, 930, 510)
    ]
    
    crops = []
    for idx, (x1, y1, x2, y2) in enumerate(rois):
        sub_arr = arr[y1:y2, x1:x2]
        sub_diff = diff[y1:y2, x1:x2]
        
        mask = sub_diff > 25.0
        
        # Frame 6: lọc component lớn nhất để xóa các đốm sparkle/chấm nhỏ xung quanh
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
        
    GLOBAL_SCALE = 0.3515
    strip = Image.new("RGBA", (128 * 6, 128), (0, 0, 0, 0))
    for i, crop in enumerate(crops):
        new_w = int(round(crop.width * GLOBAL_SCALE))
        new_h = int(round(crop.height * GLOBAL_SCALE))
        
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
    print(f"Generated new Attack strip: {out_path}")
    
    # Cập nhật các single frames
    out_frames_dir = os.path.join(MATROI_DIR, "IndividualFrames")
    for i in range(6):
        frame = strip.crop((i * 128, 0, (i + 1) * 128, 128))
        frame.save(os.path.join(out_frames_dir, f"Attack_{i}.png"))
    print("Updated Attack_0.png to Attack_5.png!")

process_attack()
