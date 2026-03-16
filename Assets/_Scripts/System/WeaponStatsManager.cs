using UnityEngine;

public class WeaponStatsManager : MonoBehaviour
{
    public static WeaponStatsManager instance;

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
