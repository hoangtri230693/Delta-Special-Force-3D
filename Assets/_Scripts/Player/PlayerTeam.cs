using DeltaSpecialForce3D.Enums;
using UnityEngine;


public class PlayerTeam : MonoBehaviour
{
    [SerializeField] private TeamName _teamName;
    [SerializeField] private CharacterName _characterName;
    [SerializeField] private int _actorID;

    public TeamName Team => _teamName;
    public CharacterName Name => _characterName;
    public int ActorID => _actorID;

    public void SetupActor(int id)
    {
        _actorID = id;
    }
}
