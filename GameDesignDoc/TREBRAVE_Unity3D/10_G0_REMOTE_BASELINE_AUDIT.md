# Gate G0 — Remote Baseline Audit / Local Safety Blockers

**Thời điểm audit:** 2026-10-23 · **Nguồn:** GitHub `thuongtran1210/Projectzombie`, branch `feat/trebrave-defense3d-first-scene`.
**Trạng thái:** `REMOTE_INSPECTED / LOCAL_NOT_VERIFIED / G0_NOT_PASSED`.
**Phạm vi assistant đã làm:** chỉ đọc repo/manifest/Unity editor scripts trên GitHub và cập nhật tài liệu. **Người dùng đã tự chạy backup local copy-only và báo cáo 8 file khớp hash**, không phải assistant có quyền truy cập ổ đĩa. **Không mở Unity Editor, không Save Scene, không ghi GLB/NavMesh, không code gameplay.**

## 0. Bổ sung bằng chứng do chủ dự án gửi qua chat — 2026-10-23

**Nguồn bằng chứng:** output PowerShell được chủ dự án tự cung cấp từ `PS C:\Users\thuon\Unity\Projectzombie>`; **không phải GitHub connector truy cập được máy local**.

- `git status --short`: **17 tracked modified (`M`)** và **8 untracked path entries (`??`)**. Quan trọng: `Assets/Features/Defense3D/DefenseRun6003D.cs` (local Fast Test khác remote có thể tồn tại), **13 file `.meta` trong nhóm Defense3D** cùng `Assets/Features/Defense3D.meta`, `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings/EditorBuildSettings.asset` đều hiện modified.
- Các untracked path đặc biệt gồm `Assets/Scenes/Defense3D/` + `.meta`, `Assets/Art/Environment/` + `.meta`, `Assets/Features/DebugUI/`, `Assets/Features/Defense3D/README.md.meta`, `Packages/manifest.json.bak`, `ProjectSettings/Packages/`. `??` chỉ nói Git chưa quản lý, **không chứng minh nội dung hoặc file .unity/.glb cụ thể đã được xác minh**.
- `Test-Path 'Assets\Scenes\Defense3D'` trả **True** — chứng minh **thư mục tồn tại trên máy người dùng khi chạy lệnh**. Chưa có output `Test-Path 'Assets\Art\Environment'`, danh sách scene `.unity`, asset GLB, SHA256, local HEAD/branch, hay đường dẫn backup.
- **Trạng thái cập nhật:** G0-S1 = `PARTIAL_USER_REPORTED` (git status đã nhận, branch/HEAD/stash còn thiếu), G0-S2..S4 = `NOT_VERIFIED_LOCAL`, G0-S5/S6 = `NOT_RUN`. Việc gửi output không đánh dấu backup/Unity QA đã đạt.
- **Stop rule nghiêm ngặt:** chưa chạy `git pull`, checkout, reset/clean, stash pop, scene generator, hay sửa meta; ưu tiên **đóng Unity và copy-only snapshot ngoài repo, bảo đảm NavMesh được bao gồm** rồi xác minh hash.



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
| **G0-S1** | Local `git branch --show-current`, `git rev-parse HEAD`, `git status --short`, stash listing | **PARTIAL_USER_REPORTED — NOT_PASSED** | Đã nhận `git status --short` (17 M / 8 ??), chưa có branch/HEAD/stash hoặc backup manifest |
| **G0-S2** | Copy-only backup local scene `.unity`+`.meta`, NavMeshData liên quan, location/size/hash | **COPY_REPORTED / HASH_SUBSET_MATCH — NOT_PASSED** | Snapshot ngoài repo có; 8 file cảnh báo trọng yếu ở hai thư mục đã khớp SHA-256; **chưa có tên từng scene và NavMeshData** |
| **G0-S3** | Copy-only backup local GLB, material, importer meta, script .meta, NavMesh folder ngoài path | **COPY_REPORTED / HASH_SUBSET_MATCH — NOT_PASSED** | `Environment` local True; 8 files hash so sánh cho scene+environment, chưa xác minh từng GLB/meta và NavMesh ngoài cây |
| **G0-S4** | Backup package manifest/lock, EditorBuildSettings, restore evidence | **COPY_REPORTED / NOT_HASH_VERIFIED — NOT_PASSED** | User script `Copy-Item` cho toàn bộ `Packages` và `ProjectSettings`, không có lỗi hiển thị; chưa đối chiếu checksum / integrity / restore |
| **G0-S5** | Unity 2022.3.62f3 mở đúng scene Map Sông Quê, Console 0 compile errors, 0 Missing Mono Script | **NOT_RUN** | Không có Unity Editor local trong kết nối GitHub |
| **G0-S6** | Reopen Legacy `SampleScene` sạch, không regress; 3 bridge/10 sockets/NavMesh check | **NOT_RUN** | Remote YAML/Build Settings không chứng minh runtime |

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
