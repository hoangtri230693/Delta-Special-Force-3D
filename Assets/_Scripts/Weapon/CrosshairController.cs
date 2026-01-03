using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [SerializeField] private BarrelPointController _barrelPointController;

    private void Update()
    {
        transform.position = _barrelPointController._targetPosition;
        transform.rotation = _barrelPointController._targetRotation;
    }
}
