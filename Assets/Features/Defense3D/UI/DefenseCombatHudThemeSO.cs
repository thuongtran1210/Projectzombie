using System;
using UnityEngine;
namespace ProjectZombie.Features.Defense3D.UI
{
    [CreateAssetMenu(menuName="Defense3D/UI/Combat HUD Theme")]
    public sealed class DefenseCombatHudThemeSO : ScriptableObject
    {
        [Serializable] public sealed class SlotStyle
        {
            public DefenseSkillKind Slot;
            public Sprite Plate;
            public float Diameter;
            public float HitSize;
            public Vector2 Position;
        }
        [SerializeField] private SlotStyle[] _slots={
            new SlotStyle{Slot=DefenseSkillKind.BasicAttack,Diameter=144,HitSize=144,Position=new Vector2(-104,110)},
            new SlotStyle{Slot=DefenseSkillKind.Dash,Diameter=92,HitSize=100,Position=new Vector2(-264,106)},
            new SlotStyle{Slot=DefenseSkillKind.Relic,Diameter=100,HitSize=104,Position=new Vector2(-110,280)},
            new SlotStyle{Slot=DefenseSkillKind.Signature,Diameter=108,HitSize=112,Position=new Vector2(-267,255)}};
        [SerializeField] private int _labelFontSize=16;
        [SerializeField] private int _statusFontSize=11;
        public SlotStyle ForSlot(DefenseSkillKind slot)
        {foreach(var style in _slots)if(style.Slot==slot)return style;throw new InvalidOperationException("Missing HUD slot style.");}
        public int LabelFontSize=>_labelFontSize;
        public int StatusFontSize=>_statusFontSize;
    }
}
