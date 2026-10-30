using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D.Loadout
{
    /// <summary>Generated metadata projection of TowerCatalog; contains no tower/projectile prefab dependencies.</summary>
    [CreateAssetMenu(menuName = "ProjectZombie/Defense3D/Tower Loadout Metadata")]
    public sealed class DefenseTowerLoadoutCatalogSO : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string Id;
            public string Name;
        }
        [SerializeField] private Entry[] _entries = Array.Empty<Entry>();
        public int Count => _entries.Length;
        public Entry Get(int index) => _entries[index];
    }
}
