using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    private CancellationTokenSource _characterCts;

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
    private async void ShowMenu(int index)
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

    private async Task ShowCharacter(int index)
    {
        // 1. Hủy tác vụ đang chạy (nếu có) để tránh nhân vật cũ đè lên sau khi tải xong
        _characterCts?.Cancel();
        _characterCts?.Dispose();
        _characterCts = new CancellationTokenSource();
        var token = _characterCts.Token;

        if (_teamMenu == null) return;
        TeamData currentTeam = _teamMenu._menuTeam[_currentTeamIndex];
        CharacterData data = currentTeam.characterData[index];

        // 2. Xóa model cũ ngay lập tức để trống chỗ cho model mới
        if (_spawnedCharacterModel != null)
        {
            AddressableManager.instance.ReleaseInstance(_spawnedCharacterModel);
            _spawnedCharacterModel = null;
        }

        try
        {
            // 3. Tải Asset (Prefab) trước. Nếu đã click rồi, bước này sẽ chạy tức thì nhờ Cache
            GameObject prefab = await AddressableManager.instance.LoadAssetAsync<GameObject>(data.characterModelPrefab);

            // Kiểm tra xem trong lúc đợi tải, người dùng có click nhân vật khác không
            if (token.IsCancellationRequested) return;

            if (prefab != null)
            {
                // 4. Sinh ra nhân vật từ Prefab đã tải
                _spawnedCharacterModel = await AddressableManager.instance.InstantiatePrefabAsync(data.characterModelPrefab, _characterPreview);

                if (token.IsCancellationRequested)
                {
                    AddressableManager.instance.ReleaseInstance(_spawnedCharacterModel);
                    return;
                }

                // Setup Transform
                _spawnedCharacterModel.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                _spawnedCharacterModel.transform.localScale = Vector3.one;
            }
        }
        catch (System.OperationCanceledException) { } // Không xử lý khi bị hủy

        PlayerPrefs.SetInt("SelectedTeamID", _currentTeamIndex);
        PlayerPrefs.SetInt("SelectedCharacterID", index);
        _isSelectedCharacter = true;
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
    }

    private void ShowMap(int index)
    {
        if (_mapMenu == null || index < 0 || index >= _mapMenu._menuMap.Length) return;

        MapData data = _mapMenu._menuMap[index];
        _mapPreview.texture = data.previewImage;
        _mapPreview.color = data.previewImage != null ? Color.white : Color.clear;

        _selectedSceneName = data.mapName;
        _isSelectedMap = true;

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
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
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