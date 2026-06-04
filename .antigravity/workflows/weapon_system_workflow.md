# Tài Liệu Hệ Thống Vũ Khí (Weapon System) & Đạn (Projectile)

Hệ thống vũ khí của trò chơi được thiết kế theo cấu trúc **Đa Vũ Khí (Multi-Weapon)** kết hợp với **Kiến trúc Component-Based (Lắp ráp Lego)** dành cho các loại đạn. Điều này mang lại khả năng mở rộng vô hạn, giúp dễ dàng tạo ra hàng chục loại vũ khí mới (như game Vampire Survivors) mà không cần viết lại mã nguồn nền tảng.

---

## 1. Hệ Sinh Thái Vũ Khí (Weapon Manager)

Quản lý việc sở hữu vũ khí và logic ra đòn của nhân vật.

### `WeaponManager`
- Nơi gắn: Trên `Player`.
- Vai trò: Đóng vai trò như một chiếc "Balo". Tự động thu thập toàn bộ các vũ khí con (đang gắn vào Player) vào một danh sách.
- Mỗi khung hình, Manager sẽ lặp qua tất cả vũ khí và kích hoạt hàm `Tick()` của chúng.

### `WeaponBase`
- Vai trò: Lớp trừu tượng (Abstract) nền móng cho MỌI loại vũ khí.
- Tính năng: Quản lý thời gian hồi chiêu (Cooldown) dựa trên AttackSpeed và cung cấp hàm gọi `PerformAttack()`. Không hề ôm đồm logic Đạn hay Object Pool (đảm bảo Single Responsibility).

### `Weapon_RangedBase` (Kế thừa WeaponBase)
- Vai trò: Chuyên dùng cho vũ khí Bắn Xa.
- Tính năng: Tự động thiết lập một **Object Pool** riêng để tái chế đạn, tối ưu hóa bộ nhớ. Các vũ khí con (như `Weapon_Targeted`) sẽ dùng lớp này để gọi Đạn.

### `Weapon_MeleeBase` (Kế thừa WeaponBase)
- Vai trò: Chuyên dùng cho vũ khí Cận Chiến.
- Tính năng: Tối ưu hóa bằng cách dùng `Physics2D.OverlapBoxNonAlloc` (Zero Allocation) để quét mục tiêu thay vì phải gọi GameObject Đạn vật lý.

### Các loại Vũ Khí Thực Tế (Weapon Implementations)
- `Weapon_Targeted` (Ranged): Tự động tìm kiếm kẻ địch gần nhất trong tầm với và phóng đạn vào chúng.
- `Weapon_DualSlash` (Melee): Đánh ra 2 nhát chém ngược hướng nhau (trái/phải) cùng một lúc.
- *(Có thể tạo thêm: `Weapon_Aura`, `Weapon_RandomStrike`...)*

---

## 2. Hệ Sinh Thái Đạn Lắp Ráp (Lego Projectile)

Thay vì một file `Projectile.cs` dài ngoằng và khó quản lý, hệ thống đạn giờ được chia thành các Component nhỏ chuyên biệt. Mỗi Prefab đạn là sự kết hợp của 3 yếu tố cốt lõi:

### Phần Cốt Lõi (Core)
- **`ProjectileCore`**: Trái tim của viên đạn. Quản lý thời gian sống (Lifetime) và lưu trữ cục bộ các chỉ số sát thương (`DamageData`) được truyền vào từ Weapon.

### Cánh Tay Va Chạm (Impact)
- **`Hit_SingleTarget`**: Xử lý việc phát hiện chạm vào kẻ địch (thông qua `OnTriggerEnter2D`), trừ máu, và quản lý logic Xuyên thấu (Piercing).
- *Yêu cầu: Prefab phải có `Collider2D` được bật **IsTrigger = true**.*

### Đôi Chân Di Chuyển (Movement - Tùy chọn)
- **`Move_Linear`**: Xử lý vật lý giúp viên đạn bay theo một hướng với tốc độ cố định.
- *Yêu cầu: Prefab phải có `Rigidbody2D` (Khuyến nghị dùng Kinematic).*
- **Lưu ý**: Nếu bạn thiết kế các đòn đánh tại chỗ (như Nhát chém/Whip), bạn **KHÔNG CẦN** Component này. Viên đạn sẽ xuất hiện, chớp nhoáng gây sát thương rồi tự biến mất (Lifetime ngắn ~0.15s).

---

## Hướng Dẫn Nhanh Cách Tạo Vũ Khí Mới

1. Tạo một GameObject con nằm trong `Player`.
2. Gắn một Component Kế thừa từ `WeaponBase` (Ví dụ: `Weapon_Targeted`).
3. Tạo Prefab hình ảnh Đạn/Vệt chém, gắn đủ `Rigidbody2D`, `BoxCollider2D` và các mảnh Lego (`ProjectileCore`, `Hit_SingleTarget`...).
4. Kéo thả Prefab đó vào ô **Projectile Prefab** của `Weapon` tạo ở bước 2.
5. `WeaponManager` sẽ tự động tìm thấy và vận hành nó!
