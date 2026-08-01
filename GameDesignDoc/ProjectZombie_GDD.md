# Game Design Document — Dự Án: VONG XUYÊN

**Phiên bản:** 4.0 (Official Single Source of Truth — Google Play Store Release)  
**Thể loại:** Top-down Survival Roguelite / Auto-battler (Cơ chế Ngũ Hành & Âm Dương)  
**Nền tảng:** Android Mobile (Google Play Store — Target API 33+, IL2CPP ARM64, AAB Package)  
**Phong cách đồ họa:** 2D Top-down, Mỹ thuật dân gian Việt Nam (Tranh Đông Hồ / Hàng Trống cách điệu, tông màu u linh)  

---

## 1. Tổng quan (Overview)

### 1.1. Tầm nhìn sản phẩm (Vision Statement)
**Vong Xuyên** đưa người chơi vào hành trình sinh tồn nghẹt thở giữa cõi âm ty Việt Nam — nơi hồn ma, quỷ dữ và yêu tinh dân gian trỗi dậy từ truyền thuyết. Người chơi không chỉ "cày điểm sát thương" mà phải thấu hiểu quy luật Ngũ Hành để chế ngự từng loài yêu quái, đồng thời giữ cán cân Âm Dương trong tâm để tồn tại tới khi đối mặt Diêm Vương.

### 1.2. Đối tượng người chơi & Phong cách Đồ họa (Target & Graphic Specs)
- **Người chơi mục tiêu:** Casual/mid-core yêu thích thể loại survivor-like trên Android. Phiên chơi 10–20 phút/run.
- **Phong cách Đồ họa Pixel Art 2D Top-down (Top-down 3/4 View):**
  - **Góc nhìn (Perspective):** **Top-down 3/4 view** (thấy mặt trước/trang phục dân gian áo the, khăn đóng, cà sa, pháp bảo).
  - **Tỉ lệ cơ thể (Body Ratio):** **Chibi hóa nhẹ (1:3 đến 1:4)** giúp silhouette dễ đọc khi màn hình cực kỳ đông quái (200 enemy).
  - **Kích thước Sprite Canvas:** Nhân vật/Quái: **32×32px đến 48×48px**. Tilemap: **16×16px hoặc 32×32px**. Giới hạn tối đa **64px** (trừ Boss) để giữ AAB dưới 60MB.
  - **Viền Outline:** Viền đen mỏng **1px** bao quanh nhân vật/quái để tách khỏi nền tối.
- **Hệ thống Hướng Nhân Vật (Directional Flip System — 2 Hướng):**
  - Chỉ vẽ **1 bộ animation gốc theo hướng quay Phải (Facing Right)**.
  - Sử dụng `SpriteRenderer.flipX = true` trong C# Unity để lật sang Trái. Đi Lên/Xuống dùng sprite ngang gần nhất.
  - **Quy tắc thiết kế:** Trang phục đối xứng, chấp nhận đổi tay cầm vũ khí khi flipX để tối ưu thời gian sản xuất.

### 1.3. Điểm khác biệt độc quyền (Unique Selling Points - USPs)
- **Cơ chế Ngũ Hành (Kim - Mộc - Thủy - Hỏa - Thổ):** Buộc người chơi xoay chuyển build theo vòng Tương Khắc (+30% Sát thương) và Tương Sinh (-20% Cooldown).
- **Cán Cân Âm Dương:** Trạng thái lối chơi (Âm thịnh / Dương thịnh / Thái Cực Cân bằng) tự động lọc và mở khóa các thẻ nâng cấp Gacha độc quyền theo thời gian thực.
- **Chất liệu văn hóa dân gian Việt Nam:** Pháp bảo cổ đại, Yêu ma truyền thuyết, và Boss Âm Ty (Ngưu Đầu Mã Diện, Diêm Vương).
- **Kiến trúc Kỹ thuật Di Động Tối Ưu:** Nền tảng Offline-first Local Save JSON, Object Pooling 0 GC Allocation, chạy mượt 60 FPS trên chip ARM64.

### 1.4. Yêu cầu kỹ thuật tối thiểu (Target Performance - Android)
- **Target OS:** Android 8.0 (API Level 26)+; Target SDK API 33+ (Android 13/14).
- **Architecture:** ARM64-v8a (IL2CPP Scripting Backend).
- **Target FPS:** 60 FPS ổn định trên thiết bị tầm trung.
- **Kích thước App:** Target APK/AAB dưới 60MB.
- Hỗ trợ tối thiểu **200 Enemy + 100 Projectile** đồng thời không tụt FPS.

### 1.5. Danh sách tính năng (MVP vs Full Release)

| Hệ thống | Bản phát hành đầu (MVP) | Bản cập nhật tương lai (Update) |
|---|---|---|
| **Character (Nhân vật)** | 3 (Thư Sinh, Đạo Sĩ, Võ Tăng) | 8 (Bổ sung Pháp Sư, Thầy Mo, Kiếm Khách...) |
| **Vũ khí (Pháp bảo)** | 12 Pháp Bảo, chia đều 5 hệ Ngũ Hành | 30 Pháp Bảo + biến thể |
| **Passive (Kỹ năng bị động)** | 18 (Cộng chỉ số nền + Thẻ Ngũ Hành) | 40 |
| **Boss (Trùm)** | 2 (Ngưu Đầu Mã Diện, Diêm Vương) | 6 (Bổ sung Hồ Ly Tinh, Thần Trùng, Bạch Xà...) |
| **Map (Bản đồ)** | 1 (Bến Đò Vong Xuyên - Bounded Arena) | 5 (Rừng Ma, Đầm Lầy Ma Da, Địa Ngục Môn...) |
| **Cơ chế Ngũ Hành** | Có (Tương khắc + Tương sinh) | Mở rộng Combo liên hệ |
| **Cán cân Âm Dương** | Có (Pool Âm / Pool Dương / Thẻ Thái Cực) | Mở rộng hiệu ứng bản đồ theo Âm Dương |
| **Daily Quest** | Không | Có (Thưởng Cổ Tiền & Reroll Token) |
| **Monetization** | Rewarded Ads (x2 Cổ Tiền / Reroll) | Rewarded Ads + IAP Skin/Character |

---

## 2. Vòng Lặp Gameplay (Core Gameplay Loop)

### 2.1. Vòng lặp trong 1 trận (Moment-to-moment Loop)
1. **Sinh tồn & Tấn công tự động:** Di chuyển né tránh kẻ địch qua Dynamic Virtual Joystick. Pháp bảo tự động tấn công theo tầm và cooldown.
2. **Thu thập & Lên cấp:** Tiêu diệt Yêu ma rớt Hạt Kinh Nghiệm (Exp Gem) để Lên cấp (Level Up).
3. **Nâng cấp Gacha Ngũ Hành & Âm Dương:** Khi lên cấp, game tạm dừng hiển thị 3 thẻ nâng cấp ngẫu nhiên. Danh sách thẻ được lọc theo **Thuộc tính Ngũ Hành** và **Trạng thái Cán cân Âm Dương** hiện tại.
4. **Tiến hóa (Evolution):** Khi Pháp bảo đạt cấp 5 (Max Level) và người chơi sở hữu Thẻ Passive tương ứng, Pháp bảo tiến hóa thành phiên bản Tối Thượng.
5. **Kết thúc trận:** Thắng khi diệt Boss Diêm Vương ở mốc 20:00, hoặc Thất bại khi hết HP.

### 2.2. Vòng lặp Meta & Save Game
1. Kết thúc trận $\rightarrow$ Quy đổi điểm số thành **Cổ Tiền** (tiền xu cổ Việt Nam) tích lũy vĩnh viễn.
2. Dùng Cổ Tiền mở khóa: Nhân vật mới, Cây nâng cấp chỉ số vĩnh viễn (Permanent Upgrade Tree).
3. **Tự động lưu:** `SaveSystem.Save()` mã hóa JSON lưu xuống đĩa di động (`Application.persistentDataPath`).

### 2.3. Thiết kế Onboarding & Trải Nghiệm Người Chơi Mới (First-Run Micro-Learning)

Để giảm tải áp lực thông tin cho đối tượng Casual/Mid-Core mà vẫn truyền tải trọn vẹn 2 cơ chế Tương Khắc / Tương Sinh & Âm Dương, game áp dụng chiến lược **Mở khóa lũy tiến (Progressive Mechanics Unlocking)** và **Dạy bằng Phản hồi Thị giác (Visual-First Feedback)** mà không ép người chơi đọc văn bản dài:

#### 2.3.1. Lộ trình mở khóa cơ chế theo lượt chơi (Progressive Unlocking)
- **Run 1 — Trận Khởi Đầu (Minute 00:00 – 03:00):**
  - **Tối giản UI:** Chỉ hiển thị Joystick di chuyển + Thanh Máu/Exp + 1 Nút Signature Skill.
  - **Vòng Tương Khắc (×1.3 Damage):** Khi vũ khí đánh trúng Yêu ma bị khắc hệ, hiển thị hiệu ứng Damage Popup màu **Vàng Kim rực rỡ** (`#FFD700`) kèm biểu tượng `✦ Khắc` thu nhỏ. Tooltip gợi ý dạng banner mờ 3s xuất hiện duy nhất 1 lần: *"Vũ khí hệ Kim gây thêm +30% sát thương lên Yêu ma hệ Mộc!"*.
- **Run 1 — Phút thứ 03:00+:**
  - **Vòng Tương Sinh (-20% Cooldown):** Khi người chơi nhặt thêm Pháp bảo thứ 2 và tạo chuỗi Tương Sinh, 2 icon thuộc tính nối vệt sáng bay lên đầu nhân vật kèm âm thanh *Ting!* rộn rã. Banner mờ 3s xuất hiện: *"Tương Sinh kích hoạt! Vũ khí được giảm 20% hồi chiêu!"*.
- **Run 2+ (Sau khi hoàn thành hoặc thất bại trận đầu):**
  - Mới kích hoạt hiển thị **Thanh Cán Cân Âm Dương** trên HUD và hệ thống Thẻ **Tiến Hóa (Evolution)** khi vũ khí đạt Level 5.

#### 2.3.2. Chuẩn hóa Màu sắc, Ký hiệu Hình khối (Colorblind Support) & Palette Dân gian
- **Nguyên tắc Tổng thể Bảng màu & Tương phản (Visual Readability):**
  - *Nền / Môi trường:* Tông tối, desaturated (xám xanh, nâu bùn, tím đen) — truyền tải đúng chất u linh cõi Âm Ty.
  - *Vật thể tương tác (Nhân vật / Yêu ma / Đạn bay / VFX):* Sáng và saturated hơn nền rõ rệt (chênh 1 – 2 bậc độ sáng) — quy tắc bắt buộc để mắt người chơi phân biệt vật thể khỏi nền tĩnh trong trận chiến 150-200 quái.
  - *Giới hạn Palette:* Giữ tối đa **4 – 6 màu chủ đạo** xuyên suốt game để giữ tinh thần palette giới hạn của tranh dân gian (Đông Hồ / Hàng Trống).

- **Mã màu & Ký hiệu Hình khối Thuộc tính Ngũ Hành (Colorblind Accessibility):**  
  Mỗi hệ Ngũ Hành luôn đi kèm **Ký hiệu hình khối riêng** — không dùng màu sắc làm phương tiện phân biệt duy nhất để tránh rủi ro cho người mù màu (đặc biệt cặp Hỏa / Mộc):
  - ✦ **Kim:** Mã màu `#E8C468` (Glow: `#FFF3C4`) — 🔷 **Hình Thoi / Lưỡi Kiếm**
  - 🌿 **Mộc:** Mã màu `#4C7A3D` (Glow: `#8FC97A`) — 🔺 **Hình Lá / Tam Giác Nhọn**
  - 🌊 **Thủy:** Mã màu `#2E6E9E` (Glow: `#7FCBEA`) — 💧 **Hình Giọt Nước**
  - 🔥 **Hỏa:** Mã màu `#B8442C` (Glow: `#FF8A50`) — 🔥 **Hình Ngọn Lửa**
  - 🪨 **Thổ:** Mã màu `#8A6A3E` (Glow: `#C9A876`) — 🟩 **Hình Vuông / Khối Đất**

- **Palette Mỹ Thuật Dân Gian & Phân Bổ Họa Tiết:**
  - Sử dụng bộ màu dân gian truyền thống: **Đỏ son, Vàng đất, Nâu gụ, Đen mực nho**.
  - **Quy tắc phân bổ:** Dồn họa tiết dân gian phức tạp (Tranh Đông Hồ / Hàng Trống) vào **UI Canvas, Arena Background và Boss** (kích thước lớn đủ chứa chi tiết). Giữ nhân vật và quái nhỏ đơn giản để bảo đảm nhịp nhìn snappy 60 FPS.
- **HUD Quick Reference Wheel (Vòng Bát Quái Tra Cứu Nhanh):**
  - Trên màn hình Run HUD, bố trí 1 nút icon **Vòng Bát Quái nhỏ** ở góc màn hình. Khi ngón tay chạm giữ (Hold Touch), một bánh xe Ngũ Hành xoay tròn hiển thị ngắn gọn sơ đồ Tương Khắc (mũi tên đỏ) & Tương Sinh (mũi tên xanh) giúp người chơi tra cứu nhanh trong 1 giây mà không phải mở Pause Menu.

---

## 3. Hệ Thống Nhân Vật (Player System)

### 3.1. Roster Nhân Vật MVP — Bảng cập nhật & Định hướng Hình ảnh

| Nhân vật | Vũ khí khởi đầu | Hệ khởi điểm | Signature Skill (Kỹ năng chủ động) | Cooldown | Cơ chế lõi tương tác |
|---|---|---|---|---|---|
| **Thư Sinh** | Bút Phán Quan | Kim | **"Phán Quyết Tiền Định"** — Chèn 1 hit ảo hệ tùy chọn vào buffer Tương Sinh | 25s | `recentElementHits` (mục 4.2.2) |
| **Đạo Sĩ** | Bùa Trấn Yêu | Mộc | **"Bát Quái Trận Đồ"** — Khóa pathing quái trong vùng + ép cân bằng Âm Dương | 30s | `yinYangValue` + Enemy Pathing (mục 6.1) |
| **Võ Tăng** | Thiền Trượng | Thổ | **"Phá Giới Chấn Thế"** — Hy sinh HP đổi lấy chấn động tỉ lệ + đẩy cực Dương | 20s | `PlayerStats.HP` + `yinYangValue` (mục 6.1) |

#### 3.1.0. Định hướng Hình ảnh & Archetype Nhân vật (Art & Visual Direction)

*   **Thư Sinh (Hệ Kim):**
    *   *Archetype:* Văn nhân / Scribe cổ trang — dáng gầy, thư sinh, nho nhã, tay cầm Bút Phán Quan cỡ lớn.
    *   *Tông màu:* Vàng kim (`#E8C468`) làm điểm nhấn trên nền áo the / khăn đóng màu xám nhạt hoặc trắng ngà.
    *   *Chi tiết Idle:* Bút Phán Quan phát sáng nhẹ ánh vàng kim ở đầu bút khi đứng yên.
*   **Đạo Sĩ (Hệ Mộc):**
    *   *Archetype:* Đạo sĩ / Pháp sư cổ trang — áo choàng dài, râu tóc búi đạo gia, tay cầm phất trần hoặc lá bùa.
    *   *Tông màu:* Xanh lá mộc (`#4C7A3D`) trên áo choàng, phối thêm nâu gụ ở phụ kiện (giỏ đựng bùa, dây lưng).
    *   *Chi tiết Idle:* Vài lá bùa giấy bay lơ lửng lừng lơ quanh người ở trạng thái idle.
*   **Võ Tăng (Hệ Thổ):**
    *   *Archetype:* Tăng nhân võ thuật — dáng người chắc khỏe, cơ bắp cuồn cuộn, tay cầm Thiền Trượng đồng, ngực trần hoặc áo cà sa lệch vai gọn.
    *   *Tông màu:* Nâu đất (`#8A6A3E`) chủ đạo, điểm nhấn đỏ son (`#C0392B`) ở dây chuỗi tràng hạt / khăn quấn tay.
    *   *Chi tiết Idle:* Tư thế idle hơi khom, thủ thế vững chãi.

#### 3.1.4. Quy Chuẩn Khối Lượng Animation & Tối Ưu Sản Xuất (Animation Budget & Base Rig)

*   **Bảng Ngân Sách Frame Tối Thiểu per Nhân Vật:**
    *   `Idle`: 2 – 4 frames (Nhịp thở nhún nhẹ).
    *   `Walk`: 4 – 6 frames (Dáng bước di chuyển).
    *   `Attack / Signature Skill`: 3 – 5 frames (Đòn đánh bộc phát snappy).
    *   `Hit-react`: 1 – 2 frames (Ưu tiên dùng `HitFlashShader` làm trắng sprite thay vì tạo animation riêng).
    *   `Death`: 3 – 4 frames (Tan vỡ / ngã gục).
*   **Chiến lược Tối ưu Sản xuất (Base Rig Sharing):**
    *   Sử dụng chung **1 Base Rig / Tỉ lệ cơ thể chuẩn** cho cả 3 nhân vật MVP (chỉ thay đổi trang phục/phụ kiện).
    *   Điều này giúp tái sử dụng toàn bộ Animation State Controller C# (`animator.Play()`), không dùng Animator Transitions mũi tên (tránh Animator Spaghetti theo Mục 10 Rules), giảm 60% chi phí sản xuất Sprite Sheets cho Mobile.

#### 3.1.1. "Phán Quyết Tiền Định" (Thư Sinh)

* **Lore & UX Flow:** Khi kích hoạt, game hiển thị overlay nhỏ (không pause) cho phép người chơi chạm chọn 1 trong 5 icon hệ Ngũ Hành trong 1.5s (nếu không chọn, tự động chọn hệ khớp với vũ khí đang cooldown lâu nhất — Auto-Select Fallback để không làm gián đoạn nhịp chơi).
* **Thông số kỹ thuật:**
  * **Cooldown:** 25s
  * **Cost:** Không tiêu HP/Mana, chỉ giới hạn bằng Cooldown
  * **Hiệu ứng:** Push 1 phần tử `{hệ: <lựa chọn>, timestamp: Time.time, weapon: "SIGNATURE_VIRTUAL"}` vào đầu Queue `recentElementHits`.
  * **Điều kiện kích hoạt Tương Sinh ngay sau đó:** Nếu hit thật tiếp theo của bất kỳ vũ khí nào khớp đúng thứ tự Tương Sinh với hệ vừa chọn (Kim→Thủy→Mộc→Hỏa→Thổ→Kim), proc 20% Instant Cooldown Reduction kích hoạt ngay trên vũ khí đó — không chờ đủ 2 hit thật như cơ chế thường (mục 4.2.2).
  * **Giới hạn Balance:** Hit ảo không tính vào giới hạn "1 proc / 3 giây" của hệ thống Tương Sinh gốc — nó là 1 lần trigger cơ hội độc lập theo cooldown riêng 25s của skill, tránh việc cộng dồn 2 nguồn proc trong cùng cửa sổ.
  * **Feedback UI:** Icon hệ được chọn nổi lên đầu nhân vật với viền vàng kim (khác biệt icon proc thường) trong 1.5s chờ hit thật khớp.

#### 3.1.2. "Bát Quái Trận Đồ" (Đạo Sĩ)

* **Lore & UX Flow:** Tạo một vùng trận đồ hình bát giác cố định tại vị trí nhân vật tại thời điểm kích hoạt (không di chuyển theo người chơi), trong đó quái bị "nhốt" — AI pathing của chúng chuyển sang trạng thái đi vòng theo cạnh bát giác thay vì tìm đường thẳng tới player.
* **Thông số kỹ thuật:**
  * **Cooldown:** 30s
  * **Bán kính vùng hiệu lực:** 4.5m, thời lượng 4s
  * **Enemy Pathing Override:** Trong thời gian hiệu lực, mọi Enemy AI Agent nằm trong vùng bị set `NavAgentState = TrapCircling` (di chuyển bám theo 8 điểm neo của bát giác thay vì hướng thẳng tới Player Transform). Quái spawn mới trong lúc trận còn hiệu lực cũng bị áp trạng thái này nếu bước vào vùng.
* **Tác động yinYangValue:** Trong 4s hiệu lực, giá trị `yinYangValue` được ép nội suy tuyến tính (Lerp) về khoảng 50 (chính giữa Thái Cực) bất kể trạng thái trước đó, disable tạm thời việc cộng/trừ từ hành vi di chuyển/damage trong lúc trận đang chạy. Sau khi hết hiệu lực, hệ thống tính điểm hoạt động lại bình thường từ giá trị 50.
  * **Chủ đích thiết kế (Class Perk):** Việc ép `yinYangValue` về 50 mở ra cửa sổ Thái Cực Cân bằng (40–60) trong 4s cho phép Đạo Sĩ dễ dàng kích hoạt cơ hội xuất hiện thẻ Evolution **"Thái Cực"** đặc biệt khi Level Up trong thời gian trận chạy. Đây là **Đặc quyền Class riêng của Đạo Sĩ** để bù lại sát thương trực tiếp (Base DPS) thấp hơn so với Thư Sinh và Võ Tăng.
* **Giới hạn Balance:** Không áp dụng lên Boss (Boss miễn nhiễm Trận Pathing Override, chỉ áp dụng lên Enemy thường) để tránh trivialize DPS-race của Boss fight.
* **Feedback UI:** Vùng bát giác vẽ bằng Line Renderer/Shader giấy dó mờ, quái bị nhốt có icon xoáy nhỏ trên đầu.

#### 3.1.3. "Phá Giới Chấn Thế" (Võ Tăng)

* **Lore & UX Flow:** Võ Tăng tự đấm vào ngực dồn lực, hy sinh máu để tạo chấn động — risk-reward: skill càng mạnh khi HP hiện tại càng cao, nhưng cũng để lại nhân vật ở mức máu nguy hiểm nếu dùng sai thời điểm.
* **Thông số kỹ thuật:**
  * **Cooldown:** 20s
  * **Cost:** Trừ 30% HP hiện tại của nhân vật tại thời điểm kích hoạt (không phải HP tối đa — để tránh việc spam skill khi máu thấp tự sát).
  * **Guard Condition:** Skill bị khóa (hiển thị mờ, không thể bấm) nếu HP hiện tại < 15% HP tối đa, tránh tình huống tự kết liễu do bấm nhầm.
  * **Công thức sát thương/bán kính chấn động:**
    * $\text{ShockwaveRadius} = 3.0\text{m} + \left(\frac{\text{HP}_{\text{hy sinh}}}{\text{HP}_{\text{Max}}}\right) \times 4.0\text{m}$
    * $\text{ShockwaveDamage} = \text{BaseDamage} \times 2.5 \times \left(\frac{\text{HP}_{\text{hy sinh}}}{\text{HP}_{\text{Max}}}\right)$
    *(Ví dụ: hy sinh 30% của 500 HP Max = 150 HP $\rightarrow$ bán kính $\approx 5.4\text{m}$, sát thương $\approx \text{BaseDamage} \times 0.75$)*
  * **Hiệu ứng CC:** Đẩy lùi (Knockback Force 8m/s) + Choáng 1.2s trong bán kính hiệu lực.
  * **Tác động yinYangValue:** Cộng thẳng +25 điểm tức thời vào `yinYangValue` (đẩy nhanh về cực Dương >80 nếu đang ở mức trung bình-cao), mô phỏng đúng bản chất "phá giới" mất kiểm soát của võ tăng.
  * **Feedback UI:** Màn hình rung nhẹ (Camera Shake) + hiệu ứng nứt vỡ giấy dó lan từ tâm nhân vật.

> [!IMPORTANT]
> **Quy định UX / Control Scheme:** 
> - Vũ khí (12 Pháp Bảo) tấn công hoàn toàn **Tự động (Auto-Attack)** theo tầm và Cooldown.
> - Signature Skill của Nhân vật là kỹ năng **Chủ động (Active Skill)** duy nhất, được kích hoạt bằng nút bấm thủ công (`SignatureSkillButton.cs`) nằm ở góc dưới bên phải màn hình (vùng thao tác của ngón cái tay phải). Nút bấm có hiệu ứng đếm vòng Cooldown (Cooldown Radial Fill) & Glow khi sẵn sàng sử dụng. Kỹ năng hỗ trợ New Input System binding song song với phím `Space` / `E` khi test trên PC.

### 3.2. Chỉ số Nhân vật (Player Stats) & Kiến trúc Dữ liệu
- **Danh mục Chỉ số Cốt lõi:**
  - **Phòng thủ:** HP (Máu), Armor (Giáp), Move Speed (Tốc độ di chuyển).
  - **Tấn công:** Base Damage (Sát thương cơ bản), Attack Speed (Tốc độ đánh), Range (Tầm đánh), Crit Chance (Tỷ lệ chí mạng), Crit Damage (Sát thương chí mạng).
  - **Utility:** Pickup Radius (Bán kính hút Exp Gem), Luck (May mắn gacha), Cooldown Reduction (Giảm hồi chiêu).

> [!NOTE]
> **Kiến trúc Quản lý Baseline & Growth Curve (Balance Sheet Note):**
> - Mọi giá trị chỉ số cơ bản (Baseline Values) và công thức tăng trưởng (Growth Curve per Level / Permanent Upgrade) của từng nhân vật được cấu hình tách biệt trong **ScriptableObject `CharacterStatsConfig.cs`** (hoặc bảng Excel/CSV Balance Sheet nhập vào qua Script Importer).
> - Dev khi cài đặt tầng Model (`PlayerStats.cs`) cần inject dữ liệu từ `CharacterStatsConfig` để tránh hard-code chỉ số trực tiếp trong C# class.

### 3.3. Định Hướng Cân Bằng Đa Dạng Build (Build Diversity & Anti Mono-Element Stacking)

Mặc dù mỗi Nhân vật sở hữu Vũ khí khởi đầu đúng theo thuộc tính bổn mệnh (Thư Sinh — Kim, Đạo Sĩ — Mộc, Võ Tăng — Thổ) để chuẩn hóa Lore, hệ thống cân bằng game được thiết kế triệt để nhằm **ngăn chặn bẫy "Mono-Element Stacking"** (chỉ nhặt vũ khí cùng 1 hệ bổn mệnh), đảm bảo tính đa dạng chiến thuật Roguelite:

1. **Rào Cản Từ Vòng Tương Sinh (Kim $\rightarrow$ Thủy $\rightarrow$ Mộc $\rightarrow$ Hỏa $\rightarrow$ Thổ $\rightarrow$ Kim):**
   - Vòng Tương Sinh bắt buộc phải có **2 đòn đánh thuộc 2 hệ KHÁC NHAU theo đúng thứ tự sinh** để kích hoạt giảm 20% Cooldown (VD: Kim sinh Thủy).
   - Nếu người chơi cố tình stack toàn bộ vũ khí thuần 1 hệ (VD: Thư Sinh chỉ nhặt 6 vũ khí thuộc hệ Kim), người chơi sẽ **HOÀN TOÀN KHÔNG KÍCH HOẠT ĐƯỢC BẤT KỲ PROC TƯƠNG SINH NÀO**, chịu thiệt hại lớn về nhịp xả đạn (DPS over time).

2. **Thách Thức Từ Đội Hình Yêu Ma & Boss Đa Thuộc Tính:**
   - Yêu ma xuất hiện theo đợt và Boss (Ngưu Đầu Mã Diện, Diêm Vương) xoay luân phiên 5 hệ Ngũ Hành. Nếu build thuần 1 hệ, người chơi sẽ bị dính điểm mù khi đối mặt nhóm quái counter và không tận dụng được bonus +30% sát thương Tương Khắc.

3. **Điều Chỉnh Trọng Số Gacha Nâng Cấp (`UpgradeManager` Dynamic Weight):**
   - Khi người chơi đã sở hữu từ 2 vũ khí thuộc cùng 1 hệ trở lên, hệ thống Gacha tự động giảm 50% trọng số xuất hiện của vũ khí hệ đó, đồng thời ưu tiên nâng trọng số của các vũ khí thuộc hệ **Tương Sinh tiếp theo** hoặc các thẻ **Passive yêu cầu cho Evolution** (mục 4.3).

---

## 4. Hệ Thống Vũ Khí — Pháp Bảo & Ngũ Hành

### 4.1. Cơ sở dữ liệu 12 Pháp Bảo MVP

| ID | Tên Pháp Bảo | Hệ | Damage | Cooldown | Số mục tiêu / Phạm vi AoE | Loại Projectile | Evolution (Tối thượng) | Mô tả & Hiệu ứng đặc trưng | Độ hiếm |
|---|---|---|---|---|---|---|---|---|---|
| `W001` | **Nỏ Thần** | Kim | 12 | 0.6s | Single Target (Xuyên 2 mục tiêu) | Straight, xuyên táo | `E001` **Nỏ Liên Châu** | Mũi tên thần An Dương Vương bắn thẳng xuyên táo kẻ địch | Common |
| `W002` | **Bút Phán Quan** | Kim | 20 | 0.8s | Tối đa 3 mục tiêu (Quạt cận chiến) | Melee Slash | `E002` **Bút Sinh Tử** | Nhát chém mang uy lực phán quyết âm ty gây sát thương chí mạng 2 bên | Common |
| `W003` | **Bùa Trấn Yêu** | Mộc | 8 | 0.4s | Xoay quanh người (Tối đa 5 mục tiêu/tick) | Orbit Blade | `E003` **Bùa Cửu Huyền** | Vòng lá bùa thần xoay quanh bảo vệ và đẩy lùi yêu ma | Rare |
| `W004` | **Cửu Vĩ Hồ Trảo** | Hỏa | 18 | 1.2s | 1 mục tiêu / lần thả | Homing | `E004` **Hồ Ly Cửu Vĩ** | Móng vuốt cáo lửa tự tìm diệt quái và hút sinh khí | Rare |
| `W005` | **Trống Đồng Đông Sơn** | Thổ | 8x5 | 1.5s | 5 hướng tỏa rộng (Tối đa 15 mục tiêu) | Spread AoE | `E005` **Trống Trấn Quốc** | Sóng âm trảm linh tỏa rộng 5 hướng gây choáng diện rộng | Common |
| `W006` | **Lựu Đạn Thần Sa** | Hỏa | 45 | 2.5s | Bán kính nổ AoE 3.5m (Vùng thiêu rụi toàn bộ quái trong bán kính) | AoE Explosive | `E006` **Bão Hỏa Diệm** | Hạt thần sa phát nổ tạo bão lửa thiêu rụi vùng rộng (Knockback mạnh) | Epic |
| `W007` | **Cung Thạch Sanh** | Kim | 35 | 1.0s | Xuyên thẳng hàng (Tối đa 8 mục tiêu) | Piercing Bolt | `E007` **Cung Thần Tiễn** | Mũi tên thần lực bối cảnh Thạch Sanh xuyên qua hàng loạt yêu tinh | Rare |
| `W008` | **Đao Cửu Vĩ** | Hỏa | 8/tick | 0.25s | Luồng lửa nón ngắn (Tối đa 5 mục tiêu/tick) | Continuous Stream | `E008` **Hỏa Long Đao** | Luồng rồng lửa thiêu đốt liên tục theo đường thẳng | Rare |
| `W009` | **Trượng Long Vương** | Thủy | 25 | 1.8s | Chain nảy 6 mục tiêu liên tiếp | Chain Lightning-nước | `E009` **Long Vương Trượng** | Sét nước thủy cung lan truyền qua chuỗi 6 yêu quái (Choáng 0.5s/hit) | Epic |
| `W010` | **Linh Phù Ma Da** | Thủy | 10 | 2.0s | Bán kính độc 2.5m (AoE liên tục) | Pet Summon AoE | `E010` **Thủy Cung Linh** | Triệu hồi linh thú Ma Da phun độc sát thương liên tục | Rare |
| `W011` | **Nước Thánh Chùa Hương** | Thổ | 14/sec | 3.0s | Bãi vũng 3.0m (AoE làm chậm & sát thương theo thời gian) | Ground AoE | `E011` **Giếng Thiêng** | Tạo bãi giếng thiêng trên mặt đất làm chậm và gây sát thương liên tục | Rare |
| `W012` | **Phi Tiêu Bát Quái** | Mộc | 22 | 1.4s | Đâm & Quay về (Tối đa 4 mục tiêu/lượt) | Returning Blade | `E012` **Phi Tiêu Cửu Cung** | Phi tiêu ma thuật xoay tròn và quay lại vị trí người chơi | Common |

> [!IMPORTANT]
> **Triết lý Thiết kế Độ Hiếm Pháp Bảo (Rarity Balance Philosophy):**
> - **Độ hiếm Epic KHÔNG đồng nghĩa với DPS đơn mục tiêu/DPS thô cao nhất.** Vũ khí Epic (*Lựu Đạn Thần Sa W006, Trượng Long Vương W009*) tập trung vào **Khả năng khống chế diện rộng (Crowd Control - CC: Knockback/Choáng), tầm đánh an toàn và Sát thương bùng nổ (Burst AoE)** để giải nguy khi bị quái vây hãm.
> - Các vũ khí có dạng Orbit hoặc Stream (*Bùa Trấn Yêu W003, Đao Cửu Vĩ W008*) bắt buộc phải có **Giới hạn số mục tiêu tối đa per tick** (Tối đa 5 mục tiêu/tick) và **Tầm đánh ngắn** (rủi ro cao khi phải sát quái), để tránh DPS tổng bùng nổ vượt ngưỡng kiểm soát khi mật độ quái đông.

### 4.2. Bảng tương khắc Ngũ Hành (revised v4.2)

#### 4.2.1. Vòng tương khắc (1 chiều ×1.3 Sát thương)
Vòng tương khắc chỉ áp dụng một chiều từ sát thương do Người chơi gây ra lên Yêu ma/Boss. Sát thương Quái gây cho Người chơi giữ nguyên hệ số 1.0x (giảm scope cho bản MVP):
$$\text{Kim} \rightarrow \text{Mộc} \rightarrow \text{Thổ} \rightarrow \text{Thủy} \rightarrow \text{Hỏa} \rightarrow \text{Kim}$$

**Bảng tra cứu ElementMatchupTable (dữ liệu tĩnh):**

| Tấn công \ Mục tiêu | Kim | Mộc | Thủy | Hỏa | Thổ |
|---|---|---|---|---|---|
| **Kim** | 1.0 | **1.3** | 1.0 | 1.0 | 1.0 |
| **Mộc** | 1.0 | 1.0 | 1.0 | 1.0 | **1.3** |
| **Thủy** | 1.0 | 1.0 | 1.0 | **1.3** | 1.0 |
| **Hỏa** | **1.3** | 1.0 | 1.0 | 1.0 | 1.0 |
| **Thổ** | 1.0 | 1.0 | **1.3** | 1.0 | 1.0 |

*(Quy tắc: Vũ khí sai hệ không bị cấm dùng — giữ nguyên hệ số mặc định ×1.0, không quá gò bó nhưng thưởng lớn cho người chơi build đúng)*.

#### 4.2.2. Vòng tương sinh (Proc Giảm 20% Cooldown)
Vòng tương sinh:
$$\text{Kim} \rightarrow \text{Thủy} \rightarrow \text{Mộc} \rightarrow \text{Hỏa} \rightarrow \text{Thổ} \rightarrow \text{Kim}$$

- **Bản chất hiệu ứng Cooldown:** Giảm tức thời **20% thời gian hồi chiêu còn lại** (Instant Cooldown Reduction / Cooldown Refund) trên thanh đếm Cooldown Timer hiện tại của vũ khí vừa gây ra hit thứ 2, *KHÔNG làm thay đổi vĩnh viễn chỉ số Base Cooldown*. (Ví dụ: Vũ khí đang hồi 2.0s, đã đếm được 1.0s $\rightarrow$ giảm 20% của 2.0s = 0.4s $\rightarrow$ còn 0.6s chờ).
- **Cấu trúc Buffer `recentElementHits` (Queue tối đa 3 phần tử):** Mỗi phần tử lưu `{hệ, timestamp, weapon}`. Tự động dọn dẹp phần tử tồn tại quá 3.0s.
  - *Tại sao dùng Buffer 3 phần tử thay vì 2?* Hệ thống kiểm tra 2 hit liên tiếp gần nhất để kích hoạt hiệu ứng 2 hệ. Việc giữ Queue 3 phần tử cho phép nhận diện chuỗi **Combo 3 hệ liên tiếp** trong tương lai mà không bị trôi dữ liệu (Ví dụ: `Kim -> Thủy -> Mộc` kích hoạt 2 lần proc Tương Sinh liên tiếp cho 2 cặp: `Kim->Thủy` và `Thủy->Mộc`), tạo tiền đề mở rộng cơ chế Combo Nhị/Tam Hợp mà không phải sửa kiến trúc code.
- **Giới hạn Cân bằng (Balance):** Tối đa 1 proc / 3 giây per vũ khí (không stack dồn dập trong cùng cửa sổ 3s).
- **UI & Visual Feedback:** Phát hiệu ứng icon 2 hệ nối bằng vệt sáng bay lên trên đầu nhân vật + Âm thanh SFX Ting khi proc thành công.

#### 4.2.3. Hiển thị UI Boss Element
- Đối với Boss có hệ luân phiên (Ngưu Đầu Mã Diện, Diêm Vương): Tự động đổi màu viền/model Sprite + Icon thuộc tính hiện tại hiển thị phía trên thanh HP Boss theo đúng chu kỳ đổi hệ (`BossElementController`).

### 4.3. Bảng Yêu Cầu Tiến Hóa 12 Pháp Bảo MVP (Evolution Mapping Table)

Điều kiện để mở khóa Thẻ Tiến Hóa (Evolution Upgrade Card) trong giao diện Gacha Lên Cấp:
1. **Pháp Bảo Gốc:** Đạt cấp độ tối đa `Level 5` (Max Level).
2. **Vật Phẩm Bị Động (Passive Card):** Người chơi đã sở hữu tối thiểu 1 cấp của Thẻ Passive tương ứng trong Ba lô (`PlayerPassives`).

| ID Gốc | Tên Pháp Bảo Gốc | Hệ | ID Passive | Tên Thẻ Passive Yêu Cầu | ID Evolution | Tên Pháp Bảo Tiến Hóa (Tối Thượng) | Hiệu Ứng Cơ Chế Đặc Trưng Khi Tiến Hóa |
|---|---|---|---|---|---|---|---|
| `W001` | **Nỏ Thần** | Kim | `P001` | **Bùa Sát Thương** (*+Damage*) | `E001` | **Nỏ Liên Châu** | Bắn liên hoàn 3 mũi tên thần xuyên qua tất cả kẻ địch trên đường bay |
| `W002` | **Bút Phán Quan** | Kim | `P002` | **Ấn Chí Mạng** (*+Crit Chance*) | `E002` | **Bút Sinh Tử** | Nhát chém 360 độ chí mạng 100%, tự động kết liễu ngay quái dưới 15% HP |
| `W003` | **Bùa Trấn Yêu** | Mộc | `P003` | **Chuông Hồi Máu** (*+Health Regen*) | `E003` | **Bùa Cửu Huyền** | Mở rộng bán kính vòng bùa xoay + Hồi 1% HP tối đa mỗi 50 đòn trúng |
| `W004` | **Cửu Vĩ Hồ Trảo** | Hỏa | `P004` | **Hỏa Chủng** (*+Attack Speed*) | `E004` | **Hồ Ly Cửu Vĩ** | Triệu hồi 9 móng vuốt lửa tự tìm diệt quái và để lại vệt lửa thiêu đốt |
| `W005` | **Trống Đồng Đông Sơn** | Thổ | `P005` | **Tháp Uy Áp** (*+AoE Range*) | `E005` | **Trống Trấn Quốc** | Sóng âm trảm linh nổ 8 hướng diện rộng, gây choáng 1.5s cho toàn bộ quái |
| `W006` | **Lựu Đạn Thần Sa** | Hỏa | `P006` | **Thuốc Nổ Thần Tiên** (*+Explosion Radius*) | `E006` | **Bão Hỏa Diệm** | Nổ bán kính 5.0m để lại vùng lửa thiêu rụi 3s + Knockback cực mạnh |
| `W007` | **Cung Thạch Sanh** | Kim | `P007` | **Mộc Giáp** (*+Armor*) | `E007` | **Cung Thần Tiễn** | Bắn 3 mũi tên thần lực xuyên vô hạn kèm hiệu ứng đẩy lùi dồn quái vào góc |
| `W008` | **Đao Cửu Vĩ** | Hỏa | `P008` | **Hạt Tốc Đánh** (*+Attack Speed*) | `E008` | **Hỏa Long Đao** | Phun luồng rồng lửa 360 độ xoay quanh nhân vật liên tục không ngừng |
| `W009` | **Trượng Long Vương** | Thủy | `P009` | **Ngọc Hồi Chiêu** (*+Cooldown Reduction*) | `E009` | **Long Vương Trượng** | Sét nước nảy qua 12 quái liên tiếp + Đóng băng quái 1.0s |
| `W010` | **Linh Phù Ma Da** | Thủy | `P010` | **Túi Hút Hồn** (*+Pickup Radius*) | `E010` | **Thủy Cung Linh** | Linh thú Ma Da kích thước gấp đôi, phun độc 4m + Làm chậm quái 30% |
| `W011` | **Nước Thánh Chùa Hương** | Thổ | `P011` | **Bánh Xe Tốc Độ** (*+Move Speed*) | `E011` | **Giếng Thiêng** | Tạo vũng giếng thiêng 5m làm chậm quái 50% + Hồi HP cho Player khi đứng trong vũng |
| `W012` | **Phi Tiêu Bát Quái** | Mộc | `P012` | **Bùa May Mắn** (*+Luck / Gold Drop*) | `E012` | **Phi Tiêu Cửu Cung** | Triệu hồi 9 phi tiêu xoay theo quỹ đạo hoa sen mở rộng rồi thu về player |


---

## 5. Trí Tuệ Nhân Tạo Kẻ Địch — Yêu Ma Dân Gian

### 5.1. Cơ sở dữ liệu Yêu Ma

| Enemy ID | Tên Yêu Ma | Hệ | HP | Speed | Damage | EXP Drop | Hành vi AI & Cơ chế Đặc biệt |
|---|---|---|---|---|---|---|---|
| `E_MAGIAP` | **Ma Giáp** | Kim | 40 | 2.5 | 10 | 1 | Đi chậm, số đông bao vây |
| `E_MATROI` | **Ma Trơi** | Hỏa | 25 | 4.0 | 8 | 2 | Đốm lửa lao nhanh áp sát |
| `E_QUYNHAPTRANG` | **Quỷ Nhập Tràng** | Thổ | 150 | 1.5 | 20 | 5 | Trâu máu, **Cản Đạn** (Tiêu tốn 2 Pierce-charge của đạn xuyên) |
| `E_MADA` | **Ma Da** | Thủy | 35 | 2.0 | 12 | 3 | Phun nước độc từ xa |
| `E_HOALYTINH` | **Hồ Ly Tinh Nhỏ** | Hỏa | 30 | 3.5 | 50 (Nổ) | 4 | Lao vào phát nổ AoE |

> [!IMPORTANT]
> **Quy định Kỹ thuật Cơ chế "Cản Đạn" (`Heavy Armor Bullet Sponge`):**
> - Khi đạn xuyên (Pierce Projectile / Raycast) va chạm với `E_QUYNHAPTRANG` (Quỷ Nhập Tràng):
>   - Đạn sẽ bị **trừ ngay 2 chỉ số Pierce Count (Pierce-charge)** thay vì 1 như quái thường (`currentPierceCount -= 2`). 
>   - Nếu chỉ số `currentPierceCount` còn lại $\le 0$, đạn sẽ bị **Hủy/Thu hồi về Pool lập tức**, ngăn không cho đạn xuyên tiếp sang các yêu ma đứng phía sau.
> - Cơ chế này tạo ra một "tấm khiên sống" thịt che chắn cho các quái nhanh (*Ma Trơi*) và quái tầm xa (*Ma Da*) phía sau, buộc người chơi phải ưu tiên dồn sát thương tiêu diệt Quỷ Nhập Tràng trước hoặc chọn góc bắn linh hoạt.

### 5.2. Trùm Cuối (Boss System)

#### Boss 1: **Ngưu Đầu Mã Diện** — Xuất hiện Phút 10
* **HP Base:** 5,000 | **Tốc độ:** 2.2 | **Hệ:** Thổ (Ngưu Đầu) / Hỏa (Mã Diện, luân phiên)
* **Phase 1 (100%-50%):** *Ngưu Xung Thiên* (Bull Dash x3 speed) + *Địa Chấn Âm Ty* (Ground Slam AoE Slow 40%).
* **Phase 2 (<50%):** Luân phiên đổi hệ Thổ/Hỏa mỗi 10s + *Triệu Hồn Âm Binh* (Gọi 10 Ma Giáp) + *Hắc Khí Âm Ty* (Khói độc 5 dmg/sec).
* **Phần thưởng:** Rương U Minh (1 Thẻ Tiến Hóa / 3 Thẻ Nâng cấp + 500 Cổ Tiền).

#### Boss 2: **Diêm Vương** — Xuất hiện Phút 20 (Final Boss)
* **HP Base:** 15,000 | **Tốc độ:** 1.8 | **Hệ:** Luân phiên xoay vòng cả 5 hệ Ngũ Hành (10s/lần)
* **Phase 1 (100%-40%):** *Bút Phán Quan* (Sóng kiếm quạt 3 hướng) + *Lưới Nghiệp Báo* (Bẫy xương khóa góc 3s).
* **Phase 2 (<40%):** *Vực Vong Xuyên* (Hố đen hút vào tâm gây dmg) + *Quỷ Sứ Trấn Tứ Phương* (4 Cung thủ quỷ canh 4 góc).
* **Phần thưởng:** Rương Đầu Thai (+2,000 Cổ Tiền & Thắng Run).

---

## 6. Cơ Chế Mới: Cán Cân Âm Dương (Yin-Yang Balance)

### 6.1. Cơ chế mới: Cán cân Âm Dương (revised v6.1)

Một biến trạng thái toàn cục `yinYangValue` (thang 0–100, mặc định 50 - Cân bằng), thay đổi theo **hành vi di chuyển thuần túy** — cố tình tách biệt hoàn toàn khỏi lựa chọn hệ Ngũ Hành hay loại vũ khí, để tránh xung đột giữa hai hệ thống (ví dụ: boss buộc đổi hệ vũ khí không được phép kéo theo việc tự động đổi luôn trạng thái Âm/Dương ngoài ý muốn của người chơi):

- **Nghiêng Âm (giảm giá trị):** Dựa trên vận tốc di chuyển của nhân vật — đứng yên hoặc né tránh trong bán kính nhỏ trong khoảng thời gian dài, không quan tâm đang dùng vũ khí gì.
- **Nghiêng Dương (tăng giá trị):** Dựa trên khoảng cách giữa nhân vật và quái tại thời điểm gây sát thương — di chuyển liên tục + gây damage ở cự ly gần, không quan tâm hệ vũ khí.

*Nhờ tách theo hành vi di chuyển thay vì loại vũ khí/hệ, việc đổi vũ khí để counter Ngũ Hành của boss (mục 5.2) hoàn toàn độc lập với việc tích Âm hay Dương — hai cơ chế không còn giẫm chân nhau.*

**Bảng Trạng Thái & Hiệu Ứng:**

| Trạng thái | Ngưỡng | Hiệu ứng mở khóa Pool Thẻ |
|---|---|---|
| **Dương thịnh** | `> 80` | Mở khóa pool thẻ **"Cuồng Nộ"**: `+Damage` / `+Tốc đánh`, `-Hồi máu` |
| **Âm thịnh** | `< 20` (Nghịch mốc > 80) | Mở khóa pool thẻ **"Tịch Diệt"**: `+Né tránh` / `+Hồi máu`, `-Damage` |
| **Thái Cực Cân bằng** | `40 – 60` | Mở khóa duy nhất 1 thẻ Evolution đặc biệt: **"Thái Cực"** |

**Tích hợp kỹ thuật & Lưu ý Balance:**
- Tích hợp 1 biến global vào `UpgradeManager`, filter pool `UpgradeData` theo ngưỡng trước khi hiển thị 3 thẻ. Hệ Gacha không đổi, chỉ bổ sung điều kiện lọc theo `yinYangValue` hiện tại.
- **Xác nhận Chủ đích Thiết kế (Intended Design for Ranged Kiting & Class Perks):**
  - Người chơi build thiên về kiting/tầm xa (Nỏ Thần, Cung Thạch Sanh...) di chuyển né đòn liên tục ở khoảng cách xa sẽ giữ chỉ số `yinYangValue` ổn định trong vùng **Thái Cực Cân bằng (40 – 60)**. Đây là chủ đích thiết kế nhằm giúp người chơi lối đánh an toàn dễ dàng tiếp cận thẻ Evolution **"Thái Cực"** đặc biệt.
  - **Đặc quyền Class Đạo Sĩ (Đạo Sĩ Specific Perk):** Kỹ năng chủ động *"Bát Quái Trận Đồ"* có khả năng ép `yinYangValue` về 50 trong 4s, cho phép Đạo Sĩ chủ động tái tạo "cửa sổ Thái Cực" theo chu kỳ 30s để gacha thẻ Evolution dễ dàng hơn các class khác, bù lại lượng sát thương gây ra trực tiếp (Base DPS) của Đạo Sĩ thấp hơn.
  - Ngược lại, để tích **Dương thịnh (>80)**, người chơi tầm xa bắt buộc phải áp sát liều lĩnh (Risk-Reward Playstyle). Để tích **Âm thịnh (<20)**, người chơi phải chủ động dừng lại / di chuyển cực ngắn (Turret Playstyle).
- **Yêu cầu Playtest theo nhóm Vũ khí (Weapon-Class Playtesting):**
  - Cần playtest và tinh chỉnh ngưỡng (threshold) vận tốc & khoảng cách riêng biệt cho 2 nhóm: **Cận chiến / Tầm ngắn** (Bút Phán Quan, Bùa Trấn Yêu, Đao Cửu Vĩ) vs **Tầm xa / AoE rộng** (Nỏ Thần, Cung Thạch Sanh, Trượng Long Vương) thay vì dùng chung 1 baseline tĩnh, tránh việc vũ khí cận chiến tích Dương quá nhanh hoặc vũ khí tầm xa hoàn toàn không thể chạm mốc 80.


---

## 7. Thiết Kế Màn Chơi & Bản Đồ (Map Design)

* **Map MVP:** **Bến Đò Vong Xuyên** (Bounded Arena kích thước cố định `60m x 60m`, vật cản bia mộ/cây gạo không chặn đạn).
* **Cơ chế Spawn Yêu Ma trong Bounded Arena (`EnemySpawner.cs`):**
  - **Quy tắc Vùng Spawn (Screen-Off-Camera Ring):** Quái luôn được spawn ở **vành ngoài màn hình Camera** (bán kính `R_min = 12m` đến `R_max = 16m` so với Vị trí người chơi hiện tại).
  - **Giới hạn Biên Arena (Boundary Safety Guard):** Nếu vành ngoài Camera nằm đè lên tường biên Arena, Spawner sẽ tự động snap vị trí spawn bám sát dọc theo bờ tường biên Arena.
  - **Khoảng cách Tối thiểu (Minimum Spawn Distance):** Tuyệt đối không spawn quái trong bán kính `< 10m` so với người chơi để tránh tình trạng "pop-in" đè trực tiếp lên nhân vật.
  - **Cơ chế Chống dồn góc & Kẹt cứng cuối trận (Anti-Cornering Logic):**
    - Khi người chơi đứng sát góc/tường biên (khoảng cách `< 5m` tới tường border), tốc độ spawn từ hướng góc đó giảm 70%, đồng thời 70% lượng quái mới sẽ được dồn spawn về phía đường thoát duy nhất (phía trung tâm/vùng trống).
    - **Dynamic Repositioning Strategy:** Yêu ma khi chạm tường biên sẽ tự động kích hoạt AI tìm đường di chuyển dọc theo bờ tường ôm lấy người chơi thay vì đứng chồng chất gây kẹt cứng.
* **Pacing & Spawn Curve:**
  * `00:00 - 05:00`: Giai đoạn Khởi động (Ma Giáp & Ma Trơi xuất hiện thưa thớt, Max 50 active enemies).
  * `05:00 - 10:00`: Giai đoạn Tăng tốc (Quỷ Nhập Tràng & Ma Da xuất hiện, đợt bùng nổ phút thứ 8, Max 120 active enemies).
  * `10:00`: **Boss Ngưu Đầu Mã Diện** xuất hiện (Tự động quét sạch quái nhỏ trong bán kính 10m xung quanh Boss).
  * `10:00 - 19:59`: Giai đoạn Sống còn (Hồ Ly Tinh Nhỏ nổ tràn ngập, yêu ma phối hợp 5 hệ, Max 200 active enemies).
  * `20:00`: **Final Boss Diêm Vương** xuất hiện (Tạo vòng lửa âm phong cô lập đấu trường Boss).
* **Giải Pháp Chống Mỏi Visual Pacing Cho Map MVP (Atmosphere Palette-Swap):**
  - Để biến tấu nhịp thị giác trong phiên chơi 20 phút mà không tốn chi phí dựng bản đồ mới, game sử dụng cơ chế **Chuyển Màu Không Khí Theo Phase (Atmosphere Palette-Swap)** qua Post-Processing & Tilemap Color Grading:
    - `00:00 – 05:00`: **Sương Mờ U Linh** (Tông màu xanh chàm u tối, sương mờ).
    - `05:00 – 10:00`: **Âm Phong Hoàng Tuyền** (Tông lá úa sương mờ vàng nhạt).
    - `10:00 – 19:59` (Sau Boss 1): **Bão Hắc Khí Huyết Nguyệt** (Tông màu đỏ thẫm u uất, trăng máu).
    - `20:00` (Final Boss): **Hỏa Ngục Địa Môn** (Vùng biên rực cháy tông đỏ cam hỏa ngục).
  - Tối ưu kỹ thuật: Thực hiện Lerp nhẹ nhàng thông số `Global Volume (Color Grading / Vignette)` và Sprite Tint Color trong 3s chuyển Phase, **0 GC Allocation** và hoàn toàn không tăng dung lượng bộ nhớ Asset.

---

## 8. Kiến Trúc UI/UX — Mô Hình MVP (Model-View-Presenter)

* **Tầng View (`RunHUDView.cs`):** Passive View chỉ nhận chuỗi string/float đã được định dạng. Không tự đọc Model. 
  - **Layout HUD bao gồm:** Slider Cán cân Âm Dương, TMP Text Thuộc tính Boss, Thanh HP/Exp, Đồng hồ Run Time, và **Nút bấm kích hoạt Signature Skill (Active UI Button ở góc dưới bên phải)**.
* **Tầng Presenter (`RunHUDPresenter.cs`):** Subcribe Model Events (`YinYangManager`, `BossElementController`, `PlayerStats`, `SignatureSkillManager`), định dạng màu sắc Rich Text TMP (`<color>`) & quản lý Cooldown Radial Fill của nút bấm trước khi đẩy sang View.
* **Thẩm mỹ:** Khung UI dạng giấy dó, dấu triện đỏ, font chữ thư pháp cho tiêu đề.

---

## 9. Tích Hợp Nền Tảng Android & Tối Ưu Hiệu Năng

* **Offline-first Local Save & Lộ trình Cloud Save (Google Play Games Services):** 
  - **MVP Baseline:** `SaveSystem.cs` mã hóa JSON lưu thông tin `vongTe` (Cổ Tiền), nhân vật đã mở khóa, kỷ lục Best Run Time xuống `Application.persistentDataPath` phục vụ chơi Offline tức thời không cần kết nối mạng.
  - **Lộ trình Mở rộng (SEA Expansion & IAP):** Khi phát hành thị trường Đông Nam Á và bổ sung IAP (bản Update), hệ thống lưu game sẽ được trừu tượng hóa qua interface `ISaveProvider` để tích hợp song song **Google Play Games Services (GPGS) Saved Games API (Cloud Save)**. Điều này giúp tự động đồng bộ tiến trình qua Cloud khi người chơi đổi thiết bị di động mà không làm mất dữ liệu mua sắm hay thành tích.
* **Input System:** New Input System (`com.unity.inputsystem`) hỗ trợ Touch Virtual Joystick 360 degree trên Android.
* **Chiến lược Tối ưu Hiệu năng (Target 60 FPS / 200 Enemies + 100 Projectiles):**
  - **Object Pooling 0 GC Allocation:** Dùng `UnityEngine.Pool.ObjectPool` tái sử dụng toàn bộ Projectiles, Enemy, Damage Popups & VFX.
  - **Tối ưu Va chạm hàng loạt (Spatial Partitioning & Physics2D NonAlloc):**
    - Thiết lập **Physics2D Layer Collision Matrix** nghiêm ngặt (Tắt va chạm Quái-với-Quái nếu không cần thiết, tắt va chạm Đạn-với-Đạn).
    - Sử dụng các hàm `Physics2D.OverlapCircleNonAlloc` / `Physics2D.RaycastNonAlloc` với buffer cố định `Collider2D[50]`, tuyệt đối không dùng `OnTriggerEnter2D` / `OnCollisionEnter2D` truyền thống cho Projectiles số lượng lớn.
    - Áp dụng **Spatial Grid Hash (Spatial Partitioning)** đơn giản quản lý vị trí Enemy để query quái trong bán kính đạn cực nhanh $O(1)$ mà không cần gọi engine Physics2D liên tục.
  - **Job System & Burst Compiler (Fallback có điều kiện — Conditional Escalation Path):**
    - **Baseline MVP (Prototype hiện tại):** Logic di chuyển/chase của yêu ma dùng `Update()` loop đơn luồng thông thường (MonoBehaviour), kết hợp với Spatial Grid Hash đã thiết lập ở trên để tránh gọi Physics2D liên tục. Không yêu cầu Job System/Burst ngay từ đầu — phù hợp với quy mô và tiến độ của bản prototype.
    - **Điều kiện kích hoạt (Trigger Condition):** Chỉ chuyển sang `C# Job System` + `TransformAccessArray` cho phần di chuyển của yêu ma nếu và chỉ nếu kết quả profiling thực tế trên thiết bị tầm trung (Unity Profiler / Frame Debugger) cho thấy CPU Frame Time vượt ngưỡng 16.6ms tại mốc ~200 active enemies đồng thời, dẫn đến tụt dưới mục tiêu 60 FPS.
    - **Quy trình xác nhận (Verification Step):** Trước khi escalate lên Job System, cần xác định rõ bottleneck nằm ở đâu qua Profiler (CPU Main Thread — logic di chuyển, hay Rendering, hay GC Spike) để tránh tối ưu sai chỗ. Nếu bottleneck không nằm ở logic di chuyển, Job System sẽ không giải quyết được vấn đề và không nên áp dụng.
    - *Ghi chú:* Đây là fallback dự phòng, không phải yêu cầu bắt buộc cho milestone Prototype/MVP hiện tại của dev.
  - **Đồ họa & Asset:** ASTC Texture Compression (4x4 / 6x6), Sprite Atlas gom Draw Calls, IL2CPP ARM64-v8a Backend.

---

## 10. Danh Mục Tài Liệu Tham Chiếu & Quản Lý Dự Án (Project & Reference Docs)

Khi cần tra cứu chi tiết về thiết kế game, kế hoạch làm việc, kiến trúc hệ thống, hướng dẫn dựng UI hoặc kỹ thuật VFX, hãy tham chiếu các tài liệu sau:
- 🎮 **[ProjectZombie_GDD.md](file:///c:/Users/thuon/Unity/Projectzombie/GameDesignDoc/ProjectZombie_GDD.md)**: Game Design Document 4.0 (Official Single Source of Truth).
- 🎨 **[ART_VFX_STYLE_GUIDE.md](file:///c:/Users/thuon/Unity/Projectzombie/GameDesignDoc/ART_VFX_STYLE_GUIDE.md)**: Hướng dẫn Mỹ thuật, Bảng màu Ngũ Hành & Kỹ thuật VFX cho 2D/VFX Artist & Technical Artist.
- 🗺️ **[ROADMAP.md](file:///c:/Users/thuon/Unity/Projectzombie/ProjectManagement/ROADMAP.md)**: Lộ trình phát triển 4 giai đoạn tới Google Play Store Release.
- 📋 **[TASKS.md](file:///c:/Users/thuon/Unity/Projectzombie/ProjectManagement/TASKS.md)**: Bảng quản lý nhiệm vụ dạng Kanban Task Tracker & Kế hoạch thực thi Sprint.
- 📐 **[SYSTEM_ARCHITECTURE.md](file:///c:/Users/thuon/Unity/Projectzombie/.agents/references/SYSTEM_ARCHITECTURE.md)**: Sơ đồ kiến trúc kỹ thuật 6 tầng và mô tả chi tiết các hệ thống cốt lõi.
- 💥 **[MODULAR_VFX_SYSTEM_DOC.md](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/VFX/MODULAR_VFX_SYSTEM_DOC.md)**: Hướng dẫn hệ thống Modular VFX 4 Category & GlobalVFXPoolManager.
- 🎨 **[UI_SETUP_GUIDE.md](file:///c:/Users/thuon/Unity/Projectzombie/.agents/references/UI_SETUP_GUIDE.md)**: Hướng dẫn chi tiết thiết lập UI Canvas, TextMeshProUGUI và các Prefabs UI.

