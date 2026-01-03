using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

public enum GameState { Setup, Countdown, RoundActive, RoundEnd, MatchEnd }
public enum GameResult { Win, Lose, Draw }

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private GameObject[] _characterPrefabs;
    [SerializeField] private GameObject[] _botAIPrefabs;
    [SerializeField] private WeaponStatsSO[] _weaponStats;
    [SerializeField] private Transform[] _spawnCounter;
    [SerializeField] private Transform[] _spawnTerrorist;
    [SerializeField] private Transform[] _assaultCounter;
    [SerializeField] private Transform[] _patrolTerrorist;

    public PlayerController _playerController;
    public PlayerInventory _playerInventory;
    public PlayerHealth _playerHealth;
    
    public GameObject _player;
    public TeamType _playerTeam;
    public int _cTSpawn = 5;
    public int _terroristSpawn = 5;
    public GameState _currentGameState = GameState.Setup;

    private int _teamCTCount = 0;
    private int _teamTerroristCount = 0;
    private int _teamCTWin = 0;
    private int _teamTerroristWin = 0;
    private GameResult _playerResult = GameResult.Win;


    private void Awake()
    {
        instance = this;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        _currentGameState = GameState.Countdown;
    }

    private void Start()
    {
        SpawnTeams();    
    }

    private void Update()
    {
        UpdateRound();
        UpdateMatch();
        UpdateResult();
    }

    private void SpawnTeams()
    {
        ShuffleTransform(_spawnCounter);
        ShuffleTransform(_spawnTerrorist);

        int selectedCharacterIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);
        _playerTeam = _characterPrefabs[selectedCharacterIndex].GetComponent<PlayerTeam>()._team;

        Transform pSpawn = (_playerTeam == TeamType.CounterTerrorist) ? _spawnCounter[0] : _spawnTerrorist[0];
        _player = Instantiate(_characterPrefabs[selectedCharacterIndex], pSpawn.position, pSpawn.rotation);
        _player.AddComponent<PlayerLocal>();
        _teamCTCount++;

        _playerController = _player.GetComponent<PlayerController>();
        _playerInventory = _player.GetComponent<PlayerInventory>();
        _playerHealth = _player.GetComponent<PlayerHealth>();

        if (_playerTeam == TeamType.CounterTerrorist)
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

            if (_assaultCounter.Length > 0)
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
            }
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

            if (_patrolTerrorist.Length > 0)
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
            }
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

    private void UpdateResult()
    {
        if ((_teamCTCount <= 0 || _teamTerroristCount <= 0) && _currentGameState == GameState.RoundActive)
        {
            if (_teamCTCount <= 0)
            {
                _teamTerroristWin++;
            }
            if (_teamTerroristCount <= 0)
            {
                _teamCTWin++;
            }
            _currentGameState = GameState.RoundEnd;
        }

        if (_currentGameState == GameState.MatchEnd)
        {
            if (_teamCTWin > _teamTerroristWin && _playerTeam == TeamType.CounterTerrorist)
            {
                _playerResult = GameResult.Win;
            }
            else if (_teamTerroristWin > _teamCTWin && _playerTeam == TeamType.Terrorist)
            {
                _playerResult = GameResult.Win;
            }
            else if (_teamCTWin == _teamTerroristWin)
            {
                _playerResult = GameResult.Draw;
            }
            else
            {
                _playerResult = GameResult.Lose;
            }
        }
    }

    private void UpdateRound()
    {
        if (_currentGameState == GameState.Countdown && UIGameManager.instance._timeCount > 0)
            return;

        if (_currentGameState == GameState.Countdown && UIGameManager.instance._timeCount <= 0)
        {
            _currentGameState = GameState.RoundActive;
        }
        else if (_currentGameState == GameState.RoundActive && UIGameManager.instance._timeCount <= 0)
        {
            _currentGameState = GameState.RoundEnd;
        }
    }

    private void UpdateMatch()
    {
        if (_currentGameState == GameState.RoundEnd && !UIGameManager.instance._isMatchEnd)
        {
            _currentGameState = GameState.Countdown;
        }
        else if (_currentGameState == GameState.RoundEnd && UIGameManager.instance._isMatchEnd)
        {
            _currentGameState = GameState.MatchEnd;
        }
    }

    public void BuyWeapon(int weaponIndex, PlayerController playerController, PlayerInventory playerInventory)
    {
        if (playerController._currentCash < _weaponStats[weaponIndex].price) return;

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

            if (inventory.childCount > 0)
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

            playerController._currentCash -= _weaponStats[weaponIndex].price;
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
}
