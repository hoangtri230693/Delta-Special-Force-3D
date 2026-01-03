using UnityEngine;

public class ActionLayerWeight : StateMachineBehaviour
{
    [SerializeField] private int _primaryItemLayerIndex = 1;   
    [SerializeField] private int _meleeItemLayerIndex = 2;
    [SerializeField] private int _throwableItemLayerIndex = 3;
    [SerializeField] private int _actionLayerIndex = 4;

    private int _currentLayerIndex = 0;


    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.GetLayerWeight(_primaryItemLayerIndex) > 0f) _currentLayerIndex = _primaryItemLayerIndex;
        if (animator.GetLayerWeight(_meleeItemLayerIndex) > 0f) _currentLayerIndex = _meleeItemLayerIndex;
        if (animator.GetLayerWeight(_throwableItemLayerIndex) > 0f) _currentLayerIndex = _throwableItemLayerIndex;

        animator.SetLayerWeight(_currentLayerIndex, 0f);
        animator.SetLayerWeight(_actionLayerIndex, 1f);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {    
        animator.SetLayerWeight(_actionLayerIndex, 0f);
        animator.SetLayerWeight(_currentLayerIndex, 1f);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
