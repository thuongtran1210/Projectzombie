using UnityEngine;
using UnityEngine.UI;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>Authored non-interactive vector badge: gold countdown, type silhouette and offscreen arrow.</summary>
    public sealed class DefenseSpawnWarningGraphic : MaskableGraphic
    {
        [SerializeField] private Color _gold = new Color(.824f, .694f, .451f, 1f);
        [SerializeField] private Color _danger = new Color(.843f, .471f, .435f, .95f);
        private float _fraction, _angle, _pulse;
        private bool _offscreen;
        private DefenseSpawnWarningSymbol _symbol;
        private bool _pending, _arrived;
        private const int SEGMENTS = 64;

        public void Render(float fraction, float angle, bool offscreen, DefenseSpawnWarningSymbol symbol, float pulse,
            bool pending = false, bool arrived = false)
        {
            bool dirty = Mathf.Abs(_fraction - fraction) > .002f || Mathf.Abs(_angle - angle) > .25f ||
                _offscreen != offscreen || _symbol != symbol || Mathf.Abs(_pulse - pulse) > .02f || _pending != pending || _arrived != arrived;
            _fraction = Mathf.Clamp01(fraction);
            _angle = angle;
            _offscreen = offscreen;
            _symbol = symbol;
            _pulse = Mathf.Clamp01(pulse);
            _pending = pending; _arrived = arrived;
            if (dirty) SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Disc(vh, Vector2.zero, 18f, new Color(.05f, .09f, .095f, .94f));
            Ring(vh, 19f, 21.5f + _pulse * .5f, 1f, new Color(_gold.r, _gold.g, _gold.b, .07f + .07f * _pulse));
            Ring(vh, 17f, 19f, 1f, new Color(_danger.r, _danger.g, _danger.b, .28f));
            Color status = _arrived ? new Color(.35f, .9f, .7f, 1f) : _pending ? _danger : _gold;
            Ring(vh, 17f, 19f, _pending || _arrived ? 1f : _fraction, status);
            DrawSymbol(vh);
            if (_pending)
                for (int i = 0; i < 3; i++) Disc(vh, new Vector2(-5f + i * 5f, -12f), 1.3f + _pulse * .4f, _gold);
            if (_offscreen)
            {
                float a = _angle * Mathf.Deg2Rad;
                Vector2 forward = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                Vector2 side = new Vector2(-forward.y, forward.x);
                Triangle(vh, forward * 31f, forward * 23f + side * 5f, forward * 23f - side * 5f, _danger);
            }
        }

        private void DrawSymbol(VertexHelper vh)
        {
            Color pink = new Color(.78f, .23f, .62f, 1f);
            if (_symbol == DefenseSpawnWarningSymbol.Armored || _symbol == DefenseSpawnWarningSymbol.Commander)
            {
                Triangle(vh, new Vector2(-10,7), new Vector2(10,7), new Vector2(0,-12), new Color(.4f,.7f,.75f,1));
                Triangle(vh, new Vector2(-7,5), new Vector2(7,5), new Vector2(0,-7), new Color(.1f,.18f,.2f,1));
                Disc(vh, new Vector2(-3,1), 1.8f, Color.white);
                Disc(vh, new Vector2(3,1), 1.8f, Color.white);
                if (_symbol == DefenseSpawnWarningSymbol.Commander)
                {
                    Triangle(vh, new Vector2(-10,7), new Vector2(-9,14), new Vector2(0,8), _gold);
                    Triangle(vh, new Vector2(-6,7), new Vector2(0,15), new Vector2(6,7), _gold);
                    Triangle(vh, new Vector2(0,8), new Vector2(9,14), new Vector2(10,7), _gold);
                }
                return;
            }
            if (_symbol == DefenseSpawnWarningSymbol.Cannon)
            {
                Disc(vh, new Vector2(-5,-7), 4f, _gold);
                Disc(vh, new Vector2(6,-7), 4f, _gold);
                Triangle(vh, new Vector2(-10,-3), new Vector2(9,-3), new Vector2(4,8), _danger);
                Triangle(vh, new Vector2(0,5), new Vector2(11,10), new Vector2(13,4), _gold);
                return;
            }
            if (_symbol == DefenseSpawnWarningSymbol.Boss)
            {
                Triangle(vh, new Vector2(-10f, -5f), new Vector2(-11f, 8f), new Vector2(0f, -1f), _gold);
                Triangle(vh, new Vector2(-7f, -5f), new Vector2(0f, 12f), new Vector2(7f, -5f), _gold);
                Triangle(vh, new Vector2(0f, -1f), new Vector2(11f, 8f), new Vector2(10f, -5f), _gold);
                return;
            }
            if (_symbol == DefenseSpawnWarningSymbol.Fish)
            {
                Disc(vh, Vector2.zero, 8f, new Color(.18f, .75f, .85f, 1f));
                Triangle(vh, new Vector2(5f, 0f), new Vector2(13f, 7f), new Vector2(13f, -7f), new Color(.18f, .75f, .85f, 1f));
                Disc(vh, new Vector2(-3f, 2f), 2f, Color.white);
                return;
            }
            if (_symbol == DefenseSpawnWarningSymbol.Bat || _symbol == DefenseSpawnWarningSymbol.Elite)
            {
                Color wing = _symbol == DefenseSpawnWarningSymbol.Elite ? _gold : pink;
                Triangle(vh, new Vector2(-2f, -5f), new Vector2(-14f, 9f), new Vector2(-12f, -3f), wing);
                Triangle(vh, new Vector2(2f, -5f), new Vector2(14f, 9f), new Vector2(12f, -3f), wing);
            }
            Disc(vh, Vector2.zero, 8f, pink);
            Triangle(vh, new Vector2(-7f, 4f), new Vector2(-9f, 13f), new Vector2(-1f, 7f), _gold);
            Triangle(vh, new Vector2(7f, 4f), new Vector2(9f, 13f), new Vector2(1f, 7f), _gold);
            Disc(vh, new Vector2(-3f, 1f), 2f, Color.white);
            Disc(vh, new Vector2(3f, 1f), 2f, Color.white);
            Triangle(vh, new Vector2(-3f, -3f), new Vector2(3f, -3f), new Vector2(0f, -6f), Color.white);
            if (_symbol == DefenseSpawnWarningSymbol.Parachute)
                Triangle(vh, new Vector2(-11f, 10f), new Vector2(0f, 16f), new Vector2(11f, 10f), Color.white);
            if (_symbol == DefenseSpawnWarningSymbol.Cannon)
                Disc(vh, new Vector2(0f, 2f), 6f, new Color(.15f, .16f, .18f, 1f));
        }

        private static void Ring(VertexHelper vh, float inner, float outer, float fraction, Color tint)
        {
            int segments = Mathf.CeilToInt(SEGMENTS * fraction);
            for (int i = 0; i < segments; i++)
            {
                float a = (90f + i * 360f / SEGMENTS) * Mathf.Deg2Rad;
                float b = (90f + Mathf.Min((i + 1f) / SEGMENTS, fraction) * 360f) * Mathf.Deg2Rad;
                Vector2 first = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                Vector2 last = new Vector2(Mathf.Cos(b), Mathf.Sin(b));
                Triangle(vh, first * inner, first * outer, last * outer, tint);
                Triangle(vh, first * inner, last * outer, last * inner, tint);
            }
        }

        private static void Disc(VertexHelper vh, Vector2 center, float radius, Color tint)
        {
            for (int i = 0; i < 24; i++)
            {
                float a = i * Mathf.PI / 12f, b = (i + 1) * Mathf.PI / 12f;
                Triangle(vh, center, center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius,
                    center + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * radius, tint);
            }
        }

        private static void Triangle(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Color tint)
        {
            int index = vh.currentVertCount;
            vh.AddVert(a, tint, Vector2.zero);
            vh.AddVert(b, tint, Vector2.zero);
            vh.AddVert(c, tint, Vector2.zero);
            vh.AddTriangle(index, index + 1, index + 2);
        }
    }
}
