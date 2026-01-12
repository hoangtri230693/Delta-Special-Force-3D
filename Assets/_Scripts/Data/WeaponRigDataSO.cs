using UnityEngine;

[CreateAssetMenu(fileName = "WeaponRigDataSO", menuName = "Scriptable Objects/WeaponRigDataSO")]
public class WeaponRigDataSO : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName;

    [Header("Alpha Team Rig Offsets")]
    public Vector3 alpha_Weapon_Pos;
    public Vector3 alpha_Weapon_Rot;
    public Vector3 alpha_LeftHand_Pos;
    public Vector3 alpha_LeftHand_Rot;
    public Vector3 alpha_RightHand_Pos;
    public Vector3 alpha_RightHand_Rot;

    [Header("Bravo Team Rig Offsets")]
    public Vector3 bravo_Weapon_Pos;
    public Vector3 bravo_Weapon_Rot;
    public Vector3 bravo_LeftHand_Pos;
    public Vector3 bravo_LeftHand_Rot;
    public Vector3 bravo_RightHand_Pos;
    public Vector3 bravo_RightHand_Rot;

    [Header("Delta Team Rig Offsets")]
    public Vector3 delta_Weapon_Pos;
    public Vector3 delta_Weapon_Rot;
    public Vector3 delta_LeftHand_Pos;
    public Vector3 delta_LeftHand_Rot;
    public Vector3 delta_RightHand_Pos;
    public Vector3 delta_RightHand_Rot;

    [Header("Terrorist Rig Offsets")]
    public Vector3 terrorist_Weapon_Pos;
    public Vector3 terrorist_Weapon_Rot;
    public Vector3 terrorist_LeftHand_Pos;
    public Vector3 terrorist_LeftHand_Rot;
    public Vector3 terrorist_RightHand_Pos;
    public Vector3 terrorist_RightHand_Rot;
}
