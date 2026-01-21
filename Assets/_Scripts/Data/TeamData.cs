using UnityEngine;
using UnityEngine.AddressableAssets;


[CreateAssetMenu(fileName = "TeamData", menuName = "Scriptable Objects/TeamData")]
public class TeamData : ScriptableObject
{
    public int teamID;
    public string teamName;
    public TeamType teamType;
    public CharacterData[] characterData;

    public CharacterData GetCharacterByID(int characterID)
    {
        foreach (var character in characterData)
        {
            if (character.characterID == characterID)
                return character;
        }
        return null;
    }
}
