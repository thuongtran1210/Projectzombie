using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.UI.DamageText
{
    public enum CombatTextKind { Damage, Heal, Block, Immune, Resource, Custom }
    public enum CombatTextTarget { Enemy, Player, Ally, Structure }

    /// <summary>Dữ liệu yêu cầu hiển thị, độc lập với tính toán sát thương và cơ chế pooling.</summary>
    public readonly struct CombatTextRequest
    {
        public readonly CombatTextKind Kind;
        public readonly CombatTextTarget Target;
        public readonly float Amount;
        public readonly Vector3 Position;
        public readonly bool IsCritical, IsCounter;
        public readonly ElementType Element;
        public readonly string Text;
        public readonly Color? ColorOverride;
        public readonly float FontSizeOverride;

        public CombatTextRequest(float amount, Vector3 position, CombatTextTarget target = CombatTextTarget.Enemy,
            bool isCritical = false, bool isCounter = false, ElementType element = ElementType.None,
            CombatTextKind kind = CombatTextKind.Damage, string text = null, Color? color = null, float fontSize = 0f)
        {
            Amount = amount; Position = position; Target = target; IsCritical = isCritical;
            IsCounter = isCounter; Element = element; Kind = kind; Text = text;
            ColorOverride = color; FontSizeOverride = fontSize;
        }

        public int Priority => Target == CombatTextTarget.Player ? 2 : IsCritical || IsCounter ? 1 : 0;
        public bool IsValid => Finite(Position.x) && Finite(Position.y) && Finite(Position.z) &&
            Finite(Amount) && Finite(FontSizeOverride) && FontSizeOverride >= 0f &&
            (Kind == CombatTextKind.Custom ? !string.IsNullOrEmpty(Text) :
             Kind == CombatTextKind.Block || Kind == CombatTextKind.Immune || Amount > 0f);
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        public static CombatTextRequest FromDamage(DamageReport report) => new CombatTextRequest(
            report.Amount, report.Position, report.IsPlayerTarget ? CombatTextTarget.Player : CombatTextTarget.Enemy,
            report.IsCritical, report.IsCounter, report.Element);
    }

    /// <summary>Đầu vào dùng chung cho chữ combat của hệ 2D và Defense3D.</summary>
    public interface ICombatTextService
    {
        bool Show(in CombatTextRequest request);
    }
}
