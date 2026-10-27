using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    public struct DefenseRewardDrop
    {
        public int WoodAmount;
        public int ExpAmount;
        public Vector3 DropPosition;
        public string SourceEnemyId;

        public DefenseRewardDrop(int wood, int exp, Vector3 pos, string enemyId)
        {
            WoodAmount = wood;
            ExpAmount = exp;
            DropPosition = pos;
            SourceEnemyId = enemyId;
        }
    }

    /// <summary>
    /// U3D-008: Pure C# reward & drop distributor.
    /// Guarantees that rewards are issued exactly once upon defeat,
    /// and separates Wood from EXP.
    /// </summary>
    public sealed class DefenseRewardPipeline
    {
        public event Action<DefenseRewardDrop> RewardGranted;

        private bool granted;

        public bool TryGrantReward(DefenseRewardDrop drop)
        {
            if (granted) return false;
            granted = true;
            RewardGranted?.Invoke(drop);
            return true;
        }

        public void Reset()
        {
            granted = false;
        }
    }
}
