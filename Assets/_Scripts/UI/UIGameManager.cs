using System.Collections;
using TMPro;
using UnityEngine;

public class UIGameManager : MonoBehaviour
{
    public static UIGameManager instance;

    [SerializeField] private GameplayData _gameplayData;
    [SerializeField] private TextMeshProUGUI _health;
    [SerializeField] private TextMeshProUGUI _shield;
    [SerializeField] private TextMeshProUGUI _ammo;
    [SerializeField] private TextMeshProUGUI _reverse;
    [SerializeField] private TextMeshProUGUI _time;
    [SerializeField] private TextMeshProUGUI _cash;
    [SerializeField] private TextMeshProUGUI _buy;
    [SerializeField] private GameObject _tableBuyItem;
    [SerializeField] private GameObject _tableResult;

    [Header("Weapon Management UI")]
    [SerializeField] private TextMeshProUGUI[] _textWeaponType;
    [SerializeField] private TextMeshProUGUI[] _textWeaponName;
    [SerializeField] private GameObject[] _weaponList;
    [SerializeField] private GameObject[] _weaponPreview;
    [SerializeField] private GameObject[] _weaponData;
    [SerializeField] private int[] _weaponTypeStartIndices;

    [Header("Component Dynamic")]
    public WeaponShootController _weaponPrimaryController;
    public WeaponShootController _weaponSecondaryController;
    public WeaponMeleeController _weaponMeleeController;
    public WeaponThrowController _weaponThrowController;

    [Header("Flash Color")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _flashColor = Color.green;

    public float _timeCount;
    public int _currentRound;
    public bool _isMatchEnd = false;
    public int _indexWeaponListOpen = -1;

    private int _currentWeaponIndex = -1;
    private float _flashDuration = 0.1f;



    private void Awake()
    {
        instance = this;
        _timeCount = _gameplayData.timeCountdown;
        _currentRound = 0;
    }

    private void Update()
    {
        UpdateUIPlayerHealth();
        UpdateUIWeaponAmmo();
        UpdateUITime();
        UpdateUICash();
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
        GameManager.instance.BuyWeapon(_currentWeaponIndex, GameManager.instance._playerController, GameManager.instance._playerInventory);
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

    private void UpdateUIPlayerHealth()
    {
        _health.text = Mathf.CeilToInt(GameManager.instance._playerHealth._currentHealth).ToString();
    }

    private void UpdateUIWeaponAmmo()
    {
        if (GameManager.instance._playerController._currentItem == ItemType.PrimaryItem)
        {
            if (_weaponPrimaryController != null)
            {
                _ammo.text = _weaponPrimaryController._currentAmmo.ToString();
                _reverse.text = _weaponPrimaryController._currentReverse.ToString();
            }
            else
            {
                _ammo.text = "0";
                _reverse.text = "0";
            }
        }
        else if (GameManager.instance._playerController._currentItem == ItemType.SecondaryItem)
        {
            if (_weaponSecondaryController != null)
            {
                _ammo.text = _weaponSecondaryController._currentAmmo.ToString();
                _reverse.text = _weaponSecondaryController._currentReverse.ToString();
            }
            else
            {
                _ammo.text = "0";
                _reverse.text = "0";
            }
        }
        else if (GameManager.instance._playerController._currentItem == ItemType.MeleeItem)
        {
            if (_weaponMeleeController != null)
            {
                _ammo.text = _weaponMeleeController._currentAmmo.ToString();
                _reverse.text = _weaponMeleeController._currentReverse.ToString();
            }
            else
            {
                _ammo.text = "0";
                _reverse.text = "0";
            }
        }
        else if (GameManager.instance._playerController._currentItem == ItemType.ThrowItem)
        {
            if (_weaponThrowController != null)
            {
                _ammo.text = _weaponThrowController._currentAmmo.ToString();
                _reverse.text = _weaponThrowController._currentReverse.ToString();
            }
            else
            {
                _ammo.text = "0";
                _reverse.text = "0";
            }
        }
    }

    private void UpdateUITime()
    {
        if (GameManager.instance._currentGameState == GameState.MatchEnd) return;

        if (_timeCount > 0)
        {
            _timeCount -= Time.deltaTime;

            if (_timeCount < 0)
            {
                _timeCount = 0;
            }

            int minutes = Mathf.FloorToInt(_timeCount / 60);
            int seconds = Mathf.FloorToInt(_timeCount % 60);

            if (GameManager.instance._currentGameState == GameState.Countdown)
            {
                _time.text = $"<color=#FF0000>{minutes:00} : {seconds:00}</color>";
            }
            else if (GameManager.instance._currentGameState == GameState.RoundActive)
            {
                _time.text = $"{minutes:00} : {seconds:00}";
            }
        }
        else
        {
            if (_currentRound ==_gameplayData.totalRound)
            {
                _isMatchEnd = true;
            }
            else
            {
                if (GameManager.instance._currentGameState == GameState.Countdown)
                {
                    _timeCount = _gameplayData.timeCountdown;
                }
                else if (GameManager.instance._currentGameState == GameState.RoundActive)
                {
                    _currentRound++;
                    _timeCount = _gameplayData.timeRoundActive;
                }
            }
        }
    }

    private void UpdateUICash()
    {
        _cash.text = "$" + GameManager.instance._playerController._currentCash.ToString();
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
