using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Features.UI
{
    public struct CharacterInfo
    {
        public string characterId;
        public int unlockCost;
        public string name;
        public ElementType element;
        public string elementHexColor;
        public string description;
        public string signatureSkillName;
        public string signatureSkillDesc;
        public Sprite signatureSkillIcon;
        public string passiveTraitName;
        public string passiveTraitDesc;
        public Sprite passiveTraitIcon;
        public Sprite avatar;
        public WeaponData primaryWeapon;
        public System.Collections.Generic.List<WeaponData> relics;
        public float atkRatio;
        public float spdRatio;
        public float defRatio;
    }

    /// <summary>
    /// Presenter điều phối dữ liệu chọn nhân vật giữa Model và CharacterSelectionView.
    /// </summary>
    public class CharacterSelectionPresenter : MonoBehaviour
    {
        [Header("View Reference")]
        [SerializeField] private CharacterSelectionView _view;

        [Header("Character Database (Drag & Drop SO)")]
        [Tooltip("Database nhân vật chuẩn hóa dạng kéo thả từng file CharacterDataSO")]
        [SerializeField] private CharacterDatabaseSO _characterDatabase;

        public event System.Action<GameObject> OnCharacterSelected;

        private CharacterInfo[] _characters;
        private int _currentIndex = 0;

        private void Awake()
        {
            if (_view == null)
            {
                _view = GetComponent<CharacterSelectionView>();
                if (_view == null) _view = GetComponentInChildren<CharacterSelectionView>(true);
            }

            InitCharacterData();

            if (_view != null)
            {
                _view.OnNextClicked += OnNextCharacter;
                _view.OnPrevClicked += OnPrevCharacter;
                _view.OnSelectClicked += OnSelectCharacter;
                _view.OnHeroTabClicked += OnHeroTabSelected;
            }
        }

        private void OnEnable()
        {
            InitCharacterData();
            RenderCurrentCharacter();
        }

        private void Start()
        {
            RenderCurrentCharacter();
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.OnNextClicked -= OnNextCharacter;
                _view.OnPrevClicked -= OnPrevCharacter;
                _view.OnSelectClicked -= OnSelectCharacter;
                _view.OnHeroTabClicked -= OnHeroTabSelected;
            }
        }

        private void InitCharacterData()
        {
            // 1. Ưu tiên nạp từ CharacterDatabaseSO (Chuẩn Drag & Drop)
            if (_characterDatabase == null)
            {
                _characterDatabase = Resources.Load<CharacterDatabaseSO>("CharacterDatabase");
#if UNITY_EDITOR
                if (_characterDatabase == null)
                {
                    _characterDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterDatabaseSO>("Assets/_Data/CharacterDatabase.asset");
                }
#endif
            }

            if (_characterDatabase != null && _characterDatabase.Characters != null && _characterDatabase.Characters.Count > 0)
            {
                var list = _characterDatabase.Characters;
                _characters = new CharacterInfo[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == null) continue;
                    Sprite av = list[i].avatar;
                    if (av == null && list[i].playerPrefab != null)
                    {
                        var sr = list[i].playerPrefab.GetComponentInChildren<SpriteRenderer>();
                        if (sr != null) av = sr.sprite;
                    }

                    var relicsList = new System.Collections.Generic.List<ProjectZombie.Features.Weapons.WeaponData>();
                    if (list[i].defaultRelics != null && list[i].defaultRelics.Count > 0)
                    {
                        relicsList.AddRange(list[i].defaultRelics);
                    }
                    if (list[i].defaultRelic != null && !relicsList.Contains(list[i].defaultRelic))
                    {
                        relicsList.Insert(0, list[i].defaultRelic);
                    }

                    _characters[i] = new CharacterInfo
                    {
                        characterId = list[i].characterId,
                        unlockCost = 0,
                        name = list[i].characterName,
                        element = list[i].element,
                        elementHexColor = string.IsNullOrEmpty(list[i].elementHexColor) ? "#FFD700" : list[i].elementHexColor,
                        description = list[i].description,
                        signatureSkillName = list[i].signatureSkillName,
                        signatureSkillDesc = list[i].signatureSkillDesc,
                        signatureSkillIcon = list[i].signatureSkillIcon,
                        passiveTraitName = list[i].passiveTraitName,
                        passiveTraitDesc = list[i].passiveTraitDesc,
                        passiveTraitIcon = list[i].passiveTraitIcon,
                        avatar = av,
                        primaryWeapon = list[i].defaultPrimaryWeapon,
                        relics = relicsList,
                        atkRatio = list[i].uiAtkRatio > 0f ? list[i].uiAtkRatio : 0.8f,
                        spdRatio = list[i].uiSpdRatio > 0f ? list[i].uiSpdRatio : 0.7f,
                        defRatio = list[i].uiDefRatio > 0f ? list[i].uiDefRatio : 0.6f
                    };
                }

                if (_view != null && _characters.Length > 0)
                {
                    Sprite[] tabAvatars = new Sprite[_characters.Length];
                    for (int a = 0; a < _characters.Length; a++)
                    {
                        tabAvatars[a] = _characters[a].avatar;
                    }
                    _view.SetupHeroTabAvatars(tabAvatars);
                }
                return;
            }

            Debug.LogWarning($"[{nameof(CharacterSelectionPresenter)}] Chưa gán Database Nhân Vật! Kiểm tra Inspector.");
            _characters = new CharacterInfo[0];
        }

        private bool IsHeroUnlocked(string heroId)
        {
            if (string.IsNullOrEmpty(heroId)) return true;
            // Tướng khởi đầu mặc định luôn mở
            if (heroId == "default" || heroId == "C001_ThuSinh") return true;
            if (MetaCurrencyManager.Instance != null)
            {
                return MetaCurrencyManager.Instance.IsCharacterUnlocked(heroId);
            }
            return true;
        }

        private void OnNextCharacter()
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIClick();
            if (_characters == null || _characters.Length == 0) return;
            _currentIndex = (_currentIndex + 1) % _characters.Length;
            RenderCurrentCharacter();
        }

        private void OnPrevCharacter()
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIClick();
            if (_characters == null || _characters.Length == 0) return;
            _currentIndex = (_currentIndex - 1 + _characters.Length) % _characters.Length;
            RenderCurrentCharacter();
        }

        private void OnSelectCharacter()
        {
            if (_characters == null || _characters.Length == 0) return;
            var selected = _characters[_currentIndex];

            // Kiểm tra trạng thái mở khóa từ Save Data
            if (!IsHeroUnlocked(selected.characterId))
            {
                global::Core.Audio.AudioManager.Instance?.PlayUIClick();
                Debug.LogWarning($"[{nameof(CharacterSelectionPresenter)}] Anh Hùng '{selected.name}' ({selected.characterId}) chưa được mở khóa trong Save Data!");
                return;
            }

            global::Core.Audio.AudioManager.Instance?.PlayUIConfirm();
            Debug.Log($"[{nameof(CharacterSelectionPresenter)}] Đã chọn Anh Hùng: {selected.name} (Hệ {selected.element})");

            GameObject chosenPrefab = null;

            // Single Source of Truth: Ưu tiên lấy trực tiếp từ CharacterDatabaseSO
            if (_characterDatabase != null && _characterDatabase.Characters != null && _currentIndex < _characterDatabase.Characters.Count)
            {
                var so = _characterDatabase.Characters[_currentIndex];
                if (so != null)
                {
                    chosenPrefab = so.playerPrefab;
                    var primaryW = so.defaultPrimaryWeapon;
                    var relics = new System.Collections.Generic.List<ProjectZombie.Features.Weapons.WeaponData>(so.defaultRelics);
                    if (so.defaultRelic != null && !relics.Contains(so.defaultRelic)) relics.Insert(0, so.defaultRelic);

                    // Chuyển đổi sang CharacterEntry để nạp vào RunLoadoutState
                    var entry = new CharacterEntry
                    {
                        characterId = so.characterId,
                        characterName = so.characterName,
                        element = so.element,
                        elementHexColor = so.elementHexColor,
                        description = so.description,
                        baseMaxHealth = so.baseMaxHealth,
                        baseMoveSpeed = so.baseMoveSpeed,
                        baseDamage = so.baseDamage,
                        baseCritChance = so.baseCritChance,
                        baseDashCooldown = so.baseDashCooldown,
                        uiAtkRatio = so.uiAtkRatio,
                        uiSpdRatio = so.uiSpdRatio,
                        uiDefRatio = so.uiDefRatio,
                        signatureSkillName = so.signatureSkillName,
                        signatureSkillDesc = so.signatureSkillDesc,
                        passiveTraitName = so.passiveTraitName,
                        passiveTraitDesc = so.passiveTraitDesc,
                        avatar = so.avatar,
                        playerPrefab = so.playerPrefab,
                        basicAttackConfig = so.basicAttackConfig,
                        defaultRelic = so.defaultRelic,
                        defaultPrimaryWeapon = so.defaultPrimaryWeapon,
                        defaultRelics = so.defaultRelics
                    };

                    RunLoadoutState.SetCharacter(entry);
                    if (RunLoadoutState.SelectedPrimaryWeapon == null)
                    {
                        RunLoadoutState.SetLoadout(entry, primaryW, relics);
                    }
                    OnCharacterSelected?.Invoke(chosenPrefab);
                }
            }

            if (chosenPrefab == null)
            {
                string[] heroNames = new string[] { "Thu Sinh", "Dao Si", "Thanh Dong", "An Si" };
                if (_currentIndex >= 0 && _currentIndex < heroNames.Length)
                {
                    chosenPrefab = Resources.Load<GameObject>($"Players/{heroNames[_currentIndex]}") ??
                                   Resources.Load<GameObject>(heroNames[_currentIndex]);
                }
            }

            #if UNITY_EDITOR
            if (chosenPrefab == null)
            {
                string[] paths = new string[] {
                    "Assets/_Prefabs/Characters/Players/Thu Sinh.prefab",
                    "Assets/_Prefabs/Characters/Players/Dao Si.prefab",
                    "Assets/_Prefabs/Characters/Players/Thanh Dong.prefab",
                    "Assets/_Prefabs/Characters/Players/An Si.prefab"
                };
                if (_currentIndex >= 0 && _currentIndex < paths.Length)
                {
                    chosenPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(paths[_currentIndex]);
                }
            }
            #endif

            if (chosenPrefab == null)
            {
                Debug.LogError($"[{nameof(CharacterSelectionPresenter)}] Không tìm thấy Prefab của nhân vật tại index {_currentIndex}!");
            }

            // Cập nhật Sảnh Hoàng Tuyền
            var mainHubPresenter = FindObjectOfType<MainHubPresenter>(true);
            if (mainHubPresenter != null)
            {
                mainHubPresenter.RefreshHubState();
            }

            // Xóa focus active control của EventSystem trước khi đóng màn hình
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            }

            // Đóng Modal và quay về Sảnh Chính
            if (MetaUIManager.Instance != null)
            {
                MetaUIManager.Instance.PopScreen();
            }
            else if (gameObject != null)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnHeroTabSelected(int heroIndex)
        {
            if (_characters == null || heroIndex < 0 || heroIndex >= _characters.Length) return;
            if (_currentIndex == heroIndex) return;

            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            }

            global::Core.Audio.AudioManager.Instance?.PlayUIClick();
            _currentIndex = heroIndex;
            RenderCurrentCharacter();
        }

        private void RenderCurrentCharacter()
        {
            if (_view == null || _characters == null || _characters.Length == 0) return;

            var charInfo = _characters[_currentIndex];
            string formattedElement = $"<color={charInfo.elementHexColor}>Hệ {charInfo.element}</color>";
            string formattedSkill = $"<b>{charInfo.signatureSkillName}</b>: {charInfo.signatureSkillDesc}";
            string formattedPassive = $"<b>{charInfo.passiveTraitName}</b>: {charInfo.passiveTraitDesc}";

            RenderTexture previewTex = null;
            GameObject currentPrefab = null;
            if (_characterDatabase != null && _characterDatabase.Characters != null && _currentIndex < _characterDatabase.Characters.Count && _characterDatabase.Characters[_currentIndex] != null)
            {
                currentPrefab = _characterDatabase.Characters[_currentIndex].playerPrefab;
            }

            if (currentPrefab == null)
            {
                string[] heroNames = new string[] { "Thu Sinh", "Dao Si", "Thanh Dong", "An Si" };
                if (_currentIndex >= 0 && _currentIndex < heroNames.Length)
                {
                    currentPrefab = Resources.Load<GameObject>($"Players/{heroNames[_currentIndex]}") ??
                                    Resources.Load<GameObject>(heroNames[_currentIndex]);
                }
            }

            #if UNITY_EDITOR
            if (currentPrefab == null)
            {
                string[] paths = new string[] {
                    "Assets/_Prefabs/Characters/Players/Thu Sinh.prefab",
                    "Assets/_Prefabs/Characters/Players/Dao Si.prefab",
                    "Assets/_Prefabs/Characters/Players/Thanh Dong.prefab",
                    "Assets/_Prefabs/Characters/Players/An Si.prefab"
                };
                if (_currentIndex >= 0 && _currentIndex < paths.Length)
                {
                    currentPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(paths[_currentIndex]);
                }
            }
            #endif

            if (currentPrefab != null)
            {
                if (CharacterPreviewStage.Instance == null)
                {
                    var stageObj = new GameObject("CharacterPreviewStage");
                    stageObj.transform.position = new Vector3(2000f, 2000f, 0f);
                    stageObj.AddComponent<CharacterPreviewStage>();
                }

                if (CharacterPreviewStage.Instance != null)
                {
                    CharacterPreviewStage.Instance.DisplayCharacter(currentPrefab, "Attack");
                    previewTex = CharacterPreviewStage.Instance.PreviewTexture;
                }
            }

            _view.DisplayCharacter(charInfo.name, formattedElement, charInfo.description, formattedSkill, formattedPassive, charInfo.avatar, previewTex, charInfo.signatureSkillIcon, charInfo.passiveTraitIcon);
            _view.DisplayLoadout(charInfo.primaryWeapon, charInfo.relics);
            _view.UpdateActiveTab(_currentIndex);

            bool isUnlocked = IsHeroUnlocked(charInfo.characterId);
            _view.SetCharacterLockState(isUnlocked, charInfo.unlockCost);

            // Dữ liệu chỉ số sức mạnh chiến đấu được nạp trực tiếp từ SO của từng nhân vật
            float atkRatio = charInfo.atkRatio;
            float spdRatio = charInfo.spdRatio;
            float defRatio = charInfo.defRatio;

            _view.DisplayStats(atkRatio, spdRatio, defRatio, $"{(int)(atkRatio * 100)}%", $"{(int)(spdRatio * 100)}%", $"{(int)(defRatio * 100)}%");
        }
    }
}
