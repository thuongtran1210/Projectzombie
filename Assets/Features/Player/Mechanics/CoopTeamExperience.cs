using System;
using UnityEngine;
using ProjectZombie.Features.Player.Core;
using ProjectZombie.Core.Architecture;

namespace ProjectZombie.Features.Player.Mechanics
{
    /// <summary>
    /// Hệ thống chia sẻ điểm Kinh Nghiệm toàn đội (Shared Team Experience) cho chế độ Co-op.
    /// Giúp mọi người chơi trong phòng thăng cấp đồng đều, không bị người nhanh người chậm.
    /// </summary>
    public class CoopTeamExperience : MonoBehaviour
    {
        private PlayerExperience _localPlayerExp;

        private void Awake()
        {
            _localPlayerExp = GetComponent<PlayerExperience>();
        }

        private void OnEnable()
        {
            if (_localPlayerExp != null)
            {
                _localPlayerExp.OnExpGained += HandleLocalExpGained;
            }
        }

        private void OnDisable()
        {
            if (_localPlayerExp != null)
            {
                _localPlayerExp.OnExpGained -= HandleLocalExpGained;
            }
        }

        /// <summary>
        /// Khi người chơi này nhặt được Exp, phát tín hiệu đồng bộ cho các thành viên khác trong đội.
        /// CHỈ kích hoạt từ Local Player để tránh việc lặp đệ quy hoặc nhân đôi EXP giữa các máy.
        /// </summary>
        private void HandleLocalExpGained(float amount)
        {
            if (!ServiceContext.TryGet<IPlayerRegistry>(out var registry) || registry == null)
            {
                return;
            }

            // CHỈ người chơi cục bộ thực tế nhặt ngọc mới có quyền phân phối EXP
            if (registry.LocalPlayer == null || registry.LocalPlayer.GameObject != gameObject)
            {
                return;
            }

            // Kiểm tra xem có đang thực sự trong trận Multiplayer Co-op nhiều người hay không
            if (!ServiceContext.TryGet<ProjectZombie.Features.Multiplayer.Core.INetworkSessionService>(out var session) || 
                !session.IsInRoom || 
                session.CurrentRoom == null || 
                session.CurrentRoom.Players.Count <= 1)
            {
                return;
            }

            for (int i = 0; i < registry.ActivePlayers.Count; i++)
            {
                var teammate = registry.ActivePlayers[i];
                if (teammate == null || teammate.GameObject == gameObject || teammate.Experience == null) continue;

                // Tăng Exp cho đồng đội mà không kích hoạt đệ quy
                teammate.Experience.AddDirectExp(amount);
            }
        }
    }
}
