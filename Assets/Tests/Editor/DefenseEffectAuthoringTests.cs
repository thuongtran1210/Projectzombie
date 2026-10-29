using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseEffectAuthoringTests
{
    private const string Folder = "Assets/Features/Defense3D/Effects/Assets/";
    [Test]
    public void ImpactPrefab_HasIdentityRootAndNoGameplayComponents()
    {
        // Đọc asset, không mở scene và không chạy Play Mode.
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "VFX_DepToOng_Impact.prefab");
        Assert.IsNotNull(prefab);
        Assert.AreEqual(Vector3.one, prefab.transform.localScale);
        Assert.AreEqual(Vector3.zero, prefab.transform.localPosition);
        Assert.IsTrue(prefab.GetComponent<DefenseEffectInstance3D>().IsConfigured);
        Assert.IsEmpty(prefab.GetComponentsInChildren<DefenseGameplayBehaviour>(true));
        Assert.IsEmpty(prefab.GetComponentsInChildren<Collider>(true));
        Assert.AreEqual(2, prefab.GetComponentsInChildren<MeshRenderer>(true).Length);
        foreach (var renderer in prefab.GetComponentsInChildren<MeshRenderer>(true))
        {
            Assert.IsNotNull(renderer.sharedMaterial);
            Assert.AreEqual(UnityEngine.Rendering.ShadowCastingMode.Off, renderer.shadowCastingMode);
        }
    }
    [Test]
    public void Profile_IsValidAndDoesNotContainGameplayState()
    {
        var profile = AssetDatabase.LoadAssetAtPath<DefenseEffectProfileSO>(Folder + "FX_DepToOng_Impact.asset");
        Assert.IsNotNull(profile);
        Assert.IsTrue(profile.IsValid);
        Assert.IsFalse(profile.Sustained);
        Assert.That(profile.Duration, Is.InRange(.2f, 1f));
        Assert.IsNull(typeof(DefenseEffectProfileSO).GetField("Damage"));
    }
    [Test]
    public void MaGiapTemplate_HasExplicitAnchorsAndSeparateVisualRoot()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Defense3D/DefenseEnemy_Magiap.prefab");
        var anchors = prefab.GetComponent<DefenseActorAnchors3D>();
        Assert.IsNotNull(anchors);
        Assert.IsTrue(anchors.IsConfigured);
        Assert.AreEqual(Vector3.one, anchors.VisualRoot.localScale);
        Assert.AreEqual(prefab.transform, anchors.VisualRoot.parent);
        Assert.IsFalse(anchors.Head.IsChildOf(anchors.VisualRoot));
        foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
            if (renderer.enabled) Assert.IsTrue(renderer.transform.IsChildOf(anchors.VisualRoot));
    }
    [Test]
    public void Scene_WiresRuntimeRootsAndRelicPresentation()
    {
        string scene = File.ReadAllText("Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity");
        foreach (string field in new[] { "_enemyActors", "_allies", "_effects", "_worldEffects", "_attachedEffects", "_combatText", "_enemyPools", "_pickupPools", "_effectPools" })
            StringAssert.IsMatch(field + @": \{fileID: [1-9][0-9]*\}", scene);
        StringAssert.Contains("_effectService: {fileID: 8002200032}", scene);
        StringAssert.Contains("_source: {fileID: 8000500004}", scene);
        StringAssert.Contains(AssetDatabase.AssetPathToGUID(Folder + "FX_DepToOng_Impact.asset"), scene);
        StringAssert.Contains("m_Father: {fileID: 8002200061}", scene);
    }
}
