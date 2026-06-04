using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Component lõi của mọi loại đạn. Lưu trữ dữ liệu, phương hướng và vòng đời.
    /// </summary>
    public class ProjectileCore : MonoBehaviour
    {
        [Header("Core Settings")]
        [SerializeField] private float lifetime = 3f;

        public DamageData DamageData { get; private set; }
        public Vector2 Direction { get; private set; }

        private ProjectilePoolConfig _poolConfig;
        private float _spawnTime;

        private void Awake()
        {
            _poolConfig = GetComponent<ProjectilePoolConfig>();
        }

        private void OnEnable()
        {
            _spawnTime = Time.time;
        }

        public void Initialize(Vector2 direction, DamageData damageData)
        {
            Direction = direction;
            DamageData = damageData;
        }

        private void Update()
        {
            if (Time.time >= _spawnTime + lifetime)
            {
                ReturnToPool();
            }
        }

        public void ReturnToPool()
        {
            if (_poolConfig != null)
            {
                _poolConfig.ReturnToPool();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
