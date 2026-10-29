# Defense3D — Hiệu ứng dash

## Hành vi đã giữ nguyên

Dash vẫn do DefenseHeroMotor3D di chuyển và DefenseDashTraversal3D kiểm tra đường đi. Không thay range, duration, cooldown, va chạm hoặc điều kiện cast. Hành vi cũ được giữ: pause, disable, Downed và reset sẽ hủy dash.

VFX chỉ nghe kết quả:

```text
Motor chấp nhận BeginDash
    → Started(ID, session, run, vị trí, hướng)
    → burst đầu + trail bám anchor
Motor hoàn tất đoạn đi hợp lệ
    → Ended(Completed = true)
    → thu hồi burst đầu/trail, phát burst cuối
Motor bị hủy hoặc traversal từ chối
    → Ended(Completed = false)
    → thu hồi VFX, không phát burst hoàn tất
```

Motor đóng trạng thái trước callback, nên cancel tái nhập không phát Ended nhiều lần. ID tăng theo từng lượt và không reset khi restart. Vị trí hoàn tất dùng điểm safe đã được traversal chấp nhận; không đọc body.position chưa kịp cập nhật sau MovePosition.

## Ownership và callback cũ

DefenseDashFeedbackAdapter giữ riêng ba handle start/trail/end. Không gọi StopOwner để tránh dừng nhầm hiệu ứng đánh thường hoặc pháp bảo trên cùng player.

Adapter kiểm tra session/run, ID dash và trạng thái owner. End của lượt cũ không thể dừng trail lượt mới. Khi Play phát sinh callback cancel/reset ngay trong lúc tạo object, handle trả về muộn được thu hồi bằng revision của lượt phát. Pause, RunCleanup, Downed và Unbind đều dọn handle theo cách gọi lặp an toàn.

DefensePresentationSignal được dùng chung cho đánh thường và dash: danh sách observer được cache lúc đăng ký/gỡ, lỗi VFX hoặc logger không làm hỏng trạng thái gameplay.

## Asset và Hierarchy đã gán

Các asset nguồn nằm trong `Assets/Features/Defense3D/Effects/Assets`:

- VFX_DashBurst_VongXuyen.prefab: module ring dùng cho hai đầu dash.
- VFX_DashTrail_VongXuyen.prefab: TrailRenderer với module DefenseTrailEffectVisual3D, không particle hoặc logic gameplay.
- MAT_DashTrail_VongXuyen.mat: shader CombatLine đang có, nhận màu vertex.
- FX_DashStart: burst xanh ngọc, 0.22 giây.
- FX_DashTrail: xanh ngọc dịu, sustained, priority 3.
- FX_DashEnd: ngà vàng, 0.30 giây.
- FXSet_HeroDash: bán kính burst 0.65, trail 0.50, lift 0.04.

Scene MapSongQue3D_Prototype đã được gán DefenseHeroDashPresenter3D, motor, health, style, surface và DashGroundAnchor. Anchor là child của hero với local Y -1.08 theo chiều cao hero hiện tại. Burst đặt theo surface; trail bám anchor để đi theo pose render của player.

Instance trail nằm dưới DefenseRuntime/Effects/Attached, không parent trực tiếp vào hero. Khi dừng, pool đưa về Pools/Effects/theo prefab ở frame an toàn.

## Vòng đời trail

Profile trail là Sustained: chỉ dừng bằng handle theo sự kiện motor. Duration trong profile không quyết định thời gian dash. TrailRenderer.time = 0.16 chỉ điều khiển độ dài đuôi hình học.

Trail không autodestruct, không phát sẵn trên prefab, không đổ bóng. Mỗi lượt Prepare đặt màu/độ rộng trước khi activate. Khi trả pool, reset tắt emission, clear điểm cũ, tắt renderer và khôi phục màu/độ rộng. Việc tái sử dụng ở vị trí khác không nối một đường trail từ vị trí cũ.

Không thêm Update vào presenter hoặc module trail. Service chỉ chạy coroutine trong lúc có hiệu ứng active; TrailRenderer tự sinh hình học khi anchor di chuyển.

## Unit Test và kiểm tra

- DefenseDashFeedbackTests: start/completed/cancel, scope, pause/restart, ID cũ, duplicate, Dispose, cancel trong Play, dash mới trong callback burst cuối và reset trail.
- DefenseDashMotorTests: Begin bị từ chối, cancel tái nhập, điểm safe khi hoàn tất, traversal từ chối, pause/restart, Downed và lỗi observer không hủy gameplay.
- DefenseDashAuthoringTests: profile timed/sustained, trail không autodestruct, thời gian/độ rộng và wiring scene.

Đã biên dịch runtime/test source bằng Roslyn ngoài Unity. Chưa chạy Unit Test, Unity, Play Mode, Profiler hoặc trình biên dịch shader của Unity. Warning obsolete có sẵn trong DamageProcessorTests nằm ngoài phạm vi.

Người dùng kiểm tra trực quan khi chạy: độ cao DashGroundAnchor trên địa hình, độ rộng/độ dài trail, hai burst, dash bị chặn, Downed, pause và restart. Signature Thư Sinh đã được chuyển tiếp theo; xem DEFENSE3D_SCHOLAR_SIGNATURE_EFFECTS.md.

## Khả năng nhìn thấy trên địa hình

Material dash đặt `_ZTest = Always` để trail bám anchor sát chân không bị địa hình che. CombatLine có property depth test, mặc định vẫn LEqual cho các material khác. Burst và trail được tăng kích thước để dễ thấy trong dash 0.25 giây; không đổi thời gian di chuyển hoặc cooldown. Unit test kiểm tra cấu hình material và độ rõ của profile. Cần người dùng kiểm tra trực quan trong Unity; chưa xác nhận đây là nguyên nhân duy nhất của báo cáo mất hiệu ứng.
