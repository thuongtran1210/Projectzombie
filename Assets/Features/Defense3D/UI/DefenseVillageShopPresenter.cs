using UnityEngine;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>Source proximity interaction: hut recruitment takes priority over the nearest tower socket.</summary>
    public sealed class DefenseVillageShopPresenter : DefenseGameplayBehaviour
    {
        [SerializeField] private DefenseVillageShop3D _shop;
        [SerializeField] private DefenseVillageShopView _view;
        [SerializeField] private Camera _camera;
        private static readonly string[,] ORDER_LABELS =
        {
            { "Thủ nhà", "Hộ vệ", "Tiến công" },
            { "Gom gần nhà", "Theo nhặt gỗ", "Gom toàn map" },
            { "Trạm y tế", "Hộ vệ chủ tướng", "Tiếp tế tiền tuyến" }
        };
        private bool _dirty = true, _lastCanTrade, _lastNearHut;
        private int _lastSocket = -2, _lastLevel = -1;
        private DefenseVillageAction _lastAction;
        private string _message = "Quán Trà · Chọn lính để chiêu mộ.";

        protected override void OnGameplayBound()
        {
            Unsubscribe();
            if (_camera == null) _camera = Camera.main;
            _shop.Changed += MarkDirty;
            _view.RecruitRequested += Recruit;
            _view.OrderRequested += Order;
            _view.ContextRequested += ContextInteract;
            _shop.TowerBuilder.InteractRequested += KeyboardInteract;
            _shop.TowerBuilder.Changed += MarkDirty;
            _dirty = true;
        }
        private void MarkDirty() => _dirty = true;
        private void Recruit(int role) { _message = _shop.TryRecruit(role) ? "Đã chiêu mộ." : "Cần ở trong vùng Quán Trà, đủ gỗ và còn chỗ."; MarkDirty(); }
        private void Order(int role) { _shop.CycleOrder(role); MarkDirty(); }
        private void ContextInteract() => Interact(true);
        private void KeyboardInteract() => Interact(false);

        private DefenseVillageInteraction CurrentAction(out int socket, out int level)
        {
            var builder = _shop.TowerBuilder;
            socket = builder.NearestSocket;
            var tower = builder.NearestTower;
            level = tower != null && tower.gameObject.activeSelf ? tower.Level : 0;
            return DefenseVillageInteractionRules.Resolve(_shop.CanTrade, _shop.IsNearRecruitment, socket >= 0,
                level, _shop.Wallet.Wood, 0, level > 0 ? tower.UpgradeCost : -1, tower != null ? tower.MaxLevel : 3);
        }

        private void Interact(bool fromButton)
        {
            var action = CurrentAction(out int socket, out _);
            // Reject a stale pointer click after leaving a zone or after building changed the action.
            if (fromButton && (action.Action != _lastAction ||
                (action.Action != DefenseVillageAction.Recruit && socket != _lastSocket))) { MarkDirty(); return; }
            if (!action.Enabled && action.Action != DefenseVillageAction.Build) return;
            switch (action.Action)
            {
                case DefenseVillageAction.Recruit: _view.ToggleRecruitment(); break;
                case DefenseVillageAction.Build: _shop.TowerBuilder.RequestBuildNearest(); break;
                case DefenseVillageAction.Upgrade: _shop.TowerBuilder.TryUpgradeNearest(); break;
            }
            MarkDirty();
        }

        private void Update()
        {
            if (_shop == null || _view == null) return;
            var action = CurrentAction(out int socket, out int level);
            bool nearHut = _shop.IsNearRecruitment, available = _shop.CanTrade;
            _view.SetRecruitmentAccess(available && nearHut);
            _view.SetCommandsVisible(available);
            bool visible = action.Action != DefenseVillageAction.None && _camera != null;
            if (visible)
            {
                Vector3 anchor = action.Action == DefenseVillageAction.Recruit
                    ? _shop.RecruitmentPosition + Vector3.up * 1.8f
                    : _shop.TowerBuilder.GetSocket(socket).position + Vector3.up * (level > 0 ? 2.7f : 1.6f);
                Vector3 screen = _camera.WorldToScreenPoint(anchor);
                visible = screen.z > 0f && screen.x >= 0f && screen.x <= Screen.width && screen.y >= 0f && screen.y <= Screen.height;
                if (visible) _view.PositionContext(screen);
            }
            _view.SetContext(visible, null, action.Enabled || action.Action == DefenseVillageAction.Build);
            if (!_dirty && socket == _lastSocket && level == _lastLevel && available == _lastCanTrade && nearHut == _lastNearHut) return;
            _dirty = false; _lastSocket = socket; _lastLevel = level;
            _lastCanTrade = available; _lastNearHut = nearHut; _lastAction = action.Action;
            int wood = _shop.Wallet.Wood;
            _view.SetHeader("Gỗ: " + wood, _message);
            for (int role = 0; role < 3; role++)
            {
                var definition = _shop.Definition(role);
                bool allowed = definition != null && _shop.IsRoleAllowed(role);
                _view.SetRoleVisible(role, allowed);
                if (!allowed) continue;
                int count = _shop.Count(role), capacity = _shop.Capacity(role);
                _view.SetRow(role, definition.DisplayName + "\n<color=#EACB83>" + definition.WoodCost + " gỗ</color>  (" + count + "/" + capacity + ")",
                    ORDER_LABELS[role, (int)_shop.Order(role)], available && nearHut && wood >= definition.WoodCost && count < capacity, available && count > 0, count, (int)_shop.Order(role));
            }
            string label;
            switch (action.Action)
            {
                case DefenseVillageAction.Recruit: label = "Chiêu mộ\nQuán Trà [B]"; break;
                case DefenseVillageAction.Build: label = "Chọn trụ [B]"; break;
                case DefenseVillageAction.Upgrade: label = "Nâng cấp " + (level + 1) + "\n" + (action.Shortage > 0 ? "Thiếu " + action.Shortage : action.Cost.ToString()) + " gỗ [B]"; break;
                case DefenseVillageAction.Maximum: label = "Trụ đã đạt\ncấp tối đa"; break;
                default: label = string.Empty; break;
            }
            _view.SetContext(visible, label, action.Enabled || action.Action == DefenseVillageAction.Build);
        }
        protected override void OnGameplayGateChanged(bool active)
        {
            MarkDirty();
            if (!active) { _view.SetContext(false, null, false); _view.SetRecruitmentAccess(false); _view.SetCommandsVisible(false); }
        }
        protected override void OnGameplayCleanup() { _view.SetContext(false, null, false); _view.SetRecruitmentAccess(false); _view.SetCommandsVisible(false); }
        private void Unsubscribe()
        {
            if (_shop != null) { _shop.Changed -= MarkDirty; _shop.TowerBuilder.InteractRequested -= KeyboardInteract; _shop.TowerBuilder.Changed -= MarkDirty; }
            if (_view == null) return;
            _view.RecruitRequested -= Recruit; _view.OrderRequested -= Order; _view.ContextRequested -= ContextInteract;
        }
        protected override void OnDestroy() { Unsubscribe(); base.OnDestroy(); }
    }
}
