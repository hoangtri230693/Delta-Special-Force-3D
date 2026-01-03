using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolService : MonoBehaviour
{
    public static ObjectPoolService Instance { get; private set; }

    private Dictionary<GameObject, IObjectPool<GameObject>> _pools = new Dictionary<GameObject, IObjectPool<GameObject>>();

    [Header("Pool Settings")]
    [SerializeField] private int _defaultCapacity = 45;
    [SerializeField] private int _maxSize = 60;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- Core Pool Methods ---
    public GameObject GetPooledObject(GameObject prefab)
    {
        if (prefab == null) return null;

        if (!_pools.ContainsKey(prefab))
        {
            CreatePoolForPrefab(prefab);
        }

        //Debug.Log($"Retrieved pooled object for prefab: {prefab.name}");
        return _pools[prefab].Get(); 
    }

    public void ReleasePooledObject(GameObject obj)
    {
        if (obj.TryGetComponent(out PooledItemHelper helper) && helper.Pool != null)
        {
            helper.Pool.Release(obj);
        }
        else
        {
            Destroy(obj);
        }
        //Debug.Log($"Released pooled object: {obj.name}");
    }

    // --- Private Setup Methods ---
    private void CreatePoolForPrefab(GameObject prefab)
    {
        IObjectPool<GameObject> newPool = null;

        var factoryFunction = new System.Func<GameObject>(() => CreatePooledItem(prefab, newPool));

        newPool = new ObjectPool<GameObject>(
            factoryFunction, 
            OnTakeFromPool,
            OnReturnedToPool,
            OnDestroyPoolObject,
            true,
            _defaultCapacity,
            _maxSize
            );

        _pools.Add(prefab, newPool);
        //Debug.Log($"Created new pool for prefab: {prefab.name}");
    }
    private GameObject CreatePooledItem(GameObject prefab, IObjectPool<GameObject> pool)
    {
        GameObject item = Instantiate(prefab);
        item.name = prefab.name + " (Pooled)";
        item.transform.SetParent(this.transform);

        PooledItemHelper helper = item.AddComponent<PooledItemHelper>();
        helper.Initialize(pool, this);

        item.SetActive(false);
        return item;
    }
    private void OnTakeFromPool(GameObject item)
    {
        item.SetActive(true);

        if (item.TryGetComponent(out ParticleSystem ps))
        {
            ps.Clear(true);
            ps.Play(true);
        }

        if (item.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
    private void OnReturnedToPool(GameObject item)
    {
        item.SetActive(false);
    }
    private void OnDestroyPoolObject(GameObject item)
    {
        Destroy(item);
    }
}
