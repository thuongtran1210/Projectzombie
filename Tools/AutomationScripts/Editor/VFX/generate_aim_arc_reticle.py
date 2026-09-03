import os
from PIL import Image, ImageDraw

UI_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI\HUD"
os.makedirs(UI_DIR, exist_ok=True)
dst_path = os.path.join(UI_DIR, "Tex_Attack_Aim_Arc_Reticle.png")

SIZE = 512
img = Image.new('RGBA', (SIZE, SIZE), (0, 0, 0, 0))
draw = ImageDraw.Draw(img)

# Tâm vòng tròn (50, 256) nằm bên trái để khi xoay quanh tâm (0,0) nó tỏa ra phía trước (+X)
# Vẽ Vòng Cung Kiếm Ý 120 độ phát quang Cổ Phong
# Vòng cung ngoài (Viền Hào Quang Vàng Kim)
draw.arc([30, 36, 470, 476], start=-60, end=60, fill=(255, 215, 0, 230), width=12)

# Vòng cung lõi sáng bên trong
draw.arc([42, 48, 458, 464], start=-55, end=55, fill=(255, 255, 255, 255), width=6)

# Mũi tên định hướng tâm ở chính giữa vòng cung (đỉnh góc 0 độ)
draw.polygon([(460, 256), (420, 236), (430, 256), (420, 276)], fill=(255, 255, 255, 255), outline=(255, 215, 0, 255))

# 2 Mũi nhọn ở 2 đầu cánh vòng cung
draw.ellipse([240, 68, 256, 84], fill=(255, 215, 0, 255))
draw.ellipse([240, 428, 256, 444], fill=(255, 215, 0, 255))

img.save(dst_path, "PNG")
print(f"Generated Attack Aim Arc Reticle to: {dst_path}")
