using DeltaSpecialForce3D.Enums;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;



public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private AudioDataSO _audioData;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicSounds;
    [SerializeField] private AudioSource _sfxSounds;
    [SerializeField] private AudioSource _radioSounds;

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
            _musicCoroutine = StartCoroutine(PlayAudioTracks());
        }
        else if (sceneName == "PlayGame" || sceneName == "TeamDeathmatch" || sceneName == "ZombieSurvival")
        {
            PlayAudio(_audioData.musicBackground, true);
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

        if (_radioSounds.isPlaying)
        {
            _radioSounds.Stop();
        }
    }

    public void SetMusicVolume(float volume)
    {
        _musicSounds.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        _sfxSounds.volume = volume;
        _radioSounds.volume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    private IEnumerator PlayAudioTracks()
    {
        while (true)
        {
            if (_audioData == null || _audioData.musicTracks.Length == 0) yield break;
            PlayAudio(_audioData.musicTracks[_currentMusicTracksIndex], false);
            while (_musicSounds.isPlaying) yield return null;
            _currentMusicTracksIndex = (_currentMusicTracksIndex + 1) % _audioData.musicTracks.Length;
        }
    }

    private IEnumerator PlayRadio(AudioClip[] clips)
    {
        foreach (AudioClip clip in clips)
        {
            _radioSounds.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
        }
        _radioCoroutine = null;
    }

    private void PlayAudio(AudioClip _clip, bool _loop)
    {
        if (_musicSounds.clip != _clip)
        {
            _musicSounds.loop = _loop;
            _musicSounds.clip = _clip;
            _musicSounds.Play();
        }
    }

    public void StopAudio()
    {
        if (_musicCoroutine != null)
        {
            StopCoroutine(_musicCoroutine);
            _musicCoroutine = null;
        }
        _musicSounds.Stop();
        _musicSounds.clip = null;
    }

    public void PlaySfx(SFXSoundType _sfxType)
    {
        if (_audioData.sfxClips.Length > (int)_sfxType)
        {
            _sfxSounds.PlayOneShot(_audioData.sfxClips[(int)_sfxType]);
        }
    }

    public void PlayRadioTeam(GameState gameState, GameResult gameResult)
    {
        AudioClip[] clipsToPlay = null;

        switch (gameState)
        {
            case GameState.Countdown:
                clipsToPlay = _audioData.teamRadio.ready;
                break;
            case GameState.RoundActive:
                clipsToPlay = _audioData.teamRadio.start;
                break;
            case GameState.MatchEnd:
                switch (gameResult)
                {
                    case GameResult.Win:
                        clipsToPlay = _audioData.teamRadio.win;
                        break;
                    case GameResult.Lose:
                        clipsToPlay = _audioData.teamRadio.lose;
                        break;
                }
                break;
        }

        _radioCoroutine = StartCoroutine(PlayRadio(clipsToPlay));
    }

    public void PlayRadioZombie(GameState gameState, GameResult gameResult)
    {
        AudioClip[] clipsToPlay = null;

        switch (gameState)
        {
            case GameState.Countdown:
                clipsToPlay = _audioData.zombieRadio.ready;
                break;
            case GameState.RoundActive:
                clipsToPlay = _audioData.zombieRadio.start;
                break;
            case GameState.MatchEnd:
                switch (gameResult)
                {
                    case GameResult.Win:
                        clipsToPlay = _audioData.zombieRadio.win;
                        break;
                    case GameResult.Lose:
                        clipsToPlay = _audioData.zombieRadio.lose;
                        break;
                }
                break;
        }

        _radioCoroutine = StartCoroutine(PlayRadio(clipsToPlay));
    }

    public void PlayLore()
    {
        PlayAudio(_audioData.loreClips, false);
    }
}
