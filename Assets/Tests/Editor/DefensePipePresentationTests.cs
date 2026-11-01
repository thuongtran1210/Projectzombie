using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.UI;

/// <summary>Validates the authored pipe assets, exact area scaling, pooled reset and unchanged combat definition.</summary>
public sealed class DefensePipePresentationTests
{
    [Test]
    public void SkillHasValidPresentationAndOriginalCombatValues()
    {
        var skill = AssetDatabase.LoadAssetAtPath<DefenseSkillDefinitionSO>("Assets/Resources/Defense3D/Skill_DieuCay.asset");
        Assert.NotNull(skill.PresentationProfile);
        Assert.IsTrue(skill.PresentationProfile.IsValid);
        Assert.NotNull(skill.Icon);
        var snapshot = skill.Capture();
        Assert.AreEqual(9f, snapshot.Range);
        Assert.AreEqual(4f, snapshot.Radius);
        Assert.AreEqual(65f, snapshot.Damage);
        Assert.AreEqual(16f, snapshot.Cooldown);
        Assert.AreEqual(4.5f, snapshot.Duration);
        Assert.AreEqual(snapshot.Duration, skill.PresentationProfile.Duration);
    }

    [Test]
    public void PooledVisualResetsAllLayersAndScalesToActualDiameter()
    {
        var profile = AssetDatabase.LoadAssetAtPath<DefenseEffectProfileSO>(DefensePipeSmokeBuilder.PROFILE);
        var instance = Object.Instantiate(profile.Prefab);
        try
        {
            foreach (float diameter in new[] { 4f, 8f, 12f })
            {
                instance.Prepare(profile.Color, diameter); instance.Begin(); instance.Render(.4f);
                Assert.AreEqual(diameter, instance.transform.Find("Ground").localScale.x, .001f);
                Assert.AreEqual(3, instance.GetComponentsInChildren<MeshRenderer>().Length);
                foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>())
                {
                    Assert.IsTrue(renderer.enabled);
                    Assert.IsFalse(ShaderUtil.ShaderHasError(renderer.sharedMaterial.shader));
                    foreach (Vector3 vertex in renderer.GetComponent<MeshFilter>().sharedMesh.vertices)
                        Assert.IsTrue(DefenseDamageRules.IsFinite(vertex.x) && DefenseDamageRules.IsFinite(vertex.y) && DefenseDamageRules.IsFinite(vertex.z));
                }
                instance.ResetPresentation();
                foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>(true)) Assert.IsFalse(renderer.enabled);
            }
        }
        finally { Object.DestroyImmediate(instance.gameObject); }
    }

    [Test]
    public void SmokeAimUsesExactRadiusAndCancelColorAndHidesAllLayers()
    {
        var root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Defense3D/DefenseAimIndicator3D.prefab"));
        try
        {
            var view = root.GetComponent<DefenseAimIndicator3D>();
            DefenseAimResolver.TryResolve(new DefensePoint3(0, 1, 0), new DefensePoint3(5, 1, 2), 9, out var aim);
            view.ShowArea(aim, 4, 9, false);
            var area = root.transform.Find("PipeAreaFill").GetComponent<MeshRenderer>();
            Assert.IsTrue(area.enabled);
            Assert.AreEqual(8f, area.transform.localScale.x, .001f);
            view.ShowArea(aim, 4, 9, true);
            var block = new MaterialPropertyBlock(); area.GetPropertyBlock(block);
            Color cancelled = block.GetColor("_BaseColor");
            Assert.Greater(cancelled.r, cancelled.g);
            view.Hide();
            Assert.IsFalse(area.enabled);
            Assert.IsFalse(root.transform.Find("PipeAreaCenter").GetComponent<Renderer>().enabled);
        }
        finally { Object.DestroyImmediate(root); }
    }
}
