namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Giao diện trừu tượng cho hiệu ứng Tuyệt Kỹ (Signature Skill) của bất kỳ Tướng 3D nào.
    /// Giúp tách rời (decouple) Animator Visual Controller khỏi script chiêu thức cụ thể.
    /// </summary>
    public interface IDefenseSignatureEffect
    {
        bool IsCasting { get; }
    }
}
