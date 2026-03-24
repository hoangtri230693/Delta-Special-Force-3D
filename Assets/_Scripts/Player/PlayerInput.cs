using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInput : MonoBehaviour
{
    [Header("Player Input Actions")]
    [SerializeField] private InputActionReference _moveAction;
    [SerializeField] private InputActionReference _lookAction;
    [SerializeField] private InputActionReference _sprintAction;
    [SerializeField] private InputActionReference _jumpAction;
    [SerializeField] private InputActionReference _crouchAction;
    [SerializeField] private InputActionReference _aimAction;
    [SerializeField] private InputActionReference _switchShoulderAction;
    [SerializeField] private InputActionReference _zoomAction;
    [SerializeField] private InputActionReference _attackAction;
    [SerializeField] private InputActionReference _switchItemAction;
    [SerializeField] private InputActionReference _reloadAction;
    [SerializeField] private InputActionReference _dropAction;
    [SerializeField] private InputActionReference _pauseAction;

    [Header("Buy Menu Actions")]
    [SerializeField] private InputActionReference _openShopInGameAction;
    [SerializeField] private InputActionReference _selectItemAction;
    [SerializeField] private InputActionReference _buyItemAction;

    [Header("Result Menu Actions")]
    [SerializeField] private InputActionReference _openResultTableAction;

    private PlayerController _playerController;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        _moveAction.action.Enable();
        _lookAction.action.Enable();
        _sprintAction.action.Enable();
        _jumpAction.action.Enable();
        _crouchAction.action.Enable();
        _aimAction.action.Enable();
        _switchShoulderAction.action.Enable();
        _zoomAction.action.Enable();
        _attackAction.action.Enable();
        _switchItemAction.action.Enable();
        _reloadAction.action.Enable();
        _dropAction.action.Enable();
        _pauseAction.action.Enable();
        _openShopInGameAction.action.Enable();
        _selectItemAction.action.Enable();
        _buyItemAction.action.Enable();
        _openResultTableAction.action.Enable();
    }

    private void OnDisable()
    {
        _moveAction.action.Disable();
        _lookAction.action.Disable();
        _sprintAction.action.Disable();
        _jumpAction.action.Disable();
        _crouchAction.action.Disable();
        _aimAction.action.Disable();
        _switchShoulderAction.action.Disable();
        _zoomAction.action.Disable();
        _attackAction.action.Disable();
        _switchItemAction.action.Disable();
        _reloadAction.action.Disable();
        _dropAction.action.Disable();
        _pauseAction.action.Disable();
        _openShopInGameAction.action.Disable();
        _selectItemAction.action.Disable();
        _buyItemAction.action.Disable();
        _openResultTableAction.action.Disable();
    }

    private void Update()
    {
        Vector2 moveInput = _moveAction.action.ReadValue<Vector2>();
        Vector2 lookInput = _lookAction.action.ReadValue<Vector2>();
        bool isSprinting = _sprintAction.action.IsPressed();
        bool isJumping = _jumpAction.action.triggered;
        bool isCrouching = _crouchAction.action.triggered;
        bool isAiming = _aimAction.action.triggered;
        bool isSwitchingShoulder = _switchShoulderAction.action.triggered;
        float zoomDelta = _zoomAction.action.ReadValue<float>();
        bool isManualAttacking = _attackAction.action.triggered;
        bool isAutomaticAttacking = _attackAction.action.IsPressed();
        bool isSwitchingItem = _switchItemAction.action.triggered;
        bool isReloading = _reloadAction.action.triggered;
        bool isDropping = _dropAction.action.triggered;
        bool isOpeningShopInGame = _openShopInGameAction.action.triggered;
        bool isSelectedItem = _selectItemAction.action.triggered;
        bool isBuyingItem = _buyItemAction.action.triggered;
        bool isOpeningResultTable = _openResultTableAction.action.triggered;
        bool isPausing = _pauseAction.action.triggered;

        // ----- Pass To Controller -----
        _playerController.UpdateInputs(
            moveInput,
            lookInput,
            isSprinting,
            isJumping,
            isCrouching,
            isAiming,
            isSwitchingShoulder,
            zoomDelta,
            isManualAttacking,
            isAutomaticAttacking,
            isSwitchingItem,
            isReloading,
            isDropping,
            isOpeningShopInGame,
            isSelectedItem,
            isBuyingItem,
            isOpeningResultTable,
            isPausing);
    }
}
