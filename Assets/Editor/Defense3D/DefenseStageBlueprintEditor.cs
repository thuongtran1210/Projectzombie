using System;
using System.Collections.Generic;
using System.Linq;
using ProjectZombie.Features.Defense3D;
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.EditorTools.Defense3D
{
    /// <summary>
    /// Custom Inspector chuyên nghiệp cho DefenseStageBlueprintSO.
    /// Cung cấp trục Timeline thời gian thực trực quan, bảng ma trận thống kê quái vật
    /// và nút tiền kiểm định hợp lệ (Pre-flight Validation) dành riêng cho Game Designer.
    /// </summary>
    [CustomEditor(typeof(DefenseStageBlueprintSO))]
    public sealed class DefenseStageBlueprintEditor : Editor
    {
        private string _validationMessage;
        private MessageType _validationMessageType = MessageType.Info;
        private bool _showWaveTimeline = true;
        private bool _showEnemySummary = true;

        public override void OnInspectorGUI()
        {
            var blueprint = (DefenseStageBlueprintSO)target;

            // ── 1. BANNER TIÊU ĐỀ CỔ PHONG ĐÔNG SƠN ──
            DrawCophongHeader(blueprint);

            // ── 2. TRỤC THỜI GIAN TRỰC QUAN (TIMELINE PREVIEW) ──
            DrawWaveTimeline(blueprint);

            // ── 3. BẢNG THỐNG KÊ MA TRẬN QUÁI VẬT (ENEMY SUMMARY) ──
            DrawEnemySummary(blueprint);

            // ── 4. CÁC NÚT THAO TÁC CÂN BẰNG NHANH ──
            DrawActionButtons(blueprint);

            // ── 5. HIỂN THỊ THÔNG BÁO KIỂM TRA (VALIDATION BOX) ──
            if (!string.IsNullOrEmpty(_validationMessage))
            {
                EditorGUILayout.HelpBox(_validationMessage, _validationMessageType);
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("── CHI TIẾT CẤU HÌNH THÔNG SỐ ẢI ──", EditorStyles.boldLabel);

            // Vẽ Default Inspector bên dưới để GD vẫn có thể nhập liệu bình thường
            DrawDefaultInspector();
        }

        private void DrawCophongHeader(DefenseStageBlueprintSO bp)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 15,
                normal = { textColor = new Color(0.95f, 0.75f, 0.25f) } // Vàng Đồng Sơn
            };

            int totalWaves = bp.timelineWaves != null ? bp.timelineWaves.Count : 0;
            int totalEnemies = bp.timelineWaves != null ? bp.timelineWaves.Sum(w => w != null ? w.count : 0) : 0;
            int totalMinutes = Mathf.FloorToInt(bp.stageDurationSeconds / 60f);
            int totalSeconds = Mathf.FloorToInt(bp.stageDurationSeconds % 60f);

            EditorGUILayout.LabelField($"🏰 ẢI CHIẾN: {bp.stageName} [{bp.stageId}]", titleStyle);
            EditorGUILayout.LabelField($"⏱️ Thời Lượng: {totalMinutes:D2}:{totalSeconds:D2} | 🌊 Tổng Đợt Sóng: {totalWaves} | 👹 Tổng Số Quái: {totalEnemies}");
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private void DrawWaveTimeline(DefenseStageBlueprintSO bp)
        {
            _showWaveTimeline = EditorGUILayout.Foldout(_showWaveTimeline, "📊 Trục Dòng Thời Gian Xuất Hiện Quái (00:00 - 10:00)", true, EditorStyles.foldoutHeader);
            if (!_showWaveTimeline) return;

            float maxDuration = Mathf.Max(60f, bp.stageDurationSeconds);
            Rect timelineRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(40), GUILayout.ExpandWidth(true));
            
            // Vẽ nền thanh timeline
            EditorGUI.DrawRect(timelineRect, new Color(0.12f, 0.12f, 0.14f));

            // Vẽ các vạch chia phút (mỗi 1 phút)
            for (int m = 1; m < Mathf.CeilToInt(maxDuration / 60f); m++)
            {
                float t = m * 60f;
                float x = timelineRect.x + (t / maxDuration) * timelineRect.width;
                EditorGUI.DrawRect(new Rect(x, timelineRect.y, 1, timelineRect.height), new Color(0.3f, 0.3f, 0.35f, 0.5f));
            }

            // Vẽ các đợt quái trên timeline
            if (bp.timelineWaves != null)
            {
                foreach (var wave in bp.timelineWaves)
                {
                    if (wave == null) continue;
                    float progress = Mathf.Clamp01(wave.triggerTime / maxDuration);
                    float x = timelineRect.x + progress * timelineRect.width;

                    Color waveColor = wave.zone == DefenseSpawnZoneType.CanalWater ? new Color(0.2f, 0.8f, 1f) :
                                      wave.zone == DefenseSpawnZoneType.NorthCanopy ? new Color(0.8f, 0.3f, 1f) :
                                      new Color(0.2f, 1f, 0.4f);

                    EditorGUI.DrawRect(new Rect(x - 2, timelineRect.y + 4, 4, timelineRect.height - 8), waveColor);
                }
            }

            // Vẽ vị trí Đại Boss
            if (bp.arenaBoss != null)
            {
                float bossProgress = Mathf.Clamp01(bp.bossSpawnTime / maxDuration);
                float bx = timelineRect.x + bossProgress * timelineRect.width;
                EditorGUI.DrawRect(new Rect(bx - 3, timelineRect.y + 2, 6, timelineRect.height - 4), new Color(1f, 0.15f, 0.15f));
            }

            // Chú thích màu sắc
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("<color=#33FF66>■</color> Bộ (Đông/Nam)", new GUIStyle(EditorStyles.miniLabel) { richText = true }, GUILayout.Width(100));
            EditorGUILayout.LabelField("<color=#33CCFF>■</color> Thủy (Kênh)", new GUIStyle(EditorStyles.miniLabel) { richText = true }, GUILayout.Width(90));
            EditorGUILayout.LabelField("<color=#CC55FF>■</color> Không (Rừng)", new GUIStyle(EditorStyles.miniLabel) { richText = true }, GUILayout.Width(90));
            EditorGUILayout.LabelField("<color=#FF3333>■</color> Boss Đấu Trường", new GUIStyle(EditorStyles.miniLabel) { richText = true });
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
        }

        private void DrawEnemySummary(DefenseStageBlueprintSO bp)
        {
            _showEnemySummary = EditorGUILayout.Foldout(_showEnemySummary, "📈 Bảng Ma Trận Phân Bổ Số Lượng Yêu Ma", true, EditorStyles.foldoutHeader);
            if (!_showEnemySummary || bp.timelineWaves == null || bp.timelineWaves.Count == 0) return;

            var stats = new Dictionary<string, int>();
            foreach (var w in bp.timelineWaves)
            {
                if (w == null || w.enemy == null) continue;
                string name = w.enemy.DisplayName;
                if (!stats.ContainsKey(name)) stats[name] = 0;
                stats[name] += w.count;
            }

            int total = stats.Values.Sum();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foreach (var pair in stats.OrderByDescending(p => p.Value))
            {
                float percent = total > 0 ? (pair.Value * 100f / total) : 0f;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"• {pair.Key}", GUILayout.Width(160));
                EditorGUILayout.LabelField($"{pair.Value} con ({percent:F1}%)", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
            }
            if (bp.arenaBoss != null)
            {
                EditorGUILayout.LabelField($"👑 Đại Boss: {bp.arenaBoss.DisplayName} (Máu: {bp.bossHealthOverride:N0} HP)", EditorStyles.boldLabel);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private void DrawActionButtons(DefenseStageBlueprintSO bp)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("🔍 Kiểm Tra Hợp Lệ (Validate)", GUILayout.Height(28)))
            {
                try
                {
                    var plan = bp.Compile();
                    int totalEvents = plan.Events.Count;
                    int ordinarySlots = plan.Events.Where(e => e.Kind == DefenseSpawnKind.Enemy).Sum(e => e.Count);
                    var boss = plan.Events.FirstOrDefault(e => e.Kind == DefenseSpawnKind.ArenaBoss);

                    _validationMessage = $"✅ Cấu hình hợp lệ 100%!\n• Phiên bản Plan: {plan.ProfileVersion}\n• Số đợt quái: {totalEvents - 1} đợt thường ({ordinarySlots} con)\n• Đại Boss xuất hiện tại: {boss.TriggerTime:F0}s (Máu: {plan.BossHealth} HP)\n• Sẵn sàng cho trận chiến!";
                    _validationMessageType = MessageType.Info;
                }
                catch (Exception ex)
                {
                    _validationMessage = $"❌ Phát hiện lỗi cấu hình:\n{ex.Message}";
                    _validationMessageType = MessageType.Error;
                }
            }

            if (GUILayout.Button("⏱️ Sắp Xếp Theo Thời Gian", GUILayout.Height(28)))
            {
                if (bp.timelineWaves != null && bp.timelineWaves.Count > 1)
                {
                    Undo.RecordObject(bp, "Sort Waves by Time");
                    bp.timelineWaves = bp.timelineWaves.OrderBy(w => w != null ? w.triggerTime : 0f).ToList();
                    EditorUtility.SetDirty(bp);
                    _validationMessage = "Đã sắp xếp toàn bộ đợt sóng theo thứ tự thời gian tăng dần thành công.";
                    _validationMessageType = MessageType.Info;
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
