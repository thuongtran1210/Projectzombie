using System;
using UnityEngine;
using TikTokBridge.Core;

namespace TikTokBridge.Network
{
    // Lớp này giả lập WebSocket để test
    public class MockWebSocketClient : IWebSocketClient
    {
        public event Action<string> OnMessageReceived;
        public event Action OnConnected;
        public event Action<string> OnError;

        public void Connect(string url)
        {
            Debug.Log($"[MockWebSocket] Giả vờ kết nối tới: {url}");
            OnConnected?.Invoke();
        }

        public void Disconnect()
        {
            Debug.Log("[MockWebSocket] Đã ngắt kết nối.");
        }

        // Hàm này để chúng ta tự gọi giả lập từ Bootstrapper
        public void SimulateReceiveMessage(string json)
        {
            OnMessageReceived?.Invoke(json);
        }

        public void UpdateMessageQueue()
        {
            // No-op for mock
        }
    }
}