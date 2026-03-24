using DeltaSpecialForce3D.Enums;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UITutorial : MonoBehaviour
{
    [SerializeField] private GameObject _panelViewLore;
    [SerializeField] private GameObject _panelViewBasicGuide;

    private void Start()
    {
        OnViewLore();
    }

    private void OnViewLore()
    {
        _panelViewLore.SetActive(true);
        _panelViewBasicGuide.SetActive(false);
    }

    public void OnClickBack()
    {
        SceneManager.LoadScene("StartGame");
        AudioManager.instance.PlaySfx(SFXSoundType.MetalClick);
    }
    
    public void OnClickSkip()
    {
        _panelViewLore.SetActive(false);
        _panelViewBasicGuide.SetActive(true);
    }
}
