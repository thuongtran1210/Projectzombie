#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Execution;
using NUnit.Framework.Internal.Filters;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.UI;

/// <summary>Validates warning state transitions and renders the real UGUI badges without switching the user's scene.</summary>
public static class DefenseSpawnWarningPreview
{
    [MenuItem("Tools/ProjectZombie/Validate Spawn Warnings")]
    public static void Validate()
    {
        Render();
        var runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());
        runner.Load(typeof(DefenseSpawnWarningTests).Assembly, new Dictionary<string, object>());
        var filter = new OrFilter();
        foreach (string name in new[] { "DefenseSpawnWarningTests", "DefenseG3StageAndSpawnTests", "ProjectZombie.Tests.Editor.StageSpawnConfigTests" })
            filter.Add(new FullNameFilter(name));
        var work = WorkItem.CreateWorkItem(runner.LoadedTest, filter);
        work.InitializeContext(new TestExecutionContext { Dispatcher = new MainThreadDispatcher(), IsSingleThreaded = true });
        work.Execute();
        var result = work.Result;
        File.WriteAllText("scratch/SpawnWarningValidation.xml", result.ToXml(true).OuterXml);
        File.WriteAllText("scratch/SpawnWarningValidation.txt", result.ResultState + "\nPassed: " + result.PassCount + "\nFailed: " + result.FailCount);
        Debug.Log("Spawn warning validation: " + result.ResultState + " (" + result.PassCount + " passed)");
    }

    private sealed class MainThreadDispatcher : IWorkItemDispatcher
    {
        public void Dispatch(WorkItem work) => work.Execute();
        public void CancelRun(bool force) { }
    }

    public static void Render()
    {
        const int WIDTH = 960, HEIGHT = 280;
        Directory.CreateDirectory("scratch");
        var scene = EditorSceneManager.NewPreviewScene();
        var cameraObject = new GameObject("Warning Preview Camera");
        SceneManager.MoveGameObjectToScene(cameraObject, scene);
        var camera = cameraObject.AddComponent<Camera>();
        camera.scene = scene;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.045f,.07f,.085f);
        camera.transform.position = new Vector3(0,0,-20);
        var target = new RenderTexture(WIDTH, HEIGHT, 24);
        camera.targetTexture = target;
        var texture = new Texture2D(WIDTH, HEIGHT, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        try
        {
            var canvasObject = new GameObject("Warning Preview Canvas", typeof(RectTransform), typeof(Canvas));
            SceneManager.MoveGameObjectToScene(canvasObject, scene);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 10;
            var symbols = new[] { DefenseSpawnWarningSymbol.Armored, DefenseSpawnWarningSymbol.Commander, DefenseSpawnWarningSymbol.Boss,
                DefenseSpawnWarningSymbol.Fish, DefenseSpawnWarningSymbol.Bat, DefenseSpawnWarningSymbol.Elite,
                DefenseSpawnWarningSymbol.Parachute, DefenseSpawnWarningSymbol.Cannon, DefenseSpawnWarningSymbol.Demon };
            for (int row = 0; row < 3; row++)
            for (int column = 0; column < symbols.Length; column++)
            {
                var node = new GameObject(symbols[column].ToString(), typeof(RectTransform), typeof(CanvasRenderer), typeof(DefenseSpawnWarningGraphic));
                node.transform.SetParent(canvas.transform, false);
                var graphic = node.GetComponent<DefenseSpawnWarningGraphic>();
                graphic.raycastTarget = false;
                graphic.rectTransform.anchorMin = graphic.rectTransform.anchorMax = new Vector2(.5f,.5f);
                graphic.rectTransform.sizeDelta = new Vector2(70,70);
                graphic.rectTransform.anchoredPosition = new Vector2((column-4)*100, 85-row*85);
                graphic.Render(.6f, 25f, true, symbols[column], .7f, row == 1, row == 2);
            }
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture.active = target;
            texture.ReadPixels(new Rect(0,0,WIDTH,HEIGHT),0,0);
            texture.Apply();
            File.WriteAllBytes("scratch/EnemySpawnWarningsPreview.png", texture.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active = previous;
            camera.targetTexture = null;
            Object.DestroyImmediate(target); Object.DestroyImmediate(texture);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }
}
#endif
