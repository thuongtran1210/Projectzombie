using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Physical Wood pickup, collected only when the Hero approaches.</summary>
    public sealed class DefenseWoodDrop3D : MonoBehaviour
    {
        [SerializeField, Min(1)] private int woodAmount = 10;
        [SerializeField, Min(1f)] private float lifetime = 90f;
        [SerializeField, Min(0.1f)] private float pickupRadius = 1.4f;
        private Transform hero;
        private DefenseWoodWallet3D wallet;
        private bool collected;
        private float expireTime;

        public void Initialize(int amount, Transform collector, DefenseWoodWallet3D collectorWallet)
        {
            woodAmount = Mathf.Max(1, amount);
            hero = collector;
            wallet = collectorWallet;
            expireTime = Time.time + lifetime;
        }

        private void Update()
        {
            if (collected) return;
            if (Time.time >= expireTime)
            {
                Destroy(gameObject);
                return;
            }
            if (hero == null || wallet == null || !hero.gameObject.activeInHierarchy) return;
            Vector3 delta = hero.position - transform.position;
            delta.y = 0f;
            if (delta.sqrMagnitude > pickupRadius * pickupRadius) return;
            collected = true;
            wallet.CollectWood(woodAmount);
            Destroy(gameObject);
        }

        public static void Spawn(Vector3 deathPosition, int amount, Transform collector, DefenseWoodWallet3D collectorWallet)
        {
            if (collector == null || collectorWallet == null) return;
            var drop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            drop.name = "WoodDrop_" + amount;
            drop.transform.position = new Vector3(deathPosition.x, Mathf.Max(0.3f, deathPosition.y - 0.65f), deathPosition.z);
            drop.transform.localScale = new Vector3(0.42f, 0.28f, 0.35f);
            var collider = drop.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            var renderer = drop.GetComponent<Renderer>();
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (renderer != null && shader != null)
            {
                var material = new Material(shader);
                material.color = new Color(0.65f, 0.38f, 0.14f);
                renderer.material = material;
            }
            drop.AddComponent<DefenseWoodDrop3D>().Initialize(amount, collector, collectorWallet);
        }
    }
}
