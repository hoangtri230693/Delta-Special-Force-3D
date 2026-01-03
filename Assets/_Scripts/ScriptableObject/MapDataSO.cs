using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "MapDataSO", menuName = "Scriptable Objects/MapDataSO")]
public class MapDataSO : ScriptableObject
{
    public string mapName;
    public GameObject mapPrefab;
    public AssetReferenceTexture2D previewImageReference;
}
