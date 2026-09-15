#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades.Editor.Studio
{
    /// <summary>
    /// Bộ vẽ trực quan (Live WYSIWYG Renderer) mô phỏng chính xác thẻ bài Nâng Cấp in-game trên giao diện Editor.
    /// </summary>
    public static class UpgradeStudioCardPreviewRenderer
    {
        private static GUIStyle _cardBgStyle;
        private static GUIStyle _headerStyle;
        private static GUIStyle _nameStyle;
        private static GUIStyle _categoryStyle;
        private static GUIStyle _descStyle;
        private static GUIStyle _badgeStyle;
        private static GUIStyle _bannerStyle;

        private static void InitStyles()
        {
            if (_cardBgStyle != null) return;

            _cardBgStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(12, 12, 12, 12),
                margin = new RectOffset(6, 6, 6, 6)
            };

            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                normal = { textColor = new Color(1f, 0.85f, 0.4f) }
            };

            _nameStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                normal = { textColor = Color.white },
                wordWrap = true
            };

            _categoryStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                normal = { textColor = new Color(0.2f, 0.85f, 0.6f) }
            };

            _descStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 12,
                richText = true,
                normal = { textColor = new Color(0.9f, 0.9f, 0.9f) }
            };

            _badgeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight,
                fontSize = 10,
                richText = true
            };

            _bannerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                richText = true
            };
        }

        public static void DrawCardPreview(UpgradeData data, float width = 280f)
        {
            InitStyles();

            if (data == null)
            {
                EditorGUILayout.HelpBox("Chọn hoặc tạo một thẻ nâng cấp để xem trước giao diện trực quan.", MessageType.Info);
                return;
            }

            // Xác định màu sắc khung thẻ theo phẩm cấp
            Color borderColor = new Color(0.35f, 0.25f, 0.18f); // Gỗ Mộc (Common)
            string tierName = "THƯỜNG (GỖ MUN)";

            bool isMythic = data is MythicCoreUpgradeData || data.upgradeType == UpgradeType.MythicCore;
            bool isAugment = data is MutationAugmentUpgradeData;
            bool isMicroStat = data is StatMicroUpgradeData;
            bool isEvolution = data is EvolutionUpgradeData || data.upgradeType == UpgradeType.EvolutionUpgrade;
            bool isBreakthrough = data.upgradeType == UpgradeType.BreakthroughUltimate && !isAugment;
            bool isSynergy = data is SynergyTraitUpgradeData || data.upgradeType == UpgradeType.SynergyTrait;

            if (isMythic)
            {
                borderColor = new Color(1f, 0.84f, 0f); // Vàng Hoàng Kim
                tierName = "THẦN THOẠI (HOÀNG KIM)";
            }
            else if (isAugment)
            {
                var aug = (MutationAugmentUpgradeData)data;
                switch (aug.tier)
                {
                    case AugmentTier.Silver:
                        borderColor = new Color(0.75f, 0.75f, 0.8f);
                        tierName = "LÕI BẠC (CHỈ SỐ & TIỆN ÍCH)";
                        break;
                    case AugmentTier.Gold:
                        borderColor = new Color(1f, 0.85f, 0.2f);
                        tierName = "LÕI VÀNG (CƯỜNG HÓA GIAO TRANH)";
                        break;
                    case AugmentTier.Prismatic:
                        borderColor = new Color(0.0f, 0.9f, 1f);
                        tierName = "LÕI KIM CƯƠNG (BẺ GÃY QUY TẮC)";
                        break;
                }
            }
            else if (isMicroStat)
            {
                borderColor = new Color(0.1f, 0.8f, 0.45f); // Xanh Lục Bảo Micro-Card
                tierName = "CHỈ SỐ NỀN TẢNG (1 DÒNG)";
            }
            else if (isEvolution || isBreakthrough)
            {
                borderColor = new Color(0.9f, 0.35f, 0.2f); // Hỏa Tinh / Tiến Hóa
                tierName = isEvolution ? "TIẾN HÓA (THẦN KHÍ)" : "ĐỘT PHÁ (TUYỆT KỸ)";
            }
            else if (isSynergy)
            {
                borderColor = new Color(0.0f, 0.9f, 1f); // Lam Ngọc / Hổ Phách
                tierName = "DUYÊN PHẬN (THẦN BINH THUẬT)";
            }
            else if (data.upgradeType == UpgradeType.RareUpgrade)
            {
                borderColor = new Color(0.2f, 0.85f, 0.6f); // Xanh Ngọc Bích
                tierName = "QUÝ HIẾM (NGỌC BÍCH)";
            }

            // Vẽ Box thẻ
            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = borderColor;

            GUILayout.BeginVertical(_cardBgStyle, GUILayout.Width(width));
            GUI.backgroundColor = prevBg;

            // 1. Phẩm cấp & Tag Hệ
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>[{tierName}]</b>", _categoryStyle);
            GUILayout.FlexibleSpace();
            if (data.element != ElementType.None)
            {
                GUILayout.Label($"<b>[Hệ {data.element}]</b>", _badgeStyle);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            // 2. Icon Thẻ
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            Rect iconRect = GUILayoutUtility.GetRect(72, 72, GUILayout.Width(72), GUILayout.Height(72));
            GUI.Box(iconRect, GUIContent.none, EditorStyles.helpBox);

            if (data.icon != null && data.icon.texture != null)
            {
                GUI.DrawTexture(iconRect, data.icon.texture, ScaleMode.ScaleToFit);
            }
            else
            {
                GUI.Label(iconRect, "No Icon\n(Dự phòng)", EditorStyles.centeredGreyMiniLabel);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(8);

            // 3. Tên Thẻ & Danh mục
            string titleName = !string.IsNullOrEmpty(data.upgradeName) ? data.upgradeName : "(Chưa đặt tên)";
            GUILayout.Label(titleName, _nameStyle);
            GUILayout.Label(data.GetCategoryDisplayName(), _categoryStyle);

            GUILayout.Space(4);

            // 4. Banner Đặc Biệt (Nếu là Mythic / Augment / MicroStat / Evolution / Trait)
            if (isMythic)
            {
                var core = data as MythicCoreUpgradeData;
                string title = core != null && !string.IsNullOrEmpty(core.mythicTitle) ? core.mythicTitle : "ĐẠI LÕI KHỞI ĐẦU";
                GUILayout.Label($"<color=#FFD700><b>✦ {title} ✦</b></color>", _bannerStyle);
            }
            else if (isAugment)
            {
                var aug = (MutationAugmentUpgradeData)data;
                string bannerColor = aug.tier switch
                {
                    AugmentTier.Prismatic => "#00E5FF",
                    AugmentTier.Gold => "#FFD700",
                    _ => "#C0C0C0"
                };
                GUILayout.Label($"<color={bannerColor}><b>✦ MỐC ĐỘT BIẾN [{aug.tier.ToString().ToUpper()}] ✦</b></color>", _bannerStyle);
            }
            else if (isMicroStat)
            {
                var micro = (StatMicroUpgradeData)data;
                string sum = !string.IsNullOrEmpty(micro.oneLineSummary) ? micro.oneLineSummary : "+Chỉ Số Nền Tảng";
                GUILayout.Label($"<color=#00FF88><size=13><b>⚡ {sum}</b></size></color>", _bannerStyle);
            }
            else if (isEvolution)
            {
                GUILayout.Label("<color=#FF4444><b>CÔNG THỨC DUNG HỢP HOÀN TẤT</b></color>", _bannerStyle);
            }
            else if (isBreakthrough)
            {
                GUILayout.Label("<color=#FF7700><b>✦ ĐỘT PHÁ TUYỆT KỸ ✦</b></color>", _bannerStyle);
            }
            else if (isSynergy)
            {
                var trait = data as SynergyTraitUpgradeData;
                string arcName = trait != null ? trait.requiredArchetype.GetDisplayName() : "LÕI";
                GUILayout.Label($"<color=#00E5FF><b>✦ THẦN BINH THUẬT: {arcName.ToUpper()} ✦</b></color>", _bannerStyle);
            }

            GUILayout.Space(6);

            // 5. Mô tả thẻ Rich Text
            string desc = !string.IsNullOrEmpty(data.description) ? data.description : "<i>(Chưa có mô tả kỹ năng)</i>";
            GUILayout.Label(desc, _descStyle);

            GUILayout.Space(10);

            // 6. Footer Cấp độ & Trọng số
            GUILayout.BeginHorizontal();
            string maxLevelText = data.maxLevel > 0 ? $"Tối đa Lv.{data.maxLevel}" : "Vô hạn";
            GUILayout.Label($"Cấp: {maxLevelText}", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Trọng số: {data.spawnWeight}", EditorStyles.miniLabel);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
    }
}
#endif
