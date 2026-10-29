#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.Defense3D.Loadout;
using Object = UnityEngine.Object;

namespace ProjectZombie.Editor.UI
{
    public static class CardCodexUIGenerator
    {
        private static TMP_FontAsset Font;
        private static readonly Color Gold = new Color(.94f,.80f,.47f);
        private static readonly Color Ink = new Color(.085f,.065f,.05f,.98f);
        private const string Art = "Assets/Art/UI/VongXuyen/";
        private static Sprite SpriteAt(string name)
        {
            if (name.StartsWith("Btn_Tab_Wood")) name = "Btn_Nav_Wood_Stitched";
            return AssetDatabase.LoadAssetAtPath<Sprite>(Art + name + ".png");
        }

        [MenuItem("ProjectZombie/2. 📱 Mobile UI/1. Sảnh Chính (Meta Menu)/10. Tạo Thư Viện Thần Thẻ (Card Codex UI)", priority = 112)]
        public static GameObject GenerateCardCodexPrefab()
        {
            Directory.CreateDirectory("Assets/_Prefabs/UI");
            Directory.CreateDirectory("Assets/Resources/UI");
            Font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Regular SDF.asset")
                ?? AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset") ?? TMP_Settings.defaultFontAsset;
            var card = BuildCardPrefab();
            var unitIcons = BuildUnitIcons();
            var root = Node("Panel_CardCodex", null);
            var authoringScene = EditorSceneManager.NewPreviewScene();
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root.gameObject, authoringScene);
            Stretch(root);
            root.gameObject.AddComponent<CanvasGroup>();
            var view = root.gameObject.AddComponent<CardCodexView>();
            var presenter = root.gameObject.AddComponent<CardCodexPresenter>();
            var layout = root.gameObject.AddComponent<CodexResponsiveLayout>();
            var dim = Panel("Dim_Background", root, null, new Color(.02f,.025f,.025f,.94f)); Stretch(dim);
            var dimButton = dim.gameObject.AddComponent<Button>();
            var safe = Node("SafeArea", root); Stretch(safe);
            var modal = Panel("Modal_Container", safe, SpriteAt("Frame_Modal_TangBaoCac_9Slice"), Color.white);
            Stretch(modal, 24, 24, 24, 24);
            var header = Panel("Header_Top", modal, SpriteAt("Header_Wood_Bar_VongXuyen"), Color.white);
            Top(header, 18, 18, 80, 18);
            var title = Text("Txt_Title", header, "BÁCH BẢO CÁC", 32, Gold, TextAlignmentOptions.MidlineLeft);
            Stretch(title.rectTransform, 28, 330, 0, 0); title.fontStyle = FontStyles.Bold;
            var close = Button("Btn_Close", header, SpriteAt("Btn_Nav_Close_X_Wood"), "", 22);
            Anchor(close.GetComponent<RectTransform>(), new Vector2(1,.5f), new Vector2(1,.5f), new Vector2(-16,0), new Vector2(60,60), new Vector2(1,.5f));
            var coin = Panel("Pill_CoTien", header, SpriteAt("Pill_Currency_Wood"), Color.white);
            Anchor(coin, new Vector2(1,.5f),new Vector2(1,.5f),new Vector2(-96,0),new Vector2(160,48),new Vector2(1,.5f));
            var coinText = Text("Txt_Amount",coin,"0",24,Gold); Stretch(coinText.rectTransform);

            var loadout = Panel("Loadout_Bar", modal, null, new Color(.13f,.105f,.075f));
            Top(loadout, 38, 112, 196, 38);
            var teamTitle = Text("Txt_LoadoutTitle",loadout,"ĐỘI HÌNH XUẤT TRẬN",20,Gold,TextAlignmentOptions.MidlineLeft);
            Top(teamTitle.rectTransform,16,0,30,600);
            var summary = Text("Txt_LoadoutCount",loadout,"Lính 3/4  •  Trụ 1/4",20,new Color(.70f,.72f,.65f),TextAlignmentOptions.MidlineRight);
            Top(summary.rectTransform,600,0,30,16);
            var hero = SlotGroup(loadout,"Group_Hero","TƯỚNG",0,.12f,1);
            var relic = SlotGroup(loadout,"Group_Relic","PHÁP BẢO",.13f,.25f,1);
            var minions = SlotGroup(loadout,"Group_Minions","LÍNH",.28f,.62f,LoadoutRules.Default.MaxMinionCount);
            var towers = SlotGroup(loadout,"Group_Towers","TRỤ",.65f,.99f,LoadoutRules.Default.MaxTowerCount);

            var body = Node("Body_Area",modal); Stretch(body,38,38,34,330);
            var left = Node("Col_Left_Grid",body); left.anchorMin=Vector2.zero; left.anchorMax=new Vector2(.65f,1); left.offsetMin=Vector2.zero;left.offsetMax=new Vector2(-20,0);
            var tabs = Node("TabBar",left); Top(tabs,0,0,62,0);
            var buttons = new Button[4];
            string[] labels={"TƯỚNG","PHÁP BẢO","LÍNH","TRỤ"};
            string[] names={"Tab_Combo","Tab_Fusion","Tab_Minions","Tab_Towers"};
            for(int i=0;i<4;i++)
            {
                buttons[i]=Button(names[i],tabs,SpriteAt("Btn_Tab_Wood_Inactive"),labels[i],24);
                var rt=buttons[i].GetComponent<RectTransform>();rt.anchorMin=new Vector2(i*.25f,0);rt.anchorMax=new Vector2((i+1)*.25f,1);rt.offsetMin=new Vector2(i==0?0:5,0);rt.offsetMax=new Vector2(i==3?0:-5,0);
            }
            var scroll = Node("ScrollView_Cards",left);Stretch(scroll,0,0,0,78);
            var gridScroll=scroll.gameObject.AddComponent<ScrollRect>();gridScroll.horizontal=false;gridScroll.movementType=ScrollRect.MovementType.Clamped;
            var viewport=Panel("Viewport",scroll,null,new Color(.065f,.06f,.05f,.65f));Stretch(viewport);viewport.GetComponent<Image>().raycastTarget=true;viewport.gameObject.AddComponent<RectMask2D>();
            var content=Node("Content",viewport);content.anchorMin=new Vector2(0,1);content.anchorMax=Vector2.one;content.pivot=new Vector2(.5f,1);content.sizeDelta=new Vector2(0,600);
            var grid=content.gameObject.AddComponent<GridLayoutGroup>();grid.constraint=GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=5;grid.cellSize=new Vector2(190,224);grid.spacing=new Vector2(16,20);grid.padding=new RectOffset(8,8,8,16);
            var fit=content.gameObject.AddComponent<ContentSizeFitter>();fit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            gridScroll.viewport=viewport;gridScroll.content=content;gridScroll.scrollSensitivity=32;

            var right=Panel("Col_Right_Detail",body,SpriteAt("Slot_Inventory_Wood_9Slice"),new Color(.75f,.75f,.75f));
            right.anchorMin=new Vector2(.65f,0);right.anchorMax=Vector2.one;right.offsetMin=new Vector2(4,0);right.offsetMax=Vector2.zero;
            var detailHeader=Node("Detail_Header",right);Top(detailHeader,24,20,142,24);
            var icon=Panel("Icon_Card",detailHeader,SpriteAt("Slot_Inventory_Wood_9Slice"),Color.white);
            Anchor(icon,new Vector2(0,1),new Vector2(0,1),Vector2.zero,new Vector2(110,110),new Vector2(0,1));
            var detailIcon=Panel("Artwork",icon,null,Color.white).GetComponent<Image>();Stretch(detailIcon.rectTransform,10,10,10,10);detailIcon.preserveAspect=true;
            var name=Text("Txt_Name",detailHeader,"Chọn thẻ để xem",28,Gold,TextAlignmentOptions.TopLeft);Top(name.rectTransform,130,0,66,0);name.fontStyle=FontStyles.Bold;
            var type=Text("Txt_Type",detailHeader,"",19,new Color(.75f,.73f,.66f),TextAlignmentOptions.TopLeft);Top(type.rectTransform,130,70,32,0);
            var star=Text("Txt_StarBadge",detailHeader,"",20,Gold,TextAlignmentOptions.TopLeft);Top(star.rectTransform,130,104,28,0);
            var rule=Panel("Divider",right,null,new Color(.42f,.34f,.20f));Top(rule,26,176,1,26);
            var descScroll=Node("ScrollView_Description",right);Stretch(descScroll,26,26,224,192);
            var descViewport=Panel("Viewport",descScroll,null,Color.clear);Stretch(descViewport);descViewport.GetComponent<Image>().raycastTarget=true;descViewport.gameObject.AddComponent<RectMask2D>();
            var desc=Text("Txt_Description",descViewport,"Chọn một thẻ bên trái để xem kỹ năng và trang bị.",23,new Color(.89f,.86f,.78f),TextAlignmentOptions.TopLeft);
            var dr=desc.rectTransform;dr.anchorMin=new Vector2(0,1);dr.anchorMax=Vector2.one;dr.pivot=new Vector2(.5f,1);dr.sizeDelta=Vector2.zero;
            desc.lineSpacing=10;desc.richText=true;var df=desc.gameObject.AddComponent<ContentSizeFitter>();df.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var ds=descScroll.gameObject.AddComponent<ScrollRect>();ds.horizontal=false;ds.viewport=descViewport;ds.content=dr;ds.movementType=ScrollRect.MovementType.Clamped;
            var shards=Text("Txt_ShardProgress",right,"",21,new Color(.75f,.79f,.69f),TextAlignmentOptions.MidlineLeft);Bottom(shards.rectTransform,26,26,184,32);
            var fusion=Button("Btn_AlchemyFusion",right,SpriteAt("Btn_Tab_Wood_Inactive"),"TĂNG SAO",21);Bottom(fusion.GetComponent<RectTransform>(),24,24,114,56);fusion.gameObject.AddComponent<CanvasGroup>();
            var equip=Button("Btn_EquipLoadout",right,SpriteAt("Btn_Upgrade_Wood_Sub_9Slice"),"TRANG BỊ",26);Bottom(equip.GetComponent<RectTransform>(),24,24,28,72);
            var status=Text("Txt_EquipStatus",right,"",20,new Color(.70f,.70f,.65f));Bottom(status.rectTransform,24,24,28,72);status.gameObject.SetActive(false);

            var so=new SerializedObject(view);
            Set(so,"_modalContainer",modal);Set(so,"_backButton",close);Set(so,"_dimBackgroundButton",dimButton);Set(so,"_coTienText",coinText);
            Set(so,"_tabComboButton",buttons[0]);Set(so,"_tabFusionButton",buttons[1]);Set(so,"_tabPassivesButton",buttons[2]);Set(so,"_towerTabButton",buttons[3]);
            Set(so,"_tabComboBg",buttons[0].GetComponent<Image>());Set(so,"_tabFusionBg",buttons[1].GetComponent<Image>());Set(so,"_tabPassivesBg",buttons[2].GetComponent<Image>());
            Set(so,"_tabComboTxt",buttons[0].GetComponentInChildren<TextMeshProUGUI>());Set(so,"_tabFusionTxt",buttons[1].GetComponentInChildren<TextMeshProUGUI>());Set(so,"_tabPassivesTxt",buttons[2].GetComponentInChildren<TextMeshProUGUI>());
            Set(so,"_cardsGridContainer",content);Set(so,"_cardSlotPrefab",card);Set(so,"_detailIcon",detailIcon);Set(so,"_detailName",name);Set(so,"_detailType",type);Set(so,"_detailStarBadge",star);Set(so,"_detailDesc",desc);Set(so,"_detailShardProgress",shards);
            Set(so,"_alchemyFusionButton",fusion);Set(so,"_fusionButtonText",fusion.GetComponentInChildren<TextMeshProUGUI>());Set(so,"_equipButton",equip);Set(so,"_equipLabel",equip.GetComponentInChildren<TextMeshProUGUI>());equip.GetComponentInChildren<TextMeshProUGUI>().color = new Color(.23f,.14f,.06f);Set(so,"_equipStatus",status);Set(so,"_loadoutSummary",summary);
            Array(so,"_heroSlots",hero);Array(so,"_relicSlots",relic);Array(so,"_minionSlots",minions);Array(so,"_towerSlots",towers);
            string[] ids={Defense3DCatalog.HERO_THU_SINH,Defense3DCatalog.RELIC_DEP_TO_ONG,Defense3DCatalog.MINION_DAN_BINH,Defense3DCatalog.MINION_PHU_TAP_DICH,Defense3DCatalog.MINION_THAY_LANG,Defense3DCatalog.TOWER_CUNG_TIEN};
            string[] paths={"Assets/Art/UI/HeroAvatars/Avatar_Head_ThuSinh.png","Assets/Art/Weapons/Icon_W_SLIPPER.png","Assets/Art/UI/UpgradeIcons/Icon_P001_Damage.png","Assets/Art/UI/UpgradeIcons/Icon_P006_Speed.png","Assets/Art/UI/UpgradeIcons/Icon_P003_Health.png","Assets/Art/UI/UpgradeIcons/Icon_W001_NoThan.png"};
            var icons=so.FindProperty("_catalogIcons");icons.arraySize=ids.Length;
            for(int i=0;i<ids.Length;i++){var item=icons.GetArrayElementAtIndex(i);item.FindPropertyRelative("Id").stringValue=ids[i];item.FindPropertyRelative("Icon").objectReferenceValue=i >= 2 ? unitIcons[i-2] : AssetDatabase.LoadAssetAtPath<Sprite>(paths[i]);}
            so.ApplyModifiedPropertiesWithoutUndo();
            so=new SerializedObject(layout);Set(so,"_safeArea",safe);Set(so,"_grid",grid);so.ApplyModifiedPropertiesWithoutUndo();
            so=new SerializedObject(presenter);Set(so,"_view",view);Set(so,"_tabActiveSprite",SpriteAt("Btn_Tab_Wood_Active"));Set(so,"_tabInactiveSprite",SpriteAt("Btn_Tab_Wood_Inactive"));Set(so,"_cardSlotWoodSprite",SpriteAt("Slot_Inventory_Wood_9Slice"));Set(so,"_cardSlotSelectedSprite",SpriteAt("Slot_Inventory_Selected_Glow"));so.ApplyModifiedPropertiesWithoutUndo();
            foreach (var text in root.GetComponentsInChildren<TextMeshProUGUI>(true)) text.ForceMeshUpdate(true);
            PrefabUtility.SaveAsPrefabAsset(root.gameObject,"Assets/_Prefabs/UI/CardCodexUI.prefab");
            var saved=PrefabUtility.SaveAsPrefabAsset(root.gameObject,"Assets/Resources/UI/CardCodexUI.prefab");
            Object.DestroyImmediate(root.gameObject);EditorSceneManager.ClosePreviewScene(authoringScene);AssetDatabase.SaveAssets();return saved;
        }

        // Bake portraits from the actual game prefabs; runtime only reads these sprite assets.
        private static Sprite[] BuildUnitIcons()
        {
            string[] prefabs={"Assets/Art/Minions/Vongxuyen/Prefabs/danbinhVisual.prefab","Assets/Art/Minions/Vongxuyen/Prefabs/phutapdichVisual.prefab","Assets/Art/Minions/Vongxuyen/Prefabs/thaylangVisual.prefab","Assets/Resources/Defense3D/DefenseTower3D.prefab"};
            string[] names={"DanBinh","PhuTapDich","ThayLang","ThapCungTien"};
            Directory.CreateDirectory("Assets/Art/UI/Codex");
            var result=new Sprite[4];
            for(int i=0;i<4;i++)
            {
                string path="Assets/Art/UI/Codex/"+names[i]+".png";
                if(!File.Exists(path))
                {
                    var scene=EditorSceneManager.NewPreviewScene();
                    var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabs[i]),scene);
                    foreach(var behaviour in model.GetComponentsInChildren<MonoBehaviour>()) behaviour.enabled=false;
                    var renderers=model.GetComponentsInChildren<Renderer>();
                    var bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
                    var cameraObject=new GameObject("PortraitCamera",typeof(Camera));UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraObject,scene);
                    var camera=cameraObject.GetComponent<Camera>();camera.scene=scene;camera.overrideSceneCullingMask=EditorSceneManager.GetSceneCullingMask(scene);camera.cameraType=CameraType.Preview;camera.orthographic=true;camera.orthographicSize=Mathf.Max(bounds.extents.y,bounds.extents.x)*1.18f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.nearClipPlane=.01f;camera.farClipPlane=100;
                    camera.transform.position=bounds.center+new Vector3(.35f,.2f,-1).normalized*Mathf.Max(5,bounds.size.magnitude*2);camera.transform.LookAt(bounds.center);
                    var lightObject=new GameObject("PortraitLight",typeof(Light));UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(lightObject,scene);var light=lightObject.GetComponent<Light>();light.type=LightType.Directional;light.intensity=1.4f;lightObject.transform.rotation=Quaternion.Euler(40,-30,0);
                    var texture=new RenderTexture(256,256,24,RenderTextureFormat.ARGB32);texture.Create();camera.targetTexture=texture;camera.Render();var previous=RenderTexture.active;RenderTexture.active=texture;var output=new Texture2D(256,256,TextureFormat.RGBA32,false);output.ReadPixels(new Rect(0,0,256,256),0,0);output.Apply();RenderTexture.active=previous;camera.targetTexture=null;File.WriteAllBytes(path,output.EncodeToPNG());Object.DestroyImmediate(output);texture.Release();Object.DestroyImmediate(texture);EditorSceneManager.ClosePreviewScene(scene);
                    AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                    var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=256;importer.SaveAndReimport();
                }
                result[i]=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            return result;
        }

        private static CodexSlotItemView BuildCardPrefab()
        {
            var card=CodexSlotItemView.CreateDynamicSlot(null);
            card.name="CodexCard";
            var root=(RectTransform)card.transform;root.sizeDelta=new Vector2(190,224);
            var box=(RectTransform)root.Find("Box");Stretch(box,0,0,70,0);
            var label=root.Find("Txt_Label").GetComponent<TextMeshProUGUI>();Bottom(label.rectTransform,4,4,0,64);label.fontSize=22;label.enableWordWrapping=true;label.overflowMode=TextOverflowModes.Ellipsis;
            var inner=box.Find("InnerBg").GetComponent<Image>();inner.color=new Color(.09f,.085f,.07f);
            var star=box.Find("Badge_Star").GetComponent<TextMeshProUGUI>();star.fontSize=19;star.rectTransform.sizeDelta=new Vector2(70,28);
            box.Find("Txt_Shards").gameObject.SetActive(false);
            var equipped=Text("Badge_Equipped",box,"ĐANG DÙNG",16,new Color(.63f,.94f,.72f));Bottom(equipped.rectTransform,4,4,4,26);equipped.gameObject.SetActive(false);
            var locked=Text("Badge_Locked",box,"KHÓA",16,new Color(.85f,.79f,.67f));Bottom(locked.rectTransform,4,4,4,26);locked.gameObject.SetActive(false);
            foreach(var text in card.GetComponentsInChildren<TextMeshProUGUI>(true))text.font=Font;
            var oldButton=box.GetComponent<Button>();Object.DestroyImmediate(oldButton);
            var hit=card.gameObject.AddComponent<Image>();hit.color=Color.clear;hit.raycastTarget=true;
            var cardButton=card.gameObject.AddComponent<Button>();cardButton.targetGraphic=box.GetComponent<Image>();
            var so=new SerializedObject(card);Set(so,"_clickButton",cardButton);Set(so,"_equippedBadgeText",equipped);Set(so,"_lockedBadgeText",locked);so.ApplyModifiedPropertiesWithoutUndo();
            var saved=PrefabUtility.SaveAsPrefabAsset(card.gameObject,"Assets/_Prefabs/UI/CodexCard.prefab");Object.DestroyImmediate(card.gameObject);return saved.GetComponent<CodexSlotItemView>();
        }
        private static CodexLoadoutSlotView[] SlotGroup(RectTransform parent,string name,string title,float min,float max,int count)
        {
            var group=Node(name,parent);group.anchorMin=new Vector2(min,0);group.anchorMax=new Vector2(max,1);group.offsetMin=new Vector2(12,12);group.offsetMax=new Vector2(-12,-32);
            var caption=Text("Caption",group,title,19,new Color(.75f,.73f,.66f),TextAlignmentOptions.MidlineLeft);Top(caption.rectTransform,0,0,28,0);
            var slots=new CodexLoadoutSlotView[count];
            for(int i=0;i<count;i++)
            {
                var button=Button("Slot_"+i,group,SpriteAt("Slot_Inventory_Wood_9Slice"),"",18);var rt=button.GetComponent<RectTransform>();rt.anchorMin=new Vector2((float)i/count,0);rt.anchorMax=new Vector2((float)(i+1)/count,1);rt.offsetMin=new Vector2(4,0);rt.offsetMax=new Vector2(-4,-32);
                Object.DestroyImmediate(button.GetComponentInChildren<TextMeshProUGUI>().gameObject);
                var image=Panel("Icon",rt,null,Color.white).GetComponent<Image>();Stretch(image.rectTransform,20,20,38,12);image.preserveAspect=true;
                var empty=Text("Empty",rt,"+",42,new Color(.50f,.45f,.32f));Stretch(empty.rectTransform,4,4,38,8);
                var label=Text("Name",rt,"Thêm",18,new Color(.90f,.86f,.74f));Bottom(label.rectTransform,6,6,6,32);label.enableAutoSizing=true;label.fontSizeMin=14;label.fontSizeMax=18;label.overflowMode=TextOverflowModes.Ellipsis;
                var slot=button.gameObject.AddComponent<CodexLoadoutSlotView>();var so=new SerializedObject(slot);Set(so,"_button",button);Set(so,"_icon",image);Set(so,"_name",label);Set(so,"_empty",empty);so.ApplyModifiedPropertiesWithoutUndo();slots[i]=slot;
            }
            return slots;
        }
        private static RectTransform Node(string name,Transform parent){var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);return (RectTransform)go.transform;}
        private static RectTransform Panel(string name,Transform parent,Sprite sprite,Color color){var rt=Node(name,parent);var img=rt.gameObject.AddComponent<Image>();img.sprite=sprite;img.type=Image.Type.Sliced;img.color=color;img.raycastTarget=false;return rt;}
        private static TextMeshProUGUI Text(string name,Transform parent,string value,float size,Color color,TextAlignmentOptions align=TextAlignmentOptions.Center){var rt=Node(name,parent);var text=rt.gameObject.AddComponent<TextMeshProUGUI>();text.font=Font;text.text=value;text.fontSize=size;text.color=color;text.alignment=align;text.raycastTarget=false;return text;}
        private static Button Button(string name,Transform parent,Sprite sprite,string label,float size){var rt=Panel(name,parent,sprite,Color.white);var image=rt.GetComponent<Image>();image.raycastTarget=true;var button=rt.gameObject.AddComponent<Button>();button.targetGraphic=image;var text=Text("Text",rt,label,size,Gold);Stretch(text.rectTransform,10,10,4,4);return button;}
        private static void Stretch(RectTransform rt,float left=0,float right=0,float bottom=0,float top=0){rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=new Vector2(left,bottom);rt.offsetMax=new Vector2(-right,-top);}
        private static void Top(RectTransform rt,float left,float top,float height,float right){rt.anchorMin=new Vector2(0,1);rt.anchorMax=Vector2.one;rt.pivot=new Vector2(.5f,1);rt.offsetMin=new Vector2(left,-top-height);rt.offsetMax=new Vector2(-right,-top);}
        private static void Bottom(RectTransform rt,float left,float right,float bottom,float height){rt.anchorMin=Vector2.zero;rt.anchorMax=new Vector2(1,0);rt.pivot=new Vector2(.5f,0);rt.offsetMin=new Vector2(left,bottom);rt.offsetMax=new Vector2(-right,bottom+height);}
        private static void Anchor(RectTransform rt,Vector2 min,Vector2 max,Vector2 pos,Vector2 size,Vector2 pivot){rt.anchorMin=min;rt.anchorMax=max;rt.pivot=pivot;rt.anchoredPosition=pos;rt.sizeDelta=size;}
        private static void Set(SerializedObject so,string name,Object value){so.FindProperty(name).objectReferenceValue=value;}
        private static void Array(SerializedObject so,string name,CodexLoadoutSlotView[] values){var p=so.FindProperty(name);p.arraySize=values.Length;for(int i=0;i<values.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=values[i];}
        [MenuItem("ProjectZombie/2. 📱 Mobile UI/1. Sảnh Chính (Meta Menu)/10.1 Cập Nhật Panel_CardCodex Từ Scene Sang Prefab Gốc",priority=113)]
        public static void SaveSceneCardCodexToPrefab(){var view=Object.FindAnyObjectByType<CardCodexView>();if(view==null)return;PrefabUtility.SaveAsPrefabAsset(view.gameObject,"Assets/_Prefabs/UI/CardCodexUI.prefab");PrefabUtility.SaveAsPrefabAsset(view.gameObject,"Assets/Resources/UI/CardCodexUI.prefab");AssetDatabase.SaveAssets();}
    }
}
#endif
