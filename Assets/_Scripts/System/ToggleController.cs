using UnityEngine;
using UnityEngine.UI;

public class ToggleController : MonoBehaviour
{
    [SerializeField] private Toggle _toggle;
    [SerializeField] private Image _backgroundToggle;
    [SerializeField] private RectTransform _handleRectTransform;
    [SerializeField] private Color _onBackgroundColor = Color.green;
    [SerializeField] private Color _offBackgroundColor = Color.white;
    [SerializeField] private Vector2 _onPosition;
    [SerializeField] private Vector2 _offPosition;
    [SerializeField] private float _speed = 10f;

    private bool _currentState;

    private void Start()
    {
        _handleRectTransform.anchoredPosition = _toggle.isOn ? _onPosition : _offPosition;
        _backgroundToggle.color = _toggle.isOn ? _onBackgroundColor : _offBackgroundColor;
        _currentState = _toggle.isOn;
    }

    private void Update()
    {
        if (_currentState != _toggle.isOn)
        {
            _currentState = _toggle.isOn;
        }

        Vector2 _targetPosition = _toggle.isOn ? _onPosition : _offPosition;
        _handleRectTransform.anchoredPosition = Vector2.Lerp(_handleRectTransform.anchoredPosition, _targetPosition, _speed * Time.deltaTime);
        _backgroundToggle.color = Color.Lerp(_backgroundToggle.color, _toggle.isOn ? _onBackgroundColor : _offBackgroundColor, _speed * Time.deltaTime);
    }
}
