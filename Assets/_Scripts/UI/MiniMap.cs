using UnityEngine;

public class MiniMap : MonoBehaviour
{
    public Transform _player;

    private void Update()
    {
        if (_player == null) _player = GameManager.instance._player.transform;

        Vector3 newPosition = _player.position;
        newPosition.y = transform.position.y;
        transform.position = newPosition;
    }
}
