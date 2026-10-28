using System;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// Gói chỉ số cộng dồn của Player (dùng cho Cây nâng cấp vĩnh viễn Meta và chỉ số bổ trợ).
    /// </summary>
    [Serializable]
    public struct PlayerStatModifier
    {
        public float maxHealthBonus;
        public float moveSpeedBonus;
        public float critChanceBonus;
        public float baseDamageBonus;
        public float pickupRangeBonus;
        public float expMultiplierBonus;
        public float attackSpeedBonus;
        public float dashCooldownReduction;
        public float dashSpeedBonus;
        public float areaScaleBonus;
        public float fireDamageBonus;
    }
}
