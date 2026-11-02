#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using ProjectZombie.Features.Defense3D;

public static class DefenseSoulPickupBuilder
{
    public const string Prefab="Assets/Resources/Defense3D/DefenseWoodDrop3D.prefab";
    private const string MeshPath="Assets/Resources/Defense3D/DefenseSoulPickupMesh.asset";
    [MenuItem("Tools/ProjectZombie/VFX/Build Soul Pickup")]
    public static void Build()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Requires Edit Mode.");
        var shader=Shader.Find("ProjectZombie/Defense3D/SoulPickup");if(shader==null||ShaderUtil.ShaderHasError(shader))throw new InvalidOperationException("Soul shader must compile.");
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
        if(mesh==null){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,MeshPath);}else mesh.Clear();
        mesh.name="Soul pickup billboard";
        mesh.vertices=new[]{new Vector3(-.7f,-.8f,0),new Vector3(.7f,-.8f,0),new Vector3(.7f,.8f,0),new Vector3(-.7f,.8f,0)};
        mesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};mesh.triangles=new[]{0,2,1,0,3,2};
        mesh.RecalculateNormals();mesh.bounds=new Bounds(new Vector3(0,.24f,0),Vector3.one*2.6f);EditorUtility.SetDirty(mesh);
        const string materialPath="Assets/Resources/Defense3D/DefenseWoodDrop3D.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);material.shader=shader;material.SetColor("_Tint",new Color(.38f,.9f,.79f,1));material.SetFloat("_GlowStrength",.8f);EditorUtility.SetDirty(material);
        var root=PrefabUtility.LoadPrefabContents(Prefab);
        try
        {
            root.transform.localScale=Vector3.one*.65f;root.GetComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=root.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.enabled=true;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.lightProbeUsage=LightProbeUsage.Off;renderer.reflectionProbeUsage=ReflectionProbeUsage.Off;
            var visual=root.GetComponent<DefenseSoulPickupVisual3D>()??root.AddComponent<DefenseSoulPickupVisual3D>();
            var data=new SerializedObject(visual);data.FindProperty("_renderer").objectReferenceValue=renderer;data.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root,Prefab);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
        AssetDatabase.SaveAssets();Preview();
        File.WriteAllText("scratch/SoulPickupBuild.txt","Updated DefenseWoodDrop3D in place: jade soul silhouette, soft halo, camera-facing billboard, session-clock bob/pulse. One renderer, shared material, no Light/ParticleSystem. Pickup values and magnet unchanged.\nPreview: scratch/SoulPickupPreview.png");
    }
    public static void Preview()
    {
        var scene=EditorSceneManager.NewPreviewScene();var previous=RenderTexture.active;
        var cameraObject=new GameObject("Soul preview camera");SceneManager.MoveGameObjectToScene(cameraObject,scene);var camera=cameraObject.AddComponent<Camera>();camera.scene=scene;
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.07f,.12f,.13f);camera.orthographic=true;camera.orthographicSize=1.25f;
        camera.transform.position=new Vector3(0,2,-3);camera.transform.LookAt(new Vector3(0,.6f,0));
        var drop=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab),scene);
        drop.transform.position=new Vector3(0,.3f,0);drop.GetComponent<DefenseSoulPickupVisual3D>().Begin(0,Vector3.zero);drop.GetComponent<DefenseSoulPickupVisual3D>().RenderAt(.4f);
        var rt=new RenderTexture(800,800,24);var texture=new Texture2D(800,800,TextureFormat.RGB24,false);
        try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,800,800),0,0);texture.Apply();Directory.CreateDirectory("scratch");File.WriteAllBytes("scratch/SoulPickupPreview.png",texture.EncodeToPNG());}
        finally{camera.targetTexture=null;RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(texture);EditorSceneManager.ClosePreviewScene(scene);}
    }
}
#endif
