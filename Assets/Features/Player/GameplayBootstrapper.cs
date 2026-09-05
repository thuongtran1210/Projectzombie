using UnityEngine;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.UI.HUD;
using ProjectZombie.Features.UI.StatsAndSkills;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Arena;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Player.Input;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// Chịu trách nhiệm điều phối khởi tạo thực thể Player và gameplay scene (Composition Root).
    /// Tự động spawn nhân vật ngay khi mở game để đứng tại Sảnh Hoàng Tuyền (Hub Stage) theo Hướng 1.
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
            // 1. Đăng ký lắng nghe sự kiện đổi tướng từ UI trong Scene
            var existingUI = FindObjectOfType<CharacterSelectionPresenter>(true);
            if (existingUI != null)
            {
                existingUI.OnCharacterSelected -= HandleCharacterSelected;
                existingUI.OnCharacterSelected += HandleCharacterSelected;
            }

            // 2. Tự động spawn thực thể nhân vật đứng sẵn ở Sảnh (Hub Stage) ngay khi mở game
            SpawnPlayerFromSelection(null);

            // 3. Nếu đang ở MainMenu (Sảnh), chưa bắt đầu wave quái
            bool isMainMenu = GameStateManager.Instance == null || GameStateManager.Instance.CurrentState == GameState.MainMenu;
            if (!isMainMenu)
            {
                StartMatchFlow();
            }
        }

        /// <summary>
        /// Spawn hoặc thay đổi thực thể Player theo Prefab tướng đã chọn.
        /// </summary>
        public void SpawnPlayerFromSelection(GameObject selectedPrefab)
        {
            InitializeLevel(selectedPrefab);
        }

        private void HandleCharacterSelected(GameObject selectedPrefab)
        {
            SpawnPlayerFromSelection(selectedPrefab);
        }

        private void InitializeLevel(GameObject overridePrefab)
        {
            // 1. Resolve Prefab hợp lệ từ RunLoadoutState hoặc database
            GameObject playerPrefab = ResolvePlayerPrefab(overridePrefab);
            if (playerPrefab == null)
            {
                Debug.LogWarning("[GameplayBootstrapper] Chưa tìm thấy Player Prefab để spawn!");
                return;
            }

            // 2. Spawn Thực thể Player
            SpawnPlayer(playerPrefab);

            // 3. Đóng gói Model qua PlayerContext & Đăng ký PlayerProvider
            PlayerContext context = PlayerContext.Create(_activePlayerInstance);
            PlayerProvider.RegisterPlayer(_activePlayerInstance);

            // 4. Kết nối Camera Target theo Player
            SetupCameraFollow(context.Transform);

            // 5. Inject Dependencies vào toàn bộ UI Presenters thông qua GameplayUIBinder
            _uiBinder.BindAll(context);
        }

        private GameObject ResolvePlayerPrefab(GameObject overridePrefab)
        {
            if (overridePrefab != null) return overridePrefab;

            RunLoadoutState.EnsureInitialized();

            // Ưu tiên 1: Tướng đã lưu trong RunLoadoutState
            if (RunLoadoutState.SelectedCharacter != null && RunLoadoutState.SelectedCharacter.playerPrefab != null)
            {
                return RunLoadoutState.SelectedCharacter.playerPrefab;
            }

            // Ưu tiên 2: Dữ liệu từ CharacterSelectionData
            if (characterSelectionData != null && characterSelectionData.SelectedPlayerPrefab != null)
            {
                return characterSelectionData.SelectedPlayerPrefab;
            }

            // Ưu tiên 3: Fallback Resources hoặc AssetDatabase
            var selectionDataRes = Resources.Load<CharacterSelectionData>("CharacterSelectionData");
            #if UNITY_EDITOR
            if (selectionDataRes == null)
            {
                selectionDataRes = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterSelectionData>("Assets/_Data/CharacterSelectionData.asset");
            }
            #endif
            if (selectionDataRes != null && selectionDataRes.SelectedPlayerPrefab != null)
            {
                return selectionDataRes.SelectedPlayerPrefab;
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

            // Tự động gắn PlayerInputReader nếu chưa có
            var inputReader = _activePlayerInstance.GetComponent<PlayerInputReader>();
            if (inputReader == null)
            {
                inputReader = _activePlayerInstance.AddComponent<PlayerInputReader>();
            }

            // Tự động gắn và cấu hình CharacterCombat nếu chưa có
            var combat = _activePlayerInstance.GetComponent<CharacterCombat>();
            if (combat == null)
            {
                combat = _activePlayerInstance.AddComponent<CharacterCombat>();
            }
            if (RunLoadoutState.SelectedCharacter != null && RunLoadoutState.SelectedCharacter.basicAttackConfig != null)
            {
                combat.SetAttackConfig(RunLoadoutState.SelectedCharacter.basicAttackConfig);
            }

            // Tự động gắn TaoistYinYangTracker nếu nhân vật là Đạo Sĩ và chưa có tracker
            if (_activePlayerInstance.name.Contains("Dao Si") || _activePlayerInstance.name.Contains("DaoSi") ||
                (RunLoadoutState.SelectedCharacter != null && RunLoadoutState.SelectedCharacter.characterId.Contains("DaoSi")))
            {
                var tracker = _activePlayerInstance.GetComponent<ProjectZombie.Features.YinYang.TaoistYinYangTracker>();
                if (tracker == null)
                {
                    tracker = _activePlayerInstance.AddComponent<ProjectZombie.Features.YinYang.TaoistYinYangTracker>();
                }
            }

            // Tự động inject dependencies vào toàn bộ hệ thống UI thông qua GameplayUIBinder
            PlayerContext context = PlayerContext.Create(_activePlayerInstance);
            if (_uiBinder == null)
            {
                _uiBinder = new GameplayUIBinder(
                    runHUDPresenter,
                    playerInfoUIPresenter,
                    upgradeUIPresenter,
                    gameOverScreenPresenter,
                    characterGaugeWidgetPresenter
                );
            }
            _uiBinder.BindAll(context);

            Debug.Log($"<color=#00FF88>[GameplayBootstrapper]</color> Đã spawn nhân vật thành công: {_activePlayerInstance.name} tại {position}");
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
                cameraFollow.ResetZoom(0.1f);
            }
        }

        /// <summary>
        /// Kích hoạt đếm giờ và bắt đầu xuất hiện yêu ma khi người chơi bấm Xuất Trận.
        /// </summary>
        public void StartMatchFlow()
        {
            Time.timeScale = 1f;

            // Dọn sạch hiệu ứng / đạn bay tàn dư từ trận trước
            ProjectZombie.Features.Projectiles.Core.ProjectileSystem.Instance?.DespawnAllProjectiles();
            ProjectZombie.Features.Shared.VFX.GlobalVFXPoolManager.Instance?.ClearAllActiveEffects();

            // 1. Kiểm tra thực thể Player: Nếu chưa có hoặc đã chết ở trận trước thì spawn mới lại
            bool needRespawn = _activePlayerInstance == null || 
                               !_activePlayerInstance.activeInHierarchy || 
                               (_activePlayerInstance.TryGetComponent<HealthSystem>(out var hpCheck) && hpCheck.CurrentHealth <= 0);

            if (needRespawn)
            {
                SpawnPlayerFromSelection(null);
            }
            else
            {
                // Khôi phục đầy đủ trạng thái cho Player
                if (_activePlayerInstance.TryGetComponent<PlayerLogic>(out var logic)) logic.ResetState();
                if (_activePlayerInstance.TryGetComponent<HealthSystem>(out var hp)) hp.ResetHealth();
                if (_activePlayerInstance.TryGetComponent<PlayerController>(out var ctrl)) ctrl.enabled = true;
                if (_activePlayerInstance.TryGetComponent<Collider2D>(out var col)) col.enabled = true;
                if (_activePlayerInstance.TryGetComponent<PlayerAnimator>(out var anim)) anim.ChangeAnimationState(PlayerAnimationState.Idle);

                if (spawnPoint != null)
                {
                    _activePlayerInstance.transform.position = spawnPoint.position;
                }

                var wm = _activePlayerInstance.GetComponent<WeaponManager>();
                if (wm != null)
                {
                    wm.enabled = true;
                    wm.ReloadEquippedWeapons();
                }

                PlayerProvider.RegisterPlayer(_activePlayerInstance);
                SetupCameraFollow(_activePlayerInstance.transform);
            }

            if (CameraFollow.Instance != null)
            {
                CameraFollow.Instance.ResetZoom(0.1f);
            }

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

        /// <summary>
        /// Khôi phục thực thể nhân vật đứng sống khỏe mạnh tại Sảnh Hoàng Tuyền khi trở về Menu.
        /// </summary>
        public void ResetPlayerToHub()
        {
            Time.timeScale = 1f;

            bool needRespawn = _activePlayerInstance == null || 
                               !_activePlayerInstance.activeInHierarchy || 
                               (_activePlayerInstance.TryGetComponent<HealthSystem>(out var hpCheck) && hpCheck.CurrentHealth <= 0);

            if (needRespawn)
            {
                SpawnPlayerFromSelection(null);
            }
            else
            {
                if (_activePlayerInstance.TryGetComponent<PlayerLogic>(out var logic)) logic.ResetState();
                if (_activePlayerInstance.TryGetComponent<HealthSystem>(out var hp)) hp.ResetHealth();
                if (_activePlayerInstance.TryGetComponent<PlayerController>(out var ctrl)) ctrl.enabled = true;
                if (_activePlayerInstance.TryGetComponent<Collider2D>(out var col)) col.enabled = true;
                if (_activePlayerInstance.TryGetComponent<PlayerAnimator>(out var anim)) anim.ChangeAnimationState(PlayerAnimationState.Idle);

                if (spawnPoint != null)
                {
                    _activePlayerInstance.transform.position = spawnPoint.position;
                }

                var wm = _activePlayerInstance.GetComponent<WeaponManager>();
                if (wm != null)
                {
                    wm.enabled = true;
                    wm.ReloadEquippedWeapons();
                }

                PlayerProvider.RegisterPlayer(_activePlayerInstance);
                SetupCameraFollow(_activePlayerInstance.transform);
            }

            if (CameraFollow.Instance != null)
            {
                CameraFollow.Instance.ResetZoom(0.1f);
            }
        }
    }
}
