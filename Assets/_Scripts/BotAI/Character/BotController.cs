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
    private float _rotationSpeed = 10f;
    private float _gravity = -9.81f;
    private Vector3 _verticalVelocity;
    private Transform _target;

    private bool _roundActive => GameManager_TeamDeathmatch.instance._currentGameState == GameState.RoundActive;
    private bool _shouldDefend = false;
    public bool _canShoot = false;


    private void Start()
    {
        RandomBuyArmorItem();
        RandomBuyThrowItem();
        RandomBuySecondaryItem();
        RandomBuyPrimaryItem();

        _playerController._isAiming = true;
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
        if (_playerController._actionState == ActionState.ManualShoot)
            _playerController._actionState = ActionState.None;
    }
     
    private void EnterAttackState()
    {
        // ===== BẮN =====
        if (_playerController._canAction && _canShoot)
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
        int weaponID = Random.Range(4, 14);
        UIShopInGame.instance.BuyWeapon(weaponID, _playerController, _playerInventory, _playerHealth);
    }

    private void RandomBuySecondaryItem()
    {
        int weaponID = Random.Range(0, 4);
        UIShopInGame.instance.BuyWeapon(weaponID, _playerController, _playerInventory, _playerHealth);
    }

    private void RandomBuyThrowItem()
    {
        int weaponID = Random.Range(14, 16);
        UIShopInGame.instance.BuyWeapon(weaponID, _playerController, _playerInventory, _playerHealth);
    }

    private void RandomBuyArmorItem()
    {
        int weaponID = Random.Range(16, 18);
        UIShopInGame.instance.BuyWeapon(weaponID, _playerController, _playerInventory, _playerHealth);
    }
}
