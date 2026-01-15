using UnityEngine;

[CreateAssetMenu(fileName = "GameplayData", menuName = "Scriptable Objects/GameplayData")]
public class GameplayData : ScriptableObject
{
    public int bonusGoldWinGame = 300;
    public int bonusGoldDrawGame = 200;
    public int bonusGoldLoseGame = 100;
    public int bonusGoldPerKill = 10;
    public int useGoldPerMatch = 20;

    public int GetGoldByResult(string result)
    {
        switch (result.ToUpper())
        {
            case "WIN": return bonusGoldWinGame;
            case "DRAW": return bonusGoldDrawGame;
            case "LOSE": return bonusGoldLoseGame;
            default: return 0;
        }
    }
}
