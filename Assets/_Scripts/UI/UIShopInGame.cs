using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public enum ShopState
{
    SelectType,
    SelectWeapon
}

public class UIShopInGame : MonoBehaviour
{
    public static UIShopInGame instance;

    [Header("Data")]
    [SerializeField] private WeaponStatsSO[] _weaponStatsData;

    [Header("UI - Weapon Type (1-9)")]
    [SerializeField] private TextMeshProUGUI[] _weaponTypeSlots;

    [Header("UI - Weapon List (1-9)")]
    [SerializeField] private TextMeshProUGUI[] _weaponMenuSlots;

    [Header("Preview & Stats")]
    [SerializeField] private Transform _weaponPreview;
    [SerializeField] private TextMeshProUGUI[] _weaponStats;
    [SerializeField] private TextMeshProUGUI[] _weaponData;
    [SerializeField] private GameObject _backgroundLocked;
    [SerializeField] private TextMeshProUGUI _textCash;
    [SerializeField] private TextMeshProUGUI _textBuy;

    [Header("Flash Color")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _flashColor = Color.green;
    [SerializeField] private float _flashDuration = 0.1f;

    private Dictionary<WeaponType, List<WeaponStatsSO>> _weaponByType;
    private List<WeaponType> _availableTypes;

    private WeaponType _currentType;
    private WeaponStatsSO _currentWeapon;
    private GameObject _currentPreview;

    public ShopState shopState = ShopState.SelectType;


    private void Awake()
    {
        instance = this;

        _weaponStatsData = _weaponStatsData
            .OrderBy(w => w.weaponID)
            .ToArray();

        _weaponByType = _weaponStatsData
            .GroupBy(w => w.weaponType)
            .ToDictionary(g => g.Key, g => g.ToList());

        _availableTypes = _weaponByType.Keys.ToList();
    }

    // ================= SHOP FLOW =================
    public void OnEnableTable()
    {
        shopState = ShopState.SelectType;
        SetupWeaponTypeUI();
        ClearWeaponListUI();
        HidePreviewAndStats();
        UpdateCash();
    }

    public void OnEscape()
    {
        if (shopState == ShopState.SelectWeapon)
        {
            shopState = ShopState.SelectType;
            ClearWeaponListUI();
            HidePreviewAndStats();
        }
    }

    // ================= INPUT =================
    public void OnNumberInput(int index)
    {
        if (index < 0 || index > 9) return;

        if (shopState == ShopState.SelectType)
        {
            SelectWeaponType(index);
        }
        else
        {
            SelectWeapon(index);
        }
    }

    // ================= WEAPON TYPE =================
    private void SetupWeaponTypeUI()
    {
        for (int i = 0; i < _weaponTypeSlots.Length; i++)
        {
            if (i < _availableTypes.Count)
            {
                _weaponTypeSlots[i].text = _availableTypes[i].ToString().ToUpper();
                _weaponTypeSlots[i].gameObject.SetActive(true);
            }
            else
            {
                _weaponTypeSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private void SelectWeaponType(int index)
    {
        if (index >= _availableTypes.Count) return;

        _currentType = _availableTypes[index];
        shopState = ShopState.SelectWeapon;

        List<WeaponStatsSO> weapons = _weaponByType[_currentType];

        for (int i = 0; i < _weaponMenuSlots.Length; i++)
        {
            if (i < weapons.Count)
            {
                _weaponMenuSlots[i].text = weapons[i].weaponShortName;
                _weaponMenuSlots[i].gameObject.SetActive(true);
            }
            else
            {
                _weaponMenuSlots[i].gameObject.SetActive(false);
            }
        }
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
        StartCoroutine(FlashAndFadeColor(_weaponTypeSlots[index]));
        HidePreviewAndStats();
    }

    private void ClearWeaponListUI()
    {
        foreach (var slot in _weaponMenuSlots)
            slot.gameObject.SetActive(false);
    }

    // ================= WEAPON =================
    private void SelectWeapon(int index)
    {
        var weapons = _weaponByType[_currentType];
        if (index >= weapons.Count) return;

        _currentWeapon = weapons[index];

        ShowPreview(_currentWeapon);
        ShowStats(_currentWeapon);
        UpdateLockedState(_currentWeapon.weaponID);

        AudioManager.instance.PlaySfx(SFXType.MetalClick);
        StartCoroutine(FlashAndFadeColor(_weaponMenuSlots[index]));

        if (GameManager_TeamDeathmatch.instance != null)
        {
            var playerController = GameManager_TeamDeathmatch.instance._playerController;
            playerController._itemType = _currentWeapon.itemType;
        }
        if (GameManager_ZombieSurvival.instance != null)
        {
            var playerController = GameManager_ZombieSurvival.instance._playerController;
            playerController._itemType = _currentWeapon.itemType;
        }
    }

    public void OnClickBuy()
    {
        if (_currentWeapon == null) return;

        int id = _currentWeapon.weaponID;

        if (!IsUnlocked(id)) return;

        if (GameManager_TeamDeathmatch.instance != null)
        {
            BuyWeapon(id, GameManager_TeamDeathmatch.instance._player.GetComponent<PlayerController>(),
                          GameManager_TeamDeathmatch.instance._player.GetComponent<PlayerInventory>(),
                          GameManager_TeamDeathmatch.instance._player.GetComponent<PlayerHealth>());
        }
        if (GameManager_ZombieSurvival.instance != null)
        {
            BuyWeapon(id, GameManager_ZombieSurvival.instance._player.GetComponent<PlayerController>(),
                          GameManager_ZombieSurvival.instance._player.GetComponent<PlayerInventory>(),
                          GameManager_ZombieSurvival.instance._player.GetComponent<PlayerHealth>());
        }

        AudioManager.instance.PlaySfx(SFXType.MetalClick);
        StartCoroutine(FlashAndFadeColor(_textBuy));
    }

    public void BuyWeapon(int weaponID, PlayerController playerController, 
                                        PlayerInventory playerInventory, 
                                        PlayerHealth playerHealth)
    {
        WeaponStatsSO weapon = WeaponDataManager.instance.weaponStats
            .FirstOrDefault(w => w.weaponID == weaponID);

        if (playerController._currentCash < weapon.cash)
            return;

        Transform inventory = null;

        switch (weapon.itemType)
        {
            case ItemType.PrimaryItem:
                inventory = playerInventory._primaryItem.transform;
                playerController._itemType = ItemType.PrimaryItem;
                break;

            case ItemType.SecondaryItem:
                inventory = playerInventory._secondaryItem.transform;
                playerController._itemType = ItemType.SecondaryItem;
                break;

            case ItemType.ThrowItem:
                inventory = playerInventory._throwItem.transform;
                playerController._itemType = ItemType.ThrowItem;
                break;

            case ItemType.ArmorItem:
                playerHealth._currentArmorHealth = weapon.armorHealth;

                if (playerHealth == GameManager_TeamDeathmatch.instance?._playerHealth)
                {
                    UIGameManager_TeamDeathmatch.instance
                        .UpdateUIArmorHealth(playerHealth._currentArmorHealth, playerHealth);
                }
                else if (playerHealth == GameManager_ZombieSurvival.instance?._playerHealth)
                {
                    UIGameManager_ZombieSurvival.instance
                        .UpdateUIArmorHealth(playerHealth._currentArmorHealth, playerHealth);
                }
                break;
        }

        if (inventory != null && inventory.childCount > 0)
        {
            for (int i = inventory.childCount - 1; i >= 0; i--)
            {
                Transform child = inventory.GetChild(i);
                WeaponManager weaponManager = child.GetComponent<WeaponManager>();

                if (weaponManager != null)
                {
                    weaponManager.DropWeapon();
                }
            }
        }

        if (inventory != null)
        {
            GameObject weaponPrefab = Instantiate(weapon.weaponPrefab);
            weaponPrefab.transform.SetParent(inventory);
            weaponPrefab.transform.localPosition = Vector3.zero;
            weaponPrefab.transform.localRotation = Quaternion.identity;
            weaponPrefab.GetComponent<WeaponManager>()._playerOwner = playerController.gameObject;
        }

        playerController._currentCash -= weapon.cash;
        playerController._itemType = weapon.itemType;
        if (weapon.itemType != ItemType.ArmorItem)
            playerController.SwitchItem();
    }

    private void ShowPreview(WeaponStatsSO weapon)
    {
        if (_currentPreview)
            Destroy(_currentPreview);

        _currentPreview = Instantiate(weapon.weaponModel);
        _currentPreview.transform.SetParent(_weaponPreview, true);
    }

    private void ShowStats(WeaponStatsSO w)
    {
        HideAllStats();

        switch (w.weaponType)
        {
            case WeaponType.Pistol:
            case WeaponType.Shotgun:
            case WeaponType.SMG:
            case WeaponType.Rifle:
            case WeaponType.Sniper:
                ShowGunStats(w);
                break;

            case WeaponType.Throwable:
                ShowThrowableStats(w);
                break;

            case WeaponType.Armor:
                ShowArmorStats(w);
                break;
        }
    }

    private void ShowGunStats(WeaponStatsSO w)
    {
        SetStat(0, "Damage:", w.damage);
        SetStat(1, "Fire Rate:", $"{w.fireRate} RPM");
        SetStat(2, "Recoil:", w.recoilAmount);
        SetStat(3, "Price:", $"${w.cash}");
    }

    private void ShowThrowableStats(WeaponStatsSO w)
    {
        SetStat(0, "Damage", w.damage);
        SetStat(1, "Radius", $"{w.attackRadius}m");
        SetStat(2, "Force", w.shakeIntensity);
        SetStat(3, "Price:", $"${w.cash}");
    }

    private void ShowArmorStats(WeaponStatsSO w)
    {
        SetStat(0, "Health", w.armorHealth);
        SetStat(1, "Price:", $"${w.cash}");
    }

    private void SetStat(int index, string label, object value)
    {
        if (index >= _weaponStats.Length) return;

        _weaponStats[index].text = label + ":";
        _weaponData[index].text = value.ToString();
        _weaponStats[index].gameObject.SetActive(true);
        _weaponData[index].gameObject.SetActive(true);
    }

    private void HideAllStats()
    {
        for (int i = 0; i < _weaponStats.Length; i++)
        {
            _weaponStats[i].gameObject.SetActive(false);
            _weaponData[i].gameObject.SetActive(false);
        }
    }

    private void HidePreviewAndStats()
    {
        if (_currentPreview)
            Destroy(_currentPreview);

        HideAllStats();
        _backgroundLocked.SetActive(false);
    }

    private bool IsUnlocked(int id)
    {
        return PlayerDataManager.instance.playerSaveData.UnlockedWeaponIDs.Contains(id);
    }

    public void UpdateCash()
    {
        int cash = 0;

        if (GameManager_TeamDeathmatch.instance != null)
        {
            cash = GameManager_TeamDeathmatch.instance._playerController._currentCash;
        }
        else if (GameManager_ZombieSurvival.instance != null)
        {
            cash = GameManager_ZombieSurvival.instance._playerController._currentCash;
        }

        _textCash.text = "$" + cash;
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

    private void UpdateLockedState(int weaponID)
    {
        bool unlocked = IsUnlocked(weaponID);

        if (_backgroundLocked != null)
            _backgroundLocked.SetActive(!unlocked);
    }
}
