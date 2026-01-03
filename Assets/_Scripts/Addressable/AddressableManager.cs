using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public class AddressableManager : MonoBehaviour
{
    public static AddressableManager instance { get; private set; }

    private readonly Dictionary<object, Object> _loadedAssets = new Dictionary<object, Object>();
    private readonly Dictionary<object, AsyncOperationHandle> _handles = new Dictionary<object, AsyncOperationHandle>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- Phương Thức Tải Tài Nguyên (Loading Methods) ---
    public async Task<T> LoadAssetAsync<T>(object key) where T : Object
    {
        if (key == null) return null;

        // 1. Kiểm tra cache
        if (_loadedAssets.TryGetValue(key, out Object cachedAsset))
        {
            return cachedAsset as T;
        }

        // 2. Tải mới
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
        _handles[key] = handle;

        try
        {
            T result = await handle.Task;

            if (result != null)
            {
                _loadedAssets[key] = result;
                return result;
            }
            else
            {
                _handles.Remove(key);
                return null;
            }
        }
        catch
        {
            _handles.Remove(key);
            return null;
        }
    }

    // --- Phương Thức Khởi Tạo (Instantiation Method) ---
    public async Task<GameObject> InstantiatePrefabAsync(object key, Transform parent = null)
    {
        if (key == null) return null;

        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(key, parent);

        try
        {
            GameObject instance = await handle.Task;

            if (instance != null) return instance;
            else return null;
        }
        catch
        {
            return null;
        }
    }

    // --- Phương Thức Giải Phóng (Release Methods) ---
    public void ReleaseAsset(object key)
    {
        if (_handles.ContainsKey(key))
        {
            Addressables.Release(_handles[key]);
            _handles.Remove(key);
            _loadedAssets.Remove(key);
        }
    }

    public void ReleaseInstance(GameObject instance)
    {
        if (instance != null)
        {
            Addressables.ReleaseInstance(instance);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            foreach (var kvp in _handles)
            {
                if (kvp.Value.IsValid())
                {
                    Addressables.Release(kvp.Value);
                }
            }

            _handles.Clear();
            _loadedAssets.Clear();
        }
    }
}
