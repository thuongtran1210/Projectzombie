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
        }

        private void Start()
        {
            if (_view != null)
            {
                _view.OnNextClicked += OnNextCharacter;
                _view.OnPrevClicked += OnPrevCharacter;
                _view.OnSelectClicked += OnSelectCharacter;
            }

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

            // Fallback nếu chưa cấu hình ScriptableObject
            _characters = new CharacterInfo[]
            {
                new CharacterInfo
                {
                    name = "Thư Sinh",
                    element = ElementType.Kim,
                    elementHexColor = "#FFD700",
                    description = "Được anh linh liệt tổ & Đức Thánh Trần điểm hóa. Tay cầm bút lệnh khí thiêng sông núi phán định tà ma.",
                    signatureSkillName = "Phán Quyết Tiền Định",
                    signatureSkillDesc = "Chèn 1 hit ảo Ngũ Hành vào Queue Tương Sinh, kích hoạt giảm 20% Cooldown cho vũ khí khớp lệnh.",
                    passiveTraitName = "Văn Khí Hộ Thể",
                    passiveTraitDesc = "Khi kích hoạt Tương Sinh Ngũ Hành, tăng 15% Tốc độ di chuyển và hồi 5% HP tối đa."
                },
                new CharacterInfo
                {
                    name = "Đạo Sĩ",
                    element = ElementType.Moc,
                    elementHexColor = "#9B51E0",
                    description = "Đạo nhân tinh thông Tiên Đạo Bát Quái. Vận hành Cán Cân Âm Dương (Âm Thịnh / Dương Thịnh / Thái Cực).",
                    signatureSkillName = "Bát Quái Trận Đồ",
                    signatureSkillDesc = "Dậm chân tạo vùng Bát Quái làm chậm và gây sát thương yêu ma, ép Cán Cân Âm Dương về 50 (Thái Cực) trong 4s.",
                    passiveTraitName = "Cán Cân Âm Dương",
                    passiveTraitDesc = "Trạng thái Thái Cực (Cân bằng) tăng 25% Sát thương toàn thể và giảm 20% Sát thương nhận vào."
                },
                new CharacterInfo
                {
                    name = "Thanh Đồng",
                    element = ElementType.Moc,
                    elementHexColor = "#4C7A3D",
                    description = "Cô Đồng / Thầy Pháp Đạo Mẫu Tứ Phủ (Thiên, Nhạc, Thoải, Địa). Tay mang Chuỗi Linh Phù Tứ Phủ hộ thân trừ tà.",
                    signatureSkillName = "Giá Đồng Tứ Phủ",
                    signatureSkillDesc = "Thỉnh nhập Thánh thần Tứ Phủ ban hào quang 4 cõi (Tăng công / Tăng tốc / Giảm hồi chiêu / Giáp hộ thân) trong 5s.",
                    passiveTraitName = "Linh Lực Tứ Phủ",
                    passiveTraitDesc = "Thu thập Linh Khí tích lũy thanh Linh Lực Tứ Phủ. Khi kích hoạt Giá Đồng, nhận đồng thời hiệu ứng hộ trì của cả 4 cõi thần linh."
                },
                new CharacterInfo
                {
                    name = "Ẩn Sĩ Sơn Lâm",
                    element = ElementType.Tho,
                    elementHexColor = "#8A6A3E",
                    description = "Kỳ nhân tự tu nội lực chốn thâm sơn, hòa hợp làm một với núi rừng bản địa. Dồn lực bộc phát địa khí.",
                    signatureSkillName = "Thập Phương Chấn Thế",
                    signatureSkillDesc = "Trừ 30% HP hiện tại bộc phát địa khí chấn nứt đất đá, gây sát thương + Choáng 1.2s và đẩy lùi 8m/s.",
                    passiveTraitName = "Bàn Thạch Chi Khu",
                    passiveTraitDesc = "Máu càng thấp thủ càng cao. Khi HP dưới 50%, nhận thêm 30% Kháng sát thương và miễn nhiễm Đẩy lùi."
                }
            };
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
