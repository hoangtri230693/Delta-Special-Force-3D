using UnityEngine;


public class BarrelPointController : MonoBehaviour
{
    [SerializeField] private WeaponController _weaponController;
    [SerializeField] private float _crosshairOffset = 0.01f;
    [SerializeField] private RectTransform _crosshair;
    [SerializeField] private RectTransform _scopeCrosshair;
    [SerializeField] private Camera _aimScopeCamera;
    [SerializeField] private CrosshairController _crosshairController;
    [SerializeField] private ScopeAimController _scopeAimController;

    public Vector3 _targetPosition;
    public Quaternion _targetRotation;
    public PlayerHealth _playerHealth;
    public ZombieHealth _zombieHealth;
    public RaycastHit _lastHit;


    private void Update()
    {
        GunRaycasting();
    }

    private void GunRaycasting()
    {
        Vector3 aimDirection = transform.forward;
        Ray barrelRay = new Ray(transform.position, aimDirection);

        if (Physics.Raycast(barrelRay, out RaycastHit hit, _weaponController.WeaponStats.maxDistance, _weaponController.WeaponStats.targetMask))
        {
            _lastHit = hit;
            _playerHealth = hit.collider.GetComponentInParent<PlayerHealth>();
            _zombieHealth = hit.collider.GetComponentInParent<ZombieHealth>();
            _targetPosition = hit.point + hit.normal * _crosshairOffset;
            if (_playerHealth != null && _weaponController._botAIController != null)
                _weaponController._botAIController.SetCanShoot(true);
            Debug.DrawLine(barrelRay.origin, hit.point, Color.red);
        }
        else
        {
            _lastHit = new RaycastHit();
            _playerHealth = null;
            _zombieHealth = null;
            _targetPosition = barrelRay.GetPoint(_weaponController.WeaponStats.maxDistance);
            if (_playerHealth == null && _weaponController._botAIController != null)
                _weaponController._botAIController.SetCanShoot(false);
            Debug.DrawLine(barrelRay.origin, _targetPosition, Color.green);
        }

        if (_crosshairController != null)
            _crosshairController.UpdateTransform(_targetPosition);
    }

    public void EnableCrosshair()
    {
        if (_scopeCrosshair != null && _aimScopeCamera != null)
        {
            if (_weaponController._botAIController != null) return;

            _scopeCrosshair.gameObject.SetActive(true);
            _aimScopeCamera.gameObject.SetActive(true);
        }
        else if (_crosshair != null)
        {
            _crosshair.gameObject.SetActive(true);
        }
    }

    public void DisableCrosshair()
    {
        if (_scopeCrosshair != null && _aimScopeCamera != null)
        {
            if (_weaponController._botAIController != null) return;

            _scopeCrosshair.gameObject.SetActive(false);
            _aimScopeCamera.gameObject.SetActive(false);
        }
        else if (_crosshair != null)
        {
            _crosshair.gameObject.SetActive(false);
        }
    }

    public void HandleZoomControl(float zoomDelta)
    {
        if (_scopeAimController != null)
            _scopeAimController.UpdateZoomControl(zoomDelta);
    }
}