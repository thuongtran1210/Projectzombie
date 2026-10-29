using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Defense3D.UI;
using ProjectZombie.Features.Defense3D.Loadout;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// [TỔNG ĐẠO DIỄN KHỞI TẠO CHIẾN TRƯỜNG 3D]
    /// Chạy ở thời điểm sớm nhất (DefaultExecutionOrder -1000).
    /// Chịu trách nhiệm:
    /// 1. Khởi tạo phiên chơi (DefenseSession) duy nhất.
    /// 2. Kết nối Tướng (Hero), Quái (Enemies), Tháp (Towers), Căn Cứ (Base), Ví Gỗ (Wallet) và Giao diện (UI).
    /// 3. Inject dữ liệu chuẩn bị xuất trận (Loadout Snapshot) vào game.
    /// </summary>
    [DefaultExecutionOrder(-1000), DisallowMultipleComponent]
    public sealed class DefenseSceneCompositionRoot : MonoBehaviour
    {
        [Header("=== 1. CẤU HÌNH TƯỚNG & DANH MỤC ===")]
        [Tooltip("Bộ kỹ năng và chỉ số mặc định của Tướng trong trận")]
        [SerializeField] private DefenseHeroKitSO _heroKit;
        [Tooltip("Danh mục toàn bộ Tướng 3D để tra cứu prefab và chỉ số")]
        [SerializeField] private DefenseHeroCatalogSO _heroCatalog;

        [Header("=== 2. QUẢN LÝ VÒNG LẶP TRẬN ĐẤU & GIAO DIỆN ===")]
        [Tooltip("Bộ đếm thời gian trận 600 giây (10 phút) và điều phối Wave quái")]
        [SerializeField] private DefenseRun6003D _run;
        [Tooltip("Presenter kết nối dữ liệu trận đánh lên màn hình HUD (Thời gian, Máu, Wave)")]
        [SerializeField] private DefenseMatchPresenter _matchUI;
        [Tooltip("Ví lưu trữ tài nguyên Gỗ dùng để mua và nâng cấp tháp canh")]
        [SerializeField] private DefenseWoodWallet3D _wallet;

        [Header("=== 3. CÁC THỰC THỂ CHIẾN TRƯỜNG (SCENE PARTICIPANTS) ===")]
        [Tooltip("Danh sách các đối tượng trên sân cần lắng nghe phiên chơi (Tướng, Căn cứ, Socket tháp, Cửa hàng làng...)")]
        [SerializeField] private DefenseGameplayBehaviour[] _sceneParticipants;
        [Tooltip("Các presenter điều khiển hiển thị kỹ năng Tướng trên thanh HUD")]
        [SerializeField] private DefenseSkillPresenter3D[] _skillPresenters;

        [Header("=== 4. PHẢN HỒI CHIẾN ĐẤU (COMBAT TEXT & VFX) ===")]
        [Tooltip("Hệ thống hiển thị chữ số sát thương nhảy lên khi đánh trúng")]
        [SerializeField] private ProjectZombie.Features.UI.DamageText.DamageTextManager _combatText;
        [Tooltip("Camera chính dùng để chiếu tọa độ 3D sát thương lên Canvas")]
        [SerializeField] private Camera _combatTextCamera;
        [Tooltip("Danh sách các thực thể được gắn hiển thị số sát thương")]
        [SerializeField] private DefenseCombatTextBinding[] _combatTextActors;
        [Serializable]
        private struct DefenseCombatTextBinding
        {
            public MonoBehaviour Actor;
            public ProjectZombie.Features.UI.DamageText.CombatTextTarget Target;
            public Vector3 Offset;
        }
        [Tooltip("Dịch vụ xử lý hiệu ứng kỹ xảo VFX trong trận")]
        [SerializeField] private DefenseEffectService3D _effectService;
        [Tooltip("Các gốc chứa GameObject sinh ra trong lúc chơi để quản lý vòng đời")]
        [SerializeField] private DefenseRuntimeRoots3D _runtimeRoots;
        [Tooltip("Các presenter hiển thị hiệu ứng Cổ Vật (Relic Effects)")]
        [SerializeField] private DefenseRelicEffectPresenter3D[] _effectPresenters;

        [Header("=== 5. BỘ ĐIỀU KHIỂN & KỸ NĂNG TƯỚNG ===")]
        [Tooltip("Presenter đòn đánh thường của Tướng")]
        [SerializeField] private DefenseHeroBasicAttackPresenter3D _basicAttackPresenter;
        [Tooltip("Presenter kỹ năng lướt (Dash) né đòn của Tướng")]
        [SerializeField] private DefenseHeroDashPresenter3D _dashPresenter;
        [Tooltip("Presenter tuyệt kỹ riêng biệt của Tướng (VD: Thư Sinh)")]
        [SerializeField] private DefenseScholarSignaturePresenter3D _signaturePresenter;
        private DefenseSession _session;
        private DefenseHeroCommands _heroCommands;
        private DefenseHeroHealth3D _heroHealth;
        private readonly DefenseHeroDeviceInput _deviceInput = new DefenseHeroDeviceInput();
        private bool _hasFocus = true;
        private bool _applicationPaused;
        private DefenseLoadoutSnapshot _loadoutSnapshot;

        public IDefenseGameplayContext Gameplay => _session;

        /// <summary>Cấu hình snapshot đội hình xuất trận trước khi Session được khởi động.</summary>
        public void SetLoadoutSnapshot(DefenseLoadoutSnapshot snapshot)
        {
            _loadoutSnapshot = snapshot;
        }

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
                BindCombatText();
                BindEffects();
                if (!_run.InitializeSession(_session, _wallet, _combatText, _effectService))
                    throw new InvalidOperationException("Run configuration could not be initialized.");
                foreach (var participant in _sceneParticipants)
                    if (participant is DefenseVillageShop3D shop) shop.BindWoodDrops(_run.WoodDrops);
                foreach (var presenter in _skillPresenters) presenter.Bind(_session);
                _session.RuntimeAdapter.RegisterOwner(_matchUI.CancelRunPresentation, ResetPresentationAndWallet);
                ApplyLoadout();
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

        private void ApplyLoadout()
        {
            if (_loadoutSnapshot == null)
            {
                _loadoutSnapshot = DefenseSessionContext.ConsumeSnapshot();
            }

            var resolved = DefenseLoadoutResolver.Resolve(_loadoutSnapshot);

            // Tự động tìm catalog trong Resources nếu Inspector chưa gán
            if (_heroCatalog == null)
            {
                _heroCatalog = Resources.Load<DefenseHeroCatalogSO>("Defense3D/DefenseHeroCatalog");
            }

            DefenseHeroMotor3D motor = null;
            DefenseHeroAutoAttack3D attack = null;
            DefenseHeroVisualController3D visual = null;
            DefenseTowerBuildPrototype3D towerBuilder = null;

            foreach (var participant in _sceneParticipants)
            {
                if (participant is DefenseVillageShop3D shop)
                {
                    DefenseLoadoutRuntimeAdapter.ApplyToShop(resolved, shop);
                }
                else if (participant is DefenseHeroMotor3D m) motor = m;
                else if (participant is DefenseHeroAutoAttack3D a) attack = a;
                else if (participant is DefenseHeroVisualController3D v) visual = v;
                else if (participant is DefenseTowerBuildPrototype3D tb) towerBuilder = tb;
            }

            if (_heroHealth != null)
            {
                DefenseLoadoutRuntimeAdapter.ApplyHero(resolved, _heroCatalog, _heroHealth, motor, attack, visual);
            }

            if (towerBuilder != null)
            {
                DefenseLoadoutRuntimeAdapter.ApplyTowers(resolved, towerBuilder);
            }

            if (_heroKit != null)
            {
                DefenseLoadoutRuntimeAdapter.ApplyToHeroKit(resolved, _heroKit);
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

        private void BindEffects()
        {
            if (_effectService == null) return;
            if (_runtimeRoots == null) throw new InvalidOperationException("Assign VFX Runtime Roots.");
            _effectService.Bind(_session, _runtimeRoots);
            if (_basicAttackPresenter != null) _basicAttackPresenter.Bind(_session, _effectService);
            if (_dashPresenter != null) _dashPresenter.Bind(_session, _effectService);
            if (_signaturePresenter != null) _signaturePresenter.Bind(_session, _effectService);
            if (_effectPresenters != null) foreach (var presenter in _effectPresenters)
            {
                if (presenter == null) throw new InvalidOperationException("Missing effect presenter.");
                presenter.Bind(_session, _effectService);
            }
        }

        private void BindCombatText()
        {
            if (_combatText == null) return;
            if (_combatTextCamera == null) throw new InvalidOperationException("Assign Combat Text Camera.");
            _combatText.SetLegacyInput(false);
            _combatText.Configure(_combatTextCamera, true, () => _session != null && _session.AdvancesGameplay);
            _combatText.SetAccepting(true);
            var bound = new HashSet<IDefenseDamageable3D>();
            if (_combatTextActors != null)
                foreach (var binding in _combatTextActors)
                {
                    if (binding.Actor == null || !(binding.Actor is IDefenseDamageable3D source) ||
                        binding.Actor.gameObject.scene != gameObject.scene || !bound.Add(source))
                        throw new InvalidOperationException("Combat Text Actors require unique same-scene damageable actors.");
                    var actor = binding.Actor;
                    var offset = binding.Offset;
                    _session.Own(new DefenseCombatTextAdapter(source, _session, _combatText,
                        () => actor.transform.position + offset, binding.Target));
                }
            foreach (var participant in _sceneParticipants)
            {
                if (!(participant is IDefenseDamageable3D source) || !bound.Add(source)) continue;
                var actor = participant;
                var target = participant is DefenseHeroHealth3D
                    ? ProjectZombie.Features.UI.DamageText.CombatTextTarget.Player
                    : participant is DefenseEnemy3D ? ProjectZombie.Features.UI.DamageText.CombatTextTarget.Enemy
                    : participant is DefenseSupportTarget3D support && !support.IsStructure
                    ? ProjectZombie.Features.UI.DamageText.CombatTextTarget.Ally
                    : ProjectZombie.Features.UI.DamageText.CombatTextTarget.Structure;
                _session.Own(new DefenseCombatTextAdapter(source, _session, _combatText,
                    () => actor.transform.position + Vector3.up, target));
            }
            _session.RunCleanup += ClearCombatText;
        }
        private void ClearCombatText() { if (_combatText != null) _combatText.Clear(); }
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
            if (_basicAttackPresenter != null) _basicAttackPresenter.Unbind();
            if (_dashPresenter != null) _dashPresenter.Unbind();
            if (_signaturePresenter != null) _signaturePresenter.Unbind();
            if (_effectPresenters != null) foreach (var presenter in _effectPresenters) if (presenter != null) presenter.Unbind();
            if (_effectService != null) _effectService.Unbind();
            if (_session != null) _session.RunCleanup -= ClearCombatText;
            if (_combatText != null) { _combatText.SetAccepting(false); _combatText.Clear(); }
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
