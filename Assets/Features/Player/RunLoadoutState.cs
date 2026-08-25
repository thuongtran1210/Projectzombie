using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// Lưu trữ trạng thái Loadout xuất trận (Nhân vật + 1 Vũ Khí Chính + Tối đa 3 Pháp Bảo Hộ Thân)
    /// Được chọn từ Sảnh Chờ ngoài trận và truyền vào Scene Gameplay theo GDD v5.0.
    /// </summary>
    public static class RunLoadoutState
    {
        public static CharacterEntry SelectedCharacter { get; set; }
        public static WeaponData SelectedPrimaryWeapon { get; set; }
        public static List<WeaponData> SelectedRelics { get; set; } = new List<WeaponData>();

        public static bool HasCustomLoadout => SelectedPrimaryWeapon != null || (SelectedRelics != null && SelectedRelics.Count > 0);

        /// <summary>
        /// Thiết lập toàn bộ cấu hình Loadout trước khi xuất trận.
        /// </summary>
        public static void SetLoadout(CharacterEntry character, WeaponData primaryWeapon, List<WeaponData> relics)
        {
            SelectedCharacter = character;
            SelectedPrimaryWeapon = primaryWeapon;
            
            SelectedRelics.Clear();
            if (relics != null)
            {
                for (int i = 0; i < Mathf.Min(3, relics.Count); i++)
                {
                    if (relics[i] != null && !SelectedRelics.Contains(relics[i]))
                    {
                        SelectedRelics.Add(relics[i]);
                    }
                }
            }

            Debug.Log($"<color=#00FF88>[RunLoadoutState]</color> Đã thiết lập Loadout: Hero={(character != null ? character.characterName : "Default")}, Primary={(primaryWeapon != null ? primaryWeapon.weaponName : "None")}, Relics Count={SelectedRelics.Count}");
        }

        /// <summary>
        /// Xóa bỏ cấu hình tùy chỉnh để quay về mặc định.
        /// </summary>
        public static void ResetLoadout()
        {
            SelectedCharacter = null;
            SelectedPrimaryWeapon = null;
            SelectedRelics.Clear();
        }
    }
}
