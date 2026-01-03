using Unity.Cinemachine;
using UnityEngine;

public class WeaponShootController : MonoBehaviour
{   
    [SerializeField] private WeaponManager _weaponManager;
    [SerializeField] private GunRecoilController _gunRecoilController;
    [SerializeField] private BarrelPointController _barrelPointController;
    [SerializeField] private CinemachineImpulseSource _recoilCamera;
    [SerializeField] private WeaponAudio _weaponAudio;
    [SerializeField] private Transform _barrelPoint;
    [SerializeField] private Transform _shellEjectPoint;

    private float _nextAttackTime = 0f;
    private ParticleSystem _currentFireSmokePS;

    public int _currentAmmo;
    public int _currentReverse;


    private void OnEnable()
    {
        if (_weaponManager._playerController == null) return;

        if (_weaponManager._playerController._isSwitchItem == true)
        {
            _weaponAudio.PlayAudioCock();
            _weaponManager._playerController._isSwitchItem = false;
        }
    }

    private void Update()
    {
        if (_weaponManager._playerController == null)
            return;

        CheckActionShoot();
        CheckCanReload();
    }

    public void InitializeAmmo()
    {
        _currentAmmo = _weaponManager._weaponStats.ammoPerMag;
        _currentReverse = _weaponManager._weaponStats.ammoReverse;

        if (_weaponManager._weaponStats.itemType == ItemType.PrimaryItem && _weaponManager._playerLocal != null)
        {
            UIGameManager.instance._weaponPrimaryController = this;
        }
        if (_weaponManager._weaponStats.itemType == ItemType.SecondaryItem && _weaponManager._playerLocal != null)
        {
            UIGameManager.instance._weaponSecondaryController = this;
        }
    }

    public void AssignAnimationEvents(PlayerAnimationEvents playerAnimationEvents)
    {
        if (_weaponManager._weaponStats.itemType == ItemType.PrimaryItem)
        {
            playerAnimationEvents._primaryShootController = this;
        }
        if (_weaponManager._weaponStats.itemType == ItemType.SecondaryItem)
        {
            playerAnimationEvents._secondaryShootController = this;
        }
    }

    public void HandleReload()
    {
        _weaponAudio.PlayAudioReload();
    }

    public void HandleReload1()
    {
        int neededAmmo = _weaponManager._weaponStats.ammoPerMag - _currentAmmo;

        if (neededAmmo <= 0 || _currentReverse <= 0) return;
        int ammoToLoad = Mathf.Min(neededAmmo, _currentReverse);
        _currentAmmo += ammoToLoad;
        _currentReverse -= ammoToLoad;
        _weaponAudio.PlayAudioCock();
    }

    private void CheckActionShoot()
    {
        bool isShooting = _weaponManager._playerController._actionState == ActionState.AutomaticShoot ||
                          _weaponManager._playerController._actionState == ActionState.ManualShoot;

        if (_weaponManager._playerController._actionState == ActionState.AutomaticShoot)
        {
            if (Time.time >= _nextAttackTime)
            {
                Shoot();
                float fireDelay = 60f / _weaponManager._weaponStats.attackRate;
                _nextAttackTime = Time.time + fireDelay;
                GenerateFireSmoke();
            }
        }
        else if (_weaponManager._playerController._actionState == ActionState.ManualShoot)
        {
            if (Time.time >= _nextAttackTime)
            {
                Shoot();
                float fireDelay = 60f / _weaponManager._weaponStats.attackRate;
                _nextAttackTime = Time.time + fireDelay;
                _weaponManager._playerController._actionState = ActionState.None;
                GenerateFireSmoke();
            }
            else
            {
                _weaponManager._playerController._actionState = ActionState.None;
            }
        }

        if (!isShooting && _currentFireSmokePS != null && _currentFireSmokePS.isPlaying)
        {
            StopFireSmoke();
        }
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
            HandleAmmo();
            _weaponAudio.PlayAudioFire();
        }
        else
        {
            _weaponAudio.PlayAudioDryFire();
        }
    }

    private void HandleRecoil()
    {
        if (_gunRecoilController == null) return;

        float recoilValue = _weaponManager._weaponStats.recoilAmount;
        float aimMultiplier = _weaponManager._playerController._isAiming ? 0.6f : 1.0f;

        Vector3 recoilVector = _gunRecoilController.ApplyRecoil(recoilValue) * aimMultiplier;
        Vector3 impulseForce = new Vector3(0f, Mathf.Abs(recoilVector.x), Mathf.Abs(recoilVector.z)) * 0.03f;
        if (_recoilCamera != null && _weaponManager._playerLocal != null)
        {
            _recoilCamera.GenerateImpulse(impulseForce);
        }
    }

    private void GenerateMuzzleFlash()
    {
        GameObject muzzleFlashPrefab = _weaponManager._weaponStats.muzzleFlash.gameObject;

        if (_barrelPoint == null || ObjectPoolService.Instance == null || muzzleFlashPrefab == null) return;

        GameObject pooledFlash = ObjectPoolService.Instance.GetPooledObject(muzzleFlashPrefab);

        pooledFlash.transform.SetPositionAndRotation(_barrelPoint.position, _barrelPoint.rotation);

    }

    private void EjectShellCasing()
    {
        GameObject shellCasingPrefab = _weaponManager._weaponStats.shellCasing.gameObject;

        if (_barrelPoint == null || ObjectPoolService.Instance == null || shellCasingPrefab == null) return;

        GameObject pooledCasing = ObjectPoolService.Instance.GetPooledObject(shellCasingPrefab);

        pooledCasing.transform.SetPositionAndRotation(_shellEjectPoint.position, _shellEjectPoint.rotation);
    }

    private void GenerateFireSmoke()
    {
        if (_currentAmmo == 0) return;

        if (_currentFireSmokePS != null && _currentFireSmokePS.isPlaying) return;

        GameObject fireSmokePrefab = _weaponManager._weaponStats.fireSmoke.gameObject;

        if (_barrelPoint == null || ObjectPoolService.Instance == null || fireSmokePrefab == null) return;

        GameObject pooledSmoke = ObjectPoolService.Instance.GetPooledObject(fireSmokePrefab);

        pooledSmoke.transform.SetParent(_barrelPoint);
        pooledSmoke.transform.position = _barrelPoint.position;
        pooledSmoke.transform.rotation = Quaternion.identity;

        if (pooledSmoke.TryGetComponent(out ParticleSystem ps))
        {
            _currentFireSmokePS = ps;
            ps.Play(true);
        }
    }

    private void StopFireSmoke()
    {
        if (_currentFireSmokePS != null)
        {
            _currentFireSmokePS.Stop();
            _currentFireSmokePS = null;
        }
    }

    private void GenerateBulletImpact()
    {
        GameObject bulletImpactPrefab = _weaponManager._weaponStats.bulletImpact.gameObject;

        if (_barrelPoint == null || ObjectPoolService.Instance == null || bulletImpactPrefab == null) return;

        GameObject pooledImpact = ObjectPoolService.Instance.GetPooledObject(bulletImpactPrefab);

        pooledImpact.transform.SetPositionAndRotation(_barrelPointController._targetPosition, _barrelPointController._targetRotation);

        if (pooledImpact.TryGetComponent(out ParticleSystem ps))
        {
            ps.Play(true);
        }
    }

    private void HandleHitTarget()
    {
        if (_barrelPointController._playerHealth != null)
        {
            float baseDamage = _weaponManager._weaponStats.damage;
            float maxDistance = _weaponManager._weaponStats.maxDistance;
            float actualDistance = Vector3.Distance(_barrelPoint.position, _barrelPointController._targetPosition);
            float damageMultiplier = Mathf.Clamp01(1 - (actualDistance / maxDistance));
            float finalDamage = baseDamage * Mathf.Max(0.1f, damageMultiplier);

            _barrelPointController._playerHealth.UpdateHealth(Mathf.RoundToInt(finalDamage), _weaponManager._weaponStats.itemType);
        }
    }

    private void HandleAmmo()
    {
        _currentAmmo -= 1;
        Mathf.Clamp(_currentAmmo, 0, _weaponManager._weaponStats.ammoPerMag);
    }

    private void CheckCanReload()
    {
        if (_currentAmmo == _weaponManager._weaponStats.ammoPerMag ||
            _currentReverse == 0)
        {
            _weaponManager._playerController._canReload = false;
        }
        else if (_currentAmmo == 0 && _currentReverse != 0 && _weaponManager._playerController._canAction == true)
        {
            _weaponManager._playerController._actionState = ActionState.Reload;
            _weaponManager._playerController._canAction = false;
        }
        else
        {
            _weaponManager._playerController._canReload = true;
        }
    }
}