using TMPro;
using UnityEngine;

public class ValueChangeColor : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Color _increaseColor = Color.green;
    [SerializeField] private Color _decreaseColor = Color.red;
    [SerializeField] private float _flashDuration = 0.2f;
    [SerializeField] private float _fadeSpeed = 3f;

    private Color _originalColor;
    private float _lastValue = -1;
    private float _timer = 0f;
    private bool _isFlashing = false;

    private void Start()
    {
        if (_text != null)
            _originalColor = _text.color;
    }

    private void Update()
    {
        if (_text == null) return;

        string raw = _text.text.Replace("$", "");

        if (!int.TryParse(raw, out int current))
            return;

        if (_lastValue == -1)
        {
            _lastValue = current;
            return;
        }

        if (current > _lastValue)
            FlashColor(_increaseColor);
        else if (current < _lastValue)
            FlashColor(_decreaseColor);

        if (_isFlashing)
        {
            _timer -= Time.deltaTime;

            if (_timer <= 0f)
            {
                _text.color = Color.Lerp(_text.color, _originalColor, Time.deltaTime * _fadeSpeed);

                if (Vector4.Distance(_text.color, _originalColor) < 0.01f)
                {
                    _text.color = _originalColor;
                    _isFlashing = false;
                }
            }
        }

        _lastValue = current;
    }

    private void FlashColor(Color newColor)
    {
        _text.color = newColor;
        _timer = _flashDuration;
        _isFlashing = true;
    }
}
