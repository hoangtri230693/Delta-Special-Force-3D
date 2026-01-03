using UnityEngine;


[CreateAssetMenu(fileName = "WeaponStatsSO", menuName = "Scriptable Objects/WeaponStatsSO")]
public class WeaponStatsSO : ScriptableObject
{
    [Header("Basic Info")]
    public string weaponName;
    public GameObject weaponPrefab;   
    public ItemType itemType;
    public int price;

    [Header("Physics & Targeting")]
    public LayerMask targetMask;
    public float maxDistance;
    public float explosionForce;
    public float upwardForce;

    [Header("Range & Damage")]
    public float damage;
    public float attackRadius;

    [Header("Handling & Control")]
    public int ammoPerMag;
    public int ammoReverse;
    public float attackRate;  
    public float reloadTime;
    public float recoilAmount;  
    public float shakeIntensity;
    public float throwForce;

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
