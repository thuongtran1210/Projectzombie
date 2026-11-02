using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ProjectZombie.Core.Architecture;

namespace ProjectZombie.Tests.Editor
{
    public sealed class AppLoadingViewTests
    {
        private AppLoadingView view;
        [SetUp]public void Setup(){view=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AppArchitectureInstaller.PREFAB).GetComponentInChildren<AppLoadingView>(true));}
        [TearDown]public void Teardown(){if(view!=null)Object.DestroyImmediate(view.gameObject);}
        [Test]public void ProgressCannotMoveBackOrRunAheadAndANewTransitionResets()
        {
            view.Begin("Sông Quê",false);view.ReportProgress(.8f,"Trụ");view.ReportProgress(.2f,"Bản đồ");view.ReportProgress(float.NaN,"Invalid");
            Assert.AreEqual(.8f,view.Progress);Assert.LessOrEqual(view.DisplayedProgress,view.Progress);
            view.Begin("Sảnh",true);Assert.AreEqual(0,view.Progress);Assert.AreEqual(0,view.DisplayedProgress);
            Assert.IsTrue(view.GetComponent<CanvasGroup>().blocksRaycasts);
        }
        [Test]public void OnlyExplicitCompletionReachesOneAndReleasesInputCover()
        {
            view.Begin("Sông Quê",false);view.ReportProgress(.98f,"Ready");Assert.Less(view.Progress,1);
            view.CompleteAsync().GetAwaiter().GetResult();Assert.AreEqual(1,view.Progress);Assert.AreEqual(1,view.DisplayedProgress);
            Assert.IsFalse(view.IsVisible);Assert.IsFalse(view.GetComponent<CanvasGroup>().blocksRaycasts);
        }
        [Test]public void ErrorRemainsBlockingAndCannotBeAdvancedByAStaleProgressReport()
        {
            view.Begin("Sông Quê",false);view.ReportProgress(.7f,"Loading");view.ShowError("Đã trở về sảnh");view.ReportProgress(1,"Stale");
            view.CompleteAsync().GetAwaiter().GetResult();
            Assert.IsTrue(view.IsVisible);Assert.AreEqual(0,view.Progress);Assert.IsTrue(view.GetComponent<CanvasGroup>().blocksRaycasts);
            var so=new SerializedObject(view);Assert.IsTrue(((Button)so.FindProperty("_dismiss").objectReferenceValue).gameObject.activeSelf);
            view.Hide();Assert.IsFalse(view.GetComponent<CanvasGroup>().blocksRaycasts);
        }
        [Test]public void AuthoredFillHasASpriteAndSealHasACanvasRenderer()
        {
            var so=new SerializedObject(view);var fill=(Image)so.FindProperty("_fill").objectReferenceValue;
            Assert.IsNotNull(fill.sprite);Assert.AreEqual(Image.Type.Filled,fill.type);
            var seal=(RectTransform)so.FindProperty("_seal").objectReferenceValue;Assert.IsNotNull(seal.GetComponent<CanvasRenderer>());
            foreach(var label in view.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))Assert.IsNotNull(label.font);
        }
    }
}
