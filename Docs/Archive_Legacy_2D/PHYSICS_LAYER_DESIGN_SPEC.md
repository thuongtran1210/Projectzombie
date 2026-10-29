# Projectzombie — Physics 2D Layer & Collision Matrix Design Spec

Tài liệu này quy định cấu trúc phân chia **Physics 2D Layers**, ma trận va chạm (**Collision Matrix**) và các quy tắc tối ưu hiệu năng vật lý cho trò chơi **Projectzombie** (Top-down Survival Roguelite trên Android Mobile).

---

## 1. Danh Sách Các Physics 2D Layers (Layer Specification)

Hệ thống vật lý 2D của dự án chia làm **8 Layer chính** với trách nhiệm rõ ràng:

| Layer ID | Tên Layer (Layer Name) | Đối Tượng Áp Dụng (Target Objects) | Mô Tả & Trách Nhiệm Vật Lý |
|---|---|---|---|
| `0` | **Default** | Các GameObject trang trí, Sprite background | Không tham gia tính toán va chạm gameplay chính. |
| `3` | **Obstacle** | Tilemap cản đường, đá, cây lớn, tường rào | Cản bước di chuyển của Player và Quái vật. |
| `6` | **Player** | Nhân vật Người chơi (Player Prefab) | Nhận va chạm từ Quái, Đạn quái, và hút Vật phẩm. |
| `7` | **Enemy** | Tất cả Yêu Ma (Ma Giáp, Ma Trơi, Quỷ Nhập Tràng...) | Nhận sát thương từ Đạn Player. **Tắt va chạm giữa quái với quái**. |
| `8` | **PlayerProjectile** | Tất cả đạn/kỹ năng do Player bắn ra | Chỉ quét va chạm với `Enemy` và `Obstacle`. |
| `9` | **EnemyProjectile** | Đạn/bẫy do Boss hoặc Quái tầm xa bắn ra | Chỉ quét va chạm với `Player` và `Obstacle`. |
| `10` | **Pickup** | Hạt EXP Gem, Cổ Tiền, Rương, Bình máu | Chỉ quét va chạm với `Player` (hoặc vùng hút Magnet). |
| `11` | **PlayerHitbox** | Vòng bùa bảo vệ, Bát Quái Trận, Bùa Trấn Yêu (`W003`) | Nhận diện vùng bảo vệ/tác động riêng biệt xung quanh Player. |

---

## 2. Ma Trận Va Chạm (Physics 2D Collision Matrix)

Để đảm bảo hiệu năng 60 FPS trên thiết bị di động Android khi có **200+ quái vật** trên màn hình, Ma trận va chạm trong **Project Settings ➔ Physics 2D** phải được thiết lập nghiêm ngặt:

```text
                     [Default] [Obstacle] [Player] [Enemy] [PlayerProj] [EnemyProj] [Pickup] [PlayerHitbox]
Default                 —          —         —        —         —           —          —          —
Obstacle                —          ✅        ✅       ✅        ✅          ✅         —          —
Player                  —          ✅        —        ✅        ❌          ✅         ✅         —
Enemy                   —          ✅        ✅       ❌ (Tắt)   ✅          ❌         ❌         —
PlayerProjectile        —          ✅        ❌       ✅        ❌ (Tắt)    ❌ (Tắt)   ❌         —
EnemyProjectile         —          ✅        ✅       ❌        ❌ (Tắt)    ❌ (Tắt)   ❌         —
Pickup                  —          —         ✅       ❌        ❌          ❌         —          —
PlayerHitbox            —          —         —        ✅        ❌          ✅         —          —
```

> [!IMPORTANT]
> **2 Quy Tắc Vàng Tối Ưu Hiệu Năng:**
> 1. **Tắt `Enemy` ↔ `Enemy` (Quái không va chạm vật lý lẫn nhau):** Triệt tiêu $O(N^2)$ phép tính Rigidbody physics khi 200 quái bu quanh Player.
> 2. **Tắt `PlayerProjectile` ↔ `PlayerProjectile` / `EnemyProjectile`:** Loại bỏ hoàn toàn va chạm giữa hàng trăm viên đạn trên màn hình.

---

## 3. Mô Tả Chi Tiết & Cấu Hình Thành Phần (Component Setup)

### 3.1. Layer `Player` (Layer 6)
- **Components:** `Rigidbody2D` (Dynamic, Freeze Rotation Z), `CircleCollider2D` / `CapsuleCollider2D`, `PlayerController`.
- **Tương tác:** Cho phép va chạm với `Obstacle`, `Enemy`, `EnemyProjectile`, `Pickup`.

### 3.2. Layer `Enemy` (Layer 7)
- **Components:** `Rigidbody2D` (Dynamic, Freeze Rotation Z), `Collider2D` (Trigger hoặc Solid), `HealthSystem`, `Enemy`.
- **Tương tác:** Cho phép va chạm với `Obstacle`, `Player`, `PlayerProjectile`.

### 3.3. Layer `PlayerProjectile` (Layer 8)
- **Components:** `Rigidbody2D` (Kinematic / Dynamic 0 Gravity), `CircleCollider2D` / `BoxCollider2D` (Is Trigger = True), `ProjectileController`.
- **Cấu hình LayerMask trong C# / SO:**
  ```csharp
  // HitLayer trong ProjectileData ScriptableObject chỉ tick: Enemy (Layer 7) | Obstacle (Layer 3)
  public LayerMask HitLayer; 
  ```

### 3.4. Layer `Pickup` (Layer 10)
- **Components:** `CircleCollider2D` (Is Trigger = True), `EXPGem`.
- **Tương tác:** Chỉ nhận Trigger với `Player`.

---

## 4. Hướng Dẫn Thiết Lập Trực Tiếp Trong Unity Editor

1. Mở Unity Editor, chọn menu **Edit ➔ Project Settings ➔ Tags and Layers**.
2. Tại mục **User Layers**, nhập đúng tên Layer vào các ô tương ứng:
   - `Layer 3`: `Obstacle`
   - `Layer 6`: `Player`
   - `Layer 7`: `Enemy`
   - `Layer 8`: `PlayerProjectile`
   - `Layer 9`: `EnemyProjectile`
   - `Layer 10`: `Pickup`
   - `Layer 11`: `PlayerHitbox`
3. Chuyển sang mục **Physics 2D**:
   - Cuộn xuống phần **Layer Collision Matrix**.
   - Bỏ tick (Uncheck) toàn bộ các ô giao nhau không được phép va chạm theo đúng bảng Ma Trận ở **Mục 2**.
4. Lưu Project (**Ctrl + S**).

---

## 5. Quy Tắc Viết Code C# Tuân Thủ Layer Mask

When writing C# collision and overlap detection logic, always reference layers via masks or bitwise operations to avoid magic numbers:

```csharp
// ✅ ĐÚNG — Sử dụng LayerMask được serialize từ Inspector
[SerializeField] private LayerMask enemyLayer;

private void DetectEnemies()
{
    int count = Physics2D.OverlapCircleNonAlloc(transform.position, radius, _hitBuffer, enemyLayer);
}

// ✅ ĐÚNG — Kiểm tra Tag chuẩn xác
if (other.CompareTag("Enemy")) { ... }
```
