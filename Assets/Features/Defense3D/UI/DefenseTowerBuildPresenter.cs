using UnityEngine;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>Presents loadout/socket choices and revalidates every command against current proximity.</summary>
    public sealed class DefenseTowerBuildPresenter : DefenseGameplayBehaviour
    {
        [SerializeField] private DefenseTowerBuildPrototype3D _builder;
        [SerializeField] private DefenseWoodWallet3D _wallet;
        [SerializeField] private DefenseTowerBuildView _view;
        private int _socket = -1;
        protected override void OnGameplayBound()
        {
            if (_builder.Catalog.Count > _view.Capacity) throw new System.InvalidOperationException("Build menu has fewer rows than tower catalog entries.");
            _builder.BuildMenuRequested += Open;
            _builder.Changed += Refresh;
            _wallet.Changed += Refresh;
            _view.Selected += Build;
            _view.Closed += Close;
        }
        private void Open(int socket)
        {
            _socket = socket;
            _view.Show("CHỌN TRỤ PHÒNG THỦ");
            Refresh();
        }
        private void Build(int row)
        {
            if (row < 0 || row >= _builder.Catalog.Count) return;
            if (_builder.TryBuild(_socket, _builder.Catalog.Get(row).Data.Id)) Close();
            else Refresh();
        }
        private void Refresh()
        {
            if (!_view.IsOpen) return;
            var socket = _builder.GetSocketState(_socket);
            if (socket == null || socket.IsOccupied) { Close(); return; }
            for (int i = 0; i < _view.Capacity; i++)
            {
                if (i >= _builder.Catalog.Count) { _view.SetRow(i, false, "", false); continue; }
                var data = _builder.Catalog.Get(i).Data;
                var stats = data.GetLevelStats(1);
                bool allowed = _builder.IsAllowed(data.Id) && socket.Allows(data.Id);
                _view.SetRow(i, allowed, data.TowerName + " · " + stats.buildCost + " gỗ\n" + data.Description +
                    "\nSát thương " + stats.damage + " · Tầm " + stats.attackRange,
                    _builder.CanInteract && _wallet.Wood >= stats.buildCost);
            }
        }
        private void Close() { _socket = -1; _view.Hide(); }
        private void Update()
        {
            if (_view.IsOpen && (!_builder.CanInteract || _builder.NearestSocket != _socket)) Close();
        }
        protected override void OnGameplayGateChanged(bool active) { if (!active) Close(); }
        protected override void OnGameplayCleanup() => Close();
        protected override void OnDestroy()
        {
            if (_builder != null) { _builder.BuildMenuRequested -= Open; _builder.Changed -= Refresh; }
            if (_wallet != null) _wallet.Changed -= Refresh;
            if (_view != null) { _view.Selected -= Build; _view.Closed -= Close; }
            base.OnDestroy();
        }
    }
}
