using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using DeltaSpecialForce3D.Enums;


public class WeaponThrowController : MonoBehaviour
{
    [SerializeField] private WeaponController _weaponController;
    [SerializeField] private CinemachineImpulseSource _shakeCamera;
    [SerializeField] private WeaponAudio _weaponAudio;

    public int _currentAmmo;
    public int _currentReverse;
    private readonly Collider[] _collider = new Collider[128];

    private void OnEnable() => RefreshUI();

    private void Start()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (_weaponController._botAIController == null)
        {
            if (UIGameManager_TeamDeathmatch.instance != null)
                UIGameManager_TeamDeathmatch.instance.UpdateUIWeaponAmmo(_currentAmmo, _currentReverse);
            if (UIGameManager_ZombieSurvival.instance != null)
                UIGameManager_ZombieSurvival.instance.UpdateUIWeaponAmmo(_currentAmmo, _currentReverse);
        }
    }

    public void InitializeThrow()
    {
        _currentAmmo = _weaponController.WeaponStats.ammoPerMag;
        _currentReverse = _weaponController.WeaponStats.ammoReverse;
    }

    public void AssignAnimationEvents(PlayerAnimationEvents playerAnimationEvents)
    {
        if (_weaponController.WeaponStats.itemType == ItemType.ThrowItem)
        {
            playerAnimationEvents._throwController = this;
        }
    }

    #region Action Throw
    public void ThrowGrenade()
    {
        _weaponAudio.PlayWeaponSound(WeaponSoundType.Throw);
        HandleAmmo();

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 throwDirection = (cameraForward + Camera.main.transform.up * 0.3f).normalized;

        float safeDistance = 0.5f;
        transform.position += throwDirection * safeDistance;

        transform.SetParent(null);

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(throwDirection * _weaponController.WeaponStats.throwForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);

        StartCoroutine(ExplosionGrenadeAfter());
    }

    private IEnumerator ExplosionGrenadeAfter()
    {
        yield return new WaitForSeconds(3f);
        ToggleMeshRenderer(false);
        HandleHitTarget();
        GenerateExplosion();
        HandleAudio();
        HandleShakeCamera();
        Destroy(gameObject, 2f);
    }

    private void ToggleMeshRenderer(bool isEnabled)
    {
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in meshRenderers)
        {
            renderer.enabled = isEnabled;
        }
    }

    private void GenerateExplosion()
    {
        GameObject explosionPrefab = _weaponController.WeaponStats.explosionGrenade.gameObject;

        if (ObjectPoolService.Instance == null || explosionPrefab == null) return;

        GameObject pooledExplosion = ObjectPoolService.Instance.GetPooledObject(explosionPrefab);

        pooledExplosion.transform.SetPositionAndRotation(transform.position, Quaternion.identity);

        if (pooledExplosion.TryGetComponent(out ParticleSystem ps))
        {
            ps.Play(true);
        }
    }

    private void HandleHitTarget()
    {
        Vector3 explosionPosition = transform.position;
        float radius = _weaponController.WeaponStats.attackRadius;
        float fullDamageRadius = 10f;
        float baseDamage = _weaponController.WeaponStats.damage;
        float explosionForce = _weaponController.WeaponStats.explosionForce;
        LayerMask targetMask = _weaponController.WeaponStats.targetMask;
        LayerMask raycastMask = _weaponController.WeaponStats.raycastMask;

        int numColliders = Physics.OverlapSphereNonAlloc(explosionPosition, radius, _collider, targetMask);
        Debug.Log("Number of colliders hit: " + numColliders);

        // Dùng HashSet để tránh sát thương trùng lặp trên cùng 1 object cha
        HashSet<GameObject> hitObjects = new HashSet<GameObject>();

        // Dùng vòng lặp for với numColliders
        for (int i = 0; i < numColliders; i++)
        {
            Collider hitCollider = _collider[i];
            GameObject root = hitCollider.transform.root.gameObject;
            if (hitObjects.Contains(root)) continue;
            hitObjects.Add(root);

            PlayerHealth playerHealth = root.GetComponent<PlayerHealth>();
            ZombieHealth zombieHealth = root.GetComponent<ZombieHealth>();

            if (playerHealth == null && zombieHealth == null) continue;

            float distance = Vector3.Distance(explosionPosition, hitCollider.bounds.center);
            float fallOffMultiplier = 1f;

            if (distance <= fullDamageRadius)
            {
                fallOffMultiplier = 1f;
            }
            else
            {
                float distBeyondFullDamage = distance - fullDamageRadius;
                float fallOffRange = radius - fullDamageRadius;
                fallOffMultiplier = Mathf.Clamp01(1 - (distBeyondFullDamage / fallOffRange));
            }

            Vector3 directionToTarget = (hitCollider.bounds.center - explosionPosition).normalized;
            if (Physics.Raycast(explosionPosition, directionToTarget, out RaycastHit hit, distance, raycastMask))
            {
                if (hit.collider != hitCollider) continue;
            }

            float finalDamage = Mathf.RoundToInt(baseDamage * fallOffMultiplier);

            // -------- Apply damage --------

            if (playerHealth != null && playerHealth._currentHealth > 0)
            {
                playerHealth.UpdateHealth(finalDamage, _weaponController.WeaponStats.itemType);

                if (playerHealth._currentHealth <= 0)
                {
                    if (playerHealth.TryGetComponent<PlayerController>(out var targetController))
                    {
                        var team = _weaponController._playerController.GetComponent<PlayerTeam>().Team;
                        var targetTeam = targetController.GetComponent<PlayerTeam>().Team;

                        if (targetController != _weaponController._playerController && team != targetTeam)
                        {
                            _weaponController._playerController.IncrementKillCount();
                        }
                    }
                }
            }

            if (zombieHealth != null && zombieHealth._currentHealth > 0)
            {
                zombieHealth.UpdateHealth(finalDamage);
                //Debug.Log("Zombie Health: " + zombieHealth._currentHealth);

                if (zombieHealth._currentHealth <= 0)
                {
                    Rigidbody[] rbs = zombieHealth.GetComponentsInChildren<Rigidbody>();
                    foreach (Rigidbody rb in rbs)
                    {
                        rb.AddExplosionForce(explosionForce, explosionPosition, radius, 1f, ForceMode.Impulse);
                    }

                    _weaponController._playerController.IncrementKillCount();
                }
            }
        }

        System.Array.Clear(_collider, 0, numColliders);
    }

    private void HandleAudio()
    {
        _weaponAudio.PlayWeaponSound(WeaponSoundType.Explosion);
    }

    private void HandleShakeCamera()
    {
        float distance = Vector3.Distance(Camera.main.transform.position, transform.position);
        if (distance > _weaponController.WeaponStats.attackRadius) return;

        float falloffMultiplier = 1 - (distance / _weaponController.WeaponStats.attackRadius);
        falloffMultiplier = Mathf.Clamp01(falloffMultiplier);

        float finalShakeIntensity = _weaponController.WeaponStats.shakeIntensity * falloffMultiplier;
        Vector3 impulseForce = Vector3.one * finalShakeIntensity;
        if (_shakeCamera != null && _weaponController._botAIController == null)
        {
            _shakeCamera.GenerateImpulse(impulseForce);
        }       
    }

    private void HandleAmmo()
    {
        _currentAmmo = Mathf.Max(0, _currentAmmo - 1);
        RefreshUI();
    }
    #endregion
}
