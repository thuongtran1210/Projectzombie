using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.UI;
using Object=UnityEngine.Object;

public sealed class DefenseSkillDataTests
{
    [Test] public void ShippedCatalogHasUniqueTypedPresentations()
    {Assert.DoesNotThrow(()=>DefenseSkillCatalogSO.Load().Validate());Assert.AreEqual(7,DefenseSkillCatalogSO.Load().Skills.Count);}

    [Test] public void HudUsesProvidedIconRegardlessOfDisplayName()
    {
        var root=new GameObject("PassiveHudTest");
        try
        {
            var image=new GameObject("Icon",typeof(RectTransform),typeof(Image)).GetComponent<Image>();image.transform.SetParent(root.transform);
            var view=root.AddComponent<DefenseSkillButtonView>();var data=new SerializedObject(view);data.FindProperty("_compactSkin").boolValue=true;
            data.FindProperty("_iconImage").objectReferenceValue=image;data.ApplyModifiedPropertiesWithoutUndo();
            var icon=DefenseSkillCatalogSO.Load().Find("skill.pipe").Icon;
            view.SetSkillIdentity("Tên đã đổi hoàn toàn",icon);Assert.AreSame(icon,image.sprite);
            view.SetSkillIdentity("THIÊN LÔI",icon);Assert.AreSame(icon,image.sprite,"A name cannot select a different glyph.");
        }
        finally{Object.DestroyImmediate(root);}
    }
    [Test] public void CatalogRejectsDuplicateIds()
    {
        var catalog=ScriptableObject.CreateInstance<DefenseSkillCatalogSO>();
        try
        {
            var skill=DefenseSkillCatalogSO.Load().Find("skill.dash");var data=new SerializedObject(catalog);var array=data.FindProperty("_skills");array.arraySize=2;
            array.GetArrayElementAtIndex(0).objectReferenceValue=skill;array.GetArrayElementAtIndex(1).objectReferenceValue=skill;data.ApplyModifiedPropertiesWithoutUndo();
            Assert.Throws<InvalidOperationException>(()=>catalog.Validate());
        }
        finally{Object.DestroyImmediate(catalog);}
    }
    [Test] public void CatalogRejectsWrongBehaviorPresentation()
    {
        var catalog=ScriptableObject.CreateInstance<DefenseSkillCatalogSO>();var skill=Object.Instantiate(DefenseSkillCatalogSO.Load().Find("skill.dash"));
        try
        {
            var data=new SerializedObject(skill);data.FindProperty("_behavior").enumValueIndex=(int)DefenseSkillBehavior.ScholarSignature;data.ApplyModifiedPropertiesWithoutUndo();
            data=new SerializedObject(catalog);data.FindProperty("_skills").arraySize=1;data.FindProperty("_skills").GetArrayElementAtIndex(0).objectReferenceValue=skill;data.ApplyModifiedPropertiesWithoutUndo();
            Assert.Throws<InvalidOperationException>(()=>catalog.Validate());
        }
        finally{Object.DestroyImmediate(skill);Object.DestroyImmediate(catalog);}
    }
    [Test] public void LightningTuningIsResolvedWithoutMutatingAsset()
    {
        var source=DefenseSkillCatalogSO.Load().Find("skill.lightning_signature");
        var root=new GameObject("TuningSnapshotTest");
        try
        {
            var effect=root.AddComponent<DefenseLightningSignatureEffect3D>();source.Tuning.Apply(effect);
            Assert.AreEqual(1.5f,effect.StunDuration);Assert.AreEqual(.4f,effect.ParalysisSlowStrength);
            effect.StunDuration=5f;source.Tuning.Apply(effect);Assert.AreEqual(1.5f,effect.StunDuration);
        }
        finally{Object.DestroyImmediate(root);}
    }
}
