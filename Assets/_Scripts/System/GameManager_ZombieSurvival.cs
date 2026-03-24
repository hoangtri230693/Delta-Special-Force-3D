using System.Collections;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using DeltaSpecialForce3D.Enums;
using Cysharp.Threading.Tasks;


public class GameManager_ZombieSurvival : MonoBehaviour
{
    public static GameManager_ZombieSurvival instance;

    [Header("Gameplay Data")]
    [SerializeField] private GameplayConfigSO _gameplayConfig;
    [SerializeField] private TeamMenuSO _teamMenu;
    [SerializeField] private MapMenuSO _mapMenu;

    [Header("Game Manager")]
    private List<GameObject> _allBotCharacter;
    public GameState _currentGameState { get; private set; }
    public GameResult _currentGameResult { get; private set; }

    public GameObject _player;
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

        GetDataGameplay();
    }

    private void OnEnable()
    {
        SetupGameplay();
    }

    private async void Start()
    {
        await SpawnPlayer();
        await SpawnZombieWave();

        UIGameManager_ZombieSurvival.instance.OnLoadingScreen(false);
        UIGameManager_ZombieSurvival.instance.SetupMiniMap(_player);
        AudioManager.instance.PlayRadioZombie(_currentGameState, _currentGameResult);
    }

    private void Update()
    {
        UpdateMatch();
        UpdateTime();
    }

    private void GetDataGameplay()
    {
        _gameplayConfig = GameplayDataManager.instance._gameplayConfigSO;
        _teamMenu = GameplayDataManager.instance._teamMenuSO;
        foreach (var menu in GameplayDataManager.instance._mapMenuSO)
        {
            if (menu.gameMode == GameplayDataManager.instance.gameMode)
            {
                _mapMenu = menu;
                break;
            }          
        }
    }

    private void SetupGameplay()
    {
        PlayerDataManager.instance.UsePlayerGold(_gameplayConfig.useGoldPerMatch);
        _currentGameState = GameState.Countdown;
        _currentGameResult = GameResult.None;
        _timeCount = _gameplayConfig.timeCountdown;
        _allBotCharacter = new List<GameObject>();
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

    public void OnPlayerDeath()
    {
        if (_currentGameState == GameState.RoundActive)
        {
            _currentGameState = GameState.MatchEnd;
            _currentGameResult = GameResult.Lose;
            StartCoroutine(UpdateResultMatch());
        }
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

    private async UniTask SpawnPlayer()
    {
        int teamID = PlayerPrefs.GetInt("SelectedTeamID", 0);
        int charID = PlayerPrefs.GetInt("SelectedCharacterID", 0);
        int mapID = PlayerPrefs.GetInt("SelectedMapID", 0);
        Debug.Log("Map ID: " + mapID);

        var teamData = _teamMenu.GetTeamByID(teamID);
        var charData = teamData?.GetCharacterDataByCharacterID(charID);
        var mapData = _mapMenu.GetMapDataByMapID(mapID);

        if (charData != null && charData.characterPlayerPrefab != null)
        {
            _player = await AddressableManager.instance.InstantiatePrefabAsync(charData.characterPlayerPrefab);

            if (_player != null)
            {
                SpawnData spawnData = mapData._spawnPoint;
                _player.transform.position = spawnData.position;
                _player.transform.rotation = Quaternion.Euler(spawnData.rotation);
                _player.layer = LayerMask.NameToLayer("Player");
            }
        }
    }

    private async UniTask SpawnZombieWave()
    {
        if (_spawnWaveCount == 0)
        {
            _baseSpawnDirection = _player.transform.forward;
            _initialSpawnPoint = _player.transform.position + (_baseSpawnDirection * _gameplayConfig.initialDistanceFromPlayer);
        }

        Vector3 waveSpawnPos = _initialSpawnPoint + (_baseSpawnDirection * _gameplayConfig.distanceBetweenWaveUp * _spawnWaveCount);
       
        TeamDataSO zombieTeam = _teamMenu.GetTeamByTeamName(TeamName.Zombie);
        if (zombieTeam == null) return;

        List<UniTask<GameObject>> tasks = new List<UniTask<GameObject>>();

        int zombieCountThisWave = _gameplayConfig.zombiePerWave + (_gameplayConfig.incrementZombiePerWave * _spawnWaveCount);
        for (int i = 0; i < zombieCountThisWave; i++)
        {
            var charData = zombieTeam.characterData[Random.Range(0, zombieTeam.characterData.Length)];
            tasks.Add(AddressableManager.instance.InstantiatePrefabAsync(charData.characterAIPrefab));
        }

        GameObject[] spawnedZombies = await UniTask.WhenAll(tasks);

        foreach (var zombie in spawnedZombies)
        {
            if (zombie == null) continue;

            Vector3 randomOffset = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
            zombie.transform.position = GetGroundPosition(waveSpawnPos + randomOffset);

            if (zombie.TryGetComponent<NavMeshAgent>(out var navMesh)) navMesh.enabled = true;

            if (zombie.TryGetComponent<BehaviorGraphAgent>(out var behavior))
            {
                behavior.BlackboardReference.SetVariableValue("Target", _player);
                behavior.enabled = true;
            }
        
            _allBotCharacter.Add(zombie);
            _zombieWaveCount++;
        }

        _spawnWaveCount++;
    }

    private void UpdateTime()
    {
        if (_timeCount > 0)
        {
            _timeCount -= Time.deltaTime;
            _timeCount = Mathf.Clamp(_timeCount, 0, Mathf.Infinity);
            UIGameManager_ZombieSurvival.instance.UpdateUITime(_timeCount, _currentGameState);
        }
    }

    private void UpdateMatch()
    {
        switch (_currentGameState)
        {
            case GameState.Countdown:
                if (_timeCount <= 0)
                {
                    _currentGameState = GameState.RoundActive;
                    _timeCount = _gameplayConfig.timeRoundActive;
                    AudioManager.instance.PlayRadioZombie(_currentGameState, _currentGameResult);
                }
                break;
            case GameState.RoundActive:
                if (_timeCount <= 0)
                {
                    _currentGameState = GameState.MatchEnd;
                    _currentGameResult = GameResult.Win;
                    StartCoroutine(UpdateResultMatch());
                }
                else
                {
                    if (_zombieWaveCount <= 0)
                    {
                        SpawnZombieWave().Forget();
                    }
                }
                break;
        }
    }

    private IEnumerator UpdateResultMatch()
    {
        StopAllCharacter();
        yield return StartCoroutine(CalculateMatchRewards());     
        yield return StartCoroutine(UIGameManager_ZombieSurvival.instance.ShowUIResultMatch(_currentGameResult));
    }

    private void StopAllCharacter()
    {
        _player.GetComponent<PlayerController>().ResetPlayerState();

        foreach (var bot in _allBotCharacter)
        {
            if (bot == null) continue;

            if (bot.TryGetComponent<BehaviorGraphAgent>(out var behaviorAgent)) behaviorAgent.enabled = false;
            if (bot.TryGetComponent<NavMeshAgent>(out var agent)) agent.enabled = false;
            if (bot.TryGetComponent<Animator>(out var anim)) anim.SetFloat("Speed", 0);
        }
    }

    private IEnumerator CalculateMatchRewards()
    {
        int rewardKills = _playerKilled * _gameplayConfig.bonusGoldPerKill;
        int rewardMatch = _gameplayConfig.GetGoldByResult(_currentGameResult);
        int totalReward = rewardMatch + rewardKills;

        PlayerDataManager.instance.AddPlayerGold(totalReward);
        AudioManager.instance.PlayRadioZombie(_currentGameState, _currentGameResult);
        yield return new WaitForSecondsRealtime(5f);
    }

    private void OnDestroy()
    {
        if (_player != null)
        {
            AddressableManager.instance.ReleaseInstance(_player);
            _player = null;
        }

        if (_allBotCharacter != null)
        {
            foreach (var bot in _allBotCharacter)
            {
                if (bot != null)
                {
                    AddressableManager.instance.ReleaseInstance(bot);
                }
            }

            _allBotCharacter.Clear();
        }
    }
}
