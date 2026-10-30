using TMPro;
using UnityEngine;

namespace ProjectZombie.Core.Architecture
{
    /// <summary>Application-owned loading cover; survives destruction of the outgoing scene.</summary>
    public sealed class AppLoadingView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private TextMeshProUGUI _status;
        [SerializeField] private UnityEngine.UI.Button _dismiss;
        private void Awake()
        {
            if (_dismiss != null) _dismiss.onClick.AddListener(Hide);
        }
        private void OnDestroy()
        {
            if (_dismiss != null) _dismiss.onClick.RemoveListener(Hide);
        }
        public void Show(string message)
        {
            if (_dismiss != null) _dismiss.gameObject.SetActive(false);
            _group.alpha = 1f;
            _group.blocksRaycasts = true;
            _group.interactable = true;
            if (_status != null) _status.text = message;
        }

        public void ShowError(string message)
        {
            Show(message);
            if (_dismiss != null) _dismiss.gameObject.SetActive(true);
        }

        public void Hide()
        {
            _group.alpha = 0f;
            _group.blocksRaycasts = false;
            _group.interactable = false;
        }
    }
}
