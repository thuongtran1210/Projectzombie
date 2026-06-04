---
name: vfx_particle_setup
description: "Cấu hình chuẩn xác thông số Particle System cho hiệu ứng đòn đánh 2D (Slash VFX)."
---
# Hướng Dẫn Cấu Hình Particle System Cho Vệt Chém (2D Slash)

Để làm ra một vệt chém dứt khoát, uy lực theo kiểu "vung kiếm" trong game 2D bằng Particle System, bạn cần tinh chỉnh sao cho hệ thống chỉ bắn ra **ĐÚNG 1 hạt (particle)** mang hình ảnh vệt chém, hiện lên chớp nhoáng rồi tắt.

Dưới đây là các thông số chuẩn xác nhất để bạn nhập vào Inspector của Particle System:

## Bước 1: Khởi tạo
1. Nhấn chuột phải vào `Weapon_Whip` -> chọn **Effects -> Particle System**.
2. Đặt tên nó là `SlashVFX_Right`.

## Bước 2: Thiết lập Module chính (Main Module)
Click vào tên `SlashVFX_Right` trên Inspector và cài đặt:
- **Duration:** `0.15` (Thời gian chém rất ngắn)
- **Looping:** `BỎ CHỌN` (Chỉ chém 1 lần mỗi khi gọi)
- **Prewarm:** `BỎ CHỌN`
- **Start Delay:** `0`
- **Start Lifetime:** `0.15` (Hạt tồn tại đúng bằng thời gian đòn đánh)
- **Start Speed:** `0` (Hạt đứng im tại chỗ, không bay đi)
- **Start Size:** `2` hoặc `3` (Tùy độ to của vệt chém bạn muốn)
- **Start Rotation:** Chỉnh góc xoay nếu ảnh của bạn bị ngược (ví dụ `90` hoặc `-90`).
- **Start Color:** Màu của vệt chém (Nên để trắng để dùng màu gốc của Sprite).
- **Play On Awake:** `BỎ CHỌN` (Quan trọng: Code sẽ tự động gọi Play, nếu bật cái này nó sẽ tự nổ lúc mới vào game).

## Bước 3: Thiết lập Emission (Lượng hạt bắn ra)
- **Rate over Time:** `0` (Không bắn liên tục)
- **Rate over Distance:** `0`
- **Bursts:** Nhấn dấu `+` ở góc dưới để thêm 1 Burst.
  - **Time:** `0`
  - **Count:** `1` (Chỉ nổ ra đúng 1 vệt chém duy nhất).
  - **Cycles:** `1`
  - **Interval:** `0.01`

## Bước 4: Thiết lập Shape (Hình dáng phát nổ)
- Bạn hãy **TẮT HOÀN TOÀN** (Bỏ tích) module Shape đi. Vì Start Speed = 0 nên hạt sẽ sinh ra ngay tại tâm (0,0,0) của GameObject này.

## Bước 5: Texture Sheet Animation (Nếu vệt chém có nhiều Frame)
Nếu ảnh vệt chém của bạn là một dải Sprite Sheet (nhiều hình liên tiếp):
- **BẬT** module này lên.
- **Mode:** `Grid` (hoặc `Sprites` nếu bạn cắt sẵn từng ảnh rời).
- **Tiles:** Nhập số cột và số hàng của tấm ảnh (VD: X = 4, Y = 1).
- **Animation:** `Whole Sheet`.
- **Time Mode:** `Lifetime`.
*(Lúc này trong 0.15s tồn tại, nó sẽ chạy mượt mà từ frame đầu tới frame cuối của vệt chém).*

## Bước 6: Renderer (Vật liệu hiển thị)
- Kéo xuống dưới cùng, mở tab **Renderer**.
- **Render Mode:** `Billboard`.
- **Material:** Tạo một Material mới (chuột phải ở cửa sổ Project -> Create -> Material). 
  - Chỉnh Shader của Material đó thành `Sprites/Default`.
  - Kéo tấm ảnh vệt chém của bạn vào ô `Sprite` (hoặc `Albedo` tùy Shader).
  - Kéo Material này thả vào ô Material của Particle System.
- **Sorting Layer:** Đặt là `Player` hoặc layer nằm trên nhân vật để vệt chém đè lên trên quái.
- **Order in Layer:** `10` (Cao hơn các object khác).

---

> [!TIP]
> **Cách làm cho đòn Dual Slash (Đánh 2 bên):**
> 1. Cứ setup xong `SlashVFX_Right` theo các bước trên. Dịch chuyển nó qua bên phải Player một chút (VD: Position X = 2).
> 2. Ấn `Ctrl + D` để nhân bản nó lên, đổi tên thành `SlashVFX_Left`.
> 3. Đổi vị trí nó sang bên trái (Position X = -2) và lật trục X (Rotation Y = 180).
> 4. Gom cả hai thằng này thành con của một GameObject rỗng tên là `Weapon_Whip_VFX`.
> 5. Gắn một cái Particle System vào thằng cha `Weapon_Whip_VFX` nhưng tắt hết mọi thông số phát hạt (cho nó tịt ngòi).
> 6. Kéo thằng cha đó vào ô **Slash Particles** của `Weapon_DualSlash`. Khi code gọi `Play()`, Unity sẽ tự động Play cả thằng cha lẫn 2 thằng con bên trong. Bùm! Chém 2 bên cực đẹp!
