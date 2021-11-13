using UnityEngine;
public class ReleaseBlock : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.GetComponent<Collider2D>().enabled = false;
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject block = animator.gameObject;
        BlockObjectPool.Get().Pool.Release(block);
    }
}