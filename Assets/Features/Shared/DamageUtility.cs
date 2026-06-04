using UnityEngine;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Tiện ích tĩnh hỗ trợ tính toán sát thương đầu ra.
    /// </summary>
    public static class DamageUtility
    {
        // Hệ số chí mạng mặc định (200% = 2x sát thương)
        public const float CRIT_MULTIPLIER = 2.0f;

        /// <summary>
        /// Tính toán sát thương cuối cùng dựa trên sát thương cơ bản và tỉ lệ chí mạng.
        /// </summary>
        /// <param name="baseDamage">Sát thương cơ bản</param>
        /// <param name="critChance">Tỉ lệ chí mạng (0.0 đến 1.0)</param>
        /// <param name="critDamageMultiplier">Hệ số nhân khi chí mạng (Mặc định 2.0)</param>
        /// <returns>Dữ liệu sát thương bao gồm giá trị và cờ chí mạng</returns>
        public static DamageData CalculateDamage(float baseDamage, float critChance, float critDamageMultiplier = CRIT_MULTIPLIER)
        {
            bool isCrit = Random.value <= critChance;
            float finalDamage = isCrit ? baseDamage * critDamageMultiplier : baseDamage;

            return new DamageData(finalDamage, isCrit);
        }
    }
}
