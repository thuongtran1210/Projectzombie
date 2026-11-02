using NUnit.Framework;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseSoulPickupTests
{
    [Test]
    public void ReusedSoulClearsShaderTimeAndReenablesRenderer()
    {
        var root=new GameObject("SoulPoolTestRoot");
        using(var session=new DefenseSession())
        {
            session.Start();
            var pool=new DefenseWoodDropPool(Resources.Load<GameObject>(DefenseWoodDrop3D.ResourcePath),root.transform,gameplay:session);
            try
            {
                var first=pool.Spawn(new Vector3(22000,.3f,0),10,null,null,session,root.transform);
                first.GetComponent<DefenseSoulPickupVisual3D>().RenderAt(8f);
                var block=new MaterialPropertyBlock();first.GetComponent<Renderer>().GetPropertyBlock(block);
                Assert.AreEqual(8f,block.GetFloat("_AnimTime"));
                pool.Release(first);
                first.GetComponent<Renderer>().GetPropertyBlock(block);Assert.IsTrue(block.isEmpty);
                Assert.IsFalse(first.GetComponent<Renderer>().enabled);
                var second=pool.Spawn(new Vector3(22002,.3f,0),20,null,null,session,root.transform);
                Assert.AreSame(first,second);Assert.IsTrue(second.GetComponent<Renderer>().enabled);
                second.GetComponent<Renderer>().GetPropertyBlock(block);Assert.AreEqual(0f,block.GetFloat("_AnimTime"));
                session.Pause();float time=second.PresentationTime;session.Tick(2f,1000f,false);Assert.AreEqual(time,second.PresentationTime);
            }
            finally{pool.Dispose();}
        }
        Object.DestroyImmediate(root);
    }
}
