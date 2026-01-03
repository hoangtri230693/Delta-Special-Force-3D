using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SFXType
{
    MetalClick,
    RadioBeep,
    DefaultClick,
    SwatDead,
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicSounds;
    [SerializeField] private AudioSource _sfxSounds;

    [Header("Music Sounds")]
    [SerializeField] private AudioClip[] _musicTracks;
    [SerializeField] private AudioClip _musicBackground;

    [Header("Effect Sounds")]
    [SerializeField] private AudioClip[] _sfxClips;

    private int _currentMusicTracksIndex = 0;
    private Coroutine _musicCoroutine;

    

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        if (_musicCoroutine != null)
        {
            StopCoroutine(_musicCoroutine);
            _musicCoroutine = null;
        }

        if (sceneName == "StartGame" || sceneName == "Settings")
        {
            _musicCoroutine = StartCoroutine(PlayMusicTracks());
        }
        else if (sceneName == "PlayGame" || sceneName == "SinglePlayer" || sceneName == "MultiPlayer")
        {
            PlayMusic(_musicBackground, true);
        }
        else
        {
            _musicSounds.Stop();
        }
    }

    #region Volume and Mute
    public void SetMusicVolume(float volume)
    {
        _musicSounds.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }
    public void SetSFXVolume(float volume)
    {
        _sfxSounds.volume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }
    #endregion

    #region Music Sounds
    private IEnumerator PlayMusicTracks()
    {
        while (true)
        {
            if (_musicTracks.Length == 0) yield break;
            PlayMusic(_musicTracks[_currentMusicTracksIndex], false);
            while (_musicSounds.isPlaying) yield return null;
            _currentMusicTracksIndex = (_currentMusicTracksIndex + 1) % _musicTracks.Length;
        }
    }
    private void PlayMusic(AudioClip _clip, bool _loop)
    {
        if (_musicSounds.clip != _clip)
        {
            _musicSounds.loop = _loop;
            _musicSounds.clip = _clip;
            _musicSounds.Play();
        }
    }
    #endregion

    #region Effect Sounds
    public void PlaySfx(SFXType _sfxType)
    {
        if (_sfxClips.Length > (int)_sfxType)
        {
            _sfxSounds.PlayOneShot(_sfxClips[(int)_sfxType]);
        }
    }
    #endregion
}
