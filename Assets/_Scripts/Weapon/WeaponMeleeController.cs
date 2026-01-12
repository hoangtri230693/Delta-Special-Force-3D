using Unity.Cinemachine;
using UnityEngine;

public class WeaponMeleeController : MonoBehaviour
{
    [SerializeField] private WeaponManager _weaponManager;
    [SerializeField] private CinemachineImpulseSource _shakeCamera;
    [SerializeField] private WeaponAudio _weaponAudio;
   
    private PlayerHealth _playerHealth;
    private bool _isHitKnife = false;

    public int _currentAmmo;
    public int _currentReverse;

    private void OnEnable()
    {
        if (_weaponManager._playerLocal != null)
        {
            UIGameManager.instance.UpdateUIWeaponAmmo(_currentAmmo, _currentReverse);
        }
    }

    private void Start()
    {
        if (_weaponManager._playerLocal != null)
        {
            UIGameManager.instance.UpdateUIWeaponAmmo(_currentAmmo, _currentReverse);
        }
    }

    public void InitializeMelee()
    {
        _currentAmmo = _weaponManager._weaponStats.ammoPerMag;
        _currentReverse = _weaponManager._weaponStats.ammoReverse;
    }

    public void AssignAnimationEvents(PlayerAnimationEvents playerAnimationEvents)
    {
        if (_weaponManager._weaponStats.itemType == ItemType.MeleeItem)
        {
            playerAnimationEvents._meleeController = this;
        }
    }

    public void StabbingKnife()
    {
        _weaponAudio.PlayAudioMelee();
        
        if (_isHitKnife && _playerHealth != null)
        {
            HandleHitTarget();
            HandleShakeCamera();
        }
    }

    private void HandleShakeCamera()
    {
        float finalShakeIntensity = _weaponManager._weaponStats.shakeIntensity;
        Vector3 impulseForce = Vector3.one * finalShakeIntensity;
        if (_shakeCamera != null && _weaponManager._playerLocal != null)
        {
            _shakeCamera.GenerateImpulse(impulseForce);
        }
    }
    private void HandleHitTarget()
    {
        _playerHealth.UpdateHealth(_weaponManager._weaponStats.damage, _weaponManager._weaponStats.itemType);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            _isHitKnife = true;
            _playerHealth = other.GetComponent<PlayerHealth>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            _isHitKnife = false;
            _playerHealth = null;
        }
    }
}
