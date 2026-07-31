using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Player.Skills
{
    public enum CharacterType
    {
        ThuSinh,
        DaoSi,
        VoTang
    }

    /// <summary>
    /// Manager điều khiển Kỹ năng Chủ động (Signature Skill) trên Player.
    /// Quản lý thời gian hồi chiêu, kiểm tra điều kiện thi triển và kích hoạt sự kiện cho Presenter.
    /// </summary>
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(HealthSystem))]
    public class SignatureSkillManager : MonoBehaviour
    {
        [Header("Character Selection")]
        [SerializeField] private CharacterType _characterType = CharacterType.ThuSinh;

        [Header("Optional Prefab Overrides")]
        [SerializeField] private GameObject _batQuaiTranZonePrefab;

        public CharacterType CharacterType => _characterType;
        public ISignatureSkill ActiveSkill { get; private set; }

        public float RemainingCooldown { get; private set; }
        public float MaxCooldown => ActiveSkill != null ? ActiveSkill.Cooldown : 1f;
        public bool IsReady => RemainingCooldown <= 0f && CanExecuteCurrentSkill();

        public event System.Action<float, float> OnCooldownUpdated;
        public event System.Action OnSkillReady;
        public event System.Action OnSkillExecuted;

        private PlayerStats _playerStats;
        private HealthSystem _healthSystem;

        private void Awake()
        {
            _playerStats = GetComponent<PlayerStats>();
            _healthSystem = GetComponent<HealthSystem>();
            InitializeSkill(_characterType);
        }

        public void InitializeSkill(CharacterType type)
        {
            _characterType = type;
            switch (type)
            {
                case CharacterType.ThuSinh:
                    ActiveSkill = new ThuSinhSignatureSkill();
                    break;
                case CharacterType.DaoSi:
                    ActiveSkill = new DaoSiSignatureSkill(_batQuaiTranZonePrefab);
                    break;
                case CharacterType.VoTang:
                    ActiveSkill = new VoTangSignatureSkill();
                    break;
            }

            RemainingCooldown = 0f;
            OnSkillReady?.Invoke();
        }

        private void Update()
        {
            if (ActiveSkill == null) return;

            ActiveSkill.Tick(Time.deltaTime);

            if (RemainingCooldown > 0f)
            {
                RemainingCooldown -= Time.deltaTime;
                if (RemainingCooldown <= 0f)
                {
                    RemainingCooldown = 0f;
                    OnSkillReady?.Invoke();
                }
                OnCooldownUpdated?.Invoke(RemainingCooldown, MaxCooldown);
            }
        }

        public bool CanExecuteCurrentSkill()
        {
            if (ActiveSkill == null) return false;
            return ActiveSkill.CanExecute(_playerStats, _healthSystem);
        }

        /// <summary>
        /// Kích hoạt thi triển Signature Skill.
        /// </summary>
        public bool TryExecuteSkill(System.Action<ElementType> onElementSelectedCallback = null)
        {
            if (ActiveSkill == null || RemainingCooldown > 0f) return false;
            if (!CanExecuteCurrentSkill()) return false;

            ActiveSkill.Execute(gameObject, onElementSelectedCallback);

            RemainingCooldown = ActiveSkill.Cooldown;
            OnSkillExecuted?.Invoke();
            OnCooldownUpdated?.Invoke(RemainingCooldown, MaxCooldown);

            return true;
        }
    }
}
