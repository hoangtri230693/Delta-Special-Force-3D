using UnityEngine;

[CreateAssetMenu(fileName = "CameraStatsSO", menuName = "Scriptable Objects/CameraStatsSO")]
public class CameraStatsSO : ScriptableObject
{
    public float MouseSensitivity = 0.05f;
    public float GamepadSensitivity = 0.5f;
    public float Sensitivity = 1.5f;
    public float PitchMin = -45f;
    public float PitchMax = 45f;
    public float ShoulderSwitchSpeed = 5f;
}
