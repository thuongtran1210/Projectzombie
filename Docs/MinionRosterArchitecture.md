# Roster lính theo ID — Defense3D

Refactor ngày 01/11/2026, Unity 2022.3.62f3.

Đội hình có bốn loại lính tạo bốn đội độc lập. Dân Binh và Xạ Thủ vẫn có thể dùng
chung chiến lược `Militia` mà không thay thế nhau. Giá, chỉ số và giá trị enum
role hiện có được giữ nguyên; ID definition cũ được chuẩn hóa sang ID loadout.

## Phân trách nhiệm

| Thành phần | Trách nhiệm |
|---|---|
| `DefenseMinionCatalogSO` | Catalog asset dùng chung cho scene, tham chiếu definition |
| `DefenseMinionDefinitionSO` | ID, tên/icon, prefab, sức chứa, chỉ số, nhãn lệnh và strategy tùy chọn |
| `MinionRosterRuntime` | Chọn đúng ID đã trang bị, kiểm tra ID, tra cứu và reset lệnh |
| `MinionSquadRuntime` | Lệnh và hệ số nâng cấp của một loại lính |
| `MinionRecruitmentService` | Điều phối triển khai/trả quân/thanh toán qua interface nhỏ |
| `DefenseMinionSlotPool` | Unity ObjectPool theo từng ID, cung cấp quân đã warm và xử lý NavMesh |
| `DefenseVillageShop3D` | Adapter Unity và ngữ cảnh tương tác/AI làng; nối roster, pool, wallet, session |
| `DefenseVillageShopPresenter` | Nhận sự kiện roster/shop, định dạng nội dung và gửi yêu cầu qua interface |
| `DefenseVillageShopView` | Pool các dòng theo ID, quản lý modal/dock/scroll/safe area |
| `DefenseMinionSquadRowView` | Hiển thị thụ động và phát sự kiện kèm ID |

`IMinionRosterReader`, `IMinionSquadCommands`, `IMinionRecruitmentCommands` tách
đọc dữ liệu khỏi yêu cầu thao tác. Recruitment phụ thuộc `IMinionRecruitWallet`,
`IMinionRecruitContext`, `IMinionRecruitPool`, `IMinionRecruitLease`, giúp kiểm thử
không cần shop, NavMesh hoặc UI thật.

## Vòng đời

1. Composition bind session và các adapter scene.
2. Loadout resolver xác thực snapshot; shop nhận đúng danh sách `MinionIds`.
3. Roster phát sự kiện; UI lấy các dòng từ pool, không sinh lại mỗi frame.
4. `PrepareRosterAsync` warm quân từ prefab đã gán, nhường frame giữa các instance.
   Camera thanh máu và mục tiêu đồng minh được gắn trong giai đoạn chuẩn bị.
5. Session bắt đầu. Tuyển quân lấy actor từ native pool, triển khai thành công rồi
   mới trừ gỗ. Sai scope hoặc thanh toán thất bại trả actor lại pool.
6. Lính gục/retire trở về đúng pool của loại đó. Restart reset lệnh và trả quân,
   giữ nguyên roster và các actor đã chuẩn bị. Thoát scene dọn actor/subscription.

Không tự bổ sung ba nhóm lính hỗ trợ vào đội hình. Trường hợp loadout không hợp
lệ vẫn do resolver xử lý theo chính sách fallback hiện có. `AllowedMinionRoles`
còn là thông tin tổng hợp khả năng hành vi, không dùng làm ID đội hoặc chỉ số UI.

## Thêm loại lính

- Tạo definition với ID duy nhất; gán icon, tên, capacity, chỉ số và các nhãn lệnh.
- Tạo prefab root inactive với `DefenseMinion3D`, health, agent và các reference
  bắt buộc. Gán definition trên prefab và prefab ngược lại trên definition.
- Thêm definition vào `Assets/Features/Defense3D/Minions/Assets/MinionCatalog.asset`.
- Đăng ký ID/nội dung/mở khóa trong catalog trang bị hiện có để người chơi chọn
  được loại mới. Đây là ranh giới riêng của hệ thống loadout/meta.
- Chọn strategy mặc định theo role hoặc cung cấp `DefenseMinionBehaviorSO` cho
  hành vi mới. Strategy asset dùng chung phải stateless; trạng thái nằm trên actor.

Roster, tuyển quân, pool và UI không cần thêm nhánh theo tên loại lính. Giới hạn
số loại được trang bị vẫn thuộc luật loadout; thêm nội dung không tự mở thêm slot.
Bộ kiểm thử tạo loại thứ năm tổng hợp để xác minh roster và UI không cố định 3/4 ô.

## Migration

Menu `Tools > ProjectZombie > Minions > Migrate Squad Roster`, hoặc:

```powershell
./Tools/Invoke-UnityBridge.ps1 migrate-roster -TimeoutSeconds 60
```

Tool cập nhật scene sản xuất và CombatLab, giữ GUID các definition, tạo catalog và
hai prefab dòng UI dùng chung. Quân dormant cũ và reference hỗ trợ tương ứng được
gỡ khỏi scene để chuyển sang warm từ prefab. Backup scene lần đầu nằm trong
`scratch/RosterBefore_*.unity`. Scene đã mở và dirty bị từ chối, không tự save/bỏ.
Chạy lại không nhân đôi catalog/UI và giữ các loại mới đã đăng ký thêm.

Các mảng `_catalog`, `_slots` và các mảng UI cũ còn tồn tại như cầu nối authoring
cho installer legacy; scene đã migrate không dùng chúng để quyết định số đội.
Installer legacy kết thúc bằng bước upgrade sang roster để tránh đưa lại luồng
tuyển quân ba nhóm. `XaThuMinion.prefab` giữ model chiến đấu sẵn có; có thể thay
visual riêng qua prefab khi bổ sung art.

## Kiểm chứng

- `MinionRosterRuntimeTests`: cùng role khác ID, lệnh độc lập/reset, loại thứ năm,
  dữ liệu sai không ghi đè roster, thanh toán/triển khai thất bại, stale session,
  chống reentrant và tái bind UI.
- `DefenseMinionXaThuTests`: cấu hình/range/combo hiện có; hai test AI dùng NavMesh
  trong Play Mode thay vì giả định NavMesh tồn tại trong Edit Mode.
- `DefenseLoadoutCatalogAndAdapterTests`: đội hình được áp đúng ID, không ép thêm
  các đội không trang bị.
- Match check dùng sự kiện Button thật, tuyển cả bốn loại, đổi riêng lệnh Xạ Thủ,
  giết/tuyển lại Xạ Thủ, pause, restart, tái sử dụng pool và trở về sảnh.

```powershell
./Tools/Invoke-UnityBridge.ps1 smoke-roster -TimeoutSeconds 60
```

Kết quả: `scratch/MinionRosterMatchCheck.txt`, `MinionRosterRecruitment.png`,
`MinionRosterCommands.png`; báo cáo Unity tests lưu theo job ID của Unity MCP.
Chưa đo FPS, GC hoặc GPU trên Android và chưa kiểm chứng kéo/chạm trên thiết bị.

Kết quả kiểm thử: 50 bài liên quan roster/loadout/AI/pool đạt, 0 lỗi; 117 bài hồi
quy app đạt, 0 lỗi. Các nhóm kiểm thử có phần giao nhau, không cộng thành số bài
duy nhất. Báo cáo tiện tra cứu: `scratch/MinionRosterValidation.xml`.

Validator legacy có thể đánh dấu scene UI đang mở là dirty khi Unity tính lại
layout. `clear-validation-layout` chỉ phục hồi dirty flag của MainMenu nếu snapshot
khớp bản gốc ngoài các giá trị RectTransform đã chứng minh đang được layout điều
khiển. Tool giữ snapshot trong scratch, từ chối các thay đổi khác và không lưu hoặc
reload scene. Tránh dùng Save/Don't Save để bỏ qua guard scene trong match check.
