using UnityEngine;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Tiện ích tĩnh hỗ trợ tính toán sát thương đầu ra và Tương Khắc Ngũ Hành (v4.2).
    /// </summary>
    public static class DamageUtility
    {
        /// <summary>Records an applied hit, or an accepted client-predicted network hit.</summary>
        public static void RegisterSuccessfulHit(DamageData damage)
        {
            if (damage.Amount <= 0f || damage.Element == ElementType.None) return;
            if (damage.Owner != null && damage.Owner.CompareTag("Enemy")) return;
            if (damage.AttackRecord != null)
                damage.AttackRecord.RegisterSuccessfulHit(damage.Element);
            else
                Elements.ElementCycleManager.Instance?.RegisterHit(damage.Element, damage.SourceWeapon as Weapons.WeaponBase, damage.Owner);
        }

        public static DamageData ApplyElementCounter(DamageData damage, ElementType defender)
        {
            if (damage.ElementMultiplierApplied) return damage;
            float multiplier = GetElementMultiplier(damage.Element, defender);
            damage.Amount *= multiplier;
            damage.IsCounter = multiplier > 1f;
            damage.ElementMultiplierApplied = true;
            return damage;
        }
        // Hệ số chí mạng mặc định (200% = 2x sát thương)
        public const float CRIT_MULTIPLIER = 2.0f;

        // Hệ số tăng sát thương khi Tương Khắc (30% = 1.3x)
        public const float ELEMENT_COUNTER_MULTIPLIER = 1.3f;

        /// <summary>
        /// Bảng tra cứu tương khắc Ngũ Hành 2D (ElementMatchupTable).
        /// Hàng = Element Tấn Công (Attacker), Cột = Element Mục Tiêu (Defender).
        /// Index: 0: None, 1: Kim, 2: Mộc, 3: Thủy, 4: Hỏa, 5: Thổ.
        /// </summary>
        private static readonly float[,] ElementMatchupTable = new float[6, 6]
        {
            // Defender: None,  Kim,  Mộc, Thủy,  Hỏa,  Thổ
            /* Attacker: None */ { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f },
            /* Attacker: Kim  */ { 1.0f, 1.0f, 1.3f, 1.0f, 1.0f, 1.0f }, // Kim khắc Mộc
            /* Attacker: Mộc  */ { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.3f }, // Mộc khắc Thổ
            /* Attacker: Thủy */ { 1.0f, 1.0f, 1.0f, 1.0f, 1.3f, 1.0f }, // Thủy khắc Hỏa
            /* Attacker: Hỏa  */ { 1.0f, 1.3f, 1.0f, 1.0f, 1.0f, 1.0f }, // Hỏa khắc Kim
            /* Attacker: Thổ  */ { 1.0f, 1.0f, 1.0f, 1.3f, 1.0f, 1.0f }  // Thổ khắc Thủy
        };

        /// <summary>
        /// Tra cứu hệ số nhân sát thương từ Bảng Tra Cứu ElementMatchupTable.
        /// Áp dụng cho Player đánh Quái.
        /// </summary>
        public static float GetElementMultiplier(ElementType attacker, ElementType defender)
        {
            int attIndex = (int)attacker;
            int defIndex = (int)defender;

            if (attIndex < 0 || attIndex >= 6 || defIndex < 0 || defIndex >= 6)
                return 1.0f;

            return ElementMatchupTable[attIndex, defIndex];
        }

        /// <summary>
        /// Tính toán sát thương cuối cùng dựa trên sát thương cơ bản, tỉ lệ chí mạng và hệ số Ngũ Hành.
        /// </summary>
        public static DamageData CalculateDamage(
            float baseDamage, 
            float critChance, 
            float critDamageMultiplier = CRIT_MULTIPLIER, 
            ElementType attackerElement = ElementType.None, 
            ElementType defenderElement = ElementType.None,
            Object sourceWeapon = null)
        {
            bool isCrit = Random.value <= critChance;
            float elementMult = GetElementMultiplier(attackerElement, defenderElement);
            bool isCounter = elementMult > 1.05f; // Khắc hệ (1.3x)
            float finalDamage = (isCrit ? baseDamage * critDamageMultiplier : baseDamage) * elementMult;

            return new DamageData(finalDamage, isCrit, attackerElement, isCounter, sourceWeapon)
            {
                ElementMultiplierApplied = defenderElement != ElementType.None
            };
        }

        /// <summary>
        /// Tính toán sát thương tại điểm va chạm thực tế (khi đã biết chắc chắn defenderElement).
        /// </summary>
        public static DamageData CalculateHitDamage(
            float damageAmount,
            bool isCrit,
            ElementType attackerElement,
            ElementType defenderElement,
            Object sourceWeapon = null)
        {
            float elementMult = GetElementMultiplier(attackerElement, defenderElement);
            bool isCounter = elementMult > 1.05f;
            float finalDamage = damageAmount * elementMult;

            return new DamageData(finalDamage, isCrit, attackerElement, isCounter, sourceWeapon)
            {
                ElementMultiplierApplied = defenderElement != ElementType.None
            };
        }
    }
}


