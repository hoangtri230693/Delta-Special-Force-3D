using UnityEngine;
using Unity.Behavior;

public class LineOfSightDetector : MonoBehaviour
{
    public BehaviorGraphAgent behaviorAgent;

    [Header("Line of Sight Settings")]
    public float viewDistance = 15f;
    public float viewAngle = 45f;
    public LayerMask targetLayer;
    public LayerMask obstacleLayer;
    public float eyeHeight = 1.5f;

    [Header("Debug")]
    public Transform detectedTarget;

    public bool HasLineOfSight => detectedTarget != null;

    private void Update()
    {
        detectedTarget = null;

        Vector3 eyePos = transform.position + Vector3.up * eyeHeight;

        Collider[] targets = Physics.OverlapSphere(eyePos, viewDistance, targetLayer);

        foreach (var target in targets)
        {
            PlayerController pc = target.GetComponent<PlayerController>();
            if (pc == null || pc._lifeState != LifeState.Alive)
                continue;

            Vector3 dir = (target.transform.position - eyePos).normalized;
            float angle = Vector3.Angle(transform.forward, dir);

            if (angle > viewAngle * 0.5f) continue;

            if (!Physics.Raycast(eyePos, dir, out RaycastHit hit, viewDistance, obstacleLayer))
            {
                detectedTarget = target.transform;
                break;
            }

            if (hit.transform == target.transform)
            {
                detectedTarget = target.transform;
                break;
            }
        }

        behaviorAgent.BlackboardReference.SetVariableValue(
            "DetectTarget",
            detectedTarget ? detectedTarget.gameObject : null
        );

        behaviorAgent.BlackboardReference.SetVariableValue(
            "HasLineOfSight",
            HasLineOfSight
        );
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 eyePos = transform.position + Vector3.up * eyeHeight;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(eyePos, viewDistance);

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;

        Gizmos.DrawLine(eyePos, eyePos + leftBoundary * viewDistance);
        Gizmos.DrawLine(eyePos, eyePos + rightBoundary * viewDistance);
    }
}
