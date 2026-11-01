using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Optional stateless strategy extension; per-actor state belongs to the minion, never the shared asset.</summary>
    public abstract class DefenseMinionBehaviorSO : ScriptableObject, IMinionRoleBehavior
    {
        public abstract DefenseMinionRole Role { get; }
        public abstract void Tick(DefenseMinion3D minion, DefenseVillageShop3D shop, IDefenseGameplayContext gameplay);
    }
}
