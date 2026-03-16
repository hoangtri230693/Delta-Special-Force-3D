using UnityEngine;

public enum WeaponType { Pistol, Shotgun, SMG, AssaultRifle, SniperRifle, Throwable, Armor, Melee  }

[CreateAssetMenu(fileName = "WeaponStatsSO", menuName = "Scriptable Objects/WeaponStatsSO")]
public class WeaponStatsSO : ScriptableObject
{
    [Header("Basic Info")]
    public int weaponID;
    public string weaponName;
    public string weaponShortName;
    public GameObject weaponPrefab;
    public GameObject weaponModel;
    public WeaponType weaponType;
    public ItemType itemType;
    public int cash;
    public int gold;

    [Header("Physics & Targeting")]
    public LayerMask targetMask;
    public float maxDistance;
    public float shootForce;
    public float explosionForce;

    [Header("Range & Damage")]
    public float damage;
    public float attackRadius;

    [Header("Handling & Control")]
    public int ammoPerMag;
    public int ammoReverse;
    public float fireRate;
    public float recoilAmount;  
    public float shakeIntensity;
    public float throwForce;
    public float armorHealth;

    [Header("Effects")]  
    public ParticleSystem muzzleFlash;
    public ParticleSystem fireSmoke;
    public ParticleSystem bulletImpact;
    public ParticleSystem explosionGrenade;
    public GameObject shellCasing;

    [Header("Sounds")]
    public AudioClip cockSound;
    public AudioClip attackSound;
    public AudioClip reloadSound;
    public AudioClip explosionSound;
    public AudioClip dryFireSound;
}
