using DeltaSpecialForce3D.Enums;
using UnityEngine;


[CreateAssetMenu(fileName = "TeamMenuSO", menuName = "Scriptable Objects/TeamMenuSO")]
public class TeamMenuSO : ScriptableObject
{
    public TeamDataSO[] _menuTeam;

    public TeamDataSO GetTeamByTeamID(int teamID)
    {
        foreach (var team in _menuTeam)
        {
            if (team.teamID == teamID)
                return team;
        }
        return null;
    }

    public TeamDataSO GetTeamByTeamName(TeamName teamName)
    {
        foreach (var team in _menuTeam)
        {
            if (team.teamName == teamName)
                return team;
        }
        return null;
    }
}