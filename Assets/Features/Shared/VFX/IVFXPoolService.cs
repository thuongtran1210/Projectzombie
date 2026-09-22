using UnityEngine;

namespace ProjectZombie.Features.Shared.VFX
{
    /// <summary>
    /// Abstraction cho hệ thống dịch vụ quản lý Object Pool tập trung của VFX (Particle Systems & Modular VFX Prefabs).
    /// Hỗ trợ Dependency Injection và Decoupling, tuân thủ nguyên tắc Dependency Inversion (SOLID).
    /// </summary>
    public interface IVFXPoolService
    {
        /// <summary>
        /// Lấy hoặc tạo mới ParticleSystem từ Pool và tự động thu hồi sau autoReleaseDelay giây.
        /// </summary>
        ParticleSystem PlayEffect(ParticleSystem prefab, Vector3 position, Quaternion rotation, float autoReleaseDelay = 0.5f, Vector3? scale = null);

        /// <summary>
        /// Lấy hoặc tạo mới GameObject Modular VFX từ Pool và tự động thu hồi sau autoReleaseDelay giây.
        /// </summary>
        GameObject PlayEffect(GameObject prefab, Vector3 position, Quaternion rotation, float autoReleaseDelay = 0.5f, Vector3? scale = null, int weaponLevel = 1);

        /// <summary>
        /// Lấy hoặc tạo mới GameObject Modular VFX gắn bám theo một Transform trong suốt thời gian phát.
        /// </summary>
        GameObject PlayEffectAttached(GameObject prefab, Transform parent, float autoReleaseDelay = 0.5f, Vector3? scale = null, int weaponLevel = 1);

        /// <summary>
        /// Lấy hoặc tạo mới ParticleSystem gắn bám theo một Transform trong suốt thời gian phát.
        /// </summary>
        ParticleSystem PlayEffectAttached(ParticleSystem prefab, Transform parent, float autoReleaseDelay = 0.5f, Vector3? scale = null);

        /// <summary>
        /// Thu hồi thủ công một ParticleSystem về pool trước khi hết hạn auto-release.
        /// </summary>
        void ReleaseEffect(ParticleSystem instance);

        /// <summary>
        /// Thu hồi thủ công một GameObject Modular VFX về pool trước khi hết hạn auto-release.
        /// </summary>
        void ReleaseEffect(GameObject instance);

        /// <summary>
        /// Thu hồi và dọn dẹp toàn bộ hiệu ứng Particle & Modular VFX đang hoạt động trên màn hình về Pool.
        /// </summary>
        void ClearAllActiveEffects();
    }
}
