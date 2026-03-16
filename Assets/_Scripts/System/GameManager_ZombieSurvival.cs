using System.Collections;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;


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
    [SerializeField] private List<GameObject> _allBotCharacter;

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
    }

    private void Start()
    {
        SpawnPlayer();
        SpawnZombieWave();
        PlayRadioVoiceReadyMission();
    }

    private void Update()
    {
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
    }

    private void PlayRadioVoiceReadyMission()
    {
        AudioManager.instance.PlayRadioOnReadyMission();
    }

    private void PlayRadioVoiceStartMission()
    {
        AudioManager.instance.PlayRadioOnStartMission();
    }

    private void PlayRadioVoiceEndMission(string resultMatch)
    {
        AudioManager.instance.PlayRadioOnEndMission(resultMatch);
    }

    private void SpawnPlayer()
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

        _player.layer = LayerMask.NameToLayer("Player");
        _playerController = _player.GetComponent<PlayerController>();
        _playerInventory = _player.GetComponent<PlayerInventory>();
        _playerHealth = _player.GetComponent<PlayerHealth>();
        _playerAnimationEvents = _player.GetComponent<PlayerAnimationEvents>();

        MiniMap.instance.SetupPlayerTransform(_player.transform);
    }

    private void SpawnZombieWave()
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

            _allBotCharacter.Add(zombie);
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

    private void UpdateMatch()
    {
        if (_currentGameState == GameState.Countdown)
        {
            if (_timeCount <= 0)
            {
                _currentGameState = GameState.RoundActive;
                _timeCount = _timeRoundActive;
                PlayRadioVoiceStartMission();
            }
        }
        else if (_currentGameState == GameState.RoundActive)
        {
            if (_timeCount <= 0)
            {
                StartCoroutine(UpdateResultMatch());
            }
            else
            {
                if (_zombieWaveCount <= 0)
                {
                    SpawnZombieWave();
                }
            }
        }
    }

    public IEnumerator UpdateResultMatch()
    {
        _currentGameState = GameState.MatchEnd;
        foreach (var bot in _allBotCharacter)
        {
            if (bot == null) continue;

            if (bot.TryGetComponent<BehaviorGraphAgent>(out var behaviorAgent))
            {
                behaviorAgent.enabled = false;
            }
            if (bot.TryGetComponent<NavMeshAgent>(out var agent))
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            if (bot.TryGetComponent<Animator>(out var anim))
            {
                anim.SetFloat("Speed", 0);
            }
        }
        CalculateMatchRewards();
        yield return new WaitForSecondsRealtime(5f);
        UIGameManager_ZombieSurvival.instance.ShowUIResultMatch();
    }

    private void CalculateMatchRewards()
    {
        string resultMatch = null;
        if (IsPlayerVictorious()) resultMatch = "WIN";
        else resultMatch = "LOSE";

        int bonusGoldPerKill = GameplayDataManager.instance.GetBonusGoldPerKill();
        int rewardKills = _playerKilled * GameplayDataManager.instance.GetBonusGoldPerKill();

        int rewardMatch = GameplayDataManager.instance.GetBonusGoldByMatchResult(resultMatch);
        int totalReward = rewardMatch + rewardKills;
        PlayerDataManager.instance.AddPlayerGold(totalReward);
        PlayRadioVoiceEndMission(resultMatch);
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
        if (_timeCount <= 0 && _playerController._lifeState == LifeState.Alive)
            return true;
        return false;
    }
}
