// File: Assets/TikTokBridge/Models/GameCommandPayload.cs
using Newtonsoft.Json.Linq;

namespace TikTokBridge.Models
{
    public class GameCommandPayload
    {
        public string type { get; set; }
        public string user { get; set; }
        public string targetUser { get; set; }
        public string gameMode { get; set; }
        public string source { get; set; }
        public long timestamp { get; set; }
        public string avatar { get; set; }
        
        // Các trường bổ sung từ server thực tế
        public string giftName { get; set; }
        public int amount { get; set; }
        public string enemy { get; set; }

        public JObject additionalData { get; set; }
    }
}