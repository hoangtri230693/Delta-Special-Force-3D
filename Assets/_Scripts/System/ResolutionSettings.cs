using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResolutionSettings : MonoBehaviour
{
    public static ResolutionSettings instance;

    [SerializeField] private TMP_Dropdown _resolutionDropdown;

    private List<Resolution> _filteredResolutions;
    private int _currentResolutionIndex = 0;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Resolution[] allResolutions = Screen.resolutions;
        _filteredResolutions = new List<Resolution>();
        List<string> options = new List<string>();

        for (int i = 0; i < allResolutions.Length; i++)
        {
            if (!_filteredResolutions.Exists(res => res.width == allResolutions[i].width && res.height == allResolutions[i].height))
            {
                _filteredResolutions.Add(allResolutions[i]);
                options.Add(allResolutions[i].width + " x " + allResolutions[i].height);
            }
        }

        _resolutionDropdown.ClearOptions();
        _resolutionDropdown.AddOptions(options);

        int _savedWidth = PlayerPrefs.GetInt("ResolutionWidth", Screen.currentResolution.width);
        int _savedHeight = PlayerPrefs.GetInt("ResolutionHeight", Screen.currentResolution.height);

        for (int i = 0; i < _filteredResolutions.Count; i++)
        {
            if (_filteredResolutions[i].width == _savedWidth && _filteredResolutions[i].height == _savedHeight)
            {
                _currentResolutionIndex = i;
                break;
            }
        }

        _resolutionDropdown.SetValueWithoutNotify(_currentResolutionIndex);
        _resolutionDropdown.RefreshShownValue();

        SetResolution(_currentResolutionIndex);

        _resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    public void SetResolution(int _resolutionIndex)
    {
        if (_resolutionIndex < 0 || _resolutionIndex >= _filteredResolutions.Count) return;

        _currentResolutionIndex = _resolutionIndex;
        Resolution _res = _filteredResolutions[_currentResolutionIndex];

        bool _isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        Screen.SetResolution(_res.width, _res.height, _isFullscreen);

        PlayerPrefs.SetInt("ResolutionWidth", _res.width);
        PlayerPrefs.SetInt("ResolutionHeight", _res.height);

        if (_isFullscreen)
            PlayerPrefs.SetInt("FullscreenIndex", _currentResolutionIndex);
        else
            PlayerPrefs.SetInt("WindowIndex", _currentResolutionIndex);

        PlayerPrefs.Save();

        _resolutionDropdown.SetValueWithoutNotify(_currentResolutionIndex);
        _resolutionDropdown.RefreshShownValue();
    }
}