#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Project-local, main-thread command bridge. No network or arbitrary code execution.</summary>
[InitializeOnLoad]
public static class ProjectEditorBridge
{
    private static readonly string ROOT = Path.GetFullPath("Library/ProjectEditorBridge");
    private static double _nextPoll;

    [Serializable]
    private sealed class Request
    {
        public string id;
        public string command;
    }

    [Serializable]
    private sealed class Reply
    {
        public string id;
        public bool ok;
        public string result;
        public string error;
    }

    static ProjectEditorBridge()
    {
        Directory.CreateDirectory(ROOT);
        EditorApplication.update += Poll;
        WriteAtomic(Path.Combine(ROOT, "ready.json"), JsonUtility.ToJson(new Reply
        {
            ok = true,
            result = Application.unityVersion + " | " + DateTime.UtcNow.ToString("O")
        }));
    }

    private static void Poll()
    {
        if (EditorApplication.timeSinceStartup < _nextPoll || EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }
        _nextPoll = EditorApplication.timeSinceStartup + 0.5;
        string input = Path.Combine(ROOT, "request.json");
        if (!File.Exists(input))
        {
            return;
        }

        var reply = new Reply();
        try
        {
            var request = JsonUtility.FromJson<Request>(File.ReadAllText(input));
            File.Delete(input);
            if (request == null || !Guid.TryParseExact(request.id, "N", out _))
            {
                throw new ArgumentException("Request id must be a GUID in N format.");
            }
            reply.id = request.id;
            reply.result = Execute(request.command);
            reply.ok = true;
        }
        catch (Exception exception)
        {
            reply.error = exception.ToString();
        }
        WriteAtomic(Path.Combine(ROOT, "response.json"), JsonUtility.ToJson(reply, true));
    }

    private static string Execute(string command)
    {
        switch (command)
        {
            case "status":
                return "Unity " + Application.unityVersion + "\nPlaying: " + EditorApplication.isPlaying
                    + "\nPaused: " + EditorApplication.isPaused + "\nScene: " + SceneManager.GetActiveScene().path
                    + "\nDirty: " + SceneManager.GetActiveScene().isDirty;
            case "install-app-architecture":
                AppArchitectureInstaller.Install();
                return File.ReadAllText("scratch/AppArchitectureInstall.txt");
            case "install-towers":
                DefenseTowerArchitectureInstaller.Install();
                return File.ReadAllText("scratch/TowerArchitectureInstall.txt");
            case "validate-towers":
                DefenseTowerValidation.Validate();
                return File.ReadAllText("scratch/TowerValidation.txt");
            case "smoke-towers":
                DefenseTowerSmoke.Start();
                return "Tower Play Mode smoke scheduled; report: scratch/TowerSmoke.txt";
            case "refresh":
                AssetDatabase.Refresh();
                return "Refresh requested.";
            case "validate-app-architecture":
                AppArchitectureValidation.Validate();
                return File.ReadAllText("scratch/AppArchitectureValidation.txt");
            case "smoke-app-architecture":
                AppArchitectureSmoke.Start();
                return "Play Mode smoke scheduled; results: scratch/AppArchitectureSmoke.txt";
            case "smoke-bootstrap":
                AppArchitectureSmoke.StartFromBootstrap();
                return "Bootstrap Play Mode smoke scheduled; results: scratch/AppArchitectureSmoke.txt";
            case "smoke-direct-map":
                AppArchitectureSmoke.StartFromMap();
                return "Direct-map Play Mode smoke scheduled; results: scratch/AppArchitectureSmoke.txt";
            case "hierarchy":
                var rows = new List<string>();
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    if (!scene.isLoaded) continue;
                    rows.Add("Scene: " + scene.path);
                    foreach (var root in scene.GetRootGameObjects()) Collect(root.transform, "", rows);
                }
                return string.Join("\n", rows);
            case "capture-camera":
                return CaptureCamera();
            case "validate-enemy-health":
                if (EditorApplication.isPlaying) throw new InvalidOperationException("Validation requires Edit Mode.");
                DefenseEnemyHealthValidation.Validate();
                return File.ReadAllText("scratch/EnemyHealthValidation.txt") + "\n" + Path.GetFullPath("scratch/EnemyHealthPreview.png");
            default:
                throw new ArgumentException("Supported commands: status, hierarchy, capture-camera, validate-enemy-health");
        }
    }

    private static void Collect(Transform node, string parent, List<string> rows)
    {
        if (rows.Count >= 5000) return;
        string path = parent + "/" + node.name;
        rows.Add(path + " [active=" + node.gameObject.activeInHierarchy + "]");
        foreach (Transform child in node) Collect(child, path, rows);
    }

    private static string CaptureCamera()
    {
        var camera = Camera.main;
        if (camera == null) throw new InvalidOperationException("No active MainCamera.");
        const int WIDTH = 960;
        const int HEIGHT = 540;
        var target = new RenderTexture(WIDTH, HEIGHT, 24);
        var texture = new Texture2D(WIDTH, HEIGHT, TextureFormat.RGB24, false);
        var previousTarget = camera.targetTexture;
        var previousActive = RenderTexture.active;
        try
        {
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            texture.ReadPixels(new Rect(0, 0, WIDTH, HEIGHT), 0, 0);
            texture.Apply();
            string output = Path.Combine(ROOT, "camera.png");
            File.WriteAllBytes(output, texture.EncodeToPNG());
            return output;
        }
        finally
        {
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            UnityEngine.Object.DestroyImmediate(texture);
            UnityEngine.Object.DestroyImmediate(target);
        }
    }

    private static void WriteAtomic(string path, string contents)
    {
        string temporary = path + ".tmp";
        File.WriteAllText(temporary, contents);
        if (File.Exists(path)) File.Delete(path);
        File.Move(temporary, path);
    }
}
#endif
