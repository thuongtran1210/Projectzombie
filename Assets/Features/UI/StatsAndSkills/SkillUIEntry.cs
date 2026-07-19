using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using ProjectZombie.Features.Upgrades;

namespace ProjectZombie.Features.UI.StatsAndSkills
{
    public class SkillUIEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _skillIcon;
        [SerializeField] private TextMeshProUGUI _levelText;

        private string _skillName;
        private string _skillDescription;
        private TooltipUI _tooltip;

        public void Setup(Sprite icon, int level, string name, string description, TooltipUI tooltip)
        {
            if (_skillIcon != null) _skillIcon.sprite = icon;
            if (_levelText != null) _levelText.text = $"Lv.{level}";
            
            _skillName = name;
            _skillDescription = description;
            _tooltip = tooltip;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_tooltip != null)
            {
                _tooltip.Show(_skillName, _skillDescription);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_tooltip != null)
            {
                _tooltip.Hide();
            }
        }
    }
}
