using System;
using System.Collections.Generic;

namespace ProjectZombie.Features.Defense3D
{
    public enum DefenseUpgradeType
    {
        HeroAttackDamage,
        HeroAttackSpeed,
        HeroMoveSpeed,
        TowerDamage,
        TowerAttackRange,
        BaseMaxHp
    }

    public struct DefenseUpgradeCard
    {
        public string CardId;
        public string Title;
        public string Description;
        public DefenseUpgradeType UpgradeType;
        public float Value;

        public DefenseUpgradeCard(string id, string title, string desc, DefenseUpgradeType type, float value)
        {
            CardId = id;
            Title = title;
            Description = desc;
            UpgradeType = type;
            Value = value;
        }
    }

    /// <summary>
    /// U3D-020: 3-Card upgrade selection and stat modifiers.
    /// Invariants:
    /// - Always offers 3 unique valid choices from the pool.
    /// - Selecting a card applies actual modifiers.
    /// - Fallback provided if pool is depleted.
    /// </summary>
    public sealed class DefenseUpgradeDeck
    {
        private readonly List<DefenseUpgradeCard> cardPool;
        private readonly Dictionary<DefenseUpgradeType, float> activeModifiers = new Dictionary<DefenseUpgradeType, float>();

        public DefenseUpgradeDeck()
        {
            cardPool = new List<DefenseUpgradeCard>
            {
                new DefenseUpgradeCard("card_dmg_1", "Thư Kiếm Hợp Nhất", "+20% Sát thương Hero", DefenseUpgradeType.HeroAttackDamage, 0.20f),
                new DefenseUpgradeCard("card_aspd_1", "Bút Tẩu Long Xà", "+15% Tốc đánh Hero", DefenseUpgradeType.HeroAttackSpeed, 0.15f),
                new DefenseUpgradeCard("card_spd_1", "Lăng Ba Vi Bộ", "+10% Tốc chạy Hero", DefenseUpgradeType.HeroMoveSpeed, 0.10f),
                new DefenseUpgradeCard("card_tower_dmg_1", "Hỏa Pháo Cường Hóa", "+25% Sát thương Tháp", DefenseUpgradeType.TowerDamage, 0.25f),
                new DefenseUpgradeCard("card_tower_rng_1", "Thiên Lý Nhãn", "+20% Tầm bắn Tháp", DefenseUpgradeType.TowerAttackRange, 0.20f),
                new DefenseUpgradeCard("card_base_hp_1", "Thành Kiên Đao Cố", "+200 Máu Nhà Chính", DefenseUpgradeType.BaseMaxHp, 200f)
            };

            foreach (DefenseUpgradeType type in Enum.GetValues(typeof(DefenseUpgradeType)))
            {
                activeModifiers[type] = 0f;
            }
        }

        public List<DefenseUpgradeCard> DrawThreeCards(int seed = 0)
        {
            var random = seed == 0 ? new Random() : new Random(seed);
            var indices = new List<int>();
            for (int i = 0; i < cardPool.Count; i++) indices.Add(i);

            // Fisher-Yates shuffle
            for (int i = indices.Count - 1; i > 0; i--)
            {
                int k = random.Next(i + 1);
                int tmp = indices[i];
                indices[i] = indices[k];
                indices[k] = tmp;
            }

            var chosen = new List<DefenseUpgradeCard>(3);
            for (int i = 0; i < Math.Min(3, indices.Count); i++)
            {
                chosen.Add(cardPool[indices[i]]);
            }
            return chosen;
        }

        public bool TryApplyCard(DefenseUpgradeCard card)
        {
            if (activeModifiers.ContainsKey(card.UpgradeType))
            {
                activeModifiers[card.UpgradeType] += card.Value;
                return true;
            }
            return false;
        }

        public float GetModifier(DefenseUpgradeType type)
        {
            return activeModifiers.TryGetValue(type, out float val) ? val : 0f;
        }

        public void Reset()
        {
            foreach (DefenseUpgradeType type in Enum.GetValues(typeof(DefenseUpgradeType)))
            {
                activeModifiers[type] = 0f;
            }
        }
    }
}
