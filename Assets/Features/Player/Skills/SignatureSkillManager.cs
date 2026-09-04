using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Player.Skills
{
    /// <summary>
    /// Manager điều khiển Kỹ năng Chủ động (Signature Skill) trên Player.
    /// Quản lý thời gian hồi chiêu, kiểm tra điều kiện thi triển và kích hoạt sự kiện cho Presenter.
    /// Sử dụng ScriptableObject Factory Pattern (Open/Closed Principle).
    /// </summary>
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(HealthSystem))]
    public class SignatureSkillManager : MonoBehaviour
    {
        [Header("Skill Configuration (ScriptableObject Factory)")]
        [SerializeField] private SignatureSkillData _skillData;

        public SignatureSkillData SkillData => _skillData;
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

            if (_skillData != null)
            {
                InitializeSkill(_skillData);
            }
        }

        /// <summary>
        /// Khởi tạo skill bằng ScriptableObject Factory (Chuẩn OCP - Không sử dụng switch/case).
        /// </summary>
        public void InitializeSkill(SignatureSkillData skillData)
        {
            _skillData = skillData;
            if (_skillData != null)
            {
                ActiveSkill = _skillData.CreateSkill();
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

            try
            {
                ActiveSkill.Execute(gameObject, onElementSelectedCallback);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SignatureSkillManager] Ngoại lệ khi thực thi {ActiveSkill.GetType().Name}: {ex}");
            }

            global::Core.Audio.AudioManager.Instance?.PlayUltimateSkillCast(transform.position);

            RemainingCooldown = ActiveSkill.Cooldown;
            Debug.Log($"<color=#00FF88>[SignatureSkillManager]</color> Đã thi triển {ActiveSkill.GetType().Name}. Bắt đầu hồi chiêu: {RemainingCooldown:F1}s.");

            OnSkillExecuted?.Invoke();
            OnCooldownUpdated?.Invoke(RemainingCooldown, MaxCooldown);

            return true;
        }

        /// <summary>
        /// Giảm thời gian hồi chiêu hiện tại (dành cho Buff/Passive).
        /// </summary>
        public void ReduceCooldown(float seconds)
        {
            if (RemainingCooldown > 0f)
            {
                RemainingCooldown = Mathf.Max(0f, RemainingCooldown - seconds);
                if (RemainingCooldown <= 0f)
                {
                    RemainingCooldown = 0f;
                    OnSkillReady?.Invoke();
                }
                OnCooldownUpdated?.Invoke(RemainingCooldown, MaxCooldown);
            }
        }

        /// <summary>
        /// Làm mới ngay lập tức hồi chiêu của kỹ năng.
        /// </summary>
        public void ResetCooldown()
        {
            RemainingCooldown = 0f;
            OnSkillReady?.Invoke();
            OnCooldownUpdated?.Invoke(0f, MaxCooldown);
        }
    }
}
