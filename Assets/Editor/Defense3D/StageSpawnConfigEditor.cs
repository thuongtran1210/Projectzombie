using System.Linq;
using ProjectZombie.Features.Defense3D;
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.EditorTools.Defense3D
{
    [CustomEditor(typeof(StageSpawnConfig))]
    public sealed class StageSpawnConfigEditor : global::UnityEditor.Editor
    {
        private string validation;
        private MessageType messageType;

        public override void OnInspectorGUI()
        {
            if (DrawDefaultInspector()) validation = null;
            EditorGUILayout.Space();
            if (GUILayout.Button("Validate Spawn Config"))
            {
                try
                {
                    var plan = ((StageSpawnConfig)target).Compile();
                    int ordinaryEvents = plan.Events.Count(e => e.Kind == DefenseSpawnKind.Enemy);
                    int ordinarySlots = plan.Events.Where(e => e.Kind == DefenseSpawnKind.Enemy).Sum(e => e.Count);
                    var boss = plan.Events.Single(e => e.Kind == DefenseSpawnKind.ArenaBoss);
                    validation = plan.ProfileVersion + "\n" + ordinaryEvents + " ordinary events / " + ordinarySlots +
                        " slots\nArena boss at " + boss.TriggerTime + " gameplay seconds\nMatch duration: 600 gameplay seconds";
                    messageType = MessageType.Info;
                }
                catch (System.Exception exception)
                {
                    validation = exception.Message;
                    messageType = MessageType.Error;
                }
            }
            if (!string.IsNullOrEmpty(validation)) EditorGUILayout.HelpBox(validation, messageType);
        }
    }
}
