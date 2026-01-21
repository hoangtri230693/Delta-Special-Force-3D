using UnityEngine;

public class MiniMap : MonoBehaviour
{
    public static MiniMap instance;

    private Transform _player;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (_player == null) return;

        Vector3 newPosition = _player.position;
        newPosition.y = transform.position.y;
        transform.position = newPosition;
    }

    public void SetupPlayerTransform(Transform playerTransform)
    {
        _player = playerTransform;
    }
}
