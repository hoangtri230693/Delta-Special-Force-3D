using DeltaSpecialForce3D.Enums;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private WeaponController _weaponController;

    [Header("Inventory")]
    public GameObject _primaryItem;
    public GameObject _secondaryItem;
    public GameObject _meleeItem;
    public GameObject _throwItem;


    public void UpdateItem(ItemType itemType)
    {
        _primaryItem.SetActive(false);
        _secondaryItem.SetActive(false);
        _meleeItem.SetActive(false);
        _throwItem.SetActive(false);

        switch (itemType)
        {
            case ItemType.PrimaryItem:
                _primaryItem.SetActive(true);
                _weaponController = GetWeaponInSlot(_primaryItem);
                break;
            case ItemType.SecondaryItem:
                _secondaryItem.SetActive(true);
                _weaponController = GetWeaponInSlot(_secondaryItem);
                break;
            case ItemType.MeleeItem:
                _meleeItem.SetActive(true);
                _weaponController = GetWeaponInSlot(_meleeItem);
                break;
            case ItemType.ThrowItem:
                _throwItem.SetActive(true);
                _weaponController = GetWeaponInSlot(_throwItem);
                break;
        }
        
    }

    public void DropCurrentItem(ItemType itemType)
    {
        if (_weaponController != null)
        {
            _weaponController.DropWeapon();
            _weaponController = null;
        }       
    }

    public void ActiveCombatItem(CombatState combatState)
    {
        if (_weaponController != null)
            _weaponController.HandleActiveCombat(combatState);
    }

    public void UpdateHandleZoom(float zoomDelta)
    {
        if (_weaponController != null)
            _weaponController.HandleZoomControl(zoomDelta);
    }

    public void HandleShoot()
    {
        _weaponController.HandleWeaponShoot();
    }

    public bool HasWeapon()
    {
        return _weaponController != null;
    }

    private WeaponController GetWeaponInSlot(GameObject itemSlot)
    {
        return itemSlot.GetComponentInChildren<WeaponController>();
    }
}