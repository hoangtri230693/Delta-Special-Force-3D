using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class WeaponThrowController : MonoBehaviour
{
    [SerializeField] private WeaponManager _weaponManager;
    [SerializeField] private CinemachineImpulseSource _shakeCamera;
    [SerializeField] private WeaponAudio _weaponAudio;
    
    private Camera _playerCamera;
    private Transform _playerOwner;

    public int _currentAmmo;
    public int _currentReverse;

    private void OnEnable()
    {
        if (_weaponManager._playerLocal != null)
        {
            UIGameManager_TeamDeathmatch.instance.UpdateUIWeaponAmmo(_currentAmmo, _currentReverse);
        }
    }
    private void Start()
    {
        _playerCamera = Camera.main;
        _playerOwner = transform.root;

        if (_weaponManager._playerLocal != null)
        {
            UIGameManager_TeamDeathmatch.instance.UpdateUIWeaponAmmo(_currentAmmo, _currentReverse);
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
        Vector3 throwDirection = (cameraForward + _playerCamera.transform.up * 0.2f + _playerCamera.transform.right * 0.1f).normalized;

        float safeDistance = 0.3f;
        transform.position += throwDirection * safeDistance;

        transform.parent = null;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;

        rb.AddForce(throwDirection * _weaponManager._weaponStats.throwForce, ForceMode.VelocityChange);
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
        float baseDamage = _weaponManager._weaponStats.damage;
        LayerMask raycastMask = _weaponManager._weaponStats.targetMask;

        Collider[] colliders = Physics.OverlapSphere(explosionPosition, radius);

        foreach (Collider hitCollider in colliders)
        {
            float distance = Vector3.Distance(explosionPosition, hitCollider.transform.position);
            
            float fallOffMultiplier = 1 - (distance / radius);
            fallOffMultiplier = Mathf.Clamp01(fallOffMultiplier);

            PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                float finalDamage = baseDamage * fallOffMultiplier;
                Vector3 directionToTarget = (hitCollider.bounds.center - explosionPosition).normalized;
                float distanceToTarget = Vector3.Distance(explosionPosition, hitCollider.bounds.center);
                if (Physics.Raycast(explosionPosition, directionToTarget, out RaycastHit hit, distanceToTarget, raycastMask))
                {
                    if (hit.collider != hitCollider)
                    {
                        finalDamage = 0f;
                    }
                }
                if (finalDamage > 0f)
                {
                    finalDamage = Mathf.RoundToInt(finalDamage);
                    playerHealth.UpdateHealth(finalDamage, _weaponManager._weaponStats.itemType);
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
        float distance = Vector3.Distance(_playerOwner.position, transform.position);
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
        if (_weaponManager._playerLocal != null)
        {
            UIGameManager_TeamDeathmatch.instance.UpdateUIWeaponAmmo(_currentAmmo, _currentReverse);
        }
    }
    #endregion
}
