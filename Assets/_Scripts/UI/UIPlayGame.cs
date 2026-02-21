using UnityEngine;
using UnityEngine.SceneManagement;

public class UIPlayGame : MonoBehaviour
{
    [SerializeField] private GameObject _panelMain;
    [SerializeField] private GameObject _buttonBack;
    [SerializeField] private GameObject _panelShop;


    private void Start()
    {
        _panelShop.SetActive(false);
        _panelMain.SetActive(true);
        _buttonBack.SetActive(true);
    }

    //-------IMPLEMENT PUBLIC METHODS-------//

    public void OnClickShop()
    {
        if (_panelShop.activeSelf)
        {
            _panelShop.SetActive(false);
            _buttonBack.SetActive(true);
            _panelMain.SetActive(true);
            AudioManager.instance.PlaySfx(SFXType.MetalClick);
        }
        else
        {
            _panelShop.SetActive(true);
            _buttonBack.SetActive(false);
            _panelMain.SetActive(false);
            AudioManager.instance.PlaySfx(SFXType.MetalClick);
        }
    }

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
