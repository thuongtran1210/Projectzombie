#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.Defense3D.UI;
using ProjectZombie.Features.Defense3D.Loadout;
using Object = UnityEngine.Object;

namespace ProjectZombie.Editor.UI
{
    /// <summary>Editor-only authoring of the saved-loadout review. No runtime hierarchy construction.</summary>
    internal static class DefensePreparationUIStyler
    {
        private static readonly Color TextColor = new Color(.91f,.87f,.77f);
        private static readonly Color DarkText = new Color(.23f,.14f,.06f);
        private static Sprite Art(string name) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/" + name + ".png");
        public static void Apply(GameObject root)
        {
            var view = root.GetComponent<DefensePreparationView>();
            var serialized = new SerializedObject(view);
            var frame = (RectTransform)root.transform.Find("Frame_Defense_Content");
            var viewport = new GameObject("Defense_Viewport",typeof(RectTransform)).GetComponent<RectTransform>();
            viewport.SetParent(root.transform,false);
            viewport.anchorMin=new Vector2(.05f,.06f);viewport.anchorMax=new Vector2(.95f,.94f);
            viewport.offsetMin=viewport.offsetMax=Vector2.zero;
            frame.SetParent(viewport,false);frame.sizeDelta=new Vector2(1360,840);
            var aspect=frame.gameObject.AddComponent<AspectRatioFitter>();aspect.aspectRatio=1360f/840f;aspect.aspectMode=AspectRatioFitter.AspectMode.FitInParent;
            Place(frame.Find("Banner_Header"),24,24,1312,76,1360,840);
            frame.Find("Banner_Header").GetComponent<Image>().sprite=Art("Btn_Nav_Wood_Stitched");
            Place(frame.Find("Txt_Subtitle"),48,112,1264,32,1360,840);
            Place(frame.Find("Txt_ValidationMessage"),48,672,1264,48,1360,840);
            Place(frame.Find("Btn_StartBattle"),952,742,360,64,1360,840);
            Place(frame.Find("Btn_Close_X"),1256,36,56,56,1360,840);
            string[] cards={"Card_Hero","Card_Relic","Card_Minions","Card_Towers"};
            string[] pills={"Pill_Hero","Pill_Relic","Pill_Minions","Pill_Towers"};
            string[] captions={"TƯỚNG","PHÁP BẢO","LÍNH ĐÃ CHỌN","TRỤ ĐÃ CHỌN"};
            float[] xs={40,324,608,972};
            for(int i=0;i<4;i++)
            {
                var card=frame.Find(cards[i]);float width=i<2?264:344;
                Place(card,xs[i],164,width,480,1360,840);
                card.GetComponent<Image>().sprite=Art("Slot_Inventory_Wood_9Slice");
                var pill=card.Find(pills[i]);Place(pill,12,18,width-24,40,width,480);
                var text=pill.GetComponentInChildren<TextMeshProUGUI>();text.text=captions[i];text.color=DarkText;
            }
            foreach(string boxName in new[]{"Card_Hero/Box_Avatar","Card_Relic/Box_RelicIcon"})
            {
                var box=frame.Find(boxName);Place(box,60,86,144,144,264,480);
                Place(box.GetChild(0),10,10,124,124,144,144);
            }
            Place(frame.Find("Card_Hero/Txt_HeroName"),16,254,232,68,264,480);
            Place(frame.Find("Card_Hero/Txt_HeroRole"),16,332,232,80,264,480);
            Place(frame.Find("Card_Relic/Txt_RelicName"),16,254,232,68,264,480);
            Place(frame.Find("Card_Relic/Txt_RelicDesc"),16,332,232,112,264,480);
            foreach(string name in new[]{"Card_Hero/Btn_HeroSlot","Card_Relic/Btn_RelicSlot","Card_Minions/Txt_MinionHint","Card_Towers/Txt_TowerHint","Card_Towers/Txt_TowerDesc"})frame.Find(name).gameObject.SetActive(false);
            var minionCount=frame.Find("Card_Minions/Txt_MinionSub").GetComponent<TextMeshProUGUI>();
            var towerCount=frame.Find("Card_Towers/Txt_TowerSub").GetComponent<TextMeshProUGUI>();
            Place(minionCount.transform,16,62,312,26,344,480);Place(towerCount.transform,16,62,312,26,344,480);
            minionCount.text=$"3/{LoadoutRules.Default.MaxMinionCount} loại đã chọn";
            towerCount.text=$"1/{LoadoutRules.Default.MaxTowerCount} loại đã chọn";
            Set(serialized,"_minionCountText",minionCount);Set(serialized,"_towerCountText",towerCount);
            var minionIcons=BuildRows(serialized,"_minion",frame.Find("Card_Minions"),LoadoutRules.Default.MaxMinionCount);
            var towerIcons=BuildRows(serialized,"_tower",frame.Find("Card_Towers"),LoadoutRules.Default.MaxTowerCount);
            var start=(Button)serialized.FindProperty("_startBattleButton").objectReferenceValue;
            start.GetComponent<Image>().sprite=Art("Btn_Upgrade_Wood_Sub_9Slice");
            var edit=Object.Instantiate(start,start.transform.parent);edit.name="Btn_EditLoadout";
            edit.onClick=new Button.ButtonClickedEvent();edit.GetComponent<Image>().sprite=Art("Btn_Nav_Wood_Stitched");
            Place(edit.transform,48,742,360,64,1360,840);Set(serialized,"_editLoadoutButton",edit);
            foreach(var text in root.GetComponentsInChildren<TextMeshProUGUI>(true)){text.fontSize=24;text.color=TextColor;text.raycastTarget=false;}
            foreach(var text in root.GetComponentsInChildren<TextMeshProUGUI>(true))
                if(text.text=="Chưa chọn")text.color=new Color(.56f,.54f,.48f);
            foreach(string name in new[]{"Card_Hero/Txt_HeroName","Card_Relic/Txt_RelicName","Card_Relic/Txt_RelicDesc","Txt_ValidationMessage"})
            {
                var text=frame.Find(name).GetComponent<TextMeshProUGUI>();text.enableAutoSizing=true;text.fontSizeMin=20;text.fontSizeMax=28;
            }
            foreach(var image in root.GetComponentsInChildren<Image>(true))image.raycastTarget=image.transform==frame || image.name=="Overlay_Dark" || image.name=="Loading_Overlay" || image.GetComponent<Button>()!=null && image.GetComponent<Button>().interactable;
            for(int i=0;i<4;i++)frame.Find(cards[i]+"/"+pills[i]).GetComponentInChildren<TextMeshProUGUI>().color=DarkText;
            var title=frame.Find("Banner_Header/Txt_Title").GetComponent<TextMeshProUGUI>();title.text="ĐỘI HÌNH XUẤT TRẬN";title.fontSize=36;title.color=new Color(.96f,.82f,.48f);
            frame.Find("Txt_Subtitle").GetComponent<TextMeshProUGUI>().text="Kiểm tra đội hình trước khi bắt đầu.";
            frame.Find("Card_Hero/Txt_HeroName").GetComponent<TextMeshProUGUI>().text="Thư Sinh";
            frame.Find("Card_Hero/Txt_HeroRole").GetComponent<TextMeshProUGUI>().text="Tướng điều khiển trong trận";
            frame.Find("Card_Relic/Txt_RelicName").GetComponent<TextMeshProUGUI>().text="Dép Tổ Ong";
            frame.Find("Card_Relic/Txt_RelicDesc").GetComponent<TextMeshProUGUI>().text="Pháp bảo đã trang bị cho tướng khi xuất trận.";
            frame.Find("Txt_ValidationMessage").GetComponent<TextMeshProUGUI>().text="Đội hình sẵn sàng xuất trận.";
            frame.Find("Txt_ValidationMessage").GetComponent<TextMeshProUGUI>().color=new Color(.63f,.80f,.66f);
            var startLabel=start.GetComponentInChildren<TextMeshProUGUI>();startLabel.text="XUẤT TRẬN";startLabel.fontSize=28;startLabel.color=DarkText;
            var editLabel=edit.GetComponentInChildren<TextMeshProUGUI>();editLabel.text="CHỈNH ĐỘI HÌNH";editLabel.fontSize=26;
            string[] ids={"hero_thusinh","relic_deptoong","minion_danbinh","minion_phutapdich","minion_thaylang","tower_cungtien"};
            string[] paths={"Assets/Art/UI/HeroAvatars/Avatar_Head_ThuSinh.png","Assets/Art/Weapons/Icon_W_SLIPPER.png","Assets/Art/UI/Codex/DanBinh.png","Assets/Art/UI/Codex/PhuTapDich.png","Assets/Art/UI/Codex/ThayLang.png","Assets/Art/UI/Codex/ThapCungTien.png"};
            var icons=serialized.FindProperty("_catalogIcons");icons.arraySize=ids.Length;
            for(int i=0;i<ids.Length;i++)
            {
                var icon=AssetDatabase.LoadAssetAtPath<Sprite>(paths[i]);var entry=icons.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("Id").stringValue=ids[i];entry.FindPropertyRelative("Icon").objectReferenceValue=icon;
                if(i<2){var image=(Image)serialized.FindProperty(i==0?"_heroAvatarImage":"_relicIconImage").objectReferenceValue;image.sprite=icon;image.preserveAspect=true;}
                else if(i<5 && i-2<minionIcons.Length)minionIcons[i-2].sprite=icon;
                else if(i==5 && towerIcons.Length>0)towerIcons[0].sprite=icon;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        private static Image[] BuildRows(SerializedObject serialized,string prefix,Transform card,int count)
        {
            var buttonProperty=serialized.FindProperty(prefix+"SlotButtons");
            var iconProperty=serialized.FindProperty(prefix+"SlotCheckmarks");
            var nameProperty=serialized.FindProperty(prefix+"SlotNameTexts");
            var original=new Button[buttonProperty.arraySize];for(int i=0;i<original.Length;i++)original[i]=(Button)buttonProperty.GetArrayElementAtIndex(i).objectReferenceValue;
            buttonProperty.arraySize=iconProperty.arraySize=nameProperty.arraySize=count;
            // Clone before moving any icon out of its original checkbox container.
            var rows=new Button[count];
            for(int i=0;i<count;i++)rows[i]=i<original.Length?original[i]:Object.Instantiate(original[0],card);
            var result=new Image[count];float step=336f/Mathf.Max(1,count);
            string[] minions={"Dân Binh","Phu Tạp Dịch","Thầy Lang"};
            for(int i=0;i<count;i++)
            {
                var button=rows[i];
                button.name=(prefix=="_minion"?"Slot_Minion_":"Slot_Tower_")+i;
                button.transition=Selectable.Transition.None;button.interactable=false;
                Place(button.transform,16,100+i*step,312,step-8,344,480);
                var box=button.transform.Find("Box_Check");var icon=box.Find("Img_Checkmark").GetComponent<Image>();
                icon.transform.SetParent(button.transform,false);box.gameObject.SetActive(false);
                Place(icon.transform,14,12,52,52,312,step-8);icon.preserveAspect=true;
                bool occupied=prefix=="_minion"?i<3:i==0;icon.gameObject.SetActive(occupied);
                var name=button.transform.Find("Txt_SlotName").GetComponent<TextMeshProUGUI>();
                Place(name.transform,80,8,220,step-24,312,step-8);
                name.text=occupied?(prefix=="_minion"?minions[i]:"Tháp Cung Tiễn"):"Chưa chọn";
                buttonProperty.GetArrayElementAtIndex(i).objectReferenceValue=button;
                iconProperty.GetArrayElementAtIndex(i).objectReferenceValue=icon;
                nameProperty.GetArrayElementAtIndex(i).objectReferenceValue=name;result[i]=icon;
            }
            return result;
        }
        private static void Set(SerializedObject so,string field,Object value)=>so.FindProperty(field).objectReferenceValue=value;
        private static void Place(Transform target,float x,float y,float width,float height,float parentWidth,float parentHeight)
        {
            var rect=(RectTransform)target;rect.anchorMin=new Vector2(x/parentWidth,1-(y+height)/parentHeight);
            rect.anchorMax=new Vector2((x+width)/parentWidth,1-y/parentHeight);rect.anchoredPosition=rect.sizeDelta=Vector2.zero;
        }
    }
}
#endif
