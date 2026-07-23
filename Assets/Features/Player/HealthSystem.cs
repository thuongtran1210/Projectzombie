using System;
using UnityEngine;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Component quản lý máu, có thể dùng cho cả Player, Enemy và các vật thể phá hủy được.
    /// Tuân thủ nguyên tắc Single Responsibility (S).
    /// </summary>
    public class HealthSystem : MonoBehaviour, IDamageable, IHealable
    {
        [Header("Settings")]
        [Tooltip("Giá trị này sẽ bị ghi đè nếu Object có gắn PlayerStats hoặc EnemyAI (sử dụng cấu hình từ ScriptableObject)")]
        [SerializeField] private float maxHealth = 100f;

        private float _currentHealth;

        // Bắn sự kiện ra ngoài (Event-Driven) thay vì tự gọi script khác
        public event Action<float, float> OnHealthChanged;
        public event Action OnDied;

        // Trả về true nếu cái chết bị chặn (ví dụ: kích hoạt hồi sinh)
        public delegate bool TryDieHandler();
        public event TryDieHandler OnTryDie;

        public float CurrentHealth => _currentHealth;

        private void OnEnable()
        {
            _currentHealth = maxHealth;
        }

        public void SetMaxHealth(float newMaxHealth, bool fillCurrentHealth = true)
        {
            maxHealth = newMaxHealth;
            if (fillCurrentHealth)
            {
                _currentHealth = maxHealth;
            }
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        /// <summary>
        /// Nhân HP tối đa với hệ số scale (dùng bởi EnemySpawner cho Difficulty Scaling).
        /// Cũng scale HP hiện tại theo cùng tỷ lệ để giữ %.
        /// </summary>
        public void ScaleMaxHealth(float multiplier)
        {
            float ratio = _currentHealth / Mathf.Max(maxHealth, 0.001f);
            maxHealth *= multiplier;
            _currentHealth = maxHealth * ratio;
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        public void TakeDamage(float amount)
        {
            if (_currentHealth <= 0) return; 

            _currentHealth -= amount;
            _currentHealth = Mathf.Max(_currentHealth, 0f);

            OnHealthChanged?.Invoke(_currentHealth, maxHealth);

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        public void TakeDamage(DamageData damageData)
        {
            // Tương lai có thể thêm logic text chí mạng (Floating Text) ở đây
            TakeDamage(damageData.Amount);
        }

        public void TakeDamage(DamageContext context)
        {
            // Tương lai Combat System sẽ tính toán các yếu tố phức tạp ở đây:
            // Crit, Buff, Debuff, Element...
            TakeDamage(context.BaseDamage);
        }

        public void Heal(float amount, bool allowRevive = false)
        {
            if (_currentHealth <= 0 && !allowRevive) return; // Không thể hồi sinh bằng hàm này
            
            _currentHealth += amount;
            _currentHealth = Mathf.Min(_currentHealth, maxHealth);
            
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        private void Die()
        {
            if (OnTryDie != null)
            {
                foreach (TryDieHandler handler in OnTryDie.GetInvocationList())
                {
                    if (handler.Invoke())
                    {
                        return; // Bị chặn cái chết
                    }
                }
            }

            OnDied?.Invoke();
            // Tạm thời disable object, có thể gọi Object Pooling sau
            gameObject.SetActive(false);
        }
    }
}
