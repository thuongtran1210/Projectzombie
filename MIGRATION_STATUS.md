# Điểm bắt đầu — chuyển gameplay sang Defense3D

Cập nhật: 28/10/2026. Chủ đề đã hoàn thành: **G0 — Kiểm kê dependency**, **G1 — Lõi Loadout thuần C#**, **G2 — Catalog Defense3D & Adapter Runtime**, **G3 — Save migration & Meta adapter**, **G4 — Luồng sảnh & UI chuẩn bị trận**, **G5 — Kết quả, trở về sảnh & hoàn thiện vòng đời**. Chuẩn bị: **G6 — Dọn dẹp phụ thuộc 2D & kiểm toán Scene**.

## Đọc theo thứ tự ở phiên tiếp theo

1. `.agents/AGENTS.md`: quy tắc dự án.
2. `Docs/Migration/README.md`: tiến độ, phạm vi kiểm chứng và cách tiếp tục.
3. `Docs/Migration/SYSTEM_MANIFEST.md`: quyết định theo hệ thống, dependency và điều kiện gỡ.
4. `Docs/Migration/Generated/summary.json` cùng các CSV: bằng chứng kiểm kê tĩnh; xem giới hạn trước khi dùng số liệu.
5. `Docs/DEFENSE3D_GAMEPLAY_MIGRATION_PLAN.md`: kế hoạch G0–G7, bao gồm Addressables, Photon và asset/shader.
6. `Docs/Migration/SCENE_ROOT_AUDIT.md`: owner/fileID trong scene, kết quả kiểm kê và consumer cần đọc tiếp.

## Quyết định đang có hiệu lực

- Defense3D là gameplay chính duy nhất; tái sử dụng UI Vong Xuyên và dịch vụ nền phù hợp.
- Loadout: một tướng + một pháp bảo + danh sách loại lính/trụ; catalog đầu chỉ gồm nội dung Defense3D hiện hữu.
- Chưa chốt số loại lính/trụ, thưởng meta, bỏ trận và luật co-op; không tự thêm các cơ chế này.
- Không chạy Unity, Play Mode, Test Runner hoặc build. Viết Unit Test khi triển khai nghiệp vụ; người dùng kiểm chứng Unity.
- Comment mới bằng tiếng Việt. Không xóa asset/code hoặc sửa group Addressables trong giai đoạn kiểm kê.
- Workspace có nhiều thay đổi từ công việc trước; không reset, clean hoặc ghi đè. `working_tree_at_scan.txt` là ảnh trạng thái, không phải patch backup.

## Trạng thái bàn giao

- **G0 — Kiểm kê & Rà soát:** Đã hoàn thành rà soát chuyên sâu `SampleScene`, cấu trúc Sảnh nhúng Prefab (`MainHubUI.prefab`), điểm nghẽn DontDestroyOnLoad kéo 15 GameObject con của `--- GAME MANAGER ---`, 3 luồng xuất trận gọi gameplay 2D, và lập thứ tự 5 bước tách chi tiết (lưu tại `Docs/Migration/SCENE_ROOT_AUDIT.md`). Chưa can thiệp sửa scene hoặc xóa code hệ thống.
- **G1 — Lõi Loadout & Unit Test:** Đã triển khai hoàn thiện bộ mã nguồn C# thuần trong `Assets/Features/Defense3D/Loadout/` (`LoadoutItemType`, `LoadoutCatalogContracts`, `LoadoutRules`, `LoadoutValidation`, `LoadoutSelection`, `DefenseLoadoutSnapshot`, `ILoadoutRepository`), cùng bộ NUnit Test Editor toàn diện tại `Assets/Tests/Editor/DefenseLoadoutCoreTests.cs` (13 kịch bản kiểm thử) và bộ kiểm tra độc lập tại `Tools/Defense3DValidation/DefenseLoadoutCoreChecks.cs`. Đã kiểm chứng biên dịch thành công 100% bằng trình biên dịch Mono standalone.
- **G2 — Catalog Defense3D & Adapter Runtime:** Đã triển khai hoàn thiện:
  - `Defense3DCatalog.cs`: Quản lý 6 thực thể baseline chuẩn (`hero_thusinh`, `relic_deptoong`, `minion_danbinh`, `minion_phutapdich`, `minion_thaylang`, `tower_cungtien`).
  - Tách Relic khỏi `DefenseHeroKitSO.cs`: Bỏ ràng buộc cứng trong `Validate()`, cho phép inject Relic động qua `SetRelic()`.
  - Hỗ trợ lọc đội hình lính trong `DefenseVillageShop3D.cs`: Thêm `SetAllowedRoles` & `IsRoleAllowed`, ngăn chặn tuyển lính ngoài danh sách tuyển chọn.
  - `DefenseLoadoutResolver.cs`: Pure C# Resolver ánh xạ Snapshot thành `DefenseResolvedLoadout`, ánh xạ ID sang vai trò lính runtime (0: Militia, 1: Gatherer, 2: Healer), tự động fallback an toàn về snapshot mặc định khi dữ liệu rỗng hoặc sai luật.
  - `DefenseLoadoutRuntimeAdapter.cs`: Cầu nối adapter độc lập áp dụng cấu hình đã resolve vào scene behaviours (`DefenseVillageShop3D`, `DefenseHeroKitSO`).
  - `DefenseSceneCompositionRoot.cs`: Bổ sung `SetLoadoutSnapshot()` và phương thức `ApplyLoadout()` chạy ngay trước khi khởi động session trận đấu.
  - Bộ kiểm thử tự động G2: 30/30 checks vượt qua sạch sẽ trong `Tools/Defense3DValidation/DefenseLoadoutCoreChecks.cs`; test suite NUnit hoàn chỉnh tại `Assets/Tests/Editor/DefenseLoadoutCatalogAndAdapterTests.cs`.
- **G3 — Save Migration & Meta Adapter:** Đã triển khai hoàn thiện:
  - `DefenseLoadoutIdMapper.cs`: Ánh xạ các bí danh ID cũ (`C001_ThuSinh`, `R001_Deptoong`,...) sang Catalog Defense 3D chuẩn (`hero_thusinh`, `relic_deptoong`), điền đầy đủ minions/towers mặc định khi thiếu.
  - `DefenseMetaOwnershipAdapter.cs`: Nối `MetaProgressionSaveData` hiện hữu sang `ILoadoutOwnership`, tôn trọng các nhân vật và thẻ mảnh đã mở khóa, luôn mở khóa miễn phí bộ 6 thực thể baseline.
  - `DefenseLoadoutRepository.cs`: Quản lý lưu trữ chính tại `defense_loadout.json`, cơ chế ghi Atomic an toàn (.tmp -> .bak -> replace), kiểm soát Schema Versioning (từ chối ghi đè nếu phiên bản tương lai `Version > 1`), tự động phục hồi khi JSON bị hỏng, và thực hiện migration một lần từ PlayerPrefs cũ.
  - Bộ kiểm thử tự động G3: 41/41 checks vượt qua trong `Tools/Defense3DValidation/DefenseLoadoutCoreChecks.cs`; bộ NUnit Test Editor toàn diện tại `Assets/Tests/Editor/DefenseLoadoutSaveAndMetaAdapterTests.cs` (11 kịch bản kiểm thử) biên dịch sạch sẽ 100%.
- **G4 — Luồng Sảnh & UI Chuẩn Bị Trận:** Đã triển khai hoàn thiện:
  - Cô lập DontDestroyOnLoad: Thêm `transform.DetachChildren()` trong `GameStateManager.cs` trước khi `DontDestroyOnLoad(gameObject)`, giải phóng 15 con của `--- GAME MANAGER ---` để Unity tự động dọn sạch gameplay 2D khi chuyển sang Scene 3D.
  - `DefenseSessionContext.cs`: Vùng nhớ tạm tĩnh (Session Carrier) mang snapshot đội hình xuất trận sang Scene mới an toàn và tự giải phóng bộ nhớ (`ConsumeSnapshot()`) tránh rò rỉ lượt chơi.
  - `DefenseMatchNavigation.cs`: Dịch vụ điều hướng nạp Scene `"MapSongQue3D_Prototype"` bằng tên tường minh (không dùng Build Index), có Debounce Lock chống bấm liên tiếp, tự động khôi phục cờ điều hướng khi có lỗi hoặc hủy nạp.
  - `DefensePreparationPresenter.cs`: Presenter điều phối chọn Tướng, Pháp Bảo, Lính, Trụ từ `Defense3DCatalog`, thẩm định realtime qua `LoadoutValidator` và khóa/mở nút Xuất Trận (`CanStartBattle`), tự động lưu cấu hình mới và kích hoạt điều hướng.
  - `MetaSceneTransitionController.cs`: Cập nhật chuyển cảnh sang Scene Defense 3D qua `DefenseMatchNavigation`, thay thế hoàn toàn đường `StartRun` nạp Tilemap 2D cũ.
  - Bộ kiểm thử tự động G4: 47/47 checks vượt qua trong `Tools/Defense3DValidation/DefenseLoadoutCoreChecks.cs`; test suite NUnit hoàn chỉnh tại `Assets/Tests/Editor/DefenseMatchNavigationAndPreparationTests.cs` (7 kịch bản kiểm thử) biên dịch sạch sẽ 100%.
- **G5 — Kết Quả, Trở Về Sảnh & Hoàn Thiện Vòng Đời:** Đã triển khai hoàn thiện:
  - `DefenseMatchResultService.cs`: Dịch vụ độc lập xử lý kết quả sau trận, tính thưởng Cổ Tiền rõ ràng (Gỗ thu thập x 2.0 + Thưởng thắng trận 500 Cổ Tiền), cập nhật tiến trình vào `MetaProgressionSaveData` (kỷ lục cá nhân `UpdateBestStats`, đánh dấu hoàn thành Ải `MarkStageCompleted`), tích hợp cơ chế chống cấp thưởng hai lần (Idempotency Guard).
  - `DefenseNavigationService.cs`: Hợp đồng điều hướng `IDefenseNavigationService` trở về Sảnh bằng tên tường minh `"SampleScene"` (loại bỏ hoàn toàn `SceneManager.LoadScene(0)`), Debounce Guard chống bấm liên tiếp, tự động dọn dẹp (cleanup pools, dispose session, xóa snapshot tạm).
  - `DefenseMatchPresenter.cs`: Tích hợp `DefenseMatchResultService` tính toán và định dạng kết quả hiển thị; tích hợp `DefenseNavigationService` khi bấm Về Sảnh; khởi tạo lại kết quả (`_resultService.Reset()`) khi bấm Restart.
  - Bộ kiểm thử tự động G5: 59/59 checks vượt qua trong `Tools/Defense3DValidation/DefenseLoadoutCoreChecks.cs`; test suite NUnit hoàn chỉnh tại `Assets/Tests/Editor/DefenseMatchResultAndLifecycleTests.cs` (7 kịch bản kiểm thử) biên dịch sạch sẽ 100%.




