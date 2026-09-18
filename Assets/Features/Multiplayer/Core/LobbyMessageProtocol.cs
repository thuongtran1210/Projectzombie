using System;
using System.Text;
using UnityEngine;

namespace ProjectZombie.Features.Multiplayer.Core
{
    /// <summary>
    /// Giao thức đóng gói và giải mã các gói tin mạng trong Sảnh Chờ (Lobby Protocol Serialization).
    /// Đơn trách nhiệm: Chỉ xử lý chuyển đổi giữa Network Structs và Mảng Byte.
    /// </summary>
    public static class LobbyMessageProtocol
    {
        public const byte MSG_MATCH_START = 1;
        public const byte MSG_ROOM_STATE_SYNC = 2;
        public const byte MSG_CLIENT_PROFILE_SUBMIT = 3;
        public const byte MSG_CLIENT_READY_SUBMIT = 4;

        public static byte[] EncodeMatchStart(string stageId = "STAGE_01")
        {
            byte[] stageBytes = Encoding.UTF8.GetBytes(string.IsNullOrEmpty(stageId) ? "STAGE_01" : stageId);
            byte[] payload = new byte[1 + stageBytes.Length];
            payload[0] = MSG_MATCH_START;
            Buffer.BlockCopy(stageBytes, 0, payload, 1, stageBytes.Length);
            return payload;
        }

        public static string DecodeMatchStart(ArraySegment<byte> data)
        {
            if (data.Count <= 1 || data.Array == null) return "STAGE_01";
            return Encoding.UTF8.GetString(data.Array, data.Offset + 1, data.Count - 1);
        }

        public static byte[] EncodeRoomState(NetworkRoomInfo room)
        {
            if (room == null) return Array.Empty<byte>();
            string json = JsonUtility.ToJson(room);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            byte[] payload = new byte[1 + jsonBytes.Length];
            payload[0] = MSG_ROOM_STATE_SYNC;
            Buffer.BlockCopy(jsonBytes, 0, payload, 1, jsonBytes.Length);
            return payload;
        }

        public static NetworkRoomInfo DecodeRoomState(ArraySegment<byte> data)
        {
            if (data.Count <= 1 || data.Array == null) return null;
            string json = Encoding.UTF8.GetString(data.Array, data.Offset + 1, data.Count - 1);
            return JsonUtility.FromJson<NetworkRoomInfo>(json);
        }

        public static byte[] EncodeClientProfile(NetworkPlayerData profile)
        {
            if (profile == null) return Array.Empty<byte>();
            string json = JsonUtility.ToJson(profile);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            byte[] payload = new byte[1 + jsonBytes.Length];
            payload[0] = MSG_CLIENT_PROFILE_SUBMIT;
            Buffer.BlockCopy(jsonBytes, 0, payload, 1, jsonBytes.Length);
            return payload;
        }

        public static NetworkPlayerData DecodeClientProfile(ArraySegment<byte> data)
        {
            if (data.Count <= 1 || data.Array == null) return null;
            string json = Encoding.UTF8.GetString(data.Array, data.Offset + 1, data.Count - 1);
            return JsonUtility.FromJson<NetworkPlayerData>(json);
        }

        public static byte[] EncodeClientReady(bool isReady)
        {
            return new byte[] { MSG_CLIENT_READY_SUBMIT, (byte)(isReady ? 1 : 0) };
        }

        public static bool DecodeClientReady(ArraySegment<byte> data)
        {
            if (data.Count < 2 || data.Array == null) return false;
            return data.Array[data.Offset + 1] == 1;
        }
    }
}
