using UnityEngine;

public class WeaponCollision : MonoBehaviour
{
    [SerializeField] private WeaponManager _weaponManager;

    private void OnCollisionEnter(Collision collision)
    {
        if (IsValidTarget(collision.collider))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player == null) return;
            _weaponManager.AssignToPlayer(player.transform);
            Debug.Log("Weapon picked up by player.");
        }
    }

    private bool IsValidTarget(Collider other)
    {
        return other.CompareTag("AlphaTeam") || other.CompareTag("BravoTeam") ||
               other.CompareTag("DeltaTeam") || other.CompareTag("Terrorist") ||
               other.CompareTag("Zombie");
    }
}
