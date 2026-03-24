using UnityEngine;

[CreateAssetMenu(fileName = "CameraStatsSO", menuName = "Scriptable Objects/CameraStatsSO")]
public class CameraStatsSO : ScriptableObject
{
    public float MouseSensitivity;
    public float GamepadSensitivity;
    public float Sensitivity;
    public float PitchMin;
    public float PitchMax;
    public float ShoulderSwitchSpeed;
}
