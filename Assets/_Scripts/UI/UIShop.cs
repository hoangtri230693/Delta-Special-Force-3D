using DeltaSpecialForce3D.Enums;
using TMPro;
using UnityEngine;


public class UIShop : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform _weaponPreview;
    [SerializeField] private TextMeshProUGUI _weaponName;   
    [SerializeField] private TextMeshProUGUI[] _weaponStats;
    [SerializeField] private TextMeshProUGUI[] _weaponData;
    [SerializeField] private GameObject _backgroundLocked;
    [SerializeField] private TextMeshProUGUI _textGoldCount;

    private int _currentWeaponID;
    private WeaponStatsSO _currentWeapon;
    private GameObject _currentPreview;

    // =================== UNITY ===================

    private void OnEnable()
    {
        _currentWeaponID = 0;
        ShowWeaponByID(_currentWeaponID);
        UpdatePlayerGold();
    }

    // =================== BUTTON ===================
    public void OnClickNext()
    {
        _currentWeaponID = (_currentWeaponID + 1) % WeaponDataManager.instance.weaponStatsSO.Length;
        ShowWeaponByID(_currentWeaponID);
    }

    public void OnClickPrevious()
    {
        _currentWeaponID--;
        if (_currentWeaponID < 0)
            _currentWeaponID = WeaponDataManager.instance.weaponStatsSO.Length - 1;

        ShowWeaponByID(_currentWeaponID);
    }

    public void OnClickPurchase()
    {
        int weaponID = _currentWeapon.weaponID;

        if (IsUnlocked(weaponID))
            return;

        int gold = PlayerDataManager.instance.playerSaveData.Gold;

        if (gold >= _currentWeapon.gold)
        {
            PlayerDataManager.instance.playerSaveData.Gold -= _currentWeapon.gold;
            PlayerDataManager.instance.playerSaveData.UnlockedWeaponIDs.Add(weaponID);
            PlayerDataManager.instance.SaveData();

            UpdatePlayerGold();
            UpdateLockedState(weaponID);

            AudioManager.instance.PlaySfx(SFXSoundType.DefaultClick);
        }
        else
        {
            AudioManager.instance.PlaySfx(SFXSoundType.MetalClick);
        }
    }


    // =================== CORE ===================
    private void ShowWeaponByID(int id)
    {
        foreach (var weapon in WeaponDataManager.instance.weaponStatsSO)
        {
            if (weapon.weaponID == id)
                _currentWeapon = weapon;
        }

        _weaponName.text = _currentWeapon.weaponName;

        ShowPreview(_currentWeapon);
        HideAllStats();
        ShowStatsByType(_currentWeapon);

        UpdateLockedState(_currentWeapon.weaponID);
    }

    private void UpdateLockedState(int weaponID)
    {
        bool unlocked = IsUnlocked(weaponID);

        if (_backgroundLocked != null)
            _backgroundLocked.SetActive(!unlocked);
    }

    private bool IsUnlocked(int weaponID)
    {
        return PlayerDataManager.instance.playerSaveData.UnlockedWeaponIDs
            .Contains(weaponID);
    }

    private void UpdatePlayerGold()
    {
        if (_textGoldCount != null)
            _textGoldCount.text = PlayerDataManager.instance.playerSaveData.Gold.ToString();
    }


    // =================== PREVIEW ===================
    private void ShowPreview(WeaponStatsSO weapon)
    {
        if (_currentPreview)
            DestroyImmediate(_currentPreview);

        _currentPreview = Instantiate(weapon.weaponModel);
        _currentPreview.transform.SetParent(_weaponPreview, true);
    }

    private void ShowStatsByType(WeaponStatsSO weapon)
    {
        switch (weapon.weaponType)
        {
            case WeaponType.Pistol:
            case WeaponType.Shotgun:
            case WeaponType.SMG:
            case WeaponType.Assault:
            case WeaponType.Sniper:
                ShowGunStats(weapon);
                break;

            case WeaponType.Throwable:
                ShowThrowableStats(weapon);
                break;

            case WeaponType.Armor:
                ShowArmorStats(weapon);
                break;
        }
    }

    private void ShowGunStats(WeaponStatsSO weapon)
    {
        SetStat(0, "Damage:", weapon.damage.ToString());
        SetStat(1, "Fire Rate:", $"{weapon.fireRate} RPM");
        SetStat(2, "Recoil:", weapon.recoilAmount.ToString("0.0"));
        SetStat(3, "Gold:", weapon.gold.ToString());
        SetStat(4, "Type:", weapon.weaponType.ToString());
    }

    private void ShowThrowableStats(WeaponStatsSO weapon)
    {
        SetStat(0, "Damage:", weapon.damage.ToString());
        SetStat(1, "Radius:", weapon.attackRadius.ToString());
        SetStat(2, "Force:", weapon.throwForce.ToString());
        SetStat(3, "Gold:", weapon.gold.ToString());
        SetStat(4, "Type:", weapon.weaponType.ToString());
    }

    private void ShowArmorStats(WeaponStatsSO weapon)
    {
        SetStat(0, "Health:", weapon.armorHealth.ToString());
        SetStat(1, "Gold:", weapon.gold.ToString());
        SetStat(2, "Type:", weapon.weaponType.ToString());
    }


    // =================== UTILS ===================
    private void HideAllStats()
    {
        for (int i = 0; i < _weaponStats.Length; i++)
        {
            _weaponStats[i].gameObject.SetActive(false);
            _weaponData[i].gameObject.SetActive(false);
        }
    }

    private void SetStat(int index, string label, string value)
    {
        _weaponStats[index].text = label;
        _weaponData[index].text = value;

        _weaponStats[index].gameObject.SetActive(true);
        _weaponData[index].gameObject.SetActive(true);
    }
}
