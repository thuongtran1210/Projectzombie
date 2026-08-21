import numpy as np
from scipy.io import wavfile
import os

SAMPLE_RATE = 44100
OUTPUT_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\_Data\Audios"
os.makedirs(OUTPUT_DIR, exist_ok=True)

def normalize_and_save(filename, audio_stereo):
    # Ensure audio is float32 between -1.0 and 1.0
    max_val = np.max(np.abs(audio_stereo))
    if max_val > 0:
        audio_stereo = audio_stereo / max_val * 0.95
    
    # Convert to 16-bit PCM
    audio_int16 = (audio_stereo * 32767).astype(np.int16)
    out_path = os.path.join(OUTPUT_DIR, filename)
    wavfile.write(out_path, SAMPLE_RATE, audio_int16)
    print(f"Generated: {out_path} ({len(audio_stereo)/SAMPLE_RATE:.2f}s, stereo 16-bit 44.1kHz)")

def generate_death_gong():
    """SFX Chuông Chiêng Âm Ty Tử Trận - 3.5 giây u linh ngân vang"""
    duration = 3.5
    t = np.linspace(0, duration, int(SAMPLE_RATE * duration), endpoint=False)
    
    # Fundamental & inharmonic bell modes (Đại Hồng Chung / Trống Đồng)
    f0 = 108.0 # A2 note
    partials = [
        (1.0, 1.0, 3.5),     # Fundamental
        (1.008, 0.9, 3.2),   # Beating partner (tạo độ rung u u trầm)
        (1.52, 0.65, 2.5),   # Mode 2
        (2.11, 0.45, 1.8),   # Mode 3
        (2.85, 0.35, 1.4),   # Mode 4
        (3.72, 0.25, 1.0),   # Mode 5
        (5.15, 0.15, 0.7),   # High chime
        (7.20, 0.10, 0.4),   # Initial metallic ping
    ]
    
    # Left & Right channels for stereo space
    left = np.zeros_like(t)
    right = np.zeros_like(t)
    
    for ratio, amp, decay in partials:
        freq = f0 * ratio
        env = np.exp(-t / (decay * 0.8)) * amp
        # Left channel
        left += env * np.sin(2 * np.pi * freq * t)
        # Right channel with slight phase & pitch difference for stereo width
        right += env * np.sin(2 * np.pi * (freq * 1.002) * t + 0.3)
    
    # Transient strike (vồ gỗ gõ chuông)
    strike_len = int(0.04 * SAMPLE_RATE)
    noise = np.random.uniform(-1, 1, strike_len) * np.exp(-np.linspace(0, 10, strike_len))
    left[:strike_len] += noise * 0.5
    right[:strike_len] += noise * 0.5
    
    # Ghostly sub-bass rumble
    sub_bass = np.sin(2 * np.pi * 54.0 * t) * np.exp(-t / 3.0) * 0.4
    left += sub_bass
    right += sub_bass
    
    # Combine stereo
    stereo = np.column_stack((left, right))
    normalize_and_save("SFX_Player_Death_Gong.wav", stereo)

def generate_coin_tick():
    """SFX Tiếng Cổ Tiền Nhảy Số - 0.35 giây tiếng leng keng giòn giã"""
    duration = 0.35
    t = np.linspace(0, duration, int(SAMPLE_RATE * duration), endpoint=False)
    
    # Bright metallic ring modes of small ancient bronze coin
    freqs = [2450.0, 4890.0, 7120.0, 9450.0]
    weights = [0.8, 0.5, 0.3, 0.15]
    decays = [0.25, 0.18, 0.12, 0.08]
    
    left = np.zeros_like(t)
    right = np.zeros_like(t)
    
    for f, w, d in zip(freqs, weights, decays):
        env = np.exp(-t / d) * w
        left += env * np.sin(2 * np.pi * f * t)
        right += env * np.sin(2 * np.pi * (f + 4) * t + 0.2)
        
    # Metallic attack click (1ms)
    click_len = int(0.005 * SAMPLE_RATE)
    click = np.random.uniform(-1, 1, click_len) * np.exp(-np.linspace(0, 15, click_len)) * 0.7
    left[:click_len] += click
    right[:click_len] += click
    
    stereo = np.column_stack((left, right))
    normalize_and_save("SFX_Coin_Tick.wav", stereo)

def generate_gameover_stinger():
    """SFX Mở Panel Thất Bại Game Over - 2.8 giây rền u tối cõi Vong Xuyên"""
    duration = 2.8
    t = np.linspace(0, duration, int(SAMPLE_RATE * duration), endpoint=False)
    
    # Brass braam drop (90Hz -> 45Hz)
    freq_drop = 90.0 * np.exp(-t * 0.7) + 45.0
    phase = 2 * np.pi * np.cumsum(freq_drop) / SAMPLE_RATE
    drone = (np.sin(phase) + 0.4 * np.sin(2 * phase) + 0.2 * np.sin(3 * phase)) * np.exp(-t / 2.0)
    
    # Wind swirl (filtered noise)
    noise = np.random.uniform(-0.5, 0.5, len(t))
    window = np.sin(np.pi * t / duration) ** 2
    wind = noise * window * 0.3
    
    left = drone * 0.8 + wind
    right = drone * 0.8 + np.roll(wind, 200)
    
    stereo = np.column_stack((left, right))
    normalize_and_save("SFX_GameOver_Stinger.wav", stereo)

def generate_ui_wooden_click():
    """SFX Tiếng Gõ Mõ / Chạm Gỗ Cổ Phong cho nút bấm UI - 0.12s"""
    duration = 0.12
    t = np.linspace(0, duration, int(SAMPLE_RATE * duration), endpoint=False)
    
    # Resonant hollow wooden tap (~820Hz + 1640Hz)
    body = (np.sin(2 * np.pi * 820 * t) + 0.5 * np.sin(2 * np.pi * 1640 * t)) * np.exp(-t / 0.03)
    
    # Sharp tap click
    tap_len = int(0.008 * SAMPLE_RATE)
    tap = np.random.uniform(-1, 1, tap_len) * np.exp(-np.linspace(0, 20, tap_len))
    
    left = body
    left[:tap_len] += tap * 0.6
    right = left.copy()
    
    stereo = np.column_stack((left, right))
    normalize_and_save("SFX_UI_Wooden_Click.wav", stereo)

if __name__ == "__main__":
    generate_death_gong()
    generate_coin_tick()
    generate_gameover_stinger()
    generate_ui_wooden_click()
    print("All audio files generated successfully!")
