using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Player.Core
{
    /// <summary>
    /// Triển khai IPlayerRegistry dành riêng cho chế độ Vượt Ải Đơn (Solo Campaign) và Chạy Offline.
    /// Tối ưu hóa 0 GC Allocations, tái sử dụng danh sách tĩnh và phản hồi tức thì.
    /// </summary>
    public class SinglePlayerRegistry : IPlayerRegistry
    {
        private readonly List<PlayerContext> _activePlayers = new List<PlayerContext>(4);

        public IReadOnlyList<PlayerContext> ActivePlayers => _activePlayers;
        public PlayerContext LocalPlayer => _activePlayers.Count > 0 ? _activePlayers[0] : null;
        public bool HasAnyPlayer => _activePlayers.Count > 0 && _activePlayers[0] != null;

        public event Action<PlayerContext> OnPlayerRegistered;
        public event Action<PlayerContext> OnPlayerUnregistered;

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

            // Nếu đã tồn tại thì không thêm trùng
            if (_activePlayers.Contains(player)) return;

            // Trong SinglePlayer, nếu đã có player cũ thì dọn trước
            if (_activePlayers.Count > 0)
            {
                var oldPlayer = _activePlayers[0];
                _activePlayers.Clear();
                OnPlayerUnregistered?.Invoke(oldPlayer);
            }

            _activePlayers.Add(player);
            OnPlayerRegistered?.Invoke(player);
        }

        public void Unregister(PlayerContext player)
        {
            if (player == null) return;

            if (_activePlayers.Remove(player))
            {
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
        }
    }
}
