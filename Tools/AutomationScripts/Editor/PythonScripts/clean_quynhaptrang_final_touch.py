import os
import numpy as np
from PIL import Image

# 1. Gọt sạch spark ở frame 6 của Idle, Run, Attack và vệt cào ở frame 5 của Attack
PNG_PATHS = {
    "Idle": r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\QuyNhapTrang\QuyNhapTrang-Idle.png",
    "Run": r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\QuyNhapTrang\QuyNhapTrang-Run.png",
    "Attack": r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\QuyNhapTrang\QuyNhapTrang-Attack.png",
    "Dead": r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\QuyNhapTrang\QuyNhapTrang-Dead.png"
}

# Attack: Xóa vệt đen lẻ loi ở frame 5 (x: 4*128 đến 5*128, x_local: 0..25)
img_att = Image.open(PNG_PATHS["Attack"])
arr_att = np.array(img_att)
# frame 5 là index 4: x từ 512 đến 640
arr_att[:, 512:512+35, 3] = 0
# xóa sparkle ở frame 6 (index 5: 640..768, góc dưới phải)
arr_att[80:, 710:, 3] = 0
Image.fromarray(arr_att).save(PNG_PATHS["Attack"])

# Idle: xóa sparkle frame 6
img_idle = Image.open(PNG_PATHS["Idle"])
arr_idle = np.array(img_idle)
arr_idle[80:, 710:, 3] = 0
Image.fromarray(arr_idle).save(PNG_PATHS["Idle"])

# Run: xóa sparkle frame 6
img_run = Image.open(PNG_PATHS["Run"])
arr_run = np.array(img_run)
arr_run[80:, 710:, 3] = 0
Image.fromarray(arr_run).save(PNG_PATHS["Run"])

print("All sparkles and border artifacts removed!")
