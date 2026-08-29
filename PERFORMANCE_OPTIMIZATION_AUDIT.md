# 📋 BÁO CÁO TỔNG HỢP CÁC ĐIỂM CHƯA TỐI ƯU & HƯỚNG XỬ LÝ (PERFORMANCE_OPTIMIZATION_AUDIT.md)

Tài liệu này lưu trữ toàn bộ các phát hiện từ **Unity Profiler**, các vấn đề đã được tối ưu hóa trong phiên làm việc vừa qua và **danh sách các điểm tồn đọng cần tiếp tục tối ưu** cho các Agent kế tiếp.

---

## 🟢 1. CÁC ĐIỂM ĐÃ ĐƯỢC TỐI ƯU HÓA HOÀN TẤT (RESOLVED ISSUES)

| STT | Vấn đề phát hiện | Nguyên nhân kỹ thuật | Giải pháp đã thực thi | Kết quả Profiler |
| :---: | :--- | :--- | :--- | :--- |
| **1** | **Lag 24.2ms khi bật HUD Recast Glow** | `RoutineGlowPulse` gán trực tiếp `Image.color` mỗi frame $\rightarrow$ Canvas bị đánh dấu Dirty và Rebuild lại toàn bộ Vertex Mesh. | Chuyển sang điều khiển độ trong suốt qua `CanvasGroup.alpha`. | `UpdateBatches` giảm từ **24.2ms $\rightarrow$ 0.02ms** (0 GC Alloc). |
| **2** | **Spike 3.6KB GC Alloc khi ném Dép / Xoay Lốc** | `Weapon_Slipper.cs` sử dụng `Physics2D.OverlapCircleAll` liên tục tạo mảng `Collider2D[]` trên Heap. | Chuyển toàn bộ sang `Physics2D.OverlapCircleNonAlloc` với bộ đệm tĩnh `_slipperHitBuffer[32]`. | GC Alloc giảm từ **3.6KB $\rightarrow$ 0 Bytes**. |
| **3** | **Lướt xuyên Tilemap ra ngoài biên Map** | Trước đó chỉ dùng Raycast với Layer Obstacle mà không kiểm tra giới hạn sàn gạch `Tilemap_Ground`. | Bổ sung `MovementPhysicsUtility.CalculateDashDestination` dò từng bước `0.3m` trên `Tilemap.HasTile(cellPos)`. | Phanh dừng chính xác $100\%$ tại mép gạch cuối cùng. |
| **4** | **Quỹ đạo bay Dép không khớp chỉ dấu Parabol** | Code ném dùng `Vector2.Lerp` thẳng hàng trong khi Indicator vẽ đường cong Bezier. | Đồng bộ quỹ đạo bay bằng công thức **Quadratic Bezier Curve 3 điểm**. | Dép bay uốn lượn khớp $100\%$ với dải sáng chỉ dấu. |

---

## 🟡 2. DANH SÁCH CÁC ĐIỂM CẦN TIẾP TỤC TỐI ƯU CHO CÁC PHÁP BẢO / HỆ THỐNG KHÁC (PENDING AUDIT FOR NEXT AGENTS)

Các Agent tiếp theo khi làm việc với vũ khí, quái vật hoặc UI cần rà soát và áp dụng các quy chuẩn sau:

### 1. Quét Toàn Bộ Vũ Khí Để Loại Bỏ `Physics2D.OverlapCircleAll`
* **Hiện trạng:** Một số vũ khí AOE cũ (như `Weapon_Pot`, `Weapon_Pipe`, `Weapon_GrenadeLauncher`, `Weapon_DualSlash`) vẫn có thể đang dùng `Physics2D.OverlapCircleAll`.
* **Nhiệm vụ:**
  - Thay thế bằng `Physics2D.OverlapCircleNonAlloc` hoặc `Physics2D.BoxCastNonAlloc` kết hợp bộ đệm `TargetingUtility.HitBuffer`.
  - Đảm bảo **Zero GC Allocation** trong toàn bộ vòng lặp `FixedUpdate()` và `Coroutine`.

---

### 2. Tối Ưu Hóa Khởi Tạo Đạn Đạo & Visual Của Vũ Khí (`new GameObject` trong Coroutine)
* **Hiện trạng:** Trong `Weapon_Slipper.cs`, mỗi lần quăng dép vẫn đang gọi:
  ```csharp
  GameObject slipperVisual = new GameObject("Slipper_Projectile_Visual");
  slipperVisual.AddComponent<SpriteRenderer>();
  slipperVisual.AddComponent<TrailRenderer>();
  ```
* **Rủi ro:** Khi Hero có tốc đánh cao hoặc bồi đòn liên tục, việc `Instantiate/Destroy` GameObject động sẽ gây phân mảnh bộ nhớ (Memory Fragmentation).
* **Nhiệm vụ cho Agent sau:**
  - Chuyển `Slipper_Projectile_Visual` thành một **Prefab có sẵn trong Object Pool (`VFXPoolManager`)**.
  - Tái sử dụng GameObject thay vì khởi tạo bằng code runtime.

---

### 3. Tối Ưu `ScrollRect.LateUpdate()` Trong Các Menu Lớn (Tàng Bảo Các / Inventory)
* **Hiện trạng:** Ảnh Profiler ghi nhận `ScrollRect.LateUpdate()` tốn $161\text{ms}$ và $1.6\text{MB}$ khi mở menu nhiều vật phẩm.
* **Nhiệm vụ cho Agent sau:**
  - Triển khai cơ chế **Recycle / Virtualized ScrollRect** (chỉ sinh ra số lượng phần tử vừa đủ hiển thị trên màn hình $\approx 6 - 8$ item, tái sử dụng khi cuộn).
  - Tách các UI tĩnh và UI động ra thành **Sub-Canvases riêng biệt** để khi cuộn trang không làm Rebuild toàn bộ màn hình.

---

### 4. Tách Biệt Rõ Ràng Cơ Chế Khựng Hình (Hit-Stop vs Slow-Motion)
* **Hiện trạng:** Khi đòn đánh chí mạng (Critical), `SimpleProjectile` và `Weapon_MeleeBase` gọi `GameJuiceEvents.RequestHitStop(0.04f)` (hạ `Time.timeScale = 0.05f`).
* **Lưu ý thiết kế:**
  - Đối với các kỹ năng lướt liên tục (Dash) hoặc kỹ năng dựng tường (Nước Thánh), **không nên lạm dụng HitStop** vì sẽ làm người chơi có cảm giác bị "drop FPS".
  - Chỉ nên kích hoạt HitStop cho: **Đòn chém chí mạng đơn mục tiêu mạnh mẽ, đòn kết liễu Combo 3, hoặc khi tiêu diệt Mini-Boss/Boss**.
