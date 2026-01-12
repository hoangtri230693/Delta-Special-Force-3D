using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class AimCameraController : MonoBehaviour
{
    [SerializeField] private Transform _yawTarget;
    [SerializeField] private Transform _pitchWrapper;
    [SerializeField] private Transform _pitchTarget;

    [SerializeField] private InputActionReference _lookAction;
    [SerializeField] private InputActionReference _switchShoulderAction;
    [SerializeField] private CinemachineThirdPersonFollow _aimCamera;

    [SerializeField] private CameraStatsSO _cameraStats;

    private float _yaw;
    private float _pitch;
    private float _targetCameraSide = 1f;

    private void Awake()
    {
        Vector3 angles = _yawTarget.rotation.eulerAngles;
        _yaw = angles.y;
        _pitch = angles.x;
    }

    private void OnEnable()
    {
        _switchShoulderAction.action.performed += OnSwitchShoulder;
    }

    private void OnDisable()
    {
        _switchShoulderAction.action.performed -= OnSwitchShoulder;
    }

    private void OnSwitchShoulder(InputAction.CallbackContext context)
    {
        _targetCameraSide = _aimCamera.CameraSide < 0.5f ? 1f : 0f;
    }


    private void Update()
    {
        Vector2 lookInput = _lookAction.action.ReadValue<Vector2>();

        if (Mouse.current != null && Mouse.current.delta.IsActuated())
        {
            lookInput *= _cameraStats.MouseSensitivity;
        }
        else if (Gamepad.current != null && Gamepad.current.rightStick.IsActuated())
        {
            lookInput *= _cameraStats.GamepadSensitivity;
        }

        _yaw += lookInput.x * _cameraStats.Sensitivity;
        _pitch -= lookInput.y * _cameraStats.Sensitivity;

        _yawTarget.rotation = Quaternion.Euler(0f, _yaw, 0f);
        if (transform.root.CompareTag("Terrorist"))
        {
            _pitchWrapper.localRotation = Quaternion.Euler(
            Mathf.Clamp(_pitch, _cameraStats.PitchMin, _cameraStats.PitchMax), 0f, 0f);

            _pitchTarget.localRotation = Quaternion.Euler(
                0f, 90f, Mathf.Clamp(_pitch, -_cameraStats.PitchMax, -_cameraStats.PitchMin) - 75f);
        }
        else
        {
            _pitchTarget.localRotation = Quaternion.Euler(
            Mathf.Clamp(_pitch, _cameraStats.PitchMin, _cameraStats.PitchMax), 0f, 0f);
        }
    
        _aimCamera.CameraSide = Mathf.Lerp(
            _aimCamera.CameraSide,
            _targetCameraSide,
            Time.deltaTime * _cameraStats.ShoulderSwitchSpeed);
    }

    public void SetYawPitchFromCameraFoward(Transform cameraTransform)
    {
        Vector3 flatFoward = cameraTransform.forward;
        flatFoward.y = 0;

        if (flatFoward.sqrMagnitude < 0.001f)
            return;

        _yaw = Quaternion.LookRotation(flatFoward).eulerAngles.y;

        _yawTarget.rotation = Quaternion.Euler(0f, _yaw, 0f);
        _pitchTarget.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }
}
