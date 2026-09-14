#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Công cụ kiểm tra tính toàn vẹn của tất cả WeaponData ScriptableObjects trong dự án.
    /// Giúp phát hiện sớm các vũ khí chưa được gán weaponPrefab hoặc lỗi cấu hình trước khi Build Android.
    /// </summary>
    public static class WeaponDataValidator
    {
        [MenuItem("Tools/ProjectZombie/Validate Weapons", priority = 100)]
        public static void ValidateAllWeapons()
        {
            string[] guids = AssetDatabase.FindAssets("t:WeaponData");
            if (guids == null || guids.Length == 0)
            {
                Debug.LogWarning("[WeaponDataValidator] Không tìm thấy bất kỳ WeaponData nào trong dự án.");
                return;
            }

            int totalCount = guids.Length;
            int validCount = 0;
            int missingPrefabCount = 0;
            HashSet<string> seenIds = new HashSet<string>();
            List<string> duplicateIds = new List<string>();
            List<string> missingPrefabWeapons = new List<string>();

            Debug.Log($"<color=#00E5FF>[WeaponDataValidator]</color> Bắt đầu kiểm tra {totalCount} WeaponData assets...");

            // Lưu trữ map: weaponId -> danh sách các đường dẫn chứa weaponId đó
            Dictionary<string, List<string>> idToPathsMap = new Dictionary<string, List<string>>(System.StringComparer.OrdinalIgnoreCase);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                WeaponData weaponData = AssetDatabase.LoadAssetAtPath<WeaponData>(path);

                if (weaponData == null) continue;

                // 1. Kiểm tra weaponId rỗng
                if (string.IsNullOrEmpty(weaponData.weaponId))
                {
                    Debug.LogError($"[WeaponDataValidator] '{weaponData.name}' tại '{path}' có weaponId đang bị RỖNG!", weaponData);
                    continue;
                }

                if (!idToPathsMap.TryGetValue(weaponData.weaponId, out var pathList))
                {
                    pathList = new List<string>();
                    idToPathsMap[weaponData.weaponId] = pathList;
                }
                pathList.Add(path);

                // 2. Kiểm tra weaponPrefab
                if (weaponData.weaponPrefab == null)
                {
                    missingPrefabCount++;
                    missingPrefabWeapons.Add($"{weaponData.name} [ID: {weaponData.weaponId}] tại {path}");
                    Debug.LogError($"<color=#FF4444>[WeaponDataValidator] LỖI THIẾU PREFAB:</color> WeaponData '<b>{weaponData.name}</b>' (ID: {weaponData.weaponId}) chưa được gán 'weaponPrefab' tại '{path}'!", weaponData);
                }
                else
                {
                    validCount++;
                }
            }

            // 3. Phân tích Trùng Lặp (Phân biệt giữa Sync Bản Sao Resources và Trùng Lặp Thật Trong Cùng Thư Mục Gốc)
            List<string> genuineDuplicates = new List<string>();
            int syncedCopiesCount = 0;

            foreach (var kvp in idToPathsMap)
            {
                string id = kvp.Key;
                List<string> paths = kvp.Value;

                if (paths.Count > 1)
                {
                    // Lọc xem có bao nhiêu file nằm trong thư mục gốc master (Assets/_Data/Weapons/)
                    var masterPaths = paths.FindAll(p => p.StartsWith("Assets/_Data/", System.StringComparison.OrdinalIgnoreCase));
                    if (masterPaths.Count > 1)
                    {
                        // Đây là trùng lặp thật trong dữ liệu gốc!
                        genuineDuplicates.Add($"ID '{id}' bị định nghĩa {masterPaths.Count} lần trong _Data: [{string.Join(", ", masterPaths)}]");
                    }
                    else
                    {
                        // Đây là bản sao đồng bộ tự động giữa _Data và Resources (hoặc ScriptableObjects) cho Android Build
                        syncedCopiesCount++;
                    }
                }
            }

            if (genuineDuplicates.Count > 0)
            {
                Debug.LogError($"<color=#FF3333>[WeaponDataValidator] PHÁT HIỆN TRÙNG LẶP TRONG DỮ LIỆU GỐC (_Data):</color>\n{string.Join("\n", genuineDuplicates)}");
            }
            else
            {
                Debug.Log($"<color=#00FF88>[WeaponDataValidator] Kiểm tra ID:</color> Không có ID nào bị trùng lặp trong dữ liệu gốc _Data ({syncedCopiesCount} vũ khí có bản sao đồng bộ Resources phục vụ build Android).");
            }

            if (missingPrefabCount > 0)
            {
                Debug.LogWarning($"<color=#FFAA00>[WeaponDataValidator] TỔNG KẾT:</color> Có <b>{missingPrefabCount}/{totalCount}</b> vũ khí CHƯA GÁN PREFAB! Cần bổ sung Prefab trước khi build Android.");
            }
            else
            {
                Debug.Log($"<color=#00FF88>[WeaponDataValidator] HOÀN TẤT:</color> 100% ({validCount}/{totalCount}) WeaponData đã được gán Prefab hợp lệ và an toàn với Android IL2CPP!");
            }
        }
    }
}
#endif
