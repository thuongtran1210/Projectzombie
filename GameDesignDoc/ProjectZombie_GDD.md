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
- **Lối Chơi Chặt Chém Đã Tay (Action Combat Feel):** Đòn đánh thường đặc trưng theo từng Tướng (Melee Slash / Ranged Projectile + Chuỗi Combo 3 đòn liên hoàn), hỗ trợ ngắm thông minh (Smart Soft-Lock), cơ chế hủy hoạt ảnh bằng Lướt (Dash Cancel) và phản hồi lực đánh đã tay (Hit-stop $0.04s$, Camera Shake, Knockback).
- **Hệ Thống Đòn Đánh Tướng & 1 Pháp Bảo Hộ Thân (Character Basic Attack & Single Relic):** Bản thể nhân vật quyết định phong cách chém tay / bắn đạn chủ động, kết hợp với **đúng 1 Pháp bảo hộ thân duy nhất** tự động bảo vệ sau lưng hoặc đính kèm hiệu ứng vào đòn chém (On-Hit Imbuement).
- **Cơ chế Ngũ Hành (Kim - Mộc - Thủy - Hỏa - Thổ):** Vận dụng đòn chém tay để kích hoạt chuỗi Tương Khắc (+30% Sát thương) và chuỗi Tương Sinh (-20% Cooldown + Tăng tốc đánh).
- **Cán Cân Âm Dương Độc Quyền:** Luân chuyển giữa lối đánh Áp Sát Liều Lĩnh (Dương Thịnh) và Thả Diều Tĩnh Tại (Âm Thịnh) để mở khóa các nhánh thẻ Đột Phá hiếm.
- **Bản Sắc Văn Hóa Dân Gian Thuần Việt:** Tích hợp truyền thuyết Tứ Bất Tử, vũ khí cổ vật (Bút Phán Quan, Nỏ Thần, Bùa Trấn Yêu, Trống Đồng Đông Sơn) và yêu quái dân gian (Ma Giáp, Ma Trơi, Quỷ Nhập Tràng, Ma Da).

---

## 2. Vòng Lặp Gameplay (Core Gameplay Loop)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       SẢNH CHỜ NGOÀI TRẬN (OUT-GAME META)                    │
│  - Chọn Nhân Vật & Đòn Đánh Đặc Trưng (Character Selection)                 │
│  - Chọn 1 Pháp Bảo Hộ Thân Duy Nhất (Single Relic Loadout)                  │
│  - Nâng cấp Chỉ số Vĩnh viễn (Sanctuary Tree) & Mở khóa Pháp Bảo Mới       │
└──────────────────────────────────────┬──────────────────────────────────────┘
                                       │ Bắt đầu Trận Chiến (Enter Run)
                                       ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                     TRẬN ĐÁNH HACK & SLASH (IN-RUN ROGUELITE)               │
│  1. Thao Tác Chiến Đấu:                                                     │
│     - Tay trái: Joystick 360° di chuyển & định hướng                        │
│     - Tay phải: Nút Đánh Tướng (Combo 3-Hit / Tầm Xa) + Lướt (Dash) + Skill │
│  2. Chặt Chém & Tiêu Diệt Yêu Ma:                                           │
│     - Đòn đánh tướng gây 70-80% DPS, 1 Pháp bảo tự động hỗ trợ xoay quanh/bắn│
│     - Kích hoạt Vòng Tương Sinh qua nhịp chém tay                           │
│  3. Thu Thập Hạt Kinh Nghiệm (Exp Gem) & Lên Cấp (Level Up):                │
│     - Chọn 1 trong 3 Thẻ Nâng Cấp Trong Trận:                               │
│       * Biến hóa Đòn Đánh Tướng (Combo Augments / Bí Kíp Võ Học)            │
│       * Cường Hóa & Tiến Hóa 1 Pháp Bảo Đang Mang (Relic Awakening)         │
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
│  - Mở khóa Nhân vật và Pháp bảo mới trong Cửa Hàng                          │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Hệ Thống Điều Khiển & Giao Diện Chiến Đấu (Controls & Combat HUD)

### 3.1. Bố Cục Cụm Phím Thao Tác (Combat Action Cluster)
* **Tay Trái (Movement Area):**
  * **Dynamic Virtual Joystick (360°):** Di chuyển nhân vật mượt mà, hỗ trợ tự động căn vị trí ngón tay chạm trên nửa trái màn hình.
* **Tay Phải (Action Button Cluster):**
  * **Nút Tấn Công Nhân Vật (Character Basic Attack Button - Kích thước Lớn Nhất):** Nhấn liên tục để thực hiện chuỗi 3-Hit Combo hoặc bắn đạn tầm xa. Hỗ trợ Tap Buffer Window $0.18s$.
  * **Nút Lướt (Dash / Dodge Button):** Lướt nhanh qua kẻ địch (có $0.15s$ khung bất tử - I-Frame). Hỗ trợ **Hủy hoạt ảnh đòn chém (Animation Cancel)** để né đòn khẩn cấp. Cooldown: $1.2s$ (hồi lại nhanh).
  * **Nút Tuyệt Kỹ (Signature Skill Button):** Kích hoạt kỹ năng đặc biệt của nhân vật với Cooldown dài ($20s - 30s$), có vòng đếm Cooldown Radial Fill.

### 3.2. Cơ Chế Hỗ Trợ Ngắm Thông Minh (Smart Soft-Lock System)
* Khi bấm nút Tấn công:
  * Nếu người chơi **đang kéo Joystick:** Nhân vật chém/bắn theo hướng Joystick chỉ định.
  * Nếu người chơi **thả Joystick:** Hệ thống tự động quét mục tiêu trong góc hình nón $90^\circ$ và bán kính $5m$ phía trước mặt $\rightarrow$ Tự động xoay người chém chính xác vào yêu ma gần nhất, loại bỏ hoàn toàn hiện tượng chém hụt vào không khí.

### 3.3. Hệ Thống Chỉ Dấu Kỹ Năng & Đòn Đánh 2.5D (Telegraph & Aim Indicator System)
Hệ thống chỉ dấu được phân tách rõ ràng theo chuẩn MOBA (Liên Quân / Wild Rift) gồm 2 tầng hoạt động độc lập:

1. **Chỉ Dấu Định Hướng Bị Động Dưới Chân (`CombatAimIndicator`):**
   * Hiển thị liên tục (Passive) một mũi tên/vòng cung nhỏ sát dưới chân nhân vật ($0.4m$).
   * Tự động xoay theo hướng di chuyển của Joystick hoặc hướng quay mặt của Tướng.
   * Tự động chuyển màu bản sắc theo Ngũ hành của Tướng (Đạo Sĩ: Xanh Ngọc, Thư Sinh: Vàng Kim, Thanh Đồng: Đỏ Cam, Ẩn Sĩ: Hổ Phách).

2. **Chỉ Dấu Kỹ Năng & Đòn Đánh Chủ Động (`SkillAimIndicatorController` / `ISkillAimService`):**
   * **Kích hoạt thông minh (Smart Touch Filter):** Thiết lập `_requireHoldOrDrag = true` để **Khử chớp nháy (No Flicker)**. Khi nhấp nhanh (Quick Tap < 0.12s), đòn đánh/chiêu thức được kích hoạt ngay lập tức qua Auto-Aim mà không bật chỉ dấu; Chỉ dấu chỉ xuất hiện khi người chơi **ĐÈ (Hold > 0.12s)** hoặc **KÉO (Drag > 25px)** để căn chỉnh cự ly/góc chém.
   * **Công nghệ đồ họa URP:** 
     * `ConeSector`: Sử dụng Shader chuyên dụng `SH_URP_SkillIndicator_Sector` tính toán Tọa độ cực (Polar Coordinates) trên GPU để hiển thị góc quạt quét từ $30^\circ$ đến $270^\circ$ với viền phát sáng (Glow Border) và làm mịn cạnh (Anti-Aliasing Feather).
     * `LineArrow`: Dải định hướng tầm xa (Nỏ thần, Cung Thạch Sanh).
     * `CircleReticle`: Vòng tròn tâm rơi AOE tự do theo cự ly kéo (Nước thánh, Lựu đạn thần sa).
     * `SelfAOE`: Hào quang kích hoạt quanh thân nhân vật (Khiên hộ thể, Trống Đồng).
     * `DashLine`: Chỉ báo hướng lướt né đòn và hiển thị điểm đáp nhân vật.
   * **Vùng Hủy Chiêu (Cancel Zone) & Rung Xúc Giác (Haptic Feedback):** Khi kéo ngón tay vào biểu tượng Hủy chiêu ở góc trên, thiết bị lập tức **rung nhẹ (Haptic Vibration)** phản hồi xúc giác, toàn bộ chỉ dấu chuyển sang màu đỏ rực cảnh báo và triệt tiêu lệnh tung chiêu khi nhấc tay.
   * **Tối ưu hiệu năng:** Ứng dụng `MaterialPropertyBlock` cập nhật góc quét và màu sắc thời gian thực với Zero Memory Allocation (GC Alloc = 0).

---

## 4. Hệ Thống Đòn Đánh Nhân Vật & 1 Pháp Bảo Hộ Thân (Combat & Relic Architecture)

### 4.1. Phân Định Cấu Trúc Sức Mạnh (Combat Role & DPS Distribution)
* **Đòn Đánh Bản Thể Tướng (`CharacterCombat` — Chiếm 75 - 85% Tổng DPS):** Là công cụ tấn công chủ động bằng tay của người chơi qua nút Attack. Mỗi nhân vật sở hữu bộ Animation, VFX chém Melee hoặc đạn Ranged và combo 1-2-3 riêng biệt.
* **Pháp Bảo Hộ Thân (`Relic` — Chiếm 15 - 25% Tổng DPS):** Người chơi mang theo **duy nhất 1 Pháp bảo** vào trận. Đóng vai trò **Khống chế (CC), tự động bay quanh bảo vệ (`Orbit`), hoặc tự kích hoạt theo chu kỳ độc lập**.
* **Tuyệt Kỹ Nhân Vật (Signature Skill — Chiếm 10 - 15% Tổng DPS):** Kỹ năng giải vây, buff bùng nổ hoặc xoay chuyển thế trận.

---

### 4.2. Danh Mục 17 Pháp Bảo Hộ Thân (Relics — Hộ Vệ, Tự Động Ra Đòn & Slapstick Fun)

Mọi trang bị trong Tàng Bảo Các đều là **Pháp Bảo Hộ Thân**. Người chơi chọn **đúng 1 Pháp Bảo** mang theo vào trận để hỗ trợ tự động:

#### A. Nhóm 12 Pháp Bảo Cổ Phong (Vòng Xuyến Truyền Thuyết)
| ID | Tên Pháp Bảo | Hệ | Cơ Chế Hoạt Động Cốt Lõi | Hiệu Ứng Hộ Thân Tự Động |
|---|---|---|---|---|
| `W001` | **Nỏ Thần** | Kim | `RelicOnHitTrigger` | Bắn linh tiễn An Dương Vương xuyên thấu 2 kẻ địch gần nhất. |
| `W002` | **Bút Phán Quan** | Kim | `RelicOnHitTrigger` | Vung nhát chém phán quyết âm ty gây sát thương chí mạng 2 bên. |
| `W003` | **Bùa Trấn Yêu** | Mộc | `RelicOrbitalShield` | 3 lá bùa xoay quanh thân nhân vật, cản đạn và đẩy lùi quái áp sát. |
| `W004` | **Cửu Vĩ Hồ Trảo** | Hỏa | `RelicOnHitTrigger` | Móng vuốt cáo lửa tự tìm diệt quái và hút sinh khí hồi phục cho Tướng. |
| `W005` | **Trống Đồng Đông Sơn** | Thổ | `RelicOrbitalShield` | Phát sóng âm trảm linh 5 hướng làm choáng diện rộng xung quanh ($0.8s$). |
| `W006` | **Lựu Đạn Thần Sa** | Hỏa | `RelicOnHitTrigger` | Quăng hạt thần sa phát nổ tạo bão lửa thiêu rụi vùng rộng và đẩy lùi mạnh. |
| `W007` | **Cung Thạch Sanh** | Kim | `RelicOnHitTrigger` | Bắn mũi tên thần lực Thạch Sanh xuyên qua hàng loạt yêu tinh trên đường thẳng. |
| `W008` | **Đao Cửu Vĩ** | Hỏa | `RelicSupportAura` | Phun luồng rồng lửa thiêu đốt liên tục kẻ địch trước mặt (DoT). |
| `W009` | **Trượng Long Vương** | Thủy | `RelicSupportAura` | Phóng sét nước thủy cung lan truyền qua chuỗi 6 yêu quái gây Choáng 0.5s. |
| `W010` | **Linh Phù Ma Da** | Thủy | `RelicSupportAura` | Triệu hồi linh thú Ma Da phun độc sát thương liên tục lên kẻ địch. |
| `W011` | **Nước Thánh Chùa Hương** | Thổ | `RelicSupportAura` | Tạo bãi giếng thiêng trên mặt đất làm chậm quái $40\%$ và gây sát thương liên tục. |
| `W012` | **Phi Tiêu Bát Quái** | Mộc | `RelicOnHitTrigger` | Phi tiêu ma thuật tự động xoay tròn quét kẻ địch theo hình cánh cung rồi quy hồi. |

#### B. Nhóm 5 Pháp Bảo Dân Gian Hài Hước (Slapstick Relics)
| ID | Tên Pháp Bảo | Hệ | Cơ Chế Hoạt Động Cốt Lõi | Hiệu Ứng Hộ Thân Tự Động |
|---|---|---|---|---|
| `W_SLIPPER` | **Dép Tổ Ong Thần Sa** | Kim | `RelicOnHitTrigger` | Ném Boomerang dép tự động; Hit 3 quăng lốc dép gây hiệu ứng *"Quê Độ"* (quái tự đấm nhau). |
| `W_POT` | **Nồi Cơm Thạch Sanh** | Thổ | `RelicOrbitalShield` | Gom tối đa 3-5 quái vào nồi và phóng ra như đạn pháo; chạm đất rơi cơm nắm hồi 5% HP. |
| `W_PIPE` | **Điếu Cày Cửu U** | Hỏa | `RelicSupportAura` | Phun bão khói *"Say Thuốc Lào"* dày đặc khiến quái đi giật lùi và nổ sát thương ho sặc sụa. |
| `R007` | **Chiếu Trải Hoàng Tuyền** | Mộc | `RelicSupportAura` | Thả chiếu bẫy ngủ say 3s (nhận x2 Crit DMG); Tướng bước lên trượt ván siêu tốc (+100% Move Speed) ủi bay quái. |
| `R008` | **Chổi Lông Gà Gia Truyền** | Kim | `RelicOnHitTrigger` | Triệu hồi chổi khổng lồ giáng từ trời, tạo lực đẩy lùi cực đại 12m/s găm quái vào tường gây Choáng. |

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

## 10. Tiêu Chuẩn Kỹ Thuật Hiệu Năng & Tải Trận Đấu (Zero-Lag Performance Standard)

Để đảm bảo trải nghiệm mượt mà $60\text{ FPS}$ ổn định trên mọi thiết bị di động Android/iOS tầm trung:
1. **Real Async Loading Pipeline:** Tận dụng 100% màn hình Loading Screen để nạp ngầm toàn bộ Prefab, Object Pool quái vật và khởi tạo FMOD Audio Stream. Khi Loading Screen tắt, chiến trường bắt đầu ngay với $0\text{ms}$ lag.
2. **Time-Sliced Pooling:** Toàn bộ việc sinh đối tượng hàng loạt (Quái vật, EXP Gems, Damage Numbers) phải phân bổ qua Coroutine (tối đa 3-5 đối tượng/frame) để CPU frame time không vượt quá ngưỡng $16.6\text{ms}$.
3. **Zero Garbage Collection (0-Alloc Loop):** Vòng lặp `Update` / `LateUpdate` / `FixedUpdate` của hệ thống Combat, Chỉ dấu kỹ năng, HUD Canvas và Audio tuyệt đối không tạo `GC.Alloc` (sử dụng Static Delegates, NonAlloc Physics, MaterialPropertyBlock).
4. **Streaming Audio Mode:** Toàn bộ nhạc nền (BGM) dài $>1\text{ phút}$ phải cấu hình `loadType: 2 (Streaming)` và `loadInBackground: 1` để triệt tiêu tải giải nén CPU.

*(Chi tiết xem thêm tại tài liệu kỹ thuật chuyên sâu: [PERFORMANCE_OPTIMIZATION_REPORT.md](file:///c:/Users/thuon/Unity/Projectzombie/GameDesignDoc/PERFORMANCE_OPTIMIZATION_REPORT.md)).*

