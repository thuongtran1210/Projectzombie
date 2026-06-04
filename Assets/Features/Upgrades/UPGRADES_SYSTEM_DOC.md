# Hệ Thống Nâng Cấp (Upgrades System) - Tài liệu cho Developer

Tài liệu này mô tả chi tiết kiến trúc kỹ thuật của hệ thống Nâng Cấp (Upgrades) trong **ProjectZombie**, sau khi đã được refactor sang mô hình Đa Hình (Polymorphism) và Strategy Pattern.

---

## 1. Kiến trúc Cốt Lõi (Core Architecture)

Hệ thống Nâng cấp không còn phụ thuộc vào một "God Object" (đối tượng chứa mọi thứ) hay các lệnh `switch-case` lồng nhau. Thay vào đó, nó tận dụng **Kế thừa (Inheritance)** từ lớp trừu tượng `UpgradeData`.

### Lớp gốc: `UpgradeData` (Abstract Class)
Tất cả các thẻ nâng cấp trong game đều kế thừa từ lớp này (`ScriptableObject`).
Lớp này định nghĩa 2 phương thức ảo (abstract methods) cực kỳ quan trọng:
1. `bool IsAvailable(GameObject player)`: Mỗi loại thẻ tự quyết định xem nó có được phép xuất hiện trong danh sách bốc thăm (Gacha) của người chơi hay không.
2. `void ApplyUpgrade(GameObject player)`: Mỗi loại thẻ tự định nghĩa cách nó sẽ tác động lên người chơi (cộng máu, cộng dame, thay đổi vũ khí...) khi được chọn.

---

## 2. Các Lớp Nâng Cấp Tích Hợp Sẵn (Built-in Subclasses)

Hiện tại hệ thống có 3 loại thẻ Nâng cấp chính:

### 2.1. `CommonUpgradeData`
*   **Mục đích**: Tăng các chỉ số bị động (Passive) cho bản thân nhân vật (Máu, Tốc độ chạy, Kinh nghiệm...).
*   **IsAvailable**: Luôn trả về `true` (Thẻ luôn có thể xuất hiện, có thể update sau nếu muốn giới hạn max level).
*   **ApplyUpgrade**: Lấy component `PlayerStats` từ player và gọi các hàm `AddMaxHealth()`, `AddMoveSpeed()`... Đồng thời ghi nhận vào `PlayerPassives`.

### 2.2. `WeaponUpgradeData`
*   **Mục đích**: Tăng sức mạnh cho một loại vũ khí cụ thể, hoặc mở khóa vũ khí mới.
*   **Các biến quan trọng**: `weaponId`, `requiredCurrentLevel` (Cấp độ vũ khí hiện tại, 0 = mở khóa), `statModifier`, `overrideProjectilePrefab`.
*   **IsAvailable**: Query vào `WeaponManager`. Trả về `true` nếu player ĐANG HẾT loại vũ khí đó (nếu thẻ mở khóa), hoặc ĐÃ CÓ và cấp độ khớp với `requiredCurrentLevel`.
*   **ApplyUpgrade**: Lấy `WeaponBase` thông qua `GetWeaponById()` và gọi hàm `ApplyStatModifier()`.

### 2.3. `EvolutionUpgradeData`
*   **Mục đích**: Tiến hóa một vũ khí lên dạng tối thượng.
*   **Các biến quan trọng**: `weaponId`, `requiredPassiveId`, `requiredCurrentLevel` (mặc định = 6).
*   **IsAvailable**: Kiểm tra xem vũ khí hiện tại đã max cấp chưa và người chơi có sở hữu thẻ bị động `requiredPassiveId` hay không.
*   **ApplyUpgrade**: Hủy vũ khí cũ bằng `WeaponManager.RemoveWeapon()`, và Instantiate `weaponPrefab` mới rồi gắn vào `WeaponManager`.

---

## 3. Quy Trình Vận Hành (Workflow)

1. **Khi Lên Cấp (Level Up):** `PlayerExperience` kích hoạt sự kiện `OnLevelUp`.
2. **Hiển Thị UI:** `UpgradeUIManager` bắt sự kiện và gọi `UpgradeManager.Instance.GetRandomUpgrades(count, playerGameObject)`.
3. **Lọc Thẻ (Filtering):** `UpgradeManager` dùng hàm `Where(u => u.IsAvailable(player))` để vứt bỏ các thẻ không đủ điều kiện (Ví dụ: Thẻ nâng cấp súng lục cấp 3 sẽ không hiện ra nếu súng lục đang ở cấp 1).
4. **Bốc Thăm (Weighted Random):** Dựa vào chỉ số `spawnWeight`, hệ thống quay Gacha lấy ra `count` thẻ.
5. **Kích Hoạt (Execution):** Khi người chơi click vào UI Card, sự kiện `OnUpgradeSelected` gọi trực tiếp vào thẻ: `selectedUpgrade.ApplyUpgrade(playerGameObject)`. 

---

## 4. Hướng Dẫn Mở Rộng: Cách Thêm 1 Loại Thẻ Mới

Với kiến trúc này, việc thêm tính năng mới cực kỳ an toàn vì nó tuân thủ **Open/Closed Principle**. Bạn **KHÔNG CẦN** đụng vào `WeaponManager` hay `UpgradeManager`.

**Ví dụ: Muốn làm thẻ Hào Quang (Aura) rớt thiên thạch xuống quái**

**Bước 1:** Tạo file Script mới `AuraUpgradeData.cs`.
**Bước 2:** Kế thừa từ `UpgradeData`.
```csharp
[CreateAssetMenu(fileName = "NewAuraUpgrade", menuName = "ProjectZombie/Upgrades/Aura Upgrade")]
public class AuraUpgradeData : UpgradeData 
{
    public float auraRadius = 5f;
    public float meteorDamage = 100f;

    public override bool IsAvailable(GameObject player) 
    {
        return !player.GetComponent<MeteorAuraLogic>(); // Chỉ xuất hiện nếu chưa có Hào quang này
    }

    public override void ApplyUpgrade(GameObject player) 
    {
        // Gắn một Component mới vào Player để xử lý logic rớt thiên thạch
        var aura = player.AddComponent<MeteorAuraLogic>();
        aura.Setup(auraRadius, meteorDamage);
    }
}
```
**Bước 3:** Tạo Scriptable Object bằng cách chuột phải trên Unity `Create -> ProjectZombie -> Upgrades -> Aura Upgrade`.
Xong! Hệ thống Gacha sẽ tự động bốc trúng thẻ của bạn và thực thi code mà không gây ra bug cho các thẻ cũ.

---

## 5. Tools & Tiện ích (Editor Tools)

- Do sử dụng Đa Hình, **Inspector của Unity sẽ tự động ẩn/hiện biến rất mượt**. Đừng viết Custom Editor UI nếu không thực sự cần thiết.
- Nếu cần sinh data hàng loạt cho test, hãy sử dụng đoạn script ở `Assets/Features/Upgrades/Editor/UpgradeGeneratorTool.cs`. Tool này đã được thiết kế sẵn để Instantiate đúng các Subclass (`WeaponUpgradeData`, v.v...).
