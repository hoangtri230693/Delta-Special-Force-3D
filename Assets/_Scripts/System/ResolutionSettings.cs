using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResolutionSettings : MonoBehaviour
{
    public static ResolutionSettings instance;

    [SerializeField] private TMP_Dropdown _resolutionDropdown;

    private List<Resolution> _resolutions;
    private int _currentResolutionIndex = 0;

    private void Awake()
    {
        instance = this;    
    }

    private void Start()
    {
        _resolutions = new List<Resolution>(Screen.resolutions);

        List<string> options = new List<string>();
        int _savedWidth = PlayerPrefs.GetInt("ResolutionWidth", Screen.width);
        int _savedHeight = PlayerPrefs.GetInt("ResolutionHeight", Screen.height);

        for (int i = 0; i < _resolutions.Count; i++)
        {
            string _option = _resolutions[i].width + " x " + _resolutions[i].height;
            options.Add(_option);

            if (_resolutions[i].width == _savedWidth && _resolutions[i].height == _savedHeight)
            {
                _currentResolutionIndex = i;

                if (_resolutions[i].width == Screen.currentResolution.width &&
                    _resolutions[i].height == Screen.currentResolution.height)
                {
                    PlayerPrefs.SetInt("FullscreenIndex", i);
                }

                if (_resolutions[i].width == 800 && _resolutions[i].height == 600)
                {
                    PlayerPrefs.SetInt("WindowIndex", i);
                }
            }
        }

        _resolutionDropdown.ClearOptions();
        _resolutionDropdown.AddOptions(options);

        SetResolution(_currentResolutionIndex);
    
        _resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    public void SetResolution(int _resolutionIndex)
    {
        _resolutionDropdown.value = _resolutionIndex;
        _resolutionDropdown.RefreshShownValue();
        Resolution _resolution = _resolutions[_resolutionIndex];
        bool _isFullscreen = PlayerPrefs.GetInt("Fullscreen", 0) == 1;
        Screen.SetResolution(_resolution.width, _resolution.height, _isFullscreen);
        PlayerPrefs.SetInt("ResolutionWidth", _resolution.width);
        PlayerPrefs.SetInt("ResolutionHeight", _resolution.height);
    }
}
