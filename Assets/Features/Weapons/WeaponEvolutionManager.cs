using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Upgrades;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Quản lý công thức và logic Tiến Hóa Vũ Khí theo GDD v4.0.
    /// Kiểm tra điều kiện: Vũ khí Base ở Level 6 (Max Level) + Đã sở hữu Thẻ Passive tương ứng.
    /// </summary>
    public class WeaponEvolutionManager : MonoBehaviour
    {
        public static WeaponEvolutionManager Instance { get; private set; }

        [System.Serializable]
        public struct EvolutionRecipe
        {
            public string baseWeaponId;
            public string requiredPassiveId;
            public string evolutionWeaponId;

            public EvolutionRecipe(string baseWeaponId, string requiredPassiveId, string evolutionWeaponId)
            {
                this.baseWeaponId = baseWeaponId;
                this.requiredPassiveId = requiredPassiveId;
                this.evolutionWeaponId = evolutionWeaponId;
            }
        }

        [Header("Evolution Recipes (GDD v4.0 Matrix)")]
        [SerializeField] private List<EvolutionRecipe> _recipes = new List<EvolutionRecipe>
        {
            new EvolutionRecipe("W001", "P001", "E001"), // Nỏ Thần -> Nỏ Liên Châu
            new EvolutionRecipe("W002", "P002", "E002"), // Bút Phán Quan -> Bút Sinh Tử
            new EvolutionRecipe("W003", "P003", "E003"), // Bùa Trấn Yêu -> Bùa Cửu Huyền
            new EvolutionRecipe("W004", "P004", "E004"), // Cửu Vĩ Hồ Trảo -> Hồ Ly Cửu Vĩ
            new EvolutionRecipe("W005", "P005", "E005"), // Trống Đồng Đông Sơn -> Trống Trấn Quốc
            new EvolutionRecipe("W006", "P006", "E006"), // Lựu Đạn Thần Sa -> Bão Hỏa Diệm
            new EvolutionRecipe("W007", "P007", "E007"), // Cung Thạch Sanh -> Cung Thần Tiễn
            new EvolutionRecipe("W008", "P008", "E008"), // Đao Cửu Vĩ -> Hỏa Long Đao
            new EvolutionRecipe("W009", "P009", "E009"), // Trượng Long Vương -> Long Vương Trượng
            new EvolutionRecipe("W010", "P010", "E010"), // Linh Phù Ma Da -> Thủy Cung Linh
            new EvolutionRecipe("W011", "P011", "E011"), // Nước Thánh Chùa Hương -> Giếng Thiêng
            new EvolutionRecipe("W012", "P012", "E012"), // Phi Tiêu Bát Quái -> Phi Tiêu Cửu Cung
            
            // --- 5 Pháp Bảo Dân Gian Hài Hước (Slapstick Relics Evolution) ---
            new EvolutionRecipe("W_SLIPPER", "P001", "E_SLIPPER"), // Dép Tổ Ong -> Vạn Dép Quy Tông
            new EvolutionRecipe("W_POT", "P005", "E_POT"),         // Nồi Cơm Thạch Sanh -> Nồi Thần Bất Tử
            new EvolutionRecipe("W_PIPE", "P004", "E_PIPE"),       // Điếu Cày Cửu U -> Cửu U Long Phun Khói
            new EvolutionRecipe("R007", "P003", "E_R007"),         // Chiếu Trải Hoàng Tuyền -> Chiếu Thần Hoàng Kim
            new EvolutionRecipe("R008", "P001", "E_R008")          // Chổi Lông Gà -> Thiên Binh Chổi Quét
        };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Kiểm tra xem một vũ khí base có đạt đủ điều kiện tiến hóa hay không.
        /// </summary>
        /// <param name="weapon">Instance vũ khí hiện tại của player.</param>
        /// <param name="acquiredPassiveIds">Danh sách mã ID các thẻ passive player đã sở hữu.</param>
        /// <param name="evolutionWeaponId">Mã ID vũ khí tiến hóa nếu thỏa mãn điều kiện.</param>
        /// <returns>True nếu đạt đủ điều kiện ghép tiến hóa.</returns>
        public bool CanEvolve(WeaponBase weapon, HashSet<string> acquiredPassiveIds, out string evolutionWeaponId)
        {
            evolutionWeaponId = null;
            if (weapon == null || weapon.WeaponLevel < weapon.MaxLevel)
            {
                return false;
            }

            foreach (var recipe in _recipes)
            {
                if (recipe.baseWeaponId == weapon.weaponId)
                {
                    if (acquiredPassiveIds != null && acquiredPassiveIds.Contains(recipe.requiredPassiveId))
                    {
                        evolutionWeaponId = recipe.evolutionWeaponId;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
