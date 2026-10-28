using System;
using System.Collections.Generic;

namespace ProjectZombie.Features.Defense3D.Loadout
{
    /// <summary>
    /// Mã lỗi định danh khi kiểm tra tính hợp lệ của cấu hình Loadout.
    /// UI Presenter dựa vào mã lỗi này để hiển thị thông báo bản địa hóa phù hợp.
    /// </summary>
    public enum LoadoutValidationErrorCode
    {
        None = 0,
        SelectionNull,
        MissingHero,
        MissingRelic,
        UnknownId,
        InvalidItemType,
        ItemNotAvailable,
        ItemNotOwned,
        DuplicateMinion,
        DuplicateTower,
        MinionCountOutOfRange,
        TowerCountOutOfRange
    }

    /// <summary>
    /// Kết quả kiểm tra tính hợp lệ của Loadout (Validation Result).
    /// </summary>
    public sealed class LoadoutValidationResult
    {
        public bool IsValid { get; }
        public LoadoutValidationErrorCode ErrorCode { get; }
        public string ErrorMessage { get; }

        private LoadoutValidationResult(bool isValid, LoadoutValidationErrorCode errorCode, string errorMessage)
        {
            IsValid = isValid;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage ?? string.Empty;
        }

        public static LoadoutValidationResult Success() =>
            new LoadoutValidationResult(true, LoadoutValidationErrorCode.None, string.Empty);

        public static LoadoutValidationResult Fail(LoadoutValidationErrorCode errorCode, string message) =>
            new LoadoutValidationResult(false, errorCode, message);
    }

    /// <summary>
    /// Bộ thẩm định tính hợp lệ của Loadout C# thuần (Pure C# Validator).
    /// </summary>
    public static class LoadoutValidator
    {
        public static LoadoutValidationResult Validate(
            DefenseLoadoutSnapshot snapshot,
            ILoadoutCatalog catalog,
            ILoadoutOwnership ownership = null)
        {
            return Validate(snapshot, LoadoutRules.Default, catalog, ownership);
        }

        public static LoadoutValidationResult Validate(
            DefenseLoadoutSnapshot snapshot,
            LoadoutRules rules,
            ILoadoutCatalog catalog,
            ILoadoutOwnership ownership = null)
        {
            if (snapshot == null)
                return LoadoutValidationResult.Fail(LoadoutValidationErrorCode.SelectionNull, "DefenseLoadoutSnapshot không được để null.");

            var selection = new LoadoutSelection(snapshot.HeroId, snapshot.RelicId, snapshot.MinionIds, snapshot.TowerIds);
            return Validate(selection, rules, catalog, ownership);
        }

        public static LoadoutValidationResult Validate(
            LoadoutSelection selection,
            LoadoutRules rules,
            ILoadoutCatalog catalog,
            ILoadoutOwnership ownership = null)
        {
            if (selection == null)
                return LoadoutValidationResult.Fail(LoadoutValidationErrorCode.SelectionNull, "LoadoutSelection không được để null.");

            rules ??= LoadoutRules.Default;

            // 1. Kiểm tra Hero
            if (rules.RequireHero && string.IsNullOrWhiteSpace(selection.HeroId))
                return LoadoutValidationResult.Fail(LoadoutValidationErrorCode.MissingHero, "Bắt buộc phải chọn một Tướng xuất trận.");

            if (!string.IsNullOrWhiteSpace(selection.HeroId))
            {
                var heroResult = ValidateItem(selection.HeroId, LoadoutItemType.Hero, catalog, ownership);
                if (!heroResult.IsValid) return heroResult;
            }

            // 2. Kiểm tra Relic (Pháp bảo)
            if (rules.RequireRelic && string.IsNullOrWhiteSpace(selection.RelicId))
                return LoadoutValidationResult.Fail(LoadoutValidationErrorCode.MissingRelic, "Bắt buộc phải chọn một Pháp Bảo xuất trận.");

            if (!string.IsNullOrWhiteSpace(selection.RelicId))
            {
                var relicResult = ValidateItem(selection.RelicId, LoadoutItemType.Relic, catalog, ownership);
                if (!relicResult.IsValid) return relicResult;
            }

            // 3. Kiểm tra danh mục Lính tuyển chọn
            int minionCount = selection.MinionIds.Count;
            if (minionCount < rules.MinMinionCount || minionCount > rules.MaxMinionCount)
            {
                return LoadoutValidationResult.Fail(
                    LoadoutValidationErrorCode.MinionCountOutOfRange,
                    $"Số loại lính tuyển chọn phải từ {rules.MinMinionCount} đến {rules.MaxMinionCount} (Hiện tại: {minionCount}).");
            }

            var seenMinions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var minionId in selection.MinionIds)
            {
                if (string.IsNullOrWhiteSpace(minionId))
                    return LoadoutValidationResult.Fail(LoadoutValidationErrorCode.UnknownId, "ID lính không được để trống.");

                if (!seenMinions.Add(minionId))
                {
                    return LoadoutValidationResult.Fail(
                        LoadoutValidationErrorCode.DuplicateMinion,
                        $"Loại lính '{minionId}' bị chọn trùng lặp.");
                }

                var minionResult = ValidateItem(minionId, LoadoutItemType.Minion, catalog, ownership);
                if (!minionResult.IsValid) return minionResult;
            }

            // 4. Kiểm tra danh mục Trụ xây dựng
            int towerCount = selection.TowerIds.Count;
            if (towerCount < rules.MinTowerCount || towerCount > rules.MaxTowerCount)
            {
                return LoadoutValidationResult.Fail(
                    LoadoutValidationErrorCode.TowerCountOutOfRange,
                    $"Số loại trụ mang theo phải từ {rules.MinTowerCount} đến {rules.MaxTowerCount} (Hiện tại: {towerCount}).");
            }

            var seenTowers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var towerId in selection.TowerIds)
            {
                if (string.IsNullOrWhiteSpace(towerId))
                    return LoadoutValidationResult.Fail(LoadoutValidationErrorCode.UnknownId, "ID trụ không được để trống.");

                if (!seenTowers.Add(towerId))
                {
                    return LoadoutValidationResult.Fail(
                        LoadoutValidationErrorCode.DuplicateTower,
                        $"Loại trụ '{towerId}' bị chọn trùng lặp.");
                }

                var towerResult = ValidateItem(towerId, LoadoutItemType.Tower, catalog, ownership);
                if (!towerResult.IsValid) return towerResult;
            }

            return LoadoutValidationResult.Success();
        }

        private static LoadoutValidationResult ValidateItem(
            string id,
            LoadoutItemType expectedType,
            ILoadoutCatalog catalog,
            ILoadoutOwnership ownership)
        {
            if (catalog == null) return LoadoutValidationResult.Success();

            if (!catalog.TryGetEntry(id, out var entry))
            {
                return LoadoutValidationResult.Fail(
                    LoadoutValidationErrorCode.UnknownId,
                    $"Không tìm thấy thực thể '{id}' trong Catalog.");
            }

            if (entry.Type != expectedType)
            {
                return LoadoutValidationResult.Fail(
                    LoadoutValidationErrorCode.InvalidItemType,
                    $"Thực thể '{id}' có kiểu {entry.Type}, không khớp với kiểu yêu cầu {expectedType}.");
            }

            if (!entry.IsAvailable)
            {
                return LoadoutValidationResult.Fail(
                    LoadoutValidationErrorCode.ItemNotAvailable,
                    $"Thực thể '{id}' hiện không khả dụng trong phiên bản trò chơi này.");
            }

            if (ownership != null && !ownership.IsOwned(id))
            {
                return LoadoutValidationResult.Fail(
                    LoadoutValidationErrorCode.ItemNotOwned,
                    $"Người chơi chưa sở hữu thực thể '{id}'.");
            }

            return LoadoutValidationResult.Success();
        }
    }
}
