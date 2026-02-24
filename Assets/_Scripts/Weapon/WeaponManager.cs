using System.Collections;
using UnityEngine;


public class WeaponManager : MonoBehaviour
{
    [Header("Component Dynamic")]
    public GameObject _playerOwner;

    [Header("Component Static")]
    public WeaponStatsSO _weaponStats;
    public WeaponRigDataSO _weaponRigData;

    [Header("Component Assigned")]
    [SerializeField] private WeaponRigController _weaponRigController;
    [SerializeField] private WeaponShootController _weaponShootController;
    [SerializeField] private WeaponMeleeController _weaponMeleeController;
    [SerializeField] private WeaponThrowController _weaponThrowController;
    [SerializeField] private WeaponCollision _weaponCollision;
    [SerializeField] private WeaponAudio _weaponAudio;
    [SerializeField] private Rigidbody _rigidbody;

    public PlayerController _playerController;
    public PlayerRig _playerRig;
    public PlayerAnimationEvents _playerAnimationEvents;
    public PlayerLocal _playerLocal;
    public BotController _botController;


    private void Start()
    {
        InitializeWeapon();
    }

    public void InitializeWeapon()
    {
        _playerOwner = transform.root.gameObject;
        _playerController = _playerOwner.GetComponent<PlayerController>();
        _playerRig = _playerOwner.GetComponent<PlayerRig>();
        _playerAnimationEvents = _playerOwner.GetComponent<PlayerAnimationEvents>();
        _playerLocal = _playerOwner.GetComponent<PlayerLocal>();
        _botController = _playerOwner.GetComponent<BotController>();

        _weaponRigController.InitializeRig(_playerRig);

        if (_weaponShootController != null)
        {
            _weaponShootController.InitializeAmmo();
            _weaponShootController.AssignAnimationEvents(_playerAnimationEvents);
            _weaponAudio.PlayAudioCock();
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
    }

    public void DropWeapon()
    {
        if (_weaponRigController != null)
            _weaponRigController.ResetRig();

        transform.SetParent(null);
        transform.position += _playerController.transform.forward * 0.8f + Vector3.up * 0.3f;

        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

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
        StartCoroutine(EnableCollisionAfterDelay(weaponCol, playerCol, 0.3f));

        _playerOwner = null;
        _playerController = null;
        _playerRig = null;
        _playerAnimationEvents = null;
        _playerLocal = null;
        _botController = null;

        _weaponCollision.enabled = true;
    }

    public void AssignToPlayer(Transform newPlayer)
    {
        var playerInventory = newPlayer.GetComponent<PlayerInventory>();
        var playerLocal = newPlayer.GetComponent<PlayerLocal>();

        Transform inventory = null;
        if (_weaponStats.itemType == ItemType.PrimaryItem)
        {
            inventory = playerInventory._primaryItem.transform;
        }
        else if (_weaponStats.itemType == ItemType.SecondaryItem)
        {
            inventory = playerInventory._secondaryItem.transform;
        }

        int weaponCount = 0;
        for (int i = 0; i < inventory.childCount; i++)
        {
            WeaponManager weaponManager = inventory.GetChild(i).GetComponent<WeaponManager>();
            if (weaponManager != null) weaponCount++;
        }

        if (weaponCount > 0) return;

        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;

        transform.SetParent(inventory);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        _playerOwner = newPlayer.gameObject;
        _playerController = newPlayer.GetComponent<PlayerController>();
        _playerRig = newPlayer.GetComponent<PlayerRig>();
        _playerAnimationEvents = newPlayer.GetComponent<PlayerAnimationEvents>();
        _playerLocal = newPlayer.GetComponent<PlayerLocal>();
        _botController = newPlayer.GetComponent<BotController>();

        _weaponCollision.enabled = false;

        _weaponRigController.InitializeRig(_playerRig);

        if (_weaponShootController != null)
        {
            _weaponShootController.AssignAnimationEvents(_playerAnimationEvents);
            _weaponAudio.PlayAudioCock();
        }

        _playerController._itemType = _weaponStats.itemType;
        _playerController.SwitchItem();
    }

    private IEnumerator EnableCollisionAfterDelay(Collider weaponCol, Collider playerCol, float delay)
    {
        yield return new WaitForSeconds(delay);
        Physics.IgnoreCollision(weaponCol, playerCol, false);
    }
}