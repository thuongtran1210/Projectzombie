# 📱 TỔNG HỢP CÁC LỖI KHI BUILD ANDROID & HƯỚNG DẪN KHẮC PHỤC (PROJECT ZOMBIE)

Tài liệu này tổng hợp toàn bộ các lỗi thực tế đã phát sinh trong quá trình build và chạy game trên thiết bị Android / APK, kèm theo **nguyên nhân cốt lõi**, **cách xử lý chuẩn** và **công cụ tự động hóa 1-Click** để tra cứu nhanh khi gặp sự cố.

---

## 📑 MỤC LỤC
1. [Nhóm Lỗi Biên Dịch Script (Compile Errors / Pre-Build)](#1-nhóm-lỗi-biên-dịch-script-compile-errors--pre-build)
2. [Nhóm Lỗi Tài Nguyên Resources & NullReferenceException Trên Android](#2-nhóm-lỗi-tài-nguyên-resources--nullreferenceexception-trên-android)
3. [Nhóm Lỗi Cảm Ứng Mobile, Phím Ảo (Joystick & Action Buttons)](#3-nhóm-lỗi-cảm-ứng-mobile-phím-ảo-joystick--action-buttons)
4. [Nhóm Lỗi UI Layout & Màn Hình Cảm Ứng (Customizer, Modal, Safe Area)](#4-nhóm-lỗi-ui-layout--màn-hình-cảm-ứng-customizer-modal-safe-area)
5. [Quy Trình Chuẩn Chuẩn Bị Trước Khi Build APK (Checklist 1-Click)](#5-quy-trình-chuẩn-chuẩn-bị-trước-khi-build-apk-checklist-1-click)
6. [Cơ Chế Bảo Trì & Khóa Nóng Tính Năng/Vũ Khí Khi Đã Lên CH Play (Live-Ops Feature Flag)](#6-cơ-chế-bảo-trì--khóa-nóng-tính-năngvũ-khí-khi-đã-lên-ch-play-live-ops-feature-flag)

---

## 1. Nhóm Lỗi Biên Dịch Script (Compile Errors / Pre-Build)

### ❌ Lỗi 1.1: Sử dụng API Editor trong Runtime Code (`UnityEditor` namespace)
- **Hiện tượng**: Chạy trên Unity Editor không báo lỗi, nhưng khi nhấn **Build Android APK** thì trình biên dịch IL2CPP / Mono báo lỗi:
  ```text
  The type or namespace name 'UnityEditor' could not be found
  The name 'AssetDatabase' does not exist in the current context
  ```
- **Nguyên nhân**: Các hàm thuộc `UnityEditor` (như `AssetDatabase.LoadAssetAtPath`, `PrefabUtility`, `EditorUtility`) chỉ tồn tại trong môi trường Editor và bị xóa hoàn toàn khi đóng gói APK Android.
- **Cách khắc phục**:
  1. Đặt các script Editor vào đúng thư mục `Assets/.../Editor/`.
  2. Nếu bắt buộc phải viết trong file Runtime, bọc bằng tiền xử lý:
     ```csharp
     #if UNITY_EDITOR
     using UnityEditor;
     #endif
     // ...
     #if UNITY_EDITOR
     iconToSet = AssetDatabase.LoadAssetAtPath<Sprite>(path);
     #else
     iconToSet = Resources.Load<Sprite>(resourcePath);
     #endif
     ```

---

### ❌ Lỗi 1.2: Lỗi Ép Kiểu `FindAnyObjectByType<T>` với Class Không Kế Thừa `UnityEngine.Object`
- **Hiện tượng**:
  ```text
  error CS0311: The type 'GameplayUIBinder' cannot be used as type parameter 'T' in generic method 'Object.FindAnyObjectByType<T>()'.
  There is no implicit reference conversion from 'GameplayUIBinder' to 'UnityEngine.Object'.
  ```
- **Nguyên nhân**: `GameplayUIBinder` là class thuần C# (không kế thừa `MonoBehaviour`). `FindAnyObjectByType<T>()` của Unity yêu cầu `T : UnityEngine.Object`.
- **Cách khắc phục**:
  - Tìm GameObject Component cha (ví dụ `GameplayUIManager` hoặc `GameplayBootstrapper`) trước, sau đó truy cập property hoặc instance:
    ```csharp
    var bootstrapper = Object.FindAnyObjectByType<GameplayBootstrapper>();
    if (bootstrapper != null) { ... }
    ```

---

### ❌ Lỗi 1.3: Lỗi Thiếu Định Nghĩa Member do Sai Class (`PlayerProvider.Player`)
- **Hiện tượng**:
  ```text
  error CS0117: 'PlayerProvider' does not contain a definition for 'Player'
  ```
- **Nguyên nhân**: `PlayerProvider` lưu trữ `PlayerTransform` và `PlayerControllerInstance`, không có trường tên là `Player`.
- **Cách khắc phục**:
  - Sử dụng đúng property chuẩn:
    ```csharp
    if (PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null)
    {
        var stats = PlayerProvider.PlayerTransform.GetComponent<PlayerStats>();
    }
    ```

---

## 2. Nhóm Lỗi Tài Nguyên Resources & NullReferenceException Trên Android

### ❌ Lỗi 2.1: Bảng UI/Prefab Không Hiển Thị khi bấm nút trên Android (`Resources.Load` trả về `null`)
- **Hiện tượng**: Bấm vào nút Thông số, Cài đặt, hoặc Tùy chỉnh phím ảo nhưng không có UI nào hiện lên.
- **Nguyên nhân**:
  - Các file Prefab UI nằm ngoài thư mục `Assets/Resources/UI/`. Khi đóng gói APK, Unity sẽ loại bỏ toàn bộ file không được tham chiếu trực tiếp trong Scene nếu chúng không nằm trong thư mục `Resources`.
- **Cách khắc phục**:
  1. Đảm bảo toàn bộ Prefab Modal/Overlay được đồng bộ sang:
     - `Assets/Resources/UI/PlayerStatsMenuUI.prefab`
     - `Assets/Resources/UI/SettingsModalUI.prefab`
     - `Assets/Resources/UI/MobileControlsCustomizerUI.prefab`
  2. Sử dụng cơ chế fallback an toàn:
     ```csharp
     var prefab = Resources.Load<GameObject>("UI/MobileControlsCustomizerUI");
     #if UNITY_EDITOR
     if (prefab == null) {
         prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/UI/MobileControlsCustomizerUI.prefab");
     }
     #endif
     ```

---

### ❌ Lỗi 2.2: Thiếu Dữ Liệu Nâng Cấp (Upgrades), Âm Thanh (Audios) hoặc Quái Vật (Enemies)
- **Hiện tượng**: Lên cấp không hiện thẻ bài kỹ năng, game không có nhạc/tiếng hoặc không spawn được quái.
- **Nguyên nhân**:
  - Dữ liệu `UpgradeDataSO` nằm ở `Assets/_Data/Upgrades/` chưa được copy vào `Assets/Resources/Upgrades/`.
  - File âm thanh chưa copy vào `Assets/Resources/Audios/`.
- **Cách khắc phục**:
  - Chạy công cụ tự động hóa **`Tools > ProjectZombie > ⚡ 1-Click Sync All Resources for Android Build`**.

---

### ❌ Lỗi 2.3: Đảo Ngược Hướng Đồng Bộ Asset trong `SyncRegistry` Khiến Mất Dữ Liệu Banner Gacha / Thần Thẻ
- **Hiện tượng**: Quay Gacha trong `Modal_Gacha` (Bảo Gương) trừ tiền nhưng trong `Panel_CardCodex` (Bách Bảo Các / Thần Thẻ) không nhận được mảnh pháp bảo đã trúng.
- **Nguyên nhân**:
  - Trong `SyncRegistry.cs`, quy tắc đồng bộ đơn lẻ bị cấu hình đảo chiều nguồn và đích:
    ```csharp
    // SAI (Dẫn đến không đồng bộ cấu hình mới từ _Data sang Resources):
    SyncRule.ForSingleAsset("GachaBanner", "Assets/Resources/Gacha/banner_standard.asset", "Assets/_Data/Gacha/banner_standard.asset");
    ```
  - Khi chạy Tool Sync, file mới chứa danh sách Drop Pool trong `_Data` không được đẩy vào `Resources`, dẫn đến bản build Android load file cấu hình cũ/thiếu ID vũ khí.
- **Cách khắc phục**:
  - Đảo đúng chiều `SourcePath` (_Data) $\rightarrow$ `TargetPath` (Resources):
    ```csharp
    // ĐÚNG:
    SyncRule.ForSingleAsset("GachaBanner", "Assets/_Data/Gacha/banner_standard.asset", "Assets/Resources/Gacha/banner_standard.asset");
    ```

---

### ❌ Lỗi 2.4: Phân Tích Kiến Trúc Load Tài Nguyên Trên Android: `Resources.Load` vs `Addressables`
- **Thực trạng hiện tại**:
  - Dự án đang sử dụng mô hình **Offline-First Synchronous** qua `Resources.Load` kết hợp công cụ tiền xử lý **Android Resource Sync Tool** và nén Texture **ASTC** / Audio **Vorbis**.
- **Đặc điểm & Hạn chế của `Resources.Load` trên Android**:
  1. *Metadata Overhead*: Toàn bộ asset trong `Resources` sẽ được lập chỉ mục vào `resources.assets`, nạp metadata vào RAM ngay khi mở game.
  2. *Không hỗ trợ Hot-Update*: Muốn thêm vũ khí/tướng mới phải build lại toàn bộ APK/AAB.
  3. *Freeze Spike*: Load Prefab lớn trực tiếp trên Main Thread có thể gây khựng hình nhẹ.
- **Lộ trình nâng cấp Addressables (Khuyến nghị cho Live-Ops)**:
  - Chuyển đổi các gói tài nguyên nặng (Quái vật, VFX, Audio, UI Prefabs) sang **Addressable Groups**.
  - Tích hợp cơ chế tải bất đồng bộ `Addressables.LoadAssetAsync<T>()` và **Google Play Asset Delivery (PAD)** khi phát hành chính thức trên Google Play Store.

---

## 3. Nhóm Lỗi Cảm Ứng Mobile, Phím Ảo (Joystick & Action Buttons)

### ❌ Lỗi 3.1: Joystick Báo Cảnh Báo Thiếu Reference (`containerRect` / `handleRect`)
- **Hiện tượng**:
  ```text
  [DynamicVirtualJoystick] containerRect hoặc handleRect chưa được gán trên Inspector.
  UnityEngine.Debug:LogWarning
  ```
- **Nguyên nhân**: Khi khởi tạo lại cụm UI cảm ứng hoặc instantiate runtime, các trường `RectTransform` chưa được serialize hoặc bị mất liên kết.
- **Cách khắc phục**:
  - Viết cơ chế tự phân giải linh hoạt (`AutoResolveReferences()`):
    ```csharp
    if (containerRect == null) {
        containerRect = transform.Find("Joystick_Base") as RectTransform 
                     ?? GetComponent<RectTransform>();
    }
    if (handleRect == null && containerRect != null) {
        handleRect = containerRect.Find("Joystick_Handle") as RectTransform 
                  ?? transform.Find("Joystick_Handle") as RectTransform;
    }
    ```

---

### ❌ Lỗi 3.2: Kích Hoạt Chiêu Thức Khi Đang Trong Chế Độ Tùy Chỉnh Phím (`MobileControlsCustomizerUI`)
- **Hiện tượng**: Mở bảng chỉnh vị trí nút điều khiển, chạm vào nút đầu tiên thì chọn được; nhưng chạm vào nút thứ 2 để chỉnh tiếp thì lại **bắn chiêu/đánh thường/lướt** luôn.
- **Nguyên nhân**:
  1. Vòng lặp `Update()` và event `OnCooldownUpdated()` của các Presenter (`SignatureSkillPresenter`, `AttackButtonPresenter`, `DashButtonPresenter`) liên tục gọi `SetInteractable(true)`, ghi đè lại trạng thái khóa của nút.
  2. `SmartSkillDragHandler` nhận sự kiện `OnPointerDown`/`OnDrag` mà không kiểm tra trạng thái Edit Mode.
- **Cách khắc phục**:
  1. Tạo cờ tĩnh toàn cục: `CustomizableControlButton.IsAnyInEditMode`.
  2. Chặn toàn bộ luồng xử lý chiêu thức trong Presenter và DragHandler:
     ```csharp
     // Trong SmartSkillDragHandler.cs
     public void OnPointerDown(PointerEventData eventData) {
         if (!enabled || !_isInteractable || CustomizableControlButton.IsAnyInEditMode) return;
         // ...
     }
     
     // Trong AttackButtonPresenter.cs / SignatureSkillPresenter.cs
     private void OnAttackButtonPressed() {
         if (CustomizableControlButton.IsAnyInEditMode) return;
         // ...
     }
     ```
  3. Thêm `eventData.Use()` trong `CustomizableControlButton.OnPointerDown` để chặn event bubbling.

---

## 4. Nhóm Lỗi UI Layout & Màn Hình Cảm Ứng (Customizer, Modal, Safe Area)

### ❌ Lỗi 4.1: Thiếu Nút "LƯU" Trên Menu Tùy Chỉnh Phím Điều Khiển
- **Hiện tượng**: Mở menu tùy chỉnh phím chỉ thấy nút "Mặc định" và "Hủy", không thấy nút "Lưu".
- **Nguyên nhân**:
  - Thanh `Toolbar_Top` có kích thước chiều ngang quá hẹp (`980px`), các nút thao tác bị xếp chồng dọc và nút Lưu bị đẩy lệch ra ngoài khung nhìn trên các tỷ lệ màn hình Android dài (19.5:9, 20:9).
- **Cách khắc phục**:
  - Mở rộng thanh Top Bar (`1020 x 110px`) và bố trí layout 3 phân vùng riêng biệt theo chiều ngang:
    - **Trái:** Tiêu đề và hướng dẫn (`X = 18px`).
    - **Giữa:** Thanh trượt Cỡ và Độ mờ (`X = 370px -> 640px`).
    - **Phải:** 3 nút **💾 LƯU**, **🔄 MẶC ĐỊNH**, **❌ HỦY** (`X = 720px -> 1000px`).

---

### ❌ Lỗi 4.2: Tỉ Lệ Màn Hình / Safe Area Bị Tràn Tai Thỏ (Notch) & Đảo Lộn Chiều
- **Hiện tượng**: UI bị lẹm vào camera nốt ruồi / tai thỏ hoặc các nút nằm sát mép không thể bấm được.
- **Cách khắc phục**:
  - Trong `SettingsModalPresenter.ApplyGlobalSettingsOnBoot()`:
    ```csharp
    QualitySettings.vSyncCount = 0;
    Application.targetFrameRate = 60;
    Screen.sleepTimeout = SleepTimeout.NeverSleep;
    Input.multiTouchEnabled = true;
    ```
  - Các panel căn neo (Anchor) theo Safe Area của màn hình điện thoại.

---

### ❌ Lỗi 4.3: Lỗi Ô Vuông Trắng / Tím `[?]` (Missing Glyph) Do Dùng Emoji Unicode Trực Tiếp
- **Hiện tượng**: Trên Editor hiển thị được icon emoji (như 💀, 💰, ⚔️, ⚠️), nhưng build sang Android thì bị biến thành ký tự ô vuông `[?]` hoặc khối màu hồng/trắng.
- **Nguyên nhân**:
  - Font Asset tùy chỉnh trong TextMeshPro (ví dụ `NotoSerif-Bold SDF`, `Montserrat SDF`) chỉ nạp bảng ký tự ASCII và Tiếng Việt, **không chứa Glyph của Emoji Unicode** của Android OS.
- **Cách khắc phục**:
  - **CẤM** chèn Emoji Unicode trực tiếp vào chuỗi string C#.
  - Thay thế bằng **TMP Rich Text Tags** (`<color=...>`, `<b>...</b>`) kết hợp thuật ngữ Tiếng Việt thuần Cổ Phong (ví dụ: `[Hạ Địch]: 10` thay vì `💀 10`, `[Cổ Tiền]: 1,000` thay vì `💰 1,000`).
  - Sử dụng **TMP Sprite Asset** (`<sprite name="coin">`) hoặc đặt `Image` Component riêng nếu cần hiển thị biểu tượng đồ họa.

---

## 5. Quy Trình Chuẩn Chuẩn Bị Trước Khi Build APK (Android Wizard & Checklist)

Trước khi vào **File > Build Settings > Build (Android)**, bạn thực hiện qua cửa sổ Wizard tự động:

### ✅ Bước 1: Mở Bảng Điều Khiển Android Pre-Build Wizard
Trên thanh Menu Unity Editor:
> 👉 **`Tools > ProjectZombie > 📱 Android Pre-Build Wizard & 1-Click Sync`**

Cửa sổ sẽ:
1. **🔍 Quét chẩn đoán (Scan)**: Tự động kiểm tra Scripting Backend (IL2CPP), Target Architecture (ARM64), Target API Level (34+), và phát hiện sự chênh lệch tài nguyên hoặc thiếu hụt Prefab UI.
2. **⚙️ Tự động cấu hình Settings**: Nút hỗ trợ 1-Click thiết lập IL2CPP, ARM64 + ARMv7, Linear Color Space theo chuẩn Google Play.
3. **⚡ Đồng bộ 100% tài nguyên (1-Click Sync)**:
   - Đồng bộ **Thẻ Nâng Cấp (Upgrades)** vào `Assets/Resources/Upgrades/`.
   - Đồng bộ **Âm Thanh (BGM / SFX)** vào `Assets/Resources/Audios/`.
   - Đồng bộ **Pháp Bảo (Weapons)** vào `Assets/Resources/Weapons/`.
   - Đồng bộ **Quái Vật (Enemies)** vào `Assets/Resources/Enemies/`.
   - Tự động sinh đầy đủ **5/5 Prefab UI**: `SettingsModalUI`, `PlayerStatsMenuUI`, `MobileControlsCustomizerUI`, `WeaponLoadoutUI` (Tàng Bảo Các), `CardCodexUI` (Thư Viện Thần Thẻ & Luyện Khí).
   - Chuẩn hóa phím ảo cảm ứng `TouchZone_Left` và `DynamicVirtualJoystick`.

---

### ✅ Bước 2: Build APK / AAB
- Vào **File > Build Settings > Switch Platform sang Android**.
- Nhấn **Build** hoặc **Build and Run** để trải nghiệm game trên thiết bị Android thực tế!

---

## 6. Cơ Chế Bảo Trì & Khóa Nóng Tính Năng/Vũ Khí Khi Đã Lên CH Play (Live-Ops Feature Flag)

Khi game đã phát hành lên **Google Play (CH Play)**, để bảo trì hoặc tạm khóa một vũ khí / tính năng bị lỗi (mà không làm crash game người chơi cũ), dự án hỗ trợ 3 cấp độ vận hành:

### 🟢 Cấp Độ 1: Feature Flag Cục Bộ (Qua Bản Vá Nhỏ)
- Thêm trường `isUnderMaintenance` trong [`WeaponData.cs`](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/Weapons/WeaponData.cs):
  ```csharp
  [Header("Live-Ops & Maintenance")]
  public bool isUnderMaintenance = false;
  public string maintenanceNotice = "Pháp bảo đang được thợ rèn trùng tu!";
  ```
- **Xử lý UI**: Khi `isUnderMaintenance == true`, trong [`WeaponLoadoutPresenter.cs`](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/UI/WeaponLoadout/WeaponLoadoutPresenter.cs) gắn nhãn 🔒 **"BẢO TRÌ"** và vô hiệu hóa nút Trang Bị.
- **Xử lý Rút Thẻ**: Trong [`UpgradeManager.cs`](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/Upgrades/UpgradeManager.cs), tự động bỏ qua thẻ nâng cấp liên kết với vũ khí đang bảo trì.

---

### 🟡 Cấp Độ 2: Quản Lý Khóa Tập Trung (Centralized Blacklist)
Tạo lớp quản lý trạng thái tĩnh `MaintenanceManager.cs` để quản lý danh sách đen các ID tính năng/vũ khí:
```csharp
public static class MaintenanceManager
{
    private static readonly HashSet<string> DisabledWeaponIds = new HashSet<string> {
        // "wp_dieu_cay", // Bỏ comment để tạm khóa Điếu Cày
    };

    public static bool IsCardCodexEnabled => true; // Đổi thành false để tạm đóng Thư Viện Thần Thẻ

    public static bool IsWeaponAvailable(string weaponId)
    {
        return !DisabledWeaponIds.Contains(weaponId);
    }
}
```

---

### 🔴 Cấp Độ 3: Khóa Nóng Từ Xa Tức Thì 0 Giây (Firebase Remote Config)
*Khóa trực tiếp trên máy người chơi toàn cầu qua Cloud mà không cần đẩy bản cập nhật lên CH Play.*

#### 1. Quy trình kết nối Cloud Remote Config:
1. Tải SDK **Firebase Remote Config** vào dự án Unity (`FirebaseRemoteConfig.unitypackage`).
2. Cấu hình các Key JSON trên Firebase Console:
   - `disabled_weapons`: `"wp_dieu_cay,wp_noi_com_nieu"`
   - `disabled_features`: `"CardCodex,UpgradeShop"`
   - `maintenance_msg`: `"Hệ thống đang cân bằng lại chỉ số pháp bảo này!"`

#### 2. Lớp Quản Lý `RemoteConfigManager.cs`:
```csharp
public class RemoteConfigManager : MonoBehaviour
{
    public static RemoteConfigManager Instance { get; private set; }
    private HashSet<string> _disabledWeapons = new HashSet<string>();

    public bool IsWeaponAvailable(string weaponId)
    {
        if (string.IsNullOrEmpty(weaponId)) return true;
        return !_disabledWeapons.Contains(weaponId);
    }
}
```

#### 3. Điểm Đấu Nối Trong Game:
- **Tàng Bảo Các (`WeaponLoadoutPresenter.cs`)**:
  ```csharp
  bool isAvailable = RemoteConfigManager.Instance == null || RemoteConfigManager.Instance.IsWeaponAvailable(weapon.weaponId);
  if (!isAvailable) { /* Hiện Badge Bảo Trì & Fallback sang Kiếm Trúc */ }
  ```
- **Rút Thẻ Khi Lên Cấp (`UpgradeManager.cs`)**:
  ```csharp
  if (RemoteConfigManager.Instance != null && !RemoteConfigManager.Instance.IsWeaponAvailable(upgrade.linkedWeaponId))
  {
      continue; // Bỏ qua không bốc trúng thẻ của vũ khí đang khóa
  }
  ```
- **Nút Menu Sảnh Chính (`MainHubPresenter.cs`)**:
  ```csharp
  if (RemoteConfigManager.Instance != null && !RemoteConfigManager.Instance.IsFeatureAvailable("CardCodex"))
  {
      _btnCodex.interactable = false; // Tạm khóa mở Thư viện Thần Thẻ
  }
  ```

