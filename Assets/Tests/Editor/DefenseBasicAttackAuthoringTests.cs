using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseBasicAttackAuthoringTests
{
    private const string Folder = "Assets/Features/Defense3D/Effects/Assets/";
    [TestCase("Hero")]
    [TestCase("Enemy")]
    public void Style_UsesTimedProfilesAndDedicatedVisualModules(string actor)
    {
        var style = AssetDatabase.LoadAssetAtPath<DefenseBasicAttackEffectSetSO>(Folder + "FXSet_" + actor + "BasicAttack.asset");
        Assert.IsNotNull(style); Assert.IsTrue(style.IsValid);
        Assert.IsNotNull(style.Swing.Prefab.GetComponent<DefenseSlashEffectVisual3D>());
        Assert.IsNotNull(style.Hit.Prefab.GetComponent<DefenseHitSparkVisual3D>());
        Assert.IsFalse(style.Swing.Sustained); Assert.IsFalse(style.Hit.Sustained);
        Assert.That(style.Swing.Duration, Is.InRange(.1f, .3f));
        Assert.That(style.Hit.Duration, Is.InRange(.08f, .25f));
        Assert.AreEqual(Vector3.one, style.Swing.Prefab.transform.localScale);
        Assert.AreEqual(Vector3.one, style.Hit.Prefab.transform.localScale);
        Assert.IsEmpty(style.Hit.Prefab.GetComponentsInChildren<DefenseGameplayBehaviour>(true));
    }
    [Test]
    public void RelicPrefab_RetainsRingModuleAfterInstanceIsGeneralized()
    {
        var profile = AssetDatabase.LoadAssetAtPath<DefenseEffectProfileSO>(Folder + "FX_DepToOng_Impact.asset");
        Assert.IsTrue(profile.IsValid);
        Assert.IsNotNull(profile.Prefab.GetComponent<DefenseRingEffectVisual3D>());
    }
    [Test]
    public void Scene_WiresHeroAndEnemyProfilesAndRemovesOldBasicView()
    {
        // Đọc cấu hình đã lưu; không mở scene hoặc chạy Play Mode.
        string scene = File.ReadAllText("Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity");
        StringAssert.Contains("_basicAttackPresenter: {fileID: 8002300002}", scene);
        StringAssert.Contains(AssetDatabase.AssetPathToGUID(Folder + "FXSet_HeroBasicAttack.asset"), scene);
        StringAssert.Contains(AssetDatabase.AssetPathToGUID(Folder + "FXSet_EnemyBasicAttack.asset"), scene);
        StringAssert.DoesNotContain("8001800132", scene);
        Assert.IsNull(typeof(DefenseHeroAttackEffect3D).GetMethod("Update", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
    }
}
