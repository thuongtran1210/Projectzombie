using System;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using ProjectZombie.Core.Architecture;
using ProjectZombie.Features.MetaProgression;
using ProjectZombie.Features.UI;

namespace ProjectZombie.Tests.Editor
{
    public sealed class MainHubReadinessTests
    {
        [Test]
        public void HubWaitsForCoreAndRebindsCurrencyWithoutDuplicateSubscriptions()
        {
            bool previousReady = AppBootGate.IsCoreReady;
            var eventField = typeof(AppBootGate).GetField("OnCoreReady", BindingFlags.Static | BindingFlags.NonPublic);
            var previousListeners = eventField.GetValue(null);
            var previousService = ServiceContext.Get<IMetaCurrencyService>();
            var root = new GameObject("Hub readiness test");
            var service = new CurrencyStub();
            MainHubPresenter presenter = null;
            try
            {
                AppBootGate.ResetState();
                ServiceContext.Register<IMetaCurrencyService>(service);
                presenter = root.AddComponent<MainHubPresenter>();
                Invoke(presenter, "OnEnable");
                presenter.RefreshHubState();
                presenter.UpdateCurrencyDisplay();
                Assert.AreEqual(0, service.Subscriptions);
                typeof(AppBootGate).GetMethod("SetReady", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
                Assert.AreEqual(1, service.Subscriptions);
                Invoke(presenter, "OnEnable");
                Assert.AreEqual(1, service.Subscriptions);
                var view = root.AddComponent<MainHubView>();
                var labelObject = new GameObject("Balance", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelObject.transform.SetParent(root.transform);
                var label = labelObject.GetComponent<TextMeshProUGUI>();
                Set(view, "_coTienText", label);
                Set(presenter, "_view", view);
                presenter.UpdateCurrencyDisplay();
                StringAssert.Contains("123", label.text);
                service.AddCurrency(7);
                StringAssert.Contains("130", label.text);
                Invoke(presenter, "OnDisable");
                Assert.AreEqual(0, service.Subscriptions);
                service.AddCurrency(10);
                StringAssert.Contains("130", label.text);
                Set(presenter, "_view", null);
                Invoke(presenter, "OnEnable");
                Assert.AreEqual(1, service.Subscriptions);
            }
            finally
            {
                if (presenter != null) Invoke(presenter, "OnDestroy");
                UnityEngine.Object.DestroyImmediate(root);
                if (previousService != null) ServiceContext.Register(previousService);
                else ServiceContext.Unregister<IMetaCurrencyService>();
                AppBootGate.ResetState();
                typeof(AppBootGate).GetProperty("IsCoreReady").GetSetMethod(true).Invoke(null, new object[] { previousReady });
                eventField.SetValue(null, previousListeners);
            }
            Assert.AreEqual(0, service.Subscriptions);
        }
        private static void Invoke(object instance, string method) => instance.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(instance, null);
        private static void Set(object instance, string field, object value) => instance.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(instance, value);
        private sealed class CurrencyStub : IMetaCurrencyService
        {
            private Action<int> _changed;
            public int Subscriptions;
            public int TotalCurrency { get; private set; } = 123;
            public event Action<int> OnCurrencyChanged
            {
                add { _changed += value; Subscriptions++; }
                remove { _changed -= value; Subscriptions--; }
            }
            public void AddCurrency(int amount) { TotalCurrency += amount; _changed?.Invoke(TotalCurrency); }
            public bool SpendCurrency(int amount) { if (amount > TotalCurrency) return false; AddCurrency(-amount); return true; }
            public MetaProgressionSaveData GetSaveData() => null;
            public bool IsCharacterUnlocked(string id) => false;
            public void UnlockCharacter(string id) { }
        }
    }
}
