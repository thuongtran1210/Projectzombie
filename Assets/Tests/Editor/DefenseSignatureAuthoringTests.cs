using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseSignatureAuthoringTests
{
    private const string Folder = "Assets/Features/Defense3D/Effects/Assets/";
    [Test]
    public void StyleUsesSustainedCastAndTimedReleaseHit_WithBoundedSparkCount()
    {
        var style = AssetDatabase.LoadAssetAtPath<DefenseSignatureEffectSetSO>(Folder + "FXSet_ScholarSignature.asset");
        Assert.IsNotNull(style); Assert.IsTrue(style.IsValid);
        Assert.IsTrue(style.Cast.Sustained); Assert.IsFalse(style.Release.Sustained); Assert.IsFalse(style.Hit.Sustained);
        Assert.IsNotNull(style.Cast.Prefab.GetComponent<DefenseSealEffectVisual3D>());
        Assert.IsNotNull(style.Release.Prefab.GetComponent<DefenseRingEffectVisual3D>());
        Assert.IsNotNull(style.Hit.Prefab.GetComponent<DefenseHitSparkVisual3D>());
        Assert.That(style.MaxHitEffects, Is.InRange(1, 32));
        Assert.AreEqual(Vector3.one, style.Cast.Prefab.transform.localScale);
        Assert.IsEmpty(style.Cast.Prefab.GetComponentsInChildren<DefenseGameplayBehaviour>(true));
        Assert.IsEmpty(style.Cast.Prefab.GetComponentsInChildren<Collider>(true));
        foreach (var renderer in style.Cast.Prefab.GetComponentsInChildren<Renderer>(true)) Assert.IsNotNull(renderer.sharedMaterial);
    }
    [Test]
    public void SceneWiresSignatureAndRemovesOldView_WhileGameplayDefinitionIsPreserved()
    {
        // Kiểm tra văn bản đã lưu, không mở hoặc chạy scene.
        string scene = File.ReadAllText("Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity");
        StringAssert.Contains("_signaturePresenter: {fileID: 8002500002}", scene);
        StringAssert.Contains("_groundAnchor: {fileID: 8002400011}", scene);
        StringAssert.Contains(AssetDatabase.AssetPathToGUID(Folder + "FXSet_ScholarSignature.asset"), scene);
        StringAssert.DoesNotContain("8001800122", scene);
        string skill = File.ReadAllText("Assets/Features/Defense3D/Combat/DefenseScholarSignatureEffect3D.cs");
        StringAssert.DoesNotContain("_impactView", skill); StringAssert.DoesNotContain("Physics.Raycast", skill);
        var definition = AssetDatabase.LoadAssetAtPath<DefenseSkillDefinitionSO>("Assets/Resources/Defense3D/Skill_ScholarSignature.asset").Capture();
        Assert.AreEqual(6f, definition.Radius); Assert.AreEqual(52.5f, definition.Damage);
        Assert.AreEqual(30f, definition.Cooldown); Assert.AreEqual(.38f, definition.Duration, .001f);
        Assert.IsNull(typeof(DefenseScholarSignaturePresenter3D).GetMethod("Update", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
    }
    [Test]
    public void SealModuleRestoresRendererStateBeforeReuse()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "VFX_ScholarCast_VongXuyen.prefab");
        var copy = UnityEngine.Object.Instantiate(prefab);
        try
        {
            var instance = copy.GetComponent<DefenseEffectInstance3D>(); var renderer = copy.GetComponentInChildren<MeshRenderer>();
            instance.Prepare(Color.green, 12f); instance.Render(0f);
            Assert.IsTrue(renderer.enabled); Assert.AreEqual(Vector3.one * 12f, renderer.transform.localScale);
            instance.ResetPresentation(); Assert.IsFalse(renderer.enabled); Assert.IsFalse(copy.activeSelf);
            var block = new MaterialPropertyBlock(); renderer.GetPropertyBlock(block); Assert.IsTrue(block.isEmpty);
            instance.Prepare(Color.yellow, 6f); Assert.AreEqual(Vector3.one * 6f, renderer.transform.localScale);
        }
        finally { UnityEngine.Object.DestroyImmediate(copy); }
    }
}
