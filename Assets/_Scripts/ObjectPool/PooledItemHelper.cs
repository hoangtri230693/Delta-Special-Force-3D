using UnityEngine;
using UnityEngine.Pool;
using System.Collections;

public class PooledItemHelper : MonoBehaviour
{
    private ObjectPoolService _poolService;
    private ParticleSystem _particleSystem;
    private Rigidbody _rigidbody;

    public IObjectPool<GameObject> Pool { get; private set; }

    public void Initialize(IObjectPool<GameObject> pool, ObjectPoolService service)
    {
        Pool = pool;
        _poolService = service;
    }

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if (_particleSystem != null)
        {
            float life = _particleSystem.main.duration + _particleSystem.main.startLifetime.constantMax;
            StartCoroutine(AutoReleaseAfter(life));
        }
        else if (_rigidbody != null)
        {
            StartCoroutine(AutoReleaseAfter(3f));
        }
    }

    private IEnumerator AutoReleaseAfter(float time)
    {
        yield return new WaitForSeconds(time);

        if (_poolService != null)
        {
            _poolService.ReleasePooledObject(gameObject);
        }
    }
}
