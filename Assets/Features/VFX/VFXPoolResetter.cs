using UnityEngine;
using ProjectZombie.Features.Shared.VFX;

namespace ProjectZombie.VFX
{
    /// <summary>
    /// Component quản lý vòng đời và tự động reset trạng thái của ParticleSystem, TrailRenderer,
    /// chuẩn hóa Sorting Layer khi Prefab VFX được kích hoạt hoặc thu hồi về Object Pool.
    /// Giúp triệt tiêu hoàn toàn lỗi "dính particle rác" / visual ghosting và đảm bảo 0 GC Alloc lúc runtime.
    /// </summary>
    public class VFXPoolResetter : MonoBehaviour, IPoolableVFX
    {
        private const string LAYER_SKILL = "Skill";
        private const string LAYER_TILEMAP_DECALS = "Tilemap_Decals";
        private const string LAYER_DEFAULT = "Default";
        private const string LAYER_VFX_FRONT = "VFX_Front";
        private const string LAYER_VFX_BACK = "VFX_Back";

        private ParticleSystem[] _particleSystems;
        private TrailRenderer[] _trailRenderers;
        private Renderer[] _renderers;
        private bool _isInitialized;

        private void Awake()
        {
            EnsureInitialized();
        }

        /// <summary>
        /// Cache toàn bộ các Component con để tránh gọi GetComponentsInChildren trong suốt quá trình chơi (0 GC).
        /// </summary>
        public void CacheComponents()
        {
            _particleSystems = GetComponentsInChildren<ParticleSystem>(true);
            _trailRenderers = GetComponentsInChildren<TrailRenderer>(true);
            _renderers = GetComponentsInChildren<Renderer>(true);
            _isInitialized = true;
            ApplyStandardSortingLayers();
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                CacheComponents();
            }
        }

        /// <summary>
        /// Chuẩn hóa Sorting Layer cho toàn bộ Renderer trong hierarchy lúc khởi tạo.
        /// </summary>
        private void ApplyStandardSortingLayers()
        {
            if (_renderers == null) return;

            for (int i = 0; i < _renderers.Length; i++)
            {
                var r = _renderers[i];
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

        #region IPoolable & IPoolableVFX Implementation

        public void OnSpawn()
        {
            OnSpawnFromPool();
        }

        public void OnDespawn()
        {
            OnReturnToPool();
        }

        /// <summary>
        /// Gọi khi đối tượng được lấy ra từ Pool (tự động Play lại các Particle System con).
        /// </summary>
        public void OnSpawnFromPool()
        {
            EnsureInitialized();

            if (_particleSystems != null)
            {
                for (int i = 0; i < _particleSystems.Length; i++)
                {
                    var ps = _particleSystems[i];
                    if (ps != null)
                    {
                        ps.Play(true);
                    }
                }
            }
        }

        /// <summary>
        /// Gọi khi đối tượng được hoàn trả về Pool (tự động Stop Particle và Clear Trail).
        /// </summary>
        public void OnReturnToPool()
        {
            ResetVFXState();
        }

        #endregion

        private void OnDisable()
        {
            ResetVFXState();
        }

        /// <summary>
        /// Dọn dẹp trạng thái VFX, triệt tiêu hạt thừa và trail còn vương lại.
        /// </summary>
        public void ResetVFXState()
        {
            EnsureInitialized();

            if (_particleSystems != null)
            {
                for (int i = 0; i < _particleSystems.Length; i++)
                {
                    var ps = _particleSystems[i];
                    if (ps != null)
                    {
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                }
            }

            if (_trailRenderers != null)
            {
                for (int i = 0; i < _trailRenderers.Length; i++)
                {
                    var trail = _trailRenderers[i];
                    if (trail != null)
                    {
                        trail.Clear();
                    }
                }
            }
        }
    }
}
