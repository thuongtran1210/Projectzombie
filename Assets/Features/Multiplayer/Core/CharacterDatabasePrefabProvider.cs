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
            if (database != null)
            {
                _database = database;
            }
            else
            {
                try
                {
                    var handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<CharacterDatabaseSO>("CharacterDatabase");
                    handle.WaitForCompletion();
                    if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                    {
                        _database = handle.Result;
                    }
                }
                catch { }

                if (_database == null) _database = Resources.Load<CharacterDatabaseSO>("CharacterDatabase");
#if UNITY_EDITOR
                if (_database == null)
                {
                    _database = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterDatabaseSO>("Assets/_Data/CharacterDatabase.asset");
                }
#endif
            }
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

            return null;
        }
    }
}
