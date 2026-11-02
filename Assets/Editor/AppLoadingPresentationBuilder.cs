#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ProjectZombie.Core.Architecture;

/// <summary>Updates only the persistent loading cover in AppBootstrap; content scenes stay untouched.</summary>
public static class AppLoadingPresentationBuilder
{
    private static TMP_FontAsset font;
    private static readonly Color Gold=new Color(.88f,.73f,.43f), Ivory=new Color(.93f,.95f,.85f);
    [MenuItem("Tools/ProjectZombie/Architecture/Build Progress Loading UI")]
    public static void Build()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Requires Edit Mode.");
        var root=PrefabUtility.LoadPrefabContents(AppArchitectureInstaller.PREFAB);
        try
        {
            var view=root.GetComponentInChildren<AppLoadingView>(true);var parent=view.transform;
            font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/GameFont_Vietnamese_SD.asset");
            foreach(string name in new[]{"Backdrop","Status","Dismiss"}){var old=parent.Find(name);if(old!=null)old.gameObject.SetActive(false);}
            var generated=parent.Find("LoadingDesign");if(generated!=null)UnityEngine.Object.DestroyImmediate(generated.gameObject);
            var design=Node(parent,"LoadingDesign",Vector2.zero,Vector2.one);
            var background=Node(design,"Forest",Vector2.zero,Vector2.one);var raw=background.gameObject.AddComponent<RawImage>();
            raw.texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/UI/VongXuyen/BG_VongXuyen_Forest_Hub.png");raw.raycastTarget=false;
            var aspect=background.gameObject.AddComponent<AspectRatioFitter>();aspect.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;aspect.aspectRatio=1024f/573f;
            Image(design,"Shade",Vector2.zero,Vector2.one,new Color(.015f,.055f,.055f,.63f)).raycastTarget=true;
            Image(design,"Footer",new Vector2(0,0),new Vector2(1,.32f),new Color(.025f,.075f,.075f,.94f));
            Image(design,"Footer Rule",new Vector2(.08f,.32f),new Vector2(.92f,.321f),new Color(.65f,.56f,.33f,.45f));
            Image(design,"Top Rule",new Vector2(.08f,.91f),new Vector2(.92f,.911f),new Color(.65f,.56f,.33f,.4f));
            var brand=Text(design,"Brand",new Vector2(.1f,.835f),new Vector2(.9f,.895f),"VONG XUYÊN",30,Gold);brand.characterSpacing=7;
            var seal=Node(design,"Bronze Seal",new Vector2(.5f,.64f),new Vector2(.5f,.64f));seal.sizeDelta=new Vector2(224,224);
            var graphic=seal.gameObject.AddComponent<AppLoadingSealGraphic>();graphic.raycastTarget=false;
            var eyebrow=Text(design,"Eyebrow",new Vector2(.1f,.46f),new Vector2(.9f,.50f),"CHUẨN BỊ XUẤT TRẬN",18,Gold);eyebrow.characterSpacing=5;
            var title=Text(design,"Stage",new Vector2(.08f,.355f),new Vector2(.92f,.45f),"Sông Quê",60,Ivory);title.fontStyle=FontStyles.Bold;
            var status=Text(design,"Status",new Vector2(.16f,.212f),new Vector2(.78f,.282f),"Đang dựng trụ phòng thủ...",23,Ivory);status.alignment=TextAlignmentOptions.MidlineLeft;
            status.enableAutoSizing=true;status.fontSizeMin=17;status.fontSizeMax=23;
            var percent=Text(design,"Percent",new Vector2(.78f,.215f),new Vector2(.84f,.28f),"76%",32,Gold);percent.alignment=TextAlignmentOptions.MidlineRight;
            Image(design,"Progress Frame",new Vector2(.16f,.177f),new Vector2(.84f,.195f),new Color(.32f,.38f,.28f));
            var track=Image(design,"Progress Track",new Vector2(.162f,.18f),new Vector2(.838f,.192f),new Color(.03f,.11f,.1f));
            var fill=Image(track.transform,"Progress Fill",Vector2.zero,Vector2.one,new Color(.22f,.87f,.69f));fill.type=UnityEngine.UI.Image.Type.Filled;fill.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;fill.fillOrigin=0;fill.fillAmount=.76f;
            fill.sprite=WhiteSprite();
            var head=Node(track.transform,"Progress Spark",new Vector2(.76f,.5f),new Vector2(.76f,.5f));head.sizeDelta=new Vector2(4,22);head.gameObject.AddComponent<Image>().color=Ivory;head.GetComponent<Image>().raycastTarget=false;
            var steps=new TextMeshProUGUI[3];string[] labels={"01   BẢN ĐỒ","02   PHÒNG TUYẾN","03   SẴN SÀNG"};
            for(int i=0;i<3;i++){float x=.16f+i*.2267f;steps[i]=Text(design,"Step "+i,new Vector2(x,.12f),new Vector2(x+.2267f,.155f),labels[i],16,Gold);steps[i].characterSpacing=2;}
            Text(design,"Tip Heading",new Vector2(.12f,.075f),new Vector2(.88f,.105f),"MẸO PHÒNG THỦ",14,Gold).characterSpacing=3;
            var tip=Text(design,"Tip",new Vector2(.12f,.025f),new Vector2(.88f,.074f),"Kết hợp Băng Hà làm chậm với Hỏa Long nổ lan để chặn đàn quái đông.",20,new Color(.64f,.76f,.7f));
            var close=Image(design,"Dismiss",new Vector2(.8f,.835f),new Vector2(.92f,.895f),new Color(.1f,.3f,.25f));close.raycastTarget=true;
            var button=close.gameObject.AddComponent<Button>();button.targetGraphic=close;Text(close.transform,"Label",Vector2.zero,Vector2.one,"ĐÓNG",22,Ivory);close.gameObject.SetActive(false);
            var so=new SerializedObject(view);
            so.FindProperty("_group").objectReferenceValue=view.GetComponent<CanvasGroup>();
            so.FindProperty("_status").objectReferenceValue=status;so.FindProperty("_dismiss").objectReferenceValue=button;
            so.FindProperty("_title").objectReferenceValue=title;so.FindProperty("_percent").objectReferenceValue=percent;so.FindProperty("_eyebrow").objectReferenceValue=eyebrow;so.FindProperty("_tip").objectReferenceValue=tip;
            so.FindProperty("_fill").objectReferenceValue=fill;so.FindProperty("_head").objectReferenceValue=head;so.FindProperty("_seal").objectReferenceValue=seal;
            var stepArray=so.FindProperty("_steps");stepArray.arraySize=3;for(int i=0;i<3;i++)stepArray.GetArrayElementAtIndex(i).objectReferenceValue=steps[i];so.ApplyModifiedPropertiesWithoutUndo();
            var scaler=view.GetComponent<CanvasScaler>();scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
            view.Hide();PrefabUtility.SaveAsPrefabAsset(root,AppArchitectureInstaller.PREFAB);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
        AssetDatabase.SaveAssets();Preview();Directory.CreateDirectory("scratch");File.WriteAllText("scratch/LoadingBuild.txt","Updated persistent progress loading cover. Previews: scratch/LoadingPreview.png, scratch/LoadingWidePreview.png");
    }
    private static RectTransform Node(Transform parent,string name,Vector2 min,Vector2 max)
    {var obj=new GameObject(name,typeof(RectTransform));var r=obj.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
    private static Image Image(Transform parent,string name,Vector2 min,Vector2 max,Color color)
    {var r=Node(parent,name,min,max);var image=r.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;return image;}
    private static TextMeshProUGUI Text(Transform parent,string name,Vector2 min,Vector2 max,string value,float size,Color color)
    {var r=Node(parent,name,min,max);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=value;t.fontSize=size;t.color=color;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;t.enableWordWrapping=true;return t;}
    private static Sprite WhiteSprite()
    {
        const string path="Assets/Core/Architecture/Assets/LoadingWhite.asset";
        var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(sprite!=null)return sprite;
        var texture=new Texture2D(2,2,TextureFormat.RGBA32,false){name="LoadingWhite"};texture.SetPixels(new[]{Color.white,Color.white,Color.white,Color.white});texture.Apply();AssetDatabase.CreateAsset(texture,path);
        sprite=Sprite.Create(texture,new Rect(0,0,2,2),new Vector2(.5f,.5f),100);sprite.name="LoadingWhiteSprite";AssetDatabase.AddObjectToAsset(sprite,path);return sprite;
    }
    [MenuItem("Tools/ProjectZombie/Architecture/Preview Progress Loading UI")]
    public static void Preview(){Render(1920,1080,"scratch/LoadingPreview.png");Render(2340,1080,"scratch/LoadingWidePreview.png");}
    private static void Render(int width,int height,string path)
    {
        var scene=EditorSceneManager.NewPreviewScene();var cameraObject=new GameObject("Loading Preview Camera");SceneManager.MoveGameObjectToScene(cameraObject,scene);var camera=cameraObject.AddComponent<Camera>();camera.scene=scene;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
        var asset=AssetDatabase.LoadAssetAtPath<GameObject>(AppArchitectureInstaller.PREFAB).GetComponentInChildren<AppLoadingView>(true);
        var view=UnityEngine.Object.Instantiate(asset);SceneManager.MoveGameObjectToScene(view.gameObject,scene);
        var canvas=view.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=2;
        var rt=new RenderTexture(width,height,24);camera.targetTexture=rt;var texture=new Texture2D(width,height,TextureFormat.RGB24,false);var old=RenderTexture.active;
        try
        {
            view.Begin("Sông Quê",false);view.ReportProgress(.76f,"Đang dựng trụ phòng thủ...");
            typeof(AppLoadingView).GetField("_displayed",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(view,.76f);
            typeof(AppLoadingView).GetMethod("DrawProgress",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(view,null);
            Canvas.ForceUpdateCanvases();foreach(var text in view.GetComponentsInChildren<TextMeshProUGUI>())text.ForceMeshUpdate();camera.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();Directory.CreateDirectory("scratch");File.WriteAllBytes(path,texture.EncodeToPNG());
        }
        finally{RenderTexture.active=old;camera.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(texture);EditorSceneManager.ClosePreviewScene(scene);}
    }
}
#endif
