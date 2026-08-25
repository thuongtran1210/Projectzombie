using UnityEngine;
using UnityEditor;
using ProjectZombie.Core.ScriptableObjects;

namespace ProjectZombie.Features.Enemies.Editor
{
    [CustomEditor(typeof(EnemyConfig))]
    public class EnemyConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty moveSpeedProp;
        private SerializedProperty preferredDistanceProp;
        private SerializedProperty minDistanceProp;
        private SerializedProperty maxHealthProp;
        private SerializedProperty damageToPlayerProp;
        private SerializedProperty attackCooldownProp;
        private SerializedProperty attackRangeProp;
        private SerializedProperty expRewardProp;
        private SerializedProperty tierProp;
        private SerializedProperty elementTypeProp;
        private SerializedProperty isHeavyArmorProp;
        private SerializedProperty coinDropRateProp;
        private SerializedProperty minCoinDropProp;
        private SerializedProperty maxCoinDropProp;

        private void OnEnable()
        {
            moveSpeedProp = serializedObject.FindProperty("moveSpeed");
            preferredDistanceProp = serializedObject.FindProperty("preferredDistance");
            minDistanceProp = serializedObject.FindProperty("minDistance");
            maxHealthProp = serializedObject.FindProperty("maxHealth");
            damageToPlayerProp = serializedObject.FindProperty("damageToPlayer");
            attackCooldownProp = serializedObject.FindProperty("attackCooldown");
            attackRangeProp = serializedObject.FindProperty("attackRange");
            expRewardProp = serializedObject.FindProperty("expReward");
            tierProp = serializedObject.FindProperty("tier");
            elementTypeProp = serializedObject.FindProperty("elementType");
            isHeavyArmorProp = serializedObject.FindProperty("isHeavyArmor");
            coinDropRateProp = serializedObject.FindProperty("coinDropRate");
            minCoinDropProp = serializedObject.FindProperty("minCoinDrop");
            maxCoinDropProp = serializedObject.FindProperty("maxCoinDrop");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("ProjectZombie — Enemy Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Basic Stats
            EditorGUILayout.PropertyField(tierProp);
            EditorGUILayout.PropertyField(elementTypeProp);
            EditorGUILayout.PropertyField(maxHealthProp);
            EditorGUILayout.PropertyField(damageToPlayerProp);
            EditorGUILayout.PropertyField(isHeavyArmorProp);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Loot & Drops (Exp & Coin)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(expRewardProp, new GUIContent("EXP Reward (ExpGem)"));
            EditorGUILayout.PropertyField(coinDropRateProp, new GUIContent("Coin Drop Rate (0-1)"));
            EditorGUILayout.PropertyField(minCoinDropProp, new GUIContent("Min Coin Drop"));
            EditorGUILayout.PropertyField(maxCoinDropProp, new GUIContent("Max Coin Drop"));

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Movement & Range Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(moveSpeedProp);
            EditorGUILayout.PropertyField(attackCooldownProp);
            EditorGUILayout.PropertyField(attackRangeProp, new GUIContent("Base Attack Range", "Tầm đánh Melee hoặc tầm bắn tối đa của Ranged"));

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("Các thông số cự ly Ranged bổ sung (Kiting / Retreat):", MessageType.Info);
            EditorGUILayout.PropertyField(preferredDistanceProp, new GUIContent("Preferred Distance (Ranged)", "Khoảng cách Ranged giữ cự ly với Player"));
            EditorGUILayout.PropertyField(minDistanceProp, new GUIContent("Min Distance (Ranged)", "Khoảng cách tối thiểu trước khi Ranged lùi lại"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
