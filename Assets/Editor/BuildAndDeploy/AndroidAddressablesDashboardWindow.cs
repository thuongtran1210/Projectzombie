#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectZombie.Editor.BuildAndDeploy.Dashboard;

namespace ProjectZombie.Editor.BuildAndDeploy
{
    /// <summary>
    /// Bảng Điều Khiển Trực Quan Quy Trình Build Android & Addressables (Workflow Dashboard).
    /// Áp dụng mô hình Clean Architecture & SRP:
    /// - AddressablesWorkflowService: Quản lý toàn bộ dữ liệu, logic chuyển đổi mode và quy trình build.
    /// - AndroidDashboardDrawer: Quản lý toàn bộ giao diện GUI, layout, màu sắc.
    /// - AndroidAddressablesDashboardWindow: Controller gọn gàng (~65 dòng), quản lý vòng đời EditorWindow.
    /// </summary>
    public class AndroidAddressablesDashboardWindow : EditorWindow
    {
        private Vector2 _scrollPos;
        private AddressablesWorkflowService _service;
        private AndroidDashboardDrawer _drawer;

        [MenuItem("ProjectZombie/🤖 Android & Addressables Dashboard", priority = 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<AndroidAddressablesDashboardWindow>("Android & Addressables Control Panel");
            window.minSize = new Vector2(520, 680);
            window.Show();
        }

        private void OnEnable()
        {
            EnsureInitialized();
            _service.RefreshAll();
        }

        private void OnFocus()
        {
            EnsureInitialized();
            _service.RefreshAll();
        }

        private void EnsureInitialized()
        {
            if (_service == null)
            {
                _service = new AddressablesWorkflowService();
                _drawer = new AndroidDashboardDrawer(_service);
            }
        }

        private void OnGUI()
        {
            EnsureInitialized();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            _drawer.DrawHeader();
            EditorGUILayout.Space(8);

            _drawer.DrawStatusSection();
            EditorGUILayout.Space(10);

            _drawer.DrawQuickModeSwitcherSection();
            EditorGUILayout.Space(10);

            _drawer.DrawWorkflowChecklistSection();
            EditorGUILayout.Space(10);

            _drawer.DrawAuditReportSection();
            EditorGUILayout.Space(10);

            _drawer.DrawFooter();

            EditorGUILayout.EndScrollView();
        }
    }
}
#endif
