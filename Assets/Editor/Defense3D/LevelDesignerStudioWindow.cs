using System;
using System.Collections.Generic;
using System.Linq;
using ProjectZombie.Features.Defense3D;
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.EditorTools.Defense3D
{
    /// <summary>
    /// Giao diện làm việc chuyên nghiệp (Level Designer Studio) dành cho Game Designer.
    /// Cung cấp:
    /// 1. Trục Dòng Thời Gian Trực Quan (Interactive Wave Timeline) kéo thả đợt quái bằng chuột.
    /// 2. Bộ Tự Động Sinh Wave (Smart Curve Wave Generator) từ AnimationCurve.
    /// 3. Bảng Mô Phỏng Cân Bằng (DPS, Kinh Tế Cổ Mộc & Radar Hiệu Năng Di Động).
    /// 4. Quản lý trực quan các Vùng Spawn & Điểm Hành Quân trên bản đồ.
    /// </summary>
    public sealed class LevelDesignerStudioWindow : EditorWindow
    {
        private enum StudioTab
        {
            Timeline = 0,
            SmartGenerator = 1,
            BalanceSimulator = 2,
            SpatialZones = 3
        }

        [SerializeField] private DefenseStageBlueprintSO _selectedBlueprint;
        [SerializeField] private StageDifficultyCurveProfileSO _selectedCurveProfile;

        private StudioTab _currentTab = StudioTab.Timeline;
        private Vector2 _timelineScroll;
        private Vector2 _sidebarScroll;
        private Vector2 _simulatorScroll;

        // Trạng thái Timeline tương tác
        private float _timelineZoom = 1.0f; // Tỉ lệ pixel trên giây
        private int _selectedWaveIndex = -1;
        private bool _isDraggingWave = false;
        private int _draggingWaveIndex = -1;
        private float _dragStartX = 0f;
        private float _dragStartTime = 0f;

        // Trạng thái Bộ sinh tự động (Smart Generator)
        private int _genTargetWaveCount = 34;
        private float _genTargetDuration = 600f;
        private bool _genIncludeHordeMilestones = true;
        private DefenseEnemyDataSO _genDefaultEnemy;
        private DefenseEnemyDataSO _genLeadEnemy;
        private DefenseEnemyDataSO _genWaterEnemy;
        private DefenseEnemyDataSO _genFlyEnemy;

        // Trạng thái Báo cáo kiểm định
        private DefenseStageValidationReport _lastValidationReport;

        [MenuItem("ProjectZombie/3. 🗺️ Stage & Level/Level Designer Studio (Studio Thiết Kế Ải)", priority = 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<LevelDesignerStudioWindow>("Level Designer Studio");
            window.minSize = new Vector2(850, 600);
            window.Show();
        }

        private void OnEnable()
        {
            if (_selectedBlueprint == null)
            {
                // Thử tự động nạp Stage01 nếu có
                _selectedBlueprint = AssetDatabase.LoadAssetAtPath<DefenseStageBlueprintSO>("Assets/Resources/Defense3D/Stages/Stage01_SongQue_Blueprint.asset");
                if (_selectedBlueprint == null)
                {
                    string[] guids = AssetDatabase.FindAssets("t:DefenseStageBlueprintSO");
                    if (guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        _selectedBlueprint = AssetDatabase.LoadAssetAtPath<DefenseStageBlueprintSO>(path);
                    }
                }
            }

            if (_selectedCurveProfile == null && _selectedBlueprint != null)
            {
                _selectedCurveProfile = _selectedBlueprint.difficultyProfile;
            }

            LoadArchetypeReferences();
        }

        private void LoadArchetypeReferences()
        {
            if (_genDefaultEnemy == null)
                _genDefaultEnemy = AssetDatabase.LoadAssetAtPath<DefenseEnemyDataSO>("Assets/Resources/Defense3D/Enemies/magiap.asset");
            if (_genLeadEnemy == null)
                _genLeadEnemy = AssetDatabase.LoadAssetAtPath<DefenseEnemyDataSO>("Assets/Resources/Defense3D/Enemies/phicuong.asset");
            if (_genWaterEnemy == null)
                _genWaterEnemy = AssetDatabase.LoadAssetAtPath<DefenseEnemyDataSO>("Assets/Resources/Defense3D/Enemies/thuyquai.asset");
            if (_genFlyEnemy == null)
                _genFlyEnemy = AssetDatabase.LoadAssetAtPath<DefenseEnemyDataSO>("Assets/Resources/Defense3D/Enemies/huyetdoi.asset");
        }

        private void OnGUI()
        {
            DrawStudioHeader();

            if (_selectedBlueprint == null)
            {
                EditorGUILayout.HelpBox("Vui lòng chọn hoặc tạo một bản thiết kế ải (DefenseStageBlueprintSO) để bắt đầu thiết kế.", MessageType.Warning);
                if (GUILayout.Button("✨ Khởi Tạo Stage 01 Master Blueprint Chuẩn", GUILayout.Height(35)))
                {
                    DefenseStageBlueprintInstaller.CreateOrUpdateStage01Blueprint();
                    _selectedBlueprint = AssetDatabase.LoadAssetAtPath<DefenseStageBlueprintSO>("Assets/Resources/Defense3D/Stages/Stage01_SongQue_Blueprint.asset");
                }
                return;
            }

            DrawNavigationToolbar();

            EditorGUILayout.Space(5);

            switch (_currentTab)
            {
                case StudioTab.Timeline:
                    DrawTimelineTab();
                    break;
                case StudioTab.SmartGenerator:
                    DrawSmartGeneratorTab();
                    break;
                case StudioTab.BalanceSimulator:
                    DrawBalanceSimulatorTab();
                    break;
                case StudioTab.SpatialZones:
                    DrawSpatialZonesTab();
                    break;
            }

            DrawBottomStatusBar();
        }

        #region 1. Header & Navigation

        private void DrawStudioHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                normal = { textColor = new Color(0.98f, 0.78f, 0.25f) } // Cổ Phong Vàng Đồng
            };
            EditorGUILayout.LabelField("🗺️ LEVEL DESIGNER STUDIO — THIẾT KẾ ẢI & WAVE CHUYÊN NGHIỆP", titleStyle);

            if (GUILayout.Button("🔍 Kiểm Định Ải (Validate)", GUILayout.Width(170), GUILayout.Height(24)))
            {
                _lastValidationReport = DefenseStageValidator.Validate(_selectedBlueprint);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            _selectedBlueprint = (DefenseStageBlueprintSO)EditorGUILayout.ObjectField("Ải Đang Chọn:", _selectedBlueprint, typeof(DefenseStageBlueprintSO), false);
            _selectedCurveProfile = (StageDifficultyCurveProfileSO)EditorGUILayout.ObjectField("Đường Cong Độ Khó:", _selectedCurveProfile, typeof(StageDifficultyCurveProfileSO), false);

            if (_selectedBlueprint != null && _selectedBlueprint.difficultyProfile != _selectedCurveProfile)
            {
                _selectedBlueprint.difficultyProfile = _selectedCurveProfile;
                EditorUtility.SetDirty(_selectedBlueprint);
            }

            EditorGUILayout.EndHorizontal();

            if (_selectedBlueprint != null)
            {
                int waveCount = _selectedBlueprint.timelineWaves != null ? _selectedBlueprint.timelineWaves.Count : 0;
                int mobCount = _selectedBlueprint.timelineWaves != null ? _selectedBlueprint.timelineWaves.Sum(w => w != null ? w.count : 0) : 0;
                int minutes = Mathf.FloorToInt(_selectedBlueprint.stageDurationSeconds / 60f);
                int seconds = Mathf.FloorToInt(_selectedBlueprint.stageDurationSeconds % 60f);

                EditorGUILayout.LabelField($"🏰 <b>{_selectedBlueprint.stageName}</b> [{_selectedBlueprint.stageId}]  •  " +
                                          $"⏱️ Thời Lượng: <b>{minutes:D2}:{seconds:D2}</b>  •  " +
                                          $"🌊 Đợt Sóng: <b>{waveCount}</b>  •  " +
                                          $"👹 Tổng Quái: <b>{mobCount}</b>  •  " +
                                          $"📍 Số Vùng: <b>{_selectedBlueprint.spawnZones?.Length ?? 0}</b>", new GUIStyle(EditorStyles.label) { richText = true });
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawNavigationToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            string[] tabs = { "📊 Dòng Thời Gian (Timeline)", "⚡ Bộ Sinh Wave (Smart Curve)", "⚖️ Bảng Mô Phỏng Cân Bằng (DPS & Economy)", "📍 Vùng Xuất Hiện (Spatial Zones)" };
            _currentTab = (StudioTab)GUILayout.Toolbar((int)_currentTab, tabs, EditorStyles.toolbarButton, GUILayout.Height(24));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawBottomStatusBar()
        {
            if (_lastValidationReport != null)
            {
                EditorGUILayout.Space(4);
                MessageType msgType = _lastValidationReport.IsValid 
                    ? (_lastValidationReport.Warnings.Count > 0 ? MessageType.Warning : MessageType.Info)
                    : MessageType.Error;
                EditorGUILayout.HelpBox(_lastValidationReport.ToFormattedSummary(), msgType);
            }
        }

        #endregion

        #region 2. Tab 1: Interactive Wave Timeline

        private void DrawTimelineTab()
        {
            EditorGUILayout.BeginHorizontal();

            // ── CỘT TRÁI: DÒNG THỜI GIAN TRỰC QUAN ──
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            DrawTimelineControls();
            DrawTimelineCanvas();
            EditorGUILayout.EndVertical();

            // ── CỘT PHẢI: CHI TIẾT ĐỢT QUÁI ĐANG CHỌN (INSPECTOR DRAWER) ──
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(300));
            DrawWaveInspectorDrawer();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawTimelineControls()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Kính Lúp Phóng To (Zoom):", GUILayout.Width(130));
            _timelineZoom = EditorGUILayout.Slider(_timelineZoom, 0.5f, 3.0f, GUILayout.Width(180));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+ Thêm Đợt Quái Mới", EditorStyles.toolbarButton, GUILayout.Width(140)))
            {
                AddNewWaveAtCurrentTime();
            }

            if (GUILayout.Button("⏱️ Sắp Xếp Theo Giây", EditorStyles.toolbarButton, GUILayout.Width(130)))
            {
                SortWavesByTime();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawTimelineCanvas()
        {
            float stageDuration = Mathf.Max(60f, _selectedBlueprint.stageDurationSeconds);
            float totalWidth = stageDuration * 2.0f * _timelineZoom;
            float rulerHeight = 24f;
            float rowHeight = 44f;
            int laneCount = 4; // 4 Hàng tương ứng với 4 Zone: EastBank, NorthCanopy, SouthBank, CanalWater
            float totalCanvasHeight = rulerHeight + (laneCount * rowHeight) + 10f;

            _timelineScroll = EditorGUILayout.BeginScrollView(_timelineScroll, GUILayout.Height(360));
            Rect canvasRect = GUILayoutUtility.GetRect(totalWidth, totalCanvasHeight);

            // Nền tối chuẩn Studio
            EditorGUI.DrawRect(canvasRect, new Color(0.10f, 0.10f, 0.12f));

            // Vẽ thước thời gian (Ruler)
            DrawTimeRuler(canvasRect, stageDuration, rulerHeight);

            // Vẽ các dải biến cố Pacing Milestones (Horde, Breather, Elite)
            DrawMilestoneBands(canvasRect, stageDuration, rulerHeight, totalCanvasHeight - rulerHeight);

            // Vẽ các đường ngăn cách Zone Lanes
            DrawLaneDividers(canvasRect, rulerHeight, rowHeight, laneCount);

            // Vẽ các thẻ Wave Cards
            DrawWaveCards(canvasRect, stageDuration, rulerHeight, rowHeight);

            // Xử lý sự kiện kéo thả chuột
            HandleTimelineMouseEvents(canvasRect, stageDuration);

            EditorGUILayout.EndScrollView();
        }

        private void DrawTimeRuler(Rect canvasRect, float stageDuration, float rulerHeight)
        {
            Rect rulerRect = new Rect(canvasRect.x, canvasRect.y, canvasRect.width, rulerHeight);
            EditorGUI.DrawRect(rulerRect, new Color(0.14f, 0.14f, 0.17f));

            GUIStyle timeLabelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 10,
                normal = { textColor = new Color(0.75f, 0.75f, 0.80f) }
            };

            for (float t = 0; t <= stageDuration; t += 10f)
            {
                float x = canvasRect.x + (t / stageDuration) * canvasRect.width;
                bool isMinute = (t % 60f) == 0;
                float tickHeight = isMinute ? rulerHeight : rulerHeight * 0.4f;
                Color tickColor = isMinute ? new Color(0.5f, 0.5f, 0.55f) : new Color(0.3f, 0.3f, 0.35f);

                EditorGUI.DrawRect(new Rect(x, rulerRect.yMax - tickHeight, 1, tickHeight), tickColor);

                if (isMinute)
                {
                    int m = Mathf.FloorToInt(t / 60f);
                    GUI.Label(new Rect(x + 3, rulerRect.y + 2, 45, 16), $"{m:D2}:00", timeLabelStyle);
                }
            }
        }

        private void DrawMilestoneBands(Rect canvasRect, float stageDuration, float rulerY, float contentHeight)
        {
            if (_selectedCurveProfile == null || _selectedCurveProfile.PacingMilestones == null) return;

            foreach (var milestone in _selectedCurveProfile.PacingMilestones)
            {
                if (milestone == null) continue;
                float startX = canvasRect.x + (milestone.triggerTime / stageDuration) * canvasRect.width;
                float width = (milestone.duration / stageDuration) * canvasRect.width;

                Color bandColor = milestone.pacingType switch
                {
                    StagePacingType.HordeRush => new Color(1f, 0.2f, 0.1f, 0.15f),
                    StagePacingType.EliteMiniboss => new Color(0.8f, 0.2f, 0.9f, 0.15f),
                    StagePacingType.Breather => new Color(0.1f, 0.8f, 0.4f, 0.12f),
                    StagePacingType.ClimaxBoss => new Color(1f, 0.0f, 0.0f, 0.25f),
                    _ => new Color(0.5f, 0.5f, 0.5f, 0.1f)
                };

                Rect bandRect = new Rect(startX, canvasRect.y + rulerY, width, contentHeight);
                EditorGUI.DrawRect(bandRect, bandColor);

                GUIStyle milestoneTag = new GUIStyle(EditorStyles.miniBoldLabel)
                {
                    normal = { textColor = new Color(bandColor.r, bandColor.g, bandColor.b, 0.85f) }
                };
                GUI.Label(new Rect(startX + 2, canvasRect.y + rulerY + 2, width, 16), milestone.displayName, milestoneTag);
            }
        }

        private void DrawLaneDividers(Rect canvasRect, float rulerY, float rowHeight, int laneCount)
        {
            string[] laneNames = { "🌊 Kênh Nước (Canal)", "🌿 Tán Rừng (North)", "🌾 Bờ Nam (South)", "🏰 Bờ Đông (EastBank)" };
            GUIStyle laneHeaderStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(0.5f, 0.5f, 0.55f, 0.4f) }
            };

            for (int i = 0; i < laneCount; i++)
            {
                float y = canvasRect.y + rulerY + i * rowHeight;
                EditorGUI.DrawRect(new Rect(canvasRect.x, y, canvasRect.width, 1), new Color(0.2f, 0.2f, 0.24f));
                GUI.Label(new Rect(canvasRect.x + 8, y + 2, 200, 16), laneNames[i], laneHeaderStyle);
            }
        }

        private void DrawWaveCards(Rect canvasRect, float stageDuration, float rulerY, float rowHeight)
        {
            if (_selectedBlueprint.timelineWaves == null) return;

            for (int i = 0; i < _selectedBlueprint.timelineWaves.Count; i++)
            {
                var wave = _selectedBlueprint.timelineWaves[i];
                if (wave == null) continue;

                int laneIndex = wave.zone switch
                {
                    DefenseSpawnZoneType.CanalWater => 0,
                    DefenseSpawnZoneType.NorthCanopy => 1,
                    DefenseSpawnZoneType.SouthBank => 2,
                    _ => 3 // EastBank & default
                };

                float x = canvasRect.x + (wave.triggerTime / stageDuration) * canvasRect.width;
                float y = canvasRect.y + rulerY + laneIndex * rowHeight + 8f;
                float cardWidth = Mathf.Max(60f, 26f + (wave.count * 8f));
                float cardHeight = 28f;

                Rect cardRect = new Rect(x, y, cardWidth, cardHeight);
                bool isSelected = (i == _selectedWaveIndex);

                Color zoneColor = wave.zone switch
                {
                    DefenseSpawnZoneType.CanalWater => new Color(0.1f, 0.75f, 0.95f),
                    DefenseSpawnZoneType.NorthCanopy => new Color(0.35f, 0.85f, 0.35f),
                    DefenseSpawnZoneType.SouthBank => new Color(0.95f, 0.60f, 0.20f),
                    _ => new Color(0.95f, 0.80f, 0.25f) // EastBank vàng
                };

                // Vẽ nền Card Wave
                EditorGUI.DrawRect(cardRect, isSelected ? new Color(0.95f, 0.95f, 1f) : new Color(0.18f, 0.18f, 0.22f));
                EditorGUI.DrawRect(new Rect(cardRect.x, cardRect.y, 4, cardRect.height), zoneColor);

                // Khung viền khi được chọn
                if (isSelected)
                {
                    Handles.DrawSolidRectangleWithOutline(cardRect, Color.clear, new Color(0.2f, 0.6f, 1f));
                }

                string enemyName = wave.enemy != null ? wave.enemy.DisplayName : "Chưa Gán";
                string cardText = $"<b>{wave.eventId}</b> (x{wave.count})\n<size=9>{enemyName}</size>";
                GUIStyle cardTextStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    richText = true,
                    normal = { textColor = isSelected ? Color.black : Color.white }
                };

                GUI.Label(new Rect(cardRect.x + 6, cardRect.y + 1, cardRect.width - 8, cardRect.height), cardText, cardTextStyle);
            }
        }

        private void HandleTimelineMouseEvents(Rect canvasRect, float stageDuration)
        {
            Event e = Event.current;
            if (e == null) return;

            if (e.type == EventType.MouseDown && canvasRect.Contains(e.mousePosition))
            {
                // Kiểm tra xem có bấm trúng Wave Card nào không
                int hitWave = HitTestWave(e.mousePosition, canvasRect, stageDuration);
                if (hitWave >= 0)
                {
                    _selectedWaveIndex = hitWave;
                    _isDraggingWave = true;
                    _draggingWaveIndex = hitWave;
                    _dragStartX = e.mousePosition.x;
                    _dragStartTime = _selectedBlueprint.timelineWaves[hitWave].triggerTime;
                    e.Use();
                    Repaint();
                }
                else
                {
                    _selectedWaveIndex = -1;
                    Repaint();
                }
            }
            else if (e.type == EventType.MouseDrag && _isDraggingWave && _draggingWaveIndex >= 0)
            {
                float deltaX = e.mousePosition.x - _dragStartX;
                float deltaTime = (deltaX / canvasRect.width) * stageDuration;
                float newTime = Mathf.Clamp(_dragStartTime + deltaTime, 0f, _selectedBlueprint.stageDurationSeconds - 1f);

                // Snapping 1s
                newTime = Mathf.Round(newTime);

                Undo.RecordObject(_selectedBlueprint, "Drag Wave Trigger Time");
                _selectedBlueprint.timelineWaves[_draggingWaveIndex].triggerTime = newTime;
                EditorUtility.SetDirty(_selectedBlueprint);
                e.Use();
                Repaint();
            }
            else if (e.type == EventType.MouseUp && _isDraggingWave)
            {
                _isDraggingWave = false;
                _draggingWaveIndex = -1;
                e.Use();
                Repaint();
            }
        }

        private int HitTestWave(Vector2 mousePos, Rect canvasRect, float stageDuration)
        {
            if (_selectedBlueprint.timelineWaves == null) return -1;
            float rulerY = 24f;
            float rowHeight = 44f;

            for (int i = 0; i < _selectedBlueprint.timelineWaves.Count; i++)
            {
                var wave = _selectedBlueprint.timelineWaves[i];
                if (wave == null) continue;

                int laneIndex = wave.zone switch
                {
                    DefenseSpawnZoneType.CanalWater => 0,
                    DefenseSpawnZoneType.NorthCanopy => 1,
                    DefenseSpawnZoneType.SouthBank => 2,
                    _ => 3
                };

                float x = canvasRect.x + (wave.triggerTime / stageDuration) * canvasRect.width;
                float y = canvasRect.y + rulerY + laneIndex * rowHeight + 8f;
                float cardWidth = Mathf.Max(60f, 26f + (wave.count * 8f));
                Rect rect = new Rect(x, y, cardWidth, 28f);

                if (rect.Contains(mousePos)) return i;
            }
            return -1;
        }

        private void DrawWaveInspectorDrawer()
        {
            EditorGUILayout.LabelField("🔍 CHI TIẾT ĐỢT QUÁI (WAVE INSPECTOR)", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            if (_selectedWaveIndex < 0 || _selectedWaveIndex >= _selectedBlueprint.timelineWaves.Count)
            {
                EditorGUILayout.HelpBox("Chạm vào một Wave Card trên Timeline để chỉnh sửa nhanh thông số.", MessageType.Info);
                return;
            }

            var wave = _selectedBlueprint.timelineWaves[_selectedWaveIndex];
            if (wave == null) return;

            _sidebarScroll = EditorGUILayout.BeginScrollView(_sidebarScroll);

            EditorGUI.BeginChangeCheck();

            wave.eventId = EditorGUILayout.TextField("Mã Đợt (EventId):", wave.eventId);
            wave.triggerTime = EditorGUILayout.FloatField("Thời Điểm Xuất Hiện (s):", wave.triggerTime);

            int minutes = Mathf.FloorToInt(wave.triggerTime / 60f);
            int seconds = Mathf.FloorToInt(wave.triggerTime % 60f);
            EditorGUILayout.LabelField($"⏱️ Mốc Thời Gian: <b>{minutes:D2}:{seconds:D2}</b>", new GUIStyle(EditorStyles.label) { richText = true });

            wave.zone = (DefenseSpawnZoneType)EditorGUILayout.EnumPopup("Vùng Xuất Hiện:", wave.zone);
            wave.enemy = (DefenseEnemyDataSO)EditorGUILayout.ObjectField("Quái Chính:", wave.enemy, typeof(DefenseEnemyDataSO), false);
            wave.count = EditorGUILayout.IntSlider("Số Lượng Quái:", wave.count, 1, 24);
            wave.leadEnemy = (DefenseEnemyDataSO)EditorGUILayout.ObjectField("Quái Đầu Đàn (Lead):", wave.leadEnemy, typeof(DefenseEnemyDataSO), false);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_selectedBlueprint, "Modify Wave Entry");
                EditorUtility.SetDirty(_selectedBlueprint);
            }

            EditorGUILayout.Space(12);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("📋 Nhân Bản (Clone)", GUILayout.Height(26)))
            {
                CloneSelectedWave();
            }
            if (GUILayout.Button("🗑️ Xóa Wave", GUILayout.Height(26)))
            {
                DeleteSelectedWave();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
        }

        private void AddNewWaveAtCurrentTime()
        {
            if (_selectedBlueprint.timelineWaves == null)
                _selectedBlueprint.timelineWaves = new List<DefenseStageWaveEntry>();

            Undo.RecordObject(_selectedBlueprint, "Add New Wave");
            int nextIndex = _selectedBlueprint.timelineWaves.Count + 1;
            float trigger = _selectedBlueprint.timelineWaves.Count > 0 ? _selectedBlueprint.timelineWaves.Last().triggerTime + 18f : 10f;
            trigger = Mathf.Min(trigger, _selectedBlueprint.stageDurationSeconds - 10f);

            var newWave = new DefenseStageWaveEntry
            {
                eventId = $"wave-{nextIndex:D2}",
                triggerTime = trigger,
                enemy = _genDefaultEnemy,
                count = 4,
                zone = DefenseSpawnZoneType.EastBank
            };

            _selectedBlueprint.timelineWaves.Add(newWave);
            _selectedWaveIndex = _selectedBlueprint.timelineWaves.Count - 1;
            EditorUtility.SetDirty(_selectedBlueprint);
        }

        private void CloneSelectedWave()
        {
            if (_selectedWaveIndex < 0 || _selectedWaveIndex >= _selectedBlueprint.timelineWaves.Count) return;
            var current = _selectedBlueprint.timelineWaves[_selectedWaveIndex];
            Undo.RecordObject(_selectedBlueprint, "Clone Wave");

            var clone = new DefenseStageWaveEntry
            {
                eventId = current.eventId + "-copy",
                triggerTime = Mathf.Min(current.triggerTime + 15f, _selectedBlueprint.stageDurationSeconds - 5f),
                enemy = current.enemy,
                count = current.count,
                zone = current.zone,
                leadEnemy = current.leadEnemy
            };

            _selectedBlueprint.timelineWaves.Add(clone);
            _selectedWaveIndex = _selectedBlueprint.timelineWaves.Count - 1;
            EditorUtility.SetDirty(_selectedBlueprint);
        }

        private void DeleteSelectedWave()
        {
            if (_selectedWaveIndex < 0 || _selectedWaveIndex >= _selectedBlueprint.timelineWaves.Count) return;
            Undo.RecordObject(_selectedBlueprint, "Delete Wave");
            _selectedBlueprint.timelineWaves.RemoveAt(_selectedWaveIndex);
            _selectedWaveIndex = -1;
            EditorUtility.SetDirty(_selectedBlueprint);
        }

        private void SortWavesByTime()
        {
            if (_selectedBlueprint.timelineWaves == null) return;
            Undo.RecordObject(_selectedBlueprint, "Sort Waves By Time");
            _selectedBlueprint.timelineWaves = _selectedBlueprint.timelineWaves.OrderBy(w => w.triggerTime).ToList();
            EditorUtility.SetDirty(_selectedBlueprint);
        }

        #endregion

        #region 3. Tab 2: Smart Wave Generator (Curve-Driven)

        private void DrawSmartGeneratorTab()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("⚡ BỘ TỰ ĐỘNG SINH TIMELINE QUÁI VẬT TỪ ĐƯỜNG CONG (SMART CURVE GENERATOR)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Công cụ này cho phép Game Designer tự động sinh toàn bộ 30–40 đợt sóng trận đấu dựa trên AnimationCurve nhịp độ trận đấu, loại bỏ hoàn toàn việc gõ tay thủ công.", MessageType.Info);

            EditorGUILayout.Space(8);

            _genTargetDuration = EditorGUILayout.Slider("Tổng Thời Lượng Trận (giây):", _genTargetDuration, 180f, 1200f);
            _genTargetWaveCount = EditorGUILayout.IntSlider("Số Đợt Sóng Dự Kiến:", _genTargetWaveCount, 10, 60);
            _genIncludeHordeMilestones = EditorGUILayout.Toggle("Tích Hợp Biến Cố Cao Trào (Horde Milestones):", _genIncludeHordeMilestones);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("── DANH MỤC QUÁI VẬT DÙNG TRONG MÀN ──", EditorStyles.boldLabel);
            _genDefaultEnemy = (DefenseEnemyDataSO)EditorGUILayout.ObjectField("Quái Cơ Bản (Bộ Binh / Ma Giáp):", _genDefaultEnemy, typeof(DefenseEnemyDataSO), false);
            _genLeadEnemy = (DefenseEnemyDataSO)EditorGUILayout.ObjectField("Quái Tinh Anh (Phi Cương / Thủ Lĩnh):", _genLeadEnemy, typeof(DefenseEnemyDataSO), false);
            _genWaterEnemy = (DefenseEnemyDataSO)EditorGUILayout.ObjectField("Thủy Quái (Đường Kênh Nước):", _genWaterEnemy, typeof(DefenseEnemyDataSO), false);
            _genFlyEnemy = (DefenseEnemyDataSO)EditorGUILayout.ObjectField("Không Kích (Huyết Dơi / Rừng Trúc):", _genFlyEnemy, typeof(DefenseEnemyDataSO), false);

            EditorGUILayout.Space(12);

            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.4f);
            if (GUILayout.Button("⚡ TỰ ĐỘNG SINH TOÀN BỘ TIMELINE QUÁI (GENERATE)", GUILayout.Height(40)))
            {
                if (EditorUtility.DisplayDialog("Xác nhận sinh Timeline", 
                    "Thao tác này sẽ tạo mới toàn bộ danh sách Waves dựa trên đường cong nhịp độ hiện tại. Bạn có chắc chắn muốn tiếp tục?", "Tiến Hành", "Hủy"))
                {
                    GenerateWavesFromCurve();
                }
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndVertical();
        }

        private void GenerateWavesFromCurve()
        {
            Undo.RecordObject(_selectedBlueprint, "Generate Waves From Curve");

            var waves = new List<DefenseStageWaveEntry>();
            float interval = _genTargetDuration / _genTargetWaveCount;
            float currentTime = 15f; // Bắt đầu ở giây 15 cho người chơi chuẩn bị

            for (int i = 0; i < _genTargetWaveCount && currentTime < _genTargetDuration - 20f; i++)
            {
                float progress = currentTime / _genTargetDuration;
                float rate = _selectedCurveProfile != null ? _selectedCurveProfile.EvaluateSpawnRate(currentTime) : (10f + progress * 50f);
                var milestone = _selectedCurveProfile != null ? _selectedCurveProfile.GetActiveMilestone(currentTime) : null;

                // Xác định số lượng quái của wave
                int mobCount = Mathf.Clamp(Mathf.RoundToInt((rate / 60f) * interval * 1.5f), 2, 16);
                if (milestone != null && milestone.pacingType == StagePacingType.HordeRush)
                {
                    mobCount = Mathf.RoundToInt(mobCount * milestone.intensityMultiplier);
                }

                // Luân phiên các Zone xuất hiện
                DefenseSpawnZoneType zone = (i % 5) switch
                {
                    1 => DefenseSpawnZoneType.CanalWater,
                    2 => DefenseSpawnZoneType.NorthCanopy,
                    3 => DefenseSpawnZoneType.SouthBank,
                    _ => DefenseSpawnZoneType.EastBank
                };

                // Lựa chọn quái tương ứng
                DefenseEnemyDataSO enemy = zone switch
                {
                    DefenseSpawnZoneType.CanalWater => _genWaterEnemy ?? _genDefaultEnemy,
                    DefenseSpawnZoneType.NorthCanopy => _genFlyEnemy ?? _genDefaultEnemy,
                    _ => _genDefaultEnemy
                };

                // Gán quái thủ lĩnh nếu là mốc cao trào
                DefenseEnemyDataSO lead = (milestone != null || (i % 6 == 0)) ? _genLeadEnemy : null;

                waves.Add(new DefenseStageWaveEntry
                {
                    eventId = $"wave-{i + 1:D2}",
                    triggerTime = Mathf.Round(currentTime),
                    zone = zone,
                    enemy = enemy,
                    count = mobCount,
                    leadEnemy = lead
                });

                // Nhịp thở: Sau Horde là Breather nghỉ 25s
                float step = (milestone != null && milestone.pacingType == StagePacingType.Breather) ? interval * 1.4f : interval;
                currentTime += step;
            }

            _selectedBlueprint.timelineWaves = waves;
            _selectedBlueprint.stageDurationSeconds = _genTargetDuration;
            EditorUtility.SetDirty(_selectedBlueprint);

            _lastValidationReport = DefenseStageValidator.Validate(_selectedBlueprint);
            _currentTab = StudioTab.Timeline;
        }

        #endregion

        #region 4. Tab 3: Combat & Economy Simulator

        private void DrawBalanceSimulatorTab()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("⚖️ BẢNG PHÂN TÍCH CÂN BẰNG TỨC THỜI (BALANCE & ECONOMY SIMULATOR)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Phân tích tổng lượng máu đàn quái, yêu cầu DPS tối thiểu của tháp và dự phóng lượng Cổ Mộc người chơi thu thập được theo từng phút.", MessageType.Info);

            EditorGUILayout.Space(6);

            _simulatorScroll = EditorGUILayout.BeginScrollView(_simulatorScroll);

            float stageDuration = Mathf.Max(60f, _selectedBlueprint.stageDurationSeconds);
            int totalMinutes = Mathf.CeilToInt(stageDuration / 60f);

            // Bảng Header
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("<b>Phút</b>", new GUIStyle(EditorStyles.boldLabel) { richText = true }, GUILayout.Width(60));
            EditorGUILayout.LabelField("<b>Số Đợt</b>", new GUIStyle(EditorStyles.boldLabel) { richText = true }, GUILayout.Width(70));
            EditorGUILayout.LabelField("<b>Quái Sinh Ra</b>", new GUIStyle(EditorStyles.boldLabel) { richText = true }, GUILayout.Width(100));
            EditorGUILayout.LabelField("<b>Tổng HP Quái</b>", new GUIStyle(EditorStyles.boldLabel) { richText = true }, GUILayout.Width(110));
            EditorGUILayout.LabelField("<b>DPS Yêu Cầu</b>", new GUIStyle(EditorStyles.boldLabel) { richText = true }, GUILayout.Width(120));
            EditorGUILayout.LabelField("<b>Cổ Mộc Dự Phóng</b>", new GUIStyle(EditorStyles.boldLabel) { richText = true }, GUILayout.Width(130));
            EditorGUILayout.LabelField("<b>Đánh Giá Áp Lực</b>", new GUIStyle(EditorStyles.boldLabel) { richText = true }, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            int cumulativeWood = 0;
            var waves = _selectedBlueprint.timelineWaves ?? new List<DefenseStageWaveEntry>();

            for (int m = 0; m < totalMinutes; m++)
            {
                float startSec = m * 60f;
                float endSec = (m + 1) * 60f;

                var minuteWaves = waves.Where(w => w != null && w.triggerTime >= startSec && w.triggerTime < endSec).ToList();
                int waveCount = minuteWaves.Count;
                int mobCount = minuteWaves.Sum(w => w.count);

                float minuteHp = minuteWaves.Sum(w => {
                    float hp = (w.enemy != null ? w.enemy.MaxHP : 100f);
                    float scale = _selectedCurveProfile != null ? _selectedCurveProfile.EvaluateHpScaling(w.triggerTime) : 1f;
                    return hp * scale * w.count;
                });

                float requiredDps = minuteHp / 60f;

                int minuteWood = minuteWaves.Sum(w => (w.enemy != null ? w.enemy.WoodDrop : 5) * w.count);
                cumulativeWood += minuteWood;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{m:D2}:00", GUILayout.Width(60));
                EditorGUILayout.LabelField($"{waveCount} đợt", GUILayout.Width(70));
                EditorGUILayout.LabelField($"{mobCount} con", GUILayout.Width(100));
                EditorGUILayout.LabelField($"{minuteHp:N0} HP", GUILayout.Width(110));

                // Đổi màu DPS
                GUIStyle dpsStyle = new GUIStyle(EditorStyles.label);
                if (requiredDps > 300f) dpsStyle.normal.textColor = Color.red;
                else if (requiredDps > 150f) dpsStyle.normal.textColor = new Color(1f, 0.6f, 0.1f);
                EditorGUILayout.LabelField($"{requiredDps:F1} dmg/s", dpsStyle, GUILayout.Width(120));

                EditorGUILayout.LabelField($"+{minuteWood} (Tổng {cumulativeWood})", GUILayout.Width(130));

                string pressureNote = mobCount > 24 ? "<color=#FF4444>⚠️ Rất Nặng (Swarm)</color>"
                    : (mobCount > 12 ? "<color=#FFAA00>⚡ Trung Bình</color>" : "<color=#00FF88>🌱 Dễ Chịu</color>");
                EditorGUILayout.LabelField(pressureNote, new GUIStyle(EditorStyles.label) { richText = true }, GUILayout.ExpandWidth(true));

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region 5. Tab 4: Spatial Zones

        private void DrawSpatialZonesTab()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("📍 QUẢN LÝ VÙNG XUẤT HIỆN & LÀN ĐƯỜNG TIẾN QUÂN (SPATIAL ZONES)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Cấu hình tọa độ cổng spawn và các Làn hành quân (Lane 0: Bắc/Trái, Lane 1: Giữa, Lane 2: Nam/Phải) trên chiến trường 3D.", MessageType.Info);

            EditorGUILayout.Space(6);

            if (_selectedBlueprint.spawnZones == null || _selectedBlueprint.spawnZones.Length == 0)
            {
                EditorGUILayout.HelpBox("Chưa có vùng xuất hiện nào. Bấm nút dưới để nạp 5 vùng chuẩn hóa.", MessageType.Warning);
                if (GUILayout.Button("✨ Khởi Tạo 5 Vùng Chuẩn (EastBank, Canal, North, South, Arena)"))
                {
                    DefenseStageBlueprintInstaller.CreateOrUpdateStage01Blueprint();
                }
                EditorGUILayout.EndVertical();
                return;
            }

            for (int i = 0; i < _selectedBlueprint.spawnZones.Length; i++)
            {
                var zone = _selectedBlueprint.spawnZones[i];
                if (zone == null) continue;

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"Vùng {i + 1}: <b>{zone.zoneType}</b> ({zone.zoneType.ToZoneId()})", new GUIStyle(EditorStyles.boldLabel) { richText = true });
                zone.relativeToEastMarker = EditorGUILayout.Toggle("Tính Theo Mốc Bờ Đông:", zone.relativeToEastMarker);
                zone.origin = EditorGUILayout.Vector3Field("Tọa Độ Tâm Điểm (Origin):", zone.origin);
                EditorGUILayout.LabelField($"Số Điểm Xuất Hiện Chi Tiết: {zone.points?.Length ?? 0}");
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(3);
            }

            EditorGUILayout.EndVertical();
        }

        #endregion
    }
}
