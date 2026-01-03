using UnityEngine;


public class PlayerAnimator : MonoBehaviour
{
    private PlayerController _playerController;
    private Animator _animator;

    private int _primaryItemLayerIndex;
    private int _meleeLayerIndex;
    private int _throwItemLayerIndex;


    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {    
        _primaryItemLayerIndex = _animator.GetLayerIndex("Primary Item Layer");
        _meleeLayerIndex = _animator.GetLayerIndex("Melee Item Layer");
        _throwItemLayerIndex = _animator.GetLayerIndex("Throw Item Layer");
    }

    private void Update()
    {
        UpdateMovementState();
        UpdateActionState();
        UpdateItemType();
        UpdateLifeState();
    }
                               
    private void UpdateMovementState()
    {
        _animator.SetFloat("Speed", _playerController._currentSpeed, 0.1f, Time.deltaTime);
        _animator.SetFloat("Direction", _playerController._currentDirection, 0.1f, Time.deltaTime);

        if (_playerController._movementState == MovementState.JumpOI)
        {
            _animator.SetTrigger("JumpOI");
            _playerController._movementState = MovementState.Idle;
        }

        if (_playerController._movementState == MovementState.JumpOM)
        {
            _animator.SetTrigger("JumpOM");
            _playerController._movementState = MovementState.Idle;
        }

        if (_playerController._stanceState == StanceState.Crouching)
        {
            _animator.SetBool("isCrouching", true);
        }
        else if (_playerController._stanceState == StanceState.Standing)
        {
            _animator.SetBool("isCrouching", false);
        }
    }


    private void UpdateActionState()
    {
        if (_playerController._actionState == ActionState.Melee)
        {
            _animator.SetTrigger("Stabbing");
            _playerController._actionState = ActionState.None;
        }

        if (_playerController._actionState == ActionState.Throw)
        {
            _animator.SetTrigger("Throwing");
            _playerController._actionState = ActionState.None;
        }

        if (_playerController._actionState == ActionState.Reload)
        {
            if (_playerController._stanceState == StanceState.Standing)
            {
                _animator.SetTrigger("ReloadOS");
                _playerController._actionState = ActionState.None;
            }
            else if (_playerController._stanceState == StanceState.Crouching)
            {
                _animator.SetTrigger("ReloadOC");
                _playerController._actionState = ActionState.None;
            }          
        }
    }
    
    private void UpdateItemType()
    {
        if (_playerController._actionState == ActionState.SwitchItem)
        {
            bool isPrimaryItem = _playerController._currentItem == ItemType.PrimaryItem;
            bool isMeleeItem = _playerController._currentItem == ItemType.MeleeItem;
            bool isThrowableItem = _playerController._currentItem == ItemType.ThrowItem;

            _animator.SetLayerWeight(_primaryItemLayerIndex, isPrimaryItem ? 1f : 0f);
            _animator.SetLayerWeight(_meleeLayerIndex, isMeleeItem ? 1f : 0f);
            _animator.SetLayerWeight(_throwItemLayerIndex, isThrowableItem ? 1f : 0f);

            _playerController._actionState = ActionState.None;
        }          
    }

    private void UpdateLifeState()
    {
        if (_playerController._lifeState == LifeState.Hit)
        {
            _animator.SetTrigger("Hit");
            _playerController._lifeState = LifeState.Alive;
        }
        if (_playerController._lifeState == LifeState.DeathShoot)
        {
            _animator.SetTrigger("DeathShoot");
            _playerController._lifeState = LifeState.None;
        }
        if (_playerController._lifeState == LifeState.DeathMelee)
        {
            _animator.SetTrigger("DeathMelee");
            _playerController._lifeState = LifeState.None;
        }
        if (_playerController._lifeState == LifeState.DeathThrow)
        {
            _animator.SetTrigger("DeathThrow");
            _playerController._lifeState = LifeState.None;
        }
    }
}
