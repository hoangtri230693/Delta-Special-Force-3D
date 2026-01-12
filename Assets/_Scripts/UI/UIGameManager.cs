using System.Collections;
using TMPro;
using UnityEngine;

public class UIGameManager : MonoBehaviour
{
    public static UIGameManager instance;

    [Header("Gameplay UI")]
    [SerializeField] private TextMeshProUGUI _health;
    [SerializeField] private TextMeshProUGUI _armor;
    [SerializeField] private TextMeshProUGUI _ammo;
    [SerializeField] private TextMeshProUGUI _time;
    [SerializeField] private TextMeshProUGUI _cash;
    [SerializeField] private TextMeshProUGUI _buy;
    [SerializeField] private GameObject _tableBuyItem;
    [SerializeField] private GameObject _tableResult;
    [SerializeField] private TextMeshProUGUI[] _killedCounter;
    [SerializeField] private TextMeshProUGUI[] _deathCounter;
    [SerializeField] private TextMeshProUGUI[] _resultCounter;
    [SerializeField] private TextMeshProUGUI[] _killedTerrorist;
    [SerializeField] private TextMeshProUGUI[] _deathTerrorist;
    [SerializeField] private TextMeshProUGUI[] _resultTerrorist;

    [Header("Weapon Management UI")]
    [SerializeField] private TextMeshProUGUI[] _textWeaponType;
    [SerializeField] private TextMeshProUGUI[] _textWeaponName;
    [SerializeField] private GameObject[] _weaponList;
    [SerializeField] private GameObject[] _weaponPreview;
    [SerializeField] private GameObject[] _weaponData;
    [SerializeField] private int[] _weaponTypeStartIndices;

    [Header("Flash Color")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _flashColor = Color.green;
    [SerializeField] private Color _winColor = Color.green;
    [SerializeField] private Color _loseColor = Color.red;
    [SerializeField] private Color _drawColor = Color.gray;

    public bool _isMatchEnd = false;
    public int _indexWeaponListOpen = -1;

    private int _currentWeaponIndex = -1;
    private float _flashDuration = 0.1f;


    private void Awake()
    {
        instance = this;
    }

    public void UpdateUIResult()
    {     
        if (GameManager.instance._teamCTWin > GameManager.instance._teamTerroristWin)
        {
            for (int i = 0; i < _resultCounter.Length; i++)
            {
                _resultCounter[i].text = "WINNER";
                _resultCounter[i].color = _winColor;
            }
            for (int i = 0; i < _resultTerrorist.Length; i++)
            {
                _resultTerrorist[i].text = "LOSER";
                _resultTerrorist[i].color = _loseColor;
            }
        }
        else if (GameManager.instance._teamCTWin < GameManager.instance._teamTerroristWin)
        {
            for (int i = 0; i < _resultTerrorist.Length; i++)
            {
                _resultTerrorist[i].text = "WINNER";
                _resultTerrorist[i].color = _winColor;
            }
            for (int i = 0; i < _resultCounter.Length; i++)
            {
                _resultCounter[i].text = "LOSER";
                _resultCounter[i].color = _loseColor;
            }
        }
        else if (GameManager.instance._teamCTWin == GameManager.instance._teamTerroristWin)
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
        HideAllMenuWeapon();
    }

    public void OnShowWeaponList(int weaponListIndex)
    {
        foreach (GameObject list in _weaponList) list.SetActive(false);

        if (weaponListIndex >= 0 && weaponListIndex < _weaponList.Length)
        {
            _weaponList[weaponListIndex].SetActive(true);
            AudioManager.instance.PlaySfx(SFXType.MetalClick);
            _indexWeaponListOpen = weaponListIndex;
            
        }

        StartCoroutine(FlashAndFadeColor(_textWeaponType[weaponListIndex]));
    }

    public void OnShowWeapon(int localWeaponIndex)
    {
        int weaponListIndex = _indexWeaponListOpen;
        int childCount = GetChildCount(_weaponList[weaponListIndex].transform);
        if (localWeaponIndex > childCount - 1) return;
        if (weaponListIndex < 0 || weaponListIndex >= _weaponTypeStartIndices.Length) return;

        int startIndex = _weaponTypeStartIndices[weaponListIndex];
        int lastIndex = _weaponTypeStartIndices[weaponListIndex + 1] - 1;

        int globalWeaponIndex = Mathf.Clamp(startIndex + localWeaponIndex, startIndex, lastIndex);
        
        foreach (GameObject preview in _weaponPreview) preview.SetActive(false);
        foreach (GameObject data in _weaponData) data.SetActive(false);

        _weaponPreview[globalWeaponIndex].SetActive(true);
        _weaponData[globalWeaponIndex].SetActive(true);
        _currentWeaponIndex = globalWeaponIndex;
        AudioManager.instance.PlaySfx(SFXType.MetalClick);

        StartCoroutine(FlashAndFadeColor(_textWeaponName[globalWeaponIndex]));
    }

    public void OnBuyWeapon()
    {
        GameManager.instance.BuyWeapon(_currentWeaponIndex, GameManager.instance._playerController, GameManager.instance._playerInventory, GameManager.instance._playerHealth);
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
        StartCoroutine(FlashAndFadeColor(_buy));
    }

    public void HideAllMenuWeapon()
    {
        foreach (GameObject list in _weaponList) list.SetActive(false);
        foreach (GameObject preview in _weaponPreview) preview.SetActive(false);
        foreach (GameObject data in _weaponData) data.SetActive(false);

        _indexWeaponListOpen = -1;
    }

    public void UpdateUIWeaponAmmo(int currentAmmo, int currentReverse)
    {
        _ammo.text = currentAmmo.ToString() + " / " + currentReverse.ToString();
    }

    public void UpdateUITime(float timeCount)
    {
        int minutes = Mathf.FloorToInt(timeCount / 60);
        int seconds = Mathf.FloorToInt(timeCount % 60);

        if (GameManager.instance._currentGameState == GameState.Countdown)
        {
            _time.text = $"<color=#FF0000>{minutes:00} : {seconds:00}</color>";
        }
        else if (GameManager.instance._currentGameState == GameState.RoundActive)
        {
            _time.text = $"{minutes:00} : {seconds:00}";
        }
    }

    public void UpdateUICash(float currentCash)
    {
        _cash.text = "$" + currentCash.ToString();
    }

    public void UpdateUIArmorHealth(float currentArmorHealth, PlayerHealth playerHealth)
    {
        if (playerHealth == GameManager.instance._playerHealth)
            _armor.text = currentArmorHealth.ToString();
    }

    public void UpdateUIPlayerHealth(float currentHealth, PlayerHealth playerHealth)
    {
        if (playerHealth == GameManager.instance._playerHealth)
            _health.text = currentHealth.ToString();
    }

    private IEnumerator FlashAndFadeColor(TextMeshProUGUI text)
    {
        text.color = _flashColor;

        yield return new WaitForSeconds(_flashDuration);

        Color currentColor = text.color;
        float timeElapsed = 0f;

        while (timeElapsed < 1f)
        {
            text.color = Color.Lerp(currentColor, _normalColor, timeElapsed);

            timeElapsed += Time.deltaTime * 3f;

            yield return null;
        }

        text.color = _normalColor;
    }

    private int GetChildCount(Transform parent)
    {
        int count = 0;
        foreach (Transform child in parent)
        {
            count++;
        }
        return count;
    }
}
