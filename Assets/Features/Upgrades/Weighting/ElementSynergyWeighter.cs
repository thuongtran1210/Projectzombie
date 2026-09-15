using ProjectZombie.Features.Player;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades.Weighting
{
    /// <summary>
    /// Tăng trọng số xuất hiện của thẻ dựa theo nguyên tắc Ngũ Hành:
    /// - Cùng hệ với vũ khí đang sở hữu: +35% trọng số (x1.35)
    /// - Tương sinh với vũ khí đang sở hữu: +25% trọng số (x1.25)
    /// </summary>
    public class ElementSynergyWeighter : IUpgradeWeighter
    {
        private const float SAME_ELEMENT_MULTIPLIER = 1.35f;
        private const float GENERATIVE_ELEMENT_MULTIPLIER = 1.25f;

        public float CalculateMultiplier(UpgradeData upgrade, PlayerContext context)
        {
            if (upgrade == null || upgrade.element == ElementType.None || context?.WeaponManager == null)
            {
                return 1.0f;
            }

            var weapons = context.WeaponManager.ActiveWeapons;
            if (weapons == null || weapons.Count == 0) return 1.0f;

            bool isSameElement = false;
            bool isGenerativeElement = false;

            for (int i = 0; i < weapons.Count; i++)
            {
                var w = weapons[i];
                if (w == null || w.element == ElementType.None) continue;

                if (w.element == upgrade.element)
                {
                    isSameElement = true;
                    break;
                }

                if (ElementSynergyRules.IsElementGenerative(w.element, upgrade.element))
                {
                    isGenerativeElement = true;
                }
            }

            if (isSameElement) return SAME_ELEMENT_MULTIPLIER;
            if (isGenerativeElement) return GENERATIVE_ELEMENT_MULTIPLIER;

            return 1.0f;
        }
    }
}
