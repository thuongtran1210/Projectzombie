using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectZombie.Editor.AddressablesTools
{
    /// <summary>
    /// Trạng thái so sánh của từng Asset giữa Local Cache và Remote Firebase CDN.
    /// </summary>
    public enum AssetSyncStatus
    {
        Unknown,
        LocalInApk,        // Nhóm đóng gói tĩnh trong APK
        SyncedUpToDate,    // Đã có trong Cache hoặc khớp 100% với Remote (DownloadSize = 0)
        NeedsDownload,     // Chưa có trong máy hoặc có bản cập nhật mới trên CDN (DownloadSize > 0)
        Checking           // Đang kiểm tra dung lượng
    }

    public class AssetItemReport
    {
        public string GroupName;
        public string AssetName;
        public string AddressKey;
        public bool IsRemoteGroup;
        public AssetSyncStatus Status = AssetSyncStatus.Unknown;
        public long DownloadSizeBytes = 0;
        public string Details = "";
    }

    /// <summary>
    /// Editor Tool chuyên dụng kiểm tra, đối chiếu và kiểm toán dữ liệu Addressables giữa Local (Android Cache/Editor Cache) và Remote CDN Firebase.
    /// Cho phép lập trình viên biết chính xác mục nào đã cập nhật, mục nào chưa cập nhật và tải trước / dọn dẹp cache.
    /// </summary>
    public class AddressablesCDNComparatorWindow : EditorWindow
    {
        private List<AssetItemReport> _reports = new();
        private Vector2 _scrollPos;
        private bool _isAuditing = false;
        private string _catalogStatusText = "Chưa kiểm tra Catalog CDN";
        private int _totalNeedsUpdate = 0;
        private int _totalSynced = 0;
        private int _totalLocalApk = 0;
        private long _totalDownloadBytes = 0;
        private string _filterText = "";

        [MenuItem("Tools/ProjectZombie/Addressables/CDN Content Comparator & Audit Tool", false, 50)]
        public static void ShowWindow()
        {
            var win = GetWindow<AddressablesCDNComparatorWindow>("Addressables CDN Comparator");
            win.minSize = new Vector2(820, 560);
            win.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            DrawHeader();
            EditorGUILayout.Space(6);
            DrawActionsBar();
            EditorGUILayout.Space(6);
            DrawSummaryStats();
            EditorGUILayout.Space(6);
            DrawFilterBar();
            EditorGUILayout.Space(4);
            DrawReportList();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                normal = { textColor = new Color(0.2f, 0.8f, 0.3f) }
            };
            EditorGUILayout.LabelField("ADDRESSABLES CDN & CACHE COMPARATOR", titleStyle);
            EditorGUILayout.LabelField("So sánh, đối chiếu danh mục dữ liệu Asset giữa Local Cache (Android/PC) và Remote CDN Firebase.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawActionsBar()
        {
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = !_isAuditing;
            if (GUILayout.Button("Kiểm Tra & So Sánh Toàn Bộ Asset (Audit CDN)", GUILayout.Height(32)))
            {
                _ = AuditAllAssetsAsync();
            }

            if (GUILayout.Button("Kiểm Tra Catalog Updates", GUILayout.Height(32), GUILayout.Width(180)))
            {
                _ = CheckCatalogUpdatesAsync();
            }

            if (GUILayout.Button("Xóa Toàn Bộ Local Cache", GUILayout.Height(32), GUILayout.Width(170)))
            {
                if (EditorUtility.DisplayDialog("Xác nhận xóa Cache", "Bạn có chắc chắn muốn xóa toàn bộ AssetBundle Cache cục bộ để giả lập thiết bị mới cài game?", "Xóa Cache", "Hủy"))
                {
                    ClearAllAddressablesCache();
                }
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSummaryStats()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Trạng Thái Catalog: {_catalogStatusText}", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUI.color = new Color(0.4f, 0.9f, 0.4f);
            EditorGUILayout.LabelField($"Khớp / Đã Tải: {_totalSynced}", EditorStyles.boldLabel, GUILayout.Width(150));

            GUI.color = new Color(1f, 0.5f, 0.3f);
            EditorGUILayout.LabelField($"Cần Cập Nhật / Tải: {_totalNeedsUpdate} ({_totalDownloadBytes / (1024f * 1024f):0.00} MB)", EditorStyles.boldLabel, GUILayout.Width(240));

            GUI.color = new Color(0.6f, 0.8f, 1f);
            EditorGUILayout.LabelField($"Đóng Gói Trong APK: {_totalLocalApk}", EditorStyles.boldLabel, GUILayout.Width(180));

            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawFilterBar()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Tìm kiếm:", GUILayout.Width(60));
            _filterText = EditorGUILayout.TextField(_filterText);
            if (GUILayout.Button("Xóa Lọc", GUILayout.Width(70)))
            {
                _filterText = "";
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawReportList()
        {
            EditorGUILayout.BeginVertical("box");

            // Table Header
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Nhóm (Group)", EditorStyles.boldLabel, GUILayout.Width(180));
            GUILayout.Label("Tên Tài Nguyên / Key", EditorStyles.boldLabel, GUILayout.Width(220));
            GUILayout.Label("Trạng Thái So Với CDN", EditorStyles.boldLabel, GUILayout.Width(180));
            GUILayout.Label("Dung Lượng Tải", EditorStyles.boldLabel, GUILayout.Width(110));
            GUILayout.Label("Thao Tác", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            if (_reports.Count == 0)
            {
                EditorGUILayout.Space(20);
                EditorGUILayout.LabelField("Chưa có báo cáo. Nhấn 'Kiểm Tra & So Sánh Toàn Bộ Asset (Audit CDN)' để bắt đầu đối chiếu dữ liệu.", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                foreach (var rep in _reports)
                {
                    if (!string.IsNullOrEmpty(_filterText))
                    {
                        bool match = rep.GroupName.IndexOf(_filterText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     rep.AddressKey.IndexOf(_filterText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     rep.AssetName.IndexOf(_filterText, StringComparison.OrdinalIgnoreCase) >= 0;
                        if (!match) continue;
                    }

                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    // 1. Group Name
                    GUILayout.Label(rep.GroupName, GUILayout.Width(180));

                    // 2. Asset Name & Key
                    EditorGUILayout.BeginVertical(GUILayout.Width(220));
                    GUILayout.Label(rep.AssetName, EditorStyles.boldLabel);
                    GUILayout.Label(rep.AddressKey, EditorStyles.miniLabel);
                    EditorGUILayout.EndVertical();

                    // 3. Status Tag
                    DrawStatusTag(rep.Status);

                    // 4. Download Size
                    if (rep.DownloadSizeBytes > 0)
                    {
                        GUILayout.Label($"{rep.DownloadSizeBytes / 1024f:0.0} KB", EditorStyles.boldLabel, GUILayout.Width(110));
                    }
                    else if (rep.IsRemoteGroup && rep.Status == AssetSyncStatus.SyncedUpToDate)
                    {
                        GUILayout.Label("0 B (Cached)", EditorStyles.miniLabel, GUILayout.Width(110));
                    }
                    else
                    {
                        GUILayout.Label("-", GUILayout.Width(110));
                    }

                    // 5. Quick Actions
                    if (rep.IsRemoteGroup && rep.Status == AssetSyncStatus.NeedsDownload)
                    {
                        if (GUILayout.Button("Tải Ngay", GUILayout.Width(75)))
                        {
                            _ = DownloadSpecificKeyAsync(rep);
                        }
                    }
                    else if (rep.IsRemoteGroup && rep.Status == AssetSyncStatus.SyncedUpToDate)
                    {
                        if (GUILayout.Button("Xóa Cache", GUILayout.Width(75)))
                        {
                            ClearKeyCache(rep.AddressKey);
                            _ = CheckSingleAssetStatusAsync(rep);
                        }
                    }
                    else
                    {
                        GUILayout.Label("", GUILayout.Width(75));
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawStatusTag(AssetSyncStatus status)
        {
            var oldColor = GUI.color;
            switch (status)
            {
                case AssetSyncStatus.LocalInApk:
                    GUI.color = new Color(0.7f, 0.85f, 1f);
                    GUILayout.Label("[Trong APK / Local]", EditorStyles.boldLabel, GUILayout.Width(180));
                    break;
                case AssetSyncStatus.SyncedUpToDate:
                    GUI.color = new Color(0.4f, 1f, 0.4f);
                    GUILayout.Label("[Đã Đồng Bộ / Mới Nhất]", EditorStyles.boldLabel, GUILayout.Width(180));
                    break;
                case AssetSyncStatus.NeedsDownload:
                    GUI.color = new Color(1f, 0.4f, 0.3f);
                    GUILayout.Label("[Cần Cập Nhật / Chưa Tải]", EditorStyles.boldLabel, GUILayout.Width(180));
                    break;
                case AssetSyncStatus.Checking:
                    GUI.color = Color.yellow;
                    GUILayout.Label("[Đang kiểm tra...]", EditorStyles.miniLabel, GUILayout.Width(180));
                    break;
                default:
                    GUI.color = Color.gray;
                    GUILayout.Label("[Chưa rõ]", EditorStyles.miniLabel, GUILayout.Width(180));
                    break;
            }
            GUI.color = oldColor;
        }

        private async Task CheckCatalogUpdatesAsync()
        {
            _isAuditing = true;
            _catalogStatusText = "Đang kiểm tra file catalog.hash trên Remote Firebase...";
            Repaint();

            try
            {
                var initHandle = Addressables.InitializeAsync();
                await initHandle.Task;

                var checkHandle = Addressables.CheckForCatalogUpdates(false);
                var catalogs = await checkHandle.Task;

                if (catalogs != null && catalogs.Count > 0)
                {
                    _catalogStatusText = $"Phát hiện {catalogs.Count} Catalog mới trên CDN cần cập nhật!";
                    if (EditorUtility.DisplayDialog("Cập nhật Catalog", $"Tìm thấy {catalogs.Count} catalog mới trên máy chủ. Bạn có muốn cập nhật catalog ngay không?", "Cập nhật", "Bỏ qua"))
                    {
                        var updateHandle = Addressables.UpdateCatalogs(catalogs, false);
                        await updateHandle.Task;
                        _catalogStatusText = "Đã cập nhật Catalog CDN mới nhất thành công!";
                    }
                }
                else
                {
                    _catalogStatusText = "Catalog cục bộ hoàn toàn trùng khớp với Remote CDN (catalog.hash giống nhau).";
                }
            }
            catch (Exception ex)
            {
                _catalogStatusText = $"Lỗi kiểm tra catalog: {ex.Message}";
                Debug.LogError($"[CDN Comparator] {ex}");
            }
            finally
            {
                _isAuditing = false;
                Repaint();
            }
        }

        private async Task AuditAllAssetsAsync()
        {
            _isAuditing = true;
            _reports.Clear();
            _totalNeedsUpdate = 0;
            _totalSynced = 0;
            _totalLocalApk = 0;
            _totalDownloadBytes = 0;

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy AddressableAssetSettings trong dự án!", "OK");
                _isAuditing = false;
                return;
            }

            // Thu thập tất cả entries trong các Group
            foreach (var group in settings.groups)
            {
                if (group == null) continue;
                bool isRemote = group.Name.StartsWith("Group_DLC_") || group.Name.Contains("Remote");

                foreach (var entry in group.entries)
                {
                    if (entry == null) continue;

                    var item = new AssetItemReport
                    {
                        GroupName = group.Name,
                        AssetName = entry.MainAsset != null ? entry.MainAsset.name : entry.address,
                        AddressKey = entry.address,
                        IsRemoteGroup = isRemote,
                        Status = isRemote ? AssetSyncStatus.Checking : AssetSyncStatus.LocalInApk
                    };

                    _reports.Add(item);
                }
            }

            Repaint();

            // Khởi tạo Addressables runtime để so sánh
            var initHandle = Addressables.InitializeAsync();
            await initHandle.Task;

            // Kiểm tra từng asset Remote xem dung lượng tải về là bao nhiêu
            foreach (var item in _reports)
            {
                if (!item.IsRemoteGroup)
                {
                    _totalLocalApk++;
                    continue;
                }

                await CheckSingleAssetStatusAsync(item);
            }

            _isAuditing = false;
            Repaint();
        }

        private async Task CheckSingleAssetStatusAsync(AssetItemReport item)
        {
            try
            {
                var sizeHandle = Addressables.GetDownloadSizeAsync(item.AddressKey);
                long bytes = await sizeHandle.Task;

                item.DownloadSizeBytes = bytes;
                if (bytes > 0)
                {
                    item.Status = AssetSyncStatus.NeedsDownload;
                    _totalNeedsUpdate++;
                    _totalDownloadBytes += bytes;
                }
                else
                {
                    item.Status = AssetSyncStatus.SyncedUpToDate;
                    _totalSynced++;
                }
            }
            catch (Exception ex)
            {
                item.Status = AssetSyncStatus.Unknown;
                item.Details = ex.Message;
            }
        }

        private async Task DownloadSpecificKeyAsync(AssetItemReport item)
        {
            item.Status = AssetSyncStatus.Checking;
            Repaint();

            try
            {
                var handle = Addressables.DownloadDependenciesAsync(item.AddressKey, false);
                await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    Addressables.Release(handle);
                    await CheckSingleAssetStatusAsync(item);
                }
                else
                {
                    Addressables.Release(handle);
                    item.Status = AssetSyncStatus.NeedsDownload;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CDN Comparator] Lỗi tải {item.AddressKey}: {ex.Message}");
                item.Status = AssetSyncStatus.NeedsDownload;
            }

            Repaint();
        }

        private void ClearKeyCache(string key)
        {
            Addressables.ClearDependencyCacheAsync(key, true);
            Debug.Log($"[CDN Comparator] Đã xóa cache của key: {key}");
        }

        private void ClearAllAddressablesCache()
        {
            Caching.ClearCache();
            Addressables.CleanBundleCache();
            Debug.Log("<color=#55FF55>[CDN Comparator] Đã dọn sạch toàn bộ AssetBundle Cache trong máy!</color>");
            EditorUtility.DisplayDialog("Thành công", "Đã xóa toàn bộ bộ nhớ Cache Addressables. Bạn có thể nhấn 'Kiểm Tra & So Sánh' để thấy toàn bộ Remote Assets chuyển sang trạng thái [Cần Tải].", "OK");
            _ = AuditAllAssetsAsync();
        }
    }
}
