using Unity.Behavior;
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
    [SerializeField] private GameObject[] _zombiePrefabs;
    [SerializeField] private Transform _spawnPoint;

    [Header("Zombie Settings")]
    [SerializeField] private int _zombiesPerWave = 10;
    [SerializeField] private int _incrementZombiesPerWave = 10;
    [SerializeField] private float _distanceBetweenWaves = 30f;
    [SerializeField] private float _initialDistanceFromPlayer = 60f;

    [Header("Player Component References")]
    public PlayerController _playerController;
    public PlayerInventory _playerInventory;
    public PlayerHealth _playerHealth;
    public PlayerTeam _playerTeam;
    public PlayerAnimationEvents _playerAnimationEvents;

    [Header("Game Manager")]
    public GameObject _player;
    public GameState _currentGameState = GameState.Setup;
    public int _playerKilled = 0;

    private float _timeCount;
    private int _currentRound;
    private bool _isMatchEnded = false;
    
    private Vector3 _baseSpawnDirection;
    private Vector3 _initialSpawnPoint;
    private int _spawnWaveCount = 0;
    private int _zombieWaveCount = 0;


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
    }

    private void Update()
    {
        UpdateRound();
        UpdateMatch();
        UpdateTime();
    }

    public void PauseMenu(bool isOpen)
    {
        if (isOpen)
        {
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            UIGameManager_ZombieSurvival.instance.OpenPauseMenu(true);
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            UIGameManager_ZombieSurvival.instance.OpenPauseMenu(false);
        }
    }

    public void UpdatePlayerKilled()
    {
        _playerKilled++;
        _zombieWaveCount--;
        if (_zombieWaveCount <= 0)
        {
            SpawnZombie();
        }
    }

    public void UpdatePlayerDeath()
    {
        if (_playerHealth._isDead && !_isMatchEnded)
        {
            _isMatchEnded = true;
            _currentGameState = GameState.MatchEnd;
            CalculateMatchRewards();
            UIGameManager_ZombieSurvival.instance.ShowUIResultMatch();
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

        int zombieCountThisWave = _zombiesPerWave + (_incrementZombiesPerWave * _spawnWaveCount);
        for (int i = 0; i < zombieCountThisWave; i++)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
            Vector3 finalSpawnPos = GetGroundPosition(waveSpawnPosition + randomOffset);
            int randomIndexPrefabs = Random.Range(0, _zombiePrefabs.Length);
            GameObject zombie = Instantiate(_zombiePrefabs[randomIndexPrefabs], finalSpawnPos, Quaternion.identity);
            _zombieWaveCount++;

            if (zombie.TryGetComponent<BehaviorGraphAgent>(out var behaviorAgent))
            {
                behaviorAgent.BlackboardReference.SetVariableValue("Target", _player);
            }
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
        string resultMatch = null;
        if (_timeCount <= 0 && !_player.GetComponent<PlayerHealth>()._isDead) resultMatch = "WIN";
        if (_player.GetComponent<PlayerHealth>()._isDead) resultMatch = "LOSE";

        int bonusGoldPerKill = GameplayDataManager.instance.GetBonusGoldPerKill();
        int rewardKills = _playerKilled * GameplayDataManager.instance.GetBonusGoldPerKill();

        int rewardMatch = GameplayDataManager.instance.GetBonusGoldByMatchResult(resultMatch);
        int totalReward = rewardMatch + rewardKills;
        PlayerDataManager.instance.AddPlayerGold(totalReward);
    }

    private Vector3 GetGroundPosition(Vector3 spawnPos)
    {
        Ray ray = new Ray(spawnPos + Vector3.up * 100f, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 200f))
        {
            return hit.point;
        }

        return spawnPos;
    }

    public bool IsPlayerVictorious()
    {
        if (_timeCount <= 0 && !_playerHealth._isDead)
            return true;
        return false;
    }
}
