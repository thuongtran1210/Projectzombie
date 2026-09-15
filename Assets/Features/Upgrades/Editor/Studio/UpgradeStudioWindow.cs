#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades.Editor.Studio
{
    /// <summary>
    /// Bảng điều khiển Studio trực quan toàn diện dành cho Game Designer thiết kế, cân bằng và giả lập Thẻ Nâng Cấp in-game.
    /// </summary>
    public class UpgradeStudioWindow : EditorWindow
    {
        private enum MainTab
        {
            StudioEditor,
            GachaSimulator,
            HealthAudit
        }

        private MainTab _currentTab = MainTab.StudioEditor;

        // Dữ liệu Database
        private List<UpgradeData> _allUpgrades = new List<UpgradeData>();
        private List<UpgradeData> _filteredUpgrades = new List<UpgradeData>();
        private UpgradeData _selectedUpgrade;
        private UnityEditor.Editor _cachedEditor;

        // Bộ lọc & Tìm kiếm
        private string _searchQuery = string.Empty;
        private int _selectedCategoryFilter = 0; // 0 = Tất cả, 1 = Mythic, 2 = Traits, 3 = Weapons, 4 = Evolutions, 5 = Passives, 6 = Fusions
        private ElementType _selectedElementFilter = ElementType.None;
        private bool _filterByElement = false;

        // Scroll Views
        private Vector2 _sidebarScroll;
        private Vector2 _editorScroll;
        private Vector2 _previewScroll;
        private Vector2 _auditScroll;
        private Vector2 _simScroll;

        // Simulator State
        private MythicArchetype _simArchetype = MythicArchetype.PhuDongThienUy;
        private int _simRollCount = 500;
        private SimulationResult? _lastSimResult;

        // Audit State
        private List<AuditIssue> _auditIssues = new List<AuditIssue>();
        private int _auditSeverityFilter = 0; // 0 = Tất cả, 1 = Chỉ Errors, 2 = Chỉ Warnings
        private AuditCategory _auditCategoryFilter = AuditCategory.All;
        private string _auditSearchQuery = string.Empty;

        [MenuItem("ProjectZombie/🛠️ Upgrade Studio (Game Designer Dashboard)", priority = -200)]
        public static void ShowWindow()
        {
            var window = GetWindow<UpgradeStudioWindow>("Upgrade Studio");
            window.minSize = new Vector2(980, 620);
            window.Show();
        }

        private void OnEnable()
        {
            ReloadAllAssets();
        }

        private void OnDisable()
        {
            if (_cachedEditor != null)
            {
                DestroyImmediate(_cachedEditor);
                _cachedEditor = null;
            }
        }

        private void ReloadAllAssets()
        {
            _allUpgrades.Clear();
            
            // Ưu tiên nạp từ thư mục Master Source: Assets/_Data/Upgrades
            string[] guids = AssetDatabase.FindAssets("t:UpgradeData", new[] { "Assets/_Data/Upgrades" });
            if (guids.Length == 0)
            {
                guids = AssetDatabase.FindAssets("t:UpgradeData", new[] { "Assets/Resources/Upgrades" });
            }

            var loadedPaths = new HashSet<string>();
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (loadedPaths.Add(path))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<UpgradeData>(path);
                    if (asset != null && !_allUpgrades.Contains(asset))
                    {
                        _allUpgrades.Add(asset);
                    }
                }
            }

            // Sắp xếp mặc định theo ID
            _allUpgrades.Sort((a, b) => string.Compare(a.id, b.id, StringComparison.OrdinalIgnoreCase));
            ApplyFilter();

            if (_selectedUpgrade == null && _filteredUpgrades.Count > 0)
            {
                SelectUpgrade(_filteredUpgrades[0]);
            }
        }

        private void ApplyFilter()
        {
            _filteredUpgrades = _allUpgrades.Where(u =>
            {
                if (u == null) return false;

                // 1. Lọc theo tìm kiếm
                if (!string.IsNullOrEmpty(_searchQuery))
                {
                    bool matchId = !string.IsNullOrEmpty(u.id) && u.id.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0;
                    bool matchName = !string.IsNullOrEmpty(u.upgradeName) && u.upgradeName.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0;
                    bool matchDesc = !string.IsNullOrEmpty(u.description) && u.description.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!matchId && !matchName && !matchDesc) return false;
                }

                // 2. Lọc theo danh mục
                switch (_selectedCategoryFilter)
                {
                    case 1: // Mythic
                        if (!(u is MythicCoreUpgradeData || u.upgradeType == UpgradeType.MythicCore)) return false;
                        break;
                    case 2: // Traits
                        if (!(u is SynergyTraitUpgradeData || u.upgradeType == UpgradeType.SynergyTrait)) return false;
                        break;
                    case 3: // Weapons
                        if (!(u is WeaponUpgradeData || u.upgradeType == UpgradeType.WeaponUpgrade)) return false;
                        break;
                    case 4: // Evolutions
                        if (!(u is EvolutionUpgradeData || u.upgradeType == UpgradeType.EvolutionUpgrade)) return false;
                        break;
                    case 5: // Passives
                        if (!(u is CommonUpgradeData || u.upgradeType == UpgradeType.CommonUpgrade || u.upgradeType == UpgradeType.RareUpgrade)) return false;
                        break;
                    case 6: // Fusions
                        if (!(u is FusionUpgradeData || u.upgradeType == UpgradeType.RelicFusion)) return false;
                        break;
                }

                // 3. Lọc theo Hệ Ngũ Hành
                if (_filterByElement && u.element != _selectedElementFilter)
                {
                    return false;
                }

                return true;
            }).ToList();
        }

        private void SelectUpgrade(UpgradeData u)
        {
            _selectedUpgrade = u;
            if (_cachedEditor != null)
            {
                DestroyImmediate(_cachedEditor);
                _cachedEditor = null;
            }
            if (_selectedUpgrade != null)
            {
                _cachedEditor = UnityEditor.Editor.CreateEditor(_selectedUpgrade);
            }
        }

        private void OnGUI()
        {
            DrawTopToolbar();

            switch (_currentTab)
            {
                case MainTab.StudioEditor:
                    DrawStudioEditorTab();
                    break;

                case MainTab.GachaSimulator:
                    DrawGachaSimulatorTab();
                    break;

                case MainTab.HealthAudit:
                    DrawHealthAuditTab();
                    break;
            }
        }

        #region Top Toolbar
        private void DrawTopToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            // Tab Buttons
            if (GUILayout.Toggle(_currentTab == MainTab.StudioEditor, "🎨 Thiết Kế & Chỉnh Sửa", EditorStyles.toolbarButton))
            {
                _currentTab = MainTab.StudioEditor;
            }
            if (GUILayout.Toggle(_currentTab == MainTab.GachaSimulator, "🎲 Giả Lập Bốc Thẻ (Gacha Simulator)", EditorStyles.toolbarButton))
            {
                _currentTab = MainTab.GachaSimulator;
            }
            if (GUILayout.Toggle(_currentTab == MainTab.HealthAudit, "🔍 Thẩm Định & Quét Lỗi", EditorStyles.toolbarButton))
            {
                _currentTab = MainTab.HealthAudit;
            }

            GUILayout.FlexibleSpace();

            // Quick Actions
            if (GUILayout.Button("🔄 Làm Mới Dữ Liệu", EditorStyles.toolbarButton))
            {
                ReloadAllAssets();
            }

            if (GUILayout.Button("📦 1-Click Sync sang Resources", EditorStyles.toolbarButton))
            {
                int count = UpgradeStudioAuditEngine.SyncDataToResources();
                EditorUtility.DisplayDialog("Đồng Bộ Thành Công", $"Đã đồng bộ {count} thẻ nâng cấp sang Resources/Upgrades!", "OK");
                ReloadAllAssets();
            }

            GUILayout.EndHorizontal();
        }
        #endregion

        #region Tab 1: Studio Editor
        private void DrawStudioEditorTab()
        {
            GUILayout.BeginHorizontal();

            // Cột 1: Sidebar & Explorer (30% Width)
            DrawSidebar(GUILayout.Width(position.width * 0.30f));

            // Cột 2: Inspector & Property Editor (42% Width)
            DrawPropertyEditor(GUILayout.Width(position.width * 0.42f));

            // Cột 3: In-Game Card Preview (28% Width)
            DrawCardPreviewPanel(GUILayout.Width(position.width * 0.28f));

            GUILayout.EndHorizontal();
        }

        private void DrawSidebar(params GUILayoutOption[] options)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox, options);

            // 1. Header & New Template Button
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>DANH SÁCH THẺ ({_filteredUpgrades.Count}/{_allUpgrades.Count})</b>", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+ Tạo Mới", EditorStyles.miniButtonRight, GUILayout.Width(75)))
            {
                ShowCreateTemplateMenu();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            // 2. Search Bar
            GUILayout.BeginHorizontal();
            string newSearch = EditorGUILayout.TextField(_searchQuery, EditorStyles.toolbarSearchField);
            if (newSearch != _searchQuery)
            {
                _searchQuery = newSearch;
                ApplyFilter();
            }
            if (!string.IsNullOrEmpty(_searchQuery) && GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                _searchQuery = string.Empty;
                ApplyFilter();
            }
            GUILayout.EndHorizontal();

            // 3. Category Filter Dropdown
            GUILayout.BeginHorizontal();
            GUILayout.Label("Phân loại:", EditorStyles.miniLabel, GUILayout.Width(55));
            string[] categories = new[] { "Tất Cả", "Đại Lõi (Mythic)", "Thần Binh Thuật (Traits)", "Pháp Bảo (Weapons)", "Tiến Hóa (Evolutions)", "Khí Vận (Passives)", "Dung Hợp (Fusions)" };
            int newCat = EditorGUILayout.Popup(_selectedCategoryFilter, categories, EditorStyles.miniPullDown);
            if (newCat != _selectedCategoryFilter)
            {
                _selectedCategoryFilter = newCat;
                ApplyFilter();
            }
            GUILayout.EndHorizontal();

            // 4. Element Filter
            GUILayout.BeginHorizontal();
            _filterByElement = EditorGUILayout.ToggleLeft("Hệ:", _filterByElement, GUILayout.Width(45));
            if (_filterByElement)
            {
                ElementType newEl = (ElementType)EditorGUILayout.EnumPopup(_selectedElementFilter, EditorStyles.miniPullDown);
                if (newEl != _selectedElementFilter)
                {
                    _selectedElementFilter = newEl;
                    ApplyFilter();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            // 5. Upgrade List View
            _sidebarScroll = GUILayout.BeginScrollView(_sidebarScroll);

            for (int i = 0; i < _filteredUpgrades.Count; i++)
            {
                var u = _filteredUpgrades[i];
                if (u == null) continue;

                bool isSelected = u == _selectedUpgrade;
                var prevColor = GUI.backgroundColor;
                if (isSelected) GUI.backgroundColor = new Color(0.2f, 0.6f, 1f, 1f);

                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUI.backgroundColor = prevColor;

                // Mini Icon
                if (u.icon != null && u.icon.texture != null)
                {
                    GUILayout.Label(new GUIContent(u.icon.texture), GUILayout.Width(28), GUILayout.Height(28));
                }
                else
                {
                    GUILayout.Box("?", GUILayout.Width(28), GUILayout.Height(28));
                }

                // ID & Name
                GUILayout.BeginVertical();
                string displayName = !string.IsNullOrEmpty(u.upgradeName) ? u.upgradeName : "(Chưa đặt tên)";
                GUILayout.Label($"<b>{u.id}</b> - {displayName}", EditorStyles.label);
                GUILayout.Label($"W: {u.spawnWeight} | LvMax: {(u.maxLevel > 0 ? u.maxLevel.ToString() : "∞")}", EditorStyles.miniLabel);
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                // Select Button
                if (GUILayout.Button(isSelected ? "●" : "Chọn", EditorStyles.miniButton, GUILayout.Width(45), GUILayout.Height(28)))
                {
                    SelectUpgrade(u);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawPropertyEditor(params GUILayoutOption[] options)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox, options);

            if (_selectedUpgrade == null)
            {
                EditorGUILayout.HelpBox("Chọn một thẻ từ cột trái để bắt đầu chỉnh sửa.", MessageType.Info);
                GUILayout.EndVertical();
                return;
            }

            // Header Controls
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label($"<b>CHỈNH SỬA: {_selectedUpgrade.id}</b>", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("📂 Ping", EditorStyles.toolbarButton))
            {
                EditorGUIUtility.PingObject(_selectedUpgrade);
            }
            if (GUILayout.Button("📋 Nhân Bản", EditorStyles.toolbarButton))
            {
                DuplicateSelectedUpgrade();
            }
            if (GUILayout.Button("🗑️ Xóa", EditorStyles.toolbarButton))
            {
                if (EditorUtility.DisplayDialog("Xác nhận xóa", $"Bạn có chắc muốn xóa thẻ '{_selectedUpgrade.upgradeName}' ({_selectedUpgrade.id})?", "Xóa", "Hủy"))
                {
                    DeleteSelectedUpgrade();
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    return;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            _editorScroll = GUILayout.BeginScrollView(_editorScroll);

            // Custom Inspector Fields
            EditorGUI.BeginChangeCheck();

            // 1. Thông tin cơ bản
            GUILayout.Label("1. THÔNG TIN CƠ BẢN", EditorStyles.boldLabel);
            _selectedUpgrade.id = EditorGUILayout.TextField("Mã Thẻ (ID):", _selectedUpgrade.id);
            _selectedUpgrade.upgradeName = EditorGUILayout.TextField("Tên Thẻ:", _selectedUpgrade.upgradeName);
            _selectedUpgrade.icon = (Sprite)EditorGUILayout.ObjectField("Icon Sprite:", _selectedUpgrade.icon, typeof(Sprite), false);
            _selectedUpgrade.upgradeType = (UpgradeType)EditorGUILayout.EnumPopup("Phân Loại (Type):", _selectedUpgrade.upgradeType);
            _selectedUpgrade.element = (ElementType)EditorGUILayout.EnumPopup("Hệ Ngũ Hành:", _selectedUpgrade.element);
            _selectedUpgrade.spawnWeight = EditorGUILayout.FloatField("Trọng Số (Spawn Weight):", _selectedUpgrade.spawnWeight);
            _selectedUpgrade.maxLevel = EditorGUILayout.IntField("Cấp Tối Đa (Max Level):", _selectedUpgrade.maxLevel);

            GUILayout.Space(6);

            // 2. Mô tả Rich Text
            GUILayout.Label("2. MÔ TẢ KỸ NĂNG (RICH TEXT)", EditorStyles.boldLabel);
            _selectedUpgrade.description = EditorGUILayout.TextArea(_selectedUpgrade.description, GUILayout.Height(65));

            // Thanh công cụ hỗ trợ chèn màu nhanh
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Vàng Kim", EditorStyles.miniButton)) _selectedUpgrade.description += "<color=#FFD700>Text</color>";
            if (GUILayout.Button("+ Hỏa Đỏ", EditorStyles.miniButton)) _selectedUpgrade.description += "<color=#FF4444>Text</color>";
            if (GUILayout.Button("+ Lam Thủy", EditorStyles.miniButton)) _selectedUpgrade.description += "<color=#00E5FF>Text</color>";
            if (GUILayout.Button("+ In Đậm", EditorStyles.miniButton)) _selectedUpgrade.description += "<b>Text</b>";
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // 3. Thuộc tính chuyên sâu theo từng phân loại
            GUILayout.Label("3. THÔNG SỐ CHUYÊN SÂU", EditorStyles.boldLabel);

            if (_selectedUpgrade is MythicCoreUpgradeData mythic)
            {
                mythic.archetype = (MythicArchetype)EditorGUILayout.EnumPopup("Trường Phái Archetype:", mythic.archetype);
                mythic.mythicTitle = EditorGUILayout.TextField("Danh Hiệu Thần Thoại:", mythic.mythicTitle);
                mythic.runtimePrefab = (MythicCoreRuntime)EditorGUILayout.ObjectField("Runtime Prefab:", mythic.runtimePrefab, typeof(MythicCoreRuntime), false);
            }
            else if (_selectedUpgrade is SynergyTraitUpgradeData trait)
            {
                trait.requiredArchetype = (MythicArchetype)EditorGUILayout.EnumPopup("Yêu Cầu Đại Lõi:", trait.requiredArchetype);
                DrawPlayerStatModifierEditor(ref trait.playerStatModifier);
            }
            else if (_selectedUpgrade is WeaponUpgradeData weaponUp)
            {
                weaponUp.weaponId = EditorGUILayout.TextField("ID Pháp Bảo Gốc:", weaponUp.weaponId);
                weaponUp.requiredCurrentLevel = EditorGUILayout.IntField("Cấp Yêu Cầu Hiện Tại:", weaponUp.requiredCurrentLevel);
                DrawWeaponStatModifierEditor(ref weaponUp.statModifier);
            }
            else if (_selectedUpgrade is EvolutionUpgradeData evo)
            {
                evo.weaponId = EditorGUILayout.TextField("ID Pháp Bảo Gốc:", evo.weaponId);
                evo.requiredCurrentLevel = EditorGUILayout.IntField("Cấp Pháp Bảo Yêu Cầu:", evo.requiredCurrentLevel);
                evo.requiredPassiveId = EditorGUILayout.TextField("ID Khí Vận Yêu Cầu (Passive):", evo.requiredPassiveId);
                evo.weaponPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab Tiến Hóa Mới:", evo.weaponPrefab, typeof(GameObject), false);
            }
            else if (_selectedUpgrade is CommonUpgradeData common)
            {
                DrawPlayerStatModifierEditor(ref common.playerStatModifier);
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_selectedUpgrade);
            }

            GUILayout.Space(16);

            if (GUILayout.Button("💾 Lưu Thay Đổi Vào File", GUILayout.Height(32)))
            {
                EditorUtility.SetDirty(_selectedUpgrade);
                AssetDatabase.SaveAssets();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawPlayerStatModifierEditor(ref PlayerStatModifier mod)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Chỉ Số Nhân Vật (PlayerStatModifier):", EditorStyles.miniBoldLabel);
            mod.baseDamageBonus = EditorGUILayout.FloatField("Sát thương cơ bản (+):", mod.baseDamageBonus);
            mod.critChanceBonus = EditorGUILayout.FloatField("Tỉ lệ chí mạng (0.05 = 5%):", mod.critChanceBonus);
            mod.maxHealthBonus = EditorGUILayout.FloatField("Máu tối đa (+):", mod.maxHealthBonus);
            mod.moveSpeedBonus = EditorGUILayout.FloatField("Tốc độ di chuyển (+):", mod.moveSpeedBonus);
            mod.attackSpeedBonus = EditorGUILayout.FloatField("Tốc độ đánh (+):", mod.attackSpeedBonus);
            mod.dashCooldownReduction = EditorGUILayout.FloatField("Giảm hồi chiêu lướt (s):", mod.dashCooldownReduction);
            mod.dashSpeedBonus = EditorGUILayout.FloatField("Tăng tốc lướt (%):", mod.dashSpeedBonus);
            mod.areaScaleBonus = EditorGUILayout.FloatField("Phạm vi ảnh hưởng AoE (%):", mod.areaScaleBonus);
            mod.pickupRangeBonus = EditorGUILayout.FloatField("Tầm hút ngọc & đồ (+):", mod.pickupRangeBonus);
            mod.fireDamageBonus = EditorGUILayout.FloatField("Sát thương Hỏa (%):", mod.fireDamageBonus);
            GUILayout.EndVertical();
        }

        private void DrawWeaponStatModifierEditor(ref WeaponStatModifier mod)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Chỉ Số Pháp Bảo (WeaponStatModifier):", EditorStyles.miniBoldLabel);
            mod.damageBonus = EditorGUILayout.FloatField("Sát thương (+):", mod.damageBonus);
            mod.attackSpeedBonus = EditorGUILayout.FloatField("Tốc độ bắn/chém (+):", mod.attackSpeedBonus);
            mod.projectileCountBonus = EditorGUILayout.IntField("Số lượng đạn (+):", mod.projectileCountBonus);
            mod.pierceBonus = EditorGUILayout.IntField("Số lần xuyên thấu (+):", mod.pierceBonus);
            mod.scaleBonus = EditorGUILayout.FloatField("Kích thước đạn (%):", mod.scaleBonus);
            mod.critChanceBonus = EditorGUILayout.FloatField("Chí mạng (+):", mod.critChanceBonus);
            GUILayout.EndVertical();
        }

        private void DrawCardPreviewPanel(params GUILayoutOption[] options)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox, options);

            GUILayout.Label("<b>👁️ LIVE CARD PREVIEW (WYSIWYG)</b>", EditorStyles.boldLabel);
            GUILayout.Space(6);

            _previewScroll = GUILayout.BeginScrollView(_previewScroll);

            if (_selectedUpgrade != null)
            {
                UpgradeStudioCardPreviewRenderer.DrawCardPreview(_selectedUpgrade, position.width * 0.26f);
            }
            else
            {
                EditorGUILayout.HelpBox("Chọn một thẻ để xem trước trực quan.", MessageType.Info);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void ShowCreateTemplateMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("🌟 Đại Lõi Thần Thoại (Mythic Core)"), false, () => CreateNewUpgrade(UpgradeType.MythicCore));
            menu.AddItem(new GUIContent("✨ Thần Binh Thuật (Synergy Trait)"), false, () => CreateNewUpgrade(UpgradeType.SynergyTrait));
            menu.AddItem(new GUIContent("⚔️ Cường Hóa Pháp Bảo (Weapon Upgrade)"), false, () => CreateNewUpgrade(UpgradeType.WeaponUpgrade));
            menu.AddItem(new GUIContent("🔥 Thần Pháp Tiến Hóa (Evolution)"), false, () => CreateNewUpgrade(UpgradeType.EvolutionUpgrade));
            menu.AddItem(new GUIContent("📜 Bổ Trợ Khí Vận (Common/Passive)"), false, () => CreateNewUpgrade(UpgradeType.CommonUpgrade));
            menu.AddItem(new GUIContent("🔮 Dung Hợp Pháp Bảo (Relic Fusion)"), false, () => CreateNewUpgrade(UpgradeType.RelicFusion));
            menu.ShowAsContext();
        }

        private void CreateNewUpgrade(UpgradeType type)
        {
            var newAsset = UpgradeStudioTemplateFactory.CreateTemplate(type);
            ReloadAllAssets();
            SelectUpgrade(newAsset);
        }

        private void DuplicateSelectedUpgrade()
        {
            if (_selectedUpgrade == null) return;

            string path = AssetDatabase.GetAssetPath(_selectedUpgrade);
            string newPath = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CopyAsset(path, newPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var duplicated = AssetDatabase.LoadAssetAtPath<UpgradeData>(newPath);
            if (duplicated != null)
            {
                duplicated.id = duplicated.id + "_COPY";
                duplicated.upgradeName = duplicated.upgradeName + " (Bản Sao)";
                EditorUtility.SetDirty(duplicated);
                AssetDatabase.SaveAssets();
            }

            ReloadAllAssets();
            SelectUpgrade(duplicated);
        }

        private void DeleteSelectedUpgrade()
        {
            if (_selectedUpgrade == null) return;
            string path = AssetDatabase.GetAssetPath(_selectedUpgrade);
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            _selectedUpgrade = null;
            ReloadAllAssets();
        }
        #endregion

        #region Tab 2: Gacha Simulator
        private void DrawGachaSimulatorTab()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("<b>🎲 GIẢ LẬP BỐC THẺ ROGUELITE (MONTE CARLO SIMULATOR)</b>", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Công cụ giả lập hàng nghìn lượt Level Up để Game Designer đánh giá phân bổ xác suất, tỉ lệ xuất hiện của thẻ và độ cân bằng của hệ thống Thần Binh Thuật.", MessageType.Info);

            GUILayout.Space(8);

            GUILayout.BeginHorizontal();
            _simArchetype = (MythicArchetype)EditorGUILayout.EnumPopup("Đại Lõi Giả Lập:", _simArchetype, GUILayout.Width(350));
            _simRollCount = EditorGUILayout.IntSlider("Số Lượt Level Up:", _simRollCount, 50, 5000, GUILayout.Width(350));

            if (GUILayout.Button("🚀 Chạy Giả Lập", GUILayout.Height(24), GUILayout.Width(130)))
            {
                _lastSimResult = UpgradeStudioSimulator.RunSimulation(_allUpgrades, _simArchetype, _simRollCount);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (_lastSimResult.HasValue)
            {
                var sim = _lastSimResult.Value;
                int totalPickedCards = sim.TotalRolls * sim.ChoiceCount;

                // Thống kê tổng quát
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label($"<b>KẾT QUẢ GIẢ LẬP ({sim.TotalRolls} lượt - {totalPickedCards} thẻ được chào mời):</b>", EditorStyles.boldLabel);
                GUILayout.Label($"- Lõi đang mang: <color=#00E5FF><b>{sim.ActiveArchetype.GetDisplayName()}</b></color>", new GUIStyle(EditorStyles.label) { richText = true });
                GUILayout.Label($"- Số lần kích hoạt Bảo Hiểm Lõi (Guaranteed Synergy): <b>{sim.SynergyGuaranteedCount} / {sim.TotalRolls}</b> ({((float)sim.SynergyGuaranteedCount / sim.TotalRolls * 100f):F1}%)");

                // Phân bổ theo phân loại
                GUILayout.Space(4);
                GUILayout.Label("<b>Phân bố theo Loại Thẻ:</b>", EditorStyles.boldLabel);
                foreach (var kvp in sim.CategoryCounts)
                {
                    float percent = (float)kvp.Value / totalPickedCards * 100f;
                    GUILayout.Label($"  • {kvp.Key}: <b>{kvp.Value} thẻ</b> ({percent:F1}%)");
                }
                GUILayout.EndVertical();

                GUILayout.Space(8);

                // Bảng chi tiết từng thẻ
                GUILayout.Label("<b>BẢNG TẦN SUẤT XUẤT HIỆN TỪNG THẺ (TOP PICK RATE):</b>", EditorStyles.boldLabel);

                _simScroll = GUILayout.BeginScrollView(_simScroll);

                var sortedPicks = sim.PickCounts.OrderByDescending(p => p.Value).ToList();
                for (int i = 0; i < sortedPicks.Count; i++)
                {
                    var kvp = sortedPicks[i];
                    if (kvp.Value == 0) continue;

                    float pickRate = (float)kvp.Value / sim.TotalRolls * 100f;

                    GUILayout.BeginHorizontal(EditorStyles.helpBox);
                    GUILayout.Label($"#{i + 1}", EditorStyles.boldLabel, GUILayout.Width(35));
                    GUILayout.Label($"<b>{kvp.Key.id}</b> - {kvp.Key.upgradeName}", GUILayout.Width(280));
                    GUILayout.Label($"[{kvp.Key.upgradeType}]", GUILayout.Width(150));
                    GUILayout.Label($"Xuất hiện: <b>{kvp.Value} lần</b>", GUILayout.Width(120));

                    // Thanh ProgressBar
                    Rect barRect = GUILayoutUtility.GetRect(150, 18);
                    EditorGUI.ProgressBar(barRect, pickRate / 100f, $"{pickRate:F1}% Pick Rate");

                    GUILayout.EndHorizontal();
                }

                GUILayout.EndScrollView();
            }

            GUILayout.EndVertical();
        }
        #endregion

        #region Tab 3: Health Audit
        private void DrawHealthAuditTab()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("<b>🔍 THẨM ĐỊNH TOÀN DIỆN DATABASE (HEALTH AUDIT)</b>", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Quét toàn bộ thẻ nâng cấp để phát hiện các lỗi cấu hình như thiếu Icon, trùng ID, ID rỗng, sai thông số hoặc liên kết hỏng.", MessageType.Info);

            GUILayout.Space(6);

            // Nút Quét Lỗi & Nút Copy Báo Cáo
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🔍 Bắt Đầu Quét Lỗi Toàn Bộ Database", GUILayout.Height(30)))
            {
                _auditIssues = UpgradeStudioAuditEngine.RunAudit(_allUpgrades);
            }

            if (_auditIssues != null && _auditIssues.Count > 0)
            {
                if (GUILayout.Button("📋 Sao Chép Báo Cáo Lỗi (Markdown)", GUILayout.Height(30), GUILayout.Width(240)))
                {
                    string filterInfo = $"{(_auditSeverityFilter == 1 ? "Chỉ Errors" : _auditSeverityFilter == 2 ? "Chỉ Warnings" : "Tất cả Mức Độ")} | Nhóm: {_auditCategoryFilter}";
                    string report = UpgradeStudioAuditEngine.GenerateAuditReport(GetFilteredAuditIssues(), filterInfo);
                    EditorGUIUtility.systemCopyBuffer = report;
                    EditorUtility.DisplayDialog("Đã Sao Chép", "Đã sao chép toàn bộ báo cáo lỗi dạng bảng Markdown vào Clipboard!\nBạn có thể dán (Ctrl+V) vào tin nhắn hoặc file tài liệu.", "OK");
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (_auditIssues != null)
            {
                int totalErrors = _auditIssues.Count(i => i.Severity == AuditIssue.IssueSeverity.Error);
                int totalWarnings = _auditIssues.Count(i => i.Severity == AuditIssue.IssueSeverity.Warning);

                // 1. Thanh Lọc Mức Độ & Nhóm Lỗi
                GUILayout.BeginVertical(EditorStyles.helpBox);
                
                // Hàng 1: Lọc Mức Độ Lỗi
                GUILayout.BeginHorizontal();
                GUILayout.Label("<b>Mức độ:</b>", EditorStyles.boldLabel, GUILayout.Width(65));
                string[] severityTabs = new[] { $"Tất Cả ({_auditIssues.Count})", $"🔴 Chỉ Lỗi ({totalErrors})", $"🟡 Chỉ Cảnh Báo ({totalWarnings})" };
                _auditSeverityFilter = GUILayout.Toolbar(_auditSeverityFilter, severityTabs, EditorStyles.toolbarButton);
                GUILayout.EndHorizontal();

                GUILayout.Space(4);

                // Hàng 2: Lọc Nhóm Lỗi (Categories)
                GUILayout.BeginHorizontal();
                GUILayout.Label("<b>Nhóm lỗi:</b>", EditorStyles.boldLabel, GUILayout.Width(65));
                
                int countId = _auditIssues.Count(i => i.Category == AuditCategory.Identification);
                int countIcon = _auditIssues.Count(i => i.Category == AuditCategory.IconMissing);
                int countWeight = _auditIssues.Count(i => i.Category == AuditCategory.SpawnWeight);
                int countLink = _auditIssues.Count(i => i.Category == AuditCategory.EvolutionLink);
                int countTrait = _auditIssues.Count(i => i.Category == AuditCategory.TraitArchetype);

                if (GUILayout.Toggle(_auditCategoryFilter == AuditCategory.All, "Tất Cả", EditorStyles.miniButton)) _auditCategoryFilter = AuditCategory.All;
                if (GUILayout.Toggle(_auditCategoryFilter == AuditCategory.Identification, $"ID/Trùng ({countId})", EditorStyles.miniButton)) _auditCategoryFilter = AuditCategory.Identification;
                if (GUILayout.Toggle(_auditCategoryFilter == AuditCategory.IconMissing, $"Thiếu Icon ({countIcon})", EditorStyles.miniButton)) _auditCategoryFilter = AuditCategory.IconMissing;
                if (GUILayout.Toggle(_auditCategoryFilter == AuditCategory.SpawnWeight, $"Trọng Số ({countWeight})", EditorStyles.miniButton)) _auditCategoryFilter = AuditCategory.SpawnWeight;
                if (GUILayout.Toggle(_auditCategoryFilter == AuditCategory.EvolutionLink, $"Vũ Khí/Tiến Hóa ({countLink})", EditorStyles.miniButton)) _auditCategoryFilter = AuditCategory.EvolutionLink;
                if (GUILayout.Toggle(_auditCategoryFilter == AuditCategory.TraitArchetype, $"Lõi Thần Binh ({countTrait})", EditorStyles.miniButton)) _auditCategoryFilter = AuditCategory.TraitArchetype;
                
                GUILayout.EndHorizontal();

                GUILayout.Space(4);

                // Hàng 3: Tìm kiếm nhanh trong danh sách lỗi
                GUILayout.BeginHorizontal();
                GUILayout.Label("<b>Tìm lỗi:</b>", EditorStyles.boldLabel, GUILayout.Width(65));
                _auditSearchQuery = EditorGUILayout.TextField(_auditSearchQuery, EditorStyles.toolbarSearchField);
                if (!string.IsNullOrEmpty(_auditSearchQuery) && GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    _auditSearchQuery = string.Empty;
                }
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                GUILayout.Space(8);

                // 2. Danh sách lỗi đã lọc
                var filteredList = GetFilteredAuditIssues();
                GUILayout.Label($"<b>Hiển thị: {filteredList.Count} vấn đề</b>", EditorStyles.boldLabel);

                _auditScroll = GUILayout.BeginScrollView(_auditScroll);

                if (_auditIssues.Count == 0)
                {
                    EditorGUILayout.HelpBox("Tuyệt vời! Toàn bộ cơ sở dữ liệu Upgrades hoạt động hoàn hảo, không có lỗi cấu hình nào.", MessageType.Info);
                }
                else if (filteredList.Count == 0)
                {
                    EditorGUILayout.HelpBox("Không có vấn đề nào khớp với bộ lọc hiện tại.", MessageType.Info);
                }
                else
                {
                    for (int i = 0; i < filteredList.Count; i++)
                    {
                        var issue = filteredList[i];
                        MessageType msgType = issue.Severity == AuditIssue.IssueSeverity.Error ? MessageType.Error : MessageType.Warning;

                        GUILayout.BeginHorizontal(EditorStyles.helpBox);
                        
                        // Icon mức độ
                        GUILayout.Label(issue.Severity == AuditIssue.IssueSeverity.Error ? "🔴" : "🟡", GUILayout.Width(22));

                        // Nhóm lỗi
                        GUILayout.Label($"<b>[{issue.Category}]</b>", EditorStyles.miniBoldLabel, GUILayout.Width(110));

                        // Nội dung chi tiết
                        EditorGUILayout.HelpBox(issue.Message, msgType);

                        // Nút Copy từng dòng
                        if (GUILayout.Button("📋", GUILayout.Width(30), GUILayout.Height(36)))
                        {
                            EditorGUIUtility.systemCopyBuffer = $"[{issue.Severity}] [{issue.Category}] {issue.Message}";
                            Debug.Log($"<color=#00FF88>[UpgradeStudio]</color> Đã sao chép: {issue.Message}");
                        }

                        // Nút Xem / Sửa
                        if (issue.TargetAsset != null)
                        {
                            if (GUILayout.Button("Sửa", GUILayout.Width(45), GUILayout.Height(36)))
                            {
                                SelectUpgrade(issue.TargetAsset);
                                _currentTab = MainTab.StudioEditor;
                            }
                        }

                        GUILayout.EndHorizontal();
                    }
                }

                GUILayout.EndScrollView();
            }

            GUILayout.EndVertical();
        }

        private List<AuditIssue> GetFilteredAuditIssues()
        {
            if (_auditIssues == null) return new List<AuditIssue>();

            return _auditIssues.Where(issue =>
            {
                // 1. Lọc theo Severity
                if (_auditSeverityFilter == 1 && issue.Severity != AuditIssue.IssueSeverity.Error) return false;
                if (_auditSeverityFilter == 2 && issue.Severity != AuditIssue.IssueSeverity.Warning) return false;

                // 2. Lọc theo Category
                if (_auditCategoryFilter != AuditCategory.All && issue.Category != _auditCategoryFilter) return false;

                // 3. Lọc theo Search Query
                if (!string.IsNullOrEmpty(_auditSearchQuery))
                {
                    bool matchMsg = issue.Message != null && issue.Message.IndexOf(_auditSearchQuery, StringComparison.OrdinalIgnoreCase) >= 0;
                    bool matchAsset = issue.TargetAsset != null && (!string.IsNullOrEmpty(issue.TargetAsset.id) && issue.TargetAsset.id.IndexOf(_auditSearchQuery, StringComparison.OrdinalIgnoreCase) >= 0 || !string.IsNullOrEmpty(issue.TargetAsset.upgradeName) && issue.TargetAsset.upgradeName.IndexOf(_auditSearchQuery, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (!matchMsg && !matchAsset) return false;
                }

                return true;
            }).ToList();
        }
        #endregion
    }
}
#endif
