using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Upgrades.Weighting
{
    /// <summary>
    /// Ưu tiên tăng +40% trọng số xuất hiện cho các thẻ nâng cấp của Vũ Khí mà người chơi đang sở hữu,
    /// giúp người chơi nhanh chóng nâng max cấp để tiến hóa thành Thần Binh Tối Thượng (Evolutions).
    /// </summary>
    public class OwnedWeaponPriorityWeighter : IUpgradeWeighter
    {
        private const float OWNED_WEAPON_MULTIPLIER = 1.40f; // +40%

        public float CalculateMultiplier(UpgradeData upgrade, PlayerContext context)
        {
            if (upgrade == null || context?.WeaponManager == null) return 1.0f;

            if (upgrade is WeaponUpgradeData weaponUp && !string.IsNullOrEmpty(weaponUp.weaponId))
            {
                if (context.WeaponManager.GetWeaponById(weaponUp.weaponId) != null)
                {
                    return OWNED_WEAPON_MULTIPLIER;
                }
            }

            return 1.0f;
        }
    }
}
