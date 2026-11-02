#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using ProjectZombie.Features.Defense3D;

public static class DefenseHeavenlyLightningBuilder
{
    public const string FOLDER="Assets/Features/Defense3D/Effects/Assets/Thunder/";
    public const string STYLE=FOLDER+"FXSet_HeavenlyLightning.asset";
    [MenuItem("Tools/VFX Generator/Build Heavenly Lightning")]
    public static void Build()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Requires Edit Mode.");
        Directory.CreateDirectory(FOLDER);AssetDatabase.Refresh();
        var bolt=Material("ThunderBolt","ProjectZombie/Defense3D/HeavenlyLightning");
        var ground=Material("ThunderSeal","ProjectZombie/Defense3D/ThunderSeal");
        var flash=Material("ThunderFlash","ProjectZombie/Defense3D/IceLaserGlow");
        var profiles=new DefenseEffectProfileSO[3];string[] names={"Cast","Release","Hit"};
        for(int phase=0;phase<3;phase++)
        {
            var root=new GameObject("HeavenlyLightning_"+names[phase]);
            try
            {
                int count=phase==1?7:3;var lines=new LineRenderer[count];
                for(int i=0;i<count;i++)
                {
                    var obj=new GameObject("Bolt "+i);obj.transform.SetParent(root.transform,false);var line=obj.AddComponent<LineRenderer>();lines[i]=line;
                    line.sharedMaterial=bolt;line.useWorldSpace=false;line.numCapVertices=2;line.numCornerVertices=2;
                    line.widthCurve=AnimationCurve.Linear(0,.65f,1,1);line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;
                    const int points=13;line.positionCount=points;
                    for(int j=0;j<points;j++)
                    {
                        float t=j/(float)(points-1);Vector3 p;
                        if(phase==1)
                        {
                            Vector3 end=i==0?Vector3.zero:i<3?new Vector3(i==1?-.47f:.5f,0,i==1?.2f:-.2f):new Vector3(Mathf.Cos(i*2.4f)*.85f,0,Mathf.Sin(i*2.4f)*.85f);
                            Vector3 start=i<3?new Vector3((i-1)*.12f,1f-i*.09f,0):new Vector3(0,.6f,0);
                            p=Vector3.Lerp(start,end,t);if(j>0&&j<points-1){p.x+=Mathf.Sin(j*13.7f+i)*.11f;p.z+=Mathf.Cos(j*9.7f+i)*.07f;}
                        }
                        else
                        {
                            float angle=t*Mathf.PI*.75f+i*Mathf.PI*2/3;
                            float radius=phase==0?.78f:.55f;
                            p=new Vector3(Mathf.Cos(angle)*radius,(phase==0?.1f:.05f)+Mathf.Sin(t*Mathf.PI)*.35f,Mathf.Sin(angle)*radius);
                            if(j>0&&j<points-1)p.y+=Mathf.Sin(j*3.4f)*.08f;
                        }
                        line.SetPosition(j,p);
                    }
                }
                var seal=Quad(root.transform,"Ground Seal",ground);seal.transform.localRotation=Quaternion.Euler(90,0,0);seal.transform.localPosition=Vector3.up*.015f;
                var glow=Quad(root.transform,"Impact Flash",flash);
                var visual=root.AddComponent<DefenseHeavenlyLightningVisual3D>();var so=new SerializedObject(visual);
                so.FindProperty("_phase").intValue=phase;var array=so.FindProperty("_bolts");array.arraySize=count;for(int i=0;i<count;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=lines[i];
                so.FindProperty("_ground").objectReferenceValue=seal;so.FindProperty("_flash").objectReferenceValue=glow;so.ApplyModifiedPropertiesWithoutUndo();
                var instance=root.AddComponent<DefenseEffectInstance3D>();so=new SerializedObject(instance);array=so.FindProperty("_visuals");array.arraySize=1;array.GetArrayElementAtIndex(0).objectReferenceValue=visual;so.ApplyModifiedPropertiesWithoutUndo();
                var prefab=PrefabUtility.SaveAsPrefabAsset(root,FOLDER+"VFX_Thunder"+names[phase]+".prefab");
                var profile=Asset<DefenseEffectProfileSO>(FOLDER+"FX_Thunder"+names[phase]+".asset");profiles[phase]=profile;so=new SerializedObject(profile);
                so.FindProperty("_prefab").objectReferenceValue=prefab.GetComponent<DefenseEffectInstance3D>();so.FindProperty("_duration").floatValue=phase==0?.25f:phase==1?.8f:.3f;
                so.FindProperty("_sustained").boolValue=phase==0;so.FindProperty("_animateSustained").boolValue=phase==0;
                so.FindProperty("_color").colorValue=new Color(.3f,.72f,1,1);so.FindProperty("_priority").intValue=phase==1?8:5;so.ApplyModifiedPropertiesWithoutUndo();
            }
            finally{UnityEngine.Object.DestroyImmediate(root);}
        }
        var style=Asset<DefenseSignatureEffectSetSO>(STYLE);var data=new SerializedObject(style);
        data.FindProperty("_cast").objectReferenceValue=profiles[0];data.FindProperty("_release").objectReferenceValue=profiles[1];data.FindProperty("_hit").objectReferenceValue=profiles[2];
        data.FindProperty("_hitRadius").floatValue=.55f;data.FindProperty("_lift").floatValue=.035f;data.FindProperty("_maxHitEffects").intValue=12;
        data.FindProperty("_cameraImpulseAmplitude").floatValue=.1f;data.FindProperty("_cameraImpulseDuration").floatValue=.19f;data.ApplyModifiedPropertiesWithoutUndo();
        var skill=AssetDatabase.LoadAssetAtPath<DefenseSkillDefinitionSO>("Assets/Resources/Defense3D/Skill_DaoSi_Signature.asset");data=new SerializedObject(skill);
        if(skill.Presentation==null)throw new InvalidOperationException("Migrate skill data before rebuilding VFX.");
        var presentationData=new SerializedObject(skill.Presentation);
        if(presentationData.FindProperty("_effects").objectReferenceValue==null)presentationData.FindProperty("_effects").objectReferenceValue=style;
        presentationData.ApplyModifiedPropertiesWithoutUndo();
        if(!style.IsValid)throw new InvalidOperationException("Generated thunder style is invalid.");
        AssetDatabase.SaveAssets();Preview();Directory.CreateDirectory("scratch");File.WriteAllText("scratch/ThunderBuild.txt","Built and wired Heavenly Lightning cast/release/hit profiles, session-driven shaders and branching bolts. Gameplay damage/radius/cooldown preserved. Preview: scratch/ThunderPreview.png");
    }
    private static T Asset<T>(string path)where T:ScriptableObject
    {var a=AssetDatabase.LoadAssetAtPath<T>(path);if(a==null){a=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(a,path);}return a;}
    private static Material Material(string name,string shaderName)
    {var shader=Shader.Find(shaderName);if(shader==null)throw new InvalidOperationException("Import "+shaderName);var mat=AssetDatabase.LoadAssetAtPath<Material>(FOLDER+name+".mat");if(mat==null){mat=new Material(shader);AssetDatabase.CreateAsset(mat,FOLDER+name+".mat");}mat.shader=shader;EditorUtility.SetDirty(mat);return mat;}
    private static MeshRenderer Quad(Transform parent,string name,Material material)
    {var obj=GameObject.CreatePrimitive(PrimitiveType.Quad);obj.name=name;UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());obj.transform.SetParent(parent,false);var r=obj.GetComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;return r;}
    [MenuItem("Tools/VFX Generator/Preview Heavenly Lightning")]
    public static void Preview()
    {
        var scene=EditorSceneManager.NewPreviewScene();var camObject=new GameObject("Thunder Camera");SceneManager.MoveGameObjectToScene(camObject,scene);var camera=camObject.AddComponent<Camera>();camera.scene=scene;
        camera.transform.position=new Vector3(10,14,-17);camera.transform.LookAt(Vector3.up*3);camera.orthographic=true;camera.orthographicSize=8.2f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.06f,.08f,.13f);
        var lightObject=new GameObject("Preview light");SceneManager.MoveGameObjectToScene(lightObject,scene);var light=lightObject.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.2f;light.transform.rotation=Quaternion.Euler(45,-30,0);
        var plane=GameObject.CreatePrimitive(PrimitiveType.Plane);SceneManager.MoveGameObjectToScene(plane,scene);plane.transform.localScale=Vector3.one*1.6f;
        var planeMaterial=new Material(Shader.Find("Universal Render Pipeline/Unlit"));planeMaterial.SetColor("_BaseColor",new Color(.1f,.16f,.15f));plane.GetComponent<MeshRenderer>().sharedMaterial=planeMaterial;
        const int width=480,height=480;var rt=new RenderTexture(width,height,24);camera.targetTexture=rt;var sheet=new Texture2D(width*3,height*2,TextureFormat.RGB24,false);var tile=new Texture2D(width,height,TextureFormat.RGB24,false);var previous=RenderTexture.active;
        try
        {
            var style=AssetDatabase.LoadAssetAtPath<DefenseSignatureEffectSetSO>(STYLE);
            for(int i=0;i<6;i++)
            {
                var profile=i==0?style.Cast:style.Release;float p=i==0?.75f:i%3==0?.04f:i%3==1?.2f:.65f;
                var instance=UnityEngine.Object.Instantiate(profile.Prefab);SceneManager.MoveGameObjectToScene(instance.gameObject,scene);instance.Prepare(profile.Color,9);instance.Begin();instance.Render(p);
                if(i>=3)instance.transform.Find("Ground Seal").GetComponent<Renderer>().enabled=false;
                var enemy=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Defense3D/DefenseEnemy_Magiap.prefab"));SceneManager.MoveGameObjectToScene(enemy,scene);enemy.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled=false;enemy.transform.position=Vector3.right*1.8f;enemy.SetActive(true);
                camera.Render();RenderTexture.active=rt;tile.ReadPixels(new Rect(0,0,width,height),0,0);tile.Apply();sheet.SetPixels(i%3*width,(1-i/3)*height,width,height,tile.GetPixels());
                UnityEngine.Object.DestroyImmediate(enemy);UnityEngine.Object.DestroyImmediate(instance.gameObject);
            }
            sheet.Apply();Directory.CreateDirectory("scratch");File.WriteAllBytes("scratch/ThunderPreview.png",sheet.EncodeToPNG());
        }
        finally{RenderTexture.active=previous;camera.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(sheet);UnityEngine.Object.DestroyImmediate(tile);UnityEngine.Object.DestroyImmediate(planeMaterial);EditorSceneManager.ClosePreviewScene(scene);}
    }
}
#endif
