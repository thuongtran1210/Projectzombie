# Hệ Thống Nâng Cấp Trận Đấu (In-Match Upgrades & Mythic Cores System)

Tài liệu này mô tả chi tiết toàn bộ kiến trúc kỹ thuật của hệ thống Nâng Cấp trong trận đấu (**In-Match Upgrades**) cho dự án **Projectzombie** (Unity 2022 - Top-down Survival Roguelite phong cách Cổ Phong Thần Thoại Việt Nam).

---

## 1. Kiến Trúc Cốt Lõi (Core Architecture)

Hệ thống được thiết kế theo 5 tầng phân lập độc lập, triệt tiêu hoàn toàn `GetComponent` trong gameplay loop và bảo đảm **0 GC Allocation** trên thiết bị di động Android 60 FPS:

```mermaid
graph TD
    subgraph TẦNG 1: DỮ LIỆU CẤU HÌNH (ScriptableObjects)
        A[UpgradeData] --> B[MythicCoreUpgradeData]
        A --> C[SynergyTraitUpgradeData]
        A --> D[WeaponUpgradeData]
        A --> E[EvolutionUpgradeData]
    end

    subgraph TẦNG 2: TUYỂN CHỌN & GACHA (Selection & Filter Pipeline)
        F[UpgradeSelector] --> G[ArchetypeExclusionFilter]
        F --> H[DynamicSynergyWeighter]
    end

    subgraph TẦNG 3: COMPOSITION ROOT (PlayerContext)
        I[PlayerContext] --> J[PlayerStats]
        I --> K[HealthSystem]
        I --> L[PlayerCombatEvents]
        I --> M[PlayerMythicManager]
    end

    subgraph TẦNG 4: THỰC THI RUNTIME & LIFECYCLE (Execution Layer)
        M --> N[MythicCoreRuntime]
        N --> O[PhuDongCoreRuntime]
        N --> P[KimQuyCoreRuntime]
        N --> Q[SonThanhCoreRuntime]
        N --> R[ThuyBaCoreRuntime]
        N --> S[LongTienCoreRuntime]
    end

    subgraph TẦNG 5: GIAO DIỆN HIỂN THỊ (MVP Pattern)
        T[UpgradeUIPresenter] --> U[UpgradeCardView]
    end
```

### 1.1. Lớp Dữ Liệu: `UpgradeData` (Abstract ScriptableObject)
Mọi thẻ bài đều kế thừa từ `UpgradeData`. Điểm cải tiến quan trọng: Phương thức kiểm tra và áp dụng chỉ nhận `PlayerContext` làm tham số (không nhận `GameObject` thô):

```csharp
public abstract class UpgradeData : ScriptableObject
{
    public string id;
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;
    public UpgradeType upgradeType;
    public float spawnWeight = 1f;
    public ElementType element = ElementType.None;

    public abstract bool IsAvailable(PlayerContext context);
    public abstract void ApplyUpgrade(PlayerContext context);
    public virtual float GetDynamicWeightMultiplier(PlayerContext context) => 1.0f;
}
```

### 1.2. Hợp Đồng Sự Kiện Chiến Đấu: `CombatEventContracts.cs`
Sử dụng `readonly struct` truyền qua từ khóa `in` để đạt **0 GC Allocation** và đảm bảo tính mở rộng không làm vỡ các hàm đã subscribe:

```csharp
public readonly struct DamageDealtEvent
{
    public readonly GameObject Attacker;
    public readonly GameObject Target;
    public readonly float Damage;
    public readonly Vector2 HitPosition;
    public readonly bool IsCrit;
    public readonly ElementType Element;
}

public readonly struct KillEvent
{
    public readonly GameObject Killer;
    public readonly GameObject Enemy;
    public readonly Vector2 Position;
    public readonly bool IsCrit;
    public readonly ElementType Element;
}

public readonly struct DashEvent
{
    public readonly GameObject Instigator;
    public readonly Vector2 StartPosition;
    public readonly Vector2 EndPosition;
    public readonly Vector2 DashDirection;
}

public readonly struct HealEvent
{
    public readonly GameObject Target;
    public readonly float HealAmount;
    public readonly float CurrentHealth;
    public readonly float MaxHealth;
}

public readonly struct ReviveEvent
{
    public readonly GameObject Player;
    public readonly float HealthPercent;
    public readonly float InvulnerableDuration;
    public readonly string Source;
}
```

---

## 2. Phác Thảo Cây Thẻ 5 Đại Lõi Thần Thoại Việt Nam (Mythic Core Trees)

Trận đấu được xây dựng quanh **5 Đại Lõi Thần Thoại Cổ Phong**, mỗi Lõi sở hữu một nhánh cây kỹ năng gồm **1 Lõi Gốc (Kim Cương) + 2 Thẻ Vàng (Cơ Chế) + 3 Thẻ Bạc (Chỉ Số)**:

### Cây 1: PHÙ ĐỔNG THIÊN UY (Hệ Hỏa - Thể Tu Khổng Lồ, Càn Quét)
```mermaid
graph LR
    PD[💎 PHÙ ĐỔNG THẦN TƯỚNG<br/>Phóng to 200%, Max HP+, Quét chém văng quái]
    PD --> PD_Y1[🥇 Hỏa Ký Đạp Lôi<br/>Dash cưỡi Ngựa Sắt phun lửa & Giật sét]
    PD --> PD_Y2[🥇 Nhổ Tre Đánh Giặc<br/>Combo chém 3 làm gãy giáp & Choáng quái]
    PD --> PD_S1[🥈 Huyết Khí Thần Đồng<br/>Mỗi 100 HP -> +2% Tốc đánh +1% Dame]
    PD --> PD_S2[🥈 Thiết Giáp Bất Phá<br/>Giảm 20% sát thương khi to > 130%]
    PD --> PD_S3[🥈 Phù Đổng Nộ Hống<br/>Bị đánh đau tự gầm đẩy quái & Hồi thể lực]
```

### Cây 2: KIM QUY THẦN CƠ (Hệ Kim - Xạ Kích Vạn Tiễn, Đạn Nảy Xuyên Phá)
```mermaid
graph LR
    KQ[💎 NHẤT TIỄN VẠN TIỄN<br/>+3 Tia đạn, Đạn trúng quái nảy sang 2 mục tiêu]
    KQ --> KQ_Y1[🥇 Linh Quy Hộ Quốc Trận<br/>Đứng yên 1s tạo Mai Rùa chặn & Phản xạ đạn]
    KQ --> KQ_Y2[🥇 Mũi Tên Đồng Cổ Loa<br/>Đạn nảy từ mục tiêu 2 chắc chắn 100% Bạo Kích]
    KQ --> KQ_S1[🥈 Mắt Thần Xuyên Tâm<br/>+40% Tầm bắn, +30% Tốc đạn, +2 Xuyên]
    KQ --> KQ_S2[🥈 Kim Quy Trợ Lực<br/>Mỗi 10% Crit -> Chuyển thành +15% Tốc chạy]
    KQ --> KQ_S3[🥈 Cơ Quan Tốc Xạ<br/>Bắn trúng 5 hit liên tiếp -> +50% Tốc bắn 3s]
```

### Cây 3: TẢN VIÊN SƠN THÁNH (Hệ Thổ - Bất Tử Địa Trận, Đè Bẹp Quái)
```mermaid
graph LR
    ST[💎 BẠT SƠN DỜI LŨY<br/>Giáp Đá 100% HP, Đứng yên mọc 4 Thạch Trụ đè quái]
    ST --> ST_Y1[🥇 Chấn Địa Nham Thạch<br/>Dash / Nhận dame tạo Động Đất hất tung quái]
    ST --> ST_Y2[🥇 Thần Thổ Dưỡng Khí<br/>Thạch Trụ mọc/vỡ hồi 5% Max HP cho Player]
    ST --> ST_S1[🥈 Kim Cương Nham Bì<br/>Giảm cố định 20 sát thương từ đòn đánh quái]
    ST --> ST_S2[🥈 Địa Chấn Phản Phách<br/>Phản 50% sát thương quái đánh theo hình nón]
    ST --> ST_S3[🥈 Sơn Thần Uy Áp<br/>Quái đứng gần 6m bị giảm 40% Tốc chạy]
```

### Cây 4: THỦY BÁ CUỒNG NỘ (Hệ Thủy - Sóng Thần Cuốn Trôi, Băng Tê Liệt)
```mermaid
graph LR
    TT[💎 HÔ PHONG HOÁN VŨ<br/>Mưa bão toàn map, Quái bị Ẩm Ướt, 6s Sóng Thần gom quái]
    TT --> TT_Y1[🥇 Băng Phong Vạn Lý<br/>Đánh quái Ẩm Ướt có 30% Đóng Băng & Nổ 6 mảnh băng]
    TT --> TT_Y2[🥇 Thủy Long Cuộn Trào<br/>Dash hóa Rồng Nước bất tử & Hút quái theo đường lướt]
    TT --> TT_S1[🥈 Thủy Triều Dâng Cao<br/>+30% Tốc chạy & +20% Tầm nhặt đồ trong trời mưa]
    TT --> TT_S2[🥈 Hàn Khí Thấu Xương<br/>Quái bị Đóng Băng nhận thêm +40% Sát thương]
    TT --> TT_S3[🥈 Thủy Lưu Hồi Chuyển<br/>Đóng Băng quái giảm 0.2s hồi chiêu Lướt]
```

### Cây 5: LONG TIÊN HUYẾT MẠCH (Hệ Âm Dương - Chuyển Đổi Rồng/Tiên, Miễn Tử)
```mermaid
graph LR
    LT[💎 THÁI CỰC LONG TIÊN BIẾN<br/>Chém liên tục hóa Rồng Dame+, Thả tay hóa Tiên Hồi Máu]
    LT --> LT_Y1[🥇 Bách Noãn Hộ Thể<br/>1 Mạng Hồi Sinh Miễn Phí + 3 Trứng hộ thể phát nổ]
    LT --> LT_Y2[🥇 Âm Dương Giao Hòa<br/>Chuyển dạng Rồng/Tiên phóng Sóng Thái Cực xóa đạn]
    LT --> LT_S1[🥈 Hồng Bàng Khí Vận<br/>+35% EXP và +35% Vàng rơi ra từ quái vật]
    LT --> LT_S2[🥈 Long Uy Phấn Chấn<br/>Dạng Rồng tăng thêm +50% Tốc độ đánh]
    LT --> LT_S3[🥈 Tiên Âm Dưỡng Hồn<br/>Dạng Tiên tăng +50% Bán kính hào quang & +25% Giáp]
```

---

## 3. Quy Hoạch Kho Thẻ Toàn Diện (60 Thẻ)

| Nhóm Thẻ | Số Lượng | Cơ Chế Xuất Hiện |
|---|:---:|---|
| **Đại Lõi Thần Thoại (Prismatic Core)** | **5 Thẻ** | Chỉ xuất hiện tại **Level 1** (Khởi đầu trận). |
| **Thẻ Nhánh Độc Quyền (Synergy Traits)** | **25 Thẻ** | Mỗi Lõi có 5 thẻ con. Tự động tăng +50% tỉ lệ ra khi mang đúng Lõi. |
| **Thẻ Bổ Trợ Dùng Chung (Universal Passives)** | **15 Thẻ** | Xuất hiện tự do cho mọi Lõi (Máu, Giáp, Tốc chạy, Crit, Exp, Nam châm...). |
| **Thẻ Nâng Cấp & Tiến Hóa Vũ Khí** | **13 Thẻ** | Nâng cấp vũ khí (`W001-W012`) và mở khóa Thần Binh Tối Thượng (`E001-E012`). |
| **Thẻ Cứu Cánh (Fallback Rewards)** | **2 Thẻ** | Tiên Đan Hồi Máu (40% HP) & Túi Vàng (+150 Vàng) khi cạn pool thẻ. |
| **TỔNG CỘNG** | **60 THẺ** | **Đáp ứng chuẩn quy mô Roguelite Mobile 60 FPS.** |

---

## 4. Dòng Thời Gian Trận Đấu & Cơ Chế Tuyển Chọn (Game Flow)

```
[Bắt đầu ván] ──► [MỐC 1: NHẬP ĐẠO (Level 1)]       ──► Bốc 1 trong 3 ĐẠI LÕI KIM CƯƠNG
                        │
                        ▼ (Các Level 2, 3, 4, 5: Bốc Thẻ Bạc/Vàng bổ trợ cho Lõi)
[Giữa trận]    ──► [MỐC 2: CƯỜNG HÓA (Level 6)]      ──► Bốc 1 trong 3 LÕI VÀNG / HYBRID
                        │
                        ▼ (Các Level 7-11: Nâng cấp vũ khí & Tiến hóa Thần Binh E001-E012)
[Cuối trận]    ──► [MỐC 3: ĐỘT PHÁ TỐI THƯỢNG (Lv 12)] ──► Mở khóa Tuyệt Kỹ Thần Minh trước Boss
```

### Cơ chế Tăng Trọng Số Cộng Hưởng (Dynamic Synergy Weight):
*   Khi người chơi chọn Lõi **A**, `UpgradeSelector` tự động tăng **+50% trọng số xuất hiện** cho 5 thẻ nhánh của Lõi **A** và các thẻ cùng Hệ Ngũ Hành.
*   Bộ lọc `ArchetypeExclusionFilter` tự động chặn hoàn toàn các Đại Lõi khác xuất hiện ở các level sau.

---

## 5. Giải Quyết Xung Đột & Quản Lý Vòng Đời (Lifecycle & Conflict Resolution)

### 5.1. Xử Lý Xung Đột Chỉ Số (Layered Stat Pipeline)
Chỉ số nhân vật được tính toán qua 3 tầng rõ ràng, tránh xung đột giữa thẻ cộng % và thẻ khóa giá trị (như Khóa 1 HP):
$$\text{FinalStat} = (\text{Base} + \sum \text{FlatBonus}) \times (1 + \sum \text{PercentBonus})$$
*(Nếu có modifier dạng `Override/Clamp`, giá trị Override sẽ có quyền ưu tiên cao nhất).*

### 5.2. Quản Lý Vòng Đời Runtimes (Lifecycle Contract)
`MythicCoreRuntime` tuân thủ vòng đời nghiêm ngặt với cờ bảo vệ `_isDisposed`:
*   **Initialize:** Đăng ký lắng nghe `CombatEvents`, tạo Aura VFX.
*   **Teardown (Idempotent):** Hủy đăng ký tất cả Event, thu hồi VFX, hoàn trả chỉ số.
*   **4 Kịch Bản Hủy:**
    1. *Core Replaced:* `PlayerMythicManager` gọi `Teardown()` trước khi `Instantiate` Core mới.
    2. *Player Destroyed:* `OnDestroy()` tự động kích hoạt `Teardown()`.
    3. *Run Restarted:* `ResetState()` giải phóng toàn bộ Runtime và reset về `None`.
    4. *Scene Changed:* Unity hủy scene, `_isDisposed` ngăn chặn `NullReferenceException`.

### 5.3. Cơ Chế Soft Death & Hồi Sinh (Revive Flow)
*   Khi Máu về 0, Player **KHÔNG BỊ DESTROY** mà chuyển sang trạng thái **Hấp Hối (Incapacitated)** trong 5 giây.
*   Nếu hồi sinh (qua Ads, Linh Đan, hoặc Nội tại Miễn Tử của Long Tiên):
    *   Phát sự kiện `PlayerCombatEvents.PublishPlayerRevived(new ReviveEvent(...))`.
    *   `MythicCoreRuntime` bắt sự kiện và kích hoạt hiệu ứng Thần Thoại (Phù Đổng giáng sét dọn map, Sơn Tinh hồi giáp đá, v.v.).
    *   Player tiếp tục chơi mượt mà, không tốn chi phí nạp lại dữ liệu.
*   Nếu từ chối hồi sinh / hết giờ -> Kích hoạt **Hard Death (GameOver)** và dọn dẹp bộ nhớ sạch sẽ.

---

## 6. Hướng Dẫn Mở Rộng Thẻ Mới (Extensibility Guide)

### Cách tạo thêm 1 Lõi Thần Thoại mới:
1. **Bước 1:** Khai báo Archetype mới trong `MythicArchetype` enum.
2. **Bước 2:** Tạo Runtime Controller kế thừa từ `MythicCoreRuntime` (ví dụ: `LieuHanhCoreRuntime.cs`), override `SubscribeCombatEvents()` và `UnsubscribeCombatEvents()`.
3. **Bước 3:** Tạo ScriptableObject `MythicCoreUpgradeData`, gán Prefab chứa Runtime vừa tạo.
4. **Bước 4:** Xong! Hệ thống Gacha và Filter sẽ tự động nhận diện và vận hành mà không cần sửa bất kỳ file quản lý nào.
