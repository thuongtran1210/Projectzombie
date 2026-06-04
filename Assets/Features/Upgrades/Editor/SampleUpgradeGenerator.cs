using UnityEngine;
using UnityEditor;
using ProjectZombie.Features.Upgrades;
using System.IO;

namespace ProjectZombie.Features.Upgrades.Editor
{
    public class SampleUpgradeGenerator
    {
        [MenuItem("Tools/ProjectZombie/Generate Sample Upgrades (Old)")]
        public static void GenerateSamples()
        {
            string folderPath = "Assets/_Data/Upgrades/Samples";
            
            // Create folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder("Assets/_Data"))
            {
                AssetDatabase.CreateFolder("Assets", "_Data");
            }
            if (!AssetDatabase.IsValidFolder("Assets/_Data/Upgrades"))
            {
                AssetDatabase.CreateFolder("Assets/_Data", "Upgrades");
            }
            if (!AssetDatabase.IsValidFolder("Assets/_Data/Upgrades/Samples"))
            {
                AssetDatabase.CreateFolder("Assets/_Data/Upgrades", "Samples");
            }

            // Sample 1: Damage Upgrade
            CreateCommonUpgradeAsset(folderPath, "Damage_Up_Common", "Damage Boost", "Increase all damage by 10%.", 10f, new PlayerStatModifier { baseDamageBonus = 0.1f });

            // Sample 2: Attack Speed Upgrade (Weapon Upgrade actually fits better if it's weapon specific, but let's make it common for player if they want, or Weapon if it's weapon. Original used WeaponStatModifier for common, which was wrong, but let's fix it to use WeaponUpgradeData)
            CreateWeaponUpgradeAsset(folderPath, "AttackSpeed_Up_Weapon", "Quick Hands", "Increase attack speed by 15%.", 10f, "Pistol", new WeaponStatModifier { attackSpeedBonus = 0.15f });

            // Sample 3: Multi-Shot for Pistol
            CreateWeaponUpgradeAsset(folderPath, "Pistol_MultiShot_Rare", "Twin Barrels", "Pistol fires an additional projectile.", 5f, "Pistol", new WeaponStatModifier { projectileCountBonus = 1 });

            // Sample 4: Piercing Rounds
            CreateWeaponUpgradeAsset(folderPath, "Piercing_Rounds_Rare", "Piercing Rounds", "Projectiles pierce 1 additional enemy.", 3f, "Pistol", new WeaponStatModifier { pierceBonus = 1 });

            // Sample 5: Weapon Evolution
            CreateEvolutionUpgradeAsset(folderPath, "Pistol_Evolution", "Hand Cannon", "Evolve Pistol into Hand Cannon. Massive damage and piercing.", 1f, "Pistol", 6, "Damage Boost");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("Sample Upgrades generated successfully at " + folderPath);
        }

        private static void CreateCommonUpgradeAsset(string path, string fileName, string name, string desc, float weight, PlayerStatModifier modifier)
        {
            string fullPath = $"{path}/{fileName}.asset";
            if (AssetDatabase.LoadAssetAtPath<CommonUpgradeData>(fullPath) != null) return;

            CommonUpgradeData asset = ScriptableObject.CreateInstance<CommonUpgradeData>();
            asset.upgradeName = name;
            asset.description = desc;
            asset.upgradeType = UpgradeType.CommonUpgrade;
            asset.spawnWeight = weight;
            asset.playerStatModifier = modifier;

            AssetDatabase.CreateAsset(asset, fullPath);
        }

        private static void CreateWeaponUpgradeAsset(string path, string fileName, string name, string desc, float weight, string weaponId, WeaponStatModifier modifier)
        {
            string fullPath = $"{path}/{fileName}.asset";
            if (AssetDatabase.LoadAssetAtPath<WeaponUpgradeData>(fullPath) != null) return;

            WeaponUpgradeData asset = ScriptableObject.CreateInstance<WeaponUpgradeData>();
            asset.upgradeName = name;
            asset.description = desc;
            asset.upgradeType = UpgradeType.WeaponUpgrade;
            asset.spawnWeight = weight;
            asset.weaponId = weaponId;
            asset.statModifier = modifier;
            asset.requiredCurrentLevel = 1; // Default

            AssetDatabase.CreateAsset(asset, fullPath);
        }

        private static void CreateEvolutionUpgradeAsset(string path, string fileName, string name, string desc, float weight, string weaponId, int requiredLevel, string requiredPassive)
        {
            string fullPath = $"{path}/{fileName}.asset";
            if (AssetDatabase.LoadAssetAtPath<EvolutionUpgradeData>(fullPath) != null) return;

            EvolutionUpgradeData asset = ScriptableObject.CreateInstance<EvolutionUpgradeData>();
            asset.upgradeName = name;
            asset.description = desc;
            asset.upgradeType = UpgradeType.EvolutionUpgrade;
            asset.spawnWeight = weight;
            asset.weaponId = weaponId;
            asset.requiredCurrentLevel = requiredLevel;
            asset.requiredPassiveId = requiredPassive;

            AssetDatabase.CreateAsset(asset, fullPath);
        }
    }
}
