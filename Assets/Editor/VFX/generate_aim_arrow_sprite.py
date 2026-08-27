import os
from PIL import Image, ImageDraw

UI_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI\HUD"
os.makedirs(UI_DIR, exist_ok=True)
dst_path = os.path.join(UI_DIR, "Tex_Attack_Aim_Arrow.png")

SIZE = 256
img = Image.new('RGBA', (SIZE, SIZE), (0, 0, 0, 0))
draw = ImageDraw.Draw(img)

# Vẽ Mũi tên định hướng Cổ Phong viền vàng kim phát quang (hướng sang phải +X)
# Thân mũi tên (chevron hình thoi nhọn)
points_outer = [(40, 70), (190, 128), (40, 186), (80, 128)]
draw.polygon(points_outer, fill=(255, 215, 0, 255), outline=(255, 255, 255, 255), width=4)

# Lõi sáng trắng bên trong
points_inner = [(60, 85), (170, 128), (60, 171), (90, 128)]
draw.polygon(points_inner, fill=(255, 255, 255, 240))

# Đầu mũi nhọn
draw.polygon([(190, 128), (230, 128), (170, 110)], fill=(255, 240, 150, 255))
draw.polygon([(190, 128), (230, 128), (170, 146)], fill=(255, 240, 150, 255))

img.save(dst_path, "PNG")
print(f"Generated Attack Aim Arrow to: {dst_path}")
