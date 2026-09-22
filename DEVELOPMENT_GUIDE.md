# PROJECTZOMBIE – DEVELOPER TEAM WORKING GUIDE

**Project:** Vong Xuyên / Projectzombie
**Engine:** Unity 2022.3 LTS
**Repository:** `Projectzombie`
**Mục đích:** Chuẩn hóa cách team lập trình, review code, phát triển tính năng, sửa bug và tích hợp code vào nhánh chính.

---

# 1. Mục tiêu tài liệu

Tài liệu này là quy chuẩn chung cho tất cả developer tham gia Projectzombie.

Mọi thành viên cần tuân thủ để:

* Hạn chế conflict khi nhiều người cùng phát triển.
* Giữ kiến trúc project nhất quán.
* Tránh tạo dependency sai giữa các module.
* Giảm bug regression.
* Dễ review Pull Request.
* Đảm bảo gameplay single-player và multiplayer không phá lẫn nhau.
* Giữ hiệu năng ổn định trên mobile.
* Dễ mở rộng game về sau.

Nguyên tắc quan trọng nhất:

> Không chỉ viết code để tính năng chạy được. Code phải có khả năng được người khác đọc, sửa, test và mở rộng.

---

# 2. Cấu trúc project

Developer phải đặt code đúng module.

```text
Assets/
├── Core/
│   ├── Architecture/
│   ├── Audio/
│   ├── Pooling/
│   ├── Save/
│   └── Services/
│
├── Features/
│   ├── Boss/
│   ├── Collectibles/
│   ├── Combat/
│   ├── Elements/
│   ├── Enemies/
│   ├── Maps/
│   ├── MatchFlow/
│   ├── MetaProgression/
│   ├── Multiplayer/
│   ├── Player/
│   ├── Projectiles/
│   ├── Shared/
│   ├── Skills/
│   ├── Spawners/
│   ├── Startup/
│   ├── UI/
│   ├── Upgrades/
│   ├── Weapons/
│   └── YinYang/
│
├── _Data/
├── _Prefabs/
├── Resources/
├── AddressableAssetsData/
├── Tests/
└── Editor/
```

## 2.1 Core

`Core` chứa infrastructure hoặc hệ thống dùng xuyên suốt game.

Ví dụ:

* Object Pool
* Audio
* Save
* Addressables
* Service registry
* Architecture utilities

Không đưa logic gameplay đặc thù vào `Core`.

Sai:

```text
Core/
└── SwordAttack.cs
```

Đúng:

```text
Features/
└── Weapons/
    └── SwordAttack.cs
```

---

# 3. Feature-based architecture

Các tính năng gameplay phải nằm trong `Features`.

Ví dụ:

```text
Features/
├── Player
├── Enemies
├── Weapons
├── Upgrades
└── Multiplayer
```

Một feature nên cố gắng tự quản lý:

* runtime code;
* interfaces nội bộ;
* config;
* presenter;
* mechanic;
* domain logic.

Không tạo dependency vòng.

Ví dụ không được:

```text
Player
 ↓
Weapons
 ↓
Player
```

Ưu tiên giao tiếp qua:

```text
Interface
Event
Command
Context
Service abstraction
```

---

# 4. Dependency direction

Code phải tuân theo dependency theo hướng:

```text
UI
 ↓
Application / Flow
 ↓
Gameplay Domain
 ↓
Infrastructure
```

Multiplayer là adapter của gameplay:

```text
Multiplayer
      ↓
Gameplay
```

Gameplay không được phụ thuộc trực tiếp vào Photon/Fusion nếu không thật sự cần thiết.

Ví dụ không nên:

```csharp
if (Runner.IsServer)
{
    ApplyDamage();
}
```

bên trong class combat domain.

Nên tách:

```text
Network layer
    ↓
Damage command
    ↓
Combat system
```

---

# 5. Quy tắc phát triển tính năng mới

Trước khi code, developer phải xác định:

1. Feature thuộc module nào?
2. Có thể tái sử dụng component hiện tại không?
3. Data có nên là `ScriptableObject` không?
4. Có cần event/interface không?
5. Có ảnh hưởng multiplayer không?
6. Có chạy trong combat hot-loop không?
7. Có cần pooling không?
8. Có cần Addressables không?

Không được tạo class mới chỉ vì chưa tìm code cũ.

Developer phải search project trước khi implement.

---

# 6. Data-driven gameplay

Các giá trị gameplay không nên hard-code nếu designer cần chỉnh.

Không nên:

```csharp
damage = 50;
cooldown = 2.5f;
speed = 8f;
```

Nên lấy từ config:

```csharp
damage = weaponData.damage;
cooldown = weaponData.cooldown;
speed = projectileData.speed;
```

Các loại dữ liệu nên dùng `ScriptableObject`:

* Weapon stats
* Enemy stats
* Projectile stats
* Upgrade config
* Stage config
* Character config
* Boss config
* Wave config
* Meta progression config

Mục tiêu:

> Designer có thể balance game mà không sửa C#.

---

# 7. Quy tắc Singleton

Không tạo Singleton mới nếu không thật sự cần.

Các singleton hiện tại phải được xem là infrastructure hoặc global coordinator.

Không nên:

```csharp
public static EnemyManager Instance;
public static WeaponDatabase Instance;
public static DamageManager Instance;
public static CriticalManager Instance;
```

Chỉ dùng singleton khi object:

* tồn tại duy nhất toàn game;
* có lifecycle rõ ràng;
* thật sự là global service.

Ưu tiên:

```text
Dependency Injection
ServiceContext
Serialized reference
Context object
Interface
```

---

# 8. Không dùng Find trong runtime hot-path

Không được dùng trong `Update`, `FixedUpdate`, attack loop hoặc spawn loop:

```csharp
FindObjectOfType<T>()
GameObject.Find()
FindObjectsOfType<T>()
GetComponent<T>()
```

Mỗi frame.

Component phải được cache trong:

```text
Awake
Start
Initialize
Spawned
```

Ví dụ:

```csharp
private Rigidbody2D _rigidbody;

private void Awake()
{
    _rigidbody = GetComponent<Rigidbody2D>();
}
```

---

# 9. Object Pooling

Các object xuất hiện nhiều trong combat phải dùng pool.

Bắt buộc xem xét pooling với:

* Enemy
* Projectile
* EXP gem
* Damage popup
* VFX
* Temporary hitbox
* Audio emitter

Không được liên tục:

```csharp
Instantiate(...)
Destroy(...)
```

trong combat loop.

Nếu object xuất hiện hàng chục/hàng trăm lần trong một trận, mặc định coi đó là ứng viên cho pooling.

---

# 10. Physics

Trong các hệ thống scan nhiều target, ưu tiên API NonAlloc.

Không nên:

```csharp
Physics2D.OverlapCircleAll(...)
```

trong loop liên tục.

Ưu tiên:

```csharp
Physics2D.OverlapCircleNonAlloc(...)
```

hoặc utility chung của project.

Phải sử dụng `LayerMask` rõ ràng.

Không kiểm tra tag/string hàng trăm lần mỗi frame nếu có thể dùng layer hoặc interface.

---

# 11. Game State

Gameplay runtime phải tôn trọng `GameStateManager`.

Ví dụ system update gameplay nên có guard phù hợp:

```csharp
if (!GameStateManager.IsPlaying)
{
    return;
}
```

Không tự ý set:

```csharp
Time.timeScale = 0;
```

ở component bất kỳ.

Việc thay đổi state phải đi qua state manager hoặc flow coordinator.

Ví dụ:

```text
Playing
Paused
LevelUpSelection
GameOver
MainMenu
```

---

# 12. Multiplayer

Multiplayer dùng mô hình host/state-authoritative.

Developer phải phân biệt:

```text
Input Authority
State Authority
Remote Proxy
```

Không được để client tự quyết định những trạng thái gameplay quan trọng như:

* damage;
* death;
* drop;
* enemy spawn;
* reward;
* game result.

Client có thể prediction cho:

* movement;
* animation;
* visual feedback.

Authoritative side quyết định gameplay state.

---

# 13. Không viết hai gameplay system khác nhau cho SP và MP

Không làm:

```text
SinglePlayerWeapon.cs
MultiplayerWeapon.cs
```

nếu logic gameplay cơ bản giống nhau.

Đúng hơn:

```text
Weapon
   ↑
Local input

Weapon
   ↑
Network command
```

Gameplay domain phải được tái sử dụng tối đa.

---

# 14. Network Player

Không tiếp tục nhồi logic mới vào một Network Player root nếu có thể tách component.

Nên chia trách nhiệm:

```text
NetworkPlayerRoot
├── NetworkPlayerMovement
├── NetworkPlayerCombat
├── NetworkPlayerVitals
├── NetworkPlayerLoadout
└── NetworkPlayerPresentation
```

Một component không nên đồng thời quản lý:

* movement;
* HP;
* animation;
* loadout;
* network;
* input;
* UI;
* revive.

---

# 15. Addressables

Runtime asset mới cần xác định rõ:

```text
Built-in
Local Addressable
Remote Addressable
Resources fallback
```

Không tùy tiện thêm asset lớn vào `Resources`.

Ưu tiên Addressables cho:

* map;
* enemy prefab;
* weapon prefab;
* audio;
* DLC;
* live-ops content.

`Resources` chỉ nên dùng cho fallback hoặc các tài nguyên system bắt buộc.

---

# 16. Addressable keys

Address phải có naming rõ ràng và ổn định.

Ví dụ:

```text
Stage_01_BambooForest
Enemy_MaDa
Enemy_MaTroi
Weapon_NoThanCo
Upgrade_FireDamage_01
UI_LevelUpPanel
```

Không dùng:

```text
NewEnemy
Test1
Prefab2
abc
FinalFinal
```

Address key đã được release phải hạn chế đổi tùy tiện vì remote catalog có thể đang tham chiếu.

---

# 17. Naming convention

Class:

```csharp
PlayerController
WeaponManager
EnemySpawner
UpgradePresenter
```

Method:

```csharp
StartMatch()
ApplyDamage()
LoadStageAsync()
ResetState()
```

Private field:

```csharp
_currentHealth
_moveSpeed
_weaponManager
```

Property:

```csharp
CurrentHealth
IsAlive
EquippedWeapon
```

Interface:

```csharp
IDamageable
IPlayerRegistry
INetworkSessionService
```

ScriptableObject:

```csharp
WeaponData
EnemyConfig
StageDefinitionSO
```

---

# 18. Không dùng tên mơ hồ

Không dùng:

```text
Manager2
NewSystem
TempController
Helper
Utils2
TestScript
FinalScript
```

Tên class phải mô tả đúng trách nhiệm.

---

# 19. Một class – một trách nhiệm chính

Nếu class bắt đầu có các nhóm logic hoàn toàn khác nhau:

```text
Movement
Audio
Save
Network
UI
Damage
Animation
```

thì phải xem xét tách.

Một dấu hiệu khác:

* class > 500 dòng;
* có quá nhiều serialized fields;
* phụ thuộc > 10 subsystem;
* Awake/Start rất dài;
* có nhiều `if multiplayer`;
* có nhiều region không liên quan.

Không bắt buộc tách chỉ vì số dòng, nhưng cần review thiết kế.

---

# 20. Event-driven communication

Ưu tiên event khi một hệ thống chỉ cần thông báo trạng thái.

Ví dụ:

```text
Enemy dies
   ↓
OnEnemyKilled
   ↓
RunStatsTracker
UI
DropSystem
QuestSystem
```

Không nên để `Enemy` gọi trực tiếp:

```csharp
UIManager.Instance.UpdateKills();
QuestManager.Instance.AddKill();
AnalyticsManager.Instance.LogKill();
```

---

# 21. Async

Hàm async phải có hậu tố:

```csharp
LoadMapAsync()
DownloadPatchAsync()
StartMatchAsync()
```

Không dùng `async void` trừ:

* Unity event;
* UI callback;
* trường hợp có lý do rõ ràng.

Ưu tiên:

```csharp
async Task
```

Exception async phải được xử lý hoặc propagate có chủ đích.

---

# 22. Null handling

Không dùng null check để che dependency bị thiếu.

Ví dụ này:

```csharp
if (_weaponManager != null)
{
    ...
}
```

chỉ hợp lệ khi component thực sự optional.

Nếu bắt buộc phải có:

```csharp
[RequireComponent(typeof(WeaponManager))]
```

hoặc validation.

Developer cần phân biệt:

```text
Optional dependency
Required dependency
```

---

# 23. Logging

Không spam log trong:

```text
Update
FixedUpdate
Render
Projectile tick
Enemy movement
```

Log nên có context:

```csharp
Debug.Log("[SpawnManager] Match started.");
```

Warning:

```csharp
Debug.LogWarning("[Addressables] Stage config not found.");
```

Error:

```csharp
Debug.LogError("[WeaponManager] Missing WeaponData.");
```

Debug log tạm phải xóa trước khi merge.

---

# 24. Git branch strategy

Không code trực tiếp trên `main`.

Branch format:

```text
feature/<feature-name>
fix/<bug-name>
refactor/<system-name>
chore/<task-name>
```

Ví dụ:

```text
feature/boss-ground-slam
feature/coop-revive
fix/player-dash-cooldown
refactor/network-player
chore/addressable-cleanup
```

---

# 25. Quy trình làm task

Mỗi task nên đi theo flow:

```text
Pull latest main
      ↓
Create branch
      ↓
Implement
      ↓
Self-test
      ↓
Commit
      ↓
Push
      ↓
Create PR
      ↓
Code review
      ↓
Fix comments
      ↓
Merge
```

Trước khi bắt đầu:

```bash
git checkout main
git pull
git checkout -b feature/<task>
```

---

# 26. Commit convention

Commit message phải mô tả thay đổi.

Format khuyến nghị:

```text
feat: add boss ground slam attack
fix: prevent enemy attack while paused
refactor: split network player combat
perf: pool projectile VFX
test: add element synergy tests
docs: update multiplayer architecture
chore: update addressable config
```

Không dùng:

```text
update
fix
done
test
abc
final
final2
```

---

# 27. Một commit nên có một ý nghĩa

Không gom:

```text
add boss
fix UI
change audio
refactor player
update map
```

vào một commit.

Commit nhỏ giúp:

* review dễ;
* revert dễ;
* tìm regression dễ;
* cherry-pick dễ.

---

# 28. Pull Request

Mỗi PR cần có:

```text
Summary
Changes
How to test
Risk
Screenshots/video nếu liên quan UI/gameplay
```

Ví dụ:

```text
Summary:
Thêm Ground Slam cho Boss Ngưu Đầu.

Changes:
- thêm GroundSlamState;
- thêm radius/damage vào BossConfig;
- thêm pooled VFX;
- kết nối vào BossStateMachine.

How to test:
1. Mở Gameplay scene.
2. Spawn Boss.
3. Giữ khoảng cách < 4m.
4. Boss phải thực hiện Ground Slam.
5. Pause game và xác nhận Boss dừng.

Risk:
Ảnh hưởng BossStateMachine và AreaEffectCaster.
```

---

# 29. PR phải nhỏ

Không tạo PR vài chục nghìn dòng nếu có thể tách.

Mục tiêu:

```text
1 PR = 1 feature hoặc 1 mục tiêu kỹ thuật
```

Một PR càng nhỏ càng dễ review chính xác.

---

# 30. Checklist trước khi tạo PR

Developer phải tự kiểm tra:

* Project compile không error.
* Không còn debug code.
* Không có temporary file.
* Không có `FindObjectOfType` trong hot-loop.
* Không tạo Instantiate/Destroy spam.
* Không có hard-coded gameplay value không cần thiết.
* Không duplicate class/interface.
* Không phá single-player.
* Nếu liên quan gameplay, test multiplayer.
* Nếu thêm asset, kiểm tra Addressables.
* Nếu thêm serialized field, prefab/scene đã được cập nhật.
* Không commit Library/Temp/build artifacts.

---

# 31. Code Review

Reviewer kiểm tra theo thứ tự:

```text
Correctness
Architecture
Regression
Performance
Readability
Style
```

Không chỉ comment spacing hoặc naming mà bỏ qua lỗi kiến trúc.

Reviewer nên hỏi:

* Đây có đúng module không?
* Có duplicate system hiện có không?
* Có coupling không cần thiết không?
* Có ảnh hưởng multiplayer không?
* Có tạo GC trong hot path không?
* Có dùng pooling chưa?
* Data có nên nằm trong ScriptableObject không?
* Có test edge case chưa?

---

# 32. Review comment

Comment cần cụ thể.

Không nên:

> Code này xấu.

Nên:

> `NetworkPlayerCharacter` hiện đã quản lý health và movement. Logic weapon replication mới nên tách sang `NetworkPlayerLoadout` để tránh tiếp tục tăng trách nhiệm của root component.

Developer và reviewer tranh luận dựa trên kỹ thuật, không dựa trên cá nhân.

---

# 33. Bug workflow

Bug phải có tối thiểu:

```text
Description
Steps to reproduce
Expected
Actual
Environment
Frequency
Screenshot/video/log
```

Ví dụ:

```text
Bug:
Enemy vẫn tấn công khi mở LevelUp UI.

Steps:
1. Start Stage 1.
2. Đứng gần enemy.
3. Level up.
4. Không chọn card.
5. Quan sát HP.

Expected:
Enemy dừng trong single-player.

Actual:
Enemy tiếp tục attack.

Frequency:
100%.
```

---

# 34. Bug priority

## P0 – Critical

* Game không launch.
* Save corruption.
* Crash.
* Không vào được match.
* Multiplayer room unusable.

## P1 – High

* Combat feature chính không hoạt động.
* Player không điều khiển được.
* Boss không thể hoàn thành.
* Serious multiplayer desync.

## P2 – Medium

* UI bug.
* Visual bug.
* Minor gameplay inconsistency.

## P3 – Low

* Polish.
* Cosmetic.
* Small QoL.

---

# 35. Testing

Các domain logic có thể test độc lập nên có automated test.

Ví dụ:

```text
Element synergy
Damage calculation
Upgrade filtering
XP calculation
Drop probability
Stat modifiers
```

Không cần mọi MonoBehaviour đều có unit test.

Ưu tiên test logic dễ regression và có tính toán.

---

# 36. Manual test

Feature gameplay tối thiểu phải test:

```text
Fresh run
Pause/unpause
Restart
Game over
Level up
Scene reload
Low FPS
Android nếu liên quan performance
```

Nếu hệ thống dùng multiplayer:

```text
Host
Client
Player join
Player leave
Disconnect
Reconnect nếu support
Host/client death
State replication
```

---

# 37. Performance budget

Project target:

```text
60 FPS mobile
```

Developer phải đặc biệt chú ý những code chạy với:

* 100+ enemies;
* 100+ projectiles;
* particle/VFX;
* physics;
* network tick.

Không optimize mù.

Khi nghi ngờ performance:

```text
Unity Profiler
Memory Profiler
Frame Debugger
Network statistics
```

phải được dùng để đo.

---

# 38. Không tối ưu quá sớm nhưng không được viết hot-loop tệ

Không cần micro-optimize menu UI.

Nhưng trong combat:

```csharp
void Update()
{
    FindObjectsOfType<Enemy>();
}
```

là không chấp nhận được ngay từ đầu.

---

# 39. Scene và Prefab

Hạn chế nhiều developer sửa cùng một scene lớn.

Logic nên nằm trong prefab/component thay vì chỉnh scene liên tục.

Khi sửa scene/prefab:

* pull mới nhất;
* kiểm tra conflict;
* không save unrelated objects;
* không apply prefab thay đổi ngoài task.

---

# 40. Meta file

Không xóa `.meta` thủ công nếu không hiểu ảnh hưởng.

Unity GUID phụ thuộc `.meta`.

Mất `.meta` có thể làm mất reference của:

* prefab;
* material;
* ScriptableObject;
* Addressables;
* animation.

---

# 41. Editor scripts

Tool chỉ chạy trong Unity Editor phải nằm trong:

```text
Editor/
```

hoặc assembly Editor riêng.

Không để `UnityEditor` namespace lọt vào runtime build nếu không có guard.

Nếu bắt buộc:

```csharp
#if UNITY_EDITOR
...
#endif
```

---

# 42. Documentation

Các system lớn cần README hoặc architecture note.

Đặc biệt:

* Multiplayer
* Weapon system
* Upgrade system
* Addressables
* Save
* Boss framework

Tài liệu cần trả lời:

```text
System làm gì?
Entry point ở đâu?
Data nằm ở đâu?
Flow thế nào?
Extension point ở đâu?
Những điều không được làm?
```

---

# 43. Definition of Done

Task chỉ được xem là hoàn thành khi:

```text
Code hoàn thành
+
Compile thành công
+
Self-test thành công
+
Không có regression rõ ràng
+
PR được review
+
Comment đã resolve
+
Documentation được cập nhật nếu cần
+
Merge vào main
```

“Code chạy trên máy tôi” chưa phải Done.

---

# 44. Quy tắc thay đổi architecture

Developer không tự ý tạo framework lớn hoặc đổi architecture trong một feature PR.

Nếu muốn thay đổi các thành phần như:

```text
GameState
ServiceContext
Damage system
Save system
Network architecture
Addressables architecture
```

cần:

1. mô tả vấn đề;
2. đề xuất thiết kế;
3. impact analysis;
4. thống nhất với lead;
5. sau đó mới implement.

---

# 45. Technical Debt

Nếu phát hiện vấn đề nhưng không nằm trong scope task:

Không cần sửa luôn nếu làm PR phình lớn.

Tạo technical debt task.

Ví dụ:

```text
TECH-021
Merge duplicate IDamageable interfaces.

TECH-022
Split NetworkPlayerCharacter responsibilities.

TECH-023
Abstract multiplayer pause policy from GameStateManager.
```

Điều này tốt hơn việc âm thầm refactor nửa project trong PR nhỏ.

---

# 46. Quy tắc “Boy Scout”

Khi sửa một vùng code, có thể làm vùng đó tốt hơn một chút:

* đổi tên biến khó hiểu;
* xóa dead code;
* cache component;
* thêm validation;
* cập nhật comment sai.

Nhưng không biến task thành refactor toàn hệ thống.

---

# 47. Developer ownership

Mỗi system lớn nên có một owner hoặc maintainer chính.

Ví dụ:

```text
Player / Combat      → Dev A
Enemies / Boss       → Dev B
Weapons / Upgrades   → Dev C
UI / Meta            → Dev D
Multiplayer          → Dev E
Build / Addressables → Dev Lead
```

Owner không có nghĩa chỉ người đó được sửa code.

Owner chịu trách nhiệm:

* giữ kiến trúc nhất quán;
* review thay đổi lớn;
* hỗ trợ developer khác;
* cập nhật documentation.

---

# 48. Daily workflow đề xuất

Đầu ngày:

```text
Pull main
Check assigned tasks
Check PR comments
Resolve blocker
```

Trong ngày:

```text
Implement nhỏ từng phần
Test thường xuyên
Commit theo milestone nhỏ
```

Cuối ngày:

```text
Push branch
Cập nhật task
Ghi blocker
Không để thay đổi quan trọng chỉ nằm local
```

---

# 49. Khi bị blocker

Developer không nên ngồi quá lâu với blocker mà không báo.

Khi báo blocker cần gửi:

```text
Mục tiêu
Đã thử gì
Hiện lỗi gì
Log/screenshot
File liên quan
Giả thuyết hiện tại
```

Không chỉ nhắn:

> Code lỗi anh ơi.

---

# 50. Nguyên tắc cuối cùng

Mọi developer của Projectzombie cần nhớ:

> Ưu tiên code rõ ràng hơn code thông minh.

> Gameplay logic phải tách khỏi UI và network khi có thể.

> Data balance phải chỉnh được mà không sửa code.

> Không thêm dependency global nếu chưa cần.

> Không tạo duplicate system.

> Không merge code chưa test.

> Tối ưu những nơi thực sự chạy nhiều trong combat.

> Multiplayer không được làm hỏng single-player và ngược lại.

> Mọi hệ thống phải được thiết kế để developer tiếp theo có thể hiểu và mở rộng.

---

# Quick PR Checklist

```text
[ ] Đúng branch
[ ] Đúng module
[ ] Compile OK
[ ] Self-test OK
[ ] Không debug/temp code
[ ] Không duplicate system
[ ] Không dependency vòng
[ ] Không GC đáng kể trong hot-loop
[ ] Pooling nếu object spawn nhiều
[ ] Gameplay data không hard-code
[ ] Test pause/game state
[ ] Test multiplayer nếu liên quan
[ ] Addressables đúng nếu thêm asset
[ ] PR description đầy đủ
[ ] Documentation cập nhật nếu cần
```

---

**Document owner:** Technical Lead / Project Lead
**Applies to:** Tất cả developer Projectzombie
**Review frequency:** Khi architecture hoặc workflow chính thay đổi
