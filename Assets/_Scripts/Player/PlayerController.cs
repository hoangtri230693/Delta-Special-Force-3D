using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public enum MovementState { Idle, Walk, Run, JumpOI, JumpOM, Fall }
public enum StanceState { Standing, Crouching }
public enum ActionState { None, ManualShoot, AutomaticShoot, Melee, Throw, SwitchItem, Reload, Drop }
public enum ItemType { None, PrimaryItem, SecondaryItem, MeleeItem, ThrowItem, ArmorItem }
public enum LifeState { None, Alive, Hit, DeathShoot, DeathMelee, DeathThrow }
public enum TeamType { None, CounterTerrorist, Terrorist }


public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private CharacterStatsSO _characterStats;
    [SerializeField] private Transform _cameraThirdPerson;   
    [SerializeField] private Transform _yawTarget;
    [SerializeField] private Rig _rigAim;

    private PlayerCamera _playerCamera;
    private PlayerAudio _playerAudio;
    private PlayerTeam _playerTeam;
    private Vector3 _velocity = Vector3.zero;
    private Vector3 _moveDirection = Vector3.zero;

    private int _killedCount = 0;
    private int _deathCount = 0;

    public bool _isAiming = false;
    public bool _isCrouching = false;
    public bool _isOpeningBuyTable = false;
    public bool _isOpeningResultTable = false;
    public bool _isSelectedItem = false;
    public bool _canAction = true;
    public bool _isSwitchItem = false;
    public bool _canReload = false;
    public bool _canShoot = false;
    public bool _shouldDefend = false;

    public MovementState _movementState = MovementState.Idle;
    public StanceState _stanceState = StanceState.Standing;
    public ActionState _actionState = ActionState.None;
    public ItemType _currentItem = ItemType.SecondaryItem;
    public LifeState _lifeState = LifeState.Alive;
    public float _currentSpeed = 0;
    public float _currentDirection = 0;
    public int _currentCash = 10000;

    
    private void Awake()
    {
        _playerCamera = GetComponent<PlayerCamera>();
        _playerAudio = GetComponent<PlayerAudio>();
        _playerTeam = GetComponent<PlayerTeam>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (_lifeState == LifeState.DeathShoot || _lifeState == LifeState.DeathMelee || _lifeState == LifeState.DeathThrow)
        {
            GameManager.instance.UpdateTeamCount(_playerTeam._playerTeam);
            _isAiming = false;
            _rigAim.weight = 0f;
            IncrementDeadCount();
        }

        if (_lifeState == LifeState.None || GameManager.instance._currentGameState == GameState.Countdown)
        {
            HandleGravity();
            _characterController.Move(_velocity * Time.deltaTime);
        }

        if (_characterController.isGrounded && _lifeState == LifeState.None)
        {
            _characterController.enabled = false;
            this.enabled = false;
            return;
        }    
    }

    public void ResetPlayerState()
    {
        _lifeState = LifeState.Alive;
        _currentCash = 10000;
        _isAiming = false;
        _rigAim.weight = 0f;
        _playerCamera.ExitAimMode();
        _isCrouching = false;
        _isOpeningBuyTable = false;
        _isOpeningResultTable = false;
        _isSelectedItem = false;
        _isSwitchItem = false;
        _canReload = false;
        _canShoot = false;
        _shouldDefend = false;
        _movementState = MovementState.Idle;
        _stanceState = StanceState.Standing;
        _actionState = ActionState.None;
        _currentItem = ItemType.SecondaryItem;
        _currentSpeed = 0;
        _currentDirection = 0;
    }

    public void OnCharacterController(bool isOn)
    {
        _characterController.enabled = isOn;
    }

    public void IncrementKillCount()
    {
        _killedCount++;
        UIGameManager.instance.UpdateKilledCount(_playerTeam._playerTeam, _playerTeam._playerID, _killedCount);
        GameManager.instance.UpdatePlayerKilled(this);
    }

    public void IncrementDeadCount()
    {
        _deathCount++;
        UIGameManager.instance.UpdateDeathCount(_playerTeam._playerTeam, _playerTeam._playerID, _deathCount);
    }

    public void UpdateInputs(Vector2 moveInput, bool isSprinting, bool isJumping, bool isCrouching,
                            bool isAiming, bool isManualAttacking, bool isAutomaticAttacking, 
                            bool isSwitchingItem, bool isReloading, bool isDropping, 
                            bool isOpeningBuyTable, bool isSelectedItem, bool isBuying,
                            bool isOpeningResultTable)
    {
        if (_lifeState == LifeState.None) return;

        if (GameManager.instance._currentGameState == GameState.RoundActive)
        {
            if (_canAction)
            {
                HandleMovement(moveInput, isSprinting);
                HandleGravityAndJump(isJumping, moveInput, isCrouching);
                HandleCrouching(isCrouching);
                HandleAiming(isAiming, moveInput);
                HandleAttack(isManualAttacking, isAutomaticAttacking);
                HandleSwitchItem(isSwitchingItem);
                HandleReloading(isReloading);
                HandleDropping(isDropping);
                HandleOpeningBuyTable(isOpeningBuyTable);
                HandleOpeningResultTable(isOpeningResultTable);
            }
            else
            {
                HandleMovement(moveInput, isSprinting);
                HandleGravityAndJump(isJumping, moveInput, isCrouching);
                HandleCrouching(isCrouching);
                HandleAiming(isAiming, moveInput);
                HandleOpeningBuyTable(isOpeningBuyTable);
                HandleSelectedItem(isSelectedItem);
                HandleBuyWeapon(isBuying);
                HandleOpeningResultTable(isOpeningResultTable);
            }

            Vector3 totalMove = (_moveDirection * _currentSpeed) + _velocity;
            _characterController.Move(totalMove * Time.deltaTime);
        }
        else
        {
            HandleOpeningBuyTable(isOpeningBuyTable);
            HandleSelectedItem(isSelectedItem);
            HandleBuyWeapon(isBuying);
            HandleOpeningResultTable(isOpeningResultTable);
        }       
    }

    private void HandleMovement(Vector2 input, bool isSprinting)
    {
        _currentDirection = input.y;
        bool isMoving = input.magnitude > 0.1f;

        _currentSpeed = isMoving
            ? (isSprinting ? _characterStats.runSpeed : _characterStats.walkSpeed)
            : 0f;

        if (_isAiming)
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            _moveDirection = forward * input.y + right * input.x;
        }
        else
        {
            if (_cameraThirdPerson != null)
            {
                Vector3 forward = _cameraThirdPerson.forward;
                Vector3 right = _cameraThirdPerson.right;
                forward.y = 0;
                right.y = 0;
                forward.Normalize();
                right.Normalize();

                _moveDirection = forward * input.y + right * input.x;
            }
            else
            {
                Vector3 forward = transform.forward;
                Vector3 right = transform.right;
                forward.y = 0;
                right.y = 0;
                forward.Normalize();
                right.Normalize();

                _moveDirection = forward * input.y + right * input.x;
            }           
        }

        if (isSprinting) _movementState = MovementState.Run;
        else if (isMoving) _movementState = MovementState.Walk;
        else _movementState = MovementState.Idle;
    }

    private void HandleGravityAndJump(bool isJumping, Vector2 input, bool isCrouching)
    {
        if (_characterController.isGrounded)
        {
            if (isJumping)
            {
                if (isCrouching || _isCrouching) return;

                _velocity.y = 0f;
                _velocity.y += _characterStats.jumpForce;
                _movementState = (input.magnitude > 0.1f) ? MovementState.JumpOM : MovementState.JumpOI;
            }
            else
            {
                _velocity.y = -1f;
            }
        }
        else
        {
            _velocity.y += Physics.gravity.y * Time.deltaTime;
        }
    }

    private void HandleGravity()
    {
        if (_characterController.isGrounded)
        {
            _velocity.y = -1f;
        }
        else
        {
            _velocity.y += Physics.gravity.y * Time.deltaTime;
        }
    }
    private void HandleCrouching(bool isCrouching)
    {
        if (isCrouching && !_isCrouching)
        {
            _isCrouching = true;
            _stanceState = StanceState.Crouching;
        }          
        else if (isCrouching && _isCrouching)
        {
            _isCrouching = false;
            _stanceState = StanceState.Standing;
        }           
    }
    
    private void HandleAiming(bool isAiming, Vector2 input)
    {
        if (isAiming && !_isAiming)
        {
            _playerAudio.ZoomSound();
            _isAiming = true;
            _rigAim.weight = 1f;
            _playerCamera.EnterAimMode();
        }
        else if (isAiming && _isAiming)
        {
            _playerAudio.ZoomSound();
            _isAiming = false;
            _rigAim.weight = 0f;
            _playerCamera.ExitAimMode();
        }

        if (_isAiming)
        {
            Vector3 lookDirection = _yawTarget.forward;
            lookDirection.y = 0;

            if (lookDirection.magnitude > 0.1f)
            {
                Quaternion targetRatation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRatation, 5f * Time.deltaTime);
            }
        }
        else if (input.magnitude > 0.1f && input.y > 0)
        {
            Quaternion toRatation = Quaternion.LookRotation(_moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRatation, 5f * Time.deltaTime);
        }
    }

    private void HandleAttack(bool isManualAttacking, bool isAutomaticAttacking)
    {
        if (!_isAiming)
        {
            _actionState = ActionState.None;
            return;
        }

        if (isManualAttacking)
        {
            if (_currentItem == ItemType.MeleeItem)
            {
                _actionState = ActionState.Melee;
            }
            else if (_currentItem == ItemType.ThrowItem)
            {
                _actionState = ActionState.Throw;
            }
            else if (_currentItem == ItemType.SecondaryItem)
            {
                _actionState = ActionState.ManualShoot;
            }
            else if (_currentItem == ItemType.PrimaryItem)
            {
                _actionState = ActionState.ManualShoot;
            }
        }
        else if (isAutomaticAttacking)
        {
            if (_currentItem == ItemType.PrimaryItem)
            {
                _actionState = ActionState.AutomaticShoot;
            }
        }
        else
        {
            _actionState = ActionState.None;
        }
    }

    private void HandleSwitchItem(bool isSwitchingItem)
    {
        if (_isOpeningBuyTable) return;

        if (!isSwitchingItem) return;

        var keyboard = Keyboard.current;

        if (keyboard.digit1Key.wasPressedThisFrame) _currentItem = ItemType.PrimaryItem;         
        else if (keyboard.digit2Key.wasPressedThisFrame) _currentItem = ItemType.SecondaryItem;        
        else if (keyboard.digit3Key.wasPressedThisFrame) _currentItem = ItemType.MeleeItem;        
        else if (keyboard.digit4Key.wasPressedThisFrame) _currentItem = ItemType.ThrowItem;

        _playerAudio.SwitchItemSound();

        _isSwitchItem = true;

        _actionState = ActionState.SwitchItem;
    }

    private void HandleReloading(bool isReloading)
    {
        if (!isReloading) return;
        if (!_canReload) return;

        if (_currentItem == ItemType.PrimaryItem || _currentItem == ItemType.SecondaryItem)
        {
            _actionState = ActionState.Reload;
            _canAction = false;
        }           
    }

    private void HandleDropping(bool isDropping)
    {
        if (!isDropping) return;
      
        _actionState = ActionState.Drop;
    }

    private void HandleOpeningResultTable(bool isOpeningResultTable)
    {
        if (!isOpeningResultTable) return;

        if (isOpeningResultTable && !_isOpeningResultTable)
        {
            _isOpeningResultTable = true;
            UIGameManager.instance.OpenResultMenu(true);
        }
        else if (isOpeningResultTable && _isOpeningResultTable)
        {
            _isOpeningResultTable = false;
            UIGameManager.instance.OpenResultMenu(false);
        }
    }

    private void HandleOpeningBuyTable(bool isOpeningBuyTable)
    {
        if (isOpeningBuyTable && !_isOpeningBuyTable)
        {
            _canAction = false;
            _isOpeningBuyTable = true;
            UIGameManager.instance.OpenMenuItem(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else if (isOpeningBuyTable && _isOpeningBuyTable)
        {
            _canAction = true;
            _isOpeningBuyTable = false;
            UIGameManager.instance.OpenMenuItem(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void HandleSelectedItem(bool isBuyingItem)
    {
        if (!isBuyingItem) return;

        int indexWeaponListOpen = -1;

        indexWeaponListOpen = UIGameManager.instance._indexWeaponListOpen;

        if (_isOpeningBuyTable && indexWeaponListOpen > -1)
        {
            var keyboard = Keyboard.current;

            if (keyboard.digit1Key.wasPressedThisFrame) UIGameManager.instance.OnShowWeapon(0);
            else if (keyboard.digit2Key.wasPressedThisFrame) UIGameManager.instance.OnShowWeapon(1);
            else if (keyboard.digit3Key.wasPressedThisFrame) UIGameManager.instance.OnShowWeapon(2);
            else if (keyboard.digit4Key.wasPressedThisFrame) UIGameManager.instance.OnShowWeapon(3);
            else if (keyboard.digit5Key.wasPressedThisFrame) UIGameManager.instance.OnShowWeapon(4);
            else if (keyboard.digit6Key.wasPressedThisFrame) UIGameManager.instance.OnShowWeapon(5);
            else if (keyboard.digit7Key.wasPressedThisFrame) UIGameManager.instance.OnShowWeapon(6);
            else if (keyboard.digit8Key.wasPressedThisFrame) UIGameManager.instance.OnShowWeapon(7);
            else if (keyboard.digit9Key.wasPressedThisFrame) UIGameManager.instance.OnShowWeapon(8);
            else if (keyboard.escapeKey.wasPressedThisFrame) UIGameManager.instance.HideAllMenuWeapon();

            _isSelectedItem = true;
        }
        else if (_isOpeningBuyTable)
        {
            var keyboard = Keyboard.current;

            if (keyboard.digit1Key.wasPressedThisFrame) UIGameManager.instance.OnShowWeaponList(0);
            else if (keyboard.digit2Key.wasPressedThisFrame) UIGameManager.instance.OnShowWeaponList(1);
            else if (keyboard.digit3Key.wasPressedThisFrame) UIGameManager.instance.OnShowWeaponList(2);
            else if (keyboard.digit4Key.wasPressedThisFrame) UIGameManager.instance.OnShowWeaponList(3);
            else if (keyboard.digit5Key.wasPressedThisFrame) UIGameManager.instance.OnShowWeaponList(4);
            else if (keyboard.digit6Key.wasPressedThisFrame) UIGameManager.instance.OnShowWeaponList(5);
            else if (keyboard.digit7Key.wasPressedThisFrame) UIGameManager.instance.OnShowWeaponList(6);
            else if (keyboard.digit8Key.wasPressedThisFrame) UIGameManager.instance.OnShowWeaponList(7);
            else if (keyboard.digit9Key.wasPressedThisFrame) UIGameManager.instance.OnShowWeaponList(8);
        }       
    }

    private void HandleBuyWeapon(bool isBuying)
    {
        if (!isBuying) return;

        if (_isSelectedItem)
        {
            UIGameManager.instance.OnBuyWeapon();
        }
    }
}
