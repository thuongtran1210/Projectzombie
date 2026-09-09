using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.Upgrades;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Presenter điều phối toàn bộ dữ liệu Thư Viện Thần Thẻ & Công Thức Luyện Khí (Codex / Alchemy).
    /// </summary>
    public class CardCodexPresenter : MonoBehaviour
    {
        [SerializeField] private CardCodexView _view;

        [Header("UI Visual Sprites")]
        [SerializeField] private Sprite _tabActiveSprite;
        [SerializeField] private Sprite _tabInactiveSprite;
        [SerializeField] private Sprite _cardSlotWoodSprite;
        [SerializeField] private Sprite _cardSlotSelectedSprite;

        private List<UpgradeData> _allUpgrades = new List<UpgradeData>();
        private List<FusionUpgradeData> _allFusionUpgrades = new List<FusionUpgradeData>();
        private CodexTabType _currentTab = CodexTabType.RelicFusion;
        private UpgradeData _selectedUpgrade;

        private void Awake()
        {
            if (_view == null) _view = GetComponent<CardCodexView>();
            LoadAllUpgradeData();

            if (_view != null)
            {
                _view.OnBackClicked += HandleBack;
                _view.OnTabChanged += SetTab;
            }
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.OnBackClicked -= HandleBack;
                _view.OnTabChanged -= SetTab;
            }
        }

        private void OnEnable()
        {
            LoadAllUpgradeData();
            RefreshCurrency();
            SetTab(_currentTab);
        }

        public void LoadAllUpgradeData()
        {
            _allUpgrades.Clear();
            _allFusionUpgrades.Clear();

            var loaded = Resources.LoadAll<UpgradeData>("");
            if (loaded != null)
            {
                foreach (var u in loaded)
                {
                    if (u != null)
                    {
                        _allUpgrades.Add(u);
                        if (u is FusionUpgradeData f) _allFusionUpgrades.Add(f);
                    }
                }
            }

#if UNITY_EDITOR
            if (_allUpgrades.Count == 0 || _allFusionUpgrades.Count == 0)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:UpgradeData");
                foreach (var guid in guids)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    var u = UnityEditor.AssetDatabase.LoadAssetAtPath<UpgradeData>(path);
                    if (u != null && !_allUpgrades.Contains(u))
                    {
                        _allUpgrades.Add(u);
                        if (u is FusionUpgradeData f && !_allFusionUpgrades.Contains(f)) _allFusionUpgrades.Add(f);
                    }
                }
            }
#endif
        }

        private void RefreshCurrency()
        {
            var curMgr = FindObjectOfType<MetaCurrencyManager>();
            int coins = curMgr != null ? curMgr.TotalCurrency : 0;
            if (_view != null) _view.SetCoTienBalance($"<color=#FFD700>{coins:N0}</color>");
        }

        public void SetTab(CodexTabType tab)
        {
            _currentTab = tab;
            if (_view != null) _view.UpdateTabVisuals(tab, _tabActiveSprite, _tabInactiveSprite);
            PopulateGridForTab(tab);
        }

        private void PopulateGridForTab(CodexTabType tab)
        {
            if (_view == null || _view.CardsGridContainer == null) return;

            // Xóa các item cũ
            for (int i = _view.CardsGridContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_view.CardsGridContainer.GetChild(i).gameObject);
            }

            List<UpgradeData> filterList = new List<UpgradeData>();
            switch (tab)
            {
                case CodexTabType.RelicFusion:
                    filterList.AddRange(_allFusionUpgrades);
                    break;
                case CodexTabType.Passives:
                    filterList.AddRange(_allUpgrades.FindAll(u => u.upgradeType == UpgradeType.CommonUpgrade || u.upgradeType == UpgradeType.RareUpgrade));
                    break;
                case CodexTabType.ComboSkills:
                    filterList.AddRange(_allUpgrades.FindAll(u => u.upgradeType == UpgradeType.ComboAugment || u.upgradeType == UpgradeType.DashTrait || u.upgradeType == UpgradeType.BreakthroughUltimate));
                    break;
            }

            if (filterList.Count > 0)
            {
                for (int i = 0; i < filterList.Count; i++)
                {
                    var data = filterList[i];
                    CreateCardItemView(data);
                }
                SelectCard(filterList[0]);
            }
            else
            {
                _view.DisplayCardDetail("Chưa Có Dữ Liệu", "", "Thư viện đang được cập nhật...", null, false);
            }
        }

        private void CreateCardItemView(UpgradeData data)
        {
            GameObject itemObj = new GameObject($"CardItem_{data.id}", typeof(RectTransform), typeof(Image), typeof(Button));
            itemObj.transform.SetParent(_view.CardsGridContainer, false);

            var rt = itemObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100, 120);

            var img = itemObj.GetComponent<Image>();
            img.type = Image.Type.Sliced;
            if (_cardSlotWoodSprite != null) img.sprite = _cardSlotWoodSprite;
            img.color = Color.white;

            // Icon
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(itemObj.transform, false);
            var iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.sizeDelta = new Vector2(64, 64);
            iconRT.anchoredPosition = new Vector2(0, 10);

            var iconImg = iconObj.GetComponent<Image>();
            iconImg.sprite = data.icon;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            // Tên nhỏ
            GameObject txtObj = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(itemObj.transform, false);
            var txtRT = txtObj.GetComponent<RectTransform>();
            txtRT.anchorMin = new Vector2(0, 0);
            txtRT.anchorMax = new Vector2(1, 0);
            txtRT.pivot = new Vector2(0.5f, 0);
            txtRT.anchoredPosition = new Vector2(0, 4);
            txtRT.sizeDelta = new Vector2(-6, 26);

            var tmp = txtObj.GetComponent<TextMeshProUGUI>();
            tmp.text = data.upgradeName;
            tmp.fontSize = 11;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.95f, 0.88f, 0.70f, 1f);
            tmp.enableWordWrapping = true;

            var btn = itemObj.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                global::Core.Audio.AudioManager.Instance?.PlayUIClick();
                SelectCard(data);
            });
        }

        private void SelectCard(UpgradeData data)
        {
            if (data == null) return;
            _selectedUpgrade = data;

            bool isFusion = data is FusionUpgradeData;
            string categoryName = data.GetCategoryDisplayName();

            _view.DisplayCardDetail(data.upgradeName, categoryName, data.description, data.icon, isFusion);
        }

        private void HandleBack()
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIClick();
            var metaManager = GetComponentInParent<MetaUIManager>() ?? MetaUIManager.Instance;
            if (metaManager != null)
            {
                metaManager.PopScreen();
            }
        }
    }
}
