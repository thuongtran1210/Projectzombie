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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeCoreServices()
        {
            // Thiết lập chuyển đổi URL Firebase Storage cho Addressables sớm nhất có thể
            UnityEngine.AddressableAssets.Addressables.InternalIdTransformFunc = location =>
            {
                if (string.IsNullOrEmpty(location.InternalId)) return location.InternalId;

                if (location.InternalId.Contains("firebasestorage.googleapis.com") || location.InternalId.Contains("vongxuyen.firebasestorage.app"))
                {
                    string rawUrl = location.InternalId;
                    
                    // Tìm file .bundle hoặc .json hoặc .hash trong chuỗi URL
                    var match = System.Text.RegularExpressions.Regex.Match(rawUrl, @"(?<filename>[\w\-\._]+\.(bundle|hash|json))", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        string fileName = match.Groups["filename"].Value;
                        string correctedUrl = $"https://firebasestorage.googleapis.com/v0/b/vongxuyen.firebasestorage.app/o/Android%2F{fileName}?alt=media";
                        return correctedUrl;
                    }
                }
                return location.InternalId;
            };

            // Kiểm tra xem trong Scene đã có sẵn Core Root chưa (tránh tạo thừa)
            var existingRoot = GameObject.Find(CORE_ROOT_NAME);
            if (existingRoot != null) return;

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

            // 3. Khởi tạo liên kết dữ liệu giữa GameManager và các Domain Services
            if (gameMgr.SaveData != null)
            {
                currencyMgr.Initialize(gameMgr.SaveData);
                relicMgr.Initialize(gameMgr.SaveData);
                gachaMgr.Initialize(gameMgr.SaveData);
            }

            // Bước 2.5: GameStartupFlowController (quản lý luồng khởi động & kiểm tra bản vá CDN)
            coreRoot.AddComponent<ProjectZombie.Features.Startup.GameStartupFlowController>();

            Debug.Log("<color=#00FF88>[CoreBootstrapper]</color> Đã khởi tạo hoàn tất toàn bộ Core Services trong '--- APP CORE SERVICES ---'!");
        }
    }
}
