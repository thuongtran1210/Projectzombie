using UnityEngine;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// [LOẠI 1: CHỈ DẤU HƯỚNG ĐI / ĐỊNH HƯỚNG BƯỚC CHÂN (PASSIVE DIRECTION INDICATOR)]
    /// ---------------------------------------------------------------------------------------------
    /// - Vai trò: Hiển thị liên tục (Passive) một mũi tên/vòng cung nhỏ sát dưới chân nhân vật (0.4m).
    /// - Mục đích: Giúp người chơi nhận biết hướng mặt, góc nhìn và hướng di chuyển theo Joystick.
    /// - Phân biệt với [LOẠI 2 - SkillAimIndicatorController]:
    ///     + CombatAimIndicator: Nhỏ gọn dưới chân, luôn hiện, không thay đổi theo kích thước kỹ năng.
    ///     + SkillAimIndicatorController: Chỉ hiện khi ĐÈ/KÉO nút Skill, vẽ vùng chém/bắn lớn (MOBA Telegraph).
    /// ---------------------------------------------------------------------------------------------
    /// </summary>
    public class CombatAimIndicator : MonoBehaviour
    {
        [Header("Aim Indicator Settings")]
        [SerializeField] private Sprite indicatorSprite;
        [SerializeField] private float indicatorDistance = 0.4f;
        [SerializeField] private Vector3 indicatorScale = new Vector3(0.6f, 0.6f, 1f);

        private SpriteRenderer _aimRenderer;
        private Transform _indicatorTransform;

        public void Initialize(CharacterAttackConfig config = null)
        {
            if (_indicatorTransform != null) return;

            GameObject arrowObj = new GameObject("VFX_Attack_Aim_Indicator");
            arrowObj.transform.SetParent(transform, false);
            arrowObj.transform.localPosition = Vector3.zero;
            arrowObj.transform.localScale = indicatorScale;

            _indicatorTransform = arrowObj.transform;
            _aimRenderer = arrowObj.AddComponent<SpriteRenderer>();

            if (indicatorSprite == null)
            {
                indicatorSprite = Resources.Load<Sprite>("Art/UI/HUD/Tex_Attack_Aim_Arc_Reticle");
#if UNITY_EDITOR
                if (indicatorSprite == null)
                {
                    indicatorSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Tex_Attack_Aim_Arc_Reticle.png");
                }
#endif
            }

            _aimRenderer.sprite = indicatorSprite;
            _aimRenderer.sortingLayerName = "Skill";
            _aimRenderer.sortingOrder = 3;

            ApplyThemeColor(config);
        }

        public void ApplyThemeColor(CharacterAttackConfig config)
        {
            if (_aimRenderer == null) return;

            // Màu sắc theo bản sắc nguyên tố tướng (Thư Sinh: Vàng Kim, Đạo Sĩ: Xanh Ngọc, Thanh Đồng: Đỏ Cam, Ẩn Sĩ: Hổ Phách)
            Color themeColor = new Color(1.0f, 0.85f, 0.2f, 0.65f);
            if (config != null)
            {
                if (config.attackName.Contains("Tiên Đạo") || config.attackName.Contains("Linh Phù"))
                    themeColor = new Color(0.25f, 0.95f, 0.85f, 0.65f); // Xanh ngọc
                else if (config.attackName.Contains("Đuốc") || config.attackName.Contains("Lửa"))
                    themeColor = new Color(1.0f, 0.4f, 0.1f, 0.7f); // Đỏ cam
                else if (config.attackName.Contains("Thạch") || config.attackName.Contains("Địa"))
                    themeColor = new Color(0.9f, 0.65f, 0.25f, 0.7f); // Hổ phách
            }
            _aimRenderer.color = themeColor;
        }

        public void UpdateAim(Vector2 attackDirection)
        {
            if (_indicatorTransform == null)
            {
                Initialize();
            }

            if (_indicatorTransform != null && attackDirection != Vector2.zero)
            {
                float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
                _indicatorTransform.rotation = Quaternion.Euler(0, 0, angle);
                _indicatorTransform.localPosition = (Vector3)(attackDirection * indicatorDistance);
            }
        }

        public void SetVisible(bool isVisible)
        {
            if (_aimRenderer != null)
            {
                _aimRenderer.enabled = isVisible;
            }
        }
    }
}
