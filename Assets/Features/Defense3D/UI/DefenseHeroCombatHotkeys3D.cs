using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>Device adapter: all keys use the same scoped presenter path as pointer releases.</summary>
    public sealed class DefenseHeroCombatHotkeys3D : MonoBehaviour
    {
        [SerializeField] private DefenseSkillPresenter3D _attack;
        [SerializeField] private DefenseSkillPresenter3D _dash;
        [SerializeField] private DefenseSkillPresenter3D _signature;
        [SerializeField] private DefenseSkillPresenter3D _relic;
        private void Update()
        {
            var keys = Keyboard.current;
            if (!Application.isFocused || keys == null) return;
            var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            if (selected != null && selected.GetComponent<TMP_InputField>() != null) return;
            if (keys.jKey.isPressed) _attack?.TryQuickCast();
            if (keys.kKey.wasPressedThisFrame) _signature?.TryQuickCast();
            if (keys.spaceKey.wasPressedThisFrame) _dash?.TryQuickCast();
            if (keys.qKey.wasPressedThisFrame) _relic?.TryQuickCast();
        }
    }
}
