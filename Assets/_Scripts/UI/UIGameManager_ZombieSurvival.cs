using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DeltaSpecialForce3D.Enums;

public class UIGameManager_ZombieSurvival : MonoBehaviour
{
    public static UIGameManager_ZombieSurvival instance;

    [Header("Gameplay UI")]
    [SerializeField] private TextMeshProUGUI _health;
    [SerializeField] private TextMeshProUGUI _armor;
    [SerializeField] private TextMeshProUGUI _ammo;
    [SerializeField] private TextMeshProUGUI _time;
    [SerializeField] private TextMeshProUGUI _kill;
    [SerializeField] private GameObject _shopInGame;
    [SerializeField] private GameObject _panelMatchEnd;
    [SerializeField] private Image _victoryMatch, _defeatMatch;
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private Image _backgroundLoading;



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
        GameManager_ZombieSurvival.instance.PauseMenu(false);
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

    public void UpdateKilledCount(int killedCount)
    {
        _kill.text = killedCount.ToString();
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
