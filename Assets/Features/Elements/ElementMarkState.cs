using ProjectZombie.Features.Shared;
using UnityEngine;

namespace ProjectZombie.Features.Elements
{
    /// <summary>Stores one target-local elemental mark and its expiry.</summary>
    internal sealed class ElementMarkState
    {
        private ElementType _element;
        private Object _source;
        private float _expiresAt;

        public ElementType GetElement(float now) => now < _expiresAt ? _element : ElementType.None;
        public Object GetSource(float now) => now < _expiresAt ? _source : null;
        public float ExpiresAt => _expiresAt;

        public void Apply(ElementType element, Object source, float expiresAt)
        {
            _element = element;
            _source = source;
            _expiresAt = expiresAt;
        }

        public void Clear()
        {
            _element = ElementType.None;
            _source = null;
            _expiresAt = 0f;
        }
    }
}
