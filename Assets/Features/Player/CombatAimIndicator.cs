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
        private Shared.ElementType? _displayedElement;
        private float _resonanceUntil = float.NegativeInfinity;

        /// <summary>Uses the existing indicator for the same visible feedback on real and virtual resonance.</summary>
        public void PulseResonance(float duration)
        {
            _resonanceUntil = Time.time + duration;
            if (_indicatorTransform != null)
                _indicatorTransform.localScale = indicatorScale * (duration > 0f ? 1.2f : 1f);
        }

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

        public void ApplyElementColor(Shared.ElementType element)
        {
            if (_aimRenderer == null || _displayedElement == element) return;
            _displayedElement = element;
            if (ColorUtility.TryParseHtmlString(UI.Helpers.ElementVisualHelper.GetElementHexColor(element), out var color))
            {
                color.a = 0.65f;
                _aimRenderer.color = color;
            }
        }
        public void UpdateAim(Vector2 attackDirection)
        {
            if (_indicatorTransform == null)
            {
                Initialize();
            }

            if (_indicatorTransform != null && attackDirection != Vector2.zero)
            {
                float pulse = Time.time < _resonanceUntil ? 1.2f + 0.12f * Mathf.Sin(Time.time * 12f) : 1f;
                _indicatorTransform.localScale = indicatorScale * pulse;
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

    /// <summary>Caches pooled attack renderers once; each use receives the damage's effective element.</summary>
    public sealed class ElementAttackVisual : MonoBehaviour
    {
        private SpriteRenderer[] _sprites;
        private ParticleSystem[] _particles;
        private TrailRenderer[] _trails;
        private float[] _spriteAlpha;
        private float[] _particleAlpha;

        private void Awake()
        {
            _sprites = GetComponentsInChildren<SpriteRenderer>(true);
            _particles = GetComponentsInChildren<ParticleSystem>(true);
            _trails = GetComponentsInChildren<TrailRenderer>(true);
            _spriteAlpha = new float[_sprites.Length];
            _particleAlpha = new float[_particles.Length];
            for (int i = 0; i < _sprites.Length; i++) _spriteAlpha[i] = _sprites[i].color.a;
            for (int i = 0; i < _particles.Length; i++) _particleAlpha[i] = _particles[i].main.startColor.color.a;
        }

        public static void Apply(GameObject effect, Shared.ElementType element)
        {
            if (effect == null) return;
            if (!effect.TryGetComponent<ElementAttackVisual>(out var visual))
                visual = effect.AddComponent<ElementAttackVisual>();
            visual.SetElement(element);
        }

        private void SetElement(Shared.ElementType element)
        {
            if (_sprites == null) Awake();
            ColorUtility.TryParseHtmlString(UI.Helpers.ElementVisualHelper.GetElementHexColor(element), out var color);
            for (int i = 0; i < _sprites.Length; i++)
            {
                if (_sprites[i] == null) continue;
                _sprites[i].color = new Color(color.r, color.g, color.b, _spriteAlpha[i]);
            }
            for (int i = 0; i < _particles.Length; i++)
            {
                if (_particles[i] == null) continue;
                var main = _particles[i].main;
                main.startColor = new Color(color.r, color.g, color.b, _particleAlpha[i]);
            }
            foreach (var trail in _trails)
            {
                if (trail == null) continue;
                trail.startColor = new Color(color.r, color.g, color.b, trail.startColor.a);
                trail.endColor = new Color(color.r, color.g, color.b, trail.endColor.a);
            }
        }
    }
}
