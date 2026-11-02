using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>Reusable passive recruitment/command row. The presenter provides all formatted display data.</summary>
    public sealed class DefenseMinionSquadRowView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _count;
        [SerializeField] private Image _icon;
        [SerializeField] private bool _compactCommand;
        private string _id;
        public string Id => _id;
        public event Action<string> Requested;

        private void Awake() { _button.onClick.AddListener(Click); }
        private void Click() { if (!string.IsNullOrEmpty(_id)) Requested?.Invoke(_id); }
        public void Bind(string id) { _id = id; }
        public void Clear() { _id = null; Requested = null; }

        public void Render(string label, string name, string count, Sprite icon, bool interactable, Color color)
        {
            _button.interactable = interactable;
            if (_label != null) _label.SetText(_compactCommand ? CompactOrder(label) : label);
            if (_name != null) _name.SetText(name);
            if (_count != null) _count.SetText(count);
            if (_icon != null) { _icon.sprite = icon; _icon.enabled = icon != null; _icon.color = interactable ? Color.white : new Color(.65f, .65f, .65f, .85f); }
            if (_button.targetGraphic != null) _button.targetGraphic.color = color;
        }

        private static string CompactOrder(string label)
        {
            if (string.IsNullOrEmpty(label)) return string.Empty;
            string lower = label.ToLowerInvariant();
            if (lower.Contains("gom")) return "Gom";
            if (lower.Contains("thủ")) return "Thủ";
            if (lower.Contains("tấn") || lower.Contains("đánh")) return "Tiến";
            if (lower.Contains("y tế") || lower.Contains("trị") || lower.Contains("chữa")) return "Trị liệu";
            if (lower.Contains("theo")) return "Theo";
            return label;
        }

        private void OnDestroy() { if (_button != null) _button.onClick.RemoveListener(Click); }
    }
}
