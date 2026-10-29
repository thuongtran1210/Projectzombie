using System.IO;
using System.Collections.Generic;
using ProjectZombie.Features.Defense3D;
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.EditorTools.Defense3D
{
    /// <summary>
    /// Công cụ Editor hỗ trợ Game Designer: Tự động khởi tạo Master Blueprint chuẩn cho Stage 01 (Sông Quê Âm Ty)
    /// Trích xuất toàn bộ dữ liệu 34 đợt quái từ code C# cũ sang Asset trực quan 100% Data-Driven.
    /// </summary>
    public static class DefenseStageBlueprintInstaller
    {
        private const string BLUEPRINT_PATH = "Assets/Resources/Defense3D/Stages/Stage01_SongQue_Blueprint.asset";

        [MenuItem("ProjectZombie/3. 🗺️ Stage & Level/Dựng Stage01 Blueprint (Data-Driven Master)", priority = 20)]
        public static void CreateOrUpdateStage01Blueprint()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Cảnh báo", "Vui lòng thoát chế độ Play Mode trước khi tạo Blueprint.", "Đóng");
                return;
            }

            Directory.CreateDirectory("Assets/Resources/Defense3D/Stages");

            var blueprint = AssetDatabase.LoadAssetAtPath<DefenseStageBlueprintSO>(BLUEPRINT_PATH);
            bool isNew = blueprint == null;

            if (isNew)
            {
                blueprint = ScriptableObject.CreateInstance<DefenseStageBlueprintSO>();
            }

            // 1. Thông tin Ải chiến
            blueprint.stageId = "STAGE_01";
            blueprint.stageName = "Sông Quê Âm Ty";
            blueprint.description = "Bờ sông hoàng hôn phủ sương trắng, ma giáp và thủy quái trỗi dậy từ lòng kênh. Anh hùng trấn thủ cây cầu sinh tử bảo vệ làng quê.";
            blueprint.stageDurationSeconds = 600f;
            blueprint.mapAddress = "MapSongQue3D_Prototype";
            blueprint.bgmAddress = "BGM_SongQue";
            blueprint.baseRewardCoins = 500;
            blueprint.baseRewardExp = 250;

            // 2. Cấu hình 5 Vùng xuất hiện chuẩn hóa qua Enum
            blueprint.spawnZones = new[]
            {
                new DefenseSpawnZoneBlueprint
                {
                    zoneType = DefenseSpawnZoneType.EastBank,
                    relativeToEastMarker = true,
                    origin = new Vector3(0f, 1f, 0f),
                    points = new[]
                    {
                        new DefenseSpawnPointConfig { Offset = new Vector3(0f, 0f, -10.5f), Lane = 0 },
                        new DefenseSpawnPointConfig { Offset = new Vector3(0f, 0f, -0.5f), Lane = 1 },
                        new DefenseSpawnPointConfig { Offset = new Vector3(0f, 0f, 10.5f), Lane = 2 }
                    }
                },
                new DefenseSpawnZoneBlueprint
                {
                    zoneType = DefenseSpawnZoneType.NorthCanopy,
                    relativeToEastMarker = true,
                    origin = new Vector3(0f, 1f, 14f),
                    points = new[]
                    {
                        new DefenseSpawnPointConfig { Offset = Vector3.zero, Lane = 2 }
                    }
                },
                new DefenseSpawnZoneBlueprint
                {
                    zoneType = DefenseSpawnZoneType.SouthBank,
                    relativeToEastMarker = true,
                    origin = new Vector3(0f, 1f, -14f),
                    points = new[]
                    {
                        new DefenseSpawnPointConfig { Offset = Vector3.zero, Lane = 0 }
                    }
                },
                new DefenseSpawnZoneBlueprint
                {
                    zoneType = DefenseSpawnZoneType.CanalWater,
                    relativeToEastMarker = false,
                    origin = new Vector3(6.25f, 1.1f, 0f),
                    points = new[]
                    {
                        new DefenseSpawnPointConfig { Offset = new Vector3(0f, 0f, -10.5f), Lane = 0 },
                        new DefenseSpawnPointConfig { Offset = new Vector3(0f, 0f, -0.5f), Lane = 1 },
                        new DefenseSpawnPointConfig { Offset = new Vector3(0f, 0f, 10.5f), Lane = 2 }
                    }
                },
                new DefenseSpawnZoneBlueprint
                {
                    zoneType = DefenseSpawnZoneType.ArenaEast,
                    relativeToEastMarker = true,
                    origin = new Vector3(0f, 1f, 0f),
                    points = new[]
                    {
                        new DefenseSpawnPointConfig { Offset = Vector3.zero, Lane = 1 }
                    }
                }
            };

            // 3. Trích xuất 34 đợt quái từ Schedule cũ sang danh sách TimelineWaves
            var baselineEvents = DefenseStage01Schedule.GenerateEvents();
            blueprint.timelineWaves = new List<DefenseStageWaveEntry>(baselineEvents.Count);

            var enemyCache = new Dictionary<string, DefenseEnemyDataSO>();
            DefenseEnemyDataSO GetEnemy(string id)
            {
                if (enemyCache.TryGetValue(id, out var data)) return data;
                var loaded = AssetDatabase.LoadAssetAtPath<DefenseEnemyDataSO>(
                    "Assets/Resources/" + DefenseEnemyFactory3D.DefinitionsFolder + id + ".asset");
                enemyCache[id] = loaded;
                return loaded;
            }

            for (int i = 0; i < baselineEvents.Count; i++)
            {
                var src = baselineEvents[i];
                var waveEntry = new DefenseStageWaveEntry
                {
                    eventId = src.EventId,
                    triggerTime = src.TriggerTime,
                    enemy = GetEnemy(src.DeclaredType),
                    zone = DefenseSpawnZoneExtensions.FromZoneId(src.Zone),
                    count = src.JobCount,
                    leadEnemy = (src.DeclaredType == DefenseEnemyRegistry.ID_MAGIAP)
                        ? GetEnemy(DefenseEnemyRegistry.ID_PHICUONG)
                        : null
                };

                blueprint.timelineWaves.Add(waveEntry);
            }

            // 4. Cấu hình Đại Boss Đấu Trường
            blueprint.arenaBoss = GetEnemy(DefenseEnemyRegistry.ID_YAKSHA_BOSS);
            blueprint.bossZone = DefenseSpawnZoneType.ArenaEast;
            blueprint.bossSpawnTime = 510f;
            blueprint.arenaDuration = 45f;
            blueprint.bossHealthOverride = 1200f;

            // 5. Giới hạn hiệu năng
            blueprint.maxActiveEnemies = 32;
            blueprint.maxPendingSpawns = 40;
            blueprint.spawnBudgetPerFrame = 4;
            blueprint.jobLifetime = 25f;
            blueprint.safeHeroDistance = 3.1f;

            // Kiểm tra tính hợp lệ trước khi lưu
            blueprint.Compile();

            if (isNew)
            {
                AssetDatabase.CreateAsset(blueprint, BLUEPRINT_PATH);
            }
            else
            {
                EditorUtility.SetDirty(blueprint);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=#00FF88>[DefenseStageBlueprintInstaller]</color> Đã tạo thành công Master Blueprint tại: {BLUEPRINT_PATH}");
            Selection.activeObject = blueprint;
        }
    }
}
