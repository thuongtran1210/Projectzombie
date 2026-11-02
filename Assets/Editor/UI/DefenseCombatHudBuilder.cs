#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ProjectZombie.Features.Defense3D.UI;
using ProjectZombie.Features.Defense3D;

public static class DefenseCombatHudBuilder
{
    public const string Controls="Assets/Resources/Defense3D/DefenseSkillControlsCanvas.prefab";
    public const string Command="Assets/Features/Defense3D/Minions/Assets/MinionCommandRow.prefab";
    public const string Art="Assets/Art/UI/CombatHUD/";
    public const string Theme="Assets/Art/UI/CombatHUD/CombatHudTheme.asset";
    private static readonly Color Ivory=new Color(.94f,.90f,.80f);
    private static readonly Color Bronze=new Color(.79f,.66f,.42f);
    [MenuItem("Tools/ProjectZombie/UI/Build Compact Combat HUD")]
    public static void Build()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Requires Edit Mode.");
        for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Save scene edits first.");
        Import();
        EnsureTheme();
        DefenseSkillCatalogSO.Load().Validate();
        var root=PrefabUtility.LoadPrefabContents(Controls);
        try{Skills(root);PrefabUtility.SaveAsPrefabAsset(root,Controls);}finally{PrefabUtility.UnloadPrefabContents(root);}
        root=PrefabUtility.LoadPrefabContents(Command);
        try{Squad(root);PrefabUtility.SaveAsPrefabAsset(root,Command);}finally{PrefabUtility.UnloadPrefabContents(root);}
        var setup=EditorSceneManager.GetSceneManagerSetup();
        try
        {
            foreach(string path in new[]{DefenseMapArtUpgrade.Map,"Assets/Features/CombatLab/Scenes/DefenseBaseline.unity"})
            {
                if(!File.Exists(path))continue;
                var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                foreach(var canvas in scene.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<Canvas>(true)))
                    if(canvas.name=="DefenseSkillControlsCanvas")Skills(canvas.gameObject);
                foreach(var shop in scene.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<DefenseVillageShopView>(true)))Dock(shop);
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
        }
        finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
        AssetDatabase.SaveAssets();Directory.CreateDirectory("scratch");
        File.WriteAllText("scratch/CombatHudBuild.txt","Installed compact bronze/ink HUD. Attack 144; signature 108; relic 100; dash 92 (100 hit target); command plate 72 with 96 hit target at 1080 reference height.\nAuthored sprite borders, radial cooldown, compact status, dynamic thunder/pipe glyphs, safe area, squad count badges. Gameplay bindings preserved.");
    }
    private static void EnsureTheme()
    {
        if(AssetDatabase.LoadAssetAtPath<DefenseCombatHudThemeSO>(Theme)!=null)return;
        var theme=ScriptableObject.CreateInstance<DefenseCombatHudThemeSO>();AssetDatabase.CreateAsset(theme,Theme);
        foreach(var kind in new[]{DefenseSkillKind.BasicAttack,DefenseSkillKind.Dash,DefenseSkillKind.Relic,DefenseSkillKind.Signature})
            theme.ForSlot(kind).Plate=Sprite("Plate_"+(kind==DefenseSkillKind.BasicAttack?"Attack":kind==DefenseSkillKind.Dash?"Dash":kind==DefenseSkillKind.Relic?"Relic":"Signature"));
        EditorUtility.SetDirty(theme);
    }
    private static void Import()
    {
        AssetDatabase.Refresh();
        foreach(string file in Directory.GetFiles(Art,"*.png"))
        {
            string path=file.Replace('\\','/');var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;
            importer.maxTextureSize=512;importer.textureCompression=TextureImporterCompression.CompressedHQ;
            importer.spriteBorder=Path.GetFileName(path).StartsWith("Plate_")?Vector4.one*24:
                Path.GetFileName(path)=="Dock.png"?Vector4.one*24:Vector4.zero;
            var android=importer.GetPlatformTextureSettings("Android");android.overridden=true;android.format=TextureImporterFormat.ASTC_6x6;android.maxTextureSize=512;
            importer.SetPlatformTextureSettings(android);importer.SaveAndReimport();
        }
    }
    private static Sprite Sprite(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(Art+name+".png");
    private static void Rect(RectTransform r,Vector2 pos,Vector2 size,Vector2? anchor=null)
    {r.anchorMin=r.anchorMax=anchor??new Vector2(.5f,.5f);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;r.localScale=Vector3.one;}
    private static RectTransform Node(string name,Transform parent)
    {
        var old=parent.Find(name);if(old!=null)return (RectTransform)old;
        var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent,false);rect.gameObject.layer=5;return rect;
    }
    private static Image Picture(string name,Transform parent,string sprite,Vector2 size)
    {
        var rect=Node(name,parent);Rect(rect,Vector2.zero,size);
        var image=rect.GetComponent<Image>()??rect.gameObject.AddComponent<Image>();image.sprite=Sprite(sprite);image.color=Color.white;image.raycastTarget=false;image.type=Image.Type.Simple;
        image.preserveAspect=true;return image;
    }
    private static void Text(TMP_Text text,int size,Color color)
    {
        text.fontSize=size;text.enableAutoSizing=true;text.fontSizeMin=size-2;text.fontSizeMax=size;
        text.alignment=TextAlignmentOptions.Center;text.color=color;text.raycastTarget=false;text.enableWordWrapping=false;
        text.overflowMode=TextOverflowModes.Ellipsis;
    }
    private static void Set(SerializedObject so,string field,UnityEngine.Object value)=>so.FindProperty(field).objectReferenceValue=value;
    private static void Skills(GameObject root)
    {
        var scaler=root.GetComponent<CanvasScaler>();if(scaler!=null)scaler.matchWidthOrHeight=1f;
        var safe=Node("CombatSafeArea",root.transform);safe.anchorMin=Vector2.zero;safe.anchorMax=Vector2.one;safe.offsetMin=safe.offsetMax=Vector2.zero;
        var safeBehaviour=root.GetComponent<DefenseCombatHudSafeArea>()??root.AddComponent<DefenseCombatHudSafeArea>();
        var safeData=new SerializedObject(safeBehaviour);Set(safeData,"_area",safe);safeData.ApplyModifiedPropertiesWithoutUndo();
        foreach(Transform child in root.transform.Cast<Transform>().ToArray())if(child!=safe)child.SetParent(safe,false);
        var theme=AssetDatabase.LoadAssetAtPath<DefenseCombatHudThemeSO>(Theme);
        var catalog=DefenseSkillCatalogSO.Load();
        foreach(var view in root.GetComponentsInChildren<DefenseSkillButtonView>(true))
        {
            bool attack=view.name=="AttackButton",dash=view.name=="DashButton",relic=view.name=="DepToOngButton";
            string style=attack?"Attack":dash?"Dash":relic?"Relic":"Signature";
            var kind=attack?DefenseSkillKind.BasicAttack:dash?DefenseSkillKind.Dash:relic?DefenseSkillKind.Relic:DefenseSkillKind.Signature;
            var skin=theme.ForSlot(kind);float diameter=skin.Diameter,hit=skin.HitSize;Vector2 pos=skin.Position;
            var behavior=attack?DefenseSkillBehavior.BasicAttack:dash?DefenseSkillBehavior.Dash:relic?DefenseSkillBehavior.Slipper:DefenseSkillBehavior.ScholarSignature;
            var definition=catalog.Skills.First(x=>x.Behavior==behavior);
            Rect((RectTransform)view.transform,pos,Vector2.one*hit,new Vector2(1,0));
            var hitImage=view.GetComponent<Image>();hitImage.sprite=Sprite("Disc");hitImage.color=new Color(1,1,1,.001f);hitImage.raycastTarget=true;
            var face=Node("Face",view.transform);Rect(face,Vector2.zero,Vector2.one*diameter);face.SetAsFirstSibling();
            var plate=face.GetComponent<Image>()??face.gameObject.AddComponent<Image>();plate.sprite=skin.Plate;plate.color=Color.white;plate.type=Image.Type.Simple;plate.preserveAspect=true;plate.raycastTarget=false;
            var data=new SerializedObject(view);
            var icon=(Image)data.FindProperty("_iconImage").objectReferenceValue;
            if(icon==null)icon=view.transform.Find("Icon").GetComponent<Image>();
            icon.transform.SetParent(face,false);Rect(icon.rectTransform,Vector2.zero,Vector2.one*diameter*.64f);icon.color=Color.white;icon.raycastTarget=false;icon.preserveAspect=true;
            Set(data,"_iconImage",icon);
            icon.sprite=definition.Presentation.Icon;
            var cooldown=(Image)data.FindProperty("_cooldownFill").objectReferenceValue;
            cooldown.transform.SetParent(face,false);Rect(cooldown.rectTransform,Vector2.zero,Vector2.one*diameter*.82f);
            cooldown.sprite=Sprite("Disc");cooldown.type=Image.Type.Filled;cooldown.fillMethod=Image.FillMethod.Radial360;cooldown.fillOrigin=2;cooldown.fillClockwise=true;
            cooldown.color=new Color(.025f,.035f,.055f,.82f);cooldown.raycastTarget=false;cooldown.fillAmount=0f;
            var locked=(GameObject)data.FindProperty("_lockedOverlay").objectReferenceValue;locked.transform.SetParent(face,false);
            Rect((RectTransform)locked.transform,Vector2.zero,Vector2.one*diameter*.82f);
            var lockBackground=locked.GetComponent<Image>();if(lockBackground!=null){lockBackground.sprite=Sprite("Disc");lockBackground.color=new Color(.035f,.04f,.05f,.75f);lockBackground.raycastTarget=false;}
            Picture("Padlock",locked.transform,"Glyph_Lock",Vector2.one*diameter*.27f);locked.SetActive(false);
            var coolText=(TMP_Text)data.FindProperty("_cooldownText").objectReferenceValue;coolText.transform.SetParent(face,false);
            Rect(coolText.rectTransform,Vector2.zero,Vector2.one*diameter*.75f);Text(coolText,attack?27:30,Ivory);coolText.text=string.Empty;
            coolText.transform.SetAsLastSibling();
            var label=view.transform.Find("Label").GetComponent<TMP_Text>();
            Rect(label.rectTransform,new Vector2(0,-hit*.5f-13),new Vector2(156,24));Text(label,theme.LabelFontSize,Ivory);
            label.text=definition.Presentation.DisplayName;Set(data,"_nameLabel",label);
            var statusRect=Node("CompactStatus",view.transform);Rect(statusRect,new Vector2(0,-hit*.5f-32),new Vector2(160,18));
            var status=statusRect.GetComponent<TMP_Text>()??statusRect.gameObject.AddComponent<TextMeshProUGUI>();status.font=label.font;Text(status,theme.StatusFontSize,Bronze);status.text=string.Empty;Set(data,"_stateLabel",status);
            status.gameObject.SetActive(!attack);
            var oldStatus=view.transform.Find("PipeStatus");if(oldStatus!=null)oldStatus.gameObject.SetActive(false);
            var oldFrame=view.transform.Find("PipeFrame");if(oldFrame!=null)oldFrame.gameObject.SetActive(false);
            var ready=Picture("ReadyRing",face,"Ring",Vector2.one*diameter*.91f);Set(data,"_readyRing",ready);
            var aim=Picture("CompactAimBorder",face,"Ring",Vector2.one*(diameter+8));aim.gameObject.SetActive(false);Set(data,"_aimBorder",aim);
            var duration=(Image)data.FindProperty("_durationRing").objectReferenceValue;
            if(duration!=null)
            {
                duration.transform.SetParent(face,false);Rect(duration.rectTransform,Vector2.zero,Vector2.one*diameter*.98f);
                duration.sprite=Sprite("Ring");duration.type=Image.Type.Filled;duration.fillMethod=Image.FillMethod.Radial360;duration.color=new Color(.49f,.75f,.66f);duration.raycastTarget=false;
            }
            var oldAim=view.transform.Find("PipeAimBorder");if(oldAim!=null)oldAim.gameObject.SetActive(false);
            data.FindProperty("_compactSkin").boolValue=true;Set(data,"_face",face);
            data.ApplyModifiedPropertiesWithoutUndo();
            foreach(var graphic in view.GetComponentsInChildren<Graphic>(true))if(graphic!=hitImage)graphic.raycastTarget=false;
        }
    }
    private static void Squad(GameObject root)
    {
        // This canvas uses 720 reference height: 64 units correspond to a 96-unit hit target at 1080.
        Rect((RectTransform)root.transform,Vector2.zero,new Vector2(64,80),new Vector2(.5f,1));
        var layout=root.GetComponent<LayoutElement>();layout.preferredWidth=64;layout.preferredHeight=80;
        var row=root.GetComponent<DefenseMinionSquadRowView>();var data=new SerializedObject(row);data.FindProperty("_compactCommand").boolValue=true;
        var button=(Button)data.FindProperty("_button").objectReferenceValue;
        Rect((RectTransform)button.transform,new Vector2(0,-32),new Vector2(64,64),new Vector2(.5f,1));
        var hit=button.GetComponent<Image>();hit.sprite=Sprite("Disc");hit.color=new Color(1,1,1,.001f);hit.raycastTarget=true;
        var face=Picture("CommandFace",button.transform,"Plate_Command",new Vector2(48,48));face.type=Image.Type.Simple;face.transform.SetAsFirstSibling();button.targetGraphic=face;
        var icon=(Image)data.FindProperty("_icon").objectReferenceValue;Rect(icon.rectTransform,Vector2.zero,new Vector2(35,35));icon.preserveAspect=true;icon.raycastTarget=false;
        var name=(TMP_Text)data.FindProperty("_name").objectReferenceValue;name.gameObject.SetActive(false);
        var label=(TMP_Text)data.FindProperty("_label").objectReferenceValue;Rect(label.rectTransform,new Vector2(0,-70),new Vector2(64,17),new Vector2(.5f,1));Text(label,10,Ivory);
        var count=(TMP_Text)data.FindProperty("_count").objectReferenceValue;var badge=count.transform.parent.GetComponent<RectTransform>();
        Rect(badge,new Vector2(21,21),new Vector2(16,16));badge.GetComponent<Image>().color=new Color(.08f,.10f,.12f,.98f);
        Rect(count.rectTransform,Vector2.zero,new Vector2(17,17));Text(count,11,Ivory);
        data.ApplyModifiedPropertiesWithoutUndo();
        foreach(var graphic in root.GetComponentsInChildren<Graphic>(true))graphic.raycastTarget=graphic==hit;
    }
    private static void Dock(DefenseVillageShopView view)
    {
        var data=new SerializedObject(view);var dock=((GameObject)data.FindProperty("_commandDock").objectReferenceValue).GetComponent<RectTransform>();
        Rect(dock,new Vector2(0,12),new Vector2(232,88),new Vector2(.5f,0));dock.pivot=new Vector2(.5f,0);
        var backdrop=dock.GetComponent<Image>()??dock.gameObject.AddComponent<Image>();backdrop.sprite=Sprite("Dock");backdrop.type=Image.Type.Sliced;backdrop.color=Color.white;backdrop.raycastTarget=false;
        var content=(RectTransform)data.FindProperty("_commandContent").objectReferenceValue;
        var scroll=(RectTransform)content.parent.parent;Rect(scroll,new Vector2(0,0),new Vector2(232,88),new Vector2(.5f,1));scroll.pivot=new Vector2(.5f,1);
        var layout=content.GetComponent<HorizontalLayoutGroup>();layout.spacing=8;layout.padding=new RectOffset(12,12,4,0);
        var detailRect=Node("SquadSelectionDetail",dock);Rect(detailRect,new Vector2(0,100),new Vector2(260,22),new Vector2(.5f,0));
        var detail=detailRect.GetComponent<TMP_Text>()??detailRect.gameObject.AddComponent<TextMeshProUGUI>();
        detail.font=((TMP_Text)data.FindProperty("_wood").objectReferenceValue).font;Text(detail,11,Ivory);detail.text=string.Empty;detail.gameObject.SetActive(false);
        Set(data,"_commandDetail",detail);data.ApplyModifiedPropertiesWithoutUndo();
        var canvas=view.GetComponentInParent<CanvasScaler>();if(canvas!=null)canvas.matchWidthOrHeight=1f;
    }
}
#endif
