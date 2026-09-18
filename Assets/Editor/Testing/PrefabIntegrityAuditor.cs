#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Multiplayer.Core;
using ProjectZombie.Features.Combat.Coop;
using ProjectZombie.EditorTools.BuildSync;

namespace ProjectZombie.Editor.Testing
{
    /// <summary>
    /// Module Kiểm toán Toàn vẹn Tài nguyên (Asset & Prefab Integrity Auditor).
    /// Đơn trách nhiệm:
    /// 1. Kiểm tra tính toàn vẹn của Prefab nhân vật gốc tại Assets/_Prefabs/Characters/Players (đầy đủ các component thiết yếu).
    /// 2. Xác thực việc đăng ký đúng vào Addressables Group_Core_Preload (Chuẩn SSOT).
    /// 3. Cung cấp chức năng kiểm tra và tự sửa chữa an toàn mà không làm mất cấu hình.
    /// </summary>
    public static class PrefabIntegrityAuditor
    {
        public class AuditReport
        {
            public bool IsHealthy => Errors.Count == 0;
            public List<string> Errors { get; } = new List<string>();
            public List<string> Warnings { get; } = new List<string>();
            public List<string> DesyncedFiles { get; } = new List<string>();
        }

        private static readonly string[] PLAYER_PREFAB_NAMES = new string[]
        {
            "Dao Si.prefab",
            "An Si.prefab",
            "Thanh Dong.prefab",
            "Thu Sinh.prefab"
        };

        private const string MASTER_PLAYERS_DIR = "Assets/_Prefabs/Characters/Players";

        /// <summary>
        /// Chạy kiểm toán toàn diện tính toàn vẹn và việc đăng ký Addressables của Player Prefabs.
        /// </summary>
        public static AuditReport AuditPlayerPrefabs()
        {
            var report = new AuditReport();

            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            var preloadGroup = settings != null ? settings.FindGroup("Group_Core_Preload") : null;

            foreach (var prefabName in PLAYER_PREFAB_NAMES)
            {
                string masterPath = Path.Combine(MASTER_PLAYERS_DIR, prefabName).Replace('\\', '/');

                // 1. Kiểm tra tồn tại file Master
                if (!File.Exists(masterPath))
                {
                    report.Errors.Add($"[Thiếu Master Prefab] Không tìm thấy file gốc tại: {masterPath}");
                    continue;
                }

                // 2. Kiểm tra việc đăng ký trong Addressables
                if (settings != null)
                {
                    string guid = AssetDatabase.AssetPathToGUID(masterPath);
                    var entry = settings.FindAssetEntry(guid);
                    if (entry == null)
                    {
                        string entryAddress = Path.GetFileNameWithoutExtension(prefabName);
                        report.Warnings.Add($"[Chưa có trong Addressables Local] '{entryAddress}' chưa được đăng ký trong Group_Core_Preload.");
                        report.DesyncedFiles.Add(prefabName);
                    }
                }

                // 3. Kiểm tra các component bắt buộc trên Prefab gốc
                GameObject root = PrefabUtility.LoadPrefabContents(masterPath);
                if (root != null)
                {
                    try
                    {
                        if (!root.TryGetComponent<PlayerStats>(out _))
                            report.Errors.Add($"[{prefabName}] Thiếu Component 'PlayerStats'!");
                        
                        if (!root.TryGetComponent<HealthSystem>(out _))
                            report.Errors.Add($"[{prefabName}] Thiếu Component 'HealthSystem'!");

                        if (!root.TryGetComponent<PlayerController>(out _))
                            report.Errors.Add($"[{prefabName}] Thiếu Component 'PlayerController'!");

                        if (!root.TryGetComponent<CharacterCombat>(out _))
                            report.Errors.Add($"[{prefabName}] Thiếu Component 'CharacterCombat' (Gây lỗi đòn đánh thường)!");

                        if (!root.TryGetComponent<WeaponManager>(out _))
                            report.Errors.Add($"[{prefabName}] Thiếu Component 'WeaponManager'!");

                        if (!root.TryGetComponent<NetworkPlayerCharacter>(out _))
                            report.Warnings.Add($"[{prefabName}] Chưa gắn 'NetworkPlayerCharacter' phục vụ Multiplayer.");
                    }
                    finally
                    {
                        PrefabUtility.UnloadPrefabContents(root);
                    }
                }
            }

            return report;
        }

        /// <summary>
        /// Tự động sửa chữa an toàn: Bổ sung các component còn thiếu và đăng ký vào Addressables Local.
        /// </summary>
        public static void FixAllIssues()
        {
            MultiplayerTools.SetupPhotonPlayerPrefabsTool.SetupPlayerPrefabs();
            ProjectZombie.Editor.AddressablesTools.AddressableGroupsSetupTool.SetupStandardGroups();
        }
    }
}
#endif
