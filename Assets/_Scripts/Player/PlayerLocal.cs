using UnityEngine;

public class PlayerLocal : MonoBehaviour
{
    private void Start()
    {
        SetupPlayerLocalForWeapon();
    }

    private void SetupPlayerLocalForWeapon()
    {
        WeaponManager weaponManager = GetComponentInChildren<WeaponManager>();
        weaponManager._playerLocal = this;
    }
}
