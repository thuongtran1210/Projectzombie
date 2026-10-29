using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Lớp cơ sở trừu tượng cho vũ khí và pháp bảo.
    /// Đã được tinh giản, không phụ thuộc vào gameplay 2D hay nâng cấp tạm thời.
    /// </summary>
    public abstract class WeaponBase : MonoBehaviour
    {
        public enum RelicCastPhase
        {
            Ready,
            Active,
            RecastReady,
            Cooldown
        }

        [Header("Weapon Identity")]
        public string weaponId;
        public string displayName;
        public Sprite icon;
        [TextArea] public string description;
        public ElementType element = ElementType.None;
        public WeaponRole weaponRole = WeaponRole.RelicOnHitTrigger;

        [Header("Hybrid Relic Configuration")]
        public bool isPrimaryActiveWeapon = false;
        public bool isPassiveRelic = false;
        public float activeCooldown = 8.0f;
        public float activeDuration = 0f;
        public string skillActionName;

        public int WeaponLevel { get; set; } = 1;
        public virtual int MaxLevel => 6;

        protected ICharacterStats CharacterStats;
        protected float _lastRelicSkillCastTime = float.NegativeInfinity;
        protected RelicCastPhase _currentRelicPhase = RelicCastPhase.Ready;

        public RelicCastPhase CurrentRelicPhase => _currentRelicPhase;

        public Transform OwnerTransform
        {
            get
            {
                if (CharacterStats is MonoBehaviour mb && mb != null) return mb.transform;
                if (transform.parent != null) return transform.parent;
                return transform;
            }
        }

        public GameObject OwnerGameObject => OwnerTransform != null ? OwnerTransform.gameObject : gameObject;

        public virtual void Initialize(ICharacterStats stats)
        {
            CharacterStats = stats;
        }

        public virtual void Tick() { }
        public virtual void TriggerPrimaryAttack() { }
        public virtual void TriggerActiveRelicSkill(Vector2 direction = default) { }
    }
}
