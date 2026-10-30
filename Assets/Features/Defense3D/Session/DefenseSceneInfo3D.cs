using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Scene markers for validating the Map Sông Quê prototype.</summary>
    public sealed class DefenseSceneInfo3D : MonoBehaviour
    {
        [SerializeField] private Transform heroSpawn;
        [SerializeField] private Transform baseMarker;
        [SerializeField] private Transform eastEnemySpawn;
        [SerializeField] private Transform[] bridges;
        [SerializeField] private Transform[] towerSockets;

        public Transform HeroSpawn => heroSpawn;
        public Transform BaseMarker => baseMarker;
        public Transform EastEnemySpawn => eastEnemySpawn;
        public int BridgeCount => bridges != null ? bridges.Length : 0;
        public int TowerSocketCount => towerSockets != null ? towerSockets.Length : 0;
        public Transform GetTowerSocket(int index) => towerSockets != null && index >= 0 && index < towerSockets.Length ? towerSockets[index] : null;

        public void Configure(Transform hero, Transform defenseBase, Transform enemy, Transform[] bridgeMarkers, Transform[] sockets)
        {
            heroSpawn = hero;
            baseMarker = defenseBase;
            eastEnemySpawn = enemy;
            bridges = bridgeMarkers;
            towerSockets = sockets;
        }

        private void Start()
        {
            if (heroSpawn == null || baseMarker == null || eastEnemySpawn == null ||
                BridgeCount == 0 || TowerSocketCount == 0)
                Debug.LogError("[TREBRAVE Defense3D] Incomplete Map Sông Quê prototype scene markers.");
        }
    }
}
