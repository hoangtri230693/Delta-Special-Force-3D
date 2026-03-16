using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableManager : MonoBehaviour
{
    public static AddressableManager instance { get; private set; }

    // Lưu trữ handle để giải phóng chính xác
    private readonly Dictionary<object, AsyncOperationHandle> _assetHandles = new Dictionary<object, AsyncOperationHandle>();

    private void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    // Tải Asset và lưu vào bộ nhớ cache của Addressables
    public async Task<T> LoadAssetAsync<T>(object key) where T : Object
    {
        if (key == null) return null;

        if (_assetHandles.TryGetValue(key, out var existingHandle))
        {
            if (existingHandle.IsDone) return existingHandle.Result as T;
            await existingHandle.Task;
            return existingHandle.Result as T;
        }

        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
        _assetHandles[key] = handle;

        try
        {
            return await handle.Task;
        }
        catch
        {
            _assetHandles.Remove(key);
            return null;
        }
    }

    // Tạo Instance từ key (nhanh hơn nếu asset đã được Load trước đó)
    public async Task<GameObject> InstantiatePrefabAsync(object key, Transform parent = null)
    {
        if (key == null) return null;
        return await Addressables.InstantiateAsync(key, parent).Task;
    }

    public void ReleaseInstance(GameObject instance)
    {
        if (instance != null) Addressables.ReleaseInstance(instance);
    }

    public void ReleaseAsset(object key)
    {
        if (_assetHandles.TryGetValue(key, out var handle))
        {
            Addressables.Release(handle);
            _assetHandles.Remove(key);
        }
    }
}