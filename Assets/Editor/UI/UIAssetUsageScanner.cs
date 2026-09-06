#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace ProjectZombie.EditorTools
{
    public static class UIAssetUsageScanner
    {
        [MenuItem("Tools/Vong Xuyen/Analysis/Scan UI Images Usage", priority = 100)]
        public static void ScanUIImages()
        {
            string uiRoot = "Assets/Art/UI";
            string[] imageExtensions = new string[] { ".png", ".jpg", ".jpeg", ".tga", ".psd" };

            // 1. Thu thap tat ca file anh trong Assets/Art/UI
            var allImagePaths = Directory.GetFiles(uiRoot, "*.*", SearchOption.AllDirectories)
                .Where(f => imageExtensions.Contains(Path.GetExtension(f).ToLower()))
                .Select(f => f.Replace('\\', '/'))
                .ToList();

            // 2. Thu thap tat ca cac file co the tham chieu (Scenes, Prefabs, ScriptableObjects, Materials, Scripts)
            string[] allSearchFiles = Directory.GetFiles("Assets", "*.*", SearchOption.AllDirectories)
                .Where(f => {
                    string ext = Path.GetExtension(f).ToLower();
                    return ext == ".unity" || ext == ".prefab" || ext == ".asset" || ext == ".mat" || ext == ".cs";
                })
                .Select(f => f.Replace('\\', '/'))
                .ToArray();

            // 3. Doc noi dung toan bo cac file de kiem tra GUID hoac Path
            Dictionary<string, string> imageGuidMap = new Dictionary<string, string>();
            foreach (var imgPath in allImagePaths)
            {
                string guid = AssetDatabase.AssetPathToGUID(imgPath);
                if (!string.IsNullOrEmpty(guid))
                {
                    imageGuidMap[imgPath] = guid;
                }
            }

            // Doc toan bo text cua cac file asset
            Dictionary<string, int> usageCount = new Dictionary<string, int>();
            foreach (var kvp in imageGuidMap) usageCount[kvp.Key] = 0;

            foreach (var searchFile in allSearchFiles)
            {
                try
                {
                    string content = File.ReadAllText(searchFile);
                    foreach (var kvp in imageGuidMap)
                    {
                        string imgPath = kvp.Key;
                        string guid = kvp.Value;
                        string filename = Path.GetFileName(imgPath);

                        // Kiem tra theo GUID hoac ten file (trong ma C#)
                        if (content.Contains(guid) || (searchFile.EndsWith(".cs") && content.Contains(filename)))
                        {
                            usageCount[imgPath]++;
                        }
                    }
                }
                catch { }
            }

            var usedImages = usageCount.Where(kvp => kvp.Value > 0).OrderByDescending(kvp => kvp.Value).ToList();
            var unusedImages = usageCount.Where(kvp => kvp.Value == 0).OrderBy(kvp => kvp.Key).ToList();

            Debug.Log($"<color=#00FF88>========================================</color>");
            Debug.Log($"<color=#00FF88><b>[UIAssetUsageScanner] KET QUA QUET TAI NGUYEN UI ({allImagePaths.Count} anh tong cong):</b></color>");
            Debug.Log($"<color=#4DEEEA>So anh DANG DUOC SU DUNG: {usedImages.Count}</color>");
            Debug.Log($"<color=#FF5555>So anh CHUA DUOC SU DUNG (Unused): {unusedImages.Count}</color>");
            Debug.Log($"<color=#00FF88>========================================</color>");

            // Xuat file report
            string reportPath = "C:/Users/thuon/.gemini/antigravity-ide/brain/1a15a52b-7442-4e42-9488-7583a847c1b7/scratch/ui_usage_report.txt";
            using (StreamWriter sw = new StreamWriter(reportPath, false, System.Text.Encoding.UTF8))
            {
                sw.WriteLine($"=== KET QUA PHAN TICH ANH UI (Tong so: {allImagePaths.Count}) ===");
                sw.WriteLine($"1. SO ANH DANG DUOC SU DUNG ({usedImages.Count}):");
                foreach (var item in usedImages)
                {
                    sw.WriteLine($"  [USED x{item.Value}] {item.Key}");
                }
                sw.WriteLine();
                sw.WriteLine($"2. SO ANH CHUA DUOC SU DUNG ({unusedImages.Count}):");
                foreach (var item in unusedImages)
                {
                    sw.WriteLine($"  [UNUSED] {item.Key}");
                }
            }

            Debug.Log($"<color=#FFD700>Da xuat bao cao chi tiet tai: {reportPath}</color>");
        }
    }
}
#endif
