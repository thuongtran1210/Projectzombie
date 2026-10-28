using System;
using System.Collections.Generic;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.Loadout;

public static class DefenseLoadoutCoreChecks
{
    private static int _checks;

    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception("FAIL: " + name);
        _checks++;
        Console.WriteLine("PASS " + name);
    }

    public static void Main()
    {
        var catalog = new FakeCatalog();
        catalog.Add("hero_thusinh", LoadoutItemType.Hero, "Thư Sinh");
        catalog.Add("hero_kiemkhach", LoadoutItemType.Hero, "Kiếm Khách");
        catalog.Add("relic_deptoong", LoadoutItemType.Relic, "Dép Tổ Ong");
        catalog.Add("relic_quatchuathac", LoadoutItemType.Relic, "Quạt Chữa Thác");
        catalog.Add("minion_danbinh", LoadoutItemType.Minion, "Dân Binh");
        catalog.Add("minion_phutapdich", LoadoutItemType.Minion, "Phu Tạp Dịch");
        catalog.Add("minion_thaylang", LoadoutItemType.Minion, "Thầy Lang");
        catalog.Add("minion_xathu", LoadoutItemType.Minion, "Xạ Thủ");
        catalog.Add("tower_cungtien", LoadoutItemType.Tower, "Tháp Cung Tiễn");
        catalog.Add("tower_phaono", LoadoutItemType.Tower, "Tháp Pháo Nổ");

        var ownership = new FakeOwnership();
        ownership.Grant("hero_thusinh");
        ownership.Grant("relic_deptoong");
        ownership.Grant("minion_danbinh");
        ownership.Grant("minion_phutapdich");
        ownership.Grant("minion_thaylang");
        ownership.Grant("tower_cungtien");

        var defaultRules = LoadoutRules.Default;

        // 1. Valid selection passes validation
        var validSelection = new LoadoutSelection(
            "hero_thusinh",
            "relic_deptoong",
            new[] { "minion_danbinh", "minion_phutapdich" },
            new[] { "tower_cungtien" });

        var resValid = LoadoutValidator.Validate(validSelection, defaultRules, catalog, ownership);
        Check(resValid.IsValid && resValid.ErrorCode == LoadoutValidationErrorCode.None, "Valid selection passes validation");

        // 2. Missing hero fails validation
        var missingHero = new LoadoutSelection(null, "relic_deptoong", new[] { "minion_danbinh" }, new[] { "tower_cungtien" });
        var resMissingHero = LoadoutValidator.Validate(missingHero, defaultRules, catalog, ownership);
        Check(!resMissingHero.IsValid && resMissingHero.ErrorCode == LoadoutValidationErrorCode.MissingHero, "Missing hero fails validation");

        // 3. Missing relic fails validation when required
        var missingRelic = new LoadoutSelection("hero_thusinh", "", new[] { "minion_danbinh" }, new[] { "tower_cungtien" });
        var resMissingRelic = LoadoutValidator.Validate(missingRelic, defaultRules, catalog, ownership);
        Check(!resMissingRelic.IsValid && resMissingRelic.ErrorCode == LoadoutValidationErrorCode.MissingRelic, "Missing relic fails when required");

        // 4. Missing relic passes when optional by custom rules
        var optionalRelicRules = new LoadoutRules(requireRelic: false);
        var resOptRelic = LoadoutValidator.Validate(missingRelic, optionalRelicRules, catalog, ownership);
        Check(resOptRelic.IsValid, "Missing relic passes when optional");

        // 5. Invalid item type fails
        var invalidType = new LoadoutSelection("tower_cungtien", "relic_deptoong", new[] { "minion_danbinh" }, new[] { "tower_cungtien" });
        var resInvalidType = LoadoutValidator.Validate(invalidType, defaultRules, catalog, ownership);
        Check(!resInvalidType.IsValid && resInvalidType.ErrorCode == LoadoutValidationErrorCode.InvalidItemType, "Invalid item type in slot fails");

        // 6. Unknown item ID fails
        var unknownId = new LoadoutSelection("hero_unknown", "relic_deptoong", new[] { "minion_danbinh" }, new[] { "tower_cungtien" });
        var resUnknown = LoadoutValidator.Validate(unknownId, defaultRules, catalog, ownership);
        Check(!resUnknown.IsValid && resUnknown.ErrorCode == LoadoutValidationErrorCode.UnknownId, "Unknown ID fails");

        // 7. Unowned item fails
        var unowned = new LoadoutSelection("hero_kiemkhach", "relic_deptoong", new[] { "minion_danbinh" }, new[] { "tower_cungtien" });
        var resUnowned = LoadoutValidator.Validate(unowned, defaultRules, catalog, ownership);
        Check(!resUnowned.IsValid && resUnowned.ErrorCode == LoadoutValidationErrorCode.ItemNotOwned, "Unowned item fails");

        // 8. Duplicate minion is rejected by AddMinion
        var draftDups = new LoadoutSelection("hero_thusinh", "relic_deptoong");
        Check(draftDups.AddMinion("minion_danbinh"), "First minion add succeeds");
        Check(!draftDups.AddMinion("minion_danbinh"), "Duplicate minion add is rejected");

        // 9. Minion count out of range
        var tooFewMinions = new LoadoutSelection("hero_thusinh", "relic_deptoong", new string[0], new[] { "tower_cungtien" });
        var resTooFew = LoadoutValidator.Validate(tooFewMinions, defaultRules, catalog, ownership);
        Check(!resTooFew.IsValid && resTooFew.ErrorCode == LoadoutValidationErrorCode.MinionCountOutOfRange, "Too few minions fails");

        ownership.Grant("minion_xathu");
        var tooManyMinions = new LoadoutSelection("hero_thusinh", "relic_deptoong", new[] { "minion_danbinh", "minion_phutapdich", "minion_thaylang", "minion_xathu" }, new[] { "tower_cungtien" });
        var resTooMany = LoadoutValidator.Validate(tooManyMinions, defaultRules, catalog, ownership);
        Check(!resTooMany.IsValid && resTooMany.ErrorCode == LoadoutValidationErrorCode.MinionCountOutOfRange, "Too many minions fails");

        // 10. Immutable snapshot ignores subsequent draft mutation
        var draft = new LoadoutSelection("hero_thusinh", "relic_deptoong", new[] { "minion_danbinh", "minion_phutapdich" }, new[] { "tower_cungtien" });
        var snapshot = draft.ToSnapshot();
        draft.SetHero("hero_kiemkhach");
        draft.SetRelic("relic_quatchuathac");
        draft.AddMinion("minion_thaylang");
        draft.AddTower("tower_phaono");

        Check(snapshot.HeroId == "hero_thusinh", "Snapshot preserves original HeroId");
        Check(snapshot.RelicId == "relic_deptoong", "Snapshot preserves original RelicId");
        Check(snapshot.MinionIds.Count == 2 && snapshot.MinionIds[0] == "minion_danbinh" && snapshot.MinionIds[1] == "minion_phutapdich", "Snapshot preserves Minions");
        Check(snapshot.TowerIds.Count == 1 && snapshot.TowerIds[0] == "tower_cungtien", "Snapshot preserves Towers");

        // 11. Event firing semantics
        int eventsFired = 0;
        draft.OnChanged += () => eventsFired++;
        Check(!draft.SetHero("hero_kiemkhach"), "Setting same hero returns false");
        Check(eventsFired == 0, "No event on redundant SetHero");
        Check(draft.SetHero("hero_thusinh"), "Setting new hero returns true");
        Check(eventsFired == 1, "Event fires on valid SetHero");

        // 12. DTO Repository round-trip
        var repo = new FakeRepository();
        var originalDto = new LoadoutSaveDto("hero_thusinh", "relic_deptoong", new[] { "minion_danbinh" }, new[] { "tower_cungtien" }, version: 1);
        repo.Save(originalDto);
        var loadedDto = repo.Load();
        Check(loadedDto != null && loadedDto.Version == 1 && loadedDto.HeroId == "hero_thusinh" && loadedDto.MinionIds.Count == 1, "Repository round-trip preserves DTO");

        // 13. Default Snapshot
        var defSnap = DefenseLoadoutSnapshot.CreateDefault();
        Check(defSnap.HeroId == "hero_thusinh" && defSnap.RelicId == "relic_deptoong" && defSnap.MinionIds.Count == 3 && defSnap.TowerIds.Count == 1, "Default snapshot has baseline entities");

        // 14. Defense3DCatalog baseline entities
        var baselineCatalog = Defense3DCatalog.CreateDefault();
        Check(baselineCatalog.Contains(Defense3DCatalog.HERO_THU_SINH), "Baseline catalog has Thu Sinh");
        Check(baselineCatalog.Contains(Defense3DCatalog.RELIC_DEP_TO_ONG), "Baseline catalog has Dep To Ong");
        Check(baselineCatalog.GetEntriesByType(LoadoutItemType.Minion).Count == 3, "Baseline catalog has 3 minions");

        // 15. Resolver TryGetMinionRole mapping
        Check(DefenseLoadoutResolver.TryGetMinionRole(Defense3DCatalog.MINION_DAN_BINH, out int roleMilitia) && roleMilitia == 0, "Dan Binh maps to Militia (0)");
        Check(DefenseLoadoutResolver.TryGetMinionRole(Defense3DCatalog.MINION_PHU_TAP_DICH, out int roleGatherer) && roleGatherer == 1, "Phu Tap Dich maps to Gatherer (1)");
        Check(DefenseLoadoutResolver.TryGetMinionRole(Defense3DCatalog.MINION_THAY_LANG, out int roleHealer) && roleHealer == 2, "Thay Lang maps to Healer (2)");
        Check(!DefenseLoadoutResolver.TryGetMinionRole("unknown", out _), "Unknown minion returns false");

        // 16. Resolver null fallback
        var resolvedDefault = DefenseLoadoutResolver.Resolve(null);
        Check(resolvedDefault != null && resolvedDefault.HeroId == Defense3DCatalog.HERO_THU_SINH && resolvedDefault.AllowedMinionRoles.Count == 3, "Resolver handles null with safe default");

        // 17. Resolver specific minions maps allowed roles
        var customSnapshot = new DefenseLoadoutSnapshot(
            Defense3DCatalog.HERO_THU_SINH,
            Defense3DCatalog.RELIC_DEP_TO_ONG,
            new[] { Defense3DCatalog.MINION_DAN_BINH, Defense3DCatalog.MINION_THAY_LANG },
            new[] { Defense3DCatalog.TOWER_CUNG_TIEN });
        var resolvedCustom = DefenseLoadoutResolver.Resolve(customSnapshot);
        Check(resolvedCustom.AllowedMinionRoles.Count == 2 && resolvedCustom.IsMinionRoleAllowed(0) && resolvedCustom.IsMinionRoleAllowed(2) && !resolvedCustom.IsMinionRoleAllowed(1), "Resolver correctly filters allowed minion roles");

        // 18. ID Mapper Hero aliases
        Check(DefenseLoadoutIdMapper.MapHeroId("C001_ThuSinh") == Defense3DCatalog.HERO_THU_SINH, "C001_ThuSinh maps to hero_thusinh");
        Check(DefenseLoadoutIdMapper.MapHeroId("unknown") == Defense3DCatalog.HERO_THU_SINH, "Unknown hero maps to default hero_thusinh");

        // 19. ID Mapper Relic aliases
        Check(DefenseLoadoutIdMapper.MapRelicId("R001_Deptoong") == Defense3DCatalog.RELIC_DEP_TO_ONG, "R001_Deptoong maps to relic_deptoong");
        Check(DefenseLoadoutIdMapper.MapRelicId("unknown") == Defense3DCatalog.RELIC_DEP_TO_ONG, "Unknown relic maps to default relic_deptoong");

        // 20. ID Mapper Minions / Towers defaults
        var mappedMinions = DefenseLoadoutIdMapper.MapMinionIds(new string[0]);
        Check(mappedMinions.Count == 3, "Empty minions mapped to 3 default minions");
        var mappedTowers = DefenseLoadoutIdMapper.MapTowerIds(null);
        Check(mappedTowers.Count == 1 && mappedTowers[0] == Defense3DCatalog.TOWER_CUNG_TIEN, "Empty towers mapped to default tower");

        // 21. Ownership Adapter Baseline
        var defaultOwnership = DefenseMetaOwnershipAdapter.CreateDefault();
        Check(defaultOwnership.IsOwned(Defense3DCatalog.HERO_THU_SINH), "Hero Thu Sinh is owned by default");
        Check(defaultOwnership.IsOwned(Defense3DCatalog.RELIC_DEP_TO_ONG), "Relic Dep To Ong is owned by default");
        Check(defaultOwnership.IsOwned(Defense3DCatalog.MINION_DAN_BINH), "Minion Dan Binh is owned by default");
        Check(defaultOwnership.IsOwned(Defense3DCatalog.TOWER_CUNG_TIEN), "Tower Cung Tien is owned by default");
        Check(!defaultOwnership.IsOwned("unregistered_secret"), "Unregistered item is not owned");

        // 22. Navigation Target Scene Name is explicit string
        Check(DefenseMatchNavigation.TARGET_SCENE_NAME == "MapSongQue3D_Prototype", "Target scene is explicit string 'MapSongQue3D_Prototype'");
        Check(!int.TryParse(DefenseMatchNavigation.TARGET_SCENE_NAME, out _), "Target scene is not an index integer");

        // 23. Session Context Consume Default
        DefenseSessionContext.Clear();
        var defSession = DefenseSessionContext.ConsumeSnapshot();
        Check(defSession != null && defSession.HeroId == Defense3DCatalog.HERO_THU_SINH, "SessionContext consumes default when empty");
        Check(DefenseSessionContext.PendingSnapshot == null, "SessionContext clears after consume");

        // 24. Presenter Validation with Defaults
        var presCatalog = Defense3DCatalog.CreateDefault();
        var presRepo = new FakeRepository();
        presRepo.Save(new LoadoutSaveDto(Defense3DCatalog.HERO_THU_SINH, Defense3DCatalog.RELIC_DEP_TO_ONG, new[] { Defense3DCatalog.MINION_DAN_BINH }, new[] { Defense3DCatalog.TOWER_CUNG_TIEN }));
        using (var presenter = new ProjectZombie.Features.Defense3D.UI.DefensePreparationPresenter(presCatalog, defaultOwnership, presRepo))
        {
            Check(presenter.CanStartBattle, "Presenter can start battle with valid selection");

            // 25. Presenter Toggle Minion
            presenter.ToggleMinion(Defense3DCatalog.MINION_DAN_BINH);
            Check(!presenter.CanStartBattle, "Presenter cannot start battle when minion count is 0");
        }

        // 26. G5 - ResultService victory rewards
        DefenseMatchResultService.LogWarningHook = msg => { };
        DefenseMatchResultService.LogErrorHook = msg => { };
        var resultService = new DefenseMatchResultService();
        var testSave = new ProjectZombie.Features.MetaProgression.MetaProgressionSaveData { totalCurrency = 100 };
        var matchWon = new DefenseMatchResultData(true, 300f, 50, 40, "STAGE_01");
        bool saveCalled = false;
        var wonResult = resultService.ProcessMatchResult(matchWon, testSave, _ => saveCalled = true);
        Check(resultService.HasProcessedResult, "ResultService marks processed after match");
        Check(wonResult.IsWon, "ResultService preserves won flag");
        Check(wonResult.EarnedCurrency == 600, "ResultService awards wood*2 + 500 bonus = 600");
        Check(testSave.totalCurrency == 700, "ResultService adds earned currency to SaveData");
        Check(testSave.completedStages.Contains("STAGE_01"), "ResultService marks stage completed on victory");
        Check(saveCalled, "ResultService invokes save callback");

        // 27. G5 - ResultService Idempotency Guard
        var secondRun = resultService.ProcessMatchResult(matchWon, testSave, _ => throw new Exception("Should not save twice"));
        Check(object.ReferenceEquals(wonResult, secondRun), "ResultService returns cached result on duplicate call");
        Check(testSave.totalCurrency == 700, "ResultService does not award currency twice");

        // 28. G5 - ResultService Reset
        resultService.Reset();
        Check(!resultService.HasProcessedResult && resultService.LastProcessedResult == null, "ResultService resets state properly");

        // 29. G5 - Navigation explicit hub scene
        Check(DefenseNavigationService.HUB_SCENE_NAME == "SampleScene", "Navigation hub scene is explicit SampleScene");
        Check(!int.TryParse(DefenseNavigationService.HUB_SCENE_NAME, out _), "Navigation hub scene is not numeric index");

        // 30. G5 - Navigation debounce guard
        var navService = DefenseNavigationService.Default;
        navService.ResetStateForTesting();
        Check(!navService.IsNavigating, "Navigation service initial state is not navigating");

        Console.WriteLine($"All {_checks} DefenseLoadoutCoreChecks PASSED!");
    }

    private sealed class FakeCatalog : ILoadoutCatalog
    {
        private readonly Dictionary<string, LoadoutCatalogEntry> _entries = new Dictionary<string, LoadoutCatalogEntry>(StringComparer.OrdinalIgnoreCase);
        public void Add(string id, LoadoutItemType type, string displayName, bool isAvailable = true) => _entries[id] = new LoadoutCatalogEntry(id, type, displayName, isAvailable);
        public bool Contains(string id) => !string.IsNullOrEmpty(id) && _entries.ContainsKey(id);
        public bool TryGetEntry(string id, out LoadoutCatalogEntry entry) => _entries.TryGetValue(id ?? string.Empty, out entry);
        public IReadOnlyList<LoadoutCatalogEntry> GetEntriesByType(LoadoutItemType type)
        {
            var list = new List<LoadoutCatalogEntry>();
            foreach (var e in _entries.Values) if (e.Type == type) list.Add(e);
            return list;
        }
    }

    private sealed class FakeOwnership : ILoadoutOwnership
    {
        private readonly HashSet<string> _ownedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public void Grant(string id) => _ownedIds.Add(id);
        public bool IsOwned(string id) => !string.IsNullOrEmpty(id) && _ownedIds.Contains(id);
    }

    private sealed class FakeRepository : ILoadoutRepository
    {
        private LoadoutSaveDto _saved;
        public LoadoutSaveDto Load() => _saved;
        public void Save(LoadoutSaveDto dto) => _saved = dto;
    }
}
