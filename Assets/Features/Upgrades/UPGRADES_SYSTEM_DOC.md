# Thiết Kế Hệ Thống Tiến Trình & Nâng Cấp Kỹ Năng (Game Progression System)

> **Tài liệu chuẩn hóa (Single Source of Truth)** cho toàn bộ hệ thống Nâng Cấp & Tiến Trình Trận Đấu dự án **Projectzombie** (Unity 2022 - Top-down Survival Roguelite phong cách Thần Thoại Cổ Phong Việt Nam).

---

## 1. Hành Trình Trận Đấu (Logarithmic Progression)

Hành trình trải qua **4 giai đoạn tiến hóa** từ sơ nhập đến Thần Tướng Tối Thượng:

```text
[ 00:00 - BẮT ĐẦU TRẬN ] 
       │
       ▼
 🌟 CẤP 1: ĐẠI LÕI KHỞI NGUYÊN (Khóa Archetype & Lọc Kho Thẻ Sạch)
    └─ Chọn 1 trong 5 Đại Lõi Thần Thoại để định hình lối chơi và kích hoạt Clean Pool.
       │
       ▼
 ⚙️ LEVEL THƯỜNG (Lv. 2–4, 6–14, 16–29): BỒI ĐẮP CHỈ SỐ NỀN TẢNG
    └─ Bốc nhanh thẻ chỉ số thô (SMCK, Tốc đánh, Tầm bắn, Máu, Giáp) thay thế hoàn toàn Shop đồ.
    └─ Thiết kế 1 dòng đọc < 1 giây, giữ trọn nhịp càn quét không ngắt quãng.
       │
       ▼
 ⚡ MỐC ĐỘT BIẾN (Lv. 5 ➔ Lv. 15 ➔ Lv. 30): LÕI BIẾN DỊ QUY TẮC (TFT / Arena Style)
    └─ Bốc 1 trong 3 Lõi đột biến (Bạc / Vàng / Kim Cương) bẻ gãy cơ chế vận hành.
    └─ Cung cấp 2 Lượt Reroll xúc xắc chuyên dụng để người chơi chủ động tìm kiếm mảnh ghép.
       │
       ▼
 👑 PHÚT 12+ (Lv. 30+): QUYẾT CHIẾN BOSS SÀN ĐẤU
    └─ Bộ Build đạt ngưỡng thần hóa tối đa, bước vào giao tranh sinh tử với Boss cuối.
```

---

## 2. Chi Tiết Các Tầng Nâng Cấp

### 2.1. Tầng 1: Đại Lõi Cấp 1 (Class Archetype)

> **Thời điểm kích hoạt:** Chọn ngay khi bước vào trận đấu (Level 1).  
> **Cơ chế cốt lõi:** Cung cấp bộ khung chiến đấu độc bản và kích hoạt cơ chế **Kho Thẻ Sạch (Clean Pool)** — loại bỏ hoàn toàn các thẻ không tương thích để người chơi không bao giờ bốc phải thẻ vô dụng.

* 🔥 **Phù Đổng Thiên Uy (Hệ Hỏa — Thể Tu Khổng Lồ, Càn Quét):**
  * Càng hạ nhiều quái vật, thân thể càng phóng to (`+10% kích thước` sau mỗi mốc hạ gục tích lũy).
  * Đòn chém quét sở hữu phạm vi cực rộng.
  * Kỹ năng lướt (*Dash*) biến thành **Ngựa Sắt phun lửa**, thiêu rụi toàn bộ kẻ địch trên đường càn quét.

* ✨ **Kim Quy Thần Cơ (Hệ Kim — Mưa Tên & Bẫy Phản Xạ):**
  * Đòn đánh tự động bắn nảy sang các mục tiêu lân cận.
  * Tỷ lệ bạo kích đạt `100%` khi người chơi thực hiện thao tác thả diều (kiting).
  * Khi đứng yên, tự động kích hoạt **Mai Rùa Thần Cơ**: miễn nhiễm hoàn toàn sát thương tầm xa từ phía trước và phản xạ đòn đánh.

* ⛰️ **Tản Viên Sơn Thánh (Hệ Thổ — Pháo Đài Địa Chấn):**
  * Tích lũy lớp **Giáp Hoàng Thổ** dày đặc theo thời gian.
  * Khi đứng yên hoặc nhận sát thương, mặt đất mọc lên **Thạch Trụ** đè bẹp quái vật và kích hoạt phát nổ địa chấn diện rộng.

* 🌊 **Thủy Bá Cuồng Nộ (Hệ Thủy — Sóng Thần & Khống Chế):**
  * Khả năng gọi mưa bão gom toàn bộ quái vật trên bản đồ lại một điểm cố định.
  * Kỹ năng gây trạng thái **Ẩm Ướt ➔ Đóng Băng** hoàn toàn mục tiêu.
  * Kỹ năng lướt (*Dash*) hóa thành **Thủy Long** càn quét dữ dội trên chiến trường.

* ☯️ **Long Tiên Huyết Mạch (Hệ Âm Dương — Song Hình & Hồi Sinh):**
  * **Hóa Long:** Kích hoạt khi tấn công liên tục (tăng vọt SMCK và Tốc độ đánh).
  * **Hóa Tiên:** Kích hoạt khi tạm ngừng tay (tỏa hào quang hồi máu liên tục cho bản thân).
  * Sở hữu `1 mạng Miễn Tử`: khi nhận đòn chí tử sẽ kích hoạt vụ nổ xóa sổ toàn màn hình, dọn sạch quái vật xung quanh và hồi 50% HP.

---

### 2.2. Tầng 2: Thẻ Level Thường (Sub-Upgrades — Thay Thế Shop Đồ)

* **Phạm vi xuất hiện:** Tại các mốc Level `2–4`, `6–14`, và `16–29`.
* **Thiết kế UX/UI tối giản:**
  * Icon trực quan đi kèm đúng 1 dòng mô tả ngắn gọn *(Ví dụ: `+15% Tốc Đánh`, `+20% Tầm Đánh`, `+120 Máu Tối Đa`)*.
* **Tốc độ xử lý:**
  * Thời gian đọc hiểu dưới 1 giây, người chơi chạm/click là lập tức quay lại chiến đấu, đảm bảo không làm ngắt quãng nhịp độ càn quét quái vật 60 FPS.
* **Bộ lọc thông minh (Clean Pool):**
  * Kho thẻ tự động lọc theo Đại Lõi đã lựa chọn ở Lv. 1 *(Ví dụ: chọn Phù Đổng ưu tiên Máu / Giáp / Phạm vi ảnh hưởng; chọn Kim Quy ưu tiên Tốc đánh / Xuyên giáp)*.

---

### 2.3. Tầng 3: 3 Cột Mốc Lõi Đột Biến (Lv. 5 – Lv. 15 – Lv. 30)

> **Cơ chế:** Khi đạt đến các mốc cấp độ này, hệ thống sẽ tạm dừng trận đấu và hiển thị giao diện **Hoàng Kim / Kim Cương** đặc biệt để người chơi tính toán chiến thuật chuyên sâu.

| Cột Mốc | Bậc Phẩm | Thời Điểm | Vai Trò Trong Trận | Ví Dụ Minh Họa |
| :--- | :--- | :--- | :--- | :--- |
| **Lv. 5** | **Bạc / Vàng** | Phút 02:00 – 03:00 | **Cú hích sơ khởi:** Bổ sung cơ chế tiện ích & chuyển hóa chỉ số nền tảng nhằm dọn sạch đợt quái tinh anh đầu tiên. | **Huyết Khí Đồng Quy:** Mỗi 10% máu tối đa cộng thêm từ thẻ thường chuyển hóa thành 5% sát thương kỹ năng và 3% kích thước cơ thể. |
| **Lv. 15** | **Vàng / Kim Cương** | Phút 06:30 – 07:30 | **Bước ngoặt giữa trận:** Tạo đột biến tương tác chiêu thức diện rộng khi mật độ quái vật bắt đầu áp đảo toàn bản đồ. | **Thạch Phá Thiên Kinh:** Gây hiệu ứng khống chế cứng lên quái sẽ kích nổ chấn động gây sát thương bằng 200% Giáp hiện có lên toàn bộ mục tiêu lân cận. |
| **Lv. 30** | **Kim Cương** | Phút 10:30 – 11:30 | **Thần Hóa Tối Thượng:** Phá vỡ hoàn toàn logic vận hành thông thường, hoàn thiện bộ Build để đối đầu trực diện Boss sàn đấu. | **Vạn Kiếp Luân Hồi:** Toàn bộ sát thương diện rộng và đòn đánh của Đại Lõi có thể nổ Chí Mạng 175%; nhận thêm 1 lần dùng chiêu thức chủ động không tiêu hao thời gian hồi chiêu. |

---

## 3. Cơ Chế Bổ Trợ & Quy Luật Vận Hành

1. **Lượt Đổi Khí Vận (Reroll Token):**
   * Mỗi người chơi nhận cố định **2 lượt Reroll** cho toàn bộ trận đấu.
   * Lượt đổi chỉ áp dụng riêng cho 3 mốc Lõi Đột Biến (`Lv. 5`, `Lv. 15`, `Lv. 30`), không hỗ trợ cho các đợt lên level thường để tránh làm loãng nhịp độ.

2. **Cơ Chế Bổ Trợ Ngầm (Smart Synergy):**
   * Khi người chơi chọn Lõi Đột Biến tại `Lv. 5` hoặc `Lv. 15`, hệ thống tự động mở khóa thêm **2–3 thẻ thường độc quyền** mang hiệu ứng cộng hưởng vào danh sách bốc thẻ ở các level thường tiếp theo.

3. **Nhịp Độ Bùng Nổ Sức Mạnh (Pacing Curve):**
   * Sức mạnh của người chơi không tăng theo dạng tuyến tính đều đặn mà trải qua **3 bậc thang đột biến rõ rệt**:
     * **Bậc 1 (Phút thứ 3):** Ứng phó mật độ lính tinh anh xuất hiện đợt đầu.
     * **Bậc 2 (Phút thứ 7):** Đối phó các đợt quái số lượng lớn áp đảo.
     * **Bậc 3 (Phút thứ 12+):** Thần hóa bộ kỹ năng để nghênh chiến Boss tối thượng.
