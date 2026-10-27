using System.IO;
using ProjectZombie.Features.Defense3D;
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.EditorTools.Defense3D
{
    public static class DefenseStageSpawnConfigInstaller
    {
        [MenuItem("TREBRAVE/Defense 3D/Create Missing Stage Spawn Configs")]
        public static void CreateMissingConfigs()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play Mode first.");
            DefenseEnemyPrefabInstaller.CreateMissingPrefabs();
            Directory.CreateDirectory("Assets/Resources/Defense3D/Stages");
            AssetDatabase.Refresh();
            Create("Stage01_Production", "stage01-production-v1", 1f, 510f, 45f);
            Create("Stage01_FastSpawn", "stage01-fast-spawn-v1", 0.05f, 10f, 5f);
            AssetDatabase.SaveAssets();
        }

        private static DefenseEnemyDataSO Enemy(string id) =>
            AssetDatabase.LoadAssetAtPath<DefenseEnemyDataSO>("Assets/Resources/" + DefenseEnemyFactory3D.DefinitionsFolder + id + ".asset");

        private static void Create(string name, string version, float timeScale, float bossTime, float arenaDuration)
        {
            string path = "Assets/Resources/Defense3D/Stages/" + name + ".asset";
            if (File.Exists(path)) return; // Never overwrite authored profiles.
            var config = ScriptableObject.CreateInstance<StageSpawnConfig>();
            try
            {
                var serialized = new SerializedObject(config);
                serialized.FindProperty("profileVersion").stringValue = version;
                serialized.FindProperty("bossSpawnTime").floatValue = bossTime;
                serialized.FindProperty("arenaDuration").floatValue = arenaDuration;
                serialized.FindProperty("arenaBoss").objectReferenceValue = Enemy(DefenseEnemyRegistry.ID_YAKSHA_BOSS);
                var events = serialized.FindProperty("events");
                var baseline = DefenseStage01Schedule.GenerateEvents();
                events.arraySize = baseline.Count;
                for (int i = 0; i < baseline.Count; i++)
                {
                    var source = baseline[i];
                    var entry = events.GetArrayElementAtIndex(i);
                    entry.FindPropertyRelative("EventId").stringValue = source.EventId;
                    entry.FindPropertyRelative("TriggerTime").floatValue = source.TriggerTime * timeScale;
                    entry.FindPropertyRelative("Enemy").objectReferenceValue = Enemy(source.DeclaredType);
                    entry.FindPropertyRelative("ZoneId").stringValue = source.Zone;
                    entry.FindPropertyRelative("Count").intValue = source.JobCount;
                    entry.FindPropertyRelative("LeadEnemy").objectReferenceValue = source.DeclaredType == DefenseEnemyRegistry.ID_MAGIAP
                        ? Enemy(DefenseEnemyRegistry.ID_PHICUONG) : null;
                }
                var zones = serialized.FindProperty("zones");
                zones.arraySize = 5;
                SetZone(zones.GetArrayElementAtIndex(0), "eastBank", true, new Vector3(0f, 1f, 0f),
                    new[] { new Vector3(0f, 0f, -10.5f), new Vector3(0f, 0f, -0.5f), new Vector3(0f, 0f, 10.5f) }, new[] { 0, 1, 2 });
                SetZone(zones.GetArrayElementAtIndex(1), "northCanopy", true, new Vector3(0f, 1f, 14f), new[] { Vector3.zero }, new[] { 2 });
                SetZone(zones.GetArrayElementAtIndex(2), "southBank", true, new Vector3(0f, 1f, -14f), new[] { Vector3.zero }, new[] { 0 });
                SetZone(zones.GetArrayElementAtIndex(3), "canalWater", false, new Vector3(6.25f, 1.1f, 0f),
                    new[] { new Vector3(0f, 0f, -10.5f), new Vector3(0f, 0f, -0.5f), new Vector3(0f, 0f, 10.5f) }, new[] { 0, 1, 2 });
                SetZone(zones.GetArrayElementAtIndex(4), "arenaEast", true, new Vector3(0f, 1f, 0f), new[] { Vector3.zero }, new[] { 1 });
                serialized.ApplyModifiedPropertiesWithoutUndo();
                config.Compile();
                AssetDatabase.CreateAsset(config, path);
            }
            catch
            {
                Object.DestroyImmediate(config);
                throw;
            }
        }

        private static void SetZone(SerializedProperty zone, string id, bool relative, Vector3 origin, Vector3[] offsets, int[] lanes)
        {
            zone.FindPropertyRelative("ZoneId").stringValue = id;
            zone.FindPropertyRelative("RelativeToEastMarker").boolValue = relative;
            zone.FindPropertyRelative("Origin").vector3Value = origin;
            var points = zone.FindPropertyRelative("Points");
            points.arraySize = offsets.Length;
            for (int i = 0; i < offsets.Length; i++)
            {
                var point = points.GetArrayElementAtIndex(i);
                point.FindPropertyRelative("Offset").vector3Value = offsets[i];
                point.FindPropertyRelative("Lane").intValue = lanes[i];
            }
        }
    }
}
