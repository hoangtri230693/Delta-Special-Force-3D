using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using DeltaSpecialForce3D.Enums;


public class PlayerController : MonoBehaviour
{
    [SerializeField] private CharacterStatsSO _characterStats;
    [SerializeField] private CharacterRigSO _characterRig;
    [SerializeField] private CharacterDataSO _characterData;
    [SerializeField] private CameraStatsSO _cameraStats;
    public CharacterStatsSO CharacterStats => _characterStats;
    public CharacterRigSO CharacterRig => _characterRig;
    public CharacterDataSO CharacterData => _characterData;
    public CameraStatsSO CameraStats => _cameraStats;
    
    [Header("Player Component Group")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private PlayerAnimator _playerAnimator;
    [SerializeField] private PlayerInventory _playerInventory;
    [SerializeField] private PlayerRig _playerRig;
    [SerializeField] private PlayerCamera _playerCamera;
    [SerializeField] private PlayerAudio _playerAudio;
    [SerializeField] private PlayerTeam _playerTeam;

    [Header("Player Movement Data")]
    private Vector3 _forward = Vector3.zero;
    private Vector3 _right = Vector3.zero;
    private Vector3 _velocity = Vector3.zero;
    private Vector3 _moveDirection = Vector3.zero;

    [Header("Player Flags")]
    public bool _roundActive = false;
    public bool _canAction = true;
    private bool _isAiming = false;
    private bool _isCrouching = false;
    private bool _isOpeningShopInGame = false;
    private bool _isOpeningResultTable = false;
    private bool _isSelectedItem = false;    

    [Header("Player States")]
    public MovementState _movementState = MovementState.Idle;
    public StanceState _stanceState = StanceState.Stand;
    public CombatState _combatState = CombatState.None;
    public ActionState _actionState = ActionState.None;
    public ItemType _itemType = ItemType.SecondaryItem;
    public LifeState _lifeState = LifeState.Alive;

    [Header("Player Stats")]
    private int _killedCount = 0;
    private int _deathCount = 0;
    private float _currentSpeed = 0;
    public int _currentCash = 20000;


    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        GetDataCharacter();
    }

    private void Start()
    {      
        SwitchItem();
    }

    private void Update()
    {
       HandleLifeState();
    }

    //------------PUBLIC METHODS------------

    public void ReloadAmmo()
    {
        if (_itemType == ItemType.PrimaryItem || _itemType == ItemType.SecondaryItem)
        {
            _playerAudio.PlayCharacterSound(CharacterSoundType.Zoom);
            _actionState = ActionState.Reload;
            _playerAnimator.UpdateActionState(_actionState, _stanceState);
            _canAction = false;
        }
    }

    public void SwitchItem()
    {
        _playerAudio.PlayCharacterSound(CharacterSoundType.SwitchItem);
        _playerAnimator.UpdateItemType(_itemType);
        _playerInventory.UpdateItem(_itemType);
        _playerInventory.ActiveCombatItem(_combatState);
        _playerRig.UpdateRigWeight(_itemType);
        _playerRig.UpdateBodyOffset(_itemType, _stanceState);
    }

    public void ResetPlayerState()
    {
        _lifeState = LifeState.Alive;
        _isAiming = false;
        _playerRig.UpdateAimRigWeight(false);
        if (_playerCamera != null) _playerCamera.ExitAimMode();
        _isCrouching = false;
        _movementState = MovementState.Idle;
        _stanceState = StanceState.Stand;
        _actionState = ActionState.None;
        _combatState = CombatState.None;
        _currentSpeed = 0;
        _playerAnimator.ResetMovementState();
        _playerAnimator.UpdateAiming(_isAiming);
        _playerAnimator.UpdateStanceState(_stanceState);
        _playerAnimator.UpdateActionState(_actionState, _stanceState);
        _playerInventory.ActiveCombatItem(_combatState);
    }

    public void IncrementKillCount()
    {
        _killedCount++;

        if (UIGameManager_TeamDeathmatch.instance != null && GameManager_TeamDeathmatch.instance != null)
            GameManager_TeamDeathmatch.instance.UpdatePlayerKilled(_playerTeam.Team, _playerTeam.ActorID, _killedCount);

        if (UIGameManager_ZombieSurvival.instance != null && GameManager_ZombieSurvival.instance != null)
            GameManager_ZombieSurvival.instance.UpdatePlayerKilled(_killedCount);
    }

    public void IncrementDeadCount()
    {
        _deathCount++;

        if (UIGameManager_TeamDeathmatch.instance != null)
            GameManager_TeamDeathmatch.instance.UpdatePlayerDeath(_playerTeam.Team, _playerTeam.ActorID, _deathCount);
    }

    public void UpdateInputs(Vector2 moveInput, Vector2 lookInput, bool isSprinting, bool isJumping, bool isCrouching,
                            bool isAiming, bool isSwitchingShoulder, float zoomDelta, bool isManualAttacking, bool isAutomaticAttacking,
                            bool isSwitchingItem, bool isReloading, bool isDropping,
                            bool isOpeningShopInGame, bool isSelectedItem, bool isBuyingItem,
                            bool isOpeningResultTable, bool isPausing)
    {
        HandlePauseMenu(isPausing);

        if (_lifeState == LifeState.None) return;

        if (_roundActive)
        {
            if (_canAction)
            {
                HandleMovement(moveInput, isSprinting);
                HandleLook(lookInput);
                HandleRotation(moveInput);
                HandleGravityAndJump(isJumping, isCrouching);
                HandleCrouching(isCrouching);
                HandleAiming(isAiming);
                HandleSwitchShoulder(isSwitchingShoulder);
                HandleZoom(zoomDelta);
                HandleAttack(isManualAttacking, isAutomaticAttacking);
                HandleSwitchItem(isSwitchingItem);
                HandleReloading(isReloading);
                HandleDropping(isDropping);
                HandleOpeningShopInGame(isOpeningShopInGame);
                HandleOpeningResultTable(isOpeningResultTable);              
            }
            else
            {
                HandleMovement(moveInput, isSprinting);
                HandleLook(lookInput);
                HandleRotation(moveInput);
                HandleGravityAndJump(isJumping, isCrouching);
                HandleCrouching(isCrouching);
                HandleAiming(isAiming);
                HandleOpeningShopInGame(isOpeningShopInGame);
                HandleSelectedItem(isSelectedItem);
                HandleBuyItem(isBuyingItem);
                HandleOpeningResultTable(isOpeningResultTable);
            }

            Vector3 totalMove = (_moveDirection * _currentSpeed) + _velocity;
            _characterController.Move(totalMove * Time.deltaTime);
        }
        else
        {
            HandleSwitchItem(isSwitchingItem);
            HandleOpeningShopInGame(isOpeningShopInGame);
            HandleSelectedItem(isSelectedItem);
            HandleBuyItem(isBuyingItem);
            HandleOpeningResultTable(isOpeningResultTable);
            HandleReloading(isReloading);
        }
    }


    //------------PRIVATE METHODS------------
    private void GetDataCharacter()
    {
        _characterStats = GameplayDataManager.instance._characterStatsSO;

        foreach (var characterRig in GameplayDataManager.instance._characterRigSO)
        {
            if (_playerTeam.Name == characterRig.nameTeam)
            {
                _characterRig = characterRig;
                break;
            }
        }

        foreach (var characterData in GameplayDataManager.instance._characterDataSO)
        {
            if (_playerTeam.Name == characterData.characterName)
            {
                _characterData = characterData;
            }
        }

        _cameraStats = GameplayDataManager.instance._cameraStatsSO;
    }

    private void HandleMovement(Vector2 input, bool isSprinting)
    {
        bool isMoving = input.magnitude > 0.1f;

        _currentSpeed = isMoving
            ? (isSprinting ? CharacterStats.runSpeed : CharacterStats.walkSpeed)
            : 0f;

        if (_isAiming)
        {
            _forward = transform.forward;
            _right = transform.right;
            _forward.y = 0;
            _right.y = 0;

            _moveDirection = (_forward * input.y + _right * input.x).normalized;
        }
        else
        {
            if (_playerCamera != null)
            {
                _forward = Camera.main.transform.forward;
                _right = Camera.main.transform.right;             
            }
            else
            {
                _forward = transform.forward;
                _right = transform.right;
            }
            _forward.y = 0;
            _right.y = 0;

            _moveDirection = (_forward * input.y + _right * input.x).normalized;
        }

        if (isSprinting) _movementState = MovementState.Run;
        else if (isMoving) _movementState = MovementState.Walk;
        else _movementState = MovementState.Idle;

        _playerAnimator.UpdateMovementState(input, _movementState, _combatState);
    }

    private void HandleLook(Vector2 input)
    {
        if (_isAiming)
        {
            _playerCamera.UpdateCamera(input);
        }
    }

    private void HandleRotation(Vector2 input)
    {
        Vector3 targetDirection = Vector3.zero;

        if (_isAiming)
        {
            targetDirection = Camera.main.transform.forward.normalized;
            targetDirection.y = 0;
        }
        else if (input.sqrMagnitude > 0.1f)
        {
            targetDirection = _moveDirection;
        }

        if (targetDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                CharacterStats.rotationSpeed * Time.deltaTime
            );
        }
    }

    private void HandleGravityAndJump(bool isJumping, bool isCrouching)
    {
        if (_characterController.isGrounded)
        {
            if (isJumping)
            {
                if (isCrouching || _isCrouching) return;

                _velocity.y = 0f;
                _velocity.y += CharacterStats.jumpForce;
                _movementState = (_currentSpeed > 0f) ? MovementState.JumpOM : MovementState.JumpOI;
                _playerAnimator.UpdateJumping(_movementState);
            }
            else
            {
                _velocity.y = -2f;
            }
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
            _stanceState = StanceState.Crouch;
            _playerAnimator.UpdateStanceState(_stanceState);
            _playerRig.UpdateBodyOffset(_itemType, _stanceState);
        }          
        else if (isCrouching && _isCrouching)
        {
            _isCrouching = false;
            _stanceState = StanceState.Stand;
            _playerAnimator.UpdateStanceState(_stanceState);
            _playerRig.UpdateBodyOffset(_itemType, _stanceState);
        }           
    }

    private void HandleAiming(bool isAiming)
    {
        if (isAiming && !_isAiming)
        {
            _playerAudio.PlayCharacterSound(CharacterSoundType.Zoom);
            _isAiming = true;
            _combatState = CombatState.Aim;
            _playerAnimator.UpdateAiming(true);
            _playerInventory.ActiveCombatItem(_combatState);
            _playerRig.UpdateAimRigWeight(true);
            _playerCamera.EnterAimMode();
        }
        else if (isAiming && _isAiming)
        {
            _playerAudio.PlayCharacterSound(CharacterSoundType.Zoom);
            _isAiming = false;
            _combatState = CombatState.None;
            _playerAnimator.UpdateAiming(false);
            _playerInventory.ActiveCombatItem(_combatState);
            _playerRig.UpdateAimRigWeight(false);
            _playerCamera.ExitAimMode();
        }       
    }

    private void HandleSwitchShoulder(bool isSwitchingShoulder)
    {
        if (!isSwitchingShoulder) return;
        if (_combatState != CombatState.Aim) return;
        _playerCamera.SwitchShoulder();
    }

    private void HandleZoom(float zoomDelta)
    {
        if (_combatState != CombatState.Aim) return;
        if (Mathf.Abs(zoomDelta) < 0.01f) return;
        _playerInventory.UpdateHandleZoom(zoomDelta);
    }

    private void HandleAttack(bool isManualAttacking, bool isAutomaticAttacking)
    {
        if (!_isAiming || !_canAction) return;

        if (isManualAttacking)
        {
            switch (_itemType)
            {
                case ItemType.PrimaryItem:
                case ItemType.SecondaryItem:
                    _actionState = ActionState.ManualShoot;
                    _playerInventory.HandleShoot();
                    _actionState = ActionState.None;
                    break;
                case ItemType.MeleeItem:
                    _actionState = ActionState.Melee;
                    _playerAnimator.UpdateActionState(_actionState, _stanceState);
                    _canAction = false;
                    break;
                case ItemType.ThrowItem:
                    _actionState = ActionState.Throw;
                    _playerAnimator.UpdateActionState(_actionState, _stanceState);
                    _canAction = false;
                    break;
            }          
        }
        else if (isAutomaticAttacking)
        {
            if (_itemType == ItemType.PrimaryItem)
            {
                _actionState = ActionState.AutomaticShoot;
                _playerInventory.HandleShoot();
            }         
        }
        else
        {
            _actionState = ActionState.None;
        }
    }

    private void HandleDeath()
    {
        _isAiming = false;
        _playerRig.UpdateAimRigWeight(false);
        if (_playerCamera != null) _playerCamera.ExitAimMode();
        _velocity = Vector3.zero;
        _moveDirection = Vector3.zero;
        _currentSpeed = 0;

        if (TryGetComponent<BehaviorGraphAgent>(out var behaviorAgent)) behaviorAgent.enabled = false;
        if (TryGetComponent<NavMeshAgent>(out var navAgent)) navAgent.enabled = false;
        if (TryGetComponent<BotAIController>(out var botController)) botController.enabled = false;
        if (TryGetComponent<RangeDetector>(out var rangeDetector)) rangeDetector.enabled = false;
        if (TryGetComponent<LineOfSightDetector>(out var lineOfSightDetector)) lineOfSightDetector.enabled = false;

        _combatState = CombatState.None;
        _actionState = ActionState.None;
        _itemType = ItemType.PrimaryItem;
        _playerInventory.ActiveCombatItem(_combatState);
        _playerInventory.UpdateItem(_itemType);
        _playerInventory.DropCurrentItem();

        _itemType = ItemType.None;
        _playerAnimator.UpdateItemType(_itemType);  
        _playerInventory.UpdateItem(_itemType);
        _playerRig.UpdateRigWeight(_itemType);
        _playerRig.UpdateBodyOffset(_itemType, _stanceState);
        _playerAnimator.UpdateDeathState(_lifeState);
        _playerAudio.PlayCharacterSound(CharacterSoundType.Death);

        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach (var col in allColliders)
        {
            col.enabled = false;
        }       
    }

    private void HandleLifeState()
    {
        switch (_lifeState)
        {
            case LifeState.Hurt:
                _canAction = false;
                _playerAnimator.UpdateHurt();
                _playerAudio.PlayCharacterSound(CharacterSoundType.Hurt);
                _lifeState = LifeState.Alive;
                break;
            case LifeState.DeathShoot:
            case LifeState.DeathMelee:
            case LifeState.DeathThrow:
                HandleDeath();
                IncrementDeadCount();
                _lifeState = LifeState.None;
                break;
            case LifeState.None:
                if (GetComponent<BotAIController>() == null)
                {
                    switch (GameplayDataManager.instance.gameMode)
                    {
                        case GameMode.TeamDeathmatch:
                            GameManager_TeamDeathmatch.instance.OnPlayerDeath();
                            break;
                        case GameMode.ZombieSurvival:
                            GameManager_ZombieSurvival.instance.OnPlayerDeath();
                            break;
                    }
                }
                break;
        }
    }

    private void HandlePauseMenu(bool isPausing)
    {
        if (_isOpeningShopInGame || _isOpeningResultTable) return;

        if (isPausing)
        {
            if (GameManager_TeamDeathmatch.instance != null)
                GameManager_TeamDeathmatch.instance.PauseMenu(true);
            if (GameManager_ZombieSurvival.instance != null)
                GameManager_ZombieSurvival.instance.PauseMenu(true);
        }
    }

    private void HandleSwitchItem(bool isSwitchingItem)
    {
        if (_isOpeningShopInGame) return;

        if (!isSwitchingItem) return;

        var keyboard = Keyboard.current;

        if (keyboard.digit1Key.wasPressedThisFrame) _itemType = ItemType.PrimaryItem;         
        else if (keyboard.digit2Key.wasPressedThisFrame) _itemType = ItemType.SecondaryItem;        
        else if (keyboard.digit3Key.wasPressedThisFrame) _itemType = ItemType.MeleeItem;        
        else if (keyboard.digit4Key.wasPressedThisFrame) _itemType = ItemType.ThrowItem;

        SwitchItem();
    }

    private void HandleReloading(bool isReloading)
    {
        if (isReloading)
        {
            if (_itemType == ItemType.PrimaryItem || _itemType == ItemType.SecondaryItem)
            {
                if (!_playerInventory.HasWeapon()) return;
                _playerAudio.PlayCharacterSound(CharacterSoundType.Zoom);
                _isAiming = false;
                _combatState = CombatState.None;
                _playerAnimator.UpdateAiming(false);
                _playerInventory.ActiveCombatItem(_combatState);
                _playerRig.UpdateAimRigWeight(false);
                _playerCamera.ExitAimMode();
                _actionState = ActionState.Reload;
                _playerAnimator.UpdateActionState(_actionState, _stanceState);
                _canAction = false;
            }
        }
    }

    private void HandleDropping(bool isDropping)
    {
        if (!isDropping) return;

        _playerInventory.DropCurrentItem();
    }

    private void HandleOpeningResultTable(bool isOpeningResultTable)
    {
        if (!isOpeningResultTable) return;
        if (UIGameManager_ZombieSurvival.instance != null) return;

        if (isOpeningResultTable && !_isOpeningResultTable)
        {
            _isOpeningResultTable = true;
            UIGameManager_TeamDeathmatch.instance.OpenResultMenu(true);
        }
        else if (isOpeningResultTable && _isOpeningResultTable)
        {
            _isOpeningResultTable = false;
            UIGameManager_TeamDeathmatch.instance.OpenResultMenu(false);
        }
    }

    private void HandleOpeningShopInGame(bool isOpeningShopInGame)
    {
        if (isOpeningShopInGame && !_isOpeningShopInGame)
        {
            _canAction = false;
            _isOpeningShopInGame = true;

            if (UIGameManager_TeamDeathmatch.instance != null)
                UIGameManager_TeamDeathmatch.instance.OpenShopInGame(true);
            if (UIGameManager_ZombieSurvival.instance != null)
                UIGameManager_ZombieSurvival.instance.OpenShopInGame(true);

            UIShopInGame.instance.OnEnableTable(_currentCash);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else if (isOpeningShopInGame && _isOpeningShopInGame)
        {
            _canAction = true;
            _isOpeningShopInGame = false;

            if (UIGameManager_TeamDeathmatch.instance != null)
                UIGameManager_TeamDeathmatch.instance.OpenShopInGame(false);
            if (UIGameManager_ZombieSurvival.instance != null)
                UIGameManager_ZombieSurvival.instance.OpenShopInGame(false);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void HandleSelectedItem(bool isSelectedItem)
    {
        if (!isSelectedItem || !_isOpeningShopInGame) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            UIShopInGame.instance.OnEscape();
            return;
        }

        int index = GetNumberKeyPressed();
        if (index < 0) return;

        UIShopInGame.instance.OnNumberInput(index);
        _isSelectedItem = true;
    }

    private void HandleBuyItem(bool isBuyingItem)
    {
        if (!isBuyingItem) return;

        if (_isSelectedItem)
        {
            UIShopInGame.instance.OnClickBuy();
            _playerInventory.UpdateItem(_itemType);
            _playerRig.UpdateRigWeight(_itemType);
            _playerRig.UpdateBodyOffset(_itemType, _stanceState);                  
        }

        HandleOpeningShopInGame(true);
    }

    private int GetNumberKeyPressed()
    {
        var k = Keyboard.current;
        if (k.digit1Key.wasPressedThisFrame) return 0;
        if (k.digit2Key.wasPressedThisFrame) return 1;
        if (k.digit3Key.wasPressedThisFrame) return 2;
        if (k.digit4Key.wasPressedThisFrame) return 3;
        if (k.digit5Key.wasPressedThisFrame) return 4;
        if (k.digit6Key.wasPressedThisFrame) return 5;
        if (k.digit7Key.wasPressedThisFrame) return 6;
        if (k.digit8Key.wasPressedThisFrame) return 7;
        if (k.digit9Key.wasPressedThisFrame) return 8;
        return -1;
    }
}
