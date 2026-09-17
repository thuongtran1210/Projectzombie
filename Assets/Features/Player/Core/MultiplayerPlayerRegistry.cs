using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Player.Core
{
    /// <summary>
    /// Triển khai IPlayerRegistry cho chế độ Multiplayer Co-op (Host-Client).
    /// Quản lý danh sách toàn bộ người chơi (cả cục bộ lẫn từ xa), hỗ trợ truy vấn 0-GC.
    /// </summary>
    public class MultiplayerPlayerRegistry : IPlayerRegistry
    {
        private readonly List<PlayerContext> _activePlayers = new List<PlayerContext>(8);
        private PlayerContext _localPlayer;

        public IReadOnlyList<PlayerContext> ActivePlayers => _activePlayers;
        public PlayerContext LocalPlayer => _localPlayer;
        public bool HasAnyPlayer => _activePlayers.Count > 0;

        public event Action<PlayerContext> OnPlayerRegistered;
        public event Action<PlayerContext> OnPlayerUnregistered;

        /// <summary>
        /// Tìm kiếm người chơi còn sống gần nhất so với tọa độ chỉ định (0-GC Allocations).
        /// Dùng cho AI Quái vật và Vũ khí định hướng mục tiêu trong chế độ Co-op.
        /// </summary>
        public PlayerContext GetNearestLivingPlayer(Vector2 position)
        {
            if (_activePlayers.Count == 0) return null;

            PlayerContext nearest = null;
            float minSqrDist = float.MaxValue;

            for (int i = 0; i < _activePlayers.Count; i++)
            {
                var player = _activePlayers[i];
                if (player == null || player.Transform == null || !player.IsAlive) continue;

                float sqrDist = ((Vector2)player.Transform.position - position).sqrMagnitude;
                if (sqrDist < minSqrDist)
                {
                    minSqrDist = sqrDist;
                    nearest = player;
                }
            }

            return nearest;
        }

        public PlayerContext GetPlayerById(int playerId)
        {
            for (int i = 0; i < _activePlayers.Count; i++)
            {
                if (_activePlayers[i] != null && _activePlayers[i].PlayerId == playerId)
                {
                    return _activePlayers[i];
                }
            }
            return null;
        }

        public void Register(PlayerContext player)
        {
            if (player == null) return;

            if (_activePlayers.Contains(player)) return;

            _activePlayers.Add(player);

            if (player.IsLocal)
            {
                _localPlayer = player;
            }

            OnPlayerRegistered?.Invoke(player);
        }

        public void Unregister(PlayerContext player)
        {
            if (player == null) return;

            if (_activePlayers.Remove(player))
            {
                if (_localPlayer == player)
                {
                    _localPlayer = null;
                    // Nếu còn người chơi khác cục bộ, cập nhật lại
                    for (int i = 0; i < _activePlayers.Count; i++)
                    {
                        if (_activePlayers[i].IsLocal)
                        {
                            _localPlayer = _activePlayers[i];
                            break;
                        }
                    }
                }

                OnPlayerUnregistered?.Invoke(player);
            }
        }

        public void Clear()
        {
            for (int i = _activePlayers.Count - 1; i >= 0; i--)
            {
                var p = _activePlayers[i];
                if (p != null)
                {
                    OnPlayerUnregistered?.Invoke(p);
                }
            }
            _activePlayers.Clear();
            _localPlayer = null;
        }
    }
}
