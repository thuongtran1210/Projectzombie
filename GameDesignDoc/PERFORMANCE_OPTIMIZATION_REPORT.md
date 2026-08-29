# Báo Cáo Kỹ Thuật: Tối Ưu Hiệu Năng Game & Triệt Tiêu Freeze Spikes (Zero-Lag Profiling Guide)

**Dự án:** Project Zombie (Top-down Hack & Slash ARPG)  
**Nền tảng:** Unity 2022.3+ LTS | Universal Render Pipeline (URP) | Target Mobile 60 FPS  
**Ngày hoàn thiện:** 29/08/2026  

---

## 📊 1. Bảng Tổng Hợp Các Điểm Nghẽn Hiệu Năng Đã Khắc Phục

| Hạng Mục / Thành Phần | Nguyên Nhân Gốc Rễ (Root Cause) | Trước Tối Ưu | Sau Tối Ưu | Hiệu Quả Đạt Được |
|---|---|---|---|---|
| **1. Enemy Pooling (`EnemyPoolManager`)** | `Instantiate` đồng bộ 150-200 GameObject quái cùng 1 frame lúc start wave. | 🛑 **Freeze $400 - 500\text{ms}$** | ⚡ **Time-sliced Prewarm (3 con/frame)** | Triệt tiêu hoàn toàn spike frame khởi tạo pool. |
| **2. Âm Thanh BGM (`SoundManager.LoadFMODSound`)** | File BGM dài 3 phút để mặc định `Decompress On Load` + `loadInBackground: 0` ép CPU FMOD giải nén trên Main Thread. | 🛑 **Freeze $505 - 540\text{ms}$** | ⚡ **Streaming Mode + Preload in Loading Screen** | Giảm từ $505\text{ms}$ xuống **$0.00\text{ms}$** trên Game Loop. |
| **3. Spawner Initialization (`SpawnManager.Start`)** | Gọi `GameObject.Find("Tilemap_Ground")` và `FindObjectOfType<Tilemap>` quét toàn bộ Scene ở Frame 1. | 🛑 **$22.33\text{ms}$ + $113.9\text{ KB}$ GC** | ⚡ **Lazy Initialization (`EnsureDependencies`)** | Start() đạt **$0.00\text{ms}$ / $0\text{ B}$ GC Alloc**. |
| **4. Bảng Chọn Hệ Thư Sinh (`ThuSinhElementPickerOverlayView`)** | Chạy `Update()` liên tục ở mỗi frame dù UI đang ẩn + cấp phát Anonymous Lambda delegates cho 5 nút. | 🛑 **$7.80\text{ms}$ + $1.8\text{ KB}$ GC** | ⚡ **`enabled = false` khi ẩn + Static Instance Listeners** | Giảm về **$0.00\text{ms}$ / $0\text{ B}$ GC Alloc**. |
| **5. Quét Dữ Liệu Nâng Cấp (`UpgradeManager`)** | Gọi `AssetDatabase.FindAssets` / `Resources.LoadAll` nạp hàng chục thẻ SO từ ổ cứng mỗi khi load trận. | 🛑 **$15 - 30\text{ms}$ Disk I/O** | ⚡ **Static Memory Cache (`_cachedMasterUpgrades`)** | Nạp 1 lần duy nhất, các lần vào màn sau tốn **$0\text{ms}$**. |
| **6. Chu Trình Kiểm Tra Phase Nhạc (`PhaseAudioController`)** | Chạy kiểm tra đổi phase âm thanh ở mỗi frame ($60\text{ Hz}$). | 🛑 **CPU Poll Lặp Lại** | ⚡ **Throttled Timer $1.0\text{s/lần}$** | Giảm $98\%$ tần suất kiểm tra điều kiện. |
| **7. Quy Trình Nạp Trận Đấu (`MetaSceneTransitionController`)** | Các hệ thống độc lập tự nạp lúc `Start()`, chồng chéo tranh chấp CPU. | 🛑 **Spike phân mảnh $60 - 80\text{ms}$** | ⚡ **Real Loading Pipeline (`ShowTaskLoading`)** | Nạp ngầm $100\%$ phía sau màn hình Loading. |

---

## 🛠️ 2. Chi Tiết Phân Tích & Giải Pháp Kỹ Thuật

### 2.1. Triệt Tiêu Cú Đứng Hình FMOD Audio ($505\text{ms} \rightarrow 0\text{ms}$)
* **Vấn Đề Gặp Phải:** Profiler báo `UnitySynchronizationContext.ExecuteTasks() -> SoundManager.LoadFMODSound` ngốn tới `505.42 ms` khi bắt đầu phát nhạc nền trận đấu (`Trống Đồng Xung Trận.mp3` và `BGM_MainHub_VongXuyen.wav`).
* **Bản Chất:** Unity cấu hình mặc định các file nhạc dài ở chế độ `loadType: 0 (Decompress On Load)`. Khi gọi `AudioSource.Play()`, engine bắt CPU giải nén toàn bộ luồng âm thanh hàng chục MB vào RAM cùng lúc.
* **Giải Pháp:**
  1. Chuyển cấu hình Audio Importer sang `loadType: 2 (Streaming)` trong file `.meta`.
  2. Bật `loadInBackground: 1` và `preloadAudioData: 1`.
  3. Gọi `phaseAudio.ForceInitialPhaseAudio()` ngay trong lúc Loading Screen đang hiển thị để FMOD mở handle trước khi mở màn hình.

---

### 2.2. Kiến Trúc Tải Ngầm Thực Tế (Real Async Loading Pipeline)
* **Vấn Đề Gặp Phải:** Trước đây màn hình Loading chỉ chạy thời gian giả lập (`ShowLoading(0.85s)`), trong khi các logic nặng lại tự chạy dồn dập sau khi màn hình Loading đã tắt, gây khựng hình lúc vừa thấy nhân vật.
* **Giải Pháp Kiến Trúc:**
  * Xây dựng phương thức `ShowTaskLoading` trong [LoadingScreenPresenter.cs](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/UI/LoadingScreenPresenter.cs):
  ```
  [Bấm Xuất Trận]
         │
         ▼
  ┌────────────────────────────────────────────────────────┐
  │ MÀN HÌNH LOADING BẬT LÊN (Fade In 0.2s)                │
  ├────────────────────────────────────────────────────────┤
  │ 20%: Tạo thực thể Player, Reset HP & Camera            │
  │ 50%: Nạp Prefab quái Addressables & Time-sliced Pool   │
  │ 80%: Nạp Database Nâng Cấp & Kích hoạt BGM FMOD Handle │
  │ 100%: GameStateManager -> Playing                      │
  ├────────────────────────────────────────────────────────┤
  │ MÀN HÌNH LOADING MỜ DẦN (Fade Out 0.25s)               │
  └────────────────────────────────────────────────────────┘
         │
         ▼
  [BƯỚC VÀO CHIẾN TRƯỜNG - 60 FPS MƯỢT MÀ ZERO FREEZE]
  ```

---

### 2.3. Time-Sliced Asynchronous Object Pooling
* **Vấn Đề Gặp Phải:** Khi WavePreloader chuẩn bị 30 quái/loại cho 4-5 loại quái, CPU bị ép `Instantiate` 150 GameObjects chứa đầy đủ Rigidbody2D, Animator, BoxCollider2D trong đúng 1 frame ($>400\text{ms}$).
* **Giải Pháp:**
  * Trong [EnemyPoolManager.cs](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/Spawners/EnemyPoolManager.cs): Khởi tạo tức thì 6 con đầu tiên để sẵn sàng sử dụng, toàn bộ số lượng còn lại được phân bổ qua Coroutine `RoutinePrewarmSlice` (3 con mỗi frame) $\rightarrow$ Không bao giờ vượt quá ngân sách $16\text{ms}$/frame.

---

### 2.4. Khử GC Allocation & Update Throttling
* **Thư Sinh Overlay View:** Tắt `enabled = false` khi ẩn. Thay thế Anonymous Lambda `() => SelectElement(...)` bằng instance methods tĩnh để triệt tiêu việc sinh rác GC mỗi frame.
* **Aim Indicator & Targeting:** Áp dụng Physics Scan Throttling $20\text{ Hz}$ ($0.05\text{s}$) và dồn toàn bộ ma trận vẽ về duy nhất `LateUpdate()` với `MaterialPropertyBlock` ($0\text{ B}$ GC Alloc).

---

## 📈 3. Kết Quả Kiểm Thử Thực Tế Trên Unity Profiler

* **Tổng thời gian PlayerLoop:** Giảm từ **$549.77\text{ ms}$** xuống còn **$12 - 14\text{ ms}$** (Tương đương **$> 70\text{ FPS}$**).
* **ScriptRunBehaviourUpdate:** Duy trì ổn định từ **$0.19\text{ ms} - 0.8\text{ ms}$**.
* **GC Alloc mỗi frame:** Đạt mức tiệm cận **$0\text{ B}$** trong suốt chu trình chiến đấu.
