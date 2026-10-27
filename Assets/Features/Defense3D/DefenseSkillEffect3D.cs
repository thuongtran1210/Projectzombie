using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    public abstract class DefenseSkillEffect3D : DefenseGameplayBehaviour, IDefenseSkillEffect
    {
        protected DefenseSkillDefinition Definition { get; private set; }
        public abstract DefenseSkillKind Kind { get; }
        public abstract bool IsConfigured { get; }

        public void Configure(DefenseSkillDefinition definition)
        {
            if (definition == null || definition.Kind != Kind || !IsConfigured)
                throw new System.InvalidOperationException("Skill definition/effect binding is invalid.");
            Definition = definition;
            OnConfigured(definition);
        }

        protected virtual void OnConfigured(DefenseSkillDefinition definition) { }

        public abstract bool TryPrepare(DefenseResolvedAim requested, out DefenseResolvedAim prepared);
        public abstract void Execute(DefenseResolvedAim prepared);
    }
}
