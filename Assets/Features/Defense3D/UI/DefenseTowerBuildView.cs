using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>Passive map-owned build menu. Receives formatted strings, never reads gameplay models.</summary>
    public sealed class DefenseTowerBuildView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button[] _choices;
        [SerializeField] private TextMeshProUGUI[] _labels;
        [SerializeField] private TextMeshProUGUI _message;
        [SerializeField] private Button _close;
        public event Action<int> Selected;
        public event Action Closed;
        public bool IsOpen => _panel != null && _panel.activeSelf;
        private void Awake()
        {
            for (int i = 0; i < _choices.Length; i++)
            {
                int row = i;
                _choices[i].onClick.AddListener(() => Selected?.Invoke(row));
            }
            _close.onClick.AddListener(() => Closed?.Invoke());
            Hide();
        }
        public void Show(string message) { _panel.SetActive(true); _message.text = message; }
        public void Hide() { if (_panel != null) _panel.SetActive(false); }
        public void SetRow(int index, bool visible, string label, bool enabled)
        {
            if (index < 0 || index >= _choices.Length) return;
            _choices[index].gameObject.SetActive(visible);
            _labels[index].text = label;
            _choices[index].interactable = enabled;
        }
        public int Capacity => _choices.Length;
    }
}
