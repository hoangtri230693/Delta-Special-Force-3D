using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerAnimationEvents : MonoBehaviour
{
    public WeaponShootController _primaryShootController;
    public WeaponShootController _secondaryShootController;
    public WeaponMeleeController _meleeController;
    public WeaponThrowController _throwController;

    private BotController _botController;
    private BotNavAgent _botNavAgent;
    private RangeDetector _rangeDetector;
    private LineOfSightDetector _lineOfSightDetector;
    private PlayerController _playerController;
    private PlayerRig _playerRig;
    private PlayerInput _playerInput;
    private PlayerAnimator _playerAnimator;
    private PlayerInventory _playerInventory;
    private PlayerAudio _playerAudio;
    private PlayerAnimationEvents _playerAnimationEvents;
    private PlayerHealth _playerHealth;
    private RigBuilder _rigBuilder;
    private CapsuleCollider _capsuleCollider;
    private Animator _animator;
    private AudioSource _audioSource;

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
        _playerController = GetComponent<PlayerController>();
        _playerRig = GetComponent<PlayerRig>();
        _playerInput = GetComponent<PlayerInput>();
        _playerAnimator = GetComponent<PlayerAnimator>();
        _playerInventory = GetComponent<PlayerInventory>();
        _playerAudio = GetComponent<PlayerAudio>();
        _playerAnimationEvents = this.GetComponent<PlayerAnimationEvents>();
        _playerHealth = GetComponent<PlayerHealth>();
        _rigBuilder = GetComponent<RigBuilder>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
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
        _playerController._currentItem = ItemType.None;
        _playerController._isAiming = false;
        SetLayer(gameObject, "Default");
    }

    public void DeathEvent1()
    {
        _rigBuilder.enabled = false;
        _capsuleCollider.enabled = false;
        _animator.enabled = false;
        _audioSource.enabled = false;
        _playerRig.enabled = false;
        _playerInventory.enabled = false;
        _playerAudio.enabled = false;
        _playerHealth.enabled = false;
        _playerAnimator.enabled = false;
        _playerAnimationEvents.enabled = false;     
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

    private void SetLayer(GameObject obj, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1) return;

        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayer(child.gameObject, layerName);
        }
    }
}
