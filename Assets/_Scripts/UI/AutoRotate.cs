using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 45f;
    [SerializeField] private Vector3 _rotationAxis = Vector3.up;

    private void Update()
    {
        RotateModel();
    }

    private void RotateModel()
    {
        transform.Rotate(_rotationAxis * _rotationSpeed * Time.deltaTime);
    }
}
