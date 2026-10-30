#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Execution;
using NUnit.Framework.Internal.Filters;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.UI;

/// <summary>Runs authored enemy health and lease regression checks on the Unity main thread.</summary>
public static class DefenseEnemyHealthValidation
{
    [MenuItem("Tools/ProjectZombie/Validate Enemy Health Bars")]
    public static void Validate()
    {
        Directory.CreateDirectory("scratch");
        RenderPreview();
        var runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());
        runner.Load(typeof(DefenseEnemyHealthTests).Assembly,new Dictionary<string,object>());
        var filter = new OrFilter();
        foreach(string name in new[]{"DefenseEnemyHealthTests","DefenseEffectAuthoringTests","OwnedPoolTests"})
            filter.Add(new FullNameFilter(name));
        var work = WorkItem.CreateWorkItem(runner.LoadedTest,filter);
        work.InitializeContext(new TestExecutionContext{Dispatcher=new MainThreadDispatcher(),IsSingleThreaded=true});
        work.Execute();
        var result=work.Result;
        File.WriteAllText("scratch/EnemyHealthValidation.xml",result.ToXml(true).OuterXml);
        File.WriteAllText("scratch/EnemyHealthValidation.txt",result.ResultState+"\nPassed: "+result.PassCount+"\nFailed: "+result.FailCount);
        Debug.Log("Enemy health validation: "+result.ResultState+" ("+result.PassCount+" passed)");
    }
    private static void RenderPreview()
    {
        const int WIDTH = 960, HEIGHT = 400;
        var scene = EditorSceneManager.NewPreviewScene();
        var cameraObject = new GameObject("Enemy Health Preview Camera");
        SceneManager.MoveGameObjectToScene(cameraObject,scene);
        var camera = cameraObject.AddComponent<Camera>(); camera.scene=scene;
        camera.transform.position=new Vector3(0,6,-10);camera.transform.LookAt(new Vector3(0,1,0));
        camera.orthographic=true;camera.orthographicSize=2.5f;
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.08f,.12f,.14f);
        var target=new RenderTexture(WIDTH,HEIGHT,24);camera.targetTexture=target;
        var texture=new Texture2D(WIDTH,HEIGHT,TextureFormat.RGB24,false);
        var previous=RenderTexture.active;
        try
        {
            var lightObject=new GameObject("Health Preview Light");SceneManager.MoveGameObjectToScene(lightObject,scene);
            var light=lightObject.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.5f;light.transform.rotation=Quaternion.Euler(45,-25,0);
            string[] ids={"magiap","phicuong","yaksha_boss"};float[] ratios={1f,.55f,.2f};
            for(int i=0;i<ids.Length;i++)
            {
                var data=AssetDatabase.LoadAssetAtPath<DefenseEnemyDataSO>("Assets/Resources/Defense3D/Enemies/"+ids[i]+".asset");
                var instance=Object.Instantiate(data.Prefab);SceneManager.MoveGameObjectToScene(instance,scene);
                instance.transform.position=new Vector3((i-1)*3.4f,0,0);
                var anchors=instance.GetComponent<DefenseActorAnchors3D>();
                var view=instance.GetComponentInChildren<DefenseEnemyHealthView3D>(true);
                view.SetHealth(ratios[i],data.IsBossOrMiniboss);
                var body=instance.GetComponent<Collider>();
                Vector3 head=anchors!=null && anchors.Head!=null ? anchors.Head.position : new Vector3(body.bounds.center.x,body.bounds.max.y,body.bounds.center.z);
                view.SetPose(head+Vector3.up*.2f,camera.transform.rotation);
                view.SetVisible(true);
            }
            camera.Render();RenderTexture.active=target;
            texture.ReadPixels(new Rect(0,0,WIDTH,HEIGHT),0,0);texture.Apply();
            File.WriteAllBytes("scratch/EnemyHealthPreview.png",texture.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active=previous;camera.targetTexture=null;
            Object.DestroyImmediate(target);Object.DestroyImmediate(texture);EditorSceneManager.ClosePreviewScene(scene);
        }
    }
    private sealed class MainThreadDispatcher:IWorkItemDispatcher
    {
        public void Dispatch(WorkItem work)=>work.Execute();
        public void CancelRun(bool force){}
    }
}
#endif
