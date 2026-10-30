using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Socket build commands and proximity checks. Prefab inventory and attack behavior are separate.</summary>
    public sealed class DefenseTowerBuildPrototype3D : DefenseGameplayBehaviour
    {
        [SerializeField] private DefenseSceneInfo3D sceneInfo;
        [SerializeField] private DefenseWoodWallet3D wallet;
        [SerializeField, Min(.5f)] private float buildRadius = 1.7f;
        [SerializeField] private DefenseTowerCatalogSO _catalog;
        [SerializeField] private DefenseTowerSocket3D[] _sockets = Array.Empty<DefenseTowerSocket3D>();
        [SerializeField] private DefenseProjectilePool3D _projectiles;
        private DefenseTowerInventory3D _inventory;
        private DefenseHeroHealth3D _heroHealth;
        private IDefenseHeroCommands _commands;
        private bool _ready;
        private bool _building;
        private readonly List<string> _allowedTowerIds = new List<string>();
        public IReadOnlyList<string> AllowedTowerIds => _allowedTowerIds;
        public DefenseTowerCatalogSO Catalog => _catalog;
        public int SocketCount => _sockets.Length;
        public bool IsReady => _ready;
        public event Action InteractRequested;
        public event Action<int> BuildMenuRequested;
        public event Action Changed;
        public int NearestSocket => FindNearestSocket();
        public DefenseTowerPrototype3D NearestTower => GetTower(NearestSocket);
        public DefenseTowerPrototype3D GetTower(int index) => index >= 0 && index < _sockets.Length ? _sockets[index].Tower : null;
        public DefenseTowerSocket3D GetSocketState(int index) => index >= 0 && index < _sockets.Length ? _sockets[index] : null;
        public Transform GetSocket(int index) => GetSocketState(index)?.transform;
        public int BuildCost
        {
            get
            {
                int cheapest = int.MaxValue;
                if (_catalog != null) for (int i = 0; i < _catalog.Count; i++)
                {
                    var data = _catalog.Get(i).Data;
                    if (IsAllowed(data.Id)) cheapest = Mathf.Min(cheapest, data.GetLevelStats(1).buildCost);
                }
                return cheapest == int.MaxValue ? 0 : cheapest;
            }
        }
        public void BindCommands(IDefenseHeroCommands commands) => _commands = commands;
        public void SetAllowedTowers(IEnumerable<string> ids)
        {
            if (_ready && ids != null) foreach (string id in ids)
                if (!_inventory.Supports(id)) throw new InvalidOperationException("Changing to an unprepared tower requires a new match.");
            _allowedTowerIds.Clear();
            if (ids != null) foreach (string id in ids)
                if (!_allowedTowerIds.Contains(id)) _allowedTowerIds.Add(id);
            Changed?.Invoke();
        }
        public bool IsAllowed(string id)
        {
            foreach (string allowed in _allowedTowerIds)
                if (string.Equals(allowed, id, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
        public async Task PrepareAsync(DefenseFriendlyTargets3D friendlyTargets)
        {
            if (_ready) return;
            if (Gameplay == null || _catalog == null || _projectiles == null || sceneInfo == null || wallet == null)
                throw new InvalidOperationException("Assign tower catalog, projectile pool, scene info and wallet.");
            _heroHealth = sceneInfo.HeroSpawn.GetComponent<DefenseHeroHealth3D>();
            for (int i = 0; i < _sockets.Length; i++)
            {
                if (_sockets[i] == null || string.IsNullOrWhiteSpace(_sockets[i].SocketId)) throw new InvalidOperationException("Missing socket identity.");
                for (int j = 0; j < i; j++) if (_sockets[j].SocketId == _sockets[i].SocketId) throw new InvalidOperationException("Duplicate socket id.");
            }
            _inventory = new DefenseTowerInventory3D(_catalog);
            if (_allowedTowerIds.Count == 0) throw new InvalidOperationException("Select at least one tower before preparing the map.");
            foreach (string id in _allowedTowerIds) if (_catalog.Find(id) == null) throw new InvalidOperationException("Map has no definition for selected tower: " + id);
            await _inventory.PrepareAsync(transform, _sockets.Length, Gameplay, _projectiles, ReleaseTower, _allowedTowerIds);
            friendlyTargets.AppendRuntimeSupports(_inventory.SupportTargets);
            for (int i = 0; i < _catalog.Count; i++)
            {
                var data = _catalog.Get(i).Data;
                if (!IsAllowed(data.Id)) continue;
                if (data.Attack.Shots > _projectiles.CapacityPerType ||
                    (data.SpecialAttack != null && data.SpecialAttack.Shots > _projectiles.CapacityPerType))
                    throw new InvalidOperationException("Projectile budget is smaller than the configured burst: " + data.Id);
                var normal = data.Attack?.Projectile?.VisualPrefab;
                await _projectiles.PrewarmAsync(normal != null ? normal : data.ProjectilePrefab, _projectiles.CapacityPerType);
                var special = data.SpecialAttack?.Projectile?.VisualPrefab;
                if (special != null && special != normal) await _projectiles.PrewarmAsync(special, _projectiles.CapacityPerType);
            }
            _ready = true;
        }
        private void Update()
        {
            if (!CanBuildNow || _commands == null || !_commands.ConsumeBuild()) return;
            if (InteractRequested != null) InteractRequested.Invoke();
            else RequestBuildNearest();
        }
        private bool CanBuildNow => _ready && CanAdvanceGameplay && _heroHealth != null && !_heroHealth.IsDown;
        public bool CanInteract => CanBuildNow;
        public void RequestBuildNearest()
        {
            int index = NearestSocket;
            if (CanBuildNow && index >= 0 && !_sockets[index].IsOccupied) BuildMenuRequested?.Invoke(index);
        }
        public bool TryBuildNearest()
        {
            if (_allowedTowerIds.Count == 0) return false;
            return TryBuild(NearestSocket, _allowedTowerIds[0]);
        }
        public bool TryBuild(string socketId, string towerId)
        {
            for (int i = 0; i < _sockets.Length; i++) if (_sockets[i].SocketId == socketId) return TryBuild(i, towerId);
            return false;
        }
        public bool TryBuild(int index, string towerId)
        {
            if (_building || !CanBuildNow || index < 0 || index != NearestSocket || !IsAllowed(towerId)) return false;
            var socket = _sockets[index];
            var entry = _catalog.Find(towerId);
            if (socket.IsOccupied || !socket.Allows(towerId) || entry == null) return false;
            var tower = _inventory.FindIdle(towerId);
            int cost = entry.Data.GetLevelStats(1).buildCost;
            if (tower == null || cost < 0 || wallet.Wood < cost) return false;
            _building = true;
            try
            {
                // Prepare the inactive instance before spending; occupancy is reserved before wallet callbacks.
                tower.transform.SetPositionAndRotation(socket.transform.position, socket.transform.rotation);
                tower.PrepareForBuild();
                socket.SetTower(tower);
                if (!wallet.TrySpend(cost)) { socket.SetTower(null); return false; }
                tower.gameObject.SetActive(true);
                Changed?.Invoke();
                return true;
            }
            finally { _building = false; }
        }
        public bool TryUpgradeNearest()
        {
            var tower = NearestTower;
            if (_building || !CanBuildNow || tower == null) return false;
            _building = true;
            try
            {
                bool result = tower.TryUpgrade(wallet);
                if (result) Changed?.Invoke();
                return result;
            }
            finally { _building = false; }
        }
        private void ReleaseTower(DefenseTowerPrototype3D tower)
        {
            foreach (var socket in _sockets) if (socket != null && socket.Tower == tower) socket.SetTower(null);
            Changed?.Invoke();
        }
        public void ResetTowers()
        {
            _inventory?.Reset();
            foreach (var socket in _sockets) if (socket != null) socket.SetTower(null);
            Changed?.Invoke();
        }
        protected override void OnGameplayCleanup() => ResetTowers();
        protected override void OnGameplayReset() => ResetTowers();
        private int FindNearestSocket()
        {
            if (sceneInfo == null || sceneInfo.HeroSpawn == null) return -1;
            int bestIndex = -1;
            float best = buildRadius * buildRadius;
            for (int i = 0; i < _sockets.Length; i++)
            {
                if (_sockets[i] == null || !_sockets[i].isActiveAndEnabled) continue;
                Vector3 delta = _sockets[i].transform.position - sceneInfo.HeroSpawn.position;
                delta.y = 0f;
                if (delta.sqrMagnitude > best) continue;
                best = delta.sqrMagnitude;
                bestIndex = i;
            }
            return bestIndex;
        }
    }
}
