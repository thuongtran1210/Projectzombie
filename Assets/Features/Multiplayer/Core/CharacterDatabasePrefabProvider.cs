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
        }

        public NetworkObject GetNetworkCharacterPrefab(string characterId)
        {
            if (_database == null || string.IsNullOrEmpty(characterId)) return null;

            var charData = _database.GetCharacterById(characterId);
            if (charData != null && charData.playerPrefab != null)
            {
                return charData.playerPrefab.GetComponent<NetworkObject>();
            }

            return null;
        }

        public NetworkObject GetDefaultNetworkCharacterPrefab()
        {
            if (_database != null && _database.Characters != null && _database.Characters.Count > 0)
            {
                var first = _database.Characters[0];
                if (first != null && first.playerPrefab != null)
                {
                    return first.playerPrefab.GetComponent<NetworkObject>();
                }
            }
            return null;
        }
    }
}
