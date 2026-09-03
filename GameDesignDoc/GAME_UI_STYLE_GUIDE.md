# KIM CHỈ NAM THIẾT KẾ GIAO DIỆN NGƯỜI DÙNG (GAME UI STYLE GUIDE)
**Dự án:** VONG XUYÊN (Project Zombie)  
**Nền tảng mục tiêu:** Mobile Android (Landscape 16:9, Tối ưu hóa 19.5:9 & Tablet 4:3)  
**Phiên bản tài liệu:** 2.0 (Chuẩn hóa toàn diện 7 Trụ Cột UI/UX)

---

## 🏛️ 1. TỔNG QUAN & ĐỊNH HƯỚNG (OVERVIEW & THEME)

### 1.1. Phong Cách Cốt Lõi (Art Direction)
* **Phong cách tổng thể:** **Cổ Phong Đông Sơn - Anime Chibi Roguelite (Vietnamese Folklore Dark Fantasy)**.
* **Ngôn ngữ hình học & Cảm giác chất liệu:**
  * **Khung & Nền:** Kết hợp giữa chất liệu **Giấy Dó/Sớ Cổ** nhuộm mực khói tối, **Gỗ Mun Cẩm Lai** sẫm góc cạnh đầm chắc, viền kim khí **Đồng Thau Đông Sơn** chạm khắc hoa văn Mây Cuộn, Chim Lạc, Mặt Trời.
  * **Điểm nhấn tương tác:** Ngọc Bích phong thủy, Chuỗi Cổ Tiền đục lỗ vuông, Dây thắt chỉ đỏ Chu Sa, Bát Quái Thái Cực.
  * **Thẩm mỹ Chibi:** Đậm nét, đường viền ngoài đen than dày 2-4px (`#1A1615`), cell-shading 2 mảng phẳng (flat 2-tone matte), hoàn toàn không dùng bóng đổ tả thực 3D hay dải màu airbrush mờ nhạt.

### 1.2. Trải Nghiệm Người Dùng (UX Pillars)
1. **Khả Năng Nhận Diện Tức Thì (Instant Readability in Combat):**
   * Trong bối cảnh chiến trường tối, hỗn chiến trên 200 quái vật cùng hiệu ứng kỹ năng đạn đạo dồn dập, toàn bộ thông tin sống còn (Máu, Kỹ năng nộ, Lướt né, Báo động đỏ) bắt buộc phải nổi bật trên nền chiến đấu với viền bao bọc tương phản cao (High Contrast 1px Outline / Subtle Glow).
2. **Phản Hồi Xúc Giác Nhanh Dưới 0.1s (Ultra-fast Touch Feedback < 100ms):**
   * Mọi thao tác chạm (Touch Down) trên Joystick và nút bấm hành động phải phản hồi cơ học tức thì: nút lún xuống 2–3px, đổi màu viền sang đỏ Chu Sa hoặc vàng hổ phách, đi kèm âm thanh đanh thép (tiếng gõ lệnh bài gỗ hoặc tiếng kim loại lách cách).
3. **Quy Tắc Ngón Tay Cái (Thumb Zone First):**
   * 90% thao tác tương tác thời gian thực trong trận đấu nằm trọn vẹn trong vùng quạt ngón tay cái hai bên đáy màn hình (Joystick góc trái dưới, Nút Đánh / Kỹ năng / Lướt góc phải dưới). Các thông số thông tin tĩnh (Thời gian, Số quái hạ gục, Cài đặt) đặt ở cạnh trên.

---

## 🎨 2. QUY ĐỊNH VỀ MÀU SẮC (COLOR PALETTE & DESIGN TOKENS)

Áp dụng quy tắc phối màu chuẩn **60 - 30 - 10** trong thiết kế Game UI:

```
┌───────────────────────────────────────┬──────────────────────┬──────────┐
│      60% Nền & Khung Lớn (Primary)    │  30% Bổ Trợ (Second) │ 10% Điểm │
│        Giấy Dó Tối / Gỗ Mun Sẫm       │  Đồng Thau / Giấy Sớ │ Nhấn Rực │
└───────────────────────────────────────┴──────────────────────┴──────────┘
```

### 2.1. Bảng Màu Hệ Thống (System Tokens)

| Phân Vùng | Tên Token | Mã Hex | RGB | Tỉ Lệ | Ứng Dụng Thực Tế |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Chủ Đạo** | `UI_BG_DARK` | `#15131A` | `(21, 19, 26)` | **60%** | Nền màn hình chính, nền popup, backdrop mờ khi dừng game. |
| **Chủ Đạo** | `UI_SURFACE_WOOD` | `#231B18` | `(35, 27, 24)` | **60%** | Thân thẻ bài, panel con, thanh xà gỗ header. |
| **Phụ Trợ** | `UI_BORDER_BRONZE`| `#C9A86A` | `(201, 168, 106)`| **30%** | Khung viền kim khí 9-slice, đường viền thẻ bài, viền HUD. |
| **Phụ Trợ** | `UI_PARCHMENT_LIGHT`| `#EAD8B7`| `(234, 216, 183)`| **30%** | Nền cuộn sớ, giấy điệp ghi chi tiết chỉ số thuộc tính. |
| **Phụ Trợ** | `UI_BORDER_MUTED` | `#5A4D41` | `(90, 77, 65)` | **30%** | Đường kẻ phân tách (divider), ô chứa đồ chưa mở. |
| **Điểm Nhấn**| `UI_ACCENT_CHUSA` | `#D13838` | `(209, 56, 56)` | **10%** | Thanh máu HP, nút báo động, icon Thất bại, dấu triện đỏ. |
| **Điểm Nhấn**| `UI_ACCENT_JADE`  | `#4DEEEA` | `(77, 238, 234)` | **10%** | Thanh EXP, viền ngọc bích thẻ hiếm, slot trang bị đang chọn. |
| **Điểm Nhấn**| `UI_GLOW_GOLD`    | `#FFD700` | `(255, 215, 0)` | **10%** | Nút Xuất Trận, thẻ bài Tiến Hóa, hào quang sao nâng cấp. |

### 2.2. Màu Sắc Trạng Thái Chức Năng (Feedback States)
* 🟢 **Thành Công / An Toàn / Hồi Phục:** Xanh Lục Bảo (`#4C7A3D` / `#5CD65C`) — Sử dụng cho lượng máu hồi phục, nâng cấp thành công.
* 🔴 **Lỗi / Máu Nguy Cấp / Thất Bại:** Đỏ Chu Sa (`#D13838` / `#FF3B30`) — Sử dụng cho máu dưới 25%, cảnh báo Boss xuất hiện, màn hình Game Over Defeat.
* 🟡 **Cảnh Báo / Thời Gian Chờ:** Vàng Hổ Phách (`#FFA000` / `#FFCC00`) — Đếm ngược đợt quái, thông báo nạp đạn.

### 2.3. Bảng Màu Ngũ Hành Chuẩn Hóa (Hỗ Trợ Người Mù Màu)
Bắt buộc đồng bộ mã màu kèm ký hiệu hình khối hình học đặc thù:
* 🔷 **Hệ Kim (`#E8C468`):** Hình Thoi / Lưỡi Kiếm — Sát thương chí mạng, sắc bén.
* 🔺 **Hệ Mộc (`#4C7A3D`):** Hình Gân Lá / Tam Giác — Hồi phục, trói chân, độc tố.
* 💧 **Hệ Thủy (`#2E6E9E`):** Hình Giọt Nước — Làm chậm, đóng băng, liên hoàn.
* 🔥 **Hệ Hỏa (`#B8442C`):** Hình Ngọn Lửa — Sát thương thiêu đốt, bộc phá.
* 🟫 **Hệ Thổ (`#8C6239`):** Hình Khối Vuông / Núi Đá — Giáp hộ thân, kiên cố, choáng.

---

## 📐 3. BỐ CỤC & TỶ LỆ (LAYOUT & GRID SYSTEM)

### 3.1. Khung Lưới (8pt Grid & Spacing Standard)
Mọi kích thước khoảng cách (Padding, Margin, Gap) tuân thủ nghiêm ngặt **hệ số bội của 8px** (với tiểu tiết dùng 4px):

| Tên Spacing | Giá Trị (px) | Ứng Dụng |
| :--- | :--- | :--- |
| `Space-XXS` | `4px` | Khoảng cách viền trong icon, độ dày đường viền chia slot. |
| `Space-XS`  | `8px` | Khoảng cách giữa icon và chữ số, lề trong ô chứa đồ nhỏ. |
| `Space-SM`  | `16px` | Khoảng cách giữa các nút điều hướng phụ, padding trong card nâng cấp. |
| `Space-MD`  | `24px` | Khoảng cách giữa 3 thẻ bài nâng cấp, padding lề bảng hộp thoại. |
| `Space-LG`  | `32px` | Khoảng cách giữa các khối component lớn (Header tới Body). |
| `Space-XL`  | `48px` | Khoảng cách vùng điều khiển chiến đấu tới mép màn hình. |

### 3.2. Vùng An Toàn Màn Hình (Safe Zones)
* **Kích thước tham chiếu gốc (Canvas Reference):** `1920×1080` (Full HD Landscape 16:9), `Match Width Or Height: 0.5`.
* **Safe Zone Padding:**
  * **Cạnh Trái / Cạnh Phải (Tai thỏ / Camera Notch / Thanh cử chỉ):** Cách mép màn hình tối thiểu `64px` (tương đương 48dp).
  * **Cạnh Dưới (Home Bar):** Cách mép tối thiểu `32px`.
  * **Cạnh Trên (Status Bar):** Cách mép tối thiểu `24px`.
* Tuyệt đối không đặt nút bấm quan trọng hoặc joystick sát mép 0px của màn hình.

### 3.3. Quy Tắc Co Giãn Đa Màn Hình (Responsive Rules)
* **Màn hình siêu dài (20:9, 21:9 - Xperia, Galaxy Ultra):** Khóa cụm HUD Top neo ở giữa (`Top-Center`), Joystick neo góc `Bottom-Left`, Nút kỹ năng neo góc `Bottom-Right`. Nền Arena (`BG_VongXuyen_Forest_Hub`) mở rộng tràn viền theo tỉ lệ Cover.
* **Màn hình Tablet / iPad (4:3, 3:2):** Các bảng pop-up nâng cấp và Tàng Bảo Các tự động scale tỉ lệ vừa vặn trong khung nhìn, không tràn ra ngoài chiều dọc màn hình.

---

## 🔤 4. QUY ĐỊNH VỀ PHÔNG CHỮ (TYPOGRAPHY)

### 4.1. Họ Phông Chữ (Font Families)
Dự án giới hạn tối đa **2 phông chữ** để đảm bảo tối ưu hóa Texture Atlas và chống rối mắt:
1. **Phông Tiêu Đề Cổ Điển:** `GameFont_Vietnamese_SD` (Bake sẵn full bảng ký tự tiếng Việt có dấu, mang nét thư pháp sắc bén, hào hùng).
2. **Phông Thông Số & Đọc Nhanh:** `LiberationSans SDF` / Fallback `GameFont_Vietnamese_SD` (Nét tròn trịa, đều đặn, không chân, dễ đọc chữ số ở kích cỡ nhỏ).

### 4.2. Hệ Thống Cấp Bậc Văn Bản (Typographic Hierarchy)

| Cấp Bậc | Font & Style | Size (px) | Line Height | Màu Sắc | Vị Trí Ứng Dụng |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **H1 - Đại Tiêu Đề** | `GameFont_SD` Bold | `36 - 42` | `48px` | `#FFD700` (Vàng Kim) | Tiêu đề Pop-up lớn (THIÊN CƠ ĐỘT PHÁ, ĐẠI THẮNG). |
| **H2 - Trung Tiêu Đề**| `GameFont_SD` Bold | `24 - 28` | `32px` | `#EAD8B7` (Vàng Sớ) | Tên thẻ bài, Tên vũ khí, Tiêu đề Tàng Bảo Các. |
| **H3 - Tiểu Mục** | `GameFont_SD` Medium | `18 - 20` | `24px` | `#C9A86A` (Đồng Thau) | Tên chỉ số (Sát thương, Hồi chiêu, Cấp 5 sao). |
| **Body Text (Mô tả)**| `GameFont_SD` Regular | `14 - 16` | `20px` | `#F0EAE1` (Trắng Ngà) | Diễn giải chi tiết hiệu ứng kỹ năng trên thẻ. |
| **HUD Numbers (Số)** | `Sans SDF` Bold + Outline | `20 - 24` | `24px` | `#FFFFFF` viền `#000` | Số lượng máu (1250/1500), Thời gian (08:45), Kills. |
| **Micro Caption** | `Sans SDF` Regular | `11 - 12` | `14px` | `#A89F91` (Xám Đồng) | Đơn vị tính, chỉ số phụ dưới icon. |

---

## 🧩 5. THƯ VIỆN THÀNH PHẦN UI (UI COMPONENTS & 5 STATES)

Mỗi thành phần UI tương tác bắt buộc hỗ trợ đầy đủ **5 trạng thái trực quan**:
1. `Default`: Trạng thái nghỉ ban đầu.
2. `Hover`: Con trỏ rà chuột qua (áp dụng bản PC / Preview).
3. `Pressed / Active`: Ngón tay đang nhấn giữ trực tiếp.
4. `Selected`: Đang được chọn trong danh sách (Loadout / Thẻ nâng cấp).
5. `Disabled`: Bị khóa, chưa đủ tiền hoặc đang trong trạng thái hồi chiêu.

### 5.1. Nút Bấm (Buttons)
* **Nút Chính (Primary CTA - Ví dụ: `Btn_Battle_Hex_Amber_Glow`):**
  * *Default:* Khối lục giác gỗ viền đồng, ánh sáng hổ phách ấm áp.
  * *Pressed:* Thu nhỏ `scale = 0.94`, bừng sáng viền `+30% Brightness`.
  * *Disabled:* Nền xám tro nứt rạn (`#4A4A4A`), viền mờ tối, không nhận touch.
* **Nút Phụ Hệ Thống (Secondary Buttons - Ví dụ: `Btn_GoMun_Dark`, `Btn_SonMai_ChuSa`):**
  * Dạng thanh thỏi 9-slice bo góc triện đồng. Khi bấm lún sâu 2px, đổi màu nền nhẹ.

### 5.2. Thanh Trạng Thái (Progress Bars & Gauges)
* **Cấu trúc 3 Layer:**
  1. `Background (Bottom):` Lòng máng rãnh tối (`#15131A`).
  2. `Fill (Middle):` Dải màu Gradient phát quang (HP: Đỏ Chu Sa `#D13838`, EXP: Xanh Ngọc `#4DEEEA`).
  3. `Frame (Top):` Khung ống đồng chạm khắc 9-Slice (`Bar_HUD_Frame_VongXuyen_9Slice`).
* **Hiệu ứng cạn máu:** Khi nhận sát thương lớn, thanh máu có 1 thanh phụ màu trắng/vàng tụt chậm lại phía sau trong 0.4s (Damage Ghost Bar).

### 5.3. Bảng Biểu & Cửa Sổ (Popups & Modals)
* **Thẻ Bài Nâng Cấp (`Card_Upgrade_Wood_Totem_9Slice`):**
  * Tỉ lệ dọc `260×360px`.
  * Khung viền gỗ chạm mây 4 góc; khi chọn, viền nổi dải hào quang ngọc bích lấp lánh.
* **Bảng Game Over Thất Bại (`Panel_DongSon_GameOver`):**
  * Khung vuông viền hoa văn Đông Sơn 4 góc, nền tối mờ 85% tạo chiều sâu tập trung vào bảng thống kê.

---

## 🏷️ 6. QUY ĐỊNH VỀ BIỂU TƯỢNG & TÀI NGUYÊN (ICONS & ASSETS)

### 6.1. Khung Giới Hạn Chuẩn (Bounding Box)
Tất cả các icon trong dự án phải được thiết kế và xuất file đúng trong các khung lưới cố định:
* **Icon Chiến Đấu HUD / Chỉ số:** `64×64px` (Vùng an toàn đồ họa bên trong `52×52px`).
* **Icon Huy Hiệu Nguyên Tố Ngũ Hành / Cổ Tiền:** `128×128px` hoặc `256×256px`.
* **Icon Kỹ Năng / Pháp Bảo:** `84×84px` hoặc `128×128px`.

### 6.2. Đường Nét & Phong Cách Đồ Họa (Stroke & Shading)
* **Đổ mảng đặc (Solid Art):** Toàn bộ icon là dạng hình họa đặc tả rõ khối, không dùng icon dạng dây chỉ mảnh (Line Icon) vì sẽ bị chìm hoàn toàn trên nền game.
* **Viền bao bọc (Outer Stroke):** Viền ngoài màu đen than hoặc đồng sẫm dày `2px - 4px` bao bọc toàn bộ chu vi icon.
* **Nền trong suốt:** File xuất ra phải đạt chuẩn **RGBA 32-bit**, nền trong suốt tuyệt đối ($\alpha = 0$), không để lại quầng mờ màu xám (Zero Halo).

---

## ✨ 7. HIỆU ỨNG ĐỘNG & PHẢN HỒI (ANIMATION & VFX)

### 7.1. Thời Gian Chuyển Động (Timing & Easing Curves)
Tốc độ giao diện trong game Roguelite hành động phải dứt khoát, không gây ức chế cho người chơi:

| Hành Động UI | Thời Gian (ms) | Easing Curve | Mô Tả Chuyển Động |
| :--- | :--- | :--- | :--- |
| **Mở Popup / Modal** | `250ms - 300ms` | `EaseOutBack` | Bảng phóng to từ `0.8 -> 1.05 -> 1.0` kèm mờ nền. |
| **Đóng Popup** | `150ms - 200ms` | `EaseInCubic` | Thu nhỏ nhanh và biến mất dứt khoát. |
| **Chạm Nút (Touch Down)** | `50ms` | `Linear` | Co nhỏ nhanh `scale = 0.94`. |
| **Nhả Nút (Touch Up)** | `100ms` | `EaseOutQuad` | Bật nảy về `scale = 1.0`. |
| **Thanh Máu / EXP trượt**| `200ms` | `EaseOutQuad` | Thanh Fill lướt mượt mà đến giá trị mới. |

### 7.2. Hiệu Ứng Tương Tác Vi Mô (Micro-Interactions)
1. **Lóe Sáng Kỹ Năng Hồi Chiêu (Skill Ready Pulse):** Khi nút nộ hoặc lướt hết thời gian đếm ngược, viền nút bung ra một vòng sóng xung kích màu vàng kim mờ dần (`Scale 1.0 -> 1.4, Alpha 1.0 -> 0`).
2. **Hiệu Ứng Lật Thẻ Bài Nâng Cấp:** Khi cấp độ tăng, 3 thẻ bài rơi từ trên xuống so le nhau `60ms` kèm tiếng đập thẻ gỗ lên mặt bàn.
3. **Thái Cực Xoay Cảm Ứng:** Núm xoay Joystick (`Joystick_Knob_Taiji`) tự động xoay nhẹ nhàng theo góc nghiêng di chuyển của ngón tay người chơi.

---

## 🎯 DANH MỤC THAM CHIẾU TÀI LIỆU & FILE DỰ ÁN
* Bộ ảnh phân loại thực tế: [`Assets/Art/UI/_AtlasOverview/`](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Art/UI/_AtlasOverview/)
* Hướng dẫn Prompt AI UI: [`GameDesignDoc/UI_PROMPT_AND_ART_GUIDE.md`](file:///c:/Users/thuon/Unity/Projectzombie/GameDesignDoc/UI_PROMPT_AND_ART_GUIDE.md)
* Script sinh tự động UI 9-Slice: [`Assets/Editor/VFX/generate_ui_framework.py`](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Editor/VFX/generate_ui_framework.py)
* Công cụ cấu hình Font Tiếng Việt: [`Assets/Editor/TMPVietnameseFontSetupTool.cs`](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Editor/TMPVietnameseFontSetupTool.cs)
