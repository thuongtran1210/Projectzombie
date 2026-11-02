#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.UI;
using ProjectZombie.Features.Defense3D.Loadout;

public static class DefenseSkillDataMigration
{
    public const string Folder="Assets/Features/Defense3D/Skills/Presentation";
    public const string Catalog="Assets/Resources/Defense3D/DefenseSkillCatalog.asset";
    private const string Definitions="Assets/Resources/Defense3D/";
    private const string Effects="Assets/Features/Defense3D/Effects/Assets/";
    private const string Icons="Assets/Art/UI/CombatHUD/Glyph_";
    [MenuItem("Tools/ProjectZombie/Skills/Migrate Skill Data")]
    public static void Migrate()
    {
        RequireClean();Directory.CreateDirectory(Folder);AssetDatabase.Refresh();
        var skills=new[]{
            Ensure<DefenseAttackPresentationSO>("Skill_BasicAttack","skill.basic_attack",DefenseSkillBehavior.BasicAttack,"Đánh","Attack",Effects+"FXSet_HeroBasicAttack.asset"),
            Ensure<DefenseAttackPresentationSO>("Skill_DaoSi_BasicAttack","skill.daosi_attack",DefenseSkillBehavior.BasicAttack,"Đánh","Attack",Effects+"FXSet_HeroBasicAttack.asset"),
            Ensure<DefenseDashPresentationSO>("Skill_Dash","skill.dash",DefenseSkillBehavior.Dash,"Lướt","Dash",Effects+"FXSet_HeroDash.asset"),
            Ensure<DefenseSignaturePresentationSO>("Skill_ScholarSignature","skill.scholar_signature",DefenseSkillBehavior.ScholarSignature,"Phán Quyết","Scholar",Effects+"FXSet_ScholarSignature.asset"),
            Ensure<DefenseSignaturePresentationSO>("Skill_DaoSi_Signature","skill.lightning_signature",DefenseSkillBehavior.LightningSignature,"Thiên Lôi","Thunder",Effects+"Thunder/FXSet_HeavenlyLightning.asset"),
            Ensure<DefenseRelicPresentationSO>("Skill_DepToOng","skill.slipper",DefenseSkillBehavior.Slipper,"Dép Tổ Ong","Slipper",Effects+"FX_DepToOng_Impact.asset"),
            Ensure<DefenseRelicPresentationSO>("Skill_DieuCay","skill.pipe",DefenseSkillBehavior.Pipe,"Điếu Cày","Pipe",Effects+"PipeSmoke/FX_PipeSmoke.asset")};
        EnsureTuning<DefenseAttackTuningSO>(skills[0]);
        EnsureTuning<DefenseAttackTuningSO>(skills[1]);
        EnsureTuning<DefenseScholarTuningSO>(skills[3]);
        EnsureTuning<DefenseLightningTuningSO>(skills[4]);
        EnsureTuning<DefenseSlipperTuningSO>(skills[5]);
        EnsureTuning<DefensePipeTuningSO>(skills[6]);
        var catalog=AssetDatabase.LoadAssetAtPath<DefenseSkillCatalogSO>(Catalog);
        if(catalog==null){catalog=ScriptableObject.CreateInstance<DefenseSkillCatalogSO>();AssetDatabase.CreateAsset(catalog,Catalog);}
        var data=new SerializedObject(catalog);var list=data.FindProperty("_skills");
        foreach(var skill in skills)if(!Enumerable.Range(0,list.arraySize).Any(i=>list.GetArrayElementAtIndex(i).objectReferenceValue==skill))
        {int i=list.arraySize;list.InsertArrayElementAtIndex(i);list.GetArrayElementAtIndex(i).objectReferenceValue=skill;}
        var relics=data.FindProperty("_relics");
        foreach(var pair in new[]{(Defense3DCatalog.RELIC_DEP_TO_ONG,skills[5]),(Defense3DCatalog.RELIC_DIEU_CAY,skills[6])})
            if(!Enumerable.Range(0,relics.arraySize).Any(i=>relics.GetArrayElementAtIndex(i).FindPropertyRelative("RelicId").stringValue==pair.Item1))
            {int i=relics.arraySize;relics.InsertArrayElementAtIndex(i);var entry=relics.GetArrayElementAtIndex(i);entry.FindPropertyRelative("RelicId").stringValue=pair.Item1;entry.FindPropertyRelative("Skill").objectReferenceValue=pair.Item2;}
        data.ApplyModifiedPropertiesWithoutUndo();
        var root=PrefabUtility.LoadPrefabContents(DefenseCombatHudBuilder.Controls);
        try{ClearSlotDefinitions(root);PrefabUtility.SaveAsPrefabAsset(root,DefenseCombatHudBuilder.Controls);}finally{PrefabUtility.UnloadPrefabContents(root);}
        var setup=EditorSceneManager.GetSceneManagerSetup();
        try
        {
            foreach(string path in new[]{DefenseMapArtUpgrade.Map,"Assets/Features/CombatLab/Scenes/DefenseBaseline.unity"})
            {
                if(!File.Exists(path))continue;var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                foreach(var go in scene.GetRootGameObjects())ClearSlotDefinitions(go);
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
        }
        finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
        AssetDatabase.SaveAssets();Validate();
        File.WriteAllText("scratch/SkillDataMigration.txt","Migrated 7 definitions to stable IDs/behaviors and typed presentation assets; catalog relic bindings; cleared legacy definition fields and HUD slot definition copies. Existing authored presentation values are preserved on rerun. Gameplay values unchanged.");
    }
    private static DefenseSkillDefinitionSO Ensure<T>(string file,string id,DefenseSkillBehavior behavior,string title,string glyph,string effectPath) where T:DefenseSkillPresentationSO
    {
        var definition=AssetDatabase.LoadAssetAtPath<DefenseSkillDefinitionSO>(Definitions+file+".asset");
        if(definition==null)throw new InvalidOperationException("Missing "+file);
        var data=new SerializedObject(definition);
        bool first=definition.Presentation==null;
        if(first)
        {
            var presentation=AssetDatabase.LoadAssetAtPath<T>(Folder+"/"+file+"_Presentation.asset");
            if(presentation==null)
            {
                presentation=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(presentation,Folder+"/"+file+"_Presentation.asset");
                var p=new SerializedObject(presentation);p.FindProperty("_displayName").stringValue=title;
                p.FindProperty("_icon").objectReferenceValue=AssetDatabase.LoadAssetAtPath<Sprite>(Icons+glyph+".png");
                p.FindProperty("_accent").colorValue=behavior==DefenseSkillBehavior.BasicAttack?new Color(.85f,.67f,.39f):behavior==DefenseSkillBehavior.Dash?new Color(.61f,.68f,.73f):
                    behavior==DefenseSkillBehavior.Pipe?new Color(1,.77f,.32f):behavior==DefenseSkillBehavior.Slipper?new Color(.49f,.73f,.64f):new Color(.5f,.79f,.86f);
                var field=p.FindProperty(presentation is DefenseRelicPresentationSO?"_profile":"_effects");
                UnityEngine.Object previous=behavior==DefenseSkillBehavior.Pipe?data.FindProperty("_presentationProfile").objectReferenceValue:
                    behavior==DefenseSkillBehavior.LightningSignature?data.FindProperty("_signaturePresentation").objectReferenceValue:null;
                field.objectReferenceValue=previous!=null?previous:AssetDatabase.LoadMainAssetAtPath(effectPath);
                if(behavior==DefenseSkillBehavior.Slipper)p.FindProperty("_projectileSprite").objectReferenceValue=AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath("e5b924ce253447e6b9bd59a0515ae257"));
                p.ApplyModifiedPropertiesWithoutUndo();
            }
            data.FindProperty("_presentation").objectReferenceValue=presentation;
            data.FindProperty("_behavior").enumValueIndex=(int)behavior;
        }
        if(string.IsNullOrWhiteSpace(definition.SkillId))data.FindProperty("_skillId").stringValue=id;
        foreach(string field in new[]{"_icon","_presentationProfile","_signaturePresentation"})data.FindProperty(field).objectReferenceValue=null;
        data.ApplyModifiedPropertiesWithoutUndo();return definition;
    }
    private static void ClearSlotDefinitions(GameObject root)
    {
        foreach(var presenter in root.GetComponentsInChildren<DefenseSkillPresenter3D>(true))
        {
            var data=new SerializedObject(presenter);var legacy=(DefenseSkillDefinitionSO)data.FindProperty("_definition").objectReferenceValue;
            if(legacy!=null)data.FindProperty("_slot").intValue=(int)legacy.Capture().Kind;
            data.FindProperty("_definition").objectReferenceValue=null;data.ApplyModifiedPropertiesWithoutUndo();
        }
    }
    private static void EnsureTuning<T>(DefenseSkillDefinitionSO skill)where T:DefenseSkillTuningSO
    {
        if(skill.Tuning!=null)return;
        string path=Folder+"/"+skill.name+"_Tuning.asset";
        var tuning=AssetDatabase.LoadAssetAtPath<T>(path);
        if(tuning==null)
        {
            tuning=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(tuning,path);
            if(tuning is DefenseSlipperTuningSO)
            {
                var data=new SerializedObject(tuning);data.FindProperty("_hitStatuses").objectReferenceValue=AssetDatabase.LoadAssetAtPath<DefenseHitStatusProfileSO>(AssetDatabase.GUIDToAssetPath("d0b9b1726a9f5d8d812578f53cc95ed5"));data.ApplyModifiedPropertiesWithoutUndo();
            }
            if(tuning is DefenseAttackTuningSO)
            {
                var data=new SerializedObject(tuning);data.FindProperty("_targeting").objectReferenceValue=AssetDatabase.LoadAssetAtPath<DefenseAttackTargetingProfileSO>(AssetDatabase.GUIDToAssetPath("bd72d0b70d465bce81cc0fe76ec9d8c1"));data.ApplyModifiedPropertiesWithoutUndo();
            }
        }
        var so=new SerializedObject(skill);so.FindProperty("_tuning").objectReferenceValue=tuning;so.ApplyModifiedPropertiesWithoutUndo();
    }
    public static void Validate()
    {
        var catalog=AssetDatabase.LoadAssetAtPath<DefenseSkillCatalogSO>(Catalog);if(catalog==null)throw new InvalidOperationException("Missing skill catalog.");catalog.Validate();
        foreach(string guid in AssetDatabase.FindAssets("t:DefenseSkillDefinitionSO"))
        {
            var skill=AssetDatabase.LoadAssetAtPath<DefenseSkillDefinitionSO>(AssetDatabase.GUIDToAssetPath(guid));
            if(catalog.Find(skill.SkillId)!=skill)throw new InvalidOperationException("Unregistered skill: "+skill.name);
            var data=new SerializedObject(skill);
            foreach(string field in new[]{"_icon","_presentationProfile","_signaturePresentation"})if(data.FindProperty(field).objectReferenceValue!=null)throw new InvalidOperationException("Legacy competing reference: "+skill.name+"."+field);
        }
        var heroes=AssetDatabase.LoadAssetAtPath<DefenseHeroCatalogSO>("Assets/Resources/Defense3D/DefenseHeroCatalog.asset");
        foreach(var hero in heroes.Heroes)
        {
            if(hero==null||hero.HeroKit==null)continue;hero.HeroKit.Validate();
            foreach(var kind in new[]{DefenseSkillKind.BasicAttack,DefenseSkillKind.Dash,DefenseSkillKind.Signature,DefenseSkillKind.Relic})
            {var skill=hero.HeroKit.DefinitionFor(kind);if(skill!=null&&catalog.Find(skill.SkillId)!=skill)throw new InvalidOperationException("Hero kit references an unregistered skill.");}
        }
        Directory.CreateDirectory("scratch");File.WriteAllText("scratch/SkillDataValidation.txt","Passed: "+catalog.Skills.Count+" unique skills, valid typed presentations, relic bindings, registered hero-kit references and no legacy definition presentation copies.");
    }
    private static void RequireClean()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Requires Edit Mode.");
        for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Save scene edits first.");
    }
}
#endif
