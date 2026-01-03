using UnityEngine;

public class GunRecoilController : MonoBehaviour
{
    [Header("Recoil Settings")]
    [SerializeField] private float _recoilSnappiness = 8f;
    [SerializeField] private float _recoilReturnSpeed = 4f;

    private Quaternion _originalRotation;
    private Vector3 _currentRotation;
    private Vector3 _targetRotation;

    private float _recoilX;
    private float _recoilY;
    private float _recoilZ;

    private void Start()
    {
        _originalRotation = transform.localRotation;
    }

    private void Update()
    {
        _targetRotation = Vector3.Lerp(_targetRotation, Vector3.zero, Time.deltaTime * _recoilReturnSpeed);
        _currentRotation = Vector3.Slerp(_currentRotation, _targetRotation, Time.deltaTime * _recoilSnappiness);
        transform.localRotation = _originalRotation * Quaternion.Euler(_currentRotation);
    }

    public Vector3 ApplyRecoil(float recoilAmount)
    {
        _recoilX = -recoilAmount;
        _recoilY = Random.Range(-recoilAmount * 0.2f, recoilAmount * 0.2f);
        _recoilZ = -recoilAmount;

        Vector3 recoilVector = new Vector3(_recoilX, _recoilY, _recoilZ);

        _targetRotation += recoilVector;

        return recoilVector;
    }
}
