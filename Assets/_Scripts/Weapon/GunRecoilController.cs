using UnityEngine;

public class GunRecoilController : MonoBehaviour
{
    [Header("Recoil Settings")]
    [SerializeField] private float _recoilSnappiness = 8f;
    [SerializeField] private float _recoilReturnSpeed = 4f;

    private Quaternion _originalRotation;
    private Vector3 _currentRotation;
    private Vector3 _targetRotation;


    private void OnEnable()
    {
        CaptureOriginalRotation();
    }

    private void Update()
    {
        ApplyRecoilSmoothing();
    }

    private void CaptureOriginalRotation()
    {
        _originalRotation = transform.localRotation;
    }

    private void ApplyRecoilSmoothing()
    {
        // 1. Hồi phục dần mục tiêu về 0 (Recoil Recovery)
        _targetRotation = Vector3.Lerp(_targetRotation, Vector3.zero, Time.deltaTime * _recoilReturnSpeed);

        // 2. Nội suy vị trí hiện tại đến vị trí mục tiêu (Slerp smoothing)
        _currentRotation = Vector3.Slerp(_currentRotation, _targetRotation, Time.deltaTime * _recoilSnappiness);

        // 3. Áp dụng góc xoay vào Transform
        transform.localRotation = _originalRotation * Quaternion.Euler(_currentRotation);

        if (_currentRotation.sqrMagnitude < 0.01f && _targetRotation.sqrMagnitude < 0.01f)
        {
            _currentRotation = Vector3.zero;
            _targetRotation = Vector3.zero;
            transform.localRotation = _originalRotation;
            this.enabled = false; // Tắt script khi đã hồi phục hoàn toàn
        }
    }

    public Vector3 UpdateRecoil(float recoilAmount)
    {
        float _recoilX = -recoilAmount;
        float _recoilY = Random.Range(-recoilAmount * 0.2f, recoilAmount * 0.2f);
        float _recoilZ = -recoilAmount;

        Vector3 recoilVector = new Vector3(_recoilX, _recoilY, _recoilZ);

        _targetRotation += recoilVector;

        return recoilVector;
    }
}
