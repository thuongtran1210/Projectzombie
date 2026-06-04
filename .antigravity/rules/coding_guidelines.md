# Projectzombie - Coding Guidelines & Rules

Tài liệu này quy định các chuẩn mực viết code cho dự án **Projectzombie** (Phiên bản: **Unity 2022** + TikTok Live Interactive Game). Việc tuân thủ các quy tắc này đảm bảo source code luôn sạch, dễ đọc, dễ bảo trì và mở rộng trong tương lai.

---

## 1. Quy Tắc Đặt Tên (Naming Conventions)

Thống nhất cách đặt tên giúp code có tính nhất quán cao, người mới vào dự án có thể dễ dàng nắm bắt.

*   **Classes, Structs, Enums, Methods:** Sử dụng `PascalCase`.
    *   *Ví dụ:* `EnemySpawner`, `CalculateDamage()`, `GameState`.
*   **Interfaces:** Bắt đầu bằng chữ `I` và tiếp theo là `PascalCase`.
    *   *Ví dụ:* `ICommandDispatcher`, `IDamageable`.
*   **Biến Public, Tham số (Parameters), Biến cục bộ (Local variables):** Sử dụng `camelCase`.
    *   *Ví dụ:* `enemySpeed`, `giftId`, `spawnCount`.
*   **Biến Private, Protected (Fields):** Bắt đầu bằng dấu gạch dưới `_` và `camelCase`.
    *   *Ví dụ:* `_realWebSocketClient`, `_commandDispatcher`.
*   **Hằng số (Constants), Static Readonly:** Sử dụng `UPPER_SNAKE_CASE`.
    *   *Ví dụ:* `MAX_ZOMBIE_COUNT`, `DEFAULT_SPAWN_RATE`.

> [!TIP]
> Tên Class/Script phải phản ánh đúng trách nhiệm duy nhất của nó. Tránh những tên quá chung chung như `Manager`, `Controller` nếu có thể dùng từ mô tả chính xác hơn (vd: `TikTokMessageParser`).

---

## 2. Kiến Trúc Cấu Trúc Thư Mục (Folder Structure)

Khuyến khích cấu trúc theo **Tính năng (Feature-based)** thay vì theo Loại tệp (Type-based). Điều này giúp module hóa dự án tốt hơn.

**Nên:**
```text
Assets/
├── Core/               # Chứa bootstrapper, logic cốt lõi chạy xuyên suốt game
├── TikTokBridge/       # Đóng gói toàn bộ logic kết nối mạng, parse tin nhắn TikTok
├── Features/           # Các tính năng cụ thể trong game
│   ├── Zombies/        # Chứa EnemySpawner.cs, Zombie.prefab, ZombieMaterial.mat
│   ├── Weapons/
│   └── UI/
└── ThirdParty/         # Plugins, SDKs tải từ bên ngoài
```

---

## 3. Nguyên Tắc Thiết Kế (Design Principles)

### 3.1. Single Responsibility Principle (Đơn Trách Nhiệm)
Mỗi class chỉ làm **MỘT** việc duy nhất.
*   *Đúng:* `WebSocketClient` chỉ lo kết nối mạng, `ZombieMovement` chỉ lo di chuyển.
*   *Sai:* `GameManager` vừa lo kết nối mạng, vừa parse JSON, vừa spawn quái vật.

### 3.2. Dependency Inversion (Đảo Ngược Phụ Thuộc)
Các module cấp cao (Game Logic) không nên phụ thuộc vào module cấp thấp (Network/UI). Giao tiếp qua **Interfaces**.
*   Ví dụ: `EnemySpawner` không cần biết dữ liệu đến từ WebSocket hay tool giả lập. Nó chỉ nhận dữ liệu thông qua interface `ICommandDispatcher`.

### 3.3. Hạn Chế Singleton
Tránh lạm dụng Singleton (`public static GameManager Instance`). Nó tạo ra *tight coupling* (liên kết cứng) rất khó debug và mở rộng.
*   **Giải pháp:** Sử dụng **Dependency Injection (Tiêm phụ thuộc)**. Khởi tạo các object cần thiết ở `Bootstrapper` và truyền chúng (Inject) vào các class cần dùng thông qua Constructor hoặc hàm `Construct()`.

---

## 4. Giao Tiếp Giữa Các Hệ Thống (Event-Driven)

Với game tương tác trực tiếp (luồng dữ liệu bất định), sử dụng **Event-Driven Architecture (Kiến trúc hướng sự kiện)** thay vì kiểm tra liên tục trong `Update()`.

*   Sử dụng `System.Action` hoặc `UnityEvent` để phát tín hiệu.
*   *Ví dụ:* Khi `CommandDispatcher` nhận được lệnh, nó sẽ phát ra sự kiện `OnGiftReceived?.Invoke(giftData)`. `EnemySpawner` sẽ lắng nghe (subscribe) sự kiện này để spawn quái vật.

```csharp
// NGUỒN PHÁT (Publisher)
public class CommandDispatcher : ICommandDispatcher 
{
    public event Action<GiftData> OnGiftReceived;

    public void ProcessRawJson(string json) 
    {
        // ... parse json ...
        OnGiftReceived?.Invoke(parsedGiftData);
    }
}

// NGƯỜI NGHE (Subscriber)
public class EnemySpawner : MonoBehaviour 
{
    public void Construct(ICommandDispatcher dispatcher) 
    {
        dispatcher.OnGiftReceived += SpawnEnemy;
    }

    private void SpawnEnemy(GiftData data) 
    {
        // Logic spawn
    }
}
```

### 3.4. Giao Tiếp Qua Interface Cho Các Thực Thể Khác Nhau (Decoupling)
Khi xây dựng hệ thống dùng chung (ví dụ: Hệ thống vũ khí, sát thương), tuyệt đối không gán cứng (hardcode) với một đối tượng cụ thể (ví dụ: `PlayerStats`). 
*   **Giải pháp:** Tạo một Interface chung (ví dụ: `ICharacterStats`) và cho tất cả các đối tượng (Player, Enemy, Boss) kế thừa Interface này. Hệ thống dùng chung chỉ gọi Interface, đảm bảo tính tái sử dụng cao.

---

## 5. Tối Ưu & Tránh Hard-code

*   **Không Magic Numbers/Strings:** Tránh việc hard-code chuỗi hoặc số vào giữa logic (`if (giftName == "rose")`). Hãy gom chúng vào `public const string` hoặc Enum.
*   **Sử Dụng ScriptableObject:** Mọi dữ liệu cấu hình như chỉ số máu, sát thương, tốc độ quái vật... phải được lưu trong ScriptableObjects. Điều này giúp Game Designer điều chỉnh dễ dàng mà không phải mở file C# lên sửa.

---

## 6. Tối Ưu Hiệu Năng (Unity Performance & Optimization)

Đặc thù của game sinh tồn chống wave (Vampire Survivors) là số lượng object trên màn hình (quái vật, đạn, text sát thương) rất lớn. Nếu không tối ưu ngay từ đầu, game sẽ bị sụt giảm FPS và rác bộ nhớ (GC Spikes).

*   **Bắt buộc dùng Object Pooling:** Tuyệt đối không gọi `Instantiate()` và `Destroy()` liên tục trong quá trình chơi (Gameplay). Mọi viên đạn, quái vật, VFX đều phải lấy từ Pool và trả về Pool (Sử dụng `UnityEngine.Pool.ObjectPool`).
*   **Dùng `TryGetComponent` thay vì `GetComponent`:** Trong các vòng lặp vật lý (`OnTriggerEnter2D`, `OnCollisionStay2D`), `TryGetComponent(out T)` nhanh hơn đáng kể ở tầng C++ vì nó loại bỏ overhead của việc ném lỗi hoặc trả về null cồng kềnh.
*   **Dùng các hàm NonAlloc của Physics2D:** Khi cần quét vùng (AOE), hãy dùng `Physics2D.OverlapBoxNonAlloc` hoặc `OverlapCircleNonAlloc` kết hợp với một mảng tĩnh cấp phát sẵn (`private static readonly Collider2D[] _hitBuffer = new Collider2D[50];`) để hoàn toàn triệt tiêu rác (0 GC Allocation).
*   **Không dùng `GetComponent<>` trong `Update()`:** Phải Cache (lưu trữ) các reference vào biến ở hàm `Awake()` hoặc `Start()`.
*   **Tránh dùng `FindObjectOfType<>` hay `GameObject.Find()`:** Nếu cần liên kết các hệ thống, hãy dùng Dependency Injection (như đã nói ở mục 3) hoặc thông qua Event.
*   **Sử dụng `CompareTag("Enemy")`:** Thay vì viết `gameObject.tag == "Enemy"`, hãy dùng `gameObject.CompareTag("Enemy")` để không sinh ra chuỗi (string allocation) làm rác bộ nhớ.

---

## 7. Format Code & Comment (Quy Chuẩn Viết & Giải Thích Code)

*   **Brace Style (Dấu ngoặc nhọn):** Sử dụng chuẩn Allman cho C#. Dấu mở ngoặc `{` nằm ở một dòng riêng biệt.
*   **Thụt lề (Indentation):** Khuyên dùng 4 Spaces (thay vì Tab) để đảm bảo code hiển thị đồng nhất trên mọi Editor.
*   **XML Comments:** Các class, phương thức public, hoặc các API dùng chung cho toàn bộ dự án cần phải có XML Summary (`/// <summary>`) để giải thích mục đích và các tham số.
*   **Viết comment có ý nghĩa:** Chỉ comment để giải thích **TẠI SAO** (Why) một đoạn code được viết theo cách đó (những logic mập mờ hoặc fix lỗi cụ thể). Không comment giải thích code đang **LÀM GÌ** (What) – code sạch phải tự giải thích được chính nó.

---

## 8. Quản Lý Trạng Thái Game (Game State Management)

*   Sử dụng **State Machine (FSM)** để quản lý các trạng thái vòng đời của game (ví dụ: MainMenu, Playing, Paused, GameOver).
*   Tuyệt đối **không** rải rác các biến kiểm tra trạng thái như `bool isGameOver`, `bool isPaused` lộn xộn trong từng script (khiến logic bị rối).
*   Thay vào đó, các script nên lắng nghe (subscribe) vào các event như `OnGamePaused`, `OnGameOver` từ hệ thống Quản lý Game State để tự động dừng mọi hành động.

---

## 9. Tiêu Chuẩn Công Nghệ & Package Bắt Buộc

*   **Phiên bản Unity:** Dự án đang sử dụng **Unity 2022**. Đảm bảo sử dụng đúng phiên bản để tránh lỗi tương thích.
*   **Hệ thống UI (Text):** Bắt buộc sử dụng **TextMeshPro** cho tất cả các thành phần văn bản trên UI. Tuyệt đối không sử dụng Text (Legacy) để đảm bảo chất lượng hình ảnh sắc nét và tối ưu draw call.
*   **Hệ thống Input:** Bắt buộc sử dụng **New Input System** (package `com.unity.inputsystem`). Việc sử dụng `Input.GetKeyDown` hoặc `Input.GetAxis` kiểu cũ bị nghiêm cấm, nhằm hỗ trợ việc mở rộng điều khiển (nếu có) và quản lý Input thông qua Input Actions dễ dàng hơn.

---

## 10. Quản Lý Hoạt Ảnh (Animation Management)

Để đối phó với số lượng animation khổng lồ và tránh tình trạng "Animator Spaghetti" (mạng nhện mũi tên Transition rối rắm, kẹt logic):
*   **Animator State Machine by Script:** Quản lý các trạng thái hoạt ảnh bằng Script C# thông qua một `Enum` (ví dụ: `PlayerAnimationState { Idle, Run, Dash, Dead }`).
*   **KHÔNG sử dụng Transitions (mũi tên):** Tuyệt đối không nối các mũi tên chuyển trạng thái trong cửa sổ Animator (ngoại trừ AnyState cho các đòn đánh chớp nhoáng).
*   **Sử dụng `animator.Play()`:** Gọi trực tiếp hàm `animator.Play(StateName)` (hoặc Hash) từ C# để ép nhân vật chuyển hoạt ảnh ngay lập tức. Tên của State trong Animator Box phải đặt chuẩn xác trùng với tên Enum. Đảm bảo luồng logic luôn luôn nằm trong tầm kiểm soát của code.

---

## 11. Kiến Trúc AI Kẻ Địch (Enemy AI Architecture)

Hệ thống AI của quái vật bắt buộc tuân theo thiết kế kết hợp giữa **State Machine (FSM)** và **Strategy Pattern** nhằm tối đa hóa khả năng mở rộng.

*   **Tách Biệt FSM và Logic Đặc Thù:** Lõi FSM (`EnemyStateMachine`, `EnemyState`) chỉ quản lý luồng trạng thái (Idle, Chase, Attack, Reposition, Dead). FSM tuyệt đối không chứa logic đặc thù (như quái bắn đạn hay chém cận chiến).
*   **Strategy Pattern Cho Hành Vi:** 
    *   Bắt buộc kế thừa `AttackStrategy` (ví dụ: `MeleeAttackStrategy`, `RangedAttackStrategy`) để định nghĩa cách thức gây sát thương và logic tấn công.
    *   Bắt buộc kế thừa `CombatMovementStrategy` (ví dụ: `MeleeMovementStrategy`, `RangedMovementStrategy`) để định nghĩa cách di chuyển trong chiến đấu (giữ khoảng cách, lùi lại, tiến lên).
*   **Cấu Hình Bằng ScriptableObject:** Mọi thông số (máu, sát thương, tầm đánh, `preferredDistance`, `minDistance`) phải nằm trong `EnemyConfig`. Không được hard-code các chỉ số này vào bên trong State hoặc Strategy.
*   **Nguyên Tắc Mở Rộng (Open/Closed Principle):** Khi thêm kẻ địch mới có hành vi dị biệt, tuyệt đối KHÔNG viết câu lệnh `if (enemyType == ...)` bên trong FSM. Hãy tạo ra các `AttackStrategy` hoặc `CombatMovementStrategy` mới và gắn vào Prefab.
