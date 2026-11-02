using NUnit.Framework;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseHeroGroundSafetyTests
{
    private GameObject root, hero;
    private DefenseHeroGroundSafety3D safety;
    private Rigidbody body;
    private static readonly Vector3 Origin = new Vector3(26000f, 0f, 26000f);
    [SetUp] public void Setup()
    {
        root = new GameObject("GroundSafetyFixture");
        hero = new GameObject("Hero"); hero.transform.SetParent(root.transform);
        hero.transform.position = Origin + Vector3.up;
        var capsule = hero.AddComponent<CapsuleCollider>(); capsule.radius = .3f; capsule.height = 2f;
        body = hero.AddComponent<Rigidbody>(); body.isKinematic = true;
        safety = new DefenseHeroGroundSafety3D(capsule, LayerMask.GetMask("Ground"));
        Floor(new Vector3(-3f, -.5f, 0f), new Vector3(6f, 1f, 10f));
        Physics.SyncTransforms();
    }
    private void Floor(Vector3 center, Vector3 size)
    {
        var go = new GameObject("Floor"); go.transform.SetParent(root.transform);
        go.layer = LayerMask.NameToLayer("Ground"); go.transform.position = Origin + center;
        go.AddComponent<BoxCollider>().size = size;
    }
    [TearDown] public void Cleanup() { Object.DestroyImmediate(root); }
    [Test] public void FootprintStopsBeforeRiverAndOuterEdge()
    {
        Assert.IsTrue(safety.HasSupport(Origin + new Vector3(-.4f,1,0)));
        Assert.IsFalse(safety.HasSupport(Origin + new Vector3(-.1f,1,0)));
        var v = safety.ConstrainVelocity(Origin + new Vector3(-.4f,1,0), new Vector3(5,0,0), .1f);
        Assert.AreEqual(0f,v.x);
        Assert.IsFalse(safety.HasSupport(Origin + new Vector3(-6.1f,1,0)));
    }
    [Test] public void DiagonalMovementSlidesAlongBank()
    {
        var v=safety.ConstrainVelocity(Origin+new Vector3(-.4f,1,0),new Vector3(5,0,5),.1f);
        Assert.AreEqual(0f,v.x); Assert.AreEqual(5f,v.z);
    }
    [Test] public void BridgeConnectsBanksButItsSidesRemainBlocked()
    {
        Floor(new Vector3(2,-.5f,0),new Vector3(4.2f,1,2));
        Floor(new Vector3(7,-.5f,0),new Vector3(6,1,10)); Physics.SyncTransforms();
        var v=safety.ConstrainVelocity(Origin+new Vector3(-1,1,0),new Vector3(8,0,0),1f);
        Assert.AreEqual(8f,v.x);
        Assert.IsFalse(safety.HasSupport(Origin+new Vector3(2,1,.9f)));
    }
    [Test] public void HighSpeedDoesNotSkipWaterGap()
    {
        Floor(new Vector3(7,-.5f,0),new Vector3(6,1,10)); Physics.SyncTransforms();
        var v=safety.ConstrainVelocity(Origin+new Vector3(-1,1,0),new Vector3(8,0,0),1f);
        Assert.AreEqual(0f,v.x);
    }
    [Test] public void FallingReturnsToSupportedPositionAndResetClearsOldAnchor()
    {
        body.position=Origin+new Vector3(-1,1,0); Physics.SyncTransforms();
        Assert.IsFalse(safety.TryRecover(body,.02f));
        body.position=Origin+new Vector3(2,-3,0); Physics.SyncTransforms();
        Assert.IsTrue(safety.TryRecover(body,.3f));
        Assert.That(Vector3.Distance(body.position,Origin+new Vector3(-1,1,0)),Is.LessThan(.01f));
        safety.Reset(); body.position=Origin+new Vector3(2,-3,0); Physics.SyncTransforms();
        Assert.IsFalse(safety.TryRecover(body,.3f));
    }
    [Test] public void WaterTriggerCannotCountAsFloor()
    {
        Floor(new Vector3(2,-.5f,0),new Vector3(4,1,10));
        root.transform.GetChild(root.transform.childCount-1).GetComponent<BoxCollider>().isTrigger=true;
        Physics.SyncTransforms(); Assert.IsFalse(safety.HasSupport(Origin+new Vector3(2,1,0)));
    }
}
