using Unity.Cinemachine;
using UnityEngine;
using DeltaSpecialForce3D.Enums;

public class WeaponShootController : MonoBehaviour
{   
    [SerializeField] private WeaponController _weaponController;
    [SerializeField] private GunRecoilController _gunRecoilController;
    [SerializeField] private BarrelPointController _barrelPointController;
    [SerializeField] private CinemachineImpulseSource _recoilCamera;
    [SerializeField] private WeaponAudio _weaponAudio;
    [SerializeField] private Transform _barrelPoint;
    [SerializeField] private Transform _shellEjectPoint;

    private float _nextAttackTime = 0f;
    private ParticleSystem _currentFireSmoke;
    private float _smokeStopDelay = 0.2f;

    public int _currentAmmo;
    public int _currentReverse;


    private void OnEnable()
    {
        RefreshUI();
        _weaponAudio.PlayWeaponSound(WeaponSoundType.Cock);   
    }

    private void Start() => RefreshUI();    

    private void LateUpdate()
    {
        if (_currentFireSmoke != null)
        {
            if (_currentFireSmoke.isPlaying)
            {
                _currentFireSmoke.transform.position = _barrelPoint.position;
                _currentFireSmoke.transform.rotation = _barrelPoint.rotation;
            }

            if (Time.time > _nextAttackTime + _smokeStopDelay)
            {
                StopFireSmoke();
            }
        }
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

    public void InitializeShoot()
    {
        _currentAmmo = _weaponController.WeaponStats.ammoPerMag;
        _currentReverse = _weaponController.WeaponStats.ammoReverse;
    }

    public void AssignAnimationEvents(PlayerAnimationEvents playerAnimationEvents)
    {
        if (_weaponController.WeaponStats.itemType == ItemType.PrimaryItem)
        {
            playerAnimationEvents._primaryShootController = this;
        }
        if (_weaponController.WeaponStats.itemType == ItemType.SecondaryItem)
        {
            playerAnimationEvents._secondaryShootController = this;
        }
    }

    public void ActiveCombat(CombatState combatState)
    {
        switch (combatState)
        {
            case CombatState.Aim:
                _barrelPointController.enabled = true;
                _barrelPointController.EnableCrosshair();
                break;
            case CombatState.None:
                _barrelPointController.DisableCrosshair();
                _barrelPointController.enabled = false;
                break;
        }
    }

    public void ZoomControl(float zoomDelta)
    {
        _barrelPointController.ScopeZoom(zoomDelta);
    }

    public void HandleReload()
    {
        _weaponAudio.PlayWeaponSound(WeaponSoundType.Reload);
    }

    public void HandleReload1()
    {
        int neededAmmo = _weaponController.WeaponStats.ammoPerMag - _currentAmmo;

        if (neededAmmo <= 0 || _currentReverse <= 0) return;
        int ammoToLoad = Mathf.Min(neededAmmo, _currentReverse);
        _currentAmmo += ammoToLoad;
        _currentReverse -= ammoToLoad;
        _weaponAudio.PlayWeaponSound(WeaponSoundType.Cock);
        RefreshUI();
    }

    public void TryShoot()
    {
        if (Time.time < _nextAttackTime) return;

        Shoot();
        float fireDelay = 60f / _weaponController.WeaponStats.fireRate;
        _nextAttackTime = Time.time + fireDelay;
        GenerateFireSmoke();
    }

    private void Shoot()
    {
        if (_currentAmmo > 0)
        {
            HandleRecoil();
            EjectShellCasing();
            GenerateMuzzleFlash();
            GenerateBulletImpact();
            HandleHitTarget();
            HandleAudio();
            HandleAmmo();         
        }
        else
        {
            HandleAudio();
            HandleBotReloadAmmo();
        }
    }

    private void HandleBotReloadAmmo()
    {
        if (_weaponController._botAIController != null)
            _weaponController._botAIController.SetShouldReloadAmmo(true);
        if (_currentReverse <= 0 && _weaponController._botAIController != null)
            _currentReverse = _weaponController.WeaponStats.ammoReverse;
    }

    private void HandleAudio()
    {
        if (_currentAmmo > 0)
            _weaponAudio.PlayWeaponSound(WeaponSoundType.Fire);
        else
            _weaponAudio.PlayWeaponSound(WeaponSoundType.DryFire);
    }

    private void HandleRecoil()
    {
        if (_gunRecoilController == null) return;
        _gunRecoilController.enabled = true;

        float recoilValue = _weaponController.WeaponStats.recoilAmount;

        Vector3 recoilVector = _gunRecoilController.UpdateRecoil(recoilValue);
        Vector3 impulseForce = new Vector3(0f, Mathf.Abs(recoilVector.x), Mathf.Abs(recoilVector.z)) * 0.03f;
        if (_recoilCamera != null && _weaponController._botAIController == null)
        {
            _recoilCamera.GenerateImpulse(impulseForce);
        }
    }

    private void GenerateMuzzleFlash()
    {
        GameObject muzzleFlashPrefab = _weaponController.WeaponStats.muzzleFlash.gameObject;

        if (_barrelPoint == null || ObjectPoolService.Instance == null || muzzleFlashPrefab == null) return;

        GameObject pooledFlash = ObjectPoolService.Instance.GetPooledObject(muzzleFlashPrefab);

        pooledFlash.transform.SetPositionAndRotation(_barrelPoint.position, _barrelPoint.rotation);

    }

    private void EjectShellCasing()
    {
        GameObject shellCasingPrefab = _weaponController.WeaponStats.shellCasing.gameObject;

        if (_barrelPoint == null || ObjectPoolService.Instance == null || shellCasingPrefab == null) return;

        GameObject pooledCasing = ObjectPoolService.Instance.GetPooledObject(shellCasingPrefab);

        pooledCasing.transform.SetPositionAndRotation(_shellEjectPoint.position, _shellEjectPoint.rotation);
    }

    private void GenerateFireSmoke()
    {
        if (_currentAmmo == 0) return;

        if (_currentFireSmoke != null && _currentFireSmoke.isPlaying) return;

        GameObject fireSmokePrefab = _weaponController.WeaponStats.fireSmoke.gameObject;

        if (_barrelPoint == null || ObjectPoolService.Instance == null || fireSmokePrefab == null) return;

        GameObject pooledSmoke = ObjectPoolService.Instance.GetPooledObject(fireSmokePrefab);

        pooledSmoke.transform.position = _barrelPoint.position;
        pooledSmoke.transform.rotation = _barrelPoint.rotation;

        if (pooledSmoke.TryGetComponent(out ParticleSystem ps))
        {
            _currentFireSmoke = ps;
            ps.Play(true);
        }
    }

    private void StopFireSmoke()
    {
        if (_currentFireSmoke != null)
        {
            _currentFireSmoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            _currentFireSmoke = null;
        }
    }

    private void GenerateBulletImpact()
    {
        GameObject bulletImpactPrefab = _weaponController.WeaponStats.bulletImpact.gameObject;

        if (_barrelPoint == null || ObjectPoolService.Instance == null || bulletImpactPrefab == null) return;

        GameObject pooledImpact = ObjectPoolService.Instance.GetPooledObject(bulletImpactPrefab);

        pooledImpact.transform.SetPositionAndRotation(_barrelPointController._targetPosition, _barrelPointController._targetRotation);

        if (pooledImpact.TryGetComponent(out ParticleSystem ps))
        {
            ps.Play(true);
        }
    }

    private void HandleAudioHit()
    {
        _weaponAudio.PlayWeaponSound(WeaponSoundType.Hit);
    }

    private void HandleHitTarget()
    {
        RaycastHit hit = _barrelPointController._lastHit;
        PlayerHealth playerHealth = _barrelPointController._playerHealth;
        ZombieHealth zombieHealth = _barrelPointController._zombieHealth;

        if (playerHealth != null && playerHealth._currentHealth > 0)
        {
            HandleAudioHit();
            float damage = _weaponController.WeaponStats.damage;
            ItemType itemType = _weaponController.WeaponStats.itemType;
            playerHealth.UpdateHealth(damage, itemType);

            if (playerHealth._currentHealth <= 0)
            {
                if (playerHealth.TryGetComponent<PlayerController>(out var targetController))
                {
                    // Lấy thông tin Team của người bắn và mục tiêu
                    var team = _weaponController._playerController.GetComponent<PlayerTeam>().Team;
                    var targetTeam = targetController.GetComponent<PlayerTeam>().Team;

                    // ĐIỀU KIỆN: 
                    // - Mục tiêu không phải là chính mình
                    // - Mục tiêu phải khác Team với mình
                    if (targetController != _weaponController._playerController && team != targetTeam)
                    {
                        _weaponController._playerController.IncrementKillCount();
                    }
                }
            }
        }

        if (zombieHealth != null && zombieHealth._currentHealth > 0)
        {
            HandleAudioHit();
            float damage = _weaponController.WeaponStats.damage;
            ItemType itemType = _weaponController.WeaponStats.itemType;
            zombieHealth.UpdateHealth(damage);

            if (zombieHealth._currentHealth <= 0)
            {
                Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 forceDir = (hit.point - _barrelPoint.position).normalized;
                    float shootForce = _weaponController.WeaponStats.shootForce;
                    rb.AddForceAtPosition(forceDir * shootForce, hit.point, ForceMode.Impulse);
                }
                _weaponController._playerController.IncrementKillCount();
            }
        }
    }

    private void HandleAmmo()
    {
        _currentAmmo = Mathf.Max(0, _currentAmmo - 1);
        RefreshUI();
    }
}