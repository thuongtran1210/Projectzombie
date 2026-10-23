using System.Reflection;
using NUnit.Framework;
using ProjectZombie.Features.Collectibles;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Shared;
using UnityEngine;

namespace ProjectZombie.Tests.Editor
{
    /// <summary>
    /// Small EditMode regression checks for the hybrid 2D/3D migration bridge.
    /// The collider handlers are invoked directly; these are not PlayMode physics tests.
    /// </summary>
    [TestFixture]
    public sealed class Migration3DFoundationRegressionTests
    {
        private GameObject _hero;
        private GameObject _pickup;

        [TearDown]
        public void TearDown()
        {
            if (_pickup != null) Object.DestroyImmediate(_pickup);
            if (_hero != null) Object.DestroyImmediate(_hero);
        }

        [Test]
        public void MagnetProxy_3DTrigger_ResolvesCollectibleOnParent()
        {
            var magnet = CreateHero(is3D: true);
            var proxy = magnet.GetComponentInChildren<MagnetTriggerProxy>();
            Assert.IsNotNull(proxy);
            Assert.IsNotNull(proxy.GetComponent<SphereCollider>());

            var gem = CreateGemWithChildCollider(is3D: true, out var collider);
            Assert.IsTrue(gem.IsActiveOnGround);

            InvokeTrigger(proxy, "OnTriggerEnter", collider);

            Assert.IsFalse(gem.IsActiveOnGround,
                "The 3D callback should start the collectible's magnet effect.");
        }

        [Test]
        public void MagnetProxy_2DTrigger_StillResolvesCollectibleOnParent()
        {
            var magnet = CreateHero(is3D: false);
            var proxy = magnet.GetComponentInChildren<MagnetTriggerProxy>();
            Assert.IsNotNull(proxy);
            Assert.IsNotNull(proxy.GetComponent<CircleCollider2D>());

            var gem = CreateGemWithChildCollider(is3D: false, out var collider);
            Assert.IsTrue(gem.IsActiveOnGround);

            InvokeTrigger(proxy, "OnTriggerEnter2D", collider);

            Assert.IsFalse(gem.IsActiveOnGround,
                "The original 2D callback must continue working.");
        }

        [Test]
        public void NetworkMovement_3DOnlyBody_DoesNotRequireRigidbody2D()
        {
            CreateHero(is3D: true, withController: true);
            var controller = _hero.GetComponent<PlayerController>();
            var rigidbody3D = _hero.GetComponent<Rigidbody>();
            Assert.IsNull(_hero.GetComponent<Rigidbody2D>());
            Assert.IsNotNull(controller);
            Assert.IsNotNull(rigidbody3D);

            controller.enabled = false; // Forces the early-stop branch independently of GameState.
            rigidbody3D.velocity = new Vector3(3f, 0f, 4f);

            Assert.DoesNotThrow(() => controller.ApplyNetworkMovement(Vector2.right));
            Assert.That(rigidbody3D.velocity.x, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(rigidbody3D.velocity.z, Is.EqualTo(0f).Within(0.0001f));

            var healthField = typeof(PlayerController).GetField(
                "_healthSystem", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(healthField);
            Assert.IsNotNull(healthField.GetValue(controller),
                "Controller Awake should see HealthSystem on a fully assembled sandbox Hero.");
        }

        private Transform CreateHero(bool is3D, bool withController = false)
        {
            _hero = new GameObject("Migration3D_TestHero");
            _hero.SetActive(false);
            if (is3D)
            {
                var rb3D = _hero.AddComponent<Rigidbody>();
                rb3D.useGravity = false;
            }
            else
            {
                var rb2D = _hero.AddComponent<Rigidbody2D>();
                rb2D.gravityScale = 0f;
            }

            _hero.AddComponent<HealthSystem>();
            _hero.AddComponent<PlayerStats>();
            _hero.AddComponent<PlayerMagnetTrigger>();
            if (withController) _hero.AddComponent<PlayerController>();
            _hero.SetActive(true);
            return _hero.transform;
        }

        private ExpGem CreateGemWithChildCollider(bool is3D, out Component hitCollider)
        {
            _pickup = new GameObject("Migration3D_TestGem");
            _pickup.AddComponent<SpriteRenderer>();
            var gem = _pickup.AddComponent<ExpGem>();
            var child = new GameObject("PickupHitbox");
            child.transform.SetParent(_pickup.transform, false);
            hitCollider = is3D
                ? (Component)child.AddComponent<BoxCollider>()
                : child.AddComponent<BoxCollider2D>();
            return gem;
        }

        private static void InvokeTrigger(MagnetTriggerProxy proxy, string methodName, Component collider)
        {
            var callback = typeof(MagnetTriggerProxy).GetMethod(
                methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(callback);
            callback.Invoke(proxy, new object[] { collider });
        }
    }
}
