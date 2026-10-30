using System;
using System.Collections.Generic;

namespace ProjectZombie.Features.MetaProgression
{
    /// <summary>Commits currency and card progress together, restoring memory if persistence fails.</summary>
    public static class CardStarUpgradeTransaction
    {
        /// <summary>Applies one validated star step and persists the complete save once.</summary>
        public static bool TryCommit(MetaProgressionSaveData data, string cardId, bool isHero,
            int requiredShards, int cost, Func<MetaProgressionSaveData, bool> persist)
        {
            if (data == null || string.IsNullOrWhiteSpace(cardId) || persist == null
                || requiredShards < 0 || cost < 0 || data.totalCurrency < cost) return false;

            int star = isHero ? data.GetCharacterStarLevel(cardId) : data.GetRelicStarLevel(cardId);
            int shards = isHero ? data.GetCharacterShards(cardId) : data.GetRelicShards(cardId);
            if (star < 0 || star >= 5 || shards < requiredShards) return false;

            int previousCurrency = data.totalCurrency;
            var previousHeroes = data.characterProgressList;
            var previousRelics = data.relicProgressList;
            bool committed = false;
            try
            {
                // Copy only the changed collection so rollback preserves its original contents and identity.
                if (isHero)
                {
                    data.characterProgressList = previousHeroes == null
                        ? new List<CharacterProgressEntry>() : new List<CharacterProgressEntry>(previousHeroes);
                    data.SetCharacterProgress(cardId, shards - requiredShards, star + 1);
                }
                else
                {
                    data.relicProgressList = previousRelics == null
                        ? new List<RelicProgressEntry>() : new List<RelicProgressEntry>(previousRelics);
                    data.SetRelicProgress(cardId, shards - requiredShards, star + 1);
                }
                data.totalCurrency = previousCurrency - cost;
                committed = persist(data);
                return committed;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (!committed)
                {
                    data.totalCurrency = previousCurrency;
                    data.characterProgressList = previousHeroes;
                    data.relicProgressList = previousRelics;
                }
            }
        }
    }
}
