using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>One-run Wood wallet. Only physical WoodDrop pickups can grant Wood.</summary>
    public sealed class DefenseWoodWallet3D : MonoBehaviour
    {
        [SerializeField, Min(0)] private int startingWood = 0;
        public int Wood { get; private set; }
        public int CurrentWood => Wood;

        private void Awake()
        {
            ResetWallet();
        }

        /// <summary>Restores the authored initial balance for a new run.</summary>
        public void ResetWallet() => Wood = startingWood;

        public void CollectWood(int amount)
        {
            if (amount <= 0) return;
            Wood += amount;
            Debug.Log("[TREBRAVE Defense3D] Hero collected " + amount + " Wood. Wallet: " + Wood);
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0 || Wood < amount) return false;
            Wood -= amount;
            return true;
        }
    }
}
