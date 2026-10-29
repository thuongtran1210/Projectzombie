# Defense3D — Nhật ký refactor

## Chỉ dấu Player và cảnh báo spawn kiểu Vongxuyen — 27/10/2026

Đã triển khai cả hai loại theo xác nhận người dùng: chỉ dấu dưới chân/vùng ngắm Player và biểu tượng cảnh báo spawn trong/ngoài màn hình. Source/prefab integration đã kiểm chứng ngoài Unity; **native visual/input acceptance NOT_RUN**.

- HeroFoot giữ nguyên geometry/material tham chiếu Vongxuyen: ring0.38–0.44m, fill0.36m, mũi tên tới0.52m. Tia dò đất tăng khoảng dò8m; nếu không chạm thì dùng đáy Hero Collider làm fallback để không mất chỉ dấu chỉ vì raycast miss. Gate/down/disable vẫn ẩn.
- Attack thêm authored `AttackSectorOutline`: viền xanh nhạt và sector fill xanh mờ theo radius/góc snapshot, không tạo object/material mới. Vòng range dò mặt đất dưới Hero độc lập cao độ điểm đích. Signature không vẽ thêm target ring chồng lên range circle cùng tâm. Giữ vùng AoE Dép phù hợp native hit hiện có; chưa port boomerang.
- `DefenseSpawnWarningsCanvas.prefab` có32 slot vector Graphic author sẵn, sort85 dưới Controls90, không GraphicRaycaster/raycast target. Vòng vàng đếm ngược, nền tối/halo nhịp0.9s, biểu tượng vector theo loại quái và mũi tên khi ngoài màn hình. Không dựa font emoji, không Instantiate/New GameObject hoặc tạo Material/Mesh trực tiếp; hình vector được vẽ qua UGUI VertexHelper trong Graphic có sẵn.
- Run truyền **cùng compiled Stage plan** vào warning presenter đã được composition bind. View chỉ đọc lịch/zone, không enqueue/consume job, không delay actor, không đổi Stage/spawn queue/NavMesh. Cảnh báo trong5 giây **trước mốc spawn đã cấu hình**; wave t=0 không có pre-warning. Queue/placement có thể khiến quái ra trễ hơn mốc; warning không hứa vị trí NavMesh cuối cùng hoặc báo tiếp đến lúc actor thật xuất hiện. Dynamic cannon Landing chưa nối warning trong gói này.
- WorldToViewportPoint → projection thuần C# → camera.pixelRect giao Screen.safeArea; margin38px bảo vệ cả arrow tip. Badge cố định theo pixel thực, tách vị trí trùng theo mép và không vượt safe rectangle. Nếu thiếu chỗ/capacity thì bỏ bớt badge hiển thị, không bỏ spawn. Production peak8 badge trong cửa sổ5s, pool32. Timers/halo theo gameplay clock; pause/modal/terminal/cleanup/disable ẩn, restart tính lại từ clock mới.

**455 pure checks PASS** =422 cũ +33 projection/behind-camera/edge/safe-offset/overlap/countdown/pause/restart cases. Runtime và runtime/Editor/fixture source compile PASS bằng Mono/csc độc lập. `VerifyIndicatorWiring.py` PASS: Unique YAML/IDs/ref graph,32 slots/21 participants/4 Hero presenters, sector outline, camera binding và **120 resources/shader được bảo vệ giữ hash**. Chỉ sửa3 scene blocks cũ, thêm3 blocks prefab instance; Controls, HeroFoot prefab/material, generic Aim prefab, shader, Stage, enemy/tower assets giữ nguyên. Snapshot `Logs/DefenseIndicators_20261027_111506/`; evidence `Logs/TraversalQA/defense-indicators-*`.

Nguồn đối chiếu read-only: Vongxuyen commit `5f5d9b26d5827e55dc8ac0a0650f135e3a7cf0cd`, `SkillIndicators.js`, `SpawnIndicatorUI.js`, `spawn-indicator.css`, `WaveManager.js`. Bản web có5s emergence sau actor acquisition; native dùng pre-warning để giữ nguyên lịch/combat đã chốt. Biểu tượng vector/phát sáng là cách thể hiện tương ứng, không xác nhận pixel-identical emoji/glow của browser.

**NOT_RUN:** Unity import/serialization/UGUI mesh/material/shader render, actual ground probe, camera/device orientation/notch, visibility/layering/overlap với HUD, multi-touch/cancel và toàn trận. Không mở Unity. Source/logic PASS không chứng minh các chỉ dấu đã xuất hiện đúng trên thiết bị.

---

## Player Action — Thư Sinh, gói 1 — 27/10/2026

Đã triển khai source, prefab và scene cho **Attack / Dash / Phán Quyết / Dép**, ưu tiên Action + TD; không port Ngũ Hành và không đổi Legacy 2D. Đây là integration có kiểm chứng ngoài Unity, **chưa nghiệm thu gameplay/visual trên Unity hoặc thiết bị**.

- `HeroKit_ThuSinh.asset` là bộ bốn definition; composition kiểm tra đủ bốn slot, đúng definition/effect và participant trong cùng scene. Chưa có chọn/chuyển nhân vật trong trận hoặc full roster.
- Scene chuyển sang **đánh chủ động** (`_autoAttackEnabled=0`). Owner cũ giữ tùy chọn auto để tương thích cấu hình khác, nhưng manual và auto chia sẻ đúng một Ability/CD; không có hai vòng gây damage. Đánh thường: radius2.05m, cone210°, damage35 trước nâng cấp, CD0.65s, hit sau0.18s. Cone khóa hướng lúc tung, tâm hit theo Hero lúc resolve; lọc XYZ range + LOS, target còn sống và scope hiện tại.
- Phán Quyết là self-AoE radius6m, windup0.38s, CD30s; damage bằng1.5× damage đánh thường hiện tại (prototype52.5), rồi giảm **20% thời gian hồi còn lại** của Dép. Không có chọn hệ, cộng hưởng hoặc buff hệ. Có vòng windup theo chân; tự định vị dưới Hero, không yêu cầu điểm đất phía trước khi đứng gần biên.
- Dash ngắt pending Attack/Signature; Signature ngắt pending Attack. Pause/modal/down/disable/reset/terminal hủy pending hit và chỉ dấu, không hoàn CD đã commit. Cooldown chạy theo gameplay clock; down chặn cast nhưng không đóng băng riêng CD. Không có hit cũ sang lượt mới.
- Bốn nút + thumb pads + vùng hủy, indicator preview/impact và icon 2D tái sử dụng đều author sẵn. Không tạo GameObject/material/render mesh ở runtime. Aim ownership ở Hero ngăn hai ngón điều khiển hai hướng cùng lúc. Giữ HeroFoot indicator cũ; chưa xác nhận nó nhìn thấy trong Unity.
- Tap nhắm gần nhất cho Attack; giữ/kéo hiển thị cone, thả để đánh, kéo vào vùng hủy để bỏ. Giữ nút cảm ứng hiện là **ngắm rồi thả**, chưa phải auto-repeat/chuỗi combo như mọi hành vi trong Vongxuyen. Phán Quyết luôn lấy tâm Hero. Keyboard: giữ J đánh theo CD; K Phán Quyết, Space Dash, Q Dép; mọi phím đi qua cùng presenter/cast gate. Chưa có gamepad combat adapter hoặc touch movement joystick trong gói này.
- Signature truy cập cooldown qua `IDefenseAbilitySource`, không gọi logic UI để chỉnh timer; presenter là adapter cung cấp owner. Preview, cast và cooldown dùng chung definition; attack-speed modifier được phản ánh ở CD dùng cho cast và vòng phủ UI.

**Kiểm chứng:** 422 pure C# checks PASS (393 cũ +29 mới). Runtime và runtime/Editor/fixture **source compile** PASS bằng Mono/csc độc lập, không mở Unity. `VerifyScholarCombatWiring.py` PASS: Unique YAML/fileIDs, đúng4 controls/20 participants, references/cancel/pads/sector, scene chỉ đổi5 block cũ và thêm16 block; **109 resource cũ giữ nguyên hash**, ngoại trừ Controls prefab là thay đổi chủ đích. Generic Aim prefab, shader, Dash/Dép definitions, enemy/tower/stage/NavMesh giữ nguyên. Snapshot `Logs/DefenseScholarCombat_20261027_105249/`; logs `Logs/TraversalQA/defense-scholar-*`.

**NOT_RUN:** Unity import/serialization/Awake order, sprite/font rendering, raycast/touch đa ngón, projection/range/LOS/hit thật, native pause/down/restart callback ordering, độ rõ và cảm giác input trên thiết bị. Chưa có animation/combo3-hit, VFX/audio riêng, hit-stop/haptics, toàn bộ passive/weapon kit hay cân bằng thực chiến; không tuyên bố giống hoàn toàn Vongxuyen hoặc đóng P4 acceptance. Native physics/LOS có thể làm hit thực khác vòng XZ nếu mục tiêu ở cao độ khác; cần nghiệm thu trước khi coi chỉ dấu khớp chính xác.

Mô tả và hướng chỉnh asset: `GameDesignDoc/TREBRAVE_Unity3D/15_HERO_ACTION_AND_SKILL_UI.md`. Bước tiếp theo: hoàn thiện animation/hit feedback và combo/buffer cho Thư Sinh, rồi mới mở thêm kit; P5 reward/EXP vẫn chưa triển khai trong gói này.

---

## Chốt phạm vi: Action + TD, hoãn Ngũ Hành 3D — 27/10/2026

Người dùng xác nhận chưa cần áp dụng Ngũ Hành vào Core 3D vì chưa thấy hiệu quả chiến lược. Đã cập nhật kế hoạch: không port tương sinh/tương khắc, combo/chọn hệ hoặc buff/CD do phản ứng hệ; ưu tiên bộ điều khiển/đánh thường/Signature/Relic, chỉ dấu/cancel, né/ngắt chiêu và quyết định lane/tháp/tài nguyên. Màu nhận diện nhân vật có thể giữ ở cấp visual. Effect buff/giảm cooldown trực tiếp vẫn phải được mô tả riêng, không đồng nhất với phản ứng hệ. Legacy 2D giữ nguyên. Đợt này chỉ sửa tài liệu, không thay code/asset hoặc chạy test.

---

## P4 — Hợp nhất Boss FSM và encounter (RF3D-023) — 27/10/2026

Đã nối FSM orphan vào production path và bỏ các phase/timer song song. Đây là source integration/static verification, **chưa đóng P4 runtime acceptance**.

### Owner hiện tại

- `DefenseYakshaBehaviorFSM` là owner duy nhất của Dormant/ArenaCombat/AssaultBase/Defeated, lịch warning/detonation/recovery và historical HasSpawned/HasEnteredAssault. FSM production nhận đúng `DefenseEnemy3D.HealthAuthority`; không tạo HP thứ hai. Constructor mặc định3200HP/60-damage manual Slam chỉ còn là API standalone tương thích fixture cũ, không dùng trong production.
- `DefenseYakshaEncounter3D` là session/actor adapter: inject shared health + captured arena cycle, đổi gameplay clock delta sang FSM.Tick, thực thi assault từ PhaseChanged và stop từ CombatStopped. HasSpawned/AssaultStarted/IsDefeated chỉ đọc FSM với generation guard, không còn private phase booleans hoặc spawnedAt timer.
- `DefenseYakshaArenaCombat3D` chỉ là executor/view: không Update/LateUpdate, activeArena/warning booleans hoặc nextBlastAt/detonateAt. Nghe intent từ FSM để hiện authored LineRenderer và kiểm tra hit lên Hero. Snapshot blast origin dùng chung cho ring/hit; radius XZ5m hiện có giữ nguyên, không thêm LOS/3D hoặc Support blast ngoài baseline.
- BridgeWalker bỏ arenaMode phase flag; PrepareArenaBoss thiết lập HP **một lần lúc spawn**, BeginBossAssault chỉ Configure đường đến Base, không Initialize/SetMaxHealth. StopBossCombat dừng cả path và basic attack, không reset HP.
- Session/MatchRuntime vẫn là result owner; FSM chỉ báo defeat. MatchRuntime/result evaluator production không được thay bằng orphan evaluator khác.

### Bảo toàn hành vi production

Stage01_Production thực tế đặt **Boss HP1200, arena45s**; prefab đặt radius5, interval3s, warning1.1s, damage30. Các asset này giữ nguyên byte. FSM được cấp cycle3/1.1/30 để không vô tình lấy orphan constants1.2/60. Boss schedule510s, dự kiến Assault555s nếu phiên spawn đúng lịch; chậm placement thì arena tính từ lúc actor thật xuất hiện như trước.

Warning mới luôn có đầy đủ thời gian báo trước; frame trễ không replay các blast bị bỏ qua. Recovery sau hit vẫn max0.1/interval−warning như prototype. Arena cutoff ưu tiên hơn blast cùng tick; pending warning bị hủy trước Assault. Tính arena elapsed bằng double với epsilon float nhỏ để giữ hợp đồng fixture1.2+43.8=45.

Pause/modal đóng gameplay gate, freeze FSM timers và ẩn ring; resume khôi phục pending warning với thời gian còn lại. Terminal/cleanup/reset dừng FSM, ring, movement và attack. Không refill HP Arena→Assault. Defeat do shared health ghi nhận một lần, kể cả actor đang bị dừng combat; external disable/remove không tự biến thành kill. External lifetime mất thì dispose old health listener, cảnh báo một lần và không cấp victory; lỗi NavMesh Assault dừng combat, giữ HP/target để còn bị đánh bại.

Native bindings có generation/exact lease và captured-FSM observer guards để callback đang nằm trong invocation list cũ không chạy vào lượt/core mới. Cleanup/Dispose gỡ đúng delegates/Health.Defeated; xử lý health chết trước callback hoàn tất mà không coi natural factory release là external removal.

### Kiểm chứng

**393 pure C# checks PASS** =355 trước +38 Boss/shared-health/session-result cases. Kiểm tra từ Stage/prefab thật, shared-reference HP, native cycle vs legacy defaults, warning/recovery/pause/no duplicate, HP qua Assault, cutoff priority, long frame/catch-up, death in warning, no fake kill on stop, disposal/throwing observer, old health listener, early kill,600s/Overtime và cùng-tick Base loss priority.

Runtime và runtime/Editor/fixture source compile PASS. `VerifyBossFSMWiring.py` PASS: production shared health constructor, không autonomous arena timers/duplicate encounter state/bridge arenaMode, phase/stop/lease wiring, health unsubscription; **111 protected resources + scene entries trong manifest giữ nguyên hash** (tổng111 file gồm scene). Không thay prefab, vật liệu, shader, Stage, stats, UI hoặc NavMesh assets. Logs `Logs/TraversalQA/defense-boss-fsm-*.txt`; snapshot `Logs/DefenseBossFSM_20261027_095446/`.

**NOT_RUN:** Unity Awake/OnEnable/OnDisable/OnDestroy và Health.Defeated callback ordering, warning rendering, actual Hero blast hit, assault NavMesh/agent, actual same-frame terminal order, full run/device. `ArenaCombat.Begin(hero)` legacy giờ yêu cầu FSM đã được bind, không tự chạy arena nữa; fixture Unity lịch sử gọi Begin không context/FSM phải migrate qua encounter nếu sau này dùng lại. Source compile không chứng minh fixture runtime còn hợp lệ. Chưa thêm full Boss cleave/crescent/summon, native status immunity/counterplay nâng cao hoặc Support units; enum không đồng nghĩa mechanic đã có.

**Tiếp theo theo kế hoạch:** P5, bắt đầu RF3D-024 — death reward → physical Wood/EXP pickup → collector → wallet/progression, sau đó RF3D-025..028 và pooling. P4 vẫn cần native acceptance và các khoảng trống được ghi nhận, không gắn nhãn hoàn tất chỉ vì compiler/pure tests PASS.

---

## P4 — Trảo Quỷ Phi Cương: telegraph → cone damage (RF3D-021, gói 7) — 27/10/2026

Đã triển khai source/prefab cho Trảo Quỷ theo baseline D-06: **0.65s windup, cone120°, radius2.5m**. P4 chưa đóng acceptance; Boss FSM integration còn lại.

- `DefenseConeTelegraphRuntime` thuần C# giữ một pending snapshot origin/facing/radius/angle/scope. Chỉ sau mốc windup mới resolve một lần; clear pending trước delivery, long frame không lặp damage. Cone dùng XYZ range và góc XZ, không gây hit chỉ vì khoảng cách chiếu đất. Hướng được khóa tại thời điểm bắt đầu, không xoay theo Hero sau khi đã báo đòn.
- `DefenseEnemyAttack3D` vẫn là **owner cooldown duy nhất**. Trên PhiCương có optional ConeAttack strategy thay nhánh damage tức thời; không có thêm basic hit sau cone. Damage18/interval2s vẫn lấy immutable enemy stats hiện có, không thêm damage/cooldown vào profile. Cooldown được commit lúc bắt đầu báo đòn, không refund khi hủy.
- `DefenseConeAttack3D` nhận friendly roster/Base qua Air composition. Snapshot geometry giữ nguyên trong windup, PhiCương dừng bay và retarget. Khi resolve, kiểm tra lại từng Hero/Support còn hợp lệ và Base đúng một lần, lấy collider ClosestPoint nếu collider còn enabled (khác thì dùng root), kiểm tra cone + range XYZ + LOS mask4105 rồi mới scoped damage. Dừng delivery khi gameplay gate/generation/lifetime đổi hoặc bị khống chế trong damage callback. Không dùng enemy target registry để đánh nhầm đồng đội.
- Stun/Knockback hủy pending ngay trong status application, không phải đợi hết windup; Slow không hủy đòn. Pause/modal/cleanup/disable/dead/target mất hiệu lực hủy cone và ẩn chỉ dấu. Resume cần windup mới; thời gian cooldown đã commit vẫn giữ. Target di chuyển ra ngoài cone không làm hướng đòn bám theo: có thể né được, hit chỉ quyết định lúc resolve.
- PhiCương prefab có sẵn `ClawTelegraph_Sector120` inactive với Quad/MeshRenderer, ConeAttack/View components và profile refs. Renderer được author enabled, visibility qua child GameObject. Không tạo GameObject/Mesh/Material trong runtime. View dùng MaterialPropertyBlock cache ở Awake/first Show, không tạo native object trong field initializer.
- View chiếu origin xuống Ground/Water collider, dùng cùng radius/angle snapshot với damage. Parent PhiCương XZ scale1.2 đồng nhất, view bù scale ra đường kính5m. Biến shape mới Sector5 được thêm vào `DefenseAimOverlay.shader` bằng `_SectorCos`, giữ **SRPDefaultUnlit**, ZTest Always/ZWrite Off và toàn bộ shader paths cũ cho UI/foot/range/arrow. Màu cảnh báo đỏ cam prototype, chưa có slash model/animation hoặc âm thanh.
- `PhiCuong_Claw.asset` capture windup/radius/angle/LOS lúc Bind; damage/cooldown ở enemy definition. `PhiCuong_Telegraph.mat` là material author mới. Chỉ prefab PhiCương được thay đổi; scene giữ nguyên byte, không thay Stage/elite quota/stats/Boss/Cannon/UI assets.

**Kiểm chứng ngoài Unity:** runtime và runtime/Editor/fixture source compile PASS. **355 pure C# checks PASS** =325 trước +30 cone timeline/geometry cases: không instant hit, exact0.65s, once-only, khóa facing, angle/radius boundaries, dodge ra sau/ra ngoài, XYZ height, pause/modal/reset/terminal cancellation, interrupted pending và long frame. Các case này không chạy adapter cooldown, collider ClosestPoint hoặc Physics LOS thật.

`VerifyPhiCuongClaw.py` PASS: unique YAML keys/fileIDs, +6 prefab blocks đúng refs, child inactive/renderer enabled, mesh/material/profile chuẩn120°/2.5/0.65, existing shader paths khôi phục bằng loại bỏ phần Sector mới phải bằng bản trước, single cooldown wiring/CC cancel/Air freeze/lease guard, không runtime GameObject/Mesh/Material, **105 protected resource hashes và scene giữ nguyên**. Evidence `Logs/TraversalQA/defense-claw-*.txt`; snapshot `Logs/DefensePhiCuongClaw_20261027_093243/`.

**NOT_RUN:** Unity import/serialization/event order, shader compilation/render, actual telegraph visibility, collider/LOS/cone damage, status interruption và cooldown trong trận, balance/camera/device. Không khẳng định native gameplay acceptance từ các kiểm tra ngoài Unity. Tiếp theo RF3D-023: hợp nhất Boss FSM/encounter, HP Arena→Assault và result lifecycle; Support units/QA vẫn chưa đầy đủ.

---

## P4 — Slow/Stun/Knockback đa địa hình (RF3D-021, gói 6) — 27/10/2026

Đã nối status runtime vào sáu prefab ordinary và Dép. Đây là implementation/authoring, chưa phải acceptance Unity; RF3D-021/P4 vẫn IN_PROGRESS.

- `DefenseStatusRequest` thêm Knockback (giữ enum Slow0/Stun1), session identity và generation. Constructor bốn đối số cũ giữ tương thích source/IsValid; production runtime **yêu cầu scope hiện tại** và từ chối payload không scope, foreign/old generation, pause/terminal, input không hợp lệ hoặc duration>10s. Enemy receiver còn kiểm tra IsTargetable/current lifetime/dead gate.
- `DefenseStatusRuntime` thuần C#: tối đa8 Slow đang sống, lấy mức mạnh nhất thay vì nhân; mỗi entry giữ expiry riêng nên Slow yếu dài vẫn còn sau khi Slow mạnh ngắn hết hạn. Full buffer trả false, không cấp thêm bộ nhớ hoặc evict hiệu ứng sống. Stun refresh theo max expiry, không rút ngắn Stun cũ. Knockback chỉ một hiệu ứng, không restart/stack; strength là tỷ lệ0..1 của maximum distance2m. Direction normalize chống overflow; tiêu bước theo gameplay clock, clip phần còn lại khi frame trễ, mỗi thời điểm chỉ tiêu một lần.
- `DefenseEnemyStatus3D` author trên generic/MaGiáp/ThủyQuái/QuáiDù/HuyếtDơi/PhiCương. Cache component tại Bind, reset khi Enemy.Initialize/cleanup/disable/restart. Không tạo GameObject/material hay coroutine trong status loop. Enemy3D delegate IDefenseStatusReceiver tới component; không có thêm health/reward owner.
- Ground giữ path/waypoint khi Stun, điều chỉnh agent.speed từ base speed với Slow để không nhân lũy tiến; speed0 dừng agent. Knockback kiểm tra sample gần đúng XZ, NavMesh.Raycast không cắt đứt bề mặt, chênh cao tối đa0.25m và swept capsule/endpoint blockers. Warp theo từng bước được duyệt, replan waypoint khi xong; replan lỗi release qua factory không reward. **Các phép kiểm tra native chưa chạy thực tế.**
- Water bơi chỉ bị đẩy trong bounds kênh theo XZ và capsule clearance; Docking từ chối Knockback để giữ handoff. Slow/Stun vẫn áp dụng khi bơi/cập bến; thời hạn chờ bến được bù thời gian bị khống chế/giảm tốc để tránh retire chỉ vì CC. Sau Ground handoff dùng validator Ground, không SamplePosition swimmer lên bờ tùy ý.
- Air dịch chuyển theo XYZ qua CanMove/sphere sweep, giữ altitude/volume/clearance. Stun/Knockback chặn tự bay và attack; Slow giảm bước bay. Bộ đếm stuck được giữ lại khi immobilized và reset theo displacement, tránh retire vì Stun/full Slow.
- Ground/Air aggro không repath trong Stun/Knockback; `DefenseEnemyAttack3D` chặn hit khi status block. Cooldown vẫn tiến theo gameplay clock và không bị reset khi hiệu ứng kết thúc. Slow chỉ giảm movement, không giảm tốc đánh. Nếu displacement bị chặn thì hủy riêng Knockback, giữ Slow/Stun.
- Projectile Landing chưa phải enemy target nên không nhận status; chỉ actor quaidu sau touchdown mới nhận Ground statuses. Boss/miniboss và Linh Pháo chưa được author status receiver, hiện từ chối các effect này; không làm stun/knockback Boss khi FSM chưa hỗ trợ.

### Nguồn gây hiệu ứng

`DefenseRelicSkillEffect3D` sau damage thành công, chỉ áp status lên mục tiêu sống còn targetable. Scene effect trỏ `RelicStatus_SongQue.asset`: **Slow35%/2s, Stun0.25s, Knockback0.2s với strength0.5 của max2m (tối đa1m)**. Đây là cấu hình prototype mới, không khẳng định sao chép cân bằng Web/D-07. Damage/cooldown/radius skill asset giữ nguyên. Không có hướng đẩy XZ ở tâm AoE thì Ground từ chối Knockback; các effect khác vẫn áp. Không có status VFX/UI riêng trong gói này.

### Kiểm chứng

**325 pure C# checks PASS** =292 trước +33 scoped status core cases: strongest/overlap/expiry/capacity, refresh Stun, full Slow, pause/modal/terminal, once-only/clipped Knockback, cancellation, scope/reset/old generation, finite/overflow. Pure tests không chạy collider/NavMesh adapter.

Runtime và runtime/Editor/fixture source compile PASS. `VerifyStatusAuthoring.py` PASS: unique YAML keys/fileIDs, sáu prefab chỉ thêm status component/root binding, scene chỉ đổi Relic effect profile reference, domain displacement guards, attack interruption wiring, **98 protected resource hashes giữ nguyên** (Stage/stats/skill damage+cooldown assets/Boss/Cannon/UI/metas). Native NavMeshPath initializer regression guard còn nguyên. Logs `Logs/TraversalQA/defense-status-*.txt`; snapshot `Logs/DefenseStatuses_20261027_091652/`.

**NOT_RUN:** Unity import/lifecycle, actual Stun agent stopping/resume, Warp/baseOffset/path restoration, collision across bridge/water/air, rendered Knockback, balance/full-run/device. Không chạy Unity theo yêu cầu người dùng. Còn Phi Cương Trảo Quỷ telegraph, Boss FSM integration và nghiệm thu các domain trước khi đóng P4; Support vẫn chỉ có endpoint/roster, chưa có units gameplay.

---

## P4 — Aggro đa địa hình Hero/Support/Base (RF3D-021, gói 5) — 27/10/2026

Đã nối policy chọn mục tiêu dùng chung cho ordinary Ground, Thủy Quái sau cập bến, Tiểu Quỷ sau touchdown và Air. RF3D-021 vẫn IN_PROGRESS; không coi gói này hoàn tất Support gameplay hoặc toàn P4.

- `DefenseAggroPolicy`/`DefenseAggroRules` thuần C#: acquire4.2, retain7, lock0.75s, think0.25s, provoked3s từ `Aggro_SongQue.asset`. Chọn Hero/Support gần nhất có khả năng tiếp cận, tie giữ thứ tự roster; giữ focus thay vì đổi liên tục khi có mục tiêu gần hơn. Null focus là Base. Down/dead/inactive, removed roster hoặc mất đường đi bỏ focus ngay cả khi đang lock. Khiêu khích mở acquire đến retain range chỉ cho đúng nguồn gây damage, không cho mọi mục tiêu gần đó.
- Ground vẫn dùng profile navigation để giữ assigned lane và đường đến Base thực. Shared Aggro profile mới sở hữu các ngưỡng chọn mục tiêu; các field aggro cũ trong GroundNavigation profile và pure legacy policy được giữ để tương thích dữ liệu/fixture, không điều khiển ordinary aggro mới nữa.
- `DefenseFriendlyTargets3D` được author tại `MapSongQue/GameplayMarkers/DefenseFriendlyTargets`, inject qua RuntimeRoots/Run/factory; roster bind trước khi spawn actor. Hero health ref là component hiện có; Supports hiện **rỗng**. Không global find hay tạo GameObject phụ.
- `DefenseFriendlyTarget3D` cache health/Transform/Collider ở composition. Thêm `DefenseSupportTarget3D` làm health/damage endpoint tùy chọn có scope/generation/gate/cleanup/reset. Đây **không phải** Support spawner, AI, vũ khí, visual hoặc đơn vị dân binh hoàn chỉnh; chưa thêm Support vào trận. Khi author Support sau này phải thêm component này vào actor và nối vào roster trước Bind.
- Ordinary Ground và hai domain đã tiếp đất dùng `DefenseGroundAggro3D`: cùng bờ và complete NavMesh path mới đuổi friendly; mất focus trở về đường Base/lane hợp lệ. Attack kiểm tra lại SameBank/range/LOS tại hit. Không reset cooldown khi retarget. Boss không bị chuyển sang policy này.
- Water chỉ Configure grounded aggro sau shore validation + agent Warp thành công; lúc Swimming/Docking không có grounded combat. Handoff lỗi được stop/release qua owner không reward, không để exception trong Update giữ actor lỗi. QuaiDu chỉ vào policy sau SpawnLanded đã được duyệt, giữ landing factory guard/agent disabled trên prefab.
- Air dùng policy tương tự với metric XYZ, không ép SameBank và không NavMesh. Reachability kiểm tra swept clearance đến cruise point của mục tiêu; damage vẫn range XYZ/LOS. Mất mục tiêu quay về Base; pursuit bị kẹt trả về Base trước, Base route kẹt mới dùng release policy có sẵn. Shared enemy attack vẫn giữ cooldown qua mọi đổi target.
- Sáu prefab ordinary (generic/MaGiáp/ThủyQuái/QuáiDù/HuyếtDơi/PhiCương) trỏ shared Aggro profile; Water/QuaiDu được author GroundAggro component và LOS mask4105. Không sửa Stage counts/caps/TTL, damage/HP/reward definitions, Boss, Cannon, UI hoặc baked NavMesh.

**Kiểm chứng ngoài Unity:** runtime và runtime/Editor/fixture source compile PASS. **292 pure C# checks PASS** =265 trước +27 multi-domain policy cases: nearest/lock/hysteresis, down/removed/unreachable, Hero/Support-specific provocation, Ground XZ vs Air XYZ, blocked nearest fallback, stable ties, invalid inputs/profile. Pure reachability tests dùng predicate giả lập, không chứng minh NavMesh/Physics thật.

`VerifyMultiAggroAuthoring.py` PASS: unique YAML keys/fileIDs, roster có Hero đúng + Supports rỗng, shared profile trên sáu prefab, Water chỉ handoff ở Ground phase, bind ordering, chỉ hai scene block cũ đổi binding +ba block mới, bảo toàn **96 protected resource hashes** và native NavMeshPath vẫn chỉ tạo trong Awake. Logs `Logs/TraversalQA/defense-multi-aggro-*.txt`; snapshot `Logs/DefenseMultiAggro_20261027_090209/`.

**NOT_RUN:** Unity import/serialization/Awake/event ordering, NavMesh chasing/retarget, Air clearance/LOS, actual damage/cooldown và Support gameplay, visual/device. Support roster là cấu hình tĩnh tại Bind; chưa có runtime Support registration/spawn service. Còn slow/stun/knockback theo domain, Trảo Quỷ telegraph Phi Cương, Boss FSM và acceptance trước khi đóng P4.

---

## P4 — Landing và ba Linh Pháo (RF3D-021, gói 4) — 27/10/2026

Đã triển khai source và authoring chuỗi `BALLISTIC → PARACHUTING → LANDED → quaidu Ground`; RF3D-021 vẫn IN_PROGRESS, chưa nghiệm thu Unity/full-run.

- Có `DefenseGhostCannon3D.prefab` tái sử dụng: body/hitbox, Muzzle, một projectile view inactive và canopy inactive author sẵn. Scene có `MapSongQue/Structures/GhostCannonBattery` với ba linked prefab instance North/Mid/South ở East X18,Y1,Z12/0/−12 và ba West anchors X−9,Y0 cùng Z. Không tạo GameObject/material/projectile trong vòng lặp runtime; mỗi pháo tái sử dụng một view.
- `DefenseLandingFlight` là core thuần C#: đường ballistic dạng parabol từ Muzzle đến vị trí bung dù trên anchor; parachute hạ Y trên XZ cố định; chỉ touchdown phát landing request một lần. Dùng gameplay clock/session/generation; pause/modal đóng gate, restart/cleanup hủy đạn cũ. Đây là đường bay script, chưa có va chạm đạn vật lý/impact damage giữa hành trình.
- Pháo có health core và đăng ký Ground target của chính session, Hero/Tower/skill có thể gây damage theo capability hiện tại. Phá hủy tắt body/hitbox và ngừng phát mới; **đạn đã bắn vẫn được đáp**. Restart hồi phục pháo và reset cadence. Pháo không cấp reward mới trong gói này; reward của quaidu dùng owner/lifetime/drop hiện có sau khi actor được tạo.
- Landing enqueue vào chính SpawnPlanner hiện có, kind Landing với ID cannon+shot, giữ dedup đến cuối lượt. Không thêm quaidu vào 34 timed events hoặc thay elite replacement. Giữ chung32 active mobile enemies/40 pending/4 factory attempts per frame/TTL25s; ba công trình cố định và tối đa ba projectile view không chiếm mobile actor slots. Full queue/TTL xử lý như job thường, có warning; không bypass cap.
- Battery kiểm tra mặt đất Ground, vị trí West, NavMesh sample radius0.3, complete path đến Base và clearance capsule. Spawn predicate giữ Hero exclusion XZ3.1m. Factory có đường SpawnLanded revalidate geometry; legacy Spawn không được tạo ParatrooperLanding trực tiếp. QuaiDu prefab author NavMeshAgent disabled, chỉ bật trong owned acquisition sau touchdown. Khi tạo xong actor dùng Ground route thẳng đến Base thực; không đi vòng về East/cầu. Baked NavMesh không đổi.
- Battery/Cannon bindings được inject qua RuntimeRoots và Run; không global find, không tạo native NavMeshPath trong field initializer. Battery cache path trong Awake và fail closed trước Awake, giữ bản sửa Water serialization trước đó.

### Cấu hình prototype đang dùng

`GhostCannon_SongQue.asset`: HP300, phát đầu30s với offsets0/4/8s cho ba pháo, interval12s, ballistic2s, parachute3s, arc5m và bung dù ở worldY4. Không catch-up burst khi frame trễ. Dừng phát mới từ600s; đạn đã bay và landing jobs đã có vẫn hoàn tất trong Overtime khi gameplay gate còn mở. **Đây là lựa chọn triển khai có thể chỉnh trên asset, chưa phải số cân bằng Web/D-07 hoặc chính sách Overtime cuối đã được nghiệm thu.** Không có nguồn local WaveManager/GhostCannon được tìm thấy trong lần rà này để khẳng định port nguyên các con số đó.

### Kiểm chứng

**265 pure C# checks PASS** =232 trước +33 Landing/cadence/shared planner: không delivery trước touchdown, exact phase boundaries, parabola/descent, pause/modal, once-only/long frame, cancellation/foreign scope/old generation/terminal, source destruction, already-fired completion, cadence/no burst/cutoff/reset, active cap/frame budget/TTL/factory retry. Các case planner dùng cùng class thật, chưa chạy adapter Unity.

Runtime và runtime/Editor/fixture source compile PASS. `VerifyLandingAuthoring.py` PASS: YAML unique keys/fileIDs, chỉ hai block scene cũ đổi binding +18 block mới, ba linked prefab instances/anchors, projectile/canopy inactive, native path chỉ tạo trong Awake, QuaiDu prefab chỉ đổi enabled agent, **97 protected resource hashes giữ nguyên** (Stage/balance definitions/Water/Air/Ground/Boss/UI/metas). Evidence `Logs/TraversalQA/defense-landing-*.txt`; snapshot `Logs/DefenseLanding_20261027_084524/`.

**NOT_RUN:** Unity import/Awake/serialization, rendering/animation, Physics/LOS, actual landing NavMesh/clearance, phá pháo khi chơi, full-run/camera/device. Model pháo/dù hiện là primitive prototype. Không coi các kiểm tra ngoài Unity đã chứng minh gameplay acceptance. Tiểu Quỷ sau đáp hiện đánh Base; Hero/Support aggro đa-domain, slow/stun/knockback, Phi Cương telegraph và phần Boss còn lại chưa hoàn tất RF3D-021/P4.

---

## Sửa NavMeshPath khởi tạo khi serialization — 27/10/2026

Log người dùng xác nhận `DefenseWaterNavigation3D` tạo native NavMeshPath trong instance field initializer, bị Unity cấm khi constructor/serialization. Đã chuyển cache path sang `Awake`; `TryResolveShore` trả false nếu được gọi trước Awake, không tạo native object trong constructor. Dòng log nhắc DefenseRuntimeRoots3D là context serialization, không phải một path constructor khác trong component đó.

Rà Defense3D: GroundMovement tạo path trong method; SpawnPlacement là plain C# service được tạo trong Initialize session, không phải MonoBehaviour/serialized field. Không phát hiện field initializer native path khác trên MonoBehaviour trong feature. Runtime và runtime/Editor/fixture source compile PASS; kiểm tra source xác nhận initializer đã bỏ, Awake tạo path và pre-Awake guard có mặt. Không chạy Unity; compiler ngoài Unity không tái hiện quy tắc native serialization và không đủ để khẳng định runtime acceptance.

---

## P4 — Air và anti-air (RF3D-021, gói 3) — 27/10/2026

Đã triển khai source và authoring cho Huyết Dơi/Phi Cương bay XYZ và Hero/Tower đánh Air hợp lệ. RF3D-021 vẫn **IN_PROGRESS**, chưa đóng gameplay acceptance.

- `DefenseAirNavigation3D` + `Air_SongQue.asset` tách khỏi NavMesh. Scene có `MapSongQue/GameplayMarkers/AirNavigation_FlightVolume`: Box trigger60×5×40, centerY2.5, layer Ignore Raycast. Altitude worldY1.4..4, cruise2.2, clearance sphere0.7, obstacle mask4105 (Default/Obstacle/Ground), timeout khi không tiến gần mục tiêu10s gameplay. Đây là thông số authoring ban đầu, chưa đo camera/collision thực.
- Air spawn giữ X/Z và lane của chính zone trên actualSpawnType, chỉ đặt Y theo cruise; kiểm tra volume/clearance và safe Hero XZ3.1m. Không SamplePosition Air, không đổi elite sang zone khác, không fallback Ground. Giữ nguyên Stage assets/counts/caps/TTL/Production binding.
- Hai prefab Air: layer Enemy, NavMeshAgent disabled sẵn, `DefenseAirMovement3D` và Sphere trigger hitbox radius0.5. Factory inject navigation/context ở Staging; kiểm tra world clearance bao được hitbox trước khi kích hoạt. Không tạo component/GameObject bổ sung trong flight loop.
- Air hướng đến Base, kiểm tra overlap tại đầu/cuối và SphereCast toàn bước di chuyển; thử hướng thẳng/lên/hai bên/xuống trong giới hạn độ cao. Đây là tránh vật cản cục bộ, không phải bộ tìm đường Air hoàn chỉnh. Nếu không tiến gần Base trong10s sẽ release qua factory, không reward; speed0 giữ nguyên stationary baseline. Gate pause/modal/terminal/current lifetime chặn flight; cleanup/disable kết thúc movement.
- Attack của hai prefab Air đo range XYZ tới collider Base và LOS. Hero/Tower có hai `DefenseAttackTargetingProfileSO` riêng (Ground|Air và LOS mask4105). Selector dùng query session, Ground giữ range XZ, Air dùng XYZ; bỏ qua ứng viên bị che trước khi chọn nearest. RaycastNonAlloc16 hits, đầy buffer thì không bắn. Không tiêu cooldown khi không có ứng viên hợp lệ. Các nguồn skill AoE hiện có không bị đổi sang chính sách ranged này.
- `IDefenseSpatialTargetQuery` là API thêm mới; API FindNearest/TryCopyWithin cũ giữ semantics để không đổi các skill/fixture trước. Không thêm trạng thái thưởng/HP hoặc owner lifecycle song song.

**Kiểm chứng ngoài Unity:** runtime và runtime/Editor/fixture source compile PASS. **232 checks C# PASS** =216 hồi quy +16 geometry/target policy mới: Air cao ngoài range, XYZ exact boundary, Ground planar compatibility, finite/overflow, height bounds, blocked nearest không che ứng viên hợp lệ, target masks, unavailable/tie, clear/dispose. Check LOS trong pure harness là predicate giả lập; không chứng minh Physics LOS thực. Logs `Logs/TraversalQA/defense-air-*.txt`.

`VerifyAirAuthoring.py` PASS: YAML unique keys/fileIDs, chỉ ba scene block cũ đổi binding + bốn block mới, hai Air prefab chỉ đổi composition/agent/attack, hitbox/GUID/profile refs, 89 resource hashes cũ giữ nguyên (gồm Stage, Water, Ground, Boss, UI và metas). Snapshot `Logs/DefenseAir_20261027_083234/`.

**NOT_RUN:** Unity import/render/PlayMode, ray/sphere physics, đường bay qua cây/cầu, camera/frustum, DPS/device. Không khẳng định local avoidance luôn tìm được đường; GLB visual không có Collider sẽ không chặn sphere/LOS. Chưa triển khai Hero/Support aggro riêng cho Air, status slow/stun/knockback hoặc Trảo Quỷ telegraph của Phi Cương. Landing/GhostCannons, elite skill và các phần còn lại vẫn cần thực hiện trước khi đóng RF3D-021. Không chạy/bake Unity theo yêu cầu người dùng.

---

## P4 — Water → Shore (RF3D-021, gói 2) — 27/10/2026

Đã triển khai source và authoring cho Thủy Quái; RF3D-021 vẫn đang thực hiện, chưa đánh dấu hoàn tất hoặc chấp nhận gameplay.

- `DefenseSpawnPlacement3D` tách AmphibianWaterToShore khỏi NavMesh spawn: giữ nguyên điểm canalWater, kiểm tra bounds, khoảng cách Hero và bến có mặt đất/đường hoàn chỉnh đến Base. Không SamplePosition điểm spawn trong nước để kéo quái lên cầu/bờ.
- Scene có `MapSongQue/GameplayMarkers/WaterNavigation`, collider bounds nước và ba cặp `WaterExit_0..2`/`ShoreExit_0..2`. Water anchor X4.8/Y1.1, shore X3.8/Y0, Z−14/0/14. Chọn water anchor gần điểm spawn nhất. Không thay geometry GLB, bake data, lịch spawn hoặc Production profile.
- ThuyQuai prefab có `DefenseWaterMovement3D`, NavMeshAgent tắt sẵn. Factory inject navigation/context khi actor còn ở Staging; prefab không tự tìm dependency và không tạo thêm GameObject/component runtime.
- Swimming đi thẳng trong kênh đến water anchor; Docking đi đến shore đã kiểm tra; chỉ bật/Warp agent sau kiểm tra lại mặt đất/NavMesh/path. Ground đi đến Base thực, giữ HP, target registration, lifetime và reward owner cũ. Không disable actor khi đổi locomotion.
- Movement dừng theo gameplay gate và current lifetime/targetability; cleanup/disable kết thúc phase. Chờ bến có retry theo gameplay clock và timeout25s tính từ lúc đến water anchor; lỗi handoff trả actor về factory không phát reward. Quái có speed0 vẫn có thể đứng yên theo stats hiện có.
- Bindings scene được kiểm tra lúc khởi tạo; thiếu navigation/anchor sẽ fail rõ ràng. Legacy static Spawn không có scene roots sẽ từ chối amphibian thay vì giả lập bằng đường Ground.

**Kiểm chứng ngoài Unity:** runtime compile và runtime/Editor/fixture source compile PASS; 216 checks C# hiện có PASS (hồi quy lifecycle/combat/input/queue/Ground, không phải 216 kiểm tra Water). `Tools/Defense3DValidation/VerifyWaterShore.py` PASS: unique fileIDs, bảo toàn mọi block scene cũ trừ hai binding được phép, 15 block authoring mới, cặp anchors/bounds/ground mask, prefab agent disabled và GUID refs. Logs core: `Logs/TraversalQA/defense-water-shore-*.txt`. Snapshot: `Logs/DefenseWaterShore_20261027_000057/`.

**NOT_RUN:** Unity import, rendering, physics, baked NavMesh, collision và di chuyển thực tế. Chưa chứng minh anchor không va chạm vật trang trí; đường bơi hiện là đoạn thẳng trong canal, chưa có né vật cản nước. Water sau cập bến hiện tấn công Base, chưa có Hero aggro riêng. Air/anti-air và Landing/GhostCannons còn lại của RF3D-021 chưa triển khai. Không rebake NavMesh.

---

## Sửa cảnh báo Spawn queue overflow — 26/10/2026

### Chẩn đoán từ log và asset thực

Scene đang trỏ `Stage01_FastSpawn`, không phải Production: 34 đợt/214 ordinary slots được nén trong29.7s, khoảng0.9s/đợt thay vì18s. `benque-21` xuất hiện ở18s thay vì360s. Boss độc lập của FastSpawn ở10s thay vì510s. Cả hai profile vẫn giới hạn32 active/40 pending/4 factory attempts mỗi frame/TTL25s, nên FastSpawn có thể overflow dù đường đi hoàn toàn hợp lệ. Đây là warning về capacity, không phải exception từ stack trace.

Không chỉ từ log có thể khẳng định tất cả actor đang stuck hay tất cả geometry bị chặn. Mô phỏng planner bằng chính các mốc/count của asset, mọi placement hợp lệ và không actor nào bị thu hồi, tái hiện FastSpawn overflow; Production không overflow qua600s với bước0.1s trong cùng mô phỏng. Đây không phải mô phỏng NavMesh/combat thực.

### Đã sửa

- Scene chính chuyển reference sang `Stage01_Production`. Giữ nguyên FastSpawn asset để stress test có chủ đích; không đổi event counts, elite replacement, Boss definition hoặc giới hạn/TTL.
- Planner có expiry sweep độc lập active cap và thứ tự queue. Trước đây khi active cap đầy, ProcessSpawns dừng ở head chưa hết hạn, có thể giữ job cũ đã rotate xuống sau; scheduler còn enqueue trước khi ProcessSpawns thu hồi TTL. Nay dọn expired jobs trước enqueue/capacity check và trước xử lý frame.
- Giữ nguyên TTL strict `age > lifetime`, không kéo dài TTL cho retry. Guard chống expiry callback reset/reentrant enqueue ăn nhầm job của lượt mới.
- Thêm counters spawned/pending/expired/overflow và blocked predicate/factory của batch trước. Log overflow có profile, thời gian, active/limit, factory-owned count, pending/limit và các counter; startup log in profile đang dùng. Predicate blocked có thể là retry delay hoặc placement, không kết luận riêng NavMesh từ counter này.
- Overflow thật vẫn bị ghi nhận và không vượt cap. Không tăng queue vô hạn, không tắt warning, không đổi chính sách drop-on-overflow hiện có.

### Kiểm chứng

**216 standalone C# checks PASS:**202 trước +14 queue/profile cases: expiry ở active cap, rotated stale tail, reclaim trước enqueue, genuine overflow, exact TTL, notify once, reset accounting/reentrant reset, predicate vs factory failure, profile counts/cadence, FastSpawn reproduction và slot conservation.

Runtime và runtime/Editor/fixture source compile PASS; chỉ Run stage reference đổi trong scene, cả2 Stage asset giữ nguyên byte. Evidence `Logs/TraversalQA/defense-spawn-queue-*.txt`, checker `VerifySpawnQueueFix.py`. Snapshot `Logs/DefenseSpawnQueueFix_20261026_230832/`.

**NOT_RUN Unity/NavMesh/physics/combat/device.** Water/air/landing vẫn chưa migrate; geometry bị chặn hoặc active actor không thu hồi vẫn có thể tạo áp lực queue, cần đọc log mới thay vì suy diễn warning cũ đã chứng minh nguyên nhân đó. Catch-up với frame delta rất lớn cũng có thể overflow đúng policy.

Profile thay trên disk chỉ có hiệu lực khi nạp scene và tạo session mới; nút Restart trong cùng session giữ compiled plan cũ. Không coi đổi source ref sẽ thay một trận FastSpawn đang chạy.

---

## P4 — Ground navigation và aggro Hero/Base (RF3D-021, gói 1) — 26/10/2026

### Phạm vi đã nối

- Ground thường (prefab `DefenseEnemy3D` và `DefenseEnemy_Magiap`) có component `DefenseGroundAggro3D` và profile `GroundNavigation_SongQue.asset` được author sẵn. Bind context/Enemy/Movement/Attack/Hero/Base qua composition hiện có; không AddComponent/global find trong trận.
- `DefenseGroundNavigationRules` và `DefenseGroundAggroPolicy` là C# core tách riêng. Profile capture khi actor configure, giữ snapshot của actor đó; sửa profile chỉ ảnh hưởng các actor configure sau đó. Chưa đưa navigation profile vào compiled Stage snapshot chung.
- Quái đi từ East giữ lane qua Entry/Mid/WestExit rồi tới **Base thực**. Quái ở West đi thẳng tới Base trên NavMesh. Route không còn kết thúc ở x=-12 trong khi Base scene đang x=-22.41. Legacy/Boss route overload cũng thay endpoint bằng Transform Base thật, không reset HP.
- Actor đang đi trên đúng cầu có thể nối lại đoạn WestExit→Base khi mất aggro; không teleport về East. Ground spawn mới giữa nước vẫn bị từ chối. Retarget không thể nối route sẽ stop/retire không thưởng, qua factory lifecycle.
- Ground spawn placement ở West kiểm tra path tới Base, không ép đường ngược về bridge entry East. Các domain khác vẫn còn validator NavMesh cũ, chưa được migrate trong gói này.

### Aggro và melee

- Source D-06/A được dùng làm baseline prototype: acquire4.2m, retain/provoked7m, target lock0.75s, think/repath0.25s, provocation3s. Các giá trị nằm trong SO, không phải balance đã nghiệm thu.
- Hero cùng bờ, sống/active và có path complete mới được chọn. Hero gần acquire range sẽ kéo aggro; hysteresis/lock giảm đổi mục tiêu liên tục. Hero down/mất path/khác bờ thì chuyển về Base. Damage từ Hero trong7m có thể provoke; chưa có Support registry hoặc target score cho dân binh.
- Ground aggro từ chối water/air/landing và Boss; Yaksha/miniboss không bị kéo vào luật Hero chase mới. Boss policy riêng tiếp tục thuộc RF3D-023.
- Enemy Attack cache collider mục tiêu, đo tới **bề mặt collider** thay vì tâm Base; áp per-hit same-bank guard và LOS mask Default/Obstacle/Water=25 trên hai prefab đã migrate. Không reset attack timer khi nhắc lại cùng mục tiêu hoặc repath, tránh hit miễn phí do chuyển trạng thái.
- GroundMovement tách tolerance waypoint0.35 và final attack reach có collider extent; stopping distance phù hợp để dừng ngoài Base mà vẫn vào melee range. Tái sử dụng NavMeshPath/route buffers, không clone array hay tạo path mới theo mỗi think/repath.

### Asset và kiểm chứng

Thêm3 script +1 profile SO, thêm1 aggro component và LOS mask vào mỗi prefab Ground nêu trên. Không thêm/sửa GameObject scene. **278 file được bảo vệ giữ nguyên SHA256**, gồm toàn scene/MapSongQue, NavMesh, các enemy prefab khác, stage/stat/skill và shader chỉ dấu đã sửa pass.

Snapshot: `Logs/DefenseP4GroundAggro_20261026_224656/`.

- **202 core checks PASS:**181 trước +21 Ground route/aggro cases, gồm ba lane về Base thật, West direct, không spawn Ground giữa nước, resume đúng bridge, buffer saturation, same-bank, acquire/retain/lock/provoked, down/path loss và invalid geometry.
- Runtime và runtime/Editor/fixture source compatibility compile PASS, không thực thi Unity fixture.
- Checker `VerifyP4GroundAggro.py`: prefab refs/component/profile/LOS và protected hashes/source boundary checks PASS. Evidence `Logs/TraversalQA/defense-p4-ground-*.txt`.

**NOT_RUN:** Unity callbacks, NavMeshAgent stopping/SetPath, physics/LOS/melee thực tế, rendered UI và Android. Core policy PASS không chứng minh đường đi/aggro đã chạy đúng trong scene.

### Phần còn lại của RF3D-021

Water→shore với Exit Anchors; Air3D/anti-air/LOS/hitbox; landing contract và ba Linh Pháo; Support targeting; trạng thái stun/slow trên các domain. Các actor chưa migrate vẫn dùng bridge adapter/validator cũ; không coi Ground package này đã sửa water/air/landing. Cần nối các strategy đó ở các gói tiếp theo, không đổi enum/data để giả lập đã hỗ trợ.

---

## P4 — Gói đầu: session factory, lifecycle Enemy và runtime roots — 26/10/2026

### Trạng thái

| Ticket | Trạng thái | Kết quả / phần còn lại |
|---|---|---|
| RF3D-020 | PRODUCTION_FACTORY_LIFECYCLE_WIRED / STATIC_CHECKED / NOT_ACCEPTED | Session factory sở hữu actor, bind inactive trước enable, lifetime/reward/release hooks và runtime roots có sẵn trong scene. Chưa chạy callbacks Unity/NavMesh hoặc triển khai pool reuse |
| RF3D-006 | ACTOR_RELEASE_HARDENED / PARTIAL | Cleanup qua factory retire scope trước disable/destroy; callback cũ không giải phóng slot lượt mới. Pool lease/delayed projectile/pickup contracts đầy đủ còn ở P5 |
| RF3D-021 | NEXT | Ground/water/air/landing + aggro strategies; hiện vẫn dùng bridge adapter/movement cũ cho production |
| RF3D-022 | EXISTING_RETRY_COUNTS_PRESERVED / PARTIAL | Planner/scheduler giữ nguyên; slot chỉ được commit sau factory/encounter thành công. Chưa nghiệm thu lịch stage/traversal trong scene |
| RF3D-023 | NEXT | Thống nhất Boss FSM với encounter; chưa đổi arena/assault trong gói này |

### Đường production mới

`Stage snapshot → SpawnRuntime → DefenseEnemySessionFactory3D → shared composition helper → inactive Staging → bind context/stats/target/lifetime → Actors → route/arena → committed planner slot`.

- Run nhận `DefenseRuntimeRoots3D` qua serialized reference, validate root cùng scene, độc lập/identity, các child đúng active state. Không tạo parent GameObject bằng code runtime hoặc global find.
- Factory theo session nhận context, wallet, target index và authored roots; từ chối index/factory thuộc session khác. Acquire chặn khi gameplay gate đóng, cleanup đang chạy hoặc Actors/Pickups inactive/Staging bị bật.
- Instance được tạo dưới Staging inactive để composition có thể inject trước OnEnable. Actor được đăng ký owner, lifetime token và Removed observer trước khi chuyển sang Actors. Configure ground route thực hiện sau enable vì NavMeshAgent cần active. Thứ tự này đã được source-check, chưa thực thi callbacks Unity.
- `DefenseActorLifetime` chứa immutable session identity/generation/lease ID; latch retirement và reward một lần. Không rearm token cũ khi spawn lại. Enum validation không boxing trong đường release.
- Actor targetability/damage chặn token retired/generation cũ. Drop production claim token thay cho bool riêng; bool cũ chỉ dùng nhánh compatibility không có token. Dù spawn pickup ném lỗi, finally vẫn yêu cầu release actor.
- Defeat/SpawnFailed/Cleanup/ExternalRemoved/ExternalDisabled đi qua factory owner. Retire trước disable/destroy, unsubscribe Removed, notify một lần. Manual disable cũng release slot và không grant reward. Cleanup cố gắng mọi entry, aggregate lỗi; entry lỗi physical release còn được giữ để lần cleanup sau thử lại.
- SpawnRuntime giữ bảng **committed actor→token** cho planner bookkeeping, không destroy actor trực tiếp. Release chỉ giảm active slot nếu đúng token/generation và không phải cleanup/spawn failure; callback destruction trễ không giảm hai lần. Factory error vẫn giữ job/retry/TTL như planner trước.
- Static factory/public prefab APIs giữ compatibility cho smoke/fixture, production dùng instance owner. Chưa chuyển toàn bộ legacy caller sang ownership mới.

### Hierarchy

```text
DefenseRuntime                    # scene root, TRS identity
├── Actors                        # active; production Enemy/Boss instances
├── Staging                       # authored inactive; composition before enable
├── Pickups                       # active; collector-bound wood drops
└── Pools_Reserved                # authored inactive; chưa có pool reuse
```

Root nằm ngoài `MapSongQue`, cùng scene với Run. Wood drop nhận parent qua injection; cleanup pickup vẫn theo gameplay context hiện có, chưa có lease/pool riêng.

Scene chỉ thêm11 YAML blocks (5 GameObject/5 Transform/1 root reference component), bind `_runtimeRoots` trên Run và thêm SceneRoots entry. Tổng **228 blocks**, tất cả local refs hợp lệ. Map hierarchy,18 scene participants, UI/skill/indicator/Boss bindings cũ không đổi. **180 resource/NavMesh/metadata file giữ nguyên SHA256**; không đổi enemy prefab/stat/stage hoặc GLB/NavMesh data.

Snapshot: `Logs/DefenseP4FactoryLifecycle_20261026_222601/`.

### Kiểm chứng

- **181 C# core checks PASS:**160 cũ +21 actor lifecycle checks. Bao phủ scope/generation, pause/terminal, reward once, retirement reason, external disable, cleanup/failed spawn không reward, callback cũ sau restart, replacement identity và lỗi tạo reward không claim lần hai.
- Runtime và runtime/Editor/fixture source compatibility compile PASS, không chạy fixture Unity.
- Checker `Tools/Defense3DValidation/VerifyP4FactoryLifecycle.py`: YAML/root states/ref/hash, registration trước enable, ownership guards, planner token path và injected pickup parent PASS.
- Evidence `Logs/TraversalQA/defense-p4-factory-*.txt`; core runner `Tools/Defense3DValidation/RunCoreChecks.ps1`.

**NOT_RUN:** Unity import/Awake/OnEnable/OnDisable/OnDestroy ordering, actual reward/actor release, NavMesh/physics, rendered UI và Android. Pure token checks không chứng minh factory Unity đã chạy đúng. **Chưa pooling**: production vẫn Instantiate/Destroy tại owner; root Pools chỉ chuẩn bị authoring. Chưa sửa air/water locomotion, aggro, 3 Linh Pháo hoặc Boss FSM.

### Gói tiếp theo

RF3D-021: tách movement/aggro strategy theo locomotion và author vào prefab; giữ factory/ownership boundary vừa nối. Sau đó đối soát schedule/planner counts (RF3D-022) và Boss encounter/FSM (RF3D-023). Không coi toàn P4 hoàn thành.

---

## Sửa nguyên nhân renderer bỏ qua toàn bộ chỉ dấu — 26/10/2026

Ảnh mới tiếp tục không có vòng chân/vòng ngắm. Rà cấu hình pipeline và source URP cài trong project tìm được mismatch cụ thể: GraphicsSettings dùng `URP_2D_Asset`, default renderer là `URP_2D_Renderer`; Main Camera có renderer index -1 nên kế thừa renderer này. `Render2DLightingPass.cs` của URP14.0.12 chỉ nhận `SRPDefaultUnlit`/`Universal2D` trong draw pass liên quan; shader `DefenseAimOverlay` trước đó chỉ có `UniversalForward`, nên mesh/LineRenderer world-space bị bỏ qua. UI Canvas/cancel vẫn có thể hiển thị độc lập.

Đổi LightMode của pass chỉ dấu sang **SRPDefaultUnlit**, giữ unlit/tint/shapes/ZTestAlways/ZWriteOff. Không đổi pipeline toàn project, camera, shader GUID, scene hoặc prefab. Cả8 material range/foot/UI/impact tham chiếu shader này nên nhận cùng sửa lỗi.

`VerifyIndicatorRenderer.py` đối chiếu với source renderer thực tế: shader snapshot trước sửa bị loại, shader mới có tag được nhận. Camera/pipeline refs và8 material GUID checks PASS; scene/prefab/settings/hash bảo vệ giữ nguyên. Evidence `Logs/TraversalQA/defense-indicator-renderer-fix-checks.txt`; snapshot `Logs/DefenseIndicatorRendererFix_20261026_221451/`.

Các sửa trước về depth/preview không đủ vì chưa kiểm tra pass theo renderer đang dùng. Không dùng160 core checks làm bằng chứng renderer đã vẽ. Không thay C# nên không chạy lại core/compiler; **NOT_RUN Unity/shader compiler/render/touch**, theo yêu cầu. Chưa có ảnh xác nhận sau lần sửa pass này.

Vòng chân nhỏ là persistent khi Playing/Hero sống. Vòng tầm lớn và mũi tên như ảnh tham chiếu xuất hiện trong khi giữ/kéo ngắm; chúng được dọn sau khi thả cast/cancel. Ảnh sau cast đang cooldown không phải trạng thái để kỳ vọng vòng tầm lớn còn hiện.

---

## Gói 8: Đồng bộ thao tác ngắm và vòng chân theo Vongxuyen — 26/10/2026

### Tham chiếu và phạm vi

Đối chiếu source cục bộ `G:/ben-que-survival/src/main.js` (relic pointer handlers/beginRelicAim/smartAutoTargetRelic), `src/input/RelicTouchMath.js`, `src/ui/RelicAimHUD.js`, `src/combat/SkillIndicators.js`, `css/combat-mobile.css`. Không sửa source Vongxuyen hoặc khẳng định đây là revision remote mới nhất.

Bản trước mới nối gesture/cast, còn thiếu mô hình visual/input của nguồn: source mở ngắm ngay ở pointerdown, có thumb pad128px/thumb24px, drag threshold6px, full pull52px, minimum reach1.5m, cancel tròn cạnh nút và vòng chân thường trực. Unity trước đó dùng hold0.12/drag25/fullpull140 và hai LineRenderer target/line.

### Đã đổi

- Defense mở aim/cancel/thumb pad ngay khi nhấn. Shared gesture có mode `tapUntilDrag` riêng: preview hiện ngay nhưng nhấn/giữ không kéo vẫn dispatch quick tap. Legacy 2D giữ default tap/hold cũ, không đổi serialized fields/adapter của Legacy ở gói này.
- Hai thumb pad, thumb và cancel vòng tròn là child prefab được author. Pad neo tại screen press, thumb clamp52 screen pixels; pad128/thumb24 pixel bù Canvas scale. Cancel cạnh từng button, có X/HỦY, không còn bảng đỏ lớn giữa màn hình; vùng hit circle có padding12px.
- Trước khi kéo, preview hướng Hero ở min(3m,range×0.7). Sau drag6px: camera-relative XZ, distance=min(range,max(1.5m,range×pull)); full pull52px. Tap Dép chọn target gần nhất hoặc fallback hướng Hero min(5m,range×0.85) khi không có target, giống đường tap của source. Release vẫn yêu cầu Ground/cast gate/traversal hợp lệ.
- Presenter yêu cầu Motor hướng theo aim trong FixedUpdate; owner token ngăn một presenter clear hướng của owner khác. Pause/down/disable/reset clear facing. Không tìm component trong Update.
- Prefab indicator bổ sung range boundary, range fill mờ màu xanh và arrow hướng từ Hero. Vẫn giữ target circle khớp radius AoE thực tế của Dép; arrow chỉ là cue điều khiển, không thay AoE thành boomerang/line damage. Dash và Dép dùng definition range/radius hiện có.
- Prefab `DefenseHeroFootIndicator3D`: ring radius0.44m, inner disc0.36m và hướng trước chân đến0.52m, theo thông số source. Theo Hero/ground trong LateUpdate, ẩn khi down/gate đóng; không gắn vòng thường trực vào kỹ năng đang cooldown.
- Thêm foot prefab instance vào scene, bind Hero/Health và thêm participant thứ **18**. Scene chỉ thay composition participant list và SceneRoots, thêm3 block prefab/stripped refs; MapSongQue và bindings gameplay cũ giữ nguyên.
- Material/shader shape dùng quad có sẵn và mask procedural, không sinh ảnh/mesh/material/GameObject khi aim. Overlay shader riêng giữ ZTestAlways/ZWriteOff, hỗ trợ vertex tint cho UI và tint material/MPB cho quad. MPB cấp phát ở Awake, không mỗi frame. Shader target3.0 cho derivative AA; shader chưa được compiler Unity xác nhận.

### Kiểm chứng

- **160 core checks PASS:** 146 trước +14 cases cho immediate aim/tapUntilDrag, hold không kéo, quarter pull13/52, clamp, pointer ownership/cancel, minimum reach và camera-relative range. Các ca Legacy tap/hold cũ vẫn PASS.
- Runtime + source compatibility Editor/fixture compile PASS, ngoài Unity.
- YAML kiểm tra controls101 blocks, indicator21 blocks, foot17 blocks; local refs, decorations không nhận raycast, cancel positions/shapes, material property không trùng, scene overrides và18 participant đều PASS.
- **269 file trong snapshot giữ nguyên SHA256** ngoài phạm vi đổi được liệt kê. Scene map hierarchy, collider/NavMesh/stage và skill damage/cooldown definitions không đổi.
- Evidence: `Logs/TraversalQA/defense-vongxuyen-controls-*.txt`. Checker mới nhất: `Tools/Defense3DValidation/VerifyVongxuyenControls.py`. Snapshot: `Logs/DefenseVongxuyenControls_20261026_214558/`.

**NOT_RUN:** Unity/import, shader compilation/rendering, input touch thật, Rigidbody/ground/NavMesh và Android. Đây là port code/authoring dựa trên source; chưa xác nhận hình ảnh pixel-perfect hoặc feel trên thiết bị. Chưa thêm movement touch joystick/gamepad skill shortcuts và chưa port cơ chế boomerang/recast của Dép Vongxuyen. UI/indicator parity không có nghĩa combat parity toàn bộ.

---

## Sửa chỉ dấu ngắm không nhìn thấy — 26/10/2026

Ảnh người dùng cung cấp có vùng “THẢ ĐỂ HỦY DÉP”: gesture nhận pointer và đã vào Aiming. Vì vậy không kết luận mất EventSystem từ mô tả ban đầu.

Rà source thấy hai đường làm mất preview: projector thất bại surface query thì clear, và presenter chỉ Show khi effect.TryPrepare thành công. Đặc biệt giữ Dash chưa kéo có distance0, traversal từ chối nên không có cả vòng preview. Ngoài ra material gốc dùng depth test mặc định trên collider plane+0.05 trong khi visual GLB/scenery có thể cao hơn: đây là khả năng che khuất, chưa phải kết luận đã chứng minh bằng render.

Đã sửa:

- Preview giữ geometry hữu hạn và chuyển đỏ khi không có surface hợp lệ hoặc effect không prepare được; actor/gate không hợp lệ vẫn hide. Không tiêu cooldown trong preview.
- Release/cast tiếp tục gọi strict ground query + cast runtime/traversal; không cho phép cast vào điểm invalid chỉ vì marker được vẽ.
- Material indicator trỏ shader riêng `DefenseAimOverlay.shader`: queue3100, ZTestAlways, ZWriteOff, vertex tint/transparency; chỉ dấu đọc được qua terrain/props như overlay ngắm. Shared shader Legacy không đổi. Đây là lựa chọn hiển thị có chủ đích: marker có thể hiện xuyên vật che, không thay target geometry hoặc luật collision.
- Scene/hierarchy không đổi, không tạo object/material runtime. Snapshot `Logs/DefenseAimVisibilityFix_20261026_213850/`.

Runtime và source compatibility compile PASS; static material/shader/strict release guards và scene SHA256 PASS. Evidence `Logs/TraversalQA/defense-aim-visibility-fix-checks.txt`.

Không chạy Unity hoặc shader/render/physics/touch tests. Chưa thể khẳng định ảnh thực tế đã sửa hoàn toàn cho đến khi người dùng nạp source/material mới và quan sát. Không dùng 146 core checks lịch sử làm bằng chứng render.

---

## Gói 7: Gom MapSongQue thành một root quản lý — 26/10/2026

### Kết quả

Đổi root `[TREBRAVE] Map Song Que Defense 3D` thành **MapSongQue**, giữ `DefenseSceneInfo3D`/fileID và toàn bộ marker bindings. Root có năm nhóm:

- **Visuals:** GLB `TREBRAVE_MapSongQue_ThreeJS` vẫn là linked prefab, cùng `East_Arena_Visual`.
- **Collision:** hai bank Ground, ba bridge Ground và đối tượng nước cũ có cả mesh/collider.
- **GameplayMarkers:** enemy spawn và 10 tower socket marker.
- **Structures:** Base hiện có và nhóm 10 tháp đã author.
- **Navigation:** đối tượng NavMeshSurface cũ, đổi tên từ `Defense_Navigation`, giữ data/settings.

Hero/camera/light/UI/session/skill indicators và debug/run objects giữ ở ngoài map root. Không tạo map/UI object trong runtime; không unpack GLB hoặc merge mesh, không chạy generator rebuild scene. Đây là tổ chức scene để quản lý tập trung, chưa phải pipeline tự cập nhật collider/marker từ GLB.

### Phạm vi thay đổi và an toàn dữ liệu

- Root và bốn group mới đều identity TRS. Mọi local TRS cũ giữ nguyên; parent mới identity nên world pose của model, collider, marker, Base, tháp và Navigation không đổi. GLB vẫn giữ rotation Y180 cùng toàn bộ override hiện có.
- Thêm 4 GameObject group/4 Transform và 1 stripped Transform reference tới root GLB; không thêm component gameplay. Scene có **214 YAML blocks**, giữ tất cả ID cũ.
- Chỉ đổi tên root/map Navigation, liên kết cha/con, GLB instance parent và loại ba root cũ khỏi SceneRoots. Script, renderer, collider, NavMeshSurface settings và mọi binding UI/skill/session giữ nguyên byte trong scene.
- **247 file source/resource/model/NavMesh/metadata được bảo vệ giữ nguyên SHA256.** Không sửa C# runtime hoặc GLB/NavMesh asset; không đổi GUID.
- Snapshot trước thay đổi: `Logs/DefenseHierarchyConsolidation_20261026_212756/`.

### Kiểm chứng

`Tools/Defense3DValidation/VerifyMapHierarchy.py`: PASS. Kiểm tra duplicate YAML ID/key, mọi local ref, đúng group membership/parent, prefab overrides giữ nguyên, identity ancestry và local TRS, toàn bộ block ngoài whitelist giữ nguyên byte, protected file hashes và các scene roots ngoài phạm vi.

Bằng chứng: `Logs/TraversalQA/defense-map-hierarchy-static-checks.txt`.

Không chạy lại 146 core checks vì không thay logic C#; kết quả gói 6 là bằng chứng lịch sử của logic đó. **NOT_RUN:** Unity/import/rendering/physics/NavMesh bake/test runner, theo yêu cầu không kiểm thử trong Unity. Không suy diễn việc gom hierarchy đã được Unity nghiệm thu từ kiểm tra YAML.

Lưu ý authoring: giữ map root identity; di chuyển/scale cả map không tự cập nhật tọa độ spawn hoặc baked NavMesh. `Collision` vẫn cần active; chỉ tắt renderer nếu muốn ẩn proxy. Khi sửa hình học GLB, tiếp tục cập nhật collider/marker/NavMesh tương ứng.

---

## Gói 6: Nối UI → chỉ dấu → executor dash/Dép trong scene — 26/10/2026

### Trạng thái mới nhất

| Ticket | Trạng thái | Kết quả / phần còn lại |
|---|---|---|
| RF3D-011 | SKILL_SNAPSHOTS_ADDED / PARTIAL | Dash/Dép có SO authoring riêng, capture immutable definition cho UI/preview/effect/cooldown. Hero profile tổng vẫn chưa hoàn thiện |
| RF3D-012 | POINTER_SKILL_REQUESTS_WIRED / PARTIAL | UI release → typed session/generation cast request; movement/B giữ channel riêng. Chưa có remappable InputActionReference, gamepad skill submit hoặc joystick touch di chuyển |
| RF3D-014..017 | SCENE_WIRED / STATIC_CHECKED / NOT_ACCEPTED | Hai control, cancel zones riêng, cooldown radial/TMP/lock, projector và hai preview indicators đã có trong prefab/scene. Chưa kiểm chứng EventSystem/touch/graphics |
| RF3D-018 | BASIC_DASH_RELIC_CODE_WIRED / NOT_ACCEPTED | Basic giữ auto attack. Dash có timed Rigidbody path, Dép có point AoE session targets và impact ring; quyền cooldown duy nhất ở ability runtime. Chưa chứng minh physical traversal hoặc effect hit trong scene |
| RF3D-019 | DESIGN_PENDING | Không tự thêm signature Thư Sinh |

### Đường nối production

`Authored pointer control → shared gesture session → scoped aim release → DefenseSkillPresenter3D → DefenseSkillCastRuntime → prepared effect → cooldown commit → effect execution`.

- `DefenseSceneCompositionRoot` validate và bind hai presenter. Scene có **17 participant** (15 cũ + hai effect component trên Hero); runtime cast/ability được session sở hữu. Không tạo thêm session hoặc cooldown song song với model cũ.
- `DefenseSkillDefinitionSO` và snapshot giữ cùng range/radius/damage/cooldown/duration/tap-target policy cho presenter, renderer và executor. Asset thay đổi trong trận không tự thay snapshot; tạo session mới để đọc lại.
- Cast gate chặn actor không sẵn sàng, modal/terminal, session khác, generation cũ, origin đã dịch chuyển, nonfinite/out-of-range và duplicate/reentrant cast. Effect prepare phải thành công trước cooldown commit. Prepare bị chặn không tốn cooldown; effect ném lỗi sau commit vẫn giữ cooldown để tránh lặp miễn phí.
- Presenter format cooldown theo giây khi giá trị hiển thị đổi; view chỉ nhận text/fill/lock. Nút khóa khi down, inactive, gate đóng hoặc cooldown. Cả renderer preview và executor dùng cùng prepared endpoint.

### Dash

`Skill_Dash.asset`: range **4.5m** (default aim Dash từ 2D), cooldown **3s**, duration **0.25s** (prototype combat core), preview radius **0.5m** khớp Capsule Hero hiện tại. Đây là profile prototype có thể chỉnh trên asset, không phải balance đã nghiệm thu.

`DefenseDashTraversal3D` dùng capsule sweep/overlap, bước lấy mẫu Ground ở tâm/hai mép chân, giới hạn slope/floor step và NavMesh raycast. Mask scene: Ground=4096, blockers Default/Obstacle/Water=25, bỏ trigger; yêu cầu baked NavMesh. Clip trước vật cản/mép, không lấy endpoint bên kia sông làm bằng chứng path an toàn. Không có NavMesh/surface hợp lệ thì từ chối cast.

Motor nhận điểm đã chuẩn bị, di chuyển theo session time trong FixedUpdate, kiểm tra lại từng đoạn khi đi để phản ứng vật cản mới. Pause/modal/down/disable/reset hủy phần dash còn lại; không thêm invulnerability. Đây là implementation đã compile, **chưa được chạy physics/NavMesh**.

### Dép Tổ Ong

`Skill_DepToOng.asset`: range **10m**, radius **3.5m**, damage **180**, cooldown **12s**, impact ring **0.2s**. Giữ MVP point AoE từ prototype; chưa thêm boomerang/quỹ đạo/projectile hoặc status.

Quick tap tìm target gần nhất theo mask; không có target thì không cast. Drag cho phép chọn điểm mặt đất trong tầm. `DefenseTargetIndex.TryCopyWithin` chụp các target trong vùng vào buffer instance trước damage callback: không hit trùng do nhiều collider, không bỏ sót phần tử do target chết/unregister trong vòng lặp. Buffer 256 target, nếu đầy thì từ chối toàn cast, không tiêu cooldown hoặc cắt hit âm thầm. Snapshot này là query XZ, áp dụng mask Ground/Air.

Damage giữ session/generation và kiểm tra gate mỗi hit. Impact ring là instance đã author, chỉ show/hide; không Instantiate/Destroy VFX. Restart/shutdown clear state/buffer. Damage relic dùng profile riêng, không mặc định nhân modifier basic attack.

Cooldown mới dùng chung session clock: pause/modal dừng, down vẫn trôi như auto attack đang nối; không dùng lại cơ chế Tick/cooldown độc lập của hai prototype class cũ. `DefenseHeroCombatState`/`DefenseDepToOngRelic` còn cho compatibility/fixture, production không khởi tạo chúng.

### Authoring trong Hierarchy

- `DefenseSkillControlsCanvas` là prefab instance riêng, sorting order **90**, dưới Match UI/modal **100**. Hai button dùng sprite có sẵn từ 2D (`Btn_Dash_PhiVan`, `Btn_Circle_Relic`), radial fill, TMP cooldown/label, lock overlay và hai cancel zones riêng. Mọi décor tắt raycastTarget, chỉ button root nhận pointer.
- Hero có projector với Camera/Origin/Ground refs và hai effect + dash traversal; tất cả ref được serialize.
- Scene có `DashAimIndicator`, `DepAimIndicator`, `DepImpactIndicator`, cùng dùng prefab indicator gói 5; không sinh child/material lúc aim/cast.
- Dọn ba override scale=0 cũ trên instance `DefenseMatchUICanvas` về 1 để binding/layout dùng scale hợp lệ.

**33 resource file cũ và 113 metadata file giữ nguyên byte.** Thêm ba asset: controls prefab + hai definition SO. Scene chỉ thay Hero component list, composition bindings, ba Canvas scale overrides và thêm bốn prefab roots (controls + 3 indicators). Có 17 YAML block mới; collider/map/NavMesh/GLB/stage/balance asset cũ giữ nguyên. Snapshot: `Logs/DefenseRefactorStep6_20261026_210120/`.

### Kiểm chứng

- **146 kiểm tra C# độc lập PASS**: 23 lifecycle/spawn + 35 combat + 27 input/ability + 39 gesture/aim + **22 cast/area**.
- Cast/area checks: foreign/old scope, stale origin, invalid endpoint, no-cost failed prepare, exact prepared endpoint, cooldown commit/duplicate/reentrancy, preparation pause/restart, throwing effect, area masks/stable snapshot/saturation.
- Runtime và source compatibility (runtime + Editor tools + fixture liên quan) biên dịch độc lập thành công. Không thực thi Unity fixture.
- YAML 75-block controls prefab và 205-block scene parse không trùng ID/key; local refs và mọi prefab override target hợp lệ. Hai presenter nối đủ button/health/origin/projector/effect/indicator. Static checker: `Tools/Defense3DValidation/VerifyRefactorStep6.py`.
- Evidence: `Logs/TraversalQA/defense-refactor-step6-*.txt`; runner `Tools/Defense3DValidation/RunCoreChecks.ps1`.

**NOT_RUN:** Unity import/test runner, EventSystem/touch, rendering/shader, thực thi dash/raycast/NavMesh, damage effect trong scene hoặc Android. Đây là nối code/asset và kiểm chứng ngoài Unity, chưa phải nghiệm thu chơi thực tế hoặc hiệu năng. Keyboard/gamepad skill shortcuts và touch movement chưa được thêm.

---

## Gói 5: Gesture dùng chung, aim XZ và prefab chỉ dấu — 26/10/2026

### Trạng thái mới nhất

| Ticket | Trạng thái | Kết quả / phần còn lại |
|---|---|---|
| RF3D-014 | CORE_AND_ADAPTERS_IMPLEMENTED / NOT_ACCEPTED | Tap/hold/drag/cancel core dùng thật bởi adapter Legacy và Defense. Defense có session/ability ownership và cancel refs; chưa nối control vào scene hoặc kiểm chứng touch |
| RF3D-015 | GEOMETRY_CHECKED / PROJECTOR_SOURCE_COMPILED / NOT_ACCEPTED | Immutable resolved aim XZ, camera-relative drag, range clamp và projector ground mask/slope/height. Raycast chưa được thực thi; executor chưa tiêu thụ result |
| RF3D-016 | PREFAB_AUTHORED / NOT_SCENE_WIRED | Có prefab vòng target và đường hướng, material dùng shader URP có sẵn. Chưa nghiệm thu hình ảnh hoặc preview/effect parity |
| RF3D-017..018 | NEXT | Author button skin/presenter, bind indicator và nối dash/Dép với command/ability core |
| RF3D-019 | DESIGN_PENDING | Signature Thư Sinh vẫn chờ quyết định thiết kế |

### Mã và reuse từ 2D

- `SkillPointerGesture` là core C# không UnityEngine, có Press/Move/Tick/Release/Cancel. Giữ hành vi tap nhanh, hold 0.12s hoặc drag 25px, pull tối đa 140px; các threshold cấu hình được. Một pointer sở hữu gesture, pointer khác không thể release/steal; release/cancel xóa state trước callback. Không coroutine/WaitForSeconds hoặc cấp phát theo frame.
- `SmartSkillDragHandler` Legacy đã chuyển sang core này, giữ tên field serialized, GUID, event và public API. Bỏ coroutine hold; pointer-up đánh giá lại delta/hold nếu thiếu drag event; ignore right/middle click, cancel khi background/mất focus/customization/disable. Cancel chỉ phát khi thật sự có pointer, tránh duplicate cancellation. Adapter Legacy còn giữ `UICancelSkillZone.Instance` và customization gate để tương thích; core và Defense không phụ thuộc chúng. Không đổi scene/prefab Legacy, nhưng hành vi callback mới chưa được chạy trong Unity.
- `DefenseSkillGestureSession` reserve một ability khi press, chặn gesture thứ hai dùng cùng ability, reset pointer khi pause/modal/down/unavailable/restart/terminal/dispose. Payload mang session identity + run generation. Gesture chỉ yêu cầu cast; không tiêu cooldown, quyền commit thuộc executor sau validation. Ability thuộc session khác bị từ chối.
- `DefenseAimResolver` trả cùng một `DefenseResolvedAim` gồm Origin/Target/Direction/Distance; tính ngang bằng double để tránh overflow, clamp tầm và pull, reject nonfinite/camera axes bị collapse. Không đưa Vector2 XY của skill 2D vào world3D ngầm.
- `DefenseAimProjector3D` nhận Camera/Origin/GroundMask được author; không Camera.main hoặc global find. Drag dùng basis camera chiếu XZ; screen-point raycast clamp rồi probe lại endpoint. Target lấy độ cao mặt đất và slope hợp lệ, bỏ trigger. Đây chỉ kiểm tra endpoint, không xác nhận đường lướt đi qua cầu/sông/vật cản.
- `DefenseSkillAimButton3D` là adapter pointer/aim unbound thì fail closed. Nhận ref projector/Hero Health/cancel zone, bind context+ability+range+tap policy rõ ràng. Có PreviewChanged/PreviewCleared/AimReleased với scope; không thực thi effect hoặc tự tạo UI. Tap auto-target yêu cầu có target hợp lệ; tap không auto-target dùng hướng Hero. Callback preview pause/restart/rebind sẽ làm release cũ bị chặn.

### Prefab mới và phạm vi authoring

`Assets/Resources/Defense3D/DefenseAimIndicator3D.prefab`: root view + TargetRing 64 điểm đơn vị XZ + DirectionLine 2 điểm; hai renderer ẩn sẵn. `DefenseAimIndicator3D` chỉ đổi transform/line endpoints/tint, không tạo GameObject, material hoặc vertex array lúc aim. Material trắng dùng lại `Assets/Shaders/VFX/URP_VFX_Ink_AlphaBlend.shader` vì shader này hỗ trợ vertex color/transparency; shader gốc không đổi.

**33 scene/resource file cũ và 114 metadata file giữ nguyên byte** so với snapshot `Logs/DefenseRefactorStep5_20261026_204538/`. Prefab/material indicator và các source mới có GUID riêng. Chưa thêm instance indicator/nút mới vào scene; không tự thêm nút dash/Dép chưa có executor.

### Kiểm chứng và giới hạn

- **124 kiểm tra C# độc lập PASS:** 23 lifecycle/spawn + 35 combat + 27 input/ability + **39 gesture/aim**.
- Bao phủ tap/hold/drag/cancel, pointer ownership, duplicate up, threshold trên release, clock regression, NaN/extreme input, cancel một lần, ability reservation, foreign session, modal/down/reset/terminal, release scope và XZ camera/range math.
- Runtime source (kèm shared gesture/Legacy adapter) và source compatibility với Editor tools/fixture liên quan biên dịch thành công. Không thực thi fixture Unity.
- Kiểm tra YAML: 9 ID duy nhất, tất cả local refs hợp lệ, ring 64 điểm local-space và line 2 điểm world-space; renderer ẩn mặc định, material/shader ref hợp lệ. Static source boundary/asset/meta checks PASS.
- Evidence: `Logs/TraversalQA/defense-refactor-step5-*.txt`. Chạy lại core bằng `Tools/Defense3DValidation/RunCoreChecks.ps1`; static checker `VerifyRefactorStep5.py`.

**NOT_RUN:** Unity/import/test runner, shader compilation/rendering, callback EventSystem và touch thật, ground raycasts, physics/NavMesh hoặc Android. Kết quả standalone không chứng minh Legacy UI hoặc preview 3D đã hoạt động đúng trong scene. RF3D-014..016 chưa được nghiệm thu.

### Tiếp theo

Author skill button skin/control prefab, presenter kết nối button → indicator, rồi bind scene composition và effect executor dash/Dép. Presenter và executor phải dùng cùng resolved aim và definition snapshot; không tạo thêm cooldown owner. Cần kiểm tra đường dash riêng, không dùng ground endpoint validity thay traversal validation.

---

## Gói 4: Input command và ability core — 26/10/2026

### Trạng thái mới nhất

| Ticket | Trạng thái | Kết quả / phần còn lại |
|---|---|---|
| RF3D-012 | PARTIAL / CURRENT_MOVEMENT_BUILD_WIRED | Motor và tower builder nhận command qua interface, không đọc thiết bị. Chưa có touch/ability command routing hoặc InputActionReference được author |
| RF3D-013 | CORE_IMPLEMENTED / BASIC_ATTACK_WIRED / NOT_ACCEPTED | Một runtime sở hữu Ready/Aiming/Cooldown cho mỗi ability. Basic attack đã dùng thật; dash/relic chưa nối |
| RF3D-014..018 | NEXT | Gesture/cancel, resolved aim XZ, authored indicator/button prefabs, Defense presenters và executor dash/Dép |
| RF3D-019 | DESIGN_PENDING | Signature Thư Sinh cần quyết định thiết kế như kế hoạch gốc |

### Luồng đã nối

`Keyboard/Gamepad → DefenseHeroDeviceInput → DefenseHeroCommands → Motor / Tower Builder`.

- Composition root tạo service C# và inject qua danh sách participant đã author. Session sở hữu/dispose command channel; không thêm GameObject, MonoBehaviour hoặc control runtime.
- Adapter thiết bị dùng New Input System, giữ WASD/phím mũi tên/left stick và B như trước. Motor chỉ nhận movement XZ đã normalize, builder consume build một lần trong frame. Đây chưa phải bộ action map có thể remap trên Inspector.
- Input bị chặn khi chưa Start, Paused/LevelUp/terminal, Hero down/inactive, mất focus/background; cleanup/reset/dispose xóa dữ liệu. Hero defeat xóa command ngay qua event, tránh giữ movement đến frame kế tiếp.
- Movement giữ mẫu gần nhất cho FixedUpdate; request build được thay bằng mẫu frame mới và chỉ consume một lần, không giữ hàng đợi xuyên frame hoặc modal. UI input chưa được trộn với device adapter.
- `DefenseAbilityRuntime` dùng duy nhất session gameplay time, không coroutine hoặc clock riêng. BeginAim/CancelAim/TryActivate cung cấp state chung; cancel không tiêu cooldown, gate đóng hủy aim, restart reset, dispose khóa activation.
- Basic attack bỏ field `nextAttackAt`; cooldown do ability runtime sở hữu. Chỉ commit khi đã tìm thấy target, trước callback damage để ngăn cast lặp. Auto attack/range/damage/cooldown author và modifier vẫn giữ như trước.
- Cooldown đóng băng trong pause/modal do session clock dừng. Khi Hero down, clock session vẫn chạy và cooldown vẫn trôi, giữ hành vi auto attack cũ; chính sách cooldown down cho skill mới vẫn cần quyết định.

### Scene và kiểm chứng

**33 scene/resource files và 103 metadata files trong snapshot giữ nguyên byte.** Không đổi hierarchy, prefab UI, balance hoặc NavMesh; ba source mới có GUID riêng. Snapshot: `Logs/DefenseRefactorStep4_20261026_203528/`.

- **85 kiểm tra C# độc lập PASS:** 23 lifecycle/spawn + 35 combat + 27 input/ability. Bao gồm invalid/overflow input, session isolation, consume một lần, modal/reset/terminal, cancel không tốn cooldown, cooldown độc lập và exact-time boundary.
- Runtime Defense3D và source compatibility runtime + Editor tools + fixture liên quan biên dịch độc lập thành công. Không thực thi fixture Unity.
- Static boundary/asset/meta checks PASS. Runner bền vững: `Tools/Defense3DValidation/RunCoreChecks.ps1`; asset/source checker: `VerifyRefactorStep4.py`.
- Evidence: `Logs/TraversalQA/defense-refactor-step4-DefenseSessionChecks.txt`, `defense-refactor-step4-DefenseCombatChecks.txt`, `defense-refactor-step4-DefenseAbilityInputChecks.txt`, `defense-refactor-step4-static-checks.txt` và hai compile logs cùng prefix.

**NOT_RUN:** Unity Editor/import pipeline/test runner, device/touch, physics/NavMesh, rendering và Android. Đây chưa phải nghiệm thu P3 hoặc chứng minh input scene chạy đúng.

### Bước tiếp theo

Tách gesture/cancel core từ hành vi 2D, bổ sung resolved aim/projector XZ và author indicator/button prefab với serialized refs. Sau đó nối dash và Dép qua cùng command/ability runtime. Chưa port các presenter 2D phụ thuộc PlayerProvider/Physics2D; chưa tạo skill effect, dash hoặc signature mới trong gói này.

---

## Gói 3: Damage, target theo session và stat modifiers — 26/10/2026

### Trạng thái mới nhất

| Ticket | Trạng thái | Kết quả / phần còn lại |
|---|---|---|
| RF3D-008 | DAMAGE_CODE_IMPLEMENTED / STATUS_CONTRACT_ONLY / NOT_ACCEPTED | Base/Hero/Enemy/generic adapter dùng chung damage rules và health core; đã có boundary request status XZ. Chưa có executor stun/slow hoặc status Unity adapter |
| RF3D-009 | CODE_IMPLEMENTED / STATIC_CHECKED / NOT_ACCEPTED | Target index thuộc từng session, query theo XZ, tie-break bằng thứ tự đăng ký và mask Ground/Air. Movement/anti-air role thực tế vẫn là RF3D-021 |
| RF3D-010 | CURRENT_UPGRADES_WIRED / NOT_ACCEPTED | Sáu loại modifier hiện có được session sở hữu và actor đọc thật. Chưa chuyển toàn bộ Hero/ability balance sang profile chung |
| RF3D-011 | PARTIAL | Enemy scalar/prefab-reference snapshots được capture lúc compile stage; source SO không còn được đọc lại cho từng production spawn. Hero/ability profiles còn theo gói tiếp theo |
| RF3D-012..019 | NEXT | Input command path, ability runtime, gesture/aim và UI controls tái sử dụng từ 2D |

### Thay đổi mã

- Tách damage payload core khỏi phần Vector3 của Unity bằng partial struct. Caller Unity giữ constructor/property tương thích; health/target/stat checks chạy được không cần UnityEngine.
- Receiver chặn damage khi chưa bind, Paused/LevelUp/terminal, target dead, request không finite, session khác hoặc generation cũ. Các nguồn Hero/Tower/Enemy/Boss hiện tại đóng dấu session + generation. Wrapper ReceiveDamage(float) giữ tương thích để gọi vào receiver đang active; không được dùng làm đường phát projectile cũ không có scope.
- `DefenseHealthCore` sở hữu clamp/death latch và life version. Damage lặp/reentrant không phát death hai lần; reset trong callback không để death của lượt health cũ tác động lượt mới.
- Hero tracker dùng cùng health core, vẫn giữ Down/respawn 5 gameplay seconds và không gây LOSS trực tiếp. Sửa terminal respawn cancellation: tick sau đó không được vô tình hồi sinh nếu chưa Reset.
- `DefenseTargetIndex` là service C# riêng cho mỗi session; adapter Unity không còn HashSet global/RuntimeInitializeOnLoad reset toàn cục. Ties bằng thứ tự registration; filter invalid/dead/unavailable và Ground/Air.
- Factory nhận index tường minh và kiểm tra index thuộc đúng gameplay context. Target lifecycle gắn vào actor enable/disable/destroy và session cleanup; không tìm registry toàn scene.
- `DefenseRunStats` là ledger duy nhất của session. UpgradeDeck bind ledger này; UI reset không reset ledger lần nữa. Reset modifier do session sở hữu.
- Hero damage/attack interval/movement, Tower damage/range và Base max HP đọc ledger. Chỉ số gốc vẫn lấy từ các field đã author; không đổi giá trị balance hiện có sang các constants của core chưa nối.
- Tốc đánh tác động cooldown của đòn tiếp theo; không reset đòn đang cooldown để tạo thêm hit miễn phí. Tốc chạy/range/damage đọc giá trị mới khi actor xử lý lần kế tiếp.
- Thẻ Base +200 HP là grant capacity phẳng: tăng MaxHP và thêm đúng lượng capacity mới vào CurrentHP, giữ lượng HP đang thiếu. Không hồi sinh Base đã chết.
- Stage compile capture `DefenseEnemySpawnDefinition`: ID, scalar stats và prefab reference. Snapshot kiểm tra ID/catalog key, finite stats, reward và locomotion enum. Source asset chỉ giữ cho authoring/compatibility, không làm nguồn scalar lúc spawn trong production.

### Scene và asset

Scene chỉ thay danh sách `_sceneParticipants` của composition root: từ 13 thành **15**, thêm hai component Base Health/Hero Health đã tồn tại. Không tạo thêm GameObject hoặc health component trong runtime.

**32 prefab/data/material asset giữ nguyên byte**, **122 GUID trong phạm vi snapshot gói này giữ nguyên**. Không chỉnh balance asset, prefab visual hoặc bake NavMesh.

Snapshot trước gói: `Logs/DefenseRefactorStep3_20261026_193213/`.

### Kiểm chứng

- **58 kiểm tra C# độc lập PASS:** 23 lifecycle/spawn + 35 health/damage scope/target/stat/status-boundary checks.
- Bao phủ NaN/Infinity/invalid damage, death exactly-once/reentrant reset, capacity không revive, terminal respawn cancel, session/generation isolation, target masks/tie-break, stacking/reset và tiêu thụ modifier.
- Toàn bộ source runtime Defense3D biên dịch độc lập thành công.
- Source compatibility compile của runtime + Defense Editor tools + các fixture Defense/Stage/Traversal liên quan cũng thành công. Đây là kiểm tra biên dịch, **không thực thi fixture Unity**; các fixture runtime cũ vẫn cần migrate lifecycle/binding trước khi được chạy lại.
- Kiểm tra YAML/ID/binding/asset hashes/GUID và đường gọi snapshot/stat/gate: PASS.

Bằng chứng: `Logs/TraversalQA/defense-refactor-step3-session-checks.txt`, `defense-refactor-step3-combat-checks.txt`, `defense-refactor-step3-static-checks.txt`.

**NOT_RUN:** Unity Editor/import/compile pipeline, EditMode/PlayMode, physics/NavMesh, touch/graphics và Android. Chưa chứng minh modifier hiển thị hoặc skill effect đúng trong scene từ các kết quả core.

Giới hạn: status mới có contract/validation; chưa có stun/slow thực. Physical EXP pickup và quyền sở hữu progression/offer vẫn chưa hoàn thiện, nên không coi toàn bộ vòng kill→EXP→thẻ đã hoạt động. Pooling và các Enemy locomotion/aggro strategy chưa triển khai. Skill/aim/UI 2D chưa được port ở gói này.

---

## Gói 2: Tách runtime services và ownership khi shutdown — 26/10/2026

### Trạng thái hiện tại

| Ticket | Trạng thái | Kết quả |
|---|---|---|
| RF3D-006 | IN_PROGRESS / current-owner cleanup implemented | Session shutdown đã cleanup actor/pickup và dispose service; xử lý lỗi theo từng owner, không bỏ qua owner còn lại. Pool lease hoàn chỉnh và registry theo session thuộc các gói tiếp theo |
| RF3D-007 | CODE_SPLIT_COMPLETE / STATIC_CHECKED / NOT_ACCEPTED | Run là facade giữ serialized fields/public API. Geometry, spawn/entity lifetime, encounter, frame/result và debug rendering đã tách riêng. Khởi tạo service vẫn nằm trong InitializeSession của facade như bước chuyển tiếp để giữ binding hiện có |
| RF3D-022 | PARTIAL | Spawn chỉ commit sau factory thành công; job lỗi giữ identity/TTL/capacity và retry có giới hạn. Không coi đây là nghiệm thu navigation/wave scene |
| RF3D-008..011 | NEXT | Chuẩn hóa damage/status, target index theo session, stats/modifier và definition snapshot |

### Ranh giới mới

| Thành phần | Trách nhiệm |
|---|---|
| `DefenseRun6003D` | Facade scene: giữ reference/public read API, nối service, chuyển tiếp frame/result. Không truy vấn NavMesh, tạo/track Enemy hay tự vẽ debug |
| `DefenseSpawnPlacement3D` | Resolve zone, Hero exclusion, NavMesh và path. Spawn sử dụng chính kết quả vừa được kiểm tra, không query lần hai |
| `DefenseSpawnRuntime` | Scheduler/planner execution, factory, actor removal subscriptions, cleanup generation, retry diagnostics |
| `DefenseYakshaEncounter3D` | Arena → assault theo thời điểm spawn thực; đăng ký/hủy Boss events; không tự quyết WIN/LOSS |
| `DefenseMatchRuntime` | Thứ tự frame, respawn delta, result publication và round reset; chỉ đọc luật kết quả của session |
| `DefenseRunDebugOverlay` | Vẽ diagnostics tùy chọn; không sở hữu gameplay state |
| `DefenseSession` | Scope tài nguyên toàn session; round cleanup khác final service disposal |

### Hành vi được sửa

- `Session.Dispose` đóng gate, thử cleanup tất cả owner/observer và dispose service đã own. Lỗi được tổng hợp sau khi các bước còn lại đã được thử; Dispose lặp không release lần hai.
- Thêm `RunCleanup` cho cả restart và shutdown. Wood pickup cũ bị invalidate/disable/release ngay tại cleanup, thay vì chỉ bị hủy sau reset.
- Một gate observer lỗi không ngăn các actor khác nhận lệnh dừng. Nếu cleanup lỗi, restart không mở lại Playing hoặc tăng generation.
- Spawn scope gỡ callback `Removed` trước khi Destroy deferred. Callback cũ hoặc callback reentrant không giảm cap của generation mới.
- Factory thất bại không làm mất job và không chiếm active slot. Mỗi frame giới hạn cả số factory attempt; retry chờ một gameplay second, không gia hạn TTL; log lỗi đầu theo job.
- Tạo actor thất bại sẽ disable/release actor đã tạo một phần. Boss thiếu warning renderer được từ chối trước khi encounter bắt đầu.
- Boss defeat được encounter ghi nhận; match resolver kiểm tra cùng Base HP tại bước resolve tiếp theo. Không chốt WIN ngay trong callback Boss chết, để Base destruction trong cùng frame có cơ hội nhận ưu tiên LOSS.
- Tách spawn job/schedule contracts khỏi Unity-dependent plan để scheduler/planner có thể kiểm chứng bằng C# độc lập.

### Kiểm chứng gói 2

- **23/23 kiểm tra C# độc lập PASS**: các invariant gói 1, round cleanup vs shutdown, service ownership, cleanup/gate observer failure, failed/throwing factory, retry identity, attempt budget, original TTL và scheduler không spawn trùng.
- Toàn bộ source runtime Defense3D biên dịch độc lập thành công với reference DLL hiện có.
- **33 scene/prefab/data/material asset byte-identical** so với snapshot gói 2; **92 GUID trong phạm vi snapshot này giữ nguyên**. Gói này không sửa file scene/prefab.
- **NOT_RUN:** Unity Editor/import/compile, EditMode/PlayMode, NavMesh/physics/graphics và Android. Kiểm tra C# không chứng minh hành vi các adapter Unity trong scene.

Snapshot: `Logs/DefenseRefactorStep2_20261026_190213/`.

Bằng chứng: `Logs/TraversalQA/defense-refactor-step2-core-checks.txt`, `Logs/TraversalQA/defense-refactor-step2-static-checks.txt`.

Giới hạn vẫn còn: pool chưa triển khai, target registry còn static, API damage/status/stat chưa thống nhất; EXP/UpgradeDeck vẫn ở Presenter và skill/aim/UI controls 2D chưa port. Các fixture Unity cũ cần cấp session mới, không lấy kết quả QA cũ để đánh PASS phiên bản hiện tại.

---

## Gói 1: Baseline và session/gameplay gate — 26/10/2026

Phạm vi lượt này: bắt đầu P0/P1 của `DEFENSE3D_COMPREHENSIVE_REFACTOR_PLAN.md`. Chưa triển khai toàn bộ 32 ticket; chưa chuyển skill/aiming/UI controls 2D sang Defense.

### Trạng thái

| Ticket | Trạng thái thực tế | Bằng chứng / phần còn lại |
|---|---|---|
| RF3D-001 | BASELINE_CAPTURED | 203 file được kiểm kê; code, scene, NavMesh, prefab/data và meta được copy. GLB chỉ hash trong snapshot này, không coi đó là một bản backup GLB mới. Có Git HEAD/branch/status và SHA-256 manifest |
| RF3D-002 | OWNERSHIP_AUDITED | Bảng owner và core chưa nối bên dưới; không loại bỏ class chỉ vì chưa có caller |
| RF3D-003 | BASELINE_RECORDED / PRODUCT_CHOICES_PENDING | Giữ 600s, LOSS ưu tiên, respawn 5 gameplay s, Stage/FastSpawn và hành vi auto-attack hiện có. Bốn quyết định skill trong kế hoạch vẫn chưa chốt |
| RF3D-004 | DONE_CODE / STATIC_CHECKED / NOT_ACCEPTED | Thêm composition root author sẵn trong scene; inject session/UI commands/13 participant, fail closed khi thiếu binding |
| RF3D-005 | DONE_CODE_FOR_CURRENT_ACTORS / NOT_ACCEPTED | Hero motor/attack, tháp/build, Enemy attack/NavMesh, warning Boss và Wood pickup dùng gate/time chung. Chưa kiểm chứng vật lý/hiển thị; ability/status tương lai phải áp cùng contract |
| RF3D-006 | IN_PROGRESS | Đã đóng gate trước cleanup, reset wallet/UI/tower/cooldown và vô hiệu pickup lượt cũ. Chưa có pool lease hoàn chỉnh, chưa dọn hết ownership của mọi hệ thống |
| RF3D-007 | IN_PROGRESS | Run không tự tạo lifecycle hoặc tìm scene refs; vẫn còn spawn safety, encounter và debug render cần tách tiếp |
| RF3D-008..032 | PLANNED | Không tự đánh hoàn thành từ các thay đổi nền |

### Baseline local

`Logs/DefenseRefactorBaseline_20261026_184301/`

- `manifest.json`: đường dẫn, hash, kích thước và có copy hay chỉ hash.
- `git-head.txt`, `git-branch.txt`, `git-status.txt`: working tree đã có nhiều thay đổi local từ trước; không reset, stash hoặc ghi đè bằng remote.
- Bản sao file nằm dưới đường dẫn tương ứng `Assets/...` trong snapshot.

Kiểm tra sau thay đổi: **104 GUID cũ giữ nguyên**; **11 GLB/data/NavMesh asset giữ nguyên hash**. Trong scene, các block cũ chỉ thay SceneRoots để thêm composition root; không thay transform bản đồ/cầu/socket hoặc bake NavMesh.

### Owner/runtime sau gói 1

| Trạng thái | Owner hiện tại | Hướng tiếp theo |
|---|---|---|
| Session lifetime | `DefenseSceneCompositionRoot` | Chỉ validation, construct, bind, dispose; không chuyển logic gameplay vào root |
| State/clock | `DefenseSession` sở hữu `DefenseG1RuntimeAdapter`/`DefenseRunLifecycle` cũ | Không thêm FSM/clock song song |
| State commands từ UI | `IDefenseSessionCommands` | UI dùng OpenLevelUp/Resume/Restart; không truy cập sâu FSM |
| Quyền actor chạy và mốc cooldown | `IDefenseGameplayContext` | Read-only; actor chưa bind sẽ không chạy gameplay |
| Bind/unbind của actor | `DefenseGameplayBehaviour` | Dispose subscription đúng lifetime; không tạo GameObject hoặc component |
| Spawn schedule/cap và entity cleanup | `DefenseRun6003D` nối scheduler/planner/factory | Tách safety/spawn runner ở RF3D-007/022 |
| Enemy HP/di chuyển/tấn công | Enemy3D/GroundMovement3D/Attack3D và Walker adapter | Tách movement/aggro strategy, damage/status contract ở P2/P4 |
| EXP/UpgradeDeck | Vẫn do `DefenseMatchPresenter` sở hữu tạm thời | Chuyển sang progression ở RF3D-025; không coi là MVP hoàn chỉnh |
| Gỗ | Hero Wallet, factory inject collector vào drop | Reward pipeline/pickup services và pooling ở RF3D-024/027 |
| Tower occupancy | Builder và 10 authored instances | Nối TowerSocketCore và build/upgrade/repair commands ở RF3D-026 |

Core chưa thấy caller production ngoài khai báo tại thời điểm audit: `DefenseHeroCombatState`, `DefenseDepToOngRelic`, `DefenseTowerSocketCore`, `DefenseRewardPipeline`, `DefenseYakshaBehaviorFSM`. Không tự xoá hoặc coi các class này đã tích hợp.

### Thay đổi cụ thể

- Thêm root `[TREBRAVE] Session Composition` trong file scene, nối rõ Run/UI/Wallet và **13 participant**: Hero Motor, Hero AutoAttack, Tower Builder, 10 tháp đang inactive.
- Root khởi tạo trước adapter/UI Start. Run update sớm để actor đọc cùng clock; Rigidbody FixedUpdate chỉ dùng gate, không cộng clock thứ hai.
- Factory/Boss inject context trước khi cấu hình movement/attack/arena. Spawn không có context báo lỗi cấu hình, không tự tìm session hoặc tạo một session phụ.
- Hero không đánh khi Down; motor dừng horizontal velocity khi gate đóng/disable. NavMesh agent nhận isStopped theo gate. Đây là thay đổi mã, chưa phải bằng chứng scene physics đã đạt yêu cầu.
- Cooldown attack/tháp/quái, warning Boss, delay/expiry pickup dùng gameplay elapsed. Không dùng Time.time/Time.timeScale để thay thế gate của session.
- UI chuyển vào đúng phase LevelUp, chống chọn lại card cũ, gọi session commands. Restart do session điều phối; reset model UI hiện hữu là bước chuyển tiếp, không phải kiến trúc progression cuối cùng.
- Restart đóng gate trước cleanup; entity cũ tắt ngay trước Destroy deferred; reset generation và signal reset trước khi mở Playing. Pickup cũ bị disable/invalidate khi reset. Wallet trả về `startingWood` đã author.
- Không đổi cơ chế/balance skill, stage data hoặc prefab UI/Boss/tower trong gói này.

### Kiểm chứng không chạy Unity

1. Biên dịch độc lập toàn bộ source runtime Defense3D bằng C# compiler/Mono với các reference DLL hiện có: **PASS**. Không mở Unity.exe, không gọi Unity build/import pipeline.
2. Chạy `Tools/Defense3DValidation/DefenseSessionChecks.cs` như chương trình C# độc lập: **13 PASS**. Bao phủ Ready/Start, pause/resume, LevelUp, Overtime, early Boss defeat, LOSS ưu tiên, 3 restart, cleanup failure và disposal.
3. Parse YAML, kiểm tra ID/reference, binding 13 participant, source gameplay clock và hash/GUID: **PASS**.

Báo cáo: `Logs/TraversalQA/defense-refactor-foundation-checks.txt`.

**NOT_RUN:** Unity import/compile, EditMode/PlayMode, input thực, NavMesh/physics/visual và Android. Compiler độc lập không xác minh lifecycle Unity hoặc serialized binding sau import. Không dùng lại kết quả 33/33 cũ cho phiên bản này.

### Tương thích và việc cần làm tiếp

- Các fixture Unity/smoke caller cũ tạo Run hoặc actor trực tiếp cần được migrate để cấp session context trước khi chạy. Optional overload giữ source compatibility nhưng không cho phép spawn thiếu context. Smoke components trong scene chính vẫn tắt.
- Enemy target index còn static; death/direct-damage contracts chưa được thống nhất hoàn toàn. Không tuyên bố RF3D-008/009 hoặc toàn hệ thống đã đạt SOLID.
- Input Hero/Builder còn đọc thiết bị trực tiếp, nhưng đã bị chặn bởi gate. RF3D-012 sẽ thay bằng command input.
- Chưa có đường EXP/buff thật hoặc skill executor đầy đủ. Không tự import skill 2D để lấp khoảng trống này.
- Gói tiếp theo: tách Run/spawn/encounter rõ ownership, hoàn thiện lifecycle cleanup; sau đó damage/targets/stats trước khi chuyển skill/aim/UI controls.
