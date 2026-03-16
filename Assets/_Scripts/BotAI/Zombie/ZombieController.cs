using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    [SerializeField] private ZombieStatsSO _zombieStats;
    public ZombieStatsSO ZombieStats => _zombieStats;

    private ZombieAudio _zombieAudio;

    private void Awake()
    {
        _zombieAudio = GetComponent<ZombieAudio>();
    }

    public void HandleHurt()
    {
        _zombieAudio.PlayZombieSound(ZombieSoundType.Hurt);
    }

    public void HandleDeath()
    {
        if (TryGetComponent<CharacterController>(out var cc)) cc.enabled = false;
        if (TryGetComponent<NavMeshAgent>(out var agent)) agent.enabled = false;
        if (TryGetComponent<BehaviorGraphAgent>(out var behavior)) behavior.enabled = false;
        if (TryGetComponent<RagdollSwitcher>(out var switcher)) switcher.EnableRagdolls();

        _zombieAudio.PlayZombieSound(ZombieSoundType.Fall);
    }
}
