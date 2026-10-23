# Gate G0 — Final Readiness Checklist & 27-Ticket Backlog Review

**Review date:** 2026-10-23 (Asia/Ho_Chi_Minh)  
**Scope:** `thuongtran1210/Projectzombie`, branch `feat/trebrave-defense3d-first-scene`.  
**Status:** `DESIGN_DECISIONS_APPROVED / REMOTE_BACKLOG_REVIEWED / G0_NOT_PASSED / NO_GAMEPLAY_CODE`.  
**Authority:** Phê duyệt D-01..D-11 ≠ chấp thuận mọi ticket đã triển khai, dữ liệu local đã commit, hoặc Gate G0 pass.

## 1. Kết luận review source backlog

- **27/27 ID hiện diện, không trùng**: `U3D-001`.. `U3D-027`; **P0=21, P1=3, P2=3**.
- Đã soát các dependency ID nêu tường minh theo văn bản: **không tham chiếu ticket không tồn tại; không thấy chu trình phụ thuộc**. `U3D-024` viết `U3D-010..U3D-023` là shorthand (14 ID), **phải mở rộng khi tạo issue/đưa vào công cụ theo dõi**; không ngầm coi là danh sách explicit dependencies đã được máy parse đầy đủ.
- **24 ticket U3D-001..024** có bảng 6 cột (ID/priority/scope/dependencies/relative size/acceptance), **3 ticket U3D-025..027** là bảng post-Core 5 cột có schema khác theo G8 — không phải lỗi thiếu size.
- **Backlog requirements are reviewed, not accepted:** còn thiếu Git branch/HEAD/stash local, hash cụ thể NavMesh & meta, G0 owner closure; không có code G1 hoặc kết quả Unity PlayMode mới để đánh DONE. **27 ticket trạng thái tracking vẫn DRAFT/BLOCKED (chưa mở Issues).**

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
| **G0-S1** | Owner đã gửi `git status --short`: 17 `M`/8 `??`; local backup script đã ghi `git-branch.txt`, `git-head.txt`, `git-stash.txt` | **PARTIAL** | Đối chiếu/đọc ba file backup với `git branch --show-current`, `git rev-parse HEAD`, `git stash list`; bảo toàn dirty/untracked; không pull/checkout |
| **G0-S2** | Backup path `C:\Users\thuon\Unity\TREBRAVE_G0_Backup_20261023_211614`; owner liệt kê `MapSongQue3D_Prototype.unity` (135896 bytes), `NavMesh-Defense_Navigation.asset` trong backup. 8 file Scene/Environment khớp hash subset | **PARTIAL** | Chỉ đích danh SHA256 Scene, scene `.meta`, NavMesh `.asset/.meta`; kiểm tra backup còn tại path |
| **G0-S3** | GLB `TREBRAVE_MapSongQue_ThreeJS.glb` trong backup, 6507936 bytes; có hình root GLB trên Hierarchy | **PARTIAL** | Hash cụ thể GLB và `.meta`; nếu có importer/material ở vị trí khác phải sao lưu riêng và không đổi GUID |
| **G0-S4** | Owner cung cấp `BackedUp=True` + `HashMatches=True` cho `Packages/manifest.json`, `packages-lock.json`, `ProjectSettings/EditorBuildSettings.asset` | **3 key files VERIFIED / PARTIAL** | Ghi rõ snapshot packages/project settings khác nếu có; không tuyên bố restore test đã qua |
| **G0-S5** | 2 ảnh Console 20 Log/0 Warning/0 Error và root Hierarchy; chủ dự án xác nhận đã kiểm tra Missing Scripts, 3 bridge/10 sockets/NavMesh | **OWNER_REPORTED_VERIFIED** | Ảnh Inspector/PlayMode details nếu cần độc lập nghiệm thu; không mặc định 600s run đã pass |
| **G0-S6** | Chủ dự án xác nhận đã kiểm tra Legacy `SampleScene` | **OWNER_REPORTED_VERIFIED** | Nếu cần QA độc lập, lưu smoke steps/screenshot; không coi full regression suite đã chạy |

**Gate G0 = NOT PASSED** ở thời điểm review, vì G0-S1..S4 chưa hoàn tất bằng chứng chi tiết, ticket/baseline chưa owner đóng G0 và chưa có quyết định cấp quyền triển khai code. Không dùng GUI Scene Generator, không `git reset --hard`, `git clean -fd`, `git stash pop`, không sửa `.meta`, không rebuild/rebake NavMesh.

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
- [ ] **Local branch / HEAD / stash** được đối chiếu với snapshot backup.
- [ ] **SHA-256 cụ thể** Scene+meta, NavMeshData+meta, GLB+meta đều tồn tại và khớp.
- [ ] Nếu có NavMesh/asset/scene khác ngoài tree, **sao lưu thêm và xác minh**.
- [ ] **Chủ dự án phê duyệt đóng Gate G0** sau khi xem evidence còn thiếu và review 27 nhiệm vụ; không giả định từ câu “Tiếp tục”.
- [ ] Chỉ khi Gate G0 pass và chủ dự án cho phép code rõ ràng, mới bắt đầu `U3D-003 → U3D-004 → U3D-005` ở G1. **Không code trong bước Gate G0 review.**

**Liên kết:** [02 — Implementation Backlog](02_IMPLEMENTATION_BACKLOG.md) · [04 — G0 Approval Pack](04_GATE0_APPROVAL_PACK.md) · [07 — Local Safety Runbook](07_G0_LOCAL_SAFETY_RUNBOOK.md) · [10 — G0 Evidence Audit](10_G0_REMOTE_BASELINE_AUDIT.md).
