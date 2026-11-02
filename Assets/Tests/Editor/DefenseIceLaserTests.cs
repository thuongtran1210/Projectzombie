using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

namespace ProjectZombie.Tests.Editor
{
    public sealed class DefenseIceLaserTests
    {
        private sealed class Presentation : IDefenseEffectService
        {
            public int Beams;
            public DefenseEffectHandle Play(in DefenseEffectRequest request)
            { if(request.Profile.Prefab.GetComponent<DefenseTowerLaserVisual3D>()!=null) Beams++; return default; }
            public DefenseEffectHandle Play(DefenseEffectProfileSO profile,Vector3 position,float radius=1,Component owner=null,Transform anchor=null)
                => Play(new DefenseEffectRequest(profile,position,radius,owner,anchor));
            public bool Stop(DefenseEffectHandle handle)=>false;
            public void StopOwner(Component owner) { }
        }
        [TestCase(1,1)] [TestCase(2,1)] [TestCase(3,1)]
        [TestCase(1,3)] [TestCase(2,3)] [TestCase(3,3)]
        public void LevelControlsRealBeamDamageAndTargetsWithoutProjectilePool(int level,int enemies)
        {
            var root=new GameObject("Laser regression");var session=new DefenseSession();
            try
            {
                root.transform.position=new Vector3(10000,0,10000);
                var data=AssetDatabase.LoadAssetAtPath<DefenseTowerCatalogSO>(DefenseTowerArchitectureInstaller.CATALOG).Find("tower_bangha").Data;
                var tower=root.AddComponent<DefenseTowerPrototype3D>();tower.Configure(data,null);tower.BindGameplay(session);
                var presentation=new Presentation();tower.BindAttackPresentation(presentation);
                var targets=new List<TowerSmokeTarget>();
                for(int i=0;i<enemies;i++)
                {
                    var obj=new GameObject("Laser target");obj.transform.SetParent(root.transform,false);obj.transform.localPosition=new Vector3(2+i,0,i*.7f);
                    var target=obj.AddComponent<TowerSmokeTarget>();targets.Add(target);DefenseEnemyTargets3D.Register(session.TargetRegistry,target);
                }
                session.Start();var wallet=root.AddComponent<DefenseWoodWallet3D>();wallet.CollectWood(1000);
                while(tower.Level<level)Assert.IsTrue(tower.TryUpgrade(wallet));
                Assert.AreEqual(DefenseTowerAttackMode.Laser,data.Attack.Mode);
                Assert.IsTrue(tower.TryFire(targets[0],data.Attack));Assert.AreEqual(level,presentation.Beams);
                float damage=data.GetLevelStats(level).damage*data.Attack.DamageMultiplier;
                if(enemies==1){Assert.AreEqual(damage*level,targets[0].Damage,.001f);Assert.AreEqual(1,targets[0].Slows);}
                else for(int i=0;i<enemies;i++){Assert.AreEqual(i<level?damage:0,targets[i].Damage,.001f);Assert.AreEqual(i<level?1:0,targets[i].Slows);}
                session.Pause();Assert.IsFalse(tower.TryFire(targets[0],data.Attack));Assert.AreEqual(level,presentation.Beams);
            }
            finally{session.Dispose();Object.DestroyImmediate(root);}
        }
        [Test]
        public void BeamVisualResetsAndUsesFiniteAuthoredGeometry()
        {
            var profile=AssetDatabase.LoadAssetAtPath<DefenseEffectProfileSO>(DefenseIceLaserBuilder.PROFILE);
            Assert.IsTrue(profile.IsValid);Assert.IsFalse(profile.Sustained);
            var instance=Object.Instantiate(profile.Prefab);
            try
            {
                instance.Prepare(profile.Color,5);instance.Begin();instance.Render(.3f);
                var lines=instance.GetComponentsInChildren<LineRenderer>();Assert.AreEqual(2,lines.Length);
                foreach(var line in lines){Assert.IsTrue(line.enabled);Assert.AreEqual(5,line.GetPosition(1).z,.001f);Assert.IsFalse(ShaderUtil.ShaderHasError(line.sharedMaterial.shader));}
                var glows=instance.GetComponentsInChildren<MeshRenderer>();Assert.AreEqual(2,glows.Length);
                foreach(var glow in glows){Assert.IsTrue(glow.enabled);Assert.IsFalse(ShaderUtil.ShaderHasError(glow.sharedMaterial.shader));}
                instance.ResetPresentation();foreach(var line in lines)Assert.IsFalse(line.enabled);foreach(var glow in glows)Assert.IsFalse(glow.enabled);
                instance.Prepare(profile.Color,2);instance.Begin();foreach(var line in lines)Assert.AreEqual(2,line.GetPosition(1).z,.001f);
            }
            finally{Object.DestroyImmediate(instance.gameObject);}
        }
    }
}
