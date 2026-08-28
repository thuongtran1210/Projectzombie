using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.MetaProgression;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Presenter điều phối Sảnh Hoàng Tuyền (Main Hub).
    /// Tuân thủ MVP: Điều hướng độc lập giữa Chọn Tướng, Tàng Bảo Các, Miếu Cổ và Xuất Trận.
    /// </summary>
    public class MainHubPresenter : MonoBehaviour
    {
        [Header("View Reference")]
        [SerializeField] private MainHubView _view;

        [Header("Manager Reference")]
        [SerializeField] private MetaCurrencyManager _currencyManager;
        [SerializeField] private MetaUIManager _metaUIManager;

        public event System.Action OnStartRunRequested;

        private void Awake()
        {
            if (_view == null) _view = GetComponent<MainHubView>();
            if (_currencyManager == null) _currencyManager = FindObjectOfType<MetaCurrencyManager>();
            if (_metaUIManager == null) _metaUIManager = GetComponentInParent<MetaUIManager>() ?? MetaUIManager.Instance;
        }

        private MetaUIManager GetMetaUIManager()
        {
            if (_metaUIManager == null)
            {
                _metaUIManager = MetaUIManager.Instance ?? GetComponentInParent<MetaUIManager>();
#if UNITY_EDITOR
                if (_metaUIManager == null) _metaUIManager = FindObjectOfType<MetaUIManager>(true);
#endif
            }
            return _metaUIManager;
        }

        private void Start()
        {
            if (_view != null)
            {
                _view.OnStartRunClicked += HandleStartRunClicked;
                _view.OnHeroSelectClicked += HandleHeroSelectClicked;
                _view.OnArmoryClicked += HandleArmoryClicked;
                _view.OnSanctuaryTreeClicked += HandleSanctuaryTreeClicked;
                _view.OnCodexClicked += HandleCodexClicked;
                _view.OnSettingsClicked += HandleSettingsClicked;
            }

            if (_currencyManager != null)
            {
                _currencyManager.OnCurrencyChanged += UpdateCurrencyDisplay;
            }

            UpdateCurrencyDisplay();
            RefreshHubState();
        }

        private void OnEnable()
        {
            RefreshHubState();
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.OnStartRunClicked -= HandleStartRunClicked;
                _view.OnHeroSelectClicked -= HandleHeroSelectClicked;
                _view.OnArmoryClicked -= HandleArmoryClicked;
                _view.OnSanctuaryTreeClicked -= HandleSanctuaryTreeClicked;
                _view.OnCodexClicked -= HandleCodexClicked;
                _view.OnSettingsClicked -= HandleSettingsClicked;
            }

            if (_currencyManager != null)
            {
                _currencyManager.OnCurrencyChanged -= UpdateCurrencyDisplay;
            }
        }

        public void RefreshHubState()
        {
            UpdateCurrencyDisplay();
            UpdateSelectedHeroDisplay();
            UpdateLoadoutSummaryDisplay();
        }

        public void UpdateCurrencyDisplay(int amount = -1)
        {
            if (_view != null)
            {
                int balance = amount >= 0 ? amount : (_currencyManager != null ? _currencyManager.TotalCurrency : 0);
                _view.SetCoTienBalance($"<color=#FFD700>{balance:N0}</color>");
                _view.SetLinhHonBalance("<color=#B388FF>0</color>");
            }
        }

        private void UpdateSelectedHeroDisplay()
        {
            if (_view == null) return;

            CharacterEntry hero = RunLoadoutState.SelectedCharacter;
            if (hero == null)
            {
                var selectionData = Resources.Load<CharacterSelectionData>("CharacterSelectionData");
                #if UNITY_EDITOR
                if (selectionData == null)
                {
                    selectionData = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterSelectionData>("Assets/_Data/CharacterSelectionData.asset");
                }
                #endif
                if (selectionData != null && selectionData.Characters != null && selectionData.Characters.Count > 0)
                {
                    int idx = Mathf.Clamp(selectionData.SelectedCharacterIndex, 0, selectionData.Characters.Count - 1);
                    hero = selectionData.Characters[idx];
                    RunLoadoutState.SetCharacter(hero);
                }
            }

            RenderTexture previewTex = null;
            if (hero != null && hero.playerPrefab != null)
            {
                if (CharacterPreviewStage.Instance == null)
                {
                    var stageObj = new GameObject("CharacterPreviewStage");
                    stageObj.transform.position = new Vector3(2000f, 2000f, 0f);
                    stageObj.AddComponent<CharacterPreviewStage>();
                }

                if (CharacterPreviewStage.Instance != null)
                {
                    CharacterPreviewStage.Instance.DisplayCharacter(hero.playerPrefab, "Attack");
                    previewTex = CharacterPreviewStage.Instance.PreviewTexture;
                }
            }

            if (hero != null)
            {
                string elemStr = $"<color={hero.elementHexColor}>Hệ {hero.element}</color>";
                _view.SetSelectedHeroPreview(hero.characterName, elemStr, hero.avatar, previewTex);
            }
            else
            {
                _view.SetSelectedHeroPreview("ĐẠO SĨ", "<color=#4CAF50>Hệ Mộc</color>", null, null);
            }
        }

        private void UpdateLoadoutSummaryDisplay()
        {
            if (_view == null) return;

            CharacterEntry hero = RunLoadoutState.SelectedCharacter;
            WeaponData primary = RunLoadoutState.SelectedPrimaryWeapon;
            List<WeaponData> relics = RunLoadoutState.SelectedRelics;

            string priName = "Đòn Đánh";
            Sprite priSprite = null;

            if (hero != null)
            {
                priName = !string.IsNullOrEmpty(hero.basicAttackConfig?.attackName) 
                    ? hero.basicAttackConfig.attackName 
                    : hero.characterName;
                priSprite = hero.basicAttackConfig?.attackIcon != null ? hero.basicAttackConfig.attackIcon : hero.avatar;
            }
            else if (primary != null)
            {
                priName = primary.weaponName;
                priSprite = primary.icon;
            }

            var relicSprites = new List<Sprite>();
            if (relics != null)
            {
                foreach (var r in relics)
                {
                    if (r != null && r.icon != null) relicSprites.Add(r.icon);
                }
            }

            _view.SetEquippedLoadoutSummary(priName, priSprite, relicSprites);
        }

        private void HandleStartRunClicked()
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIConfirm();
            Debug.Log("<color=#00FF88>[MainHubPresenter]</color> Bắt đầu xuất trận với Tướng & Loadout đã lưu!");

            OnStartRunRequested?.Invoke();

            if (MetaSceneTransitionController.Instance != null)
            {
                MetaSceneTransitionController.Instance.StartRun();
            }
            else
            {
                var transitionCtrl = FindObjectOfType<MetaSceneTransitionController>();
                if (transitionCtrl != null)
                {
                    transitionCtrl.StartRun();
                }
                else if (MetaUIManager.Instance != null)
                {
                    MetaUIManager.Instance.SetMetaCanvasActive(false);
                }
            }
        }

        private void HandleHeroSelectClicked()
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIClick();
            var metaManager = GetMetaUIManager();
            if (metaManager != null)
            {
                metaManager.OpenScreen(MetaScreenType.CharacterSelect);
            }
        }

        private void HandleArmoryClicked()
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIClick();
            var metaManager = GetMetaUIManager();
            if (metaManager != null)
            {
                metaManager.OpenScreen(MetaScreenType.WeaponLoadout);
            }
        }

        private void HandleSanctuaryTreeClicked()
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIClick();
            var metaManager = GetMetaUIManager();
            if (metaManager != null)
            {
                metaManager.OpenScreen(MetaScreenType.SanctuaryTree);
            }
        }

        private void HandleCodexClicked()
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIClick();
            var metaManager = GetMetaUIManager();
            if (metaManager != null)
            {
                metaManager.OpenScreen(MetaScreenType.Codex);
            }
        }

        private void HandleSettingsClicked()
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIClick();
            var metaManager = GetMetaUIManager();
            if (metaManager != null)
            {
                metaManager.OpenScreen(MetaScreenType.Settings);
            }
        }
    }
}
