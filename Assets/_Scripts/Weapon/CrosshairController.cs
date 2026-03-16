using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    public void UpdateTransform(Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }
}
