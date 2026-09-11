#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Maps;
using ProjectZombie.Features.Spawners;

namespace ProjectZombie.Editor.Maps
{
    /// <summary>
    /// Tool tự động tạo dữ liệu mẫu cho các Màn chơi (Stage 1, 2, 3) và Database World Stages.
    /// </summary>
    public static class StageDataGeneratorTool
    {
        private const string STAGES_DIR = "Assets/_Data/Levels/Stages";
        private const string DATABASE_PATH = "Assets/_Data/Levels/WorldStageDatabase.asset";

        [MenuItem("Tools/ProjectZombie/Maps/Generate Default Stages & Database", priority = 200)]
        public static void GenerateDefaultStages()
        {
            if (!Directory.Exists(STAGES_DIR))
            {
                Directory.CreateDirectory(STAGES_DIR);
                AssetDatabase.Refresh();
            }

            // Tìm timeline Level 1 có sẵn
            var level1Timeline = AssetDatabase.LoadAssetAtPath<LevelTimelineConfig>("Assets/_Data/Levels/Level1_Timeline.asset");

            var createdStages = new List<StageDefinitionSO>();

            // 1. Stage 1: Rừng Trúc Âm Ty (Local Core - Dùng Tilemap có sẵn trong Scene)
            var stage1 = CreateOrUpdateStage(
                "Stage_01_BambooForest",
                "STAGE_01",
                "Rừng Trúc Âm Ty",
                "Nơi âm khí tích tụ ngàn năm dưới những rặng tre ma, đàn thủy quái và cương thi bắt đầu trỗi dậy.",
                MapEnvironmentType.BambooForest,
                1,
                "", // Trống để dùng Tilemap có sẵn trong Scene
                "", // BGM mặc định
                0f, // 0 MB vì nằm sẵn trong Local Core
                level1Timeline,
                600f,
                500,
                250
            );
            createdStages.Add(stage1);

            // 2. Stage 2: Cổ Thành Đông Sơn (Remote DLC)
            var stage2 = CreateOrUpdateStage(
                "Stage_02_AncientCitadel",
                "STAGE_02",
                "Cổ Thành Đông Sơn",
                "Thành quách cổ xưa chìm trong khói lửa ma quái, nơi lũ yêu quái giáp sắt hoành hành.",
                MapEnvironmentType.AncientCitadel,
                5,
                "Map_AncientCitadel",
                "BGM_AncientCitadel",
                12.4f, // 12.4 MB DLC
                level1Timeline,
                720f,
                1200,
                600
            );
            createdStages.Add(stage2);

            // 3. Stage 3: Đầm Lầy Thần Sa (Remote DLC)
            var stage3 = CreateOrUpdateStage(
                "Stage_03_CinnabarSwamp",
                "STAGE_03",
                "Đầm Lầy Thần Sa",
                "Vùng đầm lầy độc địa chứa đầy độc khí và cổ trùng hung bạo, thử thách ý chí sinh tồn cực đại.",
                MapEnvironmentType.CinnabarSwamp,
                10,
                "Map_CinnabarSwamp",
                "BGM_CinnabarSwamp",
                16.8f, // 16.8 MB DLC
                level1Timeline,
                900f,
                2500,
                1500
            );
            createdStages.Add(stage3);

            // Tạo Database chung
            var db = AssetDatabase.LoadAssetAtPath<WorldStageDatabaseSO>(DATABASE_PATH);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<WorldStageDatabaseSO>();
                AssetDatabase.CreateAsset(db, DATABASE_PATH);
            }

            db.SetStages(createdStages);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = db;
            Debug.Log($"<color=#00FF88>[StageDataGeneratorTool]</color> Đã tạo thành công {createdStages.Count} Màn chơi và cập nhật WorldStageDatabase tại: {DATABASE_PATH}");
        }

        private static StageDefinitionSO CreateOrUpdateStage(
            string assetName,
            string stageId,
            string stageName,
            string description,
            MapEnvironmentType envType,
            int recLevel,
            string mapAddress,
            string bgmAddress,
            float estimatedMb,
            LevelTimelineConfig timeline,
            float duration,
            int coins,
            int exp)
        {
            string path = $"{STAGES_DIR}/{assetName}.asset";
            var stage = AssetDatabase.LoadAssetAtPath<StageDefinitionSO>(path);
            if (stage == null)
            {
                stage = ScriptableObject.CreateInstance<StageDefinitionSO>();
                AssetDatabase.CreateAsset(stage, path);
            }

            stage.stageId = stageId;
            stage.stageName = stageName;
            stage.description = description;
            stage.environmentType = envType;
            stage.recommendedLevel = recLevel;
            stage.mapPrefabAddress = mapAddress;
            stage.bgmAddress = bgmAddress;
            stage.estimatedDlcSizeMb = estimatedMb;
            stage.timelineConfig = timeline;
            stage.stageDurationSeconds = duration;
            stage.baseRewardCoins = coins;
            stage.baseRewardExp = exp;

            EditorUtility.SetDirty(stage);
            return stage;
        }
    }
}
#endif
