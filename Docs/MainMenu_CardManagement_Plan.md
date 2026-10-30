# Mainmenu — kiểm kê quản lý thẻ và kế hoạch triển khai

Ngày: 30/10/2026. Phạm vi: Anh hùng, Thần khí, Lính, Trụ.

## 1. Cơ sở kiểm kê

- Unity Bridge `status` xác nhận MainMenu đang mở, Edit Mode, scene có thay đổi chưa lưu.
- Đọc source hiện tại trong workspace, bao gồm các thay đổi chưa commit. Không coi source này là phiên bản đã được kiểm thử.
- Không lưu scene, chạy Play Mode, thay đổi save người chơi hoặc thay đổi các file triển khai trong bước kiểm kê.
- Hierarchy được đọc qua Unity Bridge trong phiên phân tích trước; cần đọc lại trước khi sửa cấu trúc scene.

## 2. Hệ thống thực tế đã có

| Loại | Dữ liệu tiến trình | Nâng cấp hiện có | Hiển thị / trang bị |
|---|---|---|---|
| Anh hùng | `characterProgressList`: ID, mảnh, sao | `CharacterProgressionManager.TryUpgradeCharacterStar`, tối đa 5 sao | Codex HeroCards, chọn vào slot anh hùng |
| Thần khí | `relicProgressList`: ID, mảnh, sao | `RelicInventoryManager.TryFuseRelic`, tối đa 5 sao | Codex RelicFusion, chọn vào slot thần khí |
| Lính | Dùng `relicProgressList` theo ID catalog | Dùng chung TryFuseRelic và cấu hình RelicStarProgressionSO | Tab Minions, thêm / tháo trong đội hình |
| Trụ | Dùng `relicProgressList`; có thêm `unlockedTowerIds` | Dùng chung TryFuseRelic và cấu hình RelicStarProgressionSO | Tab Towers, thêm / tháo trong đội hình |

Đây là hệ tăng sao bằng mảnh và Cổ Tiền. Chưa có bằng chứng về hệ tăng level thẻ độc lập; không tự bổ sung hệ này trong đợt cải thiện UI.

Đội hình mặc định: 1 anh hùng, 1 thần khí; từ 1 đến 4 lính và từ 1 đến 4 trụ. Các giới hạn lấy từ LoadoutRules.Default, không ghi số cố định trong UI mới.

Runtime adapter đã có phép tính bonus cho bốn loại: anh hùng (máu / tốc độ), thần khí (sát thương / hồi chiêu / phạm vi), lính (máu / tốc độ), trụ (sát thương / tầm). Việc có hàm tính không thay thế kiểm chứng kết quả trên thực thể trong trận.

## 3. Khoảng thiếu và rủi ro cần giải quyết

### P0 — nhất quán dữ liệu và giao dịch

1. Quyền sở hữu có nhiều nguồn: sao trong save, danh sách unlock, các ID cấp sẵn trong DefenseMetaOwnershipAdapter. Cần thống nhất cách UI hiển thị thẻ cấp sẵn ở 1 sao với giá / bước nâng cấp được manager đọc từ save.
2. SpendCurrency lưu và phát sự kiện trước khi TryFuseRelic cập nhật mảnh / sao rồi lưu tiếp. Có khoảng trạng thái trung gian; cần thiết kế một thao tác cập nhật tiền + mảnh + sao và lưu nhất quán, kiểm tra thất bại ghi đĩa. Chưa khẳng định đã xảy ra mất dữ liệu.
3. Bốn anh hùng cùng một số thần khí / lính / trụ được cấp quyền sẵn trong adapter. Giữ nguyên hành vi hiện có trong đợt UI; thay đổi chính sách unlock là quyết định cân bằng riêng.
4. Workspace đang có nhiều sửa đổi ở progression, save, catalog và Codex. Không ghi đè hoặc hoàn nguyên các thay đổi này; chụp baseline trước mỗi chỉnh sửa.

### P1 — chức năng và điều hướng

1. Codex hiện vừa quản lý thẻ vừa chỉnh đội hình. Cần phân biệt ngữ cảnh vào màn, hành động chính và nơi quay lại, tận dụng component hiện có.
2. Chọn anh hùng và Thần Thẻ cùng mở HeroCards; Armory mở RelicFusion. Chuẩn hóa tên theo mục đích người chơi.
3. Chưa thấy API tìm kiếm / lọc / sắp xếp trong CardCodexView đã kiểm tra. Bổ sung có chọn lọc: đã sở hữu, đang dùng, có thể nâng cấp; chưa cần hệ filter phức tạp khi số thẻ ít.
4. Thiếu lý do trực tiếp trên UI khi thêm / tháo lính, trụ bị chặn bởi giới hạn slot; code hiện có nhánh return im lặng.
5. Chuẩn hóa thuật ngữ UI theo yêu cầu: Anh hùng, Thần khí, Lính, Trụ. Tên class / trường save cũ giữ để tránh migration không cần thiết.

### P2 — bố cục và phản hồi

1. Giữ lựa chọn và vị trí cuộn khi nâng cấp / trở lại từ chi tiết.
2. Hiển thị đủ trạng thái: chưa sở hữu, có thể mở khóa, có thể tăng sao, thiếu tài nguyên, tối đa, đang trang bị.
3. Chi tiết luôn phân biệt chỉ số hiện tại và sau nâng cấp. Không đưa nhãn kỹ thuật như '3D' vào mô tả người chơi.
4. Kiểm tra safe area, chữ Việt, vùng chạm và khung preview trên thiết bị.

## 4. Luồng mục tiêu

- Mainmenu → Thẻ bài → Anh hùng / Thần khí / Lính / Trụ → Chi tiết → Mở khóa hoặc Tăng sao.
- Mainmenu → Đội hình → Slot → Chọn thẻ → Xác nhận thay thế / thêm / tháo.
- Đội hình → Chi tiết → Tăng sao → Trở lại đúng ngữ cảnh đội hình.
- Thẻ bài → Đưa vào đội hình → Chọn slot / thay thế khi đầy, không ghi đè âm thầm.
- Xuất trận → Chọn màn → Kiểm tra đội hình → Vào trận; có lối Chỉnh đội hình rồi trở lại màn đã chọn.
- Miếu cổ dành cho nâng cấp chung. Luyện khí là phần của Thần khí nếu không có cơ chế chế tạo độc lập.

## 5. Các đợt triển khai

### Đợt A — baseline và kiểm chứng nền tảng

- Kiểm tra source tests hiện có về Loadout / Save / Adapter; bổ sung kiểm thử có ý nghĩa cho tính nhất quán ownership và giao dịch nâng sao.
- Kiểm tra giá / mảnh ở 0, 1, 4, 5 sao; ID alias; không đủ tiền / mảnh; khởi tạo manager trễ; lỗi lưu.
- Đối chiếu cấu hình Resources với _Data để xác định nguồn runtime.
- Xác nhận đủ đường áp dụng bonus vào thực thể trận của cả bốn loại.

Điều kiện hoàn tất: có baseline và kết quả kiểm tra; các sai lệch P0 được sửa trước khi mở rộng UI.

### Đợt B — màn quản lý thẻ

- Tái sử dụng Codex, chuẩn hóa bốn tab và panel chi tiết.
- Bổ sung trạng thái sở hữu, mảnh, sao, chi phí, thiếu hụt, chỉ số trước / sau, phản hồi thành công / thất bại.
- Thêm lọc cơ bản và giữ trạng thái danh sách.

Điều kiện hoàn tất: quản lý và tăng sao cả bốn loại qua cùng quy ước tương tác, lưu lại đúng sau khi khởi động lại.

### Đợt C — đội hình và điều hướng

- Phân biệt ngữ cảnh Bộ sưu tập / Đội hình bằng hợp đồng điều hướng rõ ràng.
- Hoàn thiện chọn slot, thay thế, giới hạn và thông báo lý do.
- Cập nhật lối vào từ MainHub; tóm tắt đội hình và luồng Back.

Điều kiện hoàn tất: không còn hai lối vào không phân biệt mục đích; thẻ đang dùng và tiến trình nâng sao đồng nhất.

### Đợt D — tích hợp scene và QA

- Kiểm tra lại StageSelect trùng, trạng thái panel, prefab _Prefabs / Resources và registry trước khi hợp nhất.
- Kiểm thử luồng Mainmenu → nâng sao → đội hình → vào trận → trở về → mở lại game.
- Kiểm tra Android, tỷ lệ màn hình và hiệu năng; ghi rõ phần nào chỉ kiểm tra tĩnh và phần nào đã chạy trong Unity.

Điều kiện hoàn tất: không lỗi Console liên quan, không mất dữ liệu, đúng màn hình và trạng thái qua các luồng chính.

## 6. Nguồn kiểm kê

- Assets/Features/UI/CardCodexPresenter.cs: subscribe tiến trình, bốn tab, xử lý nâng sao và Back.
- Assets/Features/UI/CardCodexPresenter.Loadout.cs: danh sách catalog, chi tiết, bonus, trang bị.
- Assets/Features/UI/CardCodexView.cs: hợp đồng View và loại tab.
- Assets/Features/MetaProgression/CharacterProgressionManager.cs: tiến trình anh hùng.
- Assets/Features/MetaProgression/RelicInventoryManager.cs: tiến trình thần khí, lính, trụ.
- Assets/Features/MetaProgression/MetaCurrencyManager.cs: chi tiền, lưu và phát sự kiện.
- Assets/Features/MetaProgression/MetaProgressionSaveData.cs: save, alias và migration.
- Assets/Features/Defense3D/Loadout/DefenseMetaOwnershipAdapter.cs: quyền sở hữu / cấp sẵn.
- Assets/Features/Defense3D/Loadout/DefenseLoadoutRuntimeAdapter.cs: bonus runtime.
- Assets/Features/Defense3D/Loadout/LoadoutRules.cs: giới hạn đội hình.
- Assets/Tests/Editor/DefenseLoadoutCoreTests.cs; DefenseLoadoutSaveAndMetaAdapterTests.cs; DefenseLoadoutCatalogAndAdapterTests.cs: kiểm thử hiện có, chưa chạy trong bước kiểm kê.

## 7. Tiến độ triển khai — giao dịch tăng sao

Đã sửa luồng tăng sao của CharacterProgressionManager và RelicInventoryManager:

- Cập nhật tiền, mảnh và sao trước một lần gọi lưu, thay cho lưu tiền rồi lưu tiến trình riêng.
- Khi lưu trả false hoặc ném lỗi, hoàn tác số dư và danh sách tiến trình trong bộ nhớ; không phát sự kiện nâng sao thành công.
- Chỉ đồng bộ số dư hiển thị và phát sự kiện sau khi lưu thành công.
- Từ chối thao tác nếu manager tiền và manager tiến trình dùng hai đối tượng save khác nhau, thay vì ghi dữ liệu không thống nhất.
- Chặn cấu hình chi phí / mảnh âm hoặc bước sao không khớp.
- Trường hợp không có manager tiền, thần khí / lính / trụ dùng số dư trong save như luồng anh hùng.

Kiểm chứng qua Unity Bridge `validate-card-progression`: PASS 28 tình huống trong Unity 2022.3.62f3, lúc 17:15 ngày 30/10/2026 (Asia/Saigon).

- 24 tình huống bộ xử lý giao dịch: mở khóa, tăng sao 1→2 và 4→5, thiếu tiền / mảnh, max sao, dữ liệu cấu hình âm, tài nguyên vừa đủ, lưu false / ném lỗi và khôi phục danh sách null.
- 4 tình huống lưu / đọc lại bằng LocalJsonSaveProvider với ID thật của Anh hùng, Thần khí, Lính, Trụ. Fixture nằm trong scratch/CardProgressionValidation, không dùng file save người chơi và không thay CurrentProvider.
- Báo cáo: scratch/CardProgressionValidation.txt.
- Công cụ chạy lại: Tools/Invoke-UnityBridge.ps1 -Command validate-card-progression.

Giới hạn: chưa kiểm thử tích hợp manager / UI trong Play Mode; chưa đảm bảo chống mất điện giữa lúc ghi file vì LocalJsonSaveProvider hiện ghi trực tiếp File.WriteAllText. Việc rollback ở đây bảo vệ trạng thái bộ nhớ khi provider báo thất bại, không phục hồi một file đã ghi dở.

Lưu ý môi trường: khi Unity reload script, các tool InitializeOnLoad có sẵn ApplyNewUISpritesTool và AddMultiplayerButtonTool tự chạy và báo cập nhật prefab / scene trong Editor.log. Không tự hoàn nguyên thay đổi vì workspace có chỉnh sửa của người dùng; cần đối chiếu riêng trước khi tích hợp scene.

Đợt A còn lại: kiểm chứng ownership / alias và đồng bộ manager, áp dụng bonus vào thực thể trận, độ bền ghi file. Đợt B–D chưa hoàn tất.

## 8. Tiến độ — quyền sở hữu và hợp đồng chỉ số

Đã sửa:

- CreateDefault của DefenseMetaOwnershipAdapter nhận save tùy chọn; Codex và màn chuẩn bị trận truyền save hiện tại khi có GameManager.
- Các bí danh anh hùng / thần khí được chuẩn hóa cho cả yêu cầu kiểm tra và quyền cấp thủ công.
- Tiến trình từ save chỉ mở quyền sở hữu khi starLevel >= 1; chỉ có mảnh ở 0 sao không mở khóa. Giữ nguyên quyền cấp sẵn hiện có.
- Kiểm tra unlock không sử dụng MapHeroId / MapRelicId có fallback. GetMetaCharacterId trả null với ID không được hỗ trợ, tránh đọc nhầm tiến trình Thư Sinh.
- Kiểm tra quyền từ danh sách unlock trụ không phân biệt hoa / thường và bỏ khoảng trắng.

Unity Bridge validate-card-progression: PASS 56 tình huống lúc 17:18 ngày 30/10/2026. Gồm 28 tình huống trước, 8 kiểm tra quyền / alias và 20 kiểm tra hợp đồng công thức ở 5 mốc sao của 4 loại với cấu hình Resources thực tế. Không dùng save người chơi; chưa thay thế kiểm thử UI hoặc thực thể trong Play Mode.

Đối chiếu source vào trận:

- DefenseSceneCompositionRoot gọi ApplyHero, ApplyTowerAndMinionStarBonuses và EvaluateRelicStarBonuses.
- ApplyHero hiện chỉ dùng bonus máu / tốc độ; BaseDamage được gán trực tiếp từ definition. CharacterStarProgressionSO còn mô tả sát thương / giảm hồi chiêu; chưa xác nhận các bonus này được áp dụng ở nhánh Defense3D. PlayerProgressionApplicator có xử lý ở luồng PlayerStats riêng, không coi đó là bằng chứng cho Defense3D.
- Bonus trụ hiện lấy sao cao nhất của các trụ đã chọn rồi đưa vào sessionStats chung. Không được mô tả đây là bonus độc lập của từng trụ nếu chưa thay đổi cơ chế runtime.

Việc tiếp theo: giải quyết hợp đồng tác dụng nâng sao trong Defense3D trước khi hoàn thiện mô tả trước / sau trên UI; kiểm chứng manager và provider khi ghi thất bại. Chưa đổi cân bằng hoặc cơ chế bonus trụ trong bước này.

## 9. Tiến độ — tác dụng nâng sao Anh hùng và mô tả Trụ

Đã nối bonus sát thương và hồi chiêu trong CharacterStarProgressionSO vào Defense3D:

- EvaluateHeroCombatStarBonuses cộng dồn bonus sát thương và giảm hồi chiêu từ các mốc sao, giới hạn giảm hồi chiêu 80% như SkillDefinition.
- ApplyHero gán bonus sát thương vào DefenseHeroAutoAttack3D. Bộ phận đánh thường áp dụng hệ số sau modifier trong trận; không sửa definition gốc và không cộng dồn khi gán lại.
- Kỹ năng bản mệnh vốn lấy sát thương qua tỷ lệ EffectiveDamage / BaseDamage của attack owner, nên nhận bonus một lần. Không nhân bonus lần thứ hai lên definition kỹ năng.
- CompositionRoot truyền giảm hồi chiêu vào presenter bản mệnh trước khi Bind. Không áp dụng bonus anh hùng lên Thần khí hoặc hồi chiêu đánh thường / lướt.
- UI Codex hiển thị máu, tốc chạy, sát thương đánh thường / bản mệnh và giảm hồi chiêu bản mệnh ở sao hiện tại / kế tiếp, thay cho perkDescription có thể hứa tác dụng chưa triển khai.
- Màn chuẩn bị trận ghi rõ Công nền và bonus sao, tránh nhầm với sát thương từ definition đòn đánh đang được sử dụng.
- UI Trụ giải thích bonus dùng chung theo sao cao nhất trong đội hình. Chưa đổi cơ chế cân bằng này.

Kiểm chứng Unity Bridge: PASS 63 tình huống lúc 17:22 ngày 30/10/2026. Bảy kiểm tra mới dùng GameObject tạm HideAndDontSave, DefenseSession độc lập và config fixture: attack definition không bị sửa, sát thương nhận đúng bonus, kết hợp modifier trong trận, gán lại không cộng dồn, hồi chiêu signature giảm đúng, reset modifier giữ bonus thẻ, đổi hero có thể xóa bonus.

Không chạy full Play Mode, chưa QA layout chữ trên điện thoại và chưa kiểm tra một đòn đánh vào enemy thật. Không ghi save người chơi. Bước kế tiếp là hoàn thiện trạng thái / phản hồi nâng cấp trên UI và kiểm thử tích hợp toàn luồng.

## 10. Tiến độ — phản hồi nâng cấp trên UI

Đã triển khai cho Anh hùng, Thần khí, Lính, Trụ:

- Presenter gửi số mảnh, tiền đang có / cần, lượng thiếu và trạng thái sẵn sàng / tối đa / chưa có dữ liệu tới View.
- Nâng cấp thành công hiển thị mở khóa hoặc chuyển từ sao cũ sang sao mới; thất bại hiển thị lý do kiểm tra điều kiện hoặc lỗi lưu.
- Trong giao dịch, chặn thao tác lồng nhau và các refresh từ sự kiện model; sau thao tác refresh một lần, khôi phục thẻ chọn và vị trí cuộn.
- Mở rộng vùng phản hồi từ 32 lên 116 đơn vị; dịch đáy khung mô tả có cuộn để không chồng phần phản hồi. Đồng bộ cả prefab _Prefabs / Resources và generator.
- Bổ sung Unity Bridge `preview-card-feedback` chỉ render prefab hiện có trong scene tạm; không gọi generator hoặc lưu scene đang mở.

Kiểm chứng: PASS 73 tình huống trong Unity lúc 17:26 ngày 30/10/2026. Mười kiểm tra bổ sung trên hai prefab: có reference / vùng phản hồi, trạng thái thiếu tài nguyên vô hiệu hóa nút, lỗi lưu giữ thông tin yêu cầu, chọn thẻ khác xóa kết quả cũ, thẻ đủ điều kiện bật lại nút.

Đã render và xem ảnh tại 1920x1080 và 1440x1080; vùng phản hồi mẫu không đè nút hay mô tả. Ảnh: Logs/CardCodexQA/cardcodex-1920x1080.png và cardcodex-1440x1080.png. Đây là dữ liệu minh họa bố cục, không phải ảnh thao tác nâng cấp từ save thật. Chưa kiểm tra Android / full Play Mode hoặc thử nâng cấp qua click với save tách biệt.

## 11. Tiến độ — chọn đội hình và đường Back

Đã triển khai:

- MetaUIManager.OpenChildScreen giữ màn cha trong stack. Chọn màn → Chuẩn bị trận → Codex sử dụng luồng con; Back pop về đúng màn cha thay vì mở lại review bằng cờ toàn cục.
- Chạm vị trí trong màn chuẩn bị trận mở đúng tab Codex và thẻ hiện tại; không còn tự xoay vòng anh hùng / thần khí hoặc toggle lính dựa trên index catalog.
- Yêu cầu chọn slot được xử lý khi View.Show, tránh bị prewarm / khởi tạo factory tiêu thụ trước khi màn hình thực sự được mở.
- Chạm vị trí lính / trụ đã có thẻ cho phép chọn thẻ khác và xác nhận thay thế tại đúng vị trí, giữ thứ tự và số lượng kể cả khi đội hình đầy. Thẻ đã có trong đội không thể được thêm trùng.
- Chuyển tab hủy ngữ cảnh thay vị trí cũ. Nút bị chặn khi đã tối thiểu / đầy có lời giải thích; chưa sở hữu thì yêu cầu mở khóa.
- Nút chọn anh hùng ở MainHub yêu cầu chọn slot anh hùng; lối Thẻ bài vẫn mở ngữ cảnh bộ sưu tập. Nhãn hành động và tóm tắt hiển thị ngữ cảnh chọn / thay cho đội hình.

Kiểm chứng Unity: PASS 81 tình huống lúc 17:32 ngày 30/10/2026. Tám tình huống mới kiểm tra ReplaceMinion / ReplaceTower (đầy, thứ tự, duplicate, target không tồn tại, dữ liệu rỗng, số sự kiện) và Push/OpenChild/Pop trên hierarchy inactive tách biệt với màn cha mẫu. Không thử thao tác lưu đội hình bằng save người chơi.

Phần còn lại: chạy full Play Mode từ Mainmenu với save thử nghiệm, kiểm tra đường lưu đội hình khi IO lỗi (repository hiện có API Save void và tự log lỗi), QA vùng thông báo và lựa chọn slot trên thiết bị, hoàn thiện lối vào Đội hình riêng trong bố cục MainHub. Không đánh dấu toàn bộ đợt C hoàn tất.

### Điều chỉnh theo yêu cầu người dùng

Nút Anh hùng ở Mainmenu phải mở quản lý Anh hùng: danh sách, chi tiết và tăng sao. Đã trả HandleHeroSelectClicked về RequestInitialTab(HeroCards), không mở chế độ chọn slot. Đội hình có trách nhiệm trang bị; cần tiếp tục tách rõ giao diện / hành động khỏi màn quản lý thẻ. Việc tái sử dụng Codex hiện tại chưa phải kết quả tách màn hoàn chỉnh.

### Đã tách bố cục quản lý và trang bị

- Quản lý (Anh hùng / Thần khí / Lính / Trụ): ẩn Loadout_Bar, đưa danh sách lên ngay dưới tiêu đề, ẩn trang bị, hiện mở khóa / tăng sao và tài nguyên.
- Trang bị đội hình: hiện hàng slot và nút chọn / thay / tháo, ẩn tăng sao và chặn handler nâng cấp trong chế độ này.
- Ngữ cảnh được truyền rõ qua RequestInitialTab (quản lý) và RequestLoadoutSlot (trang bị), giữ qua việc chọn tab; Show xử lý yêu cầu sau prewarm.
- Đồng bộ bố cục mặc định trong hai prefab và generator về màn quản lý, để xem prefab trong Editor cũng không còn hàng đội hình.
- PASS 85 tình huống Unity lúc 17:49 ngày 30/10/2026, gồm kiểm tra mỗi chế độ trên cả hai prefab. Đã render và xem màn Anh hùng ở 1920x1080 và 1440x1080. Ảnh dùng dữ liệu mẫu để xem bố cục, không phải nâng cấp trên save thật.
- Hai chế độ vẫn tái sử dụng component Codex; không tuyên bố đã tạo hai prefab / màn hình độc lập. Chưa kiểm thử full Play Mode và Android.

## 12. Sân đình xuất trận — bản triển khai đầu tiên

Theo lựa chọn người dùng, màn DefensePreparation được chuyển thành sân đình:

- Nền sân đình Việt Nam riêng tại Assets/Art/UI/Courtyard/BG_Courtyard_Deployment.png, tạo bằng imagegen và sao chép vào workspace.
- Anh hùng ở trung tâm sân; hồ sơ và Thần khí bên trái; Lính / Trụ chia hai cụm 2×2 bên phải; chân màn chứa tình trạng đội hình và CTA Xuất trận.
- Chạm nhân vật, Thần khí hoặc một ô lính/trụ dùng các callback chọn trang bị có sẵn. Không thêm chức năng nâng cấp vào màn Đội hình.
- Cả hai prefab _Prefabs / Resources được patch qua CourtyardDeploymentStyler theo Get-or-Create, giữ reference hành động cũ; không dựng lại scene MainMenu hoặc lưu scene đang mở.
- Visual Thư Sinh 3D tại Assets/Art/Characters/ThuSinh/ThuSinhVisual.prefab được gán rõ cho preview vì gameplay catalog hiện có HeroPrefab null. Các anh hùng chưa có visual phù hợp dùng ảnh đại diện dự phòng; không dùng nhầm mô hình Thư Sinh cho nhân vật khác.
- Camera preview căn theo bounds của mô hình. Cleanup RenderTexture trong Edit Mode dùng DestroyImmediate để hỗ trợ render scene tạm.
- Sửa ô Lính / Trụ hiển thị đúng thứ tự ID đã trang bị và xóa hình cũ ở ô trống, khớp callback chọn slot (trước đó View dùng index catalog).

Kiểm chứng: PASS 88 tình huống từ validate-card-progression, gồm ba tình huống thứ tự / ô trống bằng repository trong bộ nhớ. PASS 22 kiểm tra courtyard về reference, viewport model vuông có kích thước dương và idempotence trên cả hai prefab. Không ghi save người chơi.

Render bằng camera Unity trong scene tạm ở 1920×1080 và 1440×1080: Logs/CourtyardQA/courtyard-1920x1080.png và courtyard-1440x1080.png. Đây là dữ liệu mẫu và mô hình thật của project; chưa phải bằng chứng full Play Mode / Android. Chưa có khay chọn thẻ dưới sân hoặc mini model cho toàn bộ lính / trụ; lựa chọn thẻ vẫn đi qua màn trang bị con.

Tool chạy lại: apply-courtyard, preview-courtyard, validate-courtyard qua Tools/Invoke-UnityBridge.ps1.

### Sửa lối vào từ thẻ MainHub

Btn_CardCodex_Wood_Frame trước đó vẫn phát OnCodexClicked nên mở quản lý thẻ. Đã tách OnLoadoutClicked và nối MainHubPresenter tới MetaScreenType.DefensePreparation. Khi _loadoutCardButton / _deckCardsButton / _codexButton cùng trỏ một Button, chỉ đăng ký một callback mở Đội hình. Nút Anh hùng và lối quản lý thẻ khác vẫn giữ vai trò riêng.

Đổi tiêu đề thẻ thành ĐỘI HÌNH trên hai prefab và generator. PASS 90 tình huống Unity, gồm hai kiểm tra mới invoke onClick của đúng nút trên clone mỗi prefab MainHub, đi qua event và presenter thật tới manager / factory tách biệt: đúng một sự kiện Đội hình, không có sự kiện Codex, màn chuẩn bị hiện và Hub ẩn. Không ghi save người chơi; chưa thay thế thao tác tay trong Play Mode.

## 13. Sửa phân vai: quản lý Đội hình tách khỏi review trước trận

Theo phản hồi người dùng, việc nối thẻ MainHub tới DefensePreparation là sai vai trò. Đã thay bằng hai màn độc lập trong registry:

| Đích | ScreenType | Prefab | Vai trò |
|---|---|---|---|
| Btn_CardCodex_Wood_Frame | LoadoutManagement (11, thêm cuối enum) | DefenseLoadoutManagementUI.prefab, có cả _Prefabs / Resources | Trang bị / thay thẻ trực tiếp; nút XONG chỉ trở về; không lấy ải vào presenter, không khởi chạy trận |
| Sau khi chọn ải | DefensePreparation (10, giữ nguyên ID) | DefensePreparationUI.prefab | Review chỉ đọc, thông tin ải ở giữa; CHỈNH ĐỘI HÌNH mở màn quản lý con; XUẤT TRẬN dành riêng luồng này |

DefenseLoadoutManagementView là component kế thừa để dùng chung phần trình bày và binding, nhưng có ScreenType, prefab và hành vi riêng. Không chỉ đổi nhãn trên cùng một màn. Header, nút hành động, khả năng chọn trực tiếp và nội dung trung tâm khác nhau. Review ẩn nút đổi Thần khí và các vị trí chỉ đọc; màn quản lý ẩn nút tùy chỉnh dư thừa.

XONG trong quản lý không mở chọn ải, không hủy ngữ cảnh review của màn cha. Back từ Codex về quản lý, từ quản lý về review hoặc MainHub tùy lối vào. Chọn ải vẫn đi qua MetaSceneTransitionController tới DefensePreparation.

Installer: Tools/Invoke-UnityBridge.ps1 -Command install-loadout-screen. Tạo / cập nhật hai prefab quản lý riêng, giữ references khi chuyển component, đăng ký entry mới mà không đổi số enum cũ hoặc lưu scene đang mở.

PASS 99 tình huống trong Unity lúc 18:32 ngày 30/10/2026, gồm: route nút MainHub vào quản lý thay vì review, XONG trở về mà không mở ải / review, review chỉnh đội hình rồi Back về đúng review, hai prefab và registry có đích khác nhau. Chưa chạy full Play Mode trên save thật hoặc Android.

Ảnh render scene tạm: Logs/CourtyardQA/loadout-1920x1080.png, loadout-1440x1080.png, review-1920x1080.png. Các ghi nhận nối MainHub tới review ở phần trước đã được thay thế bởi phân vai trong mục này.
