using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.Features.UI
{
    public struct CharacterInfo
    {
        public string name;
        public ElementType element;
        public string elementHexColor;
        public string description;
        public string signatureSkillName;
        public string signatureSkillDesc;
        public string passiveTraitName;
        public string passiveTraitDesc;
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
        [SerializeField] private CharacterSelectionView _view;
        [SerializeField] private ProjectZombie.Features.Player.CharacterSelectionData _selectionData;

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
                _view.OnHeroTabClicked += OnSelectHeroIndex;
            }
        }

        private void OnEnable()
        {
            if (_characters == null || _characters.Length == 0)
            {
                InitCharacterData();
            }
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
                _view.OnHeroTabClicked -= OnSelectHeroIndex;
            }
        }

        private void InitCharacterData()
        {
            if (_selectionData == null)
            {
                _selectionData = Resources.Load<CharacterSelectionData>("CharacterSelectionData");
#if UNITY_EDITOR
                if (_selectionData == null)
                {
                    _selectionData = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterSelectionData>("Assets/_Data/CharacterSelectionData.asset");
                }
#endif
            }

            if (_selectionData != null && _selectionData.Characters != null && _selectionData.Characters.Count > 0)
            {
                var list = _selectionData.Characters;
                _characters = new CharacterInfo[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    Sprite av = list[i].avatar;
                    if (av == null && list[i].playerPrefab != null)
                    {
                        var sr = list[i].playerPrefab.GetComponentInChildren<SpriteRenderer>();
                        if (sr != null) av = sr.sprite;
                    }

                    _characters[i] = new CharacterInfo
                    {
                        name = list[i].characterName,
                        element = list[i].element,
                        elementHexColor = string.IsNullOrEmpty(list[i].elementHexColor) ? "#FFD700" : list[i].elementHexColor,
                        description = list[i].description,
                        signatureSkillName = list[i].signatureSkillName,
                        signatureSkillDesc = list[i].signatureSkillDesc,
                        passiveTraitName = list[i].passiveTraitName,
                        passiveTraitDesc = list[i].passiveTraitDesc,
                        avatar = av,
                        primaryWeapon = list[i].defaultPrimaryWeapon,
                        relics = list[i].defaultRelics,
                        atkRatio = list[i].uiAtkRatio > 0f ? list[i].uiAtkRatio : 0.8f,
                        spdRatio = list[i].uiSpdRatio > 0f ? list[i].uiSpdRatio : 0.7f,
                        defRatio = list[i].uiDefRatio > 0f ? list[i].uiDefRatio : 0.6f
                    };
                }
                return;
            }

            Debug.LogWarning($"[{nameof(CharacterSelectionPresenter)}] _selectionData chưa được gán hoặc danh sách trống! Kiểm tra Inspector.");
            _characters = new CharacterInfo[0];
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
            global::Core.Audio.AudioManager.Instance?.PlayUIConfirm();
            if (_characters == null || _characters.Length == 0) return;
            var selected = _characters[_currentIndex];
            Debug.Log($"[{nameof(CharacterSelectionPresenter)}] Đã chọn Anh Hùng: {selected.name} (Hệ {selected.element})");

            GameObject chosenPrefab = null;

            // Single Source of Truth: Lấy trực tiếp từ CharacterSelectionData
            if (_selectionData != null && _selectionData.Characters != null && _currentIndex < _selectionData.Characters.Count)
            {
                chosenPrefab = _selectionData.Characters[_currentIndex].playerPrefab;
                _selectionData.SelectCharacter(_currentIndex);
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
            // Thiết lập RunLoadoutState cho trận đấu (Action RPG v5.0)
            if (_selectionData != null && _currentIndex < _selectionData.Characters.Count)
            {
                var charEntry = _selectionData.Characters[_currentIndex];
                
                var primaryW = charEntry.defaultPrimaryWeapon;
                var relics = new System.Collections.Generic.List<ProjectZombie.Features.Weapons.WeaponData>(charEntry.defaultRelics);

                #if UNITY_EDITOR
                if (primaryW == null || relics.Count == 0)
                {
                    // Fallback tìm kiếm vũ khí phù hợp theo hệ nhân vật
                    var allWeapons = UnityEditor.AssetDatabase.FindAssets("t:WeaponData", new[] { "Assets/_Data/Weapons" });
                    foreach (var guid in allWeapons)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                        var wd = UnityEditor.AssetDatabase.LoadAssetAtPath<ProjectZombie.Features.Weapons.WeaponData>(path);
                        if (wd == null) continue;

                        if (primaryW == null && (wd.weaponId == "W002" || wd.name.Contains("Bút") || wd.name.Contains("Kiếm") || wd.name.Contains("PhiTiêu")))
                        {
                            primaryW = wd;
                        }
                        else if (relics.Count < 3 && (wd.weaponId == "W003" || wd.weaponId == "W004" || wd.weaponId == "W005"))
                        {
                            if (!relics.Contains(wd)) relics.Add(wd);
                        }
                    }
                }
                #endif

                RunLoadoutState.SetCharacter(charEntry);
                if (RunLoadoutState.SelectedPrimaryWeapon == null)
                {
                    RunLoadoutState.SetLoadout(charEntry, primaryW, relics);
                }
                OnCharacterSelected?.Invoke(charEntry?.playerPrefab);
            }

            if (_selectionData != null)
            {
                _selectionData.SelectedCharacterIndex = _currentIndex;
            }

            // Cập nhật Sảnh Hoàng Tuyền
            var mainHubPresenter = FindObjectOfType<MainHubPresenter>(true);
            if (mainHubPresenter != null)
            {
                mainHubPresenter.RefreshHubState();
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

        private void OnSelectHeroIndex(int heroIndex)
        {
            if (_characters == null || heroIndex < 0 || heroIndex >= _characters.Length) return;
            if (_currentIndex == heroIndex) return;
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
            if (_selectionData != null && _selectionData.Characters != null && _currentIndex < _selectionData.Characters.Count)
            {
                currentPrefab = _selectionData.Characters[_currentIndex].playerPrefab;
            }

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

            _view.DisplayCharacter(charInfo.name, formattedElement, charInfo.description, formattedSkill, formattedPassive, charInfo.avatar, previewTex);
            _view.DisplayLoadout(charInfo.primaryWeapon, charInfo.relics);
            _view.UpdateActiveTab(_currentIndex);

            // Dữ liệu chỉ số sức mạnh chiến đấu được nạp trực tiếp từ SO của từng nhân vật
            float atkRatio = charInfo.atkRatio;
            float spdRatio = charInfo.spdRatio;
            float defRatio = charInfo.defRatio;

            _view.DisplayStats(atkRatio, spdRatio, defRatio, $"{(int)(atkRatio * 100)}%", $"{(int)(spdRatio * 100)}%", $"{(int)(defRatio * 100)}%");
        }
    }
}
