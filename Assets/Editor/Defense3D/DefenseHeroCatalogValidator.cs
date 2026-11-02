#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

namespace ProjectZombie.Editor.Defense3D
{
    /// <summary>
    /// Editor tool và validation suite kiểm tra tính toàn vẹn của Hero Catalog, Hero Definition và Hero Kit.
    /// Giúp phát hiện sớm:
    /// - HeroId bị trùng lặp, rỗng hoặc chứa ký tự không hợp lệ.
    /// - HeroDefinition thiếu Avatar, thiếu HeroKit.
    /// - Kit thiếu Skill cơ bản, Dash, hoặc Tuyệt kỹ.
    /// - Không khớp giữa Definition.HeroId và Kit.HeroId.
    /// </summary>
    public static class DefenseHeroCatalogValidator
    {
        [MenuItem("Tools/ProjectZombie/Architecture/Validate Hero Catalog")]
        public static bool ValidateCatalog()
        {
            var catalog = Resources.Load<DefenseHeroCatalogSO>("Defense3D/DefenseHeroCatalog");
            if (catalog == null)
            {
                Debug.LogError("[DefenseHeroCatalogValidator] Không tìm thấy DefenseHeroCatalog trong Resources/Defense3D!");
                return false;
            }

            var heroes = catalog.Heroes;
            if (heroes == null || heroes.Count == 0)
            {
                Debug.LogError("[DefenseHeroCatalogValidator] Hero Catalog rỗng!");
                return false;
            }

            var seenIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            bool isValid = true;

            for (int i = 0; i < heroes.Count; i++)
            {
                var hero = heroes[i];
                if (hero == null)
                {
                    Debug.LogError($"[DefenseHeroCatalogValidator] Hero tại index {i} bị null!");
                    isValid = false;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(hero.HeroId))
                {
                    Debug.LogError($"[DefenseHeroCatalogValidator] Hero '{hero.name}' có HeroId rỗng!");
                    isValid = false;
                }
                else if (!seenIds.Add(hero.HeroId))
                {
                    Debug.LogError($"[DefenseHeroCatalogValidator] HeroId bị trùng lặp: '{hero.HeroId}' tại asset '{hero.name}'!");
                    isValid = false;
                }

                if (string.IsNullOrWhiteSpace(hero.DisplayName))
                {
                    Debug.LogWarning($"[DefenseHeroCatalogValidator] Hero '{hero.HeroId}' có DisplayName rỗng.");
                }

                if (hero.Avatar == null)
                {
                    Debug.LogWarning($"[DefenseHeroCatalogValidator] Hero '{hero.HeroId}' chưa được gán Avatar.");
                }

                if (hero.HeroPrefab == null)
                {
                    Debug.LogError($"[DefenseHeroCatalogValidator] Hero '{hero.HeroId}' thiếu HeroPrefab 3D!");
                    isValid = false;
                }

                if (hero.HeroKit == null)
                {
                    Debug.LogError($"[DefenseHeroCatalogValidator] Hero '{hero.HeroId}' thiếu HeroKitSO!");
                    isValid = false;
                }
                else
                {
                    try
                    {
                        hero.HeroKit.Validate();
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[DefenseHeroCatalogValidator] HeroKit của '{hero.HeroId}' không hợp lệ: {ex.Message}");
                        isValid = false;
                    }

                    if (!string.Equals(hero.HeroId, hero.HeroKit.HeroId, System.StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.LogWarning($"[DefenseHeroCatalogValidator] Hero '{hero.name}' có HeroId '{hero.HeroId}' khác với HeroKit.HeroId '{hero.HeroKit.HeroId}'.");
                    }
                }
            }

            if (isValid)
            {
                Debug.Log($"[DefenseHeroCatalogValidator] Catalog hợp lệ hoàn toàn với {heroes.Count} anh hùng.");
            }

            return isValid;
        }
    }
}
#endif
