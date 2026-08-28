using UnityEngine;
using System.Collections.Generic;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Presenter điều phối toàn bộ logic Miếu Tứ Bất Tử (Meta Upgrade Tree) theo kiến trúc MVP.
    /// </summary>
    public class MetaUpgradeShopPresenter : MonoBehaviour
    {
        [SerializeField] private MetaUpgradeShopView _view;
        [SerializeField] private PermanentUpgradeTreeData _treeData;

        [Header("Tab Sprites")]
        [SerializeField] private Sprite _tabActiveSprite;
        [SerializeField] private Sprite _tabInactiveSprite;

        private SanctuaryBranch _currentBranch = SanctuaryBranch.TanVienSonThanh;
        private int _selectedCardIndex = 0;

        private void Awake()
        {
            if (_view == null) _view = GetComponent<MetaUpgradeShopView>();

            if (_treeData == null)
            {
                _treeData = Resources.Load<PermanentUpgradeTreeData>("PermanentUpgradeTree");
#if UNITY_EDITOR
                if (_treeData == null)
                {
                    _treeData = UnityEditor.AssetDatabase.LoadAssetAtPath<PermanentUpgradeTreeData>("Assets/_Data/Meta/PermanentUpgradeTree.asset");
                }
#endif
            }

#if UNITY_EDITOR
            if (_tabActiveSprite == null)
                _tabActiveSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Tab_Wood_Active.png");
            if (_tabInactiveSprite == null)
                _tabInactiveSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Tab_Wood_Inactive.png");
#endif
        }

        private void Start()
        {
            if (_view != null)
            {
                _view.OnTabSelected += HandleTabSelected;
                _view.OnNodeCardSelected += HandleNodeCardSelected;
                _view.OnBuyUpgradeClicked += HandleBuyUpgrade;
            }

            if (MetaCurrencyManager.Instance != null)
            {
                MetaCurrencyManager.Instance.OnCurrencyChanged += HandleCurrencyChanged;
            }

            RenderShop();
        }

        private void OnEnable()
        {
            RenderShop();
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.OnTabSelected -= HandleTabSelected;
                _view.OnNodeCardSelected -= HandleNodeCardSelected;
                _view.OnBuyUpgradeClicked -= HandleBuyUpgrade;
            }

            if (MetaCurrencyManager.Instance != null)
            {
                MetaCurrencyManager.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
            }
        }

        private void HandleTabSelected(SanctuaryBranch branch)
        {
            if (_currentBranch != branch)
            {
                _currentBranch = branch;
                _selectedCardIndex = 0;
                RenderShop();
            }
        }

        private void HandleNodeCardSelected(int cardIndex)
        {
            _selectedCardIndex = cardIndex;
            RenderShop();
        }

        private void HandleCurrencyChanged(int newBalance)
        {
            RenderShop();
        }

        private List<PermanentUpgradeNode> GetNodesForCurrentBranch()
        {
            var list = new List<PermanentUpgradeNode>();
            if (_treeData == null || _treeData.nodes == null) return list;

            foreach (var node in _treeData.nodes)
            {
                if (node != null && node.branch == _currentBranch)
                {
                    list.Add(node);
                }
            }
            return list;
        }

        private void HandleBuyUpgrade()
        {
            var branchNodes = GetNodesForCurrentBranch();
            if (_selectedCardIndex < 0 || _selectedCardIndex >= branchNodes.Count) return;

            var node = branchNodes[_selectedCardIndex];
            int nodeIndex = _treeData.GetNodeIndex(node.nodeId);
            if (nodeIndex < 0) return;

            var saveData = GetSaveData();
            if (saveData == null) return;

            int currentLevel = saveData.GetUpgradeLevel(nodeIndex);
            if (currentLevel >= node.maxLevel) return;

            int cost = node.GetCostForLevel(currentLevel);
            if (cost <= 0) return;

            if (MetaCurrencyManager.Instance != null)
            {
                if (MetaCurrencyManager.Instance.SpendCurrency(cost))
                {
                    saveData.SetUpgradeLevel(nodeIndex, currentLevel + 1);
                    Core.Save.SaveSystem.Save(saveData);
                    Debug.Log($"<color=#00FF88>[MetaUpgradeShop]</color> Đã nâng cấp '{node.displayName}' lên Cấp {currentLevel + 1} (-{cost} Cổ Tiền)!");
                    RenderShop();
                }
            }
        }

        private MetaProgressionSaveData GetSaveData()
        {
            if (MetaCurrencyManager.Instance != null && MetaCurrencyManager.Instance.GetSaveData() != null)
            {
                return MetaCurrencyManager.Instance.GetSaveData();
            }
            return Core.Save.SaveSystem.Load();
        }

        public void RenderShop()
        {
            if (_view == null) return;

            // 1. Render Balance
            int currentBalance = MetaCurrencyManager.Instance != null ? MetaCurrencyManager.Instance.TotalCurrency : 0;
            _view.SetCoTienBalance($"<color=#FFD700>{currentBalance:N0}</color> Cổ Tiền");

            // 2. Render Tabs
            _view.UpdateTabVisuals(_currentBranch, _tabActiveSprite, _tabInactiveSprite);

            // 3. Render Branch Nodes Cards
            var branchNodes = GetNodesForCurrentBranch();
            var saveData = GetSaveData();

            for (int i = 0; i < 3; i++)
            {
                if (i < branchNodes.Count)
                {
                    var node = branchNodes[i];
                    int nodeIndex = _treeData != null ? _treeData.GetNodeIndex(node.nodeId) : -1;
                    int level = saveData != null && nodeIndex >= 0 ? saveData.GetUpgradeLevel(nodeIndex) : 0;
                    bool isMax = level >= node.maxLevel;
                    bool isSelected = (i == _selectedCardIndex);

                    string levelStr = $"Cấp {level} / {node.maxLevel}";
                    _view.RenderNodeCard(i, node.displayName, levelStr, node.icon, isSelected, isMax);
                }
            }

            // 4. Render Details Panel
            if (branchNodes.Count > 0 && _selectedCardIndex >= 0 && _selectedCardIndex < branchNodes.Count)
            {
                var selectedNode = branchNodes[_selectedCardIndex];
                int nodeIndex = _treeData != null ? _treeData.GetNodeIndex(selectedNode.nodeId) : -1;
                int currentLevel = saveData != null && nodeIndex >= 0 ? saveData.GetUpgradeLevel(nodeIndex) : 0;
                bool isMax = currentLevel >= selectedNode.maxLevel;
                int nextCost = selectedNode.GetCostForLevel(currentLevel);
                bool canAfford = currentBalance >= nextCost && nextCost > 0;

                string branchNameStr = _currentBranch switch
                {
                    SanctuaryBranch.TanVienSonThanh => "Nhánh Tản Viên Sơn Thánh (Thiên về Công)",
                    SanctuaryBranch.PhuDongThienVuong => "Nhánh Phù Đổng Thiên Vương (Thiên về Thủ)",
                    SanctuaryBranch.LieuHanhChuDongTu => "Nhánh Liễu Hạnh & Chử Đồng Tử (Tài Phú & Bổ Trợ)",
                    _ => ""
                };

                string levelDisplay = isMax ? "<color=#FFD700>ĐÃ ĐẠT CẤP TỐI ĐA (MAX)</color>" : $"Cấp Hiện Tại: <color=#00FF88>{currentLevel}</color> / {selectedNode.maxLevel}";
                float progressRatio = (float)currentLevel / Mathf.Max(1, selectedNode.maxLevel);
                string costDisplay = isMax ? "ĐÃ TỐI ĐA" : $"Chi Phí: <color={(canAfford ? "#FFD700" : "#FF5555")}>{nextCost:N0}</color> Cổ Tiền";
                string bonusPreview = FormatBonusPreview(selectedNode, currentLevel);

                _view.DisplayUpgradeDetails(
                    selectedNode.displayName,
                    branchNameStr,
                    selectedNode.description,
                    levelDisplay,
                    progressRatio,
                    bonusPreview,
                    costDisplay,
                    canAfford,
                    isMax,
                    selectedNode.icon
                );
            }
        }

        private string FormatBonusPreview(PermanentUpgradeNode node, int currentLevel)
        {
            if (node == null) return "";
            var mod = node.statBonusPerLevel;
            var sb = new System.Text.StringBuilder();

            if (mod.maxHealthBonus > 0) sb.Append($"- Máu Tối Đa: +{mod.maxHealthBonus * currentLevel} HP (Cấp kế: +{mod.maxHealthBonus * (currentLevel + 1)})\n");
            if (mod.baseDamageBonus > 0) sb.Append($"- Sát Thương Cơ Bản: +{mod.baseDamageBonus * currentLevel:0.#} (Cấp kế: +{mod.baseDamageBonus * (currentLevel + 1):0.#})\n");
            if (mod.critChanceBonus > 0) sb.Append($"- Tỉ Lệ Bạo Kích: +{mod.critChanceBonus * currentLevel * 100:0.#}% (Cấp kế: +{mod.critChanceBonus * (currentLevel + 1) * 100:0.#}%)\n");
            if (mod.attackSpeedBonus > 0) sb.Append($"- Tốc Độ Đánh: +{mod.attackSpeedBonus * currentLevel * 100:0.#}% (Cấp kế: +{mod.attackSpeedBonus * (currentLevel + 1) * 100:0.#}%)\n");
            if (mod.moveSpeedBonus > 0) sb.Append($"- Tốc Độ Di Chuyển: +{mod.moveSpeedBonus * currentLevel:0.##} m/s (Cấp kế: +{mod.moveSpeedBonus * (currentLevel + 1):0.##})\n");
            if (mod.dashCooldownReduction > 0) sb.Append($"- Giảm Hồi Chiêu Lướt: -{mod.dashCooldownReduction * currentLevel:0.##}s (Cấp kế: -{mod.dashCooldownReduction * (currentLevel + 1):0.##}s)\n");
            if (mod.pickupRangeBonus > 0) sb.Append($"- Bán Kính Nam Châm: +{mod.pickupRangeBonus * currentLevel:0.#} m (Cấp kế: +{mod.pickupRangeBonus * (currentLevel + 1):0.#})\n");
            if (mod.expMultiplierBonus > 0) sb.Append($"- Bội Số Kinh Nghiệm: +{mod.expMultiplierBonus * currentLevel * 100:0.#}% (Cấp kế: +{mod.expMultiplierBonus * (currentLevel + 1) * 100:0.#}%)\n");

            return sb.ToString().TrimEnd();
        }
    }
}
