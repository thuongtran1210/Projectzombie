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
        [SerializeField] private Image _aimBorder;
        [SerializeField] private GameObject _cancelGlow;
        public bool IsConfigured => _cooldownFill != null && _cooldownText != null && _lockedOverlay != null;

        public void Render(float fill, string label, bool locked)
        {
            if (!IsConfigured) return;
            _cooldownFill.fillAmount = Mathf.Clamp01(fill);
            if (_cooldownText.text != label) _cooldownText.SetText(label);
            if (_lockedOverlay.activeSelf != locked) _lockedOverlay.SetActive(locked);
        }

        public void SetAimFeedback(bool isAiming, bool isCancelled)
        {
            if (_aimBorder != null)
            {
                if (_aimBorder.gameObject.activeSelf != isAiming) _aimBorder.gameObject.SetActive(isAiming);
                if (isAiming) _aimBorder.color = isCancelled ? new Color(1f, 0.2f, 0.2f, 0.95f) : new Color(0.2f, 0.9f, 1f, 0.95f);
            }
            if (_cancelGlow != null && _cancelGlow.activeSelf != (isAiming && isCancelled))
            {
                _cancelGlow.SetActive(isAiming && isCancelled);
            }
        }
    }
}
