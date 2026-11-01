#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.UI;
using Object = UnityEngine.Object;

/// <summary>Renders authored target/impact/smoke/tail at the gameplay camera angle in an isolated preview scene.</summary>
public static class DefensePipeSmokePreview
{
    [MenuItem("Tools/VFX Generator/Preview Pipe Smoke Presentation")]
    public static void Render()
    {
        const int width = 640, height = 480;
        var scene = EditorSceneManager.NewPreviewScene();
        var cameraObject = new GameObject("PipePreviewCamera");
        SceneManager.MoveGameObjectToScene(cameraObject, scene);
        var camera = cameraObject.AddComponent<Camera>(); camera.scene = scene;
        camera.transform.SetPositionAndRotation(new Vector3(0, 13, -14), Quaternion.Euler(43, 0, 0));
        camera.orthographic = true; camera.orthographicSize = 7;
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.11f, .16f, .14f);
        var target = new RenderTexture(width, height, 24);
        camera.targetTexture = target;
        var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        var sheet = new Texture2D(width * 2, height * 2, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        SceneManager.MoveGameObjectToScene(floor, scene);
        floor.transform.localScale = Vector3.one * 3;
        var material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        material.color = new Color(.2f, .29f, .23f);
        floor.GetComponent<Renderer>().sharedMaterial = material;
        var hero = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Characters/ThuSinh/ThuSinhVisual.prefab"));
        SceneManager.MoveGameObjectToScene(hero, scene); hero.transform.position = new Vector3(-4, 0, -1);
        foreach (var animator in hero.GetComponentsInChildren<Animator>()) animator.enabled = false;
        var lightObject = new GameObject("PreviewLight"); SceneManager.MoveGameObjectToScene(lightObject, scene);
        var light = lightObject.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.5f;
        light.transform.rotation = Quaternion.Euler(50, -35, 0);
        var profile = AssetDatabase.LoadAssetAtPath<DefenseEffectProfileSO>(DefensePipeSmokeBuilder.PROFILE);
        var instance = Object.Instantiate(profile.Prefab); SceneManager.MoveGameObjectToScene(instance.gameObject, scene);
        var indicator = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Defense3D/DefenseAimIndicator3D.prefab"));
        SceneManager.MoveGameObjectToScene(indicator, scene);
        var aimView = indicator.GetComponent<DefenseAimIndicator3D>();
        try
        {
            for (int index = 0; index < 4; index++)
            {
                aimView.Hide(); instance.gameObject.SetActive(false);
                if (index == 0)
                {
                    DefenseAimResolver.TryResolve(new DefensePoint3(-4, 0, -1), new DefensePoint3(1, 0, 1), 9f, out var aim);
                    aimView.ShowArea(aim, 4f, 9f, false);
                }
                else
                {
                    instance.Prepare(profile.Color, 8f); instance.Begin();
                    instance.transform.position = new Vector3(1, .07f, 1);
                    instance.Render(index == 1 ? .04f : index == 2 ? .42f : .92f);
                }
                camera.Render(); RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0); texture.Apply();
                sheet.SetPixels(index % 2 * width, (1 - index / 2) * height, width, height, texture.GetPixels());
            }
            sheet.Apply(); Directory.CreateDirectory("scratch");
            File.WriteAllBytes("scratch/PipeSmokePreview.png", sheet.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active = previous; camera.targetTexture = null;
            Object.DestroyImmediate(target); Object.DestroyImmediate(texture); Object.DestroyImmediate(sheet); Object.DestroyImmediate(material);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }
}
#endif
