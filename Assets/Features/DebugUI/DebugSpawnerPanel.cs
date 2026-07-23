using UnityEngine;
using UnityEngine.UI;
using ProjectZombie.Features.Spawners;

namespace ProjectZombie.Features.DebugUI
{
    /// <summary>
    /// Bảng giao diện Debug giúp Dev test việc sinh quái trực tiếp trong Editor/Build.
    /// </summary>
    public class DebugSpawnerPanel : MonoBehaviour
    {
        [SerializeField] private SpawnManager spawnManager;

        [Header("UI Buttons - Direct Spawn")]
        [SerializeField] private Button btnSpawnSlime;
        [SerializeField] private Button btnSpawnArcher;
        [SerializeField] private Button btnSpawnElite;
        [SerializeField] private Button btnSpawnBoss;

        [Header("Prefabs / PillarConfigs for Debug Spawns")]
        [SerializeField] private PillarConfig slimePillarConfig;
        [SerializeField] private PillarConfig archerPillarConfig;
        [SerializeField] private PillarConfig elitePillarConfig;
        [SerializeField] private PillarConfig bossPillarConfig;

        private void Start()
        {
            if (spawnManager == null)
            {
                spawnManager = FindObjectOfType<SpawnManager>();
            }
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (btnSpawnSlime != null) 
                btnSpawnSlime.onClick.AddListener(() => SpawnDebugPillar(slimePillarConfig));
            
            if (btnSpawnArcher != null) 
                btnSpawnArcher.onClick.AddListener(() => SpawnDebugPillar(archerPillarConfig));
            
            if (btnSpawnElite != null) 
                btnSpawnElite.onClick.AddListener(() => SpawnDebugPillar(elitePillarConfig));

            if (btnSpawnBoss != null) 
                btnSpawnBoss.onClick.AddListener(() => SpawnDebugPillar(bossPillarConfig));
        }

        private void SpawnDebugPillar(PillarConfig config)
        {
            if (spawnManager != null)
            {
                spawnManager.SpawnPillar(config);
            }
            else
            {
                Debug.LogWarning("[DebugSpawnerPanel] SpawnManager chưa được gán!");
            }
        }
    }
}
