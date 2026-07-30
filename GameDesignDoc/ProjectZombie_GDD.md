# Game Design Document — Dự Án: VONG XUYÊN

**Phiên bản:** 4.0 (Official Single Source of Truth — Google Play Store Release)  
**Thể loại:** Top-down Survival Roguelite / Auto-battler (Cơ chế Ngũ Hành & Âm Dương)  
**Nền tảng:** Android Mobile (Google Play Store — Target API 33+, IL2CPP ARM64, AAB Package)  
**Phong cách đồ họa:** 2D Top-down, Mỹ thuật dân gian Việt Nam (Tranh Đông Hồ / Hàng Trống cách điệu, tông màu u linh)  

---

## 1. Tổng quan (Overview)

### 1.1. Tầm nhìn sản phẩm (Vision Statement)
**Vong Xuyên** đưa người chơi vào hành trình sinh tồn nghẹt thở giữa cõi âm ty Việt Nam — nơi hồn ma, quỷ dữ và yêu tinh dân gian trỗi dậy từ truyền thuyết. Người chơi không chỉ "cày điểm sát thương" mà phải thấu hiểu quy luật Ngũ Hành để chế ngự từng loài yêu quái, đồng thời giữ cán cân Âm Dương trong tâm để tồn tại tới khi đối mặt Diêm Vương.

### 1.2. Đối tượng người chơi (Target Audience)
- Người chơi di động casual/mid-core yêu thích thể loại survivor-like trên Android.
- Phiên chơi ngắn (10–20 phút/run), điều khiển mượt mà bằng Cần điều khiển ảo (Dynamic Virtual Joystick).
- Thị trường ưu tiên: Việt Nam và cộng đồng gốc Việt hải ngoại, mở rộng sang Đông Nam Á.

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

---

## 3. Hệ Thống Nhân Vật (Player System)

### 3.1. Roster Nhân Vật MVP

| Nhân vật | Vũ khí khởi đầu | Hệ khởi điểm | Signature Skill (Kỹ năng chủ động) |
|---|---|---|---|
| **Thư Sinh** | Bút Phán Quan | Kim | **"Vịnh Thơ Trấn Tà"** — Làm chậm toàn bộ yêu ma trong tầm 3s |
| **Đạo Sĩ** | Bùa Trấn Yêu | Mộc | **"Trận Bát Quái"** — Tạo vòng bảo hộ bất tử 1.5s |
| **Võ Tăng** | Thiền Trượng | Thổ | **"Sư Tử Hống"** — Đẩy lùi + Choáng diện rộng |

### 3.2. Chỉ số Nhân vật (Player Stats)
- **Phòng thủ:** HP, Armor, Move Speed.
- **Tấn công:** Base Damage, Attack Speed, Range, Crit Chance, Crit Damage.
- **Utility:** Pickup Radius (bán kính hút Exp), Luck, Cooldown Reduction.

---

## 4. Hệ Thống Vũ Khí — Pháp Bảo & Ngũ Hành

### 4.1. Cơ sở dữ liệu 12 Pháp Bảo MVP

| ID | Tên Pháp Bảo | Hệ | Damage | Cooldown | Loại Projectile | Evolution (Tối thượng) | Mô tả & Hiệu ứng đặc trưng | Độ hiếm |
|---|---|---|---|---|---|---|---|---|
| `W001` | **Nỏ Thần** | Kim | 12 | 0.6s | Straight, xuyên táo | `E001` **Nỏ Liên Châu** | Mũi tên thần An Dương Vương bắn thẳng xuyên táo kẻ địch | Common |
| `W002` | **Bút Phán Quan** | Kim | 20 | 0.8s | Melee Slash | `E002` **Bút Sinh Tử** | Nhát chém mang uy lực phán quyết âm ty gây sát thương chí mạng 2 bên | Common |
| `W003` | **Bùa Trấn Yêu** | Mộc | 15 | 0.2s | Orbit Blade | `E003` **Bùa Cửu Huyền** | Vòng lá bùa thần xoay quanh bảo vệ và đẩy lùi yêu ma | Rare |
| `W004` | **Cửu Vĩ Hồ Trảo** | Hỏa | 18 | 1.2s | Homing | `E004` **Hồ Ly Cửu Vĩ** | Móng vuốt cáo lửa tự tìm diệt quái và hút sinh khí | Rare |
| `W005` | **Trống Đồng Đông Sơn** | Thổ | 8x5 | 1.5s | Spread AoE | `E005` **Trống Trấn Quốc** | Sóng âm trảm linh tỏa rộng 5 hướng gây choáng diện rộng | Common |
| `W006` | **Lựu Đạn Thần Sa** | Hỏa | 45 | 2.5s | AoE Explosive | `E006` **Bão Hỏa Diệm** | Hạt thần sa phát nổ tạo bão lửa thiêu rụi vùng rộng | Epic |
| `W007` | **Cung Thạch Sanh** | Kim | 35 | 1.0s | Piercing Bolt | `E007` **Cung Thần Tiễn** | Mũi tên thần lực bối cảnh Thạch Sanh xuyên qua hàng loạt yêu tinh | Rare |
| `W008` | **Đao Cửu Vĩ** | Hỏa | 6/tick | 0.1s | Continuous Stream | `E008` **Hỏa Long Đao** | Luồng rồng lửa thiêu đốt liên tục theo đường thẳng | Rare |
| `W009` | **Trượng Long Vương** | Thủy | 25 | 1.8s | Chain Lightning-nước | `E009` **Long Vương Trượng** | Sét nước thủy cung lan truyền qua chuỗi nhiều yêu quái | Epic |
| `W010` | **Linh Phù Ma Da** | Thủy | 10 | 2.0s | Pet Summon AoE | `E010` **Thủy Cung Linh** | Triệu hồi linh thú Ma Da phun độc sát thương liên tục | Rare |
| `W011` | **Nước Thánh Chùa Hương** | Thổ | 14/sec | 3.0s | Ground AoE | `E011` **Giếng Thiêng** | Tạo bãi giếng thiêng trên mặt đất làm chậm và gây sát thương liên tục | Rare |
| `W012` | **Phi Tiêu Bát Quái** | Mộc | 22 | 1.4s | Returning Blade | `E012` **Phi Tiêu Cửu Cung** | Phi tiêu ma thuật xoay tròn và quay lại vị trí người chơi | Common |

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

- Player duy trì một buffer toàn cục `recentElementHits` (Queue tối đa 3 phần tử, mỗi phần tử gồm `{hệ, timestamp, weapon}`). Tự động dọn dẹp phần tử quá 3.0s.
- Sau mỗi lần hit, kiểm tra 2 hit gần nhất: Nếu khớp đúng thứ tự Tương Sinh $\rightarrow$ Giảm ngay **20% Cooldown** hiện tại của vũ khí vừa gây ra hit thứ 2.
- **Giới hạn Cân bằng (Balance):** Tối đa 1 proc / 3 giây (không stack nhiều lần trong cùng cửa sổ 3s).
- **UI & Visual Feedback:** Phát hiệu ứng icon 2 hệ nối bằng vệt sáng bay lên trên đầu nhân vật + Âm thanh SFX Ting khi proc thành công.

#### 4.2.3. Hiển thị UI Boss Element
- Đối với Boss có hệ luân phiên (Ngưu Đầu Mã Diện, Diêm Vương): Tự động đổi màu viền/model Sprite + Icon thuộc tính hiện tại hiển thị phía trên thanh HP Boss theo đúng chu kỳ đổi hệ (`BossElementController`).


---

## 5. Trí Tuệ Nhân Tạo Kẻ Địch — Yêu Ma Dân Gian

### 5.1. Cơ sở dữ liệu Yêu Ma

| Enemy ID | Tên Yêu Ma | Hệ | HP | Speed | Damage | EXP Drop | Hành vi AI |
|---|---|---|---|---|---|---|---|
| `E_MAGIAP` | **Ma Giáp** | Kim | 40 | 2.5 | 10 | 1 | Đi chậm, số đông bao vây |
| `E_MATROI` | **Ma Trơi** | Hỏa | 25 | 4.0 | 8 | 2 | Đốm lửa lao nhanh áp sát |
| `E_QUYNHAPTRANG` | **Quỷ Nhập Tràng** | Thổ | 150 | 1.5 | 20 | 5 | Trâu máu, cản đạn cho đồng bọn |
| `E_MADA` | **Ma Da** | Thủy | 35 | 2.0 | 12 | 3 | Phun nước độc từ xa |
| `E_HOALYTINH` | **Hồ Ly Tinh Nhỏ** | Hỏa | 30 | 3.5 | 50 (Nổ) | 4 | Lao vào phát nổ AoE |

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
- *Ghi chú Balance Team:* Vì `yinYangValue` không còn phụ thuộc vũ khí/hệ, cần playtest riêng để xác định ngưỡng tăng/giảm hợp lý cho vận tốc và khoảng cách gây damage — đây là 2 tham số mới hoàn toàn.


---

## 7. Thiết Kế Màn Chơi & Bản Đồ (Map Design)

* **Map MVP:** **Bến Đò Vong Xuyên** (Bounded Arena, vật cản bia mộ/cây gạo không chặn đạn).
* **Pacing & Spawn Curve:**
  * `00:00 - 05:00`: Giai đoạn Khởi động (Ma Giáp & Ma Trơi xuất hiện thưa thớt).
  * `05:00 - 10:00`: Giai đoạn Tăng tốc (Quỷ Nhập Tràng & Ma Da xuất hiện, đợt bùng nổ phút thứ 8).
  * `10:00`: **Boss Ngưu Đầu Mã Diện** xuất hiện.
  * `10:00 - 19:59`: Giai đoạn Sống còn (Hồ Ly Tinh Nhỏ nổ tràn ngập, yêu ma phối hợp 5 hệ).
  * `20:00`: **Final Boss Diêm Vương** xuất hiện.

---

## 8. Kiến Trúc UI/UX — Mô Hình MVP (Model-View-Presenter)

* **Tầng View (`RunHUDView.cs`):** Passive View chỉ nhận chuỗi string/float đã được định dạng. Không tự đọc Model. Thêm Slider Cán cân Âm Dương & TMP Text Thuộc tính Boss.
* **Tầng Presenter (`RunHUDPresenter.cs`):** Subcribe Model Events (`YinYangManager`, `BossElementController`, `PlayerStats`), định dạng màu sắc Rich Text TMP (`<color>`) trước khi đẩy sang View.
* **Thẩm mỹ:** Khung UI dạng giấy dó, dấu triện đỏ, font chữ thư pháp cho tiêu đề.

---

## 9. Tích Hợp Nền Tảng Android & Lưu Trữ

* **Offline-first Local Save:** `SaveSystem.cs` mã hóa JSON lưu thông tin `vongTe` (Cổ Tiền), nhân vật đã mở khóa, kỷ lục Best Run Time xuống `Application.persistentDataPath`.
* **Input System:** New Input System (`com.unity.inputsystem`) hỗ trợ Touch Virtual Joystick 360 degree trên Android.
* **Tối ưu hóa:** ASTC Texture Compression, Object Pooling 0 GC cho đạn và yêu ma, IL2CPP ARM64-v8a Backend.
