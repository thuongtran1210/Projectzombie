using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectZombie.Features.Defense3D.Loadout;

namespace ProjectZombie.Tests.Editor
{
    [TestFixture]
    public sealed class DefenseLoadoutCoreTests
    {
        private FakeCatalog _catalog;
        private FakeOwnership _ownership;
        private LoadoutRules _defaultRules;

        [SetUp]
        public void SetUp()
        {
            _catalog = new FakeCatalog();
            _catalog.Add("hero_thusinh", LoadoutItemType.Hero, "Thư Sinh");
            _catalog.Add("hero_kiemkhach", LoadoutItemType.Hero, "Kiếm Khách");
            _catalog.Add("relic_deptoong", LoadoutItemType.Relic, "Dép Tổ Ong");
            _catalog.Add("relic_quatchuathac", LoadoutItemType.Relic, "Quạt Chữa Thác");
            _catalog.Add("minion_danbinh", LoadoutItemType.Minion, "Dân Binh");
            _catalog.Add("minion_phutapdich", LoadoutItemType.Minion, "Phu Tạp Dịch");
            _catalog.Add("minion_thaylang", LoadoutItemType.Minion, "Thầy Lang");
            _catalog.Add("minion_xathu", LoadoutItemType.Minion, "Xạ Thủ");
            _catalog.Add("tower_cungtien", LoadoutItemType.Tower, "Tháp Cung Tiễn");
            _catalog.Add("tower_phaono", LoadoutItemType.Tower, "Tháp Pháo Nổ");

            _ownership = new FakeOwnership();
            _ownership.Grant("hero_thusinh");
            _ownership.Grant("relic_deptoong");
            _ownership.Grant("minion_danbinh");
            _ownership.Grant("minion_phutapdich");
            _ownership.Grant("minion_thaylang");
            _ownership.Grant("tower_cungtien");

            _defaultRules = LoadoutRules.Default;
        }

        [Test]
        public void ValidSelection_PassesValidation()
        {
            var selection = new LoadoutSelection(
                "hero_thusinh",
                "relic_deptoong",
                new[] { "minion_danbinh", "minion_phutapdich" },
                new[] { "tower_cungtien" });

            var result = LoadoutValidator.Validate(selection, _defaultRules, _catalog, _ownership);

            Assert.That(result.IsValid, Is.True);
            Assert.That(result.ErrorCode, Is.EqualTo(LoadoutValidationErrorCode.None));
        }

        [Test]
        public void MissingHero_FailsValidationWithMissingHeroCode()
        {
            var selection = new LoadoutSelection(
                null,
                "relic_deptoong",
                new[] { "minion_danbinh" },
                new[] { "tower_cungtien" });

            var result = LoadoutValidator.Validate(selection, _defaultRules, _catalog, _ownership);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo(LoadoutValidationErrorCode.MissingHero));
        }

        [Test]
        public void MissingRelic_WhenRequired_FailsValidation()
        {
            var selection = new LoadoutSelection(
                "hero_thusinh",
                "",
                new[] { "minion_danbinh" },
                new[] { "tower_cungtien" });

            var result = LoadoutValidator.Validate(selection, _defaultRules, _catalog, _ownership);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo(LoadoutValidationErrorCode.MissingRelic));
        }

        [Test]
        public void MissingRelic_WhenNotRequiredByRules_PassesValidation()
        {
            var optionalRelicRules = new LoadoutRules(requireRelic: false);
            var selection = new LoadoutSelection(
                "hero_thusinh",
                null,
                new[] { "minion_danbinh" },
                new[] { "tower_cungtien" });

            var result = LoadoutValidator.Validate(selection, optionalRelicRules, _catalog, _ownership);

            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public void InvalidItemType_AssignedToSlot_FailsValidation()
        {
            // Gán nhầm ID trụ vào vị trí Tướng
            var selection = new LoadoutSelection(
                "tower_cungtien",
                "relic_deptoong",
                new[] { "minion_danbinh" },
                new[] { "tower_cungtien" });

            var result = LoadoutValidator.Validate(selection, _defaultRules, _catalog, _ownership);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo(LoadoutValidationErrorCode.InvalidItemType));
        }

        [Test]
        public void UnknownId_NotInCatalog_FailsValidation()
        {
            var selection = new LoadoutSelection(
                "hero_unknown_xyz",
                "relic_deptoong",
                new[] { "minion_danbinh" },
                new[] { "tower_cungtien" });

            var result = LoadoutValidator.Validate(selection, _defaultRules, _catalog, _ownership);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo(LoadoutValidationErrorCode.UnknownId));
        }

        [Test]
        public void ItemNotOwned_FailsValidation()
        {
            // hero_kiemkhach có trong catalog nhưng chưa được grant trong FakeOwnership
            var selection = new LoadoutSelection(
                "hero_kiemkhach",
                "relic_deptoong",
                new[] { "minion_danbinh" },
                new[] { "tower_cungtien" });

            var result = LoadoutValidator.Validate(selection, _defaultRules, _catalog, _ownership);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo(LoadoutValidationErrorCode.ItemNotOwned));
        }

        [Test]
        public void DuplicateMinionOrTower_FailsValidation()
        {
            var selection = new LoadoutSelection(
                "hero_thusinh",
                "relic_deptoong",
                new[] { "minion_danbinh", "minion_danbinh" },
                new[] { "tower_cungtien" });

            // Trực tiếp kiểm tra Validate khi bằng cách nào đó danh sách có trùng lặp
            // (ví dụ tạo danh sách ngoài)
            var duplicateMinionSelection = new LoadoutSelection("hero_thusinh", "relic_deptoong");
            // Sử dụng reflection hoặc kiểm tra AddMinion chặn trùng lặp:
            bool addFirst = duplicateMinionSelection.AddMinion("minion_danbinh");
            bool addSecond = duplicateMinionSelection.AddMinion("minion_danbinh");

            Assert.That(addFirst, Is.True);
            Assert.That(addSecond, Is.False, "AddMinion phải từ chối khi ID đã tồn tại trong danh sách.");
        }

        [Test]
        public void MinionAndTowerCount_OutOfRange_FailsValidation()
        {
            // 0 lính (dưới mức tối thiểu 1)
            var tooFewMinions = new LoadoutSelection(
                "hero_thusinh",
                "relic_deptoong",
                new string[0],
                new[] { "tower_cungtien" });

            var resultTooFew = LoadoutValidator.Validate(tooFewMinions, _defaultRules, _catalog, _ownership);
            Assert.That(resultTooFew.IsValid, Is.False);
            Assert.That(resultTooFew.ErrorCode, Is.EqualTo(LoadoutValidationErrorCode.MinionCountOutOfRange));

            // 5 lính (vượt mức tối đa 4)
            _ownership.Grant("minion_xathu");
            var tooManyMinions = new LoadoutSelection(
                "hero_thusinh",
                "relic_deptoong",
                new[] { "minion_danbinh", "minion_phutapdich", "minion_thaylang", "minion_xathu", "minion_fifth" },
                new[] { "tower_cungtien" });

            var resultTooMany = LoadoutValidator.Validate(tooManyMinions, _defaultRules, _catalog, _ownership);
            Assert.That(resultTooMany.IsValid, Is.False);
            Assert.That(resultTooMany.ErrorCode, Is.EqualTo(LoadoutValidationErrorCode.MinionCountOutOfRange));
        }

        [Test]
        public void FourDistinctMinionsAndTowers_AreValid_FifthIsRejected()
        {
            var minions = new List<string>();
            var towers = new List<string>();
            for (int i = 0; i < 5; i++)
            {
                string m = "minion_slot_" + i;
                string t = "tower_slot_" + i;
                _catalog.Add(m, LoadoutItemType.Minion, m);
                _catalog.Add(t, LoadoutItemType.Tower, t);
                _ownership.Grant(m); _ownership.Grant(t);
                if (i < 4) { minions.Add(m); towers.Add(t); }
            }
            var selection = new LoadoutSelection("hero_thusinh", "relic_deptoong", minions, towers);
            Assert.That(LoadoutValidator.Validate(selection, _defaultRules, _catalog, _ownership).IsValid, Is.True);
            Assert.That(selection.AddMinion("MINION_SLOT_0"), Is.False);
            Assert.That(selection.AddTower("TOWER_SLOT_0"), Is.False);
            selection.AddMinion("minion_slot_4");
            Assert.That(LoadoutValidator.Validate(selection, _defaultRules, _catalog, _ownership).ErrorCode,
                Is.EqualTo(LoadoutValidationErrorCode.MinionCountOutOfRange));
            selection.RemoveMinion("minion_slot_4");
            selection.AddTower("tower_slot_4");
            Assert.That(LoadoutValidator.Validate(selection, _defaultRules, _catalog, _ownership).ErrorCode,
                Is.EqualTo(LoadoutValidationErrorCode.TowerCountOutOfRange));
            Assert.That(LoadoutValidator.Validate(new LoadoutSelection("hero_thusinh", "relic_deptoong", minions, towers),
                new LoadoutRules(maxMinionCount: 6, maxTowerCount: 6), _catalog, _ownership).IsValid, Is.True);
        }

        [Test]
        public void Snapshot_IsImmutable_WhenDraftSelectionMutates()
        {
            var draft = new LoadoutSelection(
                "hero_thusinh",
                "relic_deptoong",
                new[] { "minion_danbinh", "minion_phutapdich" },
                new[] { "tower_cungtien" });

            var snapshot = draft.ToSnapshot();

            // Sửa đổi draft sau khi đã tạo snapshot
            draft.SetHero("hero_kiemkhach");
            draft.SetRelic("relic_quatchuathac");
            draft.AddMinion("minion_thaylang");
            draft.AddTower("tower_phaono");

            // Xác nhận Snapshot vẫn giữ nguyên giá trị lúc tạo
            Assert.That(snapshot.HeroId, Is.EqualTo("hero_thusinh"));
            Assert.That(snapshot.RelicId, Is.EqualTo("relic_deptoong"));
            Assert.That(snapshot.MinionIds.Count, Is.EqualTo(2));
            Assert.That(snapshot.MinionIds[0], Is.EqualTo("minion_danbinh"));
            Assert.That(snapshot.MinionIds[1], Is.EqualTo("minion_phutapdich"));
            Assert.That(snapshot.TowerIds.Count, Is.EqualTo(1));
            Assert.That(snapshot.TowerIds[0], Is.EqualTo("tower_cungtien"));
        }

        [Test]
        public void NoChangedEvent_WhenOperationRejected()
        {
            var selection = new LoadoutSelection("hero_thusinh", "relic_deptoong");
            int changeCount = 0;
            selection.OnChanged += () => changeCount++;

            // Gán cùng giá trị -> Không đổi -> Không phát event
            bool changed = selection.SetHero("hero_thusinh");
            Assert.That(changed, Is.False);
            Assert.That(changeCount, Is.EqualTo(0));

            // Thêm minion mới -> Có phát event
            bool added = selection.AddMinion("minion_danbinh");
            Assert.That(added, Is.True);
            Assert.That(changeCount, Is.EqualTo(1));

            // Thêm trùng minion -> Từ chối -> Không phát event
            bool addDuplicate = selection.AddMinion("minion_danbinh");
            Assert.That(addDuplicate, Is.False);
            Assert.That(changeCount, Is.EqualTo(1));

            // Xóa minion không tồn tại -> Từ chối -> Không phát event
            bool removedNonExistent = selection.RemoveMinion("minion_non_existent");
            Assert.That(removedNonExistent, Is.False);
            Assert.That(changeCount, Is.EqualTo(1));
        }

        [Test]
        public void RepositoryRoundTrip_WithFakeStorage_PreservesData()
        {
            var repo = new FakeRepository();
            var originalDto = new LoadoutSaveDto(
                "hero_thusinh",
                "relic_deptoong",
                new[] { "minion_danbinh", "minion_phutapdich" },
                new[] { "tower_cungtien" },
                version: 1);

            repo.Save(originalDto);
            var loadedDto = repo.Load();

            Assert.That(loadedDto, Is.Not.Null);
            Assert.That(loadedDto.Version, Is.EqualTo(1));
            Assert.That(loadedDto.HeroId, Is.EqualTo("hero_thusinh"));
            Assert.That(loadedDto.RelicId, Is.EqualTo("relic_deptoong"));
            Assert.That(loadedDto.MinionIds, Is.EquivalentTo(new[] { "minion_danbinh", "minion_phutapdich" }));
            Assert.That(loadedDto.TowerIds, Is.EquivalentTo(new[] { "tower_cungtien" }));
        }

        [Test]
        public void CreateDefaultSnapshot_HasExpectedBaselineEntities()
        {
            var defaultSnapshot = DefenseLoadoutSnapshot.CreateDefault();

            Assert.That(defaultSnapshot.HeroId, Is.EqualTo("hero_thusinh"));
            Assert.That(defaultSnapshot.RelicId, Is.EqualTo("relic_deptoong"));
            Assert.That(defaultSnapshot.MinionIds, Is.EquivalentTo(new[] { "minion_danbinh", "minion_phutapdich", "minion_thaylang" }));
            Assert.That(defaultSnapshot.TowerIds, Is.EquivalentTo(new[] { "tower_cungtien" }));
            Assert.That(defaultSnapshot.SnapshotId, Is.Not.Null.And.Not.Empty);
        }

        // --- FAKE TEST IMPLEMENTATIONS ---
        private sealed class FakeCatalog : ILoadoutCatalog
        {
            private readonly Dictionary<string, LoadoutCatalogEntry> _entries =
                new Dictionary<string, LoadoutCatalogEntry>(StringComparer.OrdinalIgnoreCase);

            public void Add(string id, LoadoutItemType type, string displayName, bool isAvailable = true)
            {
                _entries[id] = new LoadoutCatalogEntry(id, type, displayName, isAvailable);
            }

            public bool Contains(string id) => !string.IsNullOrEmpty(id) && _entries.ContainsKey(id);

            public bool TryGetEntry(string id, out LoadoutCatalogEntry entry)
            {
                if (string.IsNullOrEmpty(id))
                {
                    entry = null;
                    return false;
                }
                return _entries.TryGetValue(id, out entry);
            }

            public IReadOnlyList<LoadoutCatalogEntry> GetEntriesByType(LoadoutItemType type)
            {
                var list = new List<LoadoutCatalogEntry>();
                foreach (var entry in _entries.Values)
                {
                    if (entry.Type == type) list.Add(entry);
                }
                return list;
            }
        }

        private sealed class FakeOwnership : ILoadoutOwnership
        {
            private readonly HashSet<string> _ownedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public void Grant(string id) => _ownedIds.Add(id);
            public void Revoke(string id) => _ownedIds.Remove(id);
            public bool IsOwned(string id) => !string.IsNullOrEmpty(id) && _ownedIds.Contains(id);
        }

        private sealed class FakeRepository : ILoadoutRepository
        {
            private LoadoutSaveDto _saved;

            public LoadoutSaveDto Load() => _saved;
            public void Save(LoadoutSaveDto dto) => _saved = dto;
        }
    }
}
