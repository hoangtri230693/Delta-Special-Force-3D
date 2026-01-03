using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Move Away From Target", story: "Move the [Agent] away from [Target] using NavMeshAgent", category: "Action", id: "a080788f51068409d4ffe7bd0a97f4ad")]
public partial class MoveAwayFromTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    [SerializeReference] public BlackboardVariable<float> _fleeDistance = new BlackboardVariable<float> { Value = 10f };
    [SerializeReference] public BlackboardVariable<float> _stopDistance = new BlackboardVariable<float> { Value = 1f };

    private NavMeshAgent _agent;
    private Vector3 _fleePosition;

    protected override Status OnStart()
    {
        if (Agent.Value == null || Target.Value == null)
            return Status.Failure;

        _agent = Agent.Value.GetComponent<NavMeshAgent>();
        if (_agent == null)
            return Status.Failure;

        Vector3 direction = (Agent.Value.transform.position - Target.Value.transform.position).normalized;
        float randomAngle = UnityEngine.Random.Range(-60f, 60f);
        Vector3 randomDir = Quaternion.Euler(0, randomAngle, 0) * direction;
        _fleePosition = Agent.Value.transform.position + direction * _fleeDistance.Value;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(_fleePosition, out hit, 5f, NavMesh.AllAreas))
        {
            _agent.isStopped = false;
            _agent.SetDestination(hit.position);
            return Status.Running;
        }

        return Status.Failure;
    }

    protected override Status OnUpdate()
    {
        if (_agent == null)
            return Status.Failure;

        if (!_agent.pathPending && _agent.remainingDistance <= _stopDistance.Value)
        {
            _agent.isStopped = true;
            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_agent != null)
            _agent.isStopped = true;
    }
}

