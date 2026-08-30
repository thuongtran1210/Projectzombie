using UnityEngine;

namespace ProjectZombie.Features.Shared.VFX
{
    /// <summary>
    /// Component gắn vào các Prefab VFX để tự động điều chỉnh quy mô hạt (Scale/Radius)
    /// và bật/tắt các tầng hiệu ứng con (Sub-layers) dựa trên cấp độ hiện tại của Pháp Bảo / Vũ Khí.
    /// Giúp giải quyết triệt để vấn đề: "Lv1 hiển thị full hiệu ứng hoành tráng như bản tiến hóa cuối".
    /// </summary>
    public class VFXLevelScaler : MonoBehaviour
    {
        [Header("Level Scaling Settings")]
        [Tooltip("Hệ số quy mô cơ bản tại Level 1 (VD: 0.55 = 55% kích thước tối đa)")]
        [Range(0.2f, 1.0f)]
        public float baseLevel1Scale = 0.6f;

        [Tooltip("Hệ số tăng kích thước mỗi cấp độ (từ Lv2 trở đi)")]
        [Range(0.05f, 0.3f)]
        public float scalePerLevel = 0.1f;

        [Header("Sub-Layer Activation Thresholds")]
        [Tooltip("Các ParticleSystem / GameObject con chỉ được kích hoạt từ Cấp 3 trở lên")]
        public GameObject[] tier2SubLayers;

        [Tooltip("Các ParticleSystem / GameObject con siêu hiệu ứng (Vết nứt đất, Shockwave, Hào quang bùng nổ) chỉ bật từ Cấp 5 / Evolution")]
        public GameObject[] tier3UltimateLayers;

        private ParticleSystem[] _allParticleSystems;
        private Vector3 _originalLocalScale;

        private void Awake()
        {
            _originalLocalScale = transform.localScale;
            _allParticleSystems = GetComponentsInChildren<ParticleSystem>(true);
        }

        /// <summary>
        /// Áp dụng cấp độ vũ khí vào VFX trước khi phát hiệu ứng.
        /// </summary>
        /// <param name="weaponLevel">Cấp độ vũ khí (1 -> 5+)</param>
        public void ApplyLevelScaling(int weaponLevel)
        {
            int lvl = Mathf.Max(1, weaponLevel);

            // 1. Điều chỉnh tỉ lệ kích thước tổng thể
            float levelScaleFactor = Mathf.Clamp(baseLevel1Scale + (lvl - 1) * scalePerLevel, 0.4f, 1.5f);
            transform.localScale = _originalLocalScale * levelScaleFactor;

            // 2. Bật/Tắt các tầng hiệu ứng phụ (Tier 2 - Cấp 3+)
            if (tier2SubLayers != null)
            {
                bool enableTier2 = (lvl >= 3);
                for (int i = 0; i < tier2SubLayers.Length; i++)
                {
                    if (tier2SubLayers[i] != null)
                    {
                        tier2SubLayers[i].SetActive(enableTier2);
                    }
                }
            }

            // 3. Bật/Tắt các tầng hiệu ứng tối thượng (Tier 3 - Cấp 5 / Evolution)
            if (tier3UltimateLayers != null)
            {
                bool enableTier3 = (lvl >= 5);
                for (int i = 0; i < tier3UltimateLayers.Length; i++)
                {
                    if (tier3UltimateLayers[i] != null)
                    {
                        tier3UltimateLayers[i].SetActive(enableTier3);
                    }
                }
            }
        }
    }
}
