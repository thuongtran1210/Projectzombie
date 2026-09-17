using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Factory chuyên trách khởi tạo và quản lý vòng đời bộ nhớ cache của các màn hình Meta UI (Lazy Loading).
    /// Giúp phân tách triệt để việc Instantiate đối tượng ra khỏi Navigation Stack của MetaUIManager.
    /// </summary>
    public class UIScreenFactory
    {
        private readonly UIRegistrySO _uiRegistry;
        private readonly Transform _container;
        private readonly Dictionary<MetaScreenType, BaseMetaScreenView> _instantiatedScreens = new Dictionary<MetaScreenType, BaseMetaScreenView>();

        public UIScreenFactory(UIRegistrySO uiRegistry, Transform container)
        {
            _uiRegistry = uiRegistry;
            _container = container;
        }

        /// <summary>
        /// Đăng ký trước một màn hình đã có sẵn trong Scene (nếu có).
        /// </summary>
        public void RegisterExistingScreen(BaseMetaScreenView screen)
        {
            if (screen == null) return;
            _instantiatedScreens[screen.ScreenType] = screen;
        }

        /// <summary>
        /// Lấy màn hình từ cache, nếu chưa có sẽ nạp Prefab từ UIRegistry và instantiate vào Scene (Lazy Loading).
        /// </summary>
        public BaseMetaScreenView GetOrCreateScreen(MetaScreenType screenType)
        {
            // 1. Kiểm tra cache đối tượng đã sinh trước đó
            if (_instantiatedScreens.TryGetValue(screenType, out var existingScreen) && existingScreen != null)
            {
                return existingScreen;
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();

            // 2. Tra cứu cấu hình trong UIRegistrySO (kèm tự động fallback Resources nếu null)
            var registry = _uiRegistry ?? Resources.Load<UIRegistrySO>("UI/UIRegistry");
            if (registry == null)
            {
                Debug.LogError($"[UIScreenFactory] Không thể tạo màn hình '{screenType}' vì UIRegistrySO chưa được gán và không tìm thấy tại 'Resources/UI/UIRegistry'!");
                return null;
            }

            if (!registry.TryGetScreenEntry(screenType, out var entry))
            {
                Debug.LogError($"[UIScreenFactory] Không tìm thấy cấu hình cho '{screenType}' trong UIRegistrySO!");
                return null;
            }

            // 3. Tiến hành Instantiate đối tượng làm con của Root Container (Canvas)
            BaseMetaScreenView newScreen = null;
            if (entry.screenPrefab != null)
            {
                newScreen = UnityEngine.Object.Instantiate(entry.screenPrefab, _container);
            }
#if UNITY_EDITOR
            else if (entry.screenPrefabRef != null && entry.screenPrefabRef.editorAsset != null)
            {
                var go = UnityEngine.Object.Instantiate(entry.screenPrefabRef.editorAsset as GameObject, _container);
                newScreen = go != null ? (go.GetComponent<BaseMetaScreenView>() ?? go.GetComponentInChildren<BaseMetaScreenView>(true)) : null;
            }
#else
            else if (entry.screenPrefabRef != null && entry.screenPrefabRef.Asset != null)
            {
                var go = UnityEngine.Object.Instantiate(entry.screenPrefabRef.Asset as GameObject, _container);
                newScreen = go != null ? (go.GetComponent<BaseMetaScreenView>() ?? go.GetComponentInChildren<BaseMetaScreenView>(true)) : null;
            }
#endif

            // Fallback 1: Nạp từ Resources/UI
            if (newScreen == null)
            {
                string[] resourceNames = new[]
                {
                    $"UI/{entry.screenHierarchyName}",
                    $"UI/LobbyModalUI",
                    $"UI/{screenType}ModalUI",
                    $"UI/{screenType}UI",
                    $"UI/{screenType}Panel",
                    $"UI/{screenType}_Screen",
                    $"UI/{screenType}"
                };

                foreach (var resName in resourceNames)
                {
                    var prefabGo = Resources.Load<GameObject>(resName);
                    if (prefabGo != null)
                    {
                        var go = UnityEngine.Object.Instantiate(prefabGo, _container);
                        if (go != null)
                        {
                            newScreen = go.GetComponent<BaseMetaScreenView>() ?? go.GetComponentInChildren<BaseMetaScreenView>(true);
                            if (newScreen != null) break;
                        }
                    }
                }
            }

#if UNITY_EDITOR
            // Fallback 2 (Editor Assets): Nạp trực tiếp từ AssetDatabase
            if (newScreen == null)
            {
                string[] editorPaths = new[]
                {
                    $"Assets/Resources/UI/LobbyModalUI.prefab",
                    $"Assets/_Prefabs/UI/LobbyModalUI.prefab",
                    $"Assets/Resources/UI/{entry.screenHierarchyName}.prefab",
                    $"Assets/_Prefabs/UI/{entry.screenHierarchyName}.prefab",
                    $"Assets/Resources/UI/{screenType}UI.prefab",
                    $"Assets/_Prefabs/UI/{screenType}UI.prefab"
                };

                foreach (var path in editorPaths)
                {
                    var prefabGo = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefabGo != null)
                    {
                        var go = UnityEngine.Object.Instantiate(prefabGo, _container);
                        if (go != null)
                        {
                            newScreen = go.GetComponent<BaseMetaScreenView>() ?? go.GetComponentInChildren<BaseMetaScreenView>(true);
                            if (newScreen != null) break;
                        }
                    }
                }
            }
#endif

            if (newScreen == null)
            {
                Debug.LogError($"[UIScreenFactory] Không thể khởi tạo Prefab cho '{screenType}' từ UIRegistrySO và Resources fallback!");
                return null;
            }

            string finalName = !string.IsNullOrEmpty(entry.screenHierarchyName) 
                ? entry.screenHierarchyName 
                : $"Screen_{screenType}";
            newScreen.name = finalName;

            // Đảm bảo ban đầu ở trạng thái ẩn an toàn
            newScreen.gameObject.SetActive(false);

            // Lưu vào cache tái sử dụng O(1)
            _instantiatedScreens[screenType] = newScreen;

            sw.Stop();
            Debug.Log($"<color=#00FF88>[UIScreenFactory]</color> Đã nạp thành công màn hình '{finalName}' (Lazy Loaded) trong: {sw.ElapsedMilliseconds} ms.");
            return newScreen;
        }

        /// <summary>
        /// Nạp và khởi tạo màn hình bất đồng bộ chuẩn Addressables.
        /// </summary>
        public async System.Threading.Tasks.Task<BaseMetaScreenView> GetOrCreateScreenAsync(MetaScreenType screenType)
        {
            if (_instantiatedScreens.TryGetValue(screenType, out var existingScreen) && existingScreen != null)
            {
                return existingScreen;
            }

            var registry = _uiRegistry ?? Resources.Load<UIRegistrySO>("UI/UIRegistry");
            if (registry != null && registry.TryGetScreenEntry(screenType, out var entry))
            {
                if (entry.screenPrefabRef != null && entry.screenPrefabRef.RuntimeKeyIsValid())
                {
                    var handle = entry.screenPrefabRef.InstantiateAsync(_container);
                    var go = await handle.Task;
                    if (go != null && go.TryGetComponent<BaseMetaScreenView>(out var screenView))
                    {
                        string finalName = !string.IsNullOrEmpty(entry.screenHierarchyName) ? entry.screenHierarchyName : $"Screen_{screenType}";
                        screenView.name = finalName;
                        screenView.gameObject.SetActive(false);
                        _instantiatedScreens[screenType] = screenView;
                        return screenView;
                    }
                }
            }

            return GetOrCreateScreen(screenType);
        }

        /// <summary>
        /// Kiểm tra xem màn hình đã được instantiate vào bộ nhớ chưa.
        /// </summary>
        public bool IsScreenLoaded(MetaScreenType screenType)
        {
            return _instantiatedScreens.ContainsKey(screenType) && _instantiatedScreens[screenType] != null;
        }

        /// <summary>
        /// Lấy toàn bộ danh sách các màn hình đang hoạt động trong Scene.
        /// </summary>
        public IEnumerable<BaseMetaScreenView> GetAllLoadedScreens()
        {
            return _instantiatedScreens.Values;
        }
    }
}
