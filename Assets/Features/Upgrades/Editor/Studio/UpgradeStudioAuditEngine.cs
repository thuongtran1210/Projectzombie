#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades.Editor.Studio
{
    public enum AuditCategory
    {
        All = 0,
        Identification = 1,     // ID rỗng / Trùng lặp
        IconMissing = 2,        // Thiếu Icon Sprite
        SpawnWeight = 3,        // Trọng số <= 0
        EvolutionLink = 4,      // Liên kết vũ khí / Tiến hóa hỏng
        TraitArchetype = 5,     // Thần Binh Thuật chưa gắn Lõi
        MechanicReadiness = 6   // Cơ chế chưa hoạt động / Thiếu Prefab / Rỗng Modifier
    }

    public struct AuditIssue
    {
        public enum IssueSeverity { Error, Warning, Info }
        public IssueSeverity Severity;
        public AuditCategory Category;
        public UpgradeData TargetAsset;
        public string Message;
    }

    /// <summary>
    /// Engine kiểm tra tính toàn vẹn (Health Audit) và đồng bộ hóa 1-Click cho toàn bộ Database Thẻ Nâng Cấp.
    /// </summary>
    public static class UpgradeStudioAuditEngine
    {
        private const string SOURCE_DIR = "Assets/_Data/Upgrades";
        private const string RESOURCES_DIR = "Assets/Resources/Upgrades";

        private static bool IsStatModifierEmpty(PlayerStatModifier mod)
        {
            return Mathf.Approximately(mod.maxHealthBonus, 0f) &&
                   Mathf.Approximately(mod.moveSpeedBonus, 0f) &&
                   Mathf.Approximately(mod.critChanceBonus, 0f) &&
                   Mathf.Approximately(mod.baseDamageBonus, 0f) &&
                   Mathf.Approximately(mod.pickupRangeBonus, 0f) &&
                   Mathf.Approximately(mod.expMultiplierBonus, 0f) &&
                   Mathf.Approximately(mod.attackSpeedBonus, 0f) &&
                   Mathf.Approximately(mod.dashCooldownReduction, 0f) &&
                   Mathf.Approximately(mod.dashSpeedBonus, 0f) &&
                   Mathf.Approximately(mod.areaScaleBonus, 0f) &&
                   Mathf.Approximately(mod.fireDamageBonus, 0f);
        }

        private static bool IsWeaponModifierEmpty(WeaponStatModifier mod)
        {
            return Mathf.Approximately(mod.damageBonus, 0f) &&
                   Mathf.Approximately(mod.attackSpeedBonus, 0f) &&
                   mod.projectileCountBonus == 0 &&
                   mod.pierceBonus == 0 &&
                   Mathf.Approximately(mod.scaleBonus, 0f) &&
                   Mathf.Approximately(mod.critChanceBonus, 0f) &&
                   Mathf.Approximately(mod.critDamageBonus, 0f) &&
                   Mathf.Approximately(mod.projectileSpeedBonus, 0f);
        }

        /// <summary>
        /// Quét toàn bộ danh sách thẻ để phát hiện các lỗi cấu hình tiềm ẩn.
        /// </summary>
        public static List<AuditIssue> RunAudit(IReadOnlyList<UpgradeData> allUpgrades)
        {
            var issues = new List<AuditIssue>();
            if (allUpgrades == null || allUpgrades.Count == 0) return issues;

            var idMap = new Dictionary<string, UpgradeData>(System.StringComparer.OrdinalIgnoreCase);

            foreach (var u in allUpgrades)
            {
                if (u == null) continue;

                // 1. Kiểm tra ID rỗng hoặc trùng lặp
                if (string.IsNullOrEmpty(u.id))
                {
                    issues.Add(new AuditIssue
                    {
                        Severity = AuditIssue.IssueSeverity.Error,
                        Category = AuditCategory.Identification,
                        TargetAsset = u,
                        Message = $"Thẻ '{u.upgradeName}' chưa có ID định danh (ID rỗng)."
                    });
                }
                else
                {
                    if (idMap.TryGetValue(u.id, out var existing))
                    {
                        issues.Add(new AuditIssue
                        {
                            Severity = AuditIssue.IssueSeverity.Error,
                            Category = AuditCategory.Identification,
                            TargetAsset = u,
                            Message = $"Trùng lặp ID '{u.id}' giữa thẻ '{u.upgradeName}' và '{existing.upgradeName}'."
                        });
                    }
                    else
                    {
                        idMap[u.id] = u;
                    }
                }

                // 2. Kiểm tra Icon Sprite
                if (u.icon == null)
                {
                    issues.Add(new AuditIssue
                    {
                        Severity = AuditIssue.IssueSeverity.Warning,
                        Category = AuditCategory.IconMissing,
                        TargetAsset = u,
                        Message = $"Thẻ '{u.upgradeName}' (ID: {u.id}) chưa được gán Icon Sprite."
                    });
                }

                // 3. Kiểm tra Trọng số xuất hiện (Spawn Weight)
                if (u.spawnWeight <= 0f)
                {
                    issues.Add(new AuditIssue
                    {
                        Severity = AuditIssue.IssueSeverity.Warning,
                        Category = AuditCategory.SpawnWeight,
                        TargetAsset = u,
                        Message = $"Thẻ '{u.upgradeName}' (ID: {u.id}) có trọng số xuất hiện <= 0 (Sẽ không xuất hiện trong pool)."
                    });
                }

                // 4. Kiểm tra chuyên biệt theo từng phân loại
                if (u is MythicCoreUpgradeData core)
                {
                    if (core.runtimePrefab == null)
                    {
                        issues.Add(new AuditIssue
                        {
                            Severity = AuditIssue.IssueSeverity.Error,
                            Category = AuditCategory.MechanicReadiness,
                            TargetAsset = u,
                            Message = $"Đại Lõi '{core.upgradeName}' (ID: {core.id}) chưa được gán Runtime Prefab (chưa thể hoạt động trong gameplay)."
                        });
                    }
                }
                else if (u is MutationAugmentUpgradeData mut)
                {
                    bool statEmpty = IsStatModifierEmpty(mut.statModifier);
                    bool hasPrefab = mut.mechanicRuntimePrefab != null;

                    if (statEmpty && !hasPrefab)
                    {
                        issues.Add(new AuditIssue
                        {
                            Severity = AuditIssue.IssueSeverity.Error,
                            Category = AuditCategory.MechanicReadiness,
                            TargetAsset = u,
                            Message = $"Lõi Đột Biến '{mut.upgradeName}' (ID: {mut.id}) HOÀN TOÀN CHƯA HOẠT ĐỘNG (Chỉ số rỗng và chưa có mechanicRuntimePrefab)."
                        });
                    }
                    else if (!hasPrefab && mut.tier != AugmentTier.Silver)
                    {
                        issues.Add(new AuditIssue
                        {
                            Severity = AuditIssue.IssueSeverity.Warning,
                            Category = AuditCategory.MechanicReadiness,
                            TargetAsset = u,
                            Message = $"Lõi Đột Biến '{mut.upgradeName}' [{mut.tier}] (ID: {mut.id}) chưa gắn mechanicRuntimePrefab (đang chạy Fallback chỉ cộng chỉ số, chưa có cơ chế thực thể/VFX riêng)."
                        });
                    }
                }
                else if (u is StatMicroUpgradeData micro)
                {
                    if (string.IsNullOrEmpty(micro.oneLineSummary))
                    {
                        issues.Add(new AuditIssue
                        {
                            Severity = AuditIssue.IssueSeverity.Warning,
                            Category = AuditCategory.MechanicReadiness,
                            TargetAsset = u,
                            Message = $"Thẻ Micro-Card '{micro.upgradeName}' (ID: {micro.id}) chưa có tóm tắt 1 dòng (oneLineSummary)."
                        });
                    }

                    if (IsStatModifierEmpty(micro.statModifier))
                    {
                        issues.Add(new AuditIssue
                        {
                            Severity = AuditIssue.IssueSeverity.Error,
                            Category = AuditCategory.MechanicReadiness,
                            TargetAsset = u,
                            Message = $"Thẻ Micro-Card '{micro.upgradeName}' (ID: {micro.id}) không có bất kỳ chỉ số nào trong statModifier (Chưa hoạt động)."
                        });
                    }
                }
                else if (u is SynergyTraitUpgradeData trait)
                {
                    if (trait.requiredArchetype == MythicArchetype.None)
                    {
                        issues.Add(new AuditIssue
                        {
                            Severity = AuditIssue.IssueSeverity.Warning,
                            Category = AuditCategory.TraitArchetype,
                            TargetAsset = u,
                            Message = $"Thần Binh Thuật '{trait.upgradeName}' (ID: {trait.id}) chưa thiết lập Required Archetype (None)."
                        });
                    }

                    if (IsStatModifierEmpty(trait.playerStatModifier))
                    {
                        issues.Add(new AuditIssue
                        {
                            Severity = AuditIssue.IssueSeverity.Warning,
                            Category = AuditCategory.MechanicReadiness,
                            TargetAsset = u,
                            Message = $"Thần Binh Thuật '{trait.upgradeName}' (ID: {trait.id}) không có bất kỳ chỉ số nào trong playerStatModifier."
                        });
                    }
                }
                else if (u is EvolutionUpgradeData evo)
                {
                    if (string.IsNullOrEmpty(evo.weaponId))
                    {
                        issues.Add(new AuditIssue
                        {
                            Severity = AuditIssue.IssueSeverity.Error,
                            Category = AuditCategory.EvolutionLink,
                            TargetAsset = u,
                            Message = $"Thẻ Tiến Hóa '{evo.upgradeName}' (ID: {evo.id}) chưa cấu hình Weapon ID yêu cầu."
                        });
                    }
                }
                else if (u is WeaponUpgradeData weaponUp)
                {
                    if (string.IsNullOrEmpty(weaponUp.weaponId))
                    {
                        issues.Add(new AuditIssue
                        {
                            Severity = AuditIssue.IssueSeverity.Error,
                            Category = AuditCategory.EvolutionLink,
                            TargetAsset = u,
                            Message = $"Thẻ Cường Hóa Pháp Bảo '{weaponUp.upgradeName}' (ID: {weaponUp.id}) chưa cấu hình Weapon ID."
                        });
                    }

                    if (IsWeaponModifierEmpty(weaponUp.statModifier))
                    {
                        issues.Add(new AuditIssue
                        {
                            Severity = AuditIssue.IssueSeverity.Warning,
                            Category = AuditCategory.MechanicReadiness,
                            TargetAsset = u,
                            Message = $"Thẻ Cường Hóa Pháp Bảo '{weaponUp.upgradeName}' (ID: {weaponUp.id}) không tăng bất kỳ chỉ số vũ khí nào."
                        });
                    }
                }
                else if (u is CommonUpgradeData common)
                {
                    if (IsStatModifierEmpty(common.playerStatModifier))
                    {
                        issues.Add(new AuditIssue
                        {
                            Severity = AuditIssue.IssueSeverity.Warning,
                            Category = AuditCategory.MechanicReadiness,
                            TargetAsset = u,
                            Message = $"Thẻ Bị Động '{common.upgradeName}' (ID: {common.id}) không có bất kỳ chỉ số nào trong playerStatModifier."
                        });
                    }
                }
            }

            return issues;
        }

        /// <summary>
        /// Tạo chuỗi báo cáo lỗi hoàn chỉnh dạng Markdown để chép vào Clipboard.
        /// </summary>
        public static string GenerateAuditReport(List<AuditIssue> issues, string filterInfo = "Tất cả")
        {
            if (issues == null || issues.Count == 0)
            {
                return "# Báo Cáo Thẩm Định Upgrades Database\n\n✅ Không tìm thấy lỗi hoặc cảnh báo nào. Toàn bộ cơ sở dữ liệu Upgrades hoạt động hoàn hảo!";
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("# Báo Cáo Thẩm Định Upgrades Database (Projectzombie)");
            sb.AppendLine($"- **Thời gian quét:** {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"- **Bộ lọc:** {filterInfo}");
            sb.AppendLine($"- **Tổng số vấn đề:** {issues.Count} (🔴 Errors: {issues.FindAll(i => i.Severity == AuditIssue.IssueSeverity.Error).Count}, 🟡 Warnings: {issues.FindAll(i => i.Severity == AuditIssue.IssueSeverity.Warning).Count})");
            sb.AppendLine();
            sb.AppendLine("| STT | Mức Độ | Nhóm Lỗi | Thẻ / ID | Chi Tiết Vấn Đề |");
            sb.AppendLine("|---|---|---|---|---|");

            for (int i = 0; i < issues.Count; i++)
            {
                var item = issues[i];
                string severityTag = item.Severity == AuditIssue.IssueSeverity.Error ? "🔴 ERROR" : "🟡 WARNING";
                string assetName = item.TargetAsset != null ? $"{item.TargetAsset.upgradeName} (`{item.TargetAsset.id}`)" : "N/A";
                sb.AppendLine($"| {i + 1} | {severityTag} | {item.Category} | {assetName} | {item.Message} |");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Đồng bộ hóa toàn bộ file .asset từ Assets/_Data/Upgrades sang Assets/Resources/Upgrades.
        /// </summary>
        public static int SyncDataToResources()
        {
            if (!Directory.Exists(SOURCE_DIR))
            {
                Debug.LogWarning($"[UpgradeStudioAuditEngine] Thư mục nguồn {SOURCE_DIR} không tồn tại.");
                return 0;
            }

            if (!Directory.Exists(RESOURCES_DIR))
            {
                Directory.CreateDirectory(RESOURCES_DIR);
            }

            int copiedCount = 0;
            string[] sourceFiles = Directory.GetFiles(SOURCE_DIR, "*.*", SearchOption.AllDirectories);

            foreach (var src in sourceFiles)
            {
                if (src.EndsWith(".meta")) continue;
                string ext = Path.GetExtension(src).ToLower();
                if (ext != ".asset" && ext != ".prefab") continue;

                string relPath = src.Substring(SOURCE_DIR.Length).TrimStart('\\', '/');
                string dest = Path.Combine(RESOURCES_DIR, relPath);
                string destDir = Path.GetDirectoryName(dest);

                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                File.Copy(src, dest, true);
                copiedCount++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"<color=#00FF88>[UpgradeStudioAuditEngine]</color> Đã đồng bộ thành công {copiedCount} tệp sang Resources/Upgrades!");
            return copiedCount;
        }
    }
}
#endif
