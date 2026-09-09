using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectZombie.Features.UI
{
    public enum CodexTabType
    {
        RelicFusion, // Luyện Hóa Thần Binh
        Passives,    // Thần Thẻ Bị Động
        ComboSkills  // Bí Kíp Đòn Chém
    }

    /// <summary>
    /// Passive View quản lý hiển thị Bách Bảo Các (Thần Thẻ & Luyện Khí Codex UI).
    /// </summary>
    public class CardCodexView : BaseMetaScreenView
    {
        public override MetaScreenType ScreenType => MetaScreenType.Codex;

        [Header("Header")]
        [SerializeField] private Button _backButton;
        [SerializeField] private TextMeshProUGUI _coTienText;

        [Header("Navigation Tabs")]
        [SerializeField] private Button _tabFusionButton;
        [SerializeField] private Button _tabPassivesButton;
        [SerializeField] private Button _tabComboButton;
        [SerializeField] private Image _tabFusionBg;
        [SerializeField] private Image _tabPassivesBg;
        [SerializeField] private Image _tabComboBg;
        [SerializeField] private TextMeshProUGUI _tabFusionTxt;
        [SerializeField] private TextMeshProUGUI _tabPassivesTxt;
        [SerializeField] private TextMeshProUGUI _tabComboTxt;

        [Header("Grid / List")]
        [SerializeField] private Transform _cardsGridContainer;

        [Header("Detail Panel (Cột Phải)")]
        [SerializeField] private Image _detailIcon;
        [SerializeField] private TextMeshProUGUI _detailName;
        [SerializeField] private TextMeshProUGUI _detailType;
        [SerializeField] private TextMeshProUGUI _detailDesc;
        [SerializeField] private GameObject _fusionRecipeSection;
        [SerializeField] private Transform _recipeIngredientsContainer;
        [SerializeField] private Button _alchemyFusionButton;
        [SerializeField] private TextMeshProUGUI _fusionButtonText;

        public event Action OnBackClicked;
        public event Action<CodexTabType> OnTabChanged;
        public event Action OnAlchemyFusionClicked;

        protected override void Awake()
        {
            base.Awake();

            if (_backButton != null) _backButton.onClick.AddListener(() => OnBackClicked?.Invoke());
            if (_dimBackgroundButton != null) _dimBackgroundButton.onClick.AddListener(() => OnBackClicked?.Invoke());

            if (_tabFusionButton != null) _tabFusionButton.onClick.AddListener(() => OnTabChanged?.Invoke(CodexTabType.RelicFusion));
            if (_tabPassivesButton != null) _tabPassivesButton.onClick.AddListener(() => OnTabChanged?.Invoke(CodexTabType.Passives));
            if (_tabComboButton != null) _tabComboButton.onClick.AddListener(() => OnTabChanged?.Invoke(CodexTabType.ComboSkills));

            if (_alchemyFusionButton != null) _alchemyFusionButton.onClick.AddListener(() => OnAlchemyFusionClicked?.Invoke());
        }

        public override void OnBackPressed()
        {
            OnBackClicked?.Invoke();
        }

        public void SetCoTienBalance(string formattedText)
        {
            if (_coTienText != null) _coTienText.text = formattedText;
        }

        public void UpdateTabVisuals(CodexTabType activeTab, Sprite activeSprite, Sprite inactiveSprite)
        {
            Color activeCol = new Color(0.98f, 0.88f, 0.50f, 1f);
            Color inactiveCol = new Color(0.85f, 0.78f, 0.65f, 1f);

            if (_tabFusionBg != null) _tabFusionBg.sprite = activeTab == CodexTabType.RelicFusion ? activeSprite : inactiveSprite;
            if (_tabPassivesBg != null) _tabPassivesBg.sprite = activeTab == CodexTabType.Passives ? activeSprite : inactiveSprite;
            if (_tabComboBg != null) _tabComboBg.sprite = activeTab == CodexTabType.ComboSkills ? activeSprite : inactiveSprite;

            if (_tabFusionTxt != null) _tabFusionTxt.color = activeTab == CodexTabType.RelicFusion ? activeCol : inactiveCol;
            if (_tabPassivesTxt != null) _tabPassivesTxt.color = activeTab == CodexTabType.Passives ? activeCol : inactiveCol;
            if (_tabComboTxt != null) _tabComboTxt.color = activeTab == CodexTabType.ComboSkills ? activeCol : inactiveCol;
        }

        public void DisplayCardDetail(string cardName, string category, string desc, Sprite icon, bool isFusion, List<string> ingredients = null)
        {
            if (_detailName != null) _detailName.text = cardName;
            if (_detailType != null) _detailType.text = category;
            if (_detailDesc != null) _detailDesc.text = desc;
            if (_detailIcon != null)
            {
                _detailIcon.sprite = icon;
                _detailIcon.enabled = icon != null;
            }

            if (_fusionRecipeSection != null)
            {
                _fusionRecipeSection.SetActive(isFusion);
            }
        }

        public Transform CardsGridContainer => _cardsGridContainer;
        public Transform RecipeIngredientsContainer => _recipeIngredientsContainer;
    }
}
