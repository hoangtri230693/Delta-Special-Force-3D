using Cysharp.Threading.Tasks;
using DeltaSpecialForce3D.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;


public class GameManager_TeamDeathmatch : MonoBehaviour
{
    public static GameManager_TeamDeathmatch instance;

    [Header("Gameplay Data")]
    [SerializeField] private GameplayConfigSO _gameplayConfig;
    [SerializeField] private TeamMenuSO _teamMenu;
    [SerializeField] private MapMenuSO _mapMenu;

    [Header("Game Manager")]
    public GameObject _player;
    [SerializeField] private TeamName _playerTeam;
    [SerializeField] private GameState _currentGameState;
    [SerializeField] private GameResult _currentGameResult;
    [SerializeField] private List<GameObject> _allBotCharacter;

    private SpawnData[] _spawnCounter;
    private SpawnData[] _spawnTerrorist;
    private List<GameObject> _createdAssaultPoints;
    private List<GameObject> _createdPatrolPoints;

    private int _teamCounterCount = 0;
    private int _teamTerroristCount = 0;
    private int _teamCounterWin = 0;
    private int _teamTerroristWin = 0;
    private float _timeCount;
    private int _currentRound = 0;
    private int _playerKilled = 0;
    private int _playerDeath = 0;
    private int _playerActorID;
    private List<int> _availableActorIDs_Counter;
    private List<int> _availableActorIDs_Terrorist;



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
        await SpawnMatch();
        UIGameManager_TeamDeathmatch.instance.OnLoadingScreen(false);        
        AudioManager.instance.PlayRadioTeam(_currentGameState, _currentGameResult);    
    }

    private void Update()
    {
        UpdateRound();
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

    private void SetCharactersRoundState(bool isActive)
    {
        // Set trạng thái cho Player
        if (_player != null && _player.TryGetComponent<PlayerController>(out var playerController))
        {
            playerController._roundActive = isActive;
        }

        // Set trạng thái cho tất cả Bots trong danh sách đã lưu
        if (_allBotCharacter != null)
        {
            foreach (var bot in _allBotCharacter)
            {
                if (bot != null && bot.TryGetComponent<BotAIController>(out var botAI))
                {
                    botAI._roundActive = isActive;
                }
            }
        }
    }

    private void SetupGameplay()
    {
        PlayerDataManager.instance.UsePlayerGold(_gameplayConfig.useGoldPerMatch);
        _currentGameState = GameState.Countdown;
        _currentGameResult = GameResult.None;
        _currentRound = 1;
        _timeCount = _gameplayConfig.timeCountdown;
        _allBotCharacter = new List<GameObject>();
        _createdAssaultPoints = new List<GameObject>();
        _createdPatrolPoints = new List<GameObject>();
    }

    private void SetupMapPoint(MapDataSO mapData)
    {
        if (_createdAssaultPoints != null)
        {
            foreach (var data in mapData._assaultCounter)
            {
                GameObject p = new GameObject($"AssaultPoint_{_createdAssaultPoints.Count}");
                p.transform.SetParent(this.transform);
                p.transform.position = data.position;
                _createdAssaultPoints.Add(p);
            }
        }

        if (_createdPatrolPoints != null)
        {
            foreach (var data in mapData._patrolTerrorist)
            {
                GameObject p = new GameObject($"PatrolPoint_{_createdPatrolPoints.Count}");
                p.transform.SetParent(this.transform);
                p.transform.position = data.position;
                _createdPatrolPoints.Add(p);
            }
        }
    }

    public void PauseMenu(bool isOpen)
    {
        if (isOpen)
        {
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            UIGameManager_TeamDeathmatch.instance.OpenPauseMenu(true);
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            UIGameManager_TeamDeathmatch.instance.OpenPauseMenu(false);
        }
    }

    public void UpdatePlayerKilled(TeamName team, int actorID, int killedCount)
    {
        if (actorID == _playerActorID && team == _playerTeam)
        {
            _playerKilled++;
            UIGameManager_TeamDeathmatch.instance.UpdateKilledCount(team, actorID, _playerKilled);
        }
        else
        {
            UIGameManager_TeamDeathmatch.instance.UpdateKilledCount(team, actorID, killedCount);
        }
    }

    public void UpdatePlayerDeath(TeamName team, int actorID, int deathCount)
    {
        switch (team)
        {
            case TeamName.Counter:
                _teamCounterCount--;
                break;
            case TeamName.Terrorist:
                _teamTerroristCount--;
                break;
        }

        if (actorID == _playerActorID && team == _playerTeam)
        {
            _playerDeath++;
            UIGameManager_TeamDeathmatch.instance.UpdateDeathCount(team, actorID, _playerDeath);
        }              
        else
            UIGameManager_TeamDeathmatch.instance.UpdateDeathCount(team, actorID, deathCount);     
    }

    public void OnPlayerDeath()
    {
        if (_currentGameState == GameState.RoundActive)
        {
            StopAllCharacters();
            _currentGameState = GameState.RoundEnd;
            SetCharactersRoundState(false);
            _timeCount = 5f;

            if (_playerTeam == TeamName.Counter) _teamTerroristWin++;
            else _teamCounterWin++;
            
            UpdateResultRound();
            UIGameManager_TeamDeathmatch.instance.OpenResultMenu(true);
        }
    }

    private async UniTask SpawnMatch()
    {
        _teamCounterCount = 0;
        _teamTerroristCount = 0;

        int mapID = PlayerPrefs.GetInt("SelectedMapID", 0);
        var mapData = _mapMenu.GetMapDataByMapID(mapID);
        SetupMapPoint(mapData);

        _spawnCounter = mapData._spawnCounter;
        _spawnTerrorist = mapData._spawnTerrorist;
        ShuffleArray(_spawnCounter);
        ShuffleArray(_spawnTerrorist);

        int teamSize = _gameplayConfig.teamSize;
        _availableActorIDs_Counter = new List<int>();
        _availableActorIDs_Terrorist = new List<int>();

        for (int i = 0; i < teamSize; i++)
        {
            _availableActorIDs_Counter.Add(i);
            _availableActorIDs_Terrorist.Add(i);
        }

        var teamID = PlayerPrefs.GetInt("SelectedTeamID", 0);
        var teamData = _teamMenu.GetTeamByTeamID(teamID);
        _playerTeam = teamData.teamName;
        //Debug.Log("Player Team: " + _playerTeam.ToString());

        await SpawnPlayer(teamData, teamSize);
        await SpawnTeamBots(TeamName.Counter, teamSize);
        await SpawnTeamBots(TeamName.Terrorist, teamSize);
    }

    private async UniTask SpawnPlayer(TeamDataSO teamData, int teamSize)
    {
        int charID = PlayerPrefs.GetInt("SelectedCharacterID", 0);
        var charData = teamData.GetCharacterDataByCharacterID(charID);

        if (charData != null)
        {
            _player = await AddressableManager.instance.InstantiatePrefabAsync(charData.characterPlayerPrefab);
            
            if (_player != null)
            {
                SpawnData spawnData = (_playerTeam == TeamName.Counter) ? _spawnCounter[0] : _spawnTerrorist[0];
                _player.transform.position = spawnData.position;
                _player.transform.rotation = Quaternion.Euler(spawnData.rotation);
                if (_player.TryGetComponent<PlayerTeam>(out var playerTeam))
                {
                    var list = (_playerTeam == TeamName.Counter) ? _availableActorIDs_Counter : _availableActorIDs_Terrorist;

                    int randomIndex = Random.Range(0, list.Count - 1);
                    //Debug.Log("List Count: " + list.Count);
                    _playerActorID = list[randomIndex];

                    list.RemoveAt(randomIndex);
                    playerTeam.SetupActor(_playerActorID);
                    UIGameManager_TeamDeathmatch.instance.ResetResultMenu();
                    UIGameManager_TeamDeathmatch.instance.SetColorPlayerResult(_playerActorID, _playerTeam);
                    UIGameManager_TeamDeathmatch.instance.UpdateKilledCount(_playerTeam, _playerActorID, _playerKilled);
                    UIGameManager_TeamDeathmatch.instance.UpdateDeathCount(_playerTeam, _playerActorID, _playerDeath);
                    UIGameManager_TeamDeathmatch.instance.SetupMiniMap(_player);
                    //Debug.Log("Player Team: " + _playerTeam);
                }
            }

            if (_playerTeam == TeamName.Counter) _teamCounterCount++; else _teamTerroristCount++;
        }
    }

    private async UniTask SpawnTeamBots(TeamName teamName, int teamSize)
    {
        TeamDataSO teamData = _teamMenu.GetTeamByTeamName(teamName);
        if (teamData == null) return;

        // Trừ đi 1 nếu team đó có Player
        int _teamSize = (teamName == _playerTeam) ? teamSize - 1 : teamSize;
        int startIndex = (teamName == _playerTeam) ? 1 : 0;
        SpawnData[] spawnPoints = (teamName == TeamName.Counter) ? _spawnCounter : _spawnTerrorist;

        List<UniTask<GameObject>> tasks = new List<UniTask<GameObject>>();

        for (int i = 0; i < _teamSize; i++)
        {
            var charData = teamData.characterData[Random.Range(0, teamData.characterData.Length)];
            tasks.Add(AddressableManager.instance.InstantiatePrefabAsync(charData.characterAIPrefab));
        }

        GameObject[] spawnedBots = await UniTask.WhenAll(tasks);

        for (int i = 0; i < spawnedBots.Length; i++)
        {
            GameObject bot = spawnedBots[i];
            SpawnData sPoint = spawnPoints[i + startIndex];
            bot.transform.position = sPoint.position;
            Vector3 spawnRotation = sPoint.rotation;
            bot.transform.rotation = Quaternion.Euler(0, spawnRotation.y, 0);

            if (bot.TryGetComponent<PlayerTeam>(out var botTeam))
            {
                var list = (teamName == TeamName.Counter) ? _availableActorIDs_Counter : _availableActorIDs_Terrorist;

                int randomIndex = Random.Range(0, list.Count);
                int actorID = list[randomIndex];

                list.RemoveAt(randomIndex);
                botTeam.SetupActor(actorID);
            }

            await SetupBotAI(bot, teamName, i);
           
            _allBotCharacter.Add(bot);
            if (teamName == TeamName.Counter) _teamCounterCount++; else _teamTerroristCount++;
        }
    }

    private async Task SetupBotAI(GameObject bot, TeamName team, int index)
    {
        if (!bot.TryGetComponent<BehaviorGraphAgent>(out var behavior)) return;

        if (bot.TryGetComponent<NavMeshAgent>(out var agent))
        {
            agent.avoidancePriority = Mathf.Clamp(index, 0, 99);
            agent.enabled = true;
        }

        await UniTask.Yield();

        List<GameObject> pointsForBlackboard = new List<GameObject>();

        if (team == TeamName.Counter)
        {
            behavior.BlackboardReference.SetVariableValue("AssaultPoints", _createdAssaultPoints);
        }
        else
        {
            List<GameObject> randomizedPatrolPoints = new List<GameObject>(_createdPatrolPoints);
            ShuffleArray(randomizedPatrolPoints);
            behavior.BlackboardReference.SetVariableValue("PatrolPoints", randomizedPatrolPoints);
        }

        behavior.enabled = true;
    }

    private void ShuffleArray<T>(IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(i, list.Count);
            (list[rnd], list[i]) = (list[i], list[rnd]);
        }
    }

    private void UpdateTime()
    {
        if (_timeCount > 0)
        {
            _timeCount -= Time.deltaTime;
            _timeCount = Mathf.Clamp(_timeCount, 0, Mathf.Infinity);
            UIGameManager_TeamDeathmatch.instance.UpdateUITime(_timeCount, _currentGameState);
        }
    }

    private void UpdateRound()
    {
        switch (_currentGameState)
        {
            case GameState.Countdown:
                if (_timeCount <= 0)
                {
                    _currentGameState = GameState.RoundActive;
                    _timeCount = _gameplayConfig.timeRoundActive;
                    SetCharactersRoundState(true);
                    if (_currentRound > 0) return;
                    AudioManager.instance.PlayRadioTeam(_currentGameState, _currentGameResult);
                }
                break;
            case GameState.RoundActive:
                if (_timeCount <= 0 || _teamCounterCount <= 0 || _teamTerroristCount <= 0)
                {
                    StopAllCharacters();
                    if (_teamCounterCount <= _teamTerroristCount) _teamTerroristWin++;
                    else if (_teamTerroristCount <= _teamCounterCount) _teamCounterWin++;
                    _currentGameState = GameState.RoundEnd;
                    SetCharactersRoundState(false);
                    _timeCount = 5f;
                    UpdateResultRound();
                }
                break;
        }
    }

    private void UpdateMatch()
    {
        if (_currentGameState == GameState.RoundEnd)
        {
            if (_timeCount <= 0)
            {
                if (_currentRound < _gameplayConfig.totalRound)
                {
                    PrepareNextRound();
                }
                else
                {
                    _currentGameState = GameState.MatchEnd;
                    StartCoroutine(UpdateResultMatch());
                }
            }
        }
    }

    private void UpdateResultRound()
    {
        if (_teamCounterWin > _teamTerroristWin) UIGameManager_TeamDeathmatch.instance.UpdateUIResultRound(TeamName.Counter);
        else if (_teamTerroristWin > _teamCounterWin) UIGameManager_TeamDeathmatch.instance.UpdateUIResultRound(TeamName.Terrorist);
        else UIGameManager_TeamDeathmatch.instance.UpdateUIResultRound(TeamName.None);
    }

    private IEnumerator UpdateResultMatch()
    {
        StopAllCharacters();
        yield return StartCoroutine(CalculateMatchRewards());
        yield return StartCoroutine(UIGameManager_TeamDeathmatch.instance.ShowUIResultMatch(_currentGameResult));
    }

    private void StopAllCharacters()
    {
        _player.GetComponent<PlayerController>().ResetPlayerState();

        foreach (var bot in _allBotCharacter)
        {
            if (bot == null) continue;
            if (bot.TryGetComponent<BotAIController>(out var botAI)) botAI.enabled = false;
            if (bot.TryGetComponent<BehaviorGraphAgent>(out var behavior)) behavior.enabled = false;
            if (bot.TryGetComponent<NavMeshAgent>(out var navMeshAgent)) navMeshAgent.enabled = false;
            if (bot.TryGetComponent<Animator>(out var animator)) animator.SetFloat("Speed", 0);
        }
    }

    private async void PrepareNextRound()
    {
        _currentGameState = GameState.Setup;

        UIGameManager_TeamDeathmatch.instance.OpenResultMenu(false);
        _currentRound++;

        if (_player != null)
        {
            AddressableManager.instance.ReleaseInstance(_player);
            _player = null;
        }

        for (int i = _allBotCharacter.Count - 1; i >= 0; i--)
        {
            if (_allBotCharacter[i] != null)
            {
                AddressableManager.instance.ReleaseInstance(_allBotCharacter[i]);
            }
        }
        _allBotCharacter.Clear();

        UIGameManager_TeamDeathmatch.instance.OnLoadingScreen(true);
        await SpawnMatch();
        UIGameManager_TeamDeathmatch.instance.OnLoadingScreen(false);

        _timeCount = _gameplayConfig.timeCountdown;
        _currentGameState = GameState.Countdown;
    }

    private IEnumerator CalculateMatchRewards()
    {
        if (_playerTeam == TeamName.Counter)
            _currentGameResult = (_teamCounterWin > _teamTerroristWin) ? GameResult.Win : GameResult.Lose;
        else
            _currentGameResult = (_teamTerroristWin > _teamCounterWin) ? GameResult.Win : GameResult.Lose;

        int rewardKills = _playerKilled * _gameplayConfig.bonusGoldPerKill;
        int rewardMatch = _gameplayConfig.GetGoldByResult(_currentGameResult);
        int totalReward = rewardMatch + rewardKills;

        PlayerDataManager.instance.AddPlayerGold(totalReward);
        AudioManager.instance.PlayRadioTeam(_currentGameState, _currentGameResult);
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