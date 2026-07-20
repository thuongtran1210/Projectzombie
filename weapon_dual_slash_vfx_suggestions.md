# Đề Xuất Nâng Cấp Hiệu Ứng Visual Cho Weapon_DualSlash

Tài liệu này đề xuất các phương án nâng cấp hiệu ứng hình ảnh (VFX) và cảm giác đòn đánh (Game Feel / Juiciness) cho vũ khí `Weapon_DualSlash` (Đòn chém đa hướng) trong Unity 2022.

---

## 1. Các Vấn Đề Hiện Tại
* **VFX tĩnh ở tâm:** Hiện tại game chỉ gọi `PlaySlashVFX()` kích hoạt một Particle System duy nhất tại tâm nhân vật. Khi số lượng hướng chém (`slashCount`) tăng lên (ví dụ: từ 2 lên 4, 6, 8 hướng), Particle System tĩnh không thể tự động căn chỉnh và xoay theo đúng hướng của từng nhát chém thực tế.
* **Thiếu cảm giác va chạm (Impact):** Khi chém trúng quái vật, chưa có hiệu ứng phản hồi trực quan (như tóe lửa, rung màn hình nhẹ hoặc khựng hình).

---

## 2. Các Đề Xuất Nâng Cấp Visual Đẹp Mắt

### Đề xuất 1: Xoay VFX Theo Hướng Chém Thực Tế (Dynamic Slash Placement)
Thay vì dùng một Particle System tĩnh tại tâm, chúng ta sẽ sinh ra (hoặc lấy từ Object Pool) các vệt chém hình lưỡi liềm (Crescent Slash VFX) tại đúng vị trí `hitCenter` và xoay theo đúng góc độ chém.

* **Cách hoạt động:**
  * Tạo một Prefab VFX vệt chém lưỡi liềm đẹp mắt (sử dụng Particle System với Shape dạng Arc hoặc Sprite Animation cuộn tròn).
  * Trong vòng lặp chém, Instantiate hoặc lấy từ Pool Prefab này, set vị trí tại `hitCenter` và xoay góc Z bằng `angle`.
  * Hiệu ứng này sẽ tự động biến mất sau khi phát xong (~0.15s - 0.2s).

```csharp
// Ví dụ logic tích hợp Object Pool cho VFX chém theo hướng:
[SerializeField] private ParticleSystem directionalSlashPrefab;

// Trong PerformAttack():
for (int i = 0; i < slashCount; i++)
{
    float angle = baseAngle + (i * angleStep);
    float rad = angle * Mathf.Deg2Rad;
    Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    Vector2 hitCenter = center + (direction * forwardOffset);

    // Sinh ra VFX tại hitCenter và xoay theo angle
    SpawnSlashVFX(hitCenter, angle);
}
```

---

### Đề xuất 2: Tiến Trình Hiệu Ứng Theo Cấp Độ (Visual Progression)
Khi người chơi nâng cấp vũ khí (từ Level 1 lên Level 6), màu sắc và độ hoành tráng của vệt chém nên tiến hóa để tạo cảm giác bá đạo:

* **Level 1 - 2 (2 Hướng chém):** Vệt chém màu xanh Neon thanh mảnh, sắc nét (thể hiện kiếm khí cơ bản).
* **Level 3 - 4 (4 Hướng chém):** Vệt chém đổi sang màu cam đỏ lửa (Fire Slash), phát ra thêm các tia lửa điện (sparks) nhỏ bay về phía trước nhát chém.
* **Level 5 - 6 (6+ Hướng chém):** Vệt chém chuyển sang màu tím tối/đỏ thẫm (Void/Doom), khi chém phát ra một vòng sóng xung kích (Shockwave ring) lan tỏa dưới mặt đất làm rung nhẹ cảnh vật.

---

### Đề xuất 3: Tạo Cảm Giác Đòn Đánh (Impact Feel & Juiciness)
Để đòn chém có lực và "đã tay" hơn:

1. **Hiệu ứng Tóe Lửa khi trúng quái (Hit Sparks):**
   * Khi gọi `DealDamageInArea`, nếu chém trúng quái, sinh ra một Particle tóe lửa nhỏ hoặc vệt máu bắn tại vị trí của quái vật.
2. **Khựng hình siêu ngắn (Hit Stop / Frame Freeze):**
   * Khi chém trúng đòn chí mạng (Crit Hit), giảm `Time.timeScale` xuống `0.05f` trong khoảng `0.05` giây, sau đó trả về `1f`. Điều này tạo ra độ khựng giống các game chặt chém AAA.
3. **Rung màn hình nhẹ (Camera Shake):**
   * Rung nhẹ camera dựa trên sát thương gây ra hoặc khi có chí mạng.

---

### Đề xuất 4: Vệt Chém Trên Mặt Đất (Ground Decals / Slash Marks)
* Khi nhát chém quét qua, để lại các vệt đen xém hoặc vết nứt đất mờ ảo trên nền đất. Các vệt này sẽ mờ dần (Fade out) và biến mất sau 0.5 giây để tránh làm rối màn hình.

---

## 3. Các Bước Triển Khai Tiếp Theo
Bạn muốn triển khai tính năng nào trước? Tôi có thể giúp viết code tích hợp các hiệu ứng trên vào class [Weapon_DualSlash.cs](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/Weapons/Weapon_DualSlash.cs) một cách tối ưu nhất (sử dụng Object Pool để tránh giật lag khi chơi lâu).
