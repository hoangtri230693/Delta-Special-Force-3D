using TMPro;
using UnityEngine;
using System.Linq;

public class UIShop : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform _weaponPreview;
    [SerializeField] private TextMeshProUGUI _weaponName;   
    [SerializeField] private TextMeshProUGUI[] _weaponStats;
    [SerializeField] private TextMeshProUGUI[] _weaponData;
    [SerializeField] private GameObject _backgroundLocked;
    [SerializeField] private TextMeshProUGUI _textGoldCount;

    private int _currentIndex;
    private GameObject _currentPreview;

    // =================== UNITY ===================

    private void OnEnable()
    {
        _currentIndex = 0;
        ShowWeaponByIndex(_currentIndex);
        UpdatePlayerGold();
    }

    // =================== BUTTON ===================
    public void OnClickNext()
    {
        _currentIndex = (_currentIndex + 1) % WeaponStatsManager.instance.weaponStats.Length;
        ShowWeaponByIndex(_currentIndex);
    }

    public void OnClickPrevious()
    {
        _currentIndex--;
        if (_currentIndex < 0)
            _currentIndex = WeaponStatsManager.instance.weaponStats.Length - 1;

        ShowWeaponByIndex(_currentIndex);
    }

    public void OnClickPurchase()
    {
        WeaponStatsSO weapon = WeaponStatsManager.instance.weaponStats[_currentIndex];
        int weaponID = weapon.weaponID;

        if (IsUnlocked(weaponID))
            return;

        int gold = PlayerDataManager.instance.playerSaveData.Gold;

        if (gold >= weapon.gold)
        {
            PlayerDataManager.instance.playerSaveData.Gold -= weapon.gold;
            PlayerDataManager.instance.playerSaveData.UnlockedWeaponIDs.Add(weaponID);
            PlayerDataManager.instance.SaveData();

            UpdatePlayerGold();
            UpdateLockedState(weaponID);

            AudioManager.instance.PlaySfx(SFXType.DefaultClick);
        }
        else
        {
            AudioManager.instance.PlaySfx(SFXType.MetalClick);
        }
    }

    // =================== CORE ===================
    private void ShowWeaponByIndex(int index)
    {
        WeaponStatsSO weapon = WeaponStatsManager.instance.weaponStats[index];

        _weaponName.text = weapon.weaponName;

        ShowPreview(weapon);
        HideAllStats();
        ShowStatsByType(weapon);

        UpdateLockedState(weapon.weaponID);
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

    // =================== STATS ===================
    private void ShowStatsByType(WeaponStatsSO weapon)
    {
        switch (weapon.weaponType)
        {
            case WeaponType.Pistol:
            case WeaponType.Shotgun:
            case WeaponType.SMG:
            case WeaponType.AssaultRifle:
            case WeaponType.SniperRifle:
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
