using System;
using UnityEngine;
using NativeWebSocket; // Thư viện vừa cài đặt
using TikTokBridge.Core;

namespace TikTokBridge.Network
{
    public class NativeWebSocketClient : IWebSocketClient
    {
        private WebSocket _websocket;

        public event Action<string> OnMessageReceived;
        public event Action OnConnected;
        public event Action<string> OnError;

        public async void Connect(string url)
        {
            // Nếu đang kết nối rồi thì bỏ qua
            if (_websocket != null && _websocket.State == WebSocketState.Open) return;

            _websocket = new WebSocket(url);

            // Gắn các hàm callback của thư viện vào Event nội bộ của chúng ta
            _websocket.OnOpen += () =>
            {
                Debug.Log($"[RealWebSocket] Đã kết nối thành công tới: {url}");
                OnConnected?.Invoke();
            };

            _websocket.OnError += (e) =>
            {
                Debug.LogError($"[RealWebSocket] Lỗi kết nối: {e}");
                OnError?.Invoke(e);
            };

            _websocket.OnClose += (e) =>
            {
                Debug.Log($"[RealWebSocket] Đã ngắt kết nối!");
            };

            // Khi nhận được cục Data (Byte Array) từ Server Node.js
            _websocket.OnMessage += (bytes) =>
            {
                // Chuyển mảng Byte thành chuỗi string (JSON)
                string message = System.Text.Encoding.UTF8.GetString(bytes);
                Debug.Log($"[RealWebSocket] Nhận dữ liệu: {message}");

                // Bắn chuỗi JSON này sang cho CommandDispatcher xử lý
                OnMessageReceived?.Invoke(message);
            };

            // Yêu cầu kết nối bất đồng bộ
            await _websocket.Connect();
        }

        public async void Disconnect()
        {
            if (_websocket != null)
            {
                await _websocket.Close();
                _websocket = null;
            }
        }

        // LƯU Ý QUAN TRỌNG: NativeWebSocket cần hàm này được gọi liên tục mỗi frame
        // để xử lý các tin nhắn đưa về luồng chính (Main Thread) của Unity.
        public void UpdateMessageQueue()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (_websocket != null && _websocket.State == WebSocketState.Open)
            {
                _websocket.DispatchMessageQueue();
            }
#endif
        }
    }
}