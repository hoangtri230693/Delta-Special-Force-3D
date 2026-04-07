using Unity.Cinemachine;
using UnityEngine;
using DeltaSpecialForce3D.Enums;


public class WeaponMeleeController : MonoBehaviour
{
    [SerializeField] private WeaponController _weaponController;
    [SerializeField] private CinemachineImpulseSource _shakeCamera;
    [SerializeField] private WeaponAudio _weaponAudio;
   
    private PlayerHealth _playerHealth;
    private ZombieHealth _zombieHealth;
    private bool _isHitKnife = false;

    public int _currentAmmo;
    public int _currentReverse;

    private void OnEnable() => RefreshUI();

    private void Start() => RefreshUI();

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

    public void InitializeMelee()
    {
        _currentAmmo = _weaponController.WeaponStats.ammoPerMag;
        _currentReverse = _weaponController.WeaponStats.ammoReverse;
    }

    public void AssignAnimationEvents(PlayerAnimationEvents playerAnimationEvents)
    {
        if (_weaponController.WeaponStats.itemType == ItemType.MeleeItem)
        {
            playerAnimationEvents._meleeController = this;
        }
    }

    public void StabbingKnife()
    {
        HandleAudio();
        
        if (_isHitKnife && (_playerHealth != null || _zombieHealth != null))
        {
            HandleHitTarget();
            HandleShakeCamera();
            HandleAudioHit();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsValidTarget(other))
        {
            _isHitKnife = true;
            _playerHealth = other.GetComponentInParent<PlayerHealth>();
            _zombieHealth = other.GetComponentInParent<ZombieHealth>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsValidTarget(other))
        {
            _isHitKnife = false;
            _playerHealth = null;
            _zombieHealth = null;
        }
    }

    private void HandleAudio()
    {
        _weaponAudio.PlayWeaponSound(WeaponSoundType.Melee);
    }

    private void HandleAudioHit()
    {
        _weaponAudio.PlayWeaponSound(WeaponSoundType.Hit);
    }

    private void HandleShakeCamera()
    {
        float finalShakeIntensity = _weaponController.WeaponStats.shakeIntensity;
        Vector3 impulseForce = Vector3.one * finalShakeIntensity;
        if (_shakeCamera != null && _weaponController._botAIController == null)
        {
            _shakeCamera.GenerateImpulse(impulseForce);
        }
    }

    private void HandleHitTarget()
    {
        if (_playerHealth != null && _playerHealth._currentHealth > 0)
        {
            _playerHealth.UpdateHealth(_weaponController.WeaponStats.damage, 
                                       _weaponController.WeaponStats.itemType);

            if (_playerHealth._currentHealth <= 0)
            {
                if (_playerHealth.TryGetComponent<PlayerController>(out var targetController))
                {
                    var team = _weaponController._playerController.GetComponent<PlayerTeam>().Team;
                    var targetTeam = targetController.GetComponent<PlayerTeam>().Team;

                    if (targetController != _weaponController._playerController && team != targetTeam)
                    {
                        _weaponController._playerController.IncrementKillCount();
                    }
                }
            }

            _playerHealth = null;
        }

        if (_zombieHealth != null && _zombieHealth._currentHealth > 0)
        {
            _zombieHealth.UpdateHealth(_zombieHealth._currentHealth);

            if (_zombieHealth._currentHealth <= 0)
                _weaponController._playerController.IncrementKillCount();

            _zombieHealth = null;
        }
    }

    private bool IsValidTarget(Collider other)
    {
        return other.CompareTag("AlphaTeam") || other.CompareTag("BravoTeam") ||
               other.CompareTag("DeltaTeam") || other.CompareTag("Terrorist") ||
               other.CompareTag("Zombie");
    }
}
