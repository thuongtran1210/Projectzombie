using UnityEngine;

namespace ProjectZombie.Features.Projectiles.Data
{
    /// <summary>
    /// Cấu hình dữ liệu hiệu ứng hình ảnh (VFX) và âm thanh (SFX) cho Đạn.
    /// Tách biệt hoàn toàn phần hiển thị ra khỏi logic tính toán sát thương.
    /// </summary>
    [System.Serializable]
    public struct VFXConfigData
    {
        [Header("Spawn / Launch VFX")]
        [Tooltip("Hiệu ứng khi đạn vừa xuất hiện (VD: Muzzle Flash)")]
        public ParticleSystem SpawnVFXPrefab;

        [Header("Hit Impact VFX")]
        [Tooltip("Hiệu ứng khi đạn chạm mục tiêu (VD: Hit Spark, Blood, Explosion)")]
        public ParticleSystem HitImpactVFXPrefab;

        [Header("Despawn / Expiration VFX")]
        [Tooltip("Hiệu ứng khi đạn tự hủy hoặc hết thời gian sống")]
        public ParticleSystem DespawnVFXPrefab;

        [Header("Audio SFX")]
        [Tooltip("Âm thanh khi đạn bắn ra")]
        public AudioClip LaunchSFX;
        
        [Tooltip("Âm thanh khi đạn va chạm")]
        public AudioClip HitSFX;
    }
}
