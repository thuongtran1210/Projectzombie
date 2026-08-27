import os
from PIL import Image
import numpy as np

def process_image(input_path, output_path, threshold=20):
    img = Image.open(input_path).convert("RGBA")
    data = np.array(img)
    
    # R, G, B channels
    r, g, b, a = data[:, :, 0], data[:, :, 1], data[:, :, 2], data[:, :, 3]
    
    # Calculate brightness / max RGB
    max_rgb = np.maximum(np.maximum(r, g), b)
    
    # Create smooth alpha mask
    # Pure black (< threshold) becomes 0 alpha
    # Transition zone smoothly blends alpha
    alpha = np.clip((max_rgb.astype(float) - threshold) / (threshold * 1.5), 0.0, 1.0) * 255.0
    data[:, :, 3] = alpha.astype(np.uint8)
    
    clean_img = Image.fromarray(data, mode="RGBA")
    
    # Auto crop bounding box
    bbox = clean_img.getbbox()
    if bbox:
        clean_img = clean_img.crop(bbox)
        
    os.makedirs(os.path.dirname(output_path), exist_ok=True)
    clean_img.save(output_path, "PNG")
    print(f"Processed: {output_path} | Size: {clean_img.size}")

base_dir = r"C:\Users\thuon\.gemini\antigravity-ide\brain\335cc8b0-53e6-4c93-9562-5f9b22e5ec04\.user_uploaded"
card_img = os.path.join(base_dir, "media_1787817743511.jpg")
modal_img = os.path.join(base_dir, "media_1787817853674.png")

target_card = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI\Frames\Frame_Card_Upgrade_DongSon.png"
target_modal = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI\Frames\Frame_Modal_Window_DongSon.png"

process_image(card_img, target_card, threshold=22)
process_image(modal_img, target_modal, threshold=22)
