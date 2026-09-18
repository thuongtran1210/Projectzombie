using UnityEngine;
using Fusion;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Multiplayer.Core
{
    /// <summary>
    /// Triển khai ICharacterPrefabProvider sử dụng CharacterDatabaseSO (DIP).
    /// Đơn trách nhiệm: Chỉ thực hiện tra cứu Prefab nhân vật mạng từ Database.
    /// </summary>
    public class CharacterDatabasePrefabProvider : ICharacterPrefabProvider
    {
        private readonly CharacterDatabaseSO _database;

        public CharacterDatabasePrefabProvider(CharacterDatabaseSO database = null)
        {
            _database = database != null ? database : Resources.Load<CharacterDatabaseSO>("CharacterDatabase");
#if UNITY_EDITOR
            if (_database == null)
            {
                _database = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterDatabaseSO>("Assets/_Data/CharacterDatabase.asset")
                            ?? UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterDatabaseSO>("Assets/Resources/CharacterDatabase.asset");
            }
#endif
        }

        public NetworkObject GetNetworkCharacterPrefab(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return GetDefaultNetworkCharacterPrefab();

            if (_database != null)
            {
                var charData = _database.GetCharacterById(characterId);
                if (charData != null && charData.playerPrefab != null)
                {
                    var netObj = charData.playerPrefab.GetComponent<NetworkObject>();
                    if (netObj != null) return netObj;
                }
            }

            return GetDefaultNetworkCharacterPrefab();
        }

        public NetworkObject GetDefaultNetworkCharacterPrefab()
        {
            if (_database != null && _database.Characters != null && _database.Characters.Count > 0)
            {
                var first = _database.Characters[0];
                if (first != null && first.playerPrefab != null)
                {
                    var netObj = first.playerPrefab.GetComponent<NetworkObject>();
                    if (netObj != null) return netObj;
                }
            }

            // Fallback trực tiếp từ thư mục Resources/Players/
            var fallbackPrefab = Resources.Load<GameObject>("Players/Thu Sinh");
            if (fallbackPrefab != null && fallbackPrefab.TryGetComponent<NetworkObject>(out var fallbackNetObj))
            {
                return fallbackNetObj;
            }

            return null;
        }
    }
}
