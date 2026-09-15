#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Upgrades.Editor.Studio.Panels;

namespace ProjectZombie.Features.Upgrades.Editor.Studio
{
    /// <summary>
    /// Bảng điều khiển Studio trực quan toàn diện dành cho Game Designer thiết kế, cân bằng và giả lập Thẻ Nâng Cấp in-game.
    /// Kiến trúc Modular View Panels (Clean Architecture).
    /// </summary>
    public class UpgradeStudioWindow : EditorWindow
    {
        public enum MainTab
        {
            StudioEditor,
            GachaSimulator,
            HealthAudit
        }

        private MainTab _currentTab = MainTab.StudioEditor;

        // Dữ liệu Database
        private readonly List<UpgradeData> _allUpgrades = new List<UpgradeData>();
        private UpgradeData _selectedUpgrade;

        // Modular View Panels
        private StudioSidebarPanel _sidebarPanel;
        private StudioInspectorPanel _inspectorPanel;
        private StudioPreviewPanel _previewPanel;
        private StudioSimulatorPanel _simulatorPanel;
        private StudioAuditPanel _auditPanel;

        [MenuItem("ProjectZombie/🛠️ Upgrade Studio (Game Designer Dashboard)", priority = -200)]
        public static void ShowWindow()
        {
            var window = GetWindow<UpgradeStudioWindow>("Upgrade Studio");
            window.minSize = new Vector2(980, 620);
            window.Show();
        }

        private void OnEnable()
        {
            InitPanels();
            ReloadAllAssets();
        }

        private void InitPanels()
        {
            _sidebarPanel = new StudioSidebarPanel(SelectUpgrade, CreateNewUpgrade);
            _inspectorPanel = new StudioInspectorPanel(DuplicateUpgrade, DeleteUpgrade);
            _previewPanel = new StudioPreviewPanel();
            _simulatorPanel = new StudioSimulatorPanel();
            _auditPanel = new StudioAuditPanel(FixAuditIssue);
        }

        public void ReloadAllAssets()
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

            if (_selectedUpgrade == null && _allUpgrades.Count > 0)
            {
                SelectUpgrade(_allUpgrades[0]);
            }
        }

        private void SelectUpgrade(UpgradeData u)
        {
            _selectedUpgrade = u;
            Repaint();
        }

        private void OnGUI()
        {
            if (_sidebarPanel == null) InitPanels();

            DrawTopToolbar();

            switch (_currentTab)
            {
                case MainTab.StudioEditor:
                    DrawStudioEditorTab();
                    break;

                case MainTab.GachaSimulator:
                    _simulatorPanel?.Draw(_allUpgrades);
                    break;

                case MainTab.HealthAudit:
                    _auditPanel?.Draw(_allUpgrades);
                    break;
            }
        }

        private void DrawTopToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

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

        private void DrawStudioEditorTab()
        {
            GUILayout.BeginHorizontal();

            // Cột 1: Sidebar & Explorer (30% Width)
            _sidebarPanel?.Draw(_allUpgrades, _selectedUpgrade, GUILayout.Width(position.width * 0.30f));

            // Cột 2: Inspector & Property Editor (42% Width)
            _inspectorPanel?.Draw(_selectedUpgrade, GUILayout.Width(position.width * 0.42f));

            // Cột 3: In-Game Card Preview (28% Width)
            _previewPanel?.Draw(_selectedUpgrade, position.width * 0.26f, GUILayout.Width(position.width * 0.28f));

            GUILayout.EndHorizontal();
        }

        #region Actions & Callbacks
        private void CreateNewUpgrade(UpgradeType type)
        {
            var newAsset = UpgradeStudioTemplateFactory.CreateTemplate(type);
            ReloadAllAssets();
            SelectUpgrade(newAsset);
        }

        private void DuplicateUpgrade(UpgradeData u)
        {
            if (u == null) return;
            string path = AssetDatabase.GetAssetPath(u);
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

        private void DeleteUpgrade(UpgradeData u)
        {
            if (u == null) return;
            string path = AssetDatabase.GetAssetPath(u);
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            _selectedUpgrade = null;
            ReloadAllAssets();
        }

        private void FixAuditIssue(UpgradeData u)
        {
            SelectUpgrade(u);
            _currentTab = MainTab.StudioEditor;
        }
        #endregion
    }
}
#endif
