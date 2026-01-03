using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    public float speed = 45f;

    void Update()
    {
        transform.Rotate(Vector3.up * speed * Time.deltaTime);
    }
}
