using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>Passive shop controls; only the presenter decides prices, availability and commands.</summary>
    public sealed class DefenseVillageShopView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private RectTransform _safeArea;
        [SerializeField] private Button _toggle;
        [SerializeField] private Button[] _recruit;
        [SerializeField] private Button[] _orders;
        [SerializeField] private TMP_Text[] _recruitLabels;
        [SerializeField] private TMP_Text[] _orderLabels;
        [SerializeField] private TMP_Text _wood;
        [SerializeField] private TMP_Text _status;
        [SerializeField] private Button _build;
        [SerializeField] private Button _upgrade;
        [SerializeField] private TMP_Text _buildLabel;
        [SerializeField] private TMP_Text _upgradeLabel;
        private Rect _lastSafeArea;
        private TMP_Text _contextLabel;
        public event Action ContextRequested;
        public event Action<int> RecruitRequested;
        public event Action<int> OrderRequested;
        public event Action BuildRequested;
        public event Action UpgradeRequested;

        private void Awake()
        {
            _panel.SetActive(false);
            _contextLabel = _toggle.GetComponentInChildren<TMP_Text>(true);
            _toggle.gameObject.SetActive(false);
            _build.gameObject.SetActive(false);
            _upgrade.gameObject.SetActive(false);
            _toggle.onClick.AddListener(() => ContextRequested?.Invoke());
            for (int i = 0; i < _recruit.Length; i++)
            {
                int role = i;
                _recruit[i].onClick.AddListener(() => RecruitRequested?.Invoke(role));
                _orders[i].onClick.AddListener(() => OrderRequested?.Invoke(role));
            }
            _build.onClick.AddListener(() => BuildRequested?.Invoke());
            _upgrade.onClick.AddListener(() => UpgradeRequested?.Invoke());
        }

        public void SetRecruitmentAccess(bool accessible) { if (!accessible && _panel.activeSelf) _panel.SetActive(false); }
        public void ToggleRecruitment() => _panel.SetActive(!_panel.activeSelf);
        public void SetContext(bool visible, string label, bool interactable)
        {
            if (_toggle.gameObject.activeSelf != visible) _toggle.gameObject.SetActive(visible);
            _toggle.interactable = interactable;
            if (_contextLabel != null && label != null) _contextLabel.text = label;
        }
        public void PositionContext(Vector2 screenPoint)
        {
            var rect = (RectTransform)_toggle.transform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)rect.parent, screenPoint, null, out var local))
            {
                var area = ((RectTransform)rect.parent).rect;
                local.x = Mathf.Clamp(local.x, area.xMin + rect.rect.width * .5f, area.xMax - rect.rect.width * .5f);
                local.y = Mathf.Clamp(local.y, area.yMin + rect.rect.height * .5f, area.yMax - rect.rect.height * .5f);
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
                rect.localPosition = new Vector3(local.x, local.y, 0f);
            }
        }

        private void LateUpdate()
        {
            var area = Screen.safeArea;
            if (area == _lastSafeArea || Screen.width <= 0 || Screen.height <= 0) return;
            _lastSafeArea = area;
            _safeArea.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            _safeArea.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            _safeArea.offsetMin = _safeArea.offsetMax = Vector2.zero;
        }

        public void SetHeader(string wood, string status) { _wood.text = wood; _status.text = status; }
        public void SetRow(int role, string recruit, string order, bool canRecruit, bool canOrder)
        {
            _recruitLabels[role].text = recruit; _orderLabels[role].text = order;
            _recruit[role].interactable = canRecruit; _orders[role].interactable = canOrder;
        }
        public void SetTowers(string build, string upgrade, bool canBuild, bool canUpgrade)
        {
            _buildLabel.text = build; _upgradeLabel.text = upgrade;
            _build.interactable = canBuild; _upgrade.interactable = canUpgrade;
        }
    }
}
