using UnityEngine;
using UnityEngine.UI;

public class MusicSettings : MonoBehaviour
{
    [SerializeField] private Slider _sliderMusic;

    private void Start()
    {
        float _savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        _sliderMusic.value = _savedMusicVolume;
        AudioManager.instance.SetMusicVolume(_savedMusicVolume);
        _sliderMusic.onValueChanged.AddListener(AudioManager.instance.SetMusicVolume);
    }
}
