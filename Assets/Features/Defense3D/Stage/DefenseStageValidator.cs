using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Báo cáo kết quả kiểm định tính toàn vẹn và mức độ cân bằng của Ải chiến (Stage Validation Report).
    /// </summary>
    public sealed class DefenseStageValidationReport
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();

        public int TotalWaves { get; set; }
        public int TotalEnemies { get; set; }
        public float Duration { get; set; }

        public string ToFormattedSummary()
        {
            if (IsValid)
            {
                string warnText = Warnings.Count > 0 
                    ? $"\n⚠️ Cảnh báo ({Warnings.Count}):\n• " + string.Join("\n• ", Warnings) 
                    : "\n✨ Không có cảnh báo nào!";
                return $"✅ CẤU HÌNH HỢP LỆ HOÀN TOÀN!\n• Thời lượng: {Duration:F0}s | Số đợt: {TotalWaves} | Tổng quái: {TotalEnemies}{warnText}";
            }

            return $"❌ PHÁT HIỆN {Errors.Count} LỖI CẤU HÌNH:\n• " + string.Join("\n• ", Errors);
        }
    }

    /// <summary>
    /// Bộ tiền kiểm định hợp lệ (Pre-flight Level Validator) dành cho Game Designer.
    /// Giúp phát hiện sớm các lỗi cấu hình Prefab, phân bổ Vùng, thiếu sót chỉ số quái vật và cảnh báo hiệu năng trên di động.
    /// </summary>
    public static class DefenseStageValidator
    {
        private const float MIN_REASONABLE_STAGE_DURATION = 60f;
        private const float MAX_REASONABLE_STAGE_DURATION = 1200f; // 20 phút
        private const float MIN_WAVE_INTERVAL_WARNING = 1.0f; // Quá gần nhau có thể gây giật frame

        /// <summary>
        /// Thực hiện kiểm tra toàn diện một bản thiết kế ải (DefenseStageBlueprintSO).
        /// </summary>
        public static DefenseStageValidationReport Validate(DefenseStageBlueprintSO blueprint)
        {
            var report = new DefenseStageValidationReport();

            if (blueprint == null)
            {
                report.Errors.Add("Bản thiết kế ải (Blueprint) bị null.");
                return report;
            }

            // ── 1. KIỂM TRA THÔNG TIN CHUNG ──
            if (string.IsNullOrWhiteSpace(blueprint.stageId))
                report.Errors.Add("Mã định danh Ải (StageId) không được để trống.");

            if (string.IsNullOrWhiteSpace(blueprint.stageName))
                report.Warnings.Add("Tên hiển thị của Ải (StageName) đang để trống.");

            if (!IsFinite(blueprint.stageDurationSeconds) || blueprint.stageDurationSeconds < MIN_REASONABLE_STAGE_DURATION)
                report.Errors.Add($"Thời lượng ải ({blueprint.stageDurationSeconds}s) không hợp lệ (tối thiểu {MIN_REASONABLE_STAGE_DURATION}s).");
            else if (blueprint.stageDurationSeconds > MAX_REASONABLE_STAGE_DURATION)
                report.Warnings.Add($"Thời lượng ải khá dài ({blueprint.stageDurationSeconds / 60f:F1} phút), cần lưu ý pin và trải nghiệm trên di động.");

            report.Duration = blueprint.stageDurationSeconds;

            // ── 2. KIỂM TRA CẤU HÌNH VÙNG XUẤT HIỆN (SPAWN ZONES) ──
            var definedZones = new HashSet<DefenseSpawnZoneType>();
            if (blueprint.spawnZones == null || blueprint.spawnZones.Length == 0)
            {
                report.Errors.Add("Chưa cấu hình bất kỳ Vùng Xuất Hiện nào (SpawnZones rỗng).");
            }
            else
            {
                for (int i = 0; i < blueprint.spawnZones.Length; i++)
                {
                    var zone = blueprint.spawnZones[i];
                    if (zone == null)
                    {
                        report.Errors.Add($"Vùng xuất hiện tại vị trí [{i}] bị null.");
                        continue;
                    }

                    if (!definedZones.Add(zone.zoneType))
                    {
                        report.Errors.Add($"Trùng lặp cấu hình Vùng xuất hiện: {zone.zoneType}. Mỗi loại vùng chỉ được khai báo 1 lần.");
                    }

                    if (zone.points == null || zone.points.Length == 0)
                    {
                        report.Errors.Add($"Vùng '{zone.zoneType}' chưa có điểm xuất hiện chi tiết nào (Points rỗng).");
                    }
                    else
                    {
                        for (int p = 0; p < zone.points.Length; p++)
                        {
                            var pt = zone.points[p];
                            if (pt == null)
                            {
                                report.Errors.Add($"Điểm xuất hiện [{p}] tại vùng '{zone.zoneType}' bị null.");
                            }
                            else if (pt.Lane < 0 || pt.Lane > 2)
                            {
                                report.Errors.Add($"Điểm xuất hiện [{p}] tại vùng '{zone.zoneType}' có làn ({pt.Lane}) không hợp lệ (chỉ chấp nhận làn 0, 1 hoặc 2).");
                            }
                        }
                    }
                }
            }

            // ── 3. KIỂM TRA ĐẠI BOSS ĐẤU TRƯỜNG (ARENA BOSS) ──
            if (blueprint.arenaBoss == null)
            {
                report.Errors.Add("Chưa chỉ định Đại Boss cho giai đoạn quyết chiến Đấu Trường (ArenaBoss is null).");
            }
            else
            {
                ValidateEnemyDefinition(blueprint.arenaBoss, "Đại Boss (" + blueprint.arenaBoss.name + ")", report);

                if (!IsFinite(blueprint.bossHealthOverride) || blueprint.bossHealthOverride <= 0f)
                    report.Errors.Add($"Lượng máu chỉ định cho Boss ({blueprint.bossHealthOverride} HP) phải là số dương hợp lệ.");

                if (!IsFinite(blueprint.bossSpawnTime) || blueprint.bossSpawnTime < 0f || blueprint.bossSpawnTime >= blueprint.stageDurationSeconds)
                    report.Errors.Add($"Thời điểm Boss xuất hiện ({blueprint.bossSpawnTime}s) phải nằm trong khoảng [0, {blueprint.stageDurationSeconds}s).");

                if (!IsFinite(blueprint.arenaDuration) || blueprint.arenaDuration < 5f)
                    report.Errors.Add($"Thời gian tối đa quyết chiến Đấu Trường ({blueprint.arenaDuration}s) quá ngắn (tối thiểu 5s).");
            }

            // ── 4. KIỂM TRA TIMELINE CÁC ĐỢT SÓNG QUÁI ──
            if (blueprint.timelineWaves == null || blueprint.timelineWaves.Count == 0)
            {
                report.Warnings.Add("Ải chiến hiện tại chưa có bất kỳ đợt quái thường nào.");
            }
            else
            {
                report.TotalWaves = blueprint.timelineWaves.Count;
                var eventIds = new HashSet<string>(StringComparer.Ordinal);
                float previousTriggerTime = -1f;

                for (int i = 0; i < blueprint.timelineWaves.Count; i++)
                {
                    var wave = blueprint.timelineWaves[i];
                    if (wave == null)
                    {
                        report.Errors.Add($"Đợt sóng tại vị trí [{i}] bị null.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(wave.eventId))
                    {
                        report.Errors.Add($"Đợt sóng [{i}] bị thiếu Mã Định Danh (EventId rỗng).");
                    }
                    else if (!eventIds.Add(wave.eventId))
                    {
                        report.Errors.Add($"Trùng lặp Mã Định Danh Đợt Sóng '{wave.eventId}'. Mỗi đợt phải có EventId duy nhất.");
                    }

                    if (wave.enemy == null)
                    {
                        report.Errors.Add($"Đợt sóng '{wave.eventId}' chưa được gán loại Quái Vật chính.");
                    }
                    else
                    {
                        ValidateEnemyDefinition(wave.enemy, $"Quái vật đợt '{wave.eventId}'", report);
                    }

                    if (wave.leadEnemy != null)
                    {
                        ValidateEnemyDefinition(wave.leadEnemy, $"Thủ lĩnh đợt '{wave.eventId}'", report);
                    }

                    if (wave.count < 1)
                    {
                        report.Errors.Add($"Đợt sóng '{wave.eventId}' có số lượng quái nhỏ hơn 1 ({wave.count}).");
                    }
                    else
                    {
                        report.TotalEnemies += wave.count;
                        if (wave.count > blueprint.maxActiveEnemies)
                        {
                            report.Warnings.Add($"Đợt sóng '{wave.eventId}' sinh {wave.count} quái cùng lúc, vượt giới hạn an toàn {blueprint.maxActiveEnemies} con trên sân.");
                        }
                    }

                    if (!definedZones.Contains(wave.zone))
                    {
                        report.Errors.Add($"Đợt sóng '{wave.eventId}' chỉ định vùng '{wave.zone}' nhưng vùng này chưa được định nghĩa trong SpawnZones.");
                    }

                    if (!IsFinite(wave.triggerTime) || wave.triggerTime < 0f || wave.triggerTime >= blueprint.stageDurationSeconds)
                    {
                        report.Errors.Add($"Thời điểm kích hoạt của đợt '{wave.eventId}' ({wave.triggerTime}s) nằm ngoài phạm vi ải [0, {blueprint.stageDurationSeconds}s).");
                    }
                    else
                    {
                        if (previousTriggerTime >= 0f && (wave.triggerTime - previousTriggerTime) < MIN_WAVE_INTERVAL_WARNING && (wave.triggerTime - previousTriggerTime) >= 0f)
                        {
                            report.Warnings.Add($"Đợt '{wave.eventId}' xuất hiện quá sát đợt trước ({wave.triggerTime - previousTriggerTime:F1}s), có thể gây nghẽn hàng đợi quái.");
                        }
                        previousTriggerTime = wave.triggerTime;
                    }
                }
            }

            // ── 5. KIỂM TRA GIỚI HẠN HIỆU NĂNG ──
            if (blueprint.maxActiveEnemies < 1 || blueprint.maxActiveEnemies > 100)
                report.Warnings.Add($"Giới hạn số quái tối đa ({blueprint.maxActiveEnemies}) nên nằm trong khoảng khuyến nghị [16 - 64] để đạt 60 FPS mượt mà.");

            if (blueprint.spawnBudgetPerFrame < 1 || blueprint.spawnBudgetPerFrame > 10)
                report.Warnings.Add($"Ngân sách spawn mỗi frame ({blueprint.spawnBudgetPerFrame}) nên nằm trong khoảng [2 - 6] để tránh drop frame.");

            return report;
        }

        private static void ValidateEnemyDefinition(DefenseEnemyDataSO enemy, string contextLabel, DefenseStageValidationReport report)
        {
            if (enemy == null) return;

            if (string.IsNullOrWhiteSpace(enemy.EnemyId))
                report.Errors.Add($"{contextLabel}: Mã định danh EnemyId rỗng.");

            if (enemy.Prefab == null)
            {
                report.Errors.Add($"{contextLabel}: Chưa gán Prefab 3D.");
            }
            else
            {
                if (enemy.Prefab.GetComponent<DefenseEnemyBridgeWalker3D>() == null)
                {
                    report.Errors.Add($"{contextLabel}: Prefab '{enemy.Prefab.name}' thiếu component cốt lõi 'DefenseEnemyBridgeWalker3D'.");
                }
            }

            if (!IsFinite(enemy.MaxHP) || enemy.MaxHP <= 0f)
                report.Errors.Add($"{contextLabel}: Lượng máu MaxHP ({enemy.MaxHP}) không hợp lệ.");

            if (!IsFinite(enemy.MoveSpeed) || enemy.MoveSpeed <= 0f)
                report.Errors.Add($"{contextLabel}: Tốc độ di chuyển MoveSpeed ({enemy.MoveSpeed}) không hợp lệ.");

            if (!IsFinite(enemy.AttackDamage) || enemy.AttackDamage < 0f)
                report.Errors.Add($"{contextLabel}: Sát thương AttackDamage ({enemy.AttackDamage}) không hợp lệ.");

            if (!IsFinite(enemy.AttackInterval) || enemy.AttackInterval <= 0f)
                report.Errors.Add($"{contextLabel}: Tốc độ đánh AttackInterval ({enemy.AttackInterval}) không hợp lệ.");

            if (!IsFinite(enemy.AttackRange) || enemy.AttackRange <= 0f)
                report.Errors.Add($"{contextLabel}: Tầm đánh AttackRange ({enemy.AttackRange}) không hợp lệ.");
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
