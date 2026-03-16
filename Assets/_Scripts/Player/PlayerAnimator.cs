using UnityEngine;


public class PlayerAnimator : MonoBehaviour
{
    private Animator _animator;

    private int _primaryItemLayerIndex;
    private int _meleeItemLayerIndex;
    private int _throwItemLayerIndex;


    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {    
        _primaryItemLayerIndex = _animator.GetLayerIndex("Primary Item Layer");
        _meleeItemLayerIndex = _animator.GetLayerIndex("Melee Item Layer");
        _throwItemLayerIndex = _animator.GetLayerIndex("Throw Item Layer");
    }

    public void UpdateMovementState(Vector2 input, MovementState movementState, CombatState combatState)
    {
        if (combatState == CombatState.Aim)
        {
            float multiplier = (movementState == MovementState.Run) ? 2f : 1f;
            _animator.SetFloat("Horizontal", input.x * multiplier, 0.1f, Time.deltaTime);
            _animator.SetFloat("Vertical", input.y * multiplier, 0.1f, Time.deltaTime);
        }
        else
        {
            float targetSpeed = 0f;
            switch (movementState)
            {
                case MovementState.Walk:
                    targetSpeed = 1f;
                    break;
                case MovementState.Run:
                    targetSpeed = 2f;
                    break;
                case MovementState.Idle:
                default:
                    targetSpeed = 0f;
                    break;
            }
            _animator.SetFloat("Speed", targetSpeed, 0.1f, Time.deltaTime);
        }
    }

    public void UpdateStanceState(StanceState stanceState)
    {
        _animator.SetBool("isCrouching", stanceState == StanceState.Crouch);
    }

    public void UpdateJumping(MovementState movementState)
    {
        if (movementState == MovementState.JumpOI)
        {
            _animator.SetTrigger("JumpOI");
        }
        if (movementState == MovementState.JumpOM)
        {
            _animator.SetTrigger("JumpOM");
        }
    }

    public void UpdateAiming(bool isAiming)
    {
        _animator.SetBool("isAiming", isAiming);
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
            if (stanceState == StanceState.Stand)
            {
                _animator.SetTrigger("ReloadOS");
            }
            else if (stanceState == StanceState.Crouch)
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
        _animator.SetLayerWeight(_meleeItemLayerIndex, isMeleeItem ? 1f : 0f);
        _animator.SetLayerWeight(_throwItemLayerIndex, isThrowableItem ? 1f : 0f);
    }

    public void UpdateHurt()
    {
        _animator.SetTrigger("Hurt");
    }

    public void UpdateDeathState(LifeState lifeState)
    {
        switch (lifeState)
        {
            case LifeState.DeathShoot:
                _animator.SetTrigger("DeathShoot");
                break;
            case LifeState.DeathMelee:
                _animator.SetTrigger("DeathMelee");
                break;
            case LifeState.DeathThrow:
                _animator.SetTrigger("DeathThrow");
                break;
        }

        this.enabled = false;
    }

    public void ResetMovementState()
    {
        _animator.SetFloat("Horizontal", 0f);
        _animator.SetFloat("Vertical", 0f);
        _animator.SetFloat("Speed", 0f);
    }
}
