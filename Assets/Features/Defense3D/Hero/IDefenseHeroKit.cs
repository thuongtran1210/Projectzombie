namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Interface chung cho bộ kỹ năng của Anh Hùng (cả authored ScriptableObject lẫn runtime container).
    /// Giúp tách biệt dữ liệu thiết kế cố định khỏi logic trang bị động (Relic, buff) trong trận.
    /// </summary>
    public interface IDefenseHeroKit
    {
        string HeroId { get; }
        string DisplayName { get; }
        DefenseSkillDefinitionSO Attack { get; }
        DefenseSkillDefinitionSO Dash { get; }
        DefenseSkillDefinitionSO Signature { get; }
        DefenseSkillDefinitionSO Relic { get; }

        DefenseSkillDefinitionSO DefinitionFor(DefenseSkillKind kind);
        void Validate();
    }
}
