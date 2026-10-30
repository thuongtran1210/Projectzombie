using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>A location and occupancy, independent from any particular tower prefab.</summary>
    public sealed class DefenseTowerSocket3D : MonoBehaviour
    {
        [SerializeField] private string _socketId;
        [SerializeField] private string[] _allowedTowerIds = Array.Empty<string>();
        public string SocketId => _socketId;
        public DefenseTowerPrototype3D Tower { get; private set; }
        public bool IsOccupied => Tower != null && Tower.gameObject.activeSelf;
        public bool Allows(string id)
        {
            if (_allowedTowerIds.Length == 0) return true;
            foreach (string allowed in _allowedTowerIds)
                if (string.Equals(allowed, id, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
        internal void SetTower(DefenseTowerPrototype3D tower) => Tower = tower;
    }
}
