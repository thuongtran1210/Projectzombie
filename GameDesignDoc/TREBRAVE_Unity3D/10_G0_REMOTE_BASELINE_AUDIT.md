# Gate G0 — Remote Baseline Audit / Local Safety Blockers

**Thời điểm audit:** 2026-10-23 · **Nguồn:** GitHub `thuongtran1210/Projectzombie`, branch `feat/trebrave-defense3d-first-scene`.
**Trạng thái hiện tại:** `GIT_SNAPSHOT_OWNER_VERIFIED / SIX_CRITICAL_ASSET_HASHES_MATCH / G0_AWAITING_OWNER_SIGNOFF / NO_G1_CODE`. Các mục lịch sử bên dưới ghi trạng thái tại thời điểm nhận từng bằng chứng; xem mục "Bằng chứng mới nhất" để biết kết luận cập nhật.
**Phạm vi assistant đã làm:** chỉ đọc repo/manifest/Unity editor scripts trên GitHub và cập nhật tài liệu. **Người dùng đã tự chạy backup local copy-only và báo cáo 8 file khớp hash**, không phải assistant có quyền truy cập ổ đĩa. **Không mở Unity Editor, không Save Scene, không ghi GLB/NavMesh, không code gameplay.**

## Bằng chứng mới nhất — Git snapshot và 6 critical asset hashes (owner PowerShell)

**Nguồn:** chủ dự án gửi output PowerShell đọc trực tiếp từ `C:\Users\thuon\Unity\Projectzombie` và backup `C:\Users\thuon\Unity\TREBRAVE_G0_Backup_20261023_211614`. Đây là owner-generated output, **không phải assistant chạy trên local machine**. Kết luận mới dưới đây **thay thế trạng thái chờ trong các ghi chép theo trình tự thời gian phía sau**, giữ lịch sử audit cũ để truy vết.

**G0-S1 — OWNER_OUTPUT_VERIFIED:** `git branch --show-current` và `git-branch.txt` cùng trả `feat/trebrave-defense3d-first-scene`. `git rev-parse HEAD` và `git-head.txt` cùng trả **`43ee2a806c593afe8704730f9daf012b52f56b35`**. `git stash list` và `git-stash.txt` cùng hiển thị:
- `stash@{0}: On feat/trebrave-defense3d-first-scene: Backup DefenseRun6003D Fast Test`
- `stash@{1}: On feat/trebrave-defense3d-first-scene: backup local DefenseRun6003D Fast Test`

`git status --short` vẫn có **17 `M` + 8 `??` entries**, gồm `DefenseRun6003D.cs` modified, hàng loạt `.meta` modified, untracked `Assets/Scenes/Defense3D/` và `Assets/Art/Environment/`. **Không so sánh HEAD local với tip mới nhất của remote branch**; các docs đã được cập nhật trên GitHub sau bản local snapshot, không được kết luận 2 bản đồng bộ. **Không pull/checkout/clean/reset hoặc stash pop.**

**G0-S2 / G0-S3 — CRITICAL_6_HASH_VERIFIED (owner output):** 6 đường dẫn sau đều có **`SourceExists=True; BackupExists=True; HashMatches=True`** sau so sánh SHA-256 nguồn và bản sao (script `Get-FileHash`):

| Phạm vi | Đường dẫn từ project root | SHA-256 source vs backup |
|---|---|---|
| Scene | `Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity` | **MATCH** |
| Scene meta | `Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity.meta` | **MATCH** |
| NavMesh | `Assets/Scenes/Defense3D/MapSongQue3D_Prototype/NavMesh-Defense_Navigation.asset` | **MATCH** |
| NavMesh meta | `Assets/Scenes/Defense3D/MapSongQue3D_Prototype/NavMesh-Defense_Navigation.asset.meta` | **MATCH** |
| GLB | `Assets/Art/Environment/MapSongQue/TREBRAVE_MapSongQue_ThreeJS.glb` | **MATCH** |
| GLB meta | `Assets/Art/Environment/MapSongQue/TREBRAVE_MapSongQue_ThreeJS.glb.meta` | **MATCH** |

**G0-S4:** đã xác minh từ output trước `Packages/manifest.json`, `Packages/packages-lock.json` và `ProjectSettings/EditorBuildSettings.asset` có backup + SHA-256 khớp (3/3). Chưa hash toàn bộ `Packages/`, `ProjectSettings/`, importer/material và chưa có thử khôi phục thực tế. **G0-S5/S6** vẫn **OWNER_REPORTED_VERIFIED** (Console ảnh 0 Error, người dùng xác nhận map/Legacy), không nâng thành độc lập full-run QA.

**Current decision:** G0-S1 = `OWNER_OUTPUT_VERIFIED`; G0-S2/S3 = `CRITICAL_ASSET_HASH_VERIFIED` (**6/6 đúng bản sao**, không chứng minh backup của mọi asset liên quan); G0-S4 = `THREE_KEY_FILES_HASH_VERIFIED / PARTIAL`; G0-S5/S6 = `OWNER_REPORTED_VERIFIED`. **Gate G0 = AWAITING_OWNER_SIGNOFF / NOT_PASSED**. Backlog 27 tickets đã rà soát từ xa, chưa owner sign-off/ACCEPTED; G1 chưa được cho phép, không code.

## 0. Bổ sung bằng chứng do chủ dự án gửi qua chat — 2026-10-23

**Nguồn bằng chứng:** output PowerShell được chủ dự án tự cung cấp từ `PS C:\Users\thuon\Unity\Projectzombie>`; **không phải GitHub connector truy cập được máy local**.

- `git status --short`: **17 tracked modified (`M`)** và **8 untracked path entries (`??`)**. Quan trọng: `Assets/Features/Defense3D/DefenseRun6003D.cs` (local Fast Test khác remote có thể tồn tại), **13 file `.meta` trong nhóm Defense3D** cùng `Assets/Features/Defense3D.meta`, `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings/EditorBuildSettings.asset` đều hiện modified.
- Các untracked path đặc biệt gồm `Assets/Scenes/Defense3D/` + `.meta`, `Assets/Art/Environment/` + `.meta`, `Assets/Features/DebugUI/`, `Assets/Features/Defense3D/README.md.meta`, `Packages/manifest.json.bak`, `ProjectSettings/Packages/`. `??` chỉ nói Git chưa quản lý, **không chứng minh nội dung hoặc file .unity/.glb cụ thể đã được xác minh**.
- `Test-Path 'Assets\Scenes\Defense3D'` trả **True** — chứng minh **thư mục tồn tại trên máy người dùng khi chạy lệnh**. Chưa có output `Test-Path 'Assets\Art\Environment'`, danh sách scene `.unity`, asset GLB, SHA256, local HEAD/branch, hay đường dẫn backup.
- **Trạng thái cập nhật:** G0-S1 = `PARTIAL_USER_REPORTED` (git status đã nhận, branch/HEAD/stash còn thiếu), G0-S2..S4 = `NOT_VERIFIED_LOCAL`, G0-S5/S6 = `NOT_RUN`. Việc gửi output không đánh dấu backup/Unity QA đã đạt.
- **Stop rule nghiêm ngặt:** chưa chạy `git pull`, checkout, reset/clean, stash pop, scene generator, hay sửa meta; ưu tiên **đóng Unity và copy-only snapshot ngoài repo, bảo đảm NavMesh được bao gồm** rồi xác minh hash.



### Bổ sung xác nhận của chủ dự án — G0-S5 & G0-S6 (2026-10-23)

Sau checklist trong chat yêu cầu **(1) kiểm tra Missing (Mono Script), (2) kiểm tra GLB, 3 cầu, 10 TowerSocket và NavMesh, (3) mở thử Legacy Survival 2D SampleScene**, chủ dự án trả lời **“okee đã xác minh”**. Theo ngữ cảnh, ghi nhận owner xác nhận đã kiểm tra **cả ba mục** và không báo lỗi/ngoại lệ.

- **G0-S5: `OWNER_REPORTED_VERIFIED`** cho Inspector/Missing Scripts và cấu trúc bản đồ/NavMesh; cộng với hai ảnh trước cho thấy `MapSongQue3D_Prototype`, Console tại lúc chụp 0 Warning/0 Error và các root objects. Chưa có ảnh mới hay log test đính kèm để độc lập kiểm chứng mọi component/3 lane/10 socket; không coi là full 600s PlayMode PASS.
- **G0-S6: `OWNER_REPORTED_VERIFIED`** cho mở/thử Legacy `SampleScene` không thấy vấn đề được báo cáo. Chưa có ảnh Console/PlayMode kết quả Legacy riêng để kiểm chứng độc lập.
- **Giới hạn:** owner self-report ≠ machine-captured QA evidence; chưa biết local branch/HEAD/stash, chưa có hash đích danh `NavMesh-Defense_Navigation.asset/.meta`, chưa test restore snapshot, và chưa đóng G0. **Không đụng scene, NavMesh, GUID, packages hoặc code.**

### Unity Editor G0-S5 — bằng chứng ảnh chụp Console & Hierarchy từ chủ dự án (2026-10-23)

**Nguồn:** 2 ảnh chụp màn hình Unity do chủ dự án gửi qua chat; ảnh không được lưu thêm vào GitHub, chỉ ghi nhận quan sát. **Status: `PARTIAL_SCREENSHOT_VERIFIED`**, chưa đủ `PASS`.

| Hạng mục | Bằng chứng quan sát được | Trạng thái |
|---|---|---|
| Console | Unity Console hiển thị **20 Log / 0 Warning / 0 Error** tại thời điểm chụp. Có thông báo `[TREBRAVE Defense3D] Hero hit magiap_Event_1_2...` | **SCREENSHOT_VERIFIED**, chỉ ở thời điểm ảnh, không phải full-session QA |
| Scene đang mở | Root Hierarchy tên `MapSongQue3D_Prototype`; có `[TREBRAVE] Map Song Que Defense 3D` | **SCREENSHOT_VERIFIED** |
| Hero/camera/map/NavMesh | Root entries `Hero_3D_Prototype`, `Main Camera`, `TREBRAVE_MapSongQue_ThreeJS`, `Defense_Navigation` hiện diện trong Hierarchy | **ROOT_ENTRIES_PRESENT**, chưa kiểm tra Inspector, component/script/asset reference và NavMesh bake thực tế |
| Chế độ test/spawner | `[TREBRAVE] Enemy Bridge Smoke Test` và `[TREBRAVE] Three Wave Defense Test` hiện chữ xám (phù hợp inactive); `[TREBRAVE] Tower Build Test` và `[TREBRAVE] 600s Defense Run` hiện chữ sáng | **VISUAL_OBSERVATION ONLY**; chưa xác minh các source spawner active/layer hay duplication |
| Combat runtime log | `Hero hit magiap_Event_1_2` có trong Console | Một sự kiện log combat, **không phải** bằng chứng 600s run, AI path/win/lose đã nghiệm thu |
| Missing scripts | Ảnh Hierarchy chỉ cho thấy root GameObjects, chưa mở Inspector/hệ thống kiểm tra Missing (Mono Script) | **NOT_VERIFIED** |
| Legacy | Không có ảnh `SampleScene` hay kết quả survival smoke | **G0-S6 NOT_RUN** |

**G0-S5 = `PARTIAL_SCREENSHOT_VERIFIED / NOT_PASSED`**: chứng minh Editor đã mở scene và tại lúc chụp Console 0 warning/error; chưa chứng minh Zero Missing Mono Scripts, NavMesh/3 cầu/10 TowerSocket, runtime ổn định hoặc Console sạch sau test đầy đủ. **Không đổi G0-S6**, không viết code/rebake/regenerate.

### Xác minh danh tính asset trong bản backup — báo cáo PowerShell bổ sung

Owner chạy đúng backup `C:\Users\thuon\Unity\TREBRAVE_G0_Backup_20261023_211614` và cung cấp kết quả liệt kê:

| Nhóm | Đường dẫn / tên asset trong backup | Kích thước / trạng thái |
|---|---|---|
| Scene Defense3D | `Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity` | **135896 bytes**, có bản sao |
| Map GLB | `Assets/Art/Environment/**/TREBRAVE_MapSongQue_ThreeJS.glb` | **6507936 bytes**, có bản sao (chưa có path con đầy đủ) |
| NavMesh data | `Assets/Scenes/Defense3D/MapSongQue3D_Prototype/NavMesh-Defense_Navigation.asset` | `InBackup=True` trong danh sách quét local |
| NavMesh meta | `Assets/Scenes/Defense3D/MapSongQue3D_Prototype/NavMesh-Defense_Navigation.asset.meta` | cột `InBackup` ở output bị cắt chữ thành `Tru`, cần xác nhận đầy đủ nếu muốn kết luận chính thức |

**Diễn giải bằng chứng:** Ở bước trước owner đã chạy so sánh SHA-256 với **8 files** có đuôi `.unity/.glb/.meta/.asset` trong **hai cây Scene/Environment** (0 mismatch/missing). NavMesh hiện được tìm thấy ở **cây scene đã được đưa vào tập scan**, vì vậy có cơ sở để tin nó thuộc tập hash, nhưng output trước không in ra tên từng file; **chưa có báo cáo hash chỉ đích danh NavMesh/.meta** và chưa kiểm tra khả năng Unity dùng được bake. `G0-S2/S3` giữ `BACKUP_PRESENT / HASH_SUBSET_MATCH / LOCAL_RUNTIME_NOT_RUN`, **không tự đổi PASS**.

**Next:** ưu tiên Unity Console + mở Scene/Legacy bằng File > Open Scene sau khi bảo toàn backup; nếu cần bằng chứng trực tiếp, so sánh SHA-256 đúng các đường dẫn NavMesh và `.meta` theo tên. Không chạy scene generator/rebake khi chưa review.

### Xác minh SHA-256 bổ sung — Packages & Build Settings (output do owner cung cấp)

Báo cáo PowerShell nguồn vs backup của chủ dự án có ba dòng **`BackedUp=True` và `HashMatches=True`**:

| Relative path | Backup present | Source/backup SHA-256 equal |
|---|---|---|
| `Packages/manifest.json` | **True** | **True** |
| `Packages/packages-lock.json` | **True** | **True** |
| `ProjectSettings/EditorBuildSettings.asset` | **True** | **True** |

**Giới hạn bằng chứng:** Đây là xác minh thành công **3 file cụ thể**, không phải hash-check toàn bộ Packages/ProjectSettings, restore test, Unity importer, scene/NavMesh. **G0-S4 = 3 KEY FILES HASH VERIFIED / PARTIAL, NOT_PASSED** cho tới khi kiểm kê phần còn lại. **G0-S2/S3 vẫn PARTIAL**, NavMesh chưa rõ; **G0-S5/S6 NOT_RUN; Gate G0 NOT_PASSED**.

### Backup copy-only do chủ dự án thực hiện — 2026-10-23 21:16 (Asia/Ho_Chi_Minh)

- Chủ dự án chạy PowerShell tại `C:\Users\thuon\Unity\Projectzombie`; `Test-Path 'Assets\Scenes\Defense3D'` và `Test-Path 'Assets\Art\Environment'` đều trả `True`.
- Script tạo snapshot bên ngoài repo: `C:\Users\thuon\Unity\TREBRAVE_G0_Backup_20261023_211614`. Vòng `Copy-Item -Recurse -Force -ErrorAction Stop` đã hoàn tất, không có PowerShell exception được hiển thị. Danh sách sao chép theo script gồm Defense3D scenes và .meta, Environment + .meta, Defense3D scripts, DebugUI, Editor Defense3D, `Packages` và `ProjectSettings` (mục không tồn tại tự được bỏ qua).
- PowerShell so sánh SHA-256 nguồn/bản sao cho file có extension `.unity`, `.glb`, `.meta`, `.asset` **trong đúng hai cây thư mục** `Assets\Scenes\Defense3D` và `Assets\Art\Environment`. Output: **`Critical files checked: 8`**; **`Hash mismatches / missing: 0`**; biến `$errorsFound` rỗng. Đây là **bằng chứng do user báo cáo về 8 bản sao trùng hash**, không phải audit đã liệt kê tên từng file.
- Git branch, HEAD, stash list được ghi vào backup thông qua các lệnh `Set-Content`, nhưng nội dung chưa được gửi để xác thực. **Không thể kết luận** đã có mọi `NavMeshData`, tất cả scene/GLB liên quan hoặc toàn bộ `Packages/ProjectSettings` được xác minh hash; script lọc 8 file và không so sánh hash cho những nhóm khác.
- **G0-S1 = PARTIAL_USER_REPORTED; G0-S2 = COPY_REPORTED / HASH_SUBSET_MATCH (NavMesh chưa kiểm kê); G0-S3 = COPY_REPORTED / HASH_SUBSET_MATCH (bao gồm Environment nhưng chưa biết danh sách từng file); G0-S4 = COPY_REPORTED / NOT_HASH_VERIFIED; G0-S5/S6 = NOT_RUN. Gate G0 = NOT_PASSED.**
- Bước tiếp theo: **chỉ đọc** danh sách 8 file đã so sánh và truy tìm `NavMeshData`/`NavMeshSurface` trước khi gọi backup complete; kiểm tra danh sách trong backup và hash NavMesh nếu asset nằm ngoài danh sách; sau đó mở Unity scene/Console và Legacy. **Không pull/checkout/clean/reset/generate scene/đổi GUID.**

## 1. Bảng bằng chứng GitHub (chỉ xác minh đối với BRANCH REMOTE)

| ID | Hạng mục | Bằng chứng hiện tại | Kết luận **chỉ về remote** | Hành động để đủ Gate G0 |
|---|---|---|---|---|
| R-G0-01 | Unity Editor version | `ProjectSettings/ProjectVersion.txt`: **2022.3.62f3** | `REMOTE_VERIFIED` | Kiểm tra Unity thực chạy local đúng phiên bản |
| R-G0-02 | Defense3D scripts | `Assets/Features/Defense3D` có **14 file C# gameplay**, và **14 file .cs.meta**; `Assets/Editor/Defense3D` có **8 Editor scripts**, và **8 .meta** | `REMOTE_VERIFIED` về **file presence**, không chứng minh Unity compile | Unity Console sạch, không duplicate class/CS0101/CS0111, no Missing Mono Script |
| R-G0-03 | Defense3D scene | Directory `Assets/Scenes` remote chỉ có `SampleScene.unity` và `SteamSlipTest.unity` (+ meta); **không có `Assets/Scenes/Defense3D`** trên nhánh | `REMOTE_MISSING — CRITICAL`; **không kết luận file local đã mất** | Sao lưu local **Prototype/Recovery scene** cùng `.meta`, xem `git status --short`, xác minh Unity reopen, rồi lên kế hoạch lưu git riêng sau backup/QA |
| R-G0-04 | Sông Quê GLB / Environment | `Assets/Art` remote có nhiều folder nhân vật 2D nhưng **không có `Assets/Art/Environment`** | `REMOTE_MISSING — CRITICAL`; không kết luận GLB local đã mất | Backup `Assets/Art/Environment` gồm GLB/.meta và importer state, kiểm tra material/pivot/reference trên Unity |
| R-G0-05 | AI Navigation | `Packages/manifest.json` remote ghi `com.unity.ai.navigation: 1.1.5` | `REMOTE_VERIFIED` về **khai báo package** | Unity Package Manager local, baked NavMeshData và NavMeshSurface local phải xác minh riêng |
| R-G0-06 | GLB importer glTFast | `Packages/manifest.json` remote **không có `com.unity.cloud.gltfast`** hoặc `com.atteneder.gltfast` trực tiếp | `REMOTE_NOT_DECLARED`; không kết luận local chưa cài | Ghi lại importer đã cài local, version và package manifest/lock cần bảo toàn; không cài/đổi package trong audit |
| R-G0-07 | Legacy Build Settings | `ProjectSettings/EditorBuildSettings.asset` remote chỉ bật `Assets/Scenes/SampleScene.unity`; GUID **`2cda990e2423bbf4892e6590ba056729`** trùng với `SampleScene.unity.meta` remote | `REMOTE_VERIFIED` về **Legacy entry/reference**, không phải QA mở scene | Sao lưu EditorBuildSettings local, kiểm tra không vô tình thay default/Legacy, test mở lại SampleScene |
| R-G0-08 | Scene generator risk | `DefenseMapSongQueSceneGenerator.cs` có menu `Build & Open Map Song Que Prototype`, nếu file đích tồn tại sẽ hỏi xác nhận overwrite, sau đó `NewScene` và `SaveScene` vào path cố định | `RISK_CONFIRMED`: thao tác này **có thể ghi đè scene local đã chỉnh** nếu xác nhận | **Không bấm generator**, không tái tạo scene; dùng **File > Open Scene** tới scene có sẵn |
| R-G0-09 | Duplicate spawner risk | `DefenseEnemyBridgeSmokeTest3D.cs` và `DefenseWaveSlice3D.cs` đều có trên remote, cùng `DefenseRun6003D.cs` | `RISK_IDENTIFIED` về khả năng có các spawner thử nghiệm; trạng thái bật trong scene local chưa biết | Chụp Hierarchy và kiểm tra chỉ 1 production spawner active; không xóa script/.meta; ghi trạng thái khi QA |
| R-G0-10 | Git attributes/ignore | `.gitignore` Unity không ignore chung `*.unity`/`*.glb`/`*.meta`; `.gitattributes` có text YAML/meta, nhưng **chưa khai báo rule `*.glb filter=lfs`** | `REMOTE_VERIFIED`; GLB khoảng nhiều MB có thể cần chiến lược Git LFS, **chưa tự thay .gitattributes** | Cân nhắc versioning GLB riêng **sau khi local backup**; không `git add -A` mù khi có dữ liệu chưa review |

**Điều chắc chắn từ kiểm kê:** trên **branch GitHub được kiểm tra**, mã prototype và Editor scripts đã có; **scene Defense3D và folder Environment chưa được versioned trong những vị trí dự kiến**. Không thể dùng GitHub alone để xác nhận NavMesh bake, backup, scene mở được hoặc Unity runtime hoạt động.

## 2. Gate G0-S1..S6 — trạng thái thực tế

| Gate test | Evidence required | Status hôm nay | Vì sao |
|---|---|---|---|
| **G0-S1** | Local `git branch --show-current`, `git rev-parse HEAD`, `git status --short`, stash listing | **OWNER_OUTPUT_VERIFIED** | Branch `feat/trebrave-defense3d-first-scene`, HEAD `43ee2a8...`, 2 stashes khớp nội dung snapshot; 17 M/8 ?? local vẫn phải bảo toàn |
| **G0-S2** | Copy-only backup local scene `.unity`+`.meta`, NavMeshData liên quan, location/size/hash | **CRITICAL_ASSET_HASH_VERIFIED (4/4 Scene/NavMesh)** | Owner output: Scene+meta, NavMesh asset+meta đều SourceExists/BackupExists/HashMatches=True; còn full restore/other assets chưa test |
| **G0-S3** | Copy-only backup local GLB, material, importer meta, script .meta, NavMesh folder ngoài path | **CRITICAL_ASSET_HASH_VERIFIED (2/2 GLB)** | Owner output: GLB+meta đều SourceExists/BackupExists/HashMatches=True; importer/material khác và full restore chưa test |
| **G0-S4** | Backup package manifest/lock, EditorBuildSettings, restore evidence | **3 KEY FILES HASH VERIFIED / PARTIAL — NOT_PASSED** | Owner báo cáo `BackedUp=True` và `HashMatches=True` cho `manifest.json`, `packages-lock.json`, `EditorBuildSettings.asset`; chưa kiểm tra toàn bộ folder và khả năng restore |
| **G0-S5** | Unity 2022.3.62f3 mở đúng scene Map Sông Quê, Console 0 compile errors, 0 Missing Mono Script | **OWNER_REPORTED_VERIFIED + PARTIAL SCREENSHOTS — G0 NOT_PASSED** | Hai ảnh: Console 20 Log / 0 Warning / 0 Error, map root/Hero/GLB/NavMesh/Run; owner sau đó xác nhận đã kiểm tra Missing Scripts + 3 cầu/10 sockets/NavMesh. **Không có evidence Inspector/runtime mới để độc lập nghiệm thu** |
| **G0-S6** | Reopen Legacy `SampleScene` sạch, không regress; 3 bridge/10 sockets/NavMesh check | **OWNER_REPORTED_VERIFIED — G0 NOT_PASSED** | Owner trả lời đã xác minh toàn bộ checklist gồm Legacy 2D; chưa cung cấp ảnh/log/QA Legacy riêng, chỉ ghi nhận lời xác nhận, không coi là full regression suite |

**STOP RULE:** Nếu G0-S2 hoặc G0-S3 chưa được người dùng xác minh backup, **không được chạy scene generator / clean/reset / stash pop / đổi meta**, và không bắt đầu Core G1. Nếu Unity Console có Missing Script, giữ object để chữa có kiểm soát; không gỡ component hàng loạt.

## 3. Các bước có thể tiến hành an toàn trên máy Unity

**Bước 1 — ngoài Unity:** dùng **PowerShell read-only** kiểm tra branch, HEAD, dirty/untracked, stash; **không pull/checkout/clean**:
```powershell
Set-Location -LiteralPath 'C:\Users\thuon\Unity\Projectzombie'
git branch --show-current
git rev-parse HEAD
git status --short
git stash list
Get-ChildItem -LiteralPath 'Assets\Scenes\Defense3D' -Force -ErrorAction SilentlyContinue | Select Name,Length
Get-ChildItem -LiteralPath 'Assets\Art\Environment' -Recurse -File -ErrorAction SilentlyContinue | Where-Object Extension -in '.glb','.gltf','.meta' | Select FullName,Length
```

**Bước 2 — đóng Unity và copy-only backup**: làm theo [07 — G0 Local Safety Runbook](07_G0_LOCAL_SAFETY_RUNBOOK.md) mục 3. Chỉ copy ra ngoài repo, không thay source. **Nếu NavMeshData đặt ngoài thư mục đã liệt kê, phải xác định và copy thêm.**

**Bước 3 — đối chiếu checksum/size và 3D scene local:** mở lại đúng `MapSongQue3D_Prototype` hoặc `MapSongQue3D_Recovery` đã tồn tại bằng File > Open Scene, không dùng generator. Ghi ảnh Scene+Hierarchy, NavMesh 3 cầu, 10 sockets, GLB renderer, `DefenseRun6003D`, Unity Console và Legacy SampleScene.

**Bước 4 — điền [Evidence Card](07_G0_LOCAL_SAFETY_RUNBOOK.md#6-gate-g0-evidence-card-điền-sau-chưa-pass):** local branch/HEAD/git status; backup path + verification; Unity version; scene path; lỗi Console/Missing Script; Legacy outcome. Chỉ khi có bằng chứng thì từng case G0-S1..S6 mới có thể đổi sang PASS.

**Bước 5 — khi evidence đủ:** owner review **27 backlog tickets** và quyết định **đóng Gate G0**; *không tự viết code* chỉ vì đã đóng Gate. Core G1 ưu tiên State/Clock adapter, code-only nhỏ, preserve GUID/scene references.

## 4. Link bằng chứng GitHub (remote)

- [ProjectVersion.txt](https://github.com/thuongtran1210/Projectzombie/blob/feat/trebrave-defense3d-first-scene/ProjectSettings/ProjectVersion.txt)
- [Defense3D prototype scripts](https://github.com/thuongtran1210/Projectzombie/tree/feat/trebrave-defense3d-first-scene/Assets/Features/Defense3D)
- [Editor installers + generator](https://github.com/thuongtran1210/Projectzombie/tree/feat/trebrave-defense3d-first-scene/Assets/Editor/Defense3D)
- [EditorBuildSettings.asset](https://github.com/thuongtran1210/Projectzombie/blob/feat/trebrave-defense3d-first-scene/ProjectSettings/EditorBuildSettings.asset)
- [SampleScene.unity.meta](https://github.com/thuongtran1210/Projectzombie/blob/feat/trebrave-defense3d-first-scene/Assets/Scenes/SampleScene.unity.meta)
- [Packages/manifest.json](https://github.com/thuongtran1210/Projectzombie/blob/feat/trebrave-defense3d-first-scene/Packages/manifest.json)
- [.gitignore](https://github.com/thuongtran1210/Projectzombie/blob/feat/trebrave-defense3d-first-scene/.gitignore)
- [.gitattributes](https://github.com/thuongtran1210/Projectzombie/blob/feat/trebrave-defense3d-first-scene/.gitattributes)
- [07 — Local Safety Runbook](07_G0_LOCAL_SAFETY_RUNBOOK.md)
- [04 — Gate0 Approval Pack](04_GATE0_APPROVAL_PACK.md)

**Chỉ có các đường dẫn dữ liệu từ GitHub remote được kiểm tra.** Không có quyền quan sát filesystem Windows/Unity Editor qua GitHub connector; **không coi mô tả local scene từ các lần làm việc trước là bằng chứng QA mới**.
