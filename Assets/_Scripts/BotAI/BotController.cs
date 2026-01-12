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

    private Vector3 _verticalVelocity;
    private Transform _target;

    public bool RoundActive => GameManager.instance._currentGameState == GameState.RoundActive;
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
        if (GameManager.instance._currentGameState != GameState.RoundActive) return;
        if (_playerController._lifeState == LifeState.None) return;
        ApplyMovement();
        CheckStateFromBlackBoard();
        _behaviorGraphAgent.BlackboardReference.SetVariableValue("RoundActive", RoundActive);
        _behaviorGraphAgent.BlackboardReference.SetVariableValue("ShouldDefend", ShouldDefend);
    }


    private void RandomBuyPrimaryItem()
    {
        int index = Random.Range(4, 14);
        GameManager.instance.BuyWeapon(index, _playerController, _playerInventory, _playerHealth);
    }

    private void RandomBuySecondaryItem()
    {
        int index = Random.Range(0, 4);
        GameManager.instance.BuyWeapon(index, _playerController, _playerInventory, _playerHealth);
    }

    private void RandomBuyThrowItem()
    {
        int index = Random.Range(14, 16);
        GameManager.instance.BuyWeapon(index, _playerController, _playerInventory, _playerHealth);
    }

    private void RandomBuyArmorItem()
    {
        int index = Random.Range(16, 18);
        GameManager.instance.BuyWeapon(index, _playerController, _playerInventory, _playerHealth);
    }

    private void CheckStateFromBlackBoard()
    {
        if (_behaviorGraphAgent.BlackboardReference.GetVariableValue("DetectTarget", out GameObject targetObj))
        {
            if (targetObj != null)
            {
                _target = targetObj.transform;
            }
            else
            {
                _target = null;
            }
        }

        if (_behaviorGraphAgent.BlackboardReference.GetVariableValue("TerroristState", out TerroristState currentState))
        {
            if (currentState == TerroristState.Patrol)
            {
                OnEnterPatrolState();
            }    

            if (currentState == TerroristState.Attack)
            {
                OnEnterAttackState();
            }
        }
    }

    private void OnEnterPatrolState()
    {
        if (_playerController._isAiming)
            _playerController._isAiming = false;
    }

    private void OnEnterAttackState()
    {
        if (!_playerController._isAiming)
            _playerController._isAiming = true;

        if (_playerController._actionState == ActionState.Reload) return;

        // Kiểm tra góc trước khi cho phép bắn (Aim Check)
        Vector3 directionToTarget = (_target.position - transform.position).normalized;
        directionToTarget.y = 0;
        float dot = Vector3.Dot(transform.forward, directionToTarget);

        // Chỉ bắn khi hướng mặt gần như trùng với hướng mục tiêu (dot > 0.99 nghĩa là góc < 8 độ)
        if (_playerController._canShoot && dot > 0.99f)
        {
            _playerController._actionState = ActionState.ManualShoot;
        }
        else
        {
            // Nếu chưa xoay kịp thì chuyển về Idle hoặc Aim để chờ
            _playerController._actionState = ActionState.None;
        }
    }

    // ================== MOVE ==================
    private void ApplyMovement()
    {
        Vector3 desiredVelocity = _botNavAgent.DesiredVelocity;

        // CHỈ xoay theo hướng di chuyển nếu KHÔNG đang nhắm bắn mục tiêu
        if (!_playerController._isAiming)
        {
            if (desiredVelocity.sqrMagnitude > 0.1f)
            {
                Vector3 lookDir = desiredVelocity.normalized;
                lookDir.y = 0;
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _rotationSpeed * Time.deltaTime);
            }
        }
        else if (_target != null)
        {
            // Nếu đang nhắm bắn, gọi hàm xoay về phía mục tiêu
            RotateTowardsTarget();
        }

        // Phần di chuyển Gravity giữ nguyên...
        if (_characterController.isGrounded) _verticalVelocity.y = -2f;
        else _verticalVelocity.y += _gravity * Time.deltaTime;

        Vector3 finalMove = desiredVelocity + _verticalVelocity;
        _characterController.Move(finalMove * Time.deltaTime);

        UpdateAnimator(desiredVelocity);
    }

    private void RotateTowardsTarget()
    {
        if (_target == null) return;

        // Tính toán hướng (bỏ qua trục Y để không bị nghiêng người)
        Vector3 direction = (_target.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            float angle = Quaternion.Angle(transform.rotation, targetRot);

            // NẾU góc sai lệch còn lớn (> 1 độ) HOẶC chưa thể bắn, thì tiếp tục xoay
            if (angle > 1.0f || !_playerController._canShoot)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _rotationSpeed * Time.deltaTime);
            }
            else
            {
                // Đã xoay đúng hướng và canShoot = true -> Khóa góc xoay trực tiếp
                transform.rotation = targetRot;
            }
        }
    }

    private void UpdateAnimator(Vector3 moveDir)
    {
        if (_animator == null) return;

        float speed = new Vector3(moveDir.x, 0, moveDir.z).magnitude;

        Vector3 localMove = transform.InverseTransformDirection(moveDir);

        _animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
        _animator.SetFloat("Direction", localMove.z, 0.1f, Time.deltaTime);
    }

    // ================== SET TARGET ==================
    public void SetTarget(Transform target)
    {
        _target = target;
    }
}
