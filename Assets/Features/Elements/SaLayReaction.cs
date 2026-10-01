using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Shared;
using UnityEngine;

namespace ProjectZombie.Features.Elements
{
    /// <summary>
    /// Phản ứng Sa Lầy (Thổ → Thủy):
    /// Tạo ra vũng bùn 2.0s tại chân mục tiêu, làm chậm 30% - 35% mọi kẻ địch đứng trong vùng.
    /// Không gây sát thương theo thời gian, không lan truyền chuỗi.
    /// </summary>
    internal sealed class SaLayReaction
    {
        private readonly SaLayReactionSettings _settings;

        public SaLayReaction(SaLayReactionSettings settings)
        {
            _settings = settings;
        }

        public bool TryExecute(Enemy target, DamageData triggeringDamage)
        {
            if (target == null || target.HealthSystem == null || !target.HealthSystem.IsAlive)
                return false;

            SaLayPatchManager.CreatePatch(target.transform.position);
            return true;
        }
    }
}
