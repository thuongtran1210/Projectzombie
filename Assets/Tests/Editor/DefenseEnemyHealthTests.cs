using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.UI;

public sealed class DefenseEnemyHealthTests
{
    [TestCase("magiap")]
    [TestCase("magiap_boss")]
    [TestCase("yaksha_boss")]
    [TestCase("phicuong")]
    [TestCase("huyetdoi")]
    [TestCase("thuyquai")]
    [TestCase("quaidu")]
    public void EveryProductionEnemyHasOneAuthoredHealthView(string id)
    {
        var data = AssetDatabase.LoadAssetAtPath<DefenseEnemyDataSO>("Assets/Resources/Defense3D/Enemies/"+id+".asset");
        Assert.IsNotNull(data);
        var prefab = data.Prefab;
        var presenters = prefab.GetComponents<DefenseEnemyHealthPresenter3D>();
        Assert.AreEqual(1, presenters.Length); Assert.IsTrue(presenters[0].IsConfigured);
        var views = prefab.GetComponentsInChildren<DefenseEnemyHealthView3D>(true);
        Assert.AreEqual(1, views.Length);
        var anchors = prefab.GetComponent<DefenseActorAnchors3D>();
        Assert.AreEqual(anchors != null && anchors.Head != null ? anchors.Head : prefab.transform, views[0].transform.parent);
        var renderer = views[0].GetComponent<MeshRenderer>();
        Assert.IsFalse(renderer.enabled);
        Assert.AreEqual("ProjectZombie/Defense3D/EnemyHealth", renderer.sharedMaterial.shader.name);
        Assert.IsFalse(ShaderUtil.ShaderHasError(renderer.sharedMaterial.shader));
        Assert.IsNull(views[0].GetComponent<Collider>());
    }

    [TestCase(false)]
    [TestCase(true)]
    public void HealthEventsPauseDeathAndNewLeaseResetTheBar(bool colliderFallback)
    {
        var root = new GameObject("HealthUnitEnemy");
        var cameraObject = new GameObject("HealthUnitCamera");
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        var session = new DefenseSession();
        Material material = null;
        try
        {
            quad.transform.SetParent(root.transform,false);
            var renderer = quad.GetComponent<MeshRenderer>();
            material = new Material(Shader.Find("ProjectZombie/Defense3D/EnemyHealth")); renderer.sharedMaterial = material;
            var view = quad.AddComponent<DefenseEnemyHealthView3D>(); Set(view,"_renderer",renderer);
            var presenter = root.AddComponent<DefenseEnemyHealthPresenter3D>(); Set(presenter,"_view",view);
            var head = new GameObject("Head").transform; head.SetParent(root.transform,false); head.localPosition=Vector3.up*2;
            var camera = cameraObject.AddComponent<Camera>(); camera.transform.position = new Vector3(0,3,-8); camera.transform.LookAt(head);
            var enemy = root.AddComponent<DefenseEnemy3D>();
            var body = root.AddComponent<BoxCollider>();
            session.Start(); enemy.BindGameplay(session);
            var stats = new DefenseEnemyStats("unit","unit",DefenseEnemyLocomotionType.GroundNavMesh,100,1,1,0,0);
            enemy.Initialize(stats);
            var lease = new DefenseActorLifetime(session.SessionIdentity,session.RunGeneration,1);
            enemy.BindLifetime(lease, (actor, lifetime, reason) => { });
            presenter.Bind(enemy,session,lease,colliderFallback ? null : head,camera,body);
            Assert.IsTrue(renderer.enabled);
            Assert.IsTrue(enemy.TakeDamage(new DefenseDamageContext(25f,sessionIdentity:session.SessionIdentity,runGeneration:session.RunGeneration)));
            var block = new MaterialPropertyBlock(); renderer.GetPropertyBlock(block);
            Assert.AreEqual(.75f,block.GetFloat("_Ratio"),.001f);
            session.Pause(); Assert.IsFalse(renderer.enabled);
            session.Resume(); Tick(presenter); Assert.IsTrue(renderer.enabled);
            enemy.TakeDamage(new DefenseDamageContext(1000f,sessionIdentity:session.SessionIdentity,runGeneration:session.RunGeneration));
            Assert.IsFalse(renderer.enabled);
            presenter.Unbind(); lease.TryRetire(session.SessionIdentity,session.RunGeneration,DefenseActorReleaseReason.Defeated);
            enemy.Initialize(stats);
            var next = new DefenseActorLifetime(session.SessionIdentity,session.RunGeneration,2);
            enemy.BindLifetime(next, (actor,lifetime,reason) => { });
            presenter.Bind(enemy,session,next,colliderFallback ? null : head,camera,body);
            renderer.GetPropertyBlock(block);
            Assert.AreEqual(1f,block.GetFloat("_Ratio")); Assert.IsTrue(renderer.enabled);
            presenter.Unbind(); presenter.Unbind(); renderer.GetPropertyBlock(block);
            Assert.IsFalse(renderer.enabled); Assert.IsTrue(block.isEmpty);
            enemy.SetMaxHealth(50f); Assert.IsFalse(renderer.enabled);
        }
        finally
        {
            session.Dispose(); Object.DestroyImmediate(root); Object.DestroyImmediate(cameraObject);
            if(material!=null)Object.DestroyImmediate(material);
        }
    }

    private static void Tick(DefenseEnemyHealthPresenter3D presenter) => typeof(DefenseEnemyHealthPresenter3D)
        .GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(presenter,null);
    private static void Set(object target,string field,object value) => target.GetType()
        .GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(target,value);
}
