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

# Trích xuất chính xác 6 ô của Attack (loại bỏ 100% ngôi sao sparkle ở góc dưới phải và chữ ở dưới)
def crop_attack_exact():
    img = Image.open(os.path.join(MATROI_DIR, "raw_attack.png")).convert('RGB')
    arr = np.array(img, dtype=float)
    bg = get_bg_color(arr)
    diff = np.sqrt(np.sum((arr - bg)**2, axis=2))
    
    # 6 ô ROI (x1, y1, x2, y2)
    # Cắt chính xác không chạm chữ
    rois = [
        (40, 70, 340, 380),
        (350, 110, 600, 380),
        (670, 120, 950, 380),
        (40, 480, 340, 720),
        (340, 480, 690, 720),
        (690, 480, 940, 720) # Frame 6 chỉ lấy x: 690..940 (không lấy ngôi sao sparkle ở x: 940..1024, y: 720..765)
    ]
    
    crops = []
    for idx, (x1, y1, x2, y2) in enumerate(rois):
        sub_arr = arr[y1:y2, x1:x2]
        sub_diff = diff[y1:y2, x1:x2]
        
        mask = sub_diff > 25.0
        
        # Với frame 6 (idx == 5), xóa sparkle nếu có
        if idx == 5:
            # Ngôi sao sparkle nằm ở góc dưới phải của sub_arr
            # Xóa các component nhỏ không kết nối với thân Ma Trơi
            labeled, num = label(mask)
            if num > 0:
                sizes = np.bincount(labeled.ravel())
                sizes[0] = 0
                max_lbl = np.argmax(sizes)
                # Chỉ giữ component lớn nhất là thân Ma Trơi
                mask = (labeled == max_lbl)
        elif idx == 4:
            # Frame 5 (bùng phát thiêu đốt): giữ cả khối lửa lớn
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
        
    # Render ra dải 6 frames
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
    print("MaTroi-Attack.png saved with 100% clean frames!")

crop_attack_exact()
