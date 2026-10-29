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
        private DefenseRunStats _modifiers;

        public DefenseUpgradeDeck()
        {
            _modifiers = new DefenseRunStats();
            cardPool = new List<DefenseUpgradeCard>
            {
                new DefenseUpgradeCard("card_dmg_1", "Thư Kiếm Hợp Nhất", "+20% Sát thương Hero", DefenseUpgradeType.HeroAttackDamage, 0.20f),
                new DefenseUpgradeCard("card_aspd_1", "Bút Tẩu Long Xà", "+15% Tốc đánh Hero", DefenseUpgradeType.HeroAttackSpeed, 0.15f),
                new DefenseUpgradeCard("card_spd_1", "Lăng Ba Vi Bộ", "+10% Tốc chạy Hero", DefenseUpgradeType.HeroMoveSpeed, 0.10f),
                new DefenseUpgradeCard("card_tower_dmg_1", "Hỏa Pháo Cường Hóa", "+25% Sát thương Tháp", DefenseUpgradeType.TowerDamage, 0.25f),
                new DefenseUpgradeCard("card_tower_rng_1", "Thiên Lý Nhãn", "+20% Tầm bắn Tháp", DefenseUpgradeType.TowerAttackRange, 0.20f),
                new DefenseUpgradeCard("card_base_hp_1", "Thành Kiên Đao Cố", "+200 Máu Nhà Chính", DefenseUpgradeType.BaseMaxHp, 200f)
            };

        }

        /// <summary>Composition supplies the session ledger; standalone callers retain a private ledger.</summary>
        public void BindModifiers(DefenseRunStats modifiers)
        {
            _modifiers = modifiers ?? throw new ArgumentNullException(nameof(modifiers));
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

        public List<DefenseUpgradeCard> DrawThreeChoices(int seed = 0) => DrawThreeCards(seed);

        public bool TryApplyCard(DefenseUpgradeCard card)
        {
            return _modifiers.TryApplyCard(card);
        }

        public bool SelectUpgrade(DefenseUpgradeCard card) => TryApplyCard(card);

        public float GetModifier(DefenseUpgradeType type)
        {
            return _modifiers.GetModifier(type);
        }

        public void Reset()
        {
            _modifiers.Reset();
        }
    }
}
