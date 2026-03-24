using UnityEngine;

public class MiniMap : MonoBehaviour
{
    public static MiniMap instance;

    private Transform _player;

    private void Awake()
    {
        instance = this;
    }

    private void LateUpdate()
    {
        if (_player == null) return;

        FollowPlayerTransform();
    }

    public void SetupPlayerTransform(Transform player)
    {
        _player = player;
    }

    private void FollowPlayerTransform()
    {
        Vector3 newPosition = _player.position;
        newPosition.y = _player.position.y + 20f;
        transform.position = newPosition;
        transform.rotation = Quaternion.Euler(90f, _player.eulerAngles.y, 0f);
    }
}
