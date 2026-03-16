using System.Collections;
using UnityEngine;


public class WeaponController : MonoBehaviour
{
    [SerializeField] private WeaponStatsSO _weaponStats;
    public WeaponStatsSO WeaponStats => _weaponStats;
    [SerializeField] private WeaponRigController _weaponRigController;
    [SerializeField] private WeaponShootController _weaponShootController;
    [SerializeField] private WeaponMeleeController _weaponMeleeController;
    [SerializeField] private WeaponThrowController _weaponThrowController;
    [SerializeField] private WeaponCollision _weaponCollision;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private BarrelPointController _barrelPointController;

    public GameObject _player;
    public PlayerController _playerController;
    public PlayerRig _playerRig;
    public PlayerAnimationEvents _playerAnimationEvents;
    public BotAIController _botAIController;


    private void Start()
    {
        InitializeWeapon();
    }

    public void InitializeWeapon()
    {
        _player = transform.root.gameObject;
        _playerController = _player.GetComponent<PlayerController>();
        _playerRig = _player.GetComponent<PlayerRig>();
        _playerAnimationEvents = _player.GetComponent<PlayerAnimationEvents>();
        _botAIController = _player.GetComponent<BotAIController>();

        _weaponRigController.InitializeRig(_playerRig);

        if (_weaponShootController != null)
        {
            _weaponShootController.enabled = true;
            _weaponShootController.InitializeAmmo();
            _weaponShootController.AssignAnimationEvents(_playerAnimationEvents);
        }

        if (_weaponMeleeController != null)
        {
            _weaponMeleeController.InitializeMelee();
            _weaponMeleeController.AssignAnimationEvents(_playerAnimationEvents);
        }

        if (_weaponThrowController != null)
        {
            _weaponThrowController.InitializeThrow();
            _weaponThrowController.AssignAnimationEvents(_playerAnimationEvents);
        }

        if (_weaponCollision != null)
        {
            _weaponCollision.enabled = false;
        }
        
        ActiveCombat(_playerController._combatState);
    }

    public void DropWeapon()
    {
        if (_barrelPointController != null)
        {
            _barrelPointController.DisableCrosshair();
            _barrelPointController.enabled = false;
        }
        
        if (_weaponRigController != null)
            _weaponRigController.ResetRig();

        transform.SetParent(null);
        transform.position += _playerController.transform.forward * 0.8f + Vector3.up * 0.3f;

        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;

        float ejectForce = Random.Range(3.0f, 5.0f);
        float ejectAngle = Random.Range(30f, 60f);
        Vector3 forceDirection = Quaternion.Euler(0, ejectAngle, 0) * Vector3.right;
        _rigidbody.AddForce(forceDirection * ejectForce, ForceMode.Impulse);

        float randomTorqueX = Random.Range(-10f, 10f);
        float randomTorqueY = Random.Range(-10f, 10f);
        float randomTorqueZ = Random.Range(-10f, 10f);
        Vector3 randomTorque = new Vector3(randomTorqueX, randomTorqueY, randomTorqueZ);
        _rigidbody.AddTorque(randomTorque, ForceMode.Impulse);

        Collider weaponCol = GetComponent<Collider>();
        Collider playerCol = _playerController.GetComponent<Collider>();

        Physics.IgnoreCollision(weaponCol, playerCol, true);
        StartCoroutine(EnableCollision(weaponCol, playerCol, 0.3f));

        _player = null;
        _playerController = null;
        _playerRig = null;
        _playerAnimationEvents = null;
        _botAIController = null;

        _weaponShootController.enabled = false;
        _weaponCollision.enabled = true;
    }

    public void PickUpWeapon(Transform newPlayer)
    {
        var playerInventory = newPlayer.GetComponent<PlayerInventory>();

        Transform inventory = null;
        if (_weaponStats.itemType == ItemType.PrimaryItem)
            inventory = playerInventory._primaryItem.transform;
        else if (_weaponStats.itemType == ItemType.SecondaryItem)
            inventory = playerInventory._secondaryItem.transform;

        int weaponCount = 0;
        for (int i = 0; i < inventory.childCount; i++)
        {
            WeaponController weaponManager = inventory.GetChild(i).GetComponent<WeaponController>();
            if (weaponManager != null) weaponCount++;
        }

        if (weaponCount > 0) return;

        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;

        transform.SetParent(inventory);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        InitializeWeapon();

        _playerController._itemType = _weaponStats.itemType;
        _playerController.SwitchItem();
    }

    public void ActiveCombat(CombatState combatState)
    {
        if (_barrelPointController == null) return;

        switch (combatState)
        {
            case CombatState.Aim:
                _barrelPointController.enabled = true;
                _barrelPointController.EnableCrosshair();
                break;
            case CombatState.None:            
                _barrelPointController.DisableCrosshair();
                _barrelPointController.enabled = false;
                break;
        }      
    }

    public void ZoomControl(float zoomDelta)
    {
        _barrelPointController.HandleZoomControl(zoomDelta);
    }

    private IEnumerator EnableCollision(Collider weaponCol, Collider playerCol, float delay)
    {
        yield return new WaitForSeconds(delay);
        Physics.IgnoreCollision(weaponCol, playerCol, false);
    }
}