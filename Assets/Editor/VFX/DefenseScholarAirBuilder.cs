#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using ProjectZombie.Features.Defense3D;

/// <summary>Bakes curved brush ribbons into pooled prefabs; no mesh generation happens during combat.</summary>
public static class DefenseScholarAirBuilder
{
    private const string FOLDER = "Assets/Features/Defense3D/Effects/Assets/";
    private const int SEGMENTS = 64;

    [MenuItem("Tools/VFX Generator/Build Scholar Air Signature")]
    public static void Build()
    {
        var shader = Shader.Find("ProjectZombie/Defense3D/ScholarBrush");
        if (shader == null) throw new InvalidOperationException("ScholarBrush shader is not imported.");
        var material = AssetDatabase.LoadAssetAtPath<Material>(FOLDER + "MAT_ScholarBrush.mat");
        if (material == null)
        {
            material = new Material(shader) { name = "MAT_ScholarBrush" };
            AssetDatabase.CreateAsset(material, FOLDER + "MAT_ScholarBrush.mat");
        }
        material.shader = shader;
        EditorUtility.SetDirty(material);
        foreach (string phase in new[] { "Cast", "Release", "Hit" }) BuildPhase(phase, material);
        AssetDatabase.SaveAssets();
        DefenseScholarInkPreview.Validate();
    }

    private static void BuildPhase(string phase, Material material)
    {
        string path = FOLDER + "VFX_Scholar" + phase + "_VongXuyen.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var old = root.transform.Find("AirBrush");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var air = new GameObject("AirBrush");
            air.transform.SetParent(root.transform, false);
            var visual = root.GetComponent<DefenseScholarInkVisual3D>();
            var serialized = new SerializedObject(visual);
            int count = phase == "Cast" ? 2 : phase == "Release" ? 3 : 0;
            var ribbons = serialized.FindProperty("_airRenderers");
            ribbons.arraySize = count;
            for (int i = 0; i < count; i++)
            {
                int index = i;
                bool casting = phase == "Cast";
                Func<float, Vector3> curve = u =>
                {
                    if (casting)
                    {
                        float angle = u * Mathf.PI * 1.7f + index * 2.2f;
                        float radius = 1.35f - index * .22f;
                        return new Vector3(Mathf.Cos(angle) * radius, .65f + u * 1.35f + index * .2f, Mathf.Sin(angle) * radius);
                    }
                    if (index == 1)
                        return new Vector3(Mathf.Lerp(-2.4f,2.7f,u), .45f+3f*u, .45f+Mathf.Sin(u*Mathf.PI)*.5f);
                    return new Vector3(Mathf.Lerp(-3.8f,3.8f,u),
                        Mathf.Lerp(2.8f,.65f,u)+Mathf.Sin(u*Mathf.PI)*.85f-index*.35f,
                        -.9f+Mathf.Sin(u*Mathf.PI)*1.8f+index*.35f);
                };
                var mesh = Ribbon("Mesh_Scholar" + phase + index, curve, casting ? .65f : 1.65f - index * .4f);
                var renderer = CreateRenderer("BrushRibbon" + i, air.transform, mesh, material);
                ribbons.GetArrayElementAtIndex(i).objectReferenceValue = renderer;
            }
            var fragments = serialized.FindProperty("_fragments");
            fragments.arraySize = phase == "Release" ? 10 : 0;
            if (fragments.arraySize > 0)
            {
                var mesh = Ribbon("Mesh_ScholarInkShard", u => new Vector3((u-.5f)*.65f, 0, 0), .22f);
                for (int i = 0; i < fragments.arraySize; i++)
                {
                    var renderer = CreateRenderer("InkShard" + i, air.transform, mesh, material);
                    float angle = i * 2.399963f;
                    renderer.transform.localPosition = new Vector3(Mathf.Cos(angle)*1.7f, .7f+(i%3)*.4f, Mathf.Sin(angle)*1.7f);
                    fragments.GetArrayElementAtIndex(i).objectReferenceValue = renderer;
                }
            }
            if (phase == "Hit")
            {
                var renderer = serialized.FindProperty("_renderer").objectReferenceValue as MeshRenderer;
                renderer.transform.localRotation = Quaternion.Euler(0,0,-25);
                renderer.gameObject.name = "AirHitStamp";
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static MeshRenderer CreateRenderer(string name, Transform parent, Mesh mesh, Material material)
    {
        var node = new GameObject(name);
        node.transform.SetParent(parent, false);
        node.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = node.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.enabled = false;
        return renderer;
    }

    private static Mesh Ribbon(string name, Func<float, Vector3> curve, float width)
    {
        var vertices = new Vector3[(SEGMENTS+1)*2];
        var uv = new Vector2[vertices.Length];
        var triangles = new int[SEGMENTS*6];
        for (int i = 0; i <= SEGMENTS; i++)
        {
            float u = (float)i / SEGMENTS;
            Vector3 center = curve(u);
            Vector3 tangent = curve(Mathf.Min(1,u+.01f)) - curve(Mathf.Max(0,u-.01f));
            Vector3 side = Vector3.Cross(tangent.normalized, Vector3.forward).normalized;
            if (side.sqrMagnitude < .01f) side = Vector3.up;
            float taper = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(u*Mathf.PI)), .65f) * (.65f + .35f*u);
            vertices[i*2] = center - side * width * taper * .5f;
            vertices[i*2+1] = center + side * width * taper * .5f;
            uv[i*2] = new Vector2(u,0); uv[i*2+1] = new Vector2(u,1);
            if (i == SEGMENTS) continue;
            int j = i*6, v = i*2;
            triangles[j] = v; triangles[j+1] = v+1; triangles[j+2] = v+2;
            triangles[j+3] = v+1; triangles[j+4] = v+3; triangles[j+5] = v+2;
        }
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(FOLDER+name+".asset");
        if (mesh == null)
        {
            mesh = new Mesh { name = name };
            AssetDatabase.CreateAsset(mesh,FOLDER+name+".asset");
        }
        mesh.Clear(); mesh.vertices = vertices; mesh.uv = uv; mesh.triangles = triangles;
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        EditorUtility.SetDirty(mesh);
        return mesh;
    }
}
#endif
