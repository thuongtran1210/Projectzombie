using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Định nghĩa cấu hình dữ liệu của một loại Đạn Đạo 3D (ScriptableObject).
    /// Tuân thủ Data-Driven Design: Cho phép Game Designer tùy biến tốc độ, quỹ đạo, loại sát thương,
    /// bán kính nổ và hiệu ứng VFX/SFX mà không cần chỉnh sửa code C#.
    /// </summary>
    [CreateAssetMenu(fileName = "Proj3D_NewDefinition", menuName = "Defense3D/Projectile Definition")]
    public sealed class DefenseProjectileDefinitionSO : ScriptableObject
    {
        [Header("1. Định Danh & Trình Diễn (Identity & Visuals)")]
        [SerializeField] private string _projectileId = "PROJ_ARROW";
        [SerializeField] private string _displayName = "Mũi Tên Tre";
        [SerializeField] private GameObject _visualPrefab;
        [SerializeField] private GameObject _impactVfxPrefab;

        [Header("2. Chiến Lược Quỹ Đạo Bay (Trajectory Strategy)")]
        [SerializeField] private DefenseTrajectoryType _trajectoryType = DefenseTrajectoryType.ParabolicHoming;
        [SerializeField, Min(1f)] private float _speed = 25f;
        [SerializeField, Min(0f)] private float _arcHeight = 0.5f;
        [SerializeField, Min(0.1f)] private float _maxLifetime = 5f;

        [Header("3. Chiến Lược Sát Thương & Va Chạm (Payload Strategy)")]
        [SerializeField] private DefensePayloadType _payloadType = DefensePayloadType.SingleTarget;
        [SerializeField, Min(0f)] private float _aoeRadius = 2.5f;

        [Header("4. Âm Thanh (SFX)")]
        [SerializeField] private AudioClip _launchSfx;
        [SerializeField] private AudioClip _impactSfx;

        // Public Accessors
        public string ProjectileId => _projectileId;
        public string DisplayName => _displayName;
        public GameObject VisualPrefab => _visualPrefab;
        public GameObject ImpactVfxPrefab => _impactVfxPrefab;
        public DefenseTrajectoryType TrajectoryType => _trajectoryType;
        public float Speed => _speed;
        public float ArcHeight => _arcHeight;
        public float MaxLifetime => _maxLifetime;
        public DefensePayloadType PayloadType => _payloadType;
        public float AoERadius => _aoeRadius;
        public AudioClip LaunchSfx => _launchSfx;
        public AudioClip ImpactSfx => _impactSfx;

        public IProjectileTrajectory GetTrajectory()
        {
            switch (_trajectoryType)
            {
                case DefenseTrajectoryType.StraightDirectional: return StraightDirectionalTrajectory.Instance;
                case DefenseTrajectoryType.BallisticGround: return BallisticGroundTrajectory.Instance;
                case DefenseTrajectoryType.ParabolicHoming:
                default:
                    return ParabolicHomingTrajectory.Instance;
            }
        }

        public IProjectilePayload GetPayload()
        {
            switch (_payloadType)
            {
                case DefensePayloadType.AoEExplosion: return AoEExplosionPayload.Instance;
                case DefensePayloadType.Piercing: return PiercingDamagePayload.Instance;
                case DefensePayloadType.SingleTarget:
                default:
                    return SingleTargetDamagePayload.Instance;
            }
        }
    }
}
