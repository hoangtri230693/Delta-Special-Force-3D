using UnityEngine;

public class WeaponCollision : MonoBehaviour
{
    [SerializeField] private WeaponManager _weaponManager;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Counter") || 
            collision.gameObject.layer == LayerMask.NameToLayer("Terrorist"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player == null) return;
            _weaponManager.AssignToPlayer(player.transform);
            Debug.Log("Weapon picked up by player.");
        }
    }
}
