using System;
using UnityEngine;


public class GameManager_ZombieSurvival : MonoBehaviour
{
    public static GameManager_ZombieSurvival instance;

    [Header("Gameplay UI")]
    [SerializeField] private float _timeCountdown;
    [SerializeField] private float _timeRoundActive;
    [SerializeField] private int _totalRound;

    [Header("Gameplay Data")]
    [SerializeField] private GameObject[] _counterPrefabs;
    [SerializeField] private GameObject[] _terroristPrefabs;
    [SerializeField] private GameObject _zombiePrefabs;
    [SerializeField] private Transform _spawnPoint;

    [Header("Zombie Settings")]
    [SerializeField] private int _zombiesPerWave = 10;
    [SerializeField] private float _distanceBetweenWaves = 50f;
    [SerializeField] private float _initialDistanceFromPlayer = 30f;

    [Header("Player Component References")]
    public PlayerController _playerController;
    public PlayerInventory _playerInventory;
    public PlayerHealth _playerHealth;
    public PlayerTeam _playerTeam;
    public PlayerAnimationEvents _playerAnimationEvents;

    [Header("Game Manager")]
    public GameObject _player;
    public GameState _currentGameState = GameState.Setup;

    private float _timeCount;
    private int _currentRound;
    private bool _isMatchEnded = false;
    public int _playerKilled = 0;

    private Vector3 _baseSpawnDirection;
    private Vector3 _initialSpawnPoint;
    private int _spawnWaveCount = 0;


    private void Awake()
    {
        instance = this;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        _currentGameState = GameState.Countdown;
        _timeCount = _timeCountdown;
        _currentRound = 1;
    }

    private void Start()
    {
        SpawnTeams();
        SpawnZombie();
        UIGameManager_ZombieSurvival.instance.UpdateUICash(_playerController._currentCash);
    }

    private void Update()
    {
        UpdateRound();
        UpdateMatch();
        UpdateTime();
    }

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
                    UIGameManager_ZombieSurvival.instance.UpdateUIArmorHealth(playerHealth._currentArmorHealth, playerHealth);
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
                UIGameManager_ZombieSurvival.instance.UpdateUICash(playerController._currentCash);
            }
        }
    }

    private void SpawnTeams()
    {
        int selectedTeamID = PlayerPrefs.GetInt("SelectedTeamID", 0);
        int selectedCharacterID = PlayerPrefs.GetInt("SelectedCharacterID", 0);

        if (selectedTeamID == 0)
        {
            _player = Instantiate(_counterPrefabs[selectedCharacterID], _spawnPoint.position, _spawnPoint.rotation);
        }
        if (selectedTeamID == 1)
        {
            _player = Instantiate(_terroristPrefabs[selectedCharacterID], _spawnPoint.position, _spawnPoint.rotation);                                             
        }

        _player.AddComponent<PlayerLocal>();

        _playerController = _player.GetComponent<PlayerController>();
        _playerInventory = _player.GetComponent<PlayerInventory>();
        _playerHealth = _player.GetComponent<PlayerHealth>();
        _playerAnimationEvents = _player.GetComponent<PlayerAnimationEvents>();
        _playerTeam = _player.GetComponent<PlayerTeam>();
        _playerTeam._playerID = 0;

        MiniMap.instance.SetupPlayerTransform(_player.transform);
    }

    private void SpawnZombie()
    {
        if (_spawnWaveCount == 0)
        {
            _baseSpawnDirection = _spawnPoint.transform.forward;
            _initialSpawnPoint = _spawnPoint.transform.position + (_baseSpawnDirection * _initialDistanceFromPlayer);
        }

        Vector3 waveSpawnPosition = _initialSpawnPoint + (_baseSpawnDirection * _distanceBetweenWaves * _spawnWaveCount);
        waveSpawnPosition.y = _spawnPoint.position.y;

        for (int i = 0; i <_zombiesPerWave; i++)
        {
            Vector3 randomOffset = new Vector3(UnityEngine.Random.Range(-10f, 10f), 0, UnityEngine.Random.Range(-10f, 10f));
            Instantiate(_zombiePrefabs, waveSpawnPosition + randomOffset, Quaternion.identity);
        }

        _spawnWaveCount++;
    }

    private void UpdateTime()
    {
        if (_timeCount > 0)
        {
            _timeCount -= Time.deltaTime;
            _timeCount = Mathf.Clamp(_timeCount, 0, Mathf.Infinity);
            if (_currentGameState == GameState.RoundActive || _currentGameState == GameState.Countdown)
            {
                UIGameManager_ZombieSurvival.instance.UpdateUITime(_timeCount);
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
                        UIGameManager_ZombieSurvival.instance.ShowUIResultMatch();
                    }
                }
            }
        }
    }

    private void CalculateMatchRewards()
    {
        string resultMatch = "WIN";
        int bonusGoldPerKill = GameplayDataManager.instance.GetBonusGoldPerKill();
        int rewardKills = _playerKilled * GameplayDataManager.instance.GetBonusGoldPerKill();

        int rewardMatch = GameplayDataManager.instance.GetBonusGoldByMatchResult(resultMatch);
        int totalReward = rewardMatch + rewardKills;
        PlayerDataManager.instance.AddPlayerGold(totalReward);
    }
}
