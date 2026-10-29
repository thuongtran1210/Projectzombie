using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    [CreateAssetMenu(menuName = "Defense3D/Combat VFX Profile")]
    public sealed class DefenseCombatVfxProfileSO : ScriptableObject
    {
        [Min(.05f)] public float flightDuration = .22f;
        [Min(0f)] public float flightArc = .45f;
        [Min(.05f)] public float impactDuration = .18f;
        public Color arrowColor = new Color(1f, .8f, .38f, 1f);
        public Color impactColor = new Color(1f, .94f, .65f, 1f);
    }
}
