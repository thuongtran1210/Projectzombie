using UnityEngine;
namespace ProjectZombie.Features.Defense3D
{
    public abstract class DefenseSkillTuningSO : ScriptableObject
    {
        public abstract DefenseSkillBehavior Behavior { get; }
        public abstract bool IsValid { get; }
        public abstract void Apply(DefenseSkillEffect3D effect);
        protected static bool Positive(float value)=>DefenseDamageRules.IsFinite(value)&&value>0;
        protected static bool NonNegative(float value)=>DefenseDamageRules.IsFinite(value)&&value>=0;
        protected static bool Ratio(float value)=>NonNegative(value)&&value<=1;
    }
}
