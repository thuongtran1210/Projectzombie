using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Defense3D.Loadout;
using ProjectZombie.Features.MetaProgression.Gacha.Data;

namespace ProjectZombie.Features.MetaProgression.Gacha.Core
{
    /// <summary>
    /// Nguồn cấp dữ liệu Banner Gacha mặc định từ ScriptableObject (Single Source of Truth).
    /// </summary>
    public class LocalSOGachaDataProvider : IGachaDataProvider
    {
        private readonly Dictionary<string, GachaBannerConfigSO> _bannerMap = new Dictionary<string, GachaBannerConfigSO>(StringComparer.OrdinalIgnoreCase);

        public LocalSOGachaDataProvider(IEnumerable<GachaBannerConfigSO> initialBanners = null)
        {
            if (initialBanners != null)
            {
                foreach (var b in initialBanners)
                {
                    if (b != null && !string.IsNullOrEmpty(b.bannerId) && IsValidDefenseBanner(b))
                    {
                        _bannerMap[b.bannerId] = b;
                    }
                }
            }
        }

        public GachaBannerConfigSO GetBanner(string bannerId)
        {
            if (string.IsNullOrEmpty(bannerId)) return null;
            if (_bannerMap.TryGetValue(bannerId, out var banner) && banner != null && IsValidDefenseBanner(banner))
            {
                return banner;
            }

            // 1. Nạp từ Resources (Single Source of Truth cho cả Editor & Android APK)
            var loaded = Resources.Load<GachaBannerConfigSO>($"Gacha/{bannerId}") ?? Resources.Load<GachaBannerConfigSO>(bannerId);
#if UNITY_EDITOR
            // 2. Fallback trực tiếp sang Assets/_Data/Gacha/ trong Editor
            if (loaded == null || !IsValidDefenseBanner(loaded))
            {
                loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<GachaBannerConfigSO>($"Assets/_Data/Gacha/{bannerId}.asset");
            }
#endif
            if (loaded != null)
            {
                _bannerMap[bannerId] = loaded;
                return loaded;
            }

            return null;
        }

        public void RegisterBanner(GachaBannerConfigSO banner)
        {
            if (banner != null && !string.IsNullOrEmpty(banner.bannerId))
            {
                _bannerMap[banner.bannerId] = banner;
            }
        }

        public void RefreshBannerData(Action onCompleted)
        {
            onCompleted?.Invoke();
        }

        public static bool IsValidDefenseBanner(GachaBannerConfigSO banner)
        {
            if (banner == null || banner.DropPool == null || banner.DropPool.Count == 0)
            {
                return false;
            }

            var catalog = Defense3DCatalog.CreateDefault();
            for (int i = 0; i < banner.DropPool.Count; i++)
            {
                var item = banner.DropPool[i];
                if (item == null || string.IsNullOrEmpty(item.relicId))
                {
                    return false;
                }

                if (item.dropType == GachaDropType.CharacterShard)
                {
                    if (!DefenseLoadoutIdMapper.TryMapHeroId(item.relicId, out _))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!catalog.Contains(item.relicId) && !DefenseLoadoutIdMapper.TryMapRelicId(item.relicId, out _))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
