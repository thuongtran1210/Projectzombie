import os
from PIL import Image, ImageDraw

UI_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI\Skills"
os.makedirs(UI_DIR, exist_ok=True)

SIZE = 256

def create_base_canvas():
    return Image.new('RGBA', (SIZE, SIZE), (0, 0, 0, 0))

# 1. Icon Bút Lệnh Thư Sinh (Vàng Kim / Mực Thư Pháp)
img_thusinh = create_base_canvas()
draw = ImageDraw.Draw(img_thusinh)
# Viền hào quang vàng kim
draw.ellipse([20, 20, 236, 236], fill=(40, 30, 15, 200), outline=(255, 215, 0, 255), width=8)
# Thân bút gỗ sơn mài
draw.polygon([(110, 40), (146, 40), (138, 150), (118, 150)], fill=(120, 50, 20, 255), outline=(255, 215, 0, 255))
# Khâu bút đồng vàng
draw.rectangle([116, 145, 140, 165], fill=(255, 215, 0, 255))
# Lông bút & giọt mực đen
draw.polygon([(116, 165), (140, 165), (128, 220)], fill=(20, 20, 25, 255), outline=(255, 255, 255, 180))
img_thusinh.save(os.path.join(UI_DIR, "Icon_Atk_ThuSinh_Brush.png"), "PNG")

# 2. Icon Tiên Kiếm Đạo Sĩ (Xanh Ngọc Lục Bảo / Jade)
img_daosi = create_base_canvas()
draw = ImageDraw.Draw(img_daosi)
# Vòng Bát quái xanh ngọc
draw.ellipse([20, 20, 236, 236], fill=(15, 35, 30, 200), outline=(77, 238, 234, 255), width=8)
# Lưỡi kiếm Tiên Đạo
draw.polygon([(128, 35), (145, 60), (138, 165), (118, 165), (111, 60)], fill=(220, 255, 250, 255), outline=(77, 238, 234, 255))
# Rãnh kiếm phát quang
draw.line([(128, 45), (128, 160)], fill=(77, 238, 234, 255), width=4)
# Chuôi kiếm & đốc kiếm gỗ đào
draw.rectangle([100, 165, 156, 178], fill=(160, 82, 45, 255), outline=(255, 215, 0, 255))
draw.rectangle([122, 178, 134, 225], fill=(120, 50, 20, 255))
img_daosi.save(os.path.join(UI_DIR, "Icon_Atk_DaoSi_Sword.png"), "PNG")

# 3. Icon Đuốc Lửa Thanh Đồng (Đỏ Cam Lửa Thiêng Tứ Phủ)
img_thanhdong = create_base_canvas()
draw = ImageDraw.Draw(img_thanhdong)
# Vòng tròn lửa đỏ
draw.ellipse([20, 20, 236, 236], fill=(45, 20, 15, 200), outline=(255, 80, 20, 255), width=8)
# Cán đuốc
draw.polygon([(120, 140), (136, 140), (132, 225), (124, 225)], fill=(100, 50, 30, 255), outline=(255, 180, 50, 255))
# Ngọn lửa thiêng 3 tầng
draw.polygon([(128, 35), (165, 95), (150, 145), (106, 145), (91, 95)], fill=(255, 60, 10, 255))
draw.polygon([(128, 60), (152, 105), (142, 140), (114, 140), (104, 105)], fill=(255, 160, 20, 255))
draw.polygon([(128, 85), (140, 115), (135, 135), (121, 135), (116, 115)], fill=(255, 240, 150, 255))
img_thanhdong.save(os.path.join(UI_DIR, "Icon_Atk_ThanhDong_Torch.png"), "PNG")

# 4. Icon Thạch Quyền Ẩn Sĩ (Nâu Hổ Phách Nứt Đá)
img_ansi = create_base_canvas()
draw = ImageDraw.Draw(img_ansi)
# Vòng chấn địa nâu vàng
draw.ellipse([20, 20, 236, 236], fill=(40, 30, 20, 200), outline=(220, 160, 50, 255), width=8)
# Nắm đấm đá thạch thể
draw.polygon([(90, 80), (166, 80), (175, 140), (155, 195), (101, 195), (81, 140)], fill=(150, 110, 70, 255), outline=(240, 200, 100, 255), width=3)
# Đường nứt phát quang
draw.line([(128, 85), (128, 145), (150, 175)], fill=(255, 215, 0, 255), width=4)
draw.line([(128, 120), (100, 150)], fill=(255, 215, 0, 255), width=4)
img_ansi.save(os.path.join(UI_DIR, "Icon_Atk_AnSi_Fist.png"), "PNG")

print("Successfully generated 4 character basic attack icons!")
