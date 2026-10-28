using UnityEngine;
using ProjectZombie.Features.Defense3D;

/// <summary>Nguồn ability giả dùng riêng cho Unit Test; không cần tạo UI pháp bảo.</summary>
public sealed class DefenseTestAbilitySource : MonoBehaviour, IDefenseAbilitySource
{
    public DefenseAbilityRuntime Ability { get; private set; }
    public void Bind(IDefenseGameplayContext gameplay)
    { Ability?.Dispose(); Ability = new DefenseAbilityRuntime(gameplay); }
    private void OnDestroy() { Ability?.Dispose(); }
}
