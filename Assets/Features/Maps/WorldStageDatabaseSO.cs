using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProjectZombie.Features.Maps
{
    /// <summary>
    /// Database lưu trữ toàn bộ các Ải / Màn chơi trong game (World Map).
    /// </summary>
    [CreateAssetMenu(fileName = "WorldStageDatabase", menuName = "ProjectZombie/Maps/World Stage Database")]
    public class WorldStageDatabaseSO : ScriptableObject
    {
        [Header("Danh Sách Các Ải (World Stages)")]
        [SerializeField] private List<StageDefinitionSO> _stages = new List<StageDefinitionSO>();

        public IReadOnlyList<StageDefinitionSO> Stages => _stages;

        public StageDefinitionSO GetStageById(string stageId)
        {
            if (_stages == null) return null;
            return _stages.Find(s => s != null && s.stageId == stageId);
        }

        public StageDefinitionSO GetStageByIndex(int index)
        {
            if (_stages == null || index < 0 || index >= _stages.Count) return null;
            return _stages[index];
        }

#if UNITY_EDITOR
        public void SetStages(List<StageDefinitionSO> stages)
        {
            _stages = stages;
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
