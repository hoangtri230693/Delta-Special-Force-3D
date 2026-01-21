using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class UITeamDeathMatch : MonoBehaviour
{
    [Header("Addressable Main Data")]
    [SerializeField] private AssetReferenceT<TeamMenu> _teamMenuRef;
    [SerializeField] private AssetReferenceT<MapMenu> _mapMenuRef;

    [Header("UI References")]
    [SerializeField] private RawImage _mapPreview;
    [SerializeField] private Transform _characterPreview;
    [SerializeField] private GameObject _backgroundLoading;

    [Header("Menu Containers")]
    [SerializeField] private GameObject _containerCounter;
    [SerializeField] private GameObject _containerTerrorist;
    [SerializeField] private GameObject _containerOperationMap;

    [Header("Text Slots")]
    [SerializeField] private TextMeshProUGUI[] _counterNameText;
    [SerializeField] private TextMeshProUGUI[] _terroristNameText;
    [SerializeField] private TextMeshProUGUI[] _mapNameText;

    private TeamMenu _teamMenu;
    private MapMenu _mapMenu;

    private int _currentTeamIndex = 0;
    private bool _isSelectedCharacter;
    private bool _isSelectedMap;
    private string _selectedSceneName;
    private GameObject _spawnedCharacterModel;

    private void Awake() => _backgroundLoading.SetActive(true);

    private async void Start() => await InitUIAsync();

    private async Task InitUIAsync()
    {
        var charTask = AddressableManager.instance.LoadAssetAsync<TeamMenu>(_teamMenuRef);
        var mapTask = AddressableManager.instance.LoadAssetAsync<MapMenu>(_mapMenuRef);

        await Task.WhenAll(charTask, mapTask);

        _teamMenu = charTask.Result;
        _mapMenu = mapTask.Result;

        SetupNameText();

        ShowMenu(0);

        _backgroundLoading.SetActive(false);
    }

    private void SetupNameText()
    {
        if (_teamMenu == null || _mapMenu == null) return;

        // Counter (Index 0)
        var counterData = _teamMenu._menuTeam[0].characterData;
        for (int i = 0; i < _counterNameText.Length; i++)
        {
            if (i < counterData.Length) _counterNameText[i].text = "    " + counterData[i].characterName;
        }

        // Terrorist (Index 1)
        if (_teamMenu._menuTeam.Length > 1)
        {
            var terroristData = _teamMenu._menuTeam[1].characterData;
            for (int i = 0; i < _terroristNameText.Length; i++)
            {
                if (i < terroristData.Length) _terroristNameText[i].text = "    " + terroristData[i].characterName;
            }
        }

        // Maps
        for (int i = 0; i < _mapNameText.Length; i++)
        {
            if (i < _mapMenu._menuMap.Length) _mapNameText[i].text = "    " + _mapMenu._menuMap[i].mapName;
        }
    }

    /// <summary>
    /// Hiển thị Menu của Team hoặc Menu Map
    /// index 0: Counter, index 1: Terrorist, index 2: Map
    /// </summary>
    public async void ShowMenu(int index)
    {
        _containerCounter.SetActive(index == 0);
        _containerTerrorist.SetActive(index == 1);
        _containerOperationMap.SetActive(index == 2);

        _characterPreview.gameObject.SetActive(index == 0 || index == 1);
        _mapPreview.gameObject.SetActive(index == 2);

        if (index == 0 || index == 1)
        {
            _currentTeamIndex = index;
        }

        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
        if (index == 0 || index == 1) await ShowCharacter(0);
        if (index == 2) ShowMap(0);
    }
    
    public async Task ShowCharacter(int index)
    {
        if (_teamMenu == null) return;
        TeamData currentTeam = _teamMenu._menuTeam[_currentTeamIndex];
        if (index < 0 || index >= currentTeam.characterData.Length) return;

        CharacterData data = currentTeam.characterData[index];

        // Giải phóng model cũ
        if (_spawnedCharacterModel != null)
        {
            AddressableManager.instance.ReleaseInstance(_spawnedCharacterModel);
            _spawnedCharacterModel = null;
        }

        // Tải model mới vào CharacterPreview
        if (data.characterModelPrefab != null)
        {
            _spawnedCharacterModel = await AddressableManager.instance.InstantiatePrefabAsync(data.characterModelPrefab, _characterPreview);
            if (_spawnedCharacterModel != null)
            {
                _spawnedCharacterModel.transform.localPosition = Vector3.zero;
                _spawnedCharacterModel.transform.localRotation = Quaternion.identity;
                _spawnedCharacterModel.transform.localScale = Vector3.one;
            }
        }

        PlayerPrefs.SetInt("SelectedTeamID", _currentTeamIndex);
        PlayerPrefs.SetInt("SelectedCharacterID", index);
        _isSelectedCharacter = true;
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
    }

    public void ShowMap(int index)
    {
        if (_mapMenu == null || index < 0 || index >= _mapMenu._menuMap.Length) return;

        MapData data = _mapMenu._menuMap[index];
        _mapPreview.texture = data.previewImage;
        _mapPreview.color = data.previewImage != null ? Color.white : Color.clear;

        _selectedSceneName = data.mapName;
        _isSelectedMap = true;

        PlayerPrefs.SetInt("SelectedMapID", index);
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
    }

    #region UI Callbacks

    public void OnClickSelectMenu(int index) => ShowMenu(index);

    public async void OnClickCharacter(int index) => await ShowCharacter(index);

    public void OnClickMap(int index) => ShowMap(index);

    public void OnClickDone()
    {
        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
        if (_isSelectedCharacter && _isSelectedMap)
        {
            _backgroundLoading.SetActive(true);
            if (GameplayDataManager.instance != null) GameplayDataManager.instance.GetUseGoldPerMatch();
            SceneManager.LoadScene(_selectedSceneName);
        }
    }

    public void OnClickBack()
    {
        SceneManager.LoadScene("PlayGame");
        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
    }

    #endregion

    private void OnDestroy()
    {
        if (_teamMenuRef.IsValid()) AddressableManager.instance.ReleaseAsset(_teamMenuRef);
        if (_mapMenuRef.IsValid()) AddressableManager.instance.ReleaseAsset(_mapMenuRef);
        if (_spawnedCharacterModel != null) AddressableManager.instance.ReleaseInstance(_spawnedCharacterModel);
    }
}