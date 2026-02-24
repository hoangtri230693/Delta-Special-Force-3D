using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIGameManager_ZombieSurvival : MonoBehaviour
{
    public static UIGameManager_ZombieSurvival instance;

    [Header("Gameplay UI")]
    [SerializeField] private TextMeshProUGUI _health;
    [SerializeField] private TextMeshProUGUI _armor;
    [SerializeField] private TextMeshProUGUI _ammo;
    [SerializeField] private TextMeshProUGUI _time;
    [SerializeField] private TextMeshProUGUI _kill;
    [SerializeField] private GameObject _tableBuyItem;
    [SerializeField] private GameObject _panelMatchEnd;
    [SerializeField] private Image _victoryMatch, _drawMatch, _defeatMatch;
    [SerializeField] private GameObject _pauseMenu;



    private void Awake()
    {
        instance = this;
    }

    public void OnClickResumeGame()
    {
        GameManager_ZombieSurvival.instance.PauseMenu(false);
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

    public void UpdateKilledCount(int killedCount)
    {
        _kill.text = killedCount.ToString();
    }

    public void ShowUIResultMatch()
    {
        Time.timeScale = 0f;
        StartCoroutine(MatchEndSequence());
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

        if (GameManager_ZombieSurvival.instance._currentGameState == GameState.Countdown)
        {
            _time.text = $"<color=#FF0000>{minutes:00} : {seconds:00}</color>";
        }
        else if (GameManager_ZombieSurvival.instance._currentGameState == GameState.RoundActive)
        {
            _time.text = $"{minutes:00} : {seconds:00}";
        }
    }

    public void UpdateUIArmorHealth(float currentArmorHealth, PlayerHealth playerHealth)
    {
        if (playerHealth == GameManager_ZombieSurvival.instance._playerHealth)
            _armor.text = currentArmorHealth.ToString();
    }

    public void UpdateUIPlayerHealth(float currentHealth, PlayerHealth playerHealth)
    {
        if (playerHealth == GameManager_ZombieSurvival.instance._playerHealth)
            _health.text = currentHealth.ToString();
    }

    private IEnumerator MatchEndSequence()
    {
        _panelMatchEnd.SetActive(true);

        if (GameManager_ZombieSurvival.instance.IsPlayerVictorious())
        {
            _victoryMatch.gameObject.SetActive(true);
            _drawMatch.gameObject.SetActive(false);
            _defeatMatch.gameObject.SetActive(false);
        }
        else
        {
            _victoryMatch.gameObject.SetActive(false);
            _drawMatch.gameObject.SetActive(false);
            _defeatMatch.gameObject.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(5f);

        Time.timeScale = 1f;
        SceneManager.LoadScene("StartGame");
    }
}
