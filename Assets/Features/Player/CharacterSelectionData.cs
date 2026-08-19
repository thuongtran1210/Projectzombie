using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Player
{
    [Serializable]
    public class CharacterEntry
    {
        public string characterId;
        public string characterName;
        public ElementType element;
        public string elementHexColor = "#FFD700";
        [TextArea(2, 4)] public string description;
        public string signatureSkillName;
        [TextArea(2, 4)] public string signatureSkillDesc;
        public Sprite avatar;
        public GameObject playerPrefab;
        public bool isUnlocked = true;
    }

    /// <summary>
    /// ScriptableObject lưu trữ danh sách toàn bộ nhân vật có thể chơi trong game (Database Nhân Vật)
    /// và ghi nhớ nhân vật hiện đang được người chơi lựa chọn.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterSelectionData", menuName = "ProjectZombie/Character Selection Data")]
    public class CharacterSelectionData : ScriptableObject
    {
        [Header("Character Database")]
        [Tooltip("Danh sách toàn bộ nhân vật có thể chọn trong game")]
        [SerializeField] private List<CharacterEntry> characters = new List<CharacterEntry>();

        [Header("Runtime Selection")]
        [Tooltip("Prefab của nhân vật đã chọn để spawn vào game")]
        [SerializeField] private GameObject selectedPlayerPrefab;

        [Tooltip("Index nhân vật được chọn gần nhất")]
        [SerializeField] private int selectedCharacterIndex = 0;

        public IReadOnlyList<CharacterEntry> Characters => characters;

        public GameObject SelectedPlayerPrefab
        {
            get
            {
                if (selectedPlayerPrefab != null) return selectedPlayerPrefab;
                if (characters != null && characters.Count > 0 && selectedCharacterIndex >= 0 && selectedCharacterIndex < characters.Count)
                {
                    return characters[selectedCharacterIndex].playerPrefab;
                }
                return null;
            }
            set => selectedPlayerPrefab = value;
        }

        public int SelectedCharacterIndex
        {
            get => selectedCharacterIndex;
            set
            {
                selectedCharacterIndex = value;
                if (characters != null && selectedCharacterIndex >= 0 && selectedCharacterIndex < characters.Count)
                {
                    selectedPlayerPrefab = characters[selectedCharacterIndex].playerPrefab;
                }
            }
        }

        public void SelectCharacter(int index)
        {
            SelectedCharacterIndex = index;
        }

        public void SetCharacters(List<CharacterEntry> list)
        {
            characters = list;
        }
    }
}
