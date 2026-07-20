using UnityEngine;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// ScriptableObject dùng để lưu trữ thông tin nhân vật được chọn từ Menu Scene.
    /// Giúp truyền dữ liệu sang Gameplay Scene để Bootstrapper spawn.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterSelectionData", menuName = "ProjectZombie/Character Selection Data")]
    public class CharacterSelectionData : ScriptableObject
    {
        [Tooltip("Prefab của nhân vật đã chọn để spawn vào game")]
        [SerializeField] private GameObject selectedPlayerPrefab;

        public GameObject SelectedPlayerPrefab
        {
            get => selectedPlayerPrefab;
            set => selectedPlayerPrefab = value;
        }
    }
}
