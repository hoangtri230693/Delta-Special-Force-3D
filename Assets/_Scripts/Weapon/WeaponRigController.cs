using DeltaSpecialForce3D.Enums;
using UnityEngine;
using UnityEngine.Animations.Rigging;


public class WeaponRigController : MonoBehaviour
{
    [SerializeField] private WeaponController _weaponController;

    private Transform _leftHandTarget;
    private Transform _rightHandTarget;


    public void InitializeRig(GameObject player)
    {
        ApplyRigTargets(player);        
        UpdateDataRig(player);

        var rigBuilder = player.GetComponentInParent<RigBuilder>();
        if (rigBuilder != null)
        {
            rigBuilder.Evaluate(Time.deltaTime);
        }
    }

    public void ResetRig()
    {
        _leftHandTarget = null;
        _rightHandTarget = null;
    }

    private void ApplyRigTargets(GameObject player)
    {
        PlayerRig playerRig = player.GetComponentInParent<PlayerRig>();
        ItemType itemType = _weaponController.WeaponStats.itemType;

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

    private void UpdateDataRig(GameObject player)
    {
        WeaponRigSO data = _weaponController.WeaponRig;
        Vector3 weaponPos = Vector3.zero;
        Vector3 weaponRot = Vector3.zero;
        Vector3 lhPos = Vector3.zero;
        Vector3 lhRot = Vector3.zero;
        Vector3 rhPos = Vector3.zero;
        Vector3 rhRot = Vector3.zero;

        PlayerTeam playerTeam = player.GetComponentInParent<PlayerTeam>();
        CharacterName characterName = playerTeam.Name;
        //Debug.Log($"Character: {characterName}, Team: {playerTeam.Team}");

        switch (characterName)
        {
            case CharacterName.Alpha:
                weaponPos = data.alpha_Weapon_Pos; weaponRot = data.alpha_Weapon_Rot;
                lhPos = data.alpha_LeftHand_Pos; lhRot = data.alpha_LeftHand_Rot;
                rhPos = data.alpha_RightHand_Pos; rhRot = data.alpha_RightHand_Rot;
                break;
            case CharacterName.Bravo:
                weaponPos = data.bravo_Weapon_Pos; weaponRot = data.bravo_Weapon_Rot;
                lhPos = data.bravo_LeftHand_Pos; lhRot = data.bravo_LeftHand_Rot;
                rhPos = data.bravo_RightHand_Pos; rhRot = data.bravo_RightHand_Rot;
                break;
            case CharacterName.Delta:
                weaponPos = data.delta_Weapon_Pos; weaponRot = data.delta_Weapon_Rot;
                lhPos = data.delta_LeftHand_Pos; lhRot = data.delta_LeftHand_Rot;
                rhPos = data.delta_RightHand_Pos; rhRot = data.delta_RightHand_Rot;
                break;
            case CharacterName.BlackViper:
            case CharacterName.IronBlood:
            case CharacterName.RedFang:
            case CharacterName.ShadowDawn:
                weaponPos = data.terrorist_Weapon_Pos; weaponRot = data.terrorist_Weapon_Rot;
                lhPos = data.terrorist_LeftHand_Pos; lhRot = data.terrorist_LeftHand_Rot;
                rhPos = data.terrorist_RightHand_Pos; rhRot = data.terrorist_RightHand_Rot;
                break;
        }

        transform.localPosition = weaponPos;
        transform.localEulerAngles = weaponRot;
        //Debug.Log("Weapon Rotation = " + weaponRot);

        if (_leftHandTarget != null)
        {
            _leftHandTarget.position = transform.TransformPoint(lhPos);
            _leftHandTarget.rotation = transform.rotation * Quaternion.Euler(lhRot);
        }

        if (_rightHandTarget != null)
        {
            bool isSpecialItem = _weaponController.WeaponStats.itemType == ItemType.MeleeItem ||
                                 _weaponController.WeaponStats.itemType == ItemType.ThrowItem;

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