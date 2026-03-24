using DeltaSpecialForce3D.Enums;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    public ZombieStatsSO ZombieStats { get; private set; }

    [SerializeField] private ZombieAudio _zombieAudio;

    private void Awake()
    {
        GetZombieStats();
    }

    private void GetZombieStats()
    {
        ZombieStats = GameplayDataManager.instance._zombieStatsSO;
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
