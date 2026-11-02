#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.UI;

/// <summary>Prefab-only structure art authoring. Bakes primitive construction into shared meshes by material.</summary>
public static class DefenseStructureArtBuilder
{
    public const string ART = "Assets/_Data/Defense3D/Towers/Models/";
    public const string FIRE = "Assets/_Prefabs/Defense3D/Towers/P_Tower_HoaLong_Cannon.prefab";
    public const string GHOST = "Assets/Resources/Defense3D/DefenseGhostCannon3D.prefab";
    private static Material stone, wood, dark, gold, red, jade, ice, purple;
    [MenuItem("Tools/ProjectZombie/Towers/Build Structure Models and Health")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Requires Edit Mode.");
        Directory.CreateDirectory(ART); AssetDatabase.Refresh();
        stone = Mat("Stone", new Color(.28f,.37f,.39f)); wood = Mat("Wood", new Color(.44f,.24f,.11f));
        dark = Mat("Iron", new Color(.075f,.12f,.15f)); gold = Mat("Bronze", new Color(.84f,.57f,.19f));
        red = Mat("Cinnabar", new Color(.65f,.12f,.075f)); jade = Mat("Jade", new Color(.08f,.47f,.4f));
        ice = Mat("Ice", new Color(.27f,.85f,1f)); purple = Mat("EnemyViolet", new Color(.39f,.13f,.5f));
        var catalog = AssetDatabase.LoadAssetAtPath<DefenseTowerCatalogSO>(DefenseTowerArchitectureInstaller.CATALOG);
        for (int type = 0; type < 3; type++)
        {
            string id = new[] { "tower_cungtien", "tower_bangha", "tower_hoalong" }[type];
            var entry = catalog.Find(id); if (entry == null) throw new InvalidOperationException("Missing tower " + id);
            string actorPath = AssetDatabase.GetAssetPath(entry.Prefab);
            if (type == 2)
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(FIRE) == null)
                {
                    var copy = UnityEngine.Object.Instantiate(entry.Prefab.gameObject);
                    try { PrefabUtility.SaveAsPrefabAsset(copy, FIRE); } finally { UnityEngine.Object.DestroyImmediate(copy); }
                }
                actorPath = FIRE;
                var so = new SerializedObject(catalog);
                var array = so.FindProperty("_entries");
                for (int i = 0; i < array.arraySize; i++)
                    if (array.GetArrayElementAtIndex(i).FindPropertyRelative("Data").objectReferenceValue == entry.Data)
                        array.GetArrayElementAtIndex(i).FindPropertyRelative("Prefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(FIRE).GetComponent<DefenseTowerPrototype3D>();
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            var data = new SerializedObject(entry.Data);
            for (int tier = 1; tier <= entry.Data.MaxLevel; tier++)
            {
                var model = new GameObject(id + "_Tier" + tier);
                try
                {
                    Model(model.transform, type, tier);
                    Bake(model.transform, id + "_" + tier);
                    var prefab = PrefabUtility.SaveAsPrefabAsset(model, ART + id + "_Tier" + tier + ".prefab");
                    data.FindProperty("_levels").GetArrayElementAtIndex(tier - 1).FindPropertyRelative("visualModelPrefab").objectReferenceValue = prefab;
                }
                finally { UnityEngine.Object.DestroyImmediate(model); }
            }
            data.ApplyModifiedPropertiesWithoutUndo();
            var actor = PrefabUtility.LoadPrefabContents(actorPath);
            try
            {
                var old = actor.transform.Find("Model_3D"); if (old != null) old.gameObject.SetActive(false);
                var fallback = actor.transform.Find("StructureFallback"); if (fallback != null) UnityEngine.Object.DestroyImmediate(fallback.gameObject);
                fallback = UnityEngine.Object.Instantiate(entry.Data.GetLevelStats(1).visualModelPrefab, actor.transform).transform;
                fallback.name = "StructureFallback";
                var muzzle = actor.transform.Find("FirePoint");
                if (muzzle == null) { muzzle = new GameObject("FirePoint").transform; muzzle.SetParent(actor.transform, false); }
                muzzle.localPosition = new Vector3(0, 1.9f, .7f);
                var tower = actor.GetComponent<DefenseTowerPrototype3D>(); tower.Configure(entry.Data, null);
                var so = new SerializedObject(tower); so.FindProperty("firePoint").objectReferenceValue = muzzle; so.ApplyModifiedPropertiesWithoutUndo();
                Health(actor, false, 3.1f);
                PrefabUtility.SaveAsPrefabAsset(actor, actorPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(actor); }
        }
        Ghost(); catalog.Validate(); AssetDatabase.SaveAssets(); Preview();
        Directory.CreateDirectory("scratch");
        File.WriteAllText("scratch/StructureArtBuild.txt", "Built 9 tier models, separate Hỏa Long actor and Ghost Cannon model; friendly/enemy structure health bars. Prefab-only; no scene saved.\nPreview: scratch/StructureArtPreview.png");
    }
    private static Material Mat(string name, Color color)
    {
        string path = ART + name + ".mat"; var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null) { mat = new Material(Shader.Find("ProjectZombie/Defense3D/StructureToon")); AssetDatabase.CreateAsset(mat, path); }
        mat.shader = Shader.Find("ProjectZombie/Defense3D/StructureToon"); mat.SetColor("_BaseColor", color); mat.enableInstancing = true; EditorUtility.SetDirty(mat); return mat;
    }
    private static Transform Part(Transform parent, string name, PrimitiveType type, Vector3 pos, Vector3 scale, Material mat, Vector3? rotation = null)
    {
        var obj = GameObject.CreatePrimitive(type); obj.name = name; UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
        obj.transform.SetParent(parent, false); obj.transform.localPosition = pos; obj.transform.localScale = scale;
        obj.transform.localRotation = Quaternion.Euler(rotation ?? Vector3.zero);
        var renderer = obj.GetComponent<MeshRenderer>(); renderer.sharedMaterial = mat; renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
        return obj.transform;
    }
    private static void Beam(Transform parent, Vector3 a, Vector3 b, float width, Material mat)
    {
        var part = Part(parent, "Brace", PrimitiveType.Cube, (a + b) * .5f, new Vector3(width,width,Vector3.Distance(a,b)), mat);
        part.rotation = parent.rotation * Quaternion.LookRotation(b - a);
    }
    private static void Model(Transform root, int type, int tier)
    {
        Part(root,"Foundation",PrimitiveType.Cylinder,new Vector3(0,.12f,0),new Vector3(1.65f,.12f,1.65f),stone);
        Part(root,"Bronze footing",PrimitiveType.Cylinder,new Vector3(0,.29f,0),new Vector3(1.4f,.055f,1.4f),gold);
        for (int i=0;i<4;i++)
        {
            float x=i%2==0?-.45f:.45f,z=i<2?-.42f:.42f;
            Part(root,"Pillar",PrimitiveType.Cube,new Vector3(x,.85f,z),new Vector3(.22f,1.1f,.22f),type==1?stone:wood);
            Part(root,"Pillar cap",PrimitiveType.Cube,new Vector3(x,1.2f,z),new Vector3(.3f,.16f,.3f),gold);
        }
        Part(root,"Core",PrimitiveType.Cylinder,new Vector3(0,.78f,0),new Vector3(.58f,.5f,.58f),type==1?jade:red);
        Part(root,"Platform trim",PrimitiveType.Cylinder,new Vector3(0,1.38f,0),new Vector3(1.42f,.08f,1.42f),dark);
        Part(root,"Platform",PrimitiveType.Cylinder,new Vector3(0,1.5f,0),new Vector3(1.3f,.055f,1.3f),gold);
        var turret=new GameObject("Turret").transform; turret.SetParent(root,false); turret.localPosition=Vector3.up*1.5f;
        if(type==0)
        {
            Part(turret,"Ballista stock",PrimitiveType.Cube,new Vector3(0,.2f,0),new Vector3(.22f,.23f,1.35f),wood);
            Beam(turret,new Vector3(-.85f,.32f,.22f),new Vector3(-.42f,.18f,-.12f),.16f,gold);
            Beam(turret,new Vector3(.85f,.32f,.22f),new Vector3(.42f,.18f,-.12f),.16f,gold);
            Beam(turret,new Vector3(-.42f,.18f,-.12f),new Vector3(.42f,.18f,-.12f),.14f,gold);
            Beam(turret,new Vector3(-.85f,.32f,.22f),new Vector3(0,.2f,-.5f),.025f,dark);
            Beam(turret,new Vector3(.85f,.32f,.22f),new Vector3(0,.2f,-.5f),.025f,dark);
            Part(turret,"Loaded arrow",PrimitiveType.Cube,new Vector3(0,.35f,.25f),new Vector3(.045f,.045f,1f),jade);
            Part(root,"Rear banner",PrimitiveType.Cube,new Vector3(-.58f,2.08f,-.55f),new Vector3(.32f,.6f,.055f),jade);
            Beam(root,new Vector3(-.78f,1.45f,-.55f),new Vector3(-.78f,2.45f,-.55f),.055f,gold);
        }
        else if(type==1)
        {
            var shard=Part(turret,"Ice heart",PrimitiveType.Cube,new Vector3(0,.5f,0),new Vector3(.65f,.95f,.65f),ice,new Vector3(0,45,0));
            shard.GetComponent<MeshFilter>().sharedMesh=Crystal(); shard.localScale=new Vector3(.9f,1.3f,.9f);
            for(int i=0;i<3;i++)
            {
                float a=i*Mathf.PI*2/3; var pos=new Vector3(Mathf.Cos(a)*.5f,.17f,Mathf.Sin(a)*.5f);
                var small=Part(turret,"Satellite crystal",PrimitiveType.Cube,pos,new Vector3(.4f,.65f,.4f),ice);
                small.GetComponent<MeshFilter>().sharedMesh=Crystal();
            }
        }
        else Barrel(turret,false);
        var muzzle=new GameObject("Muzzle").transform;muzzle.SetParent(turret,false);
        muzzle.localPosition=type==2?new Vector3(0,.85f,.8f):type==0?new Vector3(0,.35f,.8f):new Vector3(0,1.35f,0);
        for(int i=0;i<tier;i++) Part(root,"Tier sigil",PrimitiveType.Cube,new Vector3((i-(tier-1)*.5f)*.23f,.85f,-.59f),new Vector3(.13f,.13f,.065f),gold,new Vector3(0,0,45));
        if(tier>=2)
        {
            for(int i=0;i<4;i++)
            {
                float a=i*Mathf.PI*.5f;
                Part(root,"Tier guard",PrimitiveType.Cube,new Vector3(Mathf.Cos(a)*.7f,1.25f,Mathf.Sin(a)*.7f),new Vector3(.2f,.5f,.2f),type==1?ice:gold,new Vector3(0,45,15));
            }
        }
        if(tier>=3)
        {
            for(int s=-1;s<=1;s+=2)
            {
                Beam(root,new Vector3(s*.66f,1.4f,-.45f),new Vector3(s*.66f,2.4f,-.45f),.055f,gold);
                Part(root,"Master banner",PrimitiveType.Cube,new Vector3(s*.8f,2.12f,-.45f),new Vector3(.27f,.55f,.05f),type==1?jade:red);
            }
        }
    }
    private static Mesh Crystal()
    {
        string path=ART+"Crystal.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(mesh!=null)return mesh;
        mesh=new Mesh{name="Crystal"};mesh.vertices=new[]{new Vector3(0,.65f,0),new Vector3(0,-.45f,0),new Vector3(.35f,0,0),new Vector3(0,0,.35f),new Vector3(-.35f,0,0),new Vector3(0,0,-.35f)};
        mesh.triangles=new[]{0,3,2,0,4,3,0,5,4,0,2,5,1,2,3,1,3,4,1,4,5,1,5,2};mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);return mesh;
    }
    private static void Barrel(Transform root,bool hostile)
    {
        Part(root,"Mortar saddle",PrimitiveType.Cube,new Vector3(0,.05f,-.05f),new Vector3(.95f,.38f,.8f),hostile?purple:red);
        var barrel=Part(root,"Hollow barrel",PrimitiveType.Cylinder,new Vector3(0,.46f,.12f),new Vector3(.65f,.75f,.65f),dark,new Vector3(48,0,0));
        barrel.GetComponent<MeshFilter>().sharedMesh=Tube();
        var tip=barrel.localPosition+Quaternion.Euler(48,0,0)*Vector3.up*.75f;
        var rim=Part(root,"Muzzle bronze rim",PrimitiveType.Cylinder,tip,new Vector3(.78f,.12f,.78f),gold,new Vector3(48,0,0));rim.GetComponent<MeshFilter>().sharedMesh=Tube();
        for(int s=-1;s<=1;s+=2)
        {
            Part(root,"Dragon cheek",PrimitiveType.Cube,new Vector3(s*.39f,.4f,.4f),new Vector3(.18f,.32f,.65f),hostile?purple:red,new Vector3(-15,0,s*12));
            Part(root,"Dragon eye",PrimitiveType.Cube,new Vector3(s*.49f,.58f,.5f),new Vector3(.08f,.09f,.22f),hostile?jade:gold);
        }
    }
    private static Mesh Tube()
    {
        string path=ART+"MortarTube.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(mesh!=null)return mesh;
        const int n=12;var v=new Vector3[n*4];var t=new List<int>();
        for(int i=0;i<n;i++)
        {
            float a=i*Mathf.PI*2/n;var d=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
            v[i]=d*.5f+Vector3.down;v[n+i]=d*.5f+Vector3.up;v[n*2+i]=d*.36f+Vector3.up;v[n*3+i]=d*.36f+Vector3.down;
        }
        for(int ring=0;ring<4;ring++)for(int i=0;i<n;i++)
        {int a=ring*n+i,b=ring*n+(i+1)%n,c=(ring+1)%4*n+i,d=(ring+1)%4*n+(i+1)%n;t.AddRange(new[]{a,c,b,b,c,d});}
        mesh=new Mesh{name="Hollow mortar"};mesh.vertices=v;mesh.triangles=t.ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);return mesh;
    }
    private static void Bake(Transform root,string key)
    {
        // Keep independently aimed turret geometry in its own mesh groups.
        foreach(var group in root.GetComponentsInChildren<MeshRenderer>().GroupBy(r=>new { r.sharedMaterial, Turret=r.transform.IsChildOf(root.Find("Turret")) }))
        {
            var parent=group.Key.Turret?root.Find("Turret"):root;
            var combine=group.Select(r=>new CombineInstance{mesh=r.GetComponent<MeshFilter>().sharedMesh,transform=parent.worldToLocalMatrix*r.transform.localToWorldMatrix}).ToArray();
            string path=ART+key+"_"+(group.Key.Turret?"Turret_":"Base_")+group.Key.sharedMaterial.name+".asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(mesh==null){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,path);}else mesh.Clear();
            mesh.name=Path.GetFileNameWithoutExtension(path);mesh.CombineMeshes(combine,true,true);mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
            var obj=new GameObject("Baked "+group.Key.sharedMaterial.name);obj.transform.SetParent(parent,false);
            obj.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=obj.AddComponent<MeshRenderer>();renderer.sharedMaterial=group.Key.sharedMaterial;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            foreach(var old in group.ToArray())UnityEngine.Object.DestroyImmediate(old.gameObject);
        }
    }
    private static void Health(GameObject root,bool hostile,float height)
    {
        var anchor=root.transform.Find("StructureHealthAnchor");if(anchor==null){anchor=new GameObject("StructureHealthAnchor").transform;anchor.SetParent(root.transform,false);}anchor.localPosition=Vector3.up*height;
        var bar=root.transform.Find("StructureHealthBar");
        if(bar==null){var obj=GameObject.CreatePrimitive(PrimitiveType.Quad);UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());obj.name="StructureHealthBar";obj.transform.SetParent(root.transform,false);bar=obj.transform;}
        bar.localPosition=anchor.localPosition;
        var renderer=bar.GetComponent<MeshRenderer>();renderer.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Features/Defense3D/Effects/Assets/MAT_EnemyHealth.mat");renderer.enabled=false;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
        var view=bar.GetComponent<DefenseEnemyHealthView3D>()??bar.gameObject.AddComponent<DefenseEnemyHealthView3D>();
        var so=new SerializedObject(view);so.FindProperty("_renderer").objectReferenceValue=renderer;so.FindProperty("_normalSize").vector2Value=new Vector2(hostile?1.65f:1.45f,.17f);so.FindProperty("_normalColor").colorValue=hostile?new Color(1,.23f,.17f):new Color(.22f,.9f,.54f);so.ApplyModifiedPropertiesWithoutUndo();
        var presenter=root.GetComponent<DefenseStructureHealthPresenter3D>()??root.AddComponent<DefenseStructureHealthPresenter3D>();so=new SerializedObject(presenter);so.FindProperty("_view").objectReferenceValue=view;so.FindProperty("_anchor").objectReferenceValue=anchor;so.ApplyModifiedPropertiesWithoutUndo();
    }
    private static void Ghost()
    {
        var root=PrefabUtility.LoadPrefabContents(GHOST);
        try
        {
            var legacy=root.GetComponent<MeshRenderer>();if(legacy!=null)legacy.enabled=false;
            var cannon=root.GetComponent<DefenseGhostCannon3D>();var so=new SerializedObject(cannon);
            var muzzle=(Transform)so.FindProperty("_muzzle").objectReferenceValue;
            if(muzzle!=null)muzzle.SetParent(root.transform,false);
            var old=root.transform.Find("LauncherModel");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
            var model=new GameObject("LauncherModel").transform;model.SetParent(root.transform,false);model.localPosition=Vector3.down;
            Part(model,"Siege base",PrimitiveType.Cube,new Vector3(0,.25f,0),new Vector3(2.3f,.5f,1.9f),stone);
            Part(model,"Carriage",PrimitiveType.Cube,new Vector3(0,.75f,0),new Vector3(1.4f,.5f,1.4f),purple);
            for(int s=-1;s<=1;s+=2)for(int z=-1;z<=1;z+=2)Part(model,"Wheel",PrimitiveType.Cylinder,new Vector3(s*1.02f,.55f,z*.63f),new Vector3(.78f,.14f,.78f),dark,new Vector3(0,0,90));
            var turret=new GameObject("Turret").transform;turret.SetParent(model,false);turret.localPosition=Vector3.up*1.05f;turret.localScale=Vector3.one*1.4f;Barrel(turret,true);
            for(int s=-1;s<=1;s+=2){Beam(model,new Vector3(s*.8f,.8f,-.6f),new Vector3(s*.8f,2.65f,-.6f),.08f,gold);Part(model,"War banner",PrimitiveType.Cube,new Vector3(s*.59f,2.2f,-.6f),new Vector3(.4f,.75f,.06f),purple);}
            Bake(model,"EnemyLauncher");
            // The muzzle is part of the model so its authored position follows the launcher facing.
            if(muzzle==null)muzzle=new GameObject("Muzzle").transform;
            muzzle.SetParent(model,false);muzzle.localPosition=new Vector3(0,2.42f,.95f);
            so.FindProperty("_muzzle").objectReferenceValue=muzzle;
            so.FindProperty("_modelRoot").objectReferenceValue=model.gameObject;so.FindProperty("_body").objectReferenceValue=model.GetComponentInChildren<MeshRenderer>();so.ApplyModifiedPropertiesWithoutUndo();
            var hitbox=root.GetComponent<SphereCollider>();hitbox.center=new Vector3(0,.25f,0);hitbox.radius=1.05f;
            Health(root,true,2.15f);
            PrefabUtility.SaveAsPrefabAsset(root,GHOST);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
    }
    [MenuItem("Tools/ProjectZombie/Towers/Preview Structure Models")]
    public static void Preview()
    {
        var scene=EditorSceneManager.NewPreviewScene();var cameraObject=new GameObject("Structure preview");SceneManager.MoveGameObjectToScene(cameraObject,scene);
        var camera=cameraObject.AddComponent<Camera>();camera.scene=scene;camera.orthographic=true;camera.orthographicSize=2.25f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.12f,.17f,.19f);
        camera.transform.SetPositionAndRotation(new Vector3(5,6,-8),Quaternion.LookRotation(new Vector3(-5,-4.6f,8)));
        const int size=320;var rt=new RenderTexture(size,size,24);camera.targetTexture=rt;var sheet=new Texture2D(size*4,size*3,TextureFormat.RGB24,false);var tile=new Texture2D(size,size,TextureFormat.RGB24,false);var previous=RenderTexture.active;
        try
        {
            var catalog=AssetDatabase.LoadAssetAtPath<DefenseTowerCatalogSO>(DefenseTowerArchitectureInstaller.CATALOG);
            for(int row=0;row<3;row++)for(int col=0;col<4;col++)
            {
                GameObject model;
                if(col<3)model=UnityEngine.Object.Instantiate(catalog.Find(new[]{"tower_cungtien","tower_bangha","tower_hoalong"}[col]).Data.GetLevelStats(row+1).visualModelPrefab);
                else {model=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(GHOST));model.transform.position=Vector3.up;model.transform.Find("Projectile_Ballistic_Parachuting").gameObject.SetActive(false);}
                SceneManager.MoveGameObjectToScene(model,scene);
                var bar=GameObject.CreatePrimitive(PrimitiveType.Quad);SceneManager.MoveGameObjectToScene(bar,scene);UnityEngine.Object.DestroyImmediate(bar.GetComponent<Collider>());
                var renderer=bar.GetComponent<MeshRenderer>();renderer.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Features/Defense3D/Effects/Assets/MAT_EnemyHealth.mat");
                bar.transform.SetPositionAndRotation(new Vector3(0,3,0),camera.transform.rotation);bar.transform.localScale=new Vector3(1.5f,.17f,1);
                var block=new MaterialPropertyBlock();block.SetFloat("_Ratio",new[]{1f,.65f,.25f}[row]);block.SetColor("_FillColor",col==3?new Color(1,.23f,.17f):new Color(.22f,.9f,.54f));renderer.SetPropertyBlock(block);
                camera.Render();RenderTexture.active=rt;tile.ReadPixels(new Rect(0,0,size,size),0,0);tile.Apply();sheet.SetPixels(col*size,(2-row)*size,size,size,tile.GetPixels());UnityEngine.Object.DestroyImmediate(bar);UnityEngine.Object.DestroyImmediate(model);
            }
            sheet.Apply();Directory.CreateDirectory("scratch");File.WriteAllBytes("scratch/StructureArtPreview.png",sheet.EncodeToPNG());
        }
        finally{RenderTexture.active=previous;camera.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(sheet);UnityEngine.Object.DestroyImmediate(tile);EditorSceneManager.ClosePreviewScene(scene);}
    }
}
#endif
