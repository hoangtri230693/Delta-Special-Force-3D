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

        // Tương tự, dùng UniTask để có thể Cancel nếu người chơi click quá nhanh
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
        // 1. Hủy tất cả các tiến trình đang chạy để tránh lỗi truy cập bộ nhớ
        foreach (var cts in _ctsDict.Values)
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }
        }
        _ctsDict.Clear();

        // 2. Quan trọng: Giải phóng toàn bộ tài nguyên đã load vào RAM
        foreach (var handle in _assetHandles.Values)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
        _assetHandles.Clear();

        // 3. Xóa instance singleton
        if (instance == this) instance = null;
    }
}