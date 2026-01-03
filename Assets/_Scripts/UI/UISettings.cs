using UnityEngine;

public class UISettings : MonoBehaviour
{
    public void OnClickBack()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartGame");
        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
    }
}
