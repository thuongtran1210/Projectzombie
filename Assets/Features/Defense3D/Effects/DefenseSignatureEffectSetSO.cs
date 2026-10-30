using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Bộ style signature: vòng niệm duy trì, xung lực tức thời và spark cho hit đã xác nhận.</summary>
    [CreateAssetMenu(menuName = "Defense3D/Effects/Signature Set")]
    public sealed class DefenseSignatureEffectSetSO : ScriptableObject
    {
        [SerializeField] private DefenseEffectProfileSO _cast;
        [SerializeField] private DefenseEffectProfileSO _release;
        [SerializeField] private DefenseEffectProfileSO _hit;
        [SerializeField, Min(.01f)] private float _hitRadius = .4f;
        [SerializeField, Min(0f)] private float _lift = .04f;
        [SerializeField, Range(1, 64)] private int _maxHitEffects = 16;
        [SerializeField, Range(0f, .3f)] private float _cameraImpulseAmplitude = .075f;
        [SerializeField, Range(.01f, .5f)] private float _cameraImpulseDuration = .16f;
        public float CameraImpulseAmplitude => _cameraImpulseAmplitude;
        public float CameraImpulseDuration => _cameraImpulseDuration;
        public DefenseEffectProfileSO Cast => _cast;
        public DefenseEffectProfileSO Release => _release;
        public DefenseEffectProfileSO Hit => _hit;
        public float HitRadius => _hitRadius;
        public float Lift => _lift;
        public int MaxHitEffects => _maxHitEffects;
        public bool IsValid => _cast != null && _cast.IsValid && _cast.Sustained && _release != null && _release.IsValid && !_release.Sustained &&
            _hit != null && _hit.IsValid && !_hit.Sustained && DefenseDamageRules.IsFinite(_hitRadius) && _hitRadius > 0f &&
            DefenseDamageRules.IsFinite(_lift) && _lift >= 0f && _maxHitEffects >= 1 && _maxHitEffects <= 64 &&
            DefenseDamageRules.IsFinite(_cameraImpulseAmplitude) && _cameraImpulseAmplitude >= 0f && _cameraImpulseAmplitude <= .3f &&
            DefenseDamageRules.IsFinite(_cameraImpulseDuration) && _cameraImpulseDuration >= .01f && _cameraImpulseDuration <= .5f;
    }
}
