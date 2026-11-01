using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;
using System.Collections;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.Loadout;

public sealed class DefenseMinionXaThuTests
{
    [Test]
    public void Minion_XaThu_Asset_HasValidConfiguration()
    {
        var xaThuDef = Resources.Load<DefenseMinionDefinitionSO>("Defense3D/Minion_XaThu");
#if UNITY_EDITOR
        if (xaThuDef == null)
        {
            xaThuDef = UnityEditor.AssetDatabase.LoadAssetAtPath<DefenseMinionDefinitionSO>("Assets/Resources/Defense3D/Minion_XaThu.asset");
        }
#endif
        Assert.IsNotNull(xaThuDef, "Minion_XaThu.asset must exist in Resources/Defense3D/ or Assets/Resources/Defense3D/");
        Assert.AreEqual("minion_xathu", xaThuDef.Id);
        Assert.AreEqual(DefenseMinionRole.Militia, xaThuDef.Role);
        Assert.AreEqual(40, xaThuDef.WoodCost, "Xa Thu wood cost must be 40");
        Assert.AreEqual(75f, xaThuDef.MaxHp, 0.001f);
        Assert.AreEqual(4.2f, xaThuDef.Speed, 0.001f);
        Assert.AreEqual(8.5f, xaThuDef.AttackRange, 0.001f, "Xa Thu must have 8.5m long range");
        Assert.AreEqual(22f, xaThuDef.Damage, 0.001f);
        Assert.AreEqual(30f, xaThuDef.HeavyDamage, 0.001f);
        Assert.AreEqual(1.2f, xaThuDef.AttackInterval, 0.001f);
    }

    [Test]
    public void ApplyToShop_KeepsMilitiaAndXaThuAsIndependentSquads()
    {
        var actor = new GameObject("TestVillageShop");
        try
        {
            var shop = actor.AddComponent<DefenseVillageShop3D>();
            var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<DefenseMinionCatalogSO>(DefenseMinionRosterMigration.CATALOG);
            Set(shop, "_minionCatalog", catalog);
            var snapshot = new DefenseLoadoutSnapshot(Defense3DCatalog.HERO_DAO_SI, Defense3DCatalog.RELIC_DIEU_CAY,
                new[] { Defense3DCatalog.MINION_DAN_BINH, Defense3DCatalog.MINION_XA_THU }, new[] { Defense3DCatalog.TOWER_CUNG_TIEN });
            var resolved = new DefenseResolvedLoadout(snapshot, null, null, null, null, null);
            DefenseLoadoutRuntimeAdapter.ApplyToShop(resolved, shop);
            Assert.AreEqual(2, shop.Roster.Squads.Count);
            Assert.IsTrue(shop.Roster.TryGetSquad(Defense3DCatalog.MINION_DAN_BINH, out var militia));
            Assert.IsTrue(shop.Roster.TryGetSquad(Defense3DCatalog.MINION_XA_THU, out var archer));
            Assert.AreNotSame(militia, archer);
            Assert.AreEqual(40, archer.Definition.WoodCost);
        }
        finally { UnityEngine.Object.DestroyImmediate(actor); }
    }

    [UnityTest]
    public IEnumerator MilitiaCombatBehavior_WithXaThu_AttacksAtLongRange()
    {
        yield return new EnterPlayMode();
        using (var f = new Fixture())
        {
            // Đặt quái ở khoảng cách 6.5m (quá xa đối với Dân Binh 1.8m, nhưng hoàn toàn trong tầm bắn 8.5m của Xạ Thủ)
            var victim = f.AddVictim(true, Vector3.forward * 6.5f);

            var behavior = MinionBehaviorFactory.GetBehavior(DefenseMinionRole.Militia);
            Assert.IsNotNull(behavior);

            behavior.Tick(f.Minion, f.Shop, f.Session);

            // Xạ Thủ bắn tên nỏ gây 22 sát thương cơ bản và hiệu ứng làm chậm
            Assert.AreEqual(22f, victim.Damage, 0.001f);
            Assert.IsTrue(victim.HasStatus(DefenseStatusKind.Slow));
        }
        yield return new ExitPlayMode();
    }

    [UnityTest]
    public IEnumerator MilitiaCombatBehavior_HeavyShot_Inflicts30DamageAndStun()
    {
        yield return new EnterPlayMode();
        using (var f = new Fixture())
        {
            var victim = f.AddVictim(true, Vector3.forward * 5f);
            var behavior = MinionBehaviorFactory.GetBehavior(DefenseMinionRole.Militia);

            // Bắn 3 phát đầu (Normal Shots)
            for (int i = 0; i < 3; i++)
            {
                f.Minion.NextActionTime = 0f;
                behavior.Tick(f.Minion, f.Shop, f.Session);
            }
            Assert.AreEqual(22f * 3f, victim.Damage, 0.001f);
            Assert.IsFalse(victim.HasStatus(DefenseStatusKind.Stun));

            // Phát thứ 4 (Heavy Combo Shot): Đòn bắn nặng 30 damage kèm Stun và Knockback
            f.Minion.NextActionTime = 0f;
            behavior.Tick(f.Minion, f.Shop, f.Session);

            Assert.AreEqual(22f * 3f + 30f, victim.Damage, 0.001f);
            Assert.IsTrue(victim.HasStatus(DefenseStatusKind.Stun));
        }
        yield return new ExitPlayMode();
    }

    private static void Set(object target, string field, object value) =>
        target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

    private sealed class Fixture : IDisposable
    {
        private readonly List<GameObject> _objects = new List<GameObject>();
        public readonly DefenseSession Session = new DefenseSession();
        public readonly GameObject ShopGo = new GameObject("XaThuTestShop");
        public readonly GameObject MinionGo;
        private NavMeshData _navMesh;
        private NavMeshDataInstance _navMeshInstance;
        public readonly Vector3 Origin = new Vector3(52000f, 1f, 0);
        public readonly DefenseVillageShop3D Shop;
        public readonly DefenseMinion3D Minion;
        public readonly DefenseMinionDefinitionSO XaThuDefinition;

        public Fixture()
        {
            _objects.Add(ShopGo);

            Shop = ShopGo.AddComponent<DefenseVillageShop3D>();

            XaThuDefinition = Resources.Load<DefenseMinionDefinitionSO>("Defense3D/Minion_XaThu");
#if UNITY_EDITOR
            if (XaThuDefinition == null)
            {
                XaThuDefinition = UnityEditor.AssetDatabase.LoadAssetAtPath<DefenseMinionDefinitionSO>("Assets/Resources/Defense3D/Minion_XaThu.asset");
            }
#endif
            var sources = new List<NavMeshBuildSource> { new NavMeshBuildSource {
                shape = NavMeshBuildSourceShape.Box,
                transform = Matrix4x4.TRS(Origin - Vector3.up * .2f, Quaternion.identity, Vector3.one),
                size = new Vector3(32, .4f, 32), area = 0 } };
            _navMesh = NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByIndex(0), sources,
                new Bounds(Origin, new Vector3(40, 8, 40)), Vector3.zero, Quaternion.identity);
            Assert.NotNull(_navMesh);
            _navMeshInstance = NavMesh.AddNavMeshData(_navMesh);
            MinionGo = UnityEngine.Object.Instantiate(XaThuDefinition.Prefab.gameObject);
            _objects.Add(MinionGo); MinionGo.SetActive(false);
            Minion = MinionGo.GetComponent<DefenseMinion3D>();
            Set(Shop, "_catalog", new[] { XaThuDefinition });
            Shop.ConfigureRoster(new[] { XaThuDefinition.Id });
            // This remote fixture uses unrestricted advance orders; defend orders have a map-specific front line.
            var roster = (MinionRosterRuntime)Shop.Roster;
            roster.CycleOrder(XaThuDefinition.Id); roster.CycleOrder(XaThuDefinition.Id);
            Minion.Bind(Shop, Session);
            Session.Start();
            Assert.IsTrue(NavMesh.SamplePosition(Origin, out var spawn, 2f, NavMesh.AllAreas));
            Assert.IsTrue(Minion.Deploy(spawn.position, 0));

        }

        public TestVictim AddVictim(bool accepts, Vector3 offset)
        {
            var go = new GameObject("XaThuTestVictim");
            _objects.Add(go);
            go.transform.position = Origin + offset;
            var victim = new TestVictim(go.transform, accepts);
            Session.TargetRegistry.Register(victim);
            return victim;
        }

        public void Dispose()
        {
            foreach (var item in _objects)
            {
                if (item != null) UnityEngine.Object.DestroyImmediate(item);
            }
            Session.Dispose();
            _navMeshInstance.Remove();
            if (_navMesh != null) UnityEngine.Object.DestroyImmediate(_navMesh);
        }
    }

    private sealed class TestVictim : IDefenseEnemyTarget3D, IDefenseTargetPosition, IDefenseStatusReceiver
    {
        private readonly bool _accepts;
        public float Damage;
        public readonly List<DefenseStatusRequest> Statuses = new List<DefenseStatusRequest>();

        public TestVictim(Transform target, bool accepts)
        {
            TargetTransform = target;
            _accepts = accepts;
        }

        public Transform TargetTransform { get; }
        public bool IsTargetable => true;
        public object Identity => this;
        public bool IsAvailable => true;
        public bool IsAirborne => false;
        public DefensePoint3 Position => new DefensePoint3(TargetTransform.position.x, TargetTransform.position.y, TargetTransform.position.z);
        public float CurrentHP => 1000f - Damage;
        public float MaxHP => 1000f;
        public bool IsDead => false;

        public event Action<float, float> HealthChanged { add { } remove { } }
        public event Action<DefenseDamageContext> Damaged { add { } remove { } }
        public event Action Defeated { add { } remove { } }

        public bool TakeDamage(DefenseDamageContext context)
        {
            if (!_accepts) return false;
            Damage += context.Amount;
            return true;
        }

        public bool TryApplyStatus(DefenseStatusRequest request)
        {
            Statuses.Add(request);
            return true;
        }

        public bool HasStatus(DefenseStatusKind kind)
        {
            for (int i = 0; i < Statuses.Count; i++)
            {
                if (Statuses[i].Kind == kind) return true;
            }
            return false;
        }
    }
}

