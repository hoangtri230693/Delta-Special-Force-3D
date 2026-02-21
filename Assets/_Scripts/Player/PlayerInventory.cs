using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory")]
    public GameObject _primaryItem;
    public GameObject _secondaryItem;
    public GameObject _meleeItem;
    public GameObject _throwItem;


    public void UpdateItem(ItemType itemType)
    {
        _primaryItem.SetActive(itemType == ItemType.PrimaryItem);
        _secondaryItem.SetActive(itemType == ItemType.SecondaryItem);
        _meleeItem.SetActive(itemType == ItemType.MeleeItem);
        _throwItem.SetActive(itemType == ItemType.ThrowItem);
    }

    public void DropCurrentItem(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.PrimaryItem:
                var weaponPrimary = _primaryItem.GetComponentInChildren<WeaponManager>();
                weaponPrimary.DropWeapon();
                break;
            case ItemType.SecondaryItem:
                var weaponSecondary = _secondaryItem.GetComponentInChildren<WeaponManager>();
                weaponSecondary.DropWeapon();
                break;
        }
    }
}