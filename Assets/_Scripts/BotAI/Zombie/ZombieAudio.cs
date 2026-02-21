using UnityEngine;

public class ZombieAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] _footStepSounds;
    [SerializeField] private AudioClip _wave1Sounds;
    [SerializeField] private AudioClip _wave2Sounds;
    [SerializeField] private AudioClip _wave3Sounds;
    [SerializeField] private AudioClip _wave4Sounds;
    [SerializeField] private AudioClip _wave5Sounds;
    [SerializeField] private AudioClip _attackSounds;
    [SerializeField] private AudioClip _attackHitSounds;

    private float _walkStepVolume = 2.0f;
    private float _runStepVolume = 4.0f;
    private float _stepCooldown = 0.2f;
    private float _lastStepTime = 0f;

    private Animator _animator;
    private AudioSource _audioSource;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }

    public void FootStep()
    {
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
}
