using System.Collections;
using Unity.Behavior;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

public class WeaponThrowController : MonoBehaviour
{
    [SerializeField] private WeaponManager _weaponManager;
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
        if (_weaponManager._playerLocal != null)
        {
            if (UIGameManager_TeamDeathmatch.instance != null)
                UIGameManager_TeamDeathmatch.instance.UpdateUIWeaponAmmo(_currentAmmo, _currentReverse);
            if (UIGameManager_ZombieSurvival.instance != null)
                UIGameManager_ZombieSurvival.instance.UpdateUIWeaponAmmo(_currentAmmo, _currentReverse);
        }
    }

    public void InitializeThrow()
    {
        _currentAmmo = _weaponManager._weaponStats.ammoPerMag;
        _currentReverse = _weaponManager._weaponStats.ammoReverse;
    }

    public void AssignAnimationEvents(PlayerAnimationEvents playerAnimationEvents)
    {
        if (_weaponManager._weaponStats.itemType == ItemType.ThrowItem)
        {
            playerAnimationEvents._throwController = this;
        }
    }

    #region Action Throw
    public void ThrowGrenade()
    {
        _weaponAudio.PlayAudioThrow();
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

        rb.AddForce(throwDirection * _weaponManager._weaponStats.throwForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);

        StartCoroutine(ExplosionGrenadeAfter());
    }

    private IEnumerator ExplosionGrenadeAfter()
    {
        yield return new WaitForSeconds(3f);
        ToggleMeshRenderer(false);
        HandleHitTarget();
        GenerateExplosionGrenade();
        _weaponAudio.PlayAudioExplosion();
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

    private void HandleHitTarget()
    {
        Vector3 explosionPosition = transform.position;
        float radius = _weaponManager._weaponStats.attackRadius;
        float fullDamageRadius = 10f;
        float baseDamage = _weaponManager._weaponStats.damage;
        float explosionForce = _weaponManager._weaponStats.explosionForce;
        LayerMask raycastMask = _weaponManager._weaponStats.targetMask;

        Collider[] colliders = Physics.OverlapSphere(explosionPosition, radius);

        foreach (Collider hitCollider in colliders)
        {
            PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();
            if (playerHealth == null) continue;

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

            if (playerHealth != null && !playerHealth._isDead)
            {
                float finalDamage = Mathf.RoundToInt(baseDamage * fallOffMultiplier);
                playerHealth.UpdateHealth(finalDamage, _weaponManager._weaponStats.itemType);

                if (playerHealth._currentHealth <= 0)
                {
                    playerHealth._isDead = true;

                    var characterController = playerHealth.GetComponent<CharacterController>();
                    if (characterController != null) characterController.enabled = false;

                    var navAgent = playerHealth.GetComponent<NavMeshAgent>();
                    if (navAgent != null) navAgent.enabled = false;

                    var behaviorAgent = playerHealth.GetComponent<BehaviorGraphAgent>();
                    if (behaviorAgent != null) behaviorAgent.enabled = false;

                    var switcher = playerHealth.GetComponent<RagdollSwitcher>();
                    if (switcher != null) switcher.EnableRagdolls();

                    Rigidbody[] childrenRbs = playerHealth.GetComponentsInChildren<Rigidbody>();
                    foreach (Rigidbody rb in childrenRbs)
                    {
                        rb.AddExplosionForce(explosionForce, explosionPosition, radius, 1f, ForceMode.Impulse);
                    }

                    _weaponManager._playerController.IncrementKillCount();
                }
            }          
        }
    }

    private void GenerateExplosionGrenade()
    {
        GameObject explosionPrefab = _weaponManager._weaponStats.explosionGrenade.gameObject;

        if (ObjectPoolService.Instance == null || explosionPrefab == null) return;

        GameObject pooledExplosion = ObjectPoolService.Instance.GetPooledObject(explosionPrefab);

        pooledExplosion.transform.SetPositionAndRotation(transform.position, Quaternion.identity);

        if (pooledExplosion.TryGetComponent(out ParticleSystem ps))
        {
            ps.Play(true);
        }
    }

    private void HandleShakeCamera()
    {
        float distance = Vector3.Distance(_weaponManager._playerOwner.transform.position, transform.position);
        if (distance > _weaponManager._weaponStats.attackRadius) return;

        float falloffMultiplier = 1 - (distance / _weaponManager._weaponStats.attackRadius);
        falloffMultiplier = Mathf.Clamp01(falloffMultiplier);

        float finalShakeIntensity = _weaponManager._weaponStats.shakeIntensity * falloffMultiplier;
        Vector3 impulseForce = Vector3.one * finalShakeIntensity;
        if (_shakeCamera != null)
        {
            _shakeCamera.GenerateImpulse(impulseForce);
        }       
    }

    private void HandleAmmo()
    { 
        _currentAmmo -= 1;
        Mathf.Clamp(_currentAmmo, 0, _weaponManager._weaponStats.ammoPerMag);
        RefreshUI();
    }
    #endregion
}
