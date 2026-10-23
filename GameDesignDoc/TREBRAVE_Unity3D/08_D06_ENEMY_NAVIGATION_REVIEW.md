# D-06 — CHECKLIST REVIEW: Enemy AI, Navigation & Spawn Domain (Vongxuyen → Unity)

**Status:** `D-06 APPROVED — 8/8 lựa chọn A` · owner xác nhận 2026-10-23 · **PLANNING ONLY — KHÔNG CODE / KHÔNG CHỈNH SCENE / QA NOT_RUN**
**Liên quan:** G0 (duyệt hành vi), G2 (Enemy foundation), G3 (AI + spawn), G6 (GhostCannon/Boss). Chịu ràng buộc **D-05 APPROVED** (1 Miniboss tại 108s) và **D-11/A APPROVED** (1 Phi Cương thay một Ma Giáp **trong mỗi event** Ma Giáp).
**Biên bản duyệt:** Chủ dự án trả lời **“Chấp nhận, tiếp tục”** sau khi được đề nghị phương án A cho **cả 8 mục D-06-01..08**. Tất cả phương án A được đánh dấu `[x]` **chỉ là đã duyệt yêu cầu**; các ô test `[ ]` ở Phần 3 vẫn **NOT_RUN**.

## 1. Audit nguồn — đã xác minh trong code, KHÔNG đồng nghĩa Unity đã chuyển

| Loại (ID) | Điều có trong runtime Vongxuyen | Unity Defense3D hiện tại | Khoảng trống |
|---|---|---|---|
| **Ma Giáp Cương Thi `magiap`** | Ground/bouncing visual; East/South; có waypoint 3 cầu và lựa chọn đuổi Hero hoặc raid Nhà Chính | `DefenseEnemyBridgeWalker3D` NavMeshAgent theo waypoint cầu, HP/capsule chung | role data, aggro/target, bảo đảm route theo từng zone |
| **Thủy Quái `thuyquai`** | Sinh `canalWater` và được `MapBenQue.constrainPosition(pos, allowWater=true)` cho phép đi trong nước; chuyển động X–Z, body uốn lượn | Capsule sinh tại East, dùng Ground NavMesh như Ma Giáp | cần đặc tả **bơi trong rạch → lên bờ/đánh mục tiêu**; nguồn JS không có graph amphibious Unity sẵn |
| **Huyết Dơi `huyetdoi`** | Spawn `northCanopy`, thân/cánh vỗ bay; AI mục tiêu/di chuyển X–Z chia sẻ với quái khác | Capsule NavMeshAgent đi trên đất/cầu | cần quyết định **đường bay 3D thật** hay chỉ visual bay trên ground route |
| **Phi Cương `phicuong`** | Elite chuyển loại từ Ma Giáp trong `WaveManager`; visual bay; Trảo Quỷ telegraph 0.65s, cone 120°, r=2.5; movement X–Z chia sẻ | Capsule NavMeshAgent chung; chưa có elite skill/air policy | **D-11/A đã chốt số elite theo event**; chưa chốt locomotion/skill behavior |
| **Ma Giáp Thống Lĩnh `magiap_boss`** | Event thứ 7 @108, ground leader; slam telegraph/summon @50% HP | Miniboss capsule + HP thử nghiệm, walker | cần riêng combat role; giữ đúng D-05 |
| **Tiểu Quỷ Nón Lá Dù `quaidu`** | Không có trong 34 event Stage01; `GhostCannon` phóng projectile bờ Đông → dù xuống bờ Tây; lúc **tiếp đất** callback tạo Enemy thường | Spawn thẳng ở East qua vòng tên 5 quái; không có cannon/landing | cần pipeline `BALLISTIC → PARACHUTING → LANDED → ground AI` |
| **Linh Pháo `ghost_cannon`** | 3 pháo cố định bờ Đông (North/Mid/South), bắn `quaidu`; có HP, có thể bị phá | chưa có Defense3D tương đương được xác minh | cần quyết định có bắt buộc trong Core hay tách sang G6 |
| **Dạ Xoa `yaksha_boss`** | Boss module độc lập, đấu trường bờ Đông, skill riêng; Web `YakshaBoss.js` điều khiển chuyển động và kỹ năng | Prototype Arena Combat → Assault Base, dùng `DefenseEnemyBridgeWalker3D` khi rời arena | **không** coi Boss là quái ground phổ thông; chính sách Arena/Assault tách khỏi D-06 Enemy thường |

**Giới hạn hiểu đúng nguồn:** Web không sử dụng Unity NavMesh/Agent. `WaveManager.js` chủ yếu thay đổi `root.position.x/z`, `MapBenQue.js` ràng buộc rạch/3 cầu; Huyết Dơi/Phi Cương tạo cảm giác bay bằng `bodyPivot.y`, không xác nhận đã có AI bay 3D tránh chướng ngại vật. **Đề xuất bên dưới là thiết kế Unity mới**, không phải khẳng định nguồn Web đã có sẵn.

**Unity hiện tại:** `[RequireComponent(typeof(NavMeshAgent))]` ở `DefenseEnemyBridgeWalker3D`; `DefenseRun6003D` còn dựng capsule/spawn East để test. Không tháo `NavMeshAgent`, đổi `.meta` hoặc gỡ script đang gắn trên scene cho tới khi có adapter an toàn và backup.

## 2. Quyết định đã được duyệt — tất cả phương án A (D-06-01..08)

### D-06-01 — Mô hình locomotion

- [x] **A — APPROVED:** 4 domain **Ground / Water-Amphibious / Air / Landing**, cộng `Stationary` cho Linh Pháo và Boss policy riêng. Mỗi domain dùng navigation/physics phù hợp, giao tiếp chung với Target/Combat/State.
- [ ] **B:** Tất cả quái chạy Ground NavMesh và chỉ thay visual bay/bơi như prototype (rẻ hơn, nhưng không có cơ chế đặc thù).
- [ ] **C:** Khác (ghi rõ): __________

**Khuyến nghị A** để tránh việc quái nước và quái bay biến thành capsule chạy qua cầu. **Ảnh hưởng:** U3D-009/010/011/013/014.

### D-06-02 — Ground và 3 cầu

- [x] **A — APPROVED:** Ma Giáp, Miniboss và Tiểu Quỷ **sau khi tiếp đất** dùng NavMeshAgent Ground; quái từ East phải đi theo lane/bridge có `PathComplete`, không cắt xuyên nước, không teleport khi mất đường. Sinh đã ở West thì chỉ chọn tuyến West hợp lệ.
- [ ] **B:** Hỗn hợp waypoint/Transform, chấp nhận di chuyển qua nước/địa hình khi NavMesh bị đứt.
- [ ] **C:** Khác: __________

**Khuyến nghị A.** Thất bại path được ghi telemetry, xử lý stuck theo policy (retry/expire/despawn), không ép dời vị trí xuyên map.

### D-06-03 — Thủy Quái từ rạch nước

- [x] **A — APPROVED:** `thuyquai` **bơi trong canalWater**, có điểm **lên bờ hoặc cập bến** được cấu hình (Exit Anchors), sau đó mới đổi thành Ground NavMesh nếu cần đánh Hero/Nhà Chính; không sample Ground NavMesh giữa lòng rạch.
- [ ] **B:** Bơi tự do đến sát Nhà Chính, **không cần lên bờ**, dùng đường bơi/đổ bộ riêng; phải thiết kế target & hitbox dưới nước/cạnh bờ.
- [ ] **C:** Chỉ spawn vùng nước rồi đi theo cầu như ground (cần định nghĩa nơi chuyển từ nước lên cầu).
- [ ] **D:** Khác: __________

**Khuyến nghị A** cho Core: vừa thể hiện quái nước, vừa có đường đi/điểm tấn công có thể kiểm thử. **Cần inspect local map** để xác định có mép rạch/đường cập bến hợp lệ; chưa giả định NavMeshData hiện đáp ứng.

### D-06-04 — Huyết Dơi và Phi Cương có bay thật hay không?

- [x] **A — APPROVED:** **Air movement thực** theo X–Y–Z, có độ cao tối thiểu/tối đa, tránh va chạm, không cần đi cầu, có thể vượt rạch bằng không trung; `huyetdoi` bay nhanh, `phicuong` elite bay và có telegraph.
- [ ] **B:** **Visual-flight**: thân bay/dao động Y nhưng vị trí gameplay chạy tuyến X–Z và collision/route giống quái ground Web; dễ port hơn nhưng mất ý nghĩa tác chiến đường không.
- [ ] **C:** Khác: __________

**Khuyến nghị A**, nhưng chỉ khi kiểm thử được anti-air hitbox, va chạm cây/cầu, giới hạn trần/camera và target. Không cho `NavMeshAgent` tự kéo quái bay xuống đất.

### D-06-05 — Khả năng tấn công và bị tấn công với quái bay

- [x] **A — APPROVED:** Enemy bay có hitbox riêng; **Hero và Tower phải có ít nhất một nguồn sát thương hợp lệ đánh được quái bay** ngay trong Core; quyền anti-air của từng vũ khí/tháp cấu hình theo data + khoảng cách 3D/LOS, không miễn nhiễm vô tình.
- [ ] **B:** Mọi đòn đánh Hero/Tower trúng được quái bay chỉ tính khoảng cách mặt phẳng (đơn giản nhưng thiếu khác biệt chiến thuật).
- [ ] **C:** Chỉ anti-air weapon riêng, phải đưa ít nhất một anti-air tool vào scope Core để trận không bế tắc.
- [ ] **D:** Khác: __________

**Khuyến nghị A.** Chưa khóa chính xác class Tower/Range/DPS (phụ thuộc D-07).

### D-06-06 — Tiểu Quỷ Nón Lá Dù và Linh Pháo

- [x] **A — APPROVED:** **Giữ đủ cơ chế nguồn**: 3 Linh Pháo cố định bờ Đông (North/Mid/South); projectile `BALLISTIC → PARACHUTING → LANDED`; chỉ sau tiếp đất mới tạo `quaidu` Ground Enemy tại West; có thể phá pháo để ngừng bắn. Prototype model/capsule được phép nhưng hành vi thật phải có trước Core QA.
- [ ] **B:** Làm `quaidu` theo trạng thái Landing với trigger test/config trước; đưa 3 pháo và chu kỳ bắn sang G6 (cần ghi rõ Core chưa có nguồn xuất hiện `quaidu` hoàn chỉnh).
- [ ] **C:** Bỏ cơ chế pháo và spawn `quaidu` trực tiếp vào Wave/West (đây là thay đổi gameplay nguồn, cần duyệt riêng).
- [ ] **D:** Khác: __________

**Khuyến nghị A** về mục tiêu toàn Core; triển khai theo 2 bước nội bộ: landing contract ở G3, GhostCannon integration ở G6 trước full-run QA, không bắt buộc model pháo hoàn chỉnh. Điều kiện event `quaidu` **không cộng vào 34 lịch stage**. 

### D-06-07 — Target & aggro (Enemy đuổi Hero hay đánh Base?)

- [x] **A — APPROVED:** AI lựa chọn giữa **Hero, dân binh/hỗ trợ và Nhà Chính** theo data/aggro và tình huống, có lock/retarget hợp lệ; Boss/Pháo có luật riêng. Enemy **không mặc định chỉ đánh Base** như prototype Unity. Giữ 3 lane khi hành quân, tránh tấn công qua nước/trong lúc đang ở waypoint.
- [ ] **B:** Toàn bộ Enemy lao vào Base như prototype, Hero chỉ đánh chặn (ít AI nhưng lệch runtime Web).
- [ ] **C:** Khác: __________

**Khuyến nghị A.** Web hiện có `agroPlayer` dựa khoảng cách `4.2m` (hoặc sau trúng đòn và trong `7m`), `bossShouldRaid` và `supportCombat.findEnemyTarget`; dùng làm baseline **để test/tune**, không khóa cân bằng D-07. Yêu cầu chống đổi mục tiêu liên tục khi Hero lượn gần cầu.

### D-06-08 — Spawn zone, Elite D-11 và quota

- [x] **A — APPROVED:** `SpawnPlanner` dựa **eventId + declaredType + actualSpawnType + zone + locomotionDomain**, tôn trọng safe distance 3.1m, scene local anchors, 32 active/40 pending/4 frame/TTL25s (baseline). **Phi Cương dẫn đầu vẫn kế thừa vùng sự kiện Ma Giáp East/South**, nhưng phải qua validator dành cho Air; nếu vị trí không hợp lệ thì job chờ/hết hạn có log, **không tự đổi Phi Cương khác**.
- [ ] **B:** Sinh quái tại điểm East mặc định, không phân biệt zone/movement domain.
- [ ] **C:** Khác: __________

**Khuyến nghị A**, đồng nhất D-05/D-11 đã duyệt. **Chưa xác nhận:** quota 32 tính Boss và 3 pháo vào hay không; cần chốt trong G3 Spawn Director / D-07 balancing trước khi code.

## 3. Acceptance Checklist — yêu cầu được duyệt, tất cả ca **NOT_RUN**

Các ô là **yêu cầu dự kiến**, **không phải test đã chạy**.

### Spawn + đường đi
- [ ] **D06-QA-01:** Enemy ground ở East có đường `PathComplete` qua **cả 3 cầu** đến West/Base; không băng nước ngoài cầu.
- [ ] **D06-QA-02:** Enemy spawn sẵn West (ví dụ Tiểu Quỷ sau khi đáp) không chạy vòng sang East để tìm cầu.
- [ ] **D06-QA-03:** Khi NavMesh bị thiếu/ngắt cầu/agent stuck, có cảnh báo đúng event/lane/ID; không teleport qua vật cản, không softlock im lặng.
- [ ] **D06-QA-04:** `thuyquai` xuất hiện trong `canalWater`, bơi theo water path và chuyển hợp lệ lên bờ/khu giao chiến theo D-06-03 đã chốt.
- [ ] **D06-QA-05:** `huyetdoi` bay theo D-06-04; không chui qua cầu/địa hình/cây, không vọt cao khỏi camera.
- [ ] **D06-QA-06:** `phicuong` xuất hiện ở zone sự kiện Ma Giáp theo D-11/A, có air path/hitbox hợp lệ và telegraph claw đúng trước damage.
- [ ] **D06-QA-07:** `quaidu` chỉ bắt đầu AI ground sau Landing, không sinh thẳng ở East hoặc nhập sai 34 Stage01 events.
- [ ] **D06-QA-08:** Khi phá một GhostCannon, khẩu đó không bắn đợt mới; những projectile đã phóng và cách xử lý khi phá được test theo policy, 3 lane không lẫn callback.

### Combat / target
- [ ] **D06-QA-09:** Enemy chọn Hero/Support/Base hợp lệ; không đổi liên tục khi đến waypoint, không gây melee qua rạch.
- [ ] **D06-QA-10:** Hero/Tower có đường anti-air hợp lệ; sát thương 3D dựa range/LOS và loại vũ khí theo D-06-05.
- [ ] **D06-QA-11:** Enemy bay và bơi bị stun/slow/knockback có kết quả gameplay đúng; không rơi xuống dưới map hoặc xuyên Collider.
- [ ] **D06-QA-12:** Khi Hero down (D-04/A), Enemy không đánh một mục tiêu Hero vô hiệu; Base/Support vẫn bị tấn công, respawn mới nhận aggro lại.

### Schedule/Performance/Lifecycle
- [ ] **D06-QA-13:** Miniboss D-05 @108 chỉ 1 `magiap_boss`, path hợp lệ; không đi theo policy Air/Water.
- [ ] **D06-QA-14:** D-11/A 20 event Ma Giáp lên lịch 20 Phi Cương (1 suất/event), không phụ thuộc FPS; có test queue TTL/overflow và safe zone.
- [ ] **D06-QA-15:** Pause/LevelUp/Win/Loss dừng AI + damage + time + projectile updates theo state; resume không phát burst spam.
- [ ] **D06-QA-16:** Với đông quái/3 cannon/Boss, cap và queue giữ được, kiểm soát frame budget, không rò rỉ Event listener/Collider khi chết và Restart ×3.
- [ ] **D06-QA-17:** Boss Dạ Xoa ở arena bờ Đông @510, sau đó assault theo luật được chốt; không mất HP khi đổi state và không bị chung AI quái thường điều khiển.
- [ ] **D06-QA-18:** Hình học/collider và baked NavMesh gốc scene Map Sông Quê **không bị tạo lại/ghi đè**, Legacy Survival 2D không hồi quy.

### Bằng chứng QA bắt buộc khi triển khai
- [ ] Ghi **Git commit/Unity Editor version/scene path/local modified files**, Enemy ID/event/zone/movement domain, chụp Scene/Game và path debug.
- [ ] Có test tự động logic thuần (spawn ID, quota, target priority/state) **và** Unity PlayMode video cho ground 3 bridge, water, air, parachute.
- [ ] Các case trên mặc định **NOT_RUN**; không chuyển `ACCEPTED` từ GitHub code diff đơn thuần.
- [ ] Backup local scene/GLB/NavMesh/`.meta` đã kiểm tra theo [G0 Local Safety Runbook](07_G0_LOCAL_SAFETY_RUNBOOK.md) trước khi sửa prefab/scene.

## 4. Kiến trúc C# chỉ là gợi ý thiết kế, CHƯA CODE

```text
EnemyDataSO (Enemy ID, stats, domain, target policy)
          |
   EnemySpawnDirector
 (event ID / slot / zone / cap / queue)
          |
       EnemyAgent
  ├── GroundMover (NavMeshAgent / bridge route)
  ├── WaterMover  (canal + exit anchor)
  ├── AirMover    (3D flight / obstacle checks)
  └── LandingController (ballistic→chute→ground)
          |
  TargetSelector → Shared Combat/Health/Death → Wood/EXP
          |
  State/Clock authority (D-03/A, D-04/A)
```

`DefenseEnemyBridgeWalker3D` giữ nguyên MonoBehaviour/`.meta` và tham chiếu scene; về sau migrate bằng adapter, **không xóa ngay**. Boss và Linh Pháo có AI/phụ thuộc riêng, nhưng dùng chung Damage/Death/State contract.

## 5. Phạm vi đã chốt và chi tiết triển khai/QA còn mở

1. **ĐÃ DUYỆT D-06-01..08 phương án A.** Bao gồm: đa-domain Ground/Water/Air/Landing; NavMesh qua 3 cầu; Thủy Quái bơi rồi lên bờ tại exit hợp lệ; Huyết Dơi/Phi Cương **bay 3D thật**; hitbox/anti-air; **3 Linh Pháo thật về cơ chế** thả Tiểu Quỷ xuống West; aggro Hero/Support/Base; spawn zone/cap/D-11/A được kiểm soát. Code Unity vẫn chưa triển khai các lựa chọn này.
2. **Chưa chạy local QA:** chưa xác minh Unity scene NavMesh có đường water exit, air collision anchors, Cannon spawn/landing points hay cách xử lý baked artifacts.
3. **Ngoài D-06:** số liệu HP/speed/damage/cost/range thuộc **D-07 balance**; luật WIN/LOSS & Hero Down thuộc D-03/A+D-04/A đã duyệt; lịch @108 và elite per-event là D-05/D-11/A đã duyệt.
4. **Phần cần khảo sát khi implement:** layer matrix collision, Tower anti-air capabilities, projection/water surface geometry, line-of-sight và performance target trên thiết bị. Những giá trị chưa có trong source thì không tự suy từ map GLB.

## 6. Biên bản lựa chọn của chủ dự án (đã xác nhận)

```text
D-06-01 Locomotion domains: A — APPROVED
D-06-02 Ground + 3 bridges: A — APPROVED
D-06-03 Thủy Quái water→shore: A — APPROVED
D-06-04 Flight thật 3D: A — APPROVED
D-06-05 Anti-air & targetability: A — APPROVED
D-06-06 3 GhostCannon + QuaiDu Ballistic→Parachute→Ground: A — APPROVED
D-06-07 Enemy Hero/Support/Base target/aggro: A — APPROVED
D-06-08 Zone/Elite/quota: A — APPROVED
Ngày duyệt: 2026-10-23
Điều chỉnh riêng: không có trong lời duyệt.
D-06: APPROVED toàn bộ 8 nhóm; QA vẫn NOT_RUN; **Gate G0 đã OWNER APPROVED / PASSED WITH RECORDED LIMITATIONS ngày 2026-10-23**. Việc đóng G0 **không** cho phép tự code hoặc coi D06-QA đã nghiệm thu.
```

**Quyết định đã được chốt**; đồng bộ [Decision Log](03_DECISIONS_QA.md), [Migration Matrix](01_MIGRATION_MATRIX.md), backlog [U3D-009/010/011/013/014](02_IMPLEMENTATION_BACKLOG.md) và QA matrix. **D-06 APPROVED chỉ về đặc tả**, không đồng nghĩa đã có code, prefab/scene mới hay đã đóng Gate G0. Những chi tiết cần đo/khóa sau (vị trí water exit, height/clearance của Air, quota tính Cannon/Boss, projectile đang bay khi Cannon chết, DPS/range) chưa có bằng chứng Unity và không được tự khẳng định đã có.

## 7. Nguồn code audit (blob snapshot lúc review)

- [Vongxuyen WaveManager.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/combat/WaveManager.js) — `ec64826caf7de5f240a633de3f91d3b7ea46d4ab`
- [Vongxuyen EnemyFactory.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/entities/EnemyFactory.js) — `c4ac4a7111b672360740a1c4a14bf4e14fa04aa5`
- [Vongxuyen SpawnPlanner.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/combat/SpawnPlanner.js) — `feb1d52d0e764cc3ab15d958eefc46cdbe0b4a69`
- [Vongxuyen MapBenQue.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/world/MapBenQue.js) — Web movement/canal/bridge constraints
- [Vongxuyen GhostCannon.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/combat/GhostCannon.js) — `cc5f699102876dbe82cd01110b482376d0337b5b`
- [Projectzombie DefenseEnemyBridgeWalker3D.cs](https://github.com/thuongtran1210/Projectzombie/blob/feat/trebrave-defense3d-first-scene/Assets/Features/Defense3D/DefenseEnemyBridgeWalker3D.cs) — `83388f180158b7d8c89818cd96f1cf2ed270b085`
- [Projectzombie DefenseRun6003D.cs](https://github.com/thuongtran1210/Projectzombie/blob/feat/trebrave-defense3d-first-scene/Assets/Features/Defense3D/DefenseRun6003D.cs) — `cf38737d150d524770375abe76cfd7384b44cd76`
