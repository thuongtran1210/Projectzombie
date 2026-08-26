import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects, binary_erosion

IDLE_SRC = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded\media_1787739790904.png"
RUN_SRC = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded\media_1787739797637.png"

img_idle = Image.open(IDLE_SRC).convert('RGB')
arr_idle = np.array(img_idle, dtype=float)
bg_idle = np.median(arr_idle[:20, :20], axis=(0, 1))
lab_idle, _ = label(np.sqrt(np.sum((arr_idle - bg_idle)**2, axis=2)) > 35.0)
sl_idle = [s for s in find_objects(lab_idle) if s is not None and np.sum(lab_idle[s] > 0) > 1500]

img_run = Image.open(RUN_SRC).convert('RGB')
arr_run = np.array(img_run, dtype=float)
bg_run = np.median(arr_run[:20, :20], axis=(0, 1))
lab_run, _ = label(np.sqrt(np.sum((arr_run - bg_run)**2, axis=2)) > 35.0)
sl_run = [s for s in find_objects(lab_run) if s is not None and np.sum(lab_run[s] > 0) > 1000]

# Tóc đen khuôn mặt
sub_idle = np.array(img_idle.crop((sl_idle[0][1].start, sl_idle[0][0].start, sl_idle[0][1].stop, sl_idle[0][0].stop)))
# Khuôn mặt/đầu (khoảng 60% trên)
h_i, w_i = sub_idle.shape[:2]
head_idle_crop = sub_idle[:int(h_i*0.6), :]
head_idle_h = np.sum(np.any((head_idle_crop[:, :, 0] < 45) | ((head_idle_crop[:, :, 1] > 100) & (head_idle_crop[:, :, 2] > 120)), axis=1))

sub_run = np.array(img_run.crop((sl_run[0][1].start, sl_run[0][0].start, sl_run[0][1].stop, sl_run[0][0].stop)))
h_r, w_r = sub_run.shape[:2]
head_run_crop = sub_run[:int(h_r*0.7), :]
head_run_h = np.sum(np.any((head_run_crop[:, :, 0] < 45) | ((head_run_crop[:, :, 1] > 100) & (head_run_crop[:, :, 2] > 120)), axis=1))

print(f"Idle Head Height: {head_idle_h} px, Run Head Height: {head_run_h} px")
ratio = float(head_idle_h) / float(head_run_h)
print(f"Correct Scale for Run: {0.3310 * ratio:.4f}")
