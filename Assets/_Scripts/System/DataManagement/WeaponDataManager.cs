using UnityEngine;

public class WeaponDataManager : MonoBehaviour
{
    public static WeaponDataManager instance;

    public WeaponStatsSO[] weaponStatsSO;
    public WeaponRigSO[] weaponRigSO;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public WeaponStatsSO GetWeaponStatsByID(int id)
    {
        foreach (var weapon in weaponStatsSO)
        {
            if (weapon.weaponID == id)
                return weapon;
        }
        return null;
    }

    public WeaponRigSO GetWeaponRigByID(int id)
    {
        foreach(var weapon in weaponRigSO)
        {
            if (weapon.weaponID == id)
                return weapon;
        }
        return null;
    }
}
