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
        UpdateHit();
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


    public void UpdateActionState(ActionState actionState, StanceState stanceState)
    {        
        if (actionState == ActionState.Melee)
        {
            _animator.SetTrigger("Stabbing");
        }

        if (actionState == ActionState.Throw)
        {
            _animator.SetTrigger("Throwing");
        }

        if (actionState == ActionState.Reload)
        {
            if (stanceState == StanceState.Standing)
            {
                _animator.SetTrigger("ReloadOS");
            }
            else if (stanceState == StanceState.Crouching)
            {
                _animator.SetTrigger("ReloadOC");
            }          
        }
    }
    
    public void UpdateItemType(ItemType itemType)
    {
        bool isPrimaryItem = itemType == ItemType.PrimaryItem;
        bool isMeleeItem = itemType == ItemType.MeleeItem;
        bool isThrowableItem = itemType == ItemType.ThrowItem;

        _animator.SetLayerWeight(_primaryItemLayerIndex, isPrimaryItem ? 1f : 0f);
        _animator.SetLayerWeight(_meleeLayerIndex, isMeleeItem ? 1f : 0f);
        _animator.SetLayerWeight(_throwItemLayerIndex, isThrowableItem ? 1f : 0f);
    }

    public void UpdateHit()
    {
        if (_playerController._lifeState == LifeState.Hit)
        {
            _animator.SetTrigger("Hit");
            _playerController._lifeState = LifeState.Alive;
        }       
    }

    public void UpdateDeath(LifeState lifeState)
    {
        if (lifeState == LifeState.DeathShoot)
        {
            _animator.SetTrigger("DeathShoot");
            _playerController._lifeState = LifeState.None;
        }
        if (lifeState == LifeState.DeathMelee)
        {
            _animator.SetTrigger("DeathMelee");
            _playerController._lifeState = LifeState.None;
        }
        if (lifeState == LifeState.DeathThrow)
        {
            _animator.SetTrigger("DeathThrow");
            _playerController._lifeState = LifeState.None;
        }

        this.enabled = false;
    }
}
