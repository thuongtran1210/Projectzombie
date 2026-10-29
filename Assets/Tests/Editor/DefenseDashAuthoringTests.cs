using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseDashAuthoringTests
{
    private const string Folder = "Assets/Features/Defense3D/Effects/Assets/";
    [Test]
    public void DashStyle_HasTimedBurstsAndSustainedTrail()
    {
        var style = AssetDatabase.LoadAssetAtPath<DefenseDashEffectSetSO>(Folder + "FXSet_HeroDash.asset");
        Assert.IsNotNull(style); Assert.IsTrue(style.IsValid);
        Assert.IsFalse(style.Start.Sustained); Assert.IsTrue(style.Trail.Sustained); Assert.IsFalse(style.End.Sustained);
        Assert.IsNotNull(style.Start.Prefab.GetComponent<DefenseRingEffectVisual3D>());
        Assert.IsNotNull(style.Trail.Prefab.GetComponent<DefenseTrailEffectVisual3D>());
        Assert.Greater(style.Trail.Priority, style.Start.Priority);
        Assert.IsEmpty(style.Trail.Prefab.GetComponentsInChildren<DefenseGameplayBehaviour>(true));
        var trail = style.Trail.Prefab.GetComponentInChildren<TrailRenderer>();
        Assert.IsNotNull(trail); Assert.IsFalse(trail.autodestruct); Assert.IsFalse(trail.emitting);
        Assert.That(trail.time, Is.InRange(.05f, .25f));
        Assert.AreEqual(UnityEngine.Rendering.ShadowCastingMode.Off, trail.shadowCastingMode);
        Assert.IsNotNull(trail.sharedMaterial);
        Assert.Less(trail.widthCurve.Evaluate(1f), trail.widthCurve.Evaluate(0f));
    }
    [Test]
    public void DashTrail_RemainsVisibleAboveGroundOverlay()
    {
        var style = AssetDatabase.LoadAssetAtPath<DefenseDashEffectSetSO>(Folder + "FXSet_HeroDash.asset");
        Assert.IsNotNull(style);
        var trail = style.Trail.Prefab.GetComponentInChildren<TrailRenderer>(true);
        var material = trail.sharedMaterial;
        // Anchor sát mặt đất có thể lọt dưới địa hình; material dash phải dùng cùng quy tắc overlay với burst.
        Assert.IsTrue(material.HasProperty("_ZTest"));
        Assert.AreEqual((float)UnityEngine.Rendering.CompareFunction.Always, material.GetFloat("_ZTest"));
        Assert.GreaterOrEqual(style.TrailRadius, .45f);
        Assert.GreaterOrEqual(style.BurstRadius, .6f);
        Assert.GreaterOrEqual(style.End.Duration, .25f);
    }

    [Test]
    public void Scene_WiresDashPresenterGroundAnchorAndExistingService()
    {
        // Không mở scene, không khởi chạy Unity để kiểm tra cấu hình đã lưu.
        string scene = File.ReadAllText("Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity");
        StringAssert.Contains("_dashPresenter: {fileID: 8002400002}", scene);
        StringAssert.Contains("_groundAnchor: {fileID: 8002400011}", scene);
        StringAssert.Contains("m_Name: DashGroundAnchor", scene);
        StringAssert.Contains(AssetDatabase.AssetPathToGUID(Folder + "FXSet_HeroDash.asset"), scene);
        StringAssert.Contains("_effectService: {fileID: 8002200032}", scene);
        Assert.IsNull(typeof(DefenseHeroDashPresenter3D).GetMethod("Update", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
    }
}
