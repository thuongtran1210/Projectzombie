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

### `Weapon_MeleeBase` (Kế thừa WeaponBase)
- Vai trò: Chuyên dùng cho vũ khí Cận Chiến.
- Tính năng: Tối ưu hóa bằng cách dùng `Physics2D.OverlapBoxNonAlloc` (Zero Allocation) để quét mục tiêu thay vì phải gọi GameObject Đạn vật lý.

### `Weapon_ProjectileBase` (Kế thừa WeaponBase)
- Vai trò: Lớp trừu tượng cơ sở cho MỌI loại vũ khí có sinh ra đạn (`ProjectileData`).
- Tính năng: Tự động quản lý việc clone ScriptableObject `ProjectileData` độc lập cho từng vũ khí, xử lý đổi Prefab đạn khi thăng cấp (`OnLevelUp`), và dọn dẹp tài nguyên khi bị hủy (`OnDestroy`).

### `Weapon_RangedBase` (Kế thừa Weapon_ProjectileBase)
- Vai trò: Chuyên dùng cho vũ khí Bắn Xa theo hướng (Directional / Fired).
- Tính năng: Áp dụng các chỉ số riêng cho đạn bay như tăng tốc độ bay của đạn (`projectileData.Speed += modifier.projectileSpeedBonus`). Các vũ khí đại diện: `Weapon_Targeted` (Nỏ Thần, Bút Phán Quan), `Weapon_Shotgun`, `Weapon_Crossbow`.

### `Weapon_Orbit` (Kế thừa Weapon_ProjectileBase)
- Vai trò: Chuyên dùng cho vũ khí Vòng xoay / Hào quang bảo vệ (Orbit / Aura).
- Tính năng: Kích hoạt sinh đạn xoay quanh người chơi theo nhịp hồi chiêu (`PerformAttack()`). Độc lập hoàn toàn khỏi logic tăng tốc độ bay của đạn bắn xa (`RangedBase`). Tự động đăng ký nghe sự kiện `OnProjectileDespawned` từ `ProjectileSystem` để dọn dẹp danh sách quản lý đạn rác.

### Các loại Vũ Khí Thực Tế (Weapon Implementations)
- `Weapon_Targeted` (Ranged): Tự động tìm kiếm kẻ địch gần nhất trong tầm với và phóng đạn vào chúng.
- `Weapon_DualSlash` (Melee): Đánh ra 2 nhát chém ngược hướng nhau (trái/phải) cùng một lúc.
- `Weapon_Orbit` (Orbit): Sinh các lá bùa (`W003`) xoay tròn bảo vệ nhân vật theo đợt hồi chiêu.
- *(Có thể tạo thêm: `Weapon_Aura`, `Weapon_RandomStrike`...)*

---

## 2. Hệ Sinh Thái Đạn Data-Driven (Projectile System)

Hệ thống đạn được chuẩn hóa hoàn toàn theo kiến trúc **Data-Driven / GAS-Inspired** (`Assets/Features/Projectiles/`).

### Cấu trúc chính:
- **`ProjectileData`**: ScriptableObject định nghĩa các chỉ số cơ bản của đạn (Tốc độ, Thời gian sống, Layer va chạm) và danh sách các `ProjectileBehaviorData`.
- **`ProjectileSystem`**: Quản lý việc Spawn đạn và tự động tái chế qua `ProjectilePool`.
- **`ProjectileController`**: Component điều khiển cốt lõi gắn trên Prefab đạn.

### Các Behavior phổ biến:
- **`StraightBehavior`**: Đẩy đạn bay thẳng theo hướng bắn.
- **`HomingBehavior`**: Tự động bẻ hướng đạn đuổi theo mục tiêu gần nhất.
- **`PierceBehavior`**: Cho phép đạn đâm xuyên qua N mục tiêu.
- **`BounceBehavior`**: Nảy bật khi va chạm kẻ địch.
- **`ExplosionBehavior`**: Gây sát thương AOE xung quanh điểm va chạm.
- **`OrbitBehavior`**: Xoay tròn xung quanh nhân vật (dùng cho vũ khí Orbit/Aura).
- **`PeriodicHitBehavior`**: Gây sát thương giật định kỳ lên kẻ địch đứng trong vùng sát thương mà không bị Despawn.

---

## Hướng Dẫn Nhanh Cách Tạo Vũ Khí Mới

1. Tạo một GameObject con nằm trong `Player`.
2. Gắn một Component Kế thừa từ `WeaponBase` (Ví dụ: `Weapon_Targeted` hoặc `Weapon_Orbit`).
3. Tạo `ProjectileData` (chuột phải `Create > ProjectZombie > Projectiles > ProjectileData`) và gắn các Behavior tương ứng (như `Straight`, `Homing`, `Orbit`...).
4. Kéo thả `ProjectileData` đó vào ô **Projectile Data** của `Weapon`.
5. `WeaponManager` và `ProjectileSystem` sẽ tự động vận hành và tối ưu bộ nhớ qua Object Pool!

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
