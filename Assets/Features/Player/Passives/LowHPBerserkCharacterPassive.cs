using ProjectZombie.Features.Shared;
using UnityEngine;

namespace ProjectZombie.Features.Player.Passives
{
    /// <summary>
    /// Nội tại Cuồng Hăng khi máu thấp (Low HP Berserk Passive).
    /// Khi Máu xuống dưới ngưỡng threshold (VD: 30%), nhân vật nhận được thêm sát thương/tốc chạy.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBerserkPassive", menuName = "ProjectZombie/Character Passives/Low HP Berserk Passive")]
    public class LowHPBerserkCharacterPassive : CharacterPassiveData
    {
        [Header("Berserk Trigger Conditions")]
        [Tooltip("Ngưỡng máu kích hoạt (0.3 = 30%)")]
        [Range(0.05f, 0.9f)]
        public float healthThreshold = 0.3f;

        [Header("Berserk Buffs")]
        [Tooltip("Tỉ lệ sát thương tăng thêm khi Cuồng Hăng (0.5 = +50% Sát thương)")]
        public float bonusDamageMultiplier = 0.5f;

        [Tooltip("Tỉ lệ tốc chạy tăng thêm khi Cuồng Hăng (0.2 = +20% Speed)")]
        public float bonusMoveSpeedMultiplier = 0.2f;

        public override void ApplyPassive(GameObject player)
        {
            if (player == null) return;

            // Gắn component Berserk Logic vào Player để tự động theo dõi sự kiện Máu
            var berserkComponent = player.AddComponent<BerserkPassiveLogic>();
            berserkComponent.Setup(healthThreshold, bonusDamageMultiplier, bonusMoveSpeedMultiplier);

            Debug.Log($"[CharacterPassive] Applied LowHPBerserkPassive: {traitName} on {player.name}");
        }
    }

    /// <summary>
    /// Component Logic tự động kiểm tra Máu và điều chỉnh buff cho Player.
    /// </summary>
    public class BerserkPassiveLogic : MonoBehaviour
    {
        private float _threshold;
        private float _damageBuff;
        private float _speedBuff;

        private HealthSystem _healthSystem;
        private PlayerStats _playerStats;

        private bool _isBerserkActive = false;
        private float _addedDamage = 0f;
        private float _addedSpeed = 0f;

        public void Setup(float threshold, float damageBuff, float speedBuff)
        {
            _threshold = threshold;
            _damageBuff = damageBuff;
            _speedBuff = speedBuff;
        }

        private void Awake()
        {
            _healthSystem = GetComponent<HealthSystem>();
            _playerStats = GetComponent<PlayerStats>();
        }

        private void Update()
        {
            if (_healthSystem == null || _playerStats == null) return;

            float currentHealthRatio = _healthSystem.CurrentHealth / _healthSystem.MaxHealth;

            if (currentHealthRatio <= _threshold && !_isBerserkActive)
            {
                // Kích hoạt Berserk
                _isBerserkActive = true;
                _addedDamage = _playerStats.BaseDamage * _damageBuff;
                _addedSpeed = _playerStats.MoveSpeed * _speedBuff;

                _playerStats.AddBaseDamage(_addedDamage);
                _playerStats.AddMoveSpeed(_addedSpeed);

                Debug.Log($"[BerserkPassiveLogic] Berserk ACTIVATED! (+{_addedDamage} Dmg, +{_addedSpeed} Speed)");
            }
            else if (currentHealthRatio > _threshold && _isBerserkActive)
            {
                // Hủy Berserk khi được hồi máu trên ngưỡng
                _isBerserkActive = false;
                _playerStats.AddBaseDamage(-_addedDamage);
                _playerStats.AddMoveSpeed(-_addedSpeed);

                Debug.Log("[BerserkPassiveLogic] Berserk DEACTIVATED!");
            }
        }
    }
}
