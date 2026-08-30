#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Upgrades;
using ProjectZombie.Features.Player;
using System.Collections.Generic;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Debug Window hỗ trợ Tester / Game Designer thử nghiệm ngay lập tức toàn bộ các dạng Vũ Khí, Pháp Bảo và Tiến Hóa (Evolution) trong Playmode.
    /// Menu: Tools > ProjectZombie > Evolution & Relic Tester Window
    /// </summary>
    public class EvolutionAndRelicTesterWindow : EditorWindow
    {
        private Vector2 _scrollPos;
        private int _targetLevel = 5;

        [MenuItem("Tools/ProjectZombie/Evolution & Relic Tester Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<EvolutionAndRelicTesterWindow>("Weapon & Evolution Tester");
            window.minSize = new Vector2(480, 560);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("⚡ BỘ CÔNG CỤ TEST PHÁP BẢO & TIẾN HÓA (EVOLUTION)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Công cụ này hoạt động trực tiếp khi đang chạy game (Play Mode):\n" +
                "1. Trang bị ngay lập tức bất kỳ Vũ Khí / Pháp Bảo nào.\n" +
                "2. Thăng cấp nhanh vũ khí từ Lv1 -> Lv5/Lv6 để kiểm tra Scaling VFX & Chỉ số.\n" +
                "3. Kích hoạt trực tiếp các dạng Tiến Hóa Cuối (Evolution) mà không cần chờ nhặt đồ.",
                MessageType.Info
            );

            if (!Application.isPlaying)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("⚠️ Vui lòng BẤM PLAY GAME để sử dụng các chức năng kích hoạt / trang bị!", MessageType.Warning);
                return;
            }

            var player = FindObjectOfType<PlayerController>();
            var weaponMgr = player != null ? player.GetComponentInChildren<WeaponManager>() : FindObjectOfType<WeaponManager>();

            if (weaponMgr == null)
            {
                EditorGUILayout.HelpBox("❌ Không tìm thấy WeaponManager trên Player trong Scene hiện tại!", MessageType.Error);
                return;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("🔧 CÔNG CỤ THAO TÁC NHANH TRÊN PLAYER", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _targetLevel = EditorGUILayout.IntSlider("Đặt Level Vũ Khí:", _targetLevel, 1, 6);
            if (GUILayout.Button($"Set Toàn Bộ Lv{_targetLevel}", GUILayout.Width(140)))
            {
                SetAllWeaponsLevel(weaponMgr, _targetLevel);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            // =========================================================================
            // 1. NHÓM 5 PHÁP BẢO DÂN GIAN HÀI HƯỚC (SLAPSTICK RELICS)
            // =========================================================================
            EditorGUILayout.LabelField("🏮 5 PHÁP BẢO DÂN GIAN (SLAPSTICK RELICS)", EditorStyles.boldLabel);
            DrawRelicRow(weaponMgr, "W_SLIPPER", "Dép Tổ Ong Thần Sa", "E_SLIPPER", "Vạn Dép Quy Tông (Evo)");
            DrawRelicRow(weaponMgr, "W_POT", "Nồi Cơm Thạch Sanh", "E_POT", "Nồi Thần Bất Tử (Evo)");
            DrawRelicRow(weaponMgr, "W_PIPE", "Điếu Cày Cửu U", "E_PIPE", "Cửu U Long Phun Khói (Evo)");
            DrawRelicRow(weaponMgr, "R007", "Chiếu Trải Hoàng Tuyền", "E_R007", "Chiếu Thần Hoàng Kim (Evo)");
            DrawRelicRow(weaponMgr, "R008", "Chổi Lông Gà Gia Truyền", "E_R008", "Thiên Binh Chổi Quét (Evo)");

            EditorGUILayout.Space(15);

            // =========================================================================
            // 2. NHÓM VŨ KHÍ GDD V4.0 (12 VŨ KHÍ CỔ PHONG CHÍNH)
            // =========================================================================
            EditorGUILayout.LabelField("⚔️ 12 VŨ KHÍ CỔ PHONG & TIẾN HÓA (GDD V4.0)", EditorStyles.boldLabel);
            DrawRelicRow(weaponMgr, "W001", "Nỏ Thần An Dương Vương", "E001", "Nỏ Liên Châu (Evo)");
            DrawRelicRow(weaponMgr, "W002", "Bút Phán Quan", "E002", "Bút Sinh Tử (Evo)");
            DrawRelicRow(weaponMgr, "W003", "Bùa Trấn Yêu", "E003", "Bùa Cửu Huyền (Evo)");
            DrawRelicRow(weaponMgr, "W004", "Cửu Vĩ Hồ Trảo", "E004", "Hồ Ly Cửu Vĩ (Evo)");
            DrawRelicRow(weaponMgr, "W005", "Trống Đồng Đông Sơn", "E005", "Trống Trấn Quốc (Evo)");
            DrawRelicRow(weaponMgr, "W006", "Lựu Đạn Thần Sa", "E006", "Bão Hỏa Diệm (Evo)");
            DrawRelicRow(weaponMgr, "W007", "Cung Thạch Sanh", "E007", "Cung Thần Tiễn (Evo)");
            DrawRelicRow(weaponMgr, "W008", "Đao Cửu Vĩ", "E008", "Hỏa Long Đao (Evo)");
            DrawRelicRow(weaponMgr, "W009", "Trượng Long Vương", "E009", "Long Vương Trượng (Evo)");
            DrawRelicRow(weaponMgr, "W010", "Linh Phù Ma Da", "E010", "Thủy Cung Linh (Evo)");
            DrawRelicRow(weaponMgr, "W011", "Nước Thánh Chùa Hương", "E011", "Giếng Thiêng (Evo)");
            DrawRelicRow(weaponMgr, "W012", "Phi Tiêu Bát Quái", "E012", "Phi Tiêu Cửu Cung (Evo)");

            EditorGUILayout.EndScrollView();
        }

        private void DrawRelicRow(WeaponManager weaponMgr, string baseId, string baseName, string evoId, string evoName)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField($"[{baseId}] {baseName}", EditorStyles.miniBoldLabel, GUILayout.Width(200));

            if (GUILayout.Button("Trang Bị (Lv1)", GUILayout.Width(90)))
            {
                EquipById(weaponMgr, baseId, 1);
            }

            if (GUILayout.Button("Max Lv5", GUILayout.Width(65)))
            {
                EquipById(weaponMgr, baseId, 5);
            }

            if (GUILayout.Button($"⚡ {evoName}", GUILayout.Width(150)))
            {
                EquipById(weaponMgr, evoId, 1);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void EquipById(WeaponManager weaponMgr, string weaponId, int targetLvl)
        {
            // 1. Tìm WeaponData trong AssetDatabase
            string[] guids = AssetDatabase.FindAssets($"t:WeaponData {weaponId}");
            WeaponData foundData = null;

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
                if (data != null && string.Equals(data.weaponId, weaponId, System.StringComparison.OrdinalIgnoreCase))
                {
                    foundData = data;
                    break;
                }
            }

            // Nếu là dạng Evolution (E_... hoặc E00...), map sang Base Weapon tương ứng nếu chưa có asset riêng
            string targetScriptId = weaponId;
            bool isEvolutionMode = false;
            if (weaponId.StartsWith("E_") || (weaponId.StartsWith("E") && weaponId.Length == 4))
            {
                isEvolutionMode = true;
                if (WeaponEvolutionManager.Instance != null && WeaponEvolutionManager.Instance.TryGetRecipeByEvolutionId(weaponId, out var recipe))
                {
                    targetScriptId = recipe.baseWeaponId;
                }
                else
                {
                    // Fallback map thủ công
                    if (weaponId == "E_SLIPPER") targetScriptId = "W_SLIPPER";
                    else if (weaponId == "E_POT") targetScriptId = "W_POT";
                    else if (weaponId == "E_PIPE") targetScriptId = "W_PIPE";
                    else if (weaponId == "E_R007") targetScriptId = "R007";
                    else if (weaponId == "E_R008") targetScriptId = "R008";
                    else if (weaponId.StartsWith("E0")) targetScriptId = "W" + weaponId.Substring(1);
                }

                // Thử tìm lại data theo Base ID nếu Evolution Data chưa tồn tại
                if (foundData == null)
                {
                    string[] baseGuids = AssetDatabase.FindAssets($"t:WeaponData {targetScriptId}");
                    foreach (var guid in baseGuids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        var data = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
                        if (data != null && string.Equals(data.weaponId, targetScriptId, System.StringComparison.OrdinalIgnoreCase))
                        {
                            foundData = data;
                            break;
                        }
                    }
                }
            }

            // Theo luật game: Người chơi chỉ mang đúng 1 Pháp Bảo (Relic) vào trận.
            // Gỡ bỏ toàn bộ các Pháp Bảo phụ trước đó (giữ lại Vũ Khí Chính - Primary Weapon)
            var activeWeapons = new List<WeaponBase>(weaponMgr.ActiveWeapons);
            for (int i = 0; i < activeWeapons.Count; i++)
            {
                var w = activeWeapons[i];
                if (w != null && !w.isPrimaryActiveWeapon)
                {
                    weaponMgr.RemoveWeapon(w);
                }
            }

            if (foundData != null)
            {
                weaponMgr.EquipWeaponFromData(foundData, isPrimary: false);
            }
            else
            {
                // Fallback tạo WeaponData runtime
                var runtimeData = ScriptableObject.CreateInstance<WeaponData>();
                runtimeData.weaponId = targetScriptId;
                runtimeData.weaponName = weaponId;
                runtimeData.isPassiveRelic = false; // Mặc định hỗ trợ nút bấm chủ động để test
                weaponMgr.EquipWeaponFromData(runtimeData, isPrimary: false);
            }

            // Tìm weapon vừa gắn và set Level (nếu là Evolution thì đặt max level 6 + buff sức mạnh)
            var currentActive = weaponMgr.ActiveWeapons;
            for (int i = 0; i < currentActive.Count; i++)
            {
                if (string.Equals(currentActive[i].weaponId, targetScriptId, System.StringComparison.OrdinalIgnoreCase))
                {
                    currentActive[i].WeaponLevel = isEvolutionMode ? currentActive[i].MaxLevel : targetLvl;
                    break;
                }
            }

            weaponMgr.NotifyWeaponsChanged();
            Debug.Log($"<color=#00FF88>[Tester]</color> Đã thay thế & trang bị duy nhất Pháp Bảo: <b>{weaponId}</b> (Level {targetLvl})! UI Button đã tự động cập nhật.");
        }

        private void SetAllWeaponsLevel(WeaponManager weaponMgr, int targetLvl)
        {
            var activeWeapons = weaponMgr.ActiveWeapons;
            for (int i = 0; i < activeWeapons.Count; i++)
            {
                activeWeapons[i].WeaponLevel = targetLvl;
            }
            Debug.Log($"[Tester] Đã đặt Level toàn bộ {activeWeapons.Count} vũ khí thành Level {targetLvl}!");
        }
    }
}
#endif
