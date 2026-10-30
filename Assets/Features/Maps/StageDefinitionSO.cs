using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Maps
{
    /// <summary>
    /// Định nghĩa môi trường khí hậu / hiệu ứng sàn đấu của Màn chơi.
    /// </summary>
    public enum MapEnvironmentType
    {
        BambooForest,   // Rừng Trúc Âm Ty (Cản trở di chuyển, sương mù trúc ma)
        AncientCitadel, // Cổ Thành Đông Sơn (Thành quách, hào lũy phòng thủ)
        CinnabarSwamp,  // Đầm Lầy Thần Sa (Vũng bùn độc làm chậm)
        UnderworldGate  // Cửa Địa Phủ Hoàng Tuyền (Dung nham, quỷ hỏa bùng nổ)
    }

    /// <summary>
    /// Dữ liệu cấu hình một Ải / Màn chơi trong World Map (ScriptableObject).
    /// Tích hợp Addressables Remote Key cho Background Tilemap, BGM và Timeline Quái vật.
    /// </summary>
    [CreateAssetMenu(fileName = "Stage_New", menuName = "ProjectZombie/Maps/Stage Definition")]
    public class StageDefinitionSO : ScriptableObject
    {
        [Header("Defense Scene & Rules")]
        [SerializeField] private MapDefinitionSO _map;
        [SerializeField] private Defense3D.DefenseStageBlueprintSO _defenseBlueprint;
        [SerializeField] private UnityEngine.AddressableAssets.AssetReferenceT<Defense3D.StageSpawnConfig> _spawnRules;
        [SerializeField] private string[] _requiredCompletedStages = System.Array.Empty<string>();
        public MapDefinitionSO Map => _map;
        public Defense3D.DefenseStageBlueprintSO DefenseBlueprint => _defenseBlueprint;
        public UnityEngine.AddressableAssets.AssetReferenceT<Defense3D.StageSpawnConfig> SpawnRules => _spawnRules;
        public float DefenseDuration => _defenseBlueprint != null ? _defenseBlueprint.stageDurationSeconds : stageDurationSeconds;
        public bool CanLaunchDefense => !string.IsNullOrWhiteSpace(stageId) && _map != null && _map.IsValid
            && (_defenseBlueprint != null || (_spawnRules != null && _spawnRules.RuntimeKeyIsValid())) && DefenseDuration > 0f
            && !float.IsNaN(DefenseDuration) && !float.IsInfinity(DefenseDuration);

        public bool IsUnlocked(ProjectZombie.Features.MetaProgression.MetaProgressionSaveData save)
        {
            if (_requiredCompletedStages == null) return true;
            foreach (var required in _requiredCompletedStages)
                if (!string.IsNullOrWhiteSpace(required) && (save == null || !save.IsStageCompleted(required))) return false;
            return true;
        }

        [Header("Thông Tin Ải (Stage Profile)")]
        public string stageId = "STAGE_01";
        public string stageName = "Rừng Trúc Âm Ty";
        [TextArea(2, 4)]
        public string description = "Nơi âm khí tích tụ ngàn năm dưới những rặng tre ma, đàn thủy quái và cương thi bắt đầu trỗi dậy.";
        public Sprite stageThumbnail;
        public MapEnvironmentType environmentType = MapEnvironmentType.BambooForest;
        public int recommendedLevel = 1;

        [Header("Addressables Remote Keys (CDN / DLC)")]
        [Tooltip("Địa chỉ Addressable nạp Tilemap Map Prefab")]
        public string mapPrefabAddress = "Map_BambooForest";
        [Tooltip("Địa chỉ Addressable nạp Nhạc nền BGM")]
        public string bgmAddress = "BGM_BambooForest";
        [Tooltip("Dung lượng xấp xỉ của màn chơi khi tải DLC (MB)")]
        public float estimatedDlcSizeMb = 8.5f;

        [Header("Timeline Sóng Quái & Cấu Hình Gameplay")]
        public ScriptableObject timelineConfig;
        public float stageDurationSeconds = 600f; // 10 phút / màn

        [Header("Phần Thưởng Vượt Ải (Clear Rewards)")]
        public int baseRewardCoins = 500;
        public int baseRewardExp = 250;
        public List<string> dropUnlockWeaponIds = new List<string>();
    }
}
