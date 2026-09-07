using UnityEngine;

namespace ProjectZombie.Features.Enemies
{
    public abstract class CombatMovementStrategy : MonoBehaviour
    {
        [Header("Obstacle Avoidance & Steering")]
        [Tooltip("Bật tính năng né vật cản và trượt dọc góc tường thông minh")]
        [SerializeField] protected bool enableObstacleAvoidance = true;

        [Tooltip("Layer vật cản/tường cần né (mặc định: Obstacle, Default)")]
        [SerializeField] protected LayerMask obstacleLayerMask;

        [Tooltip("Khoảng cách quét tia chính diện phía trước")]
        [SerializeField] protected float lookAheadDistance = 0.9f;

        [Tooltip("Góc quét của 2 tia râu phụ trái/phải (độ)")]
        [SerializeField] protected float whiskerAngle = 35f;

        [Tooltip("Trọng số lực bẻ lái khi né vật cản")]
        [SerializeField] protected float avoidanceWeight = 1.2f;

        [Tooltip("Bán kính cơ thể quái để quét vùng va chạm (CircleCast)")]
        [SerializeField] protected float bodyRadius = 0.35f;

        protected Enemy _enemy;
        private static readonly RaycastHit2D[] _raycastBuffer = new RaycastHit2D[6];
        private static readonly Collider2D[] _overlapBuffer = new Collider2D[4];
        private static PhysicsMaterial2D _frictionlessMaterial;

        protected virtual void Awake()
        {
            _enemy = GetComponent<Enemy>();

            if (obstacleLayerMask.value == 0)
            {
                obstacleLayerMask = LayerMask.GetMask("Obstacle", "Default");
                if (obstacleLayerMask.value == 0)
                {
                    obstacleLayerMask = (1 << 3) | (1 << 0); // Fallback: Layer 3 (Obstacle) & Layer 0 (Default)
                }
            }

            // Gán PhysicsMaterial2D trơn trượt (Friction = 0) để triệt tiêu hoàn toàn hiện tượng dính tường
            ApplyFrictionlessMaterial();
        }

        private void ApplyFrictionlessMaterial()
        {
            if (_frictionlessMaterial == null)
            {
                _frictionlessMaterial = new PhysicsMaterial2D("Enemy_Frictionless")
                {
                    friction = 0f,
                    bounciness = 0f
                };
            }

            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.sharedMaterial = _frictionlessMaterial;
            }
        }

        /// <summary>
        /// Thực thi logic di chuyển trong trạng thái giao tranh/đuổi theo.
        /// </summary>
        public abstract void Move();

        /// <summary>
        /// Kiểm tra khoảng cách hiện tại có nằm trong cự ly tấn công hiệu quả hay không.
        /// </summary>
        public abstract bool IsInAttackRange(float distanceToPlayer);

        /// <summary>
        /// Kiểm tra quái có cần điều chỉnh lại vị trí (Reposition/Lùi lại) hay không (mặc định false cho Melee).
        /// </summary>
        public virtual bool ShouldReposition(float distanceToPlayer) => false;

        /// <summary>
        /// Tính toán vector chỉ hướng di chuyển tối ưu sau khi né vật cản và trượt tường (0-GC Alloc).
        /// Triệt tiêu 100% lực đâm vào tường và đẩy nhẹ ra xa để không bao giờ bị kẹt hoặc dính ma sát tường.
        /// </summary>
        protected Vector2 CalculateSteeringDirection(Vector2 desiredDirection)
        {
            if (!enableObstacleAvoidance || desiredDirection.sqrMagnitude < 0.001f)
            {
                return desiredDirection;
            }

            Vector2 origin = (Vector2)transform.position + new Vector2(0f, 0.2f);
            desiredDirection.Normalize();

            // 0. Kiểm tra nếu quái đang cọ xát/tiếp xúc sát mép tường (Overlap Check)
            int overlapCount = Physics2D.OverlapCircleNonAlloc(origin, bodyRadius + 0.12f, _overlapBuffer, obstacleLayerMask);
            for (int i = 0; i < overlapCount; i++)
            {
                var col = _overlapBuffer[i];
                if (col != null && col.gameObject != gameObject && !col.transform.IsChildOf(transform))
                {
                    Vector2 closestPt = col.ClosestPoint(origin);
                    Vector2 pushAway = (origin - closestPt);
                    if (pushAway.sqrMagnitude > 0.0001f)
                    {
                        Vector2 normal = pushAway.normalized;
                        float dot = Vector2.Dot(desiredDirection, normal);
                        if (dot < 0f)
                        {
                            // Đang muốn đâm vào tường -> Triệt tiêu hoàn toàn thành phần đâm vào tường
                            Vector2 tangent = desiredDirection - dot * normal;
                            if (tangent.sqrMagnitude < 0.01f)
                            {
                                tangent = new Vector2(-normal.y, normal.x);
                            }
                            return (tangent.normalized + normal * 0.35f).normalized;
                        }
                    }
                }
            }

            // 1. Quét khối hình thể phía trước bằng CircleCast (Phát hiện cả khi vai quái va vào góc tường)
            bool centerBlocked = CircleCastIgnoreSelf(origin, bodyRadius, desiredDirection, lookAheadDistance, out RaycastHit2D centerHit);

            // 2. Quét 2 tia râu phụ trái/phải
            float whiskerDist = lookAheadDistance * 0.75f;
            Vector2 leftWhiskerDir = RotateVector(desiredDirection, whiskerAngle);
            Vector2 rightWhiskerDir = RotateVector(desiredDirection, -whiskerAngle);

            bool leftBlocked = RaycastIgnoreSelf(origin, leftWhiskerDir, whiskerDist, out RaycastHit2D leftHit);
            bool rightBlocked = RaycastIgnoreSelf(origin, rightWhiskerDir, whiskerDist, out RaycastHit2D rightHit);

            // 3. Xử lý khi bị chắn phía trước
            if (centerBlocked)
            {
                Vector2 hitNormal = centerHit.normal;
                float dot = Vector2.Dot(desiredDirection, hitNormal);

                // Loại bỏ 100% vector đâm vuông góc vào bề mặt tường
                Vector2 tangent = desiredDirection - (dot < 0f ? dot : 0f) * hitNormal;
                if (tangent.sqrMagnitude < 0.01f)
                {
                    Vector2 tangentA = new Vector2(-hitNormal.y, hitNormal.x);
                    Vector2 tangentB = new Vector2(hitNormal.y, -hitNormal.x);
                    tangent = !rightBlocked ? tangentB : tangentA;
                }
                else
                {
                    tangent.Normalize();
                }

                // Ưu tiên rẽ về phía râu quét thoáng hơn
                if (leftBlocked && !rightBlocked)
                {
                    tangent = (tangent + rightWhiskerDir * 0.5f).normalized;
                }
                else if (rightBlocked && !leftBlocked)
                {
                    tangent = (tangent + leftWhiskerDir * 0.5f).normalized;
                }

                // Kết hợp tiếp tuyến trượt tường + Lực đẩy nhẹ ra xa mặt tường để loại bỏ ma sát cọ xát
                Vector2 steered = (tangent + hitNormal * 0.3f).normalized;
                return steered;
            }
            else if (leftBlocked && !rightBlocked)
            {
                // Râu trái chạm mép vật cản -> Bẻ lái sang phải
                return (desiredDirection + rightWhiskerDir * (avoidanceWeight * 0.5f)).normalized;
            }
            else if (rightBlocked && !leftBlocked)
            {
                // Râu phải chạm mép vật cản -> Bẻ lái sang trái
                return (desiredDirection + leftWhiskerDir * (avoidanceWeight * 0.5f)).normalized;
            }

            return desiredDirection;
        }

        private bool CircleCastIgnoreSelf(Vector2 origin, float radius, Vector2 direction, float distance, out RaycastHit2D validHit)
        {
            validHit = default;
            int count = Physics2D.CircleCastNonAlloc(origin, radius, direction, _raycastBuffer, distance, obstacleLayerMask);
            for (int i = 0; i < count; i++)
            {
                var hit = _raycastBuffer[i];
                if (hit.collider != null && hit.collider.gameObject != gameObject && !hit.collider.transform.IsChildOf(transform))
                {
                    validHit = hit;
                    return true;
                }
            }
            return false;
        }

        private bool RaycastIgnoreSelf(Vector2 origin, Vector2 direction, float distance, out RaycastHit2D validHit)
        {
            validHit = default;
            int count = Physics2D.RaycastNonAlloc(origin, direction, _raycastBuffer, distance, obstacleLayerMask);
            for (int i = 0; i < count; i++)
            {
                var hit = _raycastBuffer[i];
                if (hit.collider != null && hit.collider.gameObject != gameObject && !hit.collider.transform.IsChildOf(transform))
                {
                    validHit = hit;
                    return true;
                }
            }
            return false;
        }

        private static Vector2 RotateVector(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (!enableObstacleAvoidance) return;

            Vector2 origin = (Vector2)transform.position + new Vector2(0f, 0.2f);
            Vector2 forward = Vector2.right;

            if (_enemy != null && _enemy.PlayerTransform != null)
            {
                forward = ((Vector2)_enemy.PlayerTransform.position - origin).normalized;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(origin + forward * lookAheadDistance, bodyRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(origin, RotateVector(forward, whiskerAngle) * (lookAheadDistance * 0.75f));
            Gizmos.DrawRay(origin, RotateVector(forward, -whiskerAngle) * (lookAheadDistance * 0.75f));
        }
    }
}
