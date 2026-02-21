using System.Collections;
using UnityEngine;

public class RagdollSwitcher : MonoBehaviour
{
    [SerializeField] private Rigidbody[] _rigids;
    [SerializeField] private Collider[] _colliders;
    [SerializeField] private Animator _animator;

    private void Awake()
    {
        CollectRagdolls();
    }


    [ContextMenu("Collect ragdolls")]
    private void CollectRagdolls()
    {
        _rigids = GetComponentsInChildren<Rigidbody>();
        _colliders = GetComponentsInChildren<Collider>();
    }

    [ContextMenu("Enable ragdolls")]
    public void EnableRagdolls()
    {
        foreach (var rigidbody in _rigids)
        {
            rigidbody.isKinematic = false;
        }
        //foreach (var collider in _colliders)
        //{
        //    collider.enabled = false;
        //}

        _animator.enabled = false;

        StartCoroutine(DisableRagdollsAfterTime());
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

    private IEnumerator DisableRagdollsAfterTime()
    {
        yield return new WaitForSeconds(5f);

        foreach (var rigidbody in _rigids)
        {
            rigidbody.isKinematic = true;
        }
    }
}
