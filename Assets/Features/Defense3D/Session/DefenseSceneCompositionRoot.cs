using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Defense3D.UI;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Validates authored bindings and constructs a session without creating scene objects.</summary>
    [DefaultExecutionOrder(-1000), DisallowMultipleComponent]
    public sealed class DefenseSceneCompositionRoot : MonoBehaviour
    {
        [SerializeField] private DefenseHeroKitSO _heroKit;
        [SerializeField] private DefenseRun6003D _run;
        [SerializeField] private DefenseMatchPresenter _matchUI;
        [SerializeField] private DefenseWoodWallet3D _wallet;
        [SerializeField] private DefenseGameplayBehaviour[] _sceneParticipants;
        [SerializeField] private DefenseSkillPresenter3D[] _skillPresenters;
        private DefenseSession _session;
        private DefenseHeroCommands _heroCommands;
        private DefenseHeroHealth3D _heroHealth;
        private readonly DefenseHeroDeviceInput _deviceInput = new DefenseHeroDeviceInput();
        private bool _hasFocus = true;
        private bool _applicationPaused;

        public IDefenseGameplayContext Gameplay => _session;

        private void Start()
        {
            try
            {
                ValidateBindings();
                _hasFocus = Application.isFocused;
                _session = new DefenseSession();
                _heroCommands = new DefenseHeroCommands(_session);
                _session.Own(_heroCommands);
                foreach (var participant in _sceneParticipants) participant.BindGameplay(_session);
                foreach (var participant in _sceneParticipants)
                {
                    if (participant is DefenseHeroMotor3D motor) motor.BindCommands(_heroCommands);
                    if (participant is DefenseTowerBuildPrototype3D builder) builder.BindCommands(_heroCommands);
                    if (participant is DefenseHeroHealth3D health) _heroHealth = health;
                }
                _heroHealth.Defeated += ClearHeroCommands;
                _matchUI.BindSession(_session, _session.Modifiers);
                if (!_run.InitializeSession(_session, _wallet))
                    throw new InvalidOperationException("Run configuration could not be initialized.");
                foreach (var presenter in _skillPresenters) presenter.Bind(_session);
                _session.RuntimeAdapter.RegisterOwner(_matchUI.CancelRunPresentation, ResetPresentationAndWallet);
                _session.Start();
            }
            catch (Exception exception)
            {
                DisposeSession();
                if (_run != null) _run.enabled = false;
                Debug.LogError("[TREBRAVE Defense3D] Scene composition failed: " + exception.Message, this);
                enabled = false;
            }
        }

        private void ValidateBindings()
        {
            if (_run == null || _matchUI == null || _wallet == null)
                throw new InvalidOperationException("Assign Run, Match UI and Hero Wallet on the composition root.");
            if (_run.gameObject.scene != gameObject.scene || _matchUI.gameObject.scene != gameObject.scene ||
                _wallet.gameObject.scene != gameObject.scene)
                throw new InvalidOperationException("Run, Match UI and Wallet must be instances in the composition root's scene.");
            if (!_run.isActiveAndEnabled || !_matchUI.isActiveAndEnabled)
                throw new InvalidOperationException("Run and Match Presenter must be active and enabled before starting the session.");
            if (_sceneParticipants == null || _sceneParticipants.Length == 0)
                throw new InvalidOperationException("Assign authored gameplay participants, including inactive socket towers.");
            var unique = new HashSet<DefenseGameplayBehaviour>();
            bool hasHeroMotor = false;
            bool hasHeroAttack = false;
            bool hasTowerBuilder = false;
            bool hasBaseHealth = false;
            bool hasHeroHealth = false;
            for (int i = 0; i < _sceneParticipants.Length; i++)
            {
                if (_sceneParticipants[i] == null || !unique.Add(_sceneParticipants[i]))
                    throw new InvalidOperationException("Null or duplicate gameplay participant at index " + i + ".");
                if (_sceneParticipants[i].gameObject.scene != gameObject.scene)
                    throw new InvalidOperationException("Participant must belong to this scene: " + _sceneParticipants[i].name);
                hasHeroMotor |= _sceneParticipants[i] is DefenseHeroMotor3D;
                hasHeroAttack |= _sceneParticipants[i] is DefenseHeroAutoAttack3D;
                hasTowerBuilder |= _sceneParticipants[i] is DefenseTowerBuildPrototype3D;
                hasBaseHealth |= _sceneParticipants[i] is DefenseBasePrototype3D;
                hasHeroHealth |= _sceneParticipants[i] is DefenseHeroHealth3D;
            }
            if (!hasHeroMotor || !hasHeroAttack || !hasTowerBuilder || !hasBaseHealth || !hasHeroHealth)
                throw new InvalidOperationException("Participants must include Hero Motor/Attack/Health, Base Health and Tower Builder.");
            if (_heroKit == null) throw new InvalidOperationException("Assign authored Hero Kit.");
            _heroKit.Validate();
            if (_skillPresenters == null || _skillPresenters.Length != 4)
                throw new InvalidOperationException("Assign Attack, Dash, Signature and Relic presenters.");
            var kinds = new HashSet<DefenseSkillKind>();
            foreach (var presenter in _skillPresenters)
            {
                if (presenter == null || presenter.gameObject.scene != gameObject.scene || !presenter.isActiveAndEnabled)
                    throw new InvalidOperationException("Skill presenters must be active instances in this scene.");
                presenter.ValidateBindings();
                if (_heroKit.DefinitionFor(presenter.Kind) != presenter.DefinitionSource || !kinds.Add(presenter.Kind) || !unique.Contains(presenter.Effect))
                    throw new InvalidOperationException("Skill kinds must be unique and effects must be bound scene participants.");
            }
        }

        private void ResetPresentationAndWallet()
        {
            _wallet.ResetWallet();
            _matchUI.ResetRunPresentation();
        }

        private void Update()
        {
            if (_heroCommands == null) return;
            _deviceInput.ReadInto(_heroCommands, _hasFocus && !_applicationPaused && _session != null && _session.AdvancesGameplay &&
                _heroHealth != null && _heroHealth.isActiveAndEnabled && !_heroHealth.IsDown);
        }

        private void ClearHeroCommands() => _heroCommands?.Clear();

        private void OnApplicationFocus(bool focused)
        {
            _hasFocus = focused;
            if (!focused) ClearHeroCommands();
        }

        private void OnDisable() => ClearHeroCommands();

        private void OnApplicationPause(bool paused)
        {
            _applicationPaused = paused;
            if (paused) ClearHeroCommands();
        }

        private void OnDestroy()
        {
            DisposeSession();
        }

        private void DisposeSession()
        {
            if (_session != null && _session.AdvancesGameplay)
            {
                try { _session.Pause(); }
                catch (Exception exception) { Debug.LogException(exception, this); }
            }
            if (_heroHealth != null) _heroHealth.Defeated -= ClearHeroCommands;
            if (_skillPresenters != null)
                foreach (var presenter in _skillPresenters)
                {
                    if (presenter == null) continue;
                    try { presenter.Unbind(); }
                    catch (Exception exception) { Debug.LogException(exception, this); }
                }
            try { _session?.Dispose(); }
            catch (Exception exception) { Debug.LogException(exception, this); }
        }
    }
}
