#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectZombie.Editor.UnityMcp
{
    /// <summary>Stable object discovery and bounded, Undo-aware serialized property access.</summary>
    internal static class UnityMcpObjects
    {
        private const string SESSION_KEY = "ProjectZombie.UnityMcp.ObjectSession";

        static UnityMcpObjects()
        {
            EditorApplication.playModeStateChanged += state =>
                SessionState.SetString(SESSION_KEY, Guid.NewGuid().ToString("N"));
        }

        private static string Session
        {
            get
            {
                string session = SessionState.GetString(SESSION_KEY, "");
                if (!string.IsNullOrEmpty(session)) return session;
                session = Guid.NewGuid().ToString("N");
                SessionState.SetString(SESSION_KEY, session);
                return session;
            }
        }

        internal static string Id(Object value)
        {
            if (value == null) return null;
            GlobalObjectId global = GlobalObjectId.GetGlobalObjectIdSlow(value);
            if (global.targetObjectId != 0 && global.assetGUID.ToString() != new string('0', 32) && !EditorApplication.isPlaying)
                return global.ToString();
            return "instance:" + Session + ":" + value.GetInstanceID();
        }

        internal static Object Resolve(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("object_id is required.");
            Object value = null;
            if (id.StartsWith("instance:", StringComparison.Ordinal))
            {
                string[] parts = id.Split(':');
                if (parts.Length == 3 && parts[1] == Session && int.TryParse(parts[2], out int instance))
                    value = EditorUtility.InstanceIDToObject(instance);
            }
            else if (GlobalObjectId.TryParse(id, out GlobalObjectId global))
            {
                value = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(global);
            }
            if (value == null) throw new ArgumentException("Object ID is missing or stale. Query objects again.");
            return value;
        }

        internal static JObject Hierarchy(JObject args)
        {
            string filter = (string)args["name_contains"] ?? "";
            int offset = Math.Max(0, args.Value<int?>("offset") ?? 0);
            int limit = UnityMcpBridge.Limit(args, 500);
            var rows = new List<JObject>();
            int matched = 0;
            foreach (var scene in UnityMcpBridge.Scenes())
            {
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
                    {
                        if (transform.name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0) continue;
                        if (matched++ < offset || rows.Count >= limit) continue;
                        var names = new List<string>();
                        for (Transform current = transform; current != null; current = current.parent) names.Add(current.name);
                        names.Reverse();
                        rows.Add(new JObject { ["id"] = Id(transform.gameObject), ["name"] = transform.name,
                            ["path"] = "/" + string.Join("/", names), ["scene"] = scene.path,
                            ["activeSelf"] = transform.gameObject.activeSelf, ["activeInHierarchy"] = transform.gameObject.activeInHierarchy,
                            ["components"] = new JArray(transform.GetComponents<Component>().Select(component => component == null
                                ? new JObject { ["missingScript"] = true }
                                : new JObject { ["id"] = Id(component), ["type"] = component.GetType().FullName })) });
                    }
                }
            }
            return new JObject { ["objects"] = new JArray(rows), ["total"] = matched, ["offset"] = offset,
                ["hasMore"] = offset + rows.Count < matched,
                ["idScope"] = EditorApplication.isPlaying ? "Runtime instance IDs: re-query after Play Mode transitions." : "GlobalObjectId where available; instance IDs for unsaved objects." };
        }

        internal static JObject FindAssets(JObject args)
        {
            string[] guids = AssetDatabase.FindAssets((string)args["query"] ?? "", new[] { "Assets" });
            int offset = Math.Max(0, args.Value<int?>("offset") ?? 0);
            int limit = UnityMcpBridge.Limit(args, 500);
            return new JObject { ["total"] = guids.Length, ["offset"] = offset,
                ["hasMore"] = offset + limit < guids.Length,
                ["assets"] = new JArray(guids.Skip(offset).Take(limit).Select(guid =>
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    Object asset = AssetDatabase.LoadMainAssetAtPath(path);
                    return new JObject { ["guid"] = guid, ["path"] = path, ["id"] = Id(asset),
                        ["type"] = asset != null ? asset.GetType().FullName : null };
                })) };
        }

        internal static JObject Read(JObject args)
        {
            Object target = Resolve((string)args["object_id"]);
            var serialized = new SerializedObject(target);
            serialized.Update();
            string path = (string)args["property_path"];
            var properties = new JArray();
            if (!string.IsNullOrEmpty(path))
            {
                SerializedProperty property = serialized.FindProperty(path);
                if (property == null) throw new ArgumentException("Serialized property not found: " + path);
                properties.Add(Describe(property));
            }
            else
            {
                SerializedProperty iterator = serialized.GetIterator();
                int limit = UnityMcpBridge.Limit(args, 500);
                while (properties.Count < limit && iterator.NextVisible(true)) properties.Add(Describe(iterator));
            }
            return new JObject { ["id"] = Id(target), ["type"] = target.GetType().FullName,
                ["persistent"] = EditorUtility.IsPersistent(target), ["properties"] = properties };
        }

        private static JObject Describe(SerializedProperty property)
        {
            return new JObject { ["path"] = property.propertyPath, ["type"] = property.propertyType.ToString(),
                ["editable"] = property.editable && Supported(property) && property.propertyPath != "m_Script",
                ["value"] = Value(property) };
        }

        private static bool Supported(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Float:
                case SerializedPropertyType.Boolean:
                case SerializedPropertyType.String:
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.Vector2:
                case SerializedPropertyType.Vector3:
                case SerializedPropertyType.Vector4:
                case SerializedPropertyType.Color: return true;
                default: return false;
            }
        }

        private static JToken Value(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer: return new JValue(property.longValue);
                case SerializedPropertyType.Float: return new JValue(property.doubleValue);
                case SerializedPropertyType.Boolean: return new JValue(property.boolValue);
                case SerializedPropertyType.String: return new JValue(property.stringValue);
                case SerializedPropertyType.Enum: return new JValue(property.enumValueIndex);
                case SerializedPropertyType.ObjectReference: return new JValue(Id(property.objectReferenceValue));
                case SerializedPropertyType.Vector2:
                    Vector2 v2 = property.vector2Value; return new JArray(v2.x, v2.y);
                case SerializedPropertyType.Vector3:
                    Vector3 v3 = property.vector3Value; return new JArray(v3.x, v3.y, v3.z);
                case SerializedPropertyType.Vector4:
                    Vector4 v4 = property.vector4Value; return new JArray(v4.x, v4.y, v4.z, v4.w);
                case SerializedPropertyType.Color:
                    Color color = property.colorValue; return new JArray(color.r, color.g, color.b, color.a);
                default: return JValue.CreateNull();
            }
        }

        internal static JObject Set(JObject args)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Serialized edits require stable Edit Mode.");
            Object target = Resolve((string)args["object_id"]);
            bool persistent = EditorUtility.IsPersistent(target);
            string assetPath = AssetDatabase.GetAssetPath(target);
            if (persistent && (!(target is ScriptableObject) || !assetPath.StartsWith("Assets/", StringComparison.Ordinal)))
                throw new InvalidOperationException("Persistent edits are limited to ScriptableObjects in Assets. Edit prefab scene instances instead.");
            if (!persistent && !(target is Component) && !(target is GameObject))
                throw new InvalidOperationException("Only scene objects and ScriptableObject assets can be edited.");
            var serialized = new SerializedObject(target);
            serialized.Update();
            string path = (string)args["property_path"];
            if (string.IsNullOrEmpty(path) || path == "m_Script" || path == "m_ObjectHideFlags" ||
                path.StartsWith("m_Corresponding", StringComparison.Ordinal) || path.StartsWith("m_Prefab", StringComparison.Ordinal))
                throw new ArgumentException("This property is protected.");
            SerializedProperty property = serialized.FindProperty(path);
            if (property == null || !property.editable || !Supported(property))
                throw new ArgumentException("Property is missing, read-only, or unsupported.");
            JToken before = Value(property);
            if (!args.ContainsKey("expected_value") || !Equivalent(before, args["expected_value"]))
                throw new InvalidOperationException("Property changed or expected_value is missing. Read the current value before editing.");
            JToken next = args["value"];
            if (next == null) throw new ArgumentException("value is required (may be explicit null for a reference).");
            Assign(property, next, persistent);
            Undo.RecordObject(target, "Unity MCP: " + path);
            serialized.ApplyModifiedProperties();
            if (PrefabUtility.IsPartOfPrefabInstance(target)) PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            if (persistent) EditorUtility.SetDirty(target);
            else
            {
                GameObject gameObject = target as GameObject ?? ((Component)target).gameObject;
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
            serialized.Update();
            return new JObject { ["id"] = Id(target), ["property"] = path, ["before"] = before,
                ["after"] = Value(serialized.FindProperty(path)), ["saved"] = false, ["assetPath"] = assetPath,
                ["undoRecorded"] = true };
        }

        private static bool Equivalent(JToken left, JToken right)
        {
            if (left is JArray la && right is JArray ra)
                return la.Count == ra.Count && la.Zip(ra, Equivalent).All(equal => equal);
            if (left != null && right != null && (left.Type == JTokenType.Integer || left.Type == JTokenType.Float) &&
                (right.Type == JTokenType.Integer || right.Type == JTokenType.Float))
                return left.Value<double>() == right.Value<double>();
            return JToken.DeepEquals(left, right);
        }

        private static void RequireType(JToken value, params JTokenType[] types)
        {
            if (!types.Contains(value.Type)) throw new ArgumentException("JSON value has the wrong type.");
        }

        private static float[] Vector(JToken value, int dimensions)
        {
            if (!(value is JArray array) || array.Count != dimensions) throw new ArgumentException("Wrong vector dimensions.");
            return array.Select(item =>
            {
                RequireType(item, JTokenType.Integer, JTokenType.Float);
                float number = item.Value<float>();
                if (float.IsNaN(number) || float.IsInfinity(number)) throw new ArgumentException("Vector must be finite.");
                return number;
            }).ToArray();
        }

        private static void Assign(SerializedProperty property, JToken value, bool persistent)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    RequireType(value, JTokenType.Integer); property.longValue = value.Value<long>(); break;
                case SerializedPropertyType.Float:
                    RequireType(value, JTokenType.Float, JTokenType.Integer);
                    double number = value.Value<double>();
                    if (double.IsNaN(number) || double.IsInfinity(number)) throw new ArgumentException("Value must be finite.");
                    property.doubleValue = number; break;
                case SerializedPropertyType.Boolean:
                    RequireType(value, JTokenType.Boolean); property.boolValue = value.Value<bool>(); break;
                case SerializedPropertyType.String:
                    RequireType(value, JTokenType.String); property.stringValue = value.Value<string>(); break;
                case SerializedPropertyType.Enum:
                    RequireType(value, JTokenType.Integer);
                    int index = value.Value<int>();
                    if (index < 0 || index >= property.enumNames.Length) throw new ArgumentException("Enum index out of range.");
                    property.enumValueIndex = index; break;
                case SerializedPropertyType.ObjectReference:
                    RequireType(value, JTokenType.Null, JTokenType.String);
                    Object reference = value.Type == JTokenType.Null ? null : Resolve(value.Value<string>());
                    if (persistent && reference != null && !EditorUtility.IsPersistent(reference))
                        throw new ArgumentException("An asset cannot reference a scene object.");
                    property.objectReferenceValue = reference;
                    if (property.objectReferenceValue != reference) throw new ArgumentException("Reference type is incompatible with this property.");
                    break;
                case SerializedPropertyType.Vector2:
                    float[] v2 = Vector(value, 2); property.vector2Value = new Vector2(v2[0], v2[1]); break;
                case SerializedPropertyType.Vector3:
                    float[] v3 = Vector(value, 3); property.vector3Value = new Vector3(v3[0], v3[1], v3[2]); break;
                case SerializedPropertyType.Vector4:
                    float[] v4 = Vector(value, 4); property.vector4Value = new Vector4(v4[0], v4[1], v4[2], v4[3]); break;
                case SerializedPropertyType.Color:
                    float[] color = Vector(value, 4); property.colorValue = new Color(color[0], color[1], color[2], color[3]); break;
                default: throw new ArgumentException("Unsupported property type.");
            }
        }
    }
}
#endif
