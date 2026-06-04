using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectZombie.Features.UI.StatsAndSkills
{
    public class StatUIEntry : MonoBehaviour
    {
        [SerializeField] private Image statIcon;
        [SerializeField] private TMP_Text statNameText;
        [SerializeField] private TMP_Text statValueText;

        public void Setup(string statName, string value, Sprite icon = null)
        {
            if (statNameText != null) statNameText.text = statName;
            if (statValueText != null) statValueText.text = value;
            
            if (statIcon != null)
            {
                if (icon != null)
                {
                    statIcon.sprite = icon;
                    statIcon.gameObject.SetActive(true);
                }
                else
                {
                    statIcon.gameObject.SetActive(false);
                }
            }
        }
    }
}
