using UnityEngine;
using System.Collections;
using ProjectZombie.Features.Shared;
using DG.Tweening;

namespace ProjectZombie.Features.Spawners
{
    public class SpawnPillar : MonoBehaviour
    {
        private PillarConfig _config;
        private int _spawnedCount = 0;
        private HealthSystem _healthSystem;

        public void Initialize(PillarConfig config)
        {
            _config = config;
            _spawnedCount = 0;

            // --- HIỆU ỨNG XUẤT HIỆN BẰNG DOTWEEN ---
            Vector3 targetScale = transform.localScale;
            transform.localScale = Vector3.zero;
            transform.DOScale(targetScale, 0.5f).SetEase(Ease.OutBack);

            // Setup HealthSystem if attackable
            _healthSystem = GetComponent<HealthSystem>();
            
            if (_config.isAttackable)
            {
                if (_healthSystem == null)
                {
                    _healthSystem = gameObject.AddComponent<HealthSystem>();
                }
                
                _healthSystem.SetMaxHealth(500f, true);
                _healthSystem.OnDied -= HandleDeath;
                _healthSystem.OnDied += HandleDeath;
            }
            else
            {
                if (_healthSystem != null)
                {
                    _healthSystem.enabled = false;
                }
            }

            StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine()
        {
            while (_spawnedCount < _config.totalEnemiesToSpawn)
            {
                yield return new WaitForSeconds(_config.enemySpawnInterval);
                
                SpawnEnemy();
                _spawnedCount++;
            }

            DestroyPillar();
        }

        private void SpawnEnemy()
        {
            if (_config.enemyPrefab == null || EnemyPoolManager.Instance == null) return;

            Vector2 randomOffset = Random.insideUnitCircle * 2f;
            Vector3 spawnPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

            EnemyPoolManager.Instance.SpawnEnemy(_config.enemyPrefab, spawnPos, Quaternion.identity);
        }

        private void HandleDeath()
        {
            Debug.Log("[SpawnPillar] Destroyed by player!");
            DestroyPillar();
        }

        private void DestroyPillar()
        {
            if (_healthSystem != null)
            {
                _healthSystem.OnDied -= HandleDeath;
            }
            
            StopAllCoroutines();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (_healthSystem != null)
            {
                _healthSystem.OnDied -= HandleDeath;
            }
        }
    }
}
