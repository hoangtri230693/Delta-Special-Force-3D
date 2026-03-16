using UnityEngine;
using UnityEngine.SceneManagement;

public class UIStartGame : MonoBehaviour
{
    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnClickPlayGame()
    {
        SceneManager.LoadScene("PlayGame");
        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
    }

    public void OnClickTutorials()
    {
        SceneManager.LoadScene("Tutorial");
        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
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
