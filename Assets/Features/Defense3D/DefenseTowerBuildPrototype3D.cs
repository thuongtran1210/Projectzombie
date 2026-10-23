using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Keyboard tower building, using only the shared Hero WoodWallet.
    /// Enemy kills create physical drops; no wood is granted directly on death.
    /// </summary>
    public sealed class DefenseTowerBuildPrototype3D : MonoBehaviour
    {
        [SerializeField] private DefenseSceneInfo3D sceneInfo;
        [SerializeField] private DefenseWoodWallet3D wallet;
        [SerializeField, Min(1)] private int towerWoodCost = 25;
        [SerializeField, Min(0.5f)] private float buildRadius = 5f;
        private bool[] occupied;
        

        private void Start()
        {
            if (sceneInfo == null) sceneInfo = FindObjectOfType<DefenseSceneInfo3D>();
            if (sceneInfo == null || sceneInfo.HeroSpawn == null || sceneInfo.TowerSocketCount == 0)
            {
                Debug.LogError("[TREBRAVE Defense3D] Tower builder requires active SceneInfo, Hero and tower sockets.");
                enabled = false;
                return;
            }
            occupied = new bool[sceneInfo.TowerSocketCount];
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
            if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
                TryBuildNearest();
        }

        private void TryBuildNearest()
        {
            if (wallet == null || wallet.Wood < towerWoodCost)
            {
                Debug.Log("[TREBRAVE Defense3D] Not enough test wood to build tower.");
                return;
            }
            int candidate = -1;
            float best = buildRadius * buildRadius;
            for (int i = 0; i < occupied.Length; i++)
            {
                if (occupied[i]) continue;
                Transform socket = sceneInfo.GetTowerSocket(i);
                if (socket == null) continue;
                Vector3 offset = socket.position - sceneInfo.HeroSpawn.position;
                offset.y = 0f;
                if (offset.sqrMagnitude >= best) continue;
                best = offset.sqrMagnitude;
                candidate = i;
            }
            if (candidate < 0)
            {
                Debug.Log("[TREBRAVE Defense3D] Stand within " + buildRadius + "m of an empty TowerSocket and press B.");
                return;
            }

            if (!wallet.TrySpend(towerWoodCost)) return;
            var marker = sceneInfo.GetTowerSocket(candidate);
            var tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tower.name = "Defense_Test_Tower_" + (candidate + 1).ToString("00");
            tower.transform.position = marker.position + Vector3.up * 0.8f;
            tower.transform.localScale = new Vector3(0.65f, 1.4f, 0.65f);
            var collider = tower.GetComponent<Collider>();
            if (collider != null) Destroy(collider); // Visual-only prototype; does not change baked NavMesh.
            var renderer = tower.GetComponent<Renderer>();
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (renderer != null && shader != null)
            {
                var material = new Material(shader);
                material.color = new Color(0.95f, 0.7f, 0.25f);
                renderer.material = material;
            }
            tower.AddComponent<DefenseTowerPrototype3D>();
            occupied[candidate] = true;
            Debug.Log("[TREBRAVE Defense3D] Tower built at socket " + (candidate + 1) + ". Wood: " + wallet.Wood);
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(16, 116, 320, 72), "Tower Building (Prototype)");
            GUI.Label(new Rect(28, 143, 290, 22), "Wood: " + (wallet != null ? wallet.Wood : 0) + " | Tower Cost: " + towerWoodCost);
            GUI.Label(new Rect(28, 164, 290, 20), "Press B near an empty tower socket.");
        }
    }
}
