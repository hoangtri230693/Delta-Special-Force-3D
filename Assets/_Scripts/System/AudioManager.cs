using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SFXType
{
    MetalClick,
    RadioBeep,
    DefaultClick,
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
    [SerializeField] private AudioClip[] _radioReadyMissionTeam;
    [SerializeField] private AudioClip[] _radioReadyMissionZombie;
    [SerializeField] private AudioClip[] _radioStartMissionTeam;
    [SerializeField] private AudioClip[] _radioStartMissionZombie;
    [SerializeField] private AudioClip[] _radioEndWinMissionTeam;
    [SerializeField] private AudioClip[] _radioEndWinMissionZombie;
    [SerializeField] private AudioClip[] _radioEndLoseMissionTeam;
    [SerializeField] private AudioClip[] _radioEndLoseMissionZombie;

    private int _currentMusicTracksIndex = 0;
    private Coroutine _musicCoroutine;
    private Coroutine _radioCoroutine;



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
        else if (sceneName == "PlayGame" || sceneName == "TeamDeathmatch" || sceneName == "ZombieSurvival")
        {
            PlayMusic(_musicBackground, true);
        }
        else
        {
            _musicSounds.Stop();
        }

        if (_radioCoroutine != null)
        {
            StopCoroutine(_radioCoroutine);
            _radioCoroutine = null;
        }

        if (_sfxSounds.isPlaying)
        {
            _sfxSounds.Stop();
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
    private IEnumerator PlayRadioSequence(AudioClip[] clips)
    {
        foreach (AudioClip clip in clips)
        {
            _sfxSounds.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
        }
        _radioCoroutine = null;
    }

    public void PlaySfx(SFXType _sfxType)
    {
        if (_sfxClips.Length > (int)_sfxType)
        {
            _sfxSounds.PlayOneShot(_sfxClips[(int)_sfxType]);
        }
    }

    public void PlayRadioOnReadyMission()
    {
        if (GameManager_TeamDeathmatch.instance != null)
        {
            _radioCoroutine = StartCoroutine(PlayRadioSequence(_radioReadyMissionTeam));
        }
        else if (GameManager_ZombieSurvival.instance != null)
        {
            _radioCoroutine = StartCoroutine(PlayRadioSequence(_radioReadyMissionZombie));
        }     
    }

    public void PlayRadioOnStartMission()
    {       
        if (GameManager_TeamDeathmatch.instance != null)
        {
            _radioCoroutine = StartCoroutine(PlayRadioSequence(_radioStartMissionTeam));
        }
        else if (GameManager_ZombieSurvival.instance != null)
        {
            _radioCoroutine = StartCoroutine(PlayRadioSequence(_radioStartMissionZombie));
        }
    }

    public void PlayRadioOnEndMission(string resultMatch)
    {
        if (resultMatch == "WIN")
        {
            if (GameManager_TeamDeathmatch.instance != null)
            {
                _radioCoroutine = StartCoroutine(PlayRadioSequence(_radioEndWinMissionTeam));
            }
            else if (GameManager_ZombieSurvival.instance != null)
            {
                _radioCoroutine = StartCoroutine(PlayRadioSequence(_radioEndWinMissionZombie));
            }
        }
        else if (resultMatch == "LOSE")
        {
            if (GameManager_TeamDeathmatch.instance != null)
            {
                _radioCoroutine = StartCoroutine(PlayRadioSequence(_radioEndLoseMissionTeam));
            }
            else if (GameManager_ZombieSurvival.instance != null)
            {
                _radioCoroutine = StartCoroutine(PlayRadioSequence(_radioEndLoseMissionZombie));
            }
        }
    }

    #endregion
}
