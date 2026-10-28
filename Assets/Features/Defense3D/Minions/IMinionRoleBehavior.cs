using System;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Hợp đồng Strategy cho từng vai trò lính độc lập (Dân Binh, Phu Tạp Dịch, Thầy Lang).
    /// Tuân thủ nguyên tắc Single Responsibility và Open/Closed Principle.
    /// </summary>
    public interface IMinionRoleBehavior
    {
        DefenseMinionRole Role { get; }
        void Tick(DefenseMinion3D minion, DefenseVillageShop3D shop, IDefenseGameplayContext gameplay);
    }

    /// <summary>
    /// Factory cấp phát và quản lý các Strategy hành vi của lính (0 GC allocations, tái sử dụng Singleton).
    /// </summary>
    public static class MinionBehaviorFactory
    {
        private static readonly IMinionRoleBehavior Militia = new MilitiaCombatBehavior();
        private static readonly IMinionRoleBehavior Gatherer = new GathererLogisticsBehavior();
        private static readonly IMinionRoleBehavior Healer = new HealerSupportBehavior();

        public static IMinionRoleBehavior GetBehavior(DefenseMinionRole role)
        {
            switch (role)
            {
                case DefenseMinionRole.Militia: return Militia;
                case DefenseMinionRole.Gatherer: return Gatherer;
                case DefenseMinionRole.Healer: return Healer;
                default: return Militia;
            }
        }
    }

    /// <summary>
    /// Hành vi chiến đấu giáp lá cà của Dân Binh (Militia).
    /// Tìm quái vật trong tầm lệnh, tiếp cận, đánh combo 4 nhịp gây choáng, làm chậm, đẩy lùi.
    /// </summary>
    public sealed class MilitiaCombatBehavior : IMinionRoleBehavior
    {
        public DefenseMinionRole Role => DefenseMinionRole.Militia;

        public void Tick(DefenseMinion3D minion, DefenseVillageShop3D shop, IDefenseGameplayContext gameplay)
        {
            if (minion == null || shop == null || gameplay == null) return;

            Vector3 p = minion.transform.position;
            var definition = minion.Definition;

            var target = (gameplay.Targets as IDefenseSpatialTargetQuery)?.FindNearestSpatial(
                new DefensePoint3(p.x, p.y, p.z), definition.SearchRange, DefenseTargetMask.Ground, minion.TargetAcceptancePredicate)?.Identity as IDefenseEnemyTarget3D;

            if (target == null)
            {
                minion.Move(shop.Anchor(minion), 0.5f);
                return;
            }

            Vector3 goal = target.TargetTransform.position;
            if (!minion.Move(goal, definition.AttackRange * 0.75f)) return;

            Vector3 delta = goal - p;
            delta.y = 0f;
            if (delta.sqrMagnitude > definition.AttackRange * definition.AttackRange || minion.CurrentGameplayTime < minion.NextActionTime) return;

            if (!minion.SampleAreaPosition(goal, out var hit, 2.5f) || minion.RaycastNavMesh(p, hit.position)) return;

            minion.Stop();
            if (delta.sqrMagnitude > 0.0001f) minion.transform.rotation = Quaternion.LookRotation(delta);

            minion.NextActionTime = minion.CurrentGameplayTime + definition.AttackInterval;
            int combo = minion.AdvanceCombo();
            bool heavy = (combo % 4) == 0;
            int generation = gameplay.RunGeneration;

            if (!target.TakeDamage(new DefenseDamageContext(heavy ? definition.HeavyDamage : definition.Damage, minion, goal,
                runGeneration: generation, sessionIdentity: gameplay.SessionIdentity))) return;

            if (target is IDefenseStatusReceiver receiver && minion.IsGameplayAdvancing && gameplay.RunGeneration == generation && minion.IsAvailable)
            {
                receiver.TryApplyStatus(new DefenseStatusRequest(DefenseStatusKind.Slow, definition.SlowDuration,
                    definition.SlowStrength, default, gameplay.SessionIdentity, generation));
                if (heavy)
                {
                    receiver.TryApplyStatus(new DefenseStatusRequest(DefenseStatusKind.Stun, definition.HeavyStunDuration,
                        1f, default, gameplay.SessionIdentity, generation));
                }
                Vector3 direction = delta.sqrMagnitude > 0.0001f ? delta.normalized : minion.transform.forward;
                receiver.TryApplyStatus(new DefenseStatusRequest(DefenseStatusKind.Knockback, definition.KnockDuration,
                    definition.KnockStrength, new DefensePoint3(direction.x, 0f, direction.z), gameplay.SessionIdentity, generation));
            }
        }
    }

    /// <summary>
    /// Hành vi thu thập tài nguyên và hậu cần của Phu Tạp Dịch (Gatherer).
    /// Tìm khúc gỗ rơi, nhặt đầy ba lô, chạy về giao cho Tướng/Nhà Chính, hoặc hỗ trợ sửa công trình nếu rảnh rỗi.
    /// </summary>
    public sealed class GathererLogisticsBehavior : IMinionRoleBehavior
    {
        public DefenseMinionRole Role => DefenseMinionRole.Gatherer;

        public void Tick(DefenseMinion3D minion, DefenseVillageShop3D shop, IDefenseGameplayContext gameplay)
        {
            if (minion == null || shop == null || gameplay == null) return;

            var definition = minion.Definition;
            var drop = minion.Cargo >= definition.CarryCapacity ? null : shop.FindWood(minion);

            if (minion.Cargo > 0 && (drop == null || minion.Cargo >= definition.CarryCapacity))
            {
                if (shop.Hero.IsDown)
                {
                    minion.Move(shop.Base.transform.position + Vector3.right * 3f, 1f);
                    return;
                }
                Vector3 hero = shop.Hero.transform.position;
                minion.Move(hero, 0.8f);
                Vector3 delta = hero - minion.transform.position;
                delta.y = 0f;
                if (delta.sqrMagnitude <= 1.8f * 1.8f)
                {
                    int cargo = minion.Cargo;
                    minion.Cargo = 0;
                    shop.Deliver(cargo);
                }
                return;
            }

            if (drop == null)
            {
                if (shop.FindPatient(minion, out var repairPoint, true) is IDefenseDamageable3D structure)
                {
                    minion.Move(repairPoint, 0.7f);
                    Vector3 repairDelta = shop.InteractionPoint(structure, minion.transform.position, repairPoint) - minion.transform.position;
                    repairDelta.y = 0f;
                    if (repairDelta.sqrMagnitude <= definition.AttackRange * definition.AttackRange &&
                        minion.CurrentGameplayTime >= minion.NextActionTime && minion.RepairStructure(structure))
                    {
                        minion.NextActionTime = minion.CurrentGameplayTime + definition.RepairInterval;
                    }
                }
                else
                {
                    minion.Move(shop.Anchor(minion), 0.5f);
                }
                return;
            }

            minion.Move(drop.transform.position, 0.4f);
            Vector3 offset = drop.transform.position - minion.transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude <= 1f && drop.TryGather(gameplay, definition.CarryCapacity - minion.Cargo, out int amount))
            {
                minion.Cargo += amount;
            }
        }
    }

    /// <summary>
    /// Hành vi y tế và hỗ trợ của Thầy Lang (Healer).
    /// Tìm kiếm đồng minh bị thương (Tướng, Dân binh, Căn cứ) để chữa trị và gia cố.
    /// </summary>
    public sealed class HealerSupportBehavior : IMinionRoleBehavior
    {
        public DefenseMinionRole Role => DefenseMinionRole.Healer;

        public void Tick(DefenseMinion3D minion, DefenseVillageShop3D shop, IDefenseGameplayContext gameplay)
        {
            if (minion == null || shop == null || gameplay == null) return;

            var definition = minion.Definition;
            var patient = shop.FindPatient(minion, out var point);

            if (patient == null)
            {
                minion.Move(shop.Anchor(minion), 0.5f);
                return;
            }

            minion.Move(point, definition.AttackRange * 0.7f);
            Vector3 delta = shop.InteractionPoint(patient, minion.transform.position, point) - minion.transform.position;
            delta.y = 0f;
            if (delta.sqrMagnitude > definition.AttackRange * definition.AttackRange || minion.CurrentGameplayTime < minion.NextActionTime) return;

            bool structure = patient is DefenseBasePrototype3D || (patient is DefenseSupportTarget3D building && building.IsStructure);
            bool healed = structure ? minion.RepairStructure(patient) :
                patient is DefenseHeroHealth3D hero ? hero.Heal(definition.HeroHealAmount) :
                patient is DefenseSupportTarget3D support && support.Heal(definition.HealAmount);

            if (healed)
            {
                minion.NextActionTime = minion.CurrentGameplayTime + (structure ? definition.RepairInterval : definition.HealInterval);
            }
        }
    }
}
