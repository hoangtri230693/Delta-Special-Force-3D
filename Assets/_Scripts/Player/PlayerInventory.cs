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
        WeaponController weapon = GetWeaponInSlot(itemType);
        if (weapon != null)
        {
            weapon.DropWeapon();
        }
    }

    public void ActiveCombatItem(ItemType itemType, CombatState combatState)
    {
        WeaponController weapon = GetWeaponInSlot(itemType);
        if (weapon != null)
        {
            weapon.ActiveCombat(combatState);
        }
    }

    public void UpdateHandleZoom(ItemType itemType, float zoomDelta)
    {
        WeaponController weapon = GetWeaponInSlot(itemType);
        if (weapon != null)
        {
            weapon.ZoomControl(zoomDelta);
        }
    }

    public bool HasWeapon(ItemType itemType)
    {
        return GetWeaponInSlot(itemType) != null;
    }

    private WeaponController GetWeaponInSlot(ItemType itemType)
    {
        GameObject slot = itemType switch
        {
            ItemType.PrimaryItem => _primaryItem,
            ItemType.SecondaryItem => _secondaryItem,
            ItemType.MeleeItem => _meleeItem,
            ItemType.ThrowItem => _throwItem,
            _ => null
        };

        if (slot == null) return null;

        return slot.GetComponentInChildren<WeaponController>();
    }
}