using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerRig : MonoBehaviour
{
    [Header("Character Rig Data")]
    [SerializeField] private CharacterRigSO _characterRigData;

    [Header("Rig References")]
    [SerializeField] private Rig _primaryItemRig;
    [SerializeField] private Rig _secondaryItemRig;
    [SerializeField] private Rig _meleeItemRig;
    [SerializeField] private Rig _throwItemRig;
    [SerializeField] private Rig _aimRig;
    [SerializeField] private MultiRotationConstraint _bodyRig;
    
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

    private float _currentIKWeight = 1f;
    private ItemType _activeIKType;


    private void LateUpdate()
    {
        switch (_activeIKType)
        {
            case ItemType.PrimaryItem:
                _primaryLeftHandIK.weight = _currentIKWeight;
                break;
            case ItemType.SecondaryItem:
                _secondaryLeftHandIK.weight = _currentIKWeight;
                break;
            case ItemType.MeleeItem:
                _meleeRightHandIK.weight = _currentIKWeight;
                break;
            case ItemType.ThrowItem:
                _throwRightHandIK.weight = _currentIKWeight;
                break;
        }
    }

    public void UpdateAimRigWeight(bool isAiming)
    {
        _aimRig.weight = isAiming ? 1f : 0f;
    }

    public void UpdateRigWeight(ItemType currentItem)
    {
        _primaryItemRig.weight = 0f;
        _secondaryItemRig.weight = 0f;
        _meleeItemRig.weight = 0f;
        _throwItemRig.weight = 0f;

        switch (currentItem)
        {
            case ItemType.PrimaryItem:
                _primaryItemRig.weight = 1f;
                break;
            case ItemType.SecondaryItem:
                _secondaryItemRig.weight = 1f;
                break;
            case ItemType.MeleeItem:
                _meleeItemRig.weight = 1f;
                break;
            case ItemType.ThrowItem:
                _throwItemRig.weight = 1f;
                break;
        }
    }

    public void UpdateIKWeight(ItemType itemType, float targetWeight)
    {
        _activeIKType = itemType;
        _currentIKWeight = targetWeight;
    }

    public void UpdateBodyOffset(ItemType currentItem, StanceState stanceState)
    {
        Vector3 newOffset = Vector3.zero;

        switch (stanceState)
        {
            case StanceState.Stand:
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
            case StanceState.Crouch:
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

        _bodyRig.data.offset = newOffset;
    }
}