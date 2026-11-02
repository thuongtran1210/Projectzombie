#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

[CustomEditor(typeof(DefenseSkillDefinitionSO))]
public sealed class DefenseSkillDefinitionInspector : Editor
{
    private Editor presentationEditor;
    private Editor tuningEditor;
    private bool showPresentation=true;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.LabelField("Skill identity",EditorStyles.boldLabel);
        foreach(string field in new[]{"_skillId","_kind","_behavior"})EditorGUILayout.PropertyField(serializedObject.FindProperty(field));
        EditorGUILayout.Space();EditorGUILayout.LabelField("Gameplay (base values)",EditorStyles.boldLabel);
        foreach(string field in new[]{"_range","_radius","_damage","_cooldown","_duration","_autoTargetOnTap","_targetMask"})EditorGUILayout.PropertyField(serializedObject.FindProperty(field));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_tuning"));
        EditorGUILayout.Space();EditorGUILayout.PropertyField(serializedObject.FindProperty("_presentation"));
        serializedObject.ApplyModifiedProperties();
        var definition=(DefenseSkillDefinitionSO)target;
        if(definition.Tuning!=null)
        {
            EditorGUILayout.LabelField("Behavior tuning asset",EditorStyles.boldLabel);
            Editor.CreateCachedEditor(definition.Tuning,null,ref tuningEditor);
            using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))tuningEditor.OnInspectorGUI();
        }
        if(definition.Presentation==null){EditorGUILayout.HelpBox("Assign a typed Presentation. HUD and VFX read this reference; scene presenters do not own skill styles.",MessageType.Warning);return;}
        EditorGUILayout.HelpBox("Runtime reads identity/icon/VFX from this shared Presentation. Values below edit that asset directly.",MessageType.Info);
        showPresentation=EditorGUILayout.Foldout(showPresentation,"Presentation asset",true);
        if(showPresentation)
        {
            Editor.CreateCachedEditor(definition.Presentation,null,ref presentationEditor);
            using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))presentationEditor.OnInspectorGUI();
            if(GUILayout.Button("Select Presentation asset"))Selection.activeObject=definition.Presentation;
        }
    }
    private void OnDisable(){if(presentationEditor!=null)DestroyImmediate(presentationEditor);if(tuningEditor!=null)DestroyImmediate(tuningEditor);}
}
#endif
