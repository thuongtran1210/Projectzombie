using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using ProjectZombie.Core.Architecture;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.Loadout;
using ProjectZombie.Features.Maps;

namespace ProjectZombie.Tests.Editor
{
    public sealed class AppFlowTests
    {
        private readonly List<UnityEngine.Object> _assets = new List<UnityEngine.Object>();

        [TearDown]
        public void Cleanup()
        {
            foreach (var asset in _assets) UnityEngine.Object.DestroyImmediate(asset);
            _assets.Clear();
        }

        [Test]
        public void RestartRetainsMapResourcesAndLoadoutButCreatesNewAttempt()
        {
            var loader = new FakeLoader();
            var rules = new FakeRules();
            var flow = new AppFlowController(loader, new AssetReference("hub"), null, (scene, request) => { }, rulesLoader: rules);
            var stage = MakeStage("first", "11111111111111111111111111111111");
            Assert.IsTrue(flow.EnterMatchAsync(new MatchLaunchRequest(stage, DefenseLoadoutSnapshot.CreateDefault())).GetAwaiter().GetResult());
            var before = flow.ActiveMatch;
            var after = flow.BeginNewAttempt();
            Assert.AreNotEqual(before.RunId, after.RunId);
            Assert.AreSame(before.Loadout, after.Loadout);
            Assert.AreSame(before.ResolvedRules, after.ResolvedRules);
            Assert.AreEqual(1, loader.Keys.Count);
        }

        [Test]
        public void SameRunCannotAwardTwiceAcrossResultServiceInstances()
        {
            var save = new ProjectZombie.Features.MetaProgression.MetaProgressionSaveData();
            var data = new DefenseMatchResultData(true, 600f, 0, stageId: "second", runId: "attempt-1", victoryReward: 123);
            new DefenseMatchResultService().ProcessMatchResult(data, save, _ => { });
            new DefenseMatchResultService().ProcessMatchResult(data, save, _ => { });
            Assert.AreEqual(123, save.totalCurrency);
            Assert.IsTrue(save.IsStageCompleted("second"));
            data.RunId = "attempt-2";
            new DefenseMatchResultService().ProcessMatchResult(data, save, _ => { });
            Assert.AreEqual(246, save.totalCurrency);
        }

        [Test]
        public void LockedStageCannotEnterBeforePrerequisiteIsCompleted()
        {
            var stage = MakeStage("second", "22222222222222222222222222222222");
            var serialized = new SerializedObject(stage);
            var requirements = serialized.FindProperty("_requiredCompletedStages");
            requirements.arraySize = 1;
            requirements.GetArrayElementAtIndex(0).stringValue = "first";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var save = new ProjectZombie.Features.MetaProgression.MetaProgressionSaveData();
            Assert.IsFalse(stage.IsUnlocked(save));
            save.MarkStageCompleted("first", 600f, 3);
            Assert.IsTrue(stage.IsUnlocked(save));
        }

        [Test]
        public void DifferentStagesRouteToTheirOwnMapAndRules()
        {
            var loader = new FakeLoader();
            var initialized = new List<MatchLaunchRequest>();
            var flow = new AppFlowController(loader, new AssetReference("hub"), null, (scene, request) => initialized.Add(request), rulesLoader: new FakeRules());
            var first = MakeStage("first", "11111111111111111111111111111111");
            var second = MakeStage("second", "22222222222222222222222222222222");
            var firstRequest = new MatchLaunchRequest(first, DefenseLoadoutSnapshot.CreateDefault());
            var secondRequest = new MatchLaunchRequest(second, DefenseLoadoutSnapshot.CreateDefault());
            Assert.IsTrue(flow.EnterMatchAsync(firstRequest).GetAwaiter().GetResult());
            Assert.AreEqual(firstRequest.RunId, flow.ActiveMatch.RunId);
            Assert.IsTrue(flow.ReturnToHubAsync().GetAwaiter().GetResult());
            Assert.IsNull(flow.ActiveMatch);
            Assert.IsNull(flow.SelectedStage);
            Assert.IsTrue(flow.EnterMatchAsync(secondRequest).GetAwaiter().GetResult());
            CollectionAssert.AreEqual(new[] { first.Map.Scene.RuntimeKey, "hub", second.Map.Scene.RuntimeKey }, loader.Keys);
            CollectionAssert.AreEqual(new[] { firstRequest.RunId, secondRequest.RunId }, initialized.ConvertAll(x => x.RunId));
            Assert.AreEqual(3, loader.Unloads);
            Assert.AreEqual(AppPhase.Match, flow.Phase);
            Assert.AreNotEqual(firstRequest.RunId, secondRequest.RunId);
        }

        [Test]
        public void TransitionLockRejectsSecondLaunchAndStageChanges()
        {
            var loader = new FakeLoader { Gate = new TaskCompletionSource<Scene>() };
            var flow = new AppFlowController(loader, new AssetReference("hub"), null, (scene, request) => { }, rulesLoader: new FakeRules());
            var stage = MakeStage("first", "11111111111111111111111111111111");
            var request = new MatchLaunchRequest(stage, DefenseLoadoutSnapshot.CreateDefault());
            var pending = flow.EnterMatchAsync(request);
            Assert.IsTrue(flow.IsNavigating);
            Assert.IsFalse(flow.EnterMatchAsync(request).GetAwaiter().GetResult());
            Assert.IsFalse(flow.ReturnToHubAsync().GetAwaiter().GetResult());
            Assert.Throws<InvalidOperationException>(() => flow.SelectStage(stage));
            loader.Gate.SetResult(default);
            Assert.IsTrue(pending.GetAwaiter().GetResult());
            Assert.IsFalse(flow.IsNavigating);
        }

        [Test]
        public void FailedInitializationUnloadsPartialMapAndRecoversHub()
        {
            var loader = new FakeLoader();
            var flow = new AppFlowController(loader, new AssetReference("hub"), null,
                (scene, request) => throw new InvalidOperationException("broken composition"), exception => { }, rulesLoader: new FakeRules());
            var stage = MakeStage("first", "11111111111111111111111111111111");
            Assert.IsFalse(flow.EnterMatchAsync(new MatchLaunchRequest(stage, DefenseLoadoutSnapshot.CreateDefault())).GetAwaiter().GetResult());
            Assert.AreEqual(AppPhase.Menu, flow.Phase);
            Assert.IsNull(flow.ActiveMatch);
            Assert.IsFalse(flow.IsNavigating);
            Assert.AreEqual("hub", loader.Keys[1]);
            Assert.AreEqual(2, loader.Unloads);
        }

        [Test]
        public void InvalidRequestFailsBeforeAnySceneIsUnloaded()
        {
            var stage = ScriptableObject.CreateInstance<StageDefinitionSO>();
            _assets.Add(stage);
            Assert.Throws<ArgumentException>(() => new MatchLaunchRequest(stage, DefenseLoadoutSnapshot.CreateDefault()));
        }

        [Test]
        public void InstalledProductionStageHasValidRouteAndPreservesSpawnPlan()
        {
            var stage = AssetDatabase.LoadAssetAtPath<StageDefinitionSO>("Assets/_Data/Levels/Stages/Stage_04_SongQue_Defense3D.asset");
            Assert.IsTrue(stage.CanLaunchDefense);
            Assert.AreEqual(AssetDatabase.AssetPathToGUID(AppArchitectureInstaller.MAP), stage.Map.Scene.AssetGUID);
            Assert.AreEqual(35, AssetDatabase.LoadAssetAtPath<StageSpawnConfig>(AssetDatabase.GUIDToAssetPath(stage.SpawnRules.AssetGUID)).Compile().Events.Count);
            Assert.AreEqual(600f, stage.DefenseDuration);
            Assert.AreEqual(AppArchitectureInstaller.BOOT, EditorBuildSettings.scenes[0].path);
        }

        [TestCase(AppArchitectureInstaller.HUB)]
        [TestCase(AppArchitectureInstaller.MAP)]
        public void ContentScenesHaveNoApplicationManagerOrEventSystem(string path)
        {
            var previous = SceneManager.GetActiveScene();
            var scene = SceneManager.GetSceneByPath(path);
            bool opened = !scene.IsValid() || !scene.isLoaded;
            if (opened) scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Additive);
            try
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    Assert.IsEmpty(root.GetComponentsInChildren<UnityEngine.EventSystems.EventSystem>(true));
                    Assert.IsEmpty(root.GetComponentsInChildren<ProjectZombie.Features.Shared.GameStateManager>(true));
                    Assert.IsEmpty(root.GetComponentsInChildren<ProjectZombie.Core.Save.GameManager>(true));
                    Assert.IsEmpty(root.GetComponentsInChildren<ProjectZombie.Features.MetaProgression.MetaCurrencyManager>(true));
                }
            }
            finally
            {
                if (opened) UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
        }

        private StageDefinitionSO MakeStage(string id, string guid)
        {
            var map = ScriptableObject.CreateInstance<MapDefinitionSO>();
            var stage = ScriptableObject.CreateInstance<StageDefinitionSO>();
            _assets.Add(map);
            _assets.Add(stage);
            var mapObject = new SerializedObject(map);
            mapObject.FindProperty("_mapId").stringValue = id;
            mapObject.FindProperty("_scene.m_AssetGUID").stringValue = guid;
            mapObject.ApplyModifiedPropertiesWithoutUndo();
            var stageObject = new SerializedObject(stage);
            stageObject.FindProperty("_map").objectReferenceValue = map;
            stageObject.FindProperty("_spawnRules.m_AssetGUID").stringValue = AssetDatabase.AssetPathToGUID("Assets/_Data/Levels/Rules/Stage01_Production.asset");
            stageObject.ApplyModifiedPropertiesWithoutUndo();
            stage.stageId = id;
            return stage;
        }

        private sealed class FakeRules : IStageRulesLoader
        {
            public Task<StageSpawnConfig> LoadAsync(StageDefinitionSO stage) => Task.FromResult(AssetDatabase.LoadAssetAtPath<StageSpawnConfig>("Assets/_Data/Levels/Rules/Stage01_Production.asset"));
            public void Release() { }
        }

        private sealed class FakeLoader : IContentSceneLoader
        {
            public Scene Current => default;
            public readonly List<object> Keys = new List<object>();
            public int Unloads;
            public TaskCompletionSource<Scene> Gate;
            public Task<Scene> LoadAsync(AssetReference reference)
            {
                Keys.Add(reference.RuntimeKey);
                return Gate != null ? Gate.Task : Task.FromResult(default(Scene));
            }
            public Task UnloadAsync() { Unloads++; return Task.CompletedTask; }
        }
    }
}
