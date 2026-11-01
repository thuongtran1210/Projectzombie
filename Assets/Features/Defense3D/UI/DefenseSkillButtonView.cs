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
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _stateLabel;
        [SerializeField] private Image _durationRing;
        private Color _aimColor = new Color(.2f, .9f, 1f, .95f);
        private bool _isAiming, _isCancelled;
        private string _status = string.Empty;

        /// <summary>Presenter supplies a skill palette; the view retains no gameplay state.</summary>
        public void SetAccent(Color accent) => _aimColor = accent;

        /// <summary>Displays already formatted hazard lifetime and gesture guidance.</summary>
        public void RenderStatus(string status, float durationFill)
        {
            _status = status;
            if (_durationRing != null)
            {
                _durationRing.fillAmount = Mathf.Clamp01(durationFill);
                _durationRing.enabled = durationFill > 0f;
            }
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            if (_stateLabel == null) return;
            string label = _isAiming ? (_isCancelled ? "THẢ ĐỂ HỦY" : "THẢ ĐỂ THỔI") : _status;
            if (_stateLabel.text != label) _stateLabel.SetText(label);
            _stateLabel.color = _isCancelled ? new Color(1f, .38f, .3f) : _aimColor;
        }

        public bool IsConfigured => _cooldownFill != null && _cooldownText != null && _lockedOverlay != null;

        private void Awake()
        {
            EnsureReferences();
        }

        private void EnsureReferences()
        {
            if (_iconImage == null)
            {
                var iconTrans = transform.Find("Icon");
                if (iconTrans != null)
                {
                    _iconImage = iconTrans.GetComponent<Image>();
                }
            }

            if (_nameLabel == null)
            {
                var labelTrans = transform.Find("Label");
                if (labelTrans != null)
                {
                    _nameLabel = labelTrans.GetComponent<TMP_Text>();
                }
            }
        }

        public void SetSkillIdentity(string skillName, Sprite icon = null)
        {
            EnsureReferences();
            if (_nameLabel != null && !string.IsNullOrEmpty(skillName))
            {
                _nameLabel.text = skillName.ToUpperInvariant();
            }

            if (_iconImage != null && icon != null)
            {
                _iconImage.sprite = icon;
                _iconImage.enabled = true;
            }
        }

        public void SetIcon(Sprite icon)
        {
            SetSkillIdentity(null, icon);
        }

        public void Render(float fill, string label, bool locked)
        {
            if (!IsConfigured) return;
            _cooldownFill.fillAmount = Mathf.Clamp01(fill);
            if (_cooldownText.text != label) _cooldownText.SetText(label);
            if (_lockedOverlay.activeSelf != locked) _lockedOverlay.SetActive(locked);
        }

        public void SetAimFeedback(bool isAiming, bool isCancelled)
        {
            _isAiming = isAiming;
            _isCancelled = isAiming && isCancelled;
            RefreshStatus();
            if (_aimBorder != null)
            {
                if (_aimBorder.gameObject.activeSelf != isAiming) _aimBorder.gameObject.SetActive(isAiming);
                if (isAiming) _aimBorder.color = isCancelled ? new Color(1f, 0.2f, 0.2f, 0.95f) : _aimColor;
            }
            if (_cancelGlow != null && _cancelGlow.activeSelf != (isAiming && isCancelled))
            {
                _cancelGlow.SetActive(isAiming && isCancelled);
            }
        }
    }
}
