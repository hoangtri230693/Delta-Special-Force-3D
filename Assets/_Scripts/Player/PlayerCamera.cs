using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [Header("Cinemachine Components")]
    [SerializeField] private CinemachineCamera _freeLookCamera;
    [SerializeField] private CinemachineCamera _aimCamera;
    [SerializeField] private CinemachineInputAxisController _inputAxisController;
    [SerializeField] private CinemachineThirdPersonFollow _aimFollow;

    [Header("Targets & Wrappers")]
    [SerializeField] private Transform _yawTarget;
    [SerializeField] private Transform _pitchWrapper;
    [SerializeField] private Transform _pitchTarget;

    [Header("Settings")]
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

    private void Update()
    {
        if (Mathf.Abs(_aimFollow.CameraSide - _targetCameraSide) > 0.001f)
        {
            _aimFollow.CameraSide = Mathf.Lerp(
                _aimFollow.CameraSide,
                _targetCameraSide,
                Time.deltaTime * _cameraStats.ShoulderSwitchSpeed);
        }
    }

    public void UpdateCamera(Vector2 lookInput)
    {
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
    }

    public void EnterAimMode()
    {
        SnapAimCameraToPlayerFoward();

        _aimCamera.Priority = 20;
        _freeLookCamera.Priority = 10;

        _inputAxisController.enabled = false;
    }

    public void ExitAimMode()
    {
        SnapFreeLookBehindPlayer();

        _aimCamera.Priority = 10;
        _freeLookCamera.Priority = 20;

        _inputAxisController.enabled = true;
    }

    public void SwitchShoulder()
    {
        _targetCameraSide = _aimFollow.CameraSide < 0.5f ? 1f : 0f;
    }

    private void SnapAimCameraToPlayerFoward()
    {
        Vector3 flatForward = _freeLookCamera.transform.forward;
        flatForward.y = 0;

        if (flatForward.sqrMagnitude < 0.001f) return;

        _yaw = Quaternion.LookRotation(flatForward).eulerAngles.y;
        _yawTarget.rotation = Quaternion.Euler(0f, _yaw, 0f);
        _pitchTarget.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    private void SnapFreeLookBehindPlayer()
    {
        CinemachineOrbitalFollow orbitalFollow = _freeLookCamera.GetComponent<CinemachineOrbitalFollow>();
        Vector3 forward = _aimCamera.transform.forward;
        float angle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        orbitalFollow.HorizontalAxis.Value = angle;
    }
}
