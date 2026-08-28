using UnityEngine;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Presenter điều phối giữa CharacterCombat / WeaponManager (Model) và AttackButtonView (View).
    /// Tuân thủ kiến trúc MVP: Cập nhật icon đòn đánh của nhân vật, thanh hồi chiêu và kích hoạt đánh chủ động (Active Attack).
    /// </summary>
    public class AttackButtonPresenter : MonoBehaviour
    {
        [Header("View Reference")]
        [SerializeField] private AttackButtonView _view;

        [Header("Model References")]
        [SerializeField] private CharacterCombat _characterCombat;
        [SerializeField] private WeaponManager _weaponManager;

        private float _bufferedAttackTime;
        private const float TAP_BUFFER_WINDOW = 0.18f;

        private void Awake()
        {
            if (_view == null) _view = GetComponent<AttackButtonView>();
        }

        private void Start()
        {
            if (_view != null)
            {
                _view.OnButtonPressed += OnAttackButtonPressed;
                _view.OnAimStarted += HandleAimStarted;
                _view.OnAimUpdated += HandleAimUpdated;
                _view.OnAimReleased += HandleAimReleased;
                _view.OnAimCancelled += HandleAimCancelled;
            }

            TryBindCombat();
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.OnButtonPressed -= OnAttackButtonPressed;
                _view.OnAimStarted -= HandleAimStarted;
                _view.OnAimUpdated -= HandleAimUpdated;
                _view.OnAimReleased -= HandleAimReleased;
                _view.OnAimCancelled -= HandleAimCancelled;
            }

            if (_weaponManager != null)
            {
                _weaponManager.OnWeaponsChanged -= OnWeaponsChanged;
            }
        }

        private void HandleAimStarted()
        {
            if (_characterCombat == null && PlayerController.Instance != null)
            {
                _characterCombat = PlayerController.Instance.GetComponent<CharacterCombat>();
            }

            var config = _characterCombat != null ? _characterCombat.AimConfig : Combat.Aiming.SkillAimConfig.DefaultMelee;
            Combat.Aiming.SkillAimIndicatorController.Instance?.StartAim(config);
        }

        private void HandleAimUpdated(Vector2 direction, float pullPercent, bool isCancel)
        {
            Combat.Aiming.SkillAimIndicatorController.Instance?.UpdateAim(direction, pullPercent, isCancel);
        }

        private void HandleAimReleased(Vector2 direction, bool isQuickTap)
        {
            Combat.Aiming.SkillAimIndicatorController.Instance?.StopAim();
            if (isQuickTap)
            {
                OnAttackButtonPressed();
            }
            else
            {
                if (_characterCombat != null)
                {
                    _characterCombat.TriggerAttack(direction);
                }
            }
        }

        private void HandleAimCancelled()
        {
            Combat.Aiming.SkillAimIndicatorController.Instance?.StopAim();
        }

        public void Bind(CharacterCombat combat)
        {
            _characterCombat = combat;
            UpdateVisuals();
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

        private void TryBindCombat()
        {
            if (PlayerController.Instance != null)
            {
                if (_characterCombat == null)
                {
                    _characterCombat = PlayerController.Instance.GetComponent<CharacterCombat>();
                }

                if (_weaponManager == null)
                {
                    var wm = PlayerController.Instance.GetComponent<WeaponManager>();
                    if (wm != null) Bind(wm);
                }

                UpdateVisuals();
            }
        }

        private void Update()
        {
            if (_characterCombat == null && _weaponManager == null)
            {
                TryBindCombat();
                return;
            }

            // 1. Ưu tiên đòn đánh nhân vật (CharacterCombat)
            if (_characterCombat != null)
            {
                float remainingCd = _characterCombat.RemainingCooldown;
                float totalAttackSpeed = _characterCombat.GetTotalAttackSpeed();
                float maxCd = 1f / Mathf.Max(0.01f, totalAttackSpeed);

                _view.SetCooldown(remainingCd, maxCd);
                _view.SetInteractable(true);

                // Hỗ trợ phím bấm trực tiếp trên PC (Chuột trái Mouse0, Phím J, hoặc Phím K)
                bool pcAttackPressed = false;
#if ENABLE_INPUT_SYSTEM
                if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame) pcAttackPressed = true;
                if (UnityEngine.InputSystem.Keyboard.current != null && (UnityEngine.InputSystem.Keyboard.current.jKey.wasPressedThisFrame || UnityEngine.InputSystem.Keyboard.current.kKey.wasPressedThisFrame)) pcAttackPressed = true;
#endif
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.K)) pcAttackPressed = true;

                if (pcAttackPressed)
                {
                    OnAttackButtonPressed();
                }

                if (_bufferedAttackTime > 0 && Time.time <= _bufferedAttackTime + TAP_BUFFER_WINDOW)
                {
                    if (_characterCombat.TriggerAttack())
                    {
                        _bufferedAttackTime = 0f;
                    }
                }
            }
            // 2. Fallback sang vũ khí chính cũ nếu chưa có CharacterCombat
            else if (_weaponManager != null && _weaponManager.PrimaryWeapon != null)
            {
                WeaponBase primaryWeapon = _weaponManager.PrimaryWeapon;
                float remainingCd = primaryWeapon.RemainingCooldown;
                float totalAttackSpeed = primaryWeapon.GetTotalAttackSpeed();
                float maxCd = 1f / Mathf.Max(0.01f, totalAttackSpeed);

                _view.SetCooldown(remainingCd, maxCd);
                _view.SetInteractable(true);

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

        private void UpdateVisuals()
        {
            if (_view == null) return;

            Sprite iconToSet = null;

            // 1. Ưu tiên tuyệt đối đòn đánh cơ bản của nhân vật (Character Signature Basic Attack)
            if (_characterCombat != null && _characterCombat.AttackIcon != null)
            {
                iconToSet = _characterCombat.AttackIcon;
            }
            else if (RunLoadoutState.SelectedCharacter != null && RunLoadoutState.SelectedCharacter.basicAttackConfig != null && RunLoadoutState.SelectedCharacter.basicAttackConfig.attackIcon != null)
            {
                iconToSet = RunLoadoutState.SelectedCharacter.basicAttackConfig.attackIcon;
            }
            else if (RunLoadoutState.SelectedCharacter != null && RunLoadoutState.SelectedCharacter.avatar != null)
            {
                iconToSet = RunLoadoutState.SelectedCharacter.avatar;
            }

            if (iconToSet != null)
            {
                _view.SetIcon(iconToSet);
            }
        }

        private void OnWeaponsChanged()
        {
            UpdateVisuals();
        }

        private void OnAttackButtonPressed()
        {
            if (_characterCombat != null)
            {
                if (!_characterCombat.TriggerAttack())
                {
                    _bufferedAttackTime = Time.time;
                }
                else
                {
                    _bufferedAttackTime = 0f;
                }
                return;
            }

            if (_weaponManager != null)
            {
                if (!_weaponManager.TriggerPrimaryAttack())
                {
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
