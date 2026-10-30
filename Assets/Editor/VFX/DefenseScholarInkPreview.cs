#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Filters;
using NUnit.Framework.Internal.Execution;
using ProjectZombie.Features.Defense3D;

/// <summary>Renders the actual pooled prefabs at fixed session times without modifying a game scene.</summary>
public static class DefenseScholarInkPreview
{
    private const int TILE_SIZE = 384;
    private const string ASSET_FOLDER = "Assets/Features/Defense3D/Effects/Assets/";

    [MenuItem("Tools/VFX Generator/Validate Scholar Ink Signature")]
    public static void Validate()
    {
        RenderSheet();
        // These synchronous NUnit fixtures do not need scene reloads. Avoid TestRunnerApi's
        // save-scene prompt so validation preserves an artist's unsaved scene.
        var runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());
        runner.Load(typeof(DefenseSignatureAuthoringTests).Assembly, new Dictionary<string, object> { { "NumberOfTestWorkers", 0 } });
        var filter = new OrFilter();
        foreach (string name in new[] { "DefenseSignatureAuthoringTests", "DefenseEffectAuthoringTests", "OwnedPoolTests",
            "DefenseSignatureFeedbackTests", "DefenseSignatureInterruptionTests" })
            filter.Add(new FullNameFilter(name));
        var work = WorkItem.CreateWorkItem(runner.LoadedTest, filter);
        work.InitializeContext(new TestExecutionContext { Dispatcher = new MainThreadDispatcher(), IsSingleThreaded = true });
        work.Execute();
        var result = work.Result;
        File.WriteAllText("scratch/ScholarInkValidation.xml", result.ToXml(true).OuterXml);
        File.WriteAllText("scratch/ScholarInkValidation.txt", result.ResultState + "\nPassed: " + result.PassCount +
            "\nFailed: " + result.FailCount + "\nSkipped: " + result.SkipCount);
        Debug.Log("Scholar Ink validation: " + result.ResultState + " (" + result.PassCount + " passed)");
    }

    private sealed class MainThreadDispatcher : IWorkItemDispatcher
    {
        public void Dispatch(WorkItem work) => work.Execute();
        public void CancelRun(bool force) { }
    }

    [MenuItem("Tools/VFX Generator/Preview Scholar Ink Signature")]
    public static void RenderSheet()
    {
        var scene = EditorSceneManager.NewPreviewScene();
        var cameraObject = new GameObject("Scholar Preview Camera");
        SceneManager.MoveGameObjectToScene(cameraObject, scene);
        var camera = cameraObject.AddComponent<Camera>();
        camera.scene = scene;
        camera.transform.SetPositionAndRotation(new Vector3(0, 10, -10), Quaternion.Euler(43f, 0, 0));
        camera.orthographic = true;
        camera.orthographicSize = 5.8f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.16f, .19f, .18f);
        var target = new RenderTexture(TILE_SIZE, TILE_SIZE, 24);
        camera.targetTexture = target;
        var heroAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Characters/ThuSinh/ThuSinhVisual.prefab");
        var hero = Object.Instantiate(heroAsset);
        SceneManager.MoveGameObjectToScene(hero, scene);
        hero.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(0,180,0));
        foreach (var animator in hero.GetComponentsInChildren<Animator>()) animator.enabled = false;
        var lightObject = new GameObject("Preview Key Light");
        SceneManager.MoveGameObjectToScene(lightObject, scene);
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional; light.intensity = 1.5f;
        light.transform.rotation = Quaternion.Euler(45,-25,0);
        var sheet = new Texture2D(TILE_SIZE * 3, TILE_SIZE * 3, TextureFormat.RGB24, false);
        var tile = new Texture2D(TILE_SIZE, TILE_SIZE, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        try
        {
            string[] phases = { "Cast", "Cast", "Cast", "Release", "Release", "Release", "Release", "Release", "Release" };
            float[] progress = { .3f, .65f, .95f, 0f, .25f, .65f, 0f, .25f, .65f };
            for (int i = 0; i < phases.Length; i++)
            {
                var profile = AssetDatabase.LoadAssetAtPath<DefenseEffectProfileSO>(ASSET_FOLDER + "FX_Scholar" + phases[i] + ".asset");
                var instance = Object.Instantiate(profile.Prefab);
                SceneManager.MoveGameObjectToScene(instance.gameObject, scene);
                try
                {
                    instance.Prepare(profile.Color, 12f);
                    instance.Begin();
                    instance.Render(progress[i]);
                    if (i >= 6) instance.transform.Find("Seal").GetComponent<MeshRenderer>().enabled = false;
                    camera.Render();
                    RenderTexture.active = target;
                    tile.ReadPixels(new Rect(0, 0, TILE_SIZE, TILE_SIZE), 0, 0);
                    tile.Apply();
                    sheet.SetPixels(i % 3 * TILE_SIZE, (2 - i / 3) * TILE_SIZE, TILE_SIZE, TILE_SIZE, tile.GetPixels());
                }
                finally { Object.DestroyImmediate(instance.gameObject); }
            }
            sheet.Apply();
            Directory.CreateDirectory("scratch");
            File.WriteAllBytes("scratch/ScholarInkPreview.png", sheet.EncodeToPNG());
            Debug.Log("Scholar Ink preview: scratch/ScholarInkPreview.png (rows: windup / release / release without ground)");
        }
        finally
        {
            RenderTexture.active = previous;
            camera.targetTexture = null;
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(tile);
            Object.DestroyImmediate(sheet);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }
}
#endif

