# Kế hoạch chuyển gameplay sang Defense3D

Ngày lập: 28/10/2026. Trạng thái: đang thực hiện G0 — kiểm kê dependency; chưa triển khai gameplay hoặc xóa hệ thống.

Điểm đọc ở root: [MIGRATION_STATUS.md](../MIGRATION_STATUS.md). Tiến độ/bàn giao: [Migration/README.md](Migration/README.md). Manifest hệ thống: [Migration/SYSTEM_MANIFEST.md](Migration/SYSTEM_MANIFEST.md). Số liệu kiểm kê sinh tự động nằm trong `Docs/Migration/Generated`; không dùng chúng làm danh sách xóa tự động.

## 1. Mục tiêu và giới hạn

Defense3D là gameplay duy nhất. Tái sử dụng UI Vong Xuyên, tài nguyên phù hợp và dịch vụ nền; kết thúc việc duy trì runtime chiến đấu 2D sau khi thay thế dependency.

Luồng đích: Khởi động → Sảnh → Chuẩn bị đội hình → Nạp Defense3D → Trận → Kết quả → Lưu tiến trình → Sảnh hoặc chơi lại.

Loadout gồm đúng một tướng, đúng một pháp bảo, danh sách loại lính và danh sách loại trụ. Số loại lính/trụ do cấu hình giới hạn. Tạm hiểu đây là danh mục được phép tuyển/xây trong trận, không phải quân/trụ miễn phí được sinh sẵn. Giữ kinh tế gỗ, sức chứa và socket hiện tại.

Catalog đầu tiên chỉ gồm Thư Sinh, Dép Tổ Ong, Dân binh, Phu tạp dịch, Thầy lang và trụ hiện có của Defense3D. Không mở rộng bộ kỹ năng, ngũ hành, fusion, gacha hoặc multiplayer trong đợt chuyển đổi.

Không chạy Unity, Play Mode, Unity Test Runner hoặc build. Viết Unit Test; việc chạy và kiểm tra trực quan do người dùng thực hiện. Comment mới trong code viết bằng tiếng Việt.

## 2. Phát hiện đã xác nhận từ source

| Điểm kiểm tra | Hiện trạng | Hệ quả |
|---|---|---|
| `Features/Player/RunLoadoutState.cs` | Static, lưu CharacterEntry/WeaponData và lựa chọn cũ | Không dùng làm model gameplay mới |
| `Features/UI/WeaponLoadout/WeaponLoadoutPresenter.cs` | Đọc state cũ, nạp kho vũ khí và gọi MetaSceneTransitionController | Cần thay orchestration, có thể giữ tài nguyên và component UI |
| `Features/MatchFlow/MatchFlowService.cs` | Gọi GameplayBootstrapper, SpawnManager, UpgradeManager, GameStateManager; nạp map cũ | Không thể chỉ đổi tên scene để hoàn tất chuyển đổi |
| `Features/UI/MetaSceneTransitionController.cs` | StartRun gọi luồng combat hiện hữu | Cần nối sang application flow mới |
| `Features/Defense3D/UI/DefenseMatchPresenter.cs` | Về menu bằng LoadScene(0); đang sở hữu model EXP/upgrade | Tách điều hướng và dần đưa model về session |
| `ProjectSettings/EditorBuildSettings.asset` | SampleScene đứng trước scene Defense3D, cả hai được bật | Không xóa SampleScene trước khi tách phần sảnh/bootstrap cần giữ |
| `Features/Defense3D/Hero/DefenseHeroKitSO.cs` | Kit tướng chứa cả pháp bảo; validate bốn skill kind cụ thể | Cần tách relic và điều chỉnh binding/validation cùng lúc |
| `Features/Defense3D/Village/DefenseVillageShop3D.cs` | Catalog, slot và command dựa role/index, mảng order cố định | Tách ID nội dung khỏi role; không thay toàn bộ thuật toán lính trong cùng bước |
| `Features/Defense3D/Towers/DefenseTowerBuildPrototype3D.cs` | Trụ authored theo socket, giá xây ở builder | Cần contract cấu hình loại trụ, giữ bố trí socket |
| `Core/Save/GameManager.cs` | Lưu meta qua module, nhận kết quả run và phát tiền tệ | Tái sử dụng persistence, bổ sung tiếp nhận kết quả có chống trùng |
| `Features/MetaProgression/MetaProgressionSaveData.cs` | Đã có saveVersion; ID tướng cũ C001_ThuSinh | Cần mapping sang thu-sinh, không sửa ID âm thầm |
| `Core/Audio/PhaseAudioController.cs` | Theo SpawnManager/GameStateManager cũ | Giữ audio nền, thay nguồn sự kiện trận |
| `Features/Multiplayer/Core/Components/NetworkPlayerLoadout.cs` | Dùng WeaponManager và RunLoadoutState | Là dependency cũ cần xử lý, chưa chứng minh multiplayer hỗ trợ Defense3D |
| Defense3D | Đã dùng combat text và UI controls dùng chung | Không xóa toàn bộ UI/Shared theo tên thư mục |

Đây là kiểm tra source và cấu hình đã lưu, chưa xác nhận trạng thái runtime hoặc toàn bộ dependency asset/Addressables.

## 3. Quyết định theo nhóm hệ thống

| Nhóm | Quyết định | Điều kiện |
|---|---|---|
| Defense session/combat/AI/spawn/lính/trụ/gỗ/VFX pool | Giữ và phát triển | Chỉnh các điểm cấu hình cứng theo nhu cầu loadout |
| Asset provider/Addressables, local save, audio service | Giữ hạ tầng | Rà bootstrap, catalog và listener còn phụ thuộc gameplay cũ |
| UI card/slot/modal/button/controls, combat text | Giữ component phù hợp | Kiểm tra contract; không mặc định mọi View hiện hữu đều độc lập |
| Sảnh, loadout, chọn tướng, kho đồ, thông tin nhân vật | Cải tạo | Presenter mới, catalog mới, tái sử dụng style |
| Sở hữu, tiền tệ, cấp sao, mở khóa, kỷ lục | Giữ dữ liệu và cải tạo adapter | Chỉ áp dụng chỉ số đã có ý nghĩa trong Defense3D |
| Match flow, kết quả, HUD, audio theo phase | Cải tạo | Một nguồn trạng thái trận là DefenseSession |
| Player/enemy/combat/weapon/projectile/spawn/aim/pickup runtime 2D | Loại bỏ sau thay thế | Không còn caller, serialized reference, bootstrap hay build dependency |
| Pool/VFX cũ | Rà từng consumer | Giữ phần dùng chung còn cần; không hợp nhất pool chỉ để đổi tên |
| Ngũ hành/Âm Dương/fusion/đại lõi/gacha/multiplayer | Ngoài phạm vi | Ngừng nối vào luồng chính; không xóa save hoặc nội dung còn cần khi chưa quyết định sản phẩm |
| Art, âm thanh, shader, editor tool, test cũ | Rà riêng | Chỉ bỏ phần dành riêng cho nội dung đã nghỉ sử dụng |

## 4. Kiến trúc đích

### Lõi loadout

- `LoadoutSelection`: bản nháp giữ ID, không giữ GameObject hoặc WeaponData.
- `LoadoutRules`: số ô, bắt buộc, loại hợp lệ, quy tắc trùng lặp.
- `LoadoutValidator`: C# thuần, trả lỗi có mã; UI định dạng thông báo.
- `ILoadoutCatalog`: tra cứu danh tính, loại và tính khả dụng của nội dung.
- `ILoadoutRepository`: đọc/ghi DTO có phiên bản, implementation dùng persistence hiện hữu.
- `DefenseLoadoutResolver`: ánh xạ ID sang cấu hình Defense3D; tạo snapshot cố định cho trận.

Không tạo framework plugin hoặc một lớp base chứa mọi loại item. Chỉ tạo interface ở ranh giới lưu trữ, nội dung và điều hướng cần thay thế/kiểm thử. Core không phụ thuộc UnityEngine; dữ liệu authoring và hiển thị thuộc adapter Unity.

### Tích hợp trận

- Application flow sở hữu việc nạp scene, cancellation, lỗi nạp và trả về sảnh.
- Composition root nhận snapshot và bind đầy đủ trước khi session bắt đầu.
- Tướng sở hữu attack/dash/signature; relic là cấu hình và runtime riêng được chọn độc lập.
- Danh mục lính/trụ chọn trước trận giới hạn thao tác tuyển/xây; không tự cấp quân hoặc trụ.
- Vị trí map, số socket và sức chứa là cấu hình map; không lưu trong loadout.
- View phát thao tác, Presenter subscribe/unsubscribe và render theo event. Đồng hồ HUD cập nhật theo tick cần thiết, không polling dữ liệu loadout.
- Kết quả dùng định danh duy nhất cho mỗi lượt. Restart tạo lượt kết quả mới dù giữ cùng snapshot; callback từ generation cũ không được ghi thưởng.
- Gỗ và EXP là tài nguyên trong trận. Không tự chuyển gỗ sang tiền meta khi chưa có công thức thưởng được cấu hình.

### Save và tài nguyên

- Một nguồn lưu lựa chọn chính; PlayerPrefs cũ chỉ là đầu vào migration một lần.
- Giữ bản sao dữ liệu trước migration; không ghi đè save mới hơn phiên bản ứng dụng hiểu.
- Mapping ID tường minh; ID không hỗ trợ được giữ ở dữ liệu cũ và loại khỏi lựa chọn active với thông báo phù hợp.
- Catalog gameplay mới chỉ chứa nội dung Defense3D. Giữ handle asset đủ lâu cho snapshot/runtime và giải phóng sau teardown.
- Có cấu hình mặc định riêng cho mở trực tiếp scene Defense3D để người dùng kiểm tra trong Editor; không tự kích hoạt bootstrap 2D.

## 5. Lộ trình và tiêu chí nghiệm thu

### G0 — Kiểm kê dependency và chốt phạm vi

Đầu ra: manifest giữ/cải tạo/bỏ/theo dõi cho script, scene, prefab, ScriptableObject, editor tool, test, Addressables entry và build scene. Ghi rõ consumer và thứ tự thay thế; dùng GUID/fileID để rà reference serialized, không chỉ tìm tên class.

Nghiệm thu: nhận diện bootstrap nào sống xuyên scene, phần sảnh đang nằm trong SampleScene, dependency audio/save/meta và các asset được nạp bằng địa chỉ. Có baseline thay đổi hiện hữu để không ghi đè công việc đang làm. Manifest chưa phải bằng chứng xóa an toàn nếu chưa rà dynamic loading.

### G1 — Lõi loadout và Unit Test

Đầu ra: selection/rules/catalog contract/validator/repository contract/snapshot; không sửa scene hoặc combat.

Test: đúng một hero/relic; thiếu mục bắt buộc; sai loại; ID không biết; nội dung chưa sở hữu; trùng lựa chọn; min/max lính/trụ; đổi rule; snapshot không đổi khi draft đổi; không publish Changed khi thao tác bị từ chối.

Nghiệm thu: nghiệp vụ kiểm thử được bằng fake, không đọc singleton, PlayerPrefs, Resources hoặc Unity component.

### G2 — Catalog Defense3D và adapter runtime

Đầu ra: catalog/default loadout với nội dung hiện có; tách relic khỏi hero kit; cập nhật đồng bộ skill presenter/effect/HUD/validation; bind tuyển lính và xây trụ từ snapshot.

Ưu tiên giữ slot/instance đã author trong scene. Khi chưa có nhiều loại trụ, chỉ ánh xạ loại hiện hữu, không viết factory động cho nội dung chưa tồn tại. Mọi cấu hình phải được kiểm tra trước khi mở gameplay gate.

Test: ID resolve đúng definition; catalog trùng ID; kit thiếu skill/effect; relic không tương thích; loại lính/trụ chưa chọn bị từ chối; capacity/socket không bị thay đổi; bind thất bại không để lại session chạy dở; reset không giữ handle/lệnh cũ.

Nghiệm thu: composition root nhận snapshot hợp lệ; gameplay không đọc RunLoadoutState/WeaponManager; không sửa damage/cooldown/kinh tế ngoài cấu hình cần thiết.

### G3 — Save migration và meta adapter

Đầu ra: DTO loadout có phiên bản, repository và mapping ID; nối ownership/unlock hiện hữu sang catalog mới. Không mang thuật toán cộng chỉ số PlayerStats sang mặc định.

Test: lần cài mới; save cũ; thiếu trường; ID đã bỏ; dữ liệu hỏng; migration lặp lại; round-trip; lỗi ghi vẫn giữ dữ liệu trước; save phiên bản tương lai không bị ghi đè; snapshot chỉ nhận chỉ số meta được hỗ trợ.

Nghiệm thu: giữ tiền, sở hữu và tiến trình cũ; lựa chọn có default rõ ràng; không có hai writer tranh chấp lựa chọn trang bị.

### G4 — Luồng sảnh và UI chuẩn bị trận

Đầu ra: màn hình Vong Xuyên hiển thị tướng/pháp bảo/lính/trụ; presenter dùng core mới; application flow nạp scene Defense3D và truyền snapshot. Thay đường StartRun cũ. Tách bootstrap/sảnh khỏi gameplay 2D trong SampleScene trước khi chỉnh build entry.

Test: chọn/bỏ chọn theo rule; danh mục chỉ hiện nội dung hỗ trợ; nút xuất trận chỉ hợp lệ khi validate thành công; bấm liên tiếp chỉ nạp một lần; hủy hoặc lỗi nạp giải phóng resource và trở lại trạng thái cho phép thử lại; unsubscribe khi đóng màn hình.

Nghiệm thu: không khởi tạo GameplayBootstrapper/SpawnManager/UpgradeManager 2D trên đường vào trận; không điều hướng bằng build index; mở trực tiếp scene vẫn có default hợp lệ.

### G5 — Kết quả, trở về sảnh và hoàn thiện vòng đời

Đầu ra: result service ghi kết quả/thưởng qua save/meta; thay LoadScene(0) bằng navigation contract; audio theo phase DefenseSession; model EXP/upgrade được đưa về owner nghiệp vụ phù hợp thay vì View/Presenter.

Test: thắng/thua; callback kết quả trùng; callback lượt cũ; restart; về sảnh; save thất bại; tải lại sau hoàn tất; dispose lặp; observer được gỡ; reward không cấp hai lần. Xác định rõ hỗ trợ chống trùng trong phiên hay bền qua khởi động lại và thiết kế persistence tương ứng.

Nghiệm thu: chạy lại không giữ pool lease, subscription, target registry hoặc lệnh của lượt trước. UI kết quả không sở hữu việc tính thưởng. Chính sách thưởng và bỏ trận có cấu hình rõ ràng.

### G6 — Ngắt và loại bỏ gameplay 2D

Thực hiện theo batch nhỏ: bootstrap/match flow cũ → HUD/input/player cũ → combat/weapon/projectile cũ → enemy/spawn/pickup cũ → scene/prefab/data/editor/test chỉ còn phục vụ các nhóm đó.

Thứ tự thực tế phải theo manifest dependency: gỡ consumer trước provider, thay bridge còn dùng trước khi xóa. Multiplayer cũ và meta/editor tool có thể vẫn tham chiếu WeaponData/WeaponManager; không bỏ qua vì không bật trong scene.

Mỗi batch: rà source + YAML + Addressables + dynamic loading; cập nhật manifest; kiểm tra compile có thể thực hiện ngoài Unity; viết test cần thiết; để người dùng kiểm tra Unity trước batch xóa tiếp theo. Giữ .meta/GUID cho tài nguyên được giữ hoặc di chuyển.

Nghiệm thu: không còn bootstrap/caller/serialized reference/dynamic entry đến runtime 2D đã bỏ; build chỉ gồm scene và catalog cần dùng. Không xóa sạch Shared/UI/Weapons theo thư mục. Không di chuyển hàng loạt asset chỉ để làm đẹp cấu trúc trong cùng batch.

### G7 — Chốt bản chuyển đổi

Đầu ra: cập nhật tài liệu cấu trúc, xóa compatibility bridge hết consumer, ghi các lỗi còn lại và đối chiếu manifest.

Kiểm tra của người dùng: cold start, offline, save cũ/mới, sảnh, đổi đội hình, xuất trận, dash/skill/pháp bảo, tuyển lính/xây trụ, pause/downed/respawn, thắng/thua, restart, về sảnh, vào trận lần hai và Android. Shader, scene import, Addressables build và hiệu năng native chỉ được xác nhận sau các bước người dùng chạy.

Nghiệm thu cuối: một gameplay flow Defense3D; không giữ runtime 2D chỉ để phục vụ startup/menu; dữ liệu cần bảo toàn còn nguyên; các test và xác nhận thủ công được ghi riêng, không đánh dấu test đã pass chỉ vì compile thành công.

## 6. Thứ tự và quản lý phạm vi

### Hạng mục bắt buộc: shader và asset gameplay 2D cũ

Phạm vi dọn gồm scene, prefab, material, shader/ShaderGraph/subgraph/HLSL, texture/sprite/atlas, animation/Animator, audio, ScriptableObject, tilemap/palette, VFX, editor generator và Addressables entry của gameplay đã bỏ. Không coi tài nguyên có tên 2D hoặc dùng SpriteRenderer là tài nguyên phải xóa: Defense3D và UI vẫn có thể cần chúng.

**Kết quả rà ban đầu:** Assets/Shaders có shader sprite outline, sprite hit/status, các shader slash/ink/ground decal/distortion/sonic wave và nhóm Defense*. Có cả Assets/Art, Assets/_Art, Assets/VFX, Assets/Art/VFX, Assets/Resources cùng các bản UI ở nhiều vị trí. Đây là vùng cần đối chiếu, chưa phải bằng chứng trùng hoặc bỏ được. DefenseCombatVfxInstaller đang tìm CombatLine/CombatSlash bằng Shader.Find; các generator khác còn tìm shader URP/Lit. Không xóa shader chỉ vì không thấy material tham chiếu trực tiếp.

**Manifest asset bổ sung cho G0:** path, GUID, loại asset, mục đích, root giữ lại, incoming reference, địa chỉ/label/group, dynamic key hoặc Shader.Find, generator tạo/ghi đè, trạng thái giữ/cải tạo/chờ/xóa, lý do và batch. Quét toàn bộ Assets để lấy shader ở ngoài Assets/Shaders; inventory Packages/Plugins/Photon chỉ ghi dependency, không sửa vendor trong batch dọn nội dung.

**Cách xác định tài nguyên còn dùng:**

1. Chốt root giữ lại: bootstrap/sảnh, scene Defense3D, catalog/default loadout, UI dùng chung, settings render, Addressables local/remote còn hỗ trợ và network prefab registration nếu giữ Photon.
2. Tìm dependency bắc cầu qua scene → prefab/SO/Animator/atlas → material → shader/include/texture. Rà thêm shader/material trong GraphicsSettings, QualitySettings, URP asset/renderer feature, Always Included Shaders và ShaderVariantCollection.
3. Rà reference bằng GUID/fileID lẫn string: Resources key, Addressables address/label, Shader.Find, editor generator, JSON/data ngoài YAML và script tự nạp asset. Folder entry/atlas và asset serialization dạng binary cần bước kiểm tra Editor do người dùng chạy; không kết luận orphan từ grep đơn thuần.
4. Chỉ đánh dấu ứng viên xóa khi không có đường dùng từ root giữ lại và không có consumer động còn sống. Tài nguyên dùng chung bởi root cũ/mới giữ lại, không xóa theo thư mục nguồn.

**Phân loại shader:**

- DefenseAimOverlay, DefenseCombatLine, DefenseCombatSlash, DefenseHitSpark, DefenseScholarSeal: ưu tiên giữ; xác nhận material/prefab và generator đang dùng trước khi thay đổi.
- Sprite outline/hit/status và shader VFX có thể phù hợp billboard/sprite trong 3D: giữ nếu có consumer hoặc mục tiêu tái sử dụng cụ thể; không thay hết bằng shader Lit.
- Shader chỉ phục vụ material/prefab đã bỏ: xóa sau consumer, kèm meta; rà include/subgraph dùng chung trước.
- Không dọn keyword/variant bằng phỏng đoán. Người dùng kiểm chứng build/stripping trên Android; Shader.Find trong Editor không chứng minh shader có trong player build.

**Xử lý trùng và di chuyển:** so sánh nội dung/import settings/sprite border/pivot/atlas/material reference, không chỉ tên hoặc hash ảnh. Chọn bản chuẩn; cập nhật mọi consumer rồi mới bỏ bản dư. Giữ nguyên .meta/GUID khi di chuyển tài nguyên được giữ. Khi chuyển asset ra Resources, thay mọi key nạp cũ trước; không tạo bản copy Resources để chữa mất reference. Cập nhật generator để lần chạy tiếp theo không sinh lại asset đã nghỉ hoặc ghi đè bản chuẩn.

**Batch G6:** (1) scene/prefab/data 2D đã hết consumer; (2) animation/tilemap/VFX chỉ dành cho chúng; (3) material rồi shader/include/texture không còn dùng; (4) asset trùng đã remap; (5) catalog/group/label và generator/test cũ. Thứ tự thực tế theo dependency graph, không mặc định texture/shader cuối cùng luôn độc lập. Một thay đổi gồm asset và meta tương ứng; không đụng save người dùng, catalog CDN đã phát hành hoặc package vendor ngoài phạm vi.

**Nghiệm thu:** manifest có lý do cho từng asset xóa; không có missing GUID trong phạm vi quét; không còn dynamic key/generator trỏ asset đã bỏ; Addressables/build scene không kéo gameplay 2D; asset chung giữ GUID. Viết Unit Test/editor validation cho catalog, GUID/address, material/shader và root dependency quan trọng; không snapshot danh sách asset để biến mọi thay đổi hợp lệ thành lỗi. Người dùng chạy import/build, Build Layout/Analyze, kiểm tra shader hồng, UI/atlas/animation/VFX và Android trước batch tiếp theo. Kiểm tra text/compile không thay thế xác nhận shader/material native.

**Đầu ra cần có trước khi xóa:** danh sách đề xuất cụ thể theo batch, số lượng/dung lượng file nguồn và root còn phụ thuộc. Dung lượng source không phải mức giảm APK; chỉ báo mức giảm build sau khi có kết quả build thực tế. Git diff dùng để review/khôi phục, không chỉ chuyển toàn bộ asset cũ sang một folder Archive trong Assets vì chúng vẫn có thể bị import hoặc đóng gói.

### Hạng mục theo dõi: multiplayer Photon

Source hiện dùng Fusion (`NetworkRunner`, `NetworkBehaviour`), có PhotonFusionSessionService, lobby sync, input collector, player spawner và các component đồng bộ. Chưa xác nhận phiên bản SDK thực tế hoặc vận hành nhiều máy; không coi multiplayer hiện hữu đã hỗ trợ Defense3D.

**Giữ/cải tạo:** contract session/lobby/lifecycle, mock, UI phòng và protocol có thể tái sử dụng sau audit. PhotonFusionSessionService giữ phần kết nối/runner nhưng phải tách side effect gameplay, teardown và đường trả về solo cũ. NetworkLobbySync đang đọc RunLoadoutState để lấy tướng; chuyển sang snapshot/catalog mới. Protocol phải version hóa khi đổi schema, không giữ compatibility bằng cách gửi toàn bộ WeaponData.

**Thay adapter gameplay:** NetworkPlayerMovement phụ thuộc PlayerController/PlayerInputReader; command dash gọi PerformDash cũ; NetworkPlayerLoadout phụ thuộc WeaponManager; NetworkEnemySync phụ thuộc Enemy/HealthSystem/Rigidbody2D; spawner/provider lấy prefab từ dữ liệu tướng cũ. Không gắn nguyên các script này lên hero/enemy Defense3D. InputData có move/aim/buttons nhưng cần contract tọa độ mặt phẳng XZ, aim target, sequence và thao tác tuyển/xây riêng; tái sử dụng ý tưởng, không mặc định schema hiện tại đủ.

**Thiết kế đề xuất cho mốc co-op sau migration solo:**

- Domain Defense3D không phụ thuộc Fusion. Adapter network nhận input/lệnh và chuyển tới gameplay; adapter presentation đọc state/event được chấp nhận.
- Host/state authority quyết định damage, cooldown, HP, AI, spawn, clock, phase trận, base, gỗ, tuyển lính và socket trụ. Client gửi ý định, không gửi damage/gỗ đã tự tính để host áp dụng trực tiếp.
- Một actor không chạy đồng thời local FixedUpdate movement và network simulation. Chốt integration tick/physics trước khi làm prediction; không tuyên bố gameplay NavMesh/Rigidbody hiện tại có thể rollback chỉ bằng đổi hàm Update.
- Mỗi player có actor/commands/loadout riêng. Hiện Defense3D có nhiều reference hero đơn nên phải audit shop, heal target, camera, targeting, respawn và HUD. Quyết định ví gỗ chung/riêng, quyền điều khiển lính và quyền xây trước khi triển khai.
- Host xử lý cạnh tranh tuyển/xây nguyên tử; ID/sequence chống lặp, xác thực sender, phase, khoảng cách, cooldown và tài nguyên. State bền (HP, phase, socket, roster) tách event VFX nhất thời; late join không phát lại toàn bộ hiệu ứng cũ.
- Mỗi máy dùng pool VFX cục bộ; entity mạng dùng lifecycle spawn/despawn có ownership riêng. Không đưa network object vào pool cũ rồi chỉ SetActive để thay thao tác despawn. Asset lease của network prefab sống đến khi runner/provider không còn cần.
- Trước start, trao đổi stage ID, loadout ID, phiên bản schema/content; mỗi máy nạp nội dung và báo ready. Một nguồn điều phối scene, không cho navigation solo và runner đồng thời tải scene.
- Không dùng Time.timeScale của client để dừng cả trận. Chốt level-up/pause/respawn/disconnect cho co-op. Mốc đầu đề xuất đóng trận khi host rời; reconnect, late join và host migration là phạm vi riêng, không tự hứa hỗ trợ.
- Kết quả/ghi meta được chống trùng theo match/player; không xem host máy người chơi là cơ chế chống gian lận cho kinh tế có giá trị bên ngoài.

**Thứ tự:** G0 ghi consumer Photon trước khi xóa 2D; giữ phần room/service cần thiết, ngắt đường start gameplay 2D và UI vào co-op chưa hỗ trợ. Hoàn tất G1–G5 solo, sau đó triển khai co-op thành mốc riêng: room/loadout readiness → hai player movement → combat/AI/base → gỗ/lính/trụ → kết quả/disconnect. Chỉ xóa adapter multiplayer cũ khi đã gỡ cả caller, prefab, scene và SDK prefab registration tương ứng. Không xóa SDK Photon trong đợt bỏ gameplay 2D nếu còn giữ mục tiêu co-op.

**Unit Test:** fake session/transport, authority routing, sender sai actor, lệnh trùng/cũ, cạnh tranh cùng socket, thiếu gỗ, content mismatch, readiness barrier, timeout nạp, disconnect giữa load, result trùng và VFX dedup. Người dùng kiểm chứng network runtime trên ít nhất hai instance/thiết bị với latency/loss; không chạy Unity trong quá trình viết code/test theo yêu cầu hiện tại.

### Hạng mục xuyên suốt: Addressables

Giữ Addressables làm hạ tầng nội dung. Cải tạo service và quyền sở hữu tài nguyên trước khi đưa luồng loadout/scene mới vào production; không xóa hoặc xây lại toàn bộ hệ thống tải tài nguyên.

**Hiện trạng đã đọc từ source/config:**

- `IAssetProvider/AddressableAssetManager` và `IGameDataService/GameDataService` đều có cache, in-flight task và handle riêng. Chưa có một owner thống nhất cho cùng nội dung.
- `AddressableAssetManager` lưu cache theo address, chưa tách/kiểm tra contract kiểu dữ liệu nhất quán. Load theo label có vòng đời/ref-count khác load đơn.
- Request tạo in-flight đầu tiên hủy có thể release handle và cancel task mà các waiter khác vẫn cần. Waiter hủy chưa có cơ chế chấm dứt chờ độc lập.
- `Cleanup()` xóa dictionary nhưng không vô hiệu hóa tác vụ đang await; completion cũ có thể đưa handle trở lại cache hoặc can thiệp request mới cùng key.
- Handle tạo trong nhánh load thất bại/exception cần được kiểm toán đường release. Không suy ra service an toàn chỉ từ comment reference counting.
- `InstantiateAsync` hiện load prefab rồi Object.Instantiate, chưa trả một instance ownership token. Cần quy định giữ prefab đến khi mọi instance/pool bị hủy và sau đó release asset; không gọi ReleaseInstance như thể instance này được Addressables tạo.
- `GameDataService` vẫn fallback Resources và Editor AssetDatabase; có thể che thiếu entry hoặc sai key. `AddressableAssetManager` cũng có fallback trong môi trường development.
- Có Resources.Load trong DefenseEnemyFactory3D, DefenseEnemyPrefab3D, DefenseWoodDrop3D và khởi tạo pool wood của DefenseRun6003D. Cần phân biệt đường legacy với production consumer trước khi thay.
- Group hiện mang cấu trúc nội dung cũ: Core_Preload, Weapons_Tier1, Enemies_Stage1, DLC_Upgrades… Rà GUID entry trực tiếp trong bốn vùng Defense3D và định nghĩa lính chưa thấy entry trùng; chưa kết luận thiếu dependency vì folder entry hoặc reference gián tiếp vẫn có thể đưa asset vào build.
- Patch manager có URL transform Firebase cụ thể. Giữ chức năng nếu còn dùng CDN, nhưng tách endpoint/platform thành cấu hình và kiểm tra catalog compatibility với phiên bản code.

**Thiết kế vừa đủ:**

1. Một backend Addressables sở hữu handle. GameDataService trở thành facade tra cứu dữ liệu hoặc được thay dần; không duy trì cache handle độc lập thứ hai. Giữ adapter tương thích trong giai đoạn chuyển caller.
2. Acquire trả lease dùng Dispose lặp an toàn, xác định đúng lần acquire. Scope ứng dụng/sảnh/trận sở hữu lease, không cho consumer tự Cleanup toàn cục khi đổi scene.
3. Nhiều waiter chia sẻ load vật lý; cancellation thuộc từng waiter. Completion lỗi/hủy không làm mất tài nguyên của owner khác. Generation bảo vệ completion sau teardown; handle cũ phải release đúng một lần.
4. Một scene loader riêng quản lý load/unload scene và rollback; instance/pool có owner riêng. Không gom scene, pool, patch, game data vào một manager mới.
5. Pool giữ prefab lease đến khi đóng pool, hủy cả active/idle object và hoàn tất hủy trì hoãn cần thiết; sau đó mới release tài nguyên. Restart có thể giữ scope trận khi chủ ý tái sử dụng; về sảnh đóng scope trận, giữ scope ứng dụng/sảnh.
6. Stable content ID trong save ánh xạ qua catalog sang asset reference/address. Không dùng đường dẫn file làm ID tiến trình. Chỉ load runtime đã chọn và nội dung stage; metadata cho UI nhẹ, không kéo toàn bộ prefab dependency qua catalog preview.
7. Stage đầu và tài nguyên bắt buộc có trong nội dung local của bản cài, chạy offline. Remote chỉ cho nội dung tùy chọn; cập nhật catalog ở điểm an toàn trước khi tạo scope trận, không đổi dưới pool đang dùng. Download cache và handle RAM là hai vòng đời khác nhau.
8. Fallback chỉ giữ tạm qua compatibility adapter có cảnh báo rõ. Trước nghiệm thu Android phải có đường kiểm tra strict không dùng Resources/AssetDatabase để bù cấu hình thiếu. Không chuyển asset vào Resources để chữa lỗi build.

**Group đề xuất, chỉ tạo khi có consumer:**

| Group | Vòng đời/nội dung |
|---|---|
| Core_Local | Cấu hình khởi động và metadata tối thiểu |
| UI_Meta_Local | Sảnh, loadout, icon dùng chung |
| Defense_Stage01_Local | Scene/map và nội dung riêng stage đầu |
| Defense_Gameplay_Local | Tướng, pháp bảo, lính, trụ hiện hữu cần dùng lại |
| VFX_Shared_Local | VFX/material/texture thực sự dùng chung khi cần tách dependency |
| Audio_Shared_Local | Audio sảnh/trận dùng chung theo nhu cầu |

Đây là ranh giới ownership và đóng gói đề xuất, không bắt buộc mỗi loại item một group. Label phục vụ tra cứu/preload; không dùng label như quyền sở hữu hoặc căn cứ đảm bảo bundle được tách. Chốt packing sau khi người dùng chạy Build Layout/Analyze để kiểm tra duplicate, bundle dependency và kích thước. Scene lớn có thể kéo theo direct reference nặng dù entry được chia group.

**Thứ tự gắn vào lộ trình:**

- G0: xuất manifest address/GUID/group/label/consumer/load path, kiểm tra folder entry, built-in scene/Resources dependency và schema IncludeInBuild. Không coi spec cũ là cấu hình thực tế.
- Sau G1, trước G2: sửa lease/scope, cancellation, concurrency, failure release và teardown; bổ sung fake backend cho Unit Test. Không yêu cầu migration tất cả caller cùng lúc.
- G2–G4: chuyển asset runtime và scene load mới qua backend, bỏ production Resources.Load theo consumer; di chuyển asset giữ GUID, cập nhật Addressables entry và direct reference đồng bộ. Chọn một cơ chế phân phối scene gameplay chính, tránh cùng scene trong built-in player lẫn bundle ngoài ý muốn.
- G5: kiểm tra release theo owner khi restart/về sảnh/nạp lỗi; catalog update không chạy trong trận.
- G6: loại entry, bundle input, label và tool chỉ phục vụ 2D sau audit; không xóa nội dung CDN đã phát hành nếu client đang dùng vẫn cần.
- G7: người dùng chạy Addressables build/Analyze/Build Layout và Android offline, đối chiếu bộ nhớ qua nhiều lượt. Chưa chạy hoặc xác nhận các bước này trong lượt lập kế hoạch.

**Unit Test bắt buộc với fake backend:** acquire đồng thời cùng key; hai scope cùng dùng asset; dispose một scope vẫn giữ scope kia; type mismatch; một waiter hủy; tất cả waiter hủy; lỗi rồi retry; dispose lặp; release khi load đang chạy; completion cũ sau cleanup và request mới; label/asset cùng dùng dependency; pool chưa đóng thì prefab còn sống; scene load thất bại rollback; catalog update không được xen giữa trận. Fake test chứng minh logic ownership, không thay thế kiểm chứng bundle/native Addressables trong Unity.

G0 → G1 → G2 → G3 → G4 → G5 → G6 → G7. G6 chỉ bắt đầu sau khi vòng chơi mới được người dùng xác nhận. Trong từng giai đoạn có thể thay component nhỏ, nhưng không tranh thủ viết lại toàn bộ hệ thống Defense3D.

Các giả định cần xác nhận trước phần triển khai phụ thuộc: số loại lính/trụ được chọn mặc định; bắt buộc chọn ít nhất một loại hay cho phép rỗng; công thức thưởng meta; chính sách bỏ trận; tiến trình cũ nào được áp dụng vào chỉ số 3D. Trước mắt dùng cấu hình cho các giới hạn, giữ lựa chọn mặc định tương đương nội dung hiện hữu, không tự thêm thưởng hoặc chuyển chỉ số cũ.

Không ước lượng thời gian xóa hoặc số file chắc chắn an toàn trước G0. Bước triển khai đầu tiên là manifest dependency và lõi loadout với Unit Test; chưa bắt đầu bằng việc xóa thư mục.
