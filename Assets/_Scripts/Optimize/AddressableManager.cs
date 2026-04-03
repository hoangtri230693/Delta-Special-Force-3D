using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableManager : MonoBehaviour
{
    public static AddressableManager instance { get; private set; }

    private readonly Dictionary<object, AsyncOperationHandle> _assetHandles = new Dictionary<object, AsyncOperationHandle>();
    private readonly Dictionary<object, CancellationTokenSource> _ctsDict = new Dictionary<object, CancellationTokenSource>();

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

    //public async UniTask<T> LoadAssetAsync<T>(object key) where T : Object
    //{
    //    // 1. Kiểm tra nếu tài nguyên đã được load trước đó (Caching)
    //    if (_assetHandles.TryGetValue(key, out var handle))
    //        return await handle.Convert<T>().ToUniTask();

    //    // 2. Nếu chưa có, tiến hành load mới từ Addressables
    //    var newHandle = Addressables.LoadAssetAsync<T>(key);
    //    _assetHandles[key] = newHandle;
    //    return await newHandle.ToUniTask();
    //}

    public async UniTask<T> LoadAssetAsync<T>(object key) where T : Object
    {
        if (key == null) return null;

        CancelTask(key);

        var cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        _ctsDict[key] = cts;

        try
        {
            AsyncOperationHandle<T> handle;
            if (_assetHandles.TryGetValue(key, out var existingHandle))
            {
                handle = existingHandle.Convert<T>();
            }
            else
            {
                handle = Addressables.LoadAssetAsync<T>(key);
                _assetHandles[key] = handle;
            }
            return await handle.WithCancellation(cts.Token);
        }
        catch (System.OperationCanceledException)
        {
            return null;
        }
        finally
        {
            if (_ctsDict.TryGetValue(key, out var c) && c == cts)
            {
                _ctsDict.Remove(key);
                cts.Dispose();
            }
        }
    }

    public async UniTask<GameObject> InstantiatePrefabAsync(object key, Transform parent = null)
    {
        if (key == null) return null;

        try
        {
            return await Addressables.InstantiateAsync(key, parent).ToUniTask();
        }
        catch
        {
            return null;
        }
    }

    public void CancelTask(object key)
    {
        if (_ctsDict.TryGetValue(key, out var cts))
        {
            cts.Cancel();
        }
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

    private void OnDestroy()
    {
        // Quan trọng: Giải phóng toàn bộ tài nguyên đã load vào RAM
        foreach (var handle in _assetHandles.Values)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
        _assetHandles.Clear();
    }
}