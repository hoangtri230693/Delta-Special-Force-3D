using System.Collections.Generic;
using System.IO;
using Unity.Behavior;
using UnityEngine;

public enum GameState { Setup, Countdown, RoundActive, RoundEnd, MatchEnd }
public enum GameResult { Win, Lose, Draw }


public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private GameplayData _gameplayData;
    [SerializeField] private GameObject[] _characterPrefabs;
    [SerializeField] private GameObject[] _botAIPrefabs;
    [SerializeField] private Transform[] _spawnCounter;
    [SerializeField] private Transform[] _spawnTerrorist;
    [SerializeField] private Transform[] _assaultCounter;
    [SerializeField] private Transform[] _patrolTerrorist;
    [SerializeField] private List<GameObject> _allCounterCharacter;
    [SerializeField] private List<GameObject> _allTerroristCharacter;

    public PlayerController _playerController;
    public PlayerInventory _playerInventory;
    public PlayerHealth _playerHealth;
    public PlayerTeam _playerTeam;
    public PlayerAnimationEvents _playerAnimationEvents;

    public GameObject _player;
    public TeamType _teamType;
    public int _cTSpawn = 5;
    public int _terroristSpawn = 5;
    public GameState _currentGameState = GameState.Setup;

    public int _teamCTCount = 0;
    public int _teamTerroristCount = 0;
    public int _teamCTWin = 0;
    public int _teamTerroristWin = 0;
    public GameResult _playerResult = GameResult.Draw;

    public float _timeCount;
    public int _currentRound;


    private void Awake()
    {
        instance = this;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        _currentGameState = GameState.Countdown;
        _timeCount = _gameplayData.timeCountdown;
        _currentRound = 0;
        _allCounterCharacter = new List<GameObject>();
        _allTerroristCharacter = new List<GameObject>();
    }

    private void Start()
    {
        SpawnTeams();
        UIGameManager.instance.UpdateUIResult();
        UIGameManager.instance.UpdateUICash(_playerController._currentCash);
    }

    private void Update()
    {
        UpdateRound();
        UpdateMatch();
        UpdateTime();
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
                GameObject weaponPrefab = Instantiate(WeaponDataManager.instance.weaponStats[weaponIndex].weaponPrefab);
                weaponPrefab.transform.SetParent(inventory);
                playerController._actionState = ActionState.SwitchItem;
            }

            playerController._currentCash -= WeaponDataManager.instance.weaponStats[weaponIndex].cash;

            if (playerController == _playerController)
            {
                UIGameManager.instance.UpdateUICash(playerController._currentCash);
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
            int selectedCharacterIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);
            _teamType = _characterPrefabs[selectedCharacterIndex].GetComponent<PlayerTeam>()._playerTeam;

            Transform pSpawn = (_teamType == TeamType.CounterTerrorist) ? _spawnCounter[0] : _spawnTerrorist[0];
            _player = Instantiate(_characterPrefabs[selectedCharacterIndex], pSpawn.position, pSpawn.rotation);
            _player.AddComponent<PlayerLocal>();
            if (_teamType == TeamType.CounterTerrorist) _teamCTCount++;
            else _teamTerroristCount++;

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
            int indexCharacter = Random.Range(0, 2);
            GameObject bot = Instantiate(_botAIPrefabs[indexCharacter], spawn.position, spawn.rotation);

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
            _allCounterCharacter.Add(bot);
            _teamCTCount++;
        }
    }

    private void SpawnTerroristBots(int count, int startIndex)
    {
        ShuffleTransform(_patrolTerrorist);

        for (int i = 0; i < count; i++)
        {
            Transform spawn = _spawnTerrorist[i + startIndex];
            int indexCharacter = Random.Range(3, 6);
            GameObject bot = Instantiate(_botAIPrefabs[indexCharacter], spawn.position, spawn.rotation);

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
            _allTerroristCharacter.Add(bot);
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

    private void UpdateMatch()
    {
        if (_currentGameState == GameState.RoundEnd)
        {
            if (_timeCount <= 0)
            {
                if (_currentRound < _gameplayData.totalRound)
                {
                    UIGameManager.instance.OpenResultMenu(false);
                    PrepareNextRound();
                    _currentRound++;
                    _currentGameState = GameState.Countdown;
                    _timeCount = _gameplayData.timeCountdown;
                }
                else
                {
                    _currentGameState = GameState.MatchEnd;
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

        foreach (GameObject bot in _allCounterCharacter)
        {
            Destroy(bot);
        }
        _allCounterCharacter.Clear();

        foreach (GameObject bot in _allTerroristCharacter)
        {
            Destroy(bot);
        }
        _allTerroristCharacter.Clear();
    }   
}