#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades.Editor.Studio.Panels
{
    /// <summary>
    /// Panel chuyên trách Sidebar: Danh sách Flat View và Cây Hệ Sinh Thái Đại Lõi Trực Quan (Archetype Build Tree Visualizer).
    /// </summary>
    public class StudioSidebarPanel
    {
        private readonly Action<UpgradeData> _onSelectUpgrade;
        private readonly Action<UpgradeType> _onCreateTemplate;

        private string _searchQuery = string.Empty;
        private int _selectedCategoryFilter = 0;
        private ElementType _selectedElementFilter = ElementType.None;
        private bool _filterByElement = false;
        private int _sidebarViewMode = 1; // Mặc định mở 1 = Cây Đại Lõi trực quan
        private readonly Dictionary<string, bool> _foldoutStates = new Dictionary<string, bool>();
        private Vector2 _scrollPos;

        public StudioSidebarPanel(Action<UpgradeData> onSelectUpgrade, Action<UpgradeType> onCreateTemplate)
        {
            _onSelectUpgrade = onSelectUpgrade;
            _onCreateTemplate = onCreateTemplate;
        }

        public void Draw(List<UpgradeData> allUpgrades, UpgradeData selectedUpgrade, GUILayoutOption widthOption)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox, widthOption);

            // 1. Header & New Template Button
            GUILayout.BeginHorizontal();
            var filtered = FilterUpgrades(allUpgrades);
            GUILayout.Label($"<b>HỆ THỐNG NÂNG CẤP ({filtered.Count}/{allUpgrades.Count})</b>", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+ Tạo Mới", EditorStyles.miniButtonRight, GUILayout.Width(75)))
            {
                ShowCreateMenu();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            // 2. Chế Độ Xem Toolbar
            string[] viewModes = new[] { "🌳 Cây Đại Lõi (Build Tree)", "📋 Danh Sách (Flat)" };
            int newViewMode = GUILayout.Toolbar(_sidebarViewMode == 1 ? 0 : 1, viewModes, EditorStyles.toolbarButton);
            _sidebarViewMode = newViewMode == 0 ? 1 : 0;

            GUILayout.Space(4);

            // 3. Search Bar
            GUILayout.BeginHorizontal();
            _searchQuery = EditorGUILayout.TextField(_searchQuery, EditorStyles.toolbarSearchField);
            if (!string.IsNullOrEmpty(_searchQuery) && GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                _searchQuery = string.Empty;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            if (_sidebarViewMode == 0)
            {
                DrawFlatListView(filtered, selectedUpgrade);
            }
            else
            {
                DrawArchetypeTreeView(allUpgrades, selectedUpgrade);
            }

            GUILayout.EndVertical();
        }

        #region Flat List View
        private void DrawFlatListView(List<UpgradeData> filtered, UpgradeData selectedUpgrade)
        {
            // Category Filter Dropdown
            GUILayout.BeginHorizontal();
            GUILayout.Label("Phân loại:", EditorStyles.miniLabel, GUILayout.Width(55));
            string[] categories = new[]
            {
                "Tất Cả",
                "Đại Lõi (Mythic - Lv.1)",
                "Lõi Đột Biến (Augments - Lv.5/15/30)",
                "Chỉ Số Nền Tảng (Micro-Cards)",
                "Thần Binh Thuật (Traits)",
                "Pháp Bảo (Weapons)",
                "Tiến Hóa (Evolutions)",
                "Khí Vận (Passives)",
                "Dung Hợp (Fusions)"
            };
            _selectedCategoryFilter = EditorGUILayout.Popup(_selectedCategoryFilter, categories, EditorStyles.miniPullDown);
            GUILayout.EndHorizontal();

            // Element Filter
            GUILayout.BeginHorizontal();
            _filterByElement = EditorGUILayout.ToggleLeft("Hệ:", _filterByElement, GUILayout.Width(45));
            if (_filterByElement)
            {
                _selectedElementFilter = (ElementType)EditorGUILayout.EnumPopup(_selectedElementFilter, EditorStyles.miniPullDown);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            for (int i = 0; i < filtered.Count; i++)
            {
                DrawUpgradeItem(filtered[i], selectedUpgrade, 0);
            }
            GUILayout.EndScrollView();
        }
        #endregion

        #region Archetype Tree View (Trực Quan Hóa Cây Đại Lõi & Tiến Trình)
        private void DrawArchetypeTreeView(List<UpgradeData> allUpgrades, UpgradeData selectedUpgrade)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("📂 Mở Tất Cả", EditorStyles.miniButtonLeft)) SetAllFoldouts(true);
            if (GUILayout.Button("📁 Thu Gọn", EditorStyles.miniButtonRight)) SetAllFoldouts(false);
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            _scrollPos = GUILayout.BeginScrollView(_scrollPos);

            // 1. Kho Lõi Đột Biến Cao Trào (Mốc Lv. 5, 15, 30)
            DrawMutationAugmentsSection(allUpgrades, selectedUpgrade);

            // 2. Kho Thẻ Chỉ Số Nền Tảng (Level Thường Micro-Cards)
            DrawStatMicroSection(allUpgrades, selectedUpgrade);

            // 3. Phù Đổng Thiên Uy (Hỏa)
            DrawArchetypeSection(
                allUpgrades, selectedUpgrade,
                MythicArchetype.PhuDongThienUy,
                "🔥 PHÙ ĐỔNG THIÊN UY",
                "Hệ Hỏa • Sát Thương Bùng Nổ & Cháy Lan",
                new Color(1f, 0.35f, 0.2f),
                new[] { "W006", "W008" }
            );

            // 4. Kim Quy Thần Cơ (Kim)
            DrawArchetypeSection(
                allUpgrades, selectedUpgrade,
                MythicArchetype.KimQuyThanCo,
                "✨ KIM QUY THẦN CƠ",
                "Hệ Kim • Đạn Liên Hoàn, Nỏ Thần & Cơ Quan",
                new Color(1f, 0.85f, 0.2f),
                new[] { "W001", "W007" }
            );

            // 5. Tản Viên Sơn Thánh (Thổ)
            DrawArchetypeSection(
                allUpgrades, selectedUpgrade,
                MythicArchetype.TanVienSonThanh,
                "⛰️ TẢN VIÊN SƠN THÁNH",
                "Hệ Thổ • Phòng Ngự Cương Thể & Địa Chấn",
                new Color(0.85f, 0.65f, 0.3f),
                new[] { "W005" }
            );

            // 6. Thủy Bá Cuồng Nộ (Thủy)
            DrawArchetypeSection(
                allUpgrades, selectedUpgrade,
                MythicArchetype.ThuyBaCuongNo,
                "🌊 THỦY BÁ CUỒNG NỘ",
                "Hệ Thủy • Triều Dâng, Đóng Băng & Làm Chậm",
                new Color(0.2f, 0.75f, 1f),
                new[] { "W009", "W010", "W011" }
            );

            // 7. Long Tiên Huyết Mạch (Âm Dương)
            DrawArchetypeSection(
                allUpgrades, selectedUpgrade,
                MythicArchetype.LongTienHuyetMach,
                "☯️ LONG TIÊN HUYẾT MẠCH",
                "Âm Dương Vô Cực • Khí Công, Thần Kiếm & Chấn Động",
                new Color(0.85f, 0.45f, 1f),
                new[] { "W002", "W003", "W004", "W012" }
            );

            // 8. Nhóm Khí Vận Dùng Chung (Passives)
            DrawSharedPassivesSection(allUpgrades, selectedUpgrade);

            // 9. Nhóm Pháp Bảo Dân Gian & Dung Hợp
            DrawSlapstickAndFusionsSection(allUpgrades, selectedUpgrade);

            GUILayout.EndScrollView();
        }

        private void DrawMutationAugmentsSection(List<UpgradeData> all, UpgradeData selected)
        {
            string key = "Arc_Mutations";
            if (!_foldoutStates.ContainsKey(key)) _foldoutStates[key] = true;

            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.6f, 0.15f, 0.75f, 0.6f);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = prevBg;

            var augments = all.OfType<MutationAugmentUpgradeData>().ToList();
            GUILayout.BeginHorizontal();
            _foldoutStates[key] = EditorGUILayout.Foldout(
                _foldoutStates[key],
                $"<b><color=#00E5FF>⚡ KHO LÕI ĐỘT BIẾN ({augments.Count} THẺ)</color></b>",
                true,
                new GUIStyle(EditorStyles.foldout) { richText = true, fontStyle = FontStyle.Bold }
            );
            GUILayout.FlexibleSpace();
            GUILayout.Label("<color=#AAAAAA><size=10>Cột Mốc Lv.5, 15, 30</size></color>", new GUIStyle(EditorStyles.miniLabel) { richText = true });
            GUILayout.EndHorizontal();

            if (_foldoutStates[key])
            {
                GUILayout.Space(4);

                // Prismatic
                var prismatic = augments.Where(a => a.tier == AugmentTier.Prismatic).ToList();
                if (prismatic.Count > 0)
                {
                    DrawTreeNodeHeader($"💎 LÕI KIM CƯƠNG THẦN HÓA ({prismatic.Count})", new Color(0f, 0.9f, 1f));
                    foreach (var a in prismatic) DrawUpgradeItem(a, selected, 1, "💎 [KIM CƯƠNG]");
                }

                // Gold
                var gold = augments.Where(a => a.tier == AugmentTier.Gold).ToList();
                if (gold.Count > 0)
                {
                    DrawTreeNodeHeader($"🟡 LÕI VÀNG CƯỜNG HÓA GIAO TRANH ({gold.Count})", new Color(1f, 0.85f, 0.2f));
                    foreach (var a in gold) DrawUpgradeItem(a, selected, 1, "🟡 [LÕI VÀNG]");
                }

                // Silver
                var silver = augments.Where(a => a.tier == AugmentTier.Silver).ToList();
                if (silver.Count > 0)
                {
                    DrawTreeNodeHeader($"⚪ LÕI BẠC CHỈ SỐ & TIỆN ÍCH ({silver.Count})", new Color(0.85f, 0.85f, 0.85f));
                    foreach (var a in silver) DrawUpgradeItem(a, selected, 1, "⚪ [LÕI BẠC]");
                }

                if (augments.Count == 0)
                {
                    GUILayout.Label("<i>Chưa có Lõi Đột Biến nào. Nhấn '+ Tạo Mới' để thêm.</i>", EditorStyles.miniLabel);
                }
                else
                {
                    GUILayout.Space(4);
                    if (GUILayout.Button("⚡ Sinh/Đồng Bộ 12 Prefabs Cơ Chế (Vàng & Kim Cương)", EditorStyles.miniButton))
                    {
                        AugmentPrefabsGenerator.GenerateAllAugmentPrefabs();
                    }
                }
            }

            GUILayout.EndVertical();
            GUILayout.Space(6);
        }

        private void DrawStatMicroSection(List<UpgradeData> all, UpgradeData selected)
        {
            string key = "Arc_MicroStats";
            if (!_foldoutStates.ContainsKey(key)) _foldoutStates[key] = false;

            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.1f, 0.5f, 0.35f, 0.5f);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = prevBg;

            var microStats = all.OfType<StatMicroUpgradeData>().ToList();
            GUILayout.BeginHorizontal();
            _foldoutStates[key] = EditorGUILayout.Foldout(
                _foldoutStates[key],
                $"<b><color=#00FF88>⚙️ THẺ CHỈ SỐ NỀN TẢNG ({microStats.Count} THẺ)</color></b>",
                true,
                new GUIStyle(EditorStyles.foldout) { richText = true, fontStyle = FontStyle.Bold }
            );
            GUILayout.FlexibleSpace();
            GUILayout.Label("<color=#AAAAAA><size=10>Micro-Cards Level Thường</size></color>", new GUIStyle(EditorStyles.miniLabel) { richText = true });
            GUILayout.EndHorizontal();

            if (_foldoutStates[key])
            {
                GUILayout.Space(4);
                for (int i = 0; i < microStats.Count; i++)
                {
                    var m = microStats[i];
                    string prefix = m.preferredArchetype == MythicArchetype.None ? "⚙️ [CHUNG]" : $"⚙️ [{m.preferredArchetype.GetDisplayName()}]";
                    DrawUpgradeItem(m, selected, 1, prefix);
                }

                if (microStats.Count == 0)
                {
                    GUILayout.Label("<i>Chưa có Micro-Card nào. Nhấn '+ Tạo Mới' để thêm.</i>", EditorStyles.miniLabel);
                }
            }

            GUILayout.EndVertical();
            GUILayout.Space(6);
        }

        private void DrawArchetypeSection(
            List<UpgradeData> all,
            UpgradeData selected,
            MythicArchetype arc,
            string title,
            string subtitle,
            Color accentColor,
            string[] relatedWeapons)
        {
            string key = $"Arc_{arc}";
            if (!_foldoutStates.ContainsKey(key)) _foldoutStates[key] = true;

            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = Color.Lerp(Color.black, accentColor, 0.25f);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = prevBg;

            // Header Banner
            GUILayout.BeginHorizontal();
            _foldoutStates[key] = EditorGUILayout.Foldout(_foldoutStates[key], $"<b><color=#{ColorUtility.ToHtmlStringRGB(accentColor)}>{title}</color></b>", true, new GUIStyle(EditorStyles.foldout) { richText = true, fontStyle = FontStyle.Bold });
            GUILayout.FlexibleSpace();
            GUILayout.Label($"<color=#AAAAAA><size=10>{subtitle}</size></color>", new GUIStyle(EditorStyles.miniLabel) { richText = true });
            GUILayout.EndHorizontal();

            if (_foldoutStates[key])
            {
                GUILayout.Space(4);

                // 1. Root Node: Đại Lõi Khởi Đầu
                var core = all.OfType<MythicCoreUpgradeData>().FirstOrDefault(c => c.archetype == arc);
                if (core != null)
                {
                    DrawTreeNodeHeader("👑 ĐẠI LÕI KHỞI ĐẦU (LEVEL 1)", accentColor);
                    DrawUpgradeItem(core, selected, 1, "⭐ [ĐẠI LÕI]");
                }

                // 2. 5 Thần Binh Thuật Độc Quyền
                var traits = all.OfType<SynergyTraitUpgradeData>().Where(t => t.requiredArchetype == arc).ToList();
                if (traits.Count > 0)
                {
                    DrawTreeNodeHeader($"⚡ THẦN BINH THUẬT ĐỘC QUYỀN ({traits.Count}/5)", accentColor);
                    for (int i = 0; i < traits.Count; i++)
                    {
                        string prefix = (i == traits.Count - 1) ? "└── " : "├── ";
                        DrawUpgradeItem(traits[i], selected, 2, $"{prefix}T{i + 1}");
                    }
                }

                // 3. Chuỗi Pháp Bảo & Tiến Hóa Thần Binh
                if (relatedWeapons != null && relatedWeapons.Length > 0)
                {
                    DrawTreeNodeHeader("⚔️ CHUỖI TIẾN HÓA PHÁP BẢO ĐỒNG HỆ", accentColor);

                    foreach (var wId in relatedWeapons)
                    {
                        var weaponUps = all.OfType<WeaponUpgradeData>().Where(w => string.Equals(w.weaponId, wId, StringComparison.OrdinalIgnoreCase)).ToList();
                        var evos = all.OfType<EvolutionUpgradeData>().Where(e => string.Equals(e.weaponId, wId, StringComparison.OrdinalIgnoreCase)).ToList();

                        if (weaponUps.Count > 0 || evos.Count > 0)
                        {
                            GUILayout.BeginVertical(EditorStyles.helpBox);

                            // Pháp Bảo Gốc
                            foreach (var w in weaponUps)
                            {
                                DrawUpgradeItem(w, selected, 1, "⚔️ [GỐC]");
                            }

                            // Tiến Hóa
                            for (int eIdx = 0; eIdx < evos.Count; eIdx++)
                            {
                                var evo = evos[eIdx];
                                string passReq = !string.IsNullOrEmpty(evo.requiredPassiveId) ? $"+ {evo.requiredPassiveId}" : "";
                                string branchPrefix = (eIdx == evos.Count - 1) ? "  └── 🔥 [TIẾN HÓA] " : "  ├── 🔥 [TIẾN HÓA] ";
                                DrawUpgradeItem(evo, selected, 2, $"{branchPrefix}{passReq}");
                            }

                            GUILayout.EndVertical();
                            GUILayout.Space(2);
                        }
                    }
                }
            }

            GUILayout.EndVertical();
            GUILayout.Space(6);
        }

        private void DrawTreeNodeHeader(string title, Color accentColor)
        {
            GUILayout.Space(2);
            GUILayout.Label($"<b><color=#{ColorUtility.ToHtmlStringRGB(accentColor)}>{title}</color></b>", new GUIStyle(EditorStyles.miniBoldLabel) { richText = true });
        }

        private void DrawSharedPassivesSection(List<UpgradeData> all, UpgradeData selected)
        {
            string key = "Arc_Passives";
            if (!_foldoutStates.ContainsKey(key)) _foldoutStates[key] = false;

            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.2f, 0.35f, 0.45f, 0.5f);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = prevBg;

            GUILayout.BeginHorizontal();
            _foldoutStates[key] = EditorGUILayout.Foldout(_foldoutStates[key], "<b>📜 BỔ TRỢ KHÍ VẬN DÙNG CHUNG (P001 - P012)</b>", true, new GUIStyle(EditorStyles.foldout) { richText = true, fontStyle = FontStyle.Bold });
            GUILayout.FlexibleSpace();
            GUILayout.Label("<color=#AAAAAA><size=10>Nền tảng ghép Tiến Hóa</size></color>", new GUIStyle(EditorStyles.miniLabel) { richText = true });
            GUILayout.EndHorizontal();

            if (_foldoutStates[key])
            {
                GUILayout.Space(4);
                var passives = all.OfType<CommonUpgradeData>().ToList();
                for (int i = 0; i < passives.Count; i++)
                {
                    DrawUpgradeItem(passives[i], selected, 1, $"P{i + 1:00}");
                }
            }

            GUILayout.EndVertical();
            GUILayout.Space(6);
        }

        private void DrawSlapstickAndFusionsSection(List<UpgradeData> all, UpgradeData selected)
        {
            string key = "Arc_Slapstick";
            if (!_foldoutStates.ContainsKey(key)) _foldoutStates[key] = false;

            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.4f, 0.25f, 0.45f, 0.5f);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = prevBg;

            GUILayout.BeginHorizontal();
            _foldoutStates[key] = EditorGUILayout.Foldout(_foldoutStates[key], "<b>🔮 THẦN KHÍ DUNG HỢP & PHÁP BẢO DÂN GIAN</b>", true, new GUIStyle(EditorStyles.foldout) { richText = true, fontStyle = FontStyle.Bold });
            GUILayout.FlexibleSpace();
            GUILayout.Label("<color=#AAAAAA><size=10>Thần Khí & Vũ Khí Ẩn</size></color>", new GUIStyle(EditorStyles.miniLabel) { richText = true });
            GUILayout.EndHorizontal();

            if (_foldoutStates[key])
            {
                GUILayout.Space(4);
                var fusions = all.OfType<FusionUpgradeData>().ToList();
                if (fusions.Count > 0)
                {
                    GUILayout.Label("<b>🔮 THẦN KHÍ DUNG HỢP (RELIC FUSIONS):</b>", EditorStyles.miniBoldLabel);
                    foreach (var f in fusions) DrawUpgradeItem(f, selected, 1, "🌟 [HỢP THỂ]");
                }

                var slapstick = all.Where(u => u.id.StartsWith("UP_SLIPPER") || u.id.StartsWith("UP_POT") || u.id.StartsWith("UP_PIPE") || u.id.StartsWith("UP_R007") || u.id.StartsWith("UP_R008")).ToList();
                if (slapstick.Count > 0)
                {
                    GUILayout.Label("<b>🎭 PHÁP BẢO DÂN GIAN HÀI HƯỚC:</b>", EditorStyles.miniBoldLabel);
                    foreach (var s in slapstick) DrawUpgradeItem(s, selected, 1, "🎭 [ẨN]");
                }
            }

            GUILayout.EndVertical();
            GUILayout.Space(6);
        }
        #endregion

        #region Upgrade Item Rendering
        private void DrawUpgradeItem(UpgradeData u, UpgradeData selectedUpgrade, int indent, string badge = null)
        {
            if (u == null) return;

            // Kiểm tra tìm kiếm nếu đang ở Tree View
            if (!string.IsNullOrEmpty(_searchQuery))
            {
                bool matchId = !string.IsNullOrEmpty(u.id) && u.id.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0;
                bool matchName = !string.IsNullOrEmpty(u.upgradeName) && u.upgradeName.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0;
                if (!matchId && !matchName) return;
            }

            bool isSelected = u == selectedUpgrade;
            var prevBg = GUI.backgroundColor;
            if (isSelected) GUI.backgroundColor = new Color(0.15f, 0.6f, 1f, 1f);

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUI.backgroundColor = prevBg;

            if (indent > 0)
            {
                GUILayout.Space(indent * 8);
            }

            // Badge / Tree Branch Prefix
            if (!string.IsNullOrEmpty(badge))
            {
                GUILayout.Label($"<color=#00E5FF><b>{badge}</b></color>", new GUIStyle(EditorStyles.miniLabel) { richText = true }, GUILayout.Width(80));
            }

            // Mini Icon
            if (u.icon != null && u.icon.texture != null)
            {
                GUILayout.Label(new GUIContent(u.icon.texture), GUILayout.Width(22), GUILayout.Height(22));
            }
            else
            {
                GUILayout.Box("?", GUILayout.Width(22), GUILayout.Height(22));
            }

            // ID & Name
            GUILayout.BeginVertical();
            string displayName = !string.IsNullOrEmpty(u.upgradeName) ? u.upgradeName : "(Chưa đặt tên)";
            GUILayout.Label($"<b>{u.id}</b> - {displayName}", EditorStyles.label);
            GUILayout.Label($"Hệ: {u.element} | W: {u.spawnWeight} | Lv: {(u.maxLevel > 0 ? u.maxLevel.ToString() : "∞")}", EditorStyles.miniLabel);
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            // Select Button
            if (GUILayout.Button(isSelected ? "●" : "Sửa", EditorStyles.miniButton, GUILayout.Width(38), GUILayout.Height(22)))
            {
                _onSelectUpgrade?.Invoke(u);
            }

            GUILayout.EndHorizontal();
        }
        #endregion

        #region Helpers
        private List<UpgradeData> FilterUpgrades(List<UpgradeData> all)
        {
            return all.Where(u =>
            {
                if (u == null) return false;

                if (!string.IsNullOrEmpty(_searchQuery))
                {
                    bool matchId = !string.IsNullOrEmpty(u.id) && u.id.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0;
                    bool matchName = !string.IsNullOrEmpty(u.upgradeName) && u.upgradeName.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!matchId && !matchName) return false;
                }

                switch (_selectedCategoryFilter)
                {
                    case 1: if (!(u is MythicCoreUpgradeData || u.upgradeType == UpgradeType.MythicCore)) return false; break;
                    case 2: if (!(u is MutationAugmentUpgradeData || u.upgradeType == UpgradeType.BreakthroughUltimate)) return false; break;
                    case 3: if (!(u is StatMicroUpgradeData)) return false; break;
                    case 4: if (!(u is SynergyTraitUpgradeData || u.upgradeType == UpgradeType.SynergyTrait)) return false; break;
                    case 5: if (!(u is WeaponUpgradeData || u.upgradeType == UpgradeType.WeaponUpgrade)) return false; break;
                    case 6: if (!(u is EvolutionUpgradeData || u.upgradeType == UpgradeType.EvolutionUpgrade)) return false; break;
                    case 7: if (!(u is CommonUpgradeData || u.upgradeType == UpgradeType.CommonUpgrade || u.upgradeType == UpgradeType.RareUpgrade)) return false; break;
                    case 8: if (!(u is FusionUpgradeData || u.upgradeType == UpgradeType.RelicFusion)) return false; break;
                }

                if (_filterByElement && u.element != _selectedElementFilter) return false;

                return true;
            }).ToList();
        }

        private void SetAllFoldouts(bool state)
        {
            var keys = new List<string>(_foldoutStates.Keys);
            foreach (var k in keys) _foldoutStates[k] = state;
        }

        private void ShowCreateMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("🌟 Đại Lõi Khởi Nguyên (Level 1 Mythic Core)"), false, () => _onCreateTemplate?.Invoke(UpgradeType.MythicCore));
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("⚡ Lõi Đột Biến (Mốc Lv.5/15/30)/💎 Lõi Kim Cương (Prismatic)"), false, () =>
            {
                var asset = UpgradeStudioTemplateFactory.CreateMutationAugmentTemplate(AugmentTier.Prismatic);
                _onSelectUpgrade?.Invoke(asset);
            });
            menu.AddItem(new GUIContent("⚡ Lõi Đột Biến (Mốc Lv.5/15/30)/🟡 Lõi Vàng (Gold)"), false, () =>
            {
                var asset = UpgradeStudioTemplateFactory.CreateMutationAugmentTemplate(AugmentTier.Gold);
                _onSelectUpgrade?.Invoke(asset);
            });
            menu.AddItem(new GUIContent("⚡ Lõi Đột Biến (Mốc Lv.5/15/30)/⚪ Lõi Bạc (Silver)"), false, () =>
            {
                var asset = UpgradeStudioTemplateFactory.CreateMutationAugmentTemplate(AugmentTier.Silver);
                _onSelectUpgrade?.Invoke(asset);
            });
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("⚙️ Thẻ Chỉ Số Nền Tảng (Level Thường Micro-Card)"), false, () =>
            {
                var asset = UpgradeStudioTemplateFactory.CreateStatMicroTemplate();
                _onSelectUpgrade?.Invoke(asset);
            });
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("✨ Thần Binh Thuật (Synergy Trait)"), false, () => _onCreateTemplate?.Invoke(UpgradeType.SynergyTrait));
            menu.AddItem(new GUIContent("⚔️ Cường Hóa Pháp Bảo (Weapon Upgrade)"), false, () => _onCreateTemplate?.Invoke(UpgradeType.WeaponUpgrade));
            menu.AddItem(new GUIContent("🔥 Thần Pháp Tiến Hóa (Evolution)"), false, () => _onCreateTemplate?.Invoke(UpgradeType.EvolutionUpgrade));
            menu.AddItem(new GUIContent("📜 Bổ Trợ Khí Vận (Common/Passive)"), false, () => _onCreateTemplate?.Invoke(UpgradeType.CommonUpgrade));
            menu.AddItem(new GUIContent("🔮 Dung Hợp Pháp Bảo (Relic Fusion)"), false, () => _onCreateTemplate?.Invoke(UpgradeType.RelicFusion));
            menu.ShowAsContext();
        }
        #endregion
    }
}
#endif
