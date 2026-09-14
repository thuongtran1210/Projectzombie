#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Công cụ rà soát toàn bộ các điểm gọi FindObjectOfType, FindObjectsOfType, 
    /// GameObject.Find, GameObject.FindWithTag trong toàn bộ mã nguồn Runtime.
    /// Giúp phát hiện các điểm nghẽn hiệu năng gây tụt FPS và che giấu lỗi Prefab/Scene.
    /// </summary>
    public static class RuntimeLookupAuditor
    {
        private static readonly Regex LookupRegex = new Regex(
            @"\b(GameObject\.Find\s*\(|GameObject\.FindWithTag\s*\(|GameObject\.FindGameObjectsWithTag\s*\(|FindObjectOfType\s*<|FindObjectsOfType\s*<|FindAnyObjectByType\s*<|FindFirstObjectByType\s*<)",
            RegexOptions.Compiled);

        [MenuItem("Tools/ProjectZombie/Audit/🔍 Audit Runtime Lookups (FindObject / GameObject.Find)", priority = 103)]
        public static void AuditLookups()
        {
            Debug.Log("<color=#00E5FF>[RuntimeLookupAuditor]</color> Bắt đầu rà soát các điểm gọi Lookup Runtime trong Assets/...");

            string[] csFiles = Directory.GetFiles("Assets", "*.cs", SearchOption.AllDirectories);
            int totalLookups = 0;
            int editorFilesCount = 0;

            var runtimeLookups = new List<string>();

            foreach (var file in csFiles)
            {
                string normPath = file.Replace('\\', '/');

                // Bỏ qua thư mục Editor và Tests vì Editor tool được phép dùng Find
                if (normPath.Contains("/Editor/") || normPath.Contains("/Tests/") || normPath.Contains("/Plugins/"))
                {
                    editorFilesCount++;
                    continue;
                }

                string[] lines = File.ReadAllLines(file);
                bool inEditorBlock = false;
                int editorBlockDepth = 0;

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    
                    // Xử lý tiền xử lý preprocessor #if UNITY_EDITOR
                    if (line.StartsWith("#if"))
                    {
                        if (line.Contains("UNITY_EDITOR"))
                        {
                            inEditorBlock = true;
                            editorBlockDepth++;
                        }
                        else if (inEditorBlock)
                        {
                            editorBlockDepth++;
                        }
                    }
                    else if (line.StartsWith("#endif"))
                    {
                        if (inEditorBlock)
                        {
                            editorBlockDepth--;
                            if (editorBlockDepth <= 0)
                            {
                                inEditorBlock = false;
                                editorBlockDepth = 0;
                            }
                        }
                    }

                    if (inEditorBlock) continue;

                    // Bỏ qua comment
                    if (line.StartsWith("//") || line.StartsWith("/*") || line.StartsWith("*")) continue;

                    var match = LookupRegex.Match(line);
                    if (match.Success)
                    {
                        totalLookups++;
                        string functionCall = match.Groups[1].Value.TrimEnd('(', '<', ' ');
                        runtimeLookups.Add($"• [{normPath}:{i + 1}] Gọi '{functionCall}':\n    → {line}");
                    }
                }
            }

            Debug.Log($"[RuntimeLookupAuditor] Đã quét {csFiles.Length} files C# (Bỏ qua {editorFilesCount} files Editor/Tests/Plugins).");

            if (runtimeLookups.Count > 0)
            {
                Debug.LogWarning($"<color=#FFAA00>[RuntimeLookupAuditor] PHÁT HIỆN {runtimeLookups.Count} ĐIỂM GỌI RUNTIME LOOKUP TRONG RUNTIME CODE:</color>\n" +
                                 string.Join("\n", runtimeLookups));
            }
            else
            {
                Debug.Log("<color=#00FF88>[RuntimeLookupAuditor] HOÀN HẢO:</color> 0 điểm gọi Lookup runtime trong toàn bộ Gameplay Runtime code!");
            }
        }
    }
}
#endif
