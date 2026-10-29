# Kho Lưu Trữ Tài Nguyên 2D Cũ (Legacy 2D Assets Archive)

Thư mục này lưu trữ toàn bộ các Prefabs, ScriptableObjects (SO), và Visual Effects (VFX) thuộc hệ thống sinh tồn 2D cũ (Survival Roguelite Vampire-Survivors style), phục vụ việc tra cứu công thức, lore, chỉ số thiết kế ban đầu khi cần mà không làm rác Asset Database hoặc phình to kích thước build Android APK của hệ thống Thủ Thành 3D (**Action Tower Defense 3D**).

---

## Cấu Trúc Thư Mục Lưu Trữ

```text
Assets/Archive_Legacy_2D/
├── Data/
│   ├── Weapons/         # 17 Weapon ScriptableObjects (W001 -> W012, R007, R008, W_PIPE, W_POT, W_SLIPPER)
│   ├── Skills/          # 4 Signature Skill ScriptableObjects của 4 nhân vật 2D
│   └── Stages/          # Cấu hình ải 2D cũ (Stage 1, 2, 3 và Level1_Timeline)
├── Prefabs/
│   └── Characters/      # 4 Prefab Player 2D (Ẩn Sĩ, Đạo Sĩ, Thanh Đồng, Thư Sinh)
└── VFX/
    ├── Augments/        # 13 Prefab hiệu ứng bổ trợ Augment 2D thử nghiệm
    └── SkillLibrary/    # 33 Prefab VFX chiêu thức & đường đạn của 12 vũ khí và 4 nhân vật 2D
```

---

## Ánh Xạ Chuyển Đổi Sang Kiến Trúc 3D (Migration Mapping)

| Hệ thống 2D Cũ (Đã Archive) | Hệ thống 3D Defense Hiện Hành (Active) | Vị trí Active mới |
| :--- | :--- | :--- |
| **Vũ khí Roguelite (W001 - W012)** | Trụ Tháp Phòng Thủ (`SO_TowerData.cs`) | `Assets/_Data/Defense3D/Towers/` & `Assets/_Prefabs/Defense3D/Towers/` |
| **Kỹ năng 2D (`Skills/`)** | Kỹ năng Tướng 3D (`DefenseSkillDefinitionSO.cs`) | `Assets/Resources/Defense3D/Skill_*.asset` |
| **Nhân vật Player 2D** | Tướng 3D (`Hero_3D_Prototype`, `HeroKit`) | `Assets/Resources/Defense3D/HeroKit_ThuSinh.asset` & Scene `MapSongQue3D_Prototype` |
| **Ải 2D (Stage 1, 2, 3)** | Chiến trường Thủ Thành Sông Quê 3D | `Assets/_Data/Levels/Stages/Stage_04_SongQue_Defense3D.asset` |
| **Quái 2D (E_MAGIAP, E_MADA)** | Quái 3D Defense (`P_Enemy_Base` & Variants) | `Assets/_Prefabs/Defense3D/Enemies/` & `Assets/Resources/Defense3D/` |
| **Giao diện UIRegistry** | Chuẩn hóa UI duy nhất | `Assets/_Prefabs/UI/` kết nối qua `Assets/Resources/UI/UIRegistry.asset` |
