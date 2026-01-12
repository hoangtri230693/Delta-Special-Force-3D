using UnityEngine;
using UnityEngine.AddressableAssets;


[CreateAssetMenu(fileName = "CharacterTableSO", menuName = "Scriptable Objects/CharacterTableSO")]
public class CharacterTableSO : ScriptableObject
{
    public AssetReferenceT<CharacterDataSO>[] characterData;
}
