using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

public class UIZombieSurvival : MonoBehaviour
{
    [Header("Addressable Main Data")]
    [SerializeField] private AssetReferenceT<TeamMenu> _teamMenuRef;
    [SerializeField] private AssetReferenceT<MapMenu> _mapMenuRef;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _teamNameText;
    [SerializeField] private TextMeshProUGUI _characterNameText;
    [SerializeField] private TextMeshProUGUI _mapNameText;
    [SerializeField] private RawImage _mapPreview;
    [SerializeField] private Transform _characterPreview;
    [SerializeField] private GameObject _backgroundLoading;

    private TeamMenu _teamTable;
    private MapMenu _mapTable;

    private int _currentTeamIndex = 0;
    private int _currentCharacterIndex = 0;
    private int _currentMapIndex = 0;

    private string _selectedSceneName;

    private GameObject _spawnedCharacterModel;

    private void Awake() => _backgroundLoading.SetActive(true);

    private async void Start() => await InitUIAsync();

    private async Task InitUIAsync()
    {
        var charTask = AddressableManager.instance.LoadAssetAsync<TeamMenu>(_teamMenuRef);
        var mapTask = AddressableManager.instance.LoadAssetAsync<MapMenu>(_mapMenuRef);

        await Task.WhenAll(charTask, mapTask);

        _teamTable = charTask.Result;
        _mapTable = mapTask.Result;

        if (_teamTable != null && _mapTable != null)
        {
            // Khởi tạo hiển thị ban đầu
            ShowTeam(_currentTeamIndex);
            ShowMap(_currentMapIndex);
        }

        _backgroundLoading.SetActive(false);
    }

    public void ShowTeam(int index)
    {
        if (_teamTable == null || _teamTable._menuTeam.Length == 0) return;

        _currentTeamIndex = index;
        TeamData data = _teamTable._menuTeam[_currentTeamIndex];

        if (_teamNameText != null)
            _teamNameText.text = data.teamName;

        // Reset nhân vật về 0 khi đổi Team
        _currentCharacterIndex = 0;
        ShowCharacter(_currentCharacterIndex);

        PlayerPrefs.SetInt("SelectedTeamIndex", _currentTeamIndex);
    }

    public async void ShowCharacter(int index)
    {
        if (_teamTable == null) return;
        TeamData currentTeam = _teamTable._menuTeam[_currentTeamIndex];

        if (currentTeam.characterData == null || currentTeam.characterData.Length == 0) return;

        _currentCharacterIndex = index;
        CharacterData data = currentTeam.characterData[_currentCharacterIndex];

        // 1. Dọn dẹp model cũ
        if (_spawnedCharacterModel != null)
        {
            AddressableManager.instance.ReleaseInstance(_spawnedCharacterModel);
            _spawnedCharacterModel = null;
        }

        // 2. Cập nhật UI
        _characterNameText.text = data.characterName;

        // 3. Tải model mới
        if (data.characterModelPrefab != null)
        {
            GameObject model = await AddressableManager.instance.InstantiatePrefabAsync(data.characterModelPrefab, _characterPreview);
            if (model != null)
            {
                _spawnedCharacterModel = model;
                _spawnedCharacterModel.transform.localPosition = Vector3.zero;
                _spawnedCharacterModel.transform.localRotation = Quaternion.identity;
            }
        }

        PlayerPrefs.SetInt("SelectedCharacterIndex", _currentCharacterIndex);
    }

    public void ShowMap(int index)
    {
        if (_mapTable == null || _mapTable._menuMap.Length == 0) return;

        _currentMapIndex = index;
        MapData data = _mapTable._menuMap[_currentMapIndex];

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

        PlayerPrefs.SetInt("SelectedMapIndex", _currentMapIndex);
    }

    #region Button Events

    // --- QUẢN LÝ TEAM ---
    public void OnClickNextTeam()
    {
        if (_teamTable == null) return;
        int nextIndex = (_currentTeamIndex + 1) % _teamTable._menuTeam.Length;
        ShowTeam(nextIndex);
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
    }

    public void OnClickPreviousTeam()
    {
        if (_teamTable == null) return;
        int prevIndex = _currentTeamIndex - 1;
        if (prevIndex < 0) prevIndex = _teamTable._menuTeam.Length - 1;
        ShowTeam(prevIndex);
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
    }

    // --- QUẢN LÝ NHÂN VẬT ---
    public void OnClickNextCharacter()
    {
        if (_teamTable == null) return;
        var characters = _teamTable._menuTeam[_currentTeamIndex].characterData;
        int nextIndex = (_currentCharacterIndex + 1) % characters.Length;
        ShowCharacter(nextIndex);
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
    }

    public void OnClickPreviousCharacter()
    {
        if (_teamTable == null) return;
        var characters = _teamTable._menuTeam[_currentTeamIndex].characterData;
        int prevIndex = _currentCharacterIndex - 1;
        if (prevIndex < 0) prevIndex = characters.Length - 1;
        ShowCharacter(prevIndex);
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
    }

    // --- QUẢN LÝ MAP ---
    public void OnClickNextMap()
    {
        if (_mapTable == null) return;
        int nextIndex = (_currentMapIndex + 1) % _mapTable._menuMap.Length;
        ShowMap(nextIndex);
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
    }

    public void OnClickPreviousMap()
    {
        if (_mapTable == null) return;
        int prevIndex = _currentMapIndex - 1;
        if (prevIndex < 0) prevIndex = _mapTable._menuMap.Length - 1;
        ShowMap(prevIndex);
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
    }

    public void OnClickDone()
    {
        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
        _backgroundLoading.SetActive(true);

        SceneManager.LoadScene("" + _selectedSceneName);
    }

    public void OnClickBack()
    {
        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
        SceneManager.LoadScene("PlayGame");
    }
    #endregion

    private void OnDestroy()
    {
        if (_teamMenuRef.IsValid()) AddressableManager.instance.ReleaseAsset(_teamMenuRef);
        if (_mapMenuRef.IsValid()) AddressableManager.instance.ReleaseAsset(_mapMenuRef);

        if (_spawnedCharacterModel != null)
        {
            AddressableManager.instance.ReleaseInstance(_spawnedCharacterModel);
        }
    }
}