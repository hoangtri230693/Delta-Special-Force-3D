using DeltaSpecialForce3D.Enums;
using UnityEngine;


[CreateAssetMenu(fileName = "MapMenuSO", menuName = "Scriptable Objects/MapMenuSO")]
public class MapMenuSO : ScriptableObject
{
    public GameMode gameMode;
    public MapDataSO[] _menuMap;

    public MapDataSO GetMapDataByMapID(int mapID)
    {
        foreach (var map in _menuMap)
        {
            if (map.mapID == mapID) 
                return map;
        }
        return null;
    }
}