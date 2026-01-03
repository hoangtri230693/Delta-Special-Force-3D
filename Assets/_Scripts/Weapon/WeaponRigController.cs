using UnityEngine;

public enum NameTeam { None, Alpha, Bravo, Delta, Terrorist }

public class WeaponRigController : MonoBehaviour
{
    [SerializeField] private WeaponManager _weaponManager;

    [Header("Target Transforms")]
    private Transform _leftHandTarget;
    private Transform _rightHandTarget;

    private NameTeam _currentTeam = NameTeam.None;

    public void ResetRig()
    {
        _leftHandTarget = null;
        _rightHandTarget = null;
        _currentTeam = NameTeam.None;

        transform.SetParent(null);
    }

    public void InitializeRig(PlayerRig playerRig)
    {
        AssignRigTargets(playerRig);
        ApplyPlayerTeam();
        ApplyDataRig();
    }

    private void AssignRigTargets(PlayerRig playerRig)
    {
        ItemType itemType = _weaponManager._weaponStats.itemType;

        switch (itemType)
        {
            case ItemType.PrimaryItem:
                _leftHandTarget = playerRig._primaryLeftHandTarget;
                _rightHandTarget = playerRig._primaryRightHandTarget;
                break;
            case ItemType.SecondaryItem:
                _leftHandTarget = playerRig._secondaryLeftHandTarget;
                _rightHandTarget = playerRig._secondaryRightHandTarget;
                break;
            case ItemType.MeleeItem:
                _rightHandTarget = playerRig._meleeRightHandTarget;
                break;
            case ItemType.ThrowItem:
                _rightHandTarget = playerRig._throwRightHandTarget;
                break;
            default:
                break;
        }
    }

    private void ApplyPlayerTeam()
    {
        GameObject rootPlayer = transform.root.gameObject;

        if (rootPlayer.CompareTag("AlphaTeam")) _currentTeam = NameTeam.Alpha;
        else if (rootPlayer.CompareTag("BravoTeam")) _currentTeam = NameTeam.Bravo;
        else if (rootPlayer.CompareTag("DeltaTeam")) _currentTeam = NameTeam.Delta;
        else if (rootPlayer.CompareTag("Terrorist")) _currentTeam = NameTeam.Terrorist;
        else _currentTeam = NameTeam.None;
    }

    private void ApplyDataRig()
    {
        if (_currentTeam == NameTeam.None) return;

        WeaponRigDataSO data = _weaponManager._weaponRigData;

        Vector3 weaponPos = Vector3.zero;
        Vector3 weaponRot = Vector3.zero;
        Vector3 lhPos = Vector3.zero;
        Vector3 lhRot = Vector3.zero;
        Vector3 rhPos = Vector3.zero;
        Vector3 rhRot = Vector3.zero;

        switch (_currentTeam)
        {
            case NameTeam.Alpha:
                weaponPos = data.alpha_Weapon_Pos; weaponRot = data.alpha_Weapon_Rot;
                lhPos = data.alpha_LeftHand_Pos; lhRot = data.alpha_LeftHand_Rot;
                rhPos = data.alpha_RightHand_Pos; rhRot = data.alpha_RightHand_Rot;
                break;
            case NameTeam.Bravo:
                weaponPos = data.bravo_Weapon_Pos; weaponRot = data.bravo_Weapon_Rot;
                lhPos = data.bravo_LeftHand_Pos; lhRot = data.bravo_LeftHand_Rot;
                rhPos = data.bravo_RightHand_Pos; rhRot = data.bravo_RightHand_Rot;
                break;
            case NameTeam.Delta:
                weaponPos = data.delta_Weapon_Pos; weaponRot = data.delta_Weapon_Rot;
                lhPos = data.delta_LeftHand_Pos; lhRot = data.delta_LeftHand_Rot;
                rhPos = data.delta_RightHand_Pos; rhRot = data.delta_RightHand_Rot;
                break;
            case NameTeam.Terrorist:
                weaponPos = data.terrorist_Weapon_Pos; weaponRot = data.terrorist_Weapon_Rot;
                lhPos = data.terrorist_LeftHand_Pos; lhRot = data.terrorist_LeftHand_Rot;
                rhPos = data.terrorist_RightHand_Pos; rhRot = data.terrorist_RightHand_Rot;
                break;
        }

        transform.localPosition = weaponPos;
        transform.localRotation = Quaternion.Euler(weaponRot);

        if (_leftHandTarget != null)
        {
            _leftHandTarget.position = transform.TransformPoint(lhPos);
            _leftHandTarget.rotation = transform.rotation * Quaternion.Euler(lhRot);
        }

        if (_rightHandTarget != null)
        {
            bool isSpecialItem = _weaponManager._weaponStats.itemType == ItemType.MeleeItem ||
                                 _weaponManager._weaponStats.itemType == ItemType.ThrowItem;

            if (isSpecialItem)
            {
                _rightHandTarget.localPosition = rhPos;
                _rightHandTarget.localEulerAngles = rhRot;
            }
            else
            {
                _rightHandTarget.position = transform.TransformPoint(rhPos);
                _rightHandTarget.rotation = transform.rotation * Quaternion.Euler(rhRot);
            }
        }
    }
}