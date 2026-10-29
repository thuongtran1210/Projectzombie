#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ProjectZombie.Features.Defense3D.UI;
using Object = UnityEngine.Object;

namespace ProjectZombie.EditorTools.Defense3D
{
    /// <summary>Authors recruitment and squad orders into separate UI roots, retaining the existing button events.</summary>
    internal static class DefenseVillageShopUIStyler
    {
        private static Sprite Art(string name) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/"+name+".png");
        private static readonly string[] Names={"Dân Binh","Phu Tạp Dịch","Thầy Lang"};
        private static readonly string[] Files={"DanBinh","PhuTapDich","ThayLang"};
        private static readonly string[] States={"Thủ nhà","Gom gần nhà","Trạm y tế"};
        public static void Apply(DefenseVillageShopView view)
        {
            var so=new SerializedObject(view);
            var panel=((GameObject)so.FindProperty("_panel").objectReferenceValue).GetComponent<RectTransform>();
            var safe=(RectTransform)so.FindProperty("_safeArea").objectReferenceValue;
            var font=((TMP_Text)so.FindProperty("_wood").objectReferenceValue).font;
            Rect(panel,-16,-72,380,366,Vector2.one,Vector2.one);
            Style(panel.GetComponent<Image>(),Art("Frame_Modal_TangBaoCac_9Slice"),true,true);
            var title=panel.Find("Title").GetComponent<TMP_Text>();Rect(title.rectTransform,-18,-14,282,34);Text(title,"CHIÊU MỘ LÍNH",23);
            var wood=(TMP_Text)so.FindProperty("_wood").objectReferenceValue;Rect(wood.rectTransform,0,-52,340,28);Text(wood,"Gỗ: 100",21);
            var status=(TMP_Text)so.FindProperty("_status").objectReferenceValue;Rect(status.rectTransform,0,-314,340,38);Text(status,"Chọn lính để chiêu mộ.",18);status.fontSizeMin=14;
            var close=Object.Instantiate((Button)so.FindProperty("_recruit").GetArrayElementAtIndex(0).objectReferenceValue,panel);
            close.name="CloseShop";close.onClick=new Button.ButtonClickedEvent();Rect((RectTransform)close.transform,-10,-10,44,44,Vector2.one,Vector2.one);
            Style(close.GetComponent<Image>(),Art("Btn_Nav_Wood_Stitched"),true,true);
            var closeLabel=close.GetComponentInChildren<TMP_Text>();Rect(closeLabel.rectTransform,0,0,44,44);Text(closeLabel,"×",28);Set(so,"_close",close);
            var dock=Node("MinionCommandDock",safe);Rect(dock,40,20,408,184,new Vector2(.5f,0),new Vector2(.5f,0));
            var layout=dock.gameObject.AddComponent<HorizontalLayoutGroup>();layout.spacing=12;layout.childAlignment=TextAnchor.UpperCenter;
            layout.childControlWidth=layout.childControlHeight=layout.childForceExpandWidth=layout.childForceExpandHeight=false;
            var fitter=dock.gameObject.AddComponent<ContentSizeFitter>();fitter.horizontalFit=ContentSizeFitter.FitMode.PreferredSize;
            Set(so,"_commandDock",dock.gameObject);
            string[] arrayNames={"_commandGroups","_orderIcons","_recruitIcons","_orderCounts"};
            foreach(string name in arrayNames)so.FindProperty(name).arraySize=3;
            for(int role=0;role<3;role++)
            {
                var portrait=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Codex/"+Files[role]+".png");
                var recruit=(Button)so.FindProperty("_recruit").GetArrayElementAtIndex(role).objectReferenceValue;
                Rect((RectTransform)recruit.transform,0,-88-role*74,356,68);Style(recruit.GetComponent<Image>(),Art("Btn_Nav_Wood_Stitched"),true,true);
                var recruitLabel=(TMP_Text)so.FindProperty("_recruitLabels").GetArrayElementAtIndex(role).objectReferenceValue;
                Rect(recruitLabel.rectTransform,78,-4,270,60,new Vector2(0,1),new Vector2(0,1));Text(recruitLabel,Names[role],22);
                var recruitIcon=Picture("RecruitPortrait",recruit.transform,portrait);Rect(recruitIcon.rectTransform,12,-7,54,54,new Vector2(0,1),new Vector2(0,1));
                var tile=Node("Command_"+Files[role],dock);Rect(tile,0,0,128,184);
                var button=(Button)so.FindProperty("_orders").GetArrayElementAtIndex(role).objectReferenceValue;
                button.transform.SetParent(tile,false);Rect((RectTransform)button.transform,0,0,104,104);
                var ring=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_Circle_Relic.png");Style(button.GetComponent<Image>(),ring,true,false);
                var order=(TMP_Text)so.FindProperty("_orderLabels").GetArrayElementAtIndex(role).objectReferenceValue;
                order.transform.SetParent(tile,false);Rect(order.rectTransform,0,-136,128,44);Text(order,States[role],18);
                var name=Label("RoleName",tile,font,Names[role],19);Rect(name.rectTransform,0,-108,128,26);
                var icon=Picture("UnitPortrait",button.transform,portrait);Rect(icon.rectTransform,0,-17,70,70);
                var badge=Picture("CountBadge",button.transform,AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Mask_Circle_Solid.png"));
                Rect(badge.rectTransform,0,0,30,30,Vector2.one,Vector2.one);badge.color=new Color(.10f,.075f,.045f,.95f);
                var count=Label("Count",badge.transform,font,"0",20);Rect(count.rectTransform,0,0,30,30);
                Item(so,"_commandGroups",role,tile.gameObject);Item(so,"_orderIcons",role,icon);Item(so,"_recruitIcons",role,recruitIcon);Item(so,"_orderCounts",role,count);
            }
            foreach(string field in new[]{"_build","_upgrade"})((Button)so.FindProperty(field).objectReferenceValue).gameObject.SetActive(false);
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        private static RectTransform Node(string name,Transform parent)
        {
            var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent,false);return rect;
        }
        private static Image Picture(string name,Transform parent,Sprite sprite)
        {
            var image=Node(name,parent).gameObject.AddComponent<Image>();Style(image,sprite,false,false);return image;
        }
        private static TMP_Text Label(string name,Transform parent,TMP_FontAsset font,string text,int size)
        {
            var label=Node(name,parent).gameObject.AddComponent<TextMeshProUGUI>();label.font=font;Text(label,text,size);return label;
        }
        private static void Text(TMP_Text text,string value,int size)
        {
            text.text=value;text.fontSize=size;text.enableAutoSizing=true;text.fontSizeMin=Mathf.Min(18,size);text.fontSizeMax=size;
            text.alignment=TextAlignmentOptions.Center;text.color=new Color(.91f,.87f,.77f);text.raycastTarget=false;
        }
        private static void Style(Image image,Sprite sprite,bool raycast,bool sliced)
        {
            image.sprite=sprite;image.type=sliced?Image.Type.Sliced:Image.Type.Simple;image.preserveAspect=!sliced;image.color=Color.white;image.raycastTarget=raycast;
        }
        private static void Rect(RectTransform rect,float x,float y,float width,float height,Vector2? anchor=null,Vector2? pivot=null)
        {
            rect.anchorMin=rect.anchorMax=anchor??new Vector2(.5f,1);rect.pivot=pivot??new Vector2(.5f,1);rect.anchoredPosition=new Vector2(x,y);rect.sizeDelta=new Vector2(width,height);
        }
        private static void Set(SerializedObject so,string field,Object value)=>so.FindProperty(field).objectReferenceValue=value;
        private static void Item(SerializedObject so,string field,int index,Object value)=>so.FindProperty(field).GetArrayElementAtIndex(index).objectReferenceValue=value;
    }
}
#endif
