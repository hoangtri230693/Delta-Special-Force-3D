using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DeltaSpecialForce3D.Enums;
using System.Threading.Tasks;


public class UIZombieSurvival : MonoBehaviour
{
    private readonly GameMode _gameMode = GameMode.ZombieSurvival;
    [SerializeField] private TeamMenuSO _teamMenu;
    [SerializeField] private MapMenuSO _mapMenu;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _teamNameText;
    [SerializeField] private TextMeshProUGUI _characterNameText;
    [SerializeField] private TextMeshProUGUI _mapNameText;
    [SerializeField] private RawImage _mapPreview;
    [SerializeField] private Transform _characterPreview;
    [SerializeField] private GameObject _backgroundLoading;

    private int _currentTeamID = 0;
    private int _currentCharacterID = 0;
    private int _currentMapID = 0;
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
        ShowMap(_currentMapID);

        // Nếu ID mặc định không hợp lệ, tìm cái đầu tiên hợp lệ
        if (!IsValidTeam(_currentTeamID))
        {
            for (int i = 0; i < _teamMenu._menuTeam.Length; i++)
            {
                if (IsValidTeam(i))
                {
                    _currentTeamID = i;
                    break;
                }
            }
        }

        await ShowTeam(_currentTeamID);    
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

    private async Task ShowTeam(int id)
    {
        if (_teamMenu == null || _teamMenu._menuTeam.Length == 0) return;

        TeamDataSO teamData = _teamMenu.GetTeamByTeamID(id);

        if (teamData.teamName == TeamName.Zombie) return;

        if (_teamNameText != null)
            _teamNameText.text = teamData.teamDisplayName;

        // Reset nhân vật về 0 khi đổi Tea
        _currentCharacterID = 0;
        await ShowCharacter(_currentCharacterID);

        PlayerPrefs.SetInt("SelectedTeamID", teamData.teamID);
    }

    private async Task ShowCharacter(int id)
    {
        if (_teamMenu == null) return;
        CharacterDataSO data = _teamMenu._menuTeam[_currentTeamID].GetCharacterDataByCharacterID(id);

        if (_spawnedCharacterModel != null)
        {
            AddressableManager.instance.ReleaseInstance(_spawnedCharacterModel);
            _spawnedCharacterModel = null;
        }

        //Cập nhật UI Character Name
        _characterNameText.text = data.characterDisplayName;

        GameObject prefab = await AddressableManager.instance.LoadAssetAsync<GameObject>(data.characterModelPrefab);

        if (prefab != null)
        {
            _spawnedCharacterModel = await AddressableManager.instance.InstantiatePrefabAsync(data.characterModelPrefab, _characterPreview);

            if (_spawnedCharacterModel != null)
            {
                _spawnedCharacterModel.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                _spawnedCharacterModel.transform.localScale = Vector3.one;
            }
        }

        PlayerPrefs.SetInt("SelectedCharacterID", data.characterID);

    }

    private void ShowMap(int id)
    {
        if (_mapMenu == null || _mapMenu._menuMap.Length == 0) return;

        MapDataSO data = _mapMenu.GetMapDataByMapID(id);
        PlayerPrefs.SetInt("SelectedMapID", id);

        _mapNameText.text = data.mapName;
        _selectedSceneName = data.mapName;

        if (data.previewImage != null)
        {
            _mapPreview.texture = data.previewImage;
            _mapPreview.color = Color.white;
        }
        else
        {
            _mapPreview.color = Color.clear;
        }
    }

    private bool IsValidTeam(int id)
    {
        if (_teamMenu == null || id < 0 || id >= _teamMenu._menuTeam.Length) return false;

        TeamDataSO teamData = _teamMenu.GetTeamByTeamID(id);
        // Loại bỏ Zombie hoặc các team không xác định
        if (teamData.teamName == TeamName.Zombie || teamData.teamName == TeamName.None)
        {
            return false;
        }
        return true;
    }

    #region Button Events

    public async void OnClickNextTeam()
    {
        if (_teamMenu == null) return;
        int nextID = _currentTeamID;
        int safetyBreak = 0; // Tránh vòng lặp vô tận nếu không có team nào hợp lệ

        do
        {
            nextID = (nextID + 1) % _teamMenu._menuTeam.Length;
            safetyBreak++;
        } 
        while (!IsValidTeam(nextID) && safetyBreak < _teamMenu._menuTeam.Length);

        _currentTeamID = nextID;
        await ShowTeam(nextID);
        AudioManager.instance.PlaySfx(SFXSoundType.MetalClick);
    }

    public async void OnClickPreviousTeam()
    {
        if (_teamMenu == null) return;
        int prevID = _currentTeamID;
        int safetyBreak = 0;

        do
        {
            prevID--;
            if (prevID < 0) prevID = _teamMenu._menuTeam.Length - 1;
            safetyBreak++;
        } while (!IsValidTeam(prevID) && safetyBreak < _teamMenu._menuTeam.Length);

        _currentTeamID = prevID;
        await ShowTeam(prevID);
        AudioManager.instance.PlaySfx(SFXSoundType.MetalClick);
    }

    public async void OnClickNextCharacter()
    {
        if (_teamMenu == null) return;
        var characters = _teamMenu._menuTeam[_currentTeamID].characterData;
        int nextID = (_currentCharacterID + 1) % characters.Length;
        _currentCharacterID = nextID;
        await ShowCharacter(nextID);
        AudioManager.instance.PlaySfx(SFXSoundType.MetalClick);
    }

    public async void OnClickPreviousCharacter()
    {
        if (_teamMenu == null) return;
        var characters = _teamMenu._menuTeam[_currentTeamID].characterData;
        int prevID = _currentCharacterID - 1;
        if (prevID < 0) prevID = characters.Length - 1;
        _currentCharacterID = prevID;
        await ShowCharacter(prevID);
        AudioManager.instance.PlaySfx(SFXSoundType.MetalClick);
    }

    public void OnClickNextMap()
    {
        if (_mapMenu == null) return;
        int nextID = (_currentMapID + 1) % _mapMenu._menuMap.Length;
        _currentMapID = nextID;
        ShowMap(nextID);
        AudioManager.instance.PlaySfx(SFXSoundType.MetalClick);
    }

    public void OnClickPreviousMap()
    {
        if (_mapMenu == null) return;
        int prevID = _currentMapID - 1;
        if (prevID < 0) prevID = _mapMenu._menuMap.Length - 1;
        _currentMapID = prevID;
        ShowMap(prevID);
        AudioManager.instance.PlaySfx(SFXSoundType.MetalClick);
    }

    public void OnClickDone()
    {
        AudioManager.instance.PlaySfx(SFXSoundType.DefaultClick);
        SceneManager.LoadScene("" + _selectedSceneName);
    }

    public void OnClickBack()
    {
        AudioManager.instance.PlaySfx(SFXSoundType.MetalClick);
        SceneManager.LoadScene("PlayGame");
    }
    #endregion

    private void OnDestroy()
    {
        if (_spawnedCharacterModel != null) AddressableManager.instance.ReleaseInstance(_spawnedCharacterModel);
    }
}