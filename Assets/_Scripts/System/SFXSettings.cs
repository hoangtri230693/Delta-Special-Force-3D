using UnityEngine;
using UnityEngine.UI;

public class SFXSettings : MonoBehaviour
{
    [SerializeField] private Slider _sliderSFX;

    private void Start()
    {
        float _savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
        _sliderSFX.value = _savedSFXVolume;
        AudioManager.instance.SetSFXVolume(_savedSFXVolume);
        _sliderSFX.onValueChanged.AddListener(AudioManager.instance.SetSFXVolume);
    }
}
