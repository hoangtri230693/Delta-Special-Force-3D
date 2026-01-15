using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using System.Threading.Tasks;


public enum MenuType { None, Counter, Terrorist, Map }

public class UITeamDeathMatch : MonoBehaviour
{
    [Header("Addressable Tables")]
    [SerializeField] private AssetReferenceT<CharacterTableSO> _modelCharacterTable;
    [SerializeField] private AssetReferenceT<MapTableSO> _modelMapTable;

    [Header("UI References")]
    [SerializeField] private RawImage _mapPreview;
    [SerializeField] private Transform _characterPreview;
    [SerializeField] private GameObject _backgroundLoading;

    [Header("Menu")]
    [SerializeField] private GameObject _counterMenu;
    [SerializeField] private GameObject _terroristMenu;
    [SerializeField] private GameObject _mapMenu;

    private CharacterTableSO _characterTable;
    private MapTableSO _mapTable;
    
    private MenuType _menuType = MenuType.None;
    private Dictionary<MenuType, GameObject> _menuDict;

    private bool isSelectedCharacter;
    private bool isSelectedMap;
    private string _selectedSceneName;

    // --- QUẢN LÝ HANDLES (Cần thiết cho giải phóng bộ nhớ) ---
    private object _currentCharacterData;
    private object _currentMapData;
    private GameObject _currentCharacterPrefab;
    private object _currentMapTexture;
    

    private void Awake()
    {
        _backgroundLoading.SetActive(true);
        _menuDict = new Dictionary<MenuType, GameObject>
        {
            { MenuType.Counter, _counterMenu},
            { MenuType.Terrorist, _terroristMenu},
            { MenuType.Map, _mapMenu}
        };
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

        SetMenu(MenuType.None);
        _backgroundLoading.SetActive(false);
        isSelectedCharacter = false;
        isSelectedMap = false;
    }

    private void SetMenu(MenuType type)
    {
        _menuType = type;

        foreach (var item in _menuDict)
        {
            item.Value.SetActive(false);
        }

        if (_menuDict.ContainsKey(type))
        {
            _menuDict[type].SetActive(true);
        }

        _characterPreview.gameObject.SetActive(type == MenuType.Counter || type == MenuType.Terrorist);
        _mapPreview.gameObject.SetActive(type == MenuType.Map);
    }

    public void ToggleMenu(MenuType type)
    {
        if (_menuType == type)
        {
            SetMenu(MenuType.None);
        }
        else
        {
            SetMenu(type);
        }
    }

    public async void ShowCharacter(int index)
    {
        AudioManager.instance.PlaySfx(SFXType.MetalClick);

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
        Debug.Log("Selected Character Index: " + index);
        isSelectedCharacter = true;
    }

    public async void ShowMap(int index)
    {
        AudioManager.instance.PlaySfx(SFXType.MetalClick);

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
            _currentMapTexture = mapData.previewImageReference;
            Texture2D mapTexture = await AddressableManager.instance.LoadAssetAsync<Texture2D>(mapData.previewImageReference);
            if (mapTexture != null)
            {
                _mapPreview.texture = mapTexture;
                _mapPreview.color = new Color(1, 1, 1, 1);
            }

            _selectedSceneName = mapData.mapName;
            isSelectedMap = true;
        }
    }

    #region Button Events
    public void OnClickCounterButton()
    {
        ToggleMenu(MenuType.Counter);
        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
    }

    public void OnClickTerroristButton()
    {
        ToggleMenu(MenuType.Terrorist);
        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
    }

    public void OnClickOperationMapButton()
    {
        ToggleMenu(MenuType.Map);
        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
    }

    public void OnClickBack()
    {
        SceneManager.LoadScene("PlayGame");
        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
    }

    public void OnClickDone()
    {
        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
        if (isSelectedCharacter && isSelectedMap)
        {
            GameplayDataManager.instance.GetUseGoldPerMatch();
            _backgroundLoading.SetActive(true);
            SceneManager.LoadScene(_selectedSceneName);       
        }   
    }
    #endregion

    private void OnDestroy()
    {
        if (_modelCharacterTable.IsValid()) AddressableManager.instance.ReleaseAsset(_modelCharacterTable);
        if (_modelMapTable.IsValid()) AddressableManager.instance.ReleaseAsset(_modelMapTable);
        if (_currentCharacterData != null) AddressableManager.instance.ReleaseAsset(_currentCharacterData);
        if (_currentCharacterPrefab != null) AddressableManager.instance.ReleaseInstance(_currentCharacterPrefab);
        if (_currentMapData != null) AddressableManager.instance.ReleaseAsset(_currentMapData);
        if (_currentMapTexture != null) AddressableManager.instance.ReleaseAsset(_currentMapTexture);
    }
}
