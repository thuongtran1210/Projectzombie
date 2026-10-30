using System.Collections.Generic;
using NUnit.Framework;
using ProjectZombie.Features.Defense3D;
using UnityEngine;

namespace ProjectZombie.Tests.Editor
{
    /// <summary>
    /// Bộ kiểm thử tự động thẩm định cân bằng gameplay & kinh tế phiên chơi 600s
    /// cho Map Sông Quê 3D (Stage01_Production).
    /// </summary>
    public sealed class DefenseSessionBalancingTests
    {
        private const string STAGE_PATH = "Assets/_Data/Levels/Rules/Stage01_Production.asset";
        private const string TOWER_NOTHAN_PATH = "Defense3D/Towers/SO_Tower_NoThan_Archer";
        private const string TOWER_BANGHA_PATH = "Defense3D/Towers/SO_Tower_BangHa_Ice";

        [Test]
        public void Stage01_Production_IsConfiguredWith34ChronologicalWaves()
        {
            var stage = UnityEditor.AssetDatabase.LoadAssetAtPath<StageSpawnConfig>(STAGE_PATH);
            Assert.That(stage, Is.Not.Null, "Không tìm thấy Stage01_Production.asset trong Resources/" + STAGE_PATH);
            Assert.That(stage.Events, Is.Not.Null);
            Assert.That(stage.Events.Count, Is.EqualTo(34), "Stage01_Production phải có đúng 34 waves chuẩn hóa.");

            float lastTrigger = -1f;
            for (int i = 0; i < stage.Events.Count; i++)
            {
                var wave = stage.Events[i];
                Assert.That(wave.TriggerTime, Is.GreaterThanOrEqualTo(lastTrigger),
                    $"Wave {wave.EventId} (index {i}) có thời gian trigger {wave.TriggerTime} không tuần tự sau {lastTrigger}.");
                Assert.That(wave.TriggerTime, Is.LessThanOrEqualTo(600f),
                    $"Wave {wave.EventId} có triggerTime vượt quá 600s trận đấu.");
                Assert.That(wave.Enemy, Is.Not.Null, $"Wave {wave.EventId} bị thiếu Enemy Archetype.");
                Assert.That(wave.Count, Is.GreaterThan(0), $"Wave {wave.EventId} có số lượng Count <= 0.");

                lastTrigger = wave.TriggerTime;
            }
        }

        [Test]
        public void Stage01_BossYaksha_IsConfiguredAtMinute830()
        {
            var stage = UnityEditor.AssetDatabase.LoadAssetAtPath<StageSpawnConfig>(STAGE_PATH);
            Assert.That(stage, Is.Not.Null);
            Assert.That(stage.BossSpawnTime, Is.EqualTo(510f), "Boss Dạ Xoa phải xuất hiện đúng mốc 510s (8 phút 30 giây).");
            Assert.That(stage.ArenaBoss, Is.Not.Null, "Boss Dạ Xoa (ArenaBoss) không được để trống.");
            Assert.That(stage.ArenaBoss.EnemyId, Is.EqualTo("yaksha_boss"));
            Assert.That(stage.ArenaBoss.MaxHP, Is.GreaterThanOrEqualTo(1000), "Máu Boss Dạ Xoa phải đạt tối thiểu 1000 HP.");
        }

        [Test]
        public void AllEnemyArchetypes_HaveValidCombatAndEconomyStats()
        {
            var stage = UnityEditor.AssetDatabase.LoadAssetAtPath<StageSpawnConfig>(STAGE_PATH);
            Assert.That(stage, Is.Not.Null);

            var archetypes = new HashSet<DefenseEnemyDataSO>();
            foreach (var wave in stage.Events)
            {
                if (wave.Enemy != null) archetypes.Add(wave.Enemy);
                if (wave.LeadEnemy != null) archetypes.Add(wave.LeadEnemy);
            }
            if (stage.ArenaBoss != null) archetypes.Add(stage.ArenaBoss);

            Assert.That(archetypes.Count, Is.GreaterThanOrEqualTo(5), "Phải có tối thiểu 5 loại kẻ địch trong stage.");

            foreach (var enemy in archetypes)
            {
                Assert.That(enemy.MaxHP, Is.GreaterThan(0), $"Quái {enemy.EnemyId} có MaxHP <= 0.");
                Assert.That(enemy.MoveSpeed, Is.GreaterThan(0.5f), $"Quái {enemy.EnemyId} có tốc độ MoveSpeed quá chậm (< 0.5).");
                Assert.That(enemy.AttackDamage, Is.GreaterThan(0), $"Quái {enemy.EnemyId} có AttackDamage <= 0.");
                Assert.That(enemy.WoodDrop, Is.GreaterThan(0), $"Quái {enemy.EnemyId} có WoodDrop <= 0.");
                Assert.That(enemy.ExpDrop, Is.GreaterThan(0), $"Quái {enemy.EnemyId} có ExpDrop <= 0.");
            }
        }

        [Test]
        public void StageWoodEconomy_ProvidesSufficientResourcesForDefense()
        {
            var stage = UnityEditor.AssetDatabase.LoadAssetAtPath<StageSpawnConfig>(STAGE_PATH);
            Assert.That(stage, Is.Not.Null);

            int totalWoodInMatch = 0;
            int woodInFirst2Minutes = 0; // 120s đầu tiên

            foreach (var wave in stage.Events)
            {
                int waveWood = 0;
                if (wave.Enemy != null)
                {
                    waveWood += wave.Enemy.WoodDrop * wave.Count;
                }
                if (wave.LeadEnemy != null)
                {
                    waveWood += wave.LeadEnemy.WoodDrop;
                }

                totalWoodInMatch += waveWood;
                if (wave.TriggerTime <= 120f)
                {
                    woodInFirst2Minutes += waveWood;
                }
            }

            if (stage.ArenaBoss != null)
            {
                totalWoodInMatch += stage.ArenaBoss.WoodDrop;
            }

            // Kiểm định kinh tế:
            // 1. Trong 2 phút đầu, tổng Gỗ quái rơi phải đủ để người chơi xây ít nhất 3-4 tháp mới (>= 150 Gỗ)
            Assert.That(woodInFirst2Minutes, Is.GreaterThanOrEqualTo(150),
                $"Lượng Gỗ kiếm được trong 2 phút đầu ({woodInFirst2Minutes}) quá ít, người chơi không kịp xây tháp phòng thủ.");

            // 2. Tổng lượng Gỗ toàn trận phải đủ để người chơi phủ kín 10 tháp (khoảng 300 Gỗ) và nâng cấp (tổng >= 1200 Gỗ)
            Assert.That(totalWoodInMatch, Is.GreaterThanOrEqualTo(1200),
                $"Tổng lượng Gỗ cả trận ({totalWoodInMatch}) không đủ để nâng cấp tháp chống lại Boss.");
        }

        [Test]
        public void TowerArchetypes_HaveValidProgressionTiers()
        {
#if UNITY_EDITOR
            var nothan = UnityEditor.AssetDatabase.LoadAssetAtPath<SO_TowerData>("Assets/_Data/Defense3D/Towers/SO_Tower_NoThan_Archer.asset");
            Assert.That(nothan, Is.Not.Null, "Không tìm thấy SO_Tower_NoThan_Archer.asset tại Assets/_Data/Defense3D/Towers/");
            ValidateTowerProgression(nothan);

            var bangha = UnityEditor.AssetDatabase.LoadAssetAtPath<SO_TowerData>("Assets/_Data/Defense3D/Towers/SO_Tower_BangHa_Ice.asset");
            Assert.That(bangha, Is.Not.Null, "Không tìm thấy SO_Tower_BangHa_Ice.asset tại Assets/_Data/Defense3D/Towers/");
            ValidateTowerProgression(bangha);
#endif
        }

        private static void ValidateTowerProgression(SO_TowerData tower)
        {
            Assert.That(tower.Levels, Is.Not.Null);
            Assert.That(tower.Levels.Length, Is.EqualTo(3), $"Tháp {tower.TowerName} phải có đủ 3 cấp độ (Level 1, 2, 3).");

            for (int i = 0; i < tower.Levels.Length - 1; i++)
            {
                var cur = tower.Levels[i];
                var next = tower.Levels[i + 1];

                Assert.That(cur.buildCost, Is.GreaterThan(0), $"Level {i + 1} của {tower.TowerName} có buildCost <= 0.");
                Assert.That(next.damage, Is.GreaterThan(cur.damage),
                    $"Damage của Level {i + 2} phải lớn hơn Level {i + 1} trên tháp {tower.TowerName}.");
                Assert.That(next.attackRange, Is.GreaterThanOrEqualTo(cur.attackRange),
                    $"Tầm bắn của Level {i + 2} không được thấp hơn Level {i + 1} trên tháp {tower.TowerName}.");
            }
        }
    }
}
