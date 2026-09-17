using System;
using UnityEngine;
using ProjectZombie.Features.Player.Input;

namespace ProjectZombie.Features.Multiplayer.Core
{
    /// <summary>
    /// Cầu nối biến đổi dữ liệu nhận từ mạng (NetworkInputData) thành các sự kiện của IPlayerInputProvider.
    /// Dùng để gắn vào các thực thể nhân vật của người chơi khác (Remote Players) trên máy này.
    /// </summary>
    public class NetworkInputBridge : MonoBehaviour, IPlayerInputProvider
    {
        private Vector2 _movementInput;
        private bool _isInputBlocked;
        private NetworkInputButtons _previousButtons = NetworkInputButtons.None;

        public Vector2 MovementInput => _isInputBlocked ? Vector2.zero : _movementInput;

        public bool IsInputBlocked
        {
            get => _isInputBlocked;
            set => _isInputBlocked = value;
        }

        public event Action OnDashTriggered;
        public event Action OnAttackTriggered;
        public event Action OnSignatureSkillTriggered;
        public event Action OnRelicSkillTriggered;

        /// <summary>
        /// Nạp dữ liệu Input nhận được từ gói tin mạng vào Cầu nối (Gọi mỗi Network Tick).
        /// </summary>
        public void ApplyNetworkInput(NetworkInputData inputData)
        {
            if (_isInputBlocked) return;

            _movementInput = inputData.MoveDirection;

            // Phát hiện cạnh lên (Rising Edge: 0 -> 1) cho các nút bấm hành động
            CheckButtonTrigger(inputData.Buttons, NetworkInputButtons.Dash, OnDashTriggered);
            CheckButtonTrigger(inputData.Buttons, NetworkInputButtons.Attack, OnAttackTriggered);
            CheckButtonTrigger(inputData.Buttons, NetworkInputButtons.SignatureSkill, OnSignatureSkillTriggered);
            CheckButtonTrigger(inputData.Buttons, NetworkInputButtons.RelicSkill, OnRelicSkillTriggered);

            _previousButtons = inputData.Buttons;
        }

        private void CheckButtonTrigger(NetworkInputButtons current, NetworkInputButtons target, Action action)
        {
            bool wasPressed = (_previousButtons & target) == target;
            bool isPressed = (current & target) == target;

            if (!wasPressed && isPressed)
            {
                action?.Invoke();
            }
        }
    }
}
