using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

public class PlayerAnimationEvents : MonoBehaviour
{
    public WeaponShootController _primaryShootController;
    public WeaponShootController _secondaryShootController;
    public WeaponMeleeController _meleeController;
    public WeaponThrowController _throwController;

    [Header("Component")]
    private BotController _botController;
    private BotNavAgent _botNavAgent;
    private RangeDetector _rangeDetector;
    private LineOfSightDetector _lineOfSightDetector;
    private CapsuleCollider _capsuleCollider;
    private Animator _animator;
    private AudioSource _audioSource;
    private NavMeshAgent _navMeshAgent;
    private BehaviorGraphAgent _behaviorGraphAgent;

    [Header("Scripts Player")]
    private PlayerController _playerController;
    private PlayerRig _playerRig;
    private PlayerInput _playerInput;


    private float _primaryIKTargetWeight = 1f;
    private float _secondaryIKTargetWeight = 1f;
    private float _meleeIKTargetWeight = 1f;
    private float _throwIKTargetWeight = 1f;


    private void Awake()
    {
        _botController = GetComponent<BotController>();
        _botNavAgent = GetComponent<BotNavAgent>();
        _rangeDetector = GetComponent<RangeDetector>();
        _lineOfSightDetector = GetComponent<LineOfSightDetector>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();

        _playerController = GetComponent<PlayerController>();
        _playerRig = GetComponent<PlayerRig>();
    }

    private void LateUpdate()
    {
        if (_playerRig == null) return;
        _playerRig._primaryLeftHandIK.weight = _primaryIKTargetWeight;
        _playerRig._secondaryLeftHandIK.weight = _secondaryIKTargetWeight;
        _playerRig._meleeRightHandIK.weight = _meleeIKTargetWeight;
        _playerRig._throwRightHandIK.weight = _throwIKTargetWeight;
    }

    public void DeathEvent()
    {
        if (_playerInput != null) _playerInput.enabled = false;
        if (_botController != null) _botController.enabled = false;
        if (_botNavAgent != null) _botNavAgent.enabled = false;
        if (_rangeDetector != null) _rangeDetector.enabled = false;
        if (_lineOfSightDetector != null) _lineOfSightDetector.enabled = false;
        if (_navMeshAgent != null) _navMeshAgent.enabled = false;
        if (_behaviorGraphAgent != null) _behaviorGraphAgent.enabled = false;
    }

    public void DeathEvent1()
    {
        _capsuleCollider.enabled = false;
        _audioSource.enabled = false;
        _animator.enabled = false;
        this.enabled = false;

        if (GameManager_ZombieSurvival.instance != null)
            StartCoroutine(GameManager_ZombieSurvival.instance.UpdatePlayerDeath());
    }

    public void ReloadingEvent()
    {
        if (_playerController._itemType == ItemType.PrimaryItem)
        {
            _primaryIKTargetWeight = 0f;
            _primaryShootController.HandleReload();
        }
        if (_playerController._itemType == ItemType.SecondaryItem)
        {
            _secondaryIKTargetWeight = 0f;
            _secondaryShootController.HandleReload();
        }
    }

    public void ReloadingEvent1()
    {
        if (_playerController._itemType == ItemType.PrimaryItem)
        {
            _primaryIKTargetWeight = 1f;
            _primaryShootController.HandleReload1();
        }
        if (_playerController._itemType == ItemType.SecondaryItem)
        {
            _secondaryIKTargetWeight = 1f;
            _secondaryShootController.HandleReload1();
        }
    }

    public void ThrowGrenadeEvent()
    {
        _throwIKTargetWeight = 0f;
    }

    public void ThrowGrenadeEvent1()
    {
        if (_throwController != null)
        {
            _throwController.ThrowGrenade();
        }
    }

    public void ThrowGrenadeEvent2()
    {
        _throwIKTargetWeight = 1f;
    }

    public void StabbingKnifeEvent()
    {
        _meleeIKTargetWeight = 0f;     
    }

    public void StabbingKnifeEvent1()
    {
        _meleeController.StabbingKnife();
    }

    public void StabbingKnifeEvent2()
    {
        _meleeIKTargetWeight = 1f;
    }
}