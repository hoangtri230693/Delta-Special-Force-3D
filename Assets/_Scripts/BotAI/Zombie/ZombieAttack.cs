using UnityEngine;

public class ZombieAttack : MonoBehaviour
{
    private int damageAmount = 5;
    private GameObject targetPlayer;
    private ZombieAudio _zombieAudio;

    private void Awake()
    {
        _zombieAudio = GetComponentInParent<ZombieAudio>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            targetPlayer = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            targetPlayer = null;
        }
    }

    public void OnAttackStart()
    {
        _zombieAudio.PlayAttackSound();
    }

    public void OnAttackHit()
    {
        if (targetPlayer != null)
        {
            PlayerHealth health = targetPlayer.GetComponent<PlayerHealth>();

            if (health != null)
            {
                health.UpdateHealth(damageAmount, ItemType.None);
                _zombieAudio.PlayAttackHitSound();
            }
        }
    }
}
