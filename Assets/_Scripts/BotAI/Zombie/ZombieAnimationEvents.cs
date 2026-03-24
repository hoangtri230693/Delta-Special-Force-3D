using UnityEngine;
using DeltaSpecialForce3D.Enums;

public class ZombieAnimationEvents : MonoBehaviour
{
    private GameObject targetPlayer;
    private ZombieController _zombieController;
    private ZombieAudio _zombieAudio;

    private void Awake()
    {
        _zombieController = GetComponent<ZombieController>();
        _zombieAudio = GetComponent<ZombieAudio>();
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

    public void FootStepEvent()
    {
        _zombieAudio.PlayZombieSound(ZombieSoundType.FootStep);
    }   
    
    public void AttackStartEvent()
    {
        _zombieAudio.PlayZombieSound(ZombieSoundType.Attack);
    }

    public void AttackHitEvent()
    {
        if (targetPlayer != null)
        {
            PlayerHealth health = targetPlayer.GetComponent<PlayerHealth>();

            if (health != null)
            {
                health.UpdateHealth(_zombieController.ZombieStats.damage, ItemType.None);
                _zombieAudio.PlayZombieSound(ZombieSoundType.Hit);
            }
        }
    }
}
