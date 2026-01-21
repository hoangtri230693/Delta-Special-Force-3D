using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
{
    public int characterID;
    public string characterName;
    public TeamType teamType;
    public AssetReferenceGameObject characterPlayerPrefab;
    public AssetReferenceGameObject characterAIPrefab;
    public AssetReferenceGameObject characterModelPrefab;
}
