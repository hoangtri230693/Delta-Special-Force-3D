using UnityEngine;
using UnityEngine.SceneManagement;
using DeltaSpecialForce3D.Enums;


public class UIPlayGame : MonoBehaviour
{
    [SerializeField] private GameObject _panelMain;
    [SerializeField] private GameObject _buttonBack;
    [SerializeField] private GameObject _panelShop;


    private void Start()
    {
        OnMainMenu();
    }

    private void OnMainMenu()
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
            AudioManager.instance.PlaySfx(SFXSoundType.MetalClick);
        }
        else
        {
            _panelShop.SetActive(true);
            _buttonBack.SetActive(false);
            _panelMain.SetActive(false);
            AudioManager.instance.PlaySfx(SFXSoundType.MetalClick);
        }
    }

    public void OnClickDeathmatch()
    {
        GameplayDataManager.instance.gameMode = GameMode.TeamDeathmatch;
        SceneManager.LoadScene("TeamDeathmatch");
        AudioManager.instance.PlaySfx(SFXSoundType.DefaultClick);
    }

    public void OnClickSurvival()
    {
        GameplayDataManager.instance.gameMode = GameMode.ZombieSurvival;
        SceneManager.LoadScene("ZombieSurvival");
        AudioManager.instance.PlaySfx(SFXSoundType.DefaultClick);
    }

    public void OnClickBack()
    {
        SceneManager.LoadScene("StartGame");
        AudioManager.instance.PlaySfx(SFXSoundType.MetalClick);
    }
}
