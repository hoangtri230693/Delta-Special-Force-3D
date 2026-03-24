using UnityEngine;
using UnityEngine.AddressableAssets;
using DeltaSpecialForce3D.Enums;


[CreateAssetMenu(fileName = "CharacterDataSO", menuName = "Scriptable Objects/CharacterDataSO")]
public class CharacterDataSO : ScriptableObject
{
    public int characterID;
    public string characterDisplayName;
    public CharacterName characterName;
    public TeamName teamName;
    public AssetReferenceGameObject characterPlayerPrefab;
    public AssetReferenceGameObject characterAIPrefab;
    public AssetReferenceGameObject characterModelPrefab;
}
