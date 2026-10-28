# Root scene và các điểm phải tách trước khi bỏ gameplay 2D

Nguồn: phân tích cấu trúc YAML `SampleScene.unity`, `MapSongQue3D_Prototype.unity`, các Prefab Sảnh và source code C# liên quan ngày 28/10/2026; không mở Unity. Chưa can thiệp sửa scene hoặc xóa code runtime.

---

## 1. Kiến trúc thực tế của SampleScene: Mô hình "All-in-One" (Sảnh + Gameplay 2D)

`SampleScene` được tác giả thiết kế theo kiến trúc nguyên khối gộp chung (**All-in-One Scene**), trong đó cả Sảnh Menu (Meta Hub) và sàn đấu sinh tồn 2D (Survival Roguelite) cùng tồn tại song song trong một Scene:

### 1.1. Cấu trúc Root GameObjects trong SampleScene
- **`EventSystem` (ID: 493332492)**: Quản lý sự kiện Input/UI.
- **`Main Camera` (ID: 519420028)**: Đi kèm `CameraFollow.cs` (chế độ theo dõi Player 2D).
- **`CM_VirtualCamera_2D` (ID: 1377634652)**: Cinemachine Virtual Camera 2D đi kèm `AdaptiveCameraView.cs`.
- **`CharacterPreviewStage` (ID: 2041453062)**: Sân khấu hiển thị nhân vật 3D/2D preview trong Sảnh (`CharacterPreviewStage.cs`).
- **`SkillAimIndicatorController` (ID: 905762330)**: Bộ điều phối vòng ngắm kỹ năng 2D.
- **`MetaSceneTransitionController` (ID: 1279611765)**: Bộ điều phối chuyển cảnh nội bộ giữa Sảnh Menu và Gameplay 2D.
- **`Canvas_Master` (ID: 771661776)**: Canvas mẹ chứa toàn bộ giao diện:
  - `Canvas_MetaMenu` (ID: 950501922): Toàn bộ UI Sảnh (Meta Hub).
  - `Canvas_Gameplay` (ID: 335681717): Toàn bộ UI chiến đấu 2D (Run HUD, Nâng cấp 3 thẻ, Game Over, Mobile Controls).
  - `--- UI_PresenterManager ---` (ID: 719202291): `RunHUDPresenter`.
  - `--- SignatureSkillPresenter ---` (ID: 797243825): `SignatureSkillPresenter`.
  - `Panel_FadeOverlay` (ID: 980325162): Màn đen chuyển cảnh fade in/out.
- **`--- GAME MANAGER ---` (ID: 1217002973)**: Cụm quản lý dữ liệu và hệ thống gameplay 2D.

### 1.2. Giải mã UI Sảnh (Meta Hub) trong SampleScene
Bản quét ban đầu không thấy `MainHubPresenter` trên danh sách trực tiếp vì `MainHubUI` được nhúng dưới dạng **Prefab Instance** (`Assets/_Prefabs/UI/MainHubUI.prefab`, GUID: `993d9d6a82992a74ca49b90beb51ebaf`) làm con trực tiếp của `Canvas_MetaMenu` (Transform ID: `950501923`).

Toàn bộ 6 màn hình chức năng của Sảnh gồm có:
1. **`MainHubUI`** (Prefab Instance): Chứa `MainHubPresenter` & `MainHubView` (Hiển thị avatar tướng, số dư Cổ Tiền, vũ khí trang bị, các nút điều hướng Sảnh và nút Xuất Trận).
2. **`Panel_CharacterSelect`** (ID: 402100694): `CharacterSelectionPresenter` & `CharacterSelectionView` (Chọn tướng, nạp từ `CharacterDatabaseSO`).
3. **`Panel_WeaponLoadout`** (ID: 933490707): `WeaponLoadoutPresenter` & `WeaponLoadoutView` (Tàng Bảo Các: Chọn đòn đánh chính và 2 pháp bảo, nạp vũ khí từ Addressables/Save).
4. **`Panel_SanctuaryTree`** (ID: 1759207209): `MetaUpgradeShopPresenter` & `MetaUpgradeShopView` (Miếu Cổ / Cây Thần: Nâng cấp chỉ số vĩnh viễn tiêu Cổ Tiền).
5. **`Panel_GachaShop`** (ID: 893884392): `GachaChestPresenter` & `GachaChestView` (Quay rương pháp bảo).
6. **`Panel_CardCodex`** (ID: 875046067): `CardCodexPresenter` & `CardCodexView` (Đồ Giám thẻ bài).
7. **`StageSelect_Screen`**: Được mở thông qua `MetaUIManager.OpenScreen(MetaScreenType.StageSelect)` (Presenter `StageSelectUIPresenter` nạp `WorldStageDatabaseSO` và tải bản vá DLC ải từ CDN).

---

## 2. Rà soát các thành phần sống xuyên Scene (`DontDestroyOnLoad`)

Có 2 nguồn gốc tạo ra các đối tượng sống xuyên Scene:

### 2.1. Nguồn chuẩn kiến trúc: `CoreBootstrapper.cs`
- Phương thức `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]` tự động chạy trước khi bất kỳ Scene nào được tải.
- Tự động tạo GameObject container: **`--- APP CORE SERVICES ---`** và gán `DontDestroyOnLoad`.
- Chứa các dịch vụ lõi:
  - `GameManager` (Save / Load tiến trình vào `player_save.json`)
  - `MetaCurrencyManager` (Quản lý Cổ Tiền)
  - `RelicInventoryManager` (Kho mảnh & cấp sao pháp bảo)
  - `RelicGachaManager` (Hệ thống quay rương & pity)
  - `CharacterProgressionManager` (Tiến trình nâng sao tướng)
  - `GameStartupFlowController` (Luồng khởi động & kiểm tra bản vá CDN)
  - Đăng ký `ServiceContext`: `IMetaCurrencyService`, `IGameDataService`, `IAssetProvider`.
- **Đánh giá:** Đây là thành phần lõi sạch, hoạt động độc lập và cần được bảo toàn.

### 2.2. Điểm nghẽn nghiêm trọng: Cụm `--- GAME MANAGER ---` trong SampleScene
Trong `SampleScene.unity`, GameObject root `--- GAME MANAGER ---` (ID: 1217002973) chứa:
- Component `GameStateManager.cs`: Trong hàm `Awake()`, gọi:
  ```csharp
  if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
  ```
- Vì `--- GAME MANAGER ---` là một Root Transform không có cha (`transform.parent == null`), lệnh `DontDestroyOnLoad(gameObject)` của Unity sẽ **đưa TOÀN BỘ GameObject này và TẤT CẢ 15 GameObject con của nó vào phân vùng DontDestroyOnLoad**!
- Danh sách 15 GameObject con bị kéo sống xuyên scene ngoài ý muốn:
  1. `PhaseAudioController` (ID: 156771617)
  2. `UpgradeManager` (ID: 341505318) — Quản lý nâng cấp thẻ 2D
  3. `MapPowerUpSpawner` (ID: 348193430) — Sinh vật phẩm nhặt trên sàn 2D
  4. `ProjectileSystem` (ID: 461733973) — Hệ thống đạn bay 2D
  5. `[CoinPoolManager]` (ID: 597642207) — Pool tiền rơi 2D
  6. `SkillIndicatorManager` (ID: 904659670) — Vòng ngắm chiêu 2D
  7. `[ExpGemPoolManager]` (ID: 953515630) — Pool ngọc EXP 2D
  8. `[PowerUpPoolManager]` (ID: 1009416011) — Pool bùa lợi 2D
  9. `AudioManager` (ID: 1082831953)
  10. `EnemyPoolManager` (ID: 1152325307) — Pool quái vật 2D
  11. `VFXManager` (ID: 1267842864) — `GlobalVFXPoolManager` & `GameJuiceManager`
  12. `GameplayBootstrapper` (ID: 1373099850) — Spawner & UI binder của Player 2D
  13. `ElementCycleManager` (ID: 1518228857) — Vòng tuần hoàn Ngũ Hành 2D
  14. `RunStatsTracker` (ID: 1529856825) — Đếm giờ và hạ địch 2D
  15. `SpawnManager` (ID: 1899758233) — Quản lý đợt quái 2D
- **Hậu quả nếu chuyển Scene sang Defense3D:** Toàn bộ pool quái, đạn, bootstrapper và spawner của gameplay 2D vẫn tiếp tục tồn tại ngầm trong bộ nhớ, lắng nghe sự kiện hoặc chiếm dụng tài nguyên.
- **Xung đột Singleton:** `GameManager`, `MetaCurrencyManager`, `RelicInventoryManager`, `RelicGachaManager` gắn trên `--- GAME MANAGER ---` bị thừa so với `--- APP CORE SERVICES ---`. Khi chạy, `PersistentSingleton.Awake()` tự `Destroy(this)` các script này, nhưng vỏ GameObject và 15 con vẫn tồn tại.

---

## 3. Rà soát đường xuất trận (Match Launch Pathways) còn gọi gameplay 2D

Hiện tại có **3 luồng kích hoạt trận đấu** đều dẫn về gameplay 2D:

### 3.1. Luồng chính: Sảnh Menu -> Chọn Ải -> Xuất trận
1. Người chơi bấm "Xuất Trận" tại `MainHubView` -> `MainHubPresenter.HandleStartRunClicked()`.
2. Mở màn hình Chọn Ải: `MetaUIManager.OpenScreen(MetaScreenType.StageSelect)`.
3. Người chơi bấm bắt đầu tại `StageSelectUIView` -> `StageSelectUIPresenter.HandleStartBattle()`.
4. Phát sự kiện `StageSelectUIPresenter.OnStageSelectedForBattle(stage)`.
5. `MetaSceneTransitionController` bắt sự kiện và chạy `TransitionToCombat(stage)` -> `TransitionToCombatRoutine()`.
6. `TransitionToCombatRoutine` ủy quyền cho `MatchFlowOrchestrator.ExecuteCombatPreparationAsync(...)` -> thực thi `MatchFlowService`:
   - Bước 1: `SpawnManager.Instance.SetTimelineConfig(stage.timelineConfig)` (Cấu hình wave 2D).
   - Bước 2: `LoadMapAsync` (Nạp Tilemap 2D qua Addressables: `Stage_01_BambooForest` hoặc `WorldStageDatabase`).
   - Bước 3: `gameplayBootstrapper.StartMatchFlow()` -> Khởi tạo nhân vật 2D (`PlayerController`, `PlayerStats`, `WeaponManager`, `CharacterCombat`, `PlayerMythicManager`, `PlayerExperience`, `PlayerPassives`), gắn `PlayerInputReader`, kết nối `CameraFollow` và inject `GameplayUIBinder` vào các Presenter 2D.
   - Bước 4: `SpawnManager.Instance.StartMatchAsync()` -> Nạp trước quái vật 2D vào `EnemyPoolManager`.
   - Bước 5: `UpgradeManager.Instance.AutoPopulateUpgradesIfEmptyAsync()` -> Nạp danh sách thẻ nâng cấp Ngũ Hành 2D.
   - Bước 6: `PhaseAudioController.Instance.ForceInitialPhaseAudio()`.
   - Bước 7: `GameStateManager.Instance.ChangeState(GameState.Playing)` -> Đổi TimeScale = 1.
7. Khi màn hình sáng lại: `CheckAndPromptInitialMythicCore()` -> Mở popup chọn 3 Đại Lõi Thần Thoại (`GameState.LevelUpSelection`).

### 3.2. Luồng phụ: Tàng Bảo Các (Weapon Loadout) -> Xuất trận nhanh
1. Trong `WeaponLoadoutView`, người chơi bấm `Btn_StartBattle` -> `WeaponLoadoutPresenter.HandleStartBattle()`.
2. Lưu cấu hình trang bị: `RunLoadoutState.SetLoadout(_currentHero, _selectedPrimary, _selectedRelics)`.
3. Gọi `MetaSceneTransitionController.Instance.StartRun()` -> Chạy `TransitionToCombat(null)`.
4. Đi vào luồng 3.1 với `stage = null` (tự lấy Ải 1 `Stage_01_BambooForest` làm mặc định và nạp Tilemap 2D).

### 3.3. Luồng Co-op Multiplayer
1. Tại Sảnh, người chơi mở phòng mạng qua `PhotonFusionSessionService`.
2. Khi vào trận, `NetworkPlayerSpawner` sinh Player mạng, nhưng `SpawnManager` và `UpgradeManager` 2D vẫn được khởi chạy để điều khiển trận đấu.

### 3.4. Luồng quay về Sảnh (Return to Hub Navigation)
- **Từ Gameplay 2D:** `GameOverScreenPresenter` có nút "Chơi Tiếp" (gọi `StartRun()`) hoặc nút "Về Sảnh" (gọi `ReturnToMetaHub()` -> dọn quái/đạn 2D, reset Player về tọa độ đứng ở Sảnh, bật lại `Canvas_MetaMenu`).
- **Từ Defense 3D:** Trong `DefenseMatchPresenter.OnMainMenuClicked()`, code đang gọi cứng:
  ```csharp
  SceneManager.LoadScene(0, LoadSceneMode.Single);
  ```
  Lệnh này tải lại Build Index 0 (`SampleScene`), làm kích hoạt lại toàn bộ luồng All-in-One 2D cũ.

---

## 4. Danh mục Dependency cụ thể (Concrete Dependencies)

| Thành phần nguồn | Dependency hiện tại | Mục đích | Hướng xử lý |
|---|---|---|---|
| `MainHubPresenter` | `RunLoadoutState`, `CharacterDatabaseSO`, `MetaCurrencyManager`, `CharacterPreviewStage` | Hiển thị thông tin Sảnh | Giữ nguyên logic Presenter; thay đổi đích điều hướng khi bấm Xuất Trận |
| `WeaponLoadoutPresenter` | `RunLoadoutState`, `WeaponData` (Addressables), `MetaSceneTransitionController` | Trang bị vũ khí/pháp bảo | Đổi lời gọi `StartRun()` sang dịch vụ điều phối vào trận Defense3D |
| `StageSelectUIPresenter` | `WorldStageDatabaseSO`, `AddressablePatchManager`, `OnStageSelectedForBattle` | Chọn ải & tải DLC | Giữ cơ chế tải DLC/bản vá; thay đổi sự kiện xuất trận trỏ sang Defense3D |
| `MetaSceneTransitionController` | `MatchFlowOrchestrator`, `GameplayBootstrapper`, `SpawnManager`, `GameStateManager` | Chuyển cảnh All-in-One vào 2D | Tách riêng: không gọi `MatchFlowService` 2D, thay bằng chuyển Scene sang `MapSongQue3D_Prototype` |
| `--- GAME MANAGER ---` (SampleScene) | `GameStateManager`, 15 component con (2D Spawners, Pools, Bootstrapper) | Quản lý state và pool 2D | Hủy cơ chế `DontDestroyOnLoad` của cụm này; cô lập phạm vi chỉ trong 2D |
| `DefenseMatchPresenter` | `SceneManager.LoadScene(0)` | Nút Về Sảnh từ 3D | Thay thế Scene 0 bằng Scene Sảnh độc lập |
| `DefenseSceneCompositionRoot` | Authored scene bindings, `DefenseHeroKitSO`, `DefenseRun6003D` | Khởi tạo trận Defense 3D | Bổ sung cơ chế nhận Snapshot Loadout từ `RunLoadoutState` trước khi `session.Start()` |

---

## 5. Thứ tự các bước cần tách (Decoupling Sequence)

Để chuyển đổi sang Defense 3D an toàn mà **không làm gãy hệ thống hoặc hỏng Scene**, cần thực hiện theo đúng 5 bước sau:

```
[Bước 1: Cách ly Vòng đời Xuyên Scene]
       │
       ▼
[Bước 2: Xây dựng Bridge Loadout (Sảnh -> Defense3D)]
       │
       ▼
[Bước 3: Chuyển hướng Đường Xuất Trận sang Defense3D]
       │
       ▼
[Bước 4: Chuyển hướng Đường Về Sảnh từ Defense3D]
       │
       ▼
[Bước 5: Tách Sảnh thành Scene Độc lập & Vô hiệu hóa Provider 2D]
```

### Chi tiết từng bước:

#### Bước 1: Cách ly vòng đời xuyên Scene (Lifecycle & Persistence Isolation)
1. Xác nhận `CoreBootstrapper.cs` (tạo `--- APP CORE SERVICES ---` với `DontDestroyOnLoad`) là Single Source of Truth cho: `GameManager`, `MetaCurrencyManager`, `RelicInventoryManager`, `RelicGachaManager`, `CharacterProgressionManager`.
2. Gỡ bỏ `DontDestroyOnLoad` trên `GameStateManager` hoặc tháo các GameObject con của `--- GAME MANAGER ---` trong `SampleScene` để các hệ thống 2D (`GameplayBootstrapper`, `SpawnManager`, `EnemyPoolManager`, `ProjectileSystem`, các pool 2D) không bị sống xuyên sang scene khác khi chuyển Scene.
3. Bảo đảm `DefenseGameState` và `DefenseRunLifecycle` của Defense3D hoàn toàn độc lập với `GameStateManager` cũ.

#### Bước 2: Xây dựng Bridge nạp Loadout cho Defense 3D
1. Chuẩn hóa `RunLoadoutState`: Đảm bảo lưu trữ thông tin Tướng đã chọn, Pháp Bảo/Dép Tổ Ong, Lính làng và cấu hình Trụ.
2. Nâng cấp `DefenseSceneCompositionRoot.cs` để khi Scene `MapSongQue3D_Prototype` mở ra, nó đọc thông tin từ `RunLoadoutState` (với fallback mặc định an toàn nếu mở Scene trực tiếp trong Editor).

#### Bước 3: Chuyển hướng đường Xuất Trận sang Defense 3D
1. Thay đổi handler `OnStageSelectedForBattle` trong bộ điều phối chuyển cảnh:
   - Thay vì gọi `MatchFlowService.ExecuteCombatPreparationAsync(...)` (sinh Map Tilemap 2D + Player 2D + SpawnManager 2D),
   - Chuyển sang gọi nạp Scene `MapSongQue3D_Prototype` (bất đồng bộ qua `SceneManager.LoadSceneAsync`).
2. Thay đổi `WeaponLoadoutPresenter.HandleStartBattle()` để gọi chung cơ chế xuất trận mới sang Defense3D.
3. Giữ nguyên toàn bộ giao diện Sảnh, chọn tướng, chọn pháp bảo và tải DLC bản vá CDN.

#### Bước 4: Chuyển hướng đường Về Sảnh từ Defense 3D
1. Sửa `DefenseMatchPresenter.OnMainMenuClicked()`:
   - Thay vì gọi cứng `SceneManager.LoadScene(0)` (vốn đang tải lại nguyên khối All-in-One 2D),
   - Điều hướng về Scene Sảnh đã được tách biệt.
2. Đảm bảo trước khi thoát Scene 3D, gọi `session.Dispose()` và giải phóng đầy đủ các pool 3D tác chiến.

#### Bước 5: Tách Sảnh thành Scene độc lập & vô hiệu hóa Provider 2D
1. Đóng gói `Canvas_MetaMenu` cùng sân khấu `CharacterPreviewStage` thành một Scene Sảnh độc lập (ví dụ `Scene_MetaHub` hoặc Scene 0 mới).
2. Khi Scene Sảnh mới đã vận hành trơn tru và điều hướng 100% sang Defense 3D, toàn bộ các Provider gameplay 2D (`GameplayBootstrapper`, `SpawnManager`, `EnemyPoolManager`, `UpgradeManager`, `ProjectileSystem`, `Tilemap Addressables`, `ElementCycleManager`) sẽ trở thành dead code và có thể tiến hành đánh dấu / gỡ bỏ an toàn ở giai đoạn G6.
