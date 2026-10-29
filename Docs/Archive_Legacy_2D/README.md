# 📦 KHO LƯU TRỮ TÀI LIỆU CŨ: HỆ THỐNG 2D SURVIVAL ROGUELITE (LEGACY ARCHIVE)

> **CẢNH BÁO QUAN TRỌNG:**
> Các tài liệu trong thư mục này thuộc về **hệ thống game 2D Survival Roguelite (Pixel Art / Top-down 3/4 View / U Minh Giới)** thời kỳ trước khi chuyển đổi sang **Action Tower Defense 3D (Bản đồ Sông Quê / TREBRAVE Unity 3D)**.
> 
> Mục đích lưu trữ:
> 1. Bảo tồn toàn vẹn cốt truyện, bối cảnh thần thoại văn hóa thuần Việt (Tứ Bất Tử, các nhân vật, pháp bảo, truyền thuyết).
> 2. Lưu giữ các công thức cân bằng chỉ số cũ để tham chiếu.
> 3. Tránh gây nhầm lẫn (conflicts/ambiguity) cho Developer và Game Designer khi phát triển hệ thống **Defense 3D hiện hành**.

---

## 📑 DANH MỤC CÁC TÀI LIỆU ĐÃ LƯU TRỮ

| Tên Tệp Tài Liệu | Nội Dung Tài Liệu Ban Đầu | Lý Do Lưu Trữ |
|---|---|---|
| `ProjectZombie_GDD.md` | Game Design Document v5.1 (Bản đồ Bến Đò Vong Xuyên 20 phút) | Đã được thay thế bằng tài liệu GDD & Kiến trúc Defense 3D (`GameDesignDoc/TREBRAVE_Unity3D/`). |
| `CHARACTER_VFX_SIZE_GUIDELINES.md` | Bảng quy chuẩn kích thước nhân vật Chibi 2D (32 PPU, sprite 32-48px) | Đã chuyển sang không gian 3D metric ($1 \text{ Unit} = 1 \text{ Mét}$). |
| `TILEMAP_DESIGN_SPEC.md` | Đặc tả thiết kế Tilemap 2D (U Minh Giới 16x16 / 32x32px) | Bản đồ hiện tại là môi trường 3D Mesh & NavMesh Sông Quê. |
| `PHYSICS_LAYER_DESIGN_SPEC.md` | Thiết lập LayerMask cho hệ thống Physics2D cũ | Hệ thống hiện tại dùng Physics 3D Collider và NavMeshAgent. |
| `PROJECTZOMBIE_AI_VFX_PRODUCTION_PLAN.md` | Quy trình sinh Sprite VFX 2D bằng AI | Đã nâng cấp sang pipeline VFX 3D & URP Particles. |
| `PROJECTZOMBIE_SLASH_VFX_REWORK_SPEC.md` | Rework hiệu ứng chém 2D của An Sĩ | Thay thế bằng hệ thống VFX Combat 3D (`DefenseCombatSlashVfx3D`). |
| `WEAPON_VISUAL_MANIFESTATION_RULES.md` | Quy chuẩn hiển thị sprite pháp bảo xoay quanh nhân vật 2D | Thay thế bằng hệ thống Tháp phòng thủ và Kỹ năng 3D. |
| `FUN_COMBAT_AND_ENEMY_MECHANICS_DOC.md` | Cơ chế quái vật và đòn đánh 2D cũ | Hệ thống AI và Boss Dạ Xoa 3D hiện tại nằm tại `Assets/Features/Defense3D/AI/`. |
| `LEVEL_SCENARIO_GUIDE.md` | Kịch bản 4 map 2D cũ (Cinnabar Swamp, Ancient Citadel...) | Bản đồ chuẩn hiện tại là Chiến trường 3D Sông Quê 34 đợt sóng. |
| `DOCS_COOP_MULTIPLAYER_PLAN.md` | Kế hoạch Co-op 2 người chơi qua Photon cho bản 2D | Tạm thời đóng băng để tập trung hoàn thiện Vertical Slice Defense 3D. |
| `NGU_HANH_REGRESSION_CHECKLIST.md` | Checklist kiểm thử hệ thống Tương Sinh / Tương Khắc Ngũ Hành 2D | Cân bằng chỉ số hiện tại dựa trên hệ thống Tower & Hero Kit 3D. |
| `VONG_XUYEN_VERTICAL_SLICE_EXECUTION_PLAN.md` | Kế hoạch thực thi bản Vertical Slice 2D cũ | Thay thế bằng Kế hoạch 600s Defense Run & Stage Authoring 3D. |
| `PERFORMANCE_OPTIMIZATION_AUDIT.md` | Báo cáo kiểm toán hiệu năng 2D cũ | Thay thế bằng quy chuẩn 60 FPS Mobile Defense 3D. |
| `Uprades.doc` | Tệp nhị phân Microsoft Word cũ không còn sử dụng | Lưu trữ tệp rác. |

---

## 📌 HƯỚNG DẪN TÀI LIỆU HIỆN HÀNH (CURRENT SOURCE OF TRUTH)

Khi phát triển hoặc tích hợp tính năng mới, xin vui lòng tra cứu các tài liệu chuẩn hiện hành:
* **Kiến trúc & Kế hoạch Defense 3D:** Tra cứu tại thư mục [`GameDesignDoc/TREBRAVE_Unity3D/`](file:///c:/Users/thuon/Unity/Projectzombie/GameDesignDoc/TREBRAVE_Unity3D) và [`ProjectManagement/`](file:///c:/Users/thuon/Unity/Projectzombie/ProjectManagement).
* **Hướng dẫn thiết kế màn chơi:** Tra cứu tại [`Docs/GAME_DESIGNER_STAGE_AUTHORING_GUIDE.md`](file:///c:/Users/thuon/Unity/Projectzombie/Docs/GAME_DESIGNER_STAGE_AUTHORING_GUIDE.md).
* **Hệ thống tải dữ liệu Addressables:** Tra cứu tại [`Docs/DOCS_ADDRESSABLES_CDN_FIREBASE.md`](file:///c:/Users/thuon/Unity/Projectzombie/Docs/DOCS_ADDRESSABLES_CDN_FIREBASE.md).
* **Quy chuẩn Code & Tối ưu 60 FPS Android:** Tra cứu tại [`.agents/AGENTS.md`](file:///c:/Users/thuon/Unity/Projectzombie/.agents/AGENTS.md).
