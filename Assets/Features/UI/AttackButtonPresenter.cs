using UnityEngine;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Presenter điều phối giữa WeaponManager / WeaponBase (Model) và AttackButtonView (View).
    /// Tuân thủ kiến trúc MVP: Cập nhật icon vũ khí chính, thanh hồi chiêu và kích hoạt đánh chủ động (Active Attack).
    /// </summary>
    public class AttackButtonPresenter : MonoBehaviour
    {
        [Header("View Reference")]
        [SerializeField] private AttackButtonView _view;

        [Header("Model Reference")]
        [SerializeField] private WeaponManager _weaponManager;

        private void Awake()
        {
            if (_view == null) _view = GetComponent<AttackButtonView>();
        }

        private void Start()
        {
            if (_view != null)
            {
                _view.OnButtonPressed += OnAttackButtonPressed;
            }

            TryBindWeaponManager();
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.OnButtonPressed -= OnAttackButtonPressed;
            }

            if (_weaponManager != null)
            {
                _weaponManager.OnWeaponsChanged -= OnWeaponsChanged;
            }
        }

        public void Bind(WeaponManager weaponManager)
        {
            if (_weaponManager != null)
            {
                _weaponManager.OnWeaponsChanged -= OnWeaponsChanged;
            }

            _weaponManager = weaponManager;

            if (_weaponManager != null)
            {
                _weaponManager.OnWeaponsChanged += OnWeaponsChanged;
                OnWeaponsChanged();
            }
        }

        private void TryBindWeaponManager()
        {
            if (_weaponManager == null && PlayerController.Instance != null)
            {
                var wm = PlayerController.Instance.GetComponent<WeaponManager>();
                if (wm != null)
                {
                    Bind(wm);
                }
            }
        }

        private float _bufferedAttackTime;
        private const float TAP_BUFFER_WINDOW = 0.18f;

        private void Update()
        {
            if (_weaponManager == null)
            {
                TryBindWeaponManager();
                return;
            }

            WeaponBase primaryWeapon = _weaponManager.PrimaryWeapon;
            if (primaryWeapon != null)
            {
                float remainingCd = primaryWeapon.RemainingCooldown;
                float totalAttackSpeed = primaryWeapon.GetTotalAttackSpeed();
                float maxCd = 1f / Mathf.Max(0.01f, totalAttackSpeed);

                _view.SetCooldown(remainingCd, maxCd);
                _view.SetInteractable(true); // Luôn cho phép chạm để ăn Tap Buffer

                // Xử lý Tap Buffer: Tự động xả đòn tiếp theo nếu vừa bấm trong buffer window
                if (_bufferedAttackTime > 0 && Time.time <= _bufferedAttackTime + TAP_BUFFER_WINDOW)
                {
                    if (_weaponManager.TriggerPrimaryAttack())
                    {
                        _bufferedAttackTime = 0f;
                    }
                }
            }
            else
            {
                _view.SetCooldown(0f, 1f);
                _view.SetInteractable(false);
            }
        }

        private void OnWeaponsChanged()
        {
            if (_weaponManager != null && _view != null)
            {
                WeaponBase primaryWeapon = _weaponManager.PrimaryWeapon;
                if (primaryWeapon != null && primaryWeapon.icon != null)
                {
                    _view.SetIcon(primaryWeapon.icon);
                }
            }
        }

        private void OnAttackButtonPressed()
        {
            if (_weaponManager != null)
            {
                if (!_weaponManager.TriggerPrimaryAttack())
                {
                    // Nếu vũ khí chưa kịp hồi xong, lưu vào Tap Buffer
                    _bufferedAttackTime = Time.time;
                }
                else
                {
                    _bufferedAttackTime = 0f;
                }
            }
        }
    }
}
