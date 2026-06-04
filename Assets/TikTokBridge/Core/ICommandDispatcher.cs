using System;
using TikTokBridge.Models;

namespace TikTokBridge.Core
{
    public interface ICommandDispatcher
    {
        event Action<GameCommandPayload> OnSpawnEnemy;
        event Action<GameCommandPayload> OnSpawnBoss;
        event Action<GameCommandPayload> OnShowChat;
        event Action<GameCommandPayload> OnServerStatusChanged;
        
        // TikTok Events
        event Action<GameCommandPayload> OnLikeReceived;
        event Action<GameCommandPayload> OnFollowReceived;
        event Action<GameCommandPayload> OnGiftReceived;

        void ProcessRawJson(string jsonPayload);
    }
}