#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Clears validator-induced dirty flags only after proving every serialized difference is a driven layout value.</summary>
public static class DefenseValidationSceneCleanup
{
    private static readonly PropertyInfo DRIVEN = typeof(RectTransform).GetProperty("drivenProperties", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    private static readonly Regex MODIFICATION = new Regex(@"(- target: \{fileID: (?<id>\d+), guid: (?<guid>[a-f0-9]+), type: \d+\}\s*\n\s*propertyPath: (?<path>m_(?:Anchor(?:Min|Max)|AnchoredPosition|SizeDelta)\.[xy])\s*\n\s*value: )(?<value>[^\r\n]*)");

    public static string ClearDrivenLayoutOnly()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/MainMenu.unity" || EditorApplication.isPlaying)
            throw new InvalidOperationException("This recovery is scoped to the known validation scene in Edit Mode.");
        const string snapshot = "scratch/EditorSceneValidationSnapshot.unity";
        if (!EditorSceneManager.SaveScene(scene, snapshot, true)) throw new IOException("Cannot preserve scene snapshot.");
        string original = File.ReadAllText(scene.path), current = File.ReadAllText(snapshot);
        if (Normalize(original) != Normalize(current))
            throw new InvalidOperationException("Scene has changes beyond layout values. It has been preserved and will not be marked clean.");
        var before = new Dictionary<string, HashSet<string>>();
        foreach (Match match in MODIFICATION.Matches(original))
        {
            string key = Key(match);
            if (!before.TryGetValue(key, out var values)) { values = new HashSet<string>(); before.Add(key, values); }
            values.Add(match.Groups["value"].Value);
        }
        var rectangles = new Dictionary<string, List<RectTransform>>();
        foreach (var root in scene.GetRootGameObjects())
            foreach (var rect in root.GetComponentsInChildren<RectTransform>(true))
            {
                Add(rectangles, rect, PrefabUtility.GetCorrespondingObjectFromSource(rect));
                Add(rectangles, rect, PrefabUtility.GetCorrespondingObjectFromOriginalSource(rect));
            }
        int changed = 0;
        foreach (Match match in MODIFICATION.Matches(current))
        {
            if (!before.TryGetValue(Key(match), out var values) || values.Count != 1)
                throw new InvalidOperationException("Ambiguous layout overrides; scene preserved.");
            if (values.Contains(match.Groups["value"].Value)) continue;
            string target = match.Groups["guid"].Value + ":" + match.Groups["id"].Value;
            if (!rectangles.TryGetValue(target, out var instances) || instances.Any(rect => rect.drivenByObject == null ||
                    (Driven(rect) & Flag(match.Groups["path"].Value)) == 0))
                throw new InvalidOperationException("A changed layout value is not driven by Unity layout; scene preserved.");
            changed++;
        }
        var clear = typeof(EditorSceneManager).GetMethod("ClearSceneDirtiness", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
            null, new[] { typeof(Scene) }, null);
        if (clear == null) throw new InvalidOperationException("Unity does not expose scene dirty-flag recovery; snapshot retained.");
        clear.Invoke(null, new object[] { scene });
        return "Verified only " + changed + " driven layout values differ. Dirty flag cleared; scene content retained, no scene save/reload. Snapshot: " + Path.GetFullPath(snapshot);
    }

    private static string Key(Match match) => match.Groups["guid"].Value + ":" + match.Groups["id"].Value + ":" + match.Groups["path"].Value;
    private static DrivenTransformProperties Driven(RectTransform rect)
    {
        if (DRIVEN == null) throw new InvalidOperationException("Cannot inspect driven properties on this Unity version; scene preserved.");
        return (DrivenTransformProperties)DRIVEN.GetValue(rect);
    }
    private static string Normalize(string value)
    {
        value = MODIFICATION.Replace(value, match => match.Groups[1].Value + "<driven-layout>");
        return string.Join("\n", value.Replace("\r\n", "\n").Split('\n').Select(line => line.TrimEnd()));
    }
    private static void Add(Dictionary<string, List<RectTransform>> targets, RectTransform instance, RectTransform source)
    {
        if (source == null || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(source, out string guid, out long id)) return;
        string key = guid + ":" + id;
        if (!targets.TryGetValue(key, out var values)) { values = new List<RectTransform>(); targets.Add(key, values); }
        if (!values.Contains(instance)) values.Add(instance);
    }
    private static DrivenTransformProperties Flag(string property)
    {
        switch (property)
        {
            case "m_AnchorMin.x": return DrivenTransformProperties.AnchorMinX;
            case "m_AnchorMin.y": return DrivenTransformProperties.AnchorMinY;
            case "m_AnchorMax.x": return DrivenTransformProperties.AnchorMaxX;
            case "m_AnchorMax.y": return DrivenTransformProperties.AnchorMaxY;
            case "m_AnchoredPosition.x": return DrivenTransformProperties.AnchoredPositionX;
            case "m_AnchoredPosition.y": return DrivenTransformProperties.AnchoredPositionY;
            case "m_SizeDelta.x": return DrivenTransformProperties.SizeDeltaX;
            default: return DrivenTransformProperties.SizeDeltaY;
        }
    }
}
#endif
