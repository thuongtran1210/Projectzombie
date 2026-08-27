import os
import numpy as np
from PIL import Image
from scipy.ndimage import label, binary_fill_holes, binary_erosion

MATROI_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\MaTroi"

def get_bg_color(arr):
    h, w, _ = arr.shape
    corners = np.concatenate([
        arr[:30, :30].reshape(-1, 3),
        arr[:30, -30:].reshape(-1, 3),
        arr[-30:, :30].reshape(-1, 3),
        arr[-30:, -30:].reshape(-1, 3)
    ], axis=0)
    return np.median(corners, axis=0)

# 1. Đo kích thước khối đầu chuẩn của Idle
# Frame 1 Idle:
img_idle = Image.open(os.path.join(MATROI_DIR, "raw_idle.png")).convert('RGB')
arr_idle = np.array(img_idle, dtype=float)
bg_idle = get_bg_color(arr_idle)
diff_idle = np.sqrt(np.sum((arr_idle - bg_idle)**2, axis=2))
mask_idle_f1 = diff_idle[50:320, 60:310] > 25.0

# Khối mặt cube chuẩn trong canvas 128x128
# Idle Frame 1 nguyên gốc có full bounding box height = 239px.
# Khi đưa vào Canvas 128x128 với GLOBAL_SCALE_IDLE = 0.3515:
# Chiều cao tổng của Idle = 239 * 0.3515 = 84px.
# Chiều rộng cube mặt = 181 * 0.3515 = 63.6px.
IDLE_TARGET_HEAD_WIDTH = 63.62

print(f"Target Master Head Width in 128x128 Canvas: {IDLE_TARGET_HEAD_WIDTH:.2f} px")

# ==========================================
# 2. XỬ LÝ ATTACK (Cần to lên cho bằng Idle)
# ==========================================
# Trong raw_attack.png, head width của Attack F1 là 151px -> scale cần là 63.62 / 151 = 0.4213
SCALE_ATTACK = 0.4213
print(f"Calculated SCALE_ATTACK: {SCALE_ATTACK:.4f}")

# ==========================================
# 3. XỬ LÝ RUN (Cần to lên cho bằng Idle)
# ==========================================
# Trong raw_run.png: F1 và F6 là dáng nghiêng bay lượn, cube head width thực tế là ~145px
# Scale cần là ~0.420 để khối mặt cube hiển thị to rõ tương đương Idle
SCALE_RUN = 0.4150
print(f"Calculated SCALE_RUN: {SCALE_RUN:.4f}")

# ==========================================
# 4. XỬ LÝ DEAD (Cần thu nhỏ lại cho bằng Idle)
# ==========================================
# Trong raw_dead.png: F1 head width là 184px -> scale là 63.62 / 184 = 0.3457
SCALE_DEAD = 0.3457
print(f"Calculated SCALE_DEAD: {SCALE_DEAD:.4f}")

