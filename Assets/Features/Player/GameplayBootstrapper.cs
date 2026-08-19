using UnityEngine;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.UI.HUD;
using ProjectZombie.Features.UI.StatsAndSkills;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Arena;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// Chịu trách nhiệm khởi tạo gameplay scene.
    /// Sinh ra (spawn) nhân vật dựa trên CharacterSelectionData hoặc fallback và inject các dependencies.
    /// </summary>
    public class GameplayBootstrapper : MonoBehaviour
    {
        [Header("Character Spawner Settings")]
        [Tooltip("Dữ liệu cấu hình nhân vật chọn từ Menu")]
        [SerializeField] private CharacterSelectionData characterSelectionData;
        
        [Tooltip("Prefab UI Chọn Nhân Vật để tự động mở khi bắt đầu")]
        [SerializeField] private GameObject characterSelectionUIPrefab;

        [Tooltip("Prefab mặc định nếu không tìm thấy dữ liệu chọn nhân vật")]
        [SerializeField] private GameObject defaultPlayerPrefab;
        
        [Tooltip("Điểm xuất hiện của nhân vật")]
        [SerializeField] private Transform spawnPoint;

        [Header("Camera Settings")]
        [Tooltip("Hệ thống Camera Follow cần đi theo Player")]
        [SerializeField] private CameraFollow cameraFollow;

        [Header("UI Presenters (Dependencies to Inject)")]
        [SerializeField] private RunHUDPresenter runHUDPresenter;
        [SerializeField] private PlayerInfoUIPresenter playerInfoUIPresenter;
        [SerializeField] private UpgradeUIPresenter upgradeUIPresenter;
        [SerializeField] private GameOverScreenPresenter gameOverScreenPresenter;
        [SerializeField] private CharacterGaugeWidgetPresenter characterGaugeWidgetPresenter;

        private GameObject _activePlayerInstance;

        private void Start()
        {
            // Kiểm tra nếu có CharacterSelectionUI trong scene hoặc prefab
            var existingUI = FindObjectOfType<CharacterSelectionPresenter>(true);
            if (existingUI != null)
            {
                existingUI.gameObject.SetActive(true);
                existingUI.OnCharacterSelected += HandleCharacterSelected;
                return;
            }

            if (characterSelectionUIPrefab != null)
            {
                GameObject uiObj = Instantiate(characterSelectionUIPrefab);
                var presenter = uiObj.GetComponent<CharacterSelectionPresenter>();
                if (presenter != null)
                {
                    presenter.OnCharacterSelected += HandleCharacterSelected;
                    return;
                }
            }

            // Nếu không có UI Chọn Tướng, tự động spawn theo cấu hình sẵn có
            InitializeLevel(null);
        }

        private void HandleCharacterSelected(GameObject selectedPrefab)
        {
            InitializeLevel(selectedPrefab);
        }

        private void InitializeLevel(GameObject overridePrefab)
        {
            // 1. Xác định prefab của Player cần spawn
            GameObject playerPrefab = overridePrefab != null ? overridePrefab : defaultPlayerPrefab;
            if (playerPrefab == null && characterSelectionData != null && characterSelectionData.SelectedPlayerPrefab != null)
            {
                playerPrefab = characterSelectionData.SelectedPlayerPrefab;
            }

            if (playerPrefab == null)
            {
                Debug.LogError("[GameplayBootstrapper] Player Prefab chưa được gán! Không thể khởi tạo gameplay.");
                return;
            }

            // Hủy player cũ nếu đã tồn tại
            if (_activePlayerInstance != null)
            {
                Destroy(_activePlayerInstance);
            }

            // 2. Spawn Player
            Vector3 position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
            _activePlayerInstance = Instantiate(playerPrefab, position, rotation);
            GameObject playerInstance = _activePlayerInstance;
            
            Debug.Log($"[GameplayBootstrapper] Đã spawn nhân vật: {playerInstance.name}");

            // 3. Thu thập các components (Models) từ Player Instance
            PlayerStats stats = playerInstance.GetComponent<PlayerStats>();
            HealthSystem health = playerInstance.GetComponent<HealthSystem>();
            PlayerExperience experience = playerInstance.GetComponent<PlayerExperience>();
            WeaponManager weaponManager = playerInstance.GetComponent<WeaponManager>();
            PlayerPassives passives = playerInstance.GetComponent<PlayerPassives>();
            var gaugeProvider = playerInstance.GetComponent<ProjectZombie.Features.Player.Mechanics.ICharacterGaugeProvider>();

            // Cảnh báo nếu thiếu component quan trọng
            if (stats == null) Debug.LogWarning("[GameplayBootstrapper] Player instance thiếu PlayerStats!");
            if (health == null) Debug.LogWarning("[GameplayBootstrapper] Player instance thiếu HealthSystem!");
            if (experience == null) Debug.LogWarning("[GameplayBootstrapper] Player instance thiếu PlayerExperience!");
            if (weaponManager == null) Debug.LogWarning("[GameplayBootstrapper] Player instance thiếu WeaponManager!");

            // 3.5. Thiết lập camera đi theo nhân vật mới sinh ra
            if (cameraFollow == null)
            {
                cameraFollow = FindObjectOfType<CameraFollow>();
            }

            if (cameraFollow != null)
            {
                cameraFollow.SetTarget(playerInstance.transform);
                Debug.Log("[GameplayBootstrapper] Đã thiết lập target cho CameraFollow.");
            }
            else
            {
                Debug.LogWarning("[GameplayBootstrapper] Không tìm thấy CameraFollow trong scene để đi theo Player.");
            }

            // 4. Inject các Models vào các UI Presenters qua phương thức Construct
            if (runHUDPresenter != null)
            {
                runHUDPresenter.Construct(health, stats, experience, weaponManager, passives);
                Debug.Log("[GameplayBootstrapper] Đã inject dependencies vào RunHUDPresenter.");
            }

            if (playerInfoUIPresenter != null)
            {
                playerInfoUIPresenter.Construct(stats, health, experience, weaponManager);
                Debug.Log("[GameplayBootstrapper] Đã inject dependencies vào PlayerInfoUIPresenter.");
            }

            if (upgradeUIPresenter != null)
            {
                upgradeUIPresenter.Construct(experience, weaponManager);
                Debug.Log("[GameplayBootstrapper] Đã inject dependencies vào UpgradeUIPresenter.");
            }

            if (gameOverScreenPresenter != null)
            {
                gameOverScreenPresenter.Construct(health);
                Debug.Log("[GameplayBootstrapper] Đã inject dependencies vào GameOverScreenPresenter.");
            }

            // 5. Tự động Bind Character Gauge nếu nhân vật có thanh cơ chế (OCP)
            if (characterGaugeWidgetPresenter == null)
            {
                characterGaugeWidgetPresenter = FindObjectOfType<CharacterGaugeWidgetPresenter>(true);
            }

            if (characterGaugeWidgetPresenter != null)
            {
                characterGaugeWidgetPresenter.Bind(gaugeProvider);
                Debug.Log($"[GameplayBootstrapper] CharacterGaugeWidgetPresenter đã Bind provider: {(gaugeProvider != null ? gaugeProvider.GetType().Name : "None")}");
            }

            // 6. Bind Signature Skill Presenter với nhân vật mới spawn
            var skillManager = playerInstance.GetComponent<ProjectZombie.Features.Player.Skills.SignatureSkillManager>();
            var signatureSkillPresenter = FindObjectOfType<ProjectZombie.Features.UI.SignatureSkillPresenter>(true);
            if (signatureSkillPresenter != null)
            {
                signatureSkillPresenter.Bind(skillManager);
                Debug.Log($"[GameplayBootstrapper] SignatureSkillPresenter đã Bind SignatureSkillManager.");
            }
        }
    }
}
