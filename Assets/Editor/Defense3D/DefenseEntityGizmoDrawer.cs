using ProjectZombie.Features.Defense3D;
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.EditorTools.Defense3D
{
    /// <summary>
    /// Bộ công cụ trực quan hóa không gian (Spatial Gizmos & Handles) dành cho Game Designer.
    /// Tự động hiển thị các bán kính chiến đấu, tầm đánh, vùng cảnh giác và góc nón ngay trên Scene View
    /// khi chọn thực thể trong Editor (không cần bấm Play game).
    /// </summary>
    public static class DefenseEntityGizmoDrawer
    {
        // ── Bảng màu trực quan chuẩn Cổ Phong Đông Sơn ──
        private static readonly Color TowerRangeColor = new Color(0.2f, 0.85f, 1f, 0.75f);      // Cyan (Tháp Cung)
        private static readonly Color HeroRangeColor = new Color(0.2f, 1f, 0.4f, 0.85f);        // Xanh Lục (Anh Hùng)
        private static readonly Color MinionAttackColor = new Color(0.3f, 0.9f, 0.5f, 0.8f);    // Xanh Ngọc (Lính)
        private static readonly Color MinionSearchColor = new Color(1f, 0.9f, 0.2f, 0.25f);     // Vàng Nhạt (Tầm tìm kiếm)
        private static readonly Color EnemyAttackColor = new Color(1f, 0.35f, 0.2f, 0.9f);      // Đỏ Cam (Quái đánh)
        private static readonly Color EnemyAggroColor = new Color(1f, 0.7f, 0.1f, 0.3f);        // Vàng Cam (Vùng cảnh giác)
        private static readonly Color ConeAttackColor = new Color(1f, 0.15f, 0.15f, 0.35f);     // Đỏ Trong Suốt (Góc quét quạt)

        // ── 1. GIZMO CHO THÁP PHÒNG THỦ (TOWER) ──
        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        public static void DrawTowerGizmos(DefenseTowerPrototype3D tower, GizmoType gizmoType)
        {
            if (tower == null) return;
            Vector3 center = tower.transform.position;
            float range = tower.EffectiveRange;

            Handles.color = TowerRangeColor;
            Handles.DrawWireDisc(center, Vector3.up, range);
            Handles.color = new Color(TowerRangeColor.r, TowerRangeColor.g, TowerRangeColor.b, 0.08f);
            Handles.DrawSolidDisc(center, Vector3.up, range);

            GUIStyle labelStyle = GetLabelStyle(TowerRangeColor);
            Handles.Label(center + Vector3.forward * range, $"🏹 Tầm Bắn Tháp: {range:F1}m", labelStyle);
        }

        // ── 2. GIZMO CHO ANH HÙNG (HERO ATTACK & AUTO ATTACK) ──
        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        public static void DrawHeroGizmos(DefenseHeroAutoAttack3D heroAttack, GizmoType gizmoType)
        {
            if (heroAttack == null) return;
            Vector3 center = heroAttack.transform.position;
            float range = heroAttack.AttackRange;

            Handles.color = HeroRangeColor;
            Handles.DrawWireDisc(center, Vector3.up, range);
            Handles.color = new Color(HeroRangeColor.r, HeroRangeColor.g, HeroRangeColor.b, 0.06f);
            Handles.DrawSolidDisc(center, Vector3.up, range);

            GUIStyle labelStyle = GetLabelStyle(HeroRangeColor);
            Handles.Label(center + Vector3.forward * range, $"⚔️ Tầm Đánh Anh Hùng: {range:F1}m", labelStyle);
        }

        // ── 3. GIZMO CHO QUÂN LÍNH ĐỒNG MINH (MINION) ──
        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        public static void DrawMinionGizmos(DefenseMinion3D minion, GizmoType gizmoType)
        {
            if (minion == null) return;
            Vector3 center = minion.transform.position;

            // Đọc tầm đánh và tầm tìm kiếm
            float attackRange = 1.2f;
            float searchRange = 8.0f;

            var definition = minion.Definition;
            if (definition != null)
            {
                attackRange = definition.AttackRange;
                searchRange = definition.SearchRange;
            }

            // Vòng tầm tìm kiếm (Search Range - Màu vàng mờ)
            Handles.color = MinionSearchColor;
            Handles.DrawWireDisc(center, Vector3.up, searchRange);
            Handles.color = new Color(MinionSearchColor.r, MinionSearchColor.g, MinionSearchColor.b, 0.04f);
            Handles.DrawSolidDisc(center, Vector3.up, searchRange);

            // Vòng tầm đánh (Attack Range - Màu xanh ngọc)
            Handles.color = MinionAttackColor;
            Handles.DrawWireDisc(center, Vector3.up, attackRange);

            GUIStyle labelStyle = GetLabelStyle(MinionAttackColor);
            Handles.Label(center + Vector3.forward * attackRange, $"🛡️ {minion.name} (Đánh: {attackRange:F1}m | Quét: {searchRange:F1}m)", labelStyle);
        }

        // ── 4. GIZMO CHO QUÁI VẬT & YÊU MA (ENEMY) ──
        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        public static void DrawEnemyGizmos(DefenseEnemy3D enemy, GizmoType gizmoType)
        {
            if (enemy == null) return;
            Vector3 center = enemy.transform.position;

            float attackRange = 1.5f;
            float aggroRange = 8.0f;

            if (enemy.Definition != null)
            {
                attackRange = enemy.Definition.AttackRange > 0.1f ? enemy.Definition.AttackRange : 1.5f;
            }
            else if (enemy.Stats.AttackRange > 0.1f)
            {
                attackRange = enemy.Stats.AttackRange;
            }

            // Vùng cảnh giác / Aggro
            Handles.color = EnemyAggroColor;
            Handles.DrawWireDisc(center, Vector3.up, aggroRange);
            Handles.color = new Color(EnemyAggroColor.r, EnemyAggroColor.g, EnemyAggroColor.b, 0.04f);
            Handles.DrawSolidDisc(center, Vector3.up, aggroRange);

            // Vòng tầm đánh
            Handles.color = EnemyAttackColor;
            Handles.DrawWireDisc(center, Vector3.up, attackRange);

            GUIStyle labelStyle = GetLabelStyle(EnemyAttackColor);
            Handles.Label(center + Vector3.forward * attackRange, $"👹 {enemy.name} (Đánh: {attackRange:F1}m | Cảnh giác: {aggroRange:F1}m)", labelStyle);
        }

        // ── 5. GIZMO CHO ĐÒN ĐÁNH HÌNH QUẠT QUÉT NÓN (CONE ATTACK CỦA BOSS DẠ XOA) ──
        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        public static void DrawConeAttackGizmos(DefenseConeAttack3D cone, GizmoType gizmoType)
        {
            if (cone == null) return;
            Vector3 center = cone.transform.position;
            Vector3 forward = cone.transform.forward;

            float radius = 4.5f;
            float angle = 120f;

            Vector3 from = Quaternion.Euler(0f, -angle * 0.5f, 0f) * forward;

            Handles.color = ConeAttackColor;
            Handles.DrawSolidArc(center, Vector3.up, from, angle, radius);
            Handles.color = new Color(1f, 0.2f, 0.2f, 0.9f);
            Handles.DrawWireArc(center, Vector3.up, from, angle, radius);
            Handles.DrawLine(center, center + from * radius);
            Vector3 to = Quaternion.Euler(0f, angle * 0.5f, 0f) * forward;
            Handles.DrawLine(center, center + to * radius);

            GUIStyle labelStyle = GetLabelStyle(Color.red);
            Handles.Label(center + forward * radius, $"💥 Quét Nón Dạ Xoa ({angle}° | {radius:F1}m)", labelStyle);
        }

        // ── 6. GIZMO CHO BẢN ĐỒ CHIẾN TRƯỜNG & VÙNG SPAWN (SCENE MARKERS & SPAWN ZONES) ──
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Active)]
        public static void DrawSceneInfoGizmos(DefenseSceneInfo3D sceneInfo, GizmoType gizmoType)
        {
            if (sceneInfo == null) return;

            // 1. Cổng Xuất Hiện Quái Bờ Đông
            if (sceneInfo.EastEnemySpawn != null)
            {
                Vector3 east = sceneInfo.EastEnemySpawn.position;
                Handles.color = new Color(0.95f, 0.8f, 0.25f, 0.9f);
                Handles.DrawWireDisc(east, Vector3.up, 3.1f); // Bán kính an toàn
                Handles.color = new Color(0.95f, 0.8f, 0.25f, 0.12f);
                Handles.DrawSolidDisc(east, Vector3.up, 3.1f);
                Handles.Label(east + Vector3.up * 2f, "🚪 CỔNG BỜ ĐÔNG (EAST SPAWN)\n<size=9>Bán kính an toàn 3.1m</size>", GetLabelStyle(new Color(0.95f, 0.8f, 0.25f)));
            }

            // 2. Căn Cứ Làng (Defense Base)
            if (sceneInfo.BaseMarker != null)
            {
                Vector3 b = sceneInfo.BaseMarker.position;
                Handles.color = new Color(0.2f, 1f, 0.4f, 0.9f);
                Handles.DrawWireDisc(b, Vector3.up, 4.0f);
                Handles.color = new Color(0.2f, 1f, 0.4f, 0.10f);
                Handles.DrawSolidDisc(b, Vector3.up, 4.0f);
                Handles.Label(b + Vector3.up * 2.2f, "🏰 CĂN CỨ LÀNG (DEFENSE BASE)", GetLabelStyle(new Color(0.2f, 1f, 0.4f)));
            }

            // 3. Vị Trí Xuất Hiện Của Anh Hùng (Hero Spawn)
            if (sceneInfo.HeroSpawn != null)
            {
                Vector3 h = sceneInfo.HeroSpawn.position;
                Handles.color = new Color(0.3f, 0.8f, 1f, 0.8f);
                Handles.DrawWireDisc(h, Vector3.up, 1.5f);
                Handles.Label(h + Vector3.up * 1.8f, "⭐ VỊ TRÍ ANH HÙNG (HERO)", GetLabelStyle(new Color(0.3f, 0.8f, 1f)));
            }

            // 4. Các Bệ Đặt Tháp (Tower Sockets)
            for (int i = 0; i < sceneInfo.TowerSocketCount; i++)
            {
                var socket = sceneInfo.GetTowerSocket(i);
                if (socket == null) continue;
                Vector3 pos = socket.position;
                Handles.color = new Color(0.2f, 0.85f, 1f, 0.8f);
                Handles.DrawWireDisc(pos, Vector3.up, 1.2f);
                Handles.Label(pos + Vector3.up * 1.5f, $"🏯 Bệ Tháp #{i + 1}", GetLabelStyle(new Color(0.2f, 0.85f, 1f)));
            }

            // 5. Đường Nối Hành Quân (Lane Flow Line)
            if (sceneInfo.EastEnemySpawn != null && sceneInfo.BaseMarker != null)
            {
                Handles.color = new Color(1f, 0.4f, 0.2f, 0.6f);
                Handles.DrawDottedLine(sceneInfo.EastEnemySpawn.position, sceneInfo.BaseMarker.position, 4f);
            }
        }

        // ── TIỆN ÍCH TẠO PHONG CÁCH NHÃN CHỮ SCENE VIEW ──
        private static GUIStyle GetLabelStyle(Color textColor)
        {
            var style = new GUIStyle(EditorStyles.boldLabel);
            style.normal.textColor = textColor;
            style.fontSize = 11;
            style.alignment = TextAnchor.MiddleCenter;
            return style;
        }
    }
}
