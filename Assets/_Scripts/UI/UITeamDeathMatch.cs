using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DeltaSpecialForce3D.Enums;


public class UITeamDeathMatch : MonoBehaviour
{
    private readonly GameMode _gameMode = GameMode.TeamDeathmatch;
    [SerializeField] private TeamMenuSO _teamMenu;
    [SerializeField] private MapMenuSO _mapMenu;

    [Header("UI References")]
    [SerializeField] private RawImage _mapPreview;
    [SerializeField] private Transform _characterPreview;
    [SerializeField] private GameObject _backgroundLoading;

    [Header("Menu Containers")]
    [SerializeField] private GameObject _containerCounter;
    [SerializeField] private GameObject _containerTerrorist;
    [SerializeField] private GameObject _containerMap;

    [Header("Text Slots")]
    [SerializeField] private TextMeshProUGUI[] _counterNameText;
    [SerializeField] private TextMeshProUGUI[] _terroristNameText;
    [SerializeField] private TextMeshProUGUI[] _mapNameText;

    private TeamName _currentTeamName;
    private bool _isSelectedCharacter;
    private bool _isSelectedMap;
    private string _selectedSceneName;
    private GameObject _spawnedCharacterModel;


    private void Awake()
    {
        GetTeamMenu();
        GetMapMenu();
        _backgroundLoading.SetActive(true);
    }

    private async void Start()
    {
        SetupNameText();
        await ShowMenu(0);
        _backgroundLoading.SetActive(false);
    }

    private void GetTeamMenu()
    {
        _teamMenu = GameplayDataManager.instance._teamMenuSO;
    }

    private void GetMapMenu()
    {
        foreach (var menu in GameplayDataManager.instance._mapMenuSO)
        {
            if (menu.gameMode == _gameMode)
            {
                _mapMenu = menu;
                break;
            }
        }        
    }

    private void SetupNameText()
    {
        if (_teamMenu == null || _mapMenu == null) return;

        // Counter (Index 0)
        var counterData = _teamMenu._menuTeam[0].characterData;
        for (int i = 0; i < _counterNameText.Length; i++)
        {
            if (i < counterData.Length) _counterNameText[i].text = "    " + counterData[i].characterDisplayName;
        }

        // Terrorist (Index 1)
        if (_teamMenu._menuTeam.Length > 1)
        {
            var terroristData = _teamMenu._menuTeam[1].characterData;
            for (int i = 0; i < _terroristNameText.Length; i++)
            {
                if (i < terroristData.Length) _terroristNameText[i].text = "    " + terroristData[i].characterDisplayName;
            }
        }

        // Maps
        for (int i = 0; i < _mapNameText.Length; i++)
        {
            if (i < _mapMenu._menuMap.Length) _mapNameText[i].text = "    " + _mapMenu._menuMap[i].mapName;
        }
    }

    private async Task ShowMenu(int index)
    {
        _containerCounter.SetActive(index == 0);
        _containerTerrorist.SetActive(index == 1);
        _containerMap.SetActive(index == 2);

        _characterPreview.gameObject.SetActive(index == 0 || index == 1);
        _mapPreview.gameObject.SetActive(index == 2);

        if (index == 0) _currentTeamName = TeamName.Counter;
        if (index == 1) _currentTeamName = TeamName.Terrorist;
        var teamData = _teamMenu.GetTeamByTeamName(_currentTeamName);
        var teamID = teamData.teamID;
        PlayerPrefs.SetInt("SelectedTeamID", teamID);

        AudioManager.instance.PlaySfx(SFXSoundType.DefaultClick);
        if (index == 0 || index == 1) await ShowCharacter(0);
        if (index == 2) ShowMap(0);
    }

    private async Task ShowCharacter(int id)
    {
        if (_teamMenu == null) return;
        TeamDataSO teamData = _teamMenu.GetTeamByTeamName(_currentTeamName);
        CharacterDataSO characterData = teamData.GetCharacterDataByCharacterID(id);

        if (_spawnedCharacterModel != null)
        {
            AddressableManager.instance.ReleaseInstance(_spawnedCharacterModel);
            _spawnedCharacterModel = null;
        }

        GameObject prefab = await AddressableManager.instance.LoadAssetAsync<GameObject>(characterData.characterModelPrefab);

        if (prefab != null)
        {
            _spawnedCharacterModel = await AddressableManager.instance.InstantiatePrefabAsync(characterData.characterModelPrefab, _characterPreview);

            if (_spawnedCharacterModel != null)
            {
                _spawnedCharacterModel.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                _spawnedCharacterModel.transform.localScale = Vector3.one;
            }
        }
     
        PlayerPrefs.SetInt("SelectedCharacterID", id);
        _isSelectedCharacter = true;
        AudioManager.instance.PlaySfx(SFXSoundType.MetalClick);

    }

    private void ShowMap(int id)
    {
        if (_mapMenu == null || id < 0 || id >= _mapMenu._menuMap.Length) return;

        MapDataSO mapData = _mapMenu.GetMapDataByMapID(id);
        PlayerPrefs.SetInt("SelectedMapID", id);

        _mapPreview.texture = mapData.previewImage;
        _mapPreview.color = mapData.previewImage != null ? Color.white : Color.clear;

        _selectedSceneName = mapData.mapName;
        _isSelectedMap = true;

        AudioManager.instance.PlaySfx(SFXSoundType.MetalClick);
    }

    #region UI Callbacks

    public async void OnClickSelectMenu(int index) => await ShowMenu(index);

    public async void OnClickCharacter(int index) => await ShowCharacter(index);

    public void OnClickMap(int index) => ShowMap(index);

    public void OnClickDone()
    {
        AudioManager.instance.PlaySfx(SFXSoundType.DefaultClick);
        if (_isSelectedCharacter && _isSelectedMap)
        {
            SceneManager.LoadScene(_selectedSceneName);
        }
    }

    public void OnClickBack()
    {
        SceneManager.LoadScene("PlayGame");
        AudioManager.instance.PlaySfx(SFXSoundType.MetalClick);
    }

    #endregion

    private void OnDestroy()
    {
        if (_spawnedCharacterModel != null) AddressableManager.instance.ReleaseInstance(_spawnedCharacterModel);
    }
}