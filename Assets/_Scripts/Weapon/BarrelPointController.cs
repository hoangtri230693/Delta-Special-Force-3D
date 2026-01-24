using UnityEngine;

public class BarrelPointController : MonoBehaviour
{
    [SerializeField] private float _crosshairOffset = 0.01f;
    [SerializeField] private RectTransform _crosshair;
    [SerializeField] private RectTransform _scopeCrosshair;
    [SerializeField] private Camera _aimScopeCamera;
    [SerializeField] private WeaponStatsSO _weaponStats;

    private WeaponManager _weaponManager;
    
    public Vector3 _targetPosition;
    public Quaternion _targetRotation;
    public PlayerHealth _playerHealth;
    public RaycastHit _lastHit;


    private void Start()
    {
        _weaponManager = transform.GetComponentInParent<WeaponManager>();

        DisableCrosshair();
    }

    private void Update()
    {
        if (_weaponManager._playerController == null)
        {
            DisableCrosshair();
            return;
        }

        if (_weaponManager._playerController._isAiming)
        {
            GunRaycasting();
            EnableCrosshair();
        }
        else
        {
            DisableCrosshair();
        }
    }

    private void GunRaycasting()
    {
        Vector3 aimDirection = transform.forward;
        Ray barrelRay = new Ray(transform.position, aimDirection);

        if (Physics.Raycast(barrelRay, out RaycastHit hit, _weaponStats.maxDistance, _weaponStats.targetMask))
        {
            _lastHit = hit;
            _playerHealth = hit.collider.GetComponentInParent<PlayerHealth>();
            _targetPosition = hit.point + hit.normal * _crosshairOffset;
            _targetRotation = Quaternion.LookRotation(hit.normal);
            if (_playerHealth != null)
                _weaponManager._playerController._canShoot = true;
            Debug.DrawLine(barrelRay.origin, hit.point, Color.red);
        }
        else
        {
            _lastHit = new RaycastHit();
            _playerHealth = null;
            _targetPosition = barrelRay.GetPoint(_weaponStats.maxDistance);
            _targetRotation = Quaternion.LookRotation(aimDirection);
            if (_playerHealth == null)
                _weaponManager._playerController._canShoot = false;             
            Debug.DrawLine(barrelRay.origin, _targetPosition, Color.yellow);
        }
    }

    private void EnableCrosshair()
    {
        if (_weaponManager._playerLocal == null) return;

        if (_scopeCrosshair != null && _aimScopeCamera != null)
        {
            _scopeCrosshair.gameObject.SetActive(true);
            _aimScopeCamera.gameObject.SetActive(true);
        }
        else if (_crosshair != null)
        {
            _crosshair.gameObject.SetActive(true);
        }
    }

    private void DisableCrosshair()
    {
        if (_weaponManager._playerLocal == null) return;

        if (_scopeCrosshair != null && _aimScopeCamera != null)
        {
            _scopeCrosshair.gameObject.SetActive(false);
            _aimScopeCamera.gameObject.SetActive(false);
        }
        else if (_crosshair != null)
        {
            _crosshair.gameObject.SetActive(false);
        }
    }
}