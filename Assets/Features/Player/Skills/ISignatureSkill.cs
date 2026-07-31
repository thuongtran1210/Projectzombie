using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Player.Skills
{
    /// <summary>
    /// Contract giao diện cho tất cả Kỹ năng Chủ động (Signature Skills) của nhân vật.
    /// </summary>
    public interface ISignatureSkill
    {
        /// <summary>
        /// Thời gian hồi chiêu cơ bản (giây).
        /// </summary>
        float Cooldown { get; }

        /// <summary>
        /// Kiểm tra điều kiện có thể thi triển kỹ năng (HP, Guard Condition, etc.).
        /// </summary>
        bool CanExecute(PlayerStats stats, HealthSystem health);

        /// <summary>
        /// Thực thi logic kỹ năng chủ động.
        /// </summary>
        void Execute(GameObject playerObj, System.Action<ElementType> onElementSelectedCallback = null);

        /// <summary>
        /// Cập nhật tick per-frame nếu kỹ năng cần duy trì trạng thái đếm ngược / hiệu lực.
        /// </summary>
        void Tick(float deltaTime);
    }
}
