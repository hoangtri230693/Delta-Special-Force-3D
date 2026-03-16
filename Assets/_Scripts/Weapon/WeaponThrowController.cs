using System.Collections;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

public class WeaponThrowController : MonoBehaviour
{
    [SerializeField] private WeaponController _weaponController;
    [SerializeField] private CinemachineImpulseSource _shakeCamera;
    [SerializeField] private WeaponAudio _weaponAudio;

    private Camera _playerCamera;

    public int _currentAmmo;
    public int _currentReverse;

    private void OnEnable() => RefreshUI();

    private void Start()
    {
        _playerCamera = Camera.main;

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

        Vector3 cameraForward = _playerCamera.transform.forward;
        Vector3 throwDirection = (cameraForward + _playerCamera.transform.up * 0.3f).normalized;

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
        LayerMask raycastMask = _weaponController.WeaponStats.targetMask;

        Collider[] colliders = Physics.OverlapSphere(explosionPosition, radius);

        HashSet<GameObject> hitObjects = new HashSet<GameObject>();

        foreach (Collider hitCollider in colliders)
        {
            GameObject root = hitCollider.transform.root.gameObject;
            if (hitObjects.Contains(root)) continue;

            PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();
            ZombieHealth zombieHealth = hitCollider.GetComponent<ZombieHealth>();

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

            if (playerHealth != null)
            {
                float finalDamage = Mathf.RoundToInt(baseDamage * fallOffMultiplier);
                playerHealth.UpdateHealth(finalDamage, _weaponController.WeaponStats.itemType);

                if (playerHealth._currentHealth <= 0)
                    _weaponController._playerController.IncrementKillCount();

                hitObjects.Add(root);
            }
            
            if (zombieHealth != null)
            {
                float finalDamage = Mathf.RoundToInt(baseDamage * fallOffMultiplier);
                zombieHealth.UpdateHealth(finalDamage);

                if (zombieHealth._currentHealth <= 0)
                {
                    Rigidbody[] childrenRbs = zombieHealth.GetComponentsInChildren<Rigidbody>();
                    foreach (Rigidbody rb in childrenRbs)
                    {
                        rb.AddExplosionForce(explosionForce, explosionPosition, radius, 1f, ForceMode.Impulse);
                    }

                    _weaponController._playerController.IncrementKillCount();

                    hitObjects.Add(root);
                }
            }
        }
    }

    private void HandleAudio()
    {
        _weaponAudio.PlayWeaponSound(WeaponSoundType.Explosion);
    }

    private void HandleShakeCamera()
    {
        float distance = Vector3.Distance(_weaponController._player.transform.position, transform.position);
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
        _currentAmmo -= 1;
        Mathf.Clamp(_currentAmmo, 0, _weaponController.WeaponStats.ammoPerMag);
        RefreshUI();
    }
    #endregion
}
