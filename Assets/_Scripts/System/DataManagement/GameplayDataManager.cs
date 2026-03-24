using DeltaSpecialForce3D.Enums;
using UnityEngine;

public class GameplayDataManager : MonoBehaviour
{
    public static GameplayDataManager instance;

    public GameMode gameMode;
    public GameplayConfigSO _gameplayConfigSO;
    public CameraStatsSO _cameraStatsSO;
    public CharacterStatsSO _characterStatsSO;
    public CharacterDataSO[] _characterDataSO;
    public CharacterRigSO[] _characterRigSO;
    public MapMenuSO[] _mapMenuSO;
    public TeamMenuSO _teamMenuSO;
    public ZombieStatsSO _zombieStatsSO;
    

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
}
