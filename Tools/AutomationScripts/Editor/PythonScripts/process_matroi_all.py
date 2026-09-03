import os
import numpy as np
from PIL import Image
from scipy.ndimage import label, find_objects, binary_fill_holes, binary_erosion

MATROI_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\MaTroi"

def get_bg_color(arr):
    # Lấy mẫu màu nền xám ở 4 góc
    h, w, _ = arr.shape
    corners = np.concatenate([
        arr[:30, :30].reshape(-1, 3),
        arr[:30, -30:].reshape(-1, 3),
        arr[-30:, :30].reshape(-1, 3),
        arr[-30:, -30:].reshape(-1, 3)
    ], axis=0)
    return np.median(corners, axis=0)

def extract_6_frames_grid(img_path, is_dead=False):
    img = Image.open(img_path).convert('RGB')
    arr = np.array(img, dtype=float)
    h, w, _ = arr.shape
    
    bg = get_bg_color(arr)
    # Tính khoảng cách màu so với nền xám
    diff = np.sqrt(np.sum((arr - bg)**2, axis=2))
    
    # Chia làm 6 ô sơ bộ (2 hàng x 3 cột)
    row_h = h // 2
    col_w = w // 3
    
    frames = []
    
    for r in range(2):
        for c in range(3):
            # Vùng ROI của frame
            y1, y2 = r * row_h, (r + 1) * row_h
            x1, x2 = c * col_w, (c + 1) * col_w
            
            # Cắt bớt phần text chú thích ở dưới nếu có (30% đáy của mỗi hàng)
            y2_clean = y2 - int(row_h * 0.18)
            
            sub_arr = arr[y1:y2_clean, x1:x2]
            sub_diff = diff[y1:y2_clean, x1:x2]
            
            # Mask tiền cảnh
            threshold = 28.0 if is_dead else 35.0
            mask = sub_diff > threshold
            
            # Xóa các thành phần nhỏ li ti ở rìa ngoài góc (nếu có dấu chữ hay sparkle góc)
            labeled, num = label(mask)
            if num > 0:
                sizes = np.bincount(labeled.ravel())
                sizes[0] = 0
                if not is_dead:
                    # Lấy component lớn nhất là ma trơi
                    max_lbl = np.argmax(sizes)
                    mask = (labeled == max_lbl)
                    mask = binary_fill_holes(mask)
                else:
                    # Đối với Dead (có thể là vụn vỡ / hạt đom đóm), lấy các cụm lớn
                    valid_lbls = np.where(sizes > 15)[0]
                    mask = np.isin(labeled, valid_lbls)
            
            # Khử rìa (Defringe)
            # Tạo kênh RGBA
            sub_uint8 = sub_arr.astype(np.uint8)
            alpha = np.zeros((sub_arr.shape[0], sub_arr.shape[1]), dtype=np.uint8)
            
            # Mịn biên alpha
            alpha[mask] = 255
            
            # Đối với Dead frame 6 (F6 - GONE), nếu trống hoàn toàn thì trả về frame trong suốt
            if r == 1 and c == 2 and is_dead:
                # Frame 6 biến mất
                frame_rgba = Image.new("RGBA", (128, 128), (0, 0, 0, 0))
                frames.append(frame_rgba)
                continue
                
            rgba = np.dstack([sub_uint8, alpha])
            
            # Tìm bounding box của đối tượng trong sub frame
            if np.sum(mask) > 0:
                pos = np.where(mask)
                min_y, max_y = np.min(pos[0]), np.max(pos[0])
                min_x, max_x = np.min(pos[1]), np.max(pos[1])
                
                char_crop = Image.fromarray(rgba[min_y:max_y+1, min_x:max_x+1])
            else:
                char_crop = Image.new("RGBA", (10, 10), (0, 0, 0, 0))
                
            frames.append(char_crop)
            
    return frames

print("Script framework ready!")
