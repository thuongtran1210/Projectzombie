using ProjectZombie.Features.DebugUI;
using TikTokBridge.Core;
using TikTokBridge.Logic;
using TikTokBridge.Network;
using TikTokBridge.Systems.Spawners;
using UnityEngine;

namespace TikTokBridge
{
    public class Bootstrapper : MonoBehaviour
    {
        [Header("--- Cấu Hình Mạng ---")]
        [SerializeField] private string websocketUrl = "ws://localhost:8080";

        [Header("--- Systems ---")]
        [SerializeField] private DebugEventOverlay debugOverlay; 
        [SerializeField] private DebugSpawnerPanel debugSpawnerPanel;
        [SerializeField] private EnemyPoolManager enemyPoolManager;
        [SerializeField] private SpawnManager spawnManager;

        private ICommandDispatcher _commandDispatcher;
        private IWebSocketClient _webSocketClient;

        private void Awake()
        {
            // Khởi tạo Dispatcher phiên bản mới có dùng C# Event
            _commandDispatcher = new CommandDispatcher();
            
            // DÙNG HÀNG THẬT TẠI ĐÂY (hoặc inject Mock Client vào đây khi test)
            _webSocketClient = new NativeWebSocketClient();

            // Ghép nối: Dữ liệu mạng bắn thẳng vào Dispatcher để parse JSON
            _webSocketClient.OnMessageReceived += _commandDispatcher.ProcessRawJson;

            // Tiêm (Inject) Dispatcher vào UI Overlay để lắng nghe event
            if (debugOverlay != null)
            {
                debugOverlay.Construct(_commandDispatcher); 
            }

            // Inject Dispatcher vào SpawnManager
            if (spawnManager != null)
            {
                spawnManager.Construct(_commandDispatcher); 
            }

            // Inject Dispatcher vào DebugSpawnerPanel
            if (debugSpawnerPanel != null)
            {
                debugSpawnerPanel.Construct(_commandDispatcher);
            }
        }

        private void Start()
        {
            // Kết nối thẳng tới Server Node.js khi khởi chạy Game
            _webSocketClient.Connect(websocketUrl);
        }

        private void Update()
        {
            // CỰC KỲ QUAN TRỌNG: Phải gọi hàm này mỗi frame để NativeWebSocket hoạt động
            if (_webSocketClient != null)
            {
                _webSocketClient.UpdateMessageQueue();
            }
        }

        private void OnDestroy()
        {
            if (_webSocketClient != null)
            {
                _webSocketClient.OnMessageReceived -= _commandDispatcher.ProcessRawJson;
                _webSocketClient.Disconnect();
            }
        }
    }
}