#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Multiplayer.Core;
using ProjectZombie.Features.Combat.Coop;
using ProjectZombie.EditorTools.BuildSync;

namespace ProjectZombie.Editor.Testing
{
    /// <summary>
    /// Module Kiểm toán Toàn vẹn Tài nguyên (Asset & Prefab Integrity Auditor).
    /// Đơn trách nhiệm:
    /// 1. Kiểm tra tính toàn vẹn của Prefab nhân vật (đầy đủ các component thiết yếu).
    /// 2. So sánh và phát hiện độ lệch giữa Nguồn Gốc (Master: Assets/_Prefabs) và Bản Sao (Mirror: Assets/Resources).
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
        private const string MIRROR_PLAYERS_DIR = "Assets/Resources/Players";

        /// <summary>
        /// Chạy kiểm toán toàn diện tính toàn vẹn và độ đồng bộ của Player Prefabs.
        /// </summary>
        public static AuditReport AuditPlayerPrefabs()
        {
            var report = new AuditReport();

            foreach (var prefabName in PLAYER_PREFAB_NAMES)
            {
                string masterPath = Path.Combine(MASTER_PLAYERS_DIR, prefabName).Replace('\\', '/');
                string mirrorPath = Path.Combine(MIRROR_PLAYERS_DIR, prefabName).Replace('\\', '/');

                // 1. Kiểm tra tồn tại file Master
                if (!File.Exists(masterPath))
                {
                    report.Errors.Add($"[Thiếu Master Prefab] Không tìm thấy file gốc tại: {masterPath}");
                    continue;
                }

                // 2. Kiểm tra tồn tại file Mirror trong Resources
                if (!File.Exists(mirrorPath))
                {
                    report.Warnings.Add($"[Chưa đồng bộ Mirror] File chưa có trong Resources: {mirrorPath}");
                    report.DesyncedFiles.Add(prefabName);
                }
                else
                {
                    // So sánh nội dung/kích thước giữa Master và Mirror
                    if (!ResourceSyncEngine.AreFilesEqual(masterPath, mirrorPath))
                    {
                        report.Warnings.Add($"[Lệch phiên bản] {prefabName} ở Master và Resources có nội dung khác nhau.");
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
        /// Tự động sửa chữa an toàn: Bổ sung các component còn thiếu và đồng bộ Master -> Mirror.
        /// </summary>
        public static void FixAllIssues()
        {
            MultiplayerTools.SetupPhotonPlayerPrefabsTool.SetupPlayerPrefabs();
        }
    }
}
#endif
