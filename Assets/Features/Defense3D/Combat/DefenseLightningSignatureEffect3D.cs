using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Kỹ năng Tuyệt Kỹ Tướng Đạo Sĩ: Thiên Lôi Giáng Thế (Heavenly Lightning Strike).
    /// Triệu hồi sấm sét thần thánh giáng thẳng xuống khu vực chỉ định:
    /// - Gây sát thương sấm sét diện rộng cực lớn (scale theo lực chiến EffectiveDamage của Tướng).
    /// - Làm choáng (Stun) toàn bộ đàn yêu ma trong phạm vi bán kính (mặc định 1.5s).
    /// - Làm tê liệt giảm tốc (Slow) nhẹ sau khi chịu đòn sấm sét.
    /// - Phát đầy đủ các sự kiện Feedback (Started, Released, Hit, Ended) để đồng bộ Camera Shake và Combat Text.
    /// - Đảm bảo an toàn vòng đời và 0 GC Allocations trong trận chiến.
    /// </summary>
    public sealed class DefenseLightningSignatureEffect3D : DefenseSkillEffect3D, IDefenseSignatureEffect
    {
        private const int TARGET_BUFFER_CAPACITY = 256;

        [Header("Liên Kết Thực Thể Tướng")]
        [SerializeField] private DefenseHeroHealth3D _health;
        [SerializeField] private DefenseHeroAutoAttack3D _attack;
        [SerializeField] private DefenseHeroMotor3D _motor;

        [Header("Thông Số Khống Chế & Hiệu Ứng Sét")]
        [SerializeField, Tooltip("Thời gian làm choáng quái vật khi trúng sét (giây)")]
        private float _stunDuration = 1.5f;

        [SerializeField, Tooltip("Tỷ lệ làm tê liệt giảm tốc sau khi trúng sét (0.4 = chậm 40%)")]
        private float _paralysisSlowStrength = 0.4f;

        [SerializeField, Tooltip("Thời gian tê liệt giảm tốc (giây)")]
        private float _paralysisSlowDuration = 1.0f;

        [Header("Hiệu Ứng Hình Ảnh (VFX)")]
        [SerializeField] private GameObject _lightningVfxPrefab;

        private readonly IDefenseTargetPosition[] _targets = new IDefenseTargetPosition[TARGET_BUFFER_CAPACITY];
        private DefenseSignatureFeedbackEvents _feedback;
        private long _nextCastId, _activeCastId;
        private object _castIdentity;
        private int _castGeneration;
        private DefensePoint3 _castTargetPosition;
        private float _resolveTimer;
        private bool _isCasting;
        private bool _resolving;

        public DefenseSignatureFeedbackEvents Feedback =>
            _feedback ?? (_feedback = new DefenseSignatureFeedbackEvents(error => Debug.LogException(error, this)));

        public bool IsCasting => _isCasting;

        public override DefenseSkillKind Kind => DefenseSkillKind.Signature;

        public override bool IsConfigured =>
            _health != null && _attack != null && _motor != null;

        public float StunDuration
        {
            get => _stunDuration;
            set => _stunDuration = Mathf.Max(0f, value);
        }

        public float ParalysisSlowStrength
        {
            get => _paralysisSlowStrength;
            set => _paralysisSlowStrength = Mathf.Clamp01(value);
        }

        public float ParalysisSlowDuration
        {
            get => _paralysisSlowDuration;
            set => _paralysisSlowDuration = Mathf.Max(0f, value);
        }

        public GameObject LightningVfxPrefab
        {
            get => _lightningVfxPrefab;
            set => _lightningVfxPrefab = value;
        }

        public void ConfigureComponents(DefenseHeroHealth3D health, DefenseHeroAutoAttack3D attack, DefenseHeroMotor3D motor)
        {
            _health = health;
            _attack = attack;
            _motor = motor;
        }

        private void Awake()
        {
            if (_health == null) _health = GetComponent<DefenseHeroHealth3D>();
            if (_attack == null) _attack = GetComponent<DefenseHeroAutoAttack3D>();
            if (_motor == null) _motor = GetComponent<DefenseHeroMotor3D>();
        }

        protected override void OnGameplayBound() => CancelCasting();

        protected override void OnGameplayCleanup() => CancelCasting();

        protected override void OnGameplayReset() => CancelCasting();

        private void OnDisable() => CancelCasting();

        public override bool TryPrepare(DefenseResolvedAim requested, out DefenseResolvedAim prepared)
        {
            prepared = requested;

            return isActiveAndEnabled && CanAdvanceGameplay && IsConfigured && Definition != null &&
                   !_health.IsDown && !_motor.IsDashing && !_isCasting && !_resolving &&
                   Gameplay.Targets is IDefenseAreaTargetQuery;
        }

        public override void Execute(DefenseResolvedAim prepared)
        {
            if (!CanAdvanceGameplay || !isActiveAndEnabled || !IsConfigured || _isCasting || _resolving ||
                _health.IsDown || _motor.IsDashing) return;

            _activeCastId = checked(++_nextCastId);
            _castIdentity = Gameplay.SessionIdentity;
            _castGeneration = Gameplay.RunGeneration;
            _castTargetPosition = prepared.Target;

            _isCasting = true;
            _resolveTimer = Definition != null ? Mathf.Max(0.05f, Definition.Duration) : 0.25f;

            if (_attack != null)
            {
                _attack.CancelPendingAttack();
            }

            Feedback.PublishStarted(Snapshot());
        }

        private DefenseSignatureFeedback Snapshot(bool completed = false) =>
            new DefenseSignatureFeedback(
                _activeCastId,
                _castTargetPosition,
                Definition != null ? Definition.Radius : 4.5f,
                _castIdentity,
                _castGeneration,
                completed);

        private bool Current(long castId, object identity, int generation) =>
            _activeCastId == castId &&
            isActiveAndEnabled &&
            CanAdvanceGameplay &&
            ReferenceEquals(Gameplay.SessionIdentity, identity) &&
            Gameplay.RunGeneration == generation &&
            _health != null &&
            _health.isActiveAndEnabled &&
            !_health.IsDown &&
            _motor != null &&
            !_motor.IsDashing;

        private void Update()
        {
            if (!_isCasting) return;

            if (!CanAdvanceGameplay || _health == null || _health.IsDown || _motor == null || _motor.IsDashing)
            {
                CancelCasting();
                return;
            }

            _resolveTimer -= Time.deltaTime;
            if (_resolveTimer <= 0f)
            {
                Resolve();
            }
        }

        private void Resolve()
        {
            if (!_isCasting || _resolving) return;

            long castId = _activeCastId;
            object identity = _castIdentity;
            int generation = _castGeneration;

            if (!Current(castId, identity, generation))
            {
                CancelCasting();
                return;
            }

            _isCasting = false;
            _resolving = true;

            float radius = Definition != null ? Definition.Radius : 4.5f;
            float baseDefDamage = Definition != null ? Definition.Damage : 95f;
            DefenseTargetMask targetMask = Definition != null ? Definition.TargetMask : DefenseTargetMask.All;

            // Tính toán sát thương theo scaling sức mạnh tấn công của Tướng
            float damage = _attack != null && _attack.BaseDamage > 0f
                ? _attack.EffectiveDamage * (baseDefDamage / _attack.BaseDamage)
                : baseDefDamage;

            int count = 0;
            try
            {
                // 1. Sinh hiệu ứng hình ảnh sấm sét giáng xuống đất
                SpawnLightningVfx();

                // 2. Phát sự kiện xả chiêu (Released)
                Feedback.PublishReleased(Snapshot());

                if (Gameplay.Targets is IDefenseAreaTargetQuery area &&
                    area.TryCopyWithin(_castTargetPosition, radius, targetMask, _targets, out count))
                {
                    var damageContext = new DefenseDamageContext(
                        damage,
                        _health,
                        _castTargetPosition,
                        isCritical: false,
                        runGeneration: generation,
                        sessionIdentity: identity);

                    Vector3 strikePos = new Vector3(_castTargetPosition.X, _castTargetPosition.Y, _castTargetPosition.Z);

                    for (int i = 0; i < count; i++)
                    {
                        if (!Current(castId, identity, generation)) break;

                        var target = _targets[i];
                        if (target == null || !target.IsAvailable || !(target.Identity is IDefenseEnemyTarget3D enemy)) continue;

                        Vector3 enemyPos = enemy.TargetTransform.position;
                        var hitPosition = DefenseAttackFeedbackPoints.Capture(enemy.TargetTransform, enemyPos + Vector3.up * 0.7f);
                        Vector3 direction = enemyPos - strikePos;
                        direction.y = 0f;
                        if (direction.sqrMagnitude < 0.000001f) direction = Vector3.forward;
                        direction.Normalize();

                        bool accepted = enemy.TakeDamage(damageContext);
                        if (accepted)
                        {
                            // Bắn sự kiện trúng đòn cho Combat Text & Screen Shake
                            Feedback.PublishHit(new DefenseSignatureHitFeedback(
                                castId,
                                new DefenseHitFeedback(hitPosition, DefenseAttackFeedbackPoints.Point(direction), damage, identity, generation)));

                            // Áp dụng hiệu ứng khống chế sấm sét
                            if (enemy is IDefenseStatusReceiver receiver)
                            {
                                // Choáng sấm sét (Stun)
                                if (_stunDuration > 0f)
                                {
                                    receiver.TryApplyStatus(new DefenseStatusRequest(
                                        DefenseStatusKind.Stun,
                                        _stunDuration,
                                        1f,
                                        default,
                                        identity,
                                        generation));
                                }

                                // Tê liệt giảm tốc (Slow)
                                if (_paralysisSlowStrength > 0f && _paralysisSlowDuration > 0f)
                                {
                                    receiver.TryApplyStatus(new DefenseStatusRequest(
                                        DefenseStatusKind.Slow,
                                        _paralysisSlowDuration,
                                        _paralysisSlowStrength,
                                        default,
                                        identity,
                                        generation));
                                }
                            }
                        }
                    }
                }

                if (Current(castId, identity, generation))
                {
                    CloseCast(true);
                }
                else if (_activeCastId == castId)
                {
                    CancelCasting();
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
                if (_activeCastId == castId) CancelCasting();
                throw;
            }
            finally
            {
                Array.Clear(_targets, 0, count);
                _resolving = false;
            }
        }

        private void SpawnLightningVfx()
        {
            GameObject prefab = _lightningVfxPrefab;
            if (prefab == null)
            {
                prefab = Resources.Load<GameObject>("VFX/VFX_Augment_ChainLightning")
                      ?? Resources.Load<GameObject>("VFX/PS_ImpactSparks");

#if UNITY_EDITOR
                if (prefab == null)
                {
                    prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/VFX/VFX_Augment_ChainLightning.prefab")
                          ?? UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Archive_Legacy_2D/VFX/Augments/VFX_Augment_ChainLightning.prefab")
                          ?? UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/VFX/PS_ImpactSparks.prefab");
                }
#endif
            }

            if (prefab != null)
            {
                Vector3 strikePos = new Vector3(_castTargetPosition.X, _castTargetPosition.Y + 0.1f, _castTargetPosition.Z);
                var vfx = Instantiate(prefab, strikePos, Quaternion.identity);
                Destroy(vfx, 2.0f);
            }
        }

        private void CloseCast(bool completed)
        {
            if (_activeCastId == 0) return;
            var feedback = Snapshot(completed);
            _activeCastId = 0;
            _castIdentity = null;
            _castGeneration = -1;
            Feedback.PublishEnded(feedback);
        }

        public void CancelCasting()
        {
            if (_isCasting)
            {
                _isCasting = false;
                CloseCast(false);
            }
            else
            {
                _activeCastId = 0;
                _castIdentity = null;
                _castGeneration = -1;
            }
        }
    }
}
