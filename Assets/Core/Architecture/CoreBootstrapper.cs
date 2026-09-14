using UnityEngine;
using ProjectZombie.Core.Save;
using ProjectZombie.Features.MetaProgression;
using ProjectZombie.Features.MetaProgression.Gacha;

namespace ProjectZombie.Core.Architecture
{
    /// <summary>
    /// Bootstrapper tự động khởi tạo toàn bộ Core Services của trò chơi trước khi Scene đầu tiên tải.
    /// Gom tất cả Manager vào một GameObject duy nhất mang tên '--- APP CORE SERVICES ---'.
    /// Đảm bảo tính nhất quán tuyệt đối về thứ tự khởi tạo theo chuẩn kiến trúc Mục 3.3.
    /// </summary>
    public static class CoreBootstrapper
    {
        private const string CORE_ROOT_NAME = "--- APP CORE SERVICES ---";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticDomainState()
        {
            AppBootGate.ResetState();
            _isCoreServicesInitialized = false;
        }

        private static bool _isCoreServicesInitialized = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeCoreServices()
        {
            // Ghim cứng 60 FPS và giữ màn hình luôn sáng, chống tự động khóa máy khi chơi game
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // Cơ chế tự động xoay màn hình theo 2 hướng ngang (LandscapeLeft & LandscapeRight) dựa trên cảm biến điện thoại
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.AutoRotation;

            // Thiết lập chuyển đổi URL Firebase Storage duy nhất qua AddressablePatchManager
            ProjectZombie.Core.Services.Addressables.AddressablePatchManager.SetupInternalIdTransformStatic();


            // Kiểm tra xem trong Scene đã có sẵn Manager Root chưa (tránh tạo thừa và xung đột Singleton)
            if (_isCoreServicesInitialized || GameManager.HasInstance)
            {
                return;
            }
            _isCoreServicesInitialized = true;

            // 1. Tạo GameObject Container tập trung
            var coreRoot = new GameObject(CORE_ROOT_NAME);
            Object.DontDestroyOnLoad(coreRoot);

            // 2. Khởi tạo các Manager theo đúng thứ tự phụ thuộc dữ liệu
            // Bước 2.1: GameManager (nạp file player_save.json)
            var gameMgr = coreRoot.AddComponent<GameManager>();

            // Bước 2.2: MetaCurrencyManager (đồng bộ số dư Cổ Tiền)
            var currencyMgr = coreRoot.AddComponent<MetaCurrencyManager>();

            // Bước 2.3: RelicInventoryManager (quản lý kho mảnh & cấp sao pháp bảo)
            var relicMgr = coreRoot.AddComponent<RelicInventoryManager>();

            // Bước 2.4: RelicGachaManager (quản lý rương gacha & pity)
            var gachaMgr = coreRoot.AddComponent<RelicGachaManager>();

            // Bước 2.5: CharacterProgressionManager (quản lý nâng sao và tăng chỉ số tướng)
            var charProgMgr = coreRoot.AddComponent<CharacterProgressionManager>();

            // 3. Khởi tạo liên kết dữ liệu giữa GameManager và các Domain Services
            if (gameMgr.SaveData != null)
            {
                currencyMgr.Initialize(gameMgr.SaveData);
                relicMgr.Initialize(gameMgr.SaveData);
                gachaMgr.Initialize(gameMgr.SaveData);
                charProgMgr.Initialize(gameMgr.SaveData);
            }

            // Bước 2.6: GameStartupFlowController (quản lý luồng khởi động & kiểm tra bản vá CDN)
            coreRoot.AddComponent<ProjectZombie.Features.Startup.GameStartupFlowController>();

            // 4. Đăng ký GameDataService vào ServiceContext
            ServiceContext.Register<ProjectZombie.Core.Services.Data.IGameDataService>(ProjectZombie.Core.Services.Data.GameDataService.Instance);

            AppBootGate.SetReady();
            Debug.Log("<color=#00FF88>[CoreBootstrapper]</color> Đã khởi tạo hoàn tất toàn bộ Core Services trong '--- APP CORE SERVICES ---'!");
        }

    }

    /// <summary>
    /// Cổng kiểm soát trạng thái sẵn sàng của toàn bộ Core Services trong trò chơi (Boot Gate).
    /// Các Presenter / UI / Subsystems có thể kiểm tra hoặc subscribe sự kiện này để đảm bảo không gọi dịch vụ trước khi nạp xong.
    /// </summary>
    public static class AppBootGate
    {
        public static bool IsCoreReady { get; private set; } = false;
        public static event System.Action OnCoreReady;

        public static void ResetState()
        {
            IsCoreReady = false;
            OnCoreReady = null;
        }

        internal static void SetReady()
        {
            if (IsCoreReady) return;
            IsCoreReady = true;
            OnCoreReady?.Invoke();
        }
    }
}

