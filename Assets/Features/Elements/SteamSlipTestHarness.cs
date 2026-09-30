using System.Collections.Generic;
using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Player.Input;
using ProjectZombie.Features.Player.Skills;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace ProjectZombie.Features.Elements
{
    /// <summary>Editor Play Mode harness for spawning real actors and testing Steam Slip hit paths.</summary>
    public sealed class SteamSlipTestHarness : MonoBehaviour
    {
        private const string CharacterDatabasePath = "Assets/_Data/CharacterDatabase.asset";
        private const string EnemyFolder = "Assets/_Prefabs/Characters/Enemies";
        private const string WeaponFolder = "Assets/_Data/Weapons";
        private const string SkillFolder = "Assets/_Data/Skills";
        [SerializeField] private Vector3 playerSpawnPosition = new Vector3(0f, -1f, 0f);
        [SerializeField] private Vector3 enemySpawnPosition = new Vector3(0f, 0.8f, 0f);
        private readonly List<CharacterDataSO> _characters = new List<CharacterDataSO>();
        private readonly List<WeaponData> _weapons = new List<WeaponData>();
        private readonly List<SignatureSkillData> _skills = new List<SignatureSkillData>();
        private readonly List<EnemyChoice> _enemies = new List<EnemyChoice>();
        private int _characterIndex, _weaponIndex, _enemyIndex, _skillIndex;
        private GameObject _playerInstance;
        private Enemy _enemyInstance;
        private Vector2 _scroll;
        private string _message = "Chọn nhân vật, vũ khí và enemy rồi Spawn.";
        private sealed class EnemyChoice { public string Label; public GameObject Prefab; }

#if UNITY_EDITOR
        private void Awake() { CleanupIsolatedScene(); EnsureProjectileSystem(); LoadChoices(); BuildTouchControls(); SteamSlipReactionDiagnostics.Reported += HandleReactionDiagnostic; }
#endif
#if UNITY_EDITOR
        private void OnDestroy() { SteamSlipReactionDiagnostics.Reported -= HandleReactionDiagnostic; }
        private void HandleReactionDiagnostic(SteamSlipDiagnosticEvent diagnosticEvent)
        {
            if (diagnosticEvent.Kind == SteamSlipDiagnosticKind.ReactionTriggered)
                _message = $"BỐC HƠI triggered — enemy {diagnosticEvent.EnemyId}, incoming {diagnosticEvent.Element}.";
            else if (diagnosticEvent.Kind == SteamSlipDiagnosticKind.PatchCreated)
                _message = $"Wet patch created (slot {diagnosticEvent.Count}).";
            else if (diagnosticEvent.Kind == SteamSlipDiagnosticKind.PatchCapacityRejected)
                _message = "Reaction triggered, but patch capacity is full.";
        }
#endif
#if UNITY_EDITOR
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(16f, 16f, 420f, Mathf.Min(420f, Screen.height * 0.55f)), GUI.skin.box);
            _scroll = GUILayout.BeginScrollView(_scroll);
            GUILayout.Label("BỐC HƠI / STEAM SLIP — PLAY MODE TEST");
            GUILayout.Label("Spawn prefab thật; attack đi qua callback damage của game.");
            GUILayout.Label("1) Player");
            DrawChoice("Character", _characters.Count, ref _characterIndex, i => _characters[i].characterName);
            if (GUILayout.Button(_playerInstance == null ? "Spawn Player" : "Replace Player")) SpawnPlayer();
            GUILayout.Label(_playerInstance != null ? $"Player: {_playerInstance.name}" : "Player: chưa spawn");
            GUILayout.Label($"Kỹ năng nhân vật: {ActiveSkillLabel()}");
            DrawChoice("Dữ liệu kỹ năng", _skills.Count, ref _skillIndex, i => _skills[i].SkillName);
            if (GUILayout.Button("Nạp kỹ năng đã chọn vào nhân vật")) LoadSelectedSignatureSkill();
            GUILayout.Space(5f); GUILayout.Label("2) Weapon / relic");
            DrawChoice("Weapon", _weapons.Count, ref _weaponIndex, WeaponLabel);
            GUILayout.Label(WeaponDescription());
            if (GUILayout.Button("Equip selected weapon / relic")) EquipWeapon();
            GUILayout.Space(5f); GUILayout.Label("3) Enemy");
            DrawChoice("Enemy", _enemies.Count, ref _enemyIndex, EnemyLabel);
            GUILayout.Label(EnemyDescription());
            if (GUILayout.Button(_enemyInstance == null ? "Spawn Enemy" : "Replace Enemy")) SpawnEnemy();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Spawn enemy phụ")) SpawnSecondaryEnemy();
            if (GUILayout.Button("Despawn enemies")) DespawnEnemies();
            GUILayout.EndHorizontal();
            GUILayout.Space(8f); GUILayout.Label("Kịch bản / hành vi kỳ vọng");
            GUILayout.Label("A. Chọn enemy hệ Hỏa (ví dụ E_HOALYTINH_HồLyTinhNhỏ), equip W009 (Thủy), rồi bấm Fire: hit Thủy vào mục tiêu Hỏa → Bốc Hơi ngay.");
            GUILayout.Label("B. Mục tiêu không có hệ tự nhiên vẫn dùng được prime: Thủy rồi Hỏa trên cùng enemy trong 4 giây → Bốc Hơi.");
            GUILayout.Label("C. Hỏa rồi Thủy không kích hoạt Bốc Hơi; hệ nguyên tố tự nhiên của enemy được dùng cho phản ứng trực tiếp.");
            GUILayout.Label("D. Chọn Boss Nguu Dau Ma Dien rồi làm đúng Thủy → Hỏa → chỉ VFX/SFX, không bị trượt/choáng.");
            GUILayout.Label("E. Attack thường gọi CharacterCombat thật. Chỉ prime nếu basicAttackConfig.element là Thủy/Hỏa; element None nghĩa là không có sát thương nguyên tố.");
            GUILayout.Label("Dùng Attack/Skill ở góc phải dưới và Joystick ở góc trái dưới. Panel này chỉ dành cho chọn/spawn.");
            GUILayout.Label("W009 Fire bắn thẳng vào enemy đang chọn để kiểm tra hit callback; passive relic cũng tự đánh theo nhịp.");
            GUILayout.Label("Chọn Thanh Đồng và nạp Giá Đồng Tứ Phủ để thử sát thương Mộc lên các enemy trong vùng.");
            GUILayout.Space(6f);
            if (GUILayout.Button("Reset prime state trên tất cả enemies")) ResetPrimeStates();
            GUILayout.Label(_message); GUILayout.EndScrollView(); GUILayout.EndArea();
        }
        private void CleanupIsolatedScene()
        {
            if (SceneManager.GetActiveScene().name != "SteamSlipTest") return;
            foreach (var spawner in FindObjectsByType<ProjectZombie.Features.Spawners.SpawnManager>(FindObjectsSortMode.None))
                if (spawner != null) spawner.enabled = false;
            foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
                if (enemy != null) Destroy(enemy.gameObject);
            foreach (var player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
                if (player != null) Destroy(player.gameObject);
        }
        private void EnsureProjectileSystem()
        {
            if (ProjectZombie.Features.Projectiles.Core.ProjectileSystem.Instance != null) return;
            var systemObject = new GameObject("Steam Slip Projectile System");
            SceneManager.MoveGameObjectToScene(systemObject, SceneManager.GetActiveScene());
            systemObject.AddComponent<ProjectZombie.Features.Projectiles.Core.ProjectileSystem>();
        }
        private void BuildTouchControls()
        {
            var canvasObject = new GameObject("Steam Slip Touch Controls", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 10;
            var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080, 1920); scaler.matchWidthOrHeight = 0.5f;
            if (EventSystem.current == null) { var events = new GameObject("Steam Slip Event System", typeof(EventSystem)); events.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>(); }
            var joystickRoot = CreateControlRect("Move Joystick", canvas.transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(170, 170), new Vector2(130, 130));
            var joystickImage = joystickRoot.gameObject.AddComponent<Image>(); joystickImage.color = new Color(0.1f, 0.15f, 0.2f, 0.58f);
            joystickImage.sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            var handle = CreateControlRect("Handle", joystickRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(70, 70), Vector2.zero);
            var handleImage = handle.gameObject.AddComponent<Image>(); handleImage.color = new Color(0.35f, 0.75f, 1f, 0.85f);
            handleImage.sprite = joystickImage.sprite;
            var joystick = joystickRoot.gameObject.AddComponent<DynamicVirtualJoystick>(); joystick.Configure(joystickRoot, handle);
            CreateButton(canvas.transform, "Attack", new Vector2(1, 0), new Vector2(1, 0), new Vector2(-100, 125), new Vector2(115, 115), new Color(0.75f, 0.2f, 0.16f), TriggerBasicAttack);
            CreateButton(canvas.transform, "Fire", new Vector2(1, 0), new Vector2(1, 0), new Vector2(-235, 90), new Vector2(88, 88), new Color(0.12f, 0.38f, 0.72f), TriggerWeaponSkill);
            CreateButton(canvas.transform, "Hero Skill", new Vector2(1, 0), new Vector2(1, 0), new Vector2(-350, 125), new Vector2(100, 100), new Color(0.35f, 0.2f, 0.62f), TriggerSignatureSkill);
        }
        private static RectTransform CreateControlRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 position)
        { var obj = new GameObject(name, typeof(RectTransform)); var rect = obj.GetComponent<RectTransform>(); rect.SetParent(parent, false); rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.pivot = new Vector2(0.5f,0.5f); rect.sizeDelta = size; rect.anchoredPosition = position; return rect; }
        private static void CreateButton(Transform parent,string label,Vector2 amin,Vector2 amax,Vector2 pos,Vector2 size,Color color,UnityEngine.Events.UnityAction action)
        { var rect=CreateControlRect(label+" Button",parent,amin,amax,size,pos); var image=rect.gameObject.AddComponent<Image>(); image.color=color; var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=image; button.onClick.AddListener(action); var textObject=new GameObject("Label",typeof(RectTransform),typeof(Text)); var text=textObject.GetComponent<Text>(); text.transform.SetParent(rect,false); text.rectTransform.anchorMin=Vector2.zero; text.rectTransform.anchorMax=Vector2.one; text.rectTransform.offsetMin=Vector2.zero; text.rectTransform.offsetMax=Vector2.zero; text.text=label; text.alignment=TextAnchor.MiddleCenter; text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize=22; text.color=Color.white; text.raycastTarget=false; }
        private void LoadChoices()
        {
            _characters.Clear();
            var database = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterDatabaseSO>(CharacterDatabasePath);
            if (database != null) foreach (var c in database.Characters) if (c != null && c.playerPrefab != null) _characters.Add(c);
            _weapons.Clear();
            foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:WeaponData", new[] { WeaponFolder }))
            { var w = UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponData>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid)); if (w != null && w.weaponPrefab != null) _weapons.Add(w); }
            _weapons.Sort((a,b) => string.CompareOrdinal(a.weaponId,b.weaponId));
            _skills.Clear();
            foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:SignatureSkillData", new[] { SkillFolder }))
            {
                var skill = UnityEditor.AssetDatabase.LoadAssetAtPath<SignatureSkillData>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
                if (skill != null) _skills.Add(skill);
            }
            _skills.Sort((a, b) => string.CompareOrdinal(a.SkillName, b.SkillName));
            _skillIndex = _skills.FindIndex(skill => skill is ThanhDongSkillData);
            if (_skillIndex < 0) _skillIndex = 0;
            _enemies.Clear();
            foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[] { EnemyFolder }))
            { var p = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid)); if (p != null && p.GetComponent<Enemy>() != null) _enemies.Add(new EnemyChoice { Label=p.name, Prefab=p }); }
            _enemies.Sort((a,b) => string.CompareOrdinal(a.Label,b.Label)); _enemyIndex=FindEnemyIndex("E_HOALYTINH");
            if (_enemyIndex == 0 || _enemies[_enemyIndex].Prefab.GetComponent<Enemy>().CurrentElement != ElementType.Hoa)
                _enemyIndex = _enemies.FindIndex(choice => choice.Prefab.GetComponent<Enemy>().CurrentElement == ElementType.Hoa);
        }
        private void DrawChoice(string title,int count,ref int index,System.Func<int,string> label)
        { if(count==0){GUILayout.Label(title+": không tìm thấy asset");return;} index=Mathf.Clamp(index,0,count-1); GUILayout.BeginHorizontal(); if(GUILayout.Button("◀",GUILayout.Width(36)))index=(index-1+count)%count; GUILayout.Label(label(index),GUI.skin.box,GUILayout.ExpandWidth(true)); if(GUILayout.Button("▶",GUILayout.Width(36)))index=(index+1)%count; GUILayout.EndHorizontal(); }
        private string WeaponLabel(int i)=>$"{_weapons[i].weaponId} — {_weapons[i].weaponName} ({_weapons[i].elementType})";
        private string EnemyLabel(int i)=>$"{_enemies[i].Label} — hệ {_enemies[i].Prefab.GetComponent<Enemy>().CurrentElement} — {_enemies[i].Prefab.GetComponent<Enemy>().ReactionClass}";
        private string WeaponDescription()=>_weapons.Count==0?"Không có WeaponData hợp lệ.":$"Nguồn element: {_weapons[_weaponIndex].elementType}. W009 = Thủy; W006/W008 = Hỏa. Hit thật cần được xác nhận khi chơi.";
        private string EnemyDescription()=>_enemies.Count==0?"Không có enemy prefab hợp lệ.":$"Reaction class: {_enemies[_enemyIndex].Prefab.GetComponent<Enemy>().ReactionClass}.";
        private void SpawnPlayer()
        {
            if(_characters.Count==0)return; if(_playerInstance!=null)Destroy(_playerInstance); var data=_characters[_characterIndex];
            _playerInstance=Instantiate(data.playerPrefab,playerSpawnPosition,Quaternion.identity); _playerInstance.name=data.characterName; _playerInstance.SetActive(true);
            if(!_playerInstance.TryGetComponent<PlayerStats>(out _))_playerInstance.AddComponent<PlayerStats>();
            if(!_playerInstance.TryGetComponent<PlayerInputReader>(out _))_playerInstance.AddComponent<PlayerInputReader>();
            if(!_playerInstance.TryGetComponent<CharacterCombat>(out var combat))combat=_playerInstance.AddComponent<CharacterCombat>(); combat.SetAttackConfig(data.basicAttackConfig);
            if(!_playerInstance.TryGetComponent<PlayerLogic>(out _))_playerInstance.AddComponent<PlayerLogic>();
            if(!_playerInstance.TryGetComponent<WeaponManager>(out var wm))wm=_playerInstance.AddComponent<WeaponManager>(); wm.SetSkipAutomaticLoadout(true); wm.enabled=true;
            var skillManager = _playerInstance.GetComponent<SignatureSkillManager>();
            _message=$"Spawned {data.characterName}; skill = {(skillManager != null && skillManager.SkillData != null ? skillManager.SkillData.SkillName : "chưa được nạp")}; basic attack element = {(data.basicAttackConfig!=null?data.basicAttackConfig.element.ToString():"None")}.";
        }
        private string ActiveSkillLabel()
        {
            if (_playerInstance == null) return "chưa spawn nhân vật";
            var manager = _playerInstance.GetComponent<SignatureSkillManager>();
            if (manager == null) return "prefab không có SignatureSkillManager";
            return manager.SkillData != null ? manager.SkillData.SkillName : "chưa được nạp";
        }
        private void LoadSelectedSignatureSkill()
        {
            if (_playerInstance == null) { _message = "Spawn Player trước."; return; }
            if (_skills.Count == 0) { _message = "Không tìm thấy SignatureSkillData trong Assets/_Data/Skills."; return; }
            var manager = _playerInstance.GetComponent<SignatureSkillManager>();
            if (manager == null) manager = _playerInstance.AddComponent<SignatureSkillManager>();
            manager.InitializeSkill(_skills[_skillIndex]);
            _message = $"Đã nạp kỹ năng {_skills[_skillIndex].SkillName} cho {_playerInstance.name}.";
        }
        private void TriggerSignatureSkill()
        {
            if (_playerInstance == null) { _message = "Spawn Player trước."; return; }
            var manager = _playerInstance.GetComponent<SignatureSkillManager>();
            if (manager == null) { _message = "Nhân vật chưa có SignatureSkillManager. Hãy nạp kỹ năng."; return; }
            if (manager.ActiveSkill == null) { _message = "Chưa nạp kỹ năng. Chọn dữ liệu ở panel rồi bấm Nạp kỹ năng."; return; }
            _message = manager.TryExecuteSkill()
                ? $"Đã thi triển {manager.SkillData.SkillName}."
                : $"Chưa thi triển được {manager.SkillData.SkillName}: đang hồi chiêu hoặc thiếu điều kiện.";
        }
        private void EquipWeapon()
        { if(_playerInstance==null){_message="Spawn Player trước.";return;} if(_weapons.Count==0)return; var wm=_playerInstance.GetComponent<WeaponManager>(); if(wm==null){_message="Player thiếu WeaponManager.";return;} var current=wm.GetWeaponById(_weapons[_weaponIndex].weaponId); if(current!=null){_message="Vũ khí này đã được equip.";return;} var active=wm.ActiveWeapons; for(int i=active.Count-1;i>=0;i--)wm.RemoveWeapon(active[i]); wm.EquipWeaponFromData(_weapons[_weaponIndex]); _message=$"Equipped {_weapons[_weaponIndex].weaponName} ({_weapons[_weaponIndex].elementType})."; }
        private void TriggerBasicAttack()
        { if(_playerInstance==null){_message="Spawn Player trước.";return;} var combat=_playerInstance.GetComponent<CharacterCombat>(); Vector2 dir=_enemyInstance!=null?(Vector2)(_enemyInstance.transform.position-_playerInstance.transform.position).normalized:Vector2.right; _message=combat!=null&&combat.TriggerAttack(dir)?"CharacterCombat.TriggerAttack đã được gọi.":"Attack chưa sẵn sàng (cooldown/config)."; }
        private void TriggerWeaponSkill()
        {
            if (_playerInstance == null) { _message = "Spawn Player trước."; return; }
            if (_enemyInstance == null) { _message = "Spawn enemy trước để bắn thử."; return; }
            var weapon = _playerInstance.GetComponentInChildren<WeaponBase>();
            if (weapon == null) { _message = "Equip vũ khí trước."; return; }
            if (weapon is Weapon_LightningOrb lightningOrb)
            {
                if (lightningOrb.TryFireAt(_enemyInstance.transform)) _message = "Đã spawn projectile W009 hướng vào enemy đang chọn.";
                else _message = "W009 chưa tạo được projectile; kiểm tra Console, ProjectileData và ProjectileSystem.";
                return;
            }
            Vector2 direction = (_enemyInstance.transform.position - _playerInstance.transform.position).normalized;
            _message = weapon.TriggerActiveRelicSkill(direction)
                ? "Đã gọi skill API của relic."
                : "Không kích hoạt (vũ khí passive/cooldown/thiếu stats).";
        }
        private void SpawnEnemy()
        { if(_enemies.Count==0)return; if(_enemyInstance!=null)Destroy(_enemyInstance.gameObject); var c=_enemies[_enemyIndex]; _enemyInstance=Instantiate(c.Prefab,enemySpawnPosition,Quaternion.identity).GetComponent<Enemy>(); _message=$"Spawned {c.Label}: element={_enemyInstance.CurrentElement}, class={_enemyInstance.ReactionClass}, config={(_enemyInstance.Config!=null?_enemyInstance.Config.name:"None")}."; Debug.Log($"[SteamSlipTest] Spawned {_enemyInstance.name}; element={_enemyInstance.CurrentElement}; config={(_enemyInstance.Config!=null?_enemyInstance.Config.name:"None")}", _enemyInstance); }
        private void SpawnSecondaryEnemy()
        { if(_enemyInstance==null){_message="Spawn enemy chính trước.";return;} var c=_enemies[_enemyIndex]; Instantiate(c.Prefab,_enemyInstance.transform.position+new Vector3(.55f,.35f,0),Quaternion.identity); _message="Spawned thêm enemy cùng loại cạnh enemy chính."; }
        private void DespawnEnemies()
        { foreach(var e in FindObjectsByType<Enemy>(FindObjectsSortMode.None))if(e!=null)Destroy(e.gameObject); _enemyInstance=null; _message="Đã despawn enemies."; }
        private void ResetPrimeStates()
        { foreach(var e in FindObjectsByType<Enemy>(FindObjectsSortMode.None))e.ElementReactionController?.ResetState(); _message="Đã reset prime state trên tất cả enemies."; }
        private int FindEnemyIndex(string n){for(int i=0;i<_enemies.Count;i++)if(_enemies[i].Label==n)return i;return 0;}
#endif
    }
}








