using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Move To Position", story: "Move the [Agent] towards [CoverPosition] using NavMeshAgent", category: "Action", id: "3badca656470ade5270cfcbfa1ae10ef")]
public partial class MoveToPositionAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> CoverPosition;
    [SerializeReference] public BlackboardVariable<float> StopDistance = new BlackboardVariable<float> { Value = 1f };

    private NavMeshAgent _agent;

    protected override Status OnStart()
    {
        if (Agent.Value == null)
            return Status.Failure;

        _agent = Agent.Value.GetComponent<NavMeshAgent>();
        if (_agent == null)
            return Status.Failure;

        _agent.isStopped = false;
        _agent.SetDestination(CoverPosition.Value);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_agent == null)
            return Status.Failure;

        if (!_agent.pathPending && _agent.remainingDistance <= StopDistance.Value)
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

