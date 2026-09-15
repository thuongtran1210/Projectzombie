#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades.Editor.Studio.Panels
{
    /// <summary>
    /// Panel chuyên trách chỉnh sửa thông số chi tiết (Inspector) của thẻ nâng cấp được chọn.
    /// </summary>
    public class StudioInspectorPanel
    {
        private readonly Action<UpgradeData> _onDuplicate;
        private readonly Action<UpgradeData> _onDelete;
        private Vector2 _scrollPos;

        public StudioInspectorPanel(Action<UpgradeData> onDuplicate, Action<UpgradeData> onDelete)
        {
            _onDuplicate = onDuplicate;
            _onDelete = onDelete;
        }

        public void Draw(UpgradeData selectedUpgrade, GUILayoutOption widthOption)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox, widthOption);

            if (selectedUpgrade == null)
            {
                EditorGUILayout.HelpBox("Chọn một thẻ từ cột trái để bắt đầu chỉnh sửa.", MessageType.Info);
                GUILayout.EndVertical();
                return;
            }

            // 1. Header Toolbar
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label($"<b>CHỈNH SỬA: {selectedUpgrade.id}</b>", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("📂 Ping", EditorStyles.toolbarButton))
            {
                EditorGUIUtility.PingObject(selectedUpgrade);
            }
            if (GUILayout.Button("📋 Nhân Bản", EditorStyles.toolbarButton))
            {
                _onDuplicate?.Invoke(selectedUpgrade);
            }
            if (GUILayout.Button("🗑️ Xóa", EditorStyles.toolbarButton))
            {
                if (EditorUtility.DisplayDialog("Xác nhận xóa", $"Bạn có chắc muốn xóa thẻ '{selectedUpgrade.upgradeName}' ({selectedUpgrade.id})?", "Xóa", "Hủy"))
                {
                    _onDelete?.Invoke(selectedUpgrade);
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    return;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            _scrollPos = GUILayout.BeginScrollView(_scrollPos);

            // 2. Chỉnh sửa thuộc tính
            EditorGUI.BeginChangeCheck();

            // 2.1 Thông tin cơ bản
            GUILayout.Label("1. THÔNG TIN CƠ BẢN", EditorStyles.boldLabel);
            selectedUpgrade.id = EditorGUILayout.TextField("Mã Thẻ (ID):", selectedUpgrade.id);
            selectedUpgrade.upgradeName = EditorGUILayout.TextField("Tên Thẻ:", selectedUpgrade.upgradeName);
            selectedUpgrade.icon = (Sprite)EditorGUILayout.ObjectField("Icon Sprite:", selectedUpgrade.icon, typeof(Sprite), false);
            selectedUpgrade.upgradeType = (UpgradeType)EditorGUILayout.EnumPopup("Phân Loại (Type):", selectedUpgrade.upgradeType);
            selectedUpgrade.element = (ElementType)EditorGUILayout.EnumPopup("Hệ Ngũ Hành:", selectedUpgrade.element);
            selectedUpgrade.spawnWeight = EditorGUILayout.FloatField("Trọng Số (Spawn Weight):", selectedUpgrade.spawnWeight);
            selectedUpgrade.maxLevel = EditorGUILayout.IntField("Cấp Tối Đa (Max Level):", selectedUpgrade.maxLevel);

            GUILayout.Space(6);

            // 2.2 Mô tả Rich Text
            GUILayout.Label("2. MÔ TẢ KỸ NĂNG (RICH TEXT)", EditorStyles.boldLabel);
            selectedUpgrade.description = EditorGUILayout.TextArea(selectedUpgrade.description, GUILayout.Height(65));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Vàng Kim", EditorStyles.miniButton)) selectedUpgrade.description += "<color=#FFD700>Text</color>";
            if (GUILayout.Button("+ Hỏa Đỏ", EditorStyles.miniButton)) selectedUpgrade.description += "<color=#FF4444>Text</color>";
            if (GUILayout.Button("+ Lam Thủy", EditorStyles.miniButton)) selectedUpgrade.description += "<color=#00E5FF>Text</color>";
            if (GUILayout.Button("+ In Đậm", EditorStyles.miniButton)) selectedUpgrade.description += "<b>Text</b>";
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // 2.3 Thông số chuyên sâu theo class con
            GUILayout.Label("3. THÔNG SỐ CHUYÊN SÂU", EditorStyles.boldLabel);

            if (selectedUpgrade is MythicCoreUpgradeData mythic)
            {
                mythic.archetype = (MythicArchetype)EditorGUILayout.EnumPopup("Trường Phái Archetype:", mythic.archetype);
                mythic.mythicTitle = EditorGUILayout.TextField("Danh Hiệu Thần Thoại:", mythic.mythicTitle);
                mythic.runtimePrefab = (MythicCoreRuntime)EditorGUILayout.ObjectField("Runtime Prefab:", mythic.runtimePrefab, typeof(MythicCoreRuntime), false);
            }
            else if (selectedUpgrade is SynergyTraitUpgradeData trait)
            {
                trait.requiredArchetype = (MythicArchetype)EditorGUILayout.EnumPopup("Yêu Cầu Đại Lõi:", trait.requiredArchetype);
                DrawPlayerStatModifierEditor(ref trait.playerStatModifier);
            }
            else if (selectedUpgrade is WeaponUpgradeData weaponUp)
            {
                weaponUp.weaponId = EditorGUILayout.TextField("ID Pháp Bảo Gốc:", weaponUp.weaponId);
                weaponUp.requiredCurrentLevel = EditorGUILayout.IntField("Cấp Yêu Cầu Hiện Tại:", weaponUp.requiredCurrentLevel);
                DrawWeaponStatModifierEditor(ref weaponUp.statModifier);
            }
            else if (selectedUpgrade is EvolutionUpgradeData evo)
            {
                evo.weaponId = EditorGUILayout.TextField("ID Pháp Bảo Gốc:", evo.weaponId);
                evo.requiredCurrentLevel = EditorGUILayout.IntField("Cấp Pháp Bảo Yêu Cầu:", evo.requiredCurrentLevel);
                evo.requiredPassiveId = EditorGUILayout.TextField("ID Khí Vận Yêu Cầu (Passive):", evo.requiredPassiveId);
                evo.weaponPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab Tiến Hóa Mới:", evo.weaponPrefab, typeof(GameObject), false);
            }
            else if (selectedUpgrade is CommonUpgradeData common)
            {
                DrawPlayerStatModifierEditor(ref common.playerStatModifier);
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(selectedUpgrade);
            }

            GUILayout.Space(16);

            if (GUILayout.Button("💾 Lưu Thay Đổi Vào File", GUILayout.Height(32)))
            {
                EditorUtility.SetDirty(selectedUpgrade);
                AssetDatabase.SaveAssets();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawPlayerStatModifierEditor(ref PlayerStatModifier mod)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Chỉ Số Nhân Vật (PlayerStatModifier):", EditorStyles.miniBoldLabel);
            mod.baseDamageBonus = EditorGUILayout.FloatField("Sát thương cơ bản (+):", mod.baseDamageBonus);
            mod.critChanceBonus = EditorGUILayout.FloatField("Tỉ lệ chí mạng (0.05 = 5%):", mod.critChanceBonus);
            mod.maxHealthBonus = EditorGUILayout.FloatField("Máu tối đa (+):", mod.maxHealthBonus);
            mod.moveSpeedBonus = EditorGUILayout.FloatField("Tốc độ di chuyển (+):", mod.moveSpeedBonus);
            mod.attackSpeedBonus = EditorGUILayout.FloatField("Tốc độ đánh (+):", mod.attackSpeedBonus);
            mod.dashCooldownReduction = EditorGUILayout.FloatField("Giảm hồi chiêu lướt (s):", mod.dashCooldownReduction);
            mod.dashSpeedBonus = EditorGUILayout.FloatField("Tăng tốc lướt (%):", mod.dashSpeedBonus);
            mod.areaScaleBonus = EditorGUILayout.FloatField("Phạm vi ảnh hưởng AoE (%):", mod.areaScaleBonus);
            mod.pickupRangeBonus = EditorGUILayout.FloatField("Tầm hút ngọc & đồ (+):", mod.pickupRangeBonus);
            mod.fireDamageBonus = EditorGUILayout.FloatField("Sát thương Hỏa (%):", mod.fireDamageBonus);
            GUILayout.EndVertical();
        }

        private void DrawWeaponStatModifierEditor(ref WeaponStatModifier mod)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Chỉ Số Pháp Bảo (WeaponStatModifier):", EditorStyles.miniBoldLabel);
            mod.damageBonus = EditorGUILayout.FloatField("Sát thương (+):", mod.damageBonus);
            mod.attackSpeedBonus = EditorGUILayout.FloatField("Tốc độ bắn/chém (+):", mod.attackSpeedBonus);
            mod.projectileCountBonus = EditorGUILayout.IntField("Số lượng đạn (+):", mod.projectileCountBonus);
            mod.pierceBonus = EditorGUILayout.IntField("Số lần xuyên thấu (+):", mod.pierceBonus);
            mod.scaleBonus = EditorGUILayout.FloatField("Kích thước đạn (%):", mod.scaleBonus);
            mod.critChanceBonus = EditorGUILayout.FloatField("Chí mạng (+):", mod.critChanceBonus);
            GUILayout.EndVertical();
        }
    }
}
#endif
