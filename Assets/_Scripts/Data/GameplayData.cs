using UnityEngine;

[CreateAssetMenu(fileName = "GameplayData", menuName = "Scriptable Objects/GameplayData")]
public class GameplayData : ScriptableObject
{
    public float timeCountdown = 5f;
    public float timeRoundActive = 300f;
    public int totalRound = 10;
    public int bonusGoldWinGame = 300;
    public int bonusGoldDrawGame = 200;
    public int bonusGoldLoseGame = 100;
    public int bonusGoldPerKill = 10;
}
