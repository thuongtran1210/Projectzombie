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
    /// Panel chuyên trách quản lý Sidebar danh sách thẻ, tìm kiếm, bộ lọc phân loại và Cây Hệ Sinh Thái Đại Lõi.
    /// </summary>
    public class StudioSidebarPanel
    {
        private readonly Action<UpgradeData> _onSelectUpgrade;
        private readonly Action<UpgradeType> _onCreateTemplate;

        private string _searchQuery = string.Empty;
        private int _selectedCategoryFilter = 0;
        private ElementType _selectedElementFilter = ElementType.None;
        private bool _filterByElement = false;
        private int _sidebarViewMode = 0; // 0 = Phân Loại, 1 = Cây Đại Lõi
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
            GUILayout.Label($"<b>DANH SÁCH THẺ ({filtered.Count}/{allUpgrades.Count})</b>", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+ Tạo Mới", EditorStyles.miniButtonRight, GUILayout.Width(75)))
            {
                ShowCreateMenu();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            // 2. Chế Độ Xem Toolbar
            string[] viewModes = new[] { "📋 Phân Loại", "🌳 Cây Đại Lõi" };
            _sidebarViewMode = GUILayout.Toolbar(_sidebarViewMode, viewModes, EditorStyles.toolbarButton);

            GUILayout.Space(4);

            // 3. Search Bar
            GUILayout.BeginHorizontal();
            _searchQuery = EditorGUILayout.TextField(_searchQuery, EditorStyles.toolbarSearchField);
            if (!string.IsNullOrEmpty(_searchQuery) && GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                _searchQuery = string.Empty;
            }
            GUILayout.EndHorizontal();

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

        private void DrawFlatListView(List<UpgradeData> filtered, UpgradeData selectedUpgrade)
        {
            // Category Filter Dropdown
            GUILayout.BeginHorizontal();
            GUILayout.Label("Phân loại:", EditorStyles.miniLabel, GUILayout.Width(55));
            string[] categories = new[] { "Tất Cả", "Đại Lõi (Mythic)", "Thần Binh Thuật (Traits)", "Pháp Bảo (Weapons)", "Tiến Hóa (Evolutions)", "Khí Vận (Passives)", "Dung Hợp (Fusions)" };
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

            // List View
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            for (int i = 0; i < filtered.Count; i++)
            {
                DrawUpgradeItem(filtered[i], selectedUpgrade, 0);
            }
            GUILayout.EndScrollView();
        }

        private void DrawArchetypeTreeView(List<UpgradeData> allUpgrades, UpgradeData selectedUpgrade)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Bung Tất Cả", EditorStyles.miniButtonLeft)) SetAllFoldouts(true);
            if (GUILayout.Button("Thu Gọn", EditorStyles.miniButtonRight)) SetAllFoldouts(false);
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            _scrollPos = GUILayout.BeginScrollView(_scrollPos);

            DrawArchetypeGroup(allUpgrades, selectedUpgrade, "🌟 Phù Đổng Thiên Uy (Hệ Hỏa)", MythicArchetype.PhuDongThienUy, new[] { "W006", "W008" });
            DrawArchetypeGroup(allUpgrades, selectedUpgrade, "🌟 Kim Quy Thần Cơ (Hệ Kim)", MythicArchetype.KimQuyThanCo, new[] { "W001", "W007" });
            DrawArchetypeGroup(allUpgrades, selectedUpgrade, "🌟 Tản Viên Sơn Thánh (Hệ Thổ)", MythicArchetype.TanVienSonThanh, new[] { "W005" });
            DrawArchetypeGroup(allUpgrades, selectedUpgrade, "🌟 Thủy Bá Cuồng Nộ (Hệ Thủy)", MythicArchetype.ThuyBaCuongNo, new[] { "W009", "W010", "W011" });
            DrawArchetypeGroup(allUpgrades, selectedUpgrade, "🌟 Long Tiên Huyết Mạch (Âm Dương)", MythicArchetype.LongTienHuyetMach, new[] { "W002", "W003", "W004", "W012" });

            DrawSharedPassivesGroup(allUpgrades, selectedUpgrade);
            DrawSlapstickAndFusionsGroup(allUpgrades, selectedUpgrade);

            GUILayout.EndScrollView();
        }

        private void DrawUpgradeItem(UpgradeData u, UpgradeData selectedUpgrade, int indent)
        {
            if (u == null) return;

            bool isSelected = u == selectedUpgrade;
            var prevColor = GUI.backgroundColor;
            if (isSelected) GUI.backgroundColor = new Color(0.2f, 0.6f, 1f, 1f);

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUI.backgroundColor = prevColor;

            if (indent > 0) GUILayout.Space(indent * 12);

            if (u.icon != null && u.icon.texture != null)
            {
                GUILayout.Label(new GUIContent(u.icon.texture), GUILayout.Width(26), GUILayout.Height(26));
            }
            else
            {
                GUILayout.Box("?", GUILayout.Width(26), GUILayout.Height(26));
            }

            GUILayout.BeginVertical();
            string displayName = !string.IsNullOrEmpty(u.upgradeName) ? u.upgradeName : "(Chưa đặt tên)";
            GUILayout.Label($"<b>{u.id}</b> - {displayName}", EditorStyles.label);
            GUILayout.Label($"W: {u.spawnWeight} | Lv: {(u.maxLevel > 0 ? u.maxLevel.ToString() : "∞")}", EditorStyles.miniLabel);
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(isSelected ? "●" : "Chọn", EditorStyles.miniButton, GUILayout.Width(42), GUILayout.Height(26)))
            {
                _onSelectUpgrade?.Invoke(u);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawArchetypeGroup(List<UpgradeData> all, UpgradeData selected, string title, MythicArchetype arc, string[] relatedWeapons)
        {
            string key = $"Group_{arc}";
            if (!_foldoutStates.ContainsKey(key)) _foldoutStates[key] = true;

            _foldoutStates[key] = EditorGUILayout.Foldout(_foldoutStates[key], $"<b>{title}</b>", true, EditorStyles.foldoutHeader);
            if (!_foldoutStates[key]) return;

            var core = all.OfType<MythicCoreUpgradeData>().FirstOrDefault(c => c.archetype == arc);
            if (core != null)
            {
                GUILayout.Label("  👑 <b>ĐẠI LÕI KHỞI ĐẦU:</b>", EditorStyles.miniBoldLabel);
                DrawUpgradeItem(core, selected, 1);
            }

            var traits = all.OfType<SynergyTraitUpgradeData>().Where(t => t.requiredArchetype == arc).ToList();
            if (traits.Count > 0)
            {
                GUILayout.Label($"  ⚡ <b>THẦN BINH THUẬT ĐỘC QUYỀN ({traits.Count}/5):</b>", EditorStyles.miniBoldLabel);
                foreach (var t in traits) DrawUpgradeItem(t, selected, 2);
            }

            if (relatedWeapons != null && relatedWeapons.Length > 0)
            {
                GUILayout.Label("  ⚔️ <b>PHÁP BẢO & TIẾN HÓA ĐỒNG HỆ:</b>", EditorStyles.miniBoldLabel);
                foreach (var wId in relatedWeapons)
                {
                    var weaponUps = all.OfType<WeaponUpgradeData>().Where(w => string.Equals(w.weaponId, wId, StringComparison.OrdinalIgnoreCase)).ToList();
                    var evos = all.OfType<EvolutionUpgradeData>().Where(e => string.Equals(e.weaponId, wId, StringComparison.OrdinalIgnoreCase)).ToList();
                    foreach (var w in weaponUps) DrawUpgradeItem(w, selected, 2);
                    foreach (var e in evos) DrawUpgradeItem(e, selected, 2);
                }
            }

            GUILayout.Space(6);
        }

        private void DrawSharedPassivesGroup(List<UpgradeData> all, UpgradeData selected)
        {
            string key = "Group_Passives";
            if (!_foldoutStates.ContainsKey(key)) _foldoutStates[key] = true;

            _foldoutStates[key] = EditorGUILayout.Foldout(_foldoutStates[key], "<b>📜 Bổ Trợ Khí Vận Dùng Chung (P001 - P012)</b>", true, EditorStyles.foldoutHeader);
            if (!_foldoutStates[key]) return;

            var passives = all.OfType<CommonUpgradeData>().ToList();
            foreach (var p in passives) DrawUpgradeItem(p, selected, 1);
            GUILayout.Space(6);
        }

        private void DrawSlapstickAndFusionsGroup(List<UpgradeData> all, UpgradeData selected)
        {
            string key = "Group_Slapstick";
            if (!_foldoutStates.ContainsKey(key)) _foldoutStates[key] = false;

            _foldoutStates[key] = EditorGUILayout.Foldout(_foldoutStates[key], "<b>🔮 Pháp Bảo Dân Gian & Dung Hợp</b>", true, EditorStyles.foldoutHeader);
            if (!_foldoutStates[key]) return;

            var fusions = all.OfType<FusionUpgradeData>().ToList();
            if (fusions.Count > 0)
            {
                GUILayout.Label("  🔮 <b>THẦN KHÍ DUNG HỢP (RELIC FUSIONS):</b>", EditorStyles.miniBoldLabel);
                foreach (var f in fusions) DrawUpgradeItem(f, selected, 1);
            }

            var slapstick = all.Where(u => u.id.StartsWith("UP_SLIPPER") || u.id.StartsWith("UP_POT") || u.id.StartsWith("UP_PIPE") || u.id.StartsWith("UP_R007") || u.id.StartsWith("UP_R008")).ToList();
            if (slapstick.Count > 0)
            {
                GUILayout.Label("  🎭 <b>PHÁP BẢO DÂN GIAN HÀI HƯỚC:</b>", EditorStyles.miniBoldLabel);
                foreach (var s in slapstick) DrawUpgradeItem(s, selected, 1);
            }
            GUILayout.Space(6);
        }

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
                    case 2: if (!(u is SynergyTraitUpgradeData || u.upgradeType == UpgradeType.SynergyTrait)) return false; break;
                    case 3: if (!(u is WeaponUpgradeData || u.upgradeType == UpgradeType.WeaponUpgrade)) return false; break;
                    case 4: if (!(u is EvolutionUpgradeData || u.upgradeType == UpgradeType.EvolutionUpgrade)) return false; break;
                    case 5: if (!(u is CommonUpgradeData || u.upgradeType == UpgradeType.CommonUpgrade || u.upgradeType == UpgradeType.RareUpgrade)) return false; break;
                    case 6: if (!(u is FusionUpgradeData || u.upgradeType == UpgradeType.RelicFusion)) return false; break;
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
            menu.AddItem(new GUIContent("🌟 Đại Lõi Thần Thoại (Mythic Core)"), false, () => _onCreateTemplate?.Invoke(UpgradeType.MythicCore));
            menu.AddItem(new GUIContent("✨ Thần Binh Thuật (Synergy Trait)"), false, () => _onCreateTemplate?.Invoke(UpgradeType.SynergyTrait));
            menu.AddItem(new GUIContent("⚔️ Cường Hóa Pháp Bảo (Weapon Upgrade)"), false, () => _onCreateTemplate?.Invoke(UpgradeType.WeaponUpgrade));
            menu.AddItem(new GUIContent("🔥 Thần Pháp Tiến Hóa (Evolution)"), false, () => _onCreateTemplate?.Invoke(UpgradeType.EvolutionUpgrade));
            menu.AddItem(new GUIContent("📜 Bổ Trợ Khí Vận (Common/Passive)"), false, () => _onCreateTemplate?.Invoke(UpgradeType.CommonUpgrade));
            menu.AddItem(new GUIContent("🔮 Dung Hợp Pháp Bảo (Relic Fusion)"), false, () => _onCreateTemplate?.Invoke(UpgradeType.RelicFusion));
            menu.ShowAsContext();
        }
    }
}
#endif
