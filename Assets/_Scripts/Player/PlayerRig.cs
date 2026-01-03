using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerRig : MonoBehaviour
{
    [Header("Character Rig Data")]
    [SerializeField] private CharacterRigSO _characterRigData;

    [Header("Rig References")]
    [SerializeField] private Rig _rigPrimaryItem;
    [SerializeField] private Rig _rigSecondaryItem;
    [SerializeField] private Rig _rigMeleeItem;
    [SerializeField] private Rig _rigThrowItem;

    [Header("Rig Body Constraint")]
    [SerializeField] private MultiRotationConstraint _bodyRotationConstraint;

    [Header("Rig Weapon IK")]
    public TwoBoneIKConstraint _primaryLeftHandIK;
    public TwoBoneIKConstraint _primaryRightHandIK;
    public TwoBoneIKConstraint _secondaryLeftHandIK;
    public TwoBoneIKConstraint _secondaryRightHandIK;
    public TwoBoneIKConstraint _meleeRightHandIK;
    public TwoBoneIKConstraint _throwRightHandIK;

    [Header("Rig Weapon Target")]
    public Transform _primaryLeftHandTarget;
    public Transform _primaryRightHandTarget;
    public Transform _secondaryLeftHandTarget;
    public Transform _secondaryRightHandTarget;
    public Transform _meleeRightHandTarget;
    public Transform _throwRightHandTarget;

    public void UpdateRigWeight(ItemType currentItem)
    {
        _rigPrimaryItem.weight = 0f;
        _rigSecondaryItem.weight = 0f;
        _rigMeleeItem.weight = 0f;
        _rigThrowItem.weight = 0f;

        switch (currentItem)
        {
            case ItemType.PrimaryItem:
                _rigPrimaryItem.weight = 1f;
                break;
            case ItemType.SecondaryItem:
                _rigSecondaryItem.weight = 1f;
                break;
            case ItemType.MeleeItem:
                _rigMeleeItem.weight = 1f;
                break;
            case ItemType.ThrowItem:
                _rigThrowItem.weight = 1f;
                break;
        }
    }

    public void UpdateBodyOffset(ItemType currentItem, StanceState stanceState)
    {
        Vector3 newOffset = Vector3.zero;

        switch (stanceState)
        {
            case StanceState.Standing:
                switch (currentItem)
                {
                    case ItemType.PrimaryItem:
                        newOffset = _characterRigData._offsetBodyStandingPrimary;
                        break;
                    case ItemType.SecondaryItem:
                        newOffset = _characterRigData._offsetBodyStandingSecondary;
                        break;
                    case ItemType.MeleeItem:
                        newOffset = _characterRigData._offsetBodyStandingMelee;
                        break;
                    case ItemType.ThrowItem:
                        newOffset = _characterRigData._offsetBodyStandingThrow;
                        break;
                    default:
                        newOffset = Vector3.zero;
                        break;
                }
                break;
            case StanceState.Crouching:
                switch (currentItem)
                {
                    case ItemType.PrimaryItem:
                        newOffset = _characterRigData._offsetBodyCrouchingPrimary;
                        break;
                    case ItemType.SecondaryItem:
                        newOffset = _characterRigData._offsetBodyCrouchingSecondary;
                        break;
                    case ItemType.MeleeItem:
                        newOffset = _characterRigData._offsetBodyCrouchingMelee;
                        break;
                    case ItemType.ThrowItem:
                        newOffset = _characterRigData._offsetBodyCrouchingThrow;
                        break;
                    default:
                        newOffset = Vector3.zero;
                        break;
                }
                break;
        }

        _bodyRotationConstraint.data.offset = newOffset;
    }
}