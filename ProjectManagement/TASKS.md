# Danh Sách Nhiệm Vụ & Task Tracker — VONG XUYÊN (Defense3D)

Tài liệu quản lý danh sách công việc (Kanban Board Task Tracker) phân chia theo các hạng mục của dự án **Vong Xuyên (Defense3D — Android Mobile Release)**.  
*Tài liệu tham chiếu chính:* 📋 **[TASK.md](file:///c:/Users/thuon/Unity/Projectzombie/TASK.md)** | 📊 **[MIGRATION_STATUS.md](file:///c:/Users/thuon/Unity/Projectzombie/MIGRATION_STATUS.md)** | 📖 **[README.md](file:///c:/Users/thuon/Unity/Projectzombie/README.md)**

---

## 📋 Kanban Board Summary

| 🔴 Backlog (Sắp Tới / G7) | 🟡 In Progress (G6 - Dynamic Binding) | 🟢 Done (G0 ➔ G5 Hoàn Tất) |
|---|---|---|
| 🗜️ **[G7-01] Texture & Sprite Atlas ASTC** | 🗡️ **[G6-02.1] Hoán Đổi Pháp Bảo Điếu Cày Khói Lửa** | 🛡️ **[G0] Cô Lập DontDestroyOnLoad Game Manager** |
| 🧪 **[G7-02] Mobile Stress Test 60 FPS** | ⚡ **[G6-02.2] Hoán Đổi Tuyệt Kỹ Tướng Đạo Sĩ (Lôi Pháp)** | 📦 **[G1] Lõi Loadout Thuần C# & 13 NUnit Tests** |
| 🚀 **[G7-03] Android AAB Build & Signing** | 🏹 **[G6-02.3] Hoán Đổi Lính Xạ Thủ Tại Lều Dân Làng** | 📚 **[G2] Defense3DCatalog, Resolver & Adapters** |
| 🎵 **[G7-04] Tinh Chỉnh SFX/BGM Cổ Phong** | 🧙 **[G6-02.4] Hoán Đổi Model 3D Nhân Vật Theo Tướng** | 💾 **[G3] Atomic Save defense_loadout.json & Meta** |
| | | 🏛️ **[G4] UI Chuẩn Bị Trận & AppFlow Navigation** |
| | | 🏆 **[G5] Dịch Vụ Kết Quả & Trở Về Sảnh An Toàn** |
| | | 🔤 **[FIX-01] Thống Nhất Font TMP GameFont_Vietnamese_SD** |
| | | 🏰 **[FIX-02] Mở Rộng Menu Xây Tháp 3 Hàng Tránh Crash** |
| | | 🎴 **[G6-01] Mở Rộng Catalog Đạo Sĩ, Điếu Cày, Xạ Thủ, Hỏa Long** |

---

## 🛠️ Hướng Dẫn Kiểm Tra Tiến Độ
- Khi hoàn thành tính năng mới, kiểm tra lại với bộ test tự động:
  ```powershell
  powershell -ExecutionPolicy Bypass -File Tools/Invoke-UnityBridge.ps1 -Command validate-app-architecture -TimeoutSeconds 30
  ```
- Luôn đảm bảo tuân thủ nghiêm ngặt các quy tắc trong [`.agents/AGENTS.md`](file:///c:/Users/thuon/Unity/Projectzombie/.agents/AGENTS.md).
