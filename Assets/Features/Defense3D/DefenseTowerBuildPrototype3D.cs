using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Keyboard tower building, using only the shared Hero WoodWallet.
    /// Enemy kills create physical drops; no wood is granted directly on death.
    /// </summary>
    public sealed class DefenseTowerBuildPrototype3D : DefenseGameplayBehaviour
    {
        [SerializeField] private DefenseSceneInfo3D sceneInfo;
        [SerializeField] private DefenseWoodWallet3D wallet;
        [SerializeField, Min(1)] private int towerWoodCost = 50;
        [SerializeField, Min(0.5f)] private float buildRadius = 1.7f;
        [SerializeField] private DefenseTowerPrototype3D[] socketTowers;
        [SerializeField] private bool showDebugOverlay;
        private bool[] occupied;
        private DefenseHeroHealth3D _heroHealth;
        private IDefenseHeroCommands _commands;
        public void BindCommands(IDefenseHeroCommands commands) => _commands = commands;
        public int BuildCost => towerWoodCost;
        public int NearestSocket => FindNearestSocket();
        public Transform GetSocket(int index) => sceneInfo != null ? sceneInfo.GetTowerSocket(index) : null;
        public event System.Action InteractRequested;
        public DefenseTowerPrototype3D NearestTower => FindNearestSocket() is int index && index >= 0 ? socketTowers[index] : null;
        

        private void Start()
        {
            if (sceneInfo == null || sceneInfo.HeroSpawn == null || sceneInfo.TowerSocketCount == 0)
            {
                Debug.LogError("[TREBRAVE Defense3D] Tower builder requires active SceneInfo, Hero and tower sockets.");
                enabled = false;
                return;
            }
            occupied = new bool[sceneInfo.TowerSocketCount];
            _heroHealth = sceneInfo.HeroSpawn.GetComponent<DefenseHeroHealth3D>();
            if (socketTowers == null || socketTowers.Length != occupied.Length)
            {
                Debug.LogError("[TREBRAVE Defense3D] Assign one authored Tower per socket in the Inspector.", this);
                enabled = false;
                return;
            }
            for (int i = 0; i < socketTowers.Length; i++)
            {
                if (socketTowers[i] == null)
                {
                    Debug.LogError("[TREBRAVE Defense3D] Missing authored Tower at socket " + i, this);
                    enabled = false;
                    return;
                }
                occupied[i] = socketTowers[i].gameObject.activeSelf;
            }
            if (wallet == null) wallet = sceneInfo.HeroSpawn.GetComponent<DefenseWoodWallet3D>();
            if (wallet == null)
            {
                Debug.LogError("[TREBRAVE Defense3D] Tower builder needs DefenseWoodWallet3D on Hero. Install WoodDrop Economy in scene.");
                enabled = false;
                return;
            }
            Debug.Log("[TREBRAVE Defense3D] Tower builder ready. Wood: " + wallet.Wood + ". Press B near socket.");
        }

        private void Update()
        {
            if (!CanAdvanceGameplay || (_heroHealth != null && _heroHealth.IsDown)) return;
            if (_commands != null && _commands.ConsumeBuild())
            {
                if (InteractRequested != null) InteractRequested.Invoke();
                else TryBuildNearest();
            }
        }

        public void ResetTowers()
        {
            if (socketTowers == null) return;
            for (int i = 0; i < socketTowers.Length; i++)
            {
                if (socketTowers[i] != null) socketTowers[i].gameObject.SetActive(false);
                if (occupied != null && i < occupied.Length) occupied[i] = false;
            }
        }

        public bool TryBuildNearest()
        {
            if (!CanAdvanceGameplay || (_heroHealth != null && _heroHealth.IsDown)) return false;
            if (wallet == null || wallet.Wood < towerWoodCost)
            {
                Debug.Log("[TREBRAVE Defense3D] Not enough test wood to build tower.");
                return false;
            }
            int candidate = FindNearestSocket();
            if (candidate < 0)
            {
                Debug.Log("[TREBRAVE Defense3D] Stand within " + buildRadius + "m of an empty TowerSocket and press B.");
                return false;
            }
            if (socketTowers[candidate].gameObject.activeSelf || !wallet.TrySpend(towerWoodCost)) return false;
            socketTowers[candidate].PrepareForBuild();
            socketTowers[candidate].gameObject.SetActive(true);
            occupied[candidate] = true;
            Debug.Log("[TREBRAVE Defense3D] Tower built at socket " + (candidate + 1) + ". Wood: " + wallet.Wood);
            return true;
        }

        public bool TryUpgradeNearest()
        {
            var tower = NearestTower;
            return CanAdvanceGameplay && _heroHealth != null && !_heroHealth.IsDown && tower != null && tower.TryUpgrade(wallet);
        }

        private int FindNearestSocket()
        {
            if (sceneInfo == null || sceneInfo.HeroSpawn == null || socketTowers == null) return -1;
            int candidate = -1;
            float best = buildRadius * buildRadius;
            for (int i = 0; i < socketTowers.Length; i++)
            {
                var socket = sceneInfo.GetTowerSocket(i);
                if (socket == null || socketTowers[i] == null) continue;
                Vector3 hero = sceneInfo.HeroSpawn.position, center = socket.position;
                if (!DefenseVillageInteractionRules.InZone(new DefensePoint3(hero.x, hero.y, hero.z),
                    new DefensePoint3(center.x, center.y, center.z), buildRadius, false)) continue;
                Vector3 delta = socket.position - sceneInfo.HeroSpawn.position; delta.y = 0f;
                if (delta.sqrMagnitude >= best) continue;
                best = delta.sqrMagnitude; candidate = i;
            }
            return candidate;
        }

        private void OnGUI()
        {
            if (!showDebugOverlay) return;
            GUI.Box(new Rect(16, 116, 320, 72), "Tower Building (Prototype)");
            GUI.Label(new Rect(28, 143, 290, 22), "Wood: " + (wallet != null ? wallet.Wood : 0) + " | Tower Cost: " + towerWoodCost);
            GUI.Label(new Rect(28, 164, 290, 20), "Press B near an empty tower socket.");
        }
    }
}
