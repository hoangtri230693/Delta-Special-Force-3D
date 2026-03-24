using DeltaSpecialForce3D.Enums;
using System.Collections;
using UnityEngine;


public class WeaponController : MonoBehaviour
{
    [SerializeField] private WeaponStatsSO _weaponStats;
    [SerializeField] private WeaponRigSO _weaponRig;
    [SerializeField] private int _weaponID; 
    [SerializeField] private WeaponRigController _weaponRigController;
    [SerializeField] private WeaponShootController _weaponShootController;
    [SerializeField] private WeaponMeleeController _weaponMeleeController;
    [SerializeField] private WeaponThrowController _weaponThrowController;
    [SerializeField] private WeaponCollision _weaponCollision;
    [SerializeField] private Rigidbody _rigidbody;

    public WeaponStatsSO WeaponStats => _weaponStats; 
    public WeaponRigSO WeaponRig => _weaponRig;

    public GameObject _player;
    public PlayerController _playerController;
    public BotAIController _botAIController;


    private void Awake()
    {
        GetDataWeapon();    
    }

    private void Start()
    {
        InitializeWeapon(_player);
    }

    private void GetDataWeapon()
    {
        _weaponStats = WeaponDataManager.instance.GetWeaponStatsByID(_weaponID);
        _weaponRig = WeaponDataManager.instance.GetWeaponRigByID(_weaponID);
    }

    public void InitializeWeapon(GameObject player)
    {
        _player = player;
        _playerController = player.GetComponent<PlayerController>();
        _botAIController = player.GetComponent<BotAIController>();
        PlayerRig _playerRig = player.GetComponent<PlayerRig>();
        PlayerAnimationEvents _playerAnimationEvents = player.GetComponent<PlayerAnimationEvents>();   

        if (_weaponShootController != null)
        {
            _weaponShootController.enabled = true;
            _weaponShootController.InitializeShoot();
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

        HandleActiveCombat(_playerController._combatState);
        _weaponRigController.InitializeRig(player);
    }

    public void DropWeapon()
    {
        HandleActiveCombat(CombatState.None);

        if (_weaponRigController != null)
            _weaponRigController.ResetRig();

        if (_weaponShootController != null)
            _weaponShootController.enabled = false;

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
        _botAIController = null;

        _weaponCollision.enabled = true;
    }

    public void PickUpWeapon(Transform newPlayer)
    {
        var playerInventory = newPlayer.GetComponent<PlayerInventory>();

        Transform inventory = null;
        if (WeaponStats.itemType == ItemType.PrimaryItem)
            inventory = playerInventory._primaryItem.transform;
        else if (WeaponStats.itemType == ItemType.SecondaryItem)
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

        InitializeWeapon(newPlayer.gameObject);

        _playerController._itemType = WeaponStats.itemType;
        _playerController.SwitchItem();
    }

    public void HandleActiveCombat(CombatState combatState)
    {
        if (_weaponShootController != null)
            _weaponShootController.ActiveCombat(combatState);
    }

    public void HandleZoomControl(float zoomDelta)
    {
        if (_weaponShootController != null)
            _weaponShootController.ZoomControl(zoomDelta);
    }

    public void HandleWeaponShoot()
    {
        _weaponShootController.TryShoot();
    }

    private IEnumerator EnableCollision(Collider weaponCol, Collider playerCol, float delay)
    {
        yield return new WaitForSeconds(delay);
        Physics.IgnoreCollision(weaponCol, playerCol, false);
    }
}