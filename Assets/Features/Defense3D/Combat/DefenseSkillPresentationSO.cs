using UnityEngine;
namespace ProjectZombie.Features.Defense3D
{
    public abstract class DefenseSkillPresentationSO : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private Color _accent = new Color(.5f,.8f,.85f);
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public Color Accent => _accent;
        public virtual bool IsValid => !string.IsNullOrWhiteSpace(_displayName) && _icon != null;
    }
}
