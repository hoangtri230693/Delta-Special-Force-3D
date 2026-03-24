using UnityEngine;
using DeltaSpecialForce3D.Enums;


public class UISettings : MonoBehaviour
{
    public void OnClickBack()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartGame");
        AudioManager.instance.PlaySfx(SFXSoundType.MetalClick);
    }
}
