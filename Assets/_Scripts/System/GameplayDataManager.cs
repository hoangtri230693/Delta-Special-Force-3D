using UnityEngine;

public class GameplayDataManager : MonoBehaviour
{
    public static GameplayDataManager instance;

    [SerializeField] private GameplayData _gameplayData;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int GetUseGoldPerMatch()
    {
        return _gameplayData.useGoldPerMatch;
    }

    public int GetBonusGoldPerKill()
    {
        return _gameplayData.bonusGoldPerKill;
    }

    public int GetBonusGoldByMatchResult(string result)
    {
        return _gameplayData.GetGoldByResult(result);
    }
}
