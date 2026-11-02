using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.UI;

namespace ProjectZombie.Tests.Editor
{
    public sealed class DefenseStructurePresentationTests
    {
        private static void Pose(DefenseStructureHealthPresenter3D p) => typeof(DefenseStructureHealthPresenter3D).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(p,null);
        private static float Ratio(Renderer r) { var b=new MaterialPropertyBlock();r.GetPropertyBlock(b);return b.GetFloat("_Ratio"); }
        [TestCase("tower_cungtien")]
        [TestCase("tower_bangha")]
        [TestCase("tower_hoalong")]
        public void BarTracksDamageUpgradePauseDeathAndInventoryReuse(string id)
        {
            var session=new DefenseSession();var camObj=new GameObject("Structure camera");GameObject actor=null;
            try
            {
                var camera=camObj.AddComponent<Camera>();camera.transform.position=new Vector3(0,6,-8);camera.transform.LookAt(Vector3.up);
                var entry=AssetDatabase.LoadAssetAtPath<DefenseTowerCatalogSO>(DefenseTowerArchitectureInstaller.CATALOG).Find(id);
                actor=Object.Instantiate(entry.Prefab.gameObject);var tower=actor.GetComponent<DefenseTowerPrototype3D>();var health=actor.GetComponent<DefenseSupportTarget3D>();
                var presenter=actor.GetComponent<DefenseStructureHealthPresenter3D>();var bar=actor.transform.Find("StructureHealthBar").GetComponent<MeshRenderer>();
                health.BindGameplay(session);tower.BindGameplay(session);tower.BindHealthPresentation(camera);session.Start();actor.SetActive(true);tower.PrepareForBuild();Pose(presenter);
                Assert.IsTrue(bar.enabled);Assert.AreEqual(1,Ratio(bar),.001f);
                camera.transform.rotation=Quaternion.LookRotation(Vector3.back);Pose(presenter);Assert.IsFalse(bar.enabled);
                camera.transform.LookAt(Vector3.up);Pose(presenter);Assert.IsTrue(bar.enabled);
                Assert.IsTrue(health.TakeDamage(new DefenseDamageContext(health.MaxHP*.5f)));Assert.AreEqual(.5f,Ratio(bar),.001f);
                var wallet=actor.AddComponent<DefenseWoodWallet3D>();wallet.CollectWood(1000);
                Assert.IsTrue(tower.TryUpgrade(wallet));Assert.AreEqual(entry.Data.GetLevelStats(2).maxHealth,health.MaxHP);Assert.AreEqual(1,Ratio(bar),.001f);
                session.Pause();Pose(presenter);Assert.IsFalse(bar.enabled);session.Resume();Pose(presenter);Assert.IsTrue(bar.enabled);
                actor.SetActive(false);Pose(presenter);Assert.IsFalse(bar.enabled);tower.PrepareForBuild();actor.SetActive(true);Pose(presenter);
                Assert.IsTrue(bar.enabled);Assert.AreEqual(1,Ratio(bar),.001f);
                Assert.IsTrue(health.TakeDamage(new DefenseDamageContext(99999)));Assert.IsFalse(bar.enabled);Assert.IsFalse(actor.activeSelf);
            }
            finally {session.Dispose();if(actor!=null)Object.DestroyImmediate(actor);Object.DestroyImmediate(camObj);}
        }
        [Test]
        public void TierModelsHaveFiniteBakedMeshesMuzzlesAndDistinctActors()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<DefenseTowerCatalogSO>(DefenseTowerArchitectureInstaller.CATALOG);
            var actors=new System.Collections.Generic.HashSet<DefenseTowerPrototype3D>();
            foreach(string id in new[]{"tower_cungtien","tower_bangha","tower_hoalong"})
            {
                var entry=catalog.Find(id);Assert.IsTrue(actors.Add(entry.Prefab));Assert.IsTrue(entry.Prefab.GetComponent<DefenseStructureHealthPresenter3D>().IsConfigured);
                var tiers=new System.Collections.Generic.HashSet<GameObject>();
                for(int level=1;level<=entry.Data.MaxLevel;level++)
                {
                    var model=entry.Data.GetLevelStats(level).visualModelPrefab;Assert.IsNotNull(model);Assert.IsTrue(tiers.Add(model));Assert.IsNotNull(model.transform.Find("Turret/Muzzle"));
                    var renderers=model.GetComponentsInChildren<MeshRenderer>();Assert.That(renderers.Length,Is.InRange(2,12));
                    foreach(var r in renderers)
                    {
                        Assert.IsNotNull(r.sharedMaterial);Assert.IsFalse(ShaderUtil.ShaderHasError(r.sharedMaterial.shader));
                        var mesh=r.GetComponent<MeshFilter>().sharedMesh;Assert.IsNotNull(mesh);Assert.Greater(mesh.vertexCount,0);
                        Assert.IsTrue(((DefensePoint3)mesh.bounds.size).IsFinite);Assert.Greater(mesh.bounds.size.sqrMagnitude,0);
                    }
                    Assert.IsEmpty(model.GetComponentsInChildren<Collider>());
                }
            }
        }
        [Test]
        public void GhostDeathModelDoesNotOwnFlightOrHealthBar()
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(DefenseStructureArtBuilder.GHOST);
            var so=new SerializedObject(prefab.GetComponent<DefenseGhostCannon3D>());
            var model=(GameObject)so.FindProperty("_modelRoot").objectReferenceValue;var projectile=(Transform)so.FindProperty("_projectile").objectReferenceValue;
            var muzzle=(Transform)so.FindProperty("_muzzle").objectReferenceValue;
            Assert.IsNotNull(model);Assert.IsTrue(muzzle.IsChildOf(model.transform));Assert.IsFalse(projectile.IsChildOf(model.transform));
            Assert.IsFalse(prefab.transform.Find("StructureHealthBar").IsChildOf(model.transform));Assert.IsTrue(prefab.GetComponent<DefenseStructureHealthPresenter3D>().IsConfigured);
            Assert.IsFalse(prefab.GetComponent<MeshRenderer>().enabled);
        }
    }
}
