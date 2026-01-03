using UnityEngine;
using Unity.Behavior;
using UnityEngine.AI;

public class LineOfSightDetector : MonoBehaviour
{
    public BehaviorGraphAgent behaviorAgent;

    [Header("Line of Sight Settings")]
    public float viewDistance = 15f;
    public float viewAngle = 45f;
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
                }
                else if (hit.transform == target.transform)
                {
                    detectedTarget = target.transform;
                }
            }
        }

        GameObject targetGo = (detectedTarget != null) ? detectedTarget.gameObject : null;
        behaviorAgent.BlackboardReference.SetVariableValue("DetectTarget", targetGo);
        behaviorAgent.BlackboardReference.SetVariableValue("HasLineOfSight", HasLineOfSight);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red ;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewDistance);
    }
}