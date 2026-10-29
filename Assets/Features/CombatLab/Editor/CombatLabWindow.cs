using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.Features.CombatLab.Editor
{
    /// <summary>Opt-in Editor observer. Reflection keeps production Assembly-CSharp independent of the lab.</summary>
    public sealed class CombatLabWindow : EditorWindow
    {
        private const string ATTACK_TYPE = "ProjectZombie.Features.Defense3D.DefenseHeroAutoAttack3D";
        private const string ENEMY_TYPE = "ProjectZombie.Features.Defense3D.DefenseEnemy3D";
        private const string SOURCE_SCENE = "Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity";
        private Component _hero;
        private object _gameplay;
        private PropertyInfo _clock;
        private PropertyInfo _generationProperty;
        private int _generation;
        private CombatTrial _trial;
        private readonly List<Subscription> _subscriptions = new List<Subscription>();
        private double _startedAt, _elapsed;
        private bool _running;
        private string _status = "Enter Play Mode, assign the hero Attack component, then start.";
        private string _conditions = "Stationary / moving / group; record equipment and upgrades.";
        private string _snapshot;
        private int _trackedTargets;
        private Vector2 _scroll;

        [MenuItem("Tools/Combat Lab/Open Measurements")]
        private static void Open() => GetWindow<CombatLabWindow>("Combat Lab");

        [MenuItem("Tools/Combat Lab/Create Isolated Scene Copy")]
        private static void CreateSceneCopy()
        {
            const string folder = "Assets/Features/CombatLab/Scenes";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Features/CombatLab", "Scenes");
            string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/DefenseBaseline.unity");
            if (!AssetDatabase.CopyAsset(SOURCE_SCENE, path)) throw new IOException("Could not copy baseline scene.");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        private void OnEnable()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += Stop;
        }

        private void OnDisable()
        {
            Stop();
            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= Stop;
        }

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode) Stop();
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Read-only Editor probe. Targets are a fixed cohort present at Start; newly spawned enemies are excluded. No objects or balance values are changed.", MessageType.Info);
            using (new EditorGUI.DisabledScope(_running))
            {
                _hero = (Component)EditorGUILayout.ObjectField("Hero Attack", _hero, typeof(Component), true);
                _conditions = EditorGUILayout.TextField("Conditions", _conditions);
                if (GUILayout.Button("Start capture")) Begin();
            }
            using (new EditorGUI.DisabledScope(!_running))
            {
                if (GUILayout.Button("Stop capture")) Stop();
            }
            EditorGUILayout.LabelField(_status, EditorStyles.wordWrappedLabel);
            if (_trial == null) return;
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.SelectableLabel(Report(), EditorStyles.textArea, GUILayout.MinHeight(280));
            EditorGUILayout.EndScrollView();
            using (new EditorGUI.DisabledScope(_running))
            {
                if (GUILayout.Button("Export report")) Export();
            }
        }

        private void Begin()
        {
            Stop();
            if (!EditorApplication.isPlaying || _hero == null || _hero.GetType().FullName != ATTACK_TYPE)
            {
                _status = "Assign DefenseHeroAutoAttack3D from the running scene.";
                return;
            }
            try
            {
                Type baseType = _hero.GetType().BaseType;
                _gameplay = baseType.GetProperty("Gameplay", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_hero);
                if (_gameplay == null) throw new InvalidOperationException("Hero is not bound to gameplay.");
                Type contract = _gameplay.GetType().GetInterface("IDefenseGameplayContext");
                _clock = contract.GetProperty("ElapsedSeconds");
                _generationProperty = contract.GetProperty("RunGeneration");
                _generation = (int)_generationProperty.GetValue(_gameplay);
                _startedAt = Now();
                _elapsed = 0d;
                _trial = new CombatTrial();
                _trackedTargets = 0;
                _snapshot = $"Hero: {_hero.name}\nDamage: {Read(_hero, "EffectiveDamage")}\nInterval: {Read(_hero, "EffectiveAttackInterval")}\nRange: {Read(_hero, "AttackRange")}\nCone: {Read(_hero, "ConeAngle")}\nScene: {_hero.gameObject.scene.path}";
                Subscribe(_hero, "AttackStarted", (Action)OnAttack);
                Type enemyType = _hero.GetType().Assembly.GetType(ENEMY_TYPE, true);
                foreach (UnityEngine.Object candidate in UnityEngine.Object.FindObjectsOfType(enemyType))
                {
                    var enemy = (Component)candidate;
                    object enemyGameplay = baseType.GetProperty("Gameplay", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(enemy);
                    if (!ReferenceEquals(enemyGameplay, _gameplay)) continue;
                    var probe = new TargetProbe(this, enemy);
                    EventInfo damaged = enemyType.GetEvent("Damaged");
                    Type payload = damaged.EventHandlerType.GetGenericArguments()[0];
                    MethodInfo handler = typeof(TargetProbe).GetMethod(nameof(TargetProbe.OnDamage)).MakeGenericMethod(payload);
                    Subscribe(enemy, "Damaged", Delegate.CreateDelegate(damaged.EventHandlerType, probe, handler));
                    Subscribe(enemy, "HealthChanged", (Action<float, float>)probe.OnHealth);
                    Subscribe(enemy, "Initialized", (Action)OnTargetReused);
                    _trackedTargets++;
                }
                if (_trackedTargets == 0) throw new InvalidOperationException("No active targets in this session. Start after enemies spawn.");
                _running = true;
                _status = "Capturing fixed cohort; pause is excluded by the session clock.";
            }
            catch (Exception error)
            {
                Stop();
                _status = "Capture unavailable: " + error.GetBaseException().Message;
            }
        }

        private void OnAttack() => _trial.RecordAttack(Math.Max(0d, Now() - _startedAt));
        private void OnTargetReused()
        {
            Stop();
            _status = "Stopped: a tracked pooled target began a new lifetime.";
        }

        private void Tick()
        {
            if (!_running) return;
            if (_hero == null || !EditorApplication.isPlaying || (int)_generationProperty.GetValue(_gameplay) != _generation)
            {
                Stop();
                _status = "Stopped: hero or run changed.";
                return;
            }
            _elapsed = Math.Max(0d, Now() - _startedAt);
            Repaint();
        }

        private double Now() => Convert.ToDouble(_clock.GetValue(_gameplay));
        private static object Read(object target, string name) => target.GetType().GetProperty(name).GetValue(target);

        private void Subscribe(object source, string name, Delegate handler)
        {
            EventInfo signal = source.GetType().GetEvent(name);
            signal.AddEventHandler(source, handler);
            _subscriptions.Add(new Subscription { Source = source, Signal = signal, Handler = handler });
        }

        private void Stop()
        {
            if (_running && _gameplay != null) _elapsed = Math.Max(0d, Now() - _startedAt);
            _running = false;
            foreach (Subscription item in _subscriptions) item.Signal.RemoveEventHandler(item.Source, item.Handler);
            _subscriptions.Clear();
        }

        private string Report()
        {
            double mean = _trial.IntervalSamples > 0 ? _trial.IntervalTotal / _trial.IntervalSamples : 0d;
            return $"{_snapshot}\nConditions: {_conditions}\nTracked targets: {_trackedTargets}\nGameplay seconds: {_elapsed:F3}\nAttack starts (all targets): {_trial.Attacks}\nAccepted hits (tracked cohort): {_trial.Hits}\nMean start interval: {mean:F4}\nRequested damage: {_trial.RequestedDamage:F2}\nHP removed: {_trial.HpRemoved:F2}\nLethal overkill: {_trial.Overkill:F2}\nCohort HP DPS: {_trial.Dps(_elapsed):F2}\n\nSingle-target DPS requires one isolated target. Group DPS is cohort total. TTK and mitigation are not inferred. Attack starts include canceled/missed attacks. Upgrades changing during capture invalidate baseline comparison.";
        }

        private void Export()
        {
            string path = EditorUtility.SaveFilePanel("Export combat capture", "Logs", "CombatLab", "txt");
            if (!string.IsNullOrEmpty(path)) File.WriteAllText(path, Report());
        }

        private sealed class Subscription
        {
            internal object Source;
            internal EventInfo Signal;
            internal Delegate Handler;
        }

        private sealed class TargetProbe
        {
            private readonly CombatLabWindow _owner;
            private readonly Component _enemy;
            private double _hp;
            internal TargetProbe(CombatLabWindow owner, Component enemy)
            {
                _owner = owner;
                _enemy = enemy;
                _hp = Convert.ToDouble(Read(enemy, "CurrentHP"));
            }
            public void OnHealth(float current, float maximum) => _hp = current;
            public void OnDamage<T>(T context)
            {
                double current = Convert.ToDouble(Read(_enemy, "CurrentHP"));
                Type type = typeof(T);
                if (ReferenceEquals(type.GetField("Source").GetValue(context), _owner._hero))
                    _owner._trial.RecordHit(Convert.ToDouble(type.GetField("Amount").GetValue(context)), _hp, current);
                _hp = current;
            }
        }
    }
}
