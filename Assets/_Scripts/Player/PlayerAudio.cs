using DeltaSpecialForce3D.Enums;
using UnityEngine;



public class PlayerAudio : MonoBehaviour
{
    private PlayerController _playerController;
    private AudioSource _audioSource;
    private float _stepCooldown = 0.2f;
    private float _lastStepTime = 0f;


    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        _audioSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    public void PlayCharacterSound(CharacterSoundType type)
    {
        AudioClip clipToPlay = null;
        var stats = _playerController.CharacterStats;
        float volumeScale = 1f;

        switch (type)
        {
            case CharacterSoundType.FootStep:
                if (Time.time - _lastStepTime < _stepCooldown) return;
                _lastStepTime = Time.time;
                clipToPlay = stats.footStepSound[Random.Range(0, stats.footStepSound.Length)];
                volumeScale = 0.2f;
                break;
            case CharacterSoundType.LandStep:
                clipToPlay = stats.landStepSound;
                break;
            case CharacterSoundType.SwitchItem:
                clipToPlay = stats.switchItemSound;
                break;
            case CharacterSoundType.Zoom:
                clipToPlay = stats.zoomSound;
                break;
            case CharacterSoundType.Hurt:
                clipToPlay = stats.hurtSound[Random.Range(0, stats.hurtSound.Length)];
                break;
            case CharacterSoundType.Death:
                clipToPlay = stats.deathSound;
                break;
        }

        if (clipToPlay != null)
        {
            _audioSource.PlayOneShot(clipToPlay, volumeScale);
        }
    }
}
