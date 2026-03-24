using DeltaSpecialForce3D.Enums;
using UnityEngine;


public class PlayerTeam : MonoBehaviour
{
    [SerializeField] private TeamName _teamName;
    [SerializeField] private CharacterName _characterName;
    [SerializeField] private int _characterID;

    public TeamName Team => _teamName;
    public CharacterName Name => _characterName;
    public int ID => _characterID;
}
