import os
import numpy as np
from PIL import Image
from scipy.ndimage import label, binary_fill_holes, binary_erosion

MATROI_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\MaTroi"

def clean_file(filename):
    path = os.path.join(MATROI_DIR, filename)
    img = Image.open(path)
    arr = np.array(img)
    
    # 1. Xóa sparkle ở góc dưới phải của frame 6 (x từ 740 đến 768, y từ 90 đến 128)
    arr[90:128, 740:768, 3] = 0
    
    # 2. Với Attack, xóa các mẩu nhỏ text ở dưới đáy nếu có
    if 'Attack' in filename:
        arr[115:, :, 3] = 0
        
    # 3. Lọc bỏ các cụm pixel rời rạc < 10px
    alpha = arr[:, :, 3] > 0
    labeled, num = label(alpha)
    if num > 0:
        sizes = np.bincount(labeled.ravel())
        sizes[0] = 0
        small_lbls = np.where((sizes > 0) & (sizes < 15))[0]
        mask_small = np.isin(labeled, small_lbls)
        arr[mask_small, 3] = 0
        
    Image.fromarray(arr).save(path)
    print(f"Cleaned {filename} successfully!")

clean_file("MaTroi-Idle.png")
clean_file("MaTroi-Run.png")
clean_file("MaTroi-Attack.png")
clean_file("MaTroi-Dead.png")
