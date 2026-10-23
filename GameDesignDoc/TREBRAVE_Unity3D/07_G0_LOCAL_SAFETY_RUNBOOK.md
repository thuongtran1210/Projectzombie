# Gate G0 — Unity Local Baseline & Backup Runbook

**Status:** DRAFT / CHƯA THỰC THI TRÊN MÁY NGƯỜI DÙNG · 2026-10-23.
**Goal:** lưu và chứng minh backup sạch của scene Map Sông Quê + gameplay data **trước** code G1. Việc có hướng dẫn này **không phải xác nhận backup đã xong**.

**Remote audit completed 2026-10-23:** [10 — G0 Remote Baseline Audit](10_G0_REMOTE_BASELINE_AUDIT.md) xác minh trên GitHub hiện **không có** `Assets/Scenes/Defense3D` và `Assets/Art/Environment`; Build Settings remote chỉ có Legacy SampleScene. **Đừng pull/checkout/regen để “lấy lại scene” khi chưa backup local**. Sau copy-only backup, kiểm tra xem data NavMesh có folder ngoài danh sách; copy thêm asset đó.
**Máy mục tiêu:** Windows, Unity 2022.3.62f3, local project `C:\Users\thuon\Unity\Projectzombie`.

## 1. Nguyên tắc an toàn

- **Không chạy** Editor menu `TREBRAVE > Defense 3D > Build & Open Map Song Que Prototype` trên scene đã custom.
- **Không chạy** `git reset --hard`, `git clean -fd`, `git restore` hàng loạt, `git stash pop` tự động hoặc xóa/move `.meta` bất kỳ.
- Trước hết thoát **Play Mode**, **Save Scene** (nếu muốn lưu thay đổi đã làm), sau đó **đóng Unity Editor** để chụp snapshot trên đĩa.
- Mọi file `M` và `??` trong `git status` đều có thể chứa Scene/GLB/GUID/NavMesh hợp lệ chưa push, cần giữ nguyên.
- Chỉ sao lưu bằng copy sang **thư mục ngoài repository**; không cài package, không checkout branch, không pull hoặc commit trong runbook này.

## 2. Chụp bản kê Git (chỉ đọc)

Mở PowerShell:
```powershell
$root = 'C:\Users\thuon\Unity\Projectzombie'
Set-Location -LiteralPath $root
git branch --show-current
git rev-parse HEAD
git status --short
git stash list
```

**Không mặc định** local HEAD trùng remote branch; mã Boss Fast Test `10f` có thể còn stash/local khác nhau.

## 3. Sao lưu snapshot ở ngoài repository (copy-only)

**Chạy khi Unity đã đóng; không cần Git tree sạch.** Script dưới đây chỉ `Copy-Item`, không sửa/xóa nguồn, tự bỏ qua path không tồn tại. Tùy chỉnh vị trí backup nếu ổ C không còn dung lượng.

```powershell
$root = 'C:\Users\thuon\Unity\Projectzombie'
$stamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$backup = "C:\Users\thuon\Unity\TREBRAVE_G0_Backup_$stamp"

$paths = @(
  'Assets\Scenes\Defense3D',
  'Assets\Scenes\Defense3D.meta',
  'Assets\Art\Environment',
  'Assets\Art\Environment.meta',
  'Assets\Features\Defense3D',
  'Assets\Features\Defense3D.meta',
  'Assets\Features\DebugUI',
  'Packages\manifest.json',
  'Packages\packages-lock.json',
  'Packages\manifest.json.bak',
  'ProjectSettings\EditorBuildSettings.asset',
  'ProjectSettings\Packages'
)

New-Item -ItemType Directory -Force -Path $backup | Out-Null
foreach ($relative in $paths) {
  $src = Join-Path $root $relative
  if (-not (Test-Path -LiteralPath $src)) { continue }

  $dst = Join-Path $backup $relative
  New-Item -ItemType Directory -Force -Path (Split-Path $dst -Parent) | Out-Null
  Copy-Item -LiteralPath $src -Destination $dst -Recurse -Force -ErrorAction Stop
}

Push-Location -LiteralPath $root
git status --short | Out-File -Encoding UTF8 (Join-Path $backup 'git-status.txt')
git rev-parse HEAD | Out-File -Encoding UTF8 (Join-Path $backup 'git-head.txt')
git branch --show-current | Out-File -Encoding UTF8 (Join-Path $backup 'git-branch.txt')
git stash list | Out-File -Encoding UTF8 (Join-Path $backup 'git-stash-list.txt')
Pop-Location

Write-Host "Backup saved at: $backup"
Get-ChildItem -LiteralPath $backup -Recurse -File |
  Measure-Object -Property Length -Sum |
  Select-Object Count,Sum
```

**Giới hạn:** Không khẳng định bản backup chứa mọi NavMesh asset, vì bake data có thể nằm ở một path khác không liệt kê. Hãy mở Unity Inspector/Project window tìm `NavMeshData`/`NavMeshSurface`, ghi lại đường dẫn asset bake và **sao lưu thêm đúng folder chứa chúng** khi cần. Nếu Unity có auto-save scene chưa được ghi ra disk, snapshot không bao gồm trạng thái chưa Save.

## 4. Kiểm tra bản sao, không thay nguồn

```powershell
# Dùng đúng giá trị $backup ở bước trên
Get-ChildItem -LiteralPath $backup -Recurse -File |
  Where-Object { $_.Extension -in '.unity','.meta','.glb','.gltf','.asset' } |
  Select-Object -First 40 FullName,Length

git status --short
```

**Expected:** snapshot có các file Scene/Meta/GLB chủ chốt; bản `git status` sau backup không đổi trừ chỉnh sửa do tác vụ khác. Với các asset quan trọng, có thể đối chiếu `Get-FileHash -Algorithm SHA256` giữa bản gốc và bản copy (bằng chứng, không chỉnh sửa).

## 5. Baseline Unity QA — làm thủ công sau khi đã backup

1. Mở Unity 2022.3.62f3 đúng project, chờ import/compile; mở **scene gốc mà người dùng thực sự đang chơi** (Prototype hoặc Recovery, **không generate scene mới**).
2. Console `Clear`, xác nhận **0 errors** và **0 Missing (Mono Script)** trên các object core; nếu lỗi thì ghi bug, chưa tự xóa component.
3. Chụp Hierarchy/Inspector chứng minh đúng Hero West, Base West, 3 cầu, 10 TowerSocket, imported GLB render, MapSceneInfo refs.
4. Kiểm tra 3 bridge/NavMesh trong Scene View và chỉ một nguồn spawn `DefenseRun6003D` active; disable các smoke tests cũ **nếu cần kiểm thử**, ghi lại trạng thái ban đầu.
5. Mở `SampleScene` của Survival cũ, ghi test không regression; quay lại scene Defense theo menu Open Scene bình thường, không dùng generator.
6. Ghi lại scene path, git HEAD và các local modified files; mỗi QA ở giai đoạn planning = `NOT_RUN` cho tới khi thực sự chạy.

## 6. Gate G0 Evidence Card (điền sau, chưa pass)

```text
Date/time (Asia/Ho_Chi_Minh):
Local Git branch / HEAD / dirty?:
Unity editor version:
Active scene exact path:
Outside-repository backup path:
Backup verified: YES/NO; number files / hashes:
NavMesh asset locations included: YES/NO/UNSURE:
Project GLB path and size:
Missing Mono Scripts: 0 / count:
Console Errors: 0 / count:
Map 3 bridges + 10 sockets: VERIFIED/NOT_VERIFIED:
Legacy SampleScene: PASS/FAIL/NOT_RUN:
Tester / screenshot / log:
G0-S1..S6: pass/fail/not_run per item:
```

**Approval rule:** **Toàn bộ D-01..D-11 đã được duyệt về thiết kế** (D-07/A đủ 8/8 nguyên tắc; production numbers chưa khóa; **D-10/A là chính sách sao lưu, không phải hành động đã làm**). **Không có bằng chứng backup local G0-S1..S4 hoặc QA G0-S5..S6 tại thời điểm viết**, Gate G0 **CHƯA PASS**. Chỉ sau khi backup/QA được đối chứng và owner đóng Gate mới được cân nhắc code ở phiên khác. Không tự động code sau khi chạy script này.
