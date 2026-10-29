# 📐 BẢNG QUY CHUẨN KÍCH THƯỚC NHÂN VẬT & VFX (CHARACTER_VFX_SIZE_GUIDELINES.md)

Tài liệu này đóng vai trò là **Bộ Tiêu Chuẩn Hình Học (Geometric & Metric Specifications)** tuyệt đối của **ProjectZombie (Vong Xuyên)**, đảm bảo tính đồng bộ hoàn hảo giữa kích thước cơ thể nhân vật Chibi, quái vật, đòn đánh và các hiệu ứng kỹ năng (VFX/Hitbox).

---

## 🧍 1. QUY CHUẨN KÍCH THƯỚC NHÂN VẬT & QUÁI VẬT (METRIC SYSTEM)

Trong Unity 2D Top-down (Với quy ước chuẩn **$1\text{ Unit} = 1\text{ Mét} = 32\text{ PPU / Pixels per Unit}$**):

```
┌───────────────────────────────────────────────────────────────────────────┐
│                                                                           │
│   ● Chiều cao Tướng (Hero Height): 1.0m - 1.1m (32 - 36 pixel)            │
│   ● Bề rộng vai/thân (Body Width): 0.6m - 0.7m (20 - 22 pixel)            │
│   ● Tỷ lệ cơ thể (Anatomy Ratio): Siêu Chibi 1:1.2 đến 1:1.5              │
│     (Đầu chiếm 60% chiều cao, Thân chiếm 25%, Chân ngắn 15%)             │
│   ● Collider va chạm (Capsule/Circle): Bán kính r = 0.35m, Cao h = 0.8m   │
│                                                                           │
└───────────────────────────────────────────────────────────────────────────┘
```

### 📊 Bảng phân cấp kích thước thực thể trong Game:

| Thực Thể | Chiều Cao Unity | Bán Kính Hitbox | Tỉ Lệ So Với Hero | Ghi Chú Silhouette |
| :--- | :---: | :---: | :---: | :--- |
| **Hero (Tướng - An Sĩ)** | **`1.05m`** | **`0.35m`** | **$100\%$ (Chuẩn Gốc)** | Khối hộp chữ nhật bo góc (Squircle Chibi). |
| **Quái Nhỏ (Ma Trơi, Hoa Ly Tinh)** | `0.65m` | `0.25m` | $60\% - 70\%$ | Quái bay, lướt nhanh, kích thước nhỏ. |
| **Quái Thường (Ma Giáp, Ma Đa)** | `1.0m - 1.1m` | `0.35m - 0.4m` | $95\% - 105\%$ | Tương đương vóc dáng Hero. |
| **Quái Elite (Ma Đói No, Quỷ Nhập Tràng)** | `1.4m - 1.6m` | `0.55m - 0.65m` | $140\% - 150\%$ | Thân hình to béo, khối lượng lớn. |
| **Mini-Boss (Ngưu Đầu Mã Diện)** | `2.0m - 2.2m` | `0.8m - 0.9m` | $200\%$ | Cao gấp đôi Tướng, đòn đánh diện rộng. |
| **Final Boss (Diêm Vương)** | `2.8m - 3.2m` | `1.2m - 1.4m` | $280\% - 300\%$ | Trùm tối thượng bao quát góc màn hình. |

---

## 🔮 2. QUY CHUẨN TỈ LỆ KÍCH THƯỚC HIỆU ỨNG KỸ NĂNG (VFX SCALING RULES)

Để tránh tình trạng hiệu ứng quá to (che khuất tầm nhìn) hoặc quá nhỏ (thiếu lực Impact), toàn bộ VFX trong game được phân thành **4 cấp độ kích thước chuẩn**:

```
           [CẤP 1: MELEE AURA]       [CẤP 2: AOE CLOSE]      [CẤP 3: ULTIMATE ZONE]
                Ø 1.2m - 1.8m            Ø 2.5m - 3.5m             Ø 6.0m - 8.0m
               (Bao quanh Tướng)         (Cận - Trung bình)         (Cả màn hình)
```

| Phân Cấp VFX | Đường Kính Hiển Thị | Bán Kính Quét Sát Thương (`Radius`) | Áp Dụng Cho Các Kỹ Năng / Pháp Bảo |
| :--- | :---: | :---: | :--- |
| **Cấp 1: Melee / Self Aura** | **`1.2m - 1.8m`** | `r = 0.8m - 1.0m` | • **Lốc Dép Vạn Năng (`W_SLIPPER Whirlwind`):** Scale $0.48$, $r = 1.8m$.<br>• **Vết chém tay thường:** Scale $1.0$, $r = 1.2m$. |
| **Cấp 2: Close AOE & Traps** | **`2.5m - 3.5m`** | `r = 1.5m - 2.0m` | • **Bát Quái Trận:** $r = 2.2m$.<br>• **Chiếu Trải Hoàng Tuyền:** $3.5m \times 2.2m$.<br>• **Chổi Lông Gà:** Bán kính nện $r = 3.5m$. |
| **Cấp 3: Long Line & Walls** | **`4.0m - 6.5m`** | `r = 2.0m - 3.0m` | • **Tường Nước Thánh / Bão Khói:** Dài $4.5m - 6.0m$, dày $0.6m$.<br>• **Quỹ đạo Dép Boomerang:** Tầm ném $4.5m - 6.5m$. |
| **Cấp 4: Screen Ults / Bombs** | **`7.0m - 8.5m`** | `r = 3.5m - 4.5m` | • **Nồi Cơm Thạch Sanh:** Gom quái $6.0m$.<br>• **Lựu Đạn Thần Sa:** Bão lửa $8.5m$.<br>• **Nỏ Thần Vạn Tiễn:** Mũi tên bay xa $14.0m$. |

---

## 🎯 3. BẢN ĐỒ VỊ TRÍ TRỤC XOAY & ĐỘ SÂU LAYER (PIVOT & SORTING ORDER)

1. **Pivot Nhân Vật:** Bắt buộc đặt tại **`Bottom-Center (x: 0.5, y: 0.0)`** (Gót chân chạm đất) để tính Y-Sorting chiều sâu 2.5D chính xác.
2. **Sorting Order Chuẩn Cho Vũ Khí & VFX:**
   - `SortingLayer = "Default" / "Background"` $\rightarrow$ Sàn gạch / Decal bẫy chiếu: `Order = -10 .. 0`.
   - `SortingLayer = "Skill"` $\rightarrow$ Vòng trận địa / Bão khói / Đạn bay dưới chân: `Order = 1 .. 6`.
   - `SortingLayer = "Characters"` $\rightarrow$ Tướng & Yêu quái: `Order = Dynamic (theo trục -Y)`.
   - `SortingLayer = "Skill"` $\rightarrow$ Vệt chém / Sóng va chạm / Lốc xoáy bao thân: `Order = 10 .. 15`.
   - `SortingLayer = "UI"` $\rightarrow$ Thanh máu HUD / Nút chiêu: `Order = 100+`.
