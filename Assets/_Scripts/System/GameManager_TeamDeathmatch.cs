using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

public enum GameState { Setup, Countdown, RoundActive, RoundEnd, MatchEnd }


public class GameManager_TeamDeathmatch : MonoBehaviour
{
    public static GameManager_TeamDeathmatch instance;

    [Header("Gameplay UI")]
    [SerializeField] private float _timeCountdown;
    [SerializeField] private float _timeRoundActive;
    [SerializeField] private int _totalRound;

    [Header("Gameplay Data")]
    [SerializeField] private GameObject[] _counterPrefabs;
    [SerializeField] private GameObject[] _terroristPrefabs;
    [SerializeField] private GameObject[] _counterAIPrefabs;
    [SerializeField] private GameObject[] _terroristAIPrefabs;
    [SerializeField] private Transform[] _spawnCounter;
    [SerializeField] private Transform[] _spawnTerrorist;
    [SerializeField] private Transform[] _assaultCounter;
    [SerializeField] private Transform[] _patrolTerrorist;
    [SerializeField] private List<GameObject> _allBotCharacter;

    [Header("Player Component References")]
    public PlayerController _playerController;
    public PlayerInventory _playerInventory;
    public PlayerHealth _playerHealth;
    public PlayerTeam _playerTeam;
    public PlayerAnimationEvents _playerAnimationEvents;

    [Header("Game Manager")]
    public GameObject _player;
    public TeamType _teamType;
    public int _cTSpawn = 5;
    public int _terroristSpawn = 5;
    public GameState _currentGameState = GameState.Setup;

    public int _teamCTCount = 0;
    public int _teamTerroristCount = 0;
    public int _teamCTWin = 0;
    public int _teamTerroristWin = 0;

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
        _allBotCharacter = new List<GameObject>();
    }

    private void Start()
    {
        SpawnTeams();
        UIGameManager_TeamDeathmatch.instance.UpdateUIResultRound();
        UIGameManager_TeamDeathmatch.instance.UpdateUICash(_playerController._currentCash);
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

    public void UpdateTeamCount(TeamType teamType)
    {
        if (teamType == TeamType.CounterTerrorist)
        {
            _teamCTCount--;
        }
        if (teamType == TeamType.Terrorist)
        {
            _teamTerroristCount--;
        }
    }

    private void SpawnTeams()
    {
        ShuffleTransform(_spawnCounter);
        ShuffleTransform(_spawnTerrorist);

        if (_player == null)
        {
            int selectedTeamID = PlayerPrefs.GetInt("SelectedTeamID", 0);
            int selectedCharacterID = PlayerPrefs.GetInt("SelectedCharacterID", 0);

            Transform pSpawn;
            if (selectedTeamID == 0)
            {
                _teamType = TeamType.CounterTerrorist;
                pSpawn = _spawnCounter[0];
                _player = Instantiate(_counterPrefabs[selectedCharacterID], pSpawn.position, pSpawn.rotation);
                _teamCTCount++;
            }             
            if (selectedTeamID == 1)
            {
                _teamType = TeamType.Terrorist;
                pSpawn = _spawnTerrorist[0];
                _player = Instantiate(_terroristPrefabs[selectedCharacterID], pSpawn.position, pSpawn.rotation);
                _teamTerroristCount++;
            }
                
            _player.AddComponent<PlayerLocal>(); 

            _playerController = _player.GetComponent<PlayerController>();
            _playerInventory = _player.GetComponent<PlayerInventory>();
            _playerHealth = _player.GetComponent<PlayerHealth>();
            _playerAnimationEvents = _player.GetComponent<PlayerAnimationEvents>();
            _playerTeam = _player.GetComponent<PlayerTeam>();
            _playerTeam._playerID = 0;
        }
        else
        {
            Transform pSpawn = (_teamType == TeamType.CounterTerrorist) ? _spawnCounter[0] : _spawnTerrorist[0];
            _player.transform.position = pSpawn.position;
            _player.transform.rotation = pSpawn.rotation;
            if (_teamType == TeamType.CounterTerrorist) _teamCTCount++;
            else _teamTerroristCount++;
            _playerController.OnCharacterController(true);
            _playerController.ResetPlayerState();
            _playerHealth.ResetHealth();
        }

        MiniMap.instance.SetupPlayerTransform(_player.transform);

        if (_teamType == TeamType.CounterTerrorist)
        {
            SpawnCounterBots(_cTSpawn - 1, 1);
            SpawnTerroristBots(_terroristSpawn, 0);
        }
        else
        {
            SpawnTerroristBots(_terroristSpawn - 1, 1);
            SpawnCounterBots(_cTSpawn, 0);
        }
    }

    private void SpawnCounterBots(int count, int startIndex)
    {
        for (int i = 0; i < count; i++)
        {
            Transform spawn = _spawnCounter[i + startIndex];
            int indexCharacter = Random.Range(0, _counterAIPrefabs.Length);
            GameObject bot = Instantiate(_counterAIPrefabs[indexCharacter], spawn.position, spawn.rotation);

            if (_assaultCounter != null && _assaultCounter.Length > 0)
            {
                List<GameObject> pointList = new List<GameObject>();
                foreach (Transform child in _assaultCounter)
                {
                    pointList.Add(child.gameObject);
                }
                if (bot.TryGetComponent<BehaviorGraphAgent>(out var behaviorAgent))
                {
                    behaviorAgent.BlackboardReference.SetVariableValue("AssaultPoints", pointList);
                }
                if (bot.TryGetComponent<PlayerTeam>(out var playerTeam))
                {
                    playerTeam._playerID = i + startIndex;
                }
            }
            _allBotCharacter.Add(bot);
            _teamCTCount++;
        }
    }

    private void SpawnTerroristBots(int count, int startIndex)
    {
        ShuffleTransform(_patrolTerrorist);

        for (int i = 0; i < count; i++)
        {
            Transform spawn = _spawnTerrorist[i + startIndex];
            int indexCharacter = Random.Range(0, _terroristAIPrefabs.Length);
            GameObject bot = Instantiate(_terroristAIPrefabs[indexCharacter], spawn.position, spawn.rotation);

            if (_patrolTerrorist != null && _patrolTerrorist.Length > 0)
            {
                Transform assignedPatrolGroup = _patrolTerrorist[i % _patrolTerrorist.Length];

                List<GameObject> pointList = new List<GameObject>();
                foreach (Transform child in assignedPatrolGroup)
                {
                    pointList.Add(child.gameObject);
                }

                ShuffleList(pointList);

                if (bot.TryGetComponent<BehaviorGraphAgent>(out var behaviorAgent))
                {
                    behaviorAgent.BlackboardReference.SetVariableValue("PatrolPoints", pointList);
                }

                if (bot.TryGetComponent<PlayerTeam>(out var playerTeam))
                {
                    playerTeam._playerID = i + startIndex;
                }
            }
            _allBotCharacter.Add(bot);
            _teamTerroristCount++;
        }
    }

    private void ShuffleTransform(Transform[] transform)
    {
        for (int i = 0; i < transform.Length; i++)
        {
            Transform temp = transform[i];
            int randomIndex = Random.Range(i, transform.Length);
            transform[i] = transform[randomIndex];
            transform[randomIndex] = temp;
        }
    }

    private void ShuffleList(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            GameObject temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
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
            if (_timeCount <= 0 || _teamCTCount <= 0 || _teamTerroristCount <= 0)
            {
                if (_teamCTCount <= 0) _teamTerroristWin++;
                else if (_teamTerroristCount <= 0) _teamCTWin++;

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

    private void PrepareNextRound()
    {
        ClearOldBots();
        SpawnTeams();
    }

    private void ClearOldBots()
    {
        if (_playerHealth._isDead)
        {
            Destroy(_player);
            _player = null;
        }

        foreach (GameObject bot in _allBotCharacter)
        {
            Destroy(bot);
        }
        _allBotCharacter.Clear();
    }

    private void CalculateMatchRewards()
    {
        string resultMatch;
        int bonusGoldPerKill = GameplayDataManager.instance.GetBonusGoldPerKill();
        int rewardKills = _playerKilled * GameplayDataManager.instance.GetBonusGoldPerKill();
        bool isPlayerCT = (_teamType == TeamType.CounterTerrorist);

        if (_teamCTWin > _teamTerroristWin)
        {
            resultMatch = isPlayerCT ? "WIN" : "LOSE";
        }
        else if (_teamTerroristWin > _teamCTWin)
        {
            resultMatch = !isPlayerCT ? "WIN" : "LOSE";
        }
        else
        {
            resultMatch = "DRAW";
        }

        int rewardMatch = GameplayDataManager.instance.GetBonusGoldByMatchResult(resultMatch);
        int totalReward = rewardMatch + rewardKills;
        PlayerDataManager.instance.AddPlayerGold(totalReward);
    }
}