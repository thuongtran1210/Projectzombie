#if UNITY_EDITOR
using System;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectZombie.Editor.UnityMcp.Tests
{
    /// <summary>Exercises serialized edits, conflict protection and Undo inside Test Runner's temporary scene.</summary>
    public sealed class UnityMcpObjectTests
    {
        private Scene _scene;
        private GameObject _object;

        /// <summary>Requires Test Runner's disposable scene before creating any test object.</summary>
        [SetUp]
        public void SetUp()
        {
            _scene = SceneManager.GetActiveScene();
            Assert.IsTrue(string.IsNullOrEmpty(_scene.path), "These tests require Test Runner's temporary scene.");
            Undo.IncrementCurrentGroup();
            _object = new GameObject("UnityMcp_TestObject");
            SceneManager.MoveGameObjectToScene(_object, _scene);
        }

        /// <summary>Removes only this fixture's objects and Undo records.</summary>
        [TearDown]
        public void TearDown()
        {
            if (_object == null) return;
            Undo.ClearUndo(_object.transform);
            Undo.ClearUndo(_object);
            UnityEngine.Object.DestroyImmediate(_object);
        }

        /// <summary>Unsaved scene objects must use resolvable session IDs.</summary>
        [Test]
        public void UnsavedObjectIdRoundTrips()
        {
            string id = UnityMcpObjects.Id(_object.transform);
            Assert.AreSame(_object.transform, UnityMcpObjects.Resolve(id));
        }

        /// <summary>Serialized changes mark the scene dirty and remain reversible with Undo.</summary>
        [Test]
        public void VectorEditRecordsUndo()
        {
            var args = new JObject { ["object_id"] = UnityMcpObjects.Id(_object.transform),
                ["property_path"] = "m_LocalPosition", ["expected_value"] = new JArray(0, 0, 0),
                ["value"] = new JArray(1, 2, 3) };
            JObject result = UnityMcpObjects.Set(args);
            Assert.AreEqual(new Vector3(1, 2, 3), _object.transform.localPosition);
            Assert.IsFalse((bool)result["saved"]);
            Assert.IsTrue(_scene.isDirty);
            Undo.FlushUndoRecordObjects();
            Undo.PerformUndo();
            Assert.AreEqual(Vector3.zero, _object.transform.localPosition);
        }

        /// <summary>Stale clients cannot overwrite a more recent Inspector value.</summary>
        [Test]
        public void ConflictDoesNotMutateObject()
        {
            _object.transform.localPosition = Vector3.one;
            Assert.Throws<InvalidOperationException>(() => UnityMcpObjects.Set(new JObject {
                ["object_id"] = UnityMcpObjects.Id(_object.transform), ["property_path"] = "m_LocalPosition",
                ["expected_value"] = new JArray(0, 0, 0), ["value"] = new JArray(5, 5, 5) }));
            Assert.AreEqual(Vector3.one, _object.transform.localPosition);
        }

        /// <summary>Malformed vectors fail before any serialized changes are applied.</summary>
        [Test]
        public void InvalidVectorDoesNotMutateObject()
        {
            Assert.Throws<ArgumentException>(() => UnityMcpObjects.Set(new JObject {
                ["object_id"] = UnityMcpObjects.Id(_object.transform), ["property_path"] = "m_LocalPosition",
                ["expected_value"] = new JArray(0, 0, 0), ["value"] = new JArray(5, 5) }));
            Assert.AreEqual(Vector3.zero, _object.transform.localPosition);
        }

        /// <summary>Engine metadata is never exposed for mutation.</summary>
        [Test]
        public void ProtectedPropertyIsRejected()
        {
            Assert.Throws<ArgumentException>(() => UnityMcpObjects.Set(new JObject {
                ["object_id"] = UnityMcpObjects.Id(_object), ["property_path"] = "m_ObjectHideFlags",
                ["expected_value"] = 0, ["value"] = 1 }));
        }
    }
}
#endif
