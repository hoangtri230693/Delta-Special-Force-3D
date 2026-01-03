using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] _footStepSounds;
    [SerializeField] private AudioClip _landStepSound;
    [SerializeField] private AudioClip _switchItemSound;
    [SerializeField] private AudioClip _zoomSound;

    private float _walkStepVolume = 2.0f;
    private float _runStepVolume = 4.0f;
    private float _landStepVolume = 3.0f;
    private float _stepCooldown = 0.2f;
    private float _lastStepTime = 0f;

    private Animator _animator;
    private AudioSource _audioSource;
    

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
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
        if (_audioSource == null || _footStepSounds.Length == 0) return;
        if (Time.time - _lastStepTime < _stepCooldown) return;
        _lastStepTime = Time.time;

        float speed = _animator.GetFloat("Speed");
        if (speed < 0.1f) return;
        if (speed < 2.5f)
            _audioSource.volume = _walkStepVolume;
        else
            _audioSource.volume = _runStepVolume;

        _audioSource.PlayOneShot(_footStepSounds[Random.Range(0, _footStepSounds.Length)]);
    }

    public void LandStep()
    {
        if (_audioSource != null && _landStepSound != null)
        {
            _audioSource.volume = _landStepVolume;
            _audioSource.PlayOneShot(_landStepSound);
        }
    }
}
