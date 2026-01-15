using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class RangeDetector : MonoBehaviour
{
    [Header("Behavior")]
    public BehaviorGraphAgent behaviorAgent;

    [Header("Detection Settings")]
    public float detectionRange = 20f;
    public LayerMask detectionLayer;
    public LayerMask obstacleLayer;

    [Header("Rotation Settings")]
    public bool rotateToTarget = true;
    public float rotateSpeed = 6f;

    [Header("Defend Settings")]
    public float fleeCheckRadius = 20f;
    public float coverOffset = 2f;
    public float safeDistance = 3f;

    [Header("Debug")]
    public Transform detectedTarget;
    public Vector3 safeCoverPosition;

    public bool IsTargetInRange => detectedTarget != null;

    private void Update()
    {
        DetectTarget();
        RotateTowardsTarget();
        HandleDefendLogic();
        UpdateBlackboard();
    }

    // ========================= DETECTION =========================
    private void DetectTarget()
    {
        detectedTarget = null;

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            detectionRange,
            detectionLayer
        );

        foreach (var hit in hits)
        {
            PlayerHealth health = hit.GetComponentInChildren<PlayerHealth>();
            if (health == null || health._isDead)
                continue;

            detectedTarget = hit.transform.root;
            break;
        }
    }

    // ========================= ROTATION =========================
    private void RotateTowardsTarget()
    {
        if (!rotateToTarget || detectedTarget == null)
            return;

        Vector3 dir = detectedTarget.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotateSpeed
        );
    }

    // ========================= DEFEND / COVER =========================
    private void HandleDefendLogic()
    {
        if (!behaviorAgent.BlackboardReference.GetVariableValue("ShouldDefend", out bool shouldDefend))
            return;

        if (!shouldDefend || detectedTarget == null)
            return;

        safeCoverPosition = FindCoverPoint(detectedTarget.position);

        behaviorAgent.BlackboardReference.SetVariableValue(
            "HasCover",
            safeCoverPosition != Vector3.zero
        );

        behaviorAgent.BlackboardReference.SetVariableValue(
            "CoverPosition",
            safeCoverPosition
        );
    }

    private Vector3 FindCoverPoint(Vector3 targetPosition)
    {
        Vector3 bestPoint = Vector3.zero;
        float bestScore = float.MinValue;

        Vector3 origin = transform.position + Vector3.up * 1.5f;

        for (int i = 0; i < 36; i++)
        {
            float angle = i * 10f;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            if (!Physics.Raycast(origin, dir, out RaycastHit hit, fleeCheckRadius, obstacleLayer))
                continue;

            Vector3 behindObstacle =
                hit.point - (targetPosition - hit.point).normalized * coverOffset;

            if (!NavMesh.SamplePosition(behindObstacle, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
                continue;

            // Kiểm tra có che tầm nhìn target không
            bool blocked = Physics.Linecast(
                navHit.position + Vector3.up * 1.5f,
                targetPosition + Vector3.up * 1.5f,
                obstacleLayer
            );

            if (!blocked)
                continue;

            float distanceToTarget = Vector3.Distance(navHit.position, targetPosition);
            float distanceToBot = Vector3.Distance(navHit.position, transform.position);

            float score = distanceToTarget - distanceToBot;

            if (score > bestScore)
            {
                bestScore = score;
                bestPoint = navHit.position;
            }
        }

        return bestPoint;
    }

    // ========================= BLACKBOARD =========================
    private void UpdateBlackboard()
    {
        GameObject targetGO = detectedTarget != null ? detectedTarget.gameObject : null;

        behaviorAgent.BlackboardReference.SetVariableValue("HearTarget", targetGO);
        behaviorAgent.BlackboardReference.SetVariableValue("IsTargetInRange", detectedTarget != null);
    }

    // ========================= GIZMOS =========================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (safeCoverPosition != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(safeCoverPosition, 0.3f);
        }

        if (detectedTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                transform.position + Vector3.up * 1.5f,
                detectedTarget.position + Vector3.up * 1.5f
            );
        }
    }
}
