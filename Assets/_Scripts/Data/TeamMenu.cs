using UnityEngine;


[CreateAssetMenu(fileName = "TeamMenu", menuName = "Scriptable Objects/TeamMenu")]
public class TeamMenu : ScriptableObject
{
    public TeamData[] _menuTeam;

    public TeamData GetTeamByID(int teamID)
    {
        foreach (var team in _menuTeam)
        {
            if (team.teamID == teamID)
                return team;
        }
        return null;
    }

    public TeamData GetTeamByTeamType(TeamType teamType)
    {
        foreach (var team in _menuTeam)
        {
            if (team.teamType == teamType)
                return team;
        }
        return null;
    }
}