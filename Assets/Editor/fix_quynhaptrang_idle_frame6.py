import os
import numpy as np
from PIL import Image

PNG_PATH = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\QuyNhapTrang\QuyNhapTrang-Idle.png"
img = Image.open(PNG_PATH)
arr = np.array(img)

# Frame 6 nằm ở dải x từ 5*128 (640) đến 6*128 (768)
# Cắt bỏ vệt cánh tay duỗi sang phải ở mép phải (x > 750)
arr[:, 750:, 3] = 0
# Lấy chính xác frame 5 (512..640) thay thế cho frame 6 (640..768) để chu kỳ Idle đứng thở loop chuẩn
arr[:, 640:768, :] = arr[:, 512:640, :]

Image.fromarray(arr).save(PNG_PATH)
print("QuyNhapTrang-Idle.png perfectly fixed without cut arm on frame 6!")
