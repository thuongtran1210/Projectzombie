using UnityEngine;
using ProjectZombie.Core;
using ProjectZombie.Features.Enemies;

namespace ProjectZombie.Features.Enemies.Passives
{
    public class FactionPassive_Cult : MonoBehaviour
    {
        [Header("Cult Aura Settings")]
        public float auraRadius = 5f;
        public float damageMultiplierBuff = 0.5f; // +50% sát thương
        public LayerMask enemyLayer;
        
        private static readonly Collider2D[] _hitBuffer = new Collider2D[20];

        private void Awake()
        {
            if (enemyLayer == 0) enemyLayer = LayerMask.GetMask("Enemy");
        }

        private void OnEnable()
        {
            TickManager.OnTick += ApplyAura;
        }

        private void OnDisable()
        {
            TickManager.OnTick -= ApplyAura;
        }

        private void ApplyAura()
        {
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, auraRadius, _hitBuffer, enemyLayer);

            for (int i = 0; i < count; i++)
            {
                var hit = _hitBuffer[i];
                if (hit.gameObject != gameObject)
                {
                    var cultTarget = hit.GetComponent<CultAuraReceiver>();
                    if (cultTarget == null)
                    {
                        cultTarget = hit.gameObject.AddComponent<CultAuraReceiver>();
                    }
                    cultTarget.RefreshAura(damageMultiplierBuff);
                }
            }
        }
    }

    /// <summary>
    /// Component hỗ trợ tự động gắn vào quái vật đồng minh để quản lý thời gian tồn tại của Aura.
    /// Giúp tránh việc cộng dồn Aura vô hạn hoặc lỗi khi quái Cult bị tiêu diệt.
    /// </summary>
    public class CultAuraReceiver : MonoBehaviour
    {
        private Enemy _enemy;
        private float _expirationTime;
        private bool _isActive;
        private float _buffAmount;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
        }

        public void RefreshAura(float amount)
        {
            _buffAmount = amount;
            _expirationTime = Time.time + 0.6f; // Hết hạn sau 0.6s nếu không được refresh (chu kỳ 0.5s)
            
            if (!_isActive && _enemy != null)
            {
                _isActive = true;
                _enemy.DamageMultiplier += _buffAmount;
            }
        }

        private void Update()
        {
            if (_isActive && Time.time > _expirationTime)
            {
                RemoveBuff();
            }
        }

        private void OnDisable()
        {
            RemoveBuff();
        }

        private void RemoveBuff()
        {
            if (_isActive && _enemy != null)
            {
                _isActive = false;
                _enemy.DamageMultiplier -= _buffAmount;
            }
        }
    }
}
