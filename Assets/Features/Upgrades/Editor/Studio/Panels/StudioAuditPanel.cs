#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.Features.Upgrades.Editor.Studio.Panels
{
    /// <summary>
    /// Panel chuyên trách Thẩm Định & Quét Lỗi Cơ Sở Dữ Liệu (Health Audit) kèm bộ lọc 5 nhóm và Copy Log Markdown.
    /// </summary>
    public class StudioAuditPanel
    {
        private readonly Action<UpgradeData> _onFixIssue;

        private List<AuditIssue> _auditIssues = new List<AuditIssue>();
        private int _auditSeverityFilter = 0; // 0 = Tất cả, 1 = Chỉ Errors, 2 = Chỉ Warnings
        private AuditCategory _auditCategoryFilter = AuditCategory.All;
        private string _auditSearchQuery = string.Empty;
        private Vector2 _scrollPos;

        public StudioAuditPanel(Action<UpgradeData> onFixIssue)
        {
            _onFixIssue = onFixIssue;
        }

        public void Draw(List<UpgradeData> allUpgrades)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("<b>🔍 THẨM ĐỊNH TOÀN DIỆN DATABASE (HEALTH AUDIT)</b>", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Quét toàn bộ thẻ nâng cấp để phát hiện các lỗi cấu hình như thiếu Icon, trùng ID, ID rỗng, sai thông số hoặc liên kết hỏng.", MessageType.Info);

            GUILayout.Space(6);

            // Nút Quét Lỗi & Nút Copy Báo Cáo
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🔍 Bắt Đầu Quét Lỗi Toàn Bộ Database", GUILayout.Height(30)))
            {
                _auditIssues = UpgradeStudioAuditEngine.RunAudit(allUpgrades);
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
                int countMechanic = _auditIssues.Count(i => i.Category == AuditCategory.MechanicReadiness);

                if (GUILayout.Toggle(_auditCategoryFilter == AuditCategory.All, "Tất Cả", EditorStyles.miniButton)) _auditCategoryFilter = AuditCategory.All;
                if (GUILayout.Toggle(_auditCategoryFilter == AuditCategory.Identification, $"ID/Trùng ({countId})", EditorStyles.miniButton)) _auditCategoryFilter = AuditCategory.Identification;
                if (GUILayout.Toggle(_auditCategoryFilter == AuditCategory.IconMissing, $"Thiếu Icon ({countIcon})", EditorStyles.miniButton)) _auditCategoryFilter = AuditCategory.IconMissing;
                if (GUILayout.Toggle(_auditCategoryFilter == AuditCategory.SpawnWeight, $"Trọng Số ({countWeight})", EditorStyles.miniButton)) _auditCategoryFilter = AuditCategory.SpawnWeight;
                if (GUILayout.Toggle(_auditCategoryFilter == AuditCategory.EvolutionLink, $"Vũ Khí/Tiến Hóa ({countLink})", EditorStyles.miniButton)) _auditCategoryFilter = AuditCategory.EvolutionLink;
                if (GUILayout.Toggle(_auditCategoryFilter == AuditCategory.TraitArchetype, $"Lõi Thần Binh ({countTrait})", EditorStyles.miniButton)) _auditCategoryFilter = AuditCategory.TraitArchetype;
                if (GUILayout.Toggle(_auditCategoryFilter == AuditCategory.MechanicReadiness, $"⚙️ Chưa Hoạt Động ({countMechanic})", EditorStyles.miniButton)) _auditCategoryFilter = AuditCategory.MechanicReadiness;

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

                _scrollPos = GUILayout.BeginScrollView(_scrollPos);

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
                                _onFixIssue?.Invoke(issue.TargetAsset);
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
                if (_auditSeverityFilter == 1 && issue.Severity != AuditIssue.IssueSeverity.Error) return false;
                if (_auditSeverityFilter == 2 && issue.Severity != AuditIssue.IssueSeverity.Warning) return false;

                if (_auditCategoryFilter != AuditCategory.All && issue.Category != _auditCategoryFilter) return false;

                if (!string.IsNullOrEmpty(_auditSearchQuery))
                {
                    bool matchMsg = issue.Message != null && issue.Message.IndexOf(_auditSearchQuery, StringComparison.OrdinalIgnoreCase) >= 0;
                    bool matchAsset = issue.TargetAsset != null && (!string.IsNullOrEmpty(issue.TargetAsset.id) && issue.TargetAsset.id.IndexOf(_auditSearchQuery, StringComparison.OrdinalIgnoreCase) >= 0 || !string.IsNullOrEmpty(issue.TargetAsset.upgradeName) && issue.TargetAsset.upgradeName.IndexOf(_auditSearchQuery, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (!matchMsg && !matchAsset) return false;
                }

                return true;
            }).ToList();
        }
    }
}
#endif
