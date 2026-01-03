using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;

public class UIZombieSurvival : MonoBehaviour
{
    [Header("Addressable Tables")]
    [SerializeField] private AssetReferenceT<CharacterTableSO> _modelCharacterTable;
    [SerializeField] private AssetReferenceT<MapTableSO> _modelMapTable;

    [Header("UI References")]
    [SerializeField] private TMP_InputField _inputFieldName;
    [SerializeField] private TextMeshProUGUI _characterNameText;
    [SerializeField] private TextMeshProUGUI _mapNameText;
    [SerializeField] private RawImage _mapPreview;
    [SerializeField] private Transform _characterPreview;
    [SerializeField] private GameObject _backgroundLoading;

    private CharacterTableSO _characterTable;
    private MapTableSO _mapTable;
    private int _currentCharacterIndex;
    private int _currentMapIndex;

    // --- QUẢN LÝ HANDLES (Cần thiết cho giải phóng bộ nhớ) ---
    private object _currentCharacterData;
    private object _currentMapData;
    private GameObject _currentCharacterPrefab;
    private object _currentMapTexture;

    private void Awake()
    {
        _backgroundLoading.SetActive(true);
    }

    private async void Start()
    {
        await InitUIAsync();
    }

    private async Task InitUIAsync()
    {
        Task<CharacterTableSO> charTask = AddressableManager.instance.LoadAssetAsync<CharacterTableSO>(_modelCharacterTable);
        Task<MapTableSO> mapTask = AddressableManager.instance.LoadAssetAsync<MapTableSO>(_modelMapTable);

        await Task.WhenAll(charTask, mapTask);

        _characterTable = charTask.Result;
        _mapTable = mapTask.Result;

        ShowCharacter(0);
        ShowMap(0);

        _backgroundLoading.SetActive(false);
    }

    public async void ShowCharacter(int index)
    {
        if (_characterTable == null || index < 0 || index >= _characterTable.characterData.Length) return;

        // === 1. GIẢI PHÓNG TÀI NGUYÊN CŨ (RẤT QUAN TRỌNG) ===
        if (_currentCharacterPrefab != null)
        {
            AddressableManager.instance.ReleaseInstance(_currentCharacterPrefab);
            _currentCharacterPrefab = null;
        }
        if (_currentCharacterData != null)
        {
            AddressableManager.instance.ReleaseAsset(_currentCharacterData);
        }

        // === 2. TẢI DỮ LIỆU SO CON VÀ PREFAB MỚI ===
        AssetReferenceT<CharacterDataSO> charDataRef = _characterTable.characterData[index];
        _currentCharacterData = charDataRef;

        CharacterDataSO charData = await AddressableManager.instance.LoadAssetAsync<CharacterDataSO>(charDataRef);

        if (charData != null)
        {
            _characterNameText.text = charData.characterName;

            GameObject character = await AddressableManager.instance.
            InstantiatePrefabAsync(charData.characterPrefab, _characterPreview);

            if (character != null)
            {
                _currentCharacterPrefab = character;
                _currentCharacterPrefab.transform.localPosition = Vector3.zero;
                _currentCharacterPrefab.transform.localRotation = Quaternion.identity;
            }
        }

        // Lưu lại lựa chọn
        PlayerPrefs.SetInt("SelectedCharacterIndex", index);
    }

    public async void ShowMap(int index)
    {
        if (_mapTable == null || index < 0 || index >= _mapTable.mapData.Length) return;

        // === 1. GIẢI PHÓNG TÀI NGUYÊN CŨ (RẤT QUAN TRỌNG) ===
        _mapPreview.texture = null;
        _mapPreview.color = new Color(1, 1, 1, 0);

        if (_currentMapTexture != null)
        {
            AddressableManager.instance.ReleaseAsset(_currentMapTexture);
            _currentMapTexture = null;
        }

        if (_currentMapData != null)
        {
            AddressableManager.instance.ReleaseAsset(_currentMapData);
            _currentMapData = null;
        }

        // === 2. TẢI DỮ LIỆU SO CON VÀ PREVIEW IMAGE MỚI ===
        AssetReferenceT<MapDataSO> mapDataRef = _mapTable.mapData[index];
        _currentMapData = mapDataRef;

        MapDataSO mapData = await AddressableManager.instance.LoadAssetAsync<MapDataSO>(mapDataRef);

        if (mapData != null)
        {
            _mapNameText.text = mapData.mapName;

            _currentMapTexture = mapData.previewImageReference;
            Texture2D mapTexture = await AddressableManager.instance.LoadAssetAsync<Texture2D>(mapData.previewImageReference);
            if (mapTexture != null)
            {
                _mapPreview.texture = mapTexture;
                _mapPreview.color = new Color(1, 1, 1, 1);
            }
            PlayerPrefs.SetInt("SelectedMapIndex", index);
        }
    }

    #region Button Events
    public void OnClickNextCharacter()
    {
        if (_characterTable == null) return;
        _currentCharacterIndex = (_currentCharacterIndex + 1) % _characterTable.characterData.Length;
        ShowCharacter(_currentCharacterIndex);
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
    }

    public void OnClickPreviousCharacter()
    {
        if (_characterTable == null) return;
        _currentCharacterIndex--;
        if (_currentCharacterIndex < 0)
        {
            _currentCharacterIndex = _characterTable.characterData.Length - 1;
        }
        ShowCharacter(_currentCharacterIndex);
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
    }

    public void OnClickNextMap()
    {
        if (_mapTable == null) return;
        _currentMapIndex = (_currentMapIndex + 1) % _mapTable.mapData.Length;
        ShowMap(_currentMapIndex);
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
    }

    public void OnClickPreviousMap()
    {
        if (_mapTable == null) return;
        _currentMapIndex--;
        if (_currentMapIndex < 0)
        {
            _currentMapIndex = _mapTable.mapData.Length - 1;
        }
        ShowMap(_currentMapIndex);
        AudioManager.instance.PlaySfx(SFXType.MetalClick);
    }

    public void OnClickBack()
    {
        SceneManager.LoadScene("PlayGame");
        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
    }

    public void OnClickJoin()
    {
        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
        
        string playerName = _inputFieldName.text;
        if (string.IsNullOrEmpty(playerName)) return;
        _backgroundLoading.SetActive(true);
        PlayerPrefs.SetString("PlayerName", playerName);
    }
    #endregion

    private void OnDestroy()
    {
        if (_modelCharacterTable.IsValid()) AddressableManager.instance.ReleaseAsset(_modelCharacterTable);
        if (_modelMapTable.IsValid()) AddressableManager.instance.ReleaseAsset(_modelMapTable);
        if (_currentCharacterData != null) AddressableManager.instance.ReleaseAsset(_currentCharacterData);
        if (_currentCharacterPrefab != null) AddressableManager.instance.ReleaseInstance(_currentCharacterPrefab);
        if (_currentMapData != null) AddressableManager.instance.ReleaseAsset(_currentMapData);
    }
}