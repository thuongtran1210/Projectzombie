using UnityEngine;

namespace ProjectZombie.Features.Player.Passives
{
    /// <summary>
    /// Nội tại riêng nhân vật dạng cộng thẳng chỉ số tĩnh (Stat Buff Passive).
    /// Ví dụ: Gunslinger (+20% Attack Speed), Tank (+50 Max HP).
    /// </summary>
    [CreateAssetMenu(fileName = "NewStatBuffPassive", menuName = "ProjectZombie/Character Passives/Stat Buff Passive")]
    public class StatBuffCharacterPassive : CharacterPassiveData
    {
        [Header("Stat Modifiers")]
        public float maxHealthBonus;
        public float moveSpeedBonus;
        public float baseDamageBonus;
        public float attackSpeedBonus;
        public float critChanceBonus;
        public float pickupRangeBonus;

        public override void ApplyPassive(GameObject player)
        {
            if (player == null) return;

            var playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                if (maxHealthBonus != 0f) playerStats.AddMaxHealth(maxHealthBonus);
                if (moveSpeedBonus != 0f) playerStats.AddMoveSpeed(moveSpeedBonus);
                if (baseDamageBonus != 0f) playerStats.AddBaseDamage(baseDamageBonus);
                if (attackSpeedBonus != 0f) playerStats.AddAttackSpeed(attackSpeedBonus);
                if (critChanceBonus != 0f) playerStats.AddCritChance(critChanceBonus);
                if (pickupRangeBonus != 0f) playerStats.AddPickupRange(pickupRangeBonus);

                Debug.Log($"[CharacterPassive] Applied StatBuffPassive: {traitName} on {player.name}");
            }
        }
    }
}
