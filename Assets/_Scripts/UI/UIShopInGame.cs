using DeltaSpecialForce3D.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class UIShopInGame : MonoBehaviour
{
    public static UIShopInGame instance;

    [SerializeField] private GameObject _shopInGame;

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
    private float _flashDuration = 0.2f;

    private Dictionary<WeaponType, List<WeaponStatsSO>> _weaponByType;
    private List<WeaponType> _availableType;

    private WeaponType _currentWeaponType;
    private WeaponStatsSO _currentWeapon;
    private GameObject _currentPreview;
    private ShopState shopState = ShopState.SelectType;


    private void Awake()
    {
        instance = this;

        _weaponByType = WeaponDataManager.instance.weaponStatsSO
            .GroupBy(w => w.weaponType)
            .ToDictionary(g => g.Key, g => g.ToList());

        _availableType = _weaponByType.Keys.ToList();
    }

    // ================= SHOP FLOW =================
    public void OnEnableTable(int currentCash)
    {
        shopState = ShopState.SelectType;
        SetupWeaponTypeUI();
        ClearWeaponListUI();
        HidePreviewAndStats();
        UpdateUICash(currentCash);
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

    public void OnClickBuy()
    {
        if (_currentWeapon == null) return;

        int id = _currentWeapon.weaponID;

        if (!IsUnlocked(id)) return;

        if (GameManager_TeamDeathmatch.instance != null)
            BuyWeapon(id, GameManager_TeamDeathmatch.instance._player);
        if (GameManager_ZombieSurvival.instance != null)
            BuyWeapon(id, GameManager_ZombieSurvival.instance._player);

        AudioManager.instance.PlaySfx(SFXSoundType.MetalClick);
        StartCoroutine(FlashAndFadeColor(_textBuy));
    }

    public void BuyWeapon(int weaponID, GameObject player)
    {
        WeaponStatsSO weapon = WeaponDataManager.instance.GetWeaponStatsByID(weaponID);
        PlayerController playerController = player.GetComponent<PlayerController>();
        PlayerInventory playerInventory = player.GetComponent<PlayerInventory>();
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        BotAIController botAIController = player.GetComponent<BotAIController>();

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
                playerHealth._currentArmorHealth = Mathf.Clamp(playerHealth._currentArmorHealth + weapon.armorHealth, 0, 100);
                playerHealth.UpdateUIArmorHealth();
                //Debug.Log("Armor Health: " + playerHealth._currentArmorHealth);
                break;
        }

        if (inventory != null && inventory.childCount > 0)
        {


            for (int i = inventory.childCount - 1; i >= 0; i--)
            {
                Transform child = inventory.GetChild(i);
                WeaponController weaponController = child.GetComponent<WeaponController>();

                if (weaponController != null)
                    weaponController.DropWeapon();
            }
        }

        if (inventory != null)
        {
            GameObject weaponPrefab = Instantiate(weapon.weaponPrefab);
            weaponPrefab.transform.SetParent(inventory);
            weaponPrefab.transform.localPosition = Vector3.zero;
            weaponPrefab.transform.localRotation = Quaternion.identity;
            weaponPrefab.GetComponent<WeaponController>().InitializeWeapon(player);
        }

        playerController._currentCash -= weapon.cash;
        if (botAIController != null)
            UpdateUICash(playerController._currentCash);

        if (weapon.itemType != ItemType.ArmorItem)
        {
            playerController._itemType = weapon.itemType;
            playerController.SwitchItem();
        }
    }

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

    public void UpdateUICash(int cash)
    {
        _textCash.text = "$" + cash;
    }

    // ================= WEAPON TYPE =================
    private void SetupWeaponTypeUI()
    {
        for (int i = 0; i < _weaponTypeSlots.Length; i++)
        {
            if (i < _availableType.Count)
            {
                _weaponTypeSlots[i].text = _availableType[i].ToString();
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
        if (index >= _availableType.Count) return;

        _currentWeaponType = _availableType[index];
        shopState = ShopState.SelectWeapon;

        List<WeaponStatsSO> weapons = _weaponByType[_currentWeaponType];

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
        AudioManager.instance.PlaySfx(SFXSoundType.MetalClick);
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
        var weapons = _weaponByType[_currentWeaponType];
        if (index >= weapons.Count) return;

        _currentWeapon = weapons[index];

        ShowPreview(_currentWeapon);
        ShowStats(_currentWeapon);
        UpdateLockedState(_currentWeapon.weaponID);

        AudioManager.instance.PlaySfx(SFXSoundType.MetalClick);
        StartCoroutine(FlashAndFadeColor(_weaponMenuSlots[index]));
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
            case WeaponType.Assault:
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
        SetStat(0, "Damage", w.damage);
        SetStat(1, "Fire Rate", $"{w.fireRate} RPM");
        SetStat(2, "Recoil", w.recoilAmount);
        SetStat(3, "Price", $"${w.cash}");
    }

    private void ShowThrowableStats(WeaponStatsSO w)
    {
        SetStat(0, "Damage", w.damage);
        SetStat(1, "Radius", $"{w.attackRadius}m");
        SetStat(2, "Force", w.shakeIntensity);
        SetStat(3, "Price", $"${w.cash}");
    }

    private void ShowArmorStats(WeaponStatsSO w)
    {
        SetStat(0, "Health", w.armorHealth);
        SetStat(1, "Price", $"${w.cash}");
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
