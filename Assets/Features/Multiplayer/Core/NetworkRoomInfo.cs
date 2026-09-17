using System;
using System.Collections.Generic;

namespace ProjectZombie.Features.Multiplayer.Core
{
    /// <summary>
    /// Trạng thái của một người chơi trong phòng chờ (Lobby).
    /// </summary>
    [System.Serializable]
    public class NetworkPlayerData
    {
        public string PlayerId;
        public string DisplayName;
        public string SelectedCharacterId;
        public string SelectedRelicId;
        public bool IsHost;
        public bool IsReady;
        public int PingMs;
    }

    /// <summary>
    /// Dữ liệu phòng chơi trực tuyến (Lobby State).
    /// </summary>
    [System.Serializable]
    public class NetworkRoomInfo
    {
        public string RoomCode;
        public string RoomName;
        public int MaxPlayers = 4;
        public int CurrentPlayerCount => Players.Count;
        public bool IsGameStarted;
        public List<NetworkPlayerData> Players = new List<NetworkPlayerData>();

        public NetworkPlayerData LocalPlayer { get; set; }
        public NetworkPlayerData HostPlayer => Players.Find(p => p.IsHost);
    }
}
