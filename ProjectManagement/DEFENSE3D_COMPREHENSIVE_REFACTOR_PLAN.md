# Defense3D — Kế hoạch refactor toàn diện và tái sử dụng gameplay 2D

Ngày: 26/10/2026 · Trạng thái: P0/P1 implemented in stages; P2 IN_PROGRESS · Phương pháp: đọc mã nguồn và đối chiếu asset tại local.

Đã có các gói baseline/session/gameplay gate, tách runtime ownership và damage/target/stat. Xem [nhật ký triển khai](DEFENSE3D_REFACTOR_PROGRESS.md) để phân biệt phần đã có code, kiểm tra độc lập và phần chưa nghiệm thu. Các bảng dưới đây vẫn mô tả toàn bộ kế hoạch, không đánh dấu mọi ticket đã xong.

Kế hoạch này chưa phải phần triển khai. Không chạy Unity, compile, EditMode, PlayMode hay device test trong đợt lập kế hoạch. Không bổ sung runner/audit tự chạy khi import script. Các thay đổi chuyển UI/tháp/Boss sang scene và prefab ở lượt trước mới được kiểm tra tĩnh; kết quả 33/33 trước đó không xác minh phiên bản sau chuyển đổi.

## 1. Kết luận và phạm vi

**Có thể dùng lại tài nguyên và một phần cơ chế của ProjectZombie 2D. Không thể dùng nguyên bộ skill, aiming và Presenter 2D cho Defense3D.**

- Dùng lại sprite/icon, texture chỉ dấu, hình thức cooldown và quy tắc tap/hold/drag/cancel.
- Tách logic tương tác UI khỏi singleton và model 2D trước khi dùng chung.
- Bổ sung cách quy đổi màn hình → thế giới XZ, tìm mục tiêu 3D, kiểm tra địa hình và thực thi damage/status 3D.
- Chuẩn hóa một bộ xử lý skill/cooldown/aim cho Defense3D; không kéo PlayerController, WeaponManager và bootstrap Legacy vào scene Defense.
- Duy trì Legacy 2D; không đổi hàng loạt GUID, không tái tạo bản đồ hoặc bake NavMesh để refactor kiến trúc.

Phạm vi bao gồm lifecycle, clock/pause, composition, Hero/input, ability/aim/UI, Enemy/AI, waves/spawn, Boss, damage/status, tower/economy, EXP/upgrades, pooling, authoring và khả năng kiểm chứng. Không thêm multiplayer, monetization hoặc mở rộng roster ngoài Core đã chốt.

### Quyết định phạm vi Core 3D — 27/10/2026

**Theo chỉ đạo người dùng: chưa áp dụng Ngũ Hành vào Defense3D; tập trung Action + Tower Defense trước.** Quyết định này ưu tiên hơn các đề xuất tái sử dụng cơ chế hệ từ 2D trong tài liệu lịch sử.

- Không đưa tương sinh/tương khắc, combo hệ, hit ảo dẫn hệ, chọn hệ khi cast, buff/CD do phản ứng hệ hoặc ràng buộc build theo hệ vào Core 3D. Không tạo dependency để các tính năng action/TD phải chờ Ngũ Hành.
- Có thể giữ màu/icon nhận diện nhân vật như visual; chúng không tự cấp thuộc tính chiến đấu hoặc hệ số sát thương theo hệ.
- Signature port sang 3D phải có tác dụng action/TD độc lập: vùng đánh, damage, khống chế, hồi phục, cơ động, cooldown trực tiếp hoặc hỗ trợ phòng tuyến theo từng definition. Không kéo ngầm phần Ngũ Hành của implementation 2D vào executor 3D. Buff/cooldown reduction trực tiếp không mặc định là phản ứng hệ.
- Ưu tiên Player combat: Attack/Dash/Signature/Relic, điều khiển và phản hồi rõ, hướng/tầm đánh, hit timing, chỉ dấu vùng ảnh hưởng và vùng hủy, né/ngắt telegraph. TD ưu tiên lane/role quái, chọn mục tiêu, vị trí/nâng cấp tháp, tài nguyên, nhịp wave và bảo vệ Base.
- Mở lại thiết kế Ngũ Hành sau khi vòng lặp Action + TD ổn định và chứng minh được giá trị chiến lược qua yêu cầu/đánh giá riêng. Không xóa hoặc đổi cơ chế Ngũ Hành đang có của Legacy 2D.

## 2. Hiện trạng cần xử lý trước

| Điểm hiện tại | Hệ quả | Hướng xử lý |
|---|---|---|
| `DefenseRun6003D` điều phối lifecycle, NavMesh, spawn, Boss, cleanup và debug HUD | Nhiều lý do để sửa một class | Giữ MonoBehaviour/GUID làm facade trong giai đoạn chuyển tiếp; tách từng trách nhiệm |
| Pause/LevelUp thay state nhưng nhiều actor dùng `Time.time`, Rigidbody hoặc NavMesh riêng | Có thể dừng timer nhưng combat/movement vẫn chạy | Một clock và gameplay gate; adapter vật lý phải dừng cả agent/body |
| `DefenseHeroCombatState`, `DefenseDepToOngRelic`, `DefenseTowerSocketCore`, `DefenseRewardPipeline`, `DefenseYakshaBehaviorFSM` chưa thấy caller trong `Assets/Features` ngoài chính khai báo | Core logic và runtime prototype đang song song | Rà từng contract, chọn một owner và nối adapter; không mặc định core đúng chỉ vì có test cũ |
| Presenter tự tạo EXP/UpgradeDeck và điều khiển lifecycle | UI sở hữu trạng thái gameplay | Chuyển quyền sở hữu sang session/progression; Presenter chỉ bind/read/command |
| Chưa thấy caller production cho `AddExp` hoặc consumer của `GetModifier` trong Defense3D | EXP/upgrades chưa tác động gameplay đầy đủ | Nối death → drop → pickup → EXP → selection → stat evaluation |
| `DefenseEnemyDrop3D` tìm Wallet và gọi spawn tĩnh | Dependency ẩn tại lúc quái chết | Inject reward sink/drop factory trong bước acquire |
| Tham số skill và Hero còn nằm trong constants/constructor | Khó cân bằng bằng Inspector | Definition/Profile assets, snapshot runtime riêng |
| UI/indicator 2D có `new GameObject`, `AddComponent`, singleton và fallback tự dựng | Dùng nguyên sẽ đưa lại runtime hierarchy khó quản lý | Prefab đã dựng sẵn + validation; không fallback sinh UI |
| Instantiate/Destroy, registry static và callback lifecycle hiện hữu | Pooling/restart khó bảo đảm sạch | Session ownership, pool handle, idempotent release và reset order |

Phân biệt: thiếu caller production là kết quả tìm kiếm tĩnh hiện tại; không phải tuyên bố một tính năng đã được kiểm tra khi chơi.

## 3. Ma trận tái sử dụng 2D → Defense3D

Các đường dẫn dưới đây tính từ thư mục gốc repository.

| Thành phần nguồn | Có dùng lại? | Cách dùng và giới hạn |
|---|---|---|
| `Assets/Art/UI/Buttons/`, `HeroSkills/`, `Skills/`, HUD sprites | Có, cấp asset | Dùng chung texture/sprite GUID; không cần vẽ lại chỉ vì đổi 2D → 3D. Xem lại bố cục, khả năng đọc và kích thước chạm cho màn hình Defense |
| `Assets/Art/VFX/Indicators/TEX_Indicator_{Circle,Box,Fill}.png` | Có, cấp asset | Dùng texture trên prefab chỉ dấu 3D; trục, độ cao, depth và material phải được cấu hình riêng |
| `SH_URP_SkillIndicator_Sector.shader` | Ứng viên dùng lại | Shader sử dụng UV và URP transform; cần material/mesh XZ thích hợp. Chưa kết luận tương thích hiển thị khi chưa kiểm chứng |
| `DashButtonView` | Gần dùng trực tiếp | Passive View nhận cooldown/text và phát click. Bind với Presenter Defense, serialize đầy đủ reference; không dùng `DashButtonPresenter` 2D |
| `AttackButtonView` | Dùng lại visual và contract sau làm sạch | Hiện tự AddComponent drag handler. Shared variant phải có handler sẵn và bỏ fallback tạo component |
| `SignatureSkillButtonView` | Dùng lại sau làm sạch | `EnsureCooldownComponents` có thể sinh fill/text lúc chạy. Tạo sẵn các child, bỏ auto-build và chuyển format text về Presenter |
| `RelicSkillButtonView` | Dùng lại sau làm sạch | Hiện có thể sinh stack badge/recast glow; các phần đó cần child prefab, chỉ bật/tắt theo state |
| `SmartSkillDragHandler` | Dùng lại thuật toán/tương tác | Có pointer ownership, quick tap, hold, drag, detailed release, cancel. Tách `UICancelSkillZone.Instance`, static edit-mode flag và xử lý hold cấp phát; cấu hình threshold theo Canvas scale |
| `UICancelSkillZone` | Dùng lại hình thức và hit-test | Tách singleton, nhận reference từ scene; view chỉ hiển thị, coordinator sở hữu phiên aim |
| `SkillAimType`, `SkillAimConfig` | Dùng lại có adapter | Giữ ý nghĩa shape/range/radius. Config đang dùng chung `sectorAngle` cho cả angle và wall thickness; chuyển sang profile rõ nghĩa, có mapping Legacy |
| `AimResult` | Không dùng trực tiếp cho XZ | `Direction` là Vector2 và `FromDirection` cộng vào XY. Tạo kết quả aim có world direction/ground target rõ ràng; Legacy adapter giữ XY |
| `ISkillAimService`, `IAimIndicatorRenderer` | Tham khảo contract; không sửa signature ngay | Direction Vector2 chưa ghi rõ không gian; renderer Initialize nhận root để tự tạo GO. Thêm contract dùng chung mới, Legacy adapter giữ API cũ |
| `SkillAimIndicatorController` | Không gắn nguyên class | Phụ thuộc PlayerProvider/PlayerController, singleton, XY angle và tự dựng hierarchy. Thay bằng coordinator + projector + target query + presenter chỉ dấu có reference sẵn |
| 8 renderer trong `Combat/Aiming/Renderers/` | Dùng lại thuật toán shape | Hiện dùng SpriteRenderer, XY và quay quanh Z, nhiều renderer tạo GO. Viết adapter/render backend XZ trên prefab; không chỉ đổi mỗi góc quay |
| `AutoTargetScanner`/`TargetingUtility` | Dùng lại chính sách chọn, thay backend | TargetingUtility dùng Collider2D/Physics2D và health Legacy. Defense dùng target query theo contract, xét mặt đất/trên không, range và vật cản |
| `AttackButtonPresenter`, `DashButtonPresenter`, `SignatureSkillPresenter`, `RelicSkillPresenter` | Không dùng nguyên | Phụ thuộc PlayerProvider, PlayerController, WeaponManager, SignatureSkillManager, singleton/network buffer. Presenter mới nhận ability ports, không kéo Legacy model vào Defense |
| `ISignatureSkill`, `SignatureSkillBase`, `SignatureSkillManager` | Dùng lại ý tưởng lifecycle; không gắn trực tiếp | Contract yêu cầu `PlayerStats`/`HealthSystem`; manager tự tick Time.deltaTime và có nhánh riêng Thư Sinh. Cần context/ability runtime độc lập actor 2D |
| Các skill Thư Sinh/Thanh Đồng/Võ Tăng/Đạo Sĩ | Port theo từng effect | Dùng lại mô tả/metadata đã duyệt và asset phù hợp; viết adapter damage, status, movement, projectile/VFX. Không đổi tên Physics2D thành Physics một cách cơ học |
| `SignatureSkillData` và các asset skill | Metadata có thể ánh xạ | Factory trả ISignatureSkill Legacy; không đưa factory đó vào Defense. Tạo Definition Defense ánh xạ icon/name/description có provenance; cooldown/balance cần profile riêng |
| `IPlayerInputProvider` | Tham khảo contract | Event skill hiện không mang full aim result. `PlayerInputReader` còn nối enum/network Legacy; tạo adapter local gọn và command rõ dữ liệu |
| `AreaEffectCaster3D` | Ứng viên backend, chưa plug-and-play | Đã có NonAlloc và XZ knockback nhưng yêu cầu IStatusReceiver, có buffer hữu hạn và cần deduplicate nhiều collider cùng actor. Defense hiện chưa có adapter status tương ứng |
| `IPoolable`, audio service, VFX assets | Dùng lại sau rà dependency | Không tái sử dụng nguyên pool manager nếu manager tự tạo root hoặc gắn singleton. Root/pool catalog phải được author trong scene |

Các nút Dash/Signature/Relic hiện tìm được trong `Assets/Scenes/SampleScene.unity`; chưa tìm thấy prefab độc lập tương ứng trong các thư mục UI đã rà. Vì vậy cần **trích xuất visual/control prefab trước**, không giả định đã có một prefab skill button hoàn chỉnh để kéo vào Defense.

## 4. Kiến trúc đích

```text
Authored Scene / Prefab / Definition Assets
                   |
        DefenseSceneCompositionRoot
       (validate -> construct -> bind)
                   |
        DefenseSession / Lifecycle
       State + Clock + Command Gate
         /         |           \
  Combat/Ability  Spawn/AI   Progression/Economy
         \         |           /
        Unity adapters: physics, NavMesh,
        pools, projectiles, audio and visuals
                   |
        Read models + domain events
                   |
       UI Presenters -> Passive Views
```

- Core không phụ thuộc MonoBehaviour, SceneManager, Physics hay UI. Có thể giữ các kiểu toán học Unity trong contract hiện tại; không ép tách thư viện toán chỉ để đạt nhãn “pure”.
- Composition root là nơi kết nối dependency. Cấu hình bằng Inspector/Definition, không thêm DI framework hoặc service locator toàn cục.
- UI phát command; không gọi `RuntimeAdapter.Run.State` hoặc tự thay stat.
- Adapter nhận context từ session; không tự tìm Hero/Wallet ở thời điểm chiến đấu.
- Contract chỉ tạo tại ranh giới cần thay thế: clock/gate, input, targeting, damage/effect, pool, UI read model. Không tạo interface cho mọi class dữ liệu.
- Không tạo hệ base class lớn cho Hero/Enemy/Tower. Chia sẻ capability: damageable, targetable, ability owner, status receiver theo nhu cầu thực tế.

### 4.1 Một owner cho một loại trạng thái

| Dữ liệu | Owner đích | Người sử dụng |
|---|---|---|
| Phase, thời gian, result | DefenseSession/lifecycle | Actor adapters, spawner, UI read model |
| HP và death generation | Health core của từng actor | Damage service, rewards, HUD |
| Ability state/cooldown/aim session | Ability runtime của Hero | UI và executor; UI không giữ bản cooldown riêng |
| Base stats + upgrade modifiers | Stat evaluation của actor/session | Hero, Tower, skill damage/cooldown |
| EXP, pending level-ups, offer hiện tại | Progression runtime | Upgrade Presenter/command handler |
| Gỗ, chi phí xây/nâng/sửa | Economy runtime | Pickup collector, tower commands, HUD |
| Pool lease, spawn generation | Entity lifecycle/pool owner | Spawn caps, targeting registration, cleanup |

### 4.2 Lifecycle và pause phải có tác dụng thật

- Một gameplay clock; Hero cooldown, Enemy attack, Boss warning, DoT, respawn, pickup expiry và timed waves dùng gameplay time.
- Khi Paused/LevelUp/terminal: chặn command gameplay; zero horizontal movement, dừng NavMeshAgent; không chỉ ngừng timer.
- Vật lý FixedUpdate nhận movement intent và gate của session; không cộng thêm một clock thứ hai trong FixedUpdate.
- UI drag/hold/animation dùng unscaled time, nhưng bị cancel/lock khi gate đóng; không được cast qua modal.
- Thứ tự xử lý phải rõ: nhận command → kiểm tra gate → thực thi gameplay → resolve damage/death → resolve result với LOSS ưu tiên → chuyển UI/level-up nếu còn hợp lệ. Không mở offer sau terminal.
- Restart: khóa input/cancel aim → hủy cast/effect và bỏ listeners của lượt cũ → release entities/pickups → reset resources/stats/EXP/towers/Boss → reset state/clock → bind read model → mở Playing. Callback lượt cũ không được tác động lượt mới.
- Downed chỉ khóa quyền của Hero và hủy aim; trận/Base vẫn tiếp tục theo D-04/A. Không tự đổi quy tắc cooldown khi Downed nếu chưa có baseline quyết định.

## 5. Thiết kế skill và aim dùng chung

Các tên dưới đây là tên đề xuất, chưa có implementation.

### 5.1 Contract tối thiểu

| Contract/model | Trách nhiệm |
|---|---|
| `AbilityDefinitionSO` | ID, icon/text key, cooldown, targeting profile và effect definition. Chỉ metadata/config, không lưu cooldown runtime |
| `AbilityRuntime` | Ready/Aiming/Casting/Recovering/Cooldown và chuyển trạng thái hợp lệ |
| `AbilityCastRequest` | Slot, owner/session generation, input source, resolved aim và target identity tùy loại |
| `ResolvedAim` | World origin/direction/target point, pull, aim mode, valid/invalid reason; không để Vector2 ngầm mang nghĩa XY hoặc XZ |
| `IAimProjector` | Input màn hình hoặc drag → hướng/điểm thế giới trên mặt phẳng của mode |
| `ITargetQuery` | Chọn mục tiêu qua contract; phân biệt ground/air, alive/targetable và thứ tự ưu tiên |
| `IAbilityExecutor` | Thực thi effect/trajectory với các service được inject; không làm UI và không tự chọn skill owner |
| `IAbilityReadModel` | Icon, ready/blocked, cooldown và state cho Presenter |
| `IAbilityCommands` | Begin/update/confirm/cancel theo slot; trả kết quả có lý do, không âm thầm nuốt lỗi |
| `IAimIndicatorView` | Hiển thị resolved aim/profile/color trên hierarchy có sẵn; không tự chọn target |

Tách read/command khi có hai consumer khác nhau, tránh biến chúng thành một interface khổng lồ. Runtime có thể implement cả hai.

### 5.2 Luồng tương tác chuẩn

1. Pointer/keyboard/gamepad gửi intent; UI không gọi trực tiếp skill effect.
2. BeginAim kiểm tra phase, Hero alive/down, slot, cooldown và ownership. Chỉ một aim session điều khiển indicator tại một thời điểm.
3. Quick tap: chọn target hợp lệ theo profile; không có target thì dùng facing/fallback đã định nghĩa. Skill instant/self có thể bỏ qua aim.
4. Drag: đổi screen vector theo hướng camera lên XZ; không mặc định `screen.y == world.z` khi camera có yaw/perspective. Giữ analog pull, không normalize mất khoảng cách.
5. Project target lên ground bằng mask riêng; clamp range; áp dụng kiểm tra sông, vật cản và NavMesh theo loại skill. Không áp cùng luật ground cho skill đánh mục tiêu trên không.
6. Indicator và executor dùng cùng **ResolvedAim** cuối. Khi release phải resolve input cuối trong cùng command, không lấy result còn trễ một frame từ renderer.
7. Confirm kiểm tra lại gate/target/range. Cancel hoặc invalid không tiêu cooldown. Damage không phát sinh từ thao tác hiển thị chỉ dấu.
8. Cooldown bắt đầu tại commit point được định nghĩa cho skill; failure trước commit không tiêu tài nguyên. Callback/nút release lặp không cast lần hai.
9. Mở modal, Hero Down, mất focus, đổi scene/slot hoặc terminal đều kết thúc aim/cast theo policy rõ; không có listener hoặc ring còn treo.

### 5.3 Chỉ dấu 3D

- MVP: line, circle reticle, self circle, dash preview; cone theo nhu cầu skill Thư Sinh đã chốt.
- CurvedTrajectory, VectorWall, RhythmPulse triển khai khi có skill sử dụng, không port đủ 8 shape chỉ để đủ danh sách.
- Tất cả child renderers/mesh/line/material có sẵn trong prefab. Runtime chỉ đổi transform, vertex/position đã cấp buffer, MaterialPropertyBlock và visible state.
- Prefab chỉ dấu dưới root riêng của Hero/session, ngoài Canvas; thiết lập URP depth, ground offset và độ tương phản qua assets.
- Preview dash phải dùng cùng truy vấn đường đi với executor: không hiện điểm đến ở bên kia sông trong khi dash thật bị collider chặn.
- Màu valid/invalid/cancel thể hiện rõ; không chỉ đổi màu mà bỏ qua validity lúc cast.
- Target trên không và ground reticle là hai thông tin khác nhau. AoE cylinder/sphere/ground-only phải là policy của effect; không suy ra từ hình ảnh vòng tròn.

### 5.4 UI controls

- Trích xuất skin 2D thành prefab control đã có Icon, CooldownFill, CooldownText, DisabledOverlay, DragHandler; Relic có StackBadge/RecastGlow nếu cần. Không tự sinh child/component trong Awake hoặc SetCooldown.
- `AbilityButtonPresenter` đề xuất bind slot/read model/commands. Dùng riêng Defense adapter trước, không đổi presenter Legacy đồng loạt.
- `AbilityButtonView` chỉ nhận sprite, fill, formatted text, interactable/reason; không giữ player/session/physics.
- Giữ một command path cho pointer, quick tap, keyboard và gamepad; chống Button.onClick và PointerUp phát cast trùng.
- Pointer ownership theo pointer ID; joystick di chuyển và aim đồng thời; finger thứ hai không chiếm aim của finger thứ nhất.
- Cancel zone nhận reference qua Inspector. Gesture threshold quy đổi theo CanvasScaler; clamp độ nhạy hợp lệ; không tạo WaitForSecondsRealtime mỗi lần giữ nút trong shared implementation mới.
- Lắng nghe HP/EXP/state/slot change bằng event; cooldown fill có thể nội suy bằng UI tick giới hạn, format text chỉ khi phần hiển thị thay đổi.
- Giữ metadata text key ở model, format/render string tại Presenter. Bật raycast ở control hoặc modal blocker có chức năng thực; tắt ở trang trí.

### 5.5 Tương thích Legacy

- Giai đoạn đầu: shared asset và prefab visual sạch; Defense Presenter mới. Giữ API/GUID của code Legacy.
- Khi đưa code gesture/shape math sang shared module: thêm adapter cho Legacy, không sửa ý nghĩa Vector2 ở API cũ.
- Không sửa trực tiếp signature của `ISignatureSkill`, `ISkillAimService`, `IAimIndicatorRenderer` đang được code Legacy gọi rộng rãi.
- `AreaEffectCaster3D` chỉ reuse sau khi chứng minh adapter status/damage tương thích; deduplicate entity, xử lý buffer overflow và layer mask tường minh. Không fallback mask tất cả layer trong production.
- Bộ skill đầu tiên: basic attack + dash + một relic Dép Tổ Ong trên Thư Sinh. Signature của Thư Sinh theo baseline được xác nhận, không tự import toàn bộ mechanic 2D.

## 6. Lộ trình và backlog

Kích thước S/M/L chỉ là tương đối, không quy đổi thành ngày. Mỗi ticket bắt đầu PLANNED; “có core class” hoặc “kiểm tra tĩnh qua” không tự chuyển thành runtime accepted.

### P0 — Khóa baseline và bản đồ phụ thuộc

| ID | Việc làm | Phụ thuộc | Cỡ | Điều kiện hoàn thành |
|---|---|---|---|---|
| RF3D-001 | Inventory tất cả runtime paths, prefab/source GUID, root ownership, snapshot local | — | M | Danh sách file/asset/dependency và manifest/hash trước refactor; không yêu cầu clean toàn working tree |
| RF3D-002 | Đối chiếu core class với adapter production, lập danh sách owner trùng/orphan | 001 | M | Mỗi HP/state/clock/skill/Boss/economy có một owner đích và migration path |
| RF3D-003 | Chốt behavior baseline và các khác biệt skill 2D/Defense | 001 | S | D-02..D-11 giữ nguyên; ghi riêng các câu hỏi sản phẩm ở mục 9 |

### P1 — Session, clock, lifecycle và composition

| ID | Việc làm | Phụ thuộc | Cỡ | Điều kiện hoàn thành |
|---|---|---|---|---|
| RF3D-004 | Composition root có bindings/validation và init/dispose tường minh | 002 | M | Scene refs có sẵn; thiếu dependency báo lỗi cấu hình trước Playing; không tìm global trong combat |
| RF3D-005 | Gameplay gate/clock cho actor, input, cooldown, warning, respawn, pickup | 003,004 | L | Pause dừng cả agent/body/effect; UI unscaled vẫn hoạt động; không clock kép |
| RF3D-006 | Reset registry/ownership theo run generation | 004,005 | L | Reset idempotent; bỏ callback cũ; pickup/tower/wallet/EXP/skill/Boss đều có reset contract |
| RF3D-007 | Rút gọn DefenseRun6003D thành facade kết nối; tách spawn safety/Boss/result/read model | 004,006 | L | Không sở hữu physics-query/Boss attack/UI render trong cùng coordinator; giữ serialized GUID |

### P2 — Hợp đồng damage, stat, targeting và dữ liệu

| ID | Việc làm | Phụ thuộc | Cỡ | Điều kiện hoàn thành |
|---|---|---|---|---|
| RF3D-008 | Chuẩn hóa damage/death/status capability, exactly-once | 004,005 | M | Một health authority/actor; death phân biệt despawn; không cast về Enemy concrete trong Hero/Tower |
| RF3D-009 | Instance target index + target query policy cho ground/air | 006,008 | M | Không state static xuyên session; loại dead/untargetable; kết quả tie-break ổn định |
| RF3D-010 | Stats/balance profiles và modifier evaluation | 003,008 | M | Hero/Tower/ability dùng stat đã tính; không có dictionary modifier không consumer; lưu baseline test riêng production |
| RF3D-011 | Tách Definition khỏi state instance; route/spawn profile validation | 002,010 | M | Runtime không ghi asset chung; sửa Inspector không làm thay đổi snapshot trận đang chạy |

### P3 — Input, skill, aim và UI tái sử dụng

| ID | Việc làm | Phụ thuộc | Cỡ | Điều kiện hoàn thành |
|---|---|---|---|---|
| RF3D-012 | InputAction/joystick/button adapters → một command path | 005 | M | Motor/Builder không đọc Keyboard trực tiếp; input mang full aim; không phụ thuộc enum network Legacy |
| RF3D-013 | Ability definitions/runtime/context và slot bindings | 008,010,012 | L | Một owner cooldown/ability; validate cancel/cast/commit; không PlayerStats/WeaponManager 2D |
| RF3D-014 | Gesture dùng lại, cancel-zone binding và multi-touch ownership | 012,013 | M | Tap/hold/drag/cancel đầy đủ; không singleton, auto child, duplicate dispatch hoặc stale pointer |
| RF3D-015 | Aim projector XZ + ResolvedAim + validity | 009,013 | L | Camera-relative, ground masks, clamp range, target height; renderer/executor dùng chung result |
| RF3D-016 | Prefab indicator 3D và renderer backend được author sẵn | 015 | M | MVP shapes; không new GO/AddComponent/material mỗi lần aim; preview khớp actual effect |
| RF3D-017 | Trích xuất UI skin/control prefab từ 2D, Presenter Defense | 013,014 | M | Nút có icon/fill/text/locked state, serialize đủ; Legacy scene không đổi binding |
| RF3D-018 | Nối basic attack, dash, Dép Tổ Ong vào runtime thật | 005,013,015,016,017 | L | Có command → cast → hit/di chuyển → cooldown; guard modal/down/terminal; không còn core chỉ tồn tại riêng |
| RF3D-019 | Port signature Thư Sinh theo baseline đã chốt | 003,008,010,018 | L | Effect 3D/status/stats và UI cùng owner; không tự thêm toàn bộ skill/roster 2D |

### P4 — Enemy, navigation, spawn và Boss

| ID | Việc làm | Phụ thuộc | Cỡ | Điều kiện hoàn thành |
|---|---|---|---|---|
| RF3D-020 | Factory + lifecycle hooks + authored runtime/pool roots | 006,008,011 | M | Tạo/acquire actor chỉ tại factory; inject context; components có sẵn trong prefab |
| RF3D-021 | Movement/aggro strategies: ground, water, air, landing | 005,009,020 | L | Quyết định D-06 tách FSM/strategy; không cùng bridge walker cho mọi locomotion; 3 Linh Pháo có ticket đầy đủ |
| RF3D-022 | Schedule/planner/safety tách riêng và bảo toàn counts | 007,011,020 | M | 34 events/214 suất + Boss riêng; miniboss #7 và elite replacement giữ nguyên; không mất job khi factory lỗi |
| RF3D-023 | Nối Boss FSM vào encounter adapter, bỏ logic phase song song | 007,008,010,021,022 | L | Telegraph/phase/counterplay có owner; giữ HP arena→assault; result 600s/Overtime/Loss đúng baseline |

### P5 — Economy, progression, towers và pooling

| ID | Việc làm | Phụ thuộc | Cỡ | Điều kiện hoàn thành |
|---|---|---|---|---|
| RF3D-024 | Death reward → physical Wood/EXP pickup → collector → wallet/EXP | 006,008,010,020 | L | Không cộng trực tiếp khi kill; grant một lần/lease; expiry/pause/late collector/terminal rõ |
| RF3D-025 | Progression owns offer và stat modifier; Presenter chỉ render/command | 005,010,024 | L | Kill→EXP→offer→select→buff thật→resume; offer generation chống double click và card cũ sau restart |
| RF3D-026 | Tower build/upgrade/repair command + socket core + prefab instances | 006,010,012,024 | L | Occupancy/tài nguyên một owner; đúng 10 socket/Lv3; reset trạng thái/model/view cùng nhau |
| RF3D-027 | Pool adapters cho Enemy/projectile/pickup/VFX + prewarm budget | 006,020,024 | L | Root author sẵn; reuse reset sạch, không reward khi cleanup, callback lease cũ vô hiệu; không Instantiate/Destroy trong combat ổn định |
| RF3D-028 | Tách HUD read models/presenters, scene navigation command và debug overlay | 007,017,025,026 | M | Không model do UI sở hữu; không format mọi frame; không SceneManager trong gameplay Presenter |

### P6 — Dọn kiến trúc và hồ sơ kiểm chứng

| ID | Việc làm | Phụ thuộc | Cỡ | Điều kiện hoàn thành |
|---|---|---|---|---|
| RF3D-029 | Dependency/namespace/assembly audit, xóa core trùng sau chuyển caller | 018,019,023,025,026,027,028 | M | Không vòng phụ thuộc, không Legacy dependency vô tình; chỉ thêm asmdef khi đã rõ chiều tham chiếu |
| RF3D-030 | Authoring guide, prefab validator opt-in, migration/rollback manifest | 029 | M | Inspector chỉnh được các giá trị; validator chỉ đọc; không auto-regenerate/import tests |
| RF3D-031 | Kiểm tra tĩnh và hồ sơ các tình huống acceptance chưa chạy | 030 | M | Source/schema/GUID/reference checks có report; runtime/device ghi NOT_RUN khi chưa chạy |
| RF3D-032 | Hợp nhất báo cáo tiến độ/backlog và handoff review | 031 | S | Phân biệt code complete, static checked và runtime accepted; không dùng badge/commit thay QA evidence |

Trình tự ưu tiên: **P0 → P1 → P2 → P3 → P4 → P5 → P6**. Có thể soạn config/skin trong khi tách session, nhưng không tích hợp gameplay trước khi clock/gate ổn định. Không đưa tất cả thay đổi vào một commit lớn.

## 7. Hướng chuyển đổi file hiện có

| File/nhóm hiện có | Đích refactor |
|---|---|
| DefenseGameState/GameplayClock/RunLifecycle/G1RuntimeAdapter | Rà contract rồi dùng làm nền session; không viết state machine mới song song |
| DefenseRun6003D | Facade tương thích scene; composition root và các service sở hữu logic; loại bỏ facade chỉ khi đã migrate hết serialized refs |
| DefenseHeroMotor3D/HeroAutoAttack3D/HeroHealth3D | Unity adapters đọc input intent/stat và gameplay gate; combat state thực sự điều khiển cooldown/dash |
| DefenseDepToOngRelic | Port state machine hiện có sang ability slot/executor sau khi so baseline; chuyển constants sang profile |
| Enemy3D/GroundMovement3D/Attack3D/Drop3D/BridgeWalker3D | Giữ health/movement/attack tách biệt; Drop chỉ nhận reward context; Walker là adapter tương thích, không tiếp tục mở rộng mọi archetype vào đây |
| EnemyFactory/EnemyPrefab/EnemyTargets | Factory instance nhận catalog/pool/context; target registry thuộc session; tách Editor-only prefab tooling khỏi đường chạy build |
| StageSpawnConfig/Plan/Scheduler/SpawnPlanner | Giữ snapshot/scheduler/planner; safety NavMesh là service được inject; kiểm tra transactional spawn failure |
| YakshaBehaviorFSM/ArenaCombat3D | FSM sở hữu phase/intent; adapter scene thực thi warning/hit/movement theo tick; một authority |
| ExperienceCore/UpgradeDeck/RewardPipeline | Nối progression/economy instance; grant token theo entity lease; offer lưu trong model; selected modifiers ảnh hưởng stats |
| TowerSocketCore/TowerBuildPrototype3D/TowerPrototype3D | Core giữ occupancy/level/repair; input → command; adapter quản lý 10 prefab instance đã author |
| DefenseMatchPresenter | Tách HUD/upgrade/result/ability presenters và read models; không sở hữu EXP/stat/lifecycle |
| UI/aim Legacy | Shared assets và clean control/geometry extraction với adapter; không đổi hàng loạt API/GUID Legacy |

## 8. Tiêu chí clean code/SOLID và kiểm chứng

### Chất lượng kiến trúc

- **SRP:** lifecycle/spawn/encounter/economy/UI có owner riêng; composition chỉ nối dependency, không trở thành god object mới.
- **OCP:** thêm archetype/skill từ definition + behavior/executor registration; không sửa switch Hero/Boss trung tâm cho mỗi skill mới. Switch cho enum hữu hạn không tự động bị coi là vi phạm nếu không chứa policy mở rộng.
- **LSP:** damageable phải xử lý dead/invalid amount nhất quán; executor/target implementation không âm thầm bỏ contract. Ghi rõ semantics trước khi kết luận tương thích.
- **ISP:** read model không yêu cầu consumer biết write/reset/scene API; target query không bị ép biết toàn bộ stat/UI.
- **DIP:** dependency cụ thể được quyết định ở composition/editor tooling; combat/UI dùng port phù hợp. Serialize MonoBehaviour binding rồi kiểm tra interface ở composition nếu Inspector không serialize interface trực tiếp.
- Naming `_camelCase`, Allman, tài liệu public API; không dùng số dòng/class count làm chỉ số duy nhất cho clean code.
- Missing configuration phải có thông báo/path cụ thể; không tiếp tục Playing với dependency bắt buộc thiếu, không sửa lỗi bằng runtime auto-create.

### Kiểm chứng trong thời gian người dùng yêu cầu không chạy Unity

- Đọc graph caller và ownership, so manifest GUID/serialized refs, parse YAML/unique IDs/local refs; kiểm tra schema assets và dependency direction.
- Nếu có bộ core được tách chạy bằng công cụ .NET độc lập sau này, ghi rõ phạm vi đó; không giả định tất cả core hiện có biên dịch ngoài Unity.
- Không chạy Unity Editor, batchmode, EditMode/PlayMode test, không giả lập click nút trong scene. Không để InitializeOnLoad kích hoạt test hoặc rebuild asset.
- Không tuyên bố cast/indicator/physics/visual đã đúng từ kết quả tĩnh. Hồ sơ runtime acceptance ở dưới giữ NOT_RUN trong giai đoạn này.

### Tình huống runtime cần có trong hồ sơ bàn giao, chưa thực hiện

| Nhóm | Tình huống cần chứng minh |
|---|---|
| State/time | Pause/LevelUp đóng băng combat, NavMesh, movement, cooldown và pickup expiry; UI vẫn tương tác |
| Skill | Tap/drag/cancel; release frame cuối; hai ngón; mất focus; cooldown/down/terminal; không cast trùng hoặc tiêu cooldown khi cancel |
| Aim 3D | Camera yaw/perspective, địa hình cao thấp, sông/tường, target air, hết tầm, target chết trước confirm; preview khớp hit thật |
| Progression | Nhiều level xếp hàng, damage/speed/range/cooldown buff thực, không offer sau Win/Loss |
| Economy/tower | Wood/EXP riêng; một death một reward; chi phí build/repair một lần, 10 sockets, Lv3 |
| Lifecycle/pool | Ít nhất 3 restart, domain reload settings phù hợp, old lease không hit/reward/giảm cap của run mới |
| Full match | 600s, Boss chết sớm/Overtime/Base chết cùng tick, bridge/air/water/landing; Legacy regression |
| Device | Touch/input, độ rõ chỉ dấu, frame time/GC/memory theo thiết bị định danh; không suy FPS từ desktop |

Các tình huống này là yêu cầu nghiệm thu, không phải chỉ thị chạy Unity hiện tại.

## 9. Các quyết định sản phẩm cần ghi nhận trước khi triển khai skill

**Cập nhật triển khai 27/10:** gói Player Action Thư Sinh đã chọn prototype manual Attack trong scene, owner auto vẫn cấu hình được và dùng chung CD; Signature self-AoE1.5× damage + giảm20% relic-CD còn lại, bỏ dependency Ngũ Hành. Đây là lựa chọn triển khai công khai trong lượt “Bắt đầu”, chưa phải kết luận balance/UX đã nghiệm thu. Xem tài liệu `15_HERO_ACTION_AND_SKILL_UI.md` và progress; các mục bên dưới ghi bối cảnh lịch sử trước quyết định này.


1. **Signature Thư Sinh:** source 2D hiện là Phán Quyết với buff/ngũ hành; tài liệu lịch sử có tên/cơ chế khác. Không tự chọn một biến thể làm baseline Defense chỉ vì đã có class.
2. **Dép Tổ Ong MVP:** core Defense đang mô tả target point + AoE; Legacy có boomerang/curved preview. Chốt effect thật là projectile quay về hay impact theo điểm trước khi làm renderer CurvedTrajectory.
3. **Cooldown khi Downed:** tách rõ “không được cast” và “cooldown có tiếp tục chạy”. Core relic hiện đóng băng khi Downed; các skill khác chưa thống nhất. Giữ baseline từng skill cho đến khi chốt, không suy ra từ pause policy.
4. **Manual/auto basic attack và skill slots:** giữ hành vi auto-attack hiện có làm baseline refactor; không âm thầm đổi sang bắt buộc bấm nút. Thiết kế command path hỗ trợ manual override/aim khi được cấu hình.

Các quyết định này không cản bước inventory/session/gate. Không nhập cả roster, balance hoặc Ngũ Hành sâu vào phạm vi refactor Core.

## 10. Cách chia gói thay đổi và rollback

- Mỗi gói theo một luồng behavior: session/gate; damage/targets/stats; input/aim/ability MVP; UI bind; Enemy/Boss; progression/economy/pool. Mỗi gói ghi rõ source/asset/scene tác động.
- Giữ facade/adapter cũ chỉ trong thời gian có caller thật; đánh dấu deprecated với ticket tháo bỏ, không giữ hai hệ thống cùng tick một actor.
- Thêm serialized fields trước, migrate binding có manifest, rồi mới bỏ field cũ. Không đổi ý nghĩa field mà giữ tên khiến asset cũ đọc sai.
- Các scene/prefab author thủ công phải được bảo toàn; công cụ editor chỉ chạy khi gọi rõ ràng, ưu tiên validate/repair reference và không ghi đè layout đã chỉnh.
- Rollback bằng bản sao/commit theo đúng file của gói. Không reset/clean toàn repository và không ghi đè dirty asset không thuộc gói.
- Handoff gồm bảng trạng thái từng RF3D, dependency map, manifest GUID/source và các bước nghiệm thu chưa chạy. Không đánh toàn dự án “SOLID 100%”.

## 11. Nguồn mã đã đối chiếu

- `Assets/Features/Defense3D/DefenseRun6003D.cs`, `DefenseHeroMotor3D.cs`, `DefenseHeroCombatState.cs`, `DefenseDepToOngRelic.cs`, `DefenseEnemyDrop3D.cs`, `DefenseEnemyTargets3D.cs`, `DefenseRewardPipeline.cs`, `UI/DefenseMatchPresenter.cs`.
- `Assets/Features/Combat/Aiming/{SkillAimConfig,AimResult,IAimIndicatorRenderer,SkillAimIndicatorController,AutoTargetScanner}.cs` và các renderer.
- `Assets/Features/UI/{AttackButtonView,DashButtonView,SignatureSkillButtonView,RelicSkillButtonView,AttackButtonPresenter,DashButtonPresenter,SignatureSkillPresenter,RelicSkillPresenter}.cs`.
- `Assets/Features/UI/Controls/{SmartSkillDragHandler,UICancelSkillZone}.cs`.
- `Assets/Features/Player/Skills/{ISignatureSkill,SignatureSkillBase,SignatureSkillManager,SignatureSkillData,ThuSinhSignatureSkill,ThanhDongSignatureSkill,VoTangSignatureSkill}.cs`.
- `Assets/Features/Player/Core/{IPlayerInputProvider,PlayerInputReader}.cs`, `Assets/Features/Shared/{TargetingUtility,AreaEffectCaster3D}.cs`, `Assets/Core/Pooling/{IPoolable,PoolHierarchyManager}.cs`.
- `Assets/Scenes/SampleScene.unity`, scene/prefabs Defense3D hiện tại, `.agents/AGENTS.md`, `GameDesignDoc/TREBRAVE_Unity3D/00_MASTER_PLAN.md`, `02_IMPLEMENTATION_BACKLOG.md`, `ProjectManagement/DEFENSE3D_AUTHORING_GUIDE.md`.
