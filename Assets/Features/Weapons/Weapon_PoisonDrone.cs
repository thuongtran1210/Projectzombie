using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Vũ khí Poison Drone (W010): Drone hỗ trợ di chuyển xung quanh Player và liên tục xả khói độc sát thương AoE.
    /// </summary>
    public class Weapon_PoisonDrone : Weapon_PetSummon
    {
        protected override void PerformAttack()
        {
            base.PerformAttack();
            // Logic xả độc bổ sung nếu cần
        }
    }
}
