using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInput : MonoBehaviour
{
    [Header("Player Input Actions")]
    [SerializeField] private InputActionReference _moveAction;
    [SerializeField] private InputActionReference _sprintAction;
    [SerializeField] private InputActionReference _jumpAction;
    [SerializeField] private InputActionReference _crouchAction;
    [SerializeField] private InputActionReference _aimAction;
    [SerializeField] private InputActionReference _attackAction;
    [SerializeField] private InputActionReference _switchItemAction;
    [SerializeField] private InputActionReference _reloadAction;
    [SerializeField] private InputActionReference _dropAction;
    
    [Header("Buy Menu Actions")]
    [SerializeField] private InputActionReference _openBuyTableAction;
    [SerializeField] private InputActionReference _selectBuyItemAction;
    [SerializeField] private InputActionReference _buyAction;

    [Header("Result Menu Actions")]
    [SerializeField] private InputActionReference _openResultTableAction;

    private PlayerController _playerController;
    private FixedJoystick _joystick;
    
    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _joystick = FindFirstObjectByType<FixedJoystick>();
    }

    private void Update()
    {
        Vector2 moveInput = _moveAction.action.ReadValue<Vector2>();
        if (_joystick != null)
        {
            Vector2 joystickInput = new Vector2(_joystick.Horizontal, _joystick.Vertical);
            if (joystickInput.magnitude > 0.1f) moveInput = joystickInput;
        }

        bool isSprinting = _sprintAction.action.IsPressed();
        bool isJumping = _jumpAction.action.triggered;
        bool isCrouching = _crouchAction.action.triggered;
        bool isAiming = _aimAction.action.triggered;
        bool isManualAttacking = _attackAction.action.triggered;
        bool isAutomaticAttacking = _attackAction.action.IsPressed();
        bool isSwitchingItem = _switchItemAction.action.triggered;
        bool isReloading = _reloadAction.action.triggered;
        bool isDropping = _dropAction.action.triggered;
        bool isOpeningBuyTable = _openBuyTableAction.action.triggered;
        bool isSelectedItem = _selectBuyItemAction.action.triggered;
        bool isBuying = _buyAction.action.triggered;
        bool isOpeningResultTable = _openResultTableAction.action.triggered;

        // ----- Pass To Controller -----
        _playerController.UpdateInputs(
            moveInput,
            isSprinting,
            isJumping,
            isCrouching,
            isAiming,
            isManualAttacking,
            isAutomaticAttacking,
            isSwitchingItem,
            isReloading,
            isDropping,
            isOpeningBuyTable,
            isSelectedItem,
            isBuying,
            isOpeningResultTable);
    }
}
