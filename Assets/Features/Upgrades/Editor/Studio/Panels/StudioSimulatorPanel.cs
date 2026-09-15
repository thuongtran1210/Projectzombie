#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.Features.Upgrades.Editor.Studio.Panels
{
    /// <summary>
    /// Panel chuyên trách giao diện Giả Lập Bốc Thẻ Monte Carlo (Gacha Simulator) và biểu đồ phân phối xác suất.
    /// </summary>
    public class StudioSimulatorPanel
    {
        private MythicArchetype _simArchetype = MythicArchetype.PhuDongThienUy;
        private int _simRollCount = 500;
        private SimulationResult? _lastSimResult;
        private Vector2 _scrollPos;

        public void Draw(List<UpgradeData> allUpgrades)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("<b>🎲 GIẢ LẬP BỐC THẺ ROGUELITE (MONTE CARLO SIMULATOR)</b>", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Công cụ giả lập hàng nghìn lượt Level Up để Game Designer đánh giá phân bổ xác suất, tỉ lệ xuất hiện của thẻ và độ cân bằng của hệ thống Thần Binh Thuật.", MessageType.Info);

            GUILayout.Space(8);

            // Controls
            GUILayout.BeginHorizontal();
            _simArchetype = (MythicArchetype)EditorGUILayout.EnumPopup("Đại Lõi Giả Lập:", _simArchetype, GUILayout.Width(350));
            _simRollCount = EditorGUILayout.IntSlider("Số Lượt Level Up:", _simRollCount, 50, 5000, GUILayout.Width(350));

            if (GUILayout.Button("🚀 Chạy Giả Lập", GUILayout.Height(24), GUILayout.Width(130)))
            {
                _lastSimResult = UpgradeStudioSimulator.RunSimulation(allUpgrades, _simArchetype, _simRollCount);
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

                _scrollPos = GUILayout.BeginScrollView(_scrollPos);

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
    }
}
#endif
