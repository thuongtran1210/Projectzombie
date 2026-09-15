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
    /// Panel chuyên trách giả lập bốc thẻ Roguelite theo Tiến trình Logarithmic 4 giai đoạn:
    /// 1. Chế độ Giả Lập Tương Tác Ván Chơi (Interactive Run Sandbox): Mốc Lv.1 Đại Lõi, Mốc Lv.5/15/30 Lõi Đột Biến (+2 Reroll Tokens), Level Thường (Micro-Stats Clean Pool).
    /// 2. Chế độ Phân Tích Thống Kê Monte Carlo (500 - 5000 lượt).
    /// </summary>
    public class StudioSimulatorPanel
    {
        private int _simSubTab = 0; // 0 = Interactive Run Sandbox, 1 = Monte Carlo Analysis

        // --- Interactive Run State ---
        private int _currentRunLevel = 1;
        private int _rerollTokens = 2;
        private MythicArchetype _activeRunArchetype = MythicArchetype.None;
        private MythicCoreUpgradeData _activeCoreData;
        private readonly List<UpgradeData> _runInventory = new List<UpgradeData>();
        private List<UpgradeData> _currentRolledChoices = new List<UpgradeData>();
        private UpgradeData _targetedCardToInject;
        private Vector2 _interactiveScroll;
        private Vector2 _inventoryScroll;

        // --- Monte Carlo State ---
        private MythicArchetype _simArchetype = MythicArchetype.PhuDongThienUy;
        private int _simRollCount = 500;
        private SimulationResult? _lastSimResult;
        private Vector2 _monteCarloScroll;

        public void Draw(List<UpgradeData> allUpgrades)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            // 1. Sub-Tab Switcher
            GUILayout.BeginHorizontal();
            string[] subTabs = new[] { "🎮 Giả Lập Tiến Trình Ván Đấu (Interactive Progression)", "📊 Phân Tích Thống Kê Monte Carlo (Bulk Roll)" };
            _simSubTab = GUILayout.Toolbar(_simSubTab, subTabs, EditorStyles.toolbarButton, GUILayout.Height(26));
            GUILayout.EndHorizontal();

            GUILayout.Space(8);

            if (_simSubTab == 0)
            {
                DrawInteractiveRunSandbox(allUpgrades);
            }
            else
            {
                DrawMonteCarloAnalysis(allUpgrades);
            }

            GUILayout.EndVertical();
        }

        #region Mode 1: Interactive Run Sandbox
        private void DrawInteractiveRunSandbox(List<UpgradeData> allUpgrades)
        {
            // 1. Run Header & Global Controls
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            string levelPhaseInfo = GetProgressionPhaseTitle(_currentRunLevel);
            GUILayout.Label($"<b>CẤP ĐỘ HIỆN TẠI: <color=#00FF88>Lv.{_currentRunLevel}</color></b> - <i>{levelPhaseInfo}</i>", new GUIStyle(EditorStyles.boldLabel) { richText = true });

            string coreName = _activeRunArchetype != MythicArchetype.None ? _activeRunArchetype.GetDisplayName() : "<color=#FF4444>Chưa chọn</color>";
            GUILayout.Label($" | Đại Lõi: <b>{coreName}</b>", new GUIStyle(EditorStyles.label) { richText = true });
            GUILayout.Label($" | Reroll Token: <color=#FFD700><b>{_rerollTokens}/2</b></color>", new GUIStyle(EditorStyles.label) { richText = true });

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("🔄 Bắt Đầu Lại (Lv.1)", EditorStyles.toolbarButton))
            {
                ResetRun(allUpgrades);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            GUILayout.BeginHorizontal();

            // CỘT TRÁI (60% Width): MÀN HÌNH CHỌN THẺ / BỐC THẺ / CHỈ ĐỊNH
            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.58f));
            DrawLevelUpRollArea(allUpgrades);
            GUILayout.EndVertical();

            GUILayout.Space(6);

            // CỘT PHẢI (40% Width): KHO ĐỒ BUILD & TỔNG HỢP CHỈ SỐ
            GUILayout.BeginVertical(EditorStyles.helpBox);
            DrawRunInventoryAndStatsArea(allUpgrades);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private string GetProgressionPhaseTitle(int level)
        {
            if (level == 1) return "<color=#FFD700>Giai Đoạn 1: Đại Lõi Khởi Nguyên</color>";
            if (level == 5) return "<color=#FFD700>⭐ MỐC ĐỘT BIẾN 1 (Lõi Bạc / Vàng)</color>";
            if (level == 15) return "<color=#00E5FF>⭐⭐ MỐC ĐỘT BIẾN 2 (Lõi Vàng / Kim Cương)</color>";
            if (level == 30) return "<color=#FF3300>⭐⭐⭐ MỐC ĐỘT BIẾN 3 (Lõi Kim Cương Tối Thượng)</color>";
            if (level > 30) return "<color=#FF4444>👑 Phút 12+: Quyết Chiến Boss Sàn Đấu</color>";
            return "<color=#00FF88>Giai Đoạn 2: Bồi Đắp Chỉ Số Nền Tảng (Clean Pool)</color>";
        }

        private bool IsMutationMilestone(int level) => level == 5 || level == 15 || level == 30;

        private void DrawLevelUpRollArea(List<UpgradeData> allUpgrades)
        {
            bool isMutation = IsMutationMilestone(_currentRunLevel);

            // Toolbar Hành Động
            GUILayout.BeginHorizontal();

            if (_activeRunArchetype == MythicArchetype.None)
            {
                GUILayout.Label("<b>⭐ GIAI ĐOẠN 1: CHỌN ĐẠI LÕI KHỞI NGUYÊN (LEVEL 1)</b>", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("🎲 Roll 3 Đại Lõi Ngẫu Nhiên", EditorStyles.miniButton, GUILayout.Height(24)))
                {
                    RollLevel1Cores(allUpgrades);
                }
            }
            else if (isMutation)
            {
                GUILayout.Label($"<b>⚡ MỐC ĐỘT BIẾN LÕI QUY TẮC (Lv.{_currentRunLevel})</b>", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (_rerollTokens > 0)
                {
                    if (GUILayout.Button($"🎲 Dùng 1 Reroll Token ({_rerollTokens}/2)", EditorStyles.miniButtonLeft, GUILayout.Height(24)))
                    {
                        _rerollTokens--;
                        RollProgressionLevel(allUpgrades);
                    }
                }
                if (GUILayout.Button("Lên Cấp Ngẫu Nhiên", EditorStyles.miniButtonRight, GUILayout.Height(24)))
                {
                    RollProgressionLevel(allUpgrades);
                }
            }
            else
            {
                GUILayout.Label($"<b>⚙️ LEVEL THƯỜNG: BỒI ĐẮP CHỈ SỐ (Lv.{_currentRunLevel})</b>", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("🎲 +1 Level Nhanh (<1s)", EditorStyles.miniButton, GUILayout.Height(24)))
                {
                    RollProgressionLevel(allUpgrades);
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            // Thanh Công Cụ: CHỈ ĐỊNH ÉP THẺ (Targeted Card Injection)
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("<b>🎯 CƠ CHẾ CHỈ ĐỊNH ÉP THẺ TRỰC TIẾP (TARGETED INJECTION):</b>", EditorStyles.miniBoldLabel);
            GUILayout.BeginHorizontal();

            _targetedCardToInject = (UpgradeData)EditorGUILayout.ObjectField(_targetedCardToInject, typeof(UpgradeData), false);

            if (GUILayout.Button("Ép Thẻ Này Vào Build", GUILayout.Width(140)))
            {
                if (_targetedCardToInject != null)
                {
                    InjectCardDirectly(_targetedCardToInject);
                }
            }

            if (GUILayout.Button("🚀 Fast Sim to Lv.30", GUILayout.Width(130)))
            {
                FastSimToLevel30(allUpgrades);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(8);

            // HIỂN THỊ CÁC THẺ ĐƯỢC CHÀO MỜI (3 Card Selection Area)
            GUILayout.Label($"<b>DANH SÁCH THẺ ĐƯỢC CHÀO MỜI ({_currentRolledChoices.Count} THẺ):</b>", EditorStyles.boldLabel);

            if (_currentRolledChoices.Count == 0)
            {
                EditorGUILayout.HelpBox(_activeRunArchetype == MythicArchetype.None
                    ? "Bấm '🎲 Roll 3 Đại Lõi Ngẫu Nhiên' hoặc chọn Đại Lõi trực tiếp bên dưới để bắt đầu ván đấu."
                    : "Bấm '🎲 +1 Level Nhanh' để tiếp tục tiến trình.", MessageType.Info);

                if (_activeRunArchetype == MythicArchetype.None)
                {
                    GUILayout.Space(6);
                    GUILayout.Label("Hoặc bấm chọn trực tiếp 1 trong 5 Đại Lõi dưới đây:", EditorStyles.miniBoldLabel);
                    var allCores = allUpgrades.OfType<MythicCoreUpgradeData>().ToList();
                    foreach (var core in allCores)
                    {
                        GUILayout.BeginHorizontal(EditorStyles.helpBox);
                        GUILayout.Label($"👑 <b>{core.upgradeName}</b> ({core.archetype})", EditorStyles.label);
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Chọn Lõi Này", GUILayout.Width(110)))
                        {
                            SelectCoreDirectly(core, allUpgrades);
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }
            else
            {
                _interactiveScroll = GUILayout.BeginScrollView(_interactiveScroll);
                for (int i = 0; i < _currentRolledChoices.Count; i++)
                {
                    var card = _currentRolledChoices[i];
                    if (card == null) continue;

                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    GUILayout.BeginHorizontal();
                    // Icon
                    if (card.icon != null && card.icon.texture != null)
                    {
                        GUILayout.Label(new GUIContent(card.icon.texture), GUILayout.Width(32), GUILayout.Height(32));
                    }
                    else
                    {
                        GUILayout.Box("?", GUILayout.Width(32), GUILayout.Height(32));
                    }

                    GUILayout.BeginVertical();
                    GUILayout.Label($"<b>{card.id}</b> - {card.upgradeName}", EditorStyles.boldLabel);
                    GUILayout.Label(card.GetCategoryDisplayName(), new GUIStyle(EditorStyles.miniLabel) { richText = true });
                    GUILayout.EndVertical();

                    GUILayout.FlexibleSpace();

                    // Nút CHỌN
                    var prevBg = GUI.backgroundColor;
                    GUI.backgroundColor = isMutation ? new Color(1f, 0.84f, 0f, 1f) : new Color(0.2f, 0.8f, 0.2f, 1f);
                    if (GUILayout.Button("✔ CHỌN THẺ NÀY", GUILayout.Width(130), GUILayout.Height(32)))
                    {
                        GUI.backgroundColor = prevBg;
                        ApplyCardSelection(card, allUpgrades);
                        break;
                    }
                    GUI.backgroundColor = prevBg;

                    GUILayout.EndHorizontal();

                    // Mô tả ngắn gọn (Micro 1 dòng nếu có)
                    GUILayout.Space(2);
                    string desc = card.description;
                    if (card is StatMicroUpgradeData micro && !string.IsNullOrEmpty(micro.oneLineSummary))
                    {
                        desc = $"<color=#00FF88><b>⚡ {micro.oneLineSummary}</b></color>";
                    }
                    GUILayout.Label(desc, new GUIStyle(EditorStyles.wordWrappedMiniLabel) { richText = true });

                    GUILayout.EndVertical();
                    GUILayout.Space(4);
                }
                GUILayout.EndScrollView();
            }
        }

        private void DrawRunInventoryAndStatsArea(List<UpgradeData> allUpgrades)
        {
            GUILayout.Label("<b>🎒 KHO ĐỒ & TIẾN TRÌNH BUILD</b>", EditorStyles.boldLabel);
            GUILayout.Space(4);

            _inventoryScroll = GUILayout.BeginScrollView(_inventoryScroll);

            // 1. Đại Lõi Đang Mang
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("<b>👑 ĐẠI LÕI KHỞI NGUYÊN:</b>", EditorStyles.miniBoldLabel);
            if (_activeCoreData != null)
            {
                GUILayout.Label($"<color=#FFD700><b>{_activeCoreData.mythicTitle}</b></color> ({_activeCoreData.id})", new GUIStyle(EditorStyles.label) { richText = true });
            }
            else
            {
                GUILayout.Label("<color=#888888>Chưa chọn Đại Lõi</color>", new GUIStyle(EditorStyles.label) { richText = true });
            }
            GUILayout.EndVertical();

            GUILayout.Space(4);

            // 2. Danh sách Lõi Đột Biến
            var mutationCards = _runInventory.OfType<MutationAugmentUpgradeData>().ToList();
            if (mutationCards.Count > 0)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label($"<b>⚡ LÕI ĐỘT BIẾN ĐÃ CHỌN ({mutationCards.Count}/3):</b>", EditorStyles.miniBoldLabel);
                foreach (var mut in mutationCards)
                {
                    GUILayout.Label($"• [{mut.tier}] <b>{mut.upgradeName}</b>", EditorStyles.miniLabel);
                }
                GUILayout.EndVertical();
                GUILayout.Space(4);
            }

            // 3. Thẻ Chỉ Số Nền Tảng
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label($"<b>📜 CÁC THẺ ĐÃ NHẶT ({_runInventory.Count}):</b>", EditorStyles.miniBoldLabel);
            if (_runInventory.Count == 0)
            {
                GUILayout.Label("<color=#888888>Chưa nhặt thẻ nào</color>", new GUIStyle(EditorStyles.label) { richText = true });
            }
            else
            {
                for (int i = 0; i < _runInventory.Count; i++)
                {
                    var u = _runInventory[i];
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"• <b>{u.id}</b>: {u.upgradeName}", EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
                    {
                        _runInventory.RemoveAt(i);
                        break;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();

            GUILayout.Space(6);

            // 4. Tổng Hợp Chỉ Số Modifier
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("<b>⚡ TỔNG HỢP CHỈ SỐ BUILD CỘNG DỒN:</b>", EditorStyles.miniBoldLabel);
            DrawAccumulatedStats();
            GUILayout.EndVertical();

            GUILayout.EndScrollView();
        }

        private void DrawAccumulatedStats()
        {
            float dmg = 0f, crit = 0f, hp = 0f, spd = 0f, atkSpd = 0f, fireDmg = 0f, aoe = 0f;

            foreach (var u in _runInventory)
            {
                if (u is StatMicroUpgradeData micro)
                {
                    dmg += micro.statModifier.baseDamageBonus;
                    crit += micro.statModifier.critChanceBonus;
                    hp += micro.statModifier.maxHealthBonus;
                    spd += micro.statModifier.moveSpeedBonus;
                    atkSpd += micro.statModifier.attackSpeedBonus;
                    fireDmg += micro.statModifier.fireDamageBonus;
                    aoe += micro.statModifier.areaScaleBonus;
                }
                else if (u is MutationAugmentUpgradeData mut)
                {
                    dmg += mut.statModifier.baseDamageBonus;
                    crit += mut.statModifier.critChanceBonus;
                    hp += mut.statModifier.maxHealthBonus;
                    spd += mut.statModifier.moveSpeedBonus;
                    atkSpd += mut.statModifier.attackSpeedBonus;
                    fireDmg += mut.statModifier.fireDamageBonus;
                    aoe += mut.statModifier.areaScaleBonus;
                }
                else if (u is SynergyTraitUpgradeData trait)
                {
                    dmg += trait.playerStatModifier.baseDamageBonus;
                    crit += trait.playerStatModifier.critChanceBonus;
                    hp += trait.playerStatModifier.maxHealthBonus;
                    spd += trait.playerStatModifier.moveSpeedBonus;
                    atkSpd += trait.playerStatModifier.attackSpeedBonus;
                    fireDmg += trait.playerStatModifier.fireDamageBonus;
                    aoe += trait.playerStatModifier.areaScaleBonus;
                }
                else if (u is CommonUpgradeData common)
                {
                    dmg += common.playerStatModifier.baseDamageBonus;
                    crit += common.playerStatModifier.critChanceBonus;
                    hp += common.playerStatModifier.maxHealthBonus;
                    spd += common.playerStatModifier.moveSpeedBonus;
                    atkSpd += common.playerStatModifier.attackSpeedBonus;
                    fireDmg += common.playerStatModifier.fireDamageBonus;
                    aoe += common.playerStatModifier.areaScaleBonus;
                }
            }

            GUILayout.Label($"- Sát thương cơ bản (+): <color=#00FF88>+{dmg:F1}</color>", new GUIStyle(EditorStyles.miniLabel) { richText = true });
            GUILayout.Label($"- Tỉ lệ chí mạng (+): <color=#00FF88>+{crit * 100f:F1}%</color>", new GUIStyle(EditorStyles.miniLabel) { richText = true });
            GUILayout.Label($"- Máu tối đa (+): <color=#00FF88>+{hp:F0}</color>", new GUIStyle(EditorStyles.miniLabel) { richText = true });
            GUILayout.Label($"- Tốc độ di chuyển (+): <color=#00FF88>+{spd:F1}</color>", new GUIStyle(EditorStyles.miniLabel) { richText = true });
            GUILayout.Label($"- Tốc độ đánh (+): <color=#00FF88>+{atkSpd:F1}</color>", new GUIStyle(EditorStyles.miniLabel) { richText = true });
            GUILayout.Label($"- Sát thương Hỏa (%): <color=#FF5533>+{fireDmg:F1}%</color>", new GUIStyle(EditorStyles.miniLabel) { richText = true });
            GUILayout.Label($"- Phạm vi AoE (%): <color=#00E5FF>+{aoe:F1}%</color>", new GUIStyle(EditorStyles.miniLabel) { richText = true });
        }

        #region Progression Interactive Actions
        private void ResetRun(List<UpgradeData> allUpgrades)
        {
            _currentRunLevel = 1;
            _rerollTokens = 2;
            _activeRunArchetype = MythicArchetype.None;
            _activeCoreData = null;
            _runInventory.Clear();
            _currentRolledChoices.Clear();
            RollLevel1Cores(allUpgrades);
        }

        private void RollLevel1Cores(List<UpgradeData> allUpgrades)
        {
            _currentRolledChoices.Clear();
            _currentRolledChoices = UpgradeSelector.SelectArchetypeCores(3, allUpgrades);
        }

        private void SelectCoreDirectly(MythicCoreUpgradeData core, List<UpgradeData> allUpgrades)
        {
            _activeCoreData = core;
            _activeRunArchetype = core.archetype;
            _currentRunLevel = 2;
            RollProgressionLevel(allUpgrades);
        }

        private void ApplyCardSelection(UpgradeData card, List<UpgradeData> allUpgrades)
        {
            if (card is MythicCoreUpgradeData core)
            {
                SelectCoreDirectly(core, allUpgrades);
            }
            else
            {
                _runInventory.Add(card);
                _currentRunLevel++;
                RollProgressionLevel(allUpgrades);
            }
        }

        private void RollProgressionLevel(List<UpgradeData> allUpgrades)
        {
            _currentRolledChoices.Clear();

            if (_currentRunLevel == 1 || _activeRunArchetype == MythicArchetype.None)
            {
                RollLevel1Cores(allUpgrades);
                return;
            }

            if (IsMutationMilestone(_currentRunLevel))
            {
                // Mốc Đột Biến: Bốc Lõi Bạc / Vàng / Kim Cương
                _currentRolledChoices = UpgradeSelector.SelectMutationAugments(3, _currentRunLevel, null, allUpgrades);
            }
            else
            {
                // Level thường: Bốc Thẻ Chỉ Số Nền Tảng (Clean Pool)
                _currentRolledChoices = UpgradeSelector.SelectMicroStats(3, null, allUpgrades, null);
            }
        }

        private void InjectCardDirectly(UpgradeData card)
        {
            if (card == null) return;
            if (card is MythicCoreUpgradeData core)
            {
                _activeCoreData = core;
                _activeRunArchetype = core.archetype;
            }
            else
            {
                if (!_runInventory.Contains(card))
                {
                    _runInventory.Add(card);
                    _currentRunLevel++;
                }
            }
        }

        private void FastSimToLevel30(List<UpgradeData> allUpgrades)
        {
            while (_currentRunLevel < 30)
            {
                RollProgressionLevel(allUpgrades);
                if (_currentRolledChoices.Count > 0)
                {
                    var picked = _currentRolledChoices[0];
                    _runInventory.Add(picked);
                    _currentRunLevel++;
                }
                else
                {
                    _currentRunLevel++;
                }
            }
            RollProgressionLevel(allUpgrades);
        }
        #endregion
        #endregion

        #region Mode 2: Monte Carlo Analysis
        private void DrawMonteCarloAnalysis(List<UpgradeData> allUpgrades)
        {
            GUILayout.Label("<b>📊 PHÂN TÍCH TOÁN HỌC MONTE CARLO (BULK SIMULATION)</b>", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Công cụ giả lập hàng nghìn lượt Level Up để Game Designer đánh giá phân bổ xác suất, tỉ lệ xuất hiện của thẻ và độ cân bằng của hệ thống Thần Binh Thuật.", MessageType.Info);

            GUILayout.Space(6);

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

                _monteCarloScroll = GUILayout.BeginScrollView(_monteCarloScroll);

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
        }
        #endregion
    }
}
#endif
