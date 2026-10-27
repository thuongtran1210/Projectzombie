using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>Passive authored skin. Presenter supplies already formatted text and cooldown fill.</summary>
    public sealed class DefenseSkillButtonView : MonoBehaviour
    {
        [SerializeField] private Image _cooldownFill;
        [SerializeField] private TMP_Text _cooldownText;
        [SerializeField] private GameObject _lockedOverlay;
        public bool IsConfigured => _cooldownFill != null && _cooldownText != null && _lockedOverlay != null;

        public void Render(float fill, string label, bool locked)
        {
            if (!IsConfigured) return;
            _cooldownFill.fillAmount = Mathf.Clamp01(fill);
            if (_cooldownText.text != label) _cooldownText.SetText(label);
            if (_lockedOverlay.activeSelf != locked) _lockedOverlay.SetActive(locked);
        }
    }
}
