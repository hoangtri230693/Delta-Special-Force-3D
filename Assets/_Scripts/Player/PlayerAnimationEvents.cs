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
    private RigBuilder _rigBuilder;
    private CapsuleCollider _capsuleCollider;
    private Animator _animator;
    private AudioSource _audioSource;
    private NavMeshAgent _navMeshAgent;
    private BehaviorGraphAgent _behaviorGraphAgent;

    [Header("Scripts Player")]
    private PlayerController _playerController;
    private PlayerRig _playerRig;
    private PlayerInput _playerInput;
    private PlayerAnimator _playerAnimator;
    private PlayerInventory _playerInventory;
    private PlayerAudio _playerAudio;
    private PlayerHealth _playerHealth;
    

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
        _rigBuilder = GetComponent<RigBuilder>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();

        _playerController = GetComponent<PlayerController>();
        _playerRig = GetComponent<PlayerRig>();
        _playerInput = GetComponent<PlayerInput>();
        _playerAnimator = GetComponent<PlayerAnimator>();
        _playerInventory = GetComponent<PlayerInventory>();
        _playerAudio = GetComponent<PlayerAudio>();
        _playerHealth = GetComponent<PlayerHealth>();        
    }

    private void LateUpdate()
    {
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

        _rigBuilder.enabled = false;
        _capsuleCollider.enabled = false;
        _audioSource.enabled = false;   
    }

    public void DeathEvent1()
    {
        _animator.enabled = false;

        _playerRig.enabled = false;
        _playerInventory.enabled = false;
        _playerAudio.enabled = false;
        _playerAnimator.enabled = false;
    }    

    public void ReloadingEvent()
    {
        if (_playerController._currentItem == ItemType.PrimaryItem)
        {
            _primaryIKTargetWeight = 0f;
            _primaryShootController.HandleReload();
        }
        if (_playerController._currentItem == ItemType.SecondaryItem)
        {
            _secondaryIKTargetWeight = 0f;
            _secondaryShootController.HandleReload();
        }
    }
    
    public void ReloadingEvent1()
    {
        if (_playerController._currentItem == ItemType.PrimaryItem)
        {
            _primaryIKTargetWeight = 1f;
            _primaryShootController.HandleReload1();
        }
        if (_playerController._currentItem == ItemType.SecondaryItem)
        {
            _secondaryIKTargetWeight = 1f;
            _secondaryShootController.HandleReload1();
        }

        _playerController._canAction = true;
    }

    public void ThrowGrenadeEvent()
    {
        if (_playerController._currentItem == ItemType.ThrowItem)
        {
            _throwIKTargetWeight = 0f;
        }
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
        if (_playerController._currentItem == ItemType.ThrowItem)
        {
            _throwIKTargetWeight = 1f;
            _playerController._currentItem = ItemType.None;
        }
    }

    public void StabbingKnifeEvent()
    {
        if (_playerController._currentItem == ItemType.MeleeItem)
        {
            _meleeIKTargetWeight = 0f;
        }
    }

    public void StabbingKnifeEvent1()
    {
        _meleeController.StabbingKnife();
    }

    public void StabbingKnifeEvent2()
    {
        if (_playerController._currentItem == ItemType.MeleeItem)
        {
            _meleeIKTargetWeight = 1f;
        }
    }
}
