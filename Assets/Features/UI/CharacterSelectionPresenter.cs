using UnityEngine;
using ProjectZombie.Features.Shared;

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
    }

    /// <summary>
    /// Presenter điều phối dữ liệu chọn nhân vật giữa Model và CharacterSelectionView.
    /// </summary>
    public class CharacterSelectionPresenter : MonoBehaviour
    {
        [SerializeField] private CharacterSelectionView _view;
        [SerializeField] private ProjectZombie.Features.Player.CharacterSelectionData _selectionData;
        [SerializeField] private GameObject[] _characterPrefabs; // [0]: Thu Sinh, [1]: Dao Si, [2]: Thanh Dong, [3]: An Si

        public event System.Action<GameObject> OnCharacterSelected;

        private CharacterInfo[] _characters;
        private int _currentIndex = 0;

        private void Awake()
        {
            InitCharacterData();

            if (_view != null)
            {
                _view.OnNextClicked += OnNextCharacter;
                _view.OnPrevClicked += OnPrevCharacter;
                _view.OnSelectClicked += OnSelectCharacter;
            }
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
            }
        }

        private void InitCharacterData()
        {
            if (_selectionData != null && _selectionData.Characters != null && _selectionData.Characters.Count > 0)
            {
                var list = _selectionData.Characters;
                _characters = new CharacterInfo[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
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
                        avatar = list[i].avatar
                    };
                }
                return;
            }

            Debug.LogWarning($"[{nameof(CharacterSelectionPresenter)}] _selectionData chưa được gán hoặc danh sách trống! Kiểm tra Inspector.");
            _characters = new CharacterInfo[0];
        }

        private void OnNextCharacter()
        {
            if (_characters == null || _characters.Length == 0) return;
            _currentIndex = (_currentIndex + 1) % _characters.Length;
            RenderCurrentCharacter();
        }

        private void OnPrevCharacter()
        {
            if (_characters == null || _characters.Length == 0) return;
            _currentIndex = (_currentIndex - 1 + _characters.Length) % _characters.Length;
            RenderCurrentCharacter();
        }

        private void OnSelectCharacter()
        {
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
            else if (_characterPrefabs != null && _currentIndex < _characterPrefabs.Length)
            {
                chosenPrefab = _characterPrefabs[_currentIndex];
                if (_selectionData != null)
                {
                    _selectionData.SelectedPlayerPrefab = chosenPrefab;
                }
            }

            if (chosenPrefab == null)
            {
                Debug.LogError($"[{nameof(CharacterSelectionPresenter)}] Không tìm thấy Prefab của nhân vật tại index {_currentIndex}!");
            }
            else
            {
                Debug.Log($"[{nameof(CharacterSelectionPresenter)}] Đang phát event OnCharacterSelected với Prefab: {chosenPrefab.name}");
            }

            OnCharacterSelected?.Invoke(chosenPrefab);

            // Tự động đóng Popup Chọn Nhân Vật để vào trận
            if (gameObject != null)
            {
                gameObject.SetActive(false);
            }
        }

        private void RenderCurrentCharacter()
        {
            if (_view == null || _characters == null || _characters.Length == 0) return;

            var charInfo = _characters[_currentIndex];
            string formattedElement = $"<color={charInfo.elementHexColor}>Hệ {charInfo.element}</color>";
            string formattedSkill = $"<b>{charInfo.signatureSkillName}</b>: {charInfo.signatureSkillDesc}";
            string formattedPassive = $"<b>{charInfo.passiveTraitName}</b>: {charInfo.passiveTraitDesc}";

            _view.DisplayCharacter(charInfo.name, formattedElement, charInfo.description, formattedSkill, formattedPassive, charInfo.avatar);
        }
    }
}
