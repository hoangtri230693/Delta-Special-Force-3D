using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class UIGameManager_TeamDeathmatch : MonoBehaviour
{
    public static UIGameManager_TeamDeathmatch instance;

    [Header("Gameplay UI")]
    [SerializeField] private TextMeshProUGUI _health;
    [SerializeField] private TextMeshProUGUI _armor;
    [SerializeField] private TextMeshProUGUI _ammo;
    [SerializeField] private TextMeshProUGUI _time;
    [SerializeField] private GameObject _tableBuyItem;
    [SerializeField] private GameObject _tableResult;
    [SerializeField] private TextMeshProUGUI[] _killedCounter;
    [SerializeField] private TextMeshProUGUI[] _deathCounter;
    [SerializeField] private TextMeshProUGUI[] _resultCounter;
    [SerializeField] private TextMeshProUGUI[] _killedTerrorist;
    [SerializeField] private TextMeshProUGUI[] _deathTerrorist;
    [SerializeField] private TextMeshProUGUI[] _resultTerrorist;
    [SerializeField] private GameObject _panelMatchEnd;
    [SerializeField] private Image _victoryMatch, _drawMatch, _defeatMatch;
    [SerializeField] private GameObject _pauseMenu;

    [Header("Flash Color")]
    [SerializeField] private Color _winColor = Color.green;
    [SerializeField] private Color _loseColor = Color.red;
    [SerializeField] private Color _drawColor = Color.gray;


    private void Awake()
    {
        instance = this;
    }

    public void OnClickResumeGame()
    {
        GameManager_TeamDeathmatch.instance.PauseMenu(false);
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
    }

    public void OnClickReturnToMainMenu()
    {
        SceneManager.LoadScene("StartGame");
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
    }

    public void OpenPauseMenu(bool isOpen)
    {
        _pauseMenu.SetActive(isOpen);
    }

    public void ShowUIResultMatch()
    {
        Time.timeScale = 0f;
        StartCoroutine(MatchEndSequence());
    }  

    public void UpdateUIResultRound()
    {     
        if (GameManager_TeamDeathmatch.instance._teamCTWin > GameManager_TeamDeathmatch.instance._teamTerroristWin)
        {
            for (int i = 0; i < _resultCounter.Length; i++)
            {
                _resultCounter[i].text = "WIN";
                _resultCounter[i].color = _winColor;
            }
            for (int i = 0; i < _resultTerrorist.Length; i++)
            {
                _resultTerrorist[i].text = "LOSE";
                _resultTerrorist[i].color = _loseColor;
            }
        }
        else if (GameManager_TeamDeathmatch.instance._teamCTWin < GameManager_TeamDeathmatch.instance._teamTerroristWin)
        {
            for (int i = 0; i < _resultTerrorist.Length; i++)
            {
                _resultTerrorist[i].text = "WIN";
                _resultTerrorist[i].color = _winColor;
            }
            for (int i = 0; i < _resultCounter.Length; i++)
            {
                _resultCounter[i].text = "LOSE";
                _resultCounter[i].color = _loseColor;
            }
        }
        else if (GameManager_TeamDeathmatch.instance._teamCTWin == GameManager_TeamDeathmatch.instance._teamTerroristWin)
        {
            for (int i = 0; i < _resultCounter.Length; i++)
            {
                _resultCounter[i].text = "DRAW";
                _resultCounter[i].color = _drawColor;
            }
            for (int i = 0; i < _resultTerrorist.Length; i++)
            {
                _resultTerrorist[i].text = "DRAW";
                _resultTerrorist[i].color = _drawColor;
            }
        }
    }

    public void UpdateKilledCount(TeamType teamType, int playerID, int killedCount)
    {
        if (teamType == TeamType.CounterTerrorist)
        {
            _killedCounter[playerID].text = killedCount.ToString();
        }
        else if (teamType == TeamType.Terrorist)
        {
            _killedTerrorist[playerID].text = killedCount.ToString();
        }
    }

    public void UpdateDeathCount(TeamType teamType, int playerID, int deathCount)
    {
        if (teamType == TeamType.CounterTerrorist)
        {
            _deathCounter[playerID].text = deathCount.ToString();
        }
        else if (teamType == TeamType.Terrorist)
        {
            _deathTerrorist[playerID].text = deathCount.ToString();
        }
    }

    public void OpenResultMenu(bool isOpen)
    {
        _tableResult.SetActive(isOpen);
    }

    public void OpenMenuItem(bool isOpen)
    {
        _tableBuyItem.SetActive(isOpen);
    }

    public void UpdateUIWeaponAmmo(int currentAmmo, int currentReverse)
    {
        _ammo.text = currentAmmo.ToString() + " / " + currentReverse.ToString();
    }

    public void UpdateUITime(float timeCount)
    {
        int minutes = Mathf.FloorToInt(timeCount / 60);
        int seconds = Mathf.FloorToInt(timeCount % 60);

        if (GameManager_TeamDeathmatch.instance._currentGameState == GameState.Countdown)
        {
            _time.text = $"<color=#FF0000>{minutes:00} : {seconds:00}</color>";
        }
        else if (GameManager_TeamDeathmatch.instance._currentGameState == GameState.RoundActive)
        {
            _time.text = $"{minutes:00} : {seconds:00}";
        }
    }

    public void UpdateUIArmorHealth(float currentArmorHealth, PlayerHealth playerHealth)
    {
        if (playerHealth == GameManager_TeamDeathmatch.instance._playerHealth)
            _armor.text = currentArmorHealth.ToString();
    }

    public void UpdateUIPlayerHealth(float currentHealth, PlayerHealth playerHealth)
    {
        if (playerHealth == GameManager_TeamDeathmatch.instance._playerHealth)
            _health.text = currentHealth.ToString();
    }

    private IEnumerator MatchEndSequence()
    {
        _panelMatchEnd.SetActive(true);
        _tableResult.SetActive(false);

        int ctWins = GameManager_TeamDeathmatch.instance._teamCTWin;
        int tWins = GameManager_TeamDeathmatch.instance._teamTerroristWin;
        TeamType playerTeam = GameManager_TeamDeathmatch.instance._teamType;

        bool isVictory = false;
        bool isDraw = (ctWins == tWins);

        if (!isDraw)
        {
            if (playerTeam == TeamType.CounterTerrorist)
            {
                isVictory = ctWins > tWins;
            }
            else if (playerTeam == TeamType.Terrorist)
            {
                isVictory = tWins > ctWins;
            }
        }

        _victoryMatch.gameObject.SetActive(!isDraw && isVictory);
        _defeatMatch.gameObject.SetActive(!isDraw && !isVictory);
        _drawMatch.gameObject.SetActive(isDraw);

        yield return new WaitForSecondsRealtime(5f);

        Time.timeScale = 1f;
        SceneManager.LoadScene("StartGame");
    }
}
