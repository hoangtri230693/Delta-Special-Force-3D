using UnityEngine;
using UnityEngine.AI;

public class BotNavAgent : MonoBehaviour
{
    [SerializeField] private NavMeshAgent _agent;

    public Vector3 DesiredVelocity => _agent.desiredVelocity;

    private void Awake()
    {
        _agent.updatePosition = false;
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
    }

    private void LateUpdate()
    {
        _agent.nextPosition = transform.position;
    }

    public void MoveTo(Vector3 position)
    {
        _agent.SetDestination(position);
    }
}
