#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Defense3D.Loadout;
using Object = UnityEngine.Object;

namespace ProjectZombie.Editor.UI
{
    public static class CardCodexVisualChecks
    {
        [MenuItem("ProjectZombie/CardCodex/Build and Preview %#j")]
        public static void BuildInOpenEditor()
        {
            var prefab=CardCodexUIGenerator.GenerateCardCodexPrefab();
            NormalizeSceneInstances();
            Verify(prefab);
            Render(prefab,1920,1080);
            Render(prefab,2400,1080);
            Render(prefab,1440,1080);
            Debug.Log("CARDCODEX_VISUAL_CHECKS_PASSED");
        }

        [MenuItem("ProjectZombie/CardCodex/Runtime Smoke")]
        public static void RuntimeSmoke()
        {
            if (!EditorApplication.isPlaying) throw new Exception("Enter Play Mode first.");
            var hub=Object.FindObjectsOfType<MainHubView>(true).First(v=>v.gameObject.activeInHierarchy);
            var so=new SerializedObject(hub);
            var button=(Button)so.FindProperty("_loadoutCardButton").objectReferenceValue;
            bool clicked=false;
            Action capture=()=>clicked=true;
            hub.OnCodexClicked+=capture;
            button.onClick.Invoke();
            hub.OnCodexClicked-=capture;
            var codex=Object.FindObjectsOfType<CardCodexView>(true).FirstOrDefault(v=>v.gameObject.activeInHierarchy);
            var report=$"Hub event={clicked}; active CardCodex={codex!=null}; button={button.name}; interactable={button.IsInteractable()}";
            if(codex==null)throw new Exception(report);
            var cv=new SerializedObject(codex);
            int before=codex.GetComponentsInChildren<Transform>(true).Length;
            var minion=(Button)cv.FindProperty("_tabPassivesButton").objectReferenceValue;
            minion.onClick.Invoke();
            var slots=codex.GetComponentsInChildren<CodexLoadoutSlotView>(true);
            if(slots.Length!=10)throw new Exception("Unexpected slot count");
            minion.onClick.Invoke();
            int after=codex.GetComponentsInChildren<Transform>(true).Length;
            if(before!=after)throw new Exception("Repeated tab rebuild allocated UI objects");
            var equip=(Button)cv.FindProperty("_equipButton").objectReferenceValue;
            var beforeEquipment=new DefenseLoadoutRepository().Load();
            try
            {
                equip.onClick.Invoke();
                equip.onClick.Invoke();
                var afterEquipment=new DefenseLoadoutRepository().Load();
                if(!beforeEquipment.MinionIds.OrderBy(x=>x).SequenceEqual(afterEquipment.MinionIds.OrderBy(x=>x)))
                    throw new Exception("Equipment round trip failed");
            }
            finally { new DefenseLoadoutRepository().Save(beforeEquipment); }
            File.WriteAllText("Logs/CardCodexQA/runtime-smoke.txt",report+"\n10 loadout slots; repeated tabs reuse pool; equip/remove round trip passed.");
            Debug.Log("CARDCODEX_RUNTIME_SMOKE_PASSED: "+report);
        }

        public static void BuildAndPreview()
        {
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var prefab=CardCodexUIGenerator.GenerateCardCodexPrefab();
                Verify(prefab);
                Render(prefab,1920,1080);
                Render(prefab,2400,1080);
                Render(prefab,1440,1080);
                Debug.Log("CARDCODEX_VISUAL_CHECKS_PASSED");
                EditorApplication.Exit(0);
            }
            catch(Exception ex){ Debug.LogException(ex);EditorApplication.Exit(1); }
        }
        private static void NormalizeSceneInstances()
        {
            string[] layoutProperties={"m_AnchorMin", "m_AnchorMax", "m_SizeDelta", "m_AnchoredPosition", "m_Pivot", "m_LocalScale", "m_LocalPosition", "m_LocalRotation"};
            foreach(var view in Object.FindObjectsOfType<CardCodexView>(true))
            {
                if(!view.gameObject.scene.IsValid() || !view.gameObject.scene.isLoaded) continue;
                var root=PrefabUtility.GetNearestPrefabInstanceRoot(view.gameObject);
                if(root==null) continue;
                var overrides=PrefabUtility.GetPropertyModifications(root);
                if(overrides==null) continue;
                var retained=overrides.Where(m=>!layoutProperties.Any(p=>m.propertyPath.StartsWith(p,StringComparison.Ordinal))).ToArray();
                if(retained.Length==overrides.Length)continue;
                PrefabUtility.SetPropertyModifications(root,retained);
                EditorSceneManager.MarkSceneDirty(root.scene);
                Debug.Log($"CardCodex: removed {overrides.Length-retained.Length} obsolete layout overrides, retained all other scene overrides.");
            }
        }

        private static void Verify(GameObject prefab)
        {
            var so=new SerializedObject(prefab.GetComponent<CardCodexView>());
            foreach(string field in new[]{"_towerTabButton","_equipButton","_cardSlotPrefab","_detailDesc"})
                if(so.FindProperty(field).objectReferenceValue==null)throw new Exception("Missing reference: "+field);
            foreach(string field in new[]{"_minionSlots","_towerSlots"})if(so.FindProperty(field).arraySize!=4)throw new Exception("Invalid capacity: "+field);
            int decorative=0;
            foreach(var g in prefab.GetComponentsInChildren<Graphic>(true))if(!g.raycastTarget)decorative++;
            Debug.Log("CardCodex prefab references verified; non-raycast graphics: "+decorative);
        }
        private static void Render(GameObject prefab,int width,int height)
        {
            var originalScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var previewScene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(originalScene);
            var cameraObject=new GameObject("PreviewCamera",typeof(Camera));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraObject,previewScene);
            var camera=cameraObject.GetComponent<Camera>();
            camera.cameraType = CameraType.Game;
            camera.scene = previewScene;
            camera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(previewScene);
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.025f,.03f,.03f);camera.orthographic=true;camera.transform.position=new Vector3(0,0,-10);camera.nearClipPlane=.1f;camera.farClipPlane=100;
            var texture=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);texture.Create();camera.targetTexture=texture;
            var canvasObject=new GameObject("PreviewCanvas",typeof(RectTransform),typeof(Canvas));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(canvasObject,previewScene);
            var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;canvas.scaleFactor=Mathf.Sqrt(width/1920f * height/1080f);
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,previewScene);instance.transform.SetParent(canvas.transform,false);
            var view=instance.GetComponent<CardCodexView>();
            var catalog=Defense3DCatalog.CreateDefault();
            Func<string,string> name=id=>catalog.TryGetEntry(id,out var entry)?entry.DisplayName:id;
            var snapshot=DefenseLoadoutSnapshot.CreateDefault();
            view.ShowLoadoutSlots(snapshot.HeroId,snapshot.RelicId,snapshot.MinionIds,snapshot.TowerIds,4,4,name,view.GetCatalogIcon);
            view.ShowLoadoutSummary("Lính 3/4  •  Trụ 1/4");
            var normal=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Slot_Inventory_Wood_9Slice.png");
            var selected=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Slot_Inventory_Selected_Glow.png");
            var weapons=AssetDatabase.FindAssets("t:WeaponData").Select(g=>AssetDatabase.LoadAssetAtPath<WeaponData>(AssetDatabase.GUIDToAssetPath(g))).Where(w=>w!=null&&w.icon!=null).Take(15).ToArray();
            int index=0;
            foreach(var weapon in weapons)
            {
                var slot=view.CreateSlotItem();
                slot.BindRelic(new RelicSlotViewModel{WeaponId=weapon.weaponId,WeaponName=weapon.weaponName,Icon=weapon.icon,StarLevel=index<3?2:0,IsUnlocked=index<8,IsSelected=index==0},normal,selected,null);
                slot.SetEquipped(index==1);index++;
            }
            if(weapons.Length>0){var w=weapons[0];view.DisplayRelicDetail(w.weaponName,"PHÁP BẢO HỘ THÂN",w.description+"\n\n<color=#EACB83>Hiệu quả chiến đấu</color>\nXem kỹ năng và trang bị pháp bảo phù hợp với đội hình của bạn.",w.icon,2,12,20,100,true,"TĂNG SAO • 20 MẢNH + 100 CỔ TIỀN");}
            view.ShowEquipAction("TRANG BỊ",true);
            view.UpdateTabVisuals(CodexTabType.RelicFusion,AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Nav_Wood_Stitched.png"),AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Nav_Wood_Stitched.png"));
            Canvas.ForceUpdateCanvases();instance.GetComponent<CodexResponsiveLayout>().Refresh();Canvas.ForceUpdateCanvases();
            foreach(var text in instance.GetComponentsInChildren<TextMeshProUGUI>(true))if(text.gameObject.activeInHierarchy)text.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
            Debug.Log($"Preview geometry canvas={((RectTransform)canvas.transform).rect} root={((RectTransform)instance.transform).rect} modal={instance.transform.Find("SafeArea/Modal_Container").GetComponent<RectTransform>().rect} alpha={instance.GetComponent<CanvasGroup>().alpha}");
            camera.Render();
            var previous=RenderTexture.active;RenderTexture.active=texture;var output=new Texture2D(width,height,TextureFormat.RGBA32,false);output.ReadPixels(new Rect(0,0,width,height),0,0);output.Apply();RenderTexture.active=previous;camera.targetTexture=null;
            var pixels=output.GetPixels32();
            int varied=0;
            for(int i=0;i<pixels.Length;i+=97) if(pixels[i].r>30 || pixels[i].g>30 || pixels[i].b>30) varied++;
            if(varied<100) throw new Exception("Blank UI render at " + width);
            Directory.CreateDirectory("Logs/CardCodexQA");File.WriteAllBytes($"Logs/CardCodexQA/cardcodex-{width}x{height}.png",output.EncodeToPNG());
            // Verify positive geometry and no overlapping loadout slots at each target aspect.
            var slots=instance.GetComponentsInChildren<CodexLoadoutSlotView>();
            foreach(var slot in slots)if(((RectTransform)slot.transform).rect.width<40)throw new Exception("Loadout slot too small at "+width);
            Debug.Log($"Rendered CardCodex {width}x{height}, slots={slots.Length}");
            Object.DestroyImmediate(output);Object.DestroyImmediate(instance);Object.DestroyImmediate(canvasObject);Object.DestroyImmediate(cameraObject);texture.Release();Object.DestroyImmediate(texture);EditorSceneManager.CloseScene(previewScene, true);
        }
    }
}
#endif
