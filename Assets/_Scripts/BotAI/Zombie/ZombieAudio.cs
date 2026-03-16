using UnityEngine;

public enum ZombieSoundType
{
    FootStep,
    Growl,
    Attack,
    Hit,
    Hurt,
    Fall
}

public class ZombieAudio : MonoBehaviour
{
    private float _stepCooldown = 0.2f;
    private float _lastStepTime = 0f;

    private ZombieController _zombieController;
    private AudioSource _audioSource;

    private void Awake()
    {
        _zombieController = GetComponent<ZombieController>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        _audioSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    public void PlayZombieSound(ZombieSoundType type)
    {
        AudioClip clipToPlay = null;
        var stats = _zombieController.ZombieStats;

        switch (type)
        {
            case ZombieSoundType.FootStep:
                if (Time.time - _lastStepTime < _stepCooldown) return;
                _lastStepTime = Time.time;
                clipToPlay = stats.footStepSounds[Random.Range(0, stats.footStepSounds.Length)];
                break;
            case ZombieSoundType.Growl:
                clipToPlay = stats.growlSounds[Random.Range(0, stats.growlSounds.Length)];
                break;
            case ZombieSoundType.Attack:
                clipToPlay = stats.attackSounds[Random.Range(0, stats.attackSounds.Length)];
                break;
            case ZombieSoundType.Hit:
                clipToPlay = stats.hitSounds[Random.Range(0, stats.hitSounds.Length)];
                break;
            case ZombieSoundType.Hurt:
                clipToPlay = stats.hurtSounds[Random.Range(0, stats.hurtSounds.Length)];
                break;
            case ZombieSoundType.Fall:
                clipToPlay = stats.fallSounds[Random.Range(0, stats.fallSounds.Length)];
                break;
        }

        if (clipToPlay != null)
        {
            _audioSource.PlayOneShot(clipToPlay);
        }
    }
}
