using DeltaSpecialForce3D.Enums;
using UnityEngine;

[CreateAssetMenu(fileName = "GameplayConfigSO", menuName = "Scriptable Objects/GameplayConfigSO")]
public class GameplayConfigSO : ScriptableObject
{
    [Header("Gameplay")]
    public float timeCountdown;
    public float timeRoundActive;
    public int totalRound;

    [Header("Team Mode")]
    public int teamSize;

    [Header("Zombie Mode")]
    public int zombiePerWave;
    public int incrementZombiePerWave;
    public float distanceBetweenWaveUp;
    public float initialDistanceFromPlayer;

    [Header("Reward")]
    public int bonusGoldWinGame;
    public int bonusGoldLoseGame;
    public int bonusGoldPerKill;
    public int useGoldPerMatch;

    public int GetGoldByResult(GameResult result)
    {
        switch (result)
        {
            case GameResult.Win: 
                return bonusGoldWinGame;
            case GameResult.Lose: 
                return bonusGoldLoseGame;
            default:
                return 0;
        }
    }
}
