using UnityEngine;
using UnityEngine.SceneManagement;

public class UIStartGame : MonoBehaviour
{
    [SerializeField] private GameObject _playGameButton;
    [SerializeField] private GameObject _tutorialsButton;
    [SerializeField] private GameObject _settingsButton;
    [SerializeField] private GameObject _quitButton;

    public void OnClickPlayGame()
    {
        SceneManager.LoadScene("PlayGame");
        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
    }

    public void OnClickTutorials()
    {

    }

    public void OnClickSettings()
    {
        SceneManager.LoadScene("Settings");
        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
    }

    public void OnClickQuit()
    {
        Application.Quit();
        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
    }
}
