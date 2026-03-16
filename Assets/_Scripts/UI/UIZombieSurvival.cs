using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    private TeamMenu _teamMenu;
    private MapMenu _mapMenu;
    private CancellationTokenSource _characterCts;

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

        _teamMenu = charTask.Result;
        _mapMenu = mapTask.Result;

        if (_teamMenu != null && _mapMenu != null)
        {
            ShowTeam(_currentTeamIndex);
            ShowMap(_currentMapIndex);
        }

        _backgroundLoading.SetActive(false);
    }

    private void ShowTeam(int index)
    {
        if (_teamMenu == null || _teamMenu._menuTeam.Length == 0) return;

        _currentTeamIndex = index;
        TeamData data = _teamMenu._menuTeam[_currentTeamIndex];

        if (_teamNameText != null)
            _teamNameText.text = data.teamName;

        // Reset nhân vật về 0 khi đổi Team
        _currentCharacterIndex = 0;
        ShowCharacter(_currentCharacterIndex);

        PlayerPrefs.SetInt("SelectedTeamID", _currentTeamIndex);
    }

    private async void ShowCharacter(int index)
    {
        // 1. Hủy bỏ tác vụ load trước đó nếu người dùng click quá nhanh
        _characterCts?.Cancel();
        _characterCts?.Dispose();
        _characterCts = new CancellationTokenSource();
        var token = _characterCts.Token;

        if (_teamMenu == null) return;
        TeamData currentTeam = _teamMenu._menuTeam[_currentTeamIndex];
        if (currentTeam.characterData == null || currentTeam.characterData.Length == 0) return;

        _currentCharacterIndex = index;
        CharacterData data = currentTeam.characterData[_currentCharacterIndex];

        // 2. Dọn dẹp model cũ ngay lập tức để trống chỗ
        if (_spawnedCharacterModel != null)
        {
            AddressableManager.instance.ReleaseInstance(_spawnedCharacterModel);
            _spawnedCharacterModel = null;
        }

        // 3. Cập nhật UI Text trước cho mượt
        _characterNameText.text = data.characterName;

        // 4. Tải model
        if (data.characterModelPrefab != null)
        {
            try
            {
                // Bước này sẽ rất nhanh nếu asset đã có trong cache của AddressableManager
                GameObject prefab = await AddressableManager.instance.LoadAssetAsync<GameObject>(data.characterModelPrefab);

                // Kiểm tra xem đã bị hủy task chưa (người dùng click tiếp cái khác)
                if (token.IsCancellationRequested) return;

                if (prefab != null)
                {
                    _spawnedCharacterModel = await AddressableManager.instance.InstantiatePrefabAsync(data.characterModelPrefab, _characterPreview);

                    if (token.IsCancellationRequested)
                    {
                        AddressableManager.instance.ReleaseInstance(_spawnedCharacterModel);
                        return;
                    }

                    _spawnedCharacterModel.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                    _spawnedCharacterModel.transform.localScale = Vector3.one;
                }
            }
            catch (System.OperationCanceledException) { }
        }

        PlayerPrefs.SetInt("SelectedCharacterID", _currentCharacterIndex);
    }

    private void ShowMap(int index)
    {
        if (_mapMenu == null || _mapMenu._menuMap.Length == 0) return;

        _currentMapIndex = index;
        MapData data = _mapMenu._menuMap[_currentMapIndex];

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

    #region Button Events

    // --- QUẢN LÝ TEAM ---
    public void OnClickNextTeam()
    {
        if (_teamMenu == null) return;
        int nextIndex = (_currentTeamIndex + 1) % _teamMenu._menuTeam.Length;
        ShowTeam(nextIndex);
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
    }

    public void OnClickPreviousTeam()
    {
        if (_teamMenu == null) return;
        int prevIndex = _currentTeamIndex - 1;
        if (prevIndex < 0) prevIndex = _teamMenu._menuTeam.Length - 1;
        ShowTeam(prevIndex);
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
    }

    // --- QUẢN LÝ NHÂN VẬT ---
    public void OnClickNextCharacter()
    {
        if (_teamMenu == null) return;
        var characters = _teamMenu._menuTeam[_currentTeamIndex].characterData;
        int nextIndex = (_currentCharacterIndex + 1) % characters.Length;
        ShowCharacter(nextIndex);
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
    }

    public void OnClickPreviousCharacter()
    {
        if (_teamMenu == null) return;
        var characters = _teamMenu._menuTeam[_currentTeamIndex].characterData;
        int prevIndex = _currentCharacterIndex - 1;
        if (prevIndex < 0) prevIndex = characters.Length - 1;
        ShowCharacter(prevIndex);
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
    }

    // --- QUẢN LÝ MAP ---
    public void OnClickNextMap()
    {
        if (_mapMenu == null) return;
        int nextIndex = (_currentMapIndex + 1) % _mapMenu._menuMap.Length;
        ShowMap(nextIndex);
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
    }

    public void OnClickPreviousMap()
    {
        if (_mapMenu == null) return;
        int prevIndex = _currentMapIndex - 1;
        if (prevIndex < 0) prevIndex = _mapMenu._menuMap.Length - 1;
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
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
        SceneManager.LoadScene("PlayGame");
    }
    #endregion

    private void OnDestroy()
    {
        _characterCts?.Cancel();
        _characterCts?.Dispose();

        if (_teamMenuRef.IsValid()) AddressableManager.instance.ReleaseAsset(_teamMenuRef);
        if (_mapMenuRef.IsValid()) AddressableManager.instance.ReleaseAsset(_mapMenuRef);
        if (_spawnedCharacterModel != null) AddressableManager.instance.ReleaseInstance(_spawnedCharacterModel);
    }
}