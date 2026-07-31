using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.YinYang;

namespace ProjectZombie.Features.Player.Skills
{
    /// <summary>
    /// Kỹ năng Chủ động Thư Sinh: "Phán Quyết Tiền Định" (Mục 3.1.1 GDD v4.0).
    /// Chèn 1 hit ảo hệ tùy chọn vào buffer Tương Sinh Queue.
    /// Cooldown: 25s. Cost: 0.
    /// </summary>
    public class ThuSinhSignatureSkill : SignatureSkillBase
    {
        public override float Cooldown => 25.0f;

        public override void Execute(GameObject playerObj, System.Action<ElementType> onElementSelectedCallback = null)
        {
            if (playerObj == null) return;

            // Nếu caller truyền callback mở Overlay UI, Presenter/Overlay sẽ xử lý việc chọn
            if (onElementSelectedCallback != null)
            {
                onElementSelectedCallback.Invoke(ElementType.None);
            }
            else
            {
                // Fallback mặc định khi thi triển trực tiếp (không qua UI)
                ElementType fallbackElement = GetAutoSelectFallbackElement(playerObj);
                ApplyVirtualElementHit(fallbackElement);
            }
        }

        /// <summary>
        /// Tự động chọn thuộc tính Ngũ Hành khớp với vũ khí đang có Cooldown hồi chiêu lâu nhất.
        /// </summary>
        public ElementType GetAutoSelectFallbackElement(GameObject playerObj)
        {
            var weaponManager = playerObj.GetComponent<WeaponManager>();
            if (weaponManager == null || weaponManager.ActiveWeapons == null || weaponManager.ActiveWeapons.Count == 0)
            {
                return ElementType.Kim; // Default Kim
            }

            WeaponBase longestCdWeapon = null;
            float maxRemainingCd = -1f;

            foreach (var weapon in weaponManager.ActiveWeapons)
            {
                if (weapon == null) continue;
                float rem = weapon.RemainingCooldown;
                if (rem > maxRemainingCd)
                {
                    maxRemainingCd = rem;
                    longestCdWeapon = weapon;
                }
            }

            if (longestCdWeapon != null && longestCdWeapon.element != ElementType.None)
            {
                return longestCdWeapon.element;
            }

            return ElementType.Kim;
        }

        /// <summary>
        /// Đẩy phần tử ảo vào ElementCycleManager.
        /// </summary>
        public void ApplyVirtualElementHit(ElementType selectedElement)
        {
            if (selectedElement == ElementType.None)
            {
                selectedElement = ElementType.Kim;
            }

            if (ElementCycleManager.Instance != null)
            {
                ElementCycleManager.Instance.PushVirtualElementHit(selectedElement);
            }
            else
            {
                Debug.LogWarning("[ThuSinhSignatureSkill] ElementCycleManager.Instance chưa được khởi tạo!");
            }
        }
    }
}
