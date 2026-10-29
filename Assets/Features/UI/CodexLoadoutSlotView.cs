using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectZombie.Features.UI
{
    public sealed class CodexLoadoutSlotView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _empty;
        private Action<CodexTabType, string> _onClick;
        private CodexTabType _tab;
        private string _id;
        private void Awake() => _button.onClick.AddListener(HandleClick);
        private void HandleClick() => _onClick?.Invoke(_tab, _id);
        public void Bind(CodexTabType tab, string id, string label, Sprite icon, Action<CodexTabType, string> onClick)
        {
            _tab = tab; _id = id; _onClick = onClick;
            _name.text = string.IsNullOrEmpty(id) ? "Thêm" : label;
            _icon.sprite = icon; _icon.enabled = icon != null;
            _empty.text = string.IsNullOrEmpty(id) ? "+" : label.Substring(0, 1);
            _empty.gameObject.SetActive(icon == null);
        }
    }
}
