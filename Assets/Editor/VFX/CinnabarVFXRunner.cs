using UnityEngine;
using UnityEditor;

namespace ProjectZombie.Editor.VFX
{
    public static class CinnabarVFXRunner
    {
        [InitializeOnLoadMethod]
        private static void RunCinnabarBuildOnLoad()
        {
            if (SessionState.GetBool("CinnabarVFX_Built_v2", false)) return;

            EditorApplication.delayCall += () =>
            {
                if (SessionState.GetBool("CinnabarVFX_Built_v2", false)) return;
                SessionState.SetBool("CinnabarVFX_Built_v2", true);

                Debug.Log("<color=#00FFFF>[Cinnabar VFX Runner]</color> Refresh Asset Database & bắt đầu tạo Materials / Prefabs Thần Sa...");
                AssetDatabase.Refresh();
                VFXMaterialGenerator.GenerateAllVFXMaterials();
                WeaponVFXBuilder.BuildAllWeaponVFX();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("<color=#00FF00>[Cinnabar VFX Runner]</color> ĐÃ HOÀN TẤT TẠO HIỆU ỨNG NỔ LỰU ĐẠN THẦN SA (W006) CHUẨN HITBOX 3.5M & 6 TẦNG PARTICLES!");
            };
        }
    }
}
