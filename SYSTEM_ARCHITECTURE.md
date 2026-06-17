# Tài Liệu Kiến Trúc Hệ Thống - ProjectZombie

Tài liệu này cung cấp cái nhìn tổng quan về kiến trúc và các hệ thống cốt lõi trong dự án **ProjectZombie** (Unity/C#). Hệ thống được thiết kế theo hướng Component-Based, Data-Driven, và áp dụng nhiều mẫu thiết kế (Design Patterns) giúp dự án dễ dàng mở rộng và bảo trì.

---

## 1. Hệ Thống Nhân Vật & Chỉ Số (Player & Stats System)
Quản lý trạng thái, chỉ số cơ bản, hoạt ảnh, lượng máu và kinh nghiệm của nhân vật người chơi.

*   **[PlayerController.cs](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/Player/PlayerController.cs)**: Điểm điều khiển trung tâm của nhân vật. Xử lý di chuyển vật lý (Rigidbody2D), nhận diện input và cập nhật trạng thái hoạt ảnh.
*   **[PlayerStats.cs](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/Player/PlayerStats.cs)**: Lưu trữ và tính toán các chỉ số của nhân vật (Máu, Giáp, Tốc độ chạy, Sát thương, Tốc độ đánh, Tầm đánh, Tỷ lệ chí mạng, v.v.). Hỗ trợ hệ thống Modifier để cộng thêm chỉ số từ các thẻ Nâng cấp.
*   **[HealthSystem.cs](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/Player/HealthSystem.cs)**: Quản lý lượng máu hiện tại, nhận sát thương, hồi máu và sự kiện khi nhân vật tử vong.
*   **[PlayerExperience.cs](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/Player/PlayerExperience.cs)**: Quản lý cấp độ (Level) và điểm kinh nghiệm (EXP). Khi đủ điểm kinh nghiệm, nó sẽ kích hoạt sự kiện lên cấp để mở giao diện nâng cấp.
*   **[PlayerPassives.cs](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/Player/PlayerPassives.cs)**: Lưu trữ danh sách các kỹ năng bị động (Passives) hiện tại của nhân vật. Được dùng làm điều kiện để kích hoạt Tiến hóa Vũ khí (Evolution).

---

## 2. Hệ Thống Vũ Khí (Weapon System)
Được thiết kế theo cấu trúc **Đa Vũ Khí (Multi-Weapon)** gắn trực tiếp trên Player.

*   **[WeaponManager.cs](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/Weapons/WeaponManager.cs)**: Gắn trên Player. Quản lý danh sách các vũ khí đang hoạt động và tự động gọi hàm `Tick()` của từng vũ khí trong mỗi khung hình.
*   **[WeaponBase.cs](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/Weapons/WeaponBase.cs)**: Lớp cơ sở trừu tượng cho mọi vũ khí. Quản lý cấp độ vũ khí và thời gian hồi chiêu (Cooldown) dựa trên tốc độ tấn công của Player.
*   **[Weapon_RangedBase.cs](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/Weapons/Weapon_RangedBase.cs)**: Lớp cơ sở chuyên dùng cho vũ khí bắn xa. Quản lý việc tạo **Object Pool** riêng cho loại đạn của vũ khí đó nhằm tối ưu bộ nhớ.
*   **[Weapon_MeleeBase.cs](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/Weapons/Weapon_MeleeBase.cs)**: Lớp cơ sở cho các đòn cận chiến. Sử dụng cơ chế quét mục tiêu không cấp phát bộ nhớ (`Physics2D.OverlapBoxNonAlloc`) để tối ưu hóa hiệu năng.
*   **Các loại vũ khí thực tế**:
    *   `Weapon_Targeted`: Tự động bắn đạn vào kẻ địch gần nhất trong tầm hoạt động.
    *   `Weapon_DualSlash`: Thực hiện 2 nhát chém ngược hướng trái/phải cùng một lúc.
    *   `Weapon_Orbit`: Tạo vòng đạn bay quanh nhân vật.
    *   `Weapon_PetSummon`: Triệu hồi đệ/thú cưng đồng hành hỗ trợ chiến đấu.

---

## 3. Hệ Thống Đạn Lắp Ráp (Lego Projectile System)
Hệ thống Đạn được thiết kế theo hướng tách biệt logic di chuyển, va chạm và lõi để tăng khả năng tái sử dụng (Lego-style).

*   **[ProjectileCore.cs](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/Weapons/ProjectileCore.cs)**: Thành phần bắt buộc trên mỗi Prefab đạn. Lưu trữ dữ liệu sát thương (`DamageData`) được truyền từ Weapon và xử lý thời gian tồn tại (Lifetime).
*   **Thành phần di chuyển (Movement Components)**:
    *   `Move_Linear`: Di chuyển thẳng theo hướng chỉ định bằng Rigidbody2D.
    *   `Move_Orbit`: Cho phép đạn xoay quanh một tâm (Player).
*   **Thành phần va chạm & gây hại (Hit Components)**:
    *   `Hit_SingleTarget`: Gây sát thương một lần cho kẻ địch khi chạm phải, hỗ trợ cơ chế đâm xuyên (Piercing).
    *   `Hit_Periodic`: Gây sát thương liên tục theo chu kỳ mỗi X giây nếu kẻ địch còn nằm bên trong vùng ảnh hưởng (phù hợp với các vòng lửa, AOE).

*Ngoài ra, hệ thống còn hỗ trợ cơ chế đạn nâng cao thông qua **Data-Driven (Behavior-Based)** với `ProjectileSystem`, `ProjectileController`, và các Behavior ScriptableObject khác (như Straight, Homing, Pierce, Bounce, Explosion, Split) giúp cấu hình đạn linh hoạt ngay trên Inspector.*

---

## 4. Trí Tuệ Nhân Tạo Kẻ Địch (Enemy AI System)
Sử dụng mô hình **Finite State Machine (FSM)** kết hợp **Strategy Pattern** cho phép tạo ra nhiều loại AI với hành vi khác nhau một cách dễ dàng.

*   **Bộ Máy Trạng Thái (FSM)**:
    *   `EnemyStateMachine.cs`: Quản lý và chuyển đổi qua lại giữa các trạng thái của kẻ địch.
    *   `EnemyIdleState.cs`: Trạng thái đứng yên.
    *   `EnemyChaseState.cs`: Trạng thái đuổi theo Player.
    *   `EnemyAttackState.cs`: Trạng thái ra đòn khi nằm trong tầm đánh.
    *   `EnemyRepositionState.cs`: Trạng thái điều chỉnh vị trí để tránh dẫm đạp/kẹt nhau.
    *   `EnemyDeadState.cs`: Trạng thái khi bị tiêu diệt (kích hoạt hiệu ứng, tắt Collider, rớt EXP).
*   **Các Chiến Lược Hành Vi (Strategy Pattern)**:
    *   **Di chuyển**:
        *   `MeleeMovementStrategy`: Tiến thẳng hoặc bao vây áp sát Player để cận chiến.
        *   `RangedMovementStrategy`: Giữ khoảng cách an toàn với Player khi chiến đấu.
    *   **Tấn công**:
        *   `MeleeAttackStrategy`: Tấn công trực tiếp ở cự ly gần.
        *   `RangedAttackStrategy`: Bắn đạn từ xa về phía Player.

---

## 5. Hệ Thống Nâng Cấp & Tiến Hóa (Upgrade & Evolution System)
Hệ thống giúp nhân vật phát triển sức mạnh trong trận đấu, lấy cảm hứng từ thể loại Roguelite (Vampire Survivors). Thiết kế áp dụng **Strategy Pattern** và tính **Đa Hình (Polymorphism)**.

*   **[UpgradeData.cs](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/Upgrades/UpgradeData.cs)** (Abstract): Lớp cơ sở dưới dạng ScriptableObject.
    *   `IsAvailable(GameObject player)`: Quyết định thẻ này có đủ điều kiện xuất hiện hay không.
    *   `ApplyUpgrade(GameObject player)`: Áp dụng các thay đổi nâng cấp lên nhân vật.
*   **Phân Loại Nâng Cấp**:
    *   `CommonUpgradeData`: Nâng cấp các chỉ số bị động của nhân vật (Máu, Tốc chạy...).
    *   `WeaponUpgradeData`: Mở khóa vũ khí mới hoặc nâng cấp vũ khí hiện có (Tối đa 6 cấp độ).
    *   `EvolutionUpgradeData`: Tiến hóa vũ khí lên cấp độ tối thượng khi đạt đủ điều kiện (đạt tối đa cấp độ vũ khí và có thẻ bị động yêu cầu).
*   **Vận hành**: Gồm `UpgradeManager` xử lý bốc thăm ngẫu nhiên (Weighted Random) và `UpgradeUIManager` để hiển thị thẻ lựa chọn cho người chơi.

---

## 6. Các Hệ Thống Phụ Khác
*   **Hệ thống Nhặt đồ (Collectibles)**:
    *   `ExpGem.cs`: Hạt kinh nghiệm rơi ra từ quái vật đã chết. Có cơ chế tự động bay hút về phía Player (Magnet/Vacuum) khi Player đến gần.
*   **Hệ thống Tính sát thương (Shared Damage System)**:
    *   `DamageContext.cs`: Đóng gói thông tin gây sát thương bao gồm: Lượng dame cơ bản, tỉ lệ chí mạng, vị trí va chạm và nguồn gây sát thương.
    *   `DamageUtility.cs`: Chứa các hàm tính toán nhanh lượng sát thương cuối cùng sau khi đã áp dụng các chỉ số giảm trừ/chí mạng.
    *   `RarityColorUtility.cs`: Tiện ích xác định màu sắc hiển thị của các thẻ nâng cấp dựa trên độ hiếm (Common, Rare, Legendary...) trên UI.
