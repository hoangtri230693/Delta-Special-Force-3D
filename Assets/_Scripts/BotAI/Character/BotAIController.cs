using System.Linq;
using Unity.Behavior;
using UnityEngine;

public class BotAIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private PlayerInventory _playerInventory;
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private BotNavAgent _botNavAgent;
    [SerializeField] private Animator _animator;
    [SerializeField] private BehaviorGraphAgent _behaviorGraphAgent;

    [Header("Movement & Aiming Settings")]
    private Transform _target;
    private Vector3 _velocity;  
    private float _rotationSpeed = 10f;
    private float _scanSpeed = 5f;
    private float _scanAngle = 15f;

    private bool _roundActive => GameManager_TeamDeathmatch.instance._currentGameState == GameState.RoundActive;
    private bool _shouldDefend = false;
    public bool _canShoot = false;
    private bool _shouldReloadAmmo = false;


    private void Start()
    {
        RandomBuyByItemType(ItemType.ArmorItem);
        RandomBuyByItemType(ItemType.ThrowItem);
        RandomBuyByItemType(ItemType.SecondaryItem);
        RandomBuyByItemType(ItemType.PrimaryItem);        
    }

    private void Update()
    {
        if (!_roundActive) return;
        if (_playerController._lifeState == LifeState.None) return;

        CheckStateFromBlackBoard();
        ApplyMovement();

        _behaviorGraphAgent.BlackboardReference.SetVariableValue("RoundActive", _roundActive);
        _behaviorGraphAgent.BlackboardReference.SetVariableValue("ShouldDefend", _shouldDefend);
    }

    public void SetShouldReloadAmmo(bool value)
    {
        _shouldReloadAmmo = value;
    }

    public void SetShouldDefend(bool value)
    {
        _shouldDefend = value;
    }

    public void SetCanShoot(bool value)
    {
        _canShoot = value;
    }

    // ===================== BLACKBOARD =====================
    private void CheckStateFromBlackBoard()
    {
        if (_behaviorGraphAgent.BlackboardReference.GetVariableValue("Target", out GameObject targetObj))
            _target = targetObj ? targetObj.transform : null;

        if (_behaviorGraphAgent.BlackboardReference.GetVariableValue("TerroristState", out TerroristState terroristState))
        {
            switch (terroristState)
            {
                case TerroristState.Patrol:
                    EnterPatrolState();
                    break;
                case TerroristState.Chase:
                    EnterChaseState();
                    break;
                case TerroristState.Attack:
                    EnterAttackState();
                    break;
            }
        }

        if (_behaviorGraphAgent.BlackboardReference.GetVariableValue("CounterState", out CounterState counterState))
        {
            switch (counterState)
            {
                case CounterState.Assault:
                    EnterPatrolState();
                    break;
                case CounterState.Chase:
                    EnterChaseState();
                    break;
                case CounterState.Attack:
                    EnterAttackState();
                    break;
            }
        }
    }

    // ===================== STATES =====================
    private void EnterPatrolState()
    {
        if (_playerController._actionState == ActionState.ManualShoot)
            _playerController._actionState = ActionState.None;

        if (_playerController._combatState == CombatState.Aim)
        {
            _playerController._combatState = CombatState.None;
            _playerInventory.ActiveCombatItem(_playerController._itemType, _playerController._combatState);
        }

        if (_playerController._movementState != MovementState.Run)
            _playerController._movementState = MovementState.Run;
    }
    
    private void EnterChaseState()
    {
        if (_playerController._movementState != MovementState.Run)
            _playerController._movementState = MovementState.Run;
    }

    private void EnterAttackState()
    {
        if (_playerController._combatState == CombatState.None)
        {
            _playerController._combatState = CombatState.Aim;
            _playerInventory.ActiveCombatItem(_playerController._itemType, _playerController._combatState);
        }

        if (_playerController._movementState != MovementState.Walk)
            _playerController._movementState = MovementState.Walk;

        if (_shouldReloadAmmo)
        {
            _playerController.ReloadAmmo();
            SetShouldReloadAmmo(false);
        }
        else if (_playerController._canAction && _canShoot)
        {
            if (_playerController._actionState != ActionState.ManualShoot)
                _playerController._actionState = ActionState.ManualShoot;
        }
        else
        {
            _playerController._actionState = ActionState.None;
        }
    }

    // ===================== MOVEMENT =====================
    private void ApplyMovement()
    {
        Vector3 desiredVelocity = _botNavAgent.DesiredVelocity;

        // ===== ROTATION =====
        if (_target != null)
        {
            RotateTowardsTarget();
        }
        else if (desiredVelocity.sqrMagnitude > 0.1f)
        {
            RotateTowardsMovement(desiredVelocity);
        }

        // ===== GRAVITY =====
        if (_characterController.isGrounded)
            _velocity.y = -2f;
        else
            _velocity.y += Physics.gravity.y * Time.deltaTime;

        Vector3 finalMove = desiredVelocity + _velocity;
        _characterController.Move(finalMove * Time.deltaTime);

        UpdateAnimator(desiredVelocity);
    }

    private void RotateTowardsTarget()
    {
        Vector3 direction = (_target.position - transform.position);
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) return;

 
        Quaternion targetRot = Quaternion.LookRotation(direction.normalized);

        if (!_canShoot)
        {
            float yOffset = Mathf.Sin(Time.time * _scanSpeed) * _scanAngle;
            Quaternion scanOffset = Quaternion.Euler(0, yOffset, 0);
            targetRot *= scanOffset;
        }

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            _rotationSpeed * Time.deltaTime
        );
    }

    private void RotateTowardsMovement(Vector3 moveDir)
    {
        Vector3 dir = moveDir.normalized;
        dir.y = 0f;

        Quaternion targetRot = Quaternion.LookRotation(dir);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            _rotationSpeed * Time.deltaTime
        );
    }

    // ===================== ANIM =====================

    private void UpdateAnimator(Vector3 moveDir)
    {
        float currentSpeed = moveDir.magnitude;
        _animator.SetFloat("Speed", currentSpeed, 0.1f, Time.deltaTime);
    }

    // ===================== BUY RANDOM =====================

    private void RandomBuyByItemType(ItemType type)
    {
        var allWeapons = WeaponStatsManager.instance.weaponStats;
        var filteredWeapons = allWeapons.Where(w => w.itemType == type).ToList();

        if (filteredWeapons.Count > 0)
        {
            int randomIndex = Random.Range(0, filteredWeapons.Count);
            int selectedWeaponID = filteredWeapons[randomIndex].weaponID;

            UIShopInGame.instance.BuyWeapon(
                selectedWeaponID,
                _playerController,
                _playerInventory,
                _playerHealth
            );
        }
    }
}
