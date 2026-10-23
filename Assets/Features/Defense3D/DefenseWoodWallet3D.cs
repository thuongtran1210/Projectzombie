using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>One-run Wood wallet. Only physical WoodDrop pickups can grant Wood.</summary>
    public sealed class DefenseWoodWallet3D : MonoBehaviour
    {
        [SerializeField, Min(0)] private int startingWood = 0;
        public int Wood { get; private set; }

        private void Awake() { Wood = startingWood; }

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
