# Game Design Document — ProjectZombie

**Phiên bản:** 2.0
**Thể loại:** Top-down Survival Roguelite / Auto-battler (tương tự *Vampire Survivors*, *Brotato*)
**Nền tảng:** WebGL — tích hợp trong hệ sinh thái Web Jamstack (ReactJS + Supabase)
**Phong cách đồ họa:** 2D, góc nhìn từ trên xuống (Top-down)

---

## 1. Tổng quan (Overview)

### 1.1. Tầm nhìn sản phẩm (Vision Statement)
ProjectZombie mang đến trải nghiệm sinh tồn nghẹt thở, nơi người chơi đối đầu với đàn Zombie ngày càng đông đảo, xây dựng "cỗ máy hủy diệt" của riêng mình qua các lựa chọn nâng cấp ngẫu nhiên, và cạnh tranh thứ hạng với cộng đồng qua bảng xếp hạng toàn cầu — tất cả chạy mượt trên trình duyệt web, không cần cài đặt.

### 1.2. Đối tượng người chơi (Target Audience)
- Người chơi web casual/mid-core, quen thuộc với thể loại survivor-like.
- Phiên chơi ngắn (5–20 phút/run), phù hợp chơi giữa giờ nghỉ.
- Có yếu tố cạnh tranh nhẹ (leaderboard) thu hút người chơi quay lại.

### 1.3. Điểm khác biệt (Unique Selling Points)
- Hệ thống vũ khí "Lego" module hóa cho phép tổ hợp gameplay đa dạng mà không cần nhiều code riêng lẻ.
- Tích hợp sâu với nền tảng Web (đăng nhập, lưu tiến trình, leaderboard) thay vì là game WebGL đơn lẻ.
- Chơi được cả khi mất mạng (offline-first) nhờ hàng đợi đồng bộ.

### 1.4. Yêu cầu kỹ thuật tối thiểu (Target Performance)
- 60 FPS trên desktop browser tầm trung; tối thiểu 30 FPS ổn định trên mobile browser.
- Kích thước build WebGL ban đầu (First Load): mục tiêu dưới 25MB (nén Brotli/Gzip) để đảm bảo thời gian tải chấp nhận được trên web.
- Hỗ trợ tối thiểu 150–200 enemy + 100 projectile hoạt động đồng thời trên màn hình không giật lag.

---

## 2. Vòng lặp Gameplay (Core Gameplay Loop)

> [!NOTE]
> Gameplay tập trung vào khả năng sinh tồn của người chơi giữa bầy Zombie ngày càng đông đảo, đòi hỏi chiến thuật lựa chọn vũ khí và nâng cấp hợp lý.

### 2.1. Vòng lặp trong 1 trận (Moment-to-moment Loop)
1. **Sinh tồn & Chiến đấu**: Người chơi điều khiển nhân vật di chuyển trên bản đồ để né tránh kẻ địch. Hệ thống vũ khí tự động tấn công (Auto-attack) dựa trên tầm đánh và tốc độ đánh.
2. **Thu thập & Lên cấp**: Tiêu diệt Zombie để làm rớt Hạt Kinh Nghiệm (Exp Gem). Nhặt Exp làm đầy thanh kinh nghiệm, giúp người chơi Lên cấp (Level Up).
3. **Nâng cấp (Upgrades)**: Mỗi lần lên cấp, game tạm dừng và hiện màn hình chọn Nâng cấp (Weighted Random – Gacha). Người chơi chọn 1 trong 3 thẻ để nhận vũ khí mới, nâng cấp vũ khí cũ, hoặc nhận chỉ số bị động (Passive).
4. **Tiến hóa (Evolution)**: Khi vũ khí đạt cấp tối đa và người chơi sở hữu Passive tương ứng, vũ khí có cơ hội "Tiến hóa" thành phiên bản tối thượng.
5. **Kết thúc trận**: Trận đấu kết thúc khi người chơi chết, hoặc sống sót hết thời gian giới hạn (Survival Timer) / hạ Boss cuối.

### 2.2. Vòng lặp giữa các trận (Meta Loop) — *bổ sung*
1. Kết thúc trận → tính điểm dựa trên: thời gian sống sót, số Zombie tiêu diệt, cấp độ đạt được.
2. Điểm số quy đổi thành **Currency Meta** (vd: "Coin Sinh Tồn") tích lũy vĩnh viễn, không mất khi bắt đầu run mới.
3. Currency Meta dùng để mở khóa: nhân vật mới, vũ khí khởi đầu mới, hoặc nâng cấp vĩnh viễn (permanent upgrade tree) — vd tăng % HP khởi điểm, tăng tỉ lệ rơi Exp.
4. Người chơi quay lại chơi run mới với build mạnh hơn nhờ Meta-progression → tăng động lực retention.

### 2.3. Vòng lặp hệ thống Web Platform
- Bắt đầu chơi → Kết thúc (Chết/Thắng) → Tính điểm.
- Tạo Checksum mã hóa → Gửi điểm lên Web Platform qua JS Bridge → Cập nhật Bảng xếp hạng (Leaderboard) qua Supabase Edge Function.

---

## 3. Hệ thống Nhân vật & Điều khiển (Player System)

### 3.1. Điều khiển (Controls)
- Bàn phím (WASD / Mũi tên) để di chuyển.
- (Tùy chọn) Hỗ trợ Joystick ảo khi chơi trên Mobile Web (touch input).
- Không có nút tấn công thủ công — toàn bộ vũ khí tự động kích hoạt (giữ đúng tinh thần auto-battler).

### 3.2. Chỉ số nhân vật (Player Stats)
| Nhóm | Chỉ số | Mô tả |
|---|---|---|
| Phòng thủ | Health (Máu) | HP tối đa, hồi phục qua vật phẩm/passive |
| Phòng thủ | Armor (Giáp) | Giảm sát thương nhận vào theo % hoặc số cố định |
| Phòng thủ | Move Speed | Tốc độ di chuyển, ảnh hưởng khả năng né tránh |
| Tấn công | Damage | Sát thương cơ bản, hệ số nhân cho tất cả vũ khí |
| Tấn công | Attack Speed | Tốc độ đánh, giảm cooldown giữa các đòn |
| Tấn công | Range | Tầm đánh, mở rộng vùng ảnh hưởng vũ khí |
| Tấn công | Crit Chance / Crit Damage | Tỉ lệ và hệ số sát thương chí mạng |
| Utility | Pickup Radius *(bổ sung)* | Bán kính hút Exp Gem tự động |
| Utility | Luck *(bổ sung)* | Ảnh hưởng tỉ lệ xuất hiện Rare Upgrade / vật phẩm hiếm |
| Utility | Cooldown Reduction *(bổ sung)* | Giảm thời gian hồi của kỹ năng đặc trưng (Signature Skill) |

### 3.3. Passives
Danh sách kỹ năng bị động người chơi thu thập được trong trận, dùng làm điều kiện kích hoạt Tiến hóa (Evolution) và cộng dồn chỉ số nền.

### 3.4. Nhân vật khả dụng (Character Roster) — *bổ sung, cần hoàn thiện*
Mỗi nhân vật có:
- Bộ chỉ số khởi điểm khác nhau (vd: nhân vật tank thấp Move Speed nhưng cao HP/Armor).
- Vũ khí khởi đầu (Starting Weapon) cố định gắn với nhân vật.
- 1 Signature Skill riêng biệt (kỹ năng chủ động có cooldown, không phải auto-attack).
- Điều kiện mở khóa qua Currency Meta hoặc thành tựu trong game.

*(Ghi chú: cần bổ sung bảng danh sách nhân vật cụ thể — số lượng, tên, chỉ số, ở giai đoạn Production tiếp theo.)*

---

## 4. Hệ thống Vũ khí (Weapons & Projectiles)

> [!TIP]
> Hệ thống thiết kế theo kiến trúc Component-Based và Data-Driven (Lego-style), giúp dễ dàng kết hợp và tạo ra hàng chục loại vũ khí khác nhau.

### 4.1. Phân loại Vũ khí (Weapon Types)
Người chơi có thể sở hữu nhiều vũ khí cùng lúc (Multi-Weapon).

| Loại | Mô tả |
|---|---|
| `Weapon_Targeted` | Ngắm bắn tự động vào mục tiêu gần nhất |
| `Weapon_DualSlash` | Đòn cận chiến chém đồng thời hai bên trái/phải (dùng `OverlapBoxNonAlloc` để tối ưu bộ nhớ) |
| `Weapon_Orbit` | Đạn/vũ khí bay theo quỹ đạo xoay quanh nhân vật, tạo lớp phòng thủ sát thương |
| `Weapon_PetSummon` | Triệu hồi đệ/thú cưng đồng hành, tự động hỗ trợ tấn công |

### 4.2. Hệ thống đạn "Lego" (Projectile System)
- **Module Di chuyển (Movement)**: `Move_Linear` (bay thẳng), `Move_Orbit` (xoay quanh tâm).
- **Module Va chạm & Sát thương (Hit)**: `Hit_SingleTarget` (sát thương chạm 1 lần, hỗ trợ xuyên thấu), `Hit_Periodic` (sát thương AOE duy trì theo chu kỳ thời gian).
- **Behavior Scriptables**: Straight, Homing (đạn đuổi), Bounce (nảy), Explosion (nổ), Split (tách vỡ).

### 4.3. Giới hạn kỹ thuật vũ khí — *bổ sung*
- Giới hạn số lượng slot vũ khí đồng thời (đề xuất: 6 slot vũ khí + 6 slot Passive, theo chuẩn thể loại).
- Object Pooling bắt buộc cho toàn bộ Projectile để tránh Garbage Collection spike trên WebGL (xem mục 9.4).

---

## 5. Trí tuệ Nhân tạo Kẻ địch (Enemy AI)

Kẻ địch sử dụng **Mô hình Trạng thái (FSM)** kết hợp **Strategy Pattern**.

### 5.1. Các trạng thái FSM cơ bản
`Idle` (Đứng yên) → `Chase` (Truy đuổi) → `Attack` (Tấn công) → `Reposition` (Tách bầy tránh kẹt) → `Dead` (Chết & rớt Exp).

### 5.2. Các Chiến lược (Strategy)
- **Melee (Cận chiến)**: Áp sát, bao vây, tấn công trực tiếp.
- **Ranged (Bắn xa)**: Giữ khoảng cách an toàn, bắn đạn từ xa về phía người chơi.

### 5.3. Phân cấp độ khó & Spawn Wave — *bổ sung, cần hoàn thiện*
- **Enemy Tier**: đề xuất phân theo 3 cấp — Common (Zombie thường), Elite (biến thể mạnh hơn, xuất hiện theo mốc thời gian), Boss (xuất hiện cuối mỗi mốc lớn, vd phút thứ 5/10/15/20).
- **Spawn Curve**: Số lượng và độ mạnh của Zombie tăng dần theo thời gian sống sót của người chơi (Difficulty Scaling), cần bảng cụ thể ở mục 10.
- **Faction**: Ghi chú — hệ thống `FactionCounterUpgrade` (mục 6) yêu cầu kẻ địch được phân theo phe/loại cụ thể, cần định nghĩa rõ danh sách Faction (vd: Zombie Thường, Zombie Nhiễm Độc, Zombie Giáp...).

---

## 6. Hệ thống Nâng cấp (Upgrades System)

Quản lý hoàn toàn qua **ScriptableObjects**, giúp Game Designer tự do điều chỉnh không cần code.

| Loại thẻ | Mô tả |
|---|---|
| `WeaponUpgrade` | Nâng cấp cấp độ vũ khí đang có, hoặc mở khóa vũ khí mới |
| `CommonUpgrade` | Nâng cấp chỉ số cơ bản của nhân vật |
| `SignatureSkillUpgrade` | Nâng cấp kỹ năng đặc trưng riêng cho từng nhân vật |
| `FactionCounterUpgrade` | Tăng sát thương khi đối đầu phe địch cụ thể |
| `RareUpgrade` | Nâng cấp hiếm, mang lại chỉ số đột biến |
| `EvolutionUpgrade` | Nâng cấp tối thượng (thường ở Cấp 6) — điều kiện: vũ khí gốc đạt cấp tối đa + sở hữu Passive yêu cầu (`requiredPassiveId`); hiệu ứng: đổi Prefab đạn (`overrideProjectilePrefab`), đổi hình ảnh, buff sát thương lớn |

### 6.1. Trọng số Gacha (Weighted Random) — *bổ sung*
Cần bảng trọng số cụ thể theo độ hiếm (vd: Common 60%, Uncommon 30%, Rare 8%, Evolution-eligible 2%) và cơ chế "pity" (đảm bảo không rơi vào tình trạng không có lựa chọn hợp lệ khi túi vũ khí đã đầy).

---

## 7. Thiết kế Map & Nhịp độ trận đấu (Level Design & Pacing) — *bổ sung*

### 7.1. Bản đồ (Maps)
- Số lượng bản đồ ban đầu: đề xuất 1 map cho bản MVP, mở rộng dần.
- Map dạng open-field cuộn tự do (infinite scroll) hoặc giới hạn biên (bounded arena) — cần quyết định do ảnh hưởng cách AI Reposition và spawn logic.
- Vật cản môi trường (obstacle) ảnh hưởng di chuyển nhưng không chặn đạn, giữ nhịp game nhanh.

### 7.2. Thời lượng trận (Run Duration)
- Mục tiêu: 1 run kéo dài 15–20 phút, kết thúc bằng Boss cuối hoặc Survival Timer.
- Chia thành các mốc thời gian (Time Milestone) tương ứng với đợt tăng độ khó và sự kiện đặc biệt (vd phút thứ 10: mưa Elite; phút thứ 15: Boss xuất hiện).

---

## 8. Giao diện & Trải nghiệm người dùng (UI/UX Flow) — *bổ sung*

### 8.1. Các màn hình chính
1. **Main Menu**: Chọn nhân vật, vào Shop Meta-upgrade, xem Leaderboard, Settings.
2. **HUD trong trận**: Thanh máu, thanh Exp/Level, Timer sống sót, số Zombie đã hạ, icon vũ khí/passive đang sở hữu.
3. **Màn hình Level Up**: Hiện 3 thẻ nâng cấp (Weighted Random), tạm dừng game, cho phép reroll (nếu có cơ chế đó).
4. **Màn hình Game Over / Kết quả**: Thống kê trận đấu (thời gian sống, sát thương gây ra, Zombie tiêu diệt), điểm số, Currency Meta nhận được, nút Chia sẻ/Chơi lại.
5. **Leaderboard**: Bảng xếp hạng toàn cầu/bạn bè, lọc theo ngày/tuần/mọi thời điểm.

### 8.2. Onboarding
- Tutorial ẩn (implicit) trong 60 giây đầu: hướng dẫn di chuyển, tự động chiến đấu, và một lần chọn nâng cấp đầu tiên có chú thích trực quan.

---

## 9. Tích hợp Nền tảng Web Game (Web Integration)

> [!IMPORTANT]
> Vì ProjectZombie là WebGL Game trong hệ sinh thái Jamstack, game phải tuân thủ nghiêm ngặt các cơ chế đồng bộ dữ liệu với Frontend (ReactJS).

### 9.1. Kiến trúc tổng thể
ReactJS (Frontend) — Vercel (Hosting) — Supabase (Database + Auth + Edge Functions).

### 9.2. Giao tiếp Web–Unity
Giao tiếp 2 chiều qua `SupabaseBridge.jslib`.
- Khởi tạo: Unity gọi `NotifyGameReady()` → Web gửi Save Data vào game.
- Lưu trữ: Unity gọi `SaveGameToWeb(payload)` mỗi khi kết thúc lượt chạy.

### 9.3. Bảo mật chống Cheat
- Unity sinh mã băm SHA-256 từ `save_data` kèm Secret Salt (biên dịch vào WebAssembly).
- Điểm số gửi về Edge Function (`submit-score`) được xác thực Hash trước khi cập nhật `game_progress`.
- **Lưu ý bổ sung**: Salt nhúng trong WASM là lớp chống cheat "obscurity" — có thể bị dịch ngược, chỉ nên coi là hàng rào chặn cheat phổ thông, không phải giải pháp tuyệt đối. Nên bổ sung song song:
  - Server-side sanity check: so sánh điểm số/thời gian sống với ngưỡng hợp lý (vd không thể đạt Level 50 trong 10 giây).
  - Rate-limiting trên Edge Function để chặn spam request.
  - `service_role` key của Supabase **chỉ tồn tại phía server (Edge Function)**, không bao giờ lộ ra client/Unity build.

### 9.4. Hiệu năng & Bộ nhớ (Performance & Memory) — *bổ sung*
- Object Pooling bắt buộc cho Enemy, Projectile, Exp Gem — tránh Instantiate/Destroy liên tục gây GC spike trên WebGL (nền tảng nhạy cảm với GC hơn native build).
- Giới hạn số lượng Active Entity đồng thời (Enemy Cap) để giữ FPS ổn định; dùng kỹ thuật despawn entity ở xa tầm nhìn camera.
- Texture Atlas cho sprite Zombie/vũ khí để giảm draw call.

### 9.5. Chiến lược tải & Cache (Loading & Caching) — *bổ sung*
- Build Unity WebGL nén Brotli, phục vụ qua CDN (Vercel Edge Network hoặc Supabase Storage + CDN).
- Cache-Control header dài hạn cho asset build (hash filename theo version) để tận dụng browser cache giữa các lần chơi.
- Loading Progress Bar đồng bộ với % tải thực tế của Unity Loader.

### 9.6. Xử lý lỗi (Error Handling) — *bổ sung*
- `SaveGameToWeb` cần cơ chế retry với exponential backoff khi Edge Function timeout hoặc trả lỗi 5xx.
- Validate response từ Edge Function trước khi Unity xác nhận "đã lưu thành công" với người chơi (tránh false-positive khi mạng chập chờn).
- Log lỗi phía client (Sentry hoặc tương tự) để theo dõi tỷ lệ thất bại đồng bộ.

### 9.7. Offline Sync (Chơi không cần mạng)
- Web tích hợp `localforage` (IndexedDB). Nếu mất mạng khi gửi điểm, hệ thống lưu hàng đợi Offline.
- Khi có mạng lại, hệ thống tự đẩy (flush) kết quả lên bảng xếp hạng Supabase.
- **Bổ sung**: Cần cơ chế xử lý xung đột (conflict resolution) nếu người chơi gửi nhiều kết quả offline liền nhau — xác định thứ tự bằng timestamp sinh tại thời điểm chơi, không phải thời điểm đồng bộ.

### 9.8. CI/CD — *bổ sung, cần hoàn thiện*
- Pipeline build Unity WebGL tự động (Unity Cloud Build hoặc GitHub Actions + Unity headless build).
- Versioning đồng bộ giữa build Unity và code React (đảm bảo `SupabaseBridge.jslib` tương thích 2 phía sau mỗi lần deploy).
- Môi trường Staging riêng biệt để test tích hợp Supabase trước khi lên Production.

---

## 10. Cân bằng số liệu (Game Balance) — *bổ sung, cần hoàn thiện ở giai đoạn Production*

> Đây là khung tham chiếu ban đầu; số liệu thật cần tinh chỉnh qua playtest.

| Thông số | Giá trị đề xuất ban đầu |
|---|---|
| HP khởi điểm người chơi | 100 |
| Damage cơ bản vũ khí khởi đầu | 10/đòn |
| EXP cần cho Level 2 | 10 |
| Tốc độ tăng EXP cần thiết mỗi cấp | +20% so với cấp trước |
| Enemy HP tăng theo thời gian | +5%/phút |
| Enemy Spawn Rate tăng theo thời gian | +8%/phút |
| Số Enemy tối đa on-screen | 200 |
| Mốc xuất hiện Elite | Phút 5, 10, 15 |
| Mốc xuất hiện Boss | Phút 10, 20 |

*(Ghi chú: bảng này cần được Game Designer và Balance Team review và điều chỉnh dựa trên dữ liệu playtest thực tế, không nên dùng trực tiếp vào production.)*

---

## 11. Định hướng Âm thanh & Mỹ thuật (Audio & Art Direction) — *bổ sung, cần hoàn thiện*

- **Art Style**: Cần xác định — Pixel Art, Flat Vector, hay Hand-painted 2D? (Hiện GDD chưa quyết định.)
- **Color Palette**: Đề xuất tông tối/u ám cho môi trường, tương phản màu sáng cho Exp Gem/vật phẩm để dễ nhận diện giữa đám đông Zombie.
- **Âm thanh**: Nhạc nền tăng tempo theo mốc thời gian (đồng bộ với độ khó); SFX rõ ràng cho Level Up, Evolution, và cảnh báo Boss sắp xuất hiện.

---

## 12. Kiếm tiền (Monetization) — *bổ sung, cần quyết định*

*(Hiện tại GDD gốc không đề cập — cần Product Owner xác nhận mô hình trước khi triển khai.)*
- Tùy chọn A: Free-to-play thuần, không IAP, dùng để thu hút traffic/leaderboard cho nền tảng.
- Tùy chọn B: Rewarded Ads (xem quảng cáo để nhận thêm Currency Meta hoặc reroll thẻ nâng cấp).
- Tùy chọn C: IAP nhẹ — mở khóa nhân vật/skin bằng tiền thật, không bán power trực tiếp (giữ công bằng leaderboard).

---

## 13. Chỉ số thành công (Success Metrics) — *bổ sung*

| Chỉ số | Mục tiêu tham khảo |
|---|---|
| Thời gian chơi trung bình/run | 10–15 phút |
| Tỷ lệ quay lại sau 1 ngày (D1 Retention) | ≥ 25% |
| Tỷ lệ quay lại sau 7 ngày (D7 Retention) | ≥ 8% |
| Thời gian tải trung bình (First Load) | ≤ 8 giây trên kết nối 4G |
| Tỷ lệ đồng bộ điểm thành công (Sync Success Rate) | ≥ 99% |

---

## Ghi chú phiên bản
Các mục đánh dấu *"bổ sung"* là phần được thêm vào so với bản GDD gốc để hoàn thiện theo chuẩn tài liệu thiết kế game chuyên nghiệp (đầy đủ Meta-progression, UI/UX, Balance, Art/Audio, Monetization, Success Metrics) và bổ sung các lỗ hổng vận hành ở phần tích hợp Web (bảo mật, error handling, memory, caching, CI/CD). Các số liệu cụ thể trong mục 10 và 13 là khung tham chiếu ban đầu, cần điều chỉnh qua playtest thực tế trước khi chốt.
