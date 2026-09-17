using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.Multiplayer.Core;

namespace ProjectZombie.Features.UI.Lobby
{
    /// <summary>
    /// View thụ động hiển thị thông tin một người chơi trong danh sách phòng chờ (Lobby Slot).
    /// Tuân thủ quy chuẩn MVP và tối ưu UI Android (AGENTS.md).
    /// </summary>
    public class LobbyPlayerSlotView : MonoBehaviour
    {
        [Header("UI Text References")]
        [SerializeField] private TextMeshProUGUI _playerNameText;
        [SerializeField] private TextMeshProUGUI _characterNameText;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _pingText;

        [Header("UI Visual References")]
        [SerializeField] private Image _characterIcon;
        [SerializeField] private GameObject _hostBadge;
        [SerializeField] private GameObject _readyBadge;

        private void Awake()
        {
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }
        }

        public void Bind(NetworkPlayerData data)
        {
            if (data == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (_playerNameText != null)
            {
                _playerNameText.text = data.DisplayName;
            }

            if (_characterNameText != null)
            {
                _characterNameText.text = $"<color=#CCCCCC>Tướng:</color> <color=#FFD700>{data.SelectedCharacterId}</color>";
            }

            if (_hostBadge != null)
            {
                _hostBadge.SetActive(data.IsHost);
            }

            if (_readyBadge != null)
            {
                _readyBadge.SetActive(data.IsReady && !data.IsHost);
            }

            if (_statusText != null)
            {
                if (data.IsHost)
                {
                    _statusText.text = "<color=#FFCC00>[Chủ Phòng]</color>";
                }
                else if (data.IsReady)
                {
                    _statusText.text = "<color=#00FF88>[Đã Sẵn Sàng]</color>";
                }
                else
                {
                    _statusText.text = "<color=#888888>[Chưa Sẵn Sàng]</color>";
                }
            }

            if (_pingText != null)
            {
                string pingColor = data.PingMs < 50 ? "#00FF88" : (data.PingMs < 100 ? "#FFFF00" : "#FF4444");
                _pingText.text = $"<color={pingColor}>{data.PingMs}ms</color>";
            }
        }
    }
}
