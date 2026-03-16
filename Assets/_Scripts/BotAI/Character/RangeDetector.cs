using Unity.Behavior;
using UnityEngine;

public class RangeDetector : MonoBehaviour
{
    public BehaviorGraphAgent behaviorAgent;

    [Header("Detection Settings")]
    public float detectionRange = 20f;
    public LayerMask detectionLayer;

    [Header("Debug")]
    public Transform detectedTarget;

    public bool IsTargetInRange => detectedTarget != null;

    private void Update()
    {
        DetectTargetInRange();
    }

    private void DetectTargetInRange()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, detectionLayer);

        if (hits.Length > 0)
        {
            detectedTarget = hits[0].transform;
        }
        else
        {
            detectedTarget = null;
        }

        UpdateBlackboard();
    }

    private void UpdateBlackboard()
    {
        behaviorAgent.BlackboardReference.SetVariableValue("Target", detectedTarget ? detectedTarget.gameObject : null);
        behaviorAgent.BlackboardReference.SetVariableValue("IsTargetInRange", IsTargetInRange);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = detectedTarget ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
