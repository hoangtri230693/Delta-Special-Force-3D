using UnityEngine;
using UnityEngine.UI;

public class FullscreenSettings : MonoBehaviour
{
    [SerializeField] private Toggle _fullscreenToggle;

    private void Start()
    {
        bool _isFullscreen = PlayerPrefs.GetInt("Fullscreen", 0) == 1;
        _fullscreenToggle.isOn = _isFullscreen;
        _fullscreenToggle.onValueChanged.AddListener(ApplyFullScreen);
    }

    private void ApplyFullScreen(bool _isFullscreen)
    {
        if (_isFullscreen)
        {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            PlayerPrefs.SetInt("Fullscreen", 1);
            ResolutionSettings.instance.SetResolution(PlayerPrefs.GetInt("FullscreenIndex", 0));
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            PlayerPrefs.SetInt("Fullscreen", 0);
            ResolutionSettings.instance.SetResolution(PlayerPrefs.GetInt("WindowIndex", 0));
        }
    }
}
