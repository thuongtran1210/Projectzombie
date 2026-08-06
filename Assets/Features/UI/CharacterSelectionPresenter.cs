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
        public Sprite avatar;
    }

    /// <summary>
    /// Presenter điều phối dữ liệu chọn nhân vật giữa Model và CharacterSelectionView.
    /// </summary>
    public class CharacterSelectionPresenter : MonoBehaviour
    {
        [SerializeField] private CharacterSelectionView _view;

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
            _characters = new CharacterInfo[]
            {
                new CharacterInfo
                {
                    name = "Thư Sinh",
                    element = ElementType.Kim,
                    elementHexColor = "#FFD700",
                    description = "Được anh linh liệt tổ & Đức Thánh Trần điểm hóa. Tay cầm bút lệnh khí thiêng sông núi phán định tà ma.",
                    signatureSkillName = "Phán Quyết Tiền Định",
                    signatureSkillDesc = "Chèn 1 hit ảo Ngũ Hành vào Queue Tương Sinh, kích hoạt giảm 20% Cooldown cho vũ khí khớp lệnh."
                },
                new CharacterInfo
                {
                    name = "Thanh Đồng",
                    element = ElementType.Moc,
                    elementHexColor = "#4C7A3D",
                    description = "Thầy Pháp / Bà Đồng thỉnh nhập Thánh thần Tứ Phủ (Thiên, Nhạc, Thoải, Địa Phủ). Ép cán cân về Thái Cực.",
                    signatureSkillName = "Giá Đồng",
                    signatureSkillDesc = "Thỉnh nhập Tứ Phủ ban hào quang & buff sắc phục 5s, ép Âm Dương về 50 mở cơ hội chọn thẻ Evolution Thái Cực."
                },
                new CharacterInfo
                {
                    name = "Ẩn Sĩ Sơn Lâm",
                    element = ElementType.Tho,
                    elementHexColor = "#8A6A3E",
                    description = "Kỳ nhân tự tu nội lực chốn thâm sơn, hòa hợp làm một với núi rừng bản địa. Dồn lực bộc phát địa khí.",
                    signatureSkillName = "Thập Phương Chấn Thế",
                    signatureSkillDesc = "Trừ 30% HP hiện tại bộc phát địa khí chấn nứt đất đá, gây sát thương + Choáng 1.2s và đẩy thẳng +25 vào cực Dương."
                }
            };
        }

        private void OnNextCharacter()
        {
            _currentIndex = (_currentIndex + 1) % _characters.Length;
            RenderCurrentCharacter();
        }

        private void OnPrevCharacter()
        {
            _currentIndex = (_currentIndex - 1 + _characters.Length) % _characters.Length;
            RenderCurrentCharacter();
        }

        private void OnSelectCharacter()
        {
            var selected = _characters[_currentIndex];
            Debug.Log($"[{nameof(CharacterSelectionPresenter)}] Đã chọn Anh Hùng: {selected.name} (Hệ {selected.element})");
            // Có thể load Scene Gameplay hoặc lưu thông tin nhân vật vào GameManager tại đây
        }

        private void RenderCurrentCharacter()
        {
            if (_view == null || _characters == null || _characters.Length == 0) return;

            var charInfo = _characters[_currentIndex];
            string formattedElement = $"<color={charInfo.elementHexColor}>✦ Hệ {charInfo.element}</color>";
            string formattedSkill = $"<b>{charInfo.signatureSkillName}</b>: {charInfo.signatureSkillDesc}";

            _view.DisplayCharacter(charInfo.name, formattedElement, charInfo.description, formattedSkill, charInfo.avatar);
        }
    }
}
