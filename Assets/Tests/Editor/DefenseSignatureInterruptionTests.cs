using System;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseSignatureInterruptionTests
{
    [TestCase(false)]
    [TestCase(true)]
    public void PresenterClearsCompletedAfterglowOnDashOrDowned(bool downed)
    {
        var actor = new GameObject("SignatureInterruptionUnitHero");
        using (var session = new DefenseSession())
        {
            try
            {
                var motor = actor.AddComponent<DefenseHeroMotor3D>();
                typeof(DefenseHeroMotor3D).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(motor, null);
                var health = actor.AddComponent<DefenseHeroHealth3D>();
                typeof(DefenseHeroHealth3D).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(health, null);
                var source = actor.AddComponent<DefenseScholarSignatureEffect3D>();
                motor.BindGameplay(session); health.BindGameplay(session); source.BindGameplay(session);
                var anchor = new GameObject("SignatureInterruptionGround"); anchor.transform.SetParent(actor.transform, false);
                var presenter = actor.AddComponent<DefenseScholarSignaturePresenter3D>();
                Set(presenter, "_source", source); Set(presenter, "_motor", motor); Set(presenter, "_health", health);
                Set(presenter, "_groundAnchor", anchor.transform);
                Set(presenter, "_style", AssetDatabase.LoadAssetAtPath<DefenseSignatureEffectSetSO>("Assets/Features/Defense3D/Effects/Assets/FXSet_ScholarSignature.asset"));
                var effects = new DefenseDashFeedbackTests.Sink();
                presenter.Bind(session, effects); session.Start();
                var cue = new DefenseSignatureFeedback(1, default, 6f, session, session.RunGeneration);
                source.Feedback.PublishStarted(cue); source.Feedback.PublishReleased(cue);
                source.Feedback.PublishHit(new DefenseSignatureHitFeedback(1, new DefenseHitFeedback(default, default, 52.5f, session, session.RunGeneration)));
                source.Feedback.PublishEnded(new DefenseSignatureFeedback(1, default, 6f, session, session.RunGeneration, true));
                Assert.AreEqual(2, effects.ActiveCount);
                if (downed) health.TakeDamage(new DefenseDamageContext(99999f, sessionIdentity: session, runGeneration: session.RunGeneration));
                else motor.BeginDash(actor.transform.position + Vector3.forward, .2f, actor.AddComponent<DefenseDashTraversal3D>());
                Assert.AreEqual(0, effects.ActiveCount); Assert.AreEqual(0, effects.StopOwnerCalls);
                presenter.Unbind(); presenter.Unbind();
            }
            finally
            {
                // EditMode does not guarantee OnDestroy for manually bound, never-awakened actors.
                // Close the session while its gameplay subscribers are still alive.
                try { session.Dispose(); }
                finally { UnityEngine.Object.DestroyImmediate(actor); }
            }
        }
    }
    private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
}
