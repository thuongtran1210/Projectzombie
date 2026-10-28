# Manifest hệ thống — bản kiểm kê đầu

Cập nhật 28/10/2026. Mọi quyết định dưới đây ở cấp hệ thống, **chưa cấp quyền xóa từng file**. Root bằng chứng là source/config trong workspace; không giả định tài liệu cũ phản ánh runtime hiện tại.

## Root sử dụng và ranh giới sở hữu

| Root | Vai trò | Trạng thái |
|---|---|---|
| Assets/Scenes/SampleScene.unity | Entry build index 0; có luồng gameplay/sảnh cũ cần tách | Giữ tạm, rà owner và bootstrap |
| Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity | Scene gameplay đích, composition root và runtime roots | Giữ; consumer chính cho audit |
| Assets/Features/Defense3D | Domain/runtime/presentation đích | Giữ/cải tạo; có đường Resources legacy bên trong |
| Assets/Resources/Defense3D | Asset/config hiện hữu | Giữ GUID; chuyển ownership/load qua Addressables sau audit |
| Assets/Core và Assets/Features/Startup | Dịch vụ xuyên scene và khởi động | Rà vòng đời và dependency; không xóa nguyên thư mục |
| Assets/Features/UI, MetaProgression | UI và tiến trình có giá trị | Tách model/consumer 2D trước khi bỏ provider |
| Assets/Features/Multiplayer, Assets/Photon | Service phòng và SDK mạng | Giữ theo hướng co-op; adapter gameplay cũ cần thay |
| ProjectSettings, AddressableAssetsData | Root build/render/content/network liên quan | Rà cả GUID, folder entry và string key |

## Quyết định và thứ tự thay thế

| Hệ thống / source chính | Quyết định | Dependency cần xử lý | Giai đoạn / điều kiện |
|---|---|---|---|
| DefenseSession, DefenseSceneCompositionRoot, DefenseRuntimeRoots3D | Giữ/cải tạo | Nhận snapshot loadout, bind trước gameplay gate | G2; giữ baseline combat |
| DefenseHeroKitSO và skill presenter | Cải tạo | Tách relic khỏi bộ tướng, sửa validation bốn kind cùng lúc | G2; test resolve/binding |
| DefenseVillageShop3D / DefenseMinionDefinitionSO | Cải tạo | Role/index, slot authored, hero đơn, wallet | G2; lọc roster theo ID, giữ capacity |
| DefenseTowerBuildPrototype3D / socket towers | Cải tạo | Trụ authored, giá builder, wallet | G2; giữ socket và kinh tế |
| RunLoadoutState | Thay thế rồi bỏ runtime | UI/meta, GameplayBootstrapper, WeaponManager, Photon | G1–G4; giữ reader migration nếu cần |
| WeaponLoadoutPresenter/View, CharacterSelection, MainHub | Cải tạo | CharacterEntry/WeaponData, static state, luồng StartRun | G4; UI dùng model mới |
| MatchFlowService / MetaSceneTransitionController | Thay đường gameplay | SpawnManager, UpgradeManager, GameplayBootstrapper, map Tilemap | G4; entry vào Defense3D độc lập |
| DefenseMatchPresenter | Cải tạo | LoadScene(0), model EXP/upgrade trong presenter | G5; navigation + owner nghiệp vụ |
| SaveSystem, LocalJsonSaveProvider, ISaveableModule, GameManager | Giữ nền/cải tạo kết quả | Save version, ID tướng, cấp tiền chống trùng | G3/G5; không mất tiến trình |
| MetaCurrency/CharacterProgression/RelicInventory | Giữ dữ liệu/cải tạo adapter | GameManager, catalog cũ, chỉ số PlayerStats | G3; chỉ áp dụng chỉ số hỗ trợ |
| AddressableAssetManager / GameDataService | Hợp nhất ownership | Hai cache handle, cancellation, completion sau Cleanup | Sau G1/trước G2; lease/scope tests |
| AddressablePatchManager / Startup | Giữ/cải tạo | URL transform CDN, offline, điểm cập nhật catalog | G3–G5; không update dưới pool active |
| PhaseAudioController | Giữ audio, thay event source | SpawnManager/GameStateManager | G5 trước xóa spawn/state cũ |
| UI controls / UniversalItemSlot / DamageText | Giữ phù hợp | Model visual enums, adapter, lifecycle subscription | G2–G4; không xóa theo chữ 2D |
| Camera/juice/hit feedback | Rà từng component | Camera 2D, target PlayerStats/PlayerController | G0 rồi G4/G5; chưa kết luận cả thư mục |
| PlayerController/GameplayBootstrapper/PlayerStats | Runtime cũ chờ bỏ | Meta applicator, HUD/input, Photon, save consumer | G6 sau gỡ mọi consumer cần giữ |
| WeaponManager/WeaponData và weapon effect 2D | Runtime cũ chờ bỏ; data rà riêng | Loadout/meta/gacha generator/Photon | G6; không xóa data trước mapping |
| Enemy/HealthSystem/SpawnManager cũ | Runtime cũ chờ bỏ | MatchFlow/audio/network/damage consumer | G6 sau đường start/consumer được thay |
| Combat/aim/projectile/pickup 2D | Rà và bỏ phần chỉ dùng 2D | Shared interfaces, registry, tests, prefab | G6 theo provider/consumer, không xóa Shared |
| PhotonFusionSessionService/lobby/contracts/mock | Giữ/cải tạo | Lobby đọc RunLoadoutState, scene/lifecycle cũ | G0 bảo toàn; co-op là mốc riêng |
| NetworkPlayer*/NetworkEnemySync/action commands | Adapter cũ cần thay/gỡ | PlayerController, WeaponManager, Enemy, Rigidbody2D | Rà prefab registration trước xóa |
| Ngũ hành/Âm Dương/fusion/đại lõi/gacha | Ngoài phạm vi tích hợp mới | Save/meta/editor tool có thể còn dùng | Không tự xóa save hay đưa vào core mới |
| Shader Defense*, sprite/VFX/UI materials | Giữ hoặc rà consumer | Shader.Find, material, settings, variants, includes | G0/G6; không kết luận bằng tên |
| Texture/atlas/animation/tilemap/VFX/audio cũ | Rà từng asset | Root giữ lại và dependency bắc cầu | G6; binary/native audit còn chờ |
| Editor generator và test cũ | Giữ phần có consumer/cải tạo/bỏ | Có thể sinh lại asset nghỉ hoặc tham chiếu type đã bỏ | Cùng batch provider, không chạy tool Unity |

## Những điểm chặn việc xóa

- Đã giải quyết: Đã hoàn thiện bản đồ owner của SampleScene, Sảnh Menu (MainHubUI prefab instance), phân tích chi tiết các thành phần sống xuyên scene và 3 đường xuất trận gọi 2D (xem chi tiết tại `SCENE_ROOT_AUDIT.md`).
- Chưa phân giải semantic toàn bộ source; CSV symbol chỉ là vị trí để đọc tiếp.
- Chưa mở rộng hiệu lực Addressables folder entry hoặc kiểm tra asset binary/SDK prefab registry.
- Chưa kiểm tra Graphics/Quality/URP/variant dependency đầy đủ cho từng shader ứng viên.
- Chưa có xác nhận người dùng chạy vòng chơi mới; G6 chưa đủ điều kiện bắt đầu.

Các điểm trên không chặn G1 viết core loadout, nhưng phải có người tiếp tục rà G0 và ghi giới hạn chính xác. Không tự tạo số file “xóa an toàn” từ incoming_sources=0 hoặc conservative_root_reachable=False.

