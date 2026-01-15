using System.IO;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager instance;
    public PlayerSaveData playerSaveData;

    private string savePath;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Application.persistentDataPath + "/PlayerSave.json";
            LoadOrInitializeData();
        }
        else
        {
            Destroy(gameObject);
        }    
    }

    private void Start()
    {
        Application.OpenURL(Application.persistentDataPath);
    }

    private void LoadOrInitializeData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            playerSaveData = JsonUtility.FromJson<PlayerSaveData>(json);
        }
        else
        {
            playerSaveData = new PlayerSaveData();
            playerSaveData.UnlockedWeaponIDs.Add(1);
            playerSaveData.UnlockedWeaponIDs.Add(2);
            playerSaveData.Gold = 1000;
            SaveData();
        }
    }

    public void SaveData()
    {
        string json = JsonUtility.ToJson(playerSaveData);
        File.WriteAllText(savePath, json);
    }

    public void AddPlayerGold(int amount)
    {
        playerSaveData.Gold += amount;
        SaveData();
    }

    public void UsePlayerGold(int amount)
    {
        playerSaveData.Gold -= amount;
        SaveData();
    }
}
