using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Sổ tay Kiến trúc 3D: Thành phần hiển thị độc lập cho Đạn Đạo (Projectile Visual).
    /// Chịu trách nhiệm đồng bộ hướng bay, quản lý TrailRenderer và Particle System.
    /// Triệt tiêu lỗi Visual Stretch (kéo vệt dài xuyên màn hình) khi tái sử dụng từ Object Pool.
    /// </summary>
    [DisallowMultipleComponent]
    public class ProjectileVisual3D : MonoBehaviour
    {
        [Header("Visual Components")]
        [SerializeField] private TrailRenderer _trailRenderer;
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private GameObject _meshModel;

        private void Awake()
        {
            if (_trailRenderer == null) _trailRenderer = GetComponentInChildren<TrailRenderer>();
            if (_particleSystem == null) _particleSystem = GetComponentInChildren<ParticleSystem>();
        }

        /// <summary>
        /// Gọi khi viên đạn được lấy ra từ Pool để bắn.
        /// </summary>
        public void OnSpawn()
        {
            if (_trailRenderer != null)
            {
                _trailRenderer.Clear();
                _trailRenderer.enabled = true;
            }

            if (_particleSystem != null)
            {
                _particleSystem.Clear();
                _particleSystem.Play();
            }

            if (_meshModel != null)
            {
                _meshModel.SetActive(true);
            }
        }

        /// <summary>
        /// Cập nhật hướng xoay của đạn theo vector vận tốc/tiếp tuyến.
        /// </summary>
        public void UpdateDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        /// <summary>
        /// Gọi trước khi viên đạn được trả về Pool.
        /// </summary>
        public void OnDespawn()
        {
            if (_trailRenderer != null)
            {
                _trailRenderer.Clear();
                _trailRenderer.enabled = false;
            }

            if (_particleSystem != null)
            {
                _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            if (_meshModel != null)
            {
                _meshModel.SetActive(false);
            }
        }
    }
}
