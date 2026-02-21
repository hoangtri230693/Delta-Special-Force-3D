using Unity.Behavior;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

public class WeaponMeleeController : MonoBehaviour
{
    [SerializeField] private WeaponManager _weaponManager;
    [SerializeField] private CinemachineImpulseSource _shakeCamera;
    [SerializeField] private WeaponAudio _weaponAudio;
   
    private PlayerHealth _playerHealth;
    private bool _isHitKnife = false;

    public int _currentAmmo;
    public int _currentReverse;

    private void OnEnable() => RefreshUI();

    private void Start() => RefreshUI();

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
        if (_playerHealth != null && !_playerHealth._isDead)
        {
            if (_playerHealth.CompareTag("Zombie"))
                _playerHealth.UpdateHealth(100, _weaponManager._weaponStats.itemType);
            else
                _playerHealth.UpdateHealth(_weaponManager._weaponStats.damage, _weaponManager._weaponStats.itemType);

            if (_playerHealth._currentHealth <= 0)
            {
                var characterController = _playerHealth.GetComponent<CharacterController>();
                if (characterController != null) characterController.enabled = false;

                var navAgent = _playerHealth.GetComponent<NavMeshAgent>();
                if (navAgent != null) navAgent.enabled = false;

                var behaviorAgent = _playerHealth.GetComponent<BehaviorGraphAgent>();
                if (behaviorAgent != null) behaviorAgent.enabled = false;

                var switcher = _playerHealth.GetComponent<RagdollSwitcher>();
                if (switcher != null) switcher.EnableRagdolls();

                Rigidbody rb = _playerHealth.GetComponent<Rigidbody>();
                if (rb != null && _playerHealth.CompareTag("Zombie"))
                {
                    Vector3 forceDirection = (_playerHealth.transform.position - transform.position).normalized;
                    rb.AddForce(forceDirection * 500f);
                }

                _weaponManager._playerController.IncrementKillCount();
                _playerHealth._isDead = true;
            }
        }
            
        Debug.Log("Hit Knife");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsValidTarget(other))
        {
            _isHitKnife = true;
            _playerHealth = other.GetComponent<PlayerHealth>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsValidTarget(other))
        {
            _isHitKnife = false;
            _playerHealth = null;
        }
    }

    private bool IsValidTarget(Collider other)
    {
        return other.CompareTag("AlphaTeam") || other.CompareTag("BravoTeam") ||
               other.CompareTag("DeltaTeam") || other.CompareTag("Terrorist") ||
               other.CompareTag("Zombie");
    }
}
