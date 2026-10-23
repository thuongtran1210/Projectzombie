# Gate G0 — Final Readiness Checklist & 27-Ticket Backlog Review

> **G1 CONTINUATION — 2026-10-23:** Added pure-C# `Assets/Features/Defense3D/DefenseRunLifecycle.cs` and `Assets/Tests/Editor/DefenseG1LifecycleTests.cs` (commits `808a5dc0959830d612ba6c17577819a4cc63ca09`, `d8bd1c4d1241b4ade1fa8c4ac0ae3f099c225345`). Lifecycle joins `DefenseGameState` + `DefenseGameplayClock`, resolves BaseHP loss ahead of WIN, and exposes an explicit core reset / run generation; tests cover three core resets, suspended clock, LOSS during pause, and overtime resume reset. **U3D-003/004/005 = CODE PARTIAL / QA NOT_RUN**, not accepted. This does **not** reset Unity scene entities, Base/Hero HP, inventory, towers, pending respawn/spawn jobs, or attached listeners; integration and PlayMode validation remain required. Existing `DefenseRun6003D` and local Unity files were not modified. G0 recorded limitations remain active.

> **G1 PROGRESS UPDATE — 2026-10-23:** `U3D-003` state machine tiếp tục ở trạng thái **CODE PARTIAL / QA NOT_RUN**. Đã thêm `Assets/Features/Defense3D/DefenseGameplayClock.cs` cho `U3D-004` (**IN_PROGRESS / CODE PARTIAL**) và `Assets/Tests/Editor/DefenseG1StateClockTests.cs` (NUnit editor tests, **NOT EXECUTED**). Clock chỉ tăng ở Playing/Overtime, dừng ở Paused/LevelUp/terminal, phát DurationReached một lần, có reset; chưa nối runtime AI/spawn/damage/respawn, chưa xây Fast Test Inspector profile và chưa được nghiệm thu. `U3D-005 = NOT_STARTED`. Commit tests: `77a5bf1fe2287d8921c4521ee8492c40ca466b42`. Không chỉnh `DefenseRun6003D`, asset scene, NavMesh, GLB, GUID/.meta hoặc local snapshot.

> **G1 IMPLEMENTATION STARTED — 2026-10-23:** Bắt đầu `U3D-003` với file C# thuần `Assets/Features/Defense3D/DefenseGameState.cs` (commit `4ed04ded0e1d6de900aeed3b6271e59e951728bc`). Phạm vi: phase enum, allowed transitions, pause/level-up resume, overtime, terminal LOSS priority, reset và change event. **Status U3D-003 = IN_PROGRESS / CODE PARTIAL / QA NOT_RUN**; chưa tích hợp `DefenseRun6003D`, chưa kiểm thử Unity, chưa làm `U3D-004` hoặc `U3D-005`. Dòng trạng thái "implementation not started" trước đây là lịch sử trước commit này. Không chỉnh local dirty working tree, scene, NavMesh, GLB, GUID, `.meta` hoặc stash.

> **G1 AUTHORIZATION UPDATE — 2026-10-23 (Asia/Ho_Chi_Minh):** Chủ dự án đã phê duyệt riêng **quyền bắt đầu G1** sau khi G0 được đóng. **G1 = OWNER AUTHORIZED TO START / IMPLEMENTATION NOT STARTED BY THIS APPROVAL RECORD.** Phê duyệt này không phải xác nhận đã triển khai, QA PASS, hay cho phép tác động trực tiếp lên local dirty/untracked tree. Giữ nguyên 17 tracked modified, 8 untracked entries và 2 stashes; không pull/merge/reset/clean/stash pop, không sửa scene/NavMesh/GLB/.meta/GUID hoặc Legacy trong cập nhật tài liệu này. Trước khi thực thi G1 phải có phương án bảo toàn dữ liệu local và kiểm tra phạm vi thay đổi; các hạn chế G0 (chưa full backup restore, chưa full-run gameplay QA) vẫn có hiệu lực. Các dòng `G1 NOT AUTHORIZED` phía dưới là **ghi chép lịch sử tại thời điểm G0 sign-off**, đã được quyết định riêng này thay thế về **quyền bắt đầu code**; không có ticket nào tự chuyển IN_PROGRESS/DONE/ACCEPTED.


**Review date:** 2026-10-23 (Asia/Ho_Chi_Minh)  
**Scope:** `thuongtran1210/Projectzombie`, branch `feat/trebrave-defense3d-first-scene`.  
**Status:** `G0_OWNER_APPROVED / PASSED_WITH_RECORDED_LIMITATIONS / G1_OWNER_AUTHORIZED_TO_START / IMPLEMENTATION_NOT_STARTED` · Owner sign-off 2026-10-23.  
**Authority:** Chủ dự án **đã phê duyệt đóng G0 theo evidence hiện có**; **đã chấp thuận riêng quyền bắt đầu code G1 (sau G0 sign-off); chưa triển khai code trong lượt ghi nhận này**, chưa nghiệm thu từng ticket, chưa commit local scenes, chưa hoàn tất full-restore hoặc PlayMode 600s.

## 0. G0 SIGN-OFF — CHÍNH THỨC (2026-10-23)

**OWNER APPROVED / PASSED WITH RECORDED LIMITATIONS.** Chủ dự án đã xem [04 — Gate G0 Approval Pack](04_GATE0_APPROVAL_PACK.md), xác nhận 6/6 Scene/NavMesh/GLB + `.meta` khớp SHA-256, Git branch/HEAD/2 stash trùng backup snapshot, 3 file Packages/EditorBuildSettings khớp, Unity/Legacy được owner xác nhận và 27/27 ticket đã review. **G0 được đóng theo mức bằng chứng đó; không tự làm thêm bước phục hồi/QA.**

**Giới hạn được chủ dự án chấp nhận:** chưa thử full backup restore, chưa xác minh mọi material/importer/asset ngoài snapshot, không có independent Unity Legacy QA, không có full 600s run, 28 QA chính + 18 D06-QA + 12 D07-QA + ML tests chưa qua, production balance chưa khóa. **27 ticket không chuyển `ACCEPTED`**, đặc biệt U3D-003..005 G1 **chưa `IN_PROGRESS`**.

**QUYỀN TRIỂN KHAI:** **NO G1 CODE AUTHORIZATION**. Không pull, merge, reset, clean, stash pop, sửa scene, NavMesh, GLB, GUID, `.meta`, không tạo gameplay commit trong lần cập nhật tài liệu. Chỉ khi owner cho phép code trong bước khác và duyệt phương án bảo vệ dirty/untracked local mới lập trình.

## 1. Kết luận review source backlog

- **27/27 ID hiện diện, không trùng**: `U3D-001`.. `U3D-027`; **P0=21, P1=3, P2=3**.
- Đã soát các dependency ID nêu tường minh theo văn bản: **không tham chiếu ticket không tồn tại; không thấy chu trình phụ thuộc**. `U3D-024` viết `U3D-010..U3D-023` là shorthand (14 ID), **phải mở rộng khi tạo issue/đưa vào công cụ theo dõi**; không ngầm coi là danh sách explicit dependencies đã được máy parse đầy đủ.
- **24 ticket U3D-001..024** có bảng 6 cột (ID/priority/scope/dependencies/relative size/acceptance), **3 ticket U3D-025..027** là bảng post-Core 5 cột có schema khác theo G8 — không phải lỗi thiếu size.
- **Backlog requirements are reviewed, not accepted:** owner đã cung cấp **branch/HEAD/stash đối chiếu snapshot và 6/6 SHA-256 critical assets khớp**, nên 2 evidence blockers này đã giải quyết. **Owner Gate G0 sign-off đã nhận**; full recovery QA và G1 code/Unity PlayMode vẫn chưa được tiến hành để đánh DONE. **27 tickets ở DRAFT/BLOCKED, chưa mở Issues.**

## 2. Review từng nhóm ticket / sequence sau G0

| Nhóm | Tickets | Kết luận review thiết kế | Gate/QA hiện tại |
|---|---|---|---|
| **G0 — Safety/decision** | `U3D-001` / `U3D-002` | D-01..D-11 APPROVED; owner đã copy-only backup; 8 file Scene/Environment 0 hash mismatch; 3 file Packages/BuildSettings hash khớp; scene/GLB/NavMesh xuất hiện trong backup; Unity Console screenshot 0 Error; owner đã kiểm tra Legacy/Missing Scripts/map | **PARTIAL / NOT_ACCEPTED**; xem phần 3 |
| **G1 — State/Clock/Restart** | `U3D-003..005` | Đúng D-02/A 600 gameplay s, D-03/A WIN/OVERTIME, D-04/A Hero down 5 gameplay s. Ưu tiên adapter bảo toàn MonoBehaviour và `.meta` | **SPEC REVIEWED, CODE NOT_STARTED** |
| **G2 — Enemy/Data/Combat** | `U3D-006..009` | IDs chuẩn, declared/actual type D-11/A, Damage/Death/Reward exactly-once, prefab adapter | **SPEC REVIEWED, CODE NOT_STARTED** |
| **G3 — Navigation/Stage/GhostCannon** | `U3D-010..014` | D-06/A đã chốt Ground 3 cầu/Water shore/Air 3D/Landing; D-05 #7 @108; D-11/A 1 Phi Cương/event; 34 events/214 slots, cap/queue. GhostCannon 3 khẩu bắt buộc trước full Core QA dù task P1 | **SPEC REVIEWED, QA NOT_RUN** |
| **G4 — Tower/Wallet/Hero/Relic** | `U3D-015..018` | 10 socket/MaxLv3, repair tốn Gỗ, Wood/EXP khác nhau; Hero Thư Sinh trước, Dép Tổ Ong Relic đầu tiên | **SPEC REVIEWED; con số production chưa khóa** |
| **G5 — EXP/3-card** | `U3D-019..020` | EXP gem pickup/queued levels, 3 card hợp lệ và game freeze theo D-02/A | **SPEC REVIEWED, QA NOT_RUN** |
| **G6/G7 — Yaksha/Result/HUD/Full-run QA** | `U3D-021..024` | Boss FSM + overtime, Return/Restart/Legacy, 600s full-run và FastTest tách; không thể nghiệm thu QA từ commit GitHub | **SPEC REVIEWED, QA NOT_RUN** |
| **G8 — Polish & Android** | `U3D-025..027` | Model/VFX/mobile profiling sau playable Core, không kéo lùi G0/G1 | **POST_CORE, NOT_STARTED** |

**Điểm kiểm soát khi tách issue:** `U3D-001` và `U3D-002` có thể được reviewer duyệt nội dung, nhưng **không đánh `ACCEPTED`** khi chưa đầy đủ bằng chứng G0; `U3D-003`/004/005 chưa chuyển `IN_PROGRESS` nếu chưa được cho phép code. `U3D-014` không bị bỏ chỉ vì P1: ba Linh Pháo thuộc D-06/A trước Core full QA. `U3D-024` phải mở rộng `U3D-010..023` thành explicit list khi tạo issue, cập nhật log/QA reference.

## 3. Gate G0-S1..S6: Evidence hiện có và việc còn thiếu

| ID | Evidence hiện có | Status | Việc cần làm trước G0 review |
|---|---|---|---|
| **G0-S1** | Git branch, HEAD, 2 stash names từ local **giống hệt** `git-branch.txt`, `git-head.txt`, `git-stash.txt` của backup; HEAD `43ee2a806c593afe8704730f9daf012b52f56b35`, 17 M và 8 ?? local | **OWNER_OUTPUT_VERIFIED** | Bảo toàn dirty/untracked & 2 stashes; không pull/checkout/clean/reset; HEAD local chưa so sánh remote tip |
| **G0-S2** | `MapSongQue3D_Prototype.unity`, scene `.meta`, `NavMesh-Defense_Navigation.asset` và NavMesh `.meta` ở source+backup, **4/4 HashMatches=True** | **CRITICAL_4_HASH_VERIFIED** | Full restore chưa thử; không tái bake/regenerate, không kết luận mọi scene khác cũng đã backup |
| **G0-S3** | `TREBRAVE_MapSongQue_ThreeJS.glb` và `.meta` có trong source+backup, **2/2 HashMatches=True** | **CRITICAL_2_HASH_VERIFIED** | Material/importer file ngoài thư mục chưa chứng minh toàn bộ; root GLB trong Hierarchy đã chụp |
| **G0-S4** | Owner cung cấp `BackedUp=True` + `HashMatches=True` cho `Packages/manifest.json`, `packages-lock.json`, `ProjectSettings/EditorBuildSettings.asset` | **3 key files VERIFIED / PARTIAL** | Ghi rõ snapshot packages/project settings khác nếu có; không tuyên bố restore test đã qua |
| **G0-S5** | 2 ảnh Console 20 Log/0 Warning/0 Error và root Hierarchy; chủ dự án xác nhận đã kiểm tra Missing Scripts, 3 bridge/10 sockets/NavMesh | **OWNER_REPORTED_VERIFIED** | Ảnh Inspector/PlayMode details nếu cần độc lập nghiệm thu; không mặc định 600s run đã pass |
| **G0-S6** | Chủ dự án xác nhận đã kiểm tra Legacy `SampleScene` | **OWNER_REPORTED_VERIFIED** | Nếu cần QA độc lập, lưu smoke steps/screenshot; không coi full regression suite đã chạy |

**HISTORICAL BEFORE SIGN-OFF (nay SUPERSEDED bởi mục 0):** G0-S1 đã có Git snapshot matching; 6 critical Scene/NavMesh/GLB files+meta đã khớp hash; G0-S4 ba file cốt lõi khớp (full restore chưa thử); G0-S5/S6 owner reported verified. **Tại lúc rà soát trước sign-off, gate chưa được đóng**; cập nhật hiện tại: **G0 đã được owner đóng có giới hạn, nhưng G1 vẫn chưa được cho phép**. Không dùng GUI Scene Generator, không `git reset --hard`, `git clean -fd`, `git stash pop`, không sửa `.meta`, không rebuild/rebake NavMesh.

### Delta xác minh mới nhất — 2026-10-23
- Snapshot của Git được owner đọc lại: branch = `feat/trebrave-defense3d-first-scene`; HEAD = `43ee2a806c593afe8704730f9daf012b52f56b35`; stashes 0/1 đều khớp tên/nhánh đã lưu; dirty state có 17 file tracked modified và 8 untracked entries.
- 6 critical files Scene `.unity`/meta, baked NavMesh `.asset`/meta, map GLB/meta: **tất cả SourceExists=True, BackupExists=True, HashMatches=True**.
- Trạng thái bằng chứng: **G0-S1 owner output verified; G0-S2/S3 6 hashes verified; G0-S4 3 config hashes verified/partial; G0-S5/S6 owner reported verified**. Sau khi owner sign-off ngày 2026-10-23: **G0 PASSED WITH RECORDED LIMITATIONS; G1 NOT AUTHORIZED**.
- HEAD local không nhất thiết trùng remote tip vì tài liệu GitHub đã nhận nhiều commit sau snapshot. **Không yêu cầu `git pull` hoặc biến dirty working tree thành clean trong G0.**

## 4. Chỉ đọc: đối chiếu trạng thái Git + backup và hash NavMesh (không sửa file)

Mở **PowerShell** tại `C:\Users\thuon\Unity\Projectzombie`; lệnh bên dưới **chỉ đọc**, không ghi file, không pull/checkout/restore/clean. Chỉ copy output text để ghi Gate G0 evidence:

```powershell
$root = 'C:\Users\thuon\Unity\Projectzombie'
$backup = 'C:\Users\thuon\Unity\TREBRAVE_G0_Backup_20261023_211614'
Set-Location -LiteralPath $root

Write-Host '=== GIT LOCAL ==='
Write-Host ('Branch: ' + (git branch --show-current))
Write-Host ('HEAD: ' + (git rev-parse HEAD))
Write-Host 'Stashes:'
git stash list
Write-Host 'Local changes:'
git status --short

Write-Host '=== BACKUP GIT BASELINE ==='
foreach ($f in @('git-branch.txt','git-head.txt','git-stash.txt')) {
    $p = Join-Path $backup $f
    if (Test-Path -LiteralPath $p) {
        Write-Host ($f + ':')
        Get-Content -LiteralPath $p
    } else {
        Write-Host ($f + ': MISSING IN BACKUP')
    }
}

Write-Host '=== CRITICAL FILE HASH CHECK ==='
$critical = @(
    'Assets\Scenes\Defense3D\MapSongQue3D_Prototype.unity',
    'Assets\Scenes\Defense3D\MapSongQue3D_Prototype.unity.meta',
    'Assets\Scenes\Defense3D\MapSongQue3D_Prototype\NavMesh-Defense_Navigation.asset',
    'Assets\Scenes\Defense3D\MapSongQue3D_Prototype\NavMesh-Defense_Navigation.asset.meta'
)
$critical += @(Get-ChildItem -LiteralPath (Join-Path $root 'Assets\Art\Environment') -Filter '*.glb' -File -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
    $_.FullName.Substring($root.Length + 1)
})
$critical += @($critical | Where-Object { $_ -like '*.glb' } | ForEach-Object { $_ + '.meta' })
$critical = @($critical | Select-Object -Unique)

foreach ($rel in $critical) {
    $src = Join-Path $root $rel
    $dst = Join-Path $backup $rel
    $srcOk = Test-Path -LiteralPath $src -PathType Leaf
    $dstOk = Test-Path -LiteralPath $dst -PathType Leaf
    $match = if ($srcOk -and $dstOk) {
        (Get-FileHash -LiteralPath $src -Algorithm SHA256).Hash -eq
        (Get-FileHash -LiteralPath $dst -Algorithm SHA256).Hash
    } else { $false }

    [PSCustomObject]@{
        File = $rel
        SourceExists = $srcOk
        BackupExists = $dstOk
        HashMatches = $match
    }
}
```

**Đọc kết quả:** `HashMatches=True` với NavMesh + meta và Scene + meta + GLB + meta xác nhận **đúng bản sao trên disk**, không chứng minh `NavMeshSurface` runtime bake đủ/chạy được. Nếu thiếu bất kỳ file nào, **KHÔNG xóa/tạo lại/đổi GUID**; kiểm tra source/backup và đưa đường dẫn thiếu vào danh sách cần sao lưu bổ sung theo [Runbook 07](07_G0_LOCAL_SAFETY_RUNBOOK.md). Đoạn `foreach` cuối xuất trực tiếp từng đối tượng `PSCustomObject` ra Console; không sử dụng pipeline sau khối `foreach`, tương thích PowerShell 5.1/7.

## 5. Sign-off sau khi có output

- [x] **Owner approved design:** D-01..D-11; D-07/A đủ 8 nguyên tắc.
- [x] **Remote backlog reviewed:** 27 IDs valid, no unknown explicit dependencies/cycles, 21 P0 / 3 P1 / 3 P2; không có ticket nào `ACCEPTED`.
- [x] **Owner-reported Unity baseline:** Console screenshot 0 Error, Missing Scripts/map/Legacy đã được owner xác nhận; không coi là độc lập QA full run.
- [x] **Local branch / HEAD / stash** được owner đối chiếu với snapshot backup; vẫn giữ 17 M/8 ?? và 2 stashes.
- [x] **SHA-256 cụ thể 6/6** Scene+meta, NavMeshData+meta, GLB+meta có đủ nguồn+backup và khớp.
- [ ] **Phạm vi backup mở rộng / restore test:** nếu còn asset, material hoặc prefab quan trọng ngoài snapshot, bổ sung copy-only; kiểm tra khả năng restore trước khi thao tác có nguy cơ. Đây là hardening bổ sung, **không coi 6 hashes bao phủ mọi file**.
- [x] **Chủ dự án đã phê duyệt đóng Gate G0 ngày 2026-10-23**: `OWNER APPROVED / PASSED WITH RECORDED LIMITATIONS`. Không có quyền code G1.
- [ ] **G1 code authorization riêng chưa được cấp**; chỉ sau quyết định mới mới được bắt đầu `U3D-003 → U3D-004 → U3D-005`. **Không code trong lượt sign-off G0.**

**Liên kết:** [02 — Implementation Backlog](02_IMPLEMENTATION_BACKLOG.md) · [04 — G0 Approval Pack](04_GATE0_APPROVAL_PACK.md) · [07 — Local Safety Runbook](07_G0_LOCAL_SAFETY_RUNBOOK.md) · [10 — G0 Evidence Audit](10_G0_REMOTE_BASELINE_AUDIT.md).
