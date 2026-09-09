using System;
using System.Collections.Generic;

namespace ProjectZombie.EditorTools.BuildSync
{
    public enum RuleType
    {
        Directory,
        SingleAsset,
        UIPrefab
    }

    [Serializable]
    public class SyncRule
    {
        public string Name;
        public RuleType Type;
        public string SourcePath;
        public string TargetPath;
        public string SearchPattern;
        public string[] AllowedExtensions;
        public Action FallbackGenerator;

        public SyncRule(string name, string sourcePath, string targetPath, string searchPattern = "*.*", string[] allowedExtensions = null)
        {
            Name = name;
            Type = RuleType.Directory;
            SourcePath = sourcePath;
            TargetPath = targetPath;
            SearchPattern = searchPattern;
            AllowedExtensions = allowedExtensions;
        }

        public static SyncRule ForSingleAsset(string name, string sourcePath, string targetPath)
        {
            return new SyncRule(name, sourcePath, targetPath)
            {
                Type = RuleType.SingleAsset
            };
        }

        public static SyncRule ForUIPrefab(string prefabName, Action fallbackGenerator = null)
        {
            return new SyncRule(prefabName, $"Assets/_Prefabs/UI/{prefabName}.prefab", $"Assets/Resources/UI/{prefabName}.prefab", "*.prefab")
            {
                Type = RuleType.UIPrefab,
                FallbackGenerator = fallbackGenerator
            };
        }
    }
}
