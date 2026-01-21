using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;


public class GameManager_ZombieSurvival : MonoBehaviour
{
    public static GameManager_ZombieSurvival instance;

    [Header("Gameplay UI")]
    [SerializeField] private float _timeCountdown;
    [SerializeField] private float _timeRoundActive;
    [SerializeField] private int _totalRound;

    [Header("Gameplay Data")]
    [SerializeField] private TeamMenu _teamMenu;
    [SerializeField] private Transform _spawnPoint;

    public PlayerController _playerController;
    public PlayerInventory _playerInventory;
    public PlayerHealth _playerHealth;
    public PlayerTeam _playerTeam;
    public PlayerAnimationEvents _playerAnimationEvents;

    public GameObject _player;
    public TeamType _teamType;
    public GameState _currentGameState = GameState.Setup;

    private float _timeCount;
    private int _currentRound;
    private bool _isMatchEnded = false;
    public int _playerKilled = 0;


    private void Awake()
    {
        instance = this;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        _currentGameState = GameState.Countdown;
        _timeCount = _timeCountdown;
        _currentRound = 1;
    }

    private async void Start()
    {
        await StartNewRound();
    }

    private void Update()
    {
        UpdateRound();
        UpdateMatch();
        UpdateTime();
    }

    #region Core Spawn Logic (Optimized with Addressables)

    private async Task StartNewRound()
    {
        await SpawnPlayer();
        UIGameManager_TeamDeathmatch.instance.UpdateUIResultRound();
        UIGameManager_TeamDeathmatch.instance.UpdateUICash(_playerController._currentCash);
    }

    private async Task SpawnPlayer()
    {
        int selectedTeamID = PlayerPrefs.GetInt("SelectedTeamID", 0);
        int selectedCharID = PlayerPrefs.GetInt("SelectedCharacterID", 0);
        _teamType = (selectedTeamID == 0) ? TeamType.CounterTerrorist : TeamType.Terrorist;

        CharacterData pData = GetCharacterData(selectedTeamID, selectedCharID);

        var handle = pData.characterPlayerPrefab.InstantiateAsync(_spawnPoint.position, _spawnPoint.rotation);
        await handle.Task;

        _player = handle.Result;
        _player.AddComponent<PlayerLocal>();

        SetupPlayerComponents(_player);
    }


    #endregion

    #region Helper Functions

    private CharacterData GetCharacterData(int teamID, int charID)
    {
        foreach (var team in _teamMenu._menuTeam)
        {
            if (team.teamID == teamID)
            {
                foreach (var c in team.characterData)
                {
                    if (c.characterID == charID) return c;
                }
            }
        }
        return _teamMenu._menuTeam[0].characterData[0];
    }

    private void SetupPlayerComponents(GameObject playerObj)
    {
        _playerController = _player.GetComponent<PlayerController>();
        _playerInventory = _player.GetComponent<PlayerInventory>();
        _playerHealth = _player.GetComponent<PlayerHealth>();
        _playerAnimationEvents = _player.GetComponent<PlayerAnimationEvents>();
        _playerTeam = _player.GetComponent<PlayerTeam>();
        _playerTeam._playerID = 0;

        MiniMap.instance.SetupPlayerTransform(_player.transform);
        UIGameManager_TeamDeathmatch.instance.UpdateUIPlayerHealth(_playerHealth._currentHealth, _playerHealth);
        UIGameManager_TeamDeathmatch.instance.UpdateUIArmorHealth(_playerHealth._currentArmorHealth, _playerHealth);
    }

    private void ClearOldBots()
    {
        if (_playerHealth != null && _playerHealth._isDead)
        {
            Addressables.ReleaseInstance(_player);
            _player = null;
        }
    }

    private void CalculateMatchRewards()
    {
        string resultMatch = "WIN";
        int bonusGoldPerKill = GameplayDataManager.instance.GetBonusGoldPerKill();
        int rewardKills = _playerKilled * GameplayDataManager.instance.GetBonusGoldPerKill();
        bool isPlayerCT = (_teamType == TeamType.CounterTerrorist);

        int rewardMatch = GameplayDataManager.instance.GetBonusGoldByMatchResult(resultMatch);
        int totalReward = rewardMatch + rewardKills;
        PlayerDataManager.instance.AddPlayerGold(totalReward);
    }

    #endregion

    public void UpdatePlayerKilled(PlayerController player)
    {
        if (player == _playerController)
            _playerKilled++;
    }

    public void BuyWeapon(int weaponIndex, PlayerController playerController, PlayerInventory playerInventory, PlayerHealth playerHealth)
    {
        if (playerController._currentCash < WeaponDataManager.instance.weaponStats[weaponIndex].cash) return;

        if (weaponIndex >= 0 && weaponIndex < WeaponDataManager.instance.weaponStats.Length)
        {
            Transform inventory = null;

            if (WeaponDataManager.instance.weaponStats[weaponIndex].itemType == ItemType.PrimaryItem)
            {
                inventory = playerInventory._primaryItem.transform;
                playerController._currentItem = ItemType.PrimaryItem;
            }
            else if (WeaponDataManager.instance.weaponStats[weaponIndex].itemType == ItemType.SecondaryItem)
            {
                inventory = playerInventory._secondaryItem.transform;
                playerController._currentItem = ItemType.SecondaryItem;
            }
            else if (WeaponDataManager.instance.weaponStats[weaponIndex].itemType == ItemType.ThrowItem)
            {
                inventory = playerInventory._throwItem.transform;
                playerController._currentItem = ItemType.ThrowItem;
            }
            else if (WeaponDataManager.instance.weaponStats[weaponIndex].itemType == ItemType.ArmorItem)
            {
                playerHealth._currentArmorHealth = WeaponDataManager.instance.weaponStats[weaponIndex].armorHealth;
                if (playerHealth == _playerHealth)
                {
                    UIGameManager_TeamDeathmatch.instance.UpdateUIArmorHealth(playerHealth._currentArmorHealth, playerHealth);
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
                GameObject weaponPrefab = Instantiate(WeaponDataManager.instance.weaponStats[weaponIndex].weaponPrefab);
                weaponPrefab.transform.SetParent(inventory);
                playerController._actionState = ActionState.SwitchItem;
            }

            playerController._currentCash -= WeaponDataManager.instance.weaponStats[weaponIndex].cash;

            if (playerController == _playerController)
            {
                UIGameManager_TeamDeathmatch.instance.UpdateUICash(playerController._currentCash);
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
                UIGameManager_TeamDeathmatch.instance.UpdateUITime(_timeCount);
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
                _timeCount = _timeRoundActive;
            }
        }
        else if (_currentGameState == GameState.RoundActive)
        {
            if (_timeCount <= 0)
            {
                _currentGameState = GameState.RoundEnd;
                _timeCount = 5f;
                _playerController.OnCharacterController(false);

                UIGameManager_TeamDeathmatch.instance.UpdateUIResultRound();
                UIGameManager_TeamDeathmatch.instance.OpenResultMenu(true);
            }
        }
    }

    private void UpdateMatch()
    {
        if (_currentGameState == GameState.RoundEnd)
        {
            if (_timeCount <= 0)
            {
                if (_currentRound < _totalRound)
                {
                    UIGameManager_TeamDeathmatch.instance.OpenResultMenu(false);
                    PrepareNextRound();
                    _currentRound++;
                    _currentGameState = GameState.Countdown;
                    _timeCount = _timeCountdown;
                }
                else
                {
                    if (!_isMatchEnded)
                    {
                        _isMatchEnded = true;
                        _currentGameState = GameState.MatchEnd;
                        CalculateMatchRewards();
                        UIGameManager_TeamDeathmatch.instance.ShowUIResultMatch();
                    }
                }
            }
        }
    }

    private async void PrepareNextRound()
    {
        ClearOldBots();
        await StartNewRound();
    }
}
