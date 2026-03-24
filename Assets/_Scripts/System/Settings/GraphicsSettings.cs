using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GraphicsSettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown _qualityDropdown;

    private void Start()
    {
        LoadSaveSettings();
    }

    private void LoadSaveSettings()
    {
        _qualityDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (var quality in QualitySettings.names)
        {
            options.Add(quality);
        }
        _qualityDropdown.AddOptions(options);

        int _savedQualityLevel = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        _qualityDropdown.value = _savedQualityLevel;
        _qualityDropdown.RefreshShownValue();

        SetQuality(_savedQualityLevel);

        _qualityDropdown.onValueChanged.AddListener(SetQuality);
    }

    private void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);

        PlayerPrefs.SetInt("QualityLevel", qualityIndex);
    }
}
