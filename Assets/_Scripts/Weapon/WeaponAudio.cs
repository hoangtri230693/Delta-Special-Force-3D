using UnityEngine;

public class WeaponAudio : MonoBehaviour
{
    [SerializeField] private WeaponManager _weaponManager;
    [SerializeField] private AudioSource _audioSource;


    public void PlayAudioFire()
    {
        _audioSource.PlayOneShot(_weaponManager._weaponStats.attackSound);
    }
    public void PlayAudioDryFire()
    {
        _audioSource.PlayOneShot(_weaponManager._weaponStats.dryFireSound);
    }

    public void PlayAudioReload()
    {
        _audioSource.PlayOneShot(_weaponManager._weaponStats.reloadSound);
    }

    public void PlayAudioCock()
    {
        _audioSource.PlayOneShot(_weaponManager._weaponStats.cockSound);
    }   

    public void PlayAudioMelee()
    {
        _audioSource.PlayOneShot(_weaponManager._weaponStats.attackSound);
    }

    public void PlayAudioThrow()
    {
        _audioSource.PlayOneShot(_weaponManager._weaponStats.attackSound);
    }

    public void PlayAudioExplosion()
    {
        _audioSource.PlayOneShot(_weaponManager._weaponStats.explosionSound);
    }
}
