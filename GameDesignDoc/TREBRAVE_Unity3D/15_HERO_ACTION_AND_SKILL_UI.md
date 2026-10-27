# Defense3D — Player action và skill UI

Ngày: 27/10/2026. Trạng thái: source/prefab integration; native acceptance NOT_RUN. Core 3D ưu tiên Action + Tower Defense, hoãn toàn bộ Ngũ Hành. Legacy 2D giữ nguyên.

## Bộ Thư Sinh hiện tại

| Slot | Hành vi trong source | Thông số prototype |
|---|---|---|
| Attack — Đánh | Cone khóa hướng khi tung; tâm theo Hero lúc hit, XYZ range và LOS. Tap tự nhắm gần nhất; không có mục tiêu vẫn có thể đánh hụt. | 2.05m, 210°,35 damage,0.65s CD,0.18s windup |
| Dash | Điểm đến qua traversal đã có; ngắt Attack/Signature đang chờ hit. | Giữ `Skill_Dash.asset`:4.5m,0.25s,3s CD |
| Signature — Phán Quyết Tiền Định | Self-AoE theo vị trí Hero lúc resolve,360°;1.5× damage Attack hiện tại; giảm20% CD còn lại của Dép sau resolve hợp lệ, kể cả không có quái. | radius6m,52.5 damage cơ sở,0.38s windup,30s CD |
| Relic — Dép Tổ Ong | Giữ effect điểm đến/AoE/status đã có; không port projectile quay về hoặc cơ chế hệ. | Giữ definition/effect/profile hiện có |

Attack-speed upgrade thay CD đánh thường; Signature lấy tỷ lệ52.5/35 từ hai definition nên damage nâng cấp cũng nhân1.5. Các thông số trên là baseline native prototype có lựa chọn riêng, không phải toàn bộ balance của Vongxuyen. Chưa có chuỗi3-hit/animation/passive riêng.

## Điều khiển và chỉ dấu

Bốn nút author sẵn ở góc phải: Attack lớn phía dưới phải; Dash dưới trái; Signature trên trái; Dép trên phải. Mỗi nút có icon, cooldown fill/text, khóa, thumb pad và vùng hủy riêng. Các vùng hủy nằm cùng vị trí nhưng chỉ có một owner ngắm trên Hero; nút thứ hai bị chặn khi nút thứ nhất đang ngắm. Không tạo UI hoặc indicator bằng code trong runtime.

- Tap Attack: lấy mục tiêu gần nhất trong range hoặc hướng nhìn hiện tại, commit một đòn. Giữ/kéo: hiện vòng tầm và cone, thả để đánh; kéo vào vùng hủy rồi thả thì không tiêu CD. Giữ cảm ứng **chưa đánh liên tục**: thao tác hiện tại là aim/release.
- Signature: luôn hiện vòng quanh chân, kéo không dời tâm; vẫn có vùng hủy. Projection tìm đất dưới Hero nên không phụ thuộc điểm ở trước mặt khi đứng gần mép bản đồ.
- Dash và Dép giữ tap/drag/cancel hiện có. Bộ này chưa port mọi đường cong/boomerang của bản web.
- Indicator dưới chân thường trực vẫn dùng `DefenseHeroFootIndicator3D` đã author trước đó; preview/impact là các instance riêng, không thay hoặc tạo thêm một HeroFoot mới.
- Keyboard: giữ J để lặp Attack theo CD; K Signature, Space Dash, Q Dép. Input keyboard dùng cùng cast validation như UI, bị chặn khi game không active, Hero down, đang thao tác aim khác hoặc đang nhập TMP input. Chưa có binding combat gamepad.

CD commit trước effect callback để chặn input lặp. Cancel trước release không tiêu CD; đã tung rồi bị Dash/pause/down ngắt thì không refund. Pause/modal đóng băng gameplay clock; down chặn cast nhưng CD vẫn trôi khi trận tiếp tục. Reset dọn pending/cooldown; hit/release của generation cũ bị từ chối. Query area buffer bão hòa thì fail closed, không cố gây một phần damage hoặc refund CD.

## Authoring

Các file nằm tại `Assets/Resources/Defense3D/`:

- `HeroKit_ThuSinh.asset`: nối đúng Attack/Dash/Signature/Relic definitions. Chưa có selector thay nhân vật.
- `Skill_BasicAttack.asset`, `Skill_ScholarSignature.asset`: range/radius/damage/CD/duration; attack duration là hit delay, Signature duration là windup. Thay asset rồi khởi tạo phiên mới để capture lại.
- `DefenseSkillControlsCanvas.prefab`: layout/icon/cancel/pad; không chạy lại generator cũ để ghi đè. Kích thước tham chiếu1920×1080; chưa nghiệm thu safe area/touch ở mọi tỷ lệ màn hình.
- `DefenseHeroAttackIndicator3D.prefab`: kế thừa cấu trúc authored của generic Aim prefab, thêm Quad AttackSector và material `HeroAttack_Sector.mat`, shape5. Shader dùng angle từ snapshot qua MaterialPropertyBlock, không clone material.

Scene `Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity`: Hero có AttackEffect/ScholarSignatureEffect; composition có20 participants và4 presenters; Controls instance override projector/health/origin/effect/indicator. Signature `_relic` nhận component cung cấp `IDefenseAbilitySource`, hiện là presenter Dép; giảm CD trực tiếp trên Ability owner. Manual và optional auto dùng cùng Ability; scene hiện `_autoAttackEnabled=0`.

Đổi nhân vật trong tương lai cần kit/definition/effect tương ứng và bindings hợp lệ. Không coi HeroKit hiện tại là hệ roster động hoàn chỉnh. Giữ root indicator scale identity; shader/raycast không thay thế kiểm chứng mặt đất thật, cao độ mục tiêu hoặc LOS.

## Nguồn tham chiếu và giới hạn

Đã đọc repo `thuongtran1210/Vongxuyen` ở commit `5f5d9b26d5827e55dc8ac0a0650f135e3a7cf0cd`, nhất là `src/main.js`, `src/combat/HeroRelicSystem.js`, `src/combat/CombatResolver.js`, `src/progression/RunStats.js`. Đối chiếu shared basic attack và Phán Quyết; không dùng bản FUN proposal làm implementation mặc định. Native giữ damage35 và chọn CD Signature30s/windup0.38s; web có công thức stat giảm CD riêng, chưa port.

422 pure checks PASS và source compile PASS không chứng minh render/input/physics Unity. Chưa chạy Unity theo chỉ đạo. Còn animation/combo/buffer, VFX/audio/hit feedback, touch movement joystick, gamepad combat, full passive/roster và native/device acceptance. Các phạm vi này chưa được đánh dấu hoàn thành.


## Cập nhật chỉ dấu và cảnh báo spawn — 27/10/2026

Player: cone Attack có viền riêng và fill xanh mờ; range ring đặt trên đất dưới Hero thay vì lấy cao độ đích. Signature chỉ giữ một vòng cùng tâm. HeroFoot giữ kích thước/màu Vongxuyen, fallback đáy Collider nếu raycast không chạm đất.

Spawn warning: `DefenseSpawnWarningsCanvas.prefab` có32 slot vector sẵn trong hierarchy, sort85, không bắt pointer. Vòng vàng đếm ngược5 giây trước mốc Stage, biểu tượng quái, halo nhẹ và arrow khi ngoài màn hình; margin38px, tự tách badge gần nhau và dùng camera pixelRect giao safe area. Run tham chiếu scene instance, composition bind21 participants; UI nhận compiled plan hiện có, không tạo lịch hoặc sửa spawn.

Cảnh báo là lịch dự kiến: spawn t=0 không có pre-warning; queue/NavMesh/Hero exclusion có thể trì hoãn actor sau lúc warning hết. Vị trí từ authored zone có thể khác vị trí đã snap NavMesh. Chưa có warning cho Landing từ cannon, fade-out200ms hoặc exact emoji/browser shadow. Không delay actor để giả lập5s emergence của web. Nếu màn hình quá nhỏ hoặc hết chỗ sẽ ẩn bớt badge, gameplay vẫn giữ đủ spawn. Các icon là vector trong Graphic, có thể chỉnh màu/margin/lead ở prefab; geometry icon hiện chỉnh trong `DefenseSpawnWarningGraphic`.

455 pure checks và compile/authoring PASS; Unity rendering/input/physics/device acceptance NOT_RUN. Chưa chạy Unity theo chỉ đạo người dùng.
