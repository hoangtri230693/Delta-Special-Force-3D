using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory")]
    public GameObject _primaryItem;
    public GameObject _secondaryItem;
    public GameObject _meleeItem;
    public GameObject _throwItem;
    
    private PlayerController _playerController;
    private PlayerRig _playerRig;

    private ItemType _lastItemType;
    private StanceState _lastStanceState;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _playerRig = GetComponent<PlayerRig>();
    }

    private void Start()
    {
        _lastItemType = _playerController._currentItem;
        _lastStanceState = _playerController._stanceState;
        UpdateItem();
        UpdateRigState();
    }


    private void LateUpdate()
    {
        if (_playerController._currentItem != _lastItemType || _playerController._actionState == ActionState.SwitchItem)
        {
            UpdateItem();
            UpdateRigState();    
            _lastItemType = _playerController._currentItem;
        }

        if (_playerController._stanceState != _lastStanceState)
        {
            _lastStanceState = _playerController._stanceState;
            UpdateOffsetBody();
        }
    }
    
    private void UpdateItem()
    {
        _primaryItem.SetActive(_playerController._currentItem == ItemType.PrimaryItem);
        _secondaryItem.SetActive(_playerController._currentItem == ItemType.SecondaryItem);
        _meleeItem.SetActive(_playerController._currentItem == ItemType.MeleeItem);
        _throwItem.SetActive(_playerController._currentItem == ItemType.ThrowItem);
    }

    private void UpdateRigState()
    {
        _playerRig.UpdateRigWeight(_playerController._currentItem);
        _playerRig.UpdateBodyOffset(_playerController._currentItem, _playerController._stanceState);
    }

    private void UpdateOffsetBody()
    {
        _playerRig.UpdateBodyOffset(_playerController._currentItem, _playerController._stanceState);
    }
}