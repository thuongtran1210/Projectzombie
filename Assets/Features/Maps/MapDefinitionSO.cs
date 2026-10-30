using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectZombie.Features.Maps
{
    /// <summary>Scene identity shared by any number of stages. Contains no mutable run state.</summary>
    [CreateAssetMenu(menuName = "ProjectZombie/Maps/Map Definition")]
    public sealed class MapDefinitionSO : ScriptableObject
    {
        [SerializeField] private string _mapId;
        [SerializeField] private AssetReference _scene;
        public string MapId => _mapId;
        public AssetReference Scene => _scene;
        public bool IsValid => !string.IsNullOrWhiteSpace(_mapId) && _scene != null && _scene.RuntimeKeyIsValid();
    }
}
