import os
from PIL import Image, ImageChops, ImageEnhance

art_dir = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI\VongXuyen"
files_to_clean = [
    "Frame_Card_Wood_9Slice.png",
    "Frame_Card_Jade_9Slice.png",
    "Frame_Card_Evolution_Gold_9Slice.png",
    "Frame_Card_Synergy_9Slice.png"
]

def clean_outer_checkerboard(img_path):
    if not os.path.exists(img_path):
        print(f"File not found: {img_path}")
        return
    
    img = Image.open(img_path).convert("RGBA")
    w, h = img.size
    pixels = img.load()

    # Nhận diện vùng checkerboard xám-trắng bên ngoài viền
    # Bắt đầu flood fill hoặc quét từ 4 cạnh biên vào
    # Checkerboard có đặc điểm: R ~ G ~ B (grayscale, độ bão hòa màu gần như = 0) và RGB > 180 (xám nhạt và trắng)
    
    visited = [[False for _ in range(h)] for _ in range(w)]
    queue = []

    def is_checkerboard_or_outer_bg(r, g, b, a):
        # Checkerboard xám-trắng hoặc màu nền xám nhạt:
        max_c = max(r, g, b)
        min_c = min(r, g, b)
        diff = max_c - min_c
        # Rất ít màu (diff nhỏ < 25) và sáng (RGB > 160)
        return (diff < 30 and min_c > 150) or a == 0

    # Thêm 4 viền vào queue
    for x in range(w):
        queue.append((x, 0))
        queue.append((x, h - 1))
        visited[x][0] = True
        visited[x][h - 1] = True
        
    for y in range(h):
        queue.append((0, y))
        queue.append((w - 1, y))
        visited[0][y] = True
        visited[w - 1][y] = True

    # Flood fill để đục trong suốt tất cả pixel nền caro bên ngoài viền
    while queue:
        cx, cy = queue.pop(0)
        r, g, b, a = pixels[cx, cy]
        
        if is_checkerboard_or_outer_bg(r, g, b, a):
            pixels[cx, cy] = (0, 0, 0, 0) # Xóa thành trong suốt
            
            # Duyệt 4 lân cận
            for dx, dy in [(-1, 0), (1, 0), (0, -1), (0, 1)]:
                nx, ny = cx + dx, cy + dy
                if 0 <= nx < w and 0 <= ny < h and not visited[nx][ny]:
                    visited[nx][ny] = True
                    nr, ng, nb, na = pixels[nx, ny]
                    if is_checkerboard_or_outer_bg(nr, ng, nb, na):
                        queue.append((nx, ny))

    # Cắt xén vùng trống thừa nếu cần hoặc lưu đè
    img.save(img_path, "PNG")
    print(f"Cleaned alpha successfully: {os.path.basename(img_path)} ({w}x{h})")

for f in files_to_clean:
    clean_outer_checkerboard(os.path.join(art_dir, f))

print("All frame sprites cleaned!")
