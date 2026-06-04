using UnityEngine;
using System.Collections;
using ProjectZombie.Features.Shared;
using TikTokBridge.Models;

namespace TikTokBridge.Systems.Spawners
{
    public class SpawnPillar : MonoBehaviour
    {
        private SpawnPillarConfig _config;
        private int _spawnedCount = 0;
        private HealthSystem _healthSystem;

        public void Initialize(SpawnPillarConfig config)
        {
            _config = config;
            _spawnedCount = 0;

            // Setup HealthSystem if attackable
            _healthSystem = GetComponent<HealthSystem>();
            
            if (_config.isAttackable)
            {
                if (_healthSystem == null)
                {
                    _healthSystem = gameObject.AddComponent<HealthSystem>();
                }
                
                // Assuming a default health for the pillar, or it could be added to config later
                // For now, let's just give it a decent amount of health (e.g., 500)
                _healthSystem.SetMaxHealth(500f, true);
                
                _healthSystem.OnDied -= HandleDeath; // Prevent duplicate subscriptions
                _healthSystem.OnDied += HandleDeath;
            }
            else
            {
                // If not attackable, disable HealthSystem if it exists
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

            // Finished spawning
            DestroyPillar();
        }

        private void SpawnEnemy()
        {
            if (_config.enemyPrefab == null || EnemyPoolManager.Instance == null) return;

            // Get a random position slightly offset from the pillar
            Vector2 randomOffset = Random.insideUnitCircle * 2f;
            Vector3 spawnPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

            EnemyPoolManager.Instance.SpawnEnemy(_config.enemyPrefab, spawnPos, Quaternion.identity);
        }

        private void HandleDeath()
        {
            // Optional: Play death effect
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
            
            // Destroy or return to pool (using Destroy for now as it's not a frequent object)
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
