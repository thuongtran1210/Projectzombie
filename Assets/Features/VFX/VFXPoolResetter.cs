using UnityEngine;

namespace ProjectZombie.VFX
{
    /// <summary>
    /// Tự động reset trạng thái của ParticleSystem và TrailRenderer khi GameObject được ẩn (SetActive false)
    /// hoặc thu hồi về Object Pool. Giúp triệt tiêu hoàn toàn lỗi "dính particle rác" / ghosting visuals.
    /// </summary>
    public class VFXPoolResetter : MonoBehaviour
    {
        private ParticleSystem[] _particleSystems;
        private TrailRenderer[] _trailRenderers;

        private void Awake()
        {
            CacheComponents();
        }

        public void CacheComponents()
        {
            _particleSystems = GetComponentsInChildren<ParticleSystem>(true);
            _trailRenderers = GetComponentsInChildren<TrailRenderer>(true);
        }

        private void OnDisable()
        {
            ResetVFXState();
        }

        public void ResetVFXState()
        {
            if (_particleSystems != null)
            {
                foreach (var ps in _particleSystems)
                {
                    if (ps != null)
                    {
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                }
            }

            if (_trailRenderers != null)
            {
                foreach (var trail in _trailRenderers)
                {
                    if (trail != null)
                    {
                        trail.Clear();
                    }
                }
            }
        }
    }
}
