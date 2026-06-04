using System;
using Newtonsoft.Json.Linq;
using TikTokBridge.Core;
using TikTokBridge.Models;
using UnityEngine;

namespace TikTokBridge.Logic
{
    public class CommandDispatcher : ICommandDispatcher
    {
        public event Action<GameCommandPayload> OnSpawnEnemy;
        public event Action<GameCommandPayload> OnSpawnBoss;
        public event Action<GameCommandPayload> OnShowChat;
        public event Action<GameCommandPayload> OnServerStatusChanged;

        // TikTok Events
        public event Action<GameCommandPayload> OnLikeReceived;
        public event Action<GameCommandPayload> OnFollowReceived;
        public event Action<GameCommandPayload> OnGiftReceived;

        public void ProcessRawJson(string jsonPayload)
        {
            try
            {
                JObject jsonObj = JObject.Parse(jsonPayload);
                var command = new GameCommandPayload
                {
                    type = jsonObj["type"]?.ToString(),
                    user = jsonObj["user"]?.ToString(),
                    targetUser = jsonObj["targetUser"]?.ToString(),
                    gameMode = jsonObj["gameMode"]?.ToString(),
                    source = jsonObj["source"]?.ToString(),
                    timestamp = jsonObj["timestamp"]?.ToObject<long>() ?? 0,
                    avatar = jsonObj["avatar"]?.ToString(),
                    additionalData = jsonObj
                };

                RouteCommand(command);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CommandDispatcher] Lỗi parse JSON: {ex.Message}");
            }
        }

        private void RouteCommand(GameCommandPayload cmd)
        {
            switch (cmd.type?.ToUpper())
            {
                case "SPAWN_ENEMY": OnSpawnEnemy?.Invoke(cmd); break;
                case "SPAWN_BOSS": OnSpawnBoss?.Invoke(cmd); break;
                case "SHOW_CHAT": OnShowChat?.Invoke(cmd); break;
                case "SERVER_STATUS":
                case "GET_RULES_RESPONSE": OnServerStatusChanged?.Invoke(cmd); break;
                
                case "LIKE": OnLikeReceived?.Invoke(cmd); break;
                case "FOLLOW": OnFollowReceived?.Invoke(cmd); break;
                case "GIFT":
                case "SHOW_GIFT": OnGiftReceived?.Invoke(cmd); break;
                
                default: Debug.LogWarning($"[CommandDispatcher] Unhandled type: {cmd.type}"); break;
            }
        }
    }
}