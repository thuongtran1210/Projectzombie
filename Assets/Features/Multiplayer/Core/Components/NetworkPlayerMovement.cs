using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Player.Input;

namespace ProjectZombie.Features.Multiplayer.Core.Components
{
    /// <summary>
    /// Component quản lý điều phối di chuyển mạng và input provider (Local vs Remote).
    /// Tuân thủ Single Responsibility Principle (SRP - Mục 3.1 AGENTS.md).
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class NetworkPlayerMovement : MonoBehaviour
    {
        private PlayerController _controller;
        private PlayerInputReader _localInputReader;
        private NetworkInputBridge _networkInputBridge;

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _localInputReader = GetComponent<PlayerInputReader>();
            _networkInputBridge = GetComponent<NetworkInputBridge>();
            if (_networkInputBridge == null)
            {
                _networkInputBridge = gameObject.AddComponent<NetworkInputBridge>();
            }
        }

        public void SetupAuthority(bool isLocal)
        {
            if (_controller != null)
            {
                _controller.SetNetworkMovementMode(true);
            }

            if (isLocal)
            {
                if (_localInputReader != null)
                {
                    _localInputReader.enabled = true;
                    _localInputReader.IsNetworkMode = true;
                    _controller.SetInputProvider(_localInputReader);
                }
                _networkInputBridge.enabled = false;
            }
            else
            {
                if (_localInputReader != null)
                {
                    _localInputReader.enabled = false;
                    _localInputReader.IsNetworkMode = false;
                }
                _networkInputBridge.enabled = true;
                _controller.SetInputProvider(_networkInputBridge);
            }
        }

        public void ResetMovementMode()
        {
            if (_controller != null)
            {
                _controller.SetNetworkMovementMode(false);
            }
            if (_localInputReader != null)
            {
                _localInputReader.IsNetworkMode = false;
            }
        }

        public void ApplyMovement(Vector2 direction)
        {
            if (_controller != null)
            {
                _controller.ApplyNetworkMovement(direction);
            }
        }

        public void FeedInputBridge(NetworkInputData inputData)
        {
            if (_networkInputBridge != null && _networkInputBridge.enabled)
            {
                _networkInputBridge.ApplyNetworkInput(inputData);
            }
        }
    }
}
