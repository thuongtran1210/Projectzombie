#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using ProjectZombie.Features.MetaProgression;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Core.Save;
using ProjectZombie.Features.Defense3D.Loadout;
using ProjectZombie.Features.Defense3D;

/// <summary>Validates upgrade transactions with isolated memory data and persistence callbacks.</summary>
public static class CardProgressionValidation
{
    /// <summary>Runs without touching player saves, scenes, singletons or the active save provider.</summary>
    [MenuItem("ProjectZombie/UI Tools/Validate Card Upgrade Transactions")]
    public static string Validate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Run isolated card validation in Edit Mode.");
        int checks = 0;
        foreach (bool hero in new[] { true, false })
        {
            foreach (int initialStar in new[] { 0, 1, 4 })
            {
                var data = Create(hero, initialStar);
                int writes = 0;
                bool result = CardStarUpgradeTransaction.TryCommit(data, "test_card", hero, 10, 500, saved =>
                {
                    writes++;
                    Require(saved.totalCurrency == 500 && Star(saved, hero) == initialStar + 1
                        && Shards(saved, hero) == 10, "Persistence saw an incomplete transaction");
                    return true;
                });
                Require(result && writes == 1 && Star(data, hero) == initialStar + 1, "Successful upgrade");
                checks++;
            }

            foreach (bool throws in new[] { false, true })
            {
                var data = Create(hero, 1);
                var heroes = data.characterProgressList;
                var relics = data.relicProgressList;
                int writes = 0;
                bool result = CardStarUpgradeTransaction.TryCommit(data, "test_card", hero, 10, 500, saved =>
                {
                    writes++;
                    if (throws) throw new IOException("Simulated persistence failure");
                    return false;
                });
                Require(!result && writes == 1 && data.totalCurrency == 1000 && Star(data, hero) == 1
                    && Shards(data, hero) == 20 && ReferenceEquals(heroes, data.characterProgressList)
                    && ReferenceEquals(relics, data.relicProgressList), "Rollback after failed persistence");
                checks++;
            }

            foreach (string failure in new[] { "currency", "shards", "max", "negativeCost", "negativeShards" })
            {
                var data = Create(hero, failure == "max" ? 5 : 1);
                if (failure == "currency") data.totalCurrency = 499;
                int writes = 0;
                int previousCurrency = data.totalCurrency;
                int previousStar = Star(data, hero);
                bool result = CardStarUpgradeTransaction.TryCommit(data, "test_card", hero,
                    failure == "negativeShards" ? -1 : failure == "shards" ? 21 : 10,
                    failure == "negativeCost" ? -1 : 500, saved => { writes++; return true; });
                Require(!result && writes == 0 && data.totalCurrency == previousCurrency
                    && Star(data, hero) == previousStar && Shards(data, hero) == 20, "Rejected " + failure);
                checks++;
            }

            var exact = Create(hero, 4);
            exact.totalCurrency = 500;
            Require(CardStarUpgradeTransaction.TryCommit(exact, "test_card", hero, 20, 500, saved => true)
                && exact.totalCurrency == 0 && Shards(exact, hero) == 0 && Star(exact, hero) == 5,
                "Exact resources and max star");
            checks++;

            var empty = new MetaProgressionSaveData();
            empty.characterProgressList = null;
            empty.relicProgressList = null;
            Require(!CardStarUpgradeTransaction.TryCommit(empty, "test_card", hero, 0, 0, saved => false)
                && empty.characterProgressList == null && empty.relicProgressList == null,
                "Rollback restores absent collections");
            checks++;
        }
        string fixtureRoot = Path.GetFullPath(Path.Combine("scratch", "CardProgressionValidation", Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(fixtureRoot);
        string[] ids = { DefenseLoadoutIdMapper.GetMetaCharacterId(Defense3DCatalog.HERO_THU_SINH),
            Defense3DCatalog.RELIC_DEP_TO_ONG, Defense3DCatalog.MINION_DAN_BINH, Defense3DCatalog.TOWER_CUNG_TIEN };
        for (int i = 0; i < ids.Length; i++)
        {
            bool hero = i == 0;
            string id = ids[i];
            var data = new MetaProgressionSaveData { totalCurrency = 1000 };
            if (hero) data.SetCharacterProgress(id, 20, 1);
            else data.SetRelicProgress(id, 20, 1);
            var provider = new LocalJsonSaveProvider(Path.Combine(fixtureRoot, "card_" + i + ".json"));
            Require(CardStarUpgradeTransaction.TryCommit(data, id, hero, 10, 500, provider.Save), "JSON commit " + id);
            var loaded = provider.Load();
            Require(loaded.totalCurrency == 500
                && (hero ? loaded.GetCharacterStarLevel(id) : loaded.GetRelicStarLevel(id)) == 2
                && (hero ? loaded.GetCharacterShards(id) : loaded.GetRelicShards(id)) == 10,
                "JSON reload " + id);
            checks++;
        }
        checks += ValidateOwnershipAndBonuses();
        checks += ValidateHeroCombatIntegration();
        checks += ValidateCodexFeedback();
        checks += ValidateLoadoutReplacementAndBack();
        checks += ValidatePreparationSlotOrder();
        checks += ValidateHubLoadoutRoute();
        checks += ValidateSeparateManagementScreen();
        checks += ValidateStageReviewData();
        string report = "PASS: " + checks + " card transaction scenarios. No player save writes."
            + "\nUnity: " + Application.unityVersion + "\nUTC: " + DateTime.UtcNow.ToString("O")
            + "\nFixtures: " + fixtureRoot
            + "\nScope: transactions, isolated JSON, ownership, bonus formulas and isolated bound attack component; full scene/manager/UI playthrough remains unverified.";
        Directory.CreateDirectory("scratch");
        File.WriteAllText("scratch/CardProgressionValidation.txt", report);
        return report;
    }

    private static int ValidateOwnershipAndBonuses()
    {
        int checks = 0;
        var save = new MetaProgressionSaveData();
        var ownership = DefenseMetaOwnershipAdapter.CreateDefault(save);
        Require(ownership.IsOwned(" C001 ") && ownership.IsOwned(" R001 "), "Legacy starter aliases");
        checks++;
        Require(!ownership.IsOwned("unknown_card"), "Unknown card must not inherit starter ownership");
        checks++;
        Require(DefenseLoadoutIdMapper.GetMetaCharacterId("unknown_card") == null,
            "Unknown hero must not inherit starter progression");
        checks++;
        Require(DefenseLoadoutIdMapper.GetMetaCharacterId(" C002 ") == "C002_DaoSi", "Canonical hero save ID");
        checks++;

        // Synthetic future IDs deliberately avoid the always-owned starter list.
        save.SetCharacterProgress("future_hero", 50, 0);
        save.SetRelicProgress("future_relic", 50, 0);
        Require(!ownership.IsOwned("future_hero") && !ownership.IsOwned("future_relic"), "Shards alone do not unlock cards");
        checks++;
        save.SetCharacterProgress("future_hero", 40, 1);
        save.SetRelicProgress("future_relic", 40, 1);
        Require(ownership.IsOwned(" FUTURE_HERO ") && ownership.IsOwned(" FUTURE_RELIC "), "Ownership reads live progress");
        checks++;
        save.unlockedTowerIds = new List<string> { "future_tower" };
        Require(ownership.IsOwned(" FUTURE_TOWER "), "Explicit saved tower unlock");
        checks++;
        ownership.Grant("future_minion");
        Require(ownership.IsOwned(" FUTURE_MINION "), "Explicit grant");
        checks++;

        var heroConfig = Resources.Load<CharacterStarProgressionSO>("CharacterStarProgressionConfig");
        var relicConfig = Resources.Load<RelicStarProgressionSO>("RelicStarProgressionConfig");
        Require(heroConfig != null && relicConfig != null, "Runtime progression assets must exist");
        float heroHp = 0f, heroSpeed = 0f, relicDamage = 0f, relicCooldown = 0f, relicArea = 0f;
        for (int star = 1; star <= 5; star++)
        {
            var h = heroConfig.GetStepConfig(star);
            var r = relicConfig.GetStepConfig(star);
            Require(h != null && r != null, "Missing progression step " + star);
            if (star > 1)
            {
                heroHp += Mathf.Max(0f, h.healthMultiplierBonus);
                heroSpeed += Mathf.Max(0f, h.moveSpeedMultiplierBonus);
                relicDamage += Mathf.Max(0f, r.damageMultiplierBonus);
                relicCooldown += Mathf.Max(0f, r.cooldownReductionPercent);
                relicArea += Mathf.Max(0f, r.areaMultiplierBonus);
            }
            DefenseLoadoutRuntimeAdapter.EvaluateHeroStarBonuses(star, heroConfig, out float hp, out float speed);
            Require(Mathf.Approximately(hp, heroHp) && Mathf.Approximately(speed, heroSpeed), "Hero bonuses " + star);
            checks++;
            DefenseLoadoutRuntimeAdapter.EvaluateRelicStarBonuses(star, relicConfig, out float damage, out float cooldown, out float area);
            Require(Mathf.Approximately(damage, relicDamage) && Mathf.Approximately(cooldown, Mathf.Clamp(relicCooldown, 0f, 0.8f))
                && Mathf.Approximately(area, relicArea), "Relic bonuses " + star);
            checks++;
            DefenseLoadoutRuntimeAdapter.EvaluateMinionStarBonuses(star, out hp, out speed, relicConfig);
            Require(Mathf.Approximately(hp, star == 1 ? 0f : Mathf.Max(0f, r.minionHpMultiplierBonus))
                && Mathf.Approximately(speed, star == 1 ? 0f : Mathf.Max(0f, r.minionSpeedMultiplierBonus)), "Minion bonuses " + star);
            checks++;
            DefenseLoadoutRuntimeAdapter.EvaluateTowerStarBonuses(star, out damage, out float range, relicConfig);
            Require(Mathf.Approximately(damage, star == 1 ? 0f : Mathf.Max(0f, r.towerDamageMultiplierBonus))
                && Mathf.Approximately(range, star == 1 ? 0f : Mathf.Max(0f, r.towerRangeMultiplierBonus)), "Tower bonuses " + star);
            checks++;
        }
        return checks;
    }

    private static int ValidateHeroCombatIntegration()
    {
        var root = new GameObject("CardCombatValidation") { hideFlags = HideFlags.HideAndDontSave };
        root.SetActive(false);
        var session = new DefenseSession();
        var config = ScriptableObject.CreateInstance<CharacterStarProgressionSO>();
        try
        {
            // An explicit authored fixture makes expected combat values independent of production balance.
            for (int star = 1; star <= 5; star++)
            {
                var step = config.GetStepConfig(star);
                step.damageMultiplierBonus = star == 2 ? 0.25f : 0f;
                step.cooldownReductionBonus = star == 2 ? 0.2f : 0f;
            }
            DefenseLoadoutRuntimeAdapter.EvaluateHeroCombatStarBonuses(2, config, out float damageBonus, out float cooldown);
            Require(Mathf.Approximately(damageBonus, 0.25f) && Mathf.Approximately(cooldown, 0.2f), "Authored hero combat fixture");
            var attack = root.AddComponent<DefenseHeroAutoAttack3D>();
            attack.BindGameplay(session);
            var definition = new DefenseSkillDefinition(DefenseSkillKind.BasicAttack, 5f, 2f, 100f, 1f, 0.2f, true, DefenseTargetMask.All);
            attack.ConfigureAttack(definition);
            attack.SetMetaDamageBonus(damageBonus);
            Require(Mathf.Approximately(attack.EffectiveDamage, 125f) && Mathf.Approximately(attack.BaseDamage, 100f)
                && Mathf.Approximately(definition.Damage, 100f), "Card damage reaches bound attack without editing definition");
            session.Modifiers.TryApplyCard(new DefenseUpgradeCard("test", "test", "", DefenseUpgradeType.HeroAttackDamage, 0.2f));
            Require(Mathf.Approximately(attack.EffectiveDamage, 150f), "Card and run damage bonuses compose");
            attack.SetMetaDamageBonus(damageBonus);
            Require(Mathf.Approximately(attack.EffectiveDamage, 150f), "Reapplying card bonus does not stack it");
            var signature = new DefenseSkillDefinition(DefenseSkillKind.ScholarSignature, 5f, 2f, 200f, 10f, 1f, true, DefenseTargetMask.All);
            var effectiveSignature = signature.WithBonuses(0f, cooldown, 0f);
            Require(Mathf.Approximately(effectiveSignature.Cooldown, 8f) && Mathf.Approximately(signature.Cooldown, 10f)
                && Mathf.Approximately(effectiveSignature.Damage, 200f), "Signature cooldown changes without double-applying damage");
            session.Modifiers.Reset();
            Require(Mathf.Approximately(attack.EffectiveDamage, 125f), "Permanent card damage survives run modifier reset");
            attack.SetMetaDamageBonus(0f);
            Require(Mathf.Approximately(attack.EffectiveDamage, 100f), "Changing hero can clear permanent damage bonus");
            return 7;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
            session.Dispose();
            UnityEngine.Object.DestroyImmediate(config);
        }
    }

    private static int ValidateCodexFeedback()
    {
        int checks = 0;
        foreach (string path in new[] { "Assets/_Prefabs/UI/CardCodexUI.prefab", "Assets/Resources/UI/CardCodexUI.prefab" })
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var view = root.GetComponent<ProjectZombie.Features.UI.CardCodexView>();
                var serialized = new SerializedObject(view);
                var status = (TMPro.TextMeshProUGUI)serialized.FindProperty("_detailShardProgress").objectReferenceValue;
                var button = (UnityEngine.UI.Button)serialized.FindProperty("_alchemyFusionButton").objectReferenceValue;
                Require(status != null && button != null && status.rectTransform.sizeDelta.y >= 116f, "Authored feedback area " + path);
                checks++;
                view.DisplayRelicDetail("Test", "", "", null, 1, 2, 10, 500, false, "TĂNG SAO");
                view.SetUpgradeStatus("Thiếu 8 mảnh và 500 Cổ Tiền.");
                Require(!button.interactable && status.text.Contains("Thiếu 8"), "Disabled upgrade explains missing resources");
                checks++;
                view.ShowUpgradeOutcome("Lưu thất bại.");
                Require(status.text.Contains("Lưu thất bại.") && status.text.Contains("Thiếu 8"), "Failure retains requirements");
                checks++;
                view.DisplayRelicDetail("Other", "", "", null, 5, 2, 0, 0, false, "TỐI ĐA");
                view.SetUpgradeStatus("Đã đạt tối đa 5 sao.");
                Require(!button.interactable && !status.text.Contains("Lưu thất bại"), "Changing selection clears previous outcome");
                checks++;
                view.DisplayRelicDetail("Ready", "", "", null, 1, 10, 10, 500, true, "TĂNG SAO");
                Require(button.interactable, "Ready card re-enables upgrade");
                checks++;
                var modal = root.transform.Find("SafeArea/Modal_Container");
                var loadout = modal.Find("Loadout_Bar");
                var body = (RectTransform)modal.Find("Body_Area");
                var equip = (UnityEngine.UI.Button)serialized.FindProperty("_equipButton").objectReferenceValue;
                view.SetCollectionMode(true, "ANH HÙNG");
                view.DisplayRelicDetail("Hero", "", "", null, 1, 10, 10, 500, true, "TĂNG SAO");
                view.ShowEquipAction("TRANG BỊ", true);
                Require(!loadout.gameObject.activeSelf && button.gameObject.activeSelf && !equip.gameObject.activeSelf
                    && Mathf.Approximately(body.offsetMax.y, -126f), "Management shows upgrade without equipment layout");
                checks++;
                view.SetCollectionMode(false, "TRANG BỊ ĐỘI HÌNH");
                view.DisplayRelicDetail("Hero", "", "", null, 1, 10, 10, 500, true, "TĂNG SAO");
                view.ShowEquipAction("TRANG BỊ", true);
                Require(loadout.gameObject.activeSelf && !button.gameObject.activeSelf && equip.gameObject.activeSelf
                    && Mathf.Approximately(body.offsetMax.y, -330f), "Equipment mode hides upgrade actions");
                checks++;
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        return checks;
    }

    private static int ValidateLoadoutReplacementAndBack()
    {
        int checks = 0;
        foreach (bool minion in new[] { true, false })
        {
            var selection = new LoadoutSelection("hero", "relic", new[] { "a", "b", "c", "d" }, new[] { "a", "b", "c", "d" });
            int notifications = 0;
            selection.OnChanged += () => notifications++;
            Func<string, string, bool> replace = minion ? selection.ReplaceMinion : selection.ReplaceTower;
            Require(replace("B", "new") && notifications == 1, "Explicit slot replacement emits once");
            var ids = minion ? selection.MinionIds : selection.TowerIds;
            Require(ids.Count == 4 && ids[0] == "a" && ids[1] == "new" && ids[2] == "c", "Full team replacement preserves positions and count");
            checks++;
            Require(!replace("new", "A") && !replace("missing", "other") && !replace("new", " ")
                && notifications == 1 && ids[1] == "new", "Invalid replacements leave team unchanged");
            checks++;
        }

        // Inactive isolated hierarchy prevents lifecycle bootstrapping or player-save access.
        var root = new GameObject("CardNavigationValidation") { hideFlags = HideFlags.HideAndDontSave };
        root.SetActive(false);
        try
        {
            var manager = root.AddComponent<ProjectZombie.Features.UI.MetaUIManager>();
            var hub = AddScreen<ProjectZombie.Features.UI.MainHubView>(root.transform);
            var parent = AddScreen<ProjectZombie.Features.UI.SettingsModalView>(root.transform);
            var review = AddScreen<ProjectZombie.Features.Defense3D.UI.DefensePreparationView>(root.transform);
            var codex = AddScreen<ProjectZombie.Features.UI.CardCodexView>(root.transform);
            var factory = new ProjectZombie.Features.UI.UIScreenFactory(null, root.transform);
            factory.RegisterExistingScreen(hub);
            factory.RegisterExistingScreen(parent);
            factory.RegisterExistingScreen(review);
            factory.RegisterExistingScreen(codex);
            typeof(ProjectZombie.Features.UI.MetaUIManager).GetField("_screenFactory",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(manager, factory);
            manager.PushScreen(hub);
            manager.OpenChildScreen(parent.ScreenType);
            manager.OpenChildScreen(review.ScreenType);
            manager.OpenChildScreen(codex.ScreenType);
            Require(codex.gameObject.activeSelf && !review.gameObject.activeSelf, "Child hides parent");
            checks++;
            manager.PopScreen();
            Require(review.gameObject.activeSelf && !codex.gameObject.activeSelf, "Back returns to loadout review");
            checks++;
            manager.PopScreen();
            Require(parent.gameObject.activeSelf && !review.gameObject.activeSelf, "Back preserves original parent screen");
            checks++;
            manager.PopScreen();
            Require(hub.gameObject.activeSelf && !parent.gameObject.activeSelf, "Back returns to hub");
            checks++;
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
        return checks;
    }

    private static T AddScreen<T>(Transform parent) where T : Component
    {
        var node = new GameObject(typeof(T).Name);
        node.transform.SetParent(parent, false);
        return node.AddComponent<T>();
    }

    private sealed class MemoryLoadoutRepository : ILoadoutRepository
    {
        public LoadoutSaveDto Load() => new LoadoutSaveDto(Defense3DCatalog.HERO_THU_SINH, Defense3DCatalog.RELIC_DEP_TO_ONG,
            new[] { Defense3DCatalog.MINION_THAY_LANG, Defense3DCatalog.MINION_DAN_BINH },
            new[] { Defense3DCatalog.TOWER_CUNG_TIEN }, DefenseLoadoutRepository.CURRENT_VERSION);
        public void Save(LoadoutSaveDto dto) { }
    }

    private static int ValidatePreparationSlotOrder()
    {
        var root = PrefabUtility.LoadPrefabContents("Assets/_Prefabs/UI/DefensePreparationUI.prefab");
        try
        {
            var view = root.GetComponent<ProjectZombie.Features.Defense3D.UI.DefensePreparationView>();
            var presenter = new ProjectZombie.Features.Defense3D.UI.DefensePreparationPresenter(
                catalog: Defense3DCatalog.CreateDefault(), ownership: DefenseMetaOwnershipAdapter.CreateDefault(), repository: new MemoryLoadoutRepository());
            var type = typeof(ProjectZombie.Features.Defense3D.UI.DefensePreparationView);
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            type.GetField("_presenter", flags).SetValue(view, presenter);
            type.GetMethod("BindMinionRows", flags).Invoke(view, new object[] { presenter.Selection.MinionIds, 4 });
            var so = new SerializedObject(view);
            var labels = so.FindProperty("_minionSlotNameTexts");
            var icons = so.FindProperty("_minionSlotCheckmarks");
            Require(((TMPro.TextMeshProUGUI)labels.GetArrayElementAtIndex(0).objectReferenceValue).text == "Thầy Lang",
                "First preparation slot must show the first equipped unit, not catalog index zero");
            Require(((TMPro.TextMeshProUGUI)labels.GetArrayElementAtIndex(1).objectReferenceValue).text == "Dân Binh",
                "Second preparation slot follows equipped order");
            Require(((TMPro.TextMeshProUGUI)labels.GetArrayElementAtIndex(2).objectReferenceValue).text == "+ Thêm"
                && !((UnityEngine.UI.Image)icons.GetArrayElementAtIndex(2).objectReferenceValue).gameObject.activeSelf,
                "Empty slot clears stale unit presentation");
            return 3;
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static int ValidateHubLoadoutRoute()
    {
        int checks = 0;
        foreach (string path in new[] { "Assets/_Prefabs/UI/MainHubUI.prefab", "Assets/Resources/UI/MainHubUI.prefab" })
        {
            var root = new GameObject("HubLoadoutRouteValidation") { hideFlags = HideFlags.HideAndDontSave };
            root.SetActive(false);
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var hub = UnityEngine.Object.Instantiate(prefab, root.transform, false);
                var view = hub.GetComponent<ProjectZombie.Features.UI.MainHubView>();
                var presenter = hub.GetComponent<ProjectZombie.Features.UI.MainHubPresenter>();
                var manager = root.AddComponent<ProjectZombie.Features.UI.MetaUIManager>();
                var review = AddScreen<ProjectZombie.Features.Defense3D.UI.DefensePreparationView>(root.transform);
                var management = AddScreen<ProjectZombie.Features.Defense3D.UI.DefenseLoadoutManagementView>(root.transform);
                var factory = new ProjectZombie.Features.UI.UIScreenFactory(null, root.transform);
                factory.RegisterExistingScreen(view);
                factory.RegisterExistingScreen(review);
                factory.RegisterExistingScreen(management);
                var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                typeof(ProjectZombie.Features.UI.MetaUIManager).GetField("_screenFactory", flags).SetValue(manager, factory);
                typeof(ProjectZombie.Features.UI.MainHubPresenter).GetField("_view", flags).SetValue(presenter, view);
                typeof(ProjectZombie.Features.UI.MainHubPresenter).GetField("_metaUIManager", flags).SetValue(presenter, manager);
                typeof(ProjectZombie.Features.UI.MainHubView).GetMethod("Awake", flags).Invoke(view, null);
                typeof(ProjectZombie.Features.UI.MainHubPresenter).GetMethod("Start", flags).Invoke(presenter, null);
                int loadoutEvents = 0, codexEvents = 0;
                view.OnLoadoutClicked += () => loadoutEvents++;
                view.OnCodexClicked += () => codexEvents++;
                var button = hub.transform.Find("SafeArea_Content/Bottom_HUDRow/Container_CardCodex/Btn_CardCodex_Wood_Frame").GetComponent<UnityEngine.UI.Button>();
                manager.PushScreen(view);
                button.onClick.Invoke();
                Require(loadoutEvents == 1 && codexEvents == 0 && management.gameObject.activeSelf && !review.gameObject.activeSelf && !hub.activeSelf,
                    "Hub card must open standalone management, never pre-match review: " + path);
                typeof(ProjectZombie.Features.Defense3D.UI.DefensePreparationView).GetMethod("OnStartBattleClicked", flags).Invoke(management, null);
                Require(hub.activeSelf && !management.gameObject.activeSelf && !review.gameObject.activeSelf,
                    "Management Done returns to Hub without opening stage selection/review: " + path);
                checks++;
                manager.OpenChildScreen(review.ScreenType);
                typeof(ProjectZombie.Features.Defense3D.UI.DefensePreparationView).GetMethod("OnEditLoadoutClicked", flags).Invoke(review, null);
                Require(management.gameObject.activeSelf && !review.gameObject.activeSelf, "Review edit opens the standalone management screen");
                manager.PopScreen();
                Require(review.gameObject.activeSelf && !management.gameObject.activeSelf, "Back from management returns to the original review");
                checks++;
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
        return checks;
    }

    private static int ValidateSeparateManagementScreen()
    {
        int checks = 0;
        foreach (string folder in new[] { "Assets/_Prefabs/UI/", "Assets/Resources/UI/" })
        {
            var management = AssetDatabase.LoadAssetAtPath<GameObject>(folder + "DefenseLoadoutManagementUI.prefab");
            var review = AssetDatabase.LoadAssetAtPath<GameObject>(folder + "DefensePreparationUI.prefab");
            Require(management != null && review != null && management != review, "Separate authored screen assets");
            var managementView = management.GetComponent<ProjectZombie.Features.Defense3D.UI.DefenseLoadoutManagementView>();
            var reviewView = review.GetComponent<ProjectZombie.Features.Defense3D.UI.DefensePreparationView>();
            Require(managementView != null && managementView.ScreenType == ProjectZombie.Features.UI.MetaScreenType.LoadoutManagement
                && reviewView.ScreenType == ProjectZombie.Features.UI.MetaScreenType.DefensePreparation, "Distinct screen identities");
            checks++;
            var editor = new SerializedObject(managementView);
            Require(((TMPro.TextMeshProUGUI)editor.FindProperty("_startBattleButtonText").objectReferenceValue).text == "XONG"
                && !((UnityEngine.UI.Button)editor.FindProperty("_editLoadoutButton").objectReferenceValue).gameObject.activeSelf
                && ((UnityEngine.UI.Button)editor.FindProperty("_heroSlotButton").objectReferenceValue).interactable,
                "Management has direct equipment and Done, no review edit CTA");
            checks++;
            var confirmation = new SerializedObject(reviewView);
            Require(((UnityEngine.UI.Button)confirmation.FindProperty("_editLoadoutButton").objectReferenceValue).gameObject.activeSelf
                && !((UnityEngine.UI.Button)confirmation.FindProperty("_heroSlotButton").objectReferenceValue).interactable,
                "Review is read-only with separate edit action");
            checks++;
        }
        var registry = Resources.Load<ProjectZombie.Features.UI.UIRegistrySO>("UI/UIRegistry");
        Require(registry.TryGetScreenEntry(ProjectZombie.Features.UI.MetaScreenType.LoadoutManagement, out var editorEntry)
            && registry.TryGetScreenEntry(ProjectZombie.Features.UI.MetaScreenType.DefensePreparation, out var reviewEntry)
            && editorEntry.screenPrefab != reviewEntry.screenPrefab, "Registry keeps management and review separate");
        checks++;
        return checks;
    }

    private static int ValidateStageReviewData()
    {
        var missing = ProjectZombie.Features.Defense3D.UI.DefenseStageReviewPresentation.FromStage(null);
        Require(missing.Name == "Chưa chọn ải" && missing.Artwork == null, "Missing stage must not display fictitious mission data");
        var stage = ScriptableObject.CreateInstance<ProjectZombie.Features.Maps.StageDefinitionSO>();
        try
        {
            stage.stageName = "Ải thử (3D)";
            stage.description = "Chiến trường 3D thử nghiệm.";
            stage.stageDurationSeconds = 121;
            stage.baseRewardCoins = 1234;
            stage.baseRewardExp = 4321;
            var data = ProjectZombie.Features.Defense3D.UI.DefenseStageReviewPresentation.FromStage(stage);
            Require(data.Duration == "02:01" && data.Rewards.Contains(1234.ToString("N0")) && data.Rewards.Contains(4321.ToString("N0")),
                "Review reflects actual authored duration and rewards");
            Require(data.Name == "Ải thử" && stage.stageName == "Ải thử (3D)", "Display formatting preserves authored stage data");
            stage.stageDurationSeconds = float.NaN;
            Require(ProjectZombie.Features.Defense3D.UI.DefenseStageReviewPresentation.FromStage(stage).Duration == "Chưa có thời gian",
                "Invalid stage time is not cast to an arbitrary countdown");
            return 4;
        }
        finally { UnityEngine.Object.DestroyImmediate(stage); }
    }

    private static MetaProgressionSaveData Create(bool hero, int star)
    {
        var data = new MetaProgressionSaveData { totalCurrency = 1000 };
        if (hero) data.SetCharacterProgress("test_card", 20, star);
        else data.SetRelicProgress("test_card", 20, star);
        return data;
    }

    private static int Star(MetaProgressionSaveData data, bool hero)
        => hero ? data.GetCharacterStarLevel("test_card") : data.GetRelicStarLevel("test_card");

    private static int Shards(MetaProgressionSaveData data, bool hero)
        => hero ? data.GetCharacterShards("test_card") : data.GetRelicShards("test_card");

    private static void Require(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException("Card upgrade validation failed: " + label);
    }
}
#endif
