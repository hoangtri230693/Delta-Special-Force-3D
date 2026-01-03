using UnityEngine;
using UnityEngine.SceneManagement;

public class UIPlayGame : MonoBehaviour
{
    public void OnClickDeathmatch()
    {
        SceneManager.LoadScene("TeamDeathmatch");
        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
    }

    public void OnClickSurvival()
    {
        SceneManager.LoadScene("ZombieSurvival");
        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
    }

    public void OnClickBack()
    {
        SceneManager.LoadScene("StartGame");
        AudioManager.instance.PlaySfx(SFXType.DefaultClick);
    }
}
