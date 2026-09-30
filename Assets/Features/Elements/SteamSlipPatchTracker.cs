using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Shared;
using ProjectZombie.Core.ScriptableObjects;
using UnityEngine;

namespace ProjectZombie.Features.Elements
{
    /// <summary>Tracks patch state and bounded enemy overlap/collision work.</summary>
    internal sealed class SteamSlipPatchTracker
    {
        private sealed class PatchState
        {
            public bool Active;
            public Vector2 Center;
            public float ExpiresAt;
            public int PrimaryEnemyId;
            public int AffectedCount;
            public int CollisionCount;
            public readonly int[] AffectedIds;
            public readonly int[] CollisionA;
            public readonly int[] CollisionB;

            public PatchState(int targetCap, int collisionCap)
            {
                AffectedIds = new int[targetCap];
                CollisionA = new int[collisionCap];
                CollisionB = new int[collisionCap];
            }

            public void Reset(Enemy primary, float expiry)
            {
                Active = true;
                Center = primary.transform.position;
                ExpiresAt = expiry;
                PrimaryEnemyId = primary.GetInstanceID();
                AffectedCount = 0;
                CollisionCount = 0;
                System.Array.Clear(AffectedIds, 0, AffectedIds.Length);
                System.Array.Clear(CollisionA, 0, CollisionA.Length);
                System.Array.Clear(CollisionB, 0, CollisionB.Length);
            }
        }

        private readonly SteamSlipReactionSettings _settings;
        private readonly PatchState[] _patches;
        private readonly Collider2D[] _overlapBuffer;

        public SteamSlipPatchTracker(SteamSlipReactionSettings settings)
        {
            _settings = settings;
            _patches = new PatchState[settings.patchCapacity];
            for (int i = 0; i < _patches.Length; i++)
                _patches[i] = new PatchState(settings.secondaryTargetCap, settings.collisionImpactCap);
            _overlapBuffer = new Collider2D[settings.colliderBufferSize];
        }

        public bool TryCreatePatch(Enemy primary, float now, out Vector2 center)
        {
            int slot = FindReusableSlot(now);
            if (slot < 0)
            {
                center = default;
                SteamSlipReactionDiagnostics.Report(new SteamSlipDiagnosticEvent(
                    SteamSlipDiagnosticKind.PatchCapacityRejected, primary.GetInstanceID(), ElementType.None, now, _patches.Length));
                return false;
            }
            PatchState patch = _patches[slot];
            patch.Reset(primary, now + _settings.patchLifetime);
            center = patch.Center;
            SteamSlipReactionDiagnostics.Report(new SteamSlipDiagnosticEvent(
                SteamSlipDiagnosticKind.PatchCreated, primary.GetInstanceID(), ElementType.None, patch.ExpiresAt, slot));
            return true;
        }

        public void Tick(float now, int enemyMask)
        {
            for (int i = 0; i < _patches.Length; i++)
            {
                PatchState patch = _patches[i];
                if (!patch.Active) continue;
                if (now >= patch.ExpiresAt)
                {
                    patch.Active = false;
                    continue;
                }
                ProcessPatch(patch, now, enemyMask);
            }
        }

        private int FindReusableSlot(float now)
        {
            for (int i = 0; i < _patches.Length; i++)
            {
                PatchState patch = _patches[i];
                if (!patch.Active || patch.ExpiresAt <= now)
                    return i;
            }
            // Drop the newest reaction when all slots are active; existing patches remain deterministic.
            return -1;
        }

        private void ProcessPatch(PatchState patch, float now, int enemyMask)
        {
            int count = Physics2D.OverlapCircleNonAlloc(
                patch.Center, _settings.patchRadius, _overlapBuffer, enemyMask);
            // A full buffer may have truncated results; retry next frame instead of selecting a partial set.
            if (count >= _overlapBuffer.Length)
            {
                SteamSlipReactionDiagnostics.Report(new SteamSlipDiagnosticEvent(
                    SteamSlipDiagnosticKind.OverlapBufferSaturated, patch.PrimaryEnemyId, ElementType.None, now, count));
                return;
            }
            ApplyToNewTargets(patch, count);
            ResolveTargetCollisions(patch, count);
        }

        private void ApplyToNewTargets(PatchState patch, int count)
        {
            for (int i = 0; i < count && patch.AffectedCount < patch.AffectedIds.Length; i++)
            {
                Collider2D collider = _overlapBuffer[i];
                if (collider == null || !collider.TryGetComponent<Enemy>(out var enemy) || enemy == null)
                    continue;
                int id = enemy.GetInstanceID();
                if (enemy.ReactionClass == EnemyReactionClass.Boss ||
                    id == patch.PrimaryEnemyId ||
                    HasAffected(patch, id))
                    continue;
                Vector2 away = (Vector2)enemy.transform.position - patch.Center;
                if (away.sqrMagnitude < 0.0001f) away = Vector2.right;
                Vector2 direction = new Vector2(-away.y, away.x).normalized;
                if ((id & 1) == 0) direction = -direction;
                enemy.ApplySteamSlip(direction, _settings);
                patch.AffectedIds[patch.AffectedCount++] = id;
                SteamSlipReactionDiagnostics.Report(new SteamSlipDiagnosticEvent(
                    SteamSlipDiagnosticKind.TargetAffected, id, ElementType.None, 0f, patch.AffectedCount));
            }
        }

        private void ResolveTargetCollisions(PatchState patch, int count)
        {
            if (patch.CollisionCount >= patch.CollisionA.Length) return;
            for (int a = 0; a < patch.AffectedCount; a++)
            {
                int firstId = patch.AffectedIds[a];
                Collider2D first = FindCollider(firstId, count);
                if (first == null) continue;
                for (int b = a + 1; b < patch.AffectedCount; b++)
                {
                    int secondId = patch.AffectedIds[b];
                    if (HasCollision(patch, firstId, secondId)) continue;
                    Collider2D second = FindCollider(secondId, count);
                    if (second == null || !first.Distance(second).isOverlapped) continue;
                    if (first.TryGetComponent<Enemy>(out var firstEnemy))
                        firstEnemy.ApplySteamSlipCollisionStagger(_settings);
                    if (second.TryGetComponent<Enemy>(out var secondEnemy))
                        secondEnemy.ApplySteamSlipCollisionStagger(_settings);
                    patch.CollisionA[patch.CollisionCount] = firstId;
                    patch.CollisionB[patch.CollisionCount] = secondId;
                    patch.CollisionCount++;
                    SteamSlipReactionDiagnostics.Report(new SteamSlipDiagnosticEvent(
                        SteamSlipDiagnosticKind.CollisionStaggered, firstId, ElementType.None, 0f, patch.CollisionCount));
                    if (patch.CollisionCount >= patch.CollisionA.Length) return;
                }
            }
        }

        private bool HasAffected(PatchState patch, int id)
        {
            for (int i = 0; i < patch.AffectedCount; i++)
            {
                if (patch.AffectedIds[i] == id)
                    return true;
            }
            return false;
        }

        private bool HasCollision(PatchState patch, int firstId, int secondId)
        {
            for (int i = 0; i < patch.CollisionCount; i++)
                if ((patch.CollisionA[i] == firstId && patch.CollisionB[i] == secondId) ||
                    (patch.CollisionA[i] == secondId && patch.CollisionB[i] == firstId)) return true;
            return false;
        }

        private Collider2D FindCollider(int enemyId, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Collider2D collider = _overlapBuffer[i];
                if (collider != null && collider.TryGetComponent<Enemy>(out var enemy) &&
                    enemy != null && enemy.GetInstanceID() == enemyId) return collider;
            }
            return null;
        }
    }
}
