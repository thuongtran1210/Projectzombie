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
    /// Tự động spawn nhân vật ngay khi mở game để đứng tại Sảnh Hoàng Tuyền (Hub Stage).
    /// </summary>
    public class GameplayBootstrapper : MonoBehaviour
    {
        [Header("Character Spawner Settings")]
        [Tooltip("Database toàn bộ Anh Hùng (Chuẩn Drag & Drop SO)")]
        [SerializeField] private CharacterDatabaseSO characterDatabase;
        
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
        private CharacterSelectionPresenter _characterSelectionPresenter;

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
            _characterSelectionPresenter = FindObjectOfType<CharacterSelectionPresenter>(true);
            if (_characterSelectionPresenter != null)
            {
                _characterSelectionPresenter.OnCharacterSelected -= HandleCharacterSelected;
                _characterSelectionPresenter.OnCharacterSelected += HandleCharacterSelected;
            }

            // 2. Tự động spawn thực thể nhân vật đứng sẵn ở Sảnh (Hub Stage) ngay khi mở game
            SpawnPlayerForActiveHero();

            // 3. Nếu đang ở MainMenu (Sảnh), chưa bắt đầu wave quái
            bool isMainMenu = GameStateManager.Instance == null || GameStateManager.Instance.CurrentState == GameState.MainMenu;
            if (!isMainMenu)
            {
                StartMatchFlow();
            }
        }

        private void OnDestroy()
        {
            if (_characterSelectionPresenter != null)
            {
                _characterSelectionPresenter.OnCharacterSelected -= HandleCharacterSelected;
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi người chơi chọn tướng mới từ UI Character Selection.
        /// </summary>
        private void HandleCharacterSelected(GameObject selectedPrefab)
        {
            if (selectedPrefab == null) return;
            SpawnPlayer(selectedPrefab);
        }

        public void SpawnPlayerForActiveHero()
        {
            GameObject prefab = ResolvePlayerPrefab();
            if (prefab != null)
            {
                SpawnPlayer(prefab);
            }
            else
            {
                Debug.LogError("[GameplayBootstrapper] Không thể Resolve Player Prefab! Hãy kiểm tra CharacterDatabase hoặc defaultPlayerPrefab.");
            }
        }

        private GameObject ResolvePlayerPrefab()
        {
            RunLoadoutState.EnsureInitialized();

            // Ưu tiên 1: Lấy từ RunLoadoutState (Đã được khởi tạo từ Save/Default SO)
            if (RunLoadoutState.SelectedCharacter != null && RunLoadoutState.SelectedCharacter.playerPrefab != null)
            {
                return RunLoadoutState.SelectedCharacter.playerPrefab;
            }

            // Ưu tiên 2: Lấy tướng đầu tiên từ CharacterDatabaseSO
            if (characterDatabase == null)
            {
                characterDatabase = Resources.Load<CharacterDatabaseSO>("CharacterDatabase");
                #if UNITY_EDITOR
                if (characterDatabase == null)
                {
                    characterDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterDatabaseSO>("Assets/_Data/CharacterDatabase.asset");
                }
                #endif
            }
            if (characterDatabase != null && characterDatabase.Characters != null && characterDatabase.Characters.Count > 0)
            {
                if (characterDatabase.Characters[0] != null && characterDatabase.Characters[0].playerPrefab != null)
                {
                    return characterDatabase.Characters[0].playerPrefab;
                }
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

            // Tự động bảo đảm các component cơ bản
            if (!_activePlayerInstance.TryGetComponent<PlayerInputReader>(out _))
            {
                _activePlayerInstance.AddComponent<PlayerInputReader>();
            }

            if (!_activePlayerInstance.TryGetComponent<CharacterCombat>(out var combat))
            {
                combat = _activePlayerInstance.AddComponent<CharacterCombat>();
            }

            if (RunLoadoutState.SelectedCharacter != null && RunLoadoutState.SelectedCharacter.basicAttackConfig != null && combat != null)
            {
                combat.SetAttackConfig(RunLoadoutState.SelectedCharacter.basicAttackConfig);
            }

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
        /// Khôi phục thực thể Player về trạng thái chuẩn hoặc spawn mới nếu chưa tồn tại.
        /// </summary>
        private void ResetOrRespawnPlayer()
        {
            bool needRespawn = _activePlayerInstance == null || 
                               !_activePlayerInstance.activeInHierarchy || 
                               (_activePlayerInstance.TryGetComponent<HealthSystem>(out var hpCheck) && hpCheck.CurrentHealth <= 0);

            if (needRespawn)
            {
                SpawnPlayerForActiveHero();
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

                if (_activePlayerInstance.TryGetComponent<WeaponManager>(out var wm))
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

        /// <summary>
        /// Kích hoạt đếm giờ và bắt đầu xuất hiện yêu ma khi người chơi bấm Xuất Trận.
        /// </summary>
        public void StartMatchFlow()
        {
            Time.timeScale = 1f;

            // Dọn sạch hiệu ứng / đạn bay tàn dư từ trận trước
            ProjectZombie.Features.Projectiles.Core.ProjectileSystem.Instance?.DespawnAllProjectiles();
            ProjectZombie.Features.Shared.VFX.GlobalVFXPoolManager.Instance?.ClearAllActiveEffects();

            // Khôi phục Player
            ResetOrRespawnPlayer();

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
            ResetOrRespawnPlayer();
        }
    }
}
