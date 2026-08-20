using UnityEngine;
using ProjectZombie.Features.Projectiles.Components;
using ProjectZombie.Features.Projectiles.Data;
using ProjectZombie.Features.Projectiles.Core;

namespace ProjectZombie.Features.Projectiles.Behaviors
{
    /// <summary>
    /// Hành vi bay Boomerang Lưỡi Liềm Cắt Chéo (Dual Curved Crescent):
    /// - Bay theo đường cong lưỡi liềm xòe sang bên, cắt chéo qua trục tâm rồi bẻ lái đuổi theo người chơi.
    /// - Tự xoay tròn quanh trục Z liên tục (1080 deg/s) tạo cảm giác phi tiêu sắc bén.
    /// - Xuyên thấu toàn bộ quái vật trên đường đi (KeepAlive).
    /// </summary>
    public class CurvedBoomerangBehavior : IProjectileBehavior
    {
        private readonly ProjectileController _controller;
        private readonly CurvedBoomerangBehaviorData _data;

        private float _spawnTime;
        private float _curveSign = 1f; // +1: Uốn sang phải (Clockwise), -1: Uốn sang trái (Counter-Clockwise), 0: Thẳng
        private bool _isCurveSignExplicitlySet = false;
        private bool _isReturning = false;
        private Transform _visualRoot;
        private static int _globalSpawnCounter = 0;

        public CurvedBoomerangBehavior(ProjectileController controller, CurvedBoomerangBehaviorData data)
        {
            _controller = controller;
            _data = data;
        }

        public void SetCurveSign(float sign)
        {
            _curveSign = sign;
            _isCurveSignExplicitlySet = true;
        }

        public void OnSpawn()
        {
            _spawnTime = Time.time;
            _isReturning = false;

            if (_controller != null)
            {
                _visualRoot = _controller.transform.Find("Visual_Root");
            }

            if (!_isCurveSignExplicitlySet)
            {
                _globalSpawnCounter++;
                _curveSign = (_globalSpawnCounter % 2 == 0) ? 1f : -1f;
            }
        }

        public void OnUpdate()
        {
            if (_controller == null) return;

            // 1. Tự xoay tròn Visual Sprite của phi tiêu liên tục quanh trục Z
            float spinSpeed = _data != null ? _data.spinSpeed : 1080f;
            if (_visualRoot != null)
            {
                _visualRoot.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
            }
            else
            {
                _controller.transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
            }

            float forwardDuration = _data != null ? _data.forwardDuration : 0.38f;
            float elapsed = Time.time - _spawnTime;

            if (elapsed < forwardDuration)
            {
                // Pha 1: Uốn cong hình lưỡi liềm hướng vào tâm và cắt chéo nhau
                if (_curveSign != 0f)
                {
                    float turnRate = _data != null ? _data.curveTurnRate : 260f;
                    float angleDelta = -_curveSign * turnRate * Time.deltaTime;
                    float curAngle = Mathf.Atan2(_controller.CurrentDirection.y, _controller.CurrentDirection.x) * Mathf.Rad2Deg;
                    float newAngle = curAngle + angleDelta;
                    _controller.CurrentDirection = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad));
                }
            }
            else
            {
                // Pha 2: Bắt đầu bẻ lái quay đầu gấp về phía người chơi (Snappy U-Turn & Homing Recall)
                _isReturning = true;

                Vector2 targetPos = _controller.Owner != null 
                    ? (Vector2)_controller.Owner.transform.position 
                    : (Vector2)_controller.State.SpawnPosition;

                Vector2 toTarget = targetPos - (Vector2)_controller.transform.position;
                float distToTarget = toTarget.magnitude;

                // Khi phi tiêu đã quay về gần chạm vào nhân vật
                if (distToTarget < 0.85f && elapsed > forwardDuration + 0.15f)
                {
                    _controller.Despawn();
                    return;
                }

                if (distToTarget > 0.001f)
                {
                    float currentAngle = Mathf.Atan2(_controller.CurrentDirection.y, _controller.CurrentDirection.x) * Mathf.Rad2Deg;
                    float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
                    float returnTurnRate = _data != null ? _data.returnTurnRate : 720f;
                    float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, returnTurnRate * Time.deltaTime);

                    _controller.CurrentDirection = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad));
                }
            }
        }

        public BehaviorHitResult OnHit(ProjectileEventContext context)
        {
            // Xuyên thấu toàn bộ quái vật trên đường bay hình lưỡi liềm
            return BehaviorHitResult.KeepAlive;
        }

        public void OnDespawn()
        {
            _isReturning = false;
            _isCurveSignExplicitlySet = false;
        }
    }
}
