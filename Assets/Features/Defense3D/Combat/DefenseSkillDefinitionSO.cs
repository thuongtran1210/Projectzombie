using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    public enum DefenseSkillBehavior { BasicAttack, Dash, ScholarSignature, LightningSignature, Slipper, Pipe }
    [CreateAssetMenu(menuName = "Defense3D/Skill Definition")]
    public sealed class DefenseSkillDefinitionSO : ScriptableObject
    {
        [SerializeField] private string _skillId;
        [SerializeField] private DefenseSkillBehavior _behavior;
        [SerializeField] private DefenseSkillPresentationSO _presentation;
        [SerializeField] private DefenseSkillTuningSO _tuning;
        public DefenseSkillTuningSO Tuning=>_tuning;
        public string SkillId => _skillId;
        public DefenseSkillBehavior Behavior => _behavior;
        public DefenseSkillPresentationSO Presentation => _presentation;
        [SerializeField] private DefenseSkillKind _kind;
        [SerializeField, Min(0.01f)] private float _range;
        [SerializeField, Min(0f)] private float _radius;
        [SerializeField, Min(0f)] private float _damage;
        [SerializeField, Min(0.01f)] private float _cooldown;
        [SerializeField, Min(0.01f)] private float _duration;
        [SerializeField] private bool _autoTargetOnTap;
        [SerializeField] private DefenseTargetMask _targetMask = DefenseTargetMask.All;
        // Migration-only storage. Runtime never reads these obsolete authoring locations.
        [SerializeField, HideInInspector] private DefenseEffectProfileSO _presentationProfile;
        [SerializeField, HideInInspector] private DefenseSignatureEffectSetSO _signaturePresentation;
        [SerializeField, HideInInspector] private Sprite _icon;
        public DefenseSignatureEffectSetSO SignaturePresentation => (_presentation as DefenseSignaturePresentationSO)?.Effects;

        /// <summary>Authored visual assets, independent of damage/cooldown snapshots.</summary>
        public DefenseEffectProfileSO PresentationProfile => (_presentation as DefenseRelicPresentationSO)?.Profile;
        public Sprite Icon => _presentation != null ? _presentation.Icon : null;

        public DefenseSkillDefinition Capture() => new DefenseSkillDefinition(_kind, _range, _radius, _damage,
            _cooldown, _duration, _autoTargetOnTap, _targetMask);
    }
}
