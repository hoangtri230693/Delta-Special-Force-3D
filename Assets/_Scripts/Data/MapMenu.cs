using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "MapMenu", menuName = "Scriptable Objects/MapMenu")]
public class MapMenu : ScriptableObject
{
    public MapData[] _menuMap;
}
