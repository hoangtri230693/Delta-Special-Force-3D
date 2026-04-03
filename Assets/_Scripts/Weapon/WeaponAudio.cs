using DeltaSpecialForce3D.Enums;
using UnityEngine;


public class WeaponAudio : MonoBehaviour
{
    [SerializeField] private WeaponController _weaponController;
    [SerializeField] private AudioSource _audioSource;

    private void OnEnable()
    {
        _audioSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    public void PlayWeaponSound(WeaponSoundType type)
    {
        AudioClip clipToPlay = null;
        var stats = _weaponController.WeaponStats;

        switch (type)
        {
            case WeaponSoundType.Fire:
                clipToPlay = stats.attackSound;
                break;
            case WeaponSoundType.DryFire:
                clipToPlay = stats.dryFireSound;
                break;
            case WeaponSoundType.Reload:
                clipToPlay = stats.reloadSound;
                break;
            case WeaponSoundType.Cock:
                clipToPlay = stats.cockSound;
                break;
            case WeaponSoundType.Melee:
            case WeaponSoundType.Throw:
                clipToPlay = stats.attackSound;
                break;
            case WeaponSoundType.Explosion:
                clipToPlay = stats.explosionSound;
                break;
        }

        if (clipToPlay != null)
        {
            _audioSource.PlayOneShot(clipToPlay);
        }
    }
}
