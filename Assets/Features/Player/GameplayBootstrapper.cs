using UnityEngine;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.UI.HUD;
using ProjectZombie.Features.UI.StatsAndSkills;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Arena;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// Chịu trách nhiệm điều phối khởi tạo gameplay scene (Composition Root).
    /// Tuân thủ Single Responsibility: Chỉ điều phối luồng vòng đời, các nhiệm vụ chi tiết
    /// được ủy thác cho PlayerContext, GameplayUIBinder, PlayerProvider, GameStateManager.
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
        private GameplayUIBinder _uiBinder;

        private void Awake()
        {
            _uiBinder = new GameplayUIBinder(
                runHUDPresenter,
                playerInfoUIPresenter,
                upgradeUIPresenter,
                gameOverScreenPresenter,
                characterGaugeWidgetPresenter
            );
        }

        private void Start()
        {
            // 1. Đăng ký lắng nghe sự kiện chọn tướng nếu có UI trong Scene
            var existingUI = FindObjectOfType<CharacterSelectionPresenter>(true);
            if (existingUI != null)
            {
                existingUI.OnCharacterSelected -= HandleCharacterSelected;
                existingUI.OnCharacterSelected += HandleCharacterSelected;
            }

            // 2. Nếu có MetaUIManager quản lý Sảnh Menu (Hướng A), nhường quyền điều phối cho MetaUIManager & MetaSceneTransitionController
            if (MetaUIManager.Instance != null || FindObjectOfType<MetaUIManager>(true) != null)
            {
                Debug.Log("[GameplayBootstrapper] Phát hiện MetaUIManager: Nhường quyền khởi động cho Sảnh Hoàng Tuyền (Main Hub).");
                return;
            }

            // 3. Fallback cho Test Scene độc lập (nếu không có Meta Menu)
            if (TryInitializeCharacterSelectionUI())
            {
                return;
            }

            InitializeLevel(null);
        }

        public void SpawnPlayerFromSelection(GameObject selectedPrefab)
        {
            if (_activePlayerInstance != null)
            {
                Debug.Log("[GameplayBootstrapper] Player instance đã tồn tại, không spawn lại.");
                return;
            }
            InitializeLevel(selectedPrefab);
        }

        private void HandleCharacterSelected(GameObject selectedPrefab)
        {
            SpawnPlayerFromSelection(selectedPrefab);
        }

        private bool TryInitializeCharacterSelectionUI()
        {
            var existingUI = FindObjectOfType<CharacterSelectionPresenter>(true);
            if (existingUI != null)
            {
                existingUI.gameObject.SetActive(true);
                existingUI.OnCharacterSelected -= HandleCharacterSelected;
                existingUI.OnCharacterSelected += HandleCharacterSelected;
                Debug.Log("[GameplayBootstrapper] Đã đăng ký lắng nghe sự kiện OnCharacterSelected từ CharacterSelectionPresenter có sẵn trong scene.");
                return true;
            }

            if (characterSelectionUIPrefab != null)
            {
                GameObject uiObj = Instantiate(characterSelectionUIPrefab);
                var presenter = uiObj.GetComponent<CharacterSelectionPresenter>();
                if (presenter != null)
                {
                    presenter.OnCharacterSelected += HandleCharacterSelected;
                    Debug.Log("[GameplayBootstrapper] Đã spawn CharacterSelectionUI Prefab và đăng ký sự kiện.");
                    return true;
                }
            }

            return false;
        }

        private void InitializeLevel(GameObject overridePrefab)
        {
            // 1. Resolve Prefab hợp lệ
            GameObject playerPrefab = ResolvePlayerPrefab(overridePrefab);
            if (playerPrefab == null)
            {
                Debug.LogError("[GameplayBootstrapper] Player Prefab chưa được gán! Không thể khởi tạo gameplay.");
                return;
            }

            // 2. Spawn Thực thể Player
            SpawnPlayer(playerPrefab);

            // 3. Đóng gói Model qua PlayerContext & Đăng ký PlayerProvider
            PlayerContext context = PlayerContext.Create(_activePlayerInstance);
            PlayerProvider.RegisterPlayer(_activePlayerInstance);

            // 4. Kết nối Camera Target
            SetupCameraFollow(context.Transform);

            // 5. Inject Dependencies vào toàn bộ UI Presenters thông qua GameplayUIBinder
            _uiBinder.BindAll(context);

            // 6. Bắt đầu Vòng lặp trận đấu (Match Flow)
            StartMatchFlow();
        }

        private GameObject ResolvePlayerPrefab(GameObject overridePrefab)
        {
            if (overridePrefab != null) return overridePrefab;
            if (characterSelectionData != null && characterSelectionData.SelectedPlayerPrefab != null)
            {
                return characterSelectionData.SelectedPlayerPrefab;
            }
            return defaultPlayerPrefab;
        }

        private void SpawnPlayer(GameObject playerPrefab)
        {
            if (_activePlayerInstance != null)
            {
                Destroy(_activePlayerInstance);
            }

            Vector3 position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            position.z = 0f;
            Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            _activePlayerInstance = Instantiate(playerPrefab, position, rotation);
            _activePlayerInstance.name = playerPrefab.name;
            _activePlayerInstance.SetActive(true);

            Debug.Log($"[GameplayBootstrapper] Đã spawn nhân vật thành công: {_activePlayerInstance.name} tại {position}");
        }

        private void SetupCameraFollow(Transform target)
        {
            if (cameraFollow == null)
            {
                cameraFollow = FindObjectOfType<CameraFollow>();
            }

            if (cameraFollow != null)
            {
                cameraFollow.SetTarget(target);
                Debug.Log("[GameplayBootstrapper] Đã thiết lập target cho CameraFollow.");
            }
            else
            {
                Debug.LogWarning("[GameplayBootstrapper] Không tìm thấy CameraFollow trong scene để đi theo Player.");
            }
        }

        private void StartMatchFlow()
        {
            if (RunStatsTracker.Instance != null)
            {
                RunStatsTracker.Instance.StartTracking();
                Debug.Log("[GameplayBootstrapper] RunStatsTracker đã bắt đầu đếm thời gian từ 00:00.");
            }

            if (ProjectZombie.Features.Spawners.SpawnManager.Instance != null)
            {
                ProjectZombie.Features.Spawners.SpawnManager.Instance.StartMatch();
                Debug.Log("[GameplayBootstrapper] SpawnManager đã bắt đầu trận đấu.");
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameState.Playing);
            }
        }
    }
}
