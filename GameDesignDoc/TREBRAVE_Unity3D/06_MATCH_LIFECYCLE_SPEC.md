# Gate G0-B — Match Lifecycle & Hero Death Decision Specification (D-03 / D-04)

**Status:** **D-03/A & D-04/A OWNER APPROVED — 2026-10-23**; đặc tả triển khai G1/G6 vẫn DRAFT / PLANNING ONLY.
**Scope:** quyết định logic Game State / Match Clock / WIN-LOSS / OVERTIME / Hero Down. Người dùng đã **chọn cả hai phương án A**; chưa viết C#, chưa sửa scene hoặc nghiệm thu PlayMode.

## 1. Đối chiếu sự thật hiện có, tránh áp đặt thiết kế

| Nguồn | Hành vi thực tế theo mã/tài liệu | Trạng thái |
|---|---|---|
| Vongxuyen `docs/MASTER-GAME-DESIGN-AND-COMPLETION-PLAN.md` | gameplay loop trụ vững **600 gameplay seconds**; thua nếu Nhà Chính HP0 **hoặc Hero chết** theo bản master hiện có | tham chiếu thiết kế Web, không phải luật Unity đã chốt |
| Projectzombie `DefenseRun6003D.cs` | clock `elapsed += Time.deltaTime`, Boss @510; **WIN ngay** ở `OnYakshaDefeated` nếu Base còn sống; **LOSS** nếu BaseHP0; Boss sống sau 600 → `OVERTIME` | code prototype Unity nhánh feature; chưa có authoritative GameState |
| Projectzombie `DefenseHeroHealth3D.cs` | Hero 200 HP, down → disable `DefenseHeroMotor3D`, respawn sau `Time.time + 5s`; không phát LOSS khi Hero down | code prototype Unity; `Time.time` chưa được khóa cùng game-time |
| Projectzombie `DefenseBasePrototype3D.cs` | Base 1000 HP; `BaseDestroyed` là event LOSS; damage locked khi WIN | code prototype Unity; config chưa cân bằng theo Web |

**Lưu ý:** nguồn Web và code Unity **đang mâu thuẫn với hợp đồng mới đã duyệt**. Code hiện tại chưa được thay đổi; D-03/A và D-04/A là luật mục tiêu cho Unity, không phải tình trạng runtime.

## 2. D-03: Luật WIN / LOSS / OVERTIME — APPROVED/A

### Phương án A — **ĐÃ ĐƯỢC DUYỆT 2026-10-23**

**Mục tiêu thắng: bảo vệ Nhà Chính qua mốc 600s và đánh bại Dạ Xoa.**

- **LOSS:** Base HP giảm về 0 ở bất kỳ thời điểm nào trước khi kết quả được chốt.
- **WIN:** `BaseHP > 0 && gameplayElapsed >= 600 && YakshaDead`.
- Boss bị giết **trước mốc 600**: game vẫn tiếp tục (Enemy đang sống và những event chưa đến hạn vẫn hoạt động); khi đạt 600 và Base sống → WIN.
- Boss còn sống khi đạt 600: sang **OVERTIME**, không lên lịch thêm 34 timed-wave events sau khi wave cuối @594. Boss, quái đã có và combat chưa kết thúc tiếp tục. Nếu Boss chết và Base còn sống → WIN.
- Trong OVERTIME, BaseHP về 0 → LOSS.
- Nguyên tắc chốt kết quả trong cùng gameplay tick: resolve damage và BaseDestroyed trước WIN, **LOSS ưu tiên** nếu BaseHP=0. Sau trạng thái kết thúc, dừng mọi gameplay timer/AI/skill/attack/spawn/damage, chỉ UI có thể thao tác Result/Menu.
- Bản Core chưa tự áp giới hạn thời lượng OVERTIME; giới hạn, loot/summon và hành vi GhostCannon trong OVERTIME phải chốt riêng theo domain (chưa duyệt).

**Lợi ích:** bảo toàn mục tiêu 600s, không cho Fast Test @10s vô tình thắng trận từ sớm; Boss death là điều kiện bắt buộc.
**Chi phí:** một trận có thể dài hơn 600s, cần HUD "OVERTIME" và hướng dẫn Hero đối đầu Boss; nếu Boss không tiếp cận được có nguy cơ softlock, bắt buộc có QA.

### Phương án B — KHÔNG CHỌN (Unity prototype hiện tại)

- Boss chết với `BaseHP>0` → WIN ngay, dù game clock mới 510–599s hoặc Boss xuất hiện sớm trong Fast Test.
- Nếu Base chết trước → LOSS. Nếu đủ 600 nhưng Boss còn sống → OVERTIME tiếp tục theo Unity.
- **Lợi ích:** hành vi giống bản Unity thử nghiệm; có thể thắng sớm khi đánh tốt.
- **Chi phí:** không còn luật "phải bảo vệ qua đủ 600 giây" một cách chặt chẽ, người chơi có thể bỏ qua phần lịch wave còn lại.

### Ma trận acceptance so sánh

| Scenario / tick | A: 600 + Boss | B: Giết Boss là thắng |
|---|---|---|
| Base alive, Boss chết @540s | tiếp tục Playing tới 600s | WIN ngay @540s |
| Base alive, Boss chết @599.9s | Playing → WIN ở mốc 600s | WIN ngay @599.9s |
| Base alive, Boss còn sống @600s | OVERTIME | OVERTIME |
| Boss chết @630s trong OVERTIME, Base sống | WIN @630s | WIN @630s |
| BaseHP=0 @400s | LOSS | LOSS |
| Boss chết cùng tick BaseHP=0 | LOSS ưu tiên | LOSS ưu tiên (cần bổ sung Unity prototype) |
| Boss đã chết trong Fast Test 10s | trận vẫn tiếp tục đến 600s | WIN sớm |

**Quyết định chủ dự án:** D-03 = **A (APPROVED)**. Không tự động WIN chỉ vì đủ 600s; phải hạ Dạ Xoa và bảo vệ Base. Phương án B/C không được chọn. Chi tiết timeout OVERTIME và summon/GhostCannon chưa khóa bằng D-03, tiếp tục để backlog/risk.

## 3. D-04: Hero death/respawn — APPROVED/A

### Phương án A — **ĐÃ ĐƯỢC DUYỆT 2026-10-23**

- Hero HP về 0 → **Downed** (không đồng nghĩa LOSS); điều khiển di chuyển, tấn công, Pháp Bảo và xây tháp bị khóa riêng trên Hero.
- Hero down không pause cả trận, Enemy tiếp tục tấn công Base, player thấy đồng hồ hồi sinh.
- **Respawn sau 5 gameplay seconds** (chỉ tính khi Match state Playing hoặc Overtime; Pause/LevelUpSelection không giảm countdown), tại HeroSpawn an toàn. Lượng HP hồi theo config; **100% là baseline QA đề xuất, không phải cân bằng chính thức đã khóa (D-07 PENDING)**.
- Không nhận hit trong Downed. Khi Match WIN/LOSS → tắt lịch hồi sinh và mọi input gameplay.
- Nếu down đúng lúc WIN/LOSS: **terminal result ưu tiên**, không tái sinh sau kết quả.
- Mục tiêu: Hero có rủi ro chiến thuật (không bảo vệ Base được khi nằm xuống) nhưng không khiến run kết thúc sớm.

### Phương án B — KHÔNG CHỌN (Hero chết là LOSS)

- Hero HP=0 → trận LOSS, không hồi sinh.
- Base HP=0 vẫn LOSS.
- Hệ quả: khác nguyên mẫu Unity hiện tại; cần cân bằng lại Enemy/Arena Boss 510 và sửa Game Result, HUD, thống kê.

### Acceptance matrix

| Scenario | A: Down → Respawn | B: Hero down → LOSS |
|---|---|---|
| Hero HP0 @100s, Base còn | Downed, Base vẫn bị đánh, hồi sinh @105 gameplay s | LOSS ngay @100 |
| Hero HP0, Pause 20s wall-clock ở mốc 102 | còn 3 gameplay s sau Resume | đã LOSS |
| Hero HP0, LevelUp modal trước hồi sinh | countdown dừng | đã LOSS |
| Hero Downed, BaseHP0 | LOSS, không hồi sinh | LOSS |
| Hero Downed trong OVERTIME | hồi sinh sau 5 gameplay s | LOSS ngay |
| Hero Downed lúc WIN terminal | WIN result, không hồi sinh sau | cần quy tắc ưu tiên kết quả |

**Quyết định chủ dự án:** D-04 = **A (APPROVED)**; nhiều mạng/hồi sinh trả phí hoặc chết là LOSS không thuộc hợp đồng Core đã duyệt.

## 4. Match lifecycle contract — D-03/D-04 APPROVED; kiến trúc G1 vẫn DRAFT

Đề xuất lớp domain (chỉ đặc tả, chưa code): `MatchStateMachine` + `MatchClock` + `MatchResultResolver`, cùng interface dispatch events, không gộp vào MonoBehaviour `DefenseRun6003D`.

```text
Menu/Preparing
      |
      v
    Playing --------> Paused --------> Playing
      |
      +----> LevelUpSelection -------> Playing
      |
      +----> Overtime -----> Paused / LevelUpSelection / Overtime
      |
      +----> Win (terminal)
      |
      +----> Loss (terminal)
```

- **One source of truth**: `MatchClock.elapsedGameplaySeconds`, `MatchState`, `BossDead`, `BaseHP` và `MatchResult`. Existing `DefenseRun6003D` là legacy adapter sau khi code, không được sở hữu song song kết quả.
- **Clock policy**: khi Paused/LevelUpSelection/Win/Loss không tick game-time; khi Overtime tick elapsed/OT nhưng **không schedule thêm event Stage01**.
- **Clock boundary**: thu event tới hạn trước khi xét endgame; khi t=600 và Base chết trong cùng tick, LOSS ưu tiên.
- **Events one-shot**: `match:started`, `match:paused`, `hero:downed`, `hero:respawned`, `boss:defeated`, `base:destroyed`, `match:ended`, `match:restarted`. Event subscriptions bị hủy trên teardown.
- **Restart**: reset Hero HP, Base HP, Wood, XP/level, Tower slots, wave queue/caps, Boss phase, elapsed, pending respawn, UI modal; không nhân đôi cameras/Enemy controller/listeners.
- **Do not modify** original scene/local GUID/meta/GLB/NavMesh as part of planning.

## 5. Các ca test bắt buộc khi bắt đầu code (KHÔNG CHẠY NGAY)

| Case | Gate | Tiêu chí |
|---|---|---|
| ML-01 | G1 | Pause 30→20s wall-clock: clock/cooldown/spawn không tăng |
| ML-02 | G1 | LevelUpSelection dừng cả respawn countdown và AI tick |
| ML-03 | G1 | Restart x3: state, event subscriptions, timers sạch |
| ML-04 | G6 | Base HP về 0 và Boss chết cùng tick → LOSS theo terminal arbitration |
| ML-05 | G6 | **D-03/A:** Boss chết @540 trước 600 → tiếp tục bảo vệ cho tới mốc 600 (Base còn HP thì WIN) |
| ML-06 | G6 | Boss sống @600: OVERTIME, không schedule timed-wave sau 594 |
| ML-07 | G6 | Boss chết trong overtime, Base còn sống → WIN |
| ML-08 | G1/G6 | Hero down @100 & pause @102, countdown theo game time |
| ML-09 | G6 | Hero down, Base bị phá trong lúc chờ hồi sinh → LOSS |
| ML-10 | G1 | **D-03/A:** Fast Test Boss @10s không làm giảm yêu cầu 600 gameplay seconds để WIN |

**Trạng thái mọi ca:** NOT_RUN; chưa có Unity PlayMode evidence.

## 6. Việc làm tiếp theo và blockers (chưa cho phép code)

1. **Đã hoàn thành:** chủ dự án duyệt **D-03/A và D-04/A ngày 2026-10-23**; ghi vào [03_DECISIONS_QA.md](03_DECISIONS_QA.md).
2. **Đã hoàn thành về kế hoạch:** cập nhật acceptance QA-21/22/27/28 và backlog U3D-001/003/004/005/022 theo luật đã duyệt.
3. **Đang chờ:** D-01/D-02/D-06/D-07/D-08/D-09/D-10 và backup Scene/GLB/NavMesh/GUID; Gate G0 chưa đóng.
4. **Chỉ sau Gate G0** mới tạo code State/Clock G1. **Không tự tạo PR/commit gameplay hoặc sửa constants bây giờ.**

**Ghi chú:** D-05 (1 miniboss @108) và D-11/A (1 Phi Cương dẫn đầu event Ma Giáp) **đã được owner duyệt**, không mở lại những quyết định này.
