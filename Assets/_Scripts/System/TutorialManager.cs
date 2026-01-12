using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    [SerializeField] private GameplayData _gameplayData;
    [SerializeField] private WeaponStatsSO[] _weaponStats;

    public PlayerController _playerController;
    public PlayerInventory _playerInventory;
    public PlayerHealth _playerHealth;
    public PlayerTeam _playerTeam;
    public PlayerAnimationEvents _playerAnimationEvents;

    public GameObject _player;
    public TeamType _teamType;
    public GameState _currentGameState = GameState.Setup;
    public GameResult _playerResult = GameResult.Draw;

    public int _teamCTCount = 1;
    public int _teamTerroristCount = 5;
    public int _teamCTWin = 0;
    public int _teamTerroristWin = 0;
    public float _timeCount;


    private void Awake()
    {
        instance = this;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        _currentGameState = GameState.Countdown;
        _timeCount = _gameplayData.timeCountdown;
    }

    private void Start()
    {
        UIGameManager.instance.UpdateUIResult();
    }

    private void Update()
    {
        UpdateRound();
        UpdateTime();
    }

    public void BuyWeapon(int weaponIndex, PlayerController playerController, PlayerInventory playerInventory, PlayerHealth playerHealth)
    {
        if (playerController._currentCash < _weaponStats[weaponIndex].cash) return;

        if (weaponIndex >= 0 && weaponIndex < _weaponStats.Length)
        {
            Transform inventory = null;

            if (_weaponStats[weaponIndex].itemType == ItemType.PrimaryItem)
            {
                inventory = playerInventory._primaryItem.transform;
                playerController._currentItem = ItemType.PrimaryItem;
            }
            else if (_weaponStats[weaponIndex].itemType == ItemType.SecondaryItem)
            {
                inventory = playerInventory._secondaryItem.transform;
                playerController._currentItem = ItemType.SecondaryItem;
            }
            else if (_weaponStats[weaponIndex].itemType == ItemType.ThrowItem)
            {
                inventory = playerInventory._throwItem.transform;
                playerController._currentItem = ItemType.ThrowItem;
            }
            else if (_weaponStats[weaponIndex].itemType == ItemType.ArmorItem)
            {
                playerHealth._currentArmorHealth = _weaponStats[weaponIndex].armorHealth;
                if (playerHealth == _playerHealth)
                {
                    UIGameManager.instance.UpdateUIArmorHealth(playerHealth._currentArmorHealth, playerHealth);
                }
            }

            if (inventory != null && inventory.childCount > 0)
            {
                for (int i = 0; i < inventory.childCount; i++)
                {
                    WeaponManager weaponManager = inventory.GetChild(i).GetComponent<WeaponManager>();
                    if (weaponManager != null)
                    {
                        weaponManager.DropWeapon(inventory.GetChild(i));
                    }
                }
            }

            if (inventory != null)
            {
                GameObject weaponPrefab = Instantiate(_weaponStats[weaponIndex].weaponPrefab);
                weaponPrefab.transform.SetParent(inventory);
                playerController._actionState = ActionState.SwitchItem;
            }

            playerController._currentCash -= _weaponStats[weaponIndex].cash;

            if (playerController == _playerController)
            {
                UIGameManager.instance.UpdateUICash(playerController._currentCash);
            }
        }
    }

    private void UpdateTime()
    {
        if (_timeCount > 0)
        {
            _timeCount -= Time.deltaTime;
            _timeCount = Mathf.Clamp(_timeCount, 0, Mathf.Infinity);
            if (_currentGameState == GameState.RoundActive || _currentGameState == GameState.Countdown)
            {
                UIGameManager.instance.UpdateUITime(_timeCount);
            }
        }
    }

    private void UpdateRound()
    {
        if (_currentGameState == GameState.Countdown)
        {
            if (_timeCount <= 0)
            {
                _currentGameState = GameState.RoundActive;
                _timeCount = _gameplayData.timeRoundActive;
            }
        }
        else if (_currentGameState == GameState.RoundActive)
        {
            if (_timeCount <= 0 || _teamCTCount <= 0 || _teamTerroristCount <= 0)
            {
                _currentGameState = GameState.RoundEnd;
                _timeCount = 5f;
                _playerController.OnCharacterController(false);

                UIGameManager.instance.UpdateUIResult();
                UIGameManager.instance.OpenResultMenu(true);
            }
        }
    }
}
