using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.Features.UI.StatsAndSkills
{
    public class PlayerHUDView : MonoBehaviour
    {
        [Header("Health & EXP")]
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Slider expSlider;

        [Header("Skills Display")]
        [SerializeField] private Transform skillsContainer;
        [SerializeField] private SkillUIEntry skillEntryPrefab;
        [SerializeField] private TooltipUI tooltipUI; // Tham chiếu đến tooltip chung

        private List<SkillUIEntry> _spawnedSkills = new List<SkillUIEntry>();

        public void UpdateHealth(float current, float max)
        {
            if (hpSlider != null)
            {
                hpSlider.maxValue = max;
                hpSlider.value = current;
            }
        }

        public void UpdateExp(float currentExp, float maxExp)
        {
            if (expSlider != null)
            {
                expSlider.maxValue = maxExp;
                expSlider.value = currentExp;
            }
        }

        public void UpdateSkills(IReadOnlyList<WeaponBase> weapons)
        {
            // Clear old entries (hoặc có thể dùng Object Pooling)
            foreach (var entry in _spawnedSkills)
            {
                Destroy(entry.gameObject);
            }
            _spawnedSkills.Clear();

            // Spawn new entries
            foreach (var weapon in weapons)
            {
                // Giả định WeaponBase có property GetIcon(), GetWeaponName(), GetDescription() (tuỳ thuộc vào UpgradeData hoặc config của bạn)
                // Vì không chắc WeaponBase hiện tại có những trường nào, ta tạm giả định hoặc lấy từ Data.
                // Ở đây dùng mock data để thiết lập nếu chưa có
                
                SkillUIEntry newEntry = Instantiate(skillEntryPrefab, skillsContainer);
                
                // MOCK UP: Sẽ cần thay thế bằng dữ liệu thực từ weapon
                Sprite mockIcon = null; 
                string mockName = $"Weapon {weapon.weaponId}";
                string mockDesc = "Description for " + weapon.weaponId;
                int level = weapon.WeaponLevel;

                newEntry.Setup(mockIcon, level, mockName, mockDesc, tooltipUI);
                _spawnedSkills.Add(newEntry);
            }
        }
    }
}
