using Unity.Cinemachine;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _freeLookCamera;
    [SerializeField] private CinemachineCamera _aimCamera;
    [SerializeField] private CinemachineInputAxisController _inputAxisController;
    [SerializeField] private AimCameraController _aimCameraController;


    public void EnterAimMode()
    {
        SnapAimCameraToPlayerFoward();

        _aimCamera.Priority = 20;
        _freeLookCamera.Priority = 10;

        _inputAxisController.enabled = false;
    }

    private void SnapAimCameraToPlayerFoward()
    {
        _aimCameraController.SetYawPitchFromCameraFoward(_freeLookCamera.transform);
    }

    public void ExitAimMode()
    {
        SnapFreeLookBehindPlayer();

        _aimCamera.Priority = 10;
        _freeLookCamera.Priority = 20;

        _inputAxisController.enabled = true;
    }

    private void SnapFreeLookBehindPlayer()
    {
        CinemachineOrbitalFollow orbitalFollow = _freeLookCamera.GetComponent<CinemachineOrbitalFollow>();
        Vector3 forward = _aimCamera.transform.forward;
        float angle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        orbitalFollow.HorizontalAxis.Value = angle;
    }
}
