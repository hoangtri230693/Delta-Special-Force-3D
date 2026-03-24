using DeltaSpecialForce3D.Enums;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    public WeaponShootController _primaryShootController;
    public WeaponShootController _secondaryShootController;
    public WeaponMeleeController _meleeController;
    public WeaponThrowController _throwController;

    private PlayerController _playerController;
    private PlayerRig _playerRig;
    private PlayerAudio _playerAudio;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _playerRig = GetComponent<PlayerRig>();
        _playerAudio = GetComponent<PlayerAudio>();
    }

    public void FootStepEvent()
    {
        _playerAudio.PlayCharacterSound(CharacterSoundType.FootStep);
    }

    public void JumpEvent()
    {
        _playerAudio.PlayCharacterSound(CharacterSoundType.LandStep);
    }

    public void ReloadingEvent()
    {
        ItemType currentItem = _playerController._itemType;
        _playerRig.UpdateIKWeight(currentItem, 0f);

        if (currentItem == ItemType.PrimaryItem) _primaryShootController.HandleReload();
        else if (currentItem == ItemType.SecondaryItem) _secondaryShootController.HandleReload();
    }

    public void ReloadingEvent1()
    {
        ItemType currentItem = _playerController._itemType;
        _playerRig.UpdateIKWeight(currentItem, 1f);

        if (currentItem == ItemType.PrimaryItem) _primaryShootController.HandleReload1();
        else if (currentItem == ItemType.SecondaryItem) _secondaryShootController.HandleReload1();
    }

    public void ThrowGrenadeEvent()
    {
        _playerRig.UpdateIKWeight(ItemType.ThrowItem, 0f);
    }

    public void ThrowGrenadeEvent1()
    {
        if (_throwController != null)
        {
            _throwController.ThrowGrenade();
        }
    }

    public void ThrowGrenadeEvent2()
    {
        _playerRig.UpdateIKWeight(ItemType.ThrowItem, 1f);
    }

    public void StabbingKnifeEvent()
    {
        _playerRig.UpdateIKWeight(ItemType.MeleeItem, 0f);
    }

    public void StabbingKnifeEvent1()
    {
        _meleeController.StabbingKnife();
    }

    public void StabbingKnifeEvent2()
    {
        _playerRig.UpdateIKWeight(ItemType.MeleeItem, 1f);
    }
}