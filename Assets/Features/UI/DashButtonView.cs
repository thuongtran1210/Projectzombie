using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Passive View quản lý Nút bấm Lướt (Dash Button UI) trên di động.
    /// Gắn cùng Panel_MobileControls bên cạnh Nút Signature Skill.
    /// </summary>
    public class DashButtonView : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Button _dashButton;
        [SerializeField] private Image _cooldownRadialFill;
        [SerializeField] private TextMeshProUGUI _cooldownText;

        private PlayerStats _playerStats;

        private void Awake()
        {
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }

            if (_dashButton == null) _dashButton = GetComponent<Button>();
            if (_dashButton != null)
            {
                _dashButton.onClick.AddListener(OnDashClicked);
            }
        }

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerStats = player.GetComponent<PlayerStats>();
            }
        }

        private void Update()
        {
            if (_playerStats == null)
            {
                if (PlayerController.Instance != null)
                {
                    _playerStats = PlayerController.Instance.GetComponent<PlayerStats>();
                }
                return;
            }

            // Tính toán cooldown lướt từ PlayerStats & PlayerController
            float lastDash = PlayerController.Instance != null ? PlayerController.Instance.LastDashTime : 0f;
            float cooldown = _playerStats.DashCooldown;
            float timePassed = Time.time - lastDash;
            float remaining = Mathf.Max(0f, cooldown - timePassed);

            if (_cooldownRadialFill != null)
            {
                _cooldownRadialFill.fillAmount = cooldown > 0f ? Mathf.Clamp01(remaining / cooldown) : 0f;
            }

            if (_cooldownText != null)
            {
                _cooldownText.text = remaining > 0f ? $"{remaining:F1}s" : "";
                _cooldownText.gameObject.SetActive(remaining > 0f);
            }

            if (_dashButton != null)
            {
                _dashButton.interactable = remaining <= 0f;
            }
        }

        private void OnDashClicked()
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.PerformDash();
            }
        }
    }
}
