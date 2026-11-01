# Điếu Cày — HUD, ngắm vùng và khói chiến đấu

Hoàn thiện ngày 01/11/2026 cho Defense3D / Unity 2022.3.62f3.

- Nút dùng icon Điếu Cày hiện có trên nền ngọc tối, khung vàng. Tên và hướng dẫn
  nằm riêng dòng; hiển thị `THỔI KHÓI`, `THẢ ĐỂ THỔI`, `THẢ ĐỂ HỦY`, `ĐANG HỒI`.
- Vòng ngoài thể hiện thời gian vùng khói; số giữa nút vẫn là hồi chiêu.
- Vùng ngắm có tâm, mũi tên hướng và đĩa hổ phách bán kính bằng định nghĩa đã
  áp dụng nâng cấp. Khi hủy, vùng và hướng dẫn đổi đỏ.
- Hiệu ứng ba lớp renderer: ấn/vùng dưới đất, khói cuộn thấp, tàn lửa. Shader dùng
  tiến độ session, không dùng `_Time`; không dựng mesh/material mỗi frame.
- Giữ thông số cơ bản: tầm 9 m, bán kính 4 m, damage 65, hồi 16 s, khói 4,5 s.
  Nổ đầu bằng 0,5 damage; DoT bằng 0,15 damage mỗi 0,5 s; các hệ số có sẵn giữ nguyên.
- Pause giữ vùng khói và dừng đồng hồ; disable, hero gục, restart/cleanup dừng
  handle của vùng. Thiếu presentation không ngăn gameplay gây sát thương.

## Điểm nối

`Skill_DieuCay.asset` giữ icon và `PresentationProfile` tham chiếu trực tiếp tới
`Assets/Features/Defense3D/Effects/Assets/PipeSmoke/FX_PipeSmoke.asset`. Các asset
mới không đặt trong Resources. Composition truyền effect service của trận sau
khi áp loadout; service prewarm một instance, quản lý lifetime và trả pool.

`DefensePipeSmokeVisual3D` chỉ render; `DefensePipeRelicEffect3D` giữ hazard và
damage. HUD được presenter cung cấp chuỗi đã định dạng, cache theo phần mười giây.
Profile hiện được builder đồng bộ Duration với skill; chạy builder nếu sửa Duration.

## Authoring và kiểm chứng

Menu Unity `Tools > VFX Generator`:

- `Build Pipe Smoke Presentation`: cập nhật riêng asset Điếu Cày, HUD slot pháp
  bảo và các trường area tùy chọn trong prefab chỉ dấu; không đổi số liệu combat.
- `Preview Pipe Smoke Presentation`: scene preview cô lập; sheet 2×2 ở
  `scratch/PipeSmokePreview.png`: ngắm / impact / duy trì / tan.
- `Run Pipe Smoke Match Check`: vào trận qua luồng app thật với Đạo Sĩ/Điếu Cày,
  chụp aim/cancel/impact/active, kiểm tra cooldown owner, pool, pause, expiry và
  trở về sảnh. Tự thoát Play Mode và khôi phục playModeStartScene. Yêu cầu scene sạch.

Cũng có thể dùng bridge legacy:

```powershell
./Tools/Invoke-UnityBridge.ps1 build-pipe -TimeoutSeconds 60
./Tools/Invoke-UnityBridge.ps1 preview-pipe -TimeoutSeconds 60
./Tools/Invoke-UnityBridge.ps1 smoke-pipe -TimeoutSeconds 60
```

Kết quả trận: `scratch/PipeSmokeMatchCheck.txt` và `scratch/PipeSmokeAim.png`,
`PipeSmokeCancel.png`, `PipeSmokeImpact.png`, `PipeSmokeActive.png`.
Các fixture liên quan: `DefensePipeRelicEffect3DTests`, `DefensePipePresentationTests`,
`DefenseEffectAuthoringTests`, `DefenseSignatureInterruptionTests`, `OwnedPoolTests`.
Lần kiểm chứng cuối: 32 tests đạt, 0 lỗi; match check đạt sau bản chỉnh hình ảnh cuối.

Ảnh tĩnh và kiểm tra trận xác minh việc tích hợp/hình dạng/lifecycle. Trình check
gọi view để biểu diễn ngắm/hủy và cast qua cooldown runtime; chưa thay thế kiểm
thử kéo thả trên màn hình cảm ứng. Chưa đo FPS, allocations, overdraw hoặc GPU
trên thiết bị Android. Ba renderer/instance không đồng nghĩa ba draw calls toàn trận.
