using UnityEngine;

public class ShellCasingController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;

    private void OnEnable()
    {
        float ejectForce = Random.Range(1.5f, 2.5f);
        float ejectAngle = Random.Range(30f, 60f);
        Vector3 forceDirection = Quaternion.Euler(0, ejectAngle, 0) * Vector3.right;
        _rigidbody.AddForce(forceDirection * ejectForce, ForceMode.Impulse);
        
        float randomTorqueX = Random.Range(-10f, 10f);
        float randomTorqueY = Random.Range(-10f, 10f);
        float randomTorqueZ = Random.Range(-10f, 10f);
        Vector3 randomTorque = new Vector3(randomTorqueX, randomTorqueY, randomTorqueZ);
        _rigidbody.AddTorque(randomTorque, ForceMode.Impulse);
    }
}
