using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Core.Juice;

namespace ProjectZombie.Features.Projectiles
{
    /// <summary>
    /// Component điều khiển đầu đạn Linh Phù Tiên Đạo của Đạo Sĩ (C002).
    /// Bay thẳng, xuyên phá hoặc nổ sát thương diện rộng và tạo hiệu ứng chấn động.
    /// Tích hợp Object Pooling 0 GC Allocations trong Gameplay Loop.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class SimpleProjectile : MonoBehaviour
    {
        [Header("Piercing Settings")]
        [SerializeField] private bool isPiercing = false;
        [SerializeField] private int maxPierceCount = 5;

        private DamageData _damageData;
        private GameObject _owner;
        private float _knockbackForce = 4.0f;
        private bool _hasHit;
        private int _currentPierceHits = 0;
        private readonly System.Collections.Generic.HashSet<int> _hitInstanceIds = new System.Collections.Generic.HashSet<int>();

        public void Initialize(DamageData damage, GameObject owner, float knockback)
        {
            _damageData = damage;
            _owner = owner;
            _knockbackForce = knockback;
            _hasHit = false;
            _currentPierceHits = 0;
            _hitInstanceIds.Clear();
        }

        public void SetPiercing(bool enablePiercing, int maxHits = 5)
        {
            isPiercing = enablePiercing;
            maxPierceCount = maxHits;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision == null || _hasHit) return;

            // Không tự va chạm với chính Player
            if (_owner != null && (collision.gameObject == _owner || collision.transform.root == _owner.transform.root))
            {
                return;
            }

            if (collision.gameObject.name == "[MagnetArea]") return;

            int targetId = collision.gameObject.GetInstanceID();
            if (_hitInstanceIds.Contains(targetId)) return;

            if (collision.TryGetComponent<HealthSystem>(out var health) && health.CurrentHealth > 0)
            {
                _hitInstanceIds.Add(targetId);

                ElementType defenderElement = ElementType.None;
                if (collision.TryGetComponent<Enemy>(out var enemy))
                {
                    defenderElement = enemy.CurrentElement;
                }

                DamageData hitDamage = DamageUtility.CalculateHitDamage(
                    _damageData.Amount,
                    _damageData.IsCritical,
                    _damageData.Element,
                    defenderElement,
                    null
                );

                health.TakeDamage(hitDamage);

                if (enemy != null && !enemy.IsHeavyArmor)
                {
                    Vector2 pushDir = ((Vector2)(collision.transform.position - transform.position)).normalized;
                    enemy.ApplyKnockback(pushDir, _knockbackForce, 0.15f);
                }

                // Game Feel
                if (_damageData.IsCritical)
                {
                    GameJuiceEvents.RequestCameraShake(0.12f, 0.12f);
                    GameJuiceEvents.RequestHitStop(0.04f);
                }
                else
                {
                    GameJuiceEvents.RequestCameraShake(0.05f, 0.04f);
                }

                // Spawn Hit Sparks từ Object Pool
                if (_hitSparksPrefab == null)
                {
                    _hitSparksPrefab = Resources.Load<GameObject>("Prefabs/VFX/PS_ImpactSparks");
                    if (_hitSparksPrefab == null)
                    {
                        _hitSparksPrefab = Resources.Load<GameObject>("PS_ImpactSparks");
                    }
                }

                if (_hitSparksPrefab != null)
                {
                    ProjectZombie.Core.Pooling.VFXPoolManager.SpawnVFX(_hitSparksPrefab, transform.position, Quaternion.identity, 0.5f);
                }

                // Nếu là đạn xuyên, chỉ hủy sau khi chạm đủ số lượng quái tối đa
                if (isPiercing)
                {
                    _currentPierceHits++;
                    if (_currentPierceHits >= maxPierceCount)
                    {
                        _hasHit = true;
                        Despawn();
                    }
                }
                else
                {
                    _hasHit = true;
                    Despawn();
                }
            }
        }

        private System.Collections.IEnumerator AutoDespawnRoutine(float lifetime)
        {
            yield return new WaitForSeconds(lifetime);
            Despawn();
        }

        private void Despawn()
        {
            if (!_isActiveInPool) return;
            _isActiveInPool = false;
            StopAllCoroutines();
            gameObject.SetActive(false);

            if (_ownerPool != null)
            {
                _ownerPool.Release(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #region STATIC OBJECT POOLING
        private bool _isActiveInPool;
        private UnityEngine.Pool.IObjectPool<SimpleProjectile> _ownerPool;
        private static readonly System.Collections.Generic.Dictionary<int, UnityEngine.Pool.IObjectPool<SimpleProjectile>> _poolDict = new();

        public static SimpleProjectile Spawn(GameObject prefab, Vector3 position, Quaternion rotation, DamageData damage, GameObject owner, float knockback, float speed, Vector2 direction, float lifetime)
        {
            if (prefab == null) return null;

            int key = prefab.GetInstanceID();
            if (!_poolDict.TryGetValue(key, out var pool))
            {
                pool = new UnityEngine.Pool.ObjectPool<SimpleProjectile>(
                    createFunc: () => {
                        GameObject obj = Instantiate(prefab);
                        SimpleProjectile sp = obj.GetComponent<SimpleProjectile>() ?? obj.AddComponent<SimpleProjectile>();
                        sp._ownerPool = _poolDict[key];
                        return sp;
                    },
                    actionOnGet: sp => {
                        if (sp != null)
                        {
                            sp.gameObject.SetActive(true);
                            sp._isActiveInPool = true;
                        }
                    },
                    actionOnRelease: sp => {
                        if (sp != null)
                        {
                            sp._isActiveInPool = false;
                            sp.gameObject.SetActive(false);
                        }
                    },
                    actionOnDestroy: sp => {
                        if (sp != null) Destroy(sp.gameObject);
                    },
                    collectionCheck: false,
                    defaultCapacity: 20,
                    maxSize: 100
                );
                _poolDict[key] = pool;
            }

            SimpleProjectile projectile = pool.Get();
            if (projectile != null)
            {
                projectile.transform.position = position;
                projectile.transform.rotation = rotation;
                projectile.Initialize(damage, owner, knockback);

                if (projectile.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    rb.velocity = direction * speed;
                }

                if (lifetime > 0f)
                {
                    projectile.StartCoroutine(projectile.AutoDespawnRoutine(lifetime));
                }
            }

            return projectile;
        }
        #endregion

        private static GameObject _hitSparksPrefab;
    }
}
