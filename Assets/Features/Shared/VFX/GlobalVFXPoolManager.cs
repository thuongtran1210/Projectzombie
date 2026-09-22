using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using ProjectZombie.VFX;

namespace ProjectZombie.Features.Shared.VFX
{
    /// <summary>
    /// Manager quản lý Object Pool tập trung cho các loại Particle System và GameObject Modular VFX trong game.
    /// Giúp loại bỏ hoàn toàn việc tạo trùng lặp ObjectPool rải rác ở từng Vũ khí và Kỹ năng.
    /// Tuân thủ quy chuẩn SOLID và 0 GC Allocation pooling cho cả ParticleSystem đơn lẻ lẫn Prefab Modular VFX.
    /// </summary>
    public class GlobalVFXPoolManager : MonoBehaviour, IVFXPoolService
    {
        private const string LAYER_SKILL = "Skill";
        private const string LAYER_TILEMAP_DECALS = "Tilemap_Decals";
        private const string LAYER_DEFAULT = "Default";
        private const string LAYER_VFX_FRONT = "VFX_Front";
        private const string LAYER_VFX_BACK = "VFX_Back";

        public static GlobalVFXPoolManager Instance { get; private set; }

        private readonly Dictionary<int, ObjectPool<ParticleSystem>> _particlePoolDict = new Dictionary<int, ObjectPool<ParticleSystem>>();
        private readonly Dictionary<int, ObjectPool<GameObject>> _gameObjectPoolDict = new Dictionary<int, ObjectPool<GameObject>>();

        private readonly Dictionary<ParticleSystem, ObjectPool<ParticleSystem>> _activeParticleToPoolMap = new Dictionary<ParticleSystem, ObjectPool<ParticleSystem>>();
        private readonly Dictionary<GameObject, ObjectPool<GameObject>> _activeGameObjectToPoolMap = new Dictionary<GameObject, ObjectPool<GameObject>>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        #region ParticleSystem Pooling

        /// <summary>
        /// Lấy hoặc tạo mới ParticleSystem từ Pool và tự động thu hồi sau autoReleaseDelay giây.
        /// </summary>
        public ParticleSystem PlayEffect(ParticleSystem prefab, Vector3 position, Quaternion rotation, float autoReleaseDelay = 0.5f, Vector3? scale = null)
        {
            if (prefab == null) return null;

            int key = prefab.GetInstanceID();

            if (!_particlePoolDict.TryGetValue(key, out var pool))
            {
                pool = new ObjectPool<ParticleSystem>(
                    createFunc: () => {
                        Object spawned = Object.Instantiate((Object)prefab, transform);
                        ParticleSystem ps = null;
                        if (spawned is ParticleSystem p) ps = p;
                        else if (spawned is GameObject go) ps = go.GetComponentInChildren<ParticleSystem>(true);
                        else if (spawned is Component comp) ps = comp.GetComponentInChildren<ParticleSystem>(true);

                        if (ps != null)
                        {
                            ApplyStandardSortingLayers(ps.gameObject);
                        }
                        return ps;
                    },
                    actionOnGet: ps => { 
                        if (ps != null)
                        {
                            ps.gameObject.SetActive(true); 
                            ps.Play(true); 
                        }
                    },
                    actionOnRelease: ps => { 
                        if (ps != null)
                        {
                            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); 
                            ps.gameObject.SetActive(false); 
                        }
                    },
                    actionOnDestroy: ps => { if (ps != null) Destroy(ps.gameObject); },
                    defaultCapacity: 15,
                    maxSize: 100
                );
                _particlePoolDict.Add(key, pool);
            }

            var instance = pool.Get();
            if (instance == null) return null;

            instance.transform.position = position;
            instance.transform.rotation = rotation;
            if (scale.HasValue)
            {
                instance.transform.localScale = scale.Value;
            }

            instance.Play(true);
            _activeParticleToPoolMap[instance] = pool;

            if (autoReleaseDelay > 0f)
            {
                StartCoroutine(ReleaseParticleRoutine(pool, instance, autoReleaseDelay));
            }
            return instance;
        }

        #endregion

        #region GameObject Modular VFX Pooling

        /// <summary>
        /// Lấy hoặc tạo mới GameObject Modular VFX (Prefab lồng nhiều Particle + VFXPoolResetter) từ Pool.
        /// Tự động kích hoạt toàn bộ ParticleSystem con và tự thu hồi về Pool sau autoReleaseDelay giây (0 GC).
        /// </summary>
        public GameObject PlayEffect(GameObject prefab, Vector3 position, Quaternion rotation, float autoReleaseDelay = 0.5f, Vector3? scale = null, int weaponLevel = 1)
        {
            if (prefab == null) return null;

            int key = prefab.GetInstanceID();

            if (!_gameObjectPoolDict.TryGetValue(key, out var pool))
            {
                pool = new ObjectPool<GameObject>(
                    createFunc: () => {
                        Object spawned = Object.Instantiate((Object)prefab, transform);
                        GameObject go = null;
                        if (spawned is GameObject g) go = g;
                        else if (spawned is Component comp) go = comp.gameObject;

                        if (go != null)
                        {
                            if (!go.TryGetComponent<VFXPoolResetter>(out var resetter))
                            {
                                ApplyStandardSortingLayers(go);
                            }
                        }
                        return go;
                    },
                    actionOnGet: go => {
                        if (go != null)
                        {
                            go.SetActive(true);
                            if (go.TryGetComponent<IPoolableVFX>(out var poolable))
                            {
                                poolable.OnSpawnFromPool();
                            }
                            else if (go.TryGetComponent<VFXPoolResetter>(out var resetter))
                            {
                                resetter.ResetVFXState();
                            }
                        }
                    },
                    actionOnRelease: go => {
                        if (go != null)
                        {
                            if (go.TryGetComponent<IPoolableVFX>(out var poolable))
                            {
                                poolable.OnReturnToPool();
                            }
                            else if (go.TryGetComponent<VFXPoolResetter>(out var resetter))
                            {
                                resetter.ResetVFXState();
                            }
                            go.SetActive(false);
                        }
                    },
                    actionOnDestroy: go => { if (go != null) Destroy(go); },
                    defaultCapacity: 10,
                    maxSize: 60
                );
                _gameObjectPoolDict.Add(key, pool);
            }

            var instance = pool.Get();
            if (instance == null) return null;

            instance.transform.position = position;
            instance.transform.rotation = rotation;
            if (scale.HasValue)
            {
                instance.transform.localScale = scale.Value;
            }

            // Phân cấp quy mô hiệu ứng theo cấp độ vũ khí nếu có VFXLevelScaler
            if (instance.TryGetComponent<VFXLevelScaler>(out var levelScaler))
            {
                levelScaler.ApplyLevelScaling(weaponLevel);
            }

            _activeGameObjectToPoolMap[instance] = pool;

            if (autoReleaseDelay > 0f)
            {
                StartCoroutine(ReleaseGameObjectRoutine(pool, instance, autoReleaseDelay));
            }

            return instance;
        }

        #endregion

        #region Attached VFX

        /// <summary>
        /// Lấy hoặc tạo mới GameObject Modular VFX gắn bám theo một Transform trong suốt thời gian phát.
        /// </summary>
        public GameObject PlayEffectAttached(GameObject prefab, Transform parent, float autoReleaseDelay = 0.5f, Vector3? scale = null, int weaponLevel = 1)
        {
            if (prefab == null) return null;
            if (parent == null) return PlayEffect(prefab, Vector3.zero, Quaternion.identity, autoReleaseDelay, scale, weaponLevel);

            var instance = PlayEffect(prefab, parent.position, parent.rotation, autoReleaseDelay, scale, weaponLevel);
            if (instance != null)
            {
                instance.transform.SetParent(parent, false);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
            }
            return instance;
        }

        /// <summary>
        /// Lấy hoặc tạo mới ParticleSystem gắn bám theo một Transform trong suốt thời gian phát.
        /// </summary>
        public ParticleSystem PlayEffectAttached(ParticleSystem prefab, Transform parent, float autoReleaseDelay = 0.5f, Vector3? scale = null)
        {
            if (prefab == null) return null;
            if (parent == null) return PlayEffect(prefab, Vector3.zero, Quaternion.identity, autoReleaseDelay, scale);

            var instance = PlayEffect(prefab, parent.position, parent.rotation, autoReleaseDelay, scale);
            if (instance != null)
            {
                instance.transform.SetParent(parent, false);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
            }
            return instance;
        }

        #endregion

        #region Release & Cleanup

        /// <summary>
        /// Thu hồi thủ công một ParticleSystem về pool trước khi hết hạn auto-release.
        /// </summary>
        public void ReleaseEffect(ParticleSystem instance)
        {
            if (instance == null) return;
            if (_activeParticleToPoolMap.TryGetValue(instance, out var pool))
            {
                _activeParticleToPoolMap.Remove(instance);
                if (instance.gameObject != null)
                {
                    instance.transform.SetParent(transform, false);
                }
                pool.Release(instance);
            }
            else if (instance.gameObject != null)
            {
                instance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                instance.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Thu hồi thủ công một GameObject Modular VFX về pool trước khi hết hạn auto-release.
        /// </summary>
        public void ReleaseEffect(GameObject instance)
        {
            if (instance == null) return;
            if (_activeGameObjectToPoolMap.TryGetValue(instance, out var pool))
            {
                _activeGameObjectToPoolMap.Remove(instance);
                instance.transform.SetParent(transform, false);
                pool.Release(instance);
            }
            else
            {
                instance.SetActive(false);
            }
        }

        /// <summary>
        /// Thu hồi và dọn dẹp toàn bộ hiệu ứng Particle & Modular VFX đang hoạt động trên màn hình về ObjectPool tương ứng.
        /// </summary>
        public void ClearAllActiveEffects()
        {
            StopAllCoroutines();

            if (_activeParticleToPoolMap.Count > 0)
            {
                var activePs = new List<KeyValuePair<ParticleSystem, ObjectPool<ParticleSystem>>>(_activeParticleToPoolMap);
                _activeParticleToPoolMap.Clear();
                for (int i = 0; i < activePs.Count; i++)
                {
                    var ps = activePs[i].Key;
                    var pool = activePs[i].Value;
                    if (ps != null && ps.gameObject != null)
                    {
                        ps.transform.SetParent(transform, false);
                        pool.Release(ps);
                    }
                }
            }

            if (_activeGameObjectToPoolMap.Count > 0)
            {
                var activeGos = new List<KeyValuePair<GameObject, ObjectPool<GameObject>>>(_activeGameObjectToPoolMap);
                _activeGameObjectToPoolMap.Clear();
                for (int i = 0; i < activeGos.Count; i++)
                {
                    var go = activeGos[i].Key;
                    var pool = activeGos[i].Value;
                    if (go != null)
                    {
                        go.transform.SetParent(transform, false);
                        pool.Release(go);
                    }
                }
            }
        }

        #endregion

        #region Helpers & Routines

        private static void ApplyStandardSortingLayers(GameObject root)
        {
            if (root == null) return;
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r == null) continue;

                if (r.sortingLayerID == 0 || r.sortingLayerName == LAYER_DEFAULT || r.sortingLayerName == LAYER_VFX_FRONT)
                {
                    r.sortingLayerName = LAYER_SKILL;
                }
                else if (r.sortingLayerName == LAYER_VFX_BACK)
                {
                    r.sortingLayerName = LAYER_TILEMAP_DECALS;
                }
            }
        }

        private IEnumerator ReleaseParticleRoutine(ObjectPool<ParticleSystem> pool, ParticleSystem instance, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (instance != null && _activeParticleToPoolMap.Remove(instance))
            {
                if (instance.gameObject != null && instance.gameObject.activeSelf)
                {
                    instance.transform.SetParent(transform, false);
                    pool.Release(instance);
                }
            }
        }

        private IEnumerator ReleaseGameObjectRoutine(ObjectPool<GameObject> pool, GameObject instance, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (instance != null && _activeGameObjectToPoolMap.Remove(instance))
            {
                if (instance.activeSelf)
                {
                    instance.transform.SetParent(transform, false);
                    pool.Release(instance);
                }
            }
        }

        #endregion
    }
}
