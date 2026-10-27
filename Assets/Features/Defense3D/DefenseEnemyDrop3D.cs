using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    [DisallowMultipleComponent, RequireComponent(typeof(DefenseEnemy3D))]
    public sealed class DefenseEnemyDrop3D : DefenseGameplayBehaviour
    {
        private DefenseEnemy3D enemy;
        private bool dropped;
        private DefenseWoodWallet3D _collectorWallet;
        private Transform _pickupRoot;

        /// <summary>Collector belongs to the session; deaths do not perform scene searches.</summary>
        public void BindCollector(DefenseWoodWallet3D collectorWallet, Transform pickupRoot = null)
        {
            _collectorWallet = collectorWallet;
            _pickupRoot = pickupRoot;
            ResetDrop();
        }

        private void OnEnable()
        {
            enemy = GetComponent<DefenseEnemy3D>();
            enemy.Defeated += Drop;
            enemy.Initialized += ResetDrop;
        }
        private void OnDisable()
        {
            if (enemy == null) return;
            enemy.Defeated -= Drop;
            enemy.Initialized -= ResetDrop;
        }
        private void ResetDrop() => dropped = false;
        private void Drop()
        {
            if (!CanAdvanceGameplay || !enemy.HasCurrentLifetime) return;
            if (enemy.Lifetime != null)
            {
                if (!enemy.Lifetime.TryClaimReward(Gameplay)) return;
            }
            else
            {
                if (dropped) return;
                dropped = true;
            }
            try
            {
                if (enemy.Stats.WoodReward > 0)
                {
                    DefenseWoodDrop3D.Spawn(transform.position, enemy.Stats.WoodReward,
                        _collectorWallet != null ? _collectorWallet.transform : null, _collectorWallet, Gameplay,_pickupRoot);
                }
            }
            finally
            {
                // Production release belongs to the session factory. Legacy smoke actors retain their fallback.
                if (!enemy.RequestRelease(DefenseActorReleaseReason.Defeated)) Destroy(gameObject);
            }
        }
    }
}
