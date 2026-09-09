using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.MetaProgression.Gacha.Data;

namespace ProjectZombie.Features.MetaProgression.Gacha.Core
{
    /// <summary>
    /// Nguồn cấp dữ liệu Banner Gacha mặc định từ ScriptableObject trong Resources / SerializeField.
    /// </summary>
    public class LocalSOGachaDataProvider : IGachaDataProvider
    {
        private readonly Dictionary<string, GachaBannerConfigSO> _bannerMap = new Dictionary<string, GachaBannerConfigSO>();

        public LocalSOGachaDataProvider(IEnumerable<GachaBannerConfigSO> initialBanners = null)
        {
            if (initialBanners != null)
            {
                foreach (var b in initialBanners)
                {
                    if (b != null && !string.IsNullOrEmpty(b.bannerId))
                    {
                        _bannerMap[b.bannerId] = b;
                    }
                }
            }
        }

        public GachaBannerConfigSO GetBanner(string bannerId)
        {
            if (string.IsNullOrEmpty(bannerId)) return null;
            if (_bannerMap.TryGetValue(bannerId, out var banner))
            {
                return banner;
            }

            // Fallback: Tìm trong Resources
            var loaded = Resources.Load<GachaBannerConfigSO>($"Gacha/{bannerId}");
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
            // Local SO sẵn sàng ngay lập tức
            onCompleted?.Invoke();
        }
    }
}
