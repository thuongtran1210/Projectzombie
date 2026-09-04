using UnityEngine;
using ProjectZombie.Features.VFX.Indicators;

namespace ProjectZombie.Features.Enemies
{
    /// <summary>
    /// Hiển thị vệt đỏ báo trước đòn đánh (Telegraph Warning) cho Quái Tinh Anh và Boss theo GDD v5.0.
    /// Giúp người chơi có cửa sổ phản xạ (0.3s - 0.5s) để bấm nút Dash né đòn.
    /// </summary>
    public class EnemyAttackTelegraph : MonoBehaviour
    {
        [Header("Telegraph Settings")]
        [SerializeField] private float telegraphDuration = 0.4f;
        [SerializeField] private float attackRadius = 2.0f;
        [SerializeField] private IndicatorShape shape = IndicatorShape.Circle;
        [SerializeField] private Color warningColor = new Color(1f, 0.1f, 0.1f, 0.45f);

        private Enemy _enemy;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
        }

        /// <summary>
        /// Phát vệt đỏ báo hiệu và gọi callback khi kết thúc thời gian cảnh báo.
        /// </summary>
        public void ShowTelegraph(Vector3 targetPos, System.Action onTelegraphCompleted)
        {
            if (SkillIndicatorManager.Instance != null && telegraphDuration > 0f)
            {
                float radius = attackRadius > 0f 
                    ? attackRadius 
                    : ((_enemy != null && _enemy.Config != null) ? _enemy.Config.attackRange : 2.0f);

                Vector3 dir = (targetPos - transform.position).normalized;
                Vector2 size = new Vector2(radius * 2f, radius * 2f);

                SkillIndicatorManager.Instance.ShowIndicator(new IndicatorRequest(
                    shape,
                    transform.position,
                    dir,
                    size,
                    telegraphDuration,
                    warningColor
                ), onTelegraphCompleted);
            }
            else
            {
                // Fallback nếu không có Indicator Manager
                onTelegraphCompleted?.Invoke();
            }
        }
    }
}
