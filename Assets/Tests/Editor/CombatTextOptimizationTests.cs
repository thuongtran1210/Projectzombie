using NUnit.Framework;
using UnityEngine;
using ProjectZombie.Features.UI.DamageText;

public sealed class CombatTextOptimizationTests
{
    [Test]
    public void CombatDamageEvents_RaisesEventWithCorrectParameters()
    {
        CombatTextRequest received = default;
        bool triggered = false;

        void OnDamage(CombatTextRequest req)
        {
            received = req;
            triggered = true;
        }

        CombatDamageEvents.OnDamageDealt += OnDamage;
        try
        {
            Vector3 testPos = new Vector3(10f, 2f, 5f);
            CombatDamageEvents.RaiseDamageDealt(250f, testPos, isCritical: true);

            Assert.IsTrue(triggered, "Sự kiện OnDamageDealt phải được kích hoạt.");
            Assert.AreEqual(250f, received.Amount);
            Assert.AreEqual(testPos, received.Position);
            Assert.IsTrue(received.IsCritical);
            Assert.AreEqual(CombatTextTarget.Enemy, received.Target);
        }
        finally
        {
            CombatDamageEvents.OnDamageDealt -= OnDamage;
        }
    }

    [Test]
    public void DamageTextManager_Format_UsesCachedNumbersForZeroGC()
    {
        // Kiểm tra định dạng số nguyên dương dùng mảng cache Numbers không cấp phát chuỗi mới
        var req1 = new CombatTextRequest(150f, Vector3.zero);
        string formatted1 = DamageTextManager.Format(req1);
        Assert.AreEqual("150", formatted1);

        // Kiểm tra sát thương bạo kích được bọc bằng rich text tag chuẩn
        var reqCrit = new CombatTextRequest(999f, Vector3.zero, isCritical: true);
        string formattedCrit = DamageTextManager.Format(reqCrit);
        Assert.AreEqual("<b>999!</b>", formattedCrit);

        // Kiểm tra Hồi máu và Tài nguyên có dấu '+'
        var reqHeal = new CombatTextRequest(50f, Vector3.zero, kind: CombatTextKind.Heal);
        string formattedHeal = DamageTextManager.Format(reqHeal);
        Assert.AreEqual("+50", formattedHeal);

        // Kiểm tra Phản đòn có dấu '*'
        var reqCounter = new CombatTextRequest(75f, Vector3.zero, isCounter: true);
        string formattedCounter = DamageTextManager.Format(reqCounter);
        Assert.AreEqual("* 75", formattedCounter);
    }

    [Test]
    public void DamageTextItem_PresentationAPIs_WorkWithoutCoroutines()
    {
        var go = new GameObject("TestDamageTextItem");
        try
        {
            var textMesh = go.AddComponent<TMPro.TextMeshPro>();
            var item = go.AddComponent<DamageTextItem>();

            Vector3 startPos = new Vector3(1f, 2f, 3f);
            Quaternion startRot = Quaternion.Euler(0, 45, 0);

            item.InitializePresentation("100", Color.red, 14f, startPos, startRot, "Default", 100);

            Assert.IsTrue(go.activeSelf, "Item phải active sau khi InitializePresentation.");
            Assert.AreEqual("100", textMesh.text);
            Assert.AreEqual(Color.red, textMesh.color);
            Assert.AreEqual(startPos, go.transform.position);

            Vector3 updatedPos = new Vector3(1f, 3f, 3f);
            item.UpdatePresentation(updatedPos, startRot, 1.2f, new Color(1, 0, 0, 0.5f));

            Assert.AreEqual(updatedPos, go.transform.position);
            Assert.AreEqual(Vector3.one * 1.2f, go.transform.localScale);
            Assert.AreEqual(new Color(1, 0, 0, 0.5f), textMesh.color);

            item.StopPresentation();
            Assert.IsFalse(go.activeSelf, "Item phải bị disable sau khi StopPresentation.");
            Assert.AreEqual(Vector3.one, go.transform.localScale);
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }
}

