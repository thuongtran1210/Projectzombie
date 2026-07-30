using UnityEngine;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Tiện ích tĩnh hỗ trợ tính toán sát thương đầu ra và Tương Khắc Ngũ Hành (v4.2).
    /// </summary>
    public static class DamageUtility
    {
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
            ElementType defenderElement = ElementType.None)
        {
            bool isCrit = Random.value <= critChance;
            float elementMult = GetElementMultiplier(attackerElement, defenderElement);
            float finalDamage = (isCrit ? baseDamage * critDamageMultiplier : baseDamage) * elementMult;

            return new DamageData(finalDamage, isCrit, attackerElement);
        }
    }
}

