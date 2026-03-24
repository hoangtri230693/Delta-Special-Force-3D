using UnityEngine;
using UnityEngine.AddressableAssets;


[CreateAssetMenu(fileName = "MapDataSO", menuName = "Scriptable Objects/MapDataSO")]
public class MapDataSO : ScriptableObject
{
    public int mapID;
    public string mapName;
    public Texture2D previewImage;
    public AssetReferenceGameObject mapPrefab;
    public SpawnData _spawnPoint;
    public SpawnData[] _spawnCounter;
    public SpawnData[] _spawnTerrorist;
    public SpawnData[] _assaultCounter;
    public SpawnData[] _patrolTerrorist;
}

[System.Serializable]
public class SpawnData
{
    public Vector3 position;
    public Vector3 rotation;
}
