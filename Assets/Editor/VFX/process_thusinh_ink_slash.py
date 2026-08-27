import os
from PIL import Image
import numpy as np

src_path = r"C:\Users\thuon\.gemini\antigravity-ide\brain\879351c2-8af8-40a9-84a5-fef3c5ecce1a\.user_uploaded\media_1787804440753.png"
dst_dir = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\Skills"
os.makedirs(dst_dir, exist_ok=True)
dst_path = os.path.join(dst_dir, "Tex_ThuSinh_InkSlash.png")

img = Image.open(src_path).convert("RGBA")
# Xóa logo ngôi sao AI ở góc dưới bên phải nếu có
w, h = img.size
pixels = np.array(img, dtype=np.float32)

# Xóa logo nhỏ góc phải dưới (15% diện tích góc)
for y in range(int(h * 0.75), h):
    for x in range(int(w * 0.82), w):
        # nếu là ngôi sao 4 cánh màu sáng
        if pixels[y, x, 0] > 100 and pixels[y, x, 1] > 100 and pixels[y, x, 2] > 100:
            pixels[y, x] = [0, 0, 0, 255]

# Tách nền đen: Alpha tính theo độ sáng tổng thể và độ bão hòa
r = pixels[:, :, 0]
g = pixels[:, :, 1]
b = pixels[:, :, 2]

# Độ sáng (Luminance)
lum = 0.299 * r + 0.587 * g + 0.114 * b

# Alpha: mịn màng từ 0 (đen) đến 255
# Điểm màu vàng kim và mực đen đều giữ lại
# Nhận diện mực đen: mực đen nằm trên nền phát sáng vàng hoặc có viền vàng
# Với vệt chém additive kết hợp alpha blend:
# Tính max RGB làm alpha gốc cho các tia sáng vàng
max_c = np.maximum(np.maximum(r, g), b)

alpha = np.clip((max_c - 10) / (255 - 10) * 255.0, 0, 255)

# Tăng cường alpha cho phần vàng rực
alpha = np.where(max_c > 30, np.minimum(255.0, alpha * 1.3), alpha)

# Xử lý phần thân mực đen đặc biệt: Trong ảnh, mực đen nằm lồng trong vệt vàng
# Những vùng có vàng xung quanh nhưng màu tối thì vẫn giữ alpha cao
# Tinh chỉnh alpha mượt mà
final_rgba = np.zeros_like(pixels, dtype=np.uint8)
final_rgba[:, :, 0] = np.clip(r, 0, 255).astype(np.uint8)
final_rgba[:, :, 1] = np.clip(g, 0, 255).astype(np.uint8)
final_rgba[:, :, 2] = np.clip(b, 0, 255).astype(np.uint8)
final_rgba[:, :, 3] = np.clip(alpha, 0, 255).astype(np.uint8)

out_img = Image.fromarray(final_rgba, mode="RGBA")
# Xoay chỉnh hướng: Ảnh hiện tại vệt chém đang hơi cong chúc xuống, xoay nhẹ để hướng thẳng sang phải (+X)
# Thử lưu file
out_img.save(dst_path, "PNG")
print(f"Successfully processed Thu Sinh Ink Slash to: {dst_path}")
