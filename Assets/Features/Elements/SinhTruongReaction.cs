using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Shared;
using UnityEngine;

namespace ProjectZombie.Features.Elements
{
    /// <summary>
    /// Phản ứng Sinh Trưởng (Thủy → Mộc):
    /// Trói chân (Root) mục tiêu trong 1 - 1.25s; mục tiêu bị khóa di chuyển nhưng VẪN CÓ THỂ TẤN CÔNG.
    /// Không lan. Boss nhận thời gian trói giảm (0.25s) hoặc miễn nhiễm theo tenacity.
    /// </summary>
    internal sealed class SinhTruongReaction
    {
        private readonly SinhTruongReactionSettings _settings;

        public SinhTruongReaction(SinhTruongReactionSettings settings)
        {
            _settings = settings;
        }

        public bool TryExecute(Enemy target, DamageData triggeringDamage)
        {
            if (target == null || target.HealthSystem == null || !target.HealthSystem.IsAlive)
                return false;

            if (_settings == null || target.StatusController == null)
                return false;

            float duration = target.IsBoss ? _settings.bossRootDuration : _settings.rootDuration;
            if (duration <= 0.05f) return true;

            target.ApplyStatusEffect(StatusEffectType.Root, duration);
            global::ProjectZombie.Core.Audio.AudioService.Current?.PlayElementalReaction(target.transform.position);

            return true;
        }
    }
}
