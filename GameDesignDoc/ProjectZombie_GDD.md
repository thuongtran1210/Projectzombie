# Game Design Document — Dự Án: VONG XUYÊN

**Phiên bản:** 5.0 (Official Single Source of Truth — Action RPG Roguelite Release)  
**Thể loại:** Top-down Action RPG Roguelite / Hack & Slash (Cơ chế Ngũ Hành & Cán Cân Âm Dương)  
**Nền tảng:** Android Mobile (Google Play Store — Target API 33+, IL2CPP ARM64, AAB Package)  
**Phong cách đồ họa:** 2D Top-down (Top-down 3/4 View), Mỹ thuật dân gian Việt Nam (Tranh Đông Hồ / Hàng Trống cách điệu, tông màu u linh)  

---

## 1. Tổng Quan (Overview)

### 1.1. Tầm nhìn sản phẩm & Cốt truyện Tổng thể (Vision Statement & Grand Narrative)
**Vong Xuyên** lấy bối cảnh cõi giới thần thoại và tín ngưỡng dân gian Việt Nam khi **Ma Vương Tối Thượng** (Chúa Quỷ) thức tỉnh, làm đảo lộn trật tự Âm Dương và nhấn chìm tam giới vào bóng tối yêu ma.

* **Mục tiêu & Điểm kết của Toàn Bộ Câu Chuyện:** Hành trình của các Anh Hùng qua các miền đất thiêng để tìm kiếm di tích, vượt thử thách và **Thức tỉnh / Triệu hồi "TỨ BẤT TỬ"** (Tản Viên Sơn Thánh, Chử Đồng Tử, Phù Đổng Thiên Vương, Thánh Mẫu Liễu Hạnh) nhằm hợp nhất thần lực đánh bại Ma Vương, cứu vãn nhân gian.
* **Vị trí của Map 1 — "Bến Đò Vong Xuyên" (Bản MVP):** Đây là **Chương 1 (Mở Đầu)**. Khi kiệt sức trước đợt tấn công đầu tiên của quỷ dữ, linh hồn người chơi bị giam hãm tại Bến Đò Vong Xuyên — ranh giới u linh giữa cõi sống và cõi chết.
  * **Mục tiêu cụ thể tại Map 1:** Sống sót qua 20 phút nghẹt thở, vận dụng kỹ năng chặt chém của Vũ Khí Chính, sự trợ lực của Pháp Bảo Hộ Thân, Vòng Ngũ Hành và Cán cân Âm Dương để tiêu diệt các Cai ngục & Sứ giả địa phủ (*Ngưu Đầu Mã Diện, Diêm Vương*), **phá vỡ phong ấn Hoàng Tuyền để mở đường Trở Về Trần Thế** và mở khóa bản đồ dẫn lối tới nơi phong ấn của vị Thánh Bất Tử đầu tiên.

### 1.2. Đối tượng người chơi & Phong cách Đồ họa (Target & Graphic Specs)
- **Người chơi mục tiêu:** Casual / Mid-core yêu thích thể loại Action RPG, Chặt chém (Hack & Slash) và Roguelite trên di động. Phiên chơi 10–20 phút/run.
- **Phong cách Đồ họa Pixel Art 2D Top-down (Top-down 3/4 View):**
  - **Góc nhìn (Perspective):** **Top-down 3/4 view** (thấy mặt trước/trang phục dân gian áo the, khăn đóng, cà sa, pháp bảo).
  - **Tỉ lệ cơ thể (Body Ratio):** **Chibi hóa nhẹ (1:3 đến 1:4)** giúp silhouette nhân vật và hành động vung kiếm/ra chiêu rõ ràng, dễ đọc.
  - **Kích thước Sprite Canvas:** Nhân vật/Quái: **32×32px đến 48×48px**. Tilemap: **16×16px hoặc 32×32px**. Boss: **64×64px đến 96×96px**.
  - **Viền Outline:** Viền đen mỏng **1px** bao quanh nhân vật/quái để tách khỏi nền tối.
- **Hệ thống Hướng & Hoạt Ảnh (Action Animation System):**
  - Hỗ trợ bộ hoạt ảnh chiến đấu: `Idle` (nhịp thở), `Move` (di chuyển), `Attack_1` (chém nhẹ), `Attack_2` (chém vừa), `Attack_3` (đòn kết thúc), `Dash` (lướt né đòn) và `Signature Skill` (kỹ năng đặc biệt).
  - Cơ chế lật hướng tự động `SpriteRenderer.flipX` theo hướng di chuyển hoặc hướng ngắm mục tiêu.

### 1.3. Điểm khác biệt độc quyền (Unique Selling Points - USPs)
- **Lối Chơi Chặt Chém Đã Tay (Action Combat Feel):** Chuỗi Combo 3 đòn liên hoàn, hỗ trợ ngắm thông minh (Smart Soft-Lock), cơ chế hủy hoạt ảnh bằng Lướt (Dash Cancel) và phản hồi lực đánh đã tay (Hit-stop $0.04s$, Camera Shake, Knockback).
- **Hệ Thống Phân Cấp Vũ Khí & Pháp Bảo (Primary vs Relics):** Vũ khí chính quyết định phong cách chém tay chủ động, kết hợp với các Pháp bảo hộ thân tự động bảo vệ sau lưng hoặc đính kèm hiệu ứng vào đòn chém (On-Hit Imbuement).
- **Cơ chế Ngũ Hành (Kim - Mộc - Thủy - Hỏa - Thổ):** Vận dụng đòn chém tay để kích hoạt chuỗi Tương Khắc (+30% Sát thương) và chuỗi Tương Sinh (-20% Cooldown + Tăng tốc đánh).
- **Cán Cân Âm Dương Độc Quyền:** Luân chuyển giữa lối đánh Áp Sát Liều Lĩnh (Dương Thịnh) và Thả Diều Tĩnh Tại (Âm Thịnh) để mở khóa các nhánh thẻ Đột Phá hiếm.
- **Bản Sắc Văn Hóa Dân Gian Thuần Việt:** Tích hợp truyền thuyết Tứ Bất Tử, vũ khí cổ vật (Bút Phán Quan, Nỏ Thần, Bùa Trấn Yêu, Trống Đồng Đông Sơn) và yêu quái dân gian (Ma Giáp, Ma Trơi, Quỷ Nhập Tràng, Ma Da).

---

## 2. Vòng Lặp Gameplay (Core Gameplay Loop)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       SẢNH CHỜ NGOÀI TRẬN (OUT-GAME META)                    │
│  - Chọn Nhân Vật (Character Selection)                                      │
│  - Chọn Vũ Khí Chính (Primary Weapon)                                       │
│  - Chọn 2 - 3 Pháp Bảo Hộ Thân (Relic Loadout)                             │
│  - Nâng cấp Chỉ số Vĩnh viễn (Sanctuary Tree) & Rèn Vũ khí bằng Cổ Tiền    │
└──────────────────────────────────────┬──────────────────────────────────────┘
                                       │ Bắt đầu Trận Chiến (Enter Run)
                                       ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                     TRẬN ĐÁNH HACK & SLASH (IN-RUN ROGUELITE)               │
│  1. Thao Tác Chiến Đấu:                                                     │
│     - Tay trái: Joystick 360° di chuyển & định hướng                        │
│     - Tay phải: Nút Đánh Combo (3-Hit Chain) + Nút Lướt (Dash) + Skill      │
│  2. Chặt Chém & Tiêu Diệt Yêu Ma:                                           │
│     - Vũ khí chính gây 70% DPS, Pháp bảo hỗ trợ On-Hit & Khống chế sau lưng │
│     - Kích hoạt Vòng Tương Sinh qua nhịp chém tay                           │
│  3. Thu Thập Hạt Kinh Nghiệm (Exp Gem) & Lên Cấp (Level Up):                │
│     - Chọn 1 trong 3 Thẻ Nâng Cấp Trong Trận:                               │
│       * Biến hóa Combo Vũ Khí (Combo Augments / Bí Kíp)                     │
│       * Thức tỉnh & Cường hóa Pháp Bảo Đã Trang Bị (Relic Awakening)        │
│       * Cường hóa Kỹ năng Lướt (Dash Traits) & Chỉ số Tình huống           │
│     - Mốc Level 5 & 10: Đột Phá Tuyệt Kỹ (Overclock / Ultimate Breakthrough)│
│  4. Kết Thúc Trận:                                                          │
│     - Thắng khi diệt Diêm Vương ở 20:00 (Phá phong ấn) / Thua khi hết HP    │
└──────────────────────────────────────┬──────────────────────────────────────┘
                                       │ Quy đổi Điểm & Rơi Cổ Tiền
                                       ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                           TIẾN TRÌNH VĨNH VIỄN                              │
│  - Tích lũy Cổ Tiền (Vĩnh viễn) -> Nâng cấp cây Sanctuary Tree              │
│  - Mở khóa Nhân vật, Vũ khí và Pháp bảo mới trong Cửa Hàng                  │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Hệ Thống Điều Khiển & Giao Diện Chiến Đấu (Controls & Combat HUD)

### 3.1. Bố Cục Cụm Phím Thao Tác (Combat Action Cluster)
* **Tay Trái (Movement Area):**
  * **Dynamic Virtual Joystick (360°):** Di chuyển nhân vật mượt mà, hỗ trợ tự động căn vị trí ngón tay chạm trên nửa trái màn hình.
* **Tay Phải (Action Button Cluster):**
  * **Nút Tấn Công Chính (Primary Attack Button - Kích thước Lớn Nhất):** Nhấn liên tục để thực hiện chuỗi 3-Hit Combo. Hỗ trợ nhấn giữ (Hold) để chém nhịp tự động hoặc gồng chiêu tùy loại vũ khí.
  * **Nút Lướt (Dash / Dodge Button):** Lướt nhanh qua kẻ địch (có $0.15s$ khung bất tử - I-Frame). Hỗ trợ **Hủy hoạt ảnh đòn chém (Animation Cancel)** để né đòn khẩn cấp. Cooldown: $1.2s$ (hồi lại nhanh).
  * **Nút Tuyệt Kỹ (Signature Skill Button):** Kích hoạt kỹ năng đặc biệt của nhân vật với Cooldown dài ($20s - 30s$), có vòng đếm Cooldown Radial Fill.

### 3.2. Cơ Chế Hỗ Trợ Ngắm Thông Minh (Smart Soft-Lock System)
* Khi bấm nút Tấn công:
  * Nếu người chơi **đang kéo Joystick:** Nhân vật chém theo hướng Joystick chỉ định.
  * Nếu người chơi **thả Joystick:** Hệ thống tự động quét mục tiêu trong góc hình nón $90^\circ$ và bán kính $5m$ phía trước mặt $\rightarrow$ Tự động xoay người chém chính xác vào yêu ma gần nhất, loại bỏ hoàn toàn hiện tượng chém hụt vào không khí.

---

## 4. Hệ Thống Vũ Khí Chính & Pháp Bảo Hộ Thân (Weapons & Relics)

### 4.1. Phân Định Cấu Trúc Sức Mạnh (Combat Role & DPS Distribution)
* **Vũ Khí Chính (Primary Weapon — Chiếm 65 - 75% Tổng DPS):** Là công cụ tấn công chủ động bằng tay của người chơi. Tầm đánh định hướng, sát thương cao, nhịp combo rõ ràng.
* **Pháp Bảo Hộ Thân (Secondary Relics — Chiếm 15 - 20% Tổng DPS):** Tối đa **3 Pháp bảo** mang theo. Đóng vai trò **Khống chế (CC), bảo vệ sau lưng, và đính kèm hiệu ứng đòn đánh (On-Hit Imbuement)**.
* **Tuyệt Kỹ Nhân Vật (Signature Skill — Chiếm 10 - 15% Tổng DPS):** Kỹ năng giải vây, buff bùng nổ hoặc xoay chuyển thế trận.

---

### 4.2. Danh Mục Vũ Khí Chính (Primary Weapons — Chặt Chém, Bắn Định Hướng & Bựa/Fun)

Người chơi chọn 1 Vũ Khí Chính tại Sảnh Chờ ngoài trận:

| ID | Tên Vũ Khí | Hệ | Phong Cách Đánh | Chuỗi Combo Cơ Bản | Cơ Chế Tiến Hóa / Thần Binh |
|---|---|---|---|---|---|
| `W_SWORD` | **Thanh Long Kiếm** | Kim | Cận chiến nhanh, linh hoạt | Nhát 1: Chém xéo (100% dmg)<br/>Nhát 2: Chém quét (120% dmg)<br/>Nhát 3: Đâm kiếm khí xuyên thấu (180% dmg) | **Hiên Viên Thần Kiếm:** Đòn thứ 3 giải phóng bão kiếm khí 3 tia bay xa $6m$. |
| `W_PEN` | **Bút Phán Quan** | Kim | Cận chiến rộng, bạo kích | Nhát 1: Vung mực ngang (110% dmg)<br/>Nhát 2: Vẽ chữ Sinh (130% dmg)<br/>Nhát 3: Trảm chữ Tử (220% dmg, Crit cao) | **Bút Sinh Tử:** Đòn thứ 3 quét $360^\circ$ nổ mực đen, trảm sát quái thường dưới 15% HP. |
| `W_STAFF` | **Thiền Trượng Sơn Lâm** | Thổ | Đòn nặng, khống chế diện rộng | Nhát 1: Quét gậy thấp (120% dmg, Làm chậm)<br/>Nhát 2: Đập đất rung chuyển (160% dmg)<br/>Nhát 3: Giậm trượng Địa Chấn (250% dmg, Choáng 1s) | **Hàng Ma Trượng:** Giậm đất tạo sóng nứt đá đẩy văng toàn bộ quái trong bán kính $4m$. |
| `W_CROSSBOW`| **Nỏ Thần Cổ Loa** | Kim | Tầm xa, bắn định hướng | Nhát 1: Bắn 1 tên (100% dmg)<br/>Nhát 2: Bắn 2 tên rẻ quạt (120% dmg)<br/>Nhát 3: Bắn mũi tên Thần Sa nổ (200% dmg) | **Nỏ Liên Châu:** Bắn liên tục chùm 5 mũi tên thần lực găm nổ kẻ địch. |
| `W_SLIPPER` | **Dép Tổ Ong Thần Sa** | Kim | Ném Boomerang Slapstick, vả liên hoàn | Nhát 1: Ném chiếc trái vả bẹp mặt (110% dmg)<br/>Nhát 2: Ném chiếc phải vả bẹp mặt (130% dmg)<br/>Nhát 3: Quăng lốc dép $360^\circ$ (200% dmg, hút quái) | **Dép Thần Vạn Năng:** Đòn 3 gây hiệu ứng *"Quê Độ"*, quái xấu hổ ôm mặt đứng im $1.5s$ hoặc quay sang đấm quái bên cạnh. |
| `W_POT` | **Nồi Cơm Thạch Sanh** | Thổ | Cận chiến gom quái hỗn loạn & Phóng đạn quái | Nhát 1: Đập nắp nồi leng keng (100% dmg, Choáng nhẹ)<br/>Nhát 2: Mở nắp hút 3 quái nhỏ vào nồi<br/>Nhát 3: Bắn phọt quái ra như đạn đại bác (240% dmg) | **Cơm Niêu Vô Tận:** Quái bị bắn ra bay hình vòng cung (Ragdoll) đè bẹp cả hàng quái, nổ văng ra cơm nắm hồi 5% HP. |
| `W_PIPE` | **Điếu Cày Cửu U** | Hỏa | Phun khói tầm trung, khống chế gây lú | Nhát 1: Vung cán điếu gõ đầu (100% dmg)<br/>Nhát 2: Thổi tia tàn lửa rực đỏ (140% dmg)<br/>Nhát 3: Nhả làn khói thuốc mịt mù (200% dmg) | **Thuốc Lào Tiên Giới:** Toàn bộ quái dẫm vào khói bị *"Say Khói"*, đi loạng choạng zíc zắc rồi ho sặc sụa tự phát nổ lan. |

---

### 4.3. Danh Mục Pháp Bảo Hộ Thân (Relics — Hộ Vệ, Đính Kèm Đòn Đánh & Fun/Bựa)

Người chơi chọn tối đa **3 Pháp bảo** mang theo vào trận:

| ID | Tên Pháp Bảo | Hệ | Cơ Chế Hoạt Động Cốt Lõi | Hiệu Ứng Bổ Trợ Cho Chặt Chém |
|---|---|---|---|---|
| `R001` | **Bùa Trấn Yêu** | Mộc | **Hộ Vệ Sau Lưng:** 3 lá bùa xoay quanh thân nhân vật | Đẩy lùi quái áp sát từ phía sau lưng khi người chơi đang chém quái phía trước. |
| `R002` | **Cửu Vĩ Hồ Trảo** | Hỏa | **On-Hit Imbuement:** Đính kèm đòn chém | Mỗi khi chém trúng quái, móng vuốt lửa cào thêm 1 đòn thiêu đốt và hút 1% HP. |
| `R003` | **Trống Đồng Đông Sơn** | Thổ | **Aura Khống Chế:** Đập nhịp định kỳ mỗi 3s | Phát sóng âm làm choáng quái xung quanh trong $0.8s$, tạo khoảng trống an toàn để combo. |
| `R004` | **Lựu Đạn Thần Sa** | Hỏa | **Combo Finisher:** Nổ theo đòn kết thúc | Khi tung đòn chém thứ 3 của combo, tự động phóng ra 1 viên lựu đạn nổ đẩy lùi quái. |
| `R005` | **Trượng Long Vương** | Thủy | **On-Crit Imbuement:** Sét lan khi bạo kích | Khi đòn chém gây sát thương Chí mạng, phóng tia sét nước giật lan 4 quái lân cận. |
| `R006` | **Nước Thánh Chùa Hương** | Thủy | **Ground Hazard:** Vũng làm chậm | Để lại vũng nước thánh dưới chân nhân vật làm chậm quái $40\%$ và hồi máu nhẹ. |
| `R007` | **Chiếu Trải Hoàng Tuyền** | Mộc | **Troll Hazard & Fast Lane:** Ném chiếu cói ra sàn | Quái dẫm vào thì *"Ngủ say tại chỗ"* 3s; người chơi dẫm vào thì lướt trượt như ván trượt siêu tốc (+100% Speed) húc bay đàn quái. |
| `R008` | **Chổi Lông Gà Gia Truyền** | Kim | **On-Hit Slapstick Knockback:** Triệu hồi chổi quất | Đòn thứ 3 triệu hồi chổi lông gà khổng lồ quất quái bay dính chặt vào vách đá/mép màn hình, quái hét toáng lên. |

---

### 4.4. Kỹ Năng Tuyệt Kỹ Nhân Vật Đặc Biệt (Signature Skills — Fun & Độc Đáo)

| Nhân Vật | Tên Tuyệt Kỹ | Thời Gian Hồi | Cơ Chế Hoạt Động & Yếu Tố Fun/Bựa |
|---|---|---|---|
| **Thư Sinh** | **Bút Sa Gà Chết** | $25s$ | Cầm bút vẽ ngoáy hình một **Con Gà Chọi Khổng Lồ**. Con gà lao ra mổ lia lịa vào mông quái vật, khiến quái hoảng loạn bỏ chạy tán loạn khắp sàn đấu. |
| **Đạo Sĩ** | **Bùa Tráo Hồn** | $20s$ | Dán bùa tráo đổi vị trí ngay lập tức giữa bản thân với 1 con quái to trong đàn. Toàn bộ đạn đạo và đòn đánh của đàn quái dội thẳng vào chính đồng bọn vừa bị tráo. |
| **Thanh Đồng** | **Aura Loa Phường** | $28s$ | Bật loa phát thanh hát văn cực đại: Sóng âm làm quái ôm tai co giật, đồng thời bẻ cong phản xạ toàn bộ đạn đạo của quái bay ngược lại kẻ bắn (Reflect). |
| **Võ Tăng** | **Thiết Đầu Công** | $22s$ | Tích lực rồi phóng đầu trọc như tên lửa tông thủng đội hình quái; khi va chạm phát ra tiếng chuông chùa *"BOONG!"* ngân vang làm nổ tung đạn đạo xung quanh. |

---

## 5. Hệ Thống Nâng Cấp Trong Trận (In-Run Roguelite Upgrades)

Khi tiêu diệt Yêu ma và thu thập Hạt Kinh Nghiệm (Exp Gem) để **Lên Cấp (Level Up)**, game sẽ tạm dừng và hiển thị **3 Thẻ Nâng Cấp Ngẫu Nhiên** thuộc 4 nhóm chiến thuật:

```
                      [ GIAO DIỆN LÊN CẤP TRONG TRẬN ]
                                     │
       ┌─────────────────────┬───────┴─────────────┬─────────────────────┐
       ▼                     ▼                     ▼                     ▼
[1. Biến Hóa Combo]   [2. Thức Tỉnh]        [3. Cường Hóa Lướt]   [4. Kích Ứng]
 (Combo Augments)     (Relic Awakening)       (Dash Traits)       (Passives)
```

### 5.1. Nhóm 1: Biến Hóa Chuỗi Đòn Đánh (Combo Augments / Bí Kíp)
* **Kiếm Khí Trảm:** Đòn đánh thứ 3 giải phóng sóng kiếm khí bay thẳng $5m$.
* **Trảm Phong Liên Hoàn:** Giảm $30\%$ thời gian trễ giữa các nhát chém, mở rộng chuỗi combo lên 4 đòn.
* **Trọng Trảm Diện Rộng:** Tăng $40\%$ góc quét hình quạt của nhát chém, tăng lực đẩy lùi (Knockback).
* **Áp Sát Trảm (Lunge Slash):** Nhát chém đầu tiên tự động lướt nhanh $2m$ áp sát kẻ địch.

### 5.2. Nhóm 2: Thức Tỉnh & Cường Hóa Pháp Bảo Đã Trang Bị (Relic Awakening)
* **Cường Hóa Bùa Trấn Yêu:** Tăng số lượng từ 3 lên 5 lá bùa; lá bùa phát nổ gây choáng $1s$ khi chạm quái Tinh Anh.
* **Cường Hóa Cửu Vĩ Hồ Trảo:** Đòn cào lửa tạo vệt thiêu đốt trên mặt đất duy trì trong $3s$.
* **Cường Hóa Trống Đồng:** Sóng âm phát nổ thêm 1 đợt vọng âm sau $1s$.
* **Cường Hóa Chiếu Hoàng Tuyền:** Chiếu rộng gấp đôi, người chơi trượt trên chiếu bắn ra tia lửa điện xung quanh.

### 5.3. Nhóm 3: Cường Hóa Cơ Động & Kỹ Năng Lướt (Dash Traits)
* **Tàn Ảnh Kiếm:** Khi lướt qua kẻ địch, để lại tàn ảnh phát nổ gây sát thương hệ Phong/Mộc.
* **Lướt Trượt Vỏ Chuối (Banana Dash):** Khi lướt bỏ lại vỏ chuối trơn trượt; quái đuổi theo giẫm phải sẽ trượt chân té sấp mặt (Ragdoll Knockup) và tự nổ văng.
* **Lướt Phản Đòn (Parry Dash):** Lướt đúng lúc quái tung đòn sẽ làm quái choáng $1.5s$ và nhận $+50\%$ Crit cho đòn chém tiếp theo.
* **Lướt Vô Ảnh:** Tích lũy tối đa 2 lần lướt liên tiếp, giảm thời gian hồi lướt đi $25\%$.

### 5.4. Nhóm 4: Bổ Trợ Chỉ Số Tình Huống (Conditional Combat Passives)
* **Trảm Hậu (Backstab):** Chém vào sau lưng kẻ địch nhận $+100\%$ Sát thương Chí mạng.
* **Cuồng Nộ (Berserk):** Khi HP dưới $30\%$, tăng $+40\%$ Tốc độ đánh và $+10\%$ Hút máu.
* **Hành Quyết (Execute):** Đòn chém có $10\%$ tỷ lệ kết liễu ngay lập tức quái thường dưới $20\%$ HP.

### 5.5. Cơ Chế Đột Phá Tuyệt Kỹ (Breakthrough Milestones — Level 5 & 10)
Tại các cột mốc sức mạnh đặc biệt (Cấp 5, Cấp 10 hoặc sau khi hạ Boss Phút 10), giao diện sẽ xuất hiện **3 "Bí Tịch Tuyệt Kỹ"** biến đổi hoàn toàn phong cách chơi:
1. **Bát Quái Kiếm Trận:** Đòn chém triệu hồi phi kiếm bay lượn tự động găm vào mọi kẻ địch xung quanh.
2. **Hóa Thần Nhập Ma:** Sát thương vũ khí tăng gấp đôi ($+100\%$), đổi lại sát thương nhận vào tăng $20\%$.
3. **Thái Cực Hộ Mệnh:** Mọi Pháp bảo hộ thân tăng gấp đôi kích thước và bán kính bảo vệ.

---

## 6. Hệ Thống Ngũ Hành & Cán Cân Âm Dương Trong Action RPG

### 6.1. Vòng Tương Khắc (1 Chiều ×1.3 Sát Thương)
$$\text{Kim} \rightarrow \text{Mộc} \rightarrow \text{Thổ} \rightarrow \text{Thủy} \rightarrow \text{Hỏa} \rightarrow \text{Kim}$$
* Đòn chém tay từ Vũ Khí Chính và đòn đánh của Pháp Bảo khi trúng quái vật bị khắc hệ sẽ tự động kích hoạt **Damage Popup Vàng Kim rực rỡ** kèm hệ số sát thương $+30\%$.

### 6.2. Vòng Tương Sinh Trong Nhịp Chặt Chém (Elemental Resonance)
$$\text{Kim} \rightarrow \text{Thủy} \rightarrow \text{Mộc} \rightarrow \text{Hỏa} \rightarrow \text{Thổ} \rightarrow \text{Kim}$$
* **Cơ chế:** Đòn chém tay của Vũ khí chính đóng vai trò **"Dẫn Hệ"**. Khi đòn chém trúng quái kết hợp với Pháp bảo thuộc hệ tương sinh kế tiếp trong vòng $3s$ $\rightarrow$ Kích hoạt hiệu ứng **Cộng Hưởng Nguyên Tố**:
  * Tức thời giảm $20\%$ Cooldown của Kỹ năng & Lướt.
  * Tăng $+25\%$ Tốc độ chém tay trong $3s$.

### 6.3. Cán Cân Âm Dương (Yin-Yang Action State)
Thang đo `yinYangValue` (0 – 100, Mặc định 50 - Thái Cực):
* **Dương Thịnh (> 80):** Tích tụ khi người chơi **liên tục lướt áp sát và chém ở cự ly gần** $\rightarrow$ Mở khóa nhánh thẻ *"Cuồng Bạo"* (Tăng mạnh Damage & Tốc đánh).
* **Âm Thịnh (< 20):** Tích tụ khi người chơi **giữ khoảng cách, né tránh và đánh thả diều** $\rightarrow$ Mở khóa nhánh thẻ *"Tịch Diệt"* (Tăng Né tránh & Hồi phục).
* **Thái Cực Cân Bằng (40 – 60):** Duy trì nhịp đánh - lướt cân bằng $\rightarrow$ Mở khóa thẻ Đột Phá độc quyền *"Thái Cực"*.

---

## 7. Thiết Kế Kẻ Địch, Boss & Nhịp Độ Màn Chơi (Enemies & Combat Pacing)

### 7.1. Điều Chỉnh Mật Độ & Chất Lượng Quái (Enemy Wave Balance)
* **Số lượng quái đồng thời:** Giảm từ 200 quái xuống **30 – 50 quái/wave** để đảm bảo không gian thao tác lướt, né và chặt chém không bị nghẽn góc.
* **Máu và Sát thương:** Tăng lượng máu (HP) của quái lên $2.5\text{x}$ so với bản Survivor cũ để mỗi chuỗi combo chém vào quái có cảm giác "đã tay, đẫm lực".
* **Chỉ Báo Đòn Đánh (Telegraphing / Red Zones):**
  * Quái tinh anh và Boss có vệt đỏ/khung cảnh báo trước khi vung đòn ($0.3s - 0.5s$) để người chơi kịp dùng nút **Dash** né tránh phản xạ.

### 7.2. Danh Mục Yêu Ma MVP & Đặc Tính Chặt Chém / Slapstick

| Enemy ID | Tên Yêu Ma | Hệ | Vai Trò Combat | Cơ Chế Phản Ứng Khi Bị Chém & Yếu Tố Fun |
|---|---|---|---|---|
| `E_MAGIAP` | **Ma Giáp** | Kim | Quái lính đi bộ | Bị hất lùi (Knockback) rõ rệt theo hướng vung đao; có thể bị ném dính vào tường. |
| `E_MATROI` | **Ma Trơi** | Hỏa | Quái cơ động lao nhanh | Bay zíc zắc khó đoán; nếu chém hụt lao vào đá tự choáng bản thân $1s$. |
| `E_QUYNHAPTRANG`| **Quỷ Nhập Tràng**| Thổ | Quái Tanker Giáp Nặng | Kháng hất lùi; khi gặp hiệu ứng âm thanh (Trống/Loa) sẽ dừng đánh đứng nhảy nhót. |
| `E_MADA` | **Ma Da** | Thủy | Quái bắn tỉa từ xa | Phun nước độc; khi bị chém trúng sẽ trơn tuột văng ra xa như xà phòng. |
| `E_HOALYTINH` | **Hồ Ly Tinh Nhỏ**| Hỏa | Quái cảm tử phát nổ | Khi bị chém chết sẽ có $0.5s$ đếm ngược phát nổ; có thể dùng đòn 3 đánh bay nó vào giữa đàn quái khác. |
| `E_MADOINO` | **Ma Đòi Nợ** | Kim | Quái trộm cắp chạy nhanh | Không đánh mất máu; lao vào cướp 50 Cổ Tiền/Exp rồi bỏ chạy. Diệt trong 5s nhận thưởng gấp đôi! |

### 7.3. Boss Cai Ngục Hoàng Tuyền & Cơ Chế Tương Tác Vui Nhộn
* **Ngưu Đầu Mã Diện (Phút 10 — "Đấu Vật Biểu Diễn"):**
  * *Hành vi:* Húc càn tông thẳng (Bull Dash) và Giậm đất tạo chấn động.
  * *Tương tác Fun/Slapstick:* Khi cả 2 con cùng húc, nếu người chơi Dash né ở giữa, chúng sẽ **tự đâm đầu vào nhau** kêu *"Ối!"*, tự choáng kép $4s$ và rớt ra kho báu lớn.
* **Diêm Vương (Phút 20 — Final Boss):**
  * *Hành vi:* Quét sóng kiếm quạt 3 hướng, đặt bẫy Lưới Nghiệp Báo và luân phiên đổi 5 hệ Ngũ Hành.
  * *Chiến thuật:* Vận dụng luân chuyển chuỗi combo Ngũ Hành và sử dụng đòn Đột Phá Cấp 10 để dứt điểm.

---

## 8. Cảm Giác Đòn Đánh & Game Feel (Juice & Feedback)

Để lối chơi Chặt chém đạt mức thỏa mãn cao nhất (Satisfying Hack & Slash):
1. **Hit-Stop (Khựng hình tác động):** Khi đòn chém trúng quái, khung hình dừng lại $0.04s$ (chí mạng dừng $0.08s$) tạo cảm giác lưỡi kiếm thực sự va chạm vào mục tiêu.
2. **VFX Vệt Chém (Directional Slash Arc):** Vệt chém phát sáng theo thuộc tính Ngũ Hành (Kim vàng, Mộc lục, Thủy lam, Hỏa đỏ, Thổ cam) uốn cong theo chuyển động vung tay của nhân vật.
3. **Camera Shake & Lực Đẩy Lùi (Knockback):** Đòn kết thúc combo thứ 3 luôn đi kèm rung nhẹ màn hình ($0.1s$) và hất văng quái vật ngược về sau.

---

## 9. Lộ Trình Triển Khai Kỹ Thuật (Engineering Roadmap)

1. **Giai đoạn 1 — Tái Cấu Trúc Weapon & Input Model:**
   * Cập nhật `WeaponManager` và `WeaponBase` hỗ trợ trạng thái `PrimaryWeapon` với chuỗi 3-Hit Combo.
   * Hoàn thiện cụm phím điều khiển chiến đấu: `AttackButton`, `DashButton`, `SignatureSkillButton`.
2. **Giai đoạn 2 — Tái Cấu Trúc Hệ Thống Nâng Cấp Trong Trận:**
   * Xây dựng hệ thống thẻ nâng cấp phân nhóm: Combo Augments, Relic Awakening, Dash Traits, Passives.
   * Xây dựng giao diện Đột Phá Tuyệt Kỹ tại Level 5 & 10.
3. **Giai đoạn 3 — Cân Bằng Lại Enemy Wave & Spawner:**
   * Giảm mật độ spawn xuống $30-50$ quái/wave, tăng HP quái và bổ sung hành vi Telegraphing báo đòn.
4. **Giai đoạn 4 — Đánh Bóng Game Feel & Hiệu Năng:**
   * Tích hợp Hit-stop, Knockback, VFX vệt chém và kiểm thử 60 FPS ổn định trên Android ARM64.
