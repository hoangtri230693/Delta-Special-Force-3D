using UnityEngine;
using UnityEngine.InputSystem;

public class ScopeAimController : MonoBehaviour
{
    [SerializeField] private InputActionReference _zoomAction;
    [SerializeField] private Camera _aimScopeCamera;

    private float _defaultFOV = 25f;
    private float _zoomSpeed = 2f;
    private float _currentFOV;

    private void Start()
    {
        _currentFOV = _defaultFOV;
        _aimScopeCamera.fieldOfView = _currentFOV;
    }

    private void OnEnable()
    {
        _currentFOV = _defaultFOV;
        _aimScopeCamera.fieldOfView = _currentFOV;
    }

    private void Update()
    {
        float zoomDelta = _zoomAction.action.ReadValue<float>();
        if (zoomDelta != 0)
        {
            _currentFOV -= zoomDelta * _zoomSpeed;
            _currentFOV = Mathf.Clamp(_currentFOV, 5f, 25f);
            _aimScopeCamera.fieldOfView = _currentFOV;
        }
    }
}
