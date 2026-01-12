using UnityEngine;
using UnityEngine.UI;

public class FullscreenSettings : MonoBehaviour
{
    [SerializeField] private Toggle _fullscreenToggle;

    private void Start()
    {
        bool _isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        _fullscreenToggle.SetIsOnWithoutNotify(_isFullscreen);

        _fullscreenToggle.onValueChanged.AddListener(ApplyFullScreen);
    }

    private void ApplyFullScreen(bool _isFullscreen)
    {
        PlayerPrefs.SetInt("Fullscreen", _isFullscreen ? 1 : 0);
        PlayerPrefs.Save();

        Screen.fullScreenMode = _isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        if (ResolutionSettings.instance != null)
        {
            string key = _isFullscreen ? "FullscreenIndex" : "WindowIndex";
            int indexToLoad = PlayerPrefs.GetInt(key, 0);

            ResolutionSettings.instance.SetResolution(indexToLoad);
        }
    }
}