using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "MapTableSO", menuName = "Scriptable Objects/MapTableSO")]
public class MapTableSO : ScriptableObject
{
    public AssetReferenceT<MapDataSO>[] mapData;
}
