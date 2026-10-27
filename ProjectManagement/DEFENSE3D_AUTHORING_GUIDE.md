# Defense3D — Chỉnh trực tiếp scene và prefab

Cập nhật 26/10/2026. Thay đổi thực hiện trên file; không chạy Unity import, compile hay test trong đợt này.

## Scene chính

Mở lại `Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity` để nạp thay đổi từ disk. Không ghi đè bằng bản scene cũ còn đang mở trong Editor.

| Trong Hierarchy | Chỉnh ở đâu |
|---|---|
| `DefenseRuntime` | Actors/Pickups active, Staging/Pools_Reserved inactive; root/children giữ TRS identity. Run có reference tới component Runtime Roots. |
| `MapSongQue` | Root bản đồ chung, giữ TRS identity. Các nhóm Visuals/Collision/GameplayMarkers/Structures/Navigation ở bên dưới; SceneInfo giữ nguyên component và binding. |
| `[TREBRAVE] Session Composition` | Điểm khởi tạo session. Nối Run, Match UI, Wallet, 18 gameplay participant (gồm 10 tháp inactive, Base/Hero Health, hai skill effects và foot indicator) và hai skill presenters. Actor không có context sẽ không chạy gameplay. |
| `DefenseMatchUICanvas` | Prefab instance có sẵn. Expand để chỉnh MatchHUD, UpgradeModal và ResultModal. Không tạo Canvas trong Start. |
| `DefenseSkillControlsCanvas` | Prefab instance chứa Dash/Dép, icon/fill/TMP/lock/cancel zones. Presenter refs được override tới Hero/effect/indicator trong scene. |
| `DashAimIndicator`, `DepAimIndicator`, `DepImpactIndicator` | Ba instance có sẵn, renderer ẩn mặc định. Chỉnh visual nguồn ở DefenseAimIndicator3D.prefab. |
| `Hero_3D_Prototype` | Projector, Dash Traversal, Dash/Relic Effect và collider/motor có sẵn. Ground mask 4096; blocker mask 25; Dash yêu cầu baked NavMesh. |
| `EventSystem` | Có sẵn EventSystem và InputSystemUIInputModule; dùng cùng Input Actions với Legacy scene. |
| `MapSongQue/Structures/[TREBRAVE] Authored Towers` | 10 prefab instance `DefenseTower_Socket_01..10`, đặt tại tọa độ socket hiện có và tắt ban đầu. Bật tạm trong EditMode để chỉnh, tắt lại trước khi lưu. |
| `[TREBRAVE] Tower Build Test` | `Socket Towers` liên kết theo thứ tự `SceneInfo.TowerSockets`. Xây tháp chỉ bật instance tương ứng; Restart tắt lại. |
| `[TREBRAVE] 600s Defense Run` | Kéo reference Match UI và Tower Builder trực tiếp trong Inspector. `Show Debug Overlay` mặc định tắt. Scene chính hiện dùng Stage01_Production; FastSpawn giữ riêng cho stress test. |

Từ gói refactor session, Run không tự khởi tạo trong Start nữa: composition root cấp session và gọi InitializeSession. Khi tạo scene Defense mới hoặc dùng fixture/smoke scene cũ, cần author composition/binding tương ứng. Không tạo một session riêng trong từng actor. Chi tiết trạng thái và giới hạn kiểm chứng ở `DEFENSE3D_REFACTOR_PROGRESS.md`.

Tháp nằm trong nhóm riêng, không parent dưới marker có scale dẹt. Nếu đổi vị trí socket, chỉnh lại Transform của tháp tương ứng trong nhóm này.

## Prefab cần chỉnh

| Asset | Nội dung |
|---|---|
| `Assets/Resources/Defense3D/DefenseMatchUICanvas.prefab` | Layout, font, màu, HUD và modal. Container stretch theo Canvas; element neo theo pivot tương ứng. |
| `Assets/Resources/Defense3D/DefenseSkillControlsCanvas.prefab` | Hai nút skill/cancel zones, sprites dùng lại từ 2D, radial fill/TMP/lock và presenter. Không AddComponent/create UI trong runtime. |
| `Assets/Resources/Defense3D/Skill_Dash.asset`, `Skill_DepToOng.asset` | Range/radius/damage/cooldown/duration/tap policy. Snapshot capture khi session bind; tạo session mới để nạp lại. |
| `Assets/Resources/Defense3D/DefenseAimIndicator3D.prefab` | Ring/line và material preview/impact; không tạo renderer khi cast. |
| `Assets/Resources/Defense3D/DefenseTower3D.prefab` | Mesh, material, scale, damage, range, fire interval; không dựng cylinder/material bằng code lúc xây. |
| `Assets/Resources/Defense3D/DefenseYaksha3D.prefab` | Visual, ArenaCombat và child `Yaksha_Blast_Warning_Ring` có sẵn LineRenderer/material. Chỉnh bán kính/thời gian/sát thương ở ArenaCombat. |
| `Assets/Resources/Defense3D/DefenseEnemy_*.prefab` | Cấu trúc và component của quái. Walker chỉ lấy component có sẵn, không bổ sung bằng AddComponent lúc chạy. |
| `Assets/Resources/Defense3D/Enemies/*.asset` | Dữ liệu archetype và prefab nguồn. |
| `Assets/Resources/Defense3D/Stages/*.asset` | Lịch spawn, zone, giới hạn queue và thời điểm Boss. |

Quái/Boss và vật phẩm rơi vẫn được spawn theo trận từ prefab có sẵn. Chỉnh template trong Prefab Mode; không chỉnh clone đang chơi để làm thay đổi nguồn. Canvas, tháp và vòng cảnh báo không còn được dựng mới bằng runtime code.

Từ gói P2, các giá trị Inspector của Hero/Tháp/Base là baseline; session modifier được cộng/nhân lúc sử dụng. MaxHP grant của Base tăng cả capacity và phần HP mới, không hồi sinh Base chết. Enemy scalar stats/prefab reference được chụp khi compile Stage: sửa EnemyData trong một trận không thay scalar của các spawn tiếp theo trong trận đó; restart dùng cùng compiled plan, không tự đọc lại asset. Để nạp một plan mới cần khởi tạo session/scene mới.

Menu `Install Match UI In Open Scene` sử dụng prefab hiện có, nối reference và bổ sung EventSystem nếu thiếu; không tự sinh lại layout. Menu gán visual Yaksha lưu model thành child trong prefab Boss ở EditMode. Hai runner QA chỉ chạy khi chọn menu QA thủ công, không tự chạy khi script được import/compile và không gọi installer để ghi lại prefab.

## Kiểm tra đã thực hiện

Kiểm tra ngoài Unity: compile C# độc lập và core checks; YAML hợp lệ, ID không trùng, reference nội bộ không đứt; đủ 10 tower instances; UI/Run/Wallet/TowerBuilder được nối trong scene; composition Enemy được serialize. Phạm vi thay đổi qua các gói xem nhật ký; gói 6 thay component list Hero, composition binding, Canvas scale overrides và scene roots. Không thay NavMesh/GLB hay GUID có sẵn.

Báo cáo: `Logs/TraversalQA/defense-authoring-static-check.txt`. Bản sao trước khi chuyển đổi: `Logs/DefenseAuthoringBackup_20261026/`. Kết quả 33/33 test của lượt trước thuộc phiên bản trước thay đổi này, không được coi là xác minh cho phiên bản hiện tại.


## Lịch sử input/ability core — gói 4 (binding mới nhất ở gói 6)

Composition root inject `IDefenseHeroCommands` vào Motor và Tower Builder qua 15 participant hiện có; không cần thêm component hoặc sửa scene. Chỉ `DefenseHeroDeviceInput` đọc Keyboard/Gamepad; không đưa device polling trở lại actor. WASD/arrows/left stick và B giữ nguyên, chưa có action map Inspector hoặc touch skill control mới.

Basic attack dùng `DefenseAbilityRuntime` làm chủ cooldown. Ability mới cần một instance riêng, dispose khi owner bị hủy/rebind; không giữ thêm cooldown hoặc coroutine song song. Aim bị hủy khi gameplay gate đóng, restart reset state. Thêm touch UI phải qua adapter command có chính sách trộn nguồn rõ ràng; không ghi đè mailbox từ hai producer trong cùng frame.

Kiểm tra core ngoài Unity: `Tools/Defense3DValidation/RunCoreChecks.ps1`. Không dùng kết quả core để kết luận input device, physics hay visual đã được xác nhận.


## Lịch sử gesture/aim — gói 5 (đã nối scene ở gói 6)

- Dùng `DefenseSkillAimButton3D` cho Defense controls; không attach nguyên view/presenter Legacy vốn auto-create children hoặc phụ thuộc PlayerProvider/Physics2D. Shared `SkillPointerGesture` đã được Legacy handler và Defense adapter dùng chung.
- Projector cần Camera, Hero Origin, GroundMask loại trừ water/enemy/trigger, probe height và slope rõ ràng. Không bind khi các ref thiếu. Ground probe chỉ chứng minh endpoint có surface, không bảo đảm đường dash hợp lệ.
- Button cần Hero Health, projector và cancel-zone RectTransform riêng cho owner (hoặc một coordinator có ownership khi chia sẻ). Cancel zone không đặt trên ancestor của button; décor tắt raycastTarget. Không dùng singleton cancel của Legacy trong Defense.
- `Bind(context, ability, range, autoTargetOnTap)` phải nhận cùng session của ability. Ability/effect owner chịu dispose ability; button chỉ sở hữu/dispose gesture. Down/disable/focus/modal sẽ cancel pointer. Rebind phải bỏ listener và clear preview cũ.
- `AimReleased` chứa resolved aim + session/generation. Effect executor cần xác thực lại scope/gate/actor/traversal rồi mới commit cooldown một lần; không tiêu cooldown từ view. Chưa có executor mới được scene gọi trong gói này.
- Prefab `DefenseAimIndicator3D` đã có unit XZ ring và direction line, cả hai ẩn mặc định. Đặt root scale (1,1,1), bind presenter vào Show(resolvedAim, abilityRadius, cancelHovered)/Hide; radius phải lấy từ cùng definition với effect. Không Instantiate hoặc tạo material mỗi lần aim.
- Prefab indicator chưa có instance trong scene. Nút/control skin và presenter sẽ được author/bind ở bước kế tiếp; các source adapter hiện chưa tự kích hoạt skill.


## Cách dùng phần đã nối — gói 6

- Hai nút ở góc dưới phải. Tap Dash lướt theo hướng Hero, kéo để chọn hướng/cự ly; giữ mà chưa kéo không lướt tại chỗ. Tap Dép tự chọn quái gần nhất, không có quái thì không cast; kéo chọn điểm AoE. Thả trong cancel zone hủy, không tiêu cooldown. Đây là đường code đã nối, chưa kiểm chứng input thật trong Unity.
- Asset Dash mặc định range4.5/CD3/duration0.25; Dép range10/radius3.5/damage180/CD12, impact0.2. Chỉnh asset nguồn để nạp ở session mới. Không chỉnh cooldown ở view hoặc thêm một timer riêng.
- Pause/modal/down/disable/reset hủy dash đang chạy. Cooldown dùng session clock: pause/modal dừng, down vẫn trôi. Không có dash invulnerability hoặc boomerang trong gói này.
- Nếu dash không có Ground/NavMesh/path đủ an toàn thì bị từ chối, không tiêu cooldown. Không tắt requirement để cưỡng ép vượt sông. Baked NavMesh hiện có không bị đổi.
- Root có 17 participant và hai presenter. Scene-specific refs nằm ở prefab overrides của controls instance; prefab nguồn để trống refs Hero/projector/effect/indicator là có chủ đích, scene bắt buộc nối đủ. Tạo scene khác phải author các overrides tương ứng.
- Giữ Controls Canvas order90 dưới Match Canvas order100 để modal che UI skill. Ba override scale của Match Canvas đã được sửa từ0 về1. Indicator root scale1.
- Relic buffer capacity256 đặt trên effect. Khi saturation, toàn cast bị từ chối. Không dùng Physics overlap cấp phát hay damage theo từng collider thay query session.
- `VerifyRefactorStep6.py` kiểm tra YAML/ref/override/snapshot và core logs. Đã có 146 core checks PASS, không đồng nghĩa physics/UI/Android đã được nghiệm thu.


## Hierarchy bản đồ thống nhất — gói 7

```text
MapSongQue                         # SceneInfo giữ nguyên trên root này
├── Visuals
│   ├── TREBRAVE_MapSongQue_ThreeJS # GLB linked prefab, giữ rotation Y=180°
│   └── East_Arena_Visual
├── Collision
│   ├── West_Bank_Ground / East_Bank_Ground
│   ├── Bridge_01_Ground .. Bridge_03_Ground
│   └── Song_Que_Water_Visual       # đối tượng cũ có cả mesh và collider
├── GameplayMarkers
│   ├── EnemySpawn_East_Arena
│   └── TowerSocket_01 .. 10_VisualOnly
├── Structures
│   ├── Main_Base_West
│   └── [TREBRAVE] Authored Towers
└── Navigation                     # NavMeshSurface hiện có, cùng baked data
```

- Đây là gom hierarchy trong scene hiện có. GLB/prefab link, component/file IDs, world position/rotation/scale, layer, collider và baked NavMesh data được giữ nguyên. Không merge/unpack mesh GLB hoặc tạo runtime map root.
- `Navigation` là đối tượng `Defense_Navigation` cũ được đổi tên và parent lại; tên file NavMesh asset/GUID không đổi. Cấu hình vẫn Collect All + Physics Colliders + Ground mask4096.
- `Collision` giữ các proxy vật lý cũ, có thể vẫn chứa MeshRenderer. Giữ GameObject/collider active; muốn ẩn proxy thì chỉ tắt MeshRenderer. Không tắt cả nhóm để giấu primitive.
- Root và các nhóm giữ position0/rotation0/scale1. Đây không phải tính năng di chuyển bản đồ runtime: đổi vị trí/scale map cần cập nhật các tọa độ spawn/run và bake NavMesh tương ứng. Hero/camera/UI/session/skill indicators vẫn ở ngoài root bản đồ.
- Import lại GLB có thể cập nhật visual qua prefab link. Nếu hình học/vị trí cầu thay đổi, cần đồng bộ collider/marker/NavMesh; gói này chưa tự động hóa bước đó.
- Kiểm chứng hiện tại: `Tools/Defense3DValidation/VerifyMapHierarchy.py`; báo cáo `Logs/TraversalQA/defense-map-hierarchy-static-checks.txt`. Các checker step4/5/6 là bằng chứng theo snapshot của từng gói, không được coi là checker mới nhất cho scene đã gom.
- Khi Editor còn mở bản scene cũ, nạp lại scene từ disk trước khi lưu để thấy hierarchy mới và giữ thay đổi trên file. Không dùng menu rebuild prototype để tạo lại bản đồ đang chỉnh tay.


## Điều khiển/visual theo Vongxuyen — gói 8 (mới nhất)

- Defense ngắm ngay khi nhấn; không còn chờ0.12s. Drag6px bắt đầu điều khiển hướng,52px đạt toàn tầm. Thumb pad128px/thumb24px theo screen pixels, không nhận raycast. Nhấn/giữ chưa kéo vẫn dùng tap-target khi thả; cancel là vòng X/HỦY ngay phía trên từng nút, không dùng bảng đỏ giữa màn hình.
- Chỉnh các tham số drag/initial reach/min reach/cancel padding ở AimButton, các ref/sizes pad ở SkillStickView trên từng button. Mọi child được author trong `DefenseSkillControlsCanvas.prefab`; không auto-create bằng runtime.
- `DefenseAimIndicator3D.prefab` có range ring/fill/arrow và target ring. Các material `DefenseAim_RangeRing/RangeFill/Arrow` điều khiển shape; tint do view đặt qua MPB, không clone material.
- `HeroFootIndicator` là prefab scene root riêng, bind Hero/Health ở overrides và nằm trong participant list18. Ring0.44m, fill0.36m, arrow đầu0.52m; theo mặt Ground và yaw của Hero, ẩn khi down/modal/terminal.
- Vòng range/arrow màu xanh, invalid/cancel đỏ. Arrow của Dép là cue ngắm; target circle vẫn cho biết vùng hit AoE3.5m thật. Gói này không biến Dép thành projectile/boomerang hoặc thay damage/cooldown.
- Tap không có target sẽ ngắm fallback phía trước như source Vongxuyen; điểm vẫn phải vượt Ground/cast validation mới tiêu cooldown. Di chuyển touch joystick chưa có trong gói này.
- Checker hiện tại `VerifyVongxuyenControls.py`;160 core checks ngoài Unity PASS. Các hướng dẫn gói4/5/6 phía trên là lịch sử. Visual/shader/touch thực tế chưa được nghiệm thu.


## P4 — Runtime roots và Enemy ownership

- `DefenseRuntime` nằm ngoài MapSongQue, transform identity và không parent dưới marker/visual có scale. Actors và Pickups active; Staging và Pools_Reserved phải inactive. Không bật Staging để xem clone trong một trận đang chạy; muốn sửa enemy thì chỉnh prefab nguồn.
- Run có `_runtimeRoots` trỏ component trên root. Khi tạo scene khác phải author root và bốn child rồi bind; không gọi generator rebuild map hiện có. Actor prefab giữ component có sẵn, không AddComponent lúc spawn.
- Instance production được bind context/stats/registry/lifetime dưới Staging trước enable, sau đó nằm trong Actors. Wood drop nằm trong Pickups. Enemy chết/manual disable/cleanup phải qua session factory, không tạo thêm một danh sách Destroy riêng trong spawner.
- `DefenseActorLifetime` giữ session/generation/lease ID và latch reward/release. Không reuse token cũ hoặc tự initialize HP để giả lập spawn mới. Pool reuse chưa triển khai; P5 sẽ thay physical create/return policy tại boundary owner.
- Cleanup/manual disable/failed spawn không grant wood. Reward pickup vẫn là object vật lý, không cộng wallet trực tiếp. Nếu pickup creation lỗi, reward không được claim lặp và actor vẫn được release.
- Movement hiện còn bridge adapter cũ; root/factory mới không có nghĩa water/air đã đúng locomotion. Bước kế tiếp là author các strategy tương ứng.
- Check mới nhất cho gói này: `VerifyP4FactoryLifecycle.py`;181 checks core PASS, chưa chạy lifecycle/native physics trong Unity. Các báo cáo renderer/UI trước đó chỉ có giá trị trong phạm vi của chúng.


## P4 — Ground aggro và đường tới Base

- `DefenseEnemy3D.prefab`/`DefenseEnemy_Magiap.prefab` có GroundAggro với refs Enemy/Movement/Attack/Profile. Không tự thêm component trong runtime; prefab Ground khác/Boss chưa tự nhận Hero chase.
- Chỉnh `GroundNavigation_SongQue.asset` để chỉnh bank edges, entry/exit, ba lane Z, bridge width và acquire/retain/lock/repath/provoked. Snapshot capture theo actor lúc configure, không live-edit actor đang chạy.
- Base endpoint lấy Transform scene thực; nếu chỉnh Base trước khi vào trận, route mới lấy vị trí đó. Ground ở West không quay lại East; không lấy teleport làm fallback khi mất path.
- Hai prefab Ground dùng LOS mask25, collider-surface range và cùng-bờ trước từng hit. Thay collider size cần kiểm tra stop reach tương ứng; không nới range để che lỗi route endpoint.
- Chưa migrate water/air/landing/Support/Boss. Không coi chúng đúng locomotion chỉ vì đã có enum/data; hiện vẫn còn bridge fallback cũ.
- Checker gói này `VerifyP4GroundAggro.py`;202 core cases PASS, nhưng NavMesh/physics/aggro trong Unity chưa được xác nhận.


## Profile spawn mặc định và log queue

Scene chính đã chuyển từ `Stage01_FastSpawn` về `Stage01_Production` (18s/đợt). FastSpawn vẫn giữ0.9s/đợt để stress test, có thể overflow với cap32/40; không tăng cap hoặc tắt warning để che áp lực này.

Nạp scene từ disk và tạo session mới để dùng reference Production. Restart trong cùng session không compile lại Stage. Startup/overflow log có profileVersion để nhận biết trận đang chạy bằng profile nào.

Overflow log: active/limit, owned actor count, pending/limit, blocked predicate và factory failures của batch trước, expired/overflow totals. Blocked predicate có cả retry wait lẫn placement. Expiry được dọn trước enqueue độc lập active cap, không đổi TTL25s.

`VerifySpawnQueueFix.py` và216 checks ngoài Unity đã PASS. Production không overflow trong mô phỏng bước0.1s/all-placement-valid/no-kills; không phải bảo đảm runtime NavMesh/combat đã hoàn thiện.

## Water → Shore — 27/10/2026

Trong `MapSongQue/GameplayMarkers/WaterNavigation`, chỉnh ba cặp `WaterExit` (trong kênh) và `ShoreExit` (trên bờ tây). Component giữ collider bounds nước, sample radius0.3 và Ground mask4096. `DefenseRuntime` tham chiếu component này; không tạo anchor trong runtime. Bến được kiểm tra mặt đất và đường NavMesh hoàn chỉnh đến Base lúc spawn và lúc cập bến.

`DefenseEnemy_ThuyQuai.prefab` có WaterMovement và NavMeshAgent **tắt sẵn**. Không bật agent trong nước. WaterMovement tự bật sau handoff; chỉnh arrival distance, retry và shore timeout trên prefab. Điểm canalWater vẫn lấy từ Stage. Các anchor hiện mới được kiểm tra trên file, chưa xác nhận physics/NavMesh thực tế; không coi authoring PASS là gameplay acceptance.

## Air và anti-air — 27/10/2026

- Vùng bay: `MapSongQue/GameplayMarkers/AirNavigation_FlightVolume`. Giữ BoxCollider dạng trigger, layer Ignore Raycast; chỉnh bounds để giới hạn vùng bay. `DefenseRuntime` phải tham chiếu AirNavigation này.
- `Assets/Resources/Defense3D/Air_SongQue.asset`: world height1.4..4, cruise2.2, clearance0.7, obstacle mask Default/Obstacle/Ground và timeout10s. Clearance phải bao world radius của hitbox prefab; không coi altitude mặc định là đã xác nhận camera.
- `DefenseEnemy_HuyetDoi.prefab`/`DefenseEnemy_PhiCuong.prefab`: Enemy layer, centered SphereCollider trigger và AirMovement; NavMeshAgent tắt sẵn. Không bật agent để sửa đường bay. Flight dò vật cản bằng collider thực; mesh trang trí không collider không tự chặn flight/LOS.
- `Targeting_Hero.asset` và `Targeting_Tower.asset`: chỉnh quyền Ground/Air, LOS mask và vị trí phát đòn offset. Hero trong scene trỏ Hero profile, tháp kế thừa Tower profile từ `DefenseTower3D.prefab`. Profile thiếu không tự cấp quyền bắn. Air range đo XYZ từ vị trí phát đòn đến tâm mục tiêu; Ground giữ range XZ.
- Stage zone/elite replacement không đổi; Air bị chặn ở spawn sẽ giữ job theo planner/TTL hiện có, không chuyển zone tùy ý. Trạng thái của gói này mới được kiểm tra ngoài Unity, chưa chấp nhận chuyển động/va chạm/LOS/camera thực tế.

## Landing và Linh Pháo — 27/10/2026

`MapSongQue/Structures/GhostCannonBattery` chứa ba linked instance `GhostCannon_North/Mid/South` và ba `Landing_West_*` anchors. Chỉnh vị trí pháo và điểm đáp trực tiếp trong scene; mỗi pháo phải có CannonId riêng và đúng landing anchor. RuntimeRoots tham chiếu Battery; Battery tham chiếu đúng ba pháo và definition quaidu. Không thêm Tiểu Quỷ vào Stage events.

Mở `Assets/Resources/Defense3D/DefenseGhostCannon3D.prefab` để chỉnh body/hitbox, Muzzle, projectile và canopy. Hai view đạn/dù được author inactive; không xóa references hoặc tạo lại từ code. Chỉnh HP, first-shot time, interval, ballistic/parachute duration, arc/open height và cutoff tại `GhostCannon_SongQue.asset`. Offset của ba instance là0/4/8s. Thông số này đang là prototype, chưa phải cân bằng cuối; cutoff600s ngừng phát mới nhưng giữ đạn/job đã có.

Landing phải ở West, có Ground collider + baked NavMesh route về Base, capsule clearance hợp lệ và cách Hero ít nhất3.1m. Battery cho chỉnh ground/blocker masks, sample radius và capsule clearance. Trước Awake, query fail closed. `DefenseEnemy_QuaiDu.prefab` giữ agent disabled; chỉ owned landing acquisition mới bật agent và tạo Ground actor. Nếu bị chặn, request chờ/hết TTL theo chung planner; không ép spawn hoặc chuyển điểm đáp ngẫu nhiên.

Pháo bị hủy dừng phát mới, đạn đang bay tiếp tục; restart hủy đạn cũ và hồi phục ba pháo. Ba công trình và tối đa ba view đạn được quản lý riêng; quaidu đã sinh dùng chung32 mobile actor slots. Không thay active/pending caps để bù áp lực từ pháo. Flight đang dùng đường script, chưa có projectile impact damage/collision vật lý; primitive art và toàn bộ NavMesh/physics/visual acceptance chưa được kiểm chứng trong Unity.

## Aggro Hero/Support/Base đa địa hình — 27/10/2026

`Aggro_SongQue.asset` điều khiển acquire/retain/lock/think/provoked của cả Ground, grounded Water/Landing và Air. Chỉnh tại asset này; các ngưỡng aggro cũ trong `GroundNavigation_SongQue.asset` chỉ giữ để tương thích. Ground Navigation tiếp tục điều khiển bờ/cầu/lane. Không sửa enum locomotion của Water/Landing thành Ground để bật aggro.

`MapSongQue/GameplayMarkers/DefenseFriendlyTargets` có Hero health và Supports array. Hiện Supports rỗng vì chưa có đơn vị Support trong Defense3D. Khi có actor Support được author, thêm `DefenseSupportTarget3D` health endpoint, Collider phù hợp và nối component vào Supports trước khi tạo session. Endpoint chưa có Support AI/vũ khí/visual; danh sách được chụp lúc Bind, không đăng ký thêm bằng cách sửa array giữa trận.

Water chỉ bắt đầu GroundAggro sau cập bến/Warp hợp lệ; Tiểu Quỷ sau owned landing acquisition. Air dùng XYZ và swept clearance để chọn friendly; Ground cần cùng bờ và NavMesh path. Dead/down/inactive/unreachable thì bỏ target, quay về Base khi không còn friendly hợp lệ. Range/LOS được kiểm tra lại khi tấn công; cooldown không reset theo retarget. Boss vẫn có owner riêng.

Không có kiểm thử Unity trong gói này: các ngưỡng, chase/retarget qua cầu/bờ và tích hợp Support thật vẫn cần acceptance; không suy từ pure policy PASS rằng NavMesh/Physics đã đạt.

## Slow/Stun/Knockback — 27/10/2026

Sáu prefab quái thường có `DefenseEnemyStatus3D`: max duration10s, Slow capacity8, max knock distance2m, capsule clearance radius0.45/half-segment0.5 và blocker mask4105. Chỉnh các giới hạn tại component prefab; GroundMovement có displacement sample radius0.2 và max height delta0.25. Không bật Knockback bằng cách tắt NavMeshAgent hoặc tự đổi transform từ skill.

Nguồn Dép dùng `Assets/Resources/Defense3D/RelicStatus_SongQue.asset`, được nối vào Relic effect trong scene. Prototype hiện Slow35% trong2s, Stun0.25s và đẩy tối đa1m trong0.2s. Đặt duration0 để tắt một effect; null profile giữ skill damage-only. Các giá trị chưa chốt cân bằng. Skill damage/radius/cooldown vẫn ở definition hiện có.

Payload API cần SessionIdentity/RunGeneration hiện tại. Slow lấy mạnh nhất với expiry độc lập, Stun không bị refresh ngắn hơn; Knockback không stack, bị chặn thì từ chối/hủy riêng phần đẩy. Ground kiểm tra NavMesh continuity/height và swept clearance; Water chỉ đẩy khi Swimming trong kênh, Docking từ chối; Air giữ giới hạn độ cao/vùng bay. Quái đã tiếp đất dùng Ground; projectile dù không phải target. Boss/miniboss/Pháo chưa có status component và không nhận effect trong gói này.

Stun/full Slow không được coi là navigation stuck; khi hết hiệu ứng agent tiếp tục path. Các kiểm tra ngoài Unity chưa xác nhận hành vi NavMeshAgent.Warp/baseOffset, va chạm hoặc chuyển động nhìn thấy. Không coi pure status tests PASS là nghiệm thu gameplay.

## Trảo Quỷ Phi Cương — 27/10/2026

Mở `DefenseEnemy_PhiCuong.prefab`: root có ConeAttack + ConeTelegraphView, child `ClawTelegraph_Sector120` là Quad author inactive nhưng Renderer enabled. Không tạo lại chỉ dấu bằng code hoặc bật thêm một damage source song song. EnemyAttack `_coneAttack` phải nối component ConeAttack trên cùng prefab.

`PhiCuong_Claw.asset` giữ windup0.65s/radius2.5m/angle120°/LOS mask4105. Các thông số được capture khi spawn Bind; reload actor/session để áp cấu hình mới. Range Trảo Quỷ lấy từ profile này; damage/cooldown vẫn từ phicuong EnemyData (18/2s hiện có). `PhiCuong_Telegraph.mat` dùng AimOverlay Sector5; angle shader được View đặt qua property block theo snapshot, không tự chỉnh góc material khác profile.

Giữ parent XZ scale dương và bằng nhau để chỉ dấu không méo; view bù scale để world radius đúng. Có Ground/Water collider cho projection; không có collider bên dưới thì view giữ cao độ origin dự phòng. Shader giữ SRPDefaultUnlit cho renderer hiện có; shader chưa được compile/render xác nhận trong đợt này.

Trong0.65s, PhiCương dừng bay/đổi mục tiêu, hướng cone đã khóa. Né ra ngoài/ra sau trước lúc đánh hoặc ngắt bằng Stun/Knockback sẽ tránh pending hit. Cooldown tính từ lúc bắt đầu, không hoàn lại khi hủy. Pause/modal/death/cleanup/restart ẩn warning và hủy pending; không có hit cũ khi resume. Damage recheck XYZ range/LOS trên các friendly và Base còn hợp lệ. Support roster vẫn chưa có units trong scene.

Chưa có chém VFX/animation/audio riêng và chưa nghiệm thu Unity visibility/physics/cooldown thực tế. Không chạy legacy generator để ghi đè prefab đã author.

## Boss FSM / encounter — 27/10/2026

Không cần thêm GameObject/component: Yaksha prefab/ring và Stage giữ nguyên. Stage `bossHealth`1200/`arenaDuration`45 điều khiển spawn HP và Arena duration; `DefenseYakshaArenaCombat3D` giữ blastRadius5/blastInterval3/warningDuration1.1/blastDamage30. Những giá trị được capture khi Begin encounter; không dùng constructor standalone FSM3200HP/60 damage để cấu hình gameplay.

Encounter inject cùng actor HealthCore vào FSM. Phase/timer chỉ ở FSM; ArenaCombat không chạy Update riêng. Assault giữ HP đã mất, chỉ đổi đường đi. Thắng/thua vẫn qua Session/MatchRuntime; không gọi WIN trực tiếp khi Boss chết trước600s.

Arena ring được author sẵn, vị trí tâm khóa theo blast origin; pause ẩn warning và freeze timer, resume hiện lại pending warning, terminal/reset hủy và stop path/basic attack. Không tạo ring/material trong runtime. Nếu Boss bị remove/disable mà chưa chết thì không cấp defeat; đọc warning lifetime/assault path lỗi thay vì ép Win.

Legacy `ArenaCombat.Begin(hero)` không còn khởi động một arena độc lập: phải bind context, shared actor health FSM và actor lifetime qua encounter. Không dùng fixture Unity lịch sử unbound Begin như bằng chứng sau refactor. Chưa xác nhận Unity callback ordering, NavMesh Assault, warning render hoặc full match; không chạy Unity trong đợt này.


## Player Action Thư Sinh — 27/10/2026

Bộ control hiện tại là4 slot, thay hướng dẫn lịch sử chỉ2 Dash/Dép ở các mục trước. Xem `GameDesignDoc/TREBRAVE_Unity3D/15_HERO_ACTION_AND_SKILL_UI.md` cho thông số, đường đi bindings và giới hạn đã xác minh. `HeroKit_ThuSinh.asset` phải map đúng4 definition; composition có20 participants và4 presenters. Scene dùng manual Attack; tùy chọn auto ở owner cũ dùng chung Ability, không bật thêm executor gây damage song song.

Attack có preview/impact sector riêng trong `DefenseHeroAttackIndicator3D.prefab`; Signature có preview/impact tròn từ generic Aim prefab. Tất cả author sẵn. Không sửa generic Aim shader/prefab để thêm cone cho mọi skill; không tạo thêm HeroFoot trùng indicator thường trực. Controls chứa4 vùng hủy/pad độc lập; vị trí hủy chung hiện là(-250,600) tương đối góc phải dưới ở reference1920×1080. Keyboard adapter nằm trên Controls root, chỉ tham chiếu các presenter local.

Capture definition khi khởi tạo phiên; sau chỉnh thông số cần khởi tạo lại phiên. Signature là self-AoE, không chọn hệ. `_relic` phải là component implement `IDefenseAbilitySource` của slot Dép trong cùng session. Đã kiểm tra source/GUID/YAML ngoài Unity; chưa xác nhận icon/font, visibility, touch, hình học hit và callback lifecycle thực tế.


## Warning UI và chỉ dấu Player — 27/10/2026

Scene mới có21 participants/4 skill presenters. Warning presenter thuộc `DefenseSpawnWarningsCanvas` prefab instance; Run `_spawnWarnings` trỏ đúng component đã bind, nhận cùng compiled Stage plan. Canvas sort85, Controls90; giữ mọi badge raycastTarget=false và không thêm GraphicRaycaster. Có32 slot inactive author sẵn; không Instantiate UI trong runtime. Chỉnh lead5s/margin38px/spacing46px trên presenter của prefab, màu vàng/đỏ trên từng Graphic. Native render/device chưa nghiệm thu.

Attack indicator prefab có `AttackSectorOutline` LineRenderer local-XZ, authored34 điểm; view cập nhật theo góc/radius snapshot. Giữ reference `_sectorBorder`, material line hiện có và parent scale identity. Không dùng generic Aim prefab thiếu border làm Attack indicator. HeroFoot không tạo prefab thứ hai; source fallback đáy collider khi probe miss, nên cần Hero Collider đúng chân nhân vật.

Warnings chỉ mô tả mốc/zone Stage trước5s, không điều khiển spawn hoặc báo chính xác retry/placement/landing. Thiếu chỗ màn hình có thể ẩn badge. Snapshot và checkers lịch sử áp dụng đúng revision riêng; `VerifyIndicatorWiring.py` là checker gói mới, vẫn cần pre-change snapshot nếu chạy kiểm tra bảo toàn hash.
