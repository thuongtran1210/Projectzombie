using System.IO;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.UI.DamageText;

public sealed class VongXuyenCombatTextAssetTests
{
    private const string Folder = "Assets/Resources/UI/CombatText/";
    private const string ScenePath = "Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity";

    [Test]
    public void Style_KeepsCriticalReadableAndAnimationRestrained()
    {
        var style = AssetDatabase.LoadAssetAtPath<DamageTextStyleConfig>(Folder + "CombatTextStyle_VongXuyen.asset");
        Assert.IsNotNull(style);
        Assert.Greater(style.NormalFontSize, 0f);
        Assert.Greater(style.CritFontSize, style.NormalFontSize);
        Assert.Greater(style.PlayerDamageFontSize, style.NormalFontSize);
        Assert.LessOrEqual(style.CritFontSize / style.NormalFontSize, 1.4f);
        Assert.That(style.Lifetime, Is.InRange(.4f, .8f));
        Assert.That(style.RandomArcSpread, Is.InRange(0f, .3f));
        Assert.That(style.ScaleCurve.Evaluate(0f), Is.InRange(1f, 1.2f));
        Assert.AreEqual(1f, style.ScaleCurve.Evaluate(1f), .001f);
        Assert.AreEqual(1f, style.AlphaCurve.Evaluate(0f), .001f);
        Assert.AreEqual(0f, style.AlphaCurve.Evaluate(1f), .001f);
        Assert.AreEqual(5, style.ElementalStyles.Length);
    }

    [Test]
    public void Prefab_UsesSharedOutlinedMaterialWithoutWrappingOrShadows()
    {
        // Đọc asset trực tiếp; không tạo scene, không chạy hoạt ảnh hoặc Play Mode.
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "DamageTextItem_VongXuyen.prefab");
        var material = AssetDatabase.LoadAssetAtPath<Material>(Folder + "MAT_CombatText_VongXuyen.mat");
        Assert.IsNotNull(prefab);
        Assert.IsNotNull(material);
        Assert.IsNotNull(prefab.GetComponent<DamageTextItem>());
        var text = prefab.GetComponent<TextMeshPro>();
        Assert.IsNotNull(text);
        Assert.IsNotNull(text.font);
        Assert.AreEqual(material, text.fontSharedMaterial);
        Assert.IsNotNull(material.GetTexture("_MainTex"));
        Assert.Greater(material.GetFloat("_OutlineWidth"), 0f);
        Assert.Less(material.GetColor("_OutlineColor").grayscale, .2f);
        Assert.IsFalse(text.enableWordWrapping);
        Assert.IsFalse(text.raycastTarget);
        Assert.AreEqual(UnityEngine.Rendering.ShadowCastingMode.Off, prefab.GetComponent<MeshRenderer>().shadowCastingMode);
    }

    [Test]
    public void Scene_ConnectsPresenterCameraAndDedicatedAssets()
    {
        // Kiểm tra tham chiếu đã lưu bằng văn bản, không mở hoặc chạy scene Unity.
        string scene = File.ReadAllText(ScenePath);
        string prefabGuid = AssetDatabase.AssetPathToGUID(Folder + "DamageTextItem_VongXuyen.prefab");
        string styleGuid = AssetDatabase.AssetPathToGUID(Folder + "CombatTextStyle_VongXuyen.asset");
        Assert.IsNotEmpty(prefabGuid);
        Assert.IsNotEmpty(styleGuid);
        StringAssert.Contains("_combatText: {fileID: 8002100002}", scene);
        StringAssert.Contains("_combatTextCamera: {fileID: 1990408784}", scene);
        StringAssert.Contains("guid: " + prefabGuid, scene);
        StringAssert.Contains("guid: " + styleGuid, scene);
        StringAssert.Contains("_listenToLegacyDamage: 0", scene);
        StringAssert.Contains("_faceCamera: 1", scene);
        StringAssert.Contains("_maxActiveCount: 48", scene);
    }
}
