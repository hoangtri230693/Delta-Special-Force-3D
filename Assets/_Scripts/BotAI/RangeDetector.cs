using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class RangeDetector : MonoBehaviour
{
    public BehaviorGraphAgent behaviorAgent;

    [Header("Detection Settings")]
    public float detectionRange = 20f;
    public LayerMask detectionLayer;
    public LayerMask obstacleLayer;

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
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, detectionLayer);
        if (hits.Length > 0)
        {
            detectedTarget = hits[0].transform.root;
        }
        else
        {
            detectedTarget = null;
        }

        if (behaviorAgent.BlackboardReference.GetVariableValue("ShouldDefend", out bool shouldDefend))
        {
            if (shouldDefend)
            {
                safeCoverPosition = FindCoverPoint(detectedTarget.position);
                behaviorAgent.BlackboardReference.SetVariableValue("HasCover", safeCoverPosition != Vector3.zero);
                behaviorAgent.BlackboardReference.SetVariableValue("CoverPosition", safeCoverPosition);
            }

        }

        GameObject targetGo = (detectedTarget != null) ? detectedTarget.gameObject : null;
        behaviorAgent.BlackboardReference.SetVariableValue("HearTarget", targetGo);
        behaviorAgent.BlackboardReference.SetVariableValue("IsTargetInRange", IsTargetInRange);
    }

    private Vector3 FindCoverPoint(Vector3 targetPosition)
    {
        Vector3 bestPoint = Vector3.zero;
        float bestScore = float.MinValue;

        for (int i = 0; i < 36; i++)
        {
            float angle = i * 10f;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Vector3 origin = transform.position + Vector3.up * 1.5f;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, fleeCheckRadius, obstacleLayer))
            {
                // Điểm sau vật cản
                Vector3 behindObstacle = hit.point - (targetPosition - hit.point).normalized * coverOffset;

                // Kiểm tra điểm có nằm trên NavMesh không
                if (NavMesh.SamplePosition(behindObstacle, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
                {
                    // Kiểm tra có thực sự bị che khỏi Target không
                    if (Physics.Linecast(navHit.position + Vector3.up * 1.5f, targetPosition + Vector3.up * 1.5f, obstacleLayer))
                    {
                        float distanceToTarget = Vector3.Distance(navHit.position, targetPosition);

                        // Điểm càng xa target + gần Enemy càng được ưu tiên
                        float score = distanceToTarget - Vector3.Distance(navHit.position, transform.position);
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestPoint = navHit.position;
                        }
                    }
                }
            }
        }

        return bestPoint;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (safeCoverPosition != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(safeCoverPosition, 0.3f);
        }
    }
}