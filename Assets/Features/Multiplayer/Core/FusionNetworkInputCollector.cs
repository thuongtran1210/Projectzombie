using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Player.Input;

namespace ProjectZombie.Features.Multiplayer.Core
{
    /// <summary>
    /// Thu thập và đóng gói dữ liệu Input của người chơi cục bộ nạp vào vòng lặp mô phỏng mạng Photon Fusion.
    /// Đơn trách nhiệm: Chỉ lắng nghe Input từ thiết bị và nạp vào NetworkInput.
    /// </summary>
    public class FusionNetworkInputCollector : MonoBehaviour, INetworkRunnerCallbacks
    {
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var inputData = new NetworkInputData();
            Vector2 moveDir = Vector2.zero;
            Vector2 aimDir = Vector2.zero;
            NetworkInputButtons buttons = NetworkInputButtons.None;

            // 1. Đọc từ PlayerInputReader của Local Player
            if (PlayerProvider.HasPlayer && PlayerProvider.PlayerGameObject != null && PlayerProvider.PlayerGameObject.TryGetComponent<PlayerInputReader>(out var inputReader))
            {
                moveDir = inputReader.MovementInput;
                buttons = inputReader.ConsumePendingButtons(out aimDir);
            }

            // 2. Ưu tiên ghi đè moveDir từ Mobile Virtual Joystick nếu có thao tác chạm thực tế
            if (ProjectZombie.Features.UI.DynamicVirtualJoystick.Instance != null && ProjectZombie.Features.UI.DynamicVirtualJoystick.Instance.InputVector.sqrMagnitude > 0.001f)
            {
                moveDir = ProjectZombie.Features.UI.DynamicVirtualJoystick.Instance.InputVector;
            }

            // 3. Fallback: Đọc từ New Input System Keyboard khi chạy trong Unity Editor hoặc Standalone
            if (moveDir.sqrMagnitude < 0.001f)
            {
#if ENABLE_INPUT_SYSTEM
                if (UnityEngine.InputSystem.Keyboard.current != null)
                {
                    var kb = UnityEngine.InputSystem.Keyboard.current;
                    float h = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
                    float v = (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1f : 0f);
                    if (h != 0 || v != 0)
                    {
                        moveDir = new Vector2(h, v);
                    }
                }
#endif
            }

            inputData.MoveDirection = moveDir.sqrMagnitude > 1f ? moveDir.normalized : moveDir;
            inputData.AimDirection = aimDir;
            inputData.Buttons = buttons;
            input.Set(inputData);
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
    }
}
