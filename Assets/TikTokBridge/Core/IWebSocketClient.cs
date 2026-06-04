using System;

namespace TikTokBridge.Core
{
    public interface IWebSocketClient
    {
        void Connect(string url);
        void Disconnect();
        event Action<string> OnMessageReceived;
        event Action OnConnected;
        event Action<string> OnError;
        void UpdateMessageQueue();
    }
}