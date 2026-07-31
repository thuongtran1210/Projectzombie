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
                    description = "Phong thái nho nhã, cầm bút phán quyết sinh tử. Thiên về dồn đòn Tương Sinh liên hoàn.",
                    signatureSkillName = "Phán Quyết Tiền Định",
                    signatureSkillDesc = "Chèn 1 hit ảo Ngũ Hành vào Queue Tương Sinh, kích hoạt giảm 20% Cooldown cho vũ khí khớp lệnh."
                },
                new CharacterInfo
                {
                    name = "Đạo Sĩ",
                    element = ElementType.Moc,
                    elementHexColor = "#32CD32",
                    description = "Tinh thông Bát Quái âm dương. Có khả năng ép cán cân Âm Dương về Thái Cực Cân Bằng.",
                    signatureSkillName = "Bát Quái Trận Đồ",
                    signatureSkillDesc = "Nhốt quái trong vùng Bát Quái 4s và ép Âm Dương về 50 để mở cửa sổ chọn thẻ Thái Cực đặc biệt."
                },
                new CharacterInfo
                {
                    name = "Võ Tăng",
                    element = ElementType.Tho,
                    elementHexColor = "#8B4513",
                    description = "Thân thể kim cang, dồn lực phá giới. Đổi máu lấy chấn động sát thương diện rộng.",
                    signatureSkillName = "Phá Giới Chấn Thế",
                    signatureSkillDesc = "Trừ 30% HP hiện tại tạo sóng chấn động gây sát thương + Choáng 1.2s, đẩy thẳng +25 vào cực Dương."
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
