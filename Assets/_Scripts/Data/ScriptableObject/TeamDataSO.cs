using DeltaSpecialForce3D.Enums;
using UnityEngine;


[CreateAssetMenu(fileName = "TeamDataSO", menuName = "Scriptable Objects/TeamDataSO")]
public class TeamDataSO : ScriptableObject
{
    public int teamID;
    public string teamDisplayName;
    public TeamName teamName;
    public CharacterDataSO[] characterData;

    public CharacterDataSO GetCharacterDataByCharacterID(int characterID)
    {
        foreach (var character in characterData)
        {
            if (character.characterID == characterID)
                return character;
        }
        return null;
    }
}
