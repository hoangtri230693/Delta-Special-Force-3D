using UnityEngine;

public class ZombieAttack : MonoBehaviour
{
    public int damageAmount = 10;
    private GameObject targetPlayer;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            targetPlayer = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            targetPlayer = null;
        }
    }

    public void OnAttackHit()
    {
        if (targetPlayer != null)
        {
            PlayerHealth health = targetPlayer.GetComponent<PlayerHealth>();

            if (health != null)
            {
                //health.UpdateHealth(damageAmount); // Giả sử script PlayerHealth của bạn có hàm TakeDamage
                Debug.Log("Zombie đã vả Player một phát!");
            }
        }
    }
}
