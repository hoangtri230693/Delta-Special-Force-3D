using UnityEngine;

public class ZombieAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] _footStepSounds;
    [SerializeField] private AudioClip[] _growlSounds;
    [SerializeField] private AudioClip[] _attackSounds;
    [SerializeField] private AudioClip[] _attackHitSounds;
    [SerializeField] private AudioClip[] _bodyFallSounds;

    private float _stepCooldown = 0.2f;
    private float _lastStepTime = 0f;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        _audioSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1f);
       PlayGrowlSound();
    }

    public void FootStep()
    {
        if (Time.time - _lastStepTime < _stepCooldown) return;
        _lastStepTime = Time.time;

        _audioSource.PlayOneShot(_footStepSounds[Random.Range(0, _footStepSounds.Length)]);
    }

    public void PlayGrowlSound()
    {
        _audioSource.PlayOneShot(_growlSounds[Random.Range(0, _growlSounds.Length)]);
    }

    public void PlayAttackSound()
    {
        _audioSource.PlayOneShot(_attackSounds[Random.Range(0, _attackSounds.Length)]);
    }

    public void PlayAttackHitSound()
    {
        _audioSource.PlayOneShot(_attackHitSounds[Random.Range(0, _attackHitSounds.Length)]);
    }

    public void PlayBodyFallSound()
    {
        _audioSource.PlayOneShot(_bodyFallSounds[Random.Range(0, _bodyFallSounds.Length)]);
    }
}
