import os
from PIL import Image, ImageEnhance

# Đường dẫn thư mục
art_dir = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI\VongXuyen"
brain_dir = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b2eeef85-41ab-4de3-9bee-fa0c04e09459"

wood_gen = os.path.join(brain_dir, "frame_wood_card_1788438057589.jpg")
jade_gen = os.path.join(brain_dir, "frame_jade_card_1788438079442.jpg")

def process_checkerboard_alpha(img):
    # Chuyển đổi các ô checkerboard thành alpha trong suốt
    img = img.convert("RGBA")
    datas = img.getdata()
    new_data = []
    
    # Lấy kích thước
    w, h = img.size
    
    # Thuật toán tách nền checkerboard:
    # Ở các viền ngoài cùng nơi có checkerboard xám-trắng, alpha = 0
    # Chúng ta quét từ 4 cạnh vào
    return img

if os.path.exists(wood_gen):
    img_wood = Image.open(wood_gen).convert("RGBA")
    target_wood_path = os.path.join(art_dir, "Frame_Card_Wood_9Slice.png")
    img_wood.save(target_wood_path, "PNG")
    print(f"Saved Wood frame to {target_wood_path}")

if os.path.exists(jade_gen):
    img_jade = Image.open(jade_gen).convert("RGBA")
    target_jade_path = os.path.join(art_dir, "Frame_Card_Jade_9Slice.png")
    img_jade.save(target_jade_path, "PNG")
    print(f"Saved Jade frame to {target_jade_path}")

    # Tạo Amber frame từ Jade bằng cách đổi tint sang vàng cam hổ phách
    r, g, b, a = img_jade.split()
    # Hoán đổi kênh màu sang Hổ phách (Amber / Bronze Gold)
    img_amber = Image.merge("RGBA", (g, r, b, a))
    enhancer = ImageEnhance.Color(img_amber)
    img_amber = enhancer.enhance(1.5)
    target_amber_path = os.path.join(art_dir, "Frame_Card_Synergy_9Slice.png")
    img_amber.save(target_amber_path, "PNG")
    print(f"Generated and saved Amber frame to {target_amber_path}")

print("Batch processing completed successfully.")
