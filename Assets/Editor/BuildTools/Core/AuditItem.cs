using System;

namespace ProjectZombie.EditorTools.BuildSync
{
    [Serializable]
    public struct AuditItem
    {
        public enum SeverityLevel { Info, Warning, Error }
        public enum FixActionType { None, OptimizePlayerSettings, SyncDirectory, SyncSingleAsset, SyncUIPrefab }

        public SeverityLevel Severity;
        public string Title;
        public string Description;
        public string Recommendation;
        public FixActionType ActionType;
        public SyncRule Rule;
        public string FixButtonText;
    }
}
