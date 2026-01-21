using UnityEngine;
using Unity.Behavior;

public class BotController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterStatsSO _characterStats;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private PlayerInventory _playerInventory;
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private BotNavAgent _botNavAgent;
    [SerializeField] private Animator _animator;
    [SerializeField] private BehaviorGraphAgent _behaviorGraphAgent;

    [Header("Movement Settings")]
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private float _gravity = -9.81f;

    [Header("Attack Settings")]
    [SerializeField] private float _aimDotThreshold = 0.99f;

    private Vector3 _verticalVelocity;
    private Transform _target;

    public bool RoundActive => GameManager_TeamDeathmatch.instance._currentGameState == GameState.RoundActive;
    public bool ShouldDefend => _playerController._shouldDefend;


    private void Start()
    {
        RandomBuyThrowItem();
        RandomBuySecondaryItem();
        RandomBuyPrimaryItem();
        RandomBuyArmorItem();
    }

    private void Update()
    {
        if (!RoundActive) return;
        if (_playerController._lifeState == LifeState.None) return;

        CheckStateFromBlackBoard();
        ApplyMovement();

        _behaviorGraphAgent.BlackboardReference.SetVariableValue("RoundActive", RoundActive);
        _behaviorGraphAgent.BlackboardReference.SetVariableValue("ShouldDefend", ShouldDefend);
    }

    // ===================== BLACKBOARD =====================
    private void CheckStateFromBlackBoard()
    {
        if (_behaviorGraphAgent.BlackboardReference.GetVariableValue("DetectTarget", out GameObject targetObj))
            _target = targetObj ? targetObj.transform : null;

        if (_behaviorGraphAgent.BlackboardReference.GetVariableValue("TerroristState", out TerroristState terroristState))
        {
            switch (terroristState)
            {
                case TerroristState.Patrol:
                    EnterPatrolState();
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

                case CounterState.Attack:
                    EnterAttackState();
                    break;
            }
        }
    }

    // ===================== STATES =====================
    private void EnterPatrolState()
    {
        _playerController._isAiming = false;

        if (_playerController._actionState == ActionState.ManualShoot)
            _playerController._actionState = ActionState.None;
    }

    private void EnterAttackState()
    {
        if (_target == null) return;
        if (_playerController._actionState == ActionState.Reload) return;

        _playerController._isAiming = true;

        // ===== Aim check =====
        Vector3 dir = (_target.position - transform.position).normalized;
        dir.y = 0f;

        float dot = Vector3.Dot(transform.forward, dir);

        // ===== BẮN =====
        if (_playerController._canShoot && dot >= _aimDotThreshold)
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
        if (_target != null &&
            (_playerController._isAiming ||
             _playerController._actionState == ActionState.ManualShoot))
        {
            RotateTowardsTarget();
        }
        else if (desiredVelocity.sqrMagnitude > 0.1f)
        {
            RotateTowardsMovement(desiredVelocity);
        }

        // ===== GRAVITY =====
        if (_characterController.isGrounded)
            _verticalVelocity.y = -2f;
        else
            _verticalVelocity.y += _gravity * Time.deltaTime;

        Vector3 finalMove = desiredVelocity + _verticalVelocity;
        _characterController.Move(finalMove * Time.deltaTime);

        UpdateAnimator(desiredVelocity);
    }

    private void RotateTowardsTarget()
    {
        Vector3 direction = (_target.position - transform.position);
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(direction.normalized);

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
        if (!_animator) return;

        float speed = new Vector3(moveDir.x, 0, moveDir.z).magnitude;
        Vector3 localMove = transform.InverseTransformDirection(moveDir);

        _animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
        _animator.SetFloat("Direction", localMove.z, 0.1f, Time.deltaTime);
    }

    // ===================== BUY RANDOM =====================
    private void RandomBuyPrimaryItem()
    {
        int index = Random.Range(4, 14);
        GameManager_TeamDeathmatch.instance.BuyWeapon(index, _playerController, _playerInventory, _playerHealth);
    }

    private void RandomBuySecondaryItem()
    {
        int index = Random.Range(0, 4);
        GameManager_TeamDeathmatch.instance.BuyWeapon(index, _playerController, _playerInventory, _playerHealth);
    }

    private void RandomBuyThrowItem()
    {
        int index = Random.Range(14, 16);
        GameManager_TeamDeathmatch.instance.BuyWeapon(index, _playerController, _playerInventory, _playerHealth);
    }

    private void RandomBuyArmorItem()
    {
        int index = Random.Range(16, 18);
        GameManager_TeamDeathmatch.instance.BuyWeapon(index, _playerController, _playerInventory, _playerHealth);
    }
}
