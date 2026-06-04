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
- **`Hit_SingleTarget`**: Xử lý việc phát hiện chạm vào kẻ địch, trừ máu 1 lần và quản lý xuyên thấu (Piercing). (Dùng cho đạn bay).
- **`Hit_Periodic`**: Cho phép đạn đi xuyên qua kẻ địch và gây sát thương liên tục mỗi X giây nếu kẻ địch còn đứng trong đạn. (Dùng cho vòng xoay/lửa/Aura).
- *Yêu cầu: Prefab phải có `Collider2D` được bật **IsTrigger = true**.*

### Đôi Chân Di Chuyển (Movement - Tùy chọn)
- **`Move_Linear`**: Xử lý vật lý giúp viên đạn bay theo một hướng với tốc độ cố định. (Yêu cầu `Rigidbody2D`).
- **`Move_Orbit`**: Giúp viên đạn xoay tròn xung quanh một tâm điểm (thường là Player) mãi mãi. (Không cần `Rigidbody2D`).
- **Lưu ý**: Nếu bạn thiết kế các đòn đánh tại chỗ (như Nhát chém/Whip), bạn **KHÔNG CẦN** Component này. Viên đạn sẽ xuất hiện, chớp nhoáng gây sát thương rồi tự biến mất (Lifetime ngắn ~0.15s).

---

## Hướng Dẫn Nhanh Cách Tạo Vũ Khí Mới

1. Tạo một GameObject con nằm trong `Player`.
2. Gắn một Component Kế thừa từ `WeaponBase` (Ví dụ: `Weapon_Targeted`).
3. Tạo Prefab hình ảnh Đạn/Vệt chém, gắn đủ `Rigidbody2D`, `BoxCollider2D` và các mảnh Lego (`ProjectileCore`, `Hit_SingleTarget`...).
4. Kéo thả Prefab đó vào ô **Projectile Prefab** của `Weapon` tạo ở bước 2.
5. `WeaponManager` sẽ tự động tìm thấy và vận hành nó!

---

## 3. Cơ Chế Tiến Hóa Vũ Khí (Weapon Evolution)

Tiến hóa vũ khí là cơ chế cho phép nâng cấp một vũ khí đã đạt cấp độ tối đa (hoặc đủ điều kiện) thành một phiên bản mới, mạnh mẽ hơn và thường thay đổi hoàn toàn cơ chế bắn/đánh.

### Cách cấu hình một Tiến hóa:
1. Tạo một `UpgradeData` mới (chuột phải > `ProjectZombie/Upgrades/Upgrade Data`).
2. Đặt `Upgrade Type` là **`WeaponEvolution`**.
3. **`Weapon Id`**: Nhập chính xác ID của vũ khí gốc (để hệ thống tìm và xóa nó đi).
4. **`Weapon Prefab`**: Kéo thả Prefab của Vũ khí Tiến Hóa (phiên bản mạnh hơn).
5. **`Required Weapon Level`**: Đặt cấp độ tối thiểu mà vũ khí gốc phải đạt được (ví dụ: Level 8).
6. **`Required Passive Id`**: (Tùy chọn) Điền ID của vật phẩm bị động yêu cầu (ví dụ: "exp_tome"). Bỏ trống nếu không yêu cầu vật phẩm ghép.

### Luồng hoạt động:
- Khi lên cấp, `UpgradeManager` sẽ quét kho vũ khí của người chơi (`WeaponManager.ActiveWeapons`).
- Nếu người chơi có vũ khí khớp với `Weapon Id` và đạt đủ `Required Weapon Level`. 
- Đặc biệt, hệ thống sẽ kiểm tra xem `PlayerPassives` của nhân vật đã sở hữu thẻ Passive (được định nghĩa trong `Required Passive Id`) hay chưa.
- Khi người chơi chọn thẻ này, `WeaponManager` sẽ tự động Hủy vũ khí cũ và Instantiate vũ khí Tiến hóa mới.

---

## 4. Hệ Thống Nâng Cấp (Upgrade System)

Trong trận đấu, người chơi nhận EXP từ việc tiêu diệt quái vật. Khi lên cấp, người chơi được chọn 1 trong 3 nâng cấp ngẫu nhiên.

### Mục tiêu của hệ thống:
- Tạo cảm giác phát triển sức mạnh liên tục.
- Tạo nhiều hướng build khác nhau.
- Dễ theo dõi trên livestream.
- Không yêu cầu người chơi sử dụng kỹ năng thủ công.

### Phân loại Nâng cấp:
Mỗi nâng cấp thuộc một trong các nhóm sau:
1. **Weapon Upgrade**
2. **Signature Skill Upgrade**
3. **Common Upgrade**
4. **Faction Counter Upgrade**
5. **Rare Upgrade**
6. **Evolution Upgrade**

---

### Chi Tiết: Weapon Upgrade

Nâng cấp trực tiếp cho vũ khí chính của Hero. Mỗi vũ khí có **tối đa 6 cấp**.

- **LEVEL 1 (Cơ bản):** Mở khóa vũ khí cơ bản.
  - *Ví dụ: Stream Blade - Phóng 1 lưỡi kiếm năng lượng, tấn công mục tiêu gần nhất.*
- **LEVEL 2 (Đa mục tiêu):** Tăng số lượng đòn đánh / đạn (`+1 Projectile`).
  - *Ghi chú: Giúp tăng khả năng dọn quái đầu trận.*
- **LEVEL 3 (Sát thương):** Tăng sát thương (`+20% Damage`).
  - *Ghi chú: Là nâng cấp ổn định cho mọi build.*
- **LEVEL 4 (Hiệu ứng phụ):** Mở khóa hiệu ứng đặc biệt (Ricochet, Chain Attack, Explosion).
  - *Ghi chú: Đây là mốc thay đổi gameplay đầu tiên.*
- **LEVEL 5 (Tốc độ):** Tăng tốc độ tấn công (`+30% Attack Speed`).
  - *Ghi chú: Tăng DPS tổng thể.*
- **LEVEL 6 (Hiệu ứng cuối):** Mở khóa hiệu ứng tối thượng (Nổ diện rộng, Xuyên nhiều mục tiêu, Bắn thêm đạn phụ).
  - *Ghi chú: Đây là cấp tối đa của vũ khí.*
