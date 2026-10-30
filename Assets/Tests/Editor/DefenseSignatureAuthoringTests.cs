using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseSignatureAuthoringTests
{
    private const string Folder = "Assets/Features/Defense3D/Effects/Assets/";
    [TestCase("Cast", 2)]
    [TestCase("Release", 13)]
    public void AirBrushHasBakedHeightAndClearsAllRenderersOnPoolReturn(string phase, int count)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "VFX_Scholar" + phase + "_VongXuyen.prefab");
        var copy = Object.Instantiate(prefab);
        try
        {
            var air = copy.transform.Find("AirBrush");
            Assert.IsNotNull(air);
            var renderers = air.GetComponentsInChildren<MeshRenderer>(true);
            Assert.AreEqual(count, renderers.Length);
            var instance = copy.GetComponent<DefenseEffectInstance3D>();
            instance.Prepare(Color.green, 12f);
            instance.Render(.3f);
            foreach (var renderer in renderers)
            {
                Assert.IsTrue(renderer.enabled);
                Assert.IsFalse(ShaderUtil.ShaderHasError(renderer.sharedMaterial.shader));
            }
            var mesh = renderers[0].GetComponent<MeshFilter>().sharedMesh;
            Assert.Greater(mesh.bounds.max.y, 1.5f);
            Assert.Greater(mesh.vertexCount, 100);
            instance.ResetPresentation();
            var block = new MaterialPropertyBlock();
            foreach (var renderer in renderers)
            {
                Assert.IsFalse(renderer.enabled);
                renderer.GetPropertyBlock(block);
                Assert.IsTrue(block.isEmpty);
            }
            instance.Prepare(Color.yellow, 12f);
            instance.Render(.3f);
            Assert.IsTrue(renderers[0].enabled);
        }
        finally { Object.DestroyImmediate(copy); }
    }
    [Test]
    public void CameraImpulseClearsWithoutAccumulatingFollowDrift()
    {
        var cameraObject = new GameObject("InkImpulseTestCamera");
        var targetObject = new GameObject("InkImpulseTestTarget");
        try
        {
            var follower = cameraObject.AddComponent<ProjectZombie.Features.Player.Camera3DFollower>();
            follower.target = targetObject.transform;
            follower.offset = Vector3.zero;
            var tick = follower.GetType().GetMethod("LateUpdate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            for (int i = 0; i < 8; i++)
            {
                follower.PlayImpulse(.075f, .16f);
                tick.Invoke(follower, null);
                Assert.LessOrEqual(cameraObject.transform.position.magnitude, .08f);
                follower.ClearImpulse();
                Assert.Less(cameraObject.transform.position.magnitude, .00001f);
            }
            follower.ClearImpulse();
            Assert.AreEqual(Vector3.zero, cameraObject.transform.position);
        }
        finally
        {
            Object.DestroyImmediate(cameraObject);
            Object.DestroyImmediate(targetObject);
        }
    }
    [Test]
    public void AnimatedWindupAdvancesWithoutExpiringAndKeepsLegacySustainedBehavior()
    {
        var windup = new DefenseEffectLifetime(10f, .38f, true, true);
        Assert.AreEqual(.5f, windup.Progress(10.19f), .001f);
        Assert.AreEqual(1f, windup.Progress(11f));
        Assert.IsFalse(windup.IsExpired(11f));
        Assert.AreEqual(0f, new DefenseEffectLifetime(10f, .38f, true).Progress(11f));
        Assert.IsTrue(AssetDatabase.LoadAssetAtPath<DefenseEffectProfileSO>(Folder + "FX_ScholarCast.asset").AnimateSustained);
    }
    [Test]
    public void StyleUsesSustainedCastAndTimedReleaseHit_WithBoundedSparkCount()
    {
        var style = AssetDatabase.LoadAssetAtPath<DefenseSignatureEffectSetSO>(Folder + "FXSet_ScholarSignature.asset");
        Assert.IsNotNull(style); Assert.IsTrue(style.IsValid);
        Assert.IsTrue(style.Cast.Sustained); Assert.IsFalse(style.Release.Sustained); Assert.IsFalse(style.Hit.Sustained);
        Assert.IsNotNull(style.Cast.Prefab.GetComponent<DefenseScholarInkVisual3D>());
        Assert.IsNotNull(style.Release.Prefab.GetComponent<DefenseScholarInkVisual3D>());
        Assert.IsNotNull(style.Hit.Prefab.GetComponent<DefenseScholarInkVisual3D>());
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
    [TestCase("Cast", 0)]
    [TestCase("Release", 1)]
    [TestCase("Hit", 2)]
    public void InkPhaseUsesSharedShaderAndResetsForEveryPoolLease(string name, int phase)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "VFX_Scholar" + name + "_VongXuyen.prefab");
        var copy = Object.Instantiate(prefab);
        try
        {
            var instance = copy.GetComponent<DefenseEffectInstance3D>();
            var renderer = copy.GetComponentInChildren<MeshRenderer>();
            Assert.AreEqual("ProjectZombie/Defense3D/ScholarInk", renderer.sharedMaterial.shader.name);
            Assert.IsFalse(ShaderUtil.ShaderHasError(renderer.sharedMaterial.shader));
            var block = new MaterialPropertyBlock();
            instance.Prepare(Color.green, 12f);
            instance.Render(.7f);
            renderer.GetPropertyBlock(block);
            Assert.AreEqual(.7f, block.GetFloat("_Progress"), .001f);
            Assert.AreEqual(phase, block.GetFloat("_Phase"));
            instance.ResetPresentation();
            renderer.GetPropertyBlock(block);
            Assert.IsFalse(renderer.enabled);
            Assert.IsTrue(block.isEmpty);
            instance.Prepare(Color.yellow, 6f);
            renderer.GetPropertyBlock(block);
            Assert.AreEqual(0f, block.GetFloat("_Progress"));
            Assert.AreEqual(Vector3.one * 6f, renderer.transform.localScale);
        }
        finally { Object.DestroyImmediate(copy); }
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
            renderer.GetPropertyBlock(block);
            Assert.AreEqual(0f, block.GetFloat("_Progress"));
            Assert.AreEqual(Color.yellow, block.GetColor("_BaseColor"));
        }
        finally { UnityEngine.Object.DestroyImmediate(copy); }
    }
}
