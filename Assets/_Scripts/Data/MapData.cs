using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "MapData", menuName = "Scriptable Objects/MapData")]
public class MapData : ScriptableObject
{
    public int mapID;
    public string mapName;
    public Texture2D previewImage;
    public AssetReferenceGameObject mapPrefab;    
}
