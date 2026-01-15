using UnityEngine;

public class WeaponDataManager : MonoBehaviour
{
    public static WeaponDataManager instance;

    public WeaponStatsSO[] weaponStats;

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
}
