using Unity.Behavior;
using UnityEngine;


public class LineOfSightDetector_Zombie : MonoBehaviour
{
    public BehaviorGraphAgent behaviorAgent;

    [Header("Line of Sight Settings")]
    public float viewDistance = 15f;
    [Range(0, 180f)] public float viewAngle = 60f;
    public LayerMask targetLayer;
    public LayerMask obstacleLayer;

    [Header("Debug")]
    public Transform detectedTarget;

    public bool HasLineOfSight => detectedTarget != null;


    private void Update()
    {
        detectedTarget = null;

        Collider[] targets = Physics.OverlapSphere(transform.position, viewDistance, targetLayer);
        foreach (var target in targets)
        {
            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToTarget);

            if (angle < viewAngle)
            {
                if (!Physics.Raycast(transform.position + Vector3.up * 1.5f, directionToTarget, out RaycastHit hit, viewDistance, obstacleLayer))
                {
                    detectedTarget = target.transform;
                    break;
                }
                else if (hit.transform == target.transform)
                {
                    detectedTarget = target.transform;
                    break;
                }
            }
        }

        behaviorAgent.BlackboardReference.SetVariableValue("Target", detectedTarget ? detectedTarget.gameObject : null);
        behaviorAgent.BlackboardReference.SetVariableValue("HasLineOfSight", HasLineOfSight);
    }

    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = detectedTarget ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;

        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewDistance);
    }
}
