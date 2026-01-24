using UnityEngine;

public class RagdollSwitcher : MonoBehaviour
{
    [SerializeField] private Rigidbody[] _rigids;
    [SerializeField] private Animator _animator;

    private void Awake()
    {
        CollectRagdolls();
    }

    private void Start()
    {
        DisableRagdolls();
    }

    [ContextMenu("Collect ragdolls")]
    private void CollectRagdolls()
    {
        _rigids = GetComponentsInChildren<Rigidbody>();
    }

    [ContextMenu("Enable ragdolls")]
    public void EnableRagdolls()
    {
        foreach (var rigidbody in _rigids)
        {
            rigidbody.isKinematic = false;
        }
        _animator.enabled = false;
    }

    [ContextMenu("Disable ragdolls")]
    public void DisableRagdolls()
    {
        foreach (var rigidbody in _rigids)
        {
            rigidbody.isKinematic = true;
        }
        _animator.enabled = true;
    }
}
