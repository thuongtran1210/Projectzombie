using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Defense3D.Loadout;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// ScriptableObject chứa danh mục Catalog toàn bộ Anh Hùng trong game (Read-Only Database).
    /// Hỗ trợ tra cứu qua DefenseLoadoutIdMapper để tự động tương thích dữ liệu local save cũ.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterDatabase", menuName = "ProjectZombie/Characters/Character Database")]
    public class CharacterDatabaseSO : ScriptableObject
    {
        [Header("Character Catalog")]
        [Tooltip("Danh sách toàn bộ Anh Hùng trong game")]
        [SerializeField] private List<CharacterDataSO> characters = new List<CharacterDataSO>();

        public IReadOnlyList<CharacterDataSO> Characters => characters;

        public CharacterDataSO GetCharacterById(string id)
        {
            if (string.IsNullOrEmpty(id) || characters == null) return null;

            string clean = id.Trim();
            for (int i = 0; i < characters.Count; i++)
            {
                var c = characters[i];
                if (c != null && (string.Equals(c.characterId, clean, StringComparison.OrdinalIgnoreCase) ||
                                  string.Equals(c.characterName, clean, StringComparison.OrdinalIgnoreCase)))
                {
                    return c;
                }
            }

            if (DefenseLoadoutIdMapper.TryMapHeroId(clean, out string canonicalHeroId))
            {
                string metaId = DefenseLoadoutIdMapper.GetMetaCharacterId(canonicalHeroId);
                for (int i = 0; i < characters.Count; i++)
                {
                    var c = characters[i];
                    if (c == null) continue;
                    if ((!string.IsNullOrEmpty(metaId) && string.Equals(c.characterId, metaId, StringComparison.OrdinalIgnoreCase)) ||
                        (DefenseLoadoutIdMapper.TryMapHeroId(c.characterId, out string entryCanonical) &&
                         string.Equals(entryCanonical, canonicalHeroId, StringComparison.OrdinalIgnoreCase)))
                    {
                        return c;
                    }
                }
            }

            return null;
        }

        public CharacterDataSO GetCharacterByIndex(int index)
        {
            if (characters != null && index >= 0 && index < characters.Count)
            {
                return characters[index];
            }
            return null;
        }

        public CharacterDataSO GetCharacterByPrefab(GameObject prefab)
        {
            if (prefab == null || characters == null) return null;
            return characters.Find(c => c != null && (c.playerPrefab == prefab || (c.playerPrefab != null && c.playerPrefab.name == prefab.name)));
        }

        public void SetCharacters(List<CharacterDataSO> list)
        {
            characters = list ?? new List<CharacterDataSO>();
        }
    }
}
