using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class HoverTextColor : MonoBehaviour
{
    [SerializeField] private TMP_Text _textElement;
    [SerializeField] private Image _imageBorder;
    [SerializeField] private Color _hoverColor = Color.green;

    private Color _originalColor;
    private Color _originalBorderColor;


    private void Start()
    {
        if (_textElement != null)
        {
            _originalColor = _textElement.color;
        }
        if (_imageBorder != null)
        {
            _originalBorderColor = _imageBorder.color;
        }
    }

    public void SetHoverColor()
    {
        if (_textElement != null)
        {
            _textElement.color = _hoverColor;
            if (SceneManager.GetActiveScene().name == "StartGame") AudioManager.instance.PlaySfx(SFXType.MetalClick);
            else AudioManager.instance.PlaySfx(SFXType.RadioBeep);
        }

        if (_imageBorder != null)
        {
            _imageBorder.color = _hoverColor;
        }
    }

    public void SetNormalColor()
    {
        if (_textElement != null)
        {
            _textElement.color = _originalColor;
        }

        if (_imageBorder != null)
        {
            _imageBorder.color = _originalBorderColor;
        }
    }
}
