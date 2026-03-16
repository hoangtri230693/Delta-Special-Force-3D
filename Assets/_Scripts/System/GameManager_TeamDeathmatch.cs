using System.Collections;
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
        PlayRadioVoiceReadyMission();
        UIGameManager_TeamDeathmatch.instance.UpdateUIResultRound();
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

    public void UpdatePlayerKilled(PlayerController player)
    {
        if (player == _playerController)
            _playerKilled++;
    }

    public void UpdateTeamCount(TeamType teamType)
    {
        if (teamType == TeamType.Counter)
        {
            _teamCTCount--;
        }
        if (teamType == TeamType.Terrorist)
        {
            _teamTerroristCount--;
        }
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

    private void SpawnTeams()
    {
        _teamCTCount = 0;
        _teamTerroristCount = 0;

        ShuffleTransform(_spawnCounter);
        ShuffleTransform(_spawnTerrorist);

        if (_player == null)
        {
            int selectedTeamID = PlayerPrefs.GetInt("SelectedTeamID", 0);
            int selectedCharacterID = PlayerPrefs.GetInt("SelectedCharacterID", 0);

            Transform pSpawn;
            if (selectedTeamID == 0)
            {
                _teamType = TeamType.Counter;
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
                
            _playerController = _player.GetComponent<PlayerController>();
            _playerInventory = _player.GetComponent<PlayerInventory>();
            _playerHealth = _player.GetComponent<PlayerHealth>();
            _playerAnimationEvents = _player.GetComponent<PlayerAnimationEvents>();
            _playerTeam = _player.GetComponent<PlayerTeam>();
            _playerTeam._playerID = 0;

            MiniMap.instance.SetupPlayerTransform(_player.transform);
        }
        else
        {
            Transform pSpawn = (_teamType == TeamType.Counter) ? _spawnCounter[0] : _spawnTerrorist[0];
            _player.transform.position = pSpawn.position;
            _player.transform.rotation = pSpawn.rotation;
            if (_teamType == TeamType.Counter) _teamCTCount++;
            else _teamTerroristCount++;

            _playerController.ResetPlayerState();
            _playerHealth.ResetHealth();
        }
    

        if (_teamType == TeamType.Counter)
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
                PlayRadioVoiceStartMission();
            }
        }
        else if (_currentGameState == GameState.RoundActive)
        {
            if (_timeCount <= 0 || _teamCTCount <= 0 || _teamTerroristCount <= 0)
            {
                _playerController.ResetPlayerState();

                if (_teamCTCount <= 0) _teamTerroristWin++;
                else if (_teamTerroristCount <= 0) _teamCTWin++;

                _currentGameState = GameState.RoundEnd;
                _timeCount = 5f;

                foreach (GameObject bot in _allBotCharacter)
                {
                    if (bot.TryGetComponent<BotAIController>(out var controller))
                        controller.enabled = false;

                    if (bot.TryGetComponent<BehaviorGraphAgent>(out var agent))
                        agent.enabled = false;

                    if (bot.TryGetComponent<CharacterController>(out var charCtrl))
                        charCtrl.Move(Vector3.zero);

                    if (bot.TryGetComponent<Animator>(out var animator))
                        animator.SetFloat("Speed", 0f);                       
                }
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
                        StartCoroutine(UpdateResultMatch());
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
        if (_playerHealth._currentHealth <= 0)
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

    public IEnumerator UpdateResultMatch()
    {
        _isMatchEnded = true;
        _currentGameState = GameState.MatchEnd;
        CalculateMatchRewards();
        yield return new WaitForSecondsRealtime(5f);
        UIGameManager_TeamDeathmatch.instance.ShowUIResultMatch();
    }

    private void CalculateMatchRewards()
    {
        string resultMatch = null;
        int bonusGoldPerKill = GameplayDataManager.instance.GetBonusGoldPerKill();
        int rewardKills = _playerKilled * GameplayDataManager.instance.GetBonusGoldPerKill();
        bool isPlayerCT = (_teamType == TeamType.Counter);

        if (_teamCTWin > _teamTerroristWin)
        {
            resultMatch = isPlayerCT ? "WIN" : "LOSE";
        }
        else if (_teamTerroristWin > _teamCTWin)
        {
            resultMatch = !isPlayerCT ? "WIN" : "LOSE";
        }

        int rewardMatch = GameplayDataManager.instance.GetBonusGoldByMatchResult(resultMatch);
        int totalReward = rewardMatch + rewardKills;
        PlayerDataManager.instance.AddPlayerGold(totalReward);
        PlayRadioVoiceEndMission(resultMatch);
    }
}