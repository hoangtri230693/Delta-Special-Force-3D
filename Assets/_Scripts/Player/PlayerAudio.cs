using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] _footStepSounds;
    [SerializeField] private AudioClip _landStepSound;
    [SerializeField] private AudioClip _switchItemSound;
    [SerializeField] private AudioClip _zoomSound;

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
    }

    public void ZoomSound()
    {
        _audioSource.PlayOneShot(_zoomSound);
    }

    public void SwitchItemSound()
    {
        _audioSource.PlayOneShot(_switchItemSound);
    }

    public void FootStep()
    {
        if (Time.time - _lastStepTime < _stepCooldown) return;
        _lastStepTime = Time.time;

        _audioSource.PlayOneShot(_footStepSounds[Random.Range(0, _footStepSounds.Length)]);
    }

    public void LandStep()
    {
        _audioSource.PlayOneShot(_landStepSound);
    }
}
