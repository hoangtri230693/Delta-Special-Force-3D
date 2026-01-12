using UnityEngine;

[CreateAssetMenu(fileName = "CharacterRigSO", menuName = "Scriptable Objects/CharacterRigSO")]
public class CharacterRigSO : ScriptableObject
{
    [Header("Rig Body Standing State")]
    public Vector3 _offsetBodyStandingPrimary;
    public Vector3 _offsetBodyStandingSecondary;
    public Vector3 _offsetBodyStandingMelee;
    public Vector3 _offsetBodyStandingThrow;

    [Header("Rig Body Crouching State")]
    public Vector3 _offsetBodyCrouchingPrimary;
    public Vector3 _offsetBodyCrouchingSecondary;
    public Vector3 _offsetBodyCrouchingMelee;
    public Vector3 _offsetBodyCrouchingThrow;
}
