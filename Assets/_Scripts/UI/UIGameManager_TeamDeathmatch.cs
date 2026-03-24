using DeltaSpecialForce3D.Enums;
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
    [SerializeField] private GameObject _shopInGame;
    [SerializeField] private GameObject _tableResult;
    [SerializeField] private TextMeshProUGUI[] _killedCounter;
    [SerializeField] private TextMeshProUGUI[] _deathCounter;
    [SerializeField] private TextMeshProUGUI[] _resultCounter;
    [SerializeField] private TextMeshProUGUI[] _killedTerrorist;
    [SerializeField] private TextMeshProUGUI[] _deathTerrorist;
    [SerializeField] private TextMeshProUGUI[] _resultTerrorist;
    [SerializeField] private GameObject _panelMatchEnd;
    [SerializeField] private Image _victoryMatch, _defeatMatch;
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private Image _backgroundLoading;

    [Header("Flash Color")]
    [SerializeField] private Color _winColor = Color.green;
    [SerializeField] private Color _loseColor = Color.red;
    [SerializeField] private Color _drawColor = Color.gray;


    private void Awake()
    {
        instance = this;
        OnLoadingScreen(true);
    }

    public void SetupMiniMap(GameObject player)
    {
        MiniMap.instance.SetupPlayerTransform(player.transform);
    }

    public void OnLoadingScreen(bool isEnable)
    {
        _backgroundLoading.gameObject.SetActive(isEnable);
    }

    public void OnClickResumeGame()
    {
        GameManager_TeamDeathmatch.instance.PauseMenu(false);
        AudioManager.instance.PlaySfx(SFXSoundType.MetalClick);
    }

    public void OnClickReturnToMainMenu()
    {
        SceneManager.LoadScene("StartGame");
        AudioManager.instance.PlaySfx(SFXSoundType.MetalClick);
    }

    public void OpenPauseMenu(bool isOpen)
    {
        _pauseMenu.SetActive(isOpen);
    }

    public void OpenResultMenu(bool isOpen)
    {
        _tableResult.SetActive(isOpen);
    }

    public void OpenShopInGame(bool isOpen)
    {
        _shopInGame.SetActive(isOpen);
    }

    public IEnumerator ShowUIResultMatch(GameResult result)
    {
        Time.timeScale = 0f;
        _panelMatchEnd.SetActive(true);

        switch (result)
        {
            case GameResult.Win:
                _victoryMatch.gameObject.SetActive(true);
                _defeatMatch.gameObject.SetActive(false);
                break;
            case GameResult.Lose:
                _victoryMatch.gameObject.SetActive(false);
                _defeatMatch.gameObject.SetActive(true);
                break;
        }

        yield return new WaitForSecondsRealtime(5f);

        Time.timeScale = 1f;
        SceneManager.LoadScene("StartGame");
    }

    public void UpdateUIResultRound(TeamName teamName)
    {     
        if (teamName == TeamName.Counter)
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
        else if (teamName == TeamName.Terrorist)
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
        else if (teamName == TeamName.None)
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

        OpenResultMenu(true);
    }

    public void UpdateKilledCount(TeamName teamName, int playerID, int killedCount)
    {
        if (teamName == TeamName.Counter)
        {
            _killedCounter[playerID].text = killedCount.ToString();
        }
        else if (teamName == TeamName.Terrorist)
        {
            _killedTerrorist[playerID].text = killedCount.ToString();
        }
    }

    public void UpdateDeathCount(TeamName teamName, int playerID, int deathCount)
    {
        if (teamName == TeamName.Counter)
        {
            _deathCounter[playerID].text = deathCount.ToString();
        }
        else if (teamName == TeamName.Terrorist)
        {
            _deathTerrorist[playerID].text = deathCount.ToString();
        }
    }

    public void UpdateUIWeaponAmmo(int currentAmmo, int currentReverse)
    {
        _ammo.text = currentAmmo.ToString() + " / " + currentReverse.ToString();
    }

    public void UpdateUITime(float timeCount, GameState gameState)
    {
        int minutes = Mathf.FloorToInt(timeCount / 60);
        int seconds = Mathf.FloorToInt(timeCount % 60);

        if (gameState == GameState.Countdown)
        {
            _time.text = $"<color=#FF0000>{minutes:00} : {seconds:00}</color>";
        }
        else if (gameState == GameState.RoundActive)
        {
            _time.text = $"{minutes:00} : {seconds:00}";
        }
    }

    public void UpdateUIArmorHealth(float currentArmorHealth)
    {
        _armor.text = currentArmorHealth.ToString();
    }

    public void UpdateUIPlayerHealth(float currentHealth)
    {
        _health.text = currentHealth.ToString();
    }
}
