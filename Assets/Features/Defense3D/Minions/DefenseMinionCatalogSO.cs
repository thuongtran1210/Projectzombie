using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Shared authored catalog. New types register data here without changing recruitment or UI code.</summary>
    [CreateAssetMenu(menuName = "Defense3D/Minion Catalog")]
    public sealed class DefenseMinionCatalogSO : ScriptableObject
    {
        [SerializeField] private DefenseMinionDefinitionSO[] _definitions;
        public IReadOnlyList<DefenseMinionDefinitionSO> Definitions => _definitions;
    }
}
