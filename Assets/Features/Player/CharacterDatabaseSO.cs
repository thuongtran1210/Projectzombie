using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// ScriptableObject chứa danh mục Catalog toàn bộ Anh Hùng trong game (Read-Only Database).
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
            return characters.Find(c => c != null && (c.characterId == id || c.characterName == id));
        }

        public CharacterDataSO GetCharacterByIndex(int index)
        {
            if (characters != null && index >= 0 && index < characters.Count)
            {
                return characters[index];
            }
            return null;
        }

        public void SetCharacters(List<CharacterDataSO> list)
        {
            characters = list ?? new List<CharacterDataSO>();
        }
    }
}
