using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectZombie.Features.UI.StatsAndSkills
{
    public class StatUIEntry : MonoBehaviour
    {
        [SerializeField] private Image _statIcon;
        [SerializeField] private TextMeshProUGUI _statNameText;
        [SerializeField] private TextMeshProUGUI _statValueText;

        public void Setup(string statName, string value, Sprite icon = null)
        {
            if (_statNameText != null) _statNameText.text = statName;
            if (_statValueText != null) _statValueText.text = value;
            
            if (_statIcon != null)
            {
                if (icon != null)
                {
                    _statIcon.sprite = icon;
                    _statIcon.gameObject.SetActive(true);
                }
                else
                {
                    _statIcon.gameObject.SetActive(false);
                }
            }
        }
    }
}
