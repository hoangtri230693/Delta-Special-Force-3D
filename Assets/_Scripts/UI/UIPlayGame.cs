using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIPlayGame : MonoBehaviour
{
    [SerializeField] private GameObject _panelMain;
    [SerializeField] private GameObject _buttonBack;
    [SerializeField] private GameObject _panelShop;
    [SerializeField] private GameObject[] _weaponName;
    [SerializeField] private GameObject[] _weaponPreview;
    [SerializeField] private GameObject[] _weaponData;
    [SerializeField] private GameObject _backgroundLocked;
    [SerializeField] private TextMeshProUGUI _textGoldCount;

    private int _currentShopIndex = 0;

    private void Start()
    {
        _panelShop.SetActive(false);
        _panelMain.SetActive(true);
        _buttonBack.SetActive(true);
    }

    //-------IMPLEMENT PUBLIC METHODS-------//
    public void OnClickPurchase()
    {
        bool isUnlocked = PlayerDataManager.instance.playerSaveData.UnlockedWeaponIDs.Contains(_currentShopIndex);

        if (!isUnlocked)
        {
            if (PlayerDataManager.instance.playerSaveData.Gold >= WeaponDataManager.instance.weaponStats[_currentShopIndex].gold)
            {
                PlayerDataManager.instance.playerSaveData.Gold -= WeaponDataManager.instance.weaponStats[_currentShopIndex].gold;
                PlayerDataManager.instance.playerSaveData.UnlockedWeaponIDs.Add(_currentShopIndex);
                PlayerDataManager.instance.SaveData();
                UpdatePlayerGold();
                UpdateShowShop();
                AudioManager.instance.PlaySfx(SFXType.DefaultClick);
            }
            else
            {
                AudioManager.instance.PlaySfx(SFXType.MetalClick);
            }
        }
    }

    public void OnClickNextShop()
    {
        _currentShopIndex++;
        if (_currentShopIndex >= _weaponPreview.Length)
        {
            _currentShopIndex = 0;
        }
        UpdateShowShop();
    }

    public void OnClickPreviousShop()
    {
        _currentShopIndex--;
        if (_currentShopIndex < 0)
        {
            _currentShopIndex = _weaponPreview.Length - 1;
        }
        UpdateShowShop();
    }

    public void OnClickShop()
    {
        if (_panelShop.activeSelf)
        {
            _panelShop.SetActive(false);
            _buttonBack.SetActive(true);
            _panelMain.SetActive(true);
            _currentShopIndex = 0;
            AudioManager.instance.PlaySfx(SFXType.MetalClick);
        }
        else
        {
            _panelShop.SetActive(true);
            _buttonBack.SetActive(false);
            _panelMain.SetActive(false);
            UpdateShowShop();
            UpdatePlayerGold();
            AudioManager.instance.PlaySfx(SFXType.MetalClick);
        }
    }

    public void OnClickDeathmatch()
    {
        SceneManager.LoadScene("TeamDeathmatch");
        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
    }

    public void OnClickSurvival()
    {
        SceneManager.LoadScene("ZombieSurvival");
        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
    }

    public void OnClickBack()
    {
        SceneManager.LoadScene("StartGame");
        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
    }


    //-------IMPLEMENT PRIVATE METHODS-------//
    private void UpdatePlayerGold()
    {
        _textGoldCount.text = "$" + PlayerDataManager.instance.playerSaveData.Gold.ToString();
    }

    private void UpdateShowShop()
    {
        foreach (GameObject name in _weaponName) name.SetActive(false);
        foreach (GameObject preview in _weaponPreview) preview.SetActive(false);
        foreach (GameObject data in _weaponData) data.SetActive(false);
        _weaponName[_currentShopIndex].SetActive(true);
        _weaponPreview[_currentShopIndex].SetActive(true);
        _weaponData[_currentShopIndex].SetActive(true);

        bool isUnlocked = PlayerDataManager.instance.playerSaveData.UnlockedWeaponIDs.Contains(_currentShopIndex);
        if (isUnlocked)
        {
            _backgroundLocked.SetActive(false);
        }
        else
        {
            _backgroundLocked.SetActive(true);
        }
    }   
}
