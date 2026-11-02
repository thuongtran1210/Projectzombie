using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseHeavenlyLightningPresentationTests
{
    [Test]
    public void DaoSiDefinitionHasValidStyleAndPreservesCombatStats()
    {
        var definition=AssetDatabase.LoadAssetAtPath<DefenseSkillDefinitionSO>("Assets/Resources/Defense3D/Skill_DaoSi_Signature.asset");
        var style=definition.SignaturePresentation;Assert.IsNotNull(style);Assert.IsTrue(style.IsValid);Assert.IsTrue(style.Cast.AnimateSustained);
        var stats=definition.Capture();Assert.AreEqual(95,stats.Damage);Assert.AreEqual(4.5f,stats.Radius);Assert.AreEqual(18,stats.Cooldown);Assert.AreEqual(.25f,stats.Duration);
        Assert.IsNotNull(definition.Icon);
    }
    [Test]
    public void MissingCosmeticStyleCannotBlockSceneBinding()
    {
        var root=new GameObject("Missing cosmetic regression");var session=new DefenseSession();
        try
        {
            var presenter=root.AddComponent<DefenseLightningSignaturePresenter3D>();
            var skill=root.AddComponent<DefenseLightningSignatureEffect3D>();
            Assert.DoesNotThrow(()=>presenter.Bind(skill,session,null,null,null,null));Assert.IsFalse(presenter.IsBound);
            var service=root.AddComponent<DefenseEffectService3D>();
            Assert.DoesNotThrow(()=>presenter.Bind(skill,session,service,null,null,null));Assert.IsFalse(presenter.IsBound);
        }
        finally{session.Dispose();Object.DestroyImmediate(root);}
    }
    [TestCase("Cast")][TestCase("Release")][TestCase("Hit")]
    public void PooledVisualIsFiniteAtEveryPhaseAndResetsAllRenderers(string phase)
    {
        var profile=AssetDatabase.LoadAssetAtPath<DefenseEffectProfileSO>(DefenseHeavenlyLightningBuilder.FOLDER+"FX_Thunder"+phase+".asset");
        var instance=Object.Instantiate(profile.Prefab);
        try
        {
            instance.gameObject.SetActive(false);instance.Prepare(profile.Color,9);instance.Begin();
            foreach(float p in new[]{0f,.05f,.2f,.6f,1f})
            {
                instance.Render(p);
                foreach(var line in instance.GetComponentsInChildren<LineRenderer>())
                {
                    Assert.IsFalse(ShaderUtil.ShaderHasError(line.sharedMaterial.shader));
                    for(int i=0;i<line.positionCount;i++)Assert.IsTrue(((DefensePoint3)line.GetPosition(i)).IsFinite);
                }
            }
            instance.ResetPresentation();foreach(var renderer in instance.GetComponentsInChildren<Renderer>(true))Assert.IsFalse(renderer.enabled);
            instance.Prepare(profile.Color,4);instance.Begin();instance.Render(.05f);
            foreach(var renderer in instance.GetComponentsInChildren<Renderer>())Assert.IsFalse(ShaderUtil.ShaderHasError(renderer.sharedMaterial.shader));
        }
        finally{Object.DestroyImmediate(instance.gameObject);}
    }
    [Test]
    public void TargetedCastStaysAtAimPointAndReleasesOnce()
    {
        var session=new DefenseSession();var owner=new GameObject("Lightning owner");var events=new DefenseSignatureFeedbackEvents();var effects=new DefenseDashFeedbackTests.Sink();
        var style=AssetDatabase.LoadAssetAtPath<DefenseSignatureEffectSetSO>(DefenseHeavenlyLightningBuilder.STYLE);
        try
        {
            session.Start();using(var adapter=new DefenseSignatureFeedbackAdapter(events,session,effects,style,owner.transform,owner.transform,castAtTarget:true))
            {
                var point=new DefensePoint3(3,0,4);var cue=new DefenseSignatureFeedback(1,point,4.5f,session,session.RunGeneration);
                events.PublishStarted(cue);Assert.IsNull(effects.Requests[0].Anchor);Assert.AreEqual(new Vector3(3,style.Lift,4),effects.Requests[0].Position);
                events.PublishReleased(cue);events.PublishEnded(new DefenseSignatureFeedback(1,point,4.5f,session,session.RunGeneration,true));events.PublishReleased(cue);
                Assert.AreEqual(2,effects.Requests.Count);session.Pause();Assert.AreEqual(0,effects.ActiveCount);
            }
        }
        finally{session.Dispose();Object.DestroyImmediate(owner);}
    }
}
