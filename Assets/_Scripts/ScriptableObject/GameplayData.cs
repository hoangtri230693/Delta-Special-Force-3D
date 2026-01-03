using UnityEngine;

[CreateAssetMenu(fileName = "GameplayData", menuName = "Scriptable Objects/GameplayData")]
public class GameplayData : ScriptableObject
{
    public float timeCountdown = 5f;
    public float timeRoundActive = 300f;
    public int totalRound = 10;
}
