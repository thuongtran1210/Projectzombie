using System;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Pool;
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
        [Header("Independent squad command dock")]
        [SerializeField] private GameObject _commandDock;
        [SerializeField] private GameObject[] _commandGroups;
        [SerializeField] private Image[] _orderIcons;
        [SerializeField] private Image[] _recruitIcons;
        [SerializeField] private TMP_Text[] _orderCounts;
        [SerializeField] private Button _close;
        [Header("Roster-driven reusable rows")]
        [SerializeField] private DefenseMinionSquadRowView _recruitTemplate;
        [SerializeField] private DefenseMinionSquadRowView _commandTemplate;
        [SerializeField] private RectTransform _recruitContent;
        [SerializeField] private RectTransform _commandContent;
        private ObjectPool<DefenseMinionSquadRowView> _recruitPool, _commandPool;
        private readonly Dictionary<string, DefenseMinionSquadRowView> _recruitRows = new Dictionary<string, DefenseMinionSquadRowView>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DefenseMinionSquadRowView> _commandRows = new Dictionary<string, DefenseMinionSquadRowView>(StringComparer.OrdinalIgnoreCase);
        public int SquadRowCount => _recruitRows.Count;
        private Vector2Int _lastScreenSize;
        private Rect _lastSafeArea;
        private TMP_Text _contextLabel;
        public event Action ContextRequested;
        public event Action<string> RecruitRequested;
        public event Action<string> OrderRequested;
        public event Action BuildRequested;
        public event Action UpgradeRequested;

        private void Awake()
        {
            _panel.SetActive(false);
            SetCommandsVisible(false);
            if (_close != null) _close.onClick.AddListener(CloseRecruitment);
            _contextLabel = _toggle.GetComponentInChildren<TMP_Text>(true);
            _toggle.gameObject.SetActive(false);
            _build.gameObject.SetActive(false);
            _upgrade.gameObject.SetActive(false);
            _toggle.onClick.AddListener(() => ContextRequested?.Invoke());
            _build.onClick.AddListener(() => BuildRequested?.Invoke());
            _upgrade.onClick.AddListener(() => UpgradeRequested?.Invoke());
        }

        private void CloseRecruitment() => _panel.SetActive(false);
        private void OnDestroy()
        {
            if (_close != null) _close.onClick.RemoveListener(CloseRecruitment);
            _recruitPool?.Dispose(); _commandPool?.Dispose();
        }

        private static ObjectPool<DefenseMinionSquadRowView> Pool(DefenseMinionSquadRowView template, Transform parent)
        {
            if (template == null || parent == null) throw new InvalidOperationException("Run the minion roster migration to author row templates and content roots.");
            return new ObjectPool<DefenseMinionSquadRowView>(() => Instantiate(template, parent),
                row => row.gameObject.SetActive(true), row => { row.Clear(); row.gameObject.SetActive(false); },
                row => { if (row != null) Destroy(row.gameObject); });
        }

        /// <summary>Reuses views at roster bind/rebind; no row construction occurs in Update.</summary>
        public void BindSquads(IReadOnlyList<string> ids)
        {
            if (_recruitPool == null) _recruitPool = Pool(_recruitTemplate, _recruitContent);
            if (_commandPool == null) _commandPool = Pool(_commandTemplate, _commandContent);
            foreach (var row in _recruitRows.Values) _recruitPool.Release(row);
            foreach (var row in _commandRows.Values) _commandPool.Release(row);
            _recruitRows.Clear(); _commandRows.Clear();
            foreach (string id in ids)
            {
                var recruit = _recruitPool.Get(); recruit.Bind(id); recruit.Requested += ForwardRecruit;
                var command = _commandPool.Get(); command.Bind(id); command.Requested += ForwardOrder;
                recruit.transform.SetAsLastSibling(); command.transform.SetAsLastSibling();
                _recruitRows.Add(id, recruit); _commandRows.Add(id, command);
            }
        }
        private void ForwardRecruit(string id) => RecruitRequested?.Invoke(id);
        private void ForwardOrder(string id) => OrderRequested?.Invoke(id);

        public void RenderSquad(string id, string recruitLabel, string orderLabel, string countLabel, string name,
            Sprite icon, bool canRecruit, bool canOrder, int state)
        {
            if (!_recruitRows.TryGetValue(id, out var recruit) || !_commandRows.TryGetValue(id, out var command)) return;
            recruit.Render(recruitLabel, name, countLabel, icon, canRecruit, Color.white);
            Color tint = state == 0 ? new Color(1f, .85f, .55f) : state == 1 ? new Color(.60f, .92f, .91f) : new Color(1f, .66f, .55f);
            command.Render(orderLabel, name, countLabel, icon, canOrder, tint);
        }

        public void SetCommandsVisible(bool visible)
        {
            if (_commandDock != null && _commandDock.activeSelf != visible) _commandDock.SetActive(visible);
        }
        public void SetRecruitmentAccess(bool accessible) { if (!accessible && _panel.activeSelf) _panel.SetActive(false); }
        public void ToggleRecruitment() => _panel.SetActive(!_panel.activeSelf);
        public void SetContext(bool visible, string label, bool interactable)
        {
            visible &= !_panel.activeSelf;
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
            if (_safeArea == null || Screen.width <= 0 || Screen.height <= 0) return;
            var area = Screen.safeArea;
            var screen = new Vector2Int(Screen.width, Screen.height);
            if (area == _lastSafeArea && screen == _lastScreenSize) return;
            _lastSafeArea = area; _lastScreenSize = screen;
            if (area.width <= 0 || area.height <= 0) area = new Rect(0, 0, screen.x, screen.y);
            _safeArea.anchorMin = new Vector2(Mathf.Clamp01(area.xMin / screen.x), Mathf.Clamp01(area.yMin / screen.y));
            _safeArea.anchorMax = new Vector2(Mathf.Clamp01(area.xMax / screen.x), Mathf.Clamp01(area.yMax / screen.y));
            if (_safeArea.anchorMax.x <= _safeArea.anchorMin.x || _safeArea.anchorMax.y <= _safeArea.anchorMin.y)
            {
                _safeArea.anchorMin = Vector2.zero;
                _safeArea.anchorMax = Vector2.one;
            }
            _safeArea.offsetMin = _safeArea.offsetMax = Vector2.zero;
            LayoutRecruitment();
        }

        private void LayoutRecruitment()
        {
            if (_safeArea == null || _recruitContent == null) return;
            float dockHeight = _commandDock != null ? ((RectTransform)_commandDock.transform).rect.height : 0f;
            float height = Mathf.Clamp(_safeArea.rect.height - dockHeight - 52f, 240f, 440f);
            var panel = (RectTransform)_panel.transform;
            panel.sizeDelta = new Vector2(panel.sizeDelta.x, height);
            var scroll = (RectTransform)_recruitContent.parent.parent;
            scroll.sizeDelta = new Vector2(scroll.sizeDelta.x, Mathf.Max(96f, height - 144f));
        }

        public void SetHeader(string wood, string status) { _wood.text = wood; _status.text = status; }
        public void SetTowers(string build, string upgrade, bool canBuild, bool canUpgrade)
        {
            _buildLabel.text = build; _upgradeLabel.text = upgrade;
            _build.interactable = canBuild; _upgrade.interactable = canUpgrade;
        }
    }
}
