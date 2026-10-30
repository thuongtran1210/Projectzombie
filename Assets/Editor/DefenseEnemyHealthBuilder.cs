#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.UI;

/// <summary>Authors pooled enemy health views on the actual enemy definitions without editing open scenes.</summary>
public static class DefenseEnemyHealthBuilder
{
    private const string MATERIAL_PATH = "Assets/Features/Defense3D/Effects/Assets/MAT_EnemyHealth.mat";

    [MenuItem("Tools/ProjectZombie/Build Enemy Health Bars")]
    public static void Build()
    {
        var shader = Shader.Find("ProjectZombie/Defense3D/EnemyHealth");
        if (shader == null) throw new InvalidOperationException("Import EnemyHealth shader first.");
        var material = AssetDatabase.LoadAssetAtPath<Material>(MATERIAL_PATH);
        if (material == null)
        {
            material = new Material(shader) { name = "MAT_EnemyHealth" };
            AssetDatabase.CreateAsset(material, MATERIAL_PATH);
        }
        material.shader = shader;
        EditorUtility.SetDirty(material);
        var updated = new HashSet<string>();
        foreach (string guid in AssetDatabase.FindAssets("t:DefenseEnemyDataSO", new[] { "Assets/Resources/Defense3D/Enemies" }))
        {
            var definition = AssetDatabase.LoadAssetAtPath<DefenseEnemyDataSO>(AssetDatabase.GUIDToAssetPath(guid));
            string path = AssetDatabase.GetAssetPath(definition.Prefab);
            if (!updated.Add(path)) continue;
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var anchors = root.GetComponent<DefenseActorAnchors3D>();
                Transform parent = anchors != null && anchors.Head != null ? anchors.Head : root.transform;
                var presenter = root.GetComponent<DefenseEnemyHealthPresenter3D>();
                if (presenter == null) presenter = root.AddComponent<DefenseEnemyHealthPresenter3D>();
                var node = parent.Find("EnemyHealthBar");
                if (node == null)
                {
                    var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    UnityEngine.Object.DestroyImmediate(quad.GetComponent<Collider>());
                    quad.name = "EnemyHealthBar";
                    quad.transform.SetParent(parent, false);
                    node = quad.transform;
                }
                var renderer = node.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                renderer.enabled = false;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                var view = node.GetComponent<DefenseEnemyHealthView3D>();
                if (view == null) view = node.gameObject.AddComponent<DefenseEnemyHealthView3D>();
                var viewData = new SerializedObject(view);
                viewData.FindProperty("_renderer").objectReferenceValue = renderer;
                viewData.ApplyModifiedPropertiesWithoutUndo();
                var presenterData = new SerializedObject(presenter);
                presenterData.FindProperty("_view").objectReferenceValue = view;
                presenterData.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Authored enemy health bars on " + updated.Count + " enemy prefabs.");
        DefenseEnemyHealthValidation.Validate();
    }
}
#endif
