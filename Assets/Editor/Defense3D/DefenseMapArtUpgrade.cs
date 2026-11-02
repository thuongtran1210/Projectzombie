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
using UnityEngine.Rendering.Universal;
using UnityEditor.Rendering;

public static class DefenseMapArtUpgrade
{
    public const string Map = "Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity";
    public const string Folder = "Assets/Art/Environment/MapSongQue/ArtUpgrade";
    public const string Prefab = Folder + "/SongQue_ArtUpgrade.prefab";
    private static void CleanEditor()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Requires Edit Mode.");
        for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Save scene edits first.");
    }
    public static string Audit()
    {
        CleanEditor();var setup=EditorSceneManager.GetSceneManagerSetup();
        try
        {
            var scene=EditorSceneManager.OpenScene(Map,OpenSceneMode.Single);
            var source=Source(scene);var rows=new List<string>();
            rows.Add("Source: "+source.name+" at "+source.transform.position+" scale "+source.transform.lossyScale);
            foreach(var t in source.GetComponentsInChildren<Transform>(true).Where(x=>x.name=="HutTeaShop"||x.name.Contains("Bridge_Cau")||x.name.Contains("Bank")))
                rows.Add(t.name+" at "+t.position+" scale "+t.lossyScale);
            foreach(var mat in source.GetComponentsInChildren<MeshRenderer>(true).Select(x=>x.sharedMaterial).Where(x=>x!=null).Distinct().Take(6))
            {
                rows.Add("Material "+mat.name+" shader "+mat.shader.name);
                for(int i=0;i<mat.shader.GetPropertyCount();i++)if(mat.shader.GetPropertyType(i)==ShaderPropertyType.Color)
                    rows.Add(mat.shader.GetPropertyName(i)+"="+mat.GetColor(mat.shader.GetPropertyName(i)));
            }
            Directory.CreateDirectory("scratch");File.WriteAllLines("scratch/MapArtAudit.txt",rows);return string.Join("\n",rows);
        }
        finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
    }
    private static GameObject Source(Scene scene)
    {
        return scene.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<Transform>(true))
            .First(x=>x.name=="TREBRAVE_MapSongQue_ThreeJS"||x.name=="TREBRAVE_MapSongQue_ThreeJS_Art").gameObject;
    }

    private static readonly Dictionary<string,Material> materials=new Dictionary<string,Material>();
    private static readonly List<Mesh> temporaryMeshes=new List<Mesh>();
    private static Material wood,woodLight,woodDark,stone,plaster,roof,roofLight,leaf,leafLight,soil,grass,water,foam,clay;
    [MenuItem("Tools/ProjectZombie/Map/Build River Village Art Pass")]
    public static void Build()
    {
        CleanEditor();var setup=EditorSceneManager.GetSceneManagerSetup();GameObject staging=null;
        try
        {
            Directory.CreateDirectory(Folder);AssetDatabase.Refresh();materials.Clear();
            wood=Mat("Cedar",new Color(.36f,.22f,.12f));woodLight=Mat("Wood_Worn",new Color(.59f,.40f,.22f));
            woodDark=Mat("Wood_Shadow",new Color(.19f,.14f,.10f));stone=Mat("River_Stone",new Color(.43f,.47f,.40f));
            plaster=Mat("Lime_Plaster",new Color(.76f,.69f,.48f));roof=Mat("Terracotta",new Color(.48f,.22f,.14f));
            roofLight=Mat("Terracotta_Sun",new Color(.63f,.32f,.19f));leaf=Mat("Canopy_Shade",new Color(.17f,.32f,.22f));
            leafLight=Mat("Canopy_Sun",new Color(.36f,.48f,.25f));soil=Mat("Path_Ochre",new Color(.52f,.42f,.28f));
            grass=Mat("Meadow",new Color(.30f,.40f,.25f));water=Mat("River_Jade",new Color(.105f,.33f,.35f));
            foam=Mat("Water_Glint",new Color(.38f,.60f,.55f));clay=Mat("Ceramic",new Color(.46f,.32f,.23f));
            water.SetFloat("_Smoothness",.65f);
            var scene=EditorSceneManager.OpenScene(Map,OpenSceneMode.Single);var source=Source(scene);
            var previous=scene.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<Transform>(true)).FirstOrDefault(x=>x.name=="SongQue_ArtUpgrade");
            if(previous==null) Capture(scene,"scratch/MapArtBefore.png");
            staging=new GameObject("SongQue_ArtUpgrade");
            SceneManager.MoveGameObjectToScene(staging,scene);
            int sourceCount=0,skipped=0;
            foreach(var renderer in source.GetComponentsInChildren<MeshRenderer>(true))
            {
                var filter=renderer.GetComponent<MeshFilter>();if(filter==null||filter.sharedMesh==null||renderer.sharedMaterial==null)continue;
                sourceCount++;
                bool skip=false;
                for(var t=renderer.transform;t!=source.transform&&t!=null;t=t.parent)
                    if(t.name.EndsWith("_outline",StringComparison.OrdinalIgnoreCase)||t.name=="HutTeaShop"||t.name=="MonkeyBridge_CauKhi"||t.name.StartsWith("Ground_GrassClusters"))skip=true;
                var b=renderer.bounds;
                if(b.center.x>4.4f&&b.center.x<8.1f&&b.size.z>30&&b.size.y<.3f)skip=true;
                if(b.center.x>4.45f&&b.center.x<8.05f&&b.center.y<.05f&&b.size.y<.08f&&Mathf.Max(b.size.x,b.size.z)>.7f)skip=true;
                if(skip){skipped++;continue;}
                var copy=new GameObject(renderer.name);copy.transform.SetParent(staging.transform,false);
                copy.transform.position=renderer.transform.position;copy.transform.rotation=renderer.transform.rotation;copy.transform.localScale=renderer.transform.lossyScale;
                copy.AddComponent<MeshFilter>().sharedMesh=filter.sharedMesh;
                var old=renderer.sharedMaterial;Color color=old.HasProperty("baseColorFactor")?old.GetColor("baseColorFactor"):
                    old.HasProperty("_BaseColor")?old.GetColor("_BaseColor"):old.HasProperty("_Color")?old.color:Color.gray;
                Material mat=renderer.name.StartsWith("Grass_")?grass:renderer.name.StartsWith("Ground_")?soil:
                    Mat("Legacy_"+ColorUtility.ToHtmlStringRGB(color),color);
                copy.AddComponent<MeshRenderer>().sharedMaterial=mat;
            }
            Sample(staging.transform);
            Bake(staging.transform);
            int rendererCount=staging.GetComponentsInChildren<MeshRenderer>().Length;
            var prefab=PrefabUtility.SaveAsPrefabAsset(staging,Prefab);
            UnityEngine.Object.DestroyImmediate(staging);staging=null;
            if(previous!=null)UnityEngine.Object.DestroyImmediate(previous.gameObject);
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);instance.name="SongQue_ArtUpgrade";
            source.SetActive(false);
            HideBasePlaceholder(scene);
            Lighting(scene);
            Configure3D(scene);
            Capture(scene,"scratch/MapArtAfter.png");
            Capture(scene,"scratch/MapArtOverview.png",true);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("scratch");
            File.WriteAllText("scratch/MapArtBuild.txt","Built and installed river-village art pass. Original visual retained inactive. Collision, markers and NavMesh unchanged.\nOriginal renderers: "+sourceCount+"; skipped outline/replaced geometry: "+skipped+"; new chunk renderers: "+rendererCount+"; shared materials: "+instance.GetComponentsInChildren<MeshRenderer>().Select(x=>x.sharedMaterial).Distinct().Count()+"\nPreview: scratch/MapArtAfter.png, scratch/MapArtOverview.png");
        }
        finally
        {
            if(staging!=null)UnityEngine.Object.DestroyImmediate(staging);
            foreach(var mesh in temporaryMeshes)if(mesh!=null)UnityEngine.Object.DestroyImmediate(mesh);temporaryMeshes.Clear();
            EditorSceneManager.RestoreSceneManagerSetup(setup);
        }
    }

    private static Material Mat(string name,Color color)
    {
        if(materials.TryGetValue(name,out var existing))return existing;
        string path=Folder+"/MAT_"+name+".mat";
        var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
        mat.SetColor("_BaseColor",color);mat.SetFloat("_Smoothness",.12f);mat.enableInstancing=true;
        EditorUtility.SetDirty(mat);materials[name]=mat;return mat;
    }
    private static GameObject Part(Transform parent,string name,PrimitiveType type,Vector3 p,Vector3 s,Material mat,Vector3 rotation=default)
    {
        var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=p;go.transform.localScale=s;go.transform.localEulerAngles=rotation;
        UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<MeshRenderer>().sharedMaterial=mat;return go;
    }
    private static void Box(Transform p,string n,Vector3 pos,Vector3 scale,Material m,Vector3 rot=default)=>Part(p,n,PrimitiveType.Cube,pos,scale,m,rot);
    private static void Beam(Transform p,string n,Vector3 a,Vector3 b,float width,Material m)
    {
        var go=Part(p,n,PrimitiveType.Cylinder,(a+b)*.5f,new Vector3(width,(b-a).magnitude*.5f,width),m);
        go.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);
    }
    private static void Patch(Transform p,string name,Vector3 center,float rx,float rz,Material mat,int seed)
    {
        const int n=16;var v=new Vector3[n+1];var uv=new Vector2[n+1];var t=new int[n*3];v[0]=center;
        var random=new System.Random(seed);
        for(int i=0;i<n;i++){float a=i*Mathf.PI*2/n;float r=.86f+(float)random.NextDouble()*.14f;v[i+1]=center+new Vector3(Mathf.Cos(a)*rx*r,0,Mathf.Sin(a)*rz*r);t[i*3]=0;t[i*3+1]=(i+1)%n+1;t[i*3+2]=i+1;}
        var mesh=new Mesh{name=name};mesh.vertices=v;mesh.uv=uv;mesh.triangles=t;mesh.RecalculateNormals();mesh.RecalculateBounds();
        temporaryMeshes.Add(mesh);
        var go=new GameObject(name);go.transform.SetParent(p,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=mat;
    }
    private static void Sample(Transform p)
    {
        // Surface heights match the existing gameplay bank and bridge floors.
        Box(p,"Jade river",new Vector3(6.25f,-.24f,0),new Vector3(3.6f,.04f,40),water);
        for(int i=0;i<32;i++)
        {
            float z=-19+i*1.2f;float wave=Mathf.Sin(i*2.3f)*.25f;
            Box(p,"Current glint",new Vector3(5.5f+wave,-.211f,z),new Vector3(.42f,.008f,.045f),foam,new Vector3(0,12,0));
            Box(p,"Current glint",new Vector3(7.15f-wave,-.211f,z+.4f),new Vector3(.22f,.008f,.035f),foam,new Vector3(0,-8,0));
            for(int bank=0;bank<2;bank++)
            {
                float x=bank==0?4.03f:8.47f;
                Patch(p,"Bank silt",new Vector3(x,.051f,z),.39f,.9f,soil,i+bank*73);
                if(Mathf.Abs(z+.5f)>2&&Mathf.Abs(z-10.5f)>2&&Mathf.Abs(z+10.5f)>2)
                {
                    Part(p,"Bank stone",PrimitiveType.Sphere,new Vector3(x,.12f,z),new Vector3(.36f,.23f,.55f),stone);
                    for(int k=0;k<3;k++)Beam(p,"Reeds",new Vector3(bank==0?4.48f:8.02f,-.15f,z+k*.12f),new Vector3(bank==0?4.38f:8.12f,.5f+k*.12f,z+k*.12f),.045f,leafLight);
                }
            }
        }
        for(int i=0;i<6;i++)Patch(p,"Worn approach",new Vector3(-2.5f+i*1.2f,.062f,-.5f),1.0f,1.35f,soil,100+i);
        for(int i=0;i<12;i++)
        {
            float x=-3.8f+i*.66f;
            Box(p,"Inlaid path stone",new Vector3(x,.078f,-.7f+Mathf.Sin(i*2)*.25f),new Vector3(.45f,.045f,.55f),stone,new Vector3(0,i*17,0));
        }
        Bridge(p);
        House(p,new Vector3(-5.4f,0,3.35f));
        var defenseBase=UnityEngine.Object.FindObjectOfType<ProjectZombie.Features.Defense3D.DefenseBasePrototype3D>();
        if(defenseBase!=null)Shrine(p,new Vector3(defenseBase.transform.position.x,0,defenseBase.transform.position.z));
        Dock(p);
        Tree(p,new Vector3(-9.6f,0,8.6f),1.05f,7);
        Tree(p,new Vector3(-12.8f,0,10.5f),.85f,12);
        Tree(p,new Vector3(1.8f,0,6.7f),.72f,19);
        Tree(p,new Vector3(11.8f,0,8.1f),.8f,22);
        for(int i=0;i<9;i++)
        {
            float x=-8.5f+i*.66f;Box(p,"Garden fence post",new Vector3(x,.48f,6),new Vector3(.09f,.85f,.09f),woodLight);
        }
        Box(p,"Garden fence rail",new Vector3(-5.85f,.65f,6),new Vector3(5.4f,.1f,.1f),wood);
        Box(p,"Garden fence rail",new Vector3(-5.85f,.3f,6),new Vector3(5.4f,.1f,.1f),wood);
        for(int i=0;i<16;i++)
        {
            float x=-10+(i%8)*1.0f,z=7+(i/8)*1.2f;
            Patch(p,"Garden ground",new Vector3(x,.055f,z),.45f,.5f,soil,800+i);
            Part(p,"Garden foliage",PrimitiveType.Sphere,new Vector3(x,.25f,z),new Vector3(.6f,.4f,.55f),i%2==0?leaf:leafLight);
        }
        // Frame the background without covering the bridge approaches or tower sockets.
        foreach(var pos in new[]{new Vector3(-26,0,17),new Vector3(-17,0,18),new Vector3(-7,0,17.5f),new Vector3(14,0,17.5f),new Vector3(27,0,17)})
            Tree(p,pos,.9f,(int)pos.x+100);
    }
    private static void Bridge(Transform p)
    {
        var root=new GameObject("Central bridge");root.transform.SetParent(p,false);root.transform.localPosition=new Vector3(6.25f,0,-.5f);var t=root.transform;
        for(int i=0;i<18;i++)Box(t,"Deck plank",new Vector3(-2.15f+i*.25f,-.025f,0),new Vector3(.235f,.10f,2.8f),i%3==0?wood:woodLight);
        for(int side=-1;side<=1;side+=2)
        {
            Box(t,"Deck fascia",new Vector3(0,-.12f,side*1.37f),new Vector3(4.5f,.25f,.13f),woodDark);
            Box(t,"Hand rail",new Vector3(0,.9f,side*1.35f),new Vector3(4.6f,.12f,.12f),woodLight);
            Box(t,"Lower rail",new Vector3(0,.42f,side*1.35f),new Vector3(4.6f,.07f,.08f),wood);
            for(int i=0;i<5;i++)
            {
                float x=-2.15f+i*1.075f;
                Box(t,"Timber post",new Vector3(x,.44f,side*1.35f),new Vector3(.15f,1.12f,.15f),wood);
                Box(t,"Post cap",new Vector3(x,1.02f,side*1.35f),new Vector3(.22f,.1f,.22f),woodLight);
                if(i%2==0)Part(t,"Pier stone",PrimitiveType.Cylinder,new Vector3(x,-.38f,side*1.3f),new Vector3(.35f,.25f,.35f),stone);
            }
            for(int i=0;i<4;i++)Beam(t,"Rail brace",new Vector3(-2.1f+i*1.075f,.42f,side*1.35f),new Vector3(-1.1f+i*1.075f,.9f,side*1.35f),.06f,wood);
        }
        for(int side=-1;side<=1;side+=2)
        {
            var pos=new Vector3(side*2.12f,.75f,-1.45f);
            Beam(t,"Lantern hanger",pos,pos+Vector3.up*.6f,.06f,woodDark);
            Part(t,"Warm lantern",PrimitiveType.Sphere,pos+Vector3.up*.28f,new Vector3(.27f,.34f,.27f),Mat("Lantern_Paper",new Color(.95f,.64f,.28f)));
            Box(t,"Lantern crown",pos+Vector3.up*.48f,new Vector3(.3f,.07f,.3f),woodDark);
        }
    }
    private static void House(Transform p,Vector3 position)
    {
        var go=new GameObject("Riverside tea house");go.transform.SetParent(p,false);go.transform.localPosition=position;var t=go.transform;
        Box(t,"Stone footing",new Vector3(0,.07f,0),new Vector3(4.1f,.12f,3.2f),stone);
        Box(t,"Back plaster wall",new Vector3(0,1.12f,1.25f),new Vector3(3.7f,2.1f,.18f),plaster);
        Box(t,"Side plaster wall",new Vector3(-1.8f,1.05f,0),new Vector3(.15f,1.95f,2.6f),plaster);
        foreach(float x in new[]{-1.8f,1.8f})foreach(float z in new[]{-1.25f,1.25f})
        {
            Box(t,"Timber frame",new Vector3(x,1.2f,z),new Vector3(.17f,2.35f,.17f),woodDark);
            Part(t,"Stone column foot",PrimitiveType.Cylinder,new Vector3(x,.19f,z),new Vector3(.32f,.12f,.32f),stone);
        }
        foreach(float z in new[]{-1.25f,1.25f})Box(t,"Lintel",new Vector3(0,2.24f,z),new Vector3(3.9f,.18f,.18f),wood);
        for(int side=-1;side<=1;side+=2)
        {
            Box(t,"Roof plane",new Vector3(0,2.65f,side*.81f),new Vector3(4.5f,.18f,1.85f),roof,new Vector3(side*25,0,0));
            for(int row=0;row<7;row++)
            {
                float z=side*(.1f+row*.24f),y=3.15f-row*.112f;
                Box(t,"Tile course",new Vector3(0,y,z),new Vector3(4.55f,.07f,.09f),row%2==0?roofLight:roof);
            }
            Box(t,"Thick roof eave",new Vector3(0,2.29f,side*1.7f),new Vector3(4.65f,.16f,.17f),woodDark);
        }
        Beam(t,"Roof ridge",new Vector3(-2.35f,3.22f,0),new Vector3(2.35f,3.22f,0),.19f,roofLight);
        Box(t,"Tea counter",new Vector3(0,.6f,-1.23f),new Vector3(2.6f,1.0f,.6f),wood);
        Box(t,"Counter top",new Vector3(0,1.13f,-1.23f),new Vector3(2.85f,.12f,.8f),woodLight);
        Box(t,"Shop sign",new Vector3(.4f,1.8f,-1.35f),new Vector3(1.2f,.32f,.09f),woodDark);
        for(int i=0;i<3;i++)Part(t,"Sign carving",PrimitiveType.Cylinder,new Vector3(.05f+i*.35f,1.8f,-1.41f),new Vector3(.14f,.018f,.14f),plaster,new Vector3(90,0,0));
        for(int i=0;i<3;i++)Jar(t,new Vector3(-1.2f+i*.55f,0,-2.0f),.45f+i*.07f);
        Jar(t,new Vector3(.45f,1.2f,-1.2f),.26f);
        for(int i=0;i<3;i++)Part(t,"Tea cup",PrimitiveType.Cylinder,new Vector3(-.5f+i*.28f,1.25f,-1.3f),new Vector3(.14f,.06f,.14f),plaster);
        Box(t,"Low bench",new Vector3(1.8f,.45f,-2.0f),new Vector3(1.6f,.15f,.45f),woodLight);
        foreach(float x in new[]{1.2f,2.4f})Box(t,"Bench leg",new Vector3(x,.22f,-2),new Vector3(.12f,.44f,.25f),wood);
    }
    private static void Jar(Transform p,Vector3 pos,float size)
    {
        Part(p,"Storage jar",PrimitiveType.Sphere,pos+Vector3.up*size*.6f,new Vector3(size,size*1.2f,size),clay);
        Part(p,"Jar rim",PrimitiveType.Cylinder,pos+Vector3.up*size*1.15f,new Vector3(size*.65f,size*.07f,size*.65f),woodDark);
    }
    private static void Shrine(Transform p,Vector3 position)
    {
        var go=new GameObject("Village ancestral shrine");go.transform.SetParent(p,false);go.transform.localPosition=position;var t=go.transform;
        var bronze=Mat("Shrine_Bronze",new Color(.73f,.52f,.22f));
        Box(t,"Foundation",new Vector3(0,.15f,0),new Vector3(3.3f,.3f,3.3f),stone);
        Box(t,"Shrine walls",new Vector3(0,1.35f,0),new Vector3(2.75f,2.4f,2.75f),plaster);
        Box(t,"Door recess",new Vector3(0,1.3f,-1.39f),new Vector3(1.5f,2.1f,.08f),woodDark);
        foreach(float side in new[]{-1f,1f})
        {
            Box(t,"Door leaf",new Vector3(side*.36f,1.25f,-1.45f),new Vector3(.65f,1.85f,.08f),wood);
            Part(t,"Door boss",PrimitiveType.Sphere,new Vector3(side*.18f,1.25f,-1.52f),new Vector3(.11f,.11f,.06f),bronze);
            foreach(float z in new[]{-1.4f,1.4f})
            {
                Box(t,"Shrine column",new Vector3(side*1.4f,1.4f,z),new Vector3(.18f,2.5f,.18f),woodDark);
                Box(t,"Shrine capital",new Vector3(side*1.4f,2.5f,z),new Vector3(.33f,.13f,.33f),bronze);
            }
            Box(t,"Shrine roof",new Vector3(0,2.92f,side*.82f),new Vector3(3.8f,.2f,1.9f),roof,new Vector3(side*25,0,0));
            for(int row=0;row<7;row++)Box(t,"Shrine tile course",new Vector3(0,3.42f-row*.112f,side*(.1f+row*.24f)),new Vector3(3.85f,.07f,.09f),row%2==0?roofLight:roof);
            Box(t,"Shrine eave",new Vector3(0,2.56f,side*1.7f),new Vector3(3.9f,.18f,.18f),woodDark);
            Beam(t,"Ridge finial",new Vector3(side*1.9f,3.46f,0),new Vector3(side*2.08f,3.75f,0),.13f,bronze);
        }
        Beam(t,"Shrine ridge",new Vector3(-2,3.5f,0),new Vector3(2,3.5f,0),.17f,bronze);
        Box(t,"Ancestral plaque",new Vector3(0,2.35f,-1.52f),new Vector3(1.2f,.36f,.09f),woodDark);
        for(int i=0;i<3;i++)Part(t,"Plaque medallion",PrimitiveType.Cylinder,new Vector3(-.32f+i*.32f,2.35f,-1.58f),new Vector3(.14f,.015f,.14f),bronze,new Vector3(90,0,0));
    }
    private static void HideBasePlaceholder(Scene scene)
    {
        foreach(var actor in scene.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<ProjectZombie.Features.Defense3D.DefenseBasePrototype3D>(true)))
        {
            var renderer=actor.GetComponent<MeshRenderer>();if(renderer!=null)renderer.enabled=false;
        }
    }
    private static void Dock(Transform p)
    {
        for(int i=0;i<9;i++)Box(p,"Landing boards",new Vector3(3.35f,.07f,2.6f+i*.22f),new Vector3(1.7f,.09f,.20f),i%2==0?wood:woodLight);
        for(int i=0;i<3;i++)Jar(p,new Vector3(2.8f+i*.48f,0,4.9f),.42f);
        var boat=new GameObject("Moored sampan");boat.transform.SetParent(p,false);boat.transform.localPosition=new Vector3(5.25f,-.07f,4.7f);boat.transform.localEulerAngles=new Vector3(0,-8,0);
        Part(boat.transform,"Hull",PrimitiveType.Sphere,Vector3.zero,new Vector3(.85f,.37f,2.7f),woodDark);
        Box(boat.transform,"Boat well",new Vector3(0,.17f,0),new Vector3(.58f,.04f,1.8f),wood);
        for(int i=0;i<3;i++)Box(boat.transform,"Boat seat",new Vector3(0,.21f,-.65f+i*.65f),new Vector3(.73f,.06f,.17f),woodLight);
        Beam(boat.transform,"Oar",new Vector3(-.5f,.3f,-.9f),new Vector3(.6f,.27f,.9f),.065f,woodLight);
        Beam(p,"Mooring rope",new Vector3(4.1f,.1f,3.7f),new Vector3(5.2f,.02f,3.5f),.035f,woodLight);
    }
    private static void Tree(Transform p,Vector3 pos,float scale,int seed)
    {
        var go=new GameObject("Village broadleaf tree");go.transform.SetParent(p,false);go.transform.localPosition=pos;go.transform.localScale=Vector3.one*scale;var t=go.transform;
        Beam(t,"Trunk",Vector3.zero,new Vector3(.2f,2.7f,.1f),.5f,wood);
        Beam(t,"Branch",new Vector3(.1f,1.8f,0),new Vector3(-1.0f,3.2f,.2f),.22f,wood);
        Beam(t,"Branch",new Vector3(.15f,2.0f,0),new Vector3(1.1f,3.3f,-.2f),.22f,wood);
        var random=new System.Random(seed);
        for(int i=0;i<7;i++)
        {
            float a=i*Mathf.PI*2/7;var center=new Vector3(Mathf.Cos(a)*1.0f,3.0f+(float)random.NextDouble()*.7f,Mathf.Sin(a)*.85f);
            Part(t,"Canopy",PrimitiveType.Sphere,center,new Vector3(2.1f,1.6f,1.9f),i%3==0?leafLight:leaf);
        }
        Patch(t,"Root soil",new Vector3(0,.055f,0),1.0f,.85f,soil,seed);
    }
    private static void Bake(Transform root)
    {
        var renderers=root.GetComponentsInChildren<MeshRenderer>();
        var groups=renderers.GroupBy(r=>new {Mat=r.sharedMaterial,X=Mathf.FloorToInt(r.bounds.center.x/12f),Z=Mathf.FloorToInt(r.bounds.center.z/12f)}).ToArray();
        foreach(var group in groups)
        {
            string key="Chunk_"+group.Key.X+"_"+group.Key.Z+"_"+group.Key.Mat.name;
            string path=Folder+"/"+key+".asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(mesh==null){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,path);}else mesh.Clear();
            mesh.indexFormat=IndexFormat.UInt32;mesh.name=key;
            mesh.CombineMeshes(group.Select(r=>new CombineInstance{mesh=r.GetComponent<MeshFilter>().sharedMesh,transform=root.worldToLocalMatrix*r.transform.localToWorldMatrix}).ToArray(),true,true);
            mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
            var go=new GameObject(key);go.transform.SetParent(root,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=group.Key.Mat;renderer.shadowCastingMode=ShadowCastingMode.On;renderer.receiveShadows=true;
        }
        foreach(var r in renderers)UnityEngine.Object.DestroyImmediate(r.gameObject);
        foreach(var t in root.GetComponentsInChildren<Transform>().Reverse())if(t!=root&&t.childCount==0&&t.GetComponent<MeshRenderer>()==null)UnityEngine.Object.DestroyImmediate(t.gameObject);
    }
    private static void Lighting(Scene scene)
    {
        foreach(var light in scene.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<Light>()))if(light.type==LightType.Directional)
        {light.color=new Color(1f,.91f,.77f);light.intensity=1.15f;light.shadows=LightShadows.Soft;light.transform.rotation=Quaternion.Euler(48,-35,0);light.shadowBias=.035f;light.shadowNormalBias=.25f;}
        RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.65f,.73f,.78f);
        RenderSettings.ambientEquatorColor=new Color(.48f,.53f,.47f);RenderSettings.ambientGroundColor=new Color(.28f,.30f,.26f);
    }
    private static int RendererIndex()
    {
        const string path=Folder+"/SongQue_ForwardRenderer.asset";
        var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
        if(renderer==null)
        {
            renderer=ScriptableObject.CreateInstance<UniversalRendererData>();
            ResourceReloader.ReloadAllNullIn(renderer,"Packages/com.unity.render-pipelines.universal");
            AssetDatabase.CreateAsset(renderer,path);
        }
        var pipeline=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if(pipeline==null)throw new InvalidOperationException("Expected Universal Render Pipeline.");
        var serialized=new SerializedObject(pipeline);var list=serialized.FindProperty("m_RendererDataList");
        for(int i=0;i<list.arraySize;i++)if(list.GetArrayElementAtIndex(i).objectReferenceValue==renderer)return i;
        int index=list.arraySize;list.InsertArrayElementAtIndex(index);list.GetArrayElementAtIndex(index).objectReferenceValue=renderer;
        serialized.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(pipeline);return index;
    }
    private static void Configure3D(Scene scene)
    {
        int index=RendererIndex();
        foreach(var camera in scene.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<Camera>(true)))
            camera.GetUniversalAdditionalCameraData().SetRenderer(index);
    }
    public static void Preview()
    {
        CleanEditor();var setup=EditorSceneManager.GetSceneManagerSetup();
        try
        {
            var scene=EditorSceneManager.OpenScene(Map,OpenSceneMode.Single);Lighting(scene);Configure3D(scene);
            Capture(scene,"scratch/MapArtAfter.png");Capture(scene,"scratch/MapArtOverview.png",true);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
        finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
    }
    private static void Capture(Scene scene,string path,bool overview=false)
    {
        var go=new GameObject("Map art review camera");SceneManager.MoveGameObjectToScene(go,scene);var camera=go.AddComponent<Camera>();
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.14f,.22f,.25f);camera.orthographic=true;camera.orthographicSize=overview?26f:10.5f;
        if(scene.GetRootGameObjects().Any(x=>x.name=="SongQue_ArtUpgrade"))camera.GetUniversalAdditionalCameraData().SetRenderer(RendererIndex());
        Vector3 focus=overview?new Vector3(0,0,0):new Vector3(.0f,0,1.2f);
        go.transform.position=focus+new Vector3(-7,19,-17);go.transform.LookAt(focus);camera.nearClipPlane=.1f;camera.farClipPlane=150;
        var rt=new RenderTexture(1600,1000,24);var texture=new Texture2D(1600,1000,TextureFormat.RGB24,false);var previous=RenderTexture.active;
        try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1600,1000),0,0);texture.Apply();Directory.CreateDirectory("scratch");File.WriteAllBytes(path,texture.EncodeToPNG());}
        finally{RenderTexture.active=previous;camera.targetTexture=null;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(go);}
    }
}
#endif
