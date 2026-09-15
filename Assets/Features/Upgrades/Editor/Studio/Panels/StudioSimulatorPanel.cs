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
    /// Panel chuyên trách giả lập bốc thẻ Roguelite:
    /// 1. Chế độ Giả Lập Tương Tác Ván Chơi (Interactive Run Sandbox): Bốc ngẫu nhiên từng level hoặc chỉ định ép thẻ.
    /// 2. Chế độ Phân Tích Thống Kê Monte Carlo (500 - 5000 lượt).
    /// </summary>
    public class StudioSimulatorPanel
    {
        private int _simSubTab = 0; // 0 = Interactive Run Sandbox, 1 = Monte Carlo Analysis

        // --- Interactive Run State ---
        private int _currentRunLevel = 1;
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
            string[] subTabs = new[] { "🎮 Giả Lập Ván Chơi Tương Tác (Interactive Sandbox)", "📊 Phân Tích Thống Kê Monte Carlo (Bulk Roll)" };
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
            GUILayout.Label($"<b>CẤP ĐỘ HIỆN TẠI: <color=#00FF88>Lv.{_currentRunLevel}</color></b>", new GUIStyle(EditorStyles.boldLabel) { richText = true });
            
            string coreName = _activeRunArchetype != MythicArchetype.None ? _activeRunArchetype.GetDisplayName() : "<color=#FF4444>Chưa chọn</color>";
            GUILayout.Label($" | Đại Lõi: <b>{coreName}</b>", new GUIStyle(EditorStyles.label) { richText = true });

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("🔄 Reset Ván Chơi Mới (Lv.1)", EditorStyles.toolbarButton))
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

        private void DrawLevelUpRollArea(List<UpgradeData> allUpgrades)
        {
            // Toolbar Hành Động
            GUILayout.BeginHorizontal();

            if (_activeRunArchetype == MythicArchetype.None)
            {
                GUILayout.Label("<b>⭐ GIAI ĐOẠN 1: CHỌN ĐẠI LÕI KHỞI ĐẦU (LEVEL 1)</b>", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("🎲 Roll 3 Đại Lõi Ngẫu Nhiên", EditorStyles.miniButton, GUILayout.Height(24)))
                {
                    RollLevel1Cores(allUpgrades);
                }
            }
            else
            {
                GUILayout.Label($"<b>⚡ GIAI ĐOẠN 2: LÊN CẤP VÀ PHÁT TRIỂN NHÁNH (Lv.{_currentRunLevel})</b>", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("🎲 Lên Cấp Ngẫu Nhiên (+1 Level)", EditorStyles.miniButton, GUILayout.Height(24)))
                {
                    RollNextLevelRandom(allUpgrades);
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

            if (_activeRunArchetype != MythicArchetype.None)
            {
                if (GUILayout.Button("⚡ Ép Đủ 5 Thần Binh Thuật", GUILayout.Width(165)))
                {
                    InjectAllArchetypeTraits(allUpgrades);
                }
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
                    : "Bấm '🎲 Lên Cấp Ngẫu Nhiên (+1 Level)' hoặc sử dụng công cụ '🎯 Chỉ Định Ép Thẻ' để thử nghiệm build.", MessageType.Info);

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
                            SelectCoreDirectly(core);
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
                        GUILayout.Label(new GUIContent(card.icon.texture), GUILayout.Width(36), GUILayout.Height(36));
                    }
                    else
                    {
                        GUILayout.Box("?", GUILayout.Width(36), GUILayout.Height(36));
                    }

                    GUILayout.BeginVertical();
                    GUILayout.Label($"<b>{card.id}</b> - {card.upgradeName}", EditorStyles.boldLabel);
                    GUILayout.Label($"[{card.upgradeType}] | Hệ: {card.element} | W: {card.spawnWeight}", EditorStyles.miniLabel);
                    GUILayout.EndVertical();

                    GUILayout.FlexibleSpace();

                    // Nút CHỌN
                    var prevBg = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f, 1f);
                    if (GUILayout.Button("✔ CHỌN THẺ NÀY", GUILayout.Width(130), GUILayout.Height(36)))
                    {
                        GUI.backgroundColor = prevBg;
                        ApplyCardSelection(card, allUpgrades);
                        break;
                    }
                    GUI.backgroundColor = prevBg;

                    GUILayout.EndHorizontal();

                    // Mô tả
                    GUILayout.Space(2);
                    GUILayout.Label(card.description, new GUIStyle(EditorStyles.wordWrappedMiniLabel) { richText = true });

                    GUILayout.EndVertical();
                    GUILayout.Space(4);
                }
                GUILayout.EndScrollView();
            }
        }

        private void DrawRunInventoryAndStatsArea(List<UpgradeData> allUpgrades)
        {
            GUILayout.Label("<b>🎒 KHO ĐỒ & BUILD HIỆN TẠI</b>", EditorStyles.boldLabel);
            GUILayout.Space(4);

            _inventoryScroll = GUILayout.BeginScrollView(_inventoryScroll);

            // 1. Đại Lõi Đang Mang
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("<b>👑 ĐẠI LÕI ĐANG MANG:</b>", EditorStyles.miniBoldLabel);
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

            // 2. Danh sách Thẻ đã nhặt
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label($"<b>📜 CÁC NÂNG CẤP ĐÃ CHỌN ({_runInventory.Count}):</b>", EditorStyles.miniBoldLabel);
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

            // 3. Tổng Hợp Chỉ Số Modifier
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
                if (u is SynergyTraitUpgradeData trait)
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

        #region Interactive Run Actions
        private void ResetRun(List<UpgradeData> allUpgrades)
        {
            _currentRunLevel = 1;
            _activeRunArchetype = MythicArchetype.None;
            _activeCoreData = null;
            _runInventory.Clear();
            _currentRolledChoices.Clear();
            RollLevel1Cores(allUpgrades);
        }

        private void RollLevel1Cores(List<UpgradeData> allUpgrades)
        {
            _currentRolledChoices.Clear();
            var cores = allUpgrades.OfType<MythicCoreUpgradeData>().ToList();
            if (cores.Count == 0) return;

            // Lấy 3 core ngẫu nhiên
            var shuffled = cores.OrderBy(_ => UnityEngine.Random.value).Take(3).ToList();
            _currentRolledChoices.AddRange(shuffled);
        }

        private void SelectCoreDirectly(MythicCoreUpgradeData core)
        {
            _activeCoreData = core;
            _activeRunArchetype = core.archetype;
            _currentRunLevel = 2;
            _currentRolledChoices.Clear();
        }

        private void ApplyCardSelection(UpgradeData card, List<UpgradeData> allUpgrades)
        {
            if (card is MythicCoreUpgradeData core)
            {
                SelectCoreDirectly(core);
                RollNextLevelRandom(allUpgrades);
            }
            else
            {
                _runInventory.Add(card);
                _currentRunLevel++;
                RollNextLevelRandom(allUpgrades);
            }
        }

        private void RollNextLevelRandom(List<UpgradeData> allUpgrades)
        {
            _currentRolledChoices.Clear();

            // Lọc các thẻ hợp lệ
            var candidatePool = allUpgrades.Where(u =>
            {
                if (u == null) return false;
                if (u is MythicCoreUpgradeData) return false; // Không ra lại Core

                // Nếu là SynergyTrait, chỉ cho phép đúng Archetype
                if (u is SynergyTraitUpgradeData trait && trait.requiredArchetype != _activeRunArchetype)
                {
                    return false;
                }

                // Không ra thẻ đã max level (giả định max 1 cho demo)
                if (_runInventory.Contains(u)) return false;

                return true;
            }).ToList();

            if (candidatePool.Count == 0) return;

            // Tính Effective Weights với x3 cho Synergy Trait
            var weights = candidatePool.Select(u =>
            {
                float w = Mathf.Max(1f, u.spawnWeight);
                if (u is SynergyTraitUpgradeData trait && trait.requiredArchetype == _activeRunArchetype)
                {
                    w *= 3.0f; // x3.0 Archetype Synergy
                }
                return w;
            }).ToList();

            float totalWeight = weights.Sum();
            int choicesToPick = Mathf.Min(3, candidatePool.Count);

            // Guaranteed Synergy Slot
            var synergyTraits = candidatePool.OfType<SynergyTraitUpgradeData>().Where(t => t.requiredArchetype == _activeRunArchetype).ToList();
            if (synergyTraits.Count > 0)
            {
                var chosenSynergy = synergyTraits[UnityEngine.Random.Range(0, synergyTraits.Count)];
                _currentRolledChoices.Add(chosenSynergy);
                int idx = candidatePool.IndexOf(chosenSynergy);
                if (idx >= 0)
                {
                    totalWeight -= weights[idx];
                    candidatePool.RemoveAt(idx);
                    weights.RemoveAt(idx);
                }
            }

            // Weighted Random cho các slot còn lại
            while (_currentRolledChoices.Count < choicesToPick && candidatePool.Count > 0 && totalWeight > 0f)
            {
                float rnd = UnityEngine.Random.Range(0f, totalWeight);
                float cur = 0f;
                for (int i = 0; i < candidatePool.Count; i++)
                {
                    cur += weights[i];
                    if (cur >= rnd || i == candidatePool.Count - 1)
                    {
                        var chosen = candidatePool[i];
                        _currentRolledChoices.Add(chosen);
                        totalWeight -= weights[i];
                        candidatePool.RemoveAt(i);
                        weights.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        private void InjectCardDirectly(UpgradeData card)
        {
            if (card == null) return;
            if (card is MythicCoreUpgradeData core)
            {
                SelectCoreDirectly(core);
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

        private void InjectAllArchetypeTraits(List<UpgradeData> allUpgrades)
        {
            if (_activeRunArchetype == MythicArchetype.None) return;

            var traits = allUpgrades.OfType<SynergyTraitUpgradeData>().Where(t => t.requiredArchetype == _activeRunArchetype).ToList();
            foreach (var t in traits)
            {
                if (!_runInventory.Contains(t))
                {
                    _runInventory.Add(t);
                    _currentRunLevel++;
                }
            }
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
