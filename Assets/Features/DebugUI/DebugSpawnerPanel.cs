using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TikTokBridge.Logic;
using TikTokBridge.Core;

namespace ProjectZombie.Features.DebugUI
{
    /// <summary>
    /// Bảng giao diện Debug giúp Dev test việc sinh quái và giả lập sự kiện TikTok trực tiếp trong Editor/Build
    /// mà không cần phải cắm mạng hay dùng Node.js server.
    /// </summary>
    public class DebugSpawnerPanel : MonoBehaviour
    {
        private ICommandDispatcher _targetDispatcher;

        [Header("UI Buttons - Direct Spawn")]
        [SerializeField] private Button btnSpawnSlime;
        [SerializeField] private Button btnSpawnArcher;
        [SerializeField] private Button btnSpawnElite;
        [SerializeField] private Button btnSpawnBoss;

        [Header("UI Buttons - TikTok Simulation")]
        [SerializeField] private Button btnSimulateLike100;
        [SerializeField] private Button btnSimulateFollow;
        [SerializeField] private Button btnSimulateGiftCombo;

        // Dependency Injection từ Bootstrapper
        public void Construct(ICommandDispatcher dispatcher)
        {
            _targetDispatcher = dispatcher;
            SetupButtons();
        }

        private void SetupButtons()
        {
            // 1. Direct Spawns (Gửi thẳng Type SPAWN_ENEMY)
            if (btnSpawnSlime != null) 
                btnSpawnSlime.onClick.AddListener(() => SendMockPayload("SPAWN_ENEMY", "slime", 1, "debug_user"));
            
            if (btnSpawnArcher != null) 
                btnSpawnArcher.onClick.AddListener(() => SendMockPayload("SPAWN_ENEMY", "archer", 3, "debug_user"));
            
            if (btnSpawnElite != null) 
                btnSpawnElite.onClick.AddListener(() => SendMockPayload("SPAWN_ENEMY", "elite", 1, "debug_user"));

            if (btnSpawnBoss != null) 
                btnSpawnBoss.onClick.AddListener(() => SendMockPayload("SPAWN_BOSS", "boss_tiktok", 1, "debug_user"));

            // 2. TikTok Event Simulations
            if (btnSimulateLike100 != null)
                btnSimulateLike100.onClick.AddListener(() => SendMockCustomPayload("LIKE", "debug_user", "{\"likeCount\": 100}"));

            if (btnSimulateFollow != null)
                btnSimulateFollow.onClick.AddListener(() => SendMockPayload("FOLLOW", "", 1, "new_follower"));

            if (btnSimulateGiftCombo != null)
                btnSimulateGiftCombo.onClick.AddListener(() => SendMockCustomPayload("SHOW_GIFT", "rich_fan", "{\"giftName\": \"Tiktok\", \"amount\": 5}"));
        }

        /// <summary>
        /// Giả lập 1 chuỗi JSON chuẩn và nhét thẳng vào Hàm Parse của Dispatcher.
        /// Cách này giúp test 100% luồng logic như khi nhận từ Node.js thật.
        /// </summary>
        private void SendMockPayload(string type, string enemyName, int amount, string user)
        {
            string json = $@"{{
                ""type"": ""{type}"",
                ""enemy"": ""{enemyName}"",
                ""amount"": {amount},
                ""user"": ""{user}"",
                ""timestamp"": 1717081510000,
                ""source"": ""debug_ui""
            }}";

            _targetDispatcher?.ProcessRawJson(json);
        }

        private void SendMockCustomPayload(string type, string user, string additionalJsonBody)
        {
            // Xóa dấu ngoặc nhọn 2 đầu của additionalJsonBody để nối chuỗi
            additionalJsonBody = additionalJsonBody.Trim().Trim('{', '}');
            
            string json = $@"{{
                ""type"": ""{type}"",
                ""user"": ""{user}"",
                ""timestamp"": 1717081510000,
                ""source"": ""debug_ui"",
                {additionalJsonBody}
            }}";

            _targetDispatcher?.ProcessRawJson(json);
        }
    }
}
