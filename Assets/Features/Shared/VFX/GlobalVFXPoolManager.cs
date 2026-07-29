using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace ProjectZombie.Features.Shared.VFX
{
    /// <summary>
    /// Manager quản lý Object Pool tập trung cho các loại Particle System trong game.
    /// Giúp loại bỏ hoàn toàn việc tạo trùng lặp ObjectPool rải rác ở từng Vũ khí.
    /// </summary>
    public class GlobalVFXPoolManager : MonoBehaviour
    {
        public static GlobalVFXPoolManager Instance { get; private set; }

        private readonly Dictionary<int, ObjectPool<ParticleSystem>> _poolDictionary = new Dictionary<int, ObjectPool<ParticleSystem>>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Lấy hoặc tạo mới ParticleSystem từ Pool và tự động thu hồi sau `autoReleaseDelay` giây.
        /// </summary>
        public ParticleSystem PlayEffect(ParticleSystem prefab, Vector3 position, Quaternion rotation, float autoReleaseDelay = 0.5f, Vector3? scale = null)
        {
            if (prefab == null) return null;

            int key = prefab.GetInstanceID();

            if (!_poolDictionary.TryGetValue(key, out var pool))
            {
                pool = new ObjectPool<ParticleSystem>(
                    createFunc: () => Instantiate(prefab, transform),
                    actionOnGet: ps => { ps.gameObject.SetActive(true); ps.Play(true); },
                    actionOnRelease: ps => { 
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); 
                        ps.gameObject.SetActive(false); 
                    },
                    actionOnDestroy: ps => Destroy(ps.gameObject),
                    defaultCapacity: 15,
                    maxSize: 100
                );
                _poolDictionary.Add(key, pool);
            }

            var instance = pool.Get();
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            if (scale.HasValue)
            {
                instance.transform.localScale = scale.Value;
            }

            // Tự động đặt Sorting Layer sang "Skill" nếu Particle đang ở "Default" (tránh bị chìm dưới TileMap)
            var psRenderer = instance.GetComponent<ParticleSystemRenderer>();
            if (psRenderer != null && (psRenderer.sortingLayerID == 0 || psRenderer.sortingLayerName == "Default"))
            {
                psRenderer.sortingLayerName = "Skill";
            }

            StartCoroutine(ReleaseRoutine(pool, instance, autoReleaseDelay));
            return instance;
        }

        private IEnumerator ReleaseRoutine(ObjectPool<ParticleSystem> pool, ParticleSystem instance, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (instance != null && instance.gameObject.activeSelf)
            {
                pool.Release(instance);
            }
        }
    }
}
