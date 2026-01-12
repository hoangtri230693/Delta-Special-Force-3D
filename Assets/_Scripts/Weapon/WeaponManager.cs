using System.Collections;
using UnityEngine;


public class WeaponManager : MonoBehaviour
{
    [Header("Component Dynamic")]
    public PlayerController _playerController;
    public PlayerInventory _playerInventory;
    public PlayerAnimationEvents _playerAnimationEvents;
    public PlayerRig _playerRig;
    public PlayerHealth _playerHealth;
    public PlayerLocal _playerLocal;

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


    private void Start()
    {
        _playerController = GetComponentInParent<PlayerController>();
        _playerInventory = GetComponentInParent<PlayerInventory>();
        _playerAnimationEvents = GetComponentInParent<PlayerAnimationEvents>();
        _playerRig = GetComponentInParent<PlayerRig>();
        _playerHealth = GetComponentInParent<PlayerHealth>();
        _playerLocal = GetComponentInParent<PlayerLocal>();

        if (_playerRig != null)
        {
            _weaponRigController.InitializeRig(_playerRig);
        }

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

    private void Update()
    {
        if (_playerController == null) return;

        if (_playerController._actionState == ActionState.Drop || _playerController._lifeState == LifeState.None)
        {
            DropWeapon(transform);
        }
    }

    public void DropWeapon(Transform weapon)
    {
        if (_weaponStats.itemType == ItemType.MeleeItem || _weaponStats.itemType == ItemType.ThrowItem) return;

        if (_weaponRigController != null)
            _weaponRigController.ResetRig();

        weapon.SetParent(null);
        weapon.position += _playerController.transform.forward * 0.8f + Vector3.up * 0.3f;

        _playerController._currentItem = _weaponStats.itemType;
        _playerController._actionState = ActionState.SwitchItem;

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
        
        _playerController = null;
        _playerInventory = null;
        _playerAnimationEvents = null;
        _playerRig = null;
        _playerHealth = null;
        _playerLocal = null;

        if (_weaponCollision != null) _weaponCollision.enabled = true;

        Debug.Log("Drop Weapon");
    }

    public void AssignToPlayer(Transform newPlayer)
    {
        _playerInventory = newPlayer.GetComponent<PlayerInventory>();
        _playerLocal = newPlayer.GetComponent<PlayerLocal>();

        Transform inventory = null;
        if (_weaponStats.itemType == ItemType.PrimaryItem && _playerLocal != null)
        {
            inventory = _playerInventory._primaryItem.transform;
        }
        else if (_weaponStats.itemType == ItemType.SecondaryItem && _playerLocal != null)
        {
            inventory = _playerInventory._secondaryItem.transform;
        }

        int weaponCount = 0;
        for (int i = 0; i < inventory.childCount; i++)
        {
            WeaponManager weaponManager = inventory.GetChild(i).GetComponent<WeaponManager>();
            if (weaponManager != null) weaponCount++;
        }

        if (weaponCount > 0)
        {
            _playerInventory = null;
            _playerLocal = null;
            return;
        }
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;

        transform.SetParent(inventory);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        _playerController = newPlayer.GetComponent<PlayerController>();    
        _playerAnimationEvents = newPlayer.GetComponent<PlayerAnimationEvents>();
        _playerRig = newPlayer.GetComponent<PlayerRig>();
        _playerHealth = newPlayer.GetComponent<PlayerHealth>();
        _playerLocal = newPlayer.GetComponent<PlayerLocal>();

        _weaponCollision.enabled = false;

        if (_playerRig != null)
        {
            _weaponRigController.InitializeRig(_playerRig);
        }

        if (_weaponShootController != null)
        {
            _weaponShootController.AssignAnimationEvents(_playerAnimationEvents);
            _weaponAudio.PlayAudioCock();
        }

        _playerController._currentItem = _weaponStats.itemType;
        _playerController._actionState = ActionState.SwitchItem;
        Debug.Log("Weapon Assigned to Player");
    }

    private IEnumerator EnableCollisionAfterDelay(Collider weaponCol, Collider playerCol, float delay)
    {
        yield return new WaitForSeconds(delay);
        Physics.IgnoreCollision(weaponCol, playerCol, false);
    }
}