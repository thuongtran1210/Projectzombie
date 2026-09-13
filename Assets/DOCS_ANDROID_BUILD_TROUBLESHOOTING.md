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
7. [Nhóm Lỗi Addressables & Firebase Storage CDN (DLC / Map Download)](#7-nhóm-lỗi-addressables--firebase-storage-cdn-dlc--map-download)
8. [Nhóm Tối Ưu Độ Trễ UI & Khắc Phục Khựng Lần Đầu Mở Màn Hình (UI Freeze Spike Optimization)](#8-nhóm-tối-ưu-độ-trễ-ui--khắc-phục-khựng-lần-đầu-mở-màn-hình-ui-freeze-spike-optimization)

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

### ❌ Lỗi 2.5: Xung Đột Dữ Liệu Addressables Giữa Editor & Android Build (Play Mode Script)
- **Hiện tượng**: Trên Editor luôn hiện `[ĐÃ TẢI]` kèm nút `[XÓA]`, không test được luồng tải file từ CDN; trong khi trên Android báo `[CHƯA TẢI]`.
- **Nguyên nhân**: Editor mặc định dùng chế độ `Use Asset Database (fastest)` nên bỏ qua cache mạng, `GetDownloadSizeAsync` luôn trả về 0 bytes.
- **Cách khắc phục**:
  1. Trong cửa sổ `Window > Asset Management > Addressables > Groups`, chuyển **Play Mode Script** sang **Use Existing Build (requires built groups)** để Editor đọc đúng Cache giống thiết bị thật.
  2. Mở `Tools > ProjectZombie > Addressables > CDN Content Comparator & Audit Tool`, bấm **Xóa Toàn Bộ Local Cache** để giả lập thiết bị mới cài game.

---

### ❌ Lỗi 2.6: Lỗi Hiển Thị Đè Chữ (Text Overlap) Trên Item Của `Modal_ResourceDownload`
- **Hiện tượng**: Dòng chữ trạng thái `[CHƯA TẢI - 0.1 MB]` bị đè chồng lên nút `[TẢI VỀ]`.
- **Nguyên nhân**: Cả `Txt_Status` và `Btn_Download` cùng hiển thị nội dung dung lượng và kích thước vùng neo bị chồng chéo.
- **Cách khắc phục**:
  1. Tách biệt rõ: `Txt_Status` chỉ hiện nhãn trạng thái `[CHƯA TẢI]`, còn nút `Btn_Download` chịu trách nhiệm hiển thị dung lượng tải `TẢI VỀ (XX.X MB)`.
  2. Mở rộng chiều cao item lên `92px` và phân bổ vùng layout độc lập cho Text và Button.

---

### ❌ Lỗi 2.7: Dung Lượng Các Ải Bị Sai Lệch (Hiển Thị 0.1 MB Thay Vì 12.4 MB)
- **Hiện tượng**: Thanh tải Màn 2 / 3 chỉ báo 0.1 MB hoặc 0% do Catalog trên CDN chưa có AssetBundle hoàn chỉnh.
- **Cách khắc phục**:
  1. Tích hợp cơ chế tự động Fallback lấy `estimatedSizeMb` từ cấu hình `StageDefinitionSO` khi dung lượng mạng trả về $\le 0.05\text{ MB}$.
  2. Sử dụng công cụ **`Tools > ProjectZombie > Addressables > 🔨 Build Addressables Content Bundles`** để đóng gói toàn bộ và kéo thả lên Firebase Storage CDN.

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

---

## 7. Nhóm Lỗi Addressables & Firebase Storage CDN (DLC / Map Download)

### ❌ Lỗi 7.1: Bấm "Tải Màn Chơi" nháy % rồi quay lại nút "Tải Màn Chơi" (`HTTP 400 Bad Request` / `404 Not Found`)
- **Hiện tượng**:
  - Trên Android, người chơi mở màn hình Chọn Ải hoặc Modal Quản Lý Dữ Liệu Tải Về, bấm **Tải Màn Chơi (hoặc Tải Về)**.
  - Thanh tiến trình nháy lên `0MB / 0MB (0%)` rồi lập tức đóng lại, nút quay về trạng thái **"Tải Màn Chơi"** thay vì **"XUẤT TRẬN"**.
  - Logcat ADB xuất hiện lỗi:
    ```text
    TextDataProvider : unable to load from url : https://firebasestorage.googleapis.com/v0/b/vongxuyen.firebasestorage.app/o/Android%2F0?alt=media/catalog_1.0.hash
    UnityWebRequest result : ProtocolError : HTTP/1.1 404 Not Found
    url : https://firebasestorage.googleapis.com/v0/b/vongxuyen.firebasestorage.app/o/Android/Android/0?alt=media
    HTTP/1.1 400 Bad Request
    ```
- **Nguyên nhân gốc rễ**:
  1. **Lỗi nối chuỗi URL của Unity Addressables**:
     - Khi cấu hình `Remote.LoadPath` có tham số query `.../o/Android%2F{0}?alt=media` hoặc `.../o/Android/[BuildTarget]`.
     - Unity Addressables coi URL là thư mục tĩnh và nối tên file bundle vào cuối chuỗi URL:
       $$\rightarrow \text{https://.../Android\%2F0?alt=media/catalog\_1.0.hash}$$
     - Firebase Storage REST API không hỗ trợ nhận tên file đặt sau query param `?alt=media` nên trả về `HTTP 404 / 400 Bad Request`.
  2. **Thứ tự khởi tạo Runtime**:
     - Nếu hàm xử lý URL chỉ được gán khi khởi tạo `AddressablePatchManager` thì các thao tác kiểm tra Catalog ban đầu của Addressables sẽ vẫn dùng URL lỗi trước đó.
- **Cách khắc phục chuẩn**:
  1. **Đăng ký `Addressables.InternalIdTransformFunc` tại nguồn tin duy nhất (Single Source of Truth)**:
     - Đặt tại `AddressablePatchManager.SetupInternalIdTransformStatic()` với thuộc tính `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]`.
     - Trong `CoreBootstrapper.cs` chỉ gọi ủy quyền sang `AddressablePatchManager.SetupInternalIdTransformStatic()`, tránh phân mảnh và ghi đè chéo logic.
  2. Sử dụng Regex bóc tách chính xác tên file `.bundle`, `.hash`, `.json` và tái tạo URL REST API Firebase chuẩn query param `/o?name=...` (ngăn chặn lỗi unescape `%2F` thành `/` trên Android):
     ```csharp
     UnityEngine.AddressableAssets.Addressables.InternalIdTransformFunc = location =>
     {
         if (string.IsNullOrEmpty(location.InternalId)) return location.InternalId;

         if (location.InternalId.Contains("firebasestorage.googleapis.com") || location.InternalId.Contains("vongxuyen.firebasestorage.app"))
         {
             string rawUrl = location.InternalId;
             
             // Bóc tách tên file từ bất kỳ URL biến dạng nào
             var match = System.Text.RegularExpressions.Regex.Match(rawUrl, @"(?<filename>[\w\-\._]+\.(bundle|hash|json))", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
             if (match.Success)
             {
                 string fileName = match.Groups["filename"].Value;
                 string platformFolder = "Android";
                 return $"https://firebasestorage.googleapis.com/v0/b/vongxuyen.firebasestorage.app/o?name={platformFolder}%2F{fileName}&alt=media";
             }
         }
         return location.InternalId;
     };
     ```

### ❌ Lỗi 7.2: Quy Chuẩn Vòng Đời Đồng Bộ Giữa `Modal_ResourceDownload` và `Screen_StageSelect` Khi Xóa Bản Đồ
- **Nguyên lý thiết kế & Luồng dữ liệu (Data Flow)**:
  1. **Hành động Xóa Cache**: Khi người chơi nhấn nút `[XÓA]` trên gói bản đồ DLC (ví dụ: Ải 2 `Map_AncientCitadel` hoặc Ải 3 `Map_CinnabarSwamp`), `ResourceDownloadModalPresenter` sẽ gọi:
     ```csharp
     await _patchManager.ClearAssetCacheAsync(data.addressableKey);
     ```
     Toàn bộ file AssetBundle lưu trong persistent cache của máy bị dọn sạch.
  2. **Đồng bộ tự động khi chuyển màn hình**:
     - Khi người chơi đóng modal tải và quay lại giao diện chọn ải `Screen_StageSelect`, vòng đời `OnEnable()` của `StageSelectUIPresenter` sẽ tự động chạy `RefreshView()`.
     - Phương thức `_patchManager.CheckAssetStatusAsync(currentStage.mapPrefabAddress)` kiểm tra lại bộ nhớ và phát hiện `NeedsDownload = true` (Dung lượng tải $> 0$).
  3. **Quy tắc hiển thị nút trên View (`StageSelectUIView`)**:
     - **Ải 1 (Mặc định)**: Nằm trong APK gốc (`Resources/Maps`) $\rightarrow$ Nút `[XUẤT TRẬN]` luôn luôn sáng (`isDlcDownloaded = true`), chơi offline bình thường.
     - **Ải 2 / Ải 3 (DLC vừa bị xóa)**: `isDlcDownloaded = false` $\rightarrow$ Nút `[XUẤT TRẬN]` tự động ẩn đi (`SetActive(false)`), thay bằng nút `[TẢI MÀN CHƠI (XX KB/MB)]` (`SetActive(true)`).
     - Định dạng dung lượng thông minh: Tự động hiển thị `KB` nếu $< 0.1\text{ MB}$ và `MB` nếu $\ge 0.1\text{ MB}$.
  4. **Cơ chế Fallback an toàn (Defensive Fallback)**:
     - Trong trường hợp bất khả kháng (lỗi mạng hoặc mất kết nối đột ngột khi vừa bấm xuất trận), `MetaSceneTransitionController` sẽ tự động fallback sang Tilemap mặc định của Scene (Rừng Vọng Xuyên) để **triệt tiêu 100% nguy cơ crash app hoặc màn hình đen**.

---

## 8. Nhóm Tối Ưu Độ Trễ UI & Khắc Phục Khựng Lần Đầu Mở Màn Hình (UI Freeze Spike Optimization)

### ❌ Lỗi 8.1: Bấm Nút Mở Màn Hình UI Lần Đầu Bị Khựng Khung Hình (Freeze Spike ~350ms - 500ms)
- **Hiện tượng**:
  - Khi người chơi bấm vào các nút chức năng ở Sảnh Chính (Main Hub) như **"Tàng Bảo Các" (WeaponLoadout)** hoặc **"Bách Bảo Các / Thần Thẻ" (CardCodex)**:
    - **Lần đầu tiên bấm**: Màn hình bị đứng/khựng rõ rệt khoảng `350ms - 500ms` trước khi giao diện mở ra.
    - **Lần thứ hai trở đi**: Mở mượt mà, phản hồi tức thì dưới `50ms`.
  - Log đo lường thời gian thực tế:
    ```text
    [MetaUIManager.OpenScreen] TỔNG THỜI GIAN MỞ 'WeaponLoadout': 358 ms (Bao gồm Factory, Awake, OnEnable, Render Grid) cho lần 1. Lần 2: 47 ms
    ```
- **Nguyên nhân gốc rễ**:
  1. **Lazy Instantiate Prefab UI**: UI Screen không nằm sẵn trên Canvas mà được tạo theo nhu cầu (`UIScreenFactory.GetOrCreateScreen`). Lần đầu mở, Unity phải tải GameObject Prefab lớn và Instantiate cây Hierarchy phức tạp.
  2. **I/O Disk Đọc Tài Nguyên Đồng Bộ Trên Main Thread**: Trong `Awake()` hoặc `OnEnable()`, Presenter gọi `Resources.LoadAll<WeaponData>`, `Resources.Load<Sprite>` hàng chục lần mà không có bộ nhớ đệm (Cache tĩnh).
  3. **Tạo Mới Hàng Loạt GameObject Item UI Khi Render Grid**: Khi hiển thị danh sách vũ khí/thần thẻ, View gọi `GameObject.Instantiate()` hoặc tạo mới thủ công từ 15 đến 30 slot item (`UniversalItemSlotView` / `CodexSlotItemView`), khiến Unity phải cấp phát bộ nhớ, thêm component, ép Canvas Re-batching gây nghẽn CPU.
  4. **Render Grid Trùng Lặp 2 Lần**: Cả `Awake/Start()` và `OnEnable()` đều kích hoạt hàm populate danh sách khiến grid bị dựng lại 2 lần liên tiếp trong cùng một khung hình.

- **Giải pháp xử lý chuẩn & Triệt tiêu hoàn toàn độ trễ**:
  1. **Tái Sử Dụng & Prewarm Slot Object Pool trong View (`PrewarmSlots`)**:
     - Thay vì `Instantiate()` và `Destroy()` các slot item, View lưu trữ và tái sử dụng các Transform con có sẵn trong Grid Container:
     ```csharp
     // Trong WeaponLoadoutView.cs / CardCodexView.cs:
     public void PrewarmSlots(int count)
     {
         if (_inventoryGridContainer == null) return;
         int existing = _inventoryGridContainer.childCount;
         for (int i = existing; i < count; i++)
         {
             var slot = UniversalItemSlotView.CreateDynamicSlot(_inventoryGridContainer);
             slot.gameObject.SetActive(false);
         }
     }

     public void ClearGrid()
     {
         _activeSlotIndex = 0;
         for (int i = 0; i < _inventoryGridContainer.childCount; i++)
             _inventoryGridContainer.GetChild(i).gameObject.SetActive(false); // Ẩn thay vì Destroy
     }

     public UniversalItemSlotView CreateSlotItem()
     {
         if (_activeSlotIndex < _inventoryGridContainer.childCount)
         {
             var child = _inventoryGridContainer.GetChild(_activeSlotIndex++);
             child.gameObject.SetActive(true);
             return child.GetComponent<UniversalItemSlotView>();
         }
         // Chỉ tạo mới nếu vượt quá số lượng đã prewarm
         var newSlot = UniversalItemSlotView.CreateDynamicSlot(_inventoryGridContainer);
         newSlot.gameObject.SetActive(true);
         _activeSlotIndex++;
         return newSlot;
     }
     ```
  2. **Tĩnh Hóa Bộ Nhớ Đệm ScriptableObject (Static In-Memory Cache)**:
     - Dữ liệu `Resources.LoadAll` chỉ nạp một lần duy nhất vào bộ nhớ RAM (`static readonly List<T>`):
     ```csharp
     private static readonly List<WeaponData> _cachedWeapons = new List<WeaponData>();
     private static bool _isWeaponsLoaded = false;

     public void LoadAllWeaponsIfEmpty()
     {
         if (_isWeaponsLoaded && _cachedWeapons.Count > 0)
         {
             _allWeapons = _cachedWeapons;
             return; // Trả về ngay lập tức 0ms
         }
         // Nạp Resources.LoadAll...
         _allWeapons = _cachedWeapons;
         _isWeaponsLoaded = true;
     }
     ```
  3. **Khởi Tạo Trước Slot UI Ngay Trong `Awake()`**:
     - Gọi `PrewarmSlots(15)` ngay trong `Awake()` của Presenter khi màn hình vừa được instantiate lần đầu.
  4. **Tách Biệt Hàm Cập Nhật Trực Quan Nhẹ (Update Visuals Only)**:
     - Khi người chơi click chọn hoặc trang bị vật phẩm, **CẤM** gọi lại toàn bộ `PopulateInventoryGrid()` (sẽ duyệt và dựng lại toàn bộ slot).
     - Thay vào đó, gọi `UpdateSelectionDetailsOnly()` / `UpdateGridItemStates()` chỉ cập nhật viền sáng và dữ liệu của các slot đã lưu trong `Dictionary<Item, SlotView>`.
  5. **Loại Bỏ Gọi Trùng Lặp Giữa `Start()` và `OnEnable()`**:
     - Bỏ lệnh dựng UI trong `Start()` nếu `OnEnable()` đã đảm nhiệm việc nạp dữ liệu khi màn hình hiển thị.
